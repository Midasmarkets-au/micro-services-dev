using Bacera.Gateway.Interfaces;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services.Report.Models;

public class AccountSearchRecord : ICanExportToCsv
{
    public string ClientName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public long AccountNumber { get; set; }
    public long Uid { get; set; }
    public int ServiceId { get; set; }
    public string Server { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    public string AgentGroup { get; set; } = string.Empty;
    public long AgentId { get; set; }
    public long WalletId { get; set; }
    public string ReferPath { get; set; } = string.Empty;
    public long AgentUid { get; set; }

    public string SalesName { get; set; } = string.Empty;
    public string SalesEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CCC { get; set; } = string.Empty;
    public long SalesId { get; set; }
    public long SalesUid { get; set; }
    public string SalesCode { get; set; } = string.Empty;

    public static string Header() =>
        "Name,Email,Group,Uid,AccountNumber,Server,AgentName,AgentEmail,AgentUid,AgentGroup,SalesName,SalesEmail,SalesUid,SalesCode,CCC,PhoneNumber,ReferPath,WalletId";


    public string ToCsv()
    {
        return
            $"{ClientName},{Email},{Group},{Uid},{AccountNumber},{Server},{AgentName},{AgentEmail},{AgentUid},{AgentGroup},{SalesName},{SalesEmail},{SalesUid},{SalesCode},{CCC},{PhoneNumber},{ReferPath},{WalletId}";
    }

    public long PartyId { get; set; }
    public int FundType { get; set; }
    public int CurrencyId { get; set; }
}

public static class AccountSearchRecordExtensions
{
    public static IQueryable<AccountSearchRecord> ToRecords(this IQueryable<Account> query) =>
        query.Select(x => new AccountSearchRecord
        {
            ClientName = x.Party.NativeName,
            Email = x.Party.Email,
            PartyId = x.PartyId,
            Group = x.TradeAccountStatus == null || x.TradeAccountStatus.Group == null ? "" : x.TradeAccountStatus.Group,
            AccountNumber = x.AccountNumber,
            Uid = x.Uid,
            ServiceId = x.ServiceId,
            AgentName = x.AgentAccount != null ? x.AgentAccount.Party.NativeName : "",
            AgentEmail = x.AgentAccount != null ? x.AgentAccount.Party.Email : "",

            AgentId = x.AgentAccountId != null ? x.AgentAccountId.Value : 0,
            AgentUid = x.AgentAccount != null ? x.AgentAccount.Uid : 0,
            AgentGroup = x.Group,

            SalesName = x.SalesAccount != null ? x.SalesAccount.Party.NativeName : "",
            SalesEmail = x.SalesAccount != null ? x.SalesAccount.Party.Email : "",
            SalesId = x.SalesAccountId != null ? x.SalesAccountId.Value : 0,
            SalesUid = x.SalesAccount != null ? x.SalesAccount.Uid : 0,
            SalesCode = x.Role == (short)AccountRoleTypes.Sales
                ? x.Code
                : x.SalesAccount != null
                    ? x.SalesAccount.Code
                    : "",
            CCC = x.Party.CCC,
            PhoneNumber = x.Party.PhoneNumber,
            ReferPath = x.ReferPath,
            FundType = x.FundType,
            CurrencyId = x.CurrencyId,
        });
}