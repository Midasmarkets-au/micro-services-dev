using Amazon.Runtime.Internal.Transform;
using Bacera.Gateway;
using Bacera.Gateway.Context; // so TenantDbContext resolves
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web;
using Bacera.Gateway.Web.WebSocket;
using Bacera.Gateway.Services;
using Bacera.Gateway.Core.Types;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

public interface IMT5TickService
{
    Task PublishPriceAsync(long tenantId, string symbol, decimal bid, decimal ask);
    Task PublishPriceToAllTenantsAsync(string symbol, decimal bid, decimal ask);
    Task PublishPriceAsyncToOCEX(string symbol, decimal bid, decimal ask);
    Task StartFeedAsync();
    Task StopFeedAsync();
    Task<bool> IsRunningAsync();
    Task SetUpdateIntervalAsync(TimeSpan interval);

}
public class MT5TickService : BackgroundService, IMT5TickService
{
    private readonly IHubContext<MarketHub> _hubContext;
    private readonly IHttpClientFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MT5TickService> _logger;
    private string? _user;
    private string? _password;
    private Timer? _tickTimer;
    private bool _isRunning;
    private long _transactionId = 0;
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(5);
    private bool _credentialsLoaded;
    private bool _symbolsLoaded;
    private long _tenantIdForMarketFeed = 10000;
    private int _mt5ServiceId = 30; // default; adjust as needed or make configurable
    private readonly SemaphoreSlim _gate = new(1,1);
    private volatile bool _isAuthenticated;
    private string? _mt5BaseUrl;

    // Symbol arrays loaded from configuration (defaults if config not available)
    private string[] _symbolsMM = {
        "AUDUSD", "USDJPY", "GBPUSD", "US500", "JPN225", "XAUUSD"
    };

    private string[] _symbolsOCEX = {
        "AUDCNY", "AUDUSD", "AUDHKD", "AUDNZD", "AUDEUR", "AUDJPY", "AUDCAD", "AUDGBP", "AUDSGD"  
    };

    public MT5TickService(
        IHubContext<MarketHub> hubContext,
        IHttpClientFactory factory,
        ILogger<MT5TickService> logger,
        IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _factory = factory;
        _logger = logger;
        _serviceProvider = serviceProvider;

        _httpClient = _factory.CreateClient("mt5");
        _logger.LogInformation("MT5 BaseAddress: {Base}", _httpClient.BaseAddress);
    }

    /// <summary>
    /// Load symbol arrays from configuration using ConfigService
    /// </summary>
    private async Task EnsureSymbolsAsync()
    {
        if (_symbolsLoaded) return;

        try
        {
            // Only try to load config if database is available
            using var scope = _serviceProvider.CreateTenantScope(_tenantIdForMarketFeed);
            var configSvc = scope.ServiceProvider.GetRequiredService<ConfigService>();
            
            // Load SymbolsMM configuration
            var symbolsMMList = await configSvc.GetAsync<List<string>>(nameof(Public), 0, ConfigKeys.SymbolsMM);
            if (symbolsMMList != null && symbolsMMList.Count > 0)
            {
                _symbolsMM = symbolsMMList.ToArray();
                _logger.LogInformation("✅ Loaded SymbolsMM from config: {Symbols}", string.Join(", ", _symbolsMM));
            }
            else
            {
                _logger.LogInformation("SymbolsMM not found in configuration, using default values: {Symbols}", string.Join(", ", _symbolsMM));
            }
            
            // Load SymbolsOCEX configuration
            var symbolsOCEXList = await configSvc.GetAsync<List<string>>(nameof(Public), 0, ConfigKeys.SymbolsOCEX);
            if (symbolsOCEXList != null && symbolsOCEXList.Count > 0)
            {
                _symbolsOCEX = symbolsOCEXList.ToArray();
                _logger.LogInformation("✅ Loaded SymbolsOCEX from config: {Symbols}", string.Join(", ", _symbolsOCEX));
            }
            else
            {
                _logger.LogInformation("SymbolsOCEX not found in configuration, using default values: {Symbols}", string.Join(", ", _symbolsOCEX));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Database not available for symbol configuration, using default values - SymbolsMM: {MM}, SymbolsOCEX: {OCEX}", 
                string.Join(", ", _symbolsMM), string.Join(", ", _symbolsOCEX));
        }
        finally
        {
            _symbolsLoaded = true; // Always mark as loaded to prevent retry loops
            _logger.LogInformation("🎯 MT5TickService will use - MM symbols: {MM} | OCEX symbols: {OCEX}", 
                string.Join(", ", _symbolsMM), string.Join(", ", _symbolsOCEX));
        }
    }

