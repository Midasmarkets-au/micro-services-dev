using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class Account
{
    public Account()
    {
        Code = string.Empty;
        Group = string.Empty;
    }

    public List<long> ReferPathUids => ReferPath.Split('.', StringSplitOptions.RemoveEmptyEntries)
        .Select(long.Parse)
        .ToList();

    public int GetLevelFromReferPath() => ReferPathUids.Count;

    public bool IsUpper(long uid) => ReferPathUids.IndexOf(uid) > -1;

    public bool IsTopLevelAgent() =>
        Role is (short)AccountRoleTypes.Agent && (AgentAccountId == Id || AgentAccountId == null);

    public bool IsEmpty() => Id == 0;
    public bool CanTransfer() => Permission.Length > 0 && Permission[0] == '1';
    public static Account Empty() => new();

    public static bool TryParse(string json, out Account account)
    {
        account = new Account();
        try
        {
            var acc = JsonConvert.DeserializeObject<Account>(json);
            if (acc == null) return false;
            account = acc;
            return true;
        }
        catch
        {
            return false;
        }
    }


    public static Account Build(long partyId
        , AccountRoleTypes roleType
        , AccountTypes accountType = AccountTypes.Standard
        , SiteTypes siteId = SiteTypes.Default)
        => new()
        {
            PartyId = partyId,
            Role = (short)roleType,
            Type = (short)accountType,
            ReferPath = "",
            Group = "",
            Code = "",
            SearchText = "",
            Name = Guid.NewGuid().ToString().ToUpper()[..10],
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            SiteId = (int)siteId
        };

    public async Task AssignUid(Func<long, Task<bool>> uidExistValidator)
    {
        Uid = await Utils.GenerateUniqueIdAsync(uidExistValidator);
    }


    public CentralAccount ToCentralAccount(long tenantId)
        => new()
        {
            Uid = Uid,
            AccountId = Id,
            AccountNumber = AccountNumber,
            ServiceId = ServiceId,
            TenantId = tenantId,
            CreatedOn = CreatedOn,
        };


    public static AccountLog BuildLog(long operatorPartyId, string action, string before, string after) => new()
    {
        OperatorPartyId = operatorPartyId,
        Action = action,
        Before = before,
        After = after,
        CreatedOn = DateTime.UtcNow,
    };

    public static AccountLog BuildLog(long accountId, long operatorPartyId, string action, string before, string after)
        => new()
        {
            AccountId = accountId,
            OperatorPartyId = operatorPartyId,
            Action = action,
            Before = before,
            After = after,
            CreatedOn = DateTime.UtcNow,
        };

    public EventShopPointTransaction.MQSource ToMQSource(long tenantId)
        => new()
        {
            SourceType = EventShopPointTransactionSourceTypes.OpenAccount,
            RowId = Id,
            TenantId = tenantId
        };


    public Account UpdateSearchText(string extra = "")
    {
        SearchText = new StringBuilder(extra)
            .Append($"{Id}")
            .Append($",{Uid}")
            .Append($",{AccountNumber}")
            .Append($",{PartyId}")
            .Append($",{Name}")
            .Append($",{Enum.GetName(typeof(AccountRoleTypes), Role)}")
            .Append($",{Code}")
            .Append($",{Group}")
            .Append($",{Party.Id}")
            .Append($",{Party.Uid}")
            .Append($",{Party.FirstName} {Party.LastName}")
            .Append($",{Party.LastName} {Party.FirstName}")
            .Append($",{Party.NativeName}")
            .Append($",{Party.LastLoginIp}")
            .Append($",{Party.RegisteredIp}")
            .Append($",{Party.Email}")
            .Append($",{Party.PhoneNumber}")
            .ToString();
        return this;
    }
}