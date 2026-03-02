using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Web;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Bacera.Gateway.Web.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader;

public class MT5Service : IMetaTraderService
{
    private readonly string _endpoint;
    private readonly string _user;
    private readonly string _password;
    private readonly ILogger<MT5Service> _logger;

    private readonly HttpClient _client;
    private const int Timeout = 10;
    
    // Semaphore to prevent concurrent login attempts (thread safety)
    // Only one login attempt per service instance at a time
    private readonly SemaphoreSlim _loginSemaphore = new SemaphoreSlim(1, 1);

    public MT5Service(string endpoint, string user, string password, HttpClient client,
        ILoggerFactory? loggerFactory = null)
    {
        _endpoint = endpoint;
        _user = user;
        _password = password;
        _logger = loggerFactory == null ? NullLogger<MT5Service>.Instance : loggerFactory.CreateLogger<MT5Service>();

        _client = client;
        _client.BaseAddress = new Uri(_endpoint.EndsWith('/') ? _endpoint : _endpoint + "/");
        // _client.Timeout = TimeSpan.FromSeconds(Timeout);
        // _client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

        // _client = CreateClient();
    }

    // private HttpClient CreateClient()
    // {
    //     var handler = new HttpClientHandler();
    //     handler.ClientCertificateOptions = ClientCertificateOption.Manual;
    //     handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
    //     // handler.SslProtocols |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
    //
    //     var client = new HttpClient(handler)
    //     {
    //         Timeout = TimeSpan.FromSeconds(Timeout),
    //         DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
    //         BaseAddress = new Uri(_endpoint.EndsWith("/") ? _endpoint : _endpoint + "/"),
    //     };
    //     return client;
    // }

    public async Task<ApiResult<GetAccountInfoResult>> GetAccountInfoAsync(long login)
    {
        await LoginAsync();
        // var response = await _client.GetAsync($"api/user/get?login={login}");
        //
        // var content = await response.Content.ReadAsStringAsync();
        // var userInfo = Mt5ApiResponse<UserInfoResponse>.FromJson(content);
        // if (userInfo?.Answer == null || !userInfo.ReturnCode.StartsWith('0'))
        // {
        //     _logger.LogWarning("API request error {ReturnCode}", userInfo?.ReturnCode);
        //     return ApiResult<GetAccountInfoResult>.Error(ApiResultStatus.SystemError, content);
        // }
        var userInfoTask = Task.Run(async () =>
        {
            var response = await _client.GetAsync($"api/user/get?login={login}");
            var content = await response.Content.ReadAsStringAsync();
            var info = Mt5ApiResponse<UserInfoResponse>.FromJson(content);
            if (info?.Answer == null || !info.ReturnCode.StartsWith('0')) return null;
            return info;
        });

        var accountInfoTask = Task.Run(async () =>
        {
            var response = await _client.GetAsync($"api/user/account/get?login={login}");
            var content = await response.Content.ReadAsStringAsync();
            var info = Mt5ApiResponse<GetAccountBalanceInfoResult>.FromJson(content);
            if (info?.Answer == null || !info.ReturnCode.StartsWith('0')) return null;
            return info;
        });

        var userInfo = await userInfoTask;
        if (userInfo == null)
        {
            _logger.LogWarning("API request error {ReturnCode}", userInfo?.ReturnCode);
            return ApiResult<GetAccountInfoResult>.Error(ApiResultStatus.SystemError, "");
        }

        var accountInfo = await accountInfoTask;

        var result = ApiResult<GetAccountInfoResult>.Build();
        result.SetStatus(ApiResultStatus.Success);
        result.SetData(userInfo?.Answer?.ToAccountInfoResult() ?? new GetAccountInfoResult());
        if (accountInfo != null && accountInfo.ReturnCode.StartsWith('0') && accountInfo.Answer != null)
        {
            // BcrLog.Slack($"MT5Service_GetAccountInfoAsync: {accountInfo.ReturnCode}");
            result.Data.Balance = accountInfo.Answer.Balance;
            result.Data.Credit = accountInfo.Answer.Credit;
            result.Data.Equity = accountInfo.Answer.Equity;
            result.Data.Margin = accountInfo.Answer.Margin;
            result.Data.MarginFree = accountInfo.Answer.MarginFree;
            result.Data.MarginLevel = accountInfo.Answer.MarginLevel;
            result.Data.MarginLeverage = accountInfo.Answer.MarginLeverage;
            result.Data.Profit = accountInfo.Answer.Profit;
        }

        return result;
    }