    public async Task PublishPriceAsync(long tenantId, string symbol, decimal bid, decimal ask)
    {
        var groupName = MarketHub.GetTenantMarketGroupName(tenantId);
        var spread = CalculateSpread(symbol, bid, ask);
        var data = new Dictionary<string, string>
        {
            { "symbol", symbol },
            { "bid", bid.ToString() },
            { "ask", ask.ToString() },
            { "spread", spread.ToString() }
        };
        await _hubContext.Clients.Group(groupName).SendAsync("P", data);
    }

    public async Task PublishPriceToAllTenantsAsync(string symbol, decimal bid, decimal ask)
    {
        var spread = CalculateSpread(symbol, bid, ask);
        var data = new Dictionary<string, string>
        {
            { "symbol", symbol },
            { "bid", bid.ToString() },
            { "ask", ask.ToString() },
            { "spread", spread.ToString() }
        };
    // Send to MM group only (not to OCEX subscribers)
    var groupName = MarketHub.GetMMGroupName();
    await _hubContext.Clients.Group(groupName).SendAsync("P", data);
    }



    public async Task PublishPriceAsyncToOCEX(string symbol, decimal bid, decimal ask)
    {
        var groupName = MarketHub.GetOCEXGroupName();
        var spread = CalculateSpread(symbol, bid, ask);
        var data = new Dictionary<string, string>
        {
            { "symbol", symbol },
            { "bid", bid.ToString() },
            { "ask", ask.ToString() },
            { "spread", spread.ToString() }
        };
        await _hubContext.Clients.Group(groupName).SendAsync("P", data);
    }

    public async Task StartFeedAsync()
    {
        if (_isRunning) return;

        // Load MT5 credentials (minimal change: from TradeServices table)
        await EnsureCredentialsAsync();

        _isRunning = true;
        _tickTimer = new Timer(async _ => await FetchAndBroadcastTicks(),
            null, TimeSpan.Zero, _updateInterval);

        _logger.LogInformation("MT5 tick feed started");
        await Task.CompletedTask;
    }

    public async Task StopFeedAsync()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _tickTimer?.Dispose();
        _tickTimer = null;

