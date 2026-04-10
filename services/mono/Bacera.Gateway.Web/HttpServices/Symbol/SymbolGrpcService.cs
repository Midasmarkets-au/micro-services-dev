using Bacera.Gateway.Services;
using Grpc.Core;
using Http.V1;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtoSymbol         = Http.V1.Symbol;
using ProtoExchangeRate   = Http.V1.ExchangeRate;
using ProtoSymbolCategory = Http.V1.SymbolCategory;

namespace Bacera.Gateway.Web.HttpServices.Symbol;

/// <summary>
/// gRPC JSON Transcoding implementation of TenantSymbolService.
/// Replaces Areas/Tenant/Controllers/SymbolController.cs.
/// Routes defined via google.api.http annotations in symbol.proto.
/// </summary>
public class TenantSymbolGrpcService(TenantDbContext db) : TenantSymbolService.TenantSymbolServiceBase
{
    // ─── Symbol CRUD ──────────────────────────────────────────────────────────

    public override async Task<ListSymbolsResponse> ListSymbols(
        ListSymbolsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Symbol.Criteria
        {
            Page       = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size       = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
            CategoryId = request.HasCategoryId ? request.CategoryId : null,
            Code       = request.HasKeywords   ? request.Keywords   : null,
        };

        var items = await db.Symbols
            .PagedFilterBy<Bacera.Gateway.Symbol, int>(criteria)
            .Select(x => new { x.Id, x.Code, x.Category, x.CategoryId, x.Type })
            .ToListAsync();

        var response = new ListSymbolsResponse
        {
            Criteria = BuildMeta(criteria.Page, criteria.Size, criteria.Total)
        };
        response.Data.AddRange(items.Select(s => new ProtoSymbol
        {
            Id          = s.Id,
            Name        = s.Code,
            Description = s.Category ?? "",
            CategoryId  = s.CategoryId,
            Type        = s.Type,
        }));
        return response;
    }

    public override async Task<GetSymbolResponse> GetSymbol(GetSymbolRequest request, ServerCallContext context)
    {
        var s = await db.Symbols.FindAsync(request.Id);
        if (s == null) throw new RpcException(new Status(StatusCode.NotFound, "Symbol not found"));

        return new GetSymbolResponse
        {
            Data = new ProtoSymbol
            {
                Id          = s.Id,
                Name        = s.Code,
                Description = s.Category ?? "",
                CategoryId  = s.CategoryId,
                Type        = s.Type,
            },
        };
    }

    public override async Task<CreateSymbolResponse> CreateSymbol(
        CreateSymbolRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var entity = new Bacera.Gateway.Symbol
        {
            Code            = request.Name,
            Category        = request.Description,
            CategoryId      = request.CategoryId,
            Type            = request.Type,
            CreatedOn       = DateTime.UtcNow,
            OperatorPartyId = partyId,
        };
        db.Symbols.Add(entity);
        await db.SaveChangesAsync();

        return new CreateSymbolResponse
        {
            Data = new ProtoSymbol
            {
                Id          = entity.Id,
                Name        = entity.Code,
                Description = entity.Category ?? "",
                CategoryId  = entity.CategoryId,
                Type        = entity.Type,
            },
        };
    }

    public override async Task<UpdateSymbolResponse> UpdateSymbol(
        UpdateSymbolRequest request, ServerCallContext context)
    {
        var entity = await db.Symbols.FindAsync(request.Id);
        if (entity == null) throw new RpcException(new Status(StatusCode.NotFound, "Symbol not found"));

        entity.Code       = request.Model.Name;
        entity.Category   = request.Model.Description;
        entity.CategoryId = request.Model.CategoryId;
        entity.Type       = request.Model.Type;

        await db.SaveChangesAsync();

        return new UpdateSymbolResponse
        {
            Data = new ProtoSymbol
            {
                Id          = entity.Id,
                Name        = entity.Code,
                Description = entity.Category ?? "",
                CategoryId  = entity.CategoryId,
                Type        = entity.Type,
            },
        };
    }

    public override async Task<DeleteSymbolResponse> DeleteSymbol(
        DeleteSymbolRequest request, ServerCallContext context)
    {
        var entity = await db.Symbols.FindAsync(request.Id);
        if (entity == null) throw new RpcException(new Status(StatusCode.NotFound, "Symbol not found"));

        db.Symbols.Remove(entity);
        await db.SaveChangesAsync();
        return new DeleteSymbolResponse { Success = true };
    }

