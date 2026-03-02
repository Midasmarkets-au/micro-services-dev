using Bacera.Gateway.Vendor.MetaTrader;
using Bacera.Gateway.Vendor.MetaTrader.Models;

namespace Bacera.Gateway;

public interface ITradingApiService
{
    /// <summary>
    /// Get Account Balance and Leverage; balance is in decimal 
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="accountNumber"></param>
    /// <returns></returns>
    public Task<Tuple<bool, int, double>> GetAccountBalanceAndLeverage(int serviceId, long accountNumber);

    Task<TradeAccount> CreateAccountAsync(int serviceId, string name, string password, int leverage,
        string group, long? accountNumber);

    Task<TradeAccount> CreateAccountAsync(int serviceId, CreateAccountRequest request);

    Task<TradeAccount> UpdateAccountAsync(int serviceId, UpdateAccountRequest request);
    Task<GetAccountInfoResult> GetAccountInfoAsync(int serviceId, long accountNumber);

    Task<bool> ChangeLeverageAsync(int serviceId, long accountNumber, int leverage);
    Task<bool> ChangePasswordAsync(int serviceId, long accountNumber, string password);
    Task<bool> ChangePasswordAsync(int serviceId, long accountNumber, string password, string passwordType);
    Task<bool> CheckPasswordAsync(int serviceId, long accountNumber, string password);
    Task<bool> CheckPasswordAsync(int serviceId, long accountNumber, string password, string passwordType);
    /// <summary>
    /// Important: Ensure the amount has been scaled with *10000 before calling this method.
    /// This method will convert back the scale of *10000
    /// </summary>
    Task<Tuple<bool, string>> ChangeBalance(int serviceId, long accountNumber, decimal amount, string comment = "");
    /// <summary>
    /// Important: Ensure the amount has been scaled with *10000 before calling this method.
    /// This method will convert back the scale of *10000
    /// </summary>
    Task<Tuple<bool, string>> ChangeCredit(int serviceId, long accountNumber, decimal amount, string comment = "");
    Task<bool> IsAccountNumberExists(int serviceId, long accountNumber);
    Task<long> GetAccountPrefix(int serviceId, string matchTerm);
    Task<string> GetGroupAndSymbols(int serviceId, string group, string symbol, int transId);
    Task<List<AccountDailyReportResponse>> GetDailyReport(int serviceId, long login, DateTime from, DateTime to);
    Task<IMetaTraderService> GetMetaTraderService(int id);
}