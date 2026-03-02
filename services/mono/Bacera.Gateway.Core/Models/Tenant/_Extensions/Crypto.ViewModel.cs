using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Bacera.Gateway.Crypto;

public partial class Crypto
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public string Address { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public short Status { get; set; }
        public long OperatorPartyId { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public TenantUserBasicModel Operator { get; set; } = null!;

        public InUsePaymentModel? InUsePayment { get; set; }
    }

    public sealed class InUsePaymentModel
    {
        public long Id { get; set; }
        public long Amount { get; set; }
        public string PaymentNumber { get; set; } = null!;
        public TenantUserBasicModel User { get; set; } = null!;
    }
}

public static class CryptoViewModelExt
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q, bool hideEmail = false) => q
        .Include(x => x.OperatorParty.PartyComments)
        .Include(x => x.OperatorParty.PartyTags)
        .Include(x => x.InUsePayment!.Party.PartyComments)
        .Include(x => x.InUsePayment!.Party.PartyTags)
        .Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            Address = x.Address,
            Name = x.Name,
            Type = x.Type,
            Status = x.Status,
            OperatorPartyId = x.OperatorPartyId,
            IsDeleted = x.IsDeleted,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            Operator = x.OperatorParty.ToTenantBasicViewModel(hideEmail),
            InUsePayment = x.InUsePayment == null
                ? null
                : new M.InUsePaymentModel
                {
                    Id = x.InUsePayment.Id,
                    PaymentNumber = x.InUsePayment.Number,
                    Amount = x.InUsePayment.Amount,
                    User = x.InUsePayment.Party.ToTenantBasicViewModel(hideEmail)
                }
        });
}