    // public async Task<ApiResult<GetAccountBalanceInfoResult>> GetAccountBalanceInfoAsync(long login)
    // {
    //     await LoginAsync();
    //     var response = await _client.GetAsync($"api/user/account/get?login={login}");
    //
    //     var content = await response.Content.ReadAsStringAsync();
    //     var userInfo = Mt5ApiResponse<GetAccountBalanceInfoResult>.FromJson(content);
    //     if (userInfo?.Answer == null || !userInfo.ReturnCode.StartsWith("0"))
    //     {
    //         _logger.LogWarning("API request error {ReturnCode}", userInfo?.ReturnCode);
    //         return ApiResult<GetAccountBalanceInfoResult>.Error(ApiResultStatus.SystemError, content);
    //     }
    //
    //     var result = ApiResult<GetAccountBalanceInfoResult>.Build().SetData(userInfo.Answer);
    //     return result;
    // }

    public async Task<ApiResult<CreateAccountResponse>> CreateAccountAsync(CreateAccountRequest spec)
    {
        var loginResult = await LoginAsync();
        if (!loginResult)
        {
            _logger.LogError("[MT5 API] CreateAccountAsync failed - Login failed before calling api/user/add");
            return ApiResult<CreateAccountResponse>.Error(ApiResultStatus.SystemError, "Authentication failed");
        }

        var requestData = spec.ToMt5Spec();
        var requestBody = JsonConvert.SerializeObject(requestData);
        var request = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
        
        _logger.LogInformation("[MT5 API] api/user/add - Request URL: {Url}", $"{_client.BaseAddress}api/user/add");
        _logger.LogInformation("[MT5 API] api/user/add - Request Body: {RequestBody}", requestBody);
        _logger.LogInformation("[MT5 API] api/user/add - Original Spec Group: {Group}", spec.Group);
        _logger.LogInformation("[MT5 API] api/user/add - HttpClient Timeout: {Timeout}", _client.Timeout);
        
        var startTime = DateTime.UtcNow;
        var response = await _client.PostAsync($"api/user/add", request);
        
        var elapsedTime = DateTime.UtcNow - startTime;
        _logger.LogInformation("[MT5 API] api/user/add - Response Status: {StatusCode} {StatusText} (Elapsed: {ElapsedMs}ms)", 
            (int)response.StatusCode, response.StatusCode.ToString(), elapsedTime.TotalMilliseconds);
        
        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("[MT5 API] api/user/add - Response Content: {ResponseContent}", content);
        
        var apiResponse = Mt5ApiResponse<AddUserResponse>.FromJson(content);
        if (apiResponse?.Answer == null || !apiResponse.ReturnCode.StartsWith("0"))
        {
            _logger.LogError("[MT5 API] api/user/add - Error: ReturnCode={ReturnCode}, Spec={@Spec}, Response={Response}", 
                apiResponse?.ReturnCode ?? "null", spec, content);
            if (apiResponse != null && apiResponse.ReturnCode.StartsWith("3"))
            {
                return ApiResult<CreateAccountResponse>.Error(ApiResultStatus.InvalidIncomingParameters, content);
            }

            return ApiResult<CreateAccountResponse>.Error(ApiResultStatus.SystemError, content);
        }

        _logger.LogInformation("[MT5 API] api/user/add - Success: Login={Login}", apiResponse.Answer.Login);
        var result = ApiResult.Build<CreateAccountResponse>();
        result.SetStatus(ApiResultStatus.Success);
        result.SetData(new CreateAccountResponse { Login = long.Parse(apiResponse.Answer.Login) });
        return result;
    }