        _logger.LogInformation("MT5 tick feed stopped");
        await Task.CompletedTask;
    }

    public Task<bool> IsRunningAsync() => Task.FromResult(_isRunning);

    public async Task SetUpdateIntervalAsync(TimeSpan interval)
    {
        _updateInterval = interval;
        if (_isRunning)
        {
            await StopFeedAsync();
            await StartFeedAsync();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Comment out for now due to MT5 tick error in prod. Auto-start for production
        // await StartFeedAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task FetchAndBroadcastTicks()
    {
        if (!_isRunning) return;

        try
        {
            await EnsureCredentialsAsync();
            await EnsureSymbolsAsync();
            var loginSuccess = await LoginAsync();
            if (!loginSuccess)
            {
                _logger.LogError("MT5 authentication failed");
                return;
            }

            // 1) Fetch default MM symbols and publish to default audience (existing behavior)
            {
                var symbols = string.Join(",", _symbolsMM);
                var url = $"api/tick/last?symbol={symbols}&trans_id={_transactionId}";

                var res = await _httpClient.GetAsync(url);

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("tick/last (MM) non-200: {Status}", (int)res.StatusCode);
                    _isAuthenticated = false; // force re-login next time on 403/401
                }
                else
                {
                    var content = await res.Content.ReadAsStringAsync();
                    var tickData = JsonConvert.DeserializeObject<Mt5TickResponse>(content);

                    if (tickData?.Answer != null)
                    {
                        var tasks = new List<Task>();
                        foreach (var tick in tickData.Answer)
                        {
                            if (decimal.TryParse(tick.Bid, out var bid) &&
                                decimal.TryParse(tick.Ask, out var ask))
                            {
                                // Broadcast MM symbols only to MM clients (not OCEX clients)
                                tasks.Add(PublishPriceToAllTenantsAsync(tick.Symbol, bid, ask));
                            }
                        }
                        await Task.WhenAll(tasks);
                    }
                }
            }

            // 2) Fetch OCEX symbols and publish only to OCEX group if clients subscribed
            {
                var symbolsOcex = string.Join(",", _symbolsOCEX);
                var urlOcex = $"api/tick/last?symbol={symbolsOcex}&trans_id={_transactionId}";

                var resOcex = await _httpClient.GetAsync(urlOcex);

                if (!resOcex.IsSuccessStatusCode)
                {
                    _logger.LogWarning("tick/last (OCEX) non-200: {Status}", (int)resOcex.StatusCode);
                    _isAuthenticated = false; // force re-login next time on 403/401
                }
                else
                {
                    var contentOcex = await resOcex.Content.ReadAsStringAsync();
                    var tickDataOcex = JsonConvert.DeserializeObject<Mt5TickResponse>(contentOcex);

                    if (tickDataOcex?.Answer != null)
                    {
                        var tasksOcex = new List<Task>();
                        foreach (var tick in tickDataOcex.Answer)
                        {
                            if (decimal.TryParse(tick.Bid, out var bid) &&
                                decimal.TryParse(tick.Ask, out var ask))
                            {
                                tasksOcex.Add(PublishPriceAsyncToOCEX(tick.Symbol, bid, ask));
                            }
                        }
                        await Task.WhenAll(tasksOcex);
                    }
                }
            }

            // Advance transaction id after both fetches
            _transactionId++;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching MT5 ticks");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopFeedAsync();
        await base.StopAsync(cancellationToken);
    }

    private static string EnsureSlash(string url)
      => url.EndsWith("/") ? url : url + "/";
    private async Task EnsureCredentialsAsync()
    {
      if (_credentialsLoaded) return;

      using var scope = _serviceProvider.CreateTenantScope(_tenantIdForMarketFeed);
      var tenantDb = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

      var item = await tenantDb.TradeServices
          .FirstOrDefaultAsync(x => x.Id == _mt5ServiceId);

      if (item == null) { _credentialsLoaded = true; return; }

      var cfg = JsonConvert.DeserializeObject<TradeServiceOptions>(item.Configuration);
      if (cfg?.Api != null && !string.IsNullOrWhiteSpace(cfg.Api.User) &&
          !string.IsNullOrWhiteSpace(cfg.Api.Password) &&
          !string.IsNullOrWhiteSpace(cfg.Api.Host))
      {
        _user = cfg.Api.User;
        _password = cfg.Api.Password;
        _mt5BaseUrl = EnsureSlash(cfg.Api.Host);

        // set BaseAddress on this client instance
        _httpClient.BaseAddress = new Uri(_mt5BaseUrl);

        _credentialsLoaded = true;
        _logger.LogInformation("MT5 BaseAddress loaded from DB: {Base}", _httpClient.BaseAddress);
      }
      else
      {
        _credentialsLoaded = true; // fallback to existing defaults if any
      }
    }

    // Authentication methods (copied from MT5Service)
    private async Task<bool> LoginAsync()
    {
      await _gate.WaitAsync();
      try {
        if (string.IsNullOrWhiteSpace(_user) || string.IsNullOrWhiteSpace(_password))
        {
            _logger.LogWarning("MT5 credentials missing; EnsureCredentialsAsync did not load user/password.");
            return false;
        }
        if (_isAuthenticated) return true;

        // 1) access check once
        var chk = await _httpClient.GetAsync("api/test/access");
        if (chk.IsSuccessStatusCode) { _isAuthenticated = true; return true; }

        // 2) start
        var start = await _httpClient.GetAsync($"api/auth/start?version=3950&agent=WebManager&login={_user}&type=manager");
        if (!start.IsSuccessStatusCode) return false;
        var startJson = await start.Content.ReadAsStringAsync();
        var startObj = JsonConvert.DeserializeObject<AuthenticationResponse>(startJson);
        if (startObj?.IsSuccess() != true || string.IsNullOrEmpty(startObj.ServerRandomCode)) return false;

        // 3) answer (same HttpClient => same cookies)
    var srv = GetServerHash(_password!, startObj.ServerRandomCode!);
        var cli = GetClientHash("today");
        var answer = await _httpClient.GetAsync($"api/auth/answer?srv_rand_answer={srv}&cli_rand={cli}");
        if (!answer.IsSuccessStatusCode) return false;

        // 4) re-check access to confirm
        var ok = await _httpClient.GetAsync("api/test/access");
        _isAuthenticated = ok.IsSuccessStatusCode;
        return _isAuthenticated;
      }
      finally { _gate.Release(); }
    }

    private static string GetServerHash(string password, string serverRandomHash)
    {
        var salt = "WebAPI"u8.ToArray();
        var passwordHash = MD5.HashData(Encoding.Unicode.GetBytes(password));
        var passwordHex = MD5.HashData(passwordHash.Concat(salt).ToArray());
        var serverSalt = Convert.FromHexString(serverRandomHash);
        var serverRandomHex = MD5.HashData(passwordHex.Concat(serverSalt).ToArray());
        return Convert.ToHexString(serverRandomHex).ToLower();
    }

    private static string GetClientHash(string hash)
    {
        var passwordHash = MD5.HashData(Encoding.Unicode.GetBytes(hash));
        return Convert.ToHexString(passwordHash).ToLower();
    }

    // Authentication response classes (same as MT5Service)
    public class AuthenticationResponse
    {
        [JsonProperty("retcode")] public string? ReturnCode { get; set; }
        [JsonProperty("srv_rand")] public string? ServerRandomCode { get; set; }
        public bool IsSuccess() => !string.IsNullOrEmpty(ReturnCode) && ReturnCode.StartsWith("0");
    }

    public class AuthenticationAnswerResponse
    {
        [JsonProperty("retcode")] public string? ReturnCode { get; set; }
        public bool IsSuccess() => !string.IsNullOrEmpty(ReturnCode) && ReturnCode.StartsWith("0");
    }

    public static decimal CalculateSpread(string symbol, decimal bid, decimal ask)
    {
        // Determine multiplier based on instrument type
        decimal multiplier;

        if (symbol.EndsWith("JPY", StringComparison.OrdinalIgnoreCase))
        {
            // JPY forex pairs often have 2 decimal places, 0.01 is 1 pip
            multiplier = 100;
        }
        else if (symbol.StartsWith("US", StringComparison.OrdinalIgnoreCase) ||
                symbol.StartsWith("JPN", StringComparison.OrdinalIgnoreCase))
        {
            // Indices (US500, JPN225, etc.) are usually quoted in whole points
            multiplier = 1;
        }
        else if (symbol.StartsWith("XAU", StringComparison.OrdinalIgnoreCase))
        {
            // Gold (XAUUSD) is usually 0.1 for a pip-like move
            multiplier = 100;
        }
        else
        {
            // Default: most forex pairs, 0.0001 = 1 pip
            multiplier = 10000;
        }

        decimal spread = (ask - bid) * multiplier;
        return Math.Round(spread, 1);
    }

}