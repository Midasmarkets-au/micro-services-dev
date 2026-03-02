using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Email.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.BackgroundJobs.GeneralJob;

public partial class GeneralJob
{
    private const string AccountListForOrderPlacedForSalesRazorString = """
                                                                        @foreach (var acc in Model.Accounts) {
                                                                        <tr>
                                                                          <td style="border: 1px solid #cccccc; text-align: left;">@acc.Uid</td>
                                                                          <td style="border: 1px solid #cccccc; text-align: left;">@acc.Group</td>
                                                                          <td style="border: 1px solid #cccccc; text-align: left;">@acc.Type</td>
                                                                        </tr>
                                                                        }
                                                                        """;


    public async Task<(bool, string)> UserEventShopOrderPlaced(long tenantId, long eventShopOrderId)
    {
        using var scope = CreateTenantScopeByTenantIdAsync(tenantId);
        var tenantCtx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var sales = await tenantCtx.EventShopOrders
            .Where(x => x.Id == eventShopOrderId)
            .SelectMany(x => x.EventParty.Party.Accounts)
            .OrderBy(x => x.Id)
            .Where(x => x.Status == 0 && x.SalesAccount != null)
            .Select(x => x.SalesAccount!)
            .Select(x => new { x.Party.Email, x.Party.Language })
            .FirstOrDefaultAsync();
        if (sales == null) return (true, " __NO_SALES_ACCOUNT_FOUND__");

        var item = await tenantCtx.EventShopOrders
            .Where(x => x.Id == eventShopOrderId)
            .Select(x => new
            {
                x.TotalPoint,
                x.CreatedOn,
                UserAccounts = new SalesBasicAccountModel
                {
                    Accounts = x.EventParty.Party.Accounts
                        // .Where(y => y.Role == 300 || y.Role == 400)
                        .Select(y => new SalesBasicAccountModel.BasicAccount
                        {
                            AccountNumber = y.AccountNumber,
                            AccountUid = y.Uid,
                            Group = y.Group,
                            AccountType = y.Type
                        }).ToList()
                },
                UserName = x.EventParty.Party.NativeName,
                UserEmail = x.EventParty.Party.Email,
                ItemName = x.EventShopItem.EventShopItemLanguages.Any(y => y.Language == sales.Language)
                    ? x.EventShopItem.EventShopItemLanguages
                        .Where(y => y.Language == sales.Language)
                        .Select(y => y.Name)
                        .First()
                    : x.EventShopItem.EventShopItemLanguages
                        .Where(y => y.Language == LanguageTypes.English)
                        .Select(y => y.Name)
                        .First()
            })
            .SingleOrDefaultAsync();
        if (item == null) return (true, " __NO_EVENT_SHOP_ORDER_FOUND__");

        var model = new EventShopOrderPlacedForSalesViewModel
        {
            Email = sales.Email,
            NativeName = item.UserName,
            UserEmail = item.UserEmail,
            ItemName = item.ItemName,
            ItemPoints = (item.TotalPoint / 100000000).ToString(),
            OrderDate = item.CreatedOn,
            AccountList = await SendMailService.CompileRazorTemplate(item.UserAccounts, AccountListForOrderPlacedForSalesRazorString)
        };

        var sendMailSvc = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        await sendMailSvc.SendEmailWithTemplateAsync(model, sales.Language);
        return (true, $"__EMAIL_SENT_TO__ {sales.Email}");
    }
}

public sealed class SalesBasicAccountModel : IRazorModel
{
    public List<BasicAccount> Accounts { get; set; } = [];

    public sealed class BasicAccount
    {
        public long AccountNumber { get; set; }
        public long AccountUid { get; set; }

        public long Uid => AccountNumber == 0 ? AccountUid : AccountNumber;
        public string Group { get; set; } = "";
        public string Type => Enum.GetName(typeof(AccountTypes), AccountType) ?? "";
        public short AccountType { get; set; }
    }
}