    public Task<ApiResult<UpdateAccountResponse>> UpdateAccountAsync(UpdateAccountRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResult<ChangeBalanceResponse>> ChangeBalanceAsync(ChangeBalanceRequest request)
    {
        await LoginAsync();
        // type 2 : Balance
        request.Comment = request.Comment.Replace(" ", "%20").Replace("#", "%23");
        var uri =
            $"api/trade/balance?login={request.Login}&type=2&check_margin=0&balance={request.Value}&comment={request.Comment}";
        _logger.LogInformation("MT5ChangeBalanceAsync_Uri_{uri}", uri);
        var response =
            await _client.GetAsync(
                $"api/trade/balance?login={request.Login}&type=2&check_margin=0&balance={request.Value}&comment={request.Comment}");
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = Mt5ApiResponse<TicketResponse>.FromJson(content);
        if (apiResponse?.Answer == null || !apiResponse.ReturnCode.StartsWith('0'))
        {
            _logger.LogError("Change balance Error {@Spec} {Response} ", request, content);
            if (apiResponse != null && apiResponse.ReturnCode.StartsWith('3'))
            {
                return ApiResult<ChangeBalanceResponse>.Error(ApiResultStatus.InvalidIncomingParameters, content);
            }

            return ApiResult<ChangeBalanceResponse>.Error(ApiResultStatus.SystemError, content);
        }

        var result = ApiResult.Build<ChangeBalanceResponse>();
        result.SetStatus(ApiResultStatus.Success);
        result.SetData(new ChangeBalanceResponse
            { Login = request.Login, OrderId = apiResponse.Answer?.Ticket ?? 0 });
        return result;
    }

    public async Task<ApiResult<ChangeCreditResponse>> ChangeCreditAsync(ChangeCreditRequest request)
    {
        await LoginAsync();
        // type 3 : Credit
        request.Comment = request.Comment.Replace(" ", "%20").Replace("#", "%23");
        var uri =
            $"api/trade/balance?login={request.Login}&type=3&check_margin=0&balance={request.Value}&comment={request.Comment}";
        var response = await _client.GetAsync(uri);
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = Mt5ApiResponse<TicketResponse>.FromJson(content);
        if (apiResponse?.Answer == null || !apiResponse.ReturnCode.StartsWith("0"))
        {
            _logger.LogError("Change Credit Error {@Spec} {Response} ", request, content);
            if (apiResponse != null && apiResponse.ReturnCode.StartsWith("3"))
            {
                return ApiResult<ChangeCreditResponse>.Error(ApiResultStatus.InvalidIncomingParameters, content);
            }

            return ApiResult<ChangeCreditResponse>.Error(ApiResultStatus.SystemError, content);
        }

        var result = ApiResult.Build<ChangeCreditResponse>();
        result.SetStatus(ApiResultStatus.Success);
        result.SetData(new ChangeCreditResponse
            { Login = request.Login, OrderId = apiResponse.Answer?.Ticket ?? 0 });
        return result;
    }

    public async Task<ApiResult<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        await LoginAsync();
        // var response =
        //     await _client.GetAsync(
        //         $"api/user/change_password?login={request.Login}&type={request.GetPasswordType()}&password={request.Password}");
        // var content = await response.Content.ReadAsStringAsync();

        // var formContent = new FormUrlEncodedContent(new[]
        // {
        //     new KeyValuePair<string?, string?>("Login", request.Login.ToString()),
        //     new KeyValuePair<string?, string?>("Type", request.GetPasswordType()),
        //     new KeyValuePair<string?, string?>("Password", request.Password)
        // });

        var dict = new Dictionary<string, string>
        {
            { "Login", request.Login.ToString() },
            { "Type", request.GetPasswordType() },
            { "Password", request.Password }
        };

        // var formContent = new FormUrlEncodedContent(dict);
        var formContent = new StringContent(JsonConvert.SerializeObject(dict), Encoding.UTF8,
            "application/x-www-form-urlencoded");
        var response = await _client.PostAsync("api/user/change_password", formContent);
        var content = await response.Content.ReadAsStringAsync();

        var apiResponse = Mt5ApiResponse<ChangePasswordResponse>.FromJson(content);
        if (apiResponse?.ReturnCode == null || !apiResponse.ReturnCode.StartsWith("0"))
        {
            _logger.LogError("Change Password Error {@Spec} {Response} ", request, content);
            if (apiResponse != null && apiResponse.ReturnCode.StartsWith("3"))
            {
                return ApiResult<ChangePasswordResponse>.Error(ApiResultStatus.InvalidIncomingParameters, content);
            }

            return ApiResult<ChangePasswordResponse>.Error(ApiResultStatus.SystemError, content);
        }

        var result = ApiResult.Build<ChangePasswordResponse>();
        result.SetStatus(ApiResultStatus.Success);
        result.SetData(new ChangePasswordResponse { Login = request.Login });
        return result;
    }

