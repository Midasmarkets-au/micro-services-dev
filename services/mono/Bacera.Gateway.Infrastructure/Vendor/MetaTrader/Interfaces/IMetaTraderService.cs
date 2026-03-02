using Bacera.Gateway.Vendor.MetaTrader.Models;

namespace Bacera.Gateway.Vendor.MetaTrader;

public interface IMetaTraderService
{
    Task<ApiResult<GetAccountInfoResult>> GetAccountInfoAsync(long login);

    // Task<ApiResult<GetAccountBalanceInfoResult>> GetAccountBalanceInfoAsync(long login);
    Task<ApiResult<CreateAccountResponse>> CreateAccountAsync(CreateAccountRequest request);
    Task<ApiResult<UpdateAccountResponse>> UpdateAccountAsync(UpdateAccountRequest request);
    Task<ApiResult<ChangeBalanceResponse>> ChangeBalanceAsync(ChangeBalanceRequest request);
    Task<ApiResult<ChangeCreditResponse>> ChangeCreditAsync(ChangeCreditRequest request);
    Task<ApiResult<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordRequest request);
    Task<ApiResult<CheckPasswordResponse>> CheckPasswordAsync(CheckPasswordRequest request);
    Task<ApiResult<ChangeLeverageResponse>> ChangeLeverageAsync(ChangeLeverageRequest request);
    Task<string> GetGroupAndSymbols(string group, string symbol, int transId);
    Task<ApiResult<List<AccountDailyReportResponse>>> GetDailyReport(long login, DateTime from, DateTime to);
}