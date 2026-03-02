using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = AccountStat;

public partial class AccountStat
{
    public static M BuildToday(long accountId, DateTime? today = null) => new()
    {
        AccountId = accountId,
        Date = today ?? DateTime.UtcNow.Date,
        TradeSymbol = JsonConvert.SerializeObject(new Dictionary<string, SymbolStatModel>()),
    };
}