    public override async Task<BatchImportSymbolsResponse> BatchImportSymbols(
        BatchImportSymbolsRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        int imported = 0, skipped = 0;

        foreach (var sym in request.Symbols)
        {
            var exists = await db.Symbols.AnyAsync(x =>
                x.CategoryId == sym.CategoryId && x.Code == sym.Name && x.Type == sym.Type);

            if (exists) { skipped++; continue; }

            db.Symbols.Add(new Bacera.Gateway.Symbol
            {
                Code            = sym.Name,
                Category        = sym.Description,
                CategoryId      = sym.CategoryId,
                Type            = sym.Type,
                CreatedOn       = DateTime.UtcNow,
                OperatorPartyId = partyId,
            });
            imported++;
        }

        await db.SaveChangesAsync();
        return new BatchImportSymbolsResponse { Imported = imported, Skipped = skipped };
    }

    public override async Task<BatchDeleteSymbolsResponse> BatchDeleteSymbols(
        BatchDeleteSymbolsRequest request, ServerCallContext context)
    {
        var ids = request.Ids.ToHashSet();
        var symbols = await db.Symbols.Where(x => ids.Contains(x.Id)).ToListAsync();
        db.Symbols.RemoveRange(symbols);
        await db.SaveChangesAsync();
        return new BatchDeleteSymbolsResponse
        {
            Success = true,
            Message = $"Deleted {symbols.Count} symbols",
        };
    }

    // ─── Categories ───────────────────────────────────────────────────────────

    public override async Task<GetCategoriesResponse> GetCategories(
        GetCategoriesRequest request, ServerCallContext context)
    {
        var query = db.Symbols.AsQueryable();
        if (request.HasType) query = query.Where(x => x.Type == request.Type);

        var categories = await query
            .GroupBy(x => new { x.CategoryId, x.Category, x.Type })
            .Select(g => new { g.Key.CategoryId, g.Key.Category, g.Key.Type })
            .Distinct()
            .ToListAsync();

        var response = new GetCategoriesResponse();
        response.Categories.AddRange(categories.Select(c => new ProtoSymbolCategory
        {
            Id   = c.CategoryId,
            Name = c.Category ?? "",
            Type = c.Type,
        }));
        return response;
    }

    public override async Task<CreateCategoryResponse> CreateCategory(
        CreateCategoryRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var maxCategoryId = await db.Symbols
            .Where(x => x.Type == request.Type)
            .MaxAsync(x => (int?)x.CategoryId) ?? 0;
        var newCategoryId = maxCategoryId + 1;

        db.Symbols.Add(new Bacera.Gateway.Symbol
        {
            Code            = "default",
            Category        = request.Name,
            CategoryId      = newCategoryId,
            Type            = request.Type,
            CreatedOn       = DateTime.UtcNow,
            OperatorPartyId = partyId,
        });
        await db.SaveChangesAsync();

        return new CreateCategoryResponse
        {
            Data = new ProtoSymbolCategory
            {
                Id   = newCategoryId,
                Name = request.Name,
                Type = request.Type,
            },
        };
    }

    public override async Task<UpdateCategoryResponse> UpdateCategory(
        UpdateCategoryRequest request, ServerCallContext context)
    {
        var symbols = await db.Symbols
            .Where(x => x.Type == request.Model.Type && x.CategoryId == request.CategoryId)
            .ToListAsync();

        if (!symbols.Any())
            throw new RpcException(new Status(StatusCode.NotFound, "Category not found"));

        foreach (var s in symbols) s.Category = request.Model.Name;
        await db.SaveChangesAsync();

        return new UpdateCategoryResponse
        {
            Data = new ProtoSymbolCategory
            {
                Id   = request.CategoryId,
                Name = request.Model.Name,
                Type = request.Model.Type,
            },
        };
    }

    public override async Task<DeleteCategoryResponse> DeleteCategory(
        DeleteCategoryRequest request, ServerCallContext context)
    {
        var symbols = await db.Symbols
            .Where(x => x.CategoryId == request.CategoryId && x.Type == request.Type)
            .ToListAsync();

        if (!symbols.Any())
            throw new RpcException(new Status(StatusCode.NotFound, "Category not found"));

        db.Symbols.RemoveRange(symbols);
        await db.SaveChangesAsync();
        return new DeleteCategoryResponse { Success = true };
    }

    private static PaginationMeta BuildMeta(int page, int size, int total)
        => new PaginationMeta
        {
            Page      = page,
            Size      = size,
            Total     = total,
            PageCount = total > 0 ? (int)Math.Ceiling((double)total / size) : 0,
            HasMore   = page * size < total,
        };

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }
}

