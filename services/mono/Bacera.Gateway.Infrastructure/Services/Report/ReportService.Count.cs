using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services;

partial class ReportService
{
    /*
服务器端 新验证用户
服务器端 待审核 Client 账户
服务器端 待审核 转账 Transfer
服务器端 待审核 入金 Deposit
服务器端 待审 出金 Withdraw
服务器端 待审核 IB 账户
服务器端 待审核 Sales 账户
     */
    public async Task<int> AwaitingApproveUserVerificationCountAsync()
        => await tenantDbContext.Verifications
            .Where(x => x.Type == (int)VerificationTypes.Verification)
            .Where(x => x.Status == (int)VerificationStatusTypes.AwaitingReview)
            .CountAsync();

    public async Task<int> AwaitingReviewAndUnderReviewKycVerificationCountAsync()
        => await tenantDbContext.Verifications
            .Where(x => x.Type == (int)VerificationTypes.KycForm)
            .Where(x => x.Status == (int)VerificationStatusTypes.AwaitingReview
                        || x.Status == (int)VerificationStatusTypes.UnderReview
            )
            .CountAsync();

    public async Task<int> AwaitingApproveKycVerificationCountAsync()
        => await tenantDbContext.Verifications
            .Where(x => x.Type == (int)VerificationTypes.KycForm)
            .Where(x => x.Status == (int)VerificationStatusTypes.AwaitingApprove)
            .CountAsync();

    public async Task<int> AwaitingApproveAccountApplicationCountAsync()
        => await tenantDbContext.Applications
            .Where(x => x.Type == (int)ApplicationTypes.Account
                        || x.Type == (int)ApplicationTypes.IbAccount
                        || x.Type == (int)ApplicationTypes.SalesAccount
                        || x.Type == (int)ApplicationTypes.TradeAccount
            )
            .Where(x => x.Status == (int)ApplicationStatusTypes.AwaitingApproval)
            .CountAsync();

    public async Task<int> AwaitingApproveTransferCountAsync()
        => await tenantDbContext.Matters
            .Where(x => x.Type == (int)MatterTypes.InternalTransfer)
            .Where(x => x.StateId == (int)StateTypes.TransferAwaitingApproval)
            .CountAsync();

    public async Task<int> AwaitingApproveDepositCountAsync()
        => await tenantDbContext.Matters
            .Where(x => x.Type == (int)MatterTypes.Deposit)
            .Where(x => x.StateId == (int)StateTypes.DepositPaymentCompleted
                        || x.StateId == (int)StateTypes.DepositCreated
                        || x.StateId == (int)StateTypes.DepositCallbackCompleted
            )
            .CountAsync();

    public async Task<int> AwaitingApproveWithdrawalCountAsync()
        => await tenantDbContext.Matters
            .Where(x => x.Type == (int)MatterTypes.Withdrawal)
            .Where(x => x.StateId == (int)StateTypes.WithdrawalCreated
                        || x.StateId == (int)StateTypes.WithdrawalTenantApproved
            )
            .CountAsync();

    public async Task<int> AwaitingApproveWholesaleApplicationCountAsync()
        => await tenantDbContext.Applications
            .Where(x => x.Type == (int)ApplicationTypes.WholesaleAccount)
            .Where(x => x.Status == (int)ApplicationStatusTypes.AwaitingApproval)
            .CountAsync();

    public async Task<int> AwaitingApproveChangeLeverageApplicationCountAsync()
        => await tenantDbContext.Applications
            .Where(x => x.Type == (int)ApplicationTypes.TradeAccountChangeLeverage)
            .Where(x => x.Status == (int)ApplicationStatusTypes.AwaitingApproval)
            .CountAsync();

    public async Task<int> AwaitingApproveChangePasswordApplicationCountAsync()
        => await tenantDbContext.Applications
            .Where(x => x.Type == (int)ApplicationTypes.TradeAccountChangePassword)
            .Where(x => x.Status == (int)ApplicationStatusTypes.AwaitingApproval)
            .CountAsync();

    public async Task<int> AwaitingAutoCreatedAccountCountAsync()
        => await tenantDbContext.Accounts
            .Where(x => x.Status == 0 && x.Tags.Any(y => y.Name == AccountTagTypes.AutoCreate))
            .CountAsync();
}