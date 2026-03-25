using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Grpc.Core;
using Http.V1;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.HttpServices.Config;

/// <summary>
/// gRPC JSON Transcoding implementation of TenantConfigurationService.
/// Replaces core CRUD endpoints in Areas/Tenant/Controllers/ConfigurationController.cs.
/// Site-specific toggle endpoints (verification-quiz, high-dollar-value, etc.) remain in
/// ConfigurationController until a follow-up proto expansion.
/// Routes defined via google.api.http annotations in config.proto.
/// </summary>
public class TenantConfigurationGrpcService(
    TenantDbContext tenantDb,
    ConfigService configSvc,
    ConfigurationService configurationService)
    : TenantConfigurationService.TenantConfigurationServiceBase
{
    // ─── Sites ────────────────────────────────────────────────────────────────

    public override async Task<SitesResponse> GetSites(EmptyRequest request, ServerCallContext context)
    {
        var sites = await tenantDb.Sites.ToListAsync();
        var response = new SitesResponse();
        response.Sites.AddRange(sites.Select(s => new SiteItem
        {
            Id   = s.Id,
            Name = s.Name ?? "",
            Url  = "",   // Site entity does not carry a URL field
        }));
        return response;
    }

    // ─── Configurations ───────────────────────────────────────────────────────

    public override async Task<ListConfigurationsResponse> ListConfigurations(
        ListConfigurationsRequest request, ServerCallContext context)
    {
        var criteria = new Configuration.Criteria
        {
            Page     = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size     = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            Category = request.HasCategory ? request.Category : null,
        };

        var items = await tenantDb.Configurations
            .PagedFilterBy(criteria)
            .ToTenantViewModel()
            .ToListAsync();

        var response = new ListConfigurationsResponse
        {
            Meta = new PaginationMeta
            {
                Page      = criteria.Page,
                Size      = criteria.Size,
                Total     = criteria.Total,
                PageCount = criteria.Total > 0 ? (int)Math.Ceiling((double)criteria.Total / criteria.Size) : 0,
                HasMore   = criteria.Page * criteria.Size < criteria.Total,
            }
        };
        response.Data.AddRange(items.Select(c => new ConfigItem
        {
            Category = c.Category ?? "",
            RowId    = c.RowId,
            Key      = c.Key ?? "",
            Value    = c.ValueString ?? "",
        }));
        return response;
    }

    public override async Task<CategoryConfigResponse> GetCategoryConfig(
        GetCategoryConfigRequest request, ServerCallContext context)
    {
        var items = await tenantDb.Configurations
            .Where(x => x.Category.ToLower() == request.Category.ToLower() && x.RowId == request.RowId)
            .ToTenantViewModel()
            .ToListAsync();

        var response = new CategoryConfigResponse();
        foreach (var item in items)
            if (item.Key != null)
                response.Items[item.Key] = item.ValueString ?? "";
        return response;
    }

    public override async Task<ConfigValueResponse> GetConfigValue(
        GetConfigValueRequest request, ServerCallContext context)
    {
        var result = await GetViewModelAsync(request.Category, request.RowId, request.Key);

        if (request.HasIsInherit && request.IsInherit && result == null)
        {
            if (request.Category.Equals(nameof(Bacera.Gateway.Core.Types.Public), StringComparison.OrdinalIgnoreCase) ||
                request.Category.Equals(nameof(Party), StringComparison.OrdinalIgnoreCase))
            {
                result = await GetViewModelAsync(nameof(Bacera.Gateway.Core.Types.Public), 0, request.Key);
            }
            else if (request.Category.Equals(nameof(Account), StringComparison.OrdinalIgnoreCase))
            {
                var account = await tenantDb.Accounts
                    .Where(x => x.Id == request.RowId)
                    .Select(x => new { x.PartyId, x.SiteId })
                    .SingleOrDefaultAsync();

                if (account != null)
                {
                    result = await GetViewModelAsync(nameof(Party), account.PartyId, request.Key);
                    result ??= await GetViewModelAsync(nameof(Bacera.Gateway.Core.Types.Public), account.SiteId, request.Key);
                }
                result ??= await GetViewModelAsync(nameof(Bacera.Gateway.Core.Types.Public), 0, request.Key);
            }
        }

        if (result == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Config not found"));

        return new ConfigValueResponse { Key = result.Key ?? "", Value = result.ValueString ?? "" };
    }

    public override async Task<ConfigValueResponse> UpdateConfigValue(
        UpdateConfigValueRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var category = ConfigCategoryTypes.ParseCategory(request.Category);
        await configSvc.SetAsync<object>(category, request.RowId, request.Key, request.Spec.Value, partyId);
        var value = await configSvc.GetRawValueAsync(category, request.RowId, request.Key);
        return new ConfigValueResponse { Key = request.Key, Value = value ?? "" };
    }

    public override Task<OperationResponse> DeleteConfigValue(
        GetConfigValueRequest request, ServerCallContext context)
    {
        // Delete is not supported — matches current controller behavior
        return Task.FromResult(new OperationResponse
        {
            Success = false,
            Message = "Delete is not supported right now",
        });
    }

    // ─── Site-specific overrides ──────────────────────────────────────────────

    public override async Task<OperationResponse> UpdateSiteDefaultEmail(
        UpdateSiteStringValueRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var ok = await configurationService.SetDefaultEmailAddressAsync(request.Spec?.Value ?? "", request.SiteId, partyId);
        return new OperationResponse { Success = ok };
    }

    public override async Task<OperationResponse> UpdateSiteDefaultEmailDisplayName(
        UpdateSiteStringValueRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var ok = await configurationService.SetDefaultEmailDisplayNameAsync(request.Spec?.Value ?? "", request.SiteId, partyId);
        return new OperationResponse { Success = ok };
    }

    public override async Task<OperationResponse> UpdateSiteDefaultFundType(
        UpdateSiteIntValueRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var ok = await configurationService.SetDefaultFundTypeAsync(request.Spec?.Value ?? 0, request.SiteId, partyId);
        return new OperationResponse { Success = ok };
    }

    public override async Task<OperationResponse> UpdateSiteDefaultTradeService(
        UpdateSiteIntValueRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var ok = await configurationService.SetDefaultTradeServiceAsync(request.Spec?.Value ?? 0, request.SiteId, partyId);
        return new OperationResponse { Success = ok };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private Task<Configuration.TenantViewModel?> GetViewModelAsync(string category, long rowId, string key)
        => tenantDb.Configurations
            .Where(x => x.Category.ToLower() == category.ToLower() && x.RowId == rowId && x.Key == key)
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }
}
