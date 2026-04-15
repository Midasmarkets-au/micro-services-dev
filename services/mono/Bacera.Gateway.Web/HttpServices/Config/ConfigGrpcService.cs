using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
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
    ITenantGetter tenantGetter,
    ConfigService configSvc,
    ConfigurationService configurationService)
    : TenantConfigurationService.TenantConfigurationServiceBase
{
    // ─── Sites ────────────────────────────────────────────────────────────────

    public override async Task<GetSitesResponse> GetSites(GetSitesRequest request, ServerCallContext context)
    {
        var sites = await tenantDb.Sites.ToListAsync();
        var inner = new SitesResponse();
        inner.Sites.AddRange(sites.Select(s => new SiteItem
        {
            Id   = s.Id,
            Name = s.Name ?? "",
            Url  = "",   // Site entity does not carry a URL field
        }));
        return new GetSitesResponse { Data = inner };
    }

    // ─── Configurations ───────────────────────────────────────────────────────

    public override async Task<ListConfigurationsResponse> ListConfigurations(
        ListConfigurationsRequest request, ServerCallContext context)
    {
        // Unauthenticated page-load requests (e.g. category=public) reach here via the Host-based tenant
        // resolution in MultiTenantServiceMiddleware. If the host isn't registered in the Domains table yet,
        // tenantId stays 0 and TenantDbContext has no connection string — return empty rather than 500.
        if (tenantGetter.GetTenantId() == 0)
            return new ListConfigurationsResponse();

        var criteria = new Configuration.Criteria
        {
            Page     = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size     = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            Category = request.HasCategory ? request.Category : null,
            RowIds   = request.RowIds.Count > 0 ? [.. request.RowIds] : null,
            Keys     = request.Keys.Count  > 0 ? [.. request.Keys]   : null,
        };

        var items = await tenantDb.Configurations
            .PagedFilterBy(criteria)
            .ToTenantViewModel()
            .ToListAsync();

        var response = new ListConfigurationsResponse
        {
            Criteria = new PaginationMeta
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

    public override async Task<GetCategoryConfigResponse> GetCategoryConfig(
        GetCategoryConfigRequest request, ServerCallContext context)
    {
        var items = await tenantDb.Configurations
            .Where(x => x.Category.ToLower() == request.Category.ToLower() && x.RowId == request.RowId)
            .ToTenantViewModel()
            .ToListAsync();

        var inner = new CategoryConfigResponse();
        foreach (var item in items)
            if (item.Key != null)
                inner.Items[item.Key] = item.ValueString ?? "";
        return new GetCategoryConfigResponse { Data = inner };
    }

    public override async Task<GetConfigValueResponse> GetConfigValue(
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

        var inner = new ConfigValueResponse { Key = result.Key ?? "", Value = result.ValueString ?? "" };
        return new GetConfigValueResponse { Data = inner };
    }

    public override async Task<UpdateConfigValueResponse> UpdateConfigValue(
        UpdateConfigValueRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var category = ConfigCategoryTypes.ParseCategory(request.Category);
        await configSvc.SetAsync<object>(category, request.RowId, request.Key, request.Spec.Value, partyId);
        var value = await configSvc.GetRawValueAsync(category, request.RowId, request.Key);
        var inner = new ConfigValueResponse { Key = request.Key, Value = value ?? "" };
        return new UpdateConfigValueResponse { Data = inner };
    }

    public override Task<DeleteConfigValueResponse> DeleteConfigValue(
        DeleteConfigValueRequest request, ServerCallContext context)
    {
        // Delete is not supported — matches current controller behavior
        return Task.FromResult(new DeleteConfigValueResponse
        {
            Success = false,
            Message = "Delete is not supported right now",
        });
    }

    // ─── Site-specific overrides ──────────────────────────────────────────────

    public override async Task<UpdateSiteDefaultEmailResponse> UpdateSiteDefaultEmail(
        UpdateSiteDefaultEmailRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var ok = await configurationService.SetDefaultEmailAddressAsync(request.Spec?.Value ?? "", request.SiteId, partyId);
        return new UpdateSiteDefaultEmailResponse { Success = ok };
    }

    public override async Task<UpdateSiteDefaultEmailDisplayNameResponse> UpdateSiteDefaultEmailDisplayName(
        UpdateSiteDefaultEmailDisplayNameRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var ok = await configurationService.SetDefaultEmailDisplayNameAsync(request.Spec?.Value ?? "", request.SiteId, partyId);
        return new UpdateSiteDefaultEmailDisplayNameResponse { Success = ok };
    }

    public override async Task<UpdateSiteDefaultFundTypeResponse> UpdateSiteDefaultFundType(
        UpdateSiteDefaultFundTypeRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var ok = await configurationService.SetDefaultFundTypeAsync(request.Spec?.Value ?? 0, request.SiteId, partyId);
        return new UpdateSiteDefaultFundTypeResponse { Success = ok };
    }

    public override async Task<UpdateSiteDefaultTradeServiceResponse> UpdateSiteDefaultTradeService(
        UpdateSiteDefaultTradeServiceRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var ok = await configurationService.SetDefaultTradeServiceAsync(request.Spec?.Value ?? 0, request.SiteId, partyId);
        return new UpdateSiteDefaultTradeServiceResponse { Success = ok };
    }

    // ─── Additional site / bulk endpoints ────────────────────────────────────

    public override async Task<GetSiteAllConfigsResponse> GetSiteAllConfigs(
        GetSiteAllConfigsRequest request, ServerCallContext context)
    {
        var inner = new CategoryConfigResponse();
        // GetAllConfigurationBySiteAsync returns ApplicationConfigure.AllSetting;
        // flatten all key→value pairs from its raw Configuration list
        var rawItems = await tenantDb.Configurations
            .Where(x => x.RowId == request.SiteId)
            .ToTenantViewModel()
            .ToListAsync();
        foreach (var item in rawItems)
            if (item.Key != null)
                inner.Items[item.Key] = item.ValueString ?? "";
        return new GetSiteAllConfigsResponse { Data = inner };
    }

    public override async Task<GetAllConfigurationsResponse> GetAllConfigurations(
        GetAllConfigurationsRequest request, ServerCallContext context)
    {
        var items = await tenantDb.Configurations
            .OrderBy(x => x.Key)
            .ToTenantViewModel()
            .ToListAsync();

        var inner = new AllConfigurationsResponse();
        inner.Data.AddRange(items.Select(c => new ConfigItem
        {
            Category = c.Category ?? "",
            RowId    = c.RowId,
            Key      = c.Key ?? "",
            Value    = c.ValueString ?? "",
        }));
        return new GetAllConfigurationsResponse { Data = inner };
    }

    public override async Task<ReloadConfigurationResponse> ReloadConfiguration(
        ReloadConfigurationRequest request, ServerCallContext context)
    {
        await configurationService.ResetCacheAsync();
        await configSvc.ResetCacheAsync();
        return new ReloadConfigurationResponse { Success = true };
    }

    public override async Task<UpdateSiteConfigResponse> UpdateSiteConfig(
        UpdateSiteConfigRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var category = ConfigCategoryTypes.ParseCategory(nameof(Public));
        await configSvc.SetAsync<object>(category, request.SiteId, request.Key, request.Spec.Value, partyId);
        var value = await configSvc.GetRawValueAsync(category, request.SiteId, request.Key);
        var inner = new ConfigValueResponse { Key = request.Key, Value = value ?? "" };
        return new UpdateSiteConfigResponse { Data = inner };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private Task<Configuration.TenantViewModel?> GetViewModelAsync(string category, long rowId, string key)
        => tenantDb.Configurations
            .Where(x => x.Category.ToLower() == category.ToLower() && x.RowId == rowId && x.Key == key)
            .ToTenantViewModel()
            .FirstOrDefaultAsync();

    private static long GetPartyId(ServerCallContext ctx)
        => ctx.GetHttpContext().User.GetPartyId();
}
