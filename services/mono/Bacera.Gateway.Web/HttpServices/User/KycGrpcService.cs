using Bacera.Gateway.ViewModels.Tenant;
using Grpc.Core;
using Http.V1;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtoKycForm = Http.V1.KycForm;

namespace Bacera.Gateway.Web.HttpServices.User;

/// <summary>
/// gRPC JSON Transcoding implementation of TenantKycService.
/// Replaces Areas/Tenant/Controllers/KycController.cs.
/// Routes defined via google.api.http annotations in user.proto.
/// </summary>
public class TenantKycGrpcService(
    TenantDbContext tenantDb,
    AuthDbContext authDb)
    : TenantKycService.TenantKycServiceBase
{
    public override async Task<ListKycsResponse> ListKycs(ListKycsRequest request, ServerCallContext context)
    {
        var criteria = new Bacera.Gateway.Verification.Criteria
        {
            Page   = request.Pagination?.Page > 0 ? request.Pagination.Page : request.HasPage && request.Page > 0 ? request.Page : 1,
            Size   = request.Pagination?.Size > 0 ? request.Pagination.Size : request.HasSize && request.Size > 0 ? request.Size : 20,
            Type   = VerificationTypes.KycForm,
            Status = request.HasStatus ? (VerificationStatusTypes?)request.Status : null,
        };

        var items = await tenantDb.Verifications
            .PagedFilterBy(criteria)
            .ToTenantViewModel(false)
            .ToListAsync();

        var response = new ListKycsResponse
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
        response.Data.AddRange(items.Select(v => new ProtoKycForm
        {
            Id      = v.Id,
            PartyId = v.PartyId,
            Status  = (int)v.Status,
        }));
        return response;
    }

    public override async Task<ProtoKycForm> GetKyc(GetKycRequest request, ServerCallContext context)
    {
        var item = await GetVerificationWithItem(request.PartyId);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "KYC not found"));
        return MapToProto(item);
    }

    public override async Task<ProtoKycForm> CreateKyc(CreateKycRequest request, ServerCallContext context)
    {
        var existing = await GetVerificationWithItem(request.PartyId);
        if (existing != null)
            throw new RpcException(new Status(StatusCode.AlreadyExists, "KYC record already exists"));

        var spec = JsonConvert.DeserializeObject<KycFormViewModel>(request.Spec)
            ?? new KycFormViewModel();
        spec.StaffPartyId = GetPartyId(context);

        var vItem = new Bacera.Gateway.VerificationItem
        {
            Content  = Utils.JsonSerializeObject(spec),
            Category = VerificationCategoryTypes.KycForm,
        };
        var form = new Bacera.Gateway.Verification
        {
            PartyId           = request.PartyId,
            Type              = (short)VerificationTypes.KycForm,
            Status            = (int)VerificationStatusTypes.AwaitingReview,
            Note              = string.Empty,
            VerificationItems = new List<Bacera.Gateway.VerificationItem> { vItem },
        };
        await tenantDb.Verifications.AddAsync(form);
        await tenantDb.SaveChangesAsync();
        return MapToProto(form);
    }

    public override async Task<ProtoKycForm> UpdateKyc(UpdateKycRequest request, ServerCallContext context)
    {
        var item = await GetVerificationWithItem(request.PartyId);
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "KYC not found"));

        var spec = JsonConvert.DeserializeObject<KycFormViewModel>(request.Spec)
            ?? new KycFormViewModel();
        spec.StaffPartyId = GetPartyId(context);

        var vItem = item.VerificationItems.FirstOrDefault();
        if (vItem == null) throw new RpcException(new Status(StatusCode.NotFound, "KYC form item not found"));

        vItem.Content   = Utils.JsonSerializeObject(spec);
        vItem.UpdatedOn = DateTime.UtcNow;
        item.UpdatedOn  = DateTime.UtcNow;
        item.Status     = (int)VerificationStatusTypes.UnderReview;

        tenantDb.Verifications.Update(item);
        await tenantDb.SaveChangesAsync();
        return MapToProto(item);
    }

    public override async Task<ProtoKycForm> AwaitingReview(GetKycRequest request, ServerCallContext context)
    {
        var item = await tenantDb.Verifications
            .Where(x => x.Type == (int)VerificationTypes.KycForm && x.PartyId == request.PartyId)
            .SingleOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "KYC not found"));

        item.Status    = (int)VerificationStatusTypes.AwaitingReview;
        item.UpdatedOn = DateTime.UtcNow;
        tenantDb.Verifications.Update(item);
        await tenantDb.SaveChangesAsync();
        return MapToProto(item);
    }

    public override async Task<ProtoKycForm> SignKyc(UpdateKycRequest request, ServerCallContext context)
    {
        var spec = JsonConvert.DeserializeObject<KycFormViewModel>(request.Spec)
            ?? new KycFormViewModel();
        return await HandleKycForm(request.PartyId, spec, VerificationStatusTypes.AwaitingApprove, context);
    }

    public override async Task<ProtoKycForm> FinalizeKyc(UpdateKycRequest request, ServerCallContext context)
    {
        var spec = JsonConvert.DeserializeObject<KycFormViewModel>(request.Spec)
            ?? new KycFormViewModel();
        var result = await HandleKycForm(request.PartyId, spec, VerificationStatusTypes.Approved, context);
        await RecordHistory(request.PartyId, spec);
        return result;
    }

    public override async Task<KycHistoryResponse> GetKycHistory(GetKycRequest request, ServerCallContext context)
    {
        var supplement = await tenantDb.Supplements
            .Where(x => x.Type == (int)SupplementTypes.KycFormHistory && x.RowId == request.PartyId)
            .SingleOrDefaultAsync();

        var response = new KycHistoryResponse();
        if (supplement == null) return response;

        var models = JsonConvert.DeserializeObject<List<KycFormViewModel>>(supplement.Data)
                     ?? new List<KycFormViewModel>();
        response.Items.AddRange(models.Select(m => new KycHistoryItem
        {
            Json = JsonConvert.SerializeObject(m),
        }));
        return response;
    }

    public override async Task<ProtoKycForm> RejectKyc(GetKycRequest request, ServerCallContext context)
    {
        var item = await tenantDb.Verifications
            .Where(x => x.Type == (int)VerificationTypes.KycForm && x.PartyId == request.PartyId)
            .SingleOrDefaultAsync();
        if (item == null) throw new RpcException(new Status(StatusCode.NotFound, "KYC not found"));

        item.Status    = (int)VerificationStatusTypes.Rejected;
        item.UpdatedOn = DateTime.UtcNow;
        tenantDb.Verifications.Update(item);
        await tenantDb.SaveChangesAsync();
        return MapToProto(item);
    }

    public override async Task<ComplianceSigResponse> GetComplianceSig(EmptyRequest request, ServerCallContext context)
    {
        // Returns the hardcoded compliance signature (matches original controller behaviour)
        return new ComplianceSigResponse { Signature = GetVicSignature() };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Bacera.Gateway.Verification?> GetVerificationWithItem(long partyId)
    {
        var item = await tenantDb.Verifications
            .Where(x => x.Type == (int)VerificationTypes.KycForm)
            .Include(x => x.VerificationItems)
            .SingleOrDefaultAsync(x => x.PartyId == partyId);
        if (item?.VerificationItems != null)
            item.VerificationItems = item.VerificationItems.OrderByDescending(x => x.Id).ToList();
        return item;
    }

    private async Task<ProtoKycForm> HandleKycForm(long partyId, KycFormViewModel spec,
        VerificationStatusTypes newStatus, ServerCallContext context)
    {
        var item = await GetVerificationWithItem(partyId);
        if (item == null || item.VerificationItems.Count == 0)
            throw new RpcException(new Status(StatusCode.NotFound, "KYC not found"));

        spec.StaffPartyId = GetPartyId(context);
        var vItem = item.VerificationItems.First();
        vItem.Content   = Utils.JsonSerializeObject(spec);
        vItem.UpdatedOn = DateTime.UtcNow;
        item.Status     = (int)newStatus;
        item.UpdatedOn  = DateTime.UtcNow;

        tenantDb.Verifications.Update(item);
        await tenantDb.SaveChangesAsync();
        return MapToProto(item);
    }

    private async Task RecordHistory(long partyId, KycFormViewModel model)
    {
        const int keepRecord = 100;
        var supplementHistory = await tenantDb.Supplements
                                    .Where(x => x.Type == (int)SupplementTypes.KycFormHistory && x.RowId == partyId)
                                    .SingleOrDefaultAsync()
                                ?? Bacera.Gateway.Supplement.Build(SupplementTypes.KycFormHistory, partyId, "[]");

        var history = JsonConvert.DeserializeObject<List<KycFormViewModel>>(supplementHistory.Data)
                      ?? new List<KycFormViewModel>();
        history.Add(model);
        history = history.TakeLast(keepRecord).ToList();
        supplementHistory.Data      = Utils.JsonSerializeObject(history);
        supplementHistory.UpdatedOn = DateTime.UtcNow;
        tenantDb.Supplements.Update(supplementHistory);
        await tenantDb.SaveChangesAsync();
    }

    private static ProtoKycForm MapToProto(Bacera.Gateway.Verification v)
    {
        var formData = v.VerificationItems?.FirstOrDefault()?.Content ?? "";
        return new ProtoKycForm
        {
            Id       = v.Id,
            PartyId  = v.PartyId,
            Status   = v.Status,
            FormData = formData,
        };
    }

    private static long GetPartyId(ServerCallContext ctx)
    {
        var httpCtx = ctx.GetHttpContext();
        return httpCtx.Items.TryGetValue("PartyId", out var v) && v is long id ? id : 0;
    }

    private static string GetVicSignature() =>
        "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD/4gHYSUNDX1BST0ZJTEUAAQEAAAHIAAAAAAQwAABtbnRyUkdCIFhZWiAH4AABAAEAAAAAAABhY3NwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAA9tYAAQAAAADTLQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAlkZXNjAAAA8AAAACRyWFlaAAABFAAAABRnWFlaAAABKAAAABRiWFlaAAABPAAAABR3dHB0AAABUAAAABRyVFJDAAABZAAAAChnVFJDAAABZAAAAChiVFJDAAABZAAAAChjcHJ0AAABjAAAADxtbHVjAAAAAAAAAAEAAAAMZW5VUwAAAAgAAAAcAHMAUgBHAEJYWVogAAAAAAAAb6IAADj1AAADkFhZWiAAAAAAAABimQAAt4UAABjaWFlaIAAAAAAAACSgAAAPhAAAts9YWVogAAAAAAAA9tYAAQAAAADTLXBhcmEAAAAAAAQAAAACZmYAAPKnAAANWQAAE9AAAApbAAAAAAAAAABtbHVjAAAAAAAAAAEAAAAMZW5VUwAAACAAAAAcAEcAbwBvAGcAbABlACAASQBuAGMALgAgADIAMAAxADb/2wBDAAMCAgICAgMCAgIDAwMDBAYEBAQEBAgGBgUGCQgKCgkICQkKDA8MCgsOCwkJDRENDg8QEBEQCgwSExIQEw8QEBD/2wBDAQMDAwQDBAgEBAgQCwkLEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBD/wAARCADIAlgDASIAAhEBAxEB/8QAHgABAAICAwEBAQAAAAAAAAAAAAcIBgkCAwUECgH/xABOEAABAwMDAgQDAwcGBxEAAAAAAQIDBAURBgcIEiEJEzFBFCJRFmGBFSMyUnGRoRhisbXBwhdCgpKipbIZJCYzNDU4Q0RTcnN0k5SV0f/EABQBAQAAAAAAAAAAAAAAAAAAAAD/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwDamAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADGtytxNLbS6Dvm5Gtq2SksenqR9bWyxxOlejG9sNY3u5yqqIifVU9PUba7iaW3a0HY9yNE1slXY9Q0jK2ilkidE9WO7Ycx3drkVFRU+qL6+oGSkT8s6y5W7i9u1cbRX1NDW0mi7xUQVNNK6KWJ7KSRyOa9qorVTHqi5JYIo5ZxrLxY3iYnqugtQf1fMA4m1lxuXF/aW43evqa6tq9F2aonqamV0ksr30cblc57lVXKufVVySuRRxM6f5LGznSmE+wOn/wCr4CVwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAefE8dPw43Zkkd0ounpWIv3uexqfxVD+8CJ2VHDjaaSN2UTTsLPxa5zV/iimJ+KDeXWbg/uKsaqklalso2qn0fcaZHf6COOXhhXWS7cHtt5Jkd10zLnSKq+6R3Kpa3H3dKNT8FAtORryaajuN267XYwuiL6i5/9BMSURnyez/Jq3Z6fX7DX7H/AMCYD5eJrVbxZ2dR3r9gdP8A9XwErEY8XWJFxn2kjTOGaFsDe/3W+Ek4AAAAAAEf74757ecd9v6jcvc641FHZaeohpM01M6eV8srulrWsb6+6rnCYav7CQClni7zxw8NLpG92Fnv9rjZ96+Yrv6GqBci0XW3321UV8tNSlRQ3Gnjq6aZEVEkikajmORFRFTLVRe6ZPrMX2snjqtsNIVUTupk1ht8jV+qLTsVDKAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAApv4tT3s4Waja1ez7vamu/Z8U1f6UQyDwvIPI4L7Zp04V6Xd6/fm7Vip/DB8/il2d924Qa/fExXyUElqrGon0bcadrl/BrnL+B7Hhs0jqLhFtbC5qorqGtm7/R9fUvT/aAswRPy1q0ouLO8FRlUVNCX1qKi4VFdQTNT+KoSwQbzkq/g+IO7k2cdWla2L/PZ0f3gMp40ReRxx2qh7/m9E2Nvdcr2oIUJJI+48RpDsBtnCiYRmjrK3H0xRREggAAAAAAoD40t4fRcX9N2qPqRblrWkR6+3RHR1jlT/O6P3KX+Nd/jZNcvHjRL/ZNaRov7Voar/wDALe8Vbu+/cZNprvL1eZU6JsrpFd6q/wCCiRy/vRSUyHOGzenibs+mMf8AAmzr++kjJjAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACDeclobfOIO7lE9uUj0rW1n4wM85P4xnLg/bG2niHtFSs9H6ToKn8Zo0lX+Lz3uVVI6u4wbvUbEy6bQl+a1P53wE2P44PC4QVja7iHtFM1co3SdBD+McSMX/ZAm8rn4iNZ8Dws3VmzjqtEcP/uVMLP7xYwqv4oNYtHwZ3Kc12HSttMKff1XWkRf4ZAnjZujdb9odD0DkwtNpu2Qr+1tLGn9hmB5WlKNLdpez29MYpqCnhTH82Nqf2HqgACnXiG82tacOk28+x2k7Lel1bUXFa38ppKvlwUnw2Uj8t7cPd8T6u6kTp9ALig4QytniZMxHo2RqORHsVjkRUz3aqIqL9yplCr3NjevkHot+k9n+NG3tdctZ7hSTU8WoZKNZaGyxM6UfI5yosbZE6+vqk+RjWKqtflEQJf3K5FbE7OufDubu1pfT1UxvWtHWXGNKtW9PVltOirK7t9Gr6p9UNXniac6OP8AyV2rsm2m0tdfbpX2rU8V3fcJrYtNRup46aphVGrK5svU500bkRY0TCOyqLhFzLRPgu6v1NfK3VHITfiCatuNS+srW6ep31M1VNI5zpZHVVSjMOVy5ysLsqq+nvz5v+Hvx143cR9Q6228sl3qtSW6utrPyxdrk+efypKlkT29DOiFM9aZVI8/hkDx+P8A4sep9Obd6M2a0XxKu2r63SthoLK19t1HI+ar+HgZF53kR0D1Z1qzq6cuxnGV9Sw1g5R+Izry1/EaV4I0VnfKuY59RagbSJE1UXHXTzuhlVUXGcYXsvZM9rc7VaPsehNu9PaZ0/ZqK201FbKWJYaSnZCxXtiaiuVrERMrjuvuZYBSqa3+LTrZWPW/bF7exYTqbTRVVVMiZd7SMqGK70z8yJhExhcn10Gw/iWTMxdedWnKRzWoifDaAt0/WuVVVXqgZjHZEx649E97lACodw4v85L3TQMufiL1kEjG5clDtrb6dOpcZTqinYrkTHZVT8EyeN/Is5krJ5v+6T6r6vXH2PZj93xuC3+qdWaY0PYavVGstQ26yWigYslTXXCpZBBE36ue9URP7ShG6fiqv1de6ja/hbtTe9xtVTo6OC6S0Mq0kaI7CzR0zE82ViJ/jSLE1uUVcomFDKNcbHciNpNFXXVm7vifXq06VpYkWsqnaOo6WZMKitZFMs75PMdjCNjTrcq4RFzhYp8Pf+V/vJuxLu/JvJrt2ydBX1KQx6urUq6i/fIsaRxxY6I0a5rXOczDGORWtV7utTJNrvD53m3/ANRUm7fiB7lXO/So/wCJo9EUtb001NlE+SVYVSKFO3eOnRM9ldJnqauwmyWOzaatFHp/T1qpLZbLfC2npKOkhbFDBE1MNYxjURGoieyAfcAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMf3D087V2gNTaUamVvVnrben7ZoXx/3iAfDRvn5d4TbbSPVUmoaevt8rF9WOgr6iNEX6fK1q/iWeKn+HXEzT+hd09tFd0u0PutqOzxxqmFSDzI5I3In6rvMcqL9ygWwKj+KvUsg4Pa6ic5EWoqrPG371S5U7v6GqW4KU+LVNJVcaLFpGJ6o/VmurPZkx79TZ5f6YUAuJprzF05almz5nwUHVn6+WmT0iP44t64t6mxxppRu0zNPIxrE878sJd0l7e3l+R5Xb1zkkAAaffG31za7tunt1t7SVHm1mnLLWXKrRsnU2L42aNrGKiL8r+mk6lRURel7F7oqG2y+6t0rpb4f7TamtVo+KVyQfH1sdP5qtRFd0dap1YRUVcemT87fMXcVm83LDcPWEdxgloq3UMlvoqpHIkS0dMraWB+UVU6VihY7qz3zn3wB+i6zytqLRQztfK5slNE9HS/pqitRcu7r3+vdf2qfYQbWc1OH2m42293IjQPl0zGxsbR3mKqa1qImERYlci4THopn2id7Nm9y6t1v263Z0dqirZH5r6ez32lrJWM7ZVzInucid09UAzQqL4rKKvCDW6p7VloVf/sact0VJ8VViu4O68d+pU2d3+s6ZP7QLUWP/AJlt/bH+9Yu3+Qh9xiWqdw9B7UaGi1buNq216cstJBEx9bcqlsMfWrPlY3q/SeuFwxuXLjsilMt0fFq26luDNCcXtBX3dHWNxlWkt2KKWnonSr2a5GY+Im79+hGMRU/x2+oF6r9qCw6VtFVqDU97oLPa6JnmVNdX1LKengblE6nyPVGtTKomVX3KPbjeJDf9xNV1O0fBXa2s3P1MzLJr9PC+O0UXdWrJ8ys6movZJJHxRquMK9FTOP6N4HcgeT1ZbNf+IBu5c62kialRRaFtEzKeGlcqOT88sKJDG/CplYkc9yLh0qYwXl202p242c0zFo7a/Rts03Z4ndfw1DCjPMeqYV8j1y6R6oiIr3qrlwnfsBSGy+Gzu7vlUUmqOcHJPUeqJfOWsTTFlqOihpJHJ3a17k8tnbCKkMDPRUR65yXR2i2P2n2I023Se02hrZp23/Ks3w0eZqlyZw+eZ2ZJnd1+Z7lVE7JhOxnIAAAAAUz5+87dS8Tr/oPR23WlbLqfUGp3TVVdQVqzulipGvZHCkbIlReuaRZWtcvVhYXJ0uz2C5gNYFn8buzU6LR62453WgroE6Jm0d+ZJ+cTGfklgYrE/SXCqqphE75ylitmvFD4k7xXJlkXVtfoq5Su6YYNWU8dFHMv3VDJJIG/cj5GqvsigW1B1UtVS11NFWUVTFUU8zEkiliej2PavdFa5Oyov1Q7QAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFUeP6ro7nByQ0A/5KbUMOntY2+P0z10zoap+PfM3T3+4tcVQ3Tcm23iEbQa9eqRUO5ekrvoOrkzhjZ6Z7a6m6v5z3L0N9+2ALXlFfEVu32t3m4x7EUcjXTXvX9Nf6xuMrFT0skcaPVPorJqhU/8ALUvUa5NU1791PGW0vaInefRbYaZc2dre7WPWhnnR30yktwhRV/monsBsbAKRaa8RdNWc9HcWbPY7I/RyVFXZkvsk72VT7pTU0kkiNVXeWsazROga1Ey53S5Hd0YBkXiLbEcf9YbTXjfTeahuc1Xt9Y6ptsSkuL6dk88zmtp4HomUXqqHRNRUwvz91VMImmil2ZSDaPRO6F6+Mih1nq+qsNM1iNVH0tMyn82Vv85XzqxM+8TvT32XeNPvKtg2x0jshbZlSo1ZXPu9yRuflpKTCRMXvhUfNIjk7L3g9u2Yq8QjbS2cc+N3F3b2to46ibSlVVSXBjH4WaoVKeesRuMph0z39+rtluMgTk7wUeNC1rZG7j7mpSdPzRLXUCyK7v3R/wAJhE9O3Sv7SWeO/hrceuNW4dFuhoy5axut/tsc8dJNeblDIyFJonRPVGQQxI5eh70TqzjqVfVEVKE37fDnR4l2ravRu0lurNK6Darqeqp6GqkpbbBC5EylwrURHVDlT/qmouU/Ri9VWMeV3CfcfhdYtHa71LuhQ3y5XS7SR00NugqEZSyRNbKsnnSYyqux26UVcKvsuA35FTvFP/6DO4fr/wAbZv61pCyW314vGotBaa1BqGh+CutztFHWV1N/3FRJCx8kf+S5VT8CtnioJngzuEv0ms39a0oEp8keM+33Kva+m213Eq7tSUVNWQ3SkqrXOyOeCqjikja5OtjmuTpleitc1co7thURU106m8Krlpshf5dQcWt73VcCovQtNdZ7FcsL6sd0OWKRvZEVVkTq92ohtvosfBwYTCeU3t9OyHcBp2qtfeMpsFEj7/aNWX23U6eZIj7XQ6hYrE7ZfNTJLM1MJlfnbj1UyzRPjTa60/Xw2jfXj/TpJ1IlTPZaqahmiblUVzaWpR/WuUxhZW90XubXTorKGhuMPw1wo4KqLKO8uaNHtynouF7AU50j4t/DDUsaPvGqtRaVcqKvRd7BPIucp2zSeenfP1x2U8XlF4luz1p496g1Jxs3fsd11w6ako7ZC+mck0HmSp5k/wANUsar0bE2TurVajlZ1IqdltXqHYLYvVsb4tUbMaGu7ZHOe743T1JMvU5cudl0ar1KvfPrnuUc8Srifxh2t4v6k3E0RtHYtPanjrbdTUFXQeZAjFkqo/MRImu8tcxJIndvZO6LlEAtFwT3n1Tv7xc0buVru7UVw1NXJXU91kpY2RJ5kFbPCxXxs+WN7oo43qiIifNlERFRCfTR34enPCDjlUWnZ2o2xtNRbNW6mpm3W/tuM8VTGyZ7IUlfG9XxL5TVzhiRorW4X5svN4gHVU1NNRU0tZWVEUFPAx0sssr0ayNjUy5znL2RERFVVU0g8lucNHyP5RbX632l0XU2Sv0HqKKls1xqq5XreIXVkToVlgRjfIRXNflnW/qbMrVxjvsM8TTkfQbDcb7tYaKu8vVO4dPU2CzxMerZGwvYjaupRUTt5cUiIioqKj5Y8Gka8aU1lslqXR18vtGlLX1lDb9W2+PK9SU8kivgV3bsqpGjse2U9+wH6T9S7d7f60z9sdC6evuU6V/Kdsgqu3bt+cavbsn7itW9nhf8T93qSea0aJZoG9Pb+auGmESliavsjqT/AJOrfr0sa5f1kLV2i6Ud7tVFerfJ10tfTx1UDv1o3tRzV/cqH1gagl8PDxFthJq2j2A3qkrLHDUOfR01m1RPbFqWdSq18tJMradr+6qrVe9EVy4c71Mu0ZzG8TXYp0Fj3v4v3zX9to29EtbHZZ46p/ojUWuomSUy9vdYnOd6q5e+dp4ApdtD4rvGjXr22bcaS7bX6hZ0smo7/TufTda+zKmNFRERMKqzMi9fRfUt1pfV2k9cWeLUOi9T2m/2qfPlV1rrYqqnkx+rJG5Wr+CmIbr8ddjd8YPJ3X2u0/qOVI/KZV1NKjauJn6sdSzpmYn/AIXoVYv3haWjRN0k1bxM341ttNe1TvAyskq6GZqd0jciOZL0quM9bpU7J8qgXtBQ6S8+LZs29090se2+9dqp1R8zqNGUVdJEi5VI2t+GRH4XGEjk9OyKvr6tn8UfROlrrT6Z5M7I7h7P3aWTy3S3G2vq6BqfrpK1rJXt7dlZC5Md8gXcBhO3O920G7tDHcNsty9OakjkTPRb7hHJMztnD4s+ZGuO+HNRcexmwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACrHiO0U9p2Aot5LSxFvW0mq7LrG398dboqtkMkaqiL8isncrk9F6Uz6FpyPuQO2Dt6Nkdb7VxVMNPUamslVQUs0yL5cVS5irC92EVelsiMVcd8IuAM9gnhqoI6mnkbJFKxHse1co5qplFRfpg1neFnXpu9ya5Gb/AGoI1ZfZqqClhiX5vh6atqqmRYkd9GpQwMT7mGd0/PrTeh+Ft/8Athd6bT+8GhaSfQtTp99U34198p4/h2VETO7nRZRJlfhWtVr25VUTOT+FJsFe9muOs2p9YWmWh1DuDXpeZI529MzKBI2tpGvTKrlUWWXC4VPOwqIqATDy25TaC4wbW3nUd51FbG6pkoZPs/ZH1DVqq6pdlkbmw93LE1/d78dKI1UzlURdH1n47cttD23T/Iy37Q6zbTQXOO60Vy/J00szZIvLqGVUsbU81sLupqpM5EY5UciOyb0NZ8RdgNxd46PfTXWhIb7qi30cNJTrXTyS0jPKero5Fp1Xy3PblURVRU98Z7kxgaeeMHHnf/m7yjpuUPJPTNzt2j6SpZdmLUwPpIKvyVzR0FDFJl60zXo1XORFRzWSIr1kk6lkLxyIZXWDZ+oSRqRsrL2xzOlcq5zKNUVF9EROle3vlPoptEKveIpxs1Pyc49SaT0HQ01Xqqy3WnvFqhmmjh89Wo+KWJJZMNb1RyuXu5qKrG5UDh4ZF2tl34S7czW23UtCtPFX0lRFTomHTRV07HSOwifPJhJF98v9V9SxupdH6S1pRw27WOlrRfaWnqI6qGC50MVVHHOxepkjWyNVEe1URUcndFTKEM8FNmNUbA8XtH7aa4oWUeoqJa2pucDJ2TNjlmq5ZWtR7FVq4jdGnZVTKKT6AKqeKLF5vBnchM4w6zL/AK3oy1ZVrxPUReDm5KKuMusqeiL63ii+oFo429DGs/VREOQAAAAClvi61FLDwzu0dQrEfPfbXHB1ImVf5quXH39LXfhkukV05p8Wb5yw0vo3RtHrSlslosmpoLzd6eopXTJWwMY9itb0qmHo2SRERflXr7qmEAotzE4Q2DRPAbbPcq0U0Vv1Jt5ZqL8vtbH81a25TsfMjnfpK6KqqV6c9kY56dkRqJcLiVy22+r+GGjtz91tybPR1lktbrZeZK24RpUuqaRzok6mud1Pmljjjkx6u81F9yzeq9I6Z1zpi5aL1dZKW62O70z6OtoahnVFNC5MK1U9vuVMKioioqKiKa8J/BJ2sfrlt2p95tRRaT+J811ldbYnVfk5RViSt60RPdOpYVXGPVe6hDukbNqXxT+aFVrPUNPcKbZ7RMrm0rVplYx1BFIixUqu/RSepVUfInUrmtc7HZrTzfGt0zJQcg9G6qRj/Ku+k2UvUqt6Vkp6qdXIiInV2bMzKuz6oiemE2t7JbFbYcedERbf7U6bjtFqbKtTMvWsk1VUK1rXTTSO7veqMamfREaiIiIiIUM8b7SEtZt7tjr2OnesdpvFfaZZWtTCLVQxyMRy+v8A2N2Pb1+7IXj4u3db/wAatqLy9/W+s0VZJZFzn84tFF1/6WSTyHeG8VVDxP2gjrIHQyt0XaPkcmF6fhY+lcfe3C/iTEAAAAAACtHOzcnfbbvbm1f4D9gaDdOW61zqe60ddaZrrDSwI3LVdRwObJJ1LlOrPS3p7ovUhZcAaBN07Hv5fny6juXh30ehqyGTz0u2ntG6ltTadyfN1Ma2r+HbjGc+X2x2x3OvbHnlzd2bucVlTX+pLpT07kbLatT0L7m5rEVV6cz/AJ9qImcI2RvZET0RMb/wBWHg9y21vyj05d5de7M3bRVxsjad/wAa6KVLdc2TLJhad0rWuRzUYnU3L0+ZF6u+Es8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEJ3LhZxcvW51x3hvezlmueq7rUx1tVV1r5qiJ9QzGJEp3vWFrl6WqqoxOpUyuVVVWbAAAAAAAAAABVXxRJfJ4MbkOzheuy4/al4ol/sLVEc8hdj9O8jdo73s7qu63G22u+upXT1NvcxKhnkVMU7elXtc3u6JEXKL2VQJFa5HNRyLlFTKH9OLGoxjWIqqjURMr6nIAAAAAAAAAePqrRukNdWpbDrfStn1DbFkbKtFdaGKrgV7f0XeXK1zcplcLjKZPYAHTSUlLQUsNBQU0VNTU0bYYYYWIxkbGphrWtTsiIiIiInZEQ7gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP//Z";
}
