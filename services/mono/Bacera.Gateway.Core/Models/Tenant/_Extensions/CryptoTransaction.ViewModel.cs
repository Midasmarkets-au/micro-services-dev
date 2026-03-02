using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Vendor.TronProCrypto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = CryptoTransaction;

public partial class CryptoTransaction
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public long CryptoId { get; set; }
        public bool Confirmed { get; set; }
        public long? PaymentId { get; set; }
        public long Amount { get; set; }
        public CryptoTransactionStatusTypes Status { get; set; }

        public string FromAddress { get; set; } = null!;

        //    public string Data { get; set; } = null!;
        [JsonIgnore] public string DataRaw { get; set; } = null!;


        public DateTime CreatedOn
        {
            get
            {
                var data = Utils.JsonDeserializeObjectWithDefault<TransactionData>(DataRaw);
                var createdOn = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(data.BlockTimestamp);
                return createdOn;
            }
        }

        public DateTime SyncedOn { get; set; }

        public string TransactionHash { get; set; } = null!;

        public Crypto.InUsePaymentModel? InUsePayment { get; set; }

        public object Crypto { get; set; } = null!;
    }
}

public static class CryptoTransactionViewModelExt
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q, bool hideEmail = false) => q
        .Include(x => x.Payment!.Party.PartyComments)
        .Include(x => x.Payment!.Party.PartyTags)
        .Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            CryptoId = x.CryptoId,
            Confirmed = x.Confirmed,
            PaymentId = x.PaymentId,
            Amount = x.Amount,
            Status = (CryptoTransactionStatusTypes)x.Status,
            FromAddress = x.FromAddress,
            // CreatedOn = x.CreatedOn,
            SyncedOn = x.CreatedOn,
            TransactionHash = x.TransactionHash,
            DataRaw = x.Data,
            Crypto = new
            {
                x.Crypto.Name,
                x.Crypto.Type,
                x.Crypto.Address,
            },
            InUsePayment = x.Payment == null
                ? null
                : new Crypto.InUsePaymentModel
                {
                    Id = x.Payment.Id,
                    PaymentNumber = x.Payment.Number,
                    Amount = x.Payment.Amount,
                    User = x.Payment.Party.ToTenantBasicViewModel(hideEmail)
                }
        });
}