    public async Task<ApiResult<CheckPasswordResponse>> CheckPasswordAsync(CheckPasswordRequest request)
    {
        await LoginAsync();
        var response =
            await _client.GetAsync(
                $"api/user/check_password?login={request.Login}&type={request.GetPasswordType()}&password={request.Password}");
        var content = await response.Content.ReadAsStringAsync();

        var apiResponse = Mt5ApiResponse<CheckPasswordResponse>.FromJson(content);
        if (apiResponse?.ReturnCode == null || !apiResponse.ReturnCode.StartsWith("0"))
        {
            _logger.LogError("Check Password Error {@Spec} {Response} ", request, content);
            if (apiResponse != null && apiResponse.ReturnCode.StartsWith("3"))
            {
                return ApiResult<CheckPasswordResponse>.Error(ApiResultStatus.InvalidIncomingParameters, content);
            }

            return ApiResult<CheckPasswordResponse>.Error(ApiResultStatus.SystemError, content);
        }

        var result = ApiResult.Build<CheckPasswordResponse>();
        result.SetStatus(ApiResultStatus.Success);
        result.SetData(new CheckPasswordResponse { Login = request.Login });
        return result;
    }

    public async Task<ApiResult<ChangeLeverageResponse>> ChangeLeverageAsync(ChangeLeverageRequest request)
    {
        await LoginAsync();
        var response =
            await _client.GetAsync(
                $"api/user/update?login={request.Login}&leverage={request.Leverage}");
        var content = await response.Content.ReadAsStringAsync();

        var apiResponse = Mt5ApiResponse<CheckPasswordResponse>.FromJson(content);
        if (apiResponse?.Answer == null || !apiResponse.ReturnCode.StartsWith("0"))
        {
            _logger.LogError("Change Leverage Error {@Spec} {Response} ", request, content);
            if (apiResponse != null && apiResponse.ReturnCode.StartsWith("3"))
            {
                return ApiResult<ChangeLeverageResponse>.Error(ApiResultStatus.InvalidIncomingParameters, content);
            }

            return ApiResult<ChangeLeverageResponse>.Error(ApiResultStatus.SystemError, content);
        }

        var result = ApiResult.Build<ChangeLeverageResponse>();
        result.SetStatus(ApiResultStatus.Success);
        result.SetData(new ChangeLeverageResponse { Login = request.Login });
        return result;
    }

    public async Task<string> GetGroupAndSymbols(string group, string symbol, int transId)
    {
        await LoginAsync();
        var response =
            transId != 0
                ? await _client.GetAsync($"/api/tick/last_group?group={group}&symbol={symbol}&trans_id={transId}")
                : await _client.GetAsync($"/api/tick/last_group?group={group}&symbol={symbol}");

        var content = await response.Content.ReadAsStringAsync();
        return content;
    }

