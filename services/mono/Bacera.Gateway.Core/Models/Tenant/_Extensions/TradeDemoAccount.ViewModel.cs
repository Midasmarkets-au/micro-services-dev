using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = TradeDemoAccount;

partial class TradeDemoAccount
{
    public sealed class ClientPageModel
    {
        public long AccountNumber { get; set; }
        public CurrencyTypes CurrencyId { get; set; }
        public int ServiceId { get; set; }
        public AccountTypes Type { get; set; }
        public long Leverage { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime ExpireOn { get; set; }
        public long Balance => (long)(BalanceDouble * 100);

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public double BalanceDouble { get; set; }
    }
}

public static class TradeDemoAccountModelExt
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> q) => q.Select(x => new M.ClientPageModel
    {
        AccountNumber = x.AccountNumber,
        CurrencyId = (CurrencyTypes)x.CurrencyId,
        ServiceId = x.ServiceId,
        Type = (AccountTypes)x.Type,
        Leverage = x.Leverage,
        CreatedOn = x.CreatedOn,
        UpdatedOn = x.UpdatedOn,
        ExpireOn = x.ExpireOn,
        BalanceDouble = x.Balance,
        Id = x.Id
    });
}