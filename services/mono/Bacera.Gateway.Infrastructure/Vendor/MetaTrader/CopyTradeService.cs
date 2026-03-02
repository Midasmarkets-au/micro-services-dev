// using System.Net;
// using System.Security.Authentication;
// using System.Text;
// using Bacera.Gateway.Vendor.MetaTrader;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Logging.Abstractions;
// using Microsoft.Extensions.Options;
// using Newtonsoft.Json;
//
// namespace Bacera.Gateway.Vendor.MetaTrader;
//
// using M = CopyTrade;
// using MSG = ResultMessage.CopyTrade;
//
// public class CopyTradeService : ICopyTradeService
// {
//     private readonly string _login;
//     private readonly string _password;
//
//     private DateTime _expiration;
//     private string _token = string.Empty;
//
//     private readonly HttpClient _httpClient;
//     private readonly TenantDbContext _dbContext;
//     private readonly ILogger<CopyTradeService> _logger;
//
//     public CopyTradeService(
//         IOptions<CopyTradeOptions> options
//         , TenantDbContext tenantDbContext
//         , ILogger<CopyTradeService>? logger = null
//     )
//     {
//         _dbContext = tenantDbContext;
//         var optionValues = options.Value;
//         _login = optionValues.Login;
//         _password = optionValues.Password;
//         _logger = logger ?? new NullLogger<CopyTradeService>();
//         var handler = new HttpClientHandler();
//         handler.ServerCertificateCustomValidationCallback += (_, _, _, _) => true;
//         handler.SslProtocols = SslProtocols.None; // for match API Server TLS version 1.0, Must be set in openssl.conf
//         _httpClient = new HttpClient(handler)
//             { BaseAddress = new Uri(optionValues.Endpoint), Timeout = TimeSpan.FromSeconds(10) };
//     }
//
//     public async Task<Result<List<M>, M.Criteria>> QueryAsync(M.Criteria criteria)
//     {
//         var items = await _dbContext.CopyTrades
//             .PagedFilterBy(criteria)
//             .ToListAsync();
//         return Result<List<M>, M.Criteria>.Of(items, criteria);
//     }
//
//     public async Task<M> GetAsync(long id)
//     {
//         var item = await _dbContext.CopyTrades.FirstOrDefaultAsync(x => x.Id == id);
//         return item ?? new M();
//     }
//
//     public async Task<M> GetForPartyAsync(long id, long partyId)
//     {
//         var item = await _dbContext.CopyTrades
//             .FirstOrDefaultAsync(x => x.Id == id
//                                       && x.PartyId == partyId);
//         return item ?? new M();
//     }
//
//     public async Task<Result<bool>> DeleteAsync(long id)
//     {
//         var item = await _dbContext.CopyTrades
//             .FirstOrDefaultAsync(x => x.Id == id);
//         if (item == null)
//         {
//             return Result<bool>.Error(ResultMessage.CopyTrade.RecordNotFound);
//         }
//
//         var result = await DeleteRuleByApiAsync(item.RuleNumber);
//         if (result == false)
//         {
//             return Result<bool>.Error(ResultMessage.CopyTrade.DeleteRuleFailed);
//         }
//
//         _dbContext.CopyTrades.Remove(item);
//         await _dbContext.SaveChangesAsync();
//         return Result<bool>.Of(true);
//     }
//
//     public async Task<Result<bool>> DeleteForPartyAsync(long id, long partyId)
//     {
//         var item = await _dbContext.CopyTrades
//             .FirstOrDefaultAsync(x => x.Id == id && x.PartyId == partyId);
//         if (item == null)
//         {
//             return Result<bool>.Error(ResultMessage.CopyTrade.RecordNotFound);
//         }
//
//         var result = await DeleteRuleByApiAsync(item.RuleNumber);
//         if (result == false)
//         {
//             return Result<bool>.Error(ResultMessage.CopyTrade.DeleteRuleFailed);
//         }
//
//         _dbContext.CopyTrades.Remove(item);
//         await _dbContext.SaveChangesAsync();
//         return Result<bool>.Of(true);
//     }
//
//     public async Task<Result<M>> CreateAsync(long sourceAccountId, long targetAccountId, string mode, int? value = 0)
//     {
//         if (ModeTypes.IsValid(mode) == false)
//         {
//             return Result<M>.Error(MSG.ModeInvalid);
//         }
//
//         if (ModeTypes.IsValueValid(mode, value) == false)
//         {
//             return Result<M>.Error(MSG.ModeInvalid);
//         }
//
//         if (await _dbContext.CopyTrades.AnyAsync(x =>
//                 x.SourceAccountId == sourceAccountId && x.TargetAccountId == targetAccountId))
//         {
//             return Result<M>.Error(MSG.RecordExists);
//         }
//
//         if (await _dbContext.CopyTrades.Where(x => x.SourceAccountId == sourceAccountId).CountAsync() >= 10)
//         {
//             return Result<M>.Error(MSG.MaxRuleCountReached);
//         }
//
//         var sourceAccountNumber = await _dbContext.TradeAccounts
//             .Where(x => x.Id == sourceAccountId)
//             .Select(x => x.AccountNumber)
//             .FirstOrDefaultAsync();
//
//         if (sourceAccountNumber < 1)
//         {
//             return Result<M>.Error(MSG.SourceAccountNotFound);
//         }
//
//         var targetAccount = await _dbContext.TradeAccounts
//             .Include(x => x.IdNavigation)
//             .Where(x => x.Id == targetAccountId)
//             .FirstOrDefaultAsync();
//         if (targetAccount == null || targetAccount.AccountNumber < 1)
//         {
//             return Result<M>.Error(MSG.TargetAccountNotFound);
//         }
//
//         var ruleNumber = await CreateRuleByApiAsync(sourceAccountNumber, targetAccount.AccountNumber, mode, value);
//         if (ruleNumber < 1)
//         {
//             return Result<M>.Error(MSG.RuleCreationFailed);
//         }
//
//         var item = new M
//         {
//             PartyId = targetAccount.IdNavigation.PartyId,
//             SourceAccountId = sourceAccountId,
//             SourceAccountNumber = sourceAccountNumber,
//             TargetAccountId = targetAccountId,
//             TargetAccountNumber = targetAccount.AccountNumber,
//             RuleNumber = ruleNumber,
//             Mode = mode,
//             Value = value ?? 0,
//         };
//         await _dbContext.CopyTrades.AddAsync(item);
//         await _dbContext.SaveChangesAsync();
//         return Result<M>.Of(item);
//     }
//
//     public async Task<List<Rule>> ListRulesByApiAsync()
//     {
//         try
//         {
//             var client = await GetHttpClient();
//             var response = await client.GetAsync("/rules");
//             response.EnsureSuccessStatusCode();
//             var content = await response.Content.ReadAsStringAsync();
//             var result = JsonConvert.DeserializeObject<ListResponse>(content);
//             return result?.CopyingRules ?? new List<Rule>();
//         }
//         catch (Exception e)
//         {
//             _logger.LogError("Failed to get rule list error : {Error}", e.Message);
//             throw;
//         }
//     }
//
//     public async Task<bool> DeleteRuleByApiAsync(int ruleNumber)
//     {
//         try
//         {
//             var client = await GetHttpClient();
//             var content = new HttpRequestMessage(HttpMethod.Delete, $"/rules/{ruleNumber}");
//             content.Headers.Add("synchronous", "true");
//             var response = await client.SendAsync(content);
//             if (response.StatusCode == HttpStatusCode.OK) return true;
//
//             var errorJson = await response.Content.ReadAsStringAsync();
//             var errorResult = JsonConvert.DeserializeObject<MessageResponse>(errorJson);
//             throw new Exception(errorResult?.Message);
//         }
//         catch (Exception e)
//         {
//             _logger.LogError("Failed to delete rule {RuleNumber} error : {Error}", ruleNumber, e.Message);
//             throw;
//         }
//     }
//
//     public async Task<int> CreateRuleByApiAsync(long sourceAccountNumber, long targetAccountNumber, string mode,
//         int? value = 0)
//     {
//         try
//         {
//             var client = await GetHttpClient();
//             var request = CreateRequest.Create(sourceAccountNumber, targetAccountNumber, mode, value);
//             var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
//             content.Headers.Add("synchronous", "true");
//             var response = await client.PostAsync("/rules", content);
//
//             var resultJson = await response.Content.ReadAsStringAsync();
//             var result = JsonConvert.DeserializeObject<MessageResponse>(resultJson);
//
//             if (response.StatusCode == HttpStatusCode.OK && result is { Id: > 0 })
//             {
//                 return result.Id ?? 0;
//             }
//
//             throw new Exception(result?.Message);
//         }
//         catch (Exception e)
//         {
//             _logger.LogError(
//                 "Failed to create rule [source:{sourceAccountNumber}] [target:{targetAccountNumber}] error : {Error}"
//                 , sourceAccountNumber
//                 , targetAccountNumber,
//                 e.Message);
//             throw;
//         }
//     }
//
//     private async Task<HttpClient> GetHttpClient()
//     {
//         if (!string.IsNullOrEmpty(_token) && _expiration > DateTime.UtcNow) return _httpClient;
//         _httpClient.DefaultRequestHeaders.Remove("Authorization");
//         _httpClient.DefaultRequestHeaders.Add("Authorization", await GetToken());
//         return _httpClient;
//     }
//
//     private async Task<string> GetToken()
//     {
//         if (!string.IsNullOrEmpty(_token))
//         {
//             return _token;
//         }
//
//         try
//         {
//             // For Ubuntu 20.04 ssl 1.0 issue
//             // Have to edit /etc/ssl/openssl.cnf on server
//             // https://askubuntu.com/questions/1233186/ubuntu-20-04-how-to-set-lower-ssl-security-level
//             var response = await _httpClient.PostAsync("/authorize", new StringContent(
//                 JsonConvert.SerializeObject(new
//                 {
//                     login = _login,
//                     password = _password
//                 }), Encoding.UTF8, "application/json"));
//             if (response.StatusCode != HttpStatusCode.OK)
//             {
//                 var errorJson = await response.Content.ReadAsStringAsync();
//                 var errorResult = JsonConvert.DeserializeObject<MessageResponse>(errorJson);
//                 throw new Exception(errorResult?.Message);
//             }
//
//             response.EnsureSuccessStatusCode();
//             var json = await response.Content.ReadAsStringAsync();
//             var result = JsonConvert.DeserializeObject<TokenResponse>(json);
//             _token = result?.AccessToken ?? throw new Exception("Token is null");
//             _expiration = result.Expiration;
//             _logger.LogDebug("Copy Trade Logged in.");
//             return _token;
//         }
//         catch (Exception e)
//         {
//             _logger.LogError("Failed to get token error : {Error}", e.Message);
//             throw;
//         }
//     }
// }