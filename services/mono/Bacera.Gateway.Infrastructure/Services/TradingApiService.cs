using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Vendor.MetaTrader;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services
{
    /// <summary>
    /// Initial Trading API Service
    /// MT4, MT5 etc.
    /// </summary>
    public class TradingApiService : ITradingApiService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<TradingApiService> _logger;

        private readonly TenantDbContext _tenantDbContext;

        private readonly Dictionary<int, TradeServiceOptions> _options = new();
        private readonly Dictionary<int, IMetaTraderService> _metaTraderServices = new();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MyDbContextPool _pool;

        public TradingApiService(TenantDbContext tenantDbContext, IHttpClientFactory clientFactory,
            MyDbContextPool pool, ILoggerFactory? loggerFactory = null)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger<TradingApiService>();
            _tenantDbContext = tenantDbContext;
            _httpClientFactory = clientFactory;
            _pool = pool;
        }

        public async Task<TradeAccount> CreateAccountAsync(int serviceId, CreateAccountRequest request)
        {
            var service = await GetMetaTraderService(serviceId);
            _logger.LogInformation("CreateAccountAsync: {ServiceId} {@Request}", serviceId, request);
            var result = await service.CreateAccountAsync(request);
            if (result.IsSuccessStatus())
                return new TradeAccount { AccountNumber = result.Data.Login, ServiceId = serviceId };

            BcrLog.Slack($"CreateAccountAsync_Failed_on_service: serviceId:{serviceId}, msg: {result.Message}, request: {request.ToJson()}");
            throw new Exception(result.Message);
        }

        public async Task<TradeAccount> UpdateAccountAsync(int serviceId, UpdateAccountRequest request)
        {
            var service = await GetMetaTraderService(serviceId);
            _logger.LogInformation("UpdateAccountAsync: {ServiceId} {@Request}", serviceId, request);
            var result = await service.UpdateAccountAsync(request);
            if (result.IsSuccessStatus())
                return new TradeAccount { AccountNumber = result.Data.Login, ServiceId = serviceId };

            BcrLog.Slack($"UpdateAccountAsync_Failed_on_service : {serviceId} {request} {result.Message}");
            throw new Exception(result.Message);
        }

        public async Task<TradeAccount> CreateAccountAsync(int serviceId, string name, string password, int leverage,
            string group, long? accountNumber)
        {
            var service = await GetMetaTraderService(serviceId);
            
            // Fix double-escaping issue: Ensure group name is unescaped before creating request
            var unescapedGroup = group?.Replace("\\\\", "\\") ?? string.Empty;
            
            var request = new CreateAccountRequest
            {
                Name = Utils.ToUnicode(name),
                Password = password,
                PasswordInvestor = password,
                Leverage = leverage,
                Group = unescapedGroup,
            };

            if (accountNumber.HasValue)
                request.Login = accountNumber.Value;

            var result = await service.CreateAccountAsync(request);

            if (result.IsSuccessStatus())
            {
                return new TradeAccount { AccountNumber = result.Data.Login, ServiceId = serviceId };
            }

            BcrLog.Slack($"CreateAccountAsync_Failed_on_service: serviceId:{serviceId}, msg: {result.Message}, request: {request.ToJson()}");
            throw new Exception(result.Message);
        }

        public async Task<Tuple<bool, int, double>> GetAccountBalanceAndLeverage(int serviceId, long accountNumber)
        {
            var service = await GetMetaTraderService(serviceId);
            var result = await service.GetAccountInfoAsync(accountNumber);
            if (result.IsSuccessStatus()) return Tuple.Create(true, result.Data.Leverage, (double)result.Data.Balance);
            _logger.LogWarning(
                "Trade Account Get info error Service ID : {ServiceId} Login {AccountNumber} error: {Error}", serviceId,
                accountNumber, result.Message);

            var platform = _pool.GetPlatformByServiceId(serviceId);
            if (platform == PlatformTypes.MetaTrader5)
            {
                await using var mt5Ctx = _pool.CreateCentralMT5DbContextAsync(serviceId);
                var account = await mt5Ctx.Mt5Users
                    .Where(x => x.Login == (ulong)accountNumber)
                    .Select(x => new { x.Leverage, x.Balance })
                    .FirstOrDefaultAsync();
                if (account != null)
                {
                    return Tuple.Create(true, (int)account.Leverage, (double)account.Balance);
                }
            }
            
            throw new Exception("__GET_ACCOUNT_BALANCE_LEVERAGE_INFO_ERROR__");
        }

        public async Task<GetAccountInfoResult> GetAccountInfoAsync(int serviceId, long accountNumber)
        {
            var service = await GetMetaTraderService(serviceId);
            var result = await service.GetAccountInfoAsync(accountNumber);
            if (result.IsSuccessStatus()) return result.Data;
            throw new Exception("__GET_ACCOUNT_INFO_ERROR__");
        }

        public async Task<bool> IsAccountNumberExists(int serviceId, long accountNumber)
        {
            var service = await GetMetaTraderService(serviceId);
            var result = await service.GetAccountInfoAsync(accountNumber);
            return result.IsSuccessStatus();
        }

        public async Task<long> GetAccountPrefix(int serviceId, string matchTerm)
        {
            const long defaultPrefix = 0L;
            var options = await GetOptions(serviceId);
            if (!options.AccountPrefixes.Any())
                return defaultPrefix;

            var prefix = options.AccountPrefixes
                .Where(x => string.Equals(x.Key, matchTerm, StringComparison.CurrentCultureIgnoreCase))
                .Select(x => x.Value)
                .FirstOrDefault();

            if (prefix > 0) return prefix;

            return options.AccountPrefixes
                .Where(x => string.Equals(x.Key, "DEFAULT", StringComparison.CurrentCultureIgnoreCase))
                .Select(x => x.Value)
                .FirstOrDefault();
        }

        public async Task<bool> ChangeLeverageAsync(int serviceId, long accountNumber, int leverage)
        {
            var service = await GetMetaTraderService(serviceId);
            var result = await service.ChangeLeverageAsync(new ChangeLeverageRequest
            {
                Login = accountNumber,
                Leverage = leverage,
            });
            return result.IsSuccessStatus();
        }

    public async Task<bool> ChangePasswordAsync(int serviceId, long accountNumber, string password)
    {
        var service = await GetMetaTraderService(serviceId);
        var result = await service.ChangePasswordAsync(
            ChangePasswordRequest.Build(accountNumber, password));
        return result.IsSuccessStatus();
    }

    public async Task<bool> ChangePasswordAsync(int serviceId, long accountNumber, string password, string passwordType)
    {
        var service = await GetMetaTraderService(serviceId);
        var result = await service.ChangePasswordAsync(
            ChangePasswordRequest.Build(accountNumber, password, passwordType));
        return result.IsSuccessStatus();
    }

    public async Task<bool> CheckPasswordAsync(int serviceId, long accountNumber, string password)
    {
        var service = await GetMetaTraderService(serviceId);
        var result = await service.CheckPasswordAsync(CheckPasswordRequest.Build(accountNumber, password));
        return result.IsSuccessStatus();
    }

    public async Task<bool> CheckPasswordAsync(int serviceId, long accountNumber, string password, string passwordType)
    {
        var service = await GetMetaTraderService(serviceId);
        var result = await service.CheckPasswordAsync(CheckPasswordRequest.Build(accountNumber, password, passwordType == PasswordTypes.Investor));
        return result.IsSuccessStatus();
    }

        public async Task<Tuple<bool, string>> ChangeBalance(int serviceId, long accountNumber, decimal amount,
            string comment = "")
        {
            var service = await GetMetaTraderService(serviceId);

            // *** Convert back of *10000 in a central place before calling MT5 *** //
            amount = amount.ToCentsFromScaled();

            var result = await service.ChangeBalanceAsync(ChangeBalanceRequest.Build(accountNumber, amount, comment));
            return Tuple.Create(result.IsSuccessStatus(), result.Data.OrderId.ToString());
        }

        public async Task<Tuple<bool, string>> ChangeCredit(int serviceId, long accountNumber, decimal amount,
            string comment = "")
        {
            var service = await GetMetaTraderService(serviceId);

            // *** Convert back of *10000 in a central place before calling MT5 *** //
            amount = amount.ToCentsFromScaled();

            var result = await service.ChangeCreditAsync(ChangeCreditRequest.Build(accountNumber, amount, comment));
            return Tuple.Create(result.IsSuccessStatus(), result.Data.OrderId.ToString());
        }

        public async Task<string> GetGroupAndSymbols(int serviceId, string group, string symbol, int transId)
        {
            // var response = await 
            serviceId = 30;
            var service = await GetMetaTraderService(serviceId);
            var result = await service.GetGroupAndSymbols(group, symbol, transId);
            return result;
        }

        public async Task<List<AccountDailyReportResponse>> GetDailyReport(int serviceId, long login, DateTime from,
            DateTime to)
        {
            // var response = await 
            var service = await GetMetaTraderService(serviceId);
            var result = await service.GetDailyReport(login, from, to);
            return result.Data;
        }

        private async Task<TradeServiceOptions> GetOptions(int tradeServiceId)
        {
            if (_options.TryGetValue(tradeServiceId, out var value))
            {
                return value;
            }

            var item = await _tenantDbContext.TradeServices
                .FirstAsync(x => x.Id == tradeServiceId);

            if (item.Platform is not ((short)PlatformTypes.MetaTrader4 or (short)PlatformTypes.MetaTrader4Demo
                or (short)PlatformTypes.MetaTrader5 or (short)PlatformTypes.MetaTrader5Demo))
                throw new NotImplementedException("__ONLY_MT4_OR_MT5_SUPPORTED_NOW__");

            var configuration = JsonConvert.DeserializeObject<TradeServiceOptions>(item.Configuration);
            if (configuration?.Api == null)
            {
                throw new InvalidDataException("__CONFIGURATION_FOR_TRADE_SERVICE_NOT_FOUND__");
            }

            if (item.Platform is ((short)PlatformTypes.MetaTrader5 or (short)PlatformTypes.MetaTrader5Demo) &&
                configuration.Api.HasUserNameAndPassword() == false)
            {
                throw new InvalidDataException("__CONFIGURATION_FOR_MT5_NOT_FOUND__");
            }

            _options[tradeServiceId] = configuration;
            return configuration;
        }

        public async Task<IMetaTraderService> GetMetaTraderService(int id)
        {
            if (_metaTraderServices.TryGetValue(id, out var value))
                return value;

            var item = await _tenantDbContext.TradeServices.FirstAsync(x => x.Id == id);
            var configuration = await GetOptions(id);
            switch (item.Platform)
            {
                case (short)PlatformTypes.MetaTrader4 or (short)PlatformTypes.MetaTrader4Demo:
                {
                    var svc = new MT4Service(configuration.Api!.Host, configuration.Api.Port, _loggerFactory);
                    _metaTraderServices[id] = svc;
                    return svc;
                }
                case (short)PlatformTypes.MetaTrader5 or (short)PlatformTypes.MetaTrader5Demo:
                {
                    var svc = new MT5Service(configuration.Api!.Host, configuration.Api.User,
                        configuration.Api.Password, _httpClientFactory.CreateClient(HttpClientHandlerTypes.ManualCertificate), _loggerFactory);
                    _metaTraderServices[id] = svc;
                    return svc;
                }
            }

            throw new EntryPointNotFoundException($"__TRADE_SERVER_{item.Id}_NOT_MATCH_PLATFORM_{item.Platform}__");
        }
    }
}