/// <summary>
/// gRPC JSON Transcoding implementation of TenantExchangeRateService.
/// Replaces Areas/Tenant/Controllers/ExchangeRateController.cs.
/// Routes defined via google.api.http annotations in symbol.proto.
/// </summary>
public class TenantExchangeRateGrpcService(TenantDbContext db, TradingService tradingSvc)
    : TenantExchangeRateService.TenantExchangeRateServiceBase
{
    public override async Task<ListExchangeRatesResponse> ListExchangeRates(
        ListExchangeRatesRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.ExchangeRate.Criteria
        {
            Page = request.Pagination?.Page > 0 ? request.Pagination.Page : 1,
            Size = request.Pagination?.Size > 0 ? request.Pagination.Size : 20,
        };

        var items = await db.ExchangeRates
            .PagedFilterBy(criteria)
            .ToClientResponseModels()
            .ToListAsync();

        var response = new ListExchangeRatesResponse
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
        response.Data.AddRange(items.Select(MapToProto));
        return response;
    }

    public override async Task<GetExchangeRateResponse> GetExchangeRate(
        GetExchangeRateRequest request, ServerCallContext context)
    {
        var item = await db.ExchangeRates
            .Where(x => x.Id == request.Id)
            .ToClientResponseModels()
            .FirstOrDefaultAsync();

        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "ExchangeRate not found"));
        return new GetExchangeRateResponse { Data = MapToProto(item) };
    }

    public override async Task<GetExchangeRateHistoryResponse> GetExchangeRateHistory(
        GetExchangeRateHistoryRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Audit.Criteria
        {
            Type  = AuditTypes.ExchangeRate,
            RowId = request.Id,
            Page  = 1,
            Size  = 50,
        };

        var items = await db.Audits
            .PagedFilterBy(criteria)
            .ToTenantPageModel()
            .ToListAsync();

        var response = new GetExchangeRateHistoryResponse();
        response.Items.AddRange(items.Select(a =>
        {
            double rate = 0;
            try
            {
                var changes = JsonConvert.DeserializeObject<Audit.EntityChanges>(a.Data ?? "{}");
                if (changes?.CurrentValues.TryGetValue("BuyingRate", out var v) == true)
                    rate = Convert.ToDouble(v);
            }
            catch { /* malformed audit data — skip */ }

            return new ExchangeRateHistoryItem
            {
                Rate      = rate,
                UpdatedAt = a.CreatedOn.ToString("O"),
            };
        }));
        return response;
    }

    public override async Task<CreateExchangeRateResponse> CreateExchangeRate(
        CreateExchangeRateRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var fromId  = Enum.TryParse<CurrencyTypes>(request.FromCurrency, true, out var fc) ? fc : default;
        var toId    = Enum.TryParse<CurrencyTypes>(request.ToCurrency,   true, out var tc) ? tc : default;

        var spec = new Bacera.Gateway.ExchangeRate.CreateSpec
        {
            Name           = $"{request.FromCurrency}{request.ToCurrency}",
            FromCurrencyId = fromId,
            ToCurrencyId   = toId,
            BuyingRate     = (decimal)request.Rate,
            SellingRate    = (decimal)request.Rate,
            AdjustRate     = 0,
        };

        var result = await tradingSvc.ExchangeRateCreateAsync(spec);
        if (result.IsEmpty()) throw new RpcException(new Status(StatusCode.Internal, "Create failed"));
        return new CreateExchangeRateResponse { Data = MapToProto(result) };
    }

    public override async Task<UpdateExchangeRateResponse> UpdateExchangeRate(
        UpdateExchangeRateRequest request, ServerCallContext context)
    {
        var partyId = GetPartyId(context);
        var spec = request.Spec;

        var existing = await tradingSvc.ExchangeRateGetResponseModelAsync(request.Id);
        if (existing.IsEmpty()) throw new RpcException(new Status(StatusCode.NotFound, "ExchangeRate not found"));

        var updateSpec = new Bacera.Gateway.ExchangeRate.UpdateSpec
        {
            Id          = request.Id,
            Name        = $"{spec.FromCurrency}{spec.ToCurrency}",
            BuyingRate  = (decimal)spec.Rate,
            SellingRate = (decimal)spec.Rate,
            AdjustRate  = 0,
        };

        var result = await tradingSvc.ExchangeRateUpdateAsync(updateSpec, partyId);
        if (result.IsEmpty()) throw new RpcException(new Status(StatusCode.Internal, "Update failed"));
        return new UpdateExchangeRateResponse { Data = MapToProto(result) };
    }

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }

    private static ProtoExchangeRate MapToProto(Bacera.Gateway.ExchangeRate.ResponseModel r)
        => new ProtoExchangeRate
        {
            Id           = r.Id,
            FromCurrency = r.FromCurrencyCode ?? "",
            ToCurrency   = r.ToCurrencyCode   ?? "",
            Rate         = (double)r.BuyingRate,
            UpdatedAt    = r.UpdatedOn.ToString("O"),
        };
}