    public async Task<ApiResult<List<AccountDailyReportResponse>>> GetDailyReport(long login, DateTime from,
        DateTime to)
    {
        var loginResult = await LoginAsync();
        // from = from.AddSeconds(1);
        // to = to.AddSeconds(2);
        var fromUnix = new DateTimeOffset(from).ToUnixTimeSeconds();
        var toUnix = new DateTimeOffset(to).ToUnixTimeSeconds();
        var response = await _client.GetAsync($"/api/daily_get?from={fromUnix}&to={toUnix}&login={login}");

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = Mt5ApiResponse<List<AccountDailyReportResponse>>.FromJson(content);

        if (apiResponse?.Answer == null || !apiResponse.ReturnCode.StartsWith("0"))
        {
            _logger.LogError("Get Daily Report Error {login} {Response} {from} {to} ", login, content, from, to);
            if (apiResponse != null && apiResponse.ReturnCode.StartsWith("3"))
            {
                return ApiResult<List<AccountDailyReportResponse>>.Error(ApiResultStatus.InvalidIncomingParameters,
                    content);
            }

            return ApiResult<List<AccountDailyReportResponse>>.Error(ApiResultStatus.SystemError, content);
        }

        var result = ApiResult.Build<List<AccountDailyReportResponse>>();
        result.SetStatus(ApiResultStatus.Success);
        result.SetData(apiResponse.Answer);
        return result;
    }

    public async Task<bool> LoginAsync()
    {
        // Thread safety: prevent concurrent login attempts
        // Multiple requests sharing the same MT5Service instance will wait for login to complete
        await _loginSemaphore.WaitAsync();
        try
        {
            // Quick check: if already authenticated, return immediately
            if (await HasAccessAsync())
            {
                _logger.LogInformation("[MT5 API] LoginAsync - Already authenticated (HasAccessAsync returned true)");
                return true;
            }
            
            var loginStartTime = DateTime.UtcNow;
            
            // Retry logic: attempt login once more if first attempt fails
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                if (attempt > 1)
                {
                    _logger.LogWarning("[MT5 API] LoginAsync - Retry attempt {Attempt}", attempt);
                    await Task.Delay(500); // 0.5s delay before retry - gives time for transient network issues to resolve
                }
                
                // Step 1: api/auth/start
                var startEndpoint = $"api/auth/start?version=3950&agent=WebManager&login={_user}&type=manager";
                _logger.LogInformation("[MT5 API] api/auth/start - Request URL: {Url}", $"{_client.BaseAddress}{startEndpoint}");
                _logger.LogInformation("[MT5 API] api/auth/start - User: {User}, Timeout: {Timeout}, Attempt: {Attempt}", 
                    _user, _client.Timeout, attempt);
                
                var startTime = DateTime.UtcNow;
                var response = await _client.GetAsync(startEndpoint);
                
                var elapsedStart = DateTime.UtcNow - startTime;
                _logger.LogInformation("[MT5 API] api/auth/start - Response Status: {StatusCode} {StatusText} (Elapsed: {ElapsedMs}ms), Attempt: {Attempt}", 
                    (int)response.StatusCode, response.StatusCode.ToString(), elapsedStart.TotalMilliseconds, attempt);
                
                if (false == response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[MT5 API] api/auth/start - Failed: StatusCode={StatusCode}, Response={Response}, Attempt: {Attempt}", 
                        response.StatusCode, errorContent, attempt);
                    if (attempt == 2) return false;
                    continue;
                }
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("[MT5 API] api/auth/start - Response Content: {ResponseContent}, Attempt: {Attempt}", content, attempt);
                
                var startResult = JsonConvert.DeserializeObject<AuthenticationResponse>(content);
                if (startResult == null || false == startResult.IsSuccess())
                {
                    _logger.LogError("[MT5 API] api/auth/start - Failed: ReturnCode={ReturnCode}, Response={Response}, Attempt: {Attempt}", 
                        startResult?.ReturnCode ?? "null", content, attempt);
                    if (attempt == 2) return false;
                    continue;
                }
                
                _logger.LogInformation("[MT5 API] api/auth/start - Success: ReturnCode={ReturnCode}, ServerRandomCode={ServerRandomCode}, Attempt: {Attempt}", 
                    startResult.ReturnCode, startResult.ServerRandomCode, attempt);

                // Step 2: api/auth/answer
                var serverHash = GetServerHash(_password, startResult.ServerRandomCode!);
                var clientHash = GetClientHash("today");
                var authEndpoint = $"api/auth/answer?srv_rand_answer={serverHash}&cli_rand={clientHash}";
                _logger.LogInformation("[MT5 API] api/auth/answer - Request URL: {Url}", $"{_client.BaseAddress}{authEndpoint}");
                _logger.LogInformation("[MT5 API] api/auth/answer - ServerHash: {ServerHash}, ClientHash: {ClientHash}, Attempt: {Attempt}", 
                    serverHash, clientHash, attempt);
                
                var answerStartTime = DateTime.UtcNow;
                response = await _client.GetAsync(authEndpoint);
                
                var elapsedAnswer = DateTime.UtcNow - answerStartTime;
                _logger.LogInformation("[MT5 API] api/auth/answer - Response Status: {StatusCode} {StatusText} (Elapsed: {ElapsedMs}ms), Attempt: {Attempt}", 
                    (int)response.StatusCode, response.StatusCode.ToString(), elapsedAnswer.TotalMilliseconds, attempt);

                if (false == response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[MT5 API] api/auth/answer - Failed: StatusCode={StatusCode}, Response={Response}, RequestUrl={RequestUrl}, Attempt: {Attempt}", 
                        response.StatusCode, error, authEndpoint, attempt);
                    if (attempt == 2) return false;
                    continue;
                }

                content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("[MT5 API] api/auth/answer - Response Content: {ResponseContent}, Attempt: {Attempt}", content, attempt);
                
                AuthenticationAnswerResponse? answerResult;
                try
                {
                    answerResult = JsonConvert.DeserializeObject<AuthenticationAnswerResponse>(content);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[MT5 API] api/auth/answer - JSON Deserialization Error: User={User}, Response={Response}, Attempt: {Attempt}", 
                        _user, content, attempt);
                    if (attempt == 2) return false;
                    continue;
                }

                if (answerResult == null || !answerResult.IsSuccess())
                {
                    _logger.LogError("[MT5 API] api/auth/answer - Failed: ReturnCode={ReturnCode}, Response={Response}, Attempt: {Attempt}", 
                        answerResult?.ReturnCode ?? "null", content, attempt);
                    if (attempt == 2) return false;
                    continue;
                }
                
                var totalLoginTime = DateTime.UtcNow - loginStartTime;
                _logger.LogInformation("[MT5 API] api/auth/answer - Success: ReturnCode={ReturnCode}, AccessVersion={AccessVersion}, TotalLoginTime={TotalMs}ms, Attempt: {Attempt}", 
                    answerResult.ReturnCode, answerResult.AccessVersion, totalLoginTime.TotalMilliseconds, attempt);
                
                return true;
            }
            
            return false;
        }
        finally
        {
            _loginSemaphore.Release();
        }
    }

    private async Task<bool> HasAccessAsync()
    {
        try
        {
            var response = await _client.GetFromJsonAsync<AuthenticationResponse>("api/test/access");
            return response != null && response.IsSuccess();
        }
        catch
        {
            return false;
        }
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

    public class AuthenticationResponse
    {
        [JsonPropertyName("retcode"), JsonProperty(PropertyName = "retcode")]
        public string? ReturnCode { get; set; }

        [JsonPropertyName("version_access"), JsonProperty(PropertyName = "version_access")]
        public string? AccessVersion { get; set; }

        [JsonPropertyName("srv_rand"), JsonProperty(PropertyName = "srv_rand")]
        public string? ServerRandomCode { get; set; }

        public bool IsSuccess() => !string.IsNullOrEmpty(ReturnCode) && ReturnCode.StartsWith("0");
    }

    public class AuthenticationAnswerResponse
    {
        [JsonPropertyName("retcode"), JsonProperty("retcode")]
        public string? ReturnCode { get; set; }

        [JsonPropertyName("version_access"), JsonProperty("version_access")]
        public string? AccessVersion { get; set; }

        [JsonPropertyName("version_trade"), JsonProperty("version_trade")]
        public string? ServerVersion { get; set; }

        [JsonPropertyName("cli_rand_answer"), JsonProperty("cli_rand_answer")]
        public string? ClientRandomAnswer { get; set; }

        public bool IsSuccess() => !string.IsNullOrEmpty(ReturnCode) && ReturnCode.StartsWith("0");
    }

    public class AddUserResponse
    {
        public string Login { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string CertSerialNumber { get; set; } = string.Empty;
        public string Rights { get; set; } = string.Empty;
        public string MQID { get; set; } = string.Empty;
        public string Registration { get; set; } = string.Empty;
        public string LastAccess { get; set; } = string.Empty;
        public string LastPassChange { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
    }

    public class Mt5ApiResponse<T>
    {
        [JsonPropertyName("retcode"), JsonProperty("retcode")]
        public string ReturnCode { get; set; } = string.Empty;

        [JsonPropertyName("login"), JsonProperty("login")]
        public string Login { get; set; } = string.Empty;

        [JsonPropertyName("answer"), JsonProperty("answer")]
        public T? Answer { get; set; }

        public bool IsSuccess() => ReturnCode.Split(" ").First() == "0";

        public static Mt5ApiResponse<T>? FromJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<Mt5ApiResponse<T>>(json);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }

    public class TicketResponse
    {
        [JsonPropertyName("ticket"), JsonProperty("ticket")]
        public long Ticket { get; set; }
    }

    public class UserInfoResponse
    {
        public string? Login { get; set; }
        public string? Group { get; set; }
        public string? CertSerialNumber { get; set; }
        public string? Rights { get; set; }
        public string? MQID { get; set; }
        public string? Registration { get; set; }
        public string? LastAccess { get; set; }
        public string? LastPassChange { get; set; }
        public string? LastIP { get; set; }
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? Company { get; set; }
        public string? Account { get; set; }
        public string? Country { get; set; }
        public string? Language { get; set; }
        public string? ClientID { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ID { get; set; }
        public string? Status { get; set; }
        public string? Comment { get; set; }
        public string? Color { get; set; }
        public string? PhonePassword { get; set; }
        public string? Leverage { get; set; }
        public long? Agent { get; set; }
        public string? LimitPositions { get; set; }
        public string? LimitOrders { get; set; }
        public string? CurrencyDigits { get; set; }
        public decimal? Balance { get; set; }
        public string? Credit { get; set; }
        public string? InterestRate { get; set; }
        public string? CommissionDaily { get; set; }
        public string? CommissionMonthly { get; set; }
        public string? CommissionAgentDaily { get; set; }
        public string? CommissionAgentMonthly { get; set; }
        public string? BalancePrevDay { get; set; }
        public string? BalancePrevMonth { get; set; }
        public string? EquityPrevDay { get; set; }
        public string? EquityPrevMonth { get; set; }
        public string? TradeAccounts { get; set; }
        public string? LeadCampaign { get; set; }
        public string? LeadSource { get; set; }
    }
}

public static class MT5ServiceExt
{
    public static GetAccountInfoResult ToAccountInfoResult(this MT5Service.UserInfoResponse me)
        => new()
        {
            Login = long.Parse(me.Login ?? "0"),
            Name = me.Name ?? string.Empty,
            Email = me.Email ?? string.Empty,
            Group = me.Group ?? string.Empty,
            Leverage = int.Parse(me.Leverage ?? "0"),
            RegDate = me.Registration ?? string.Empty,
            Country = me.Country ?? string.Empty,
            State = me.State ?? string.Empty,
            Address = me.Address ?? string.Empty,
            Phone = me.Phone ?? string.Empty,
            City = me.City ?? string.Empty,
            Zip = me.ZipCode ?? string.Empty,
            Balance = me.Balance ?? 0,
            Comment = me.Comment ?? string.Empty,
            Agent = me.Agent ?? 0,
            LastDate = me.LastAccess ?? string.Empty,
        };
}