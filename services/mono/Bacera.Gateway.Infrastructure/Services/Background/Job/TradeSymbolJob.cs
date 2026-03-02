// using Bacera.Gateway.Interfaces;
// using Bacera.Gateway.Vendor.MetaTrader;
//
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Hosting;
//
// using Newtonsoft.Json;
//
// namespace Bacera.Gateway.Services.Background;
//
// public class TradeSymbolJob : BackgroundService
// {
//     private readonly long _tenantId;
//     private readonly TenantDbContext _tenantDbContext;
//     private readonly ISendMessageService _sendMessageService;
//     private readonly Dictionary<string, int> _transIdDict = new();
//
//     private List<TradeServiceSimpleOptions> _tradeAccountGroups = new();
//
//     private MT5Service? _mt5Service;
//
//     public TradeSymbolJob(long tenantId
//         , TenantDbContext tenantDbContext
//         , ISendMessageService sendMessageService)
//     {
//         _tenantId = tenantId;
//         _tenantDbContext = tenantDbContext;
//         _sendMessageService = sendMessageService;
//     }
//
//     public override async Task StartAsync(CancellationToken cancellationToken)
//     {
//         _tradeAccountGroups = await GetTradeAccountGroups();
//         var mt5Config = _tradeAccountGroups
//             .Where(x => x.Platform == PlatformTypes.MetaTrader5)
//             .Select(x => x.Configuration)
//             .FirstOrDefault();
//         if (mt5Config is null) return;
//
//         _mt5Service = new MT5Service(mt5Config.Api!.Host, mt5Config.Api.User, mt5Config.Api.Password);
//         if (_mt5Service == null) return;
//         await base.StartAsync(cancellationToken);
//     }
//
//     protected override async Task ExecuteAsync(CancellationToken cancellationToken)
//     {
//         try
//         {
//             while (!cancellationToken.IsCancellationRequested)
//             {
//                 await ProcessJob();
//                 await Task.Delay(500, cancellationToken);
//             }
//         }
//         catch (TaskCanceledException e)
//         {
//             Console.WriteLine("Check Trade Symbol Task Cancelled, Exit." + e.Message);
//         }
//     }
//
//     private async Task ProcessJob()
//     {
//         foreach (var trdSvc in _tradeAccountGroups)
//         {
//             switch (trdSvc.Platform)
//             {
//                 case PlatformTypes.MetaTrader4:
//                     await ProcessMt4(trdSvc.Id, trdSvc.Groups);
//                     break;
//
//                 case PlatformTypes.MetaTrader5:
//                     await ProcessMt5(trdSvc.Id, trdSvc.Groups);
//                     break;
//             }
//         }
//     }
//
//     private async Task ProcessMt4(int serviceId, List<string> groups)
//     {
//         await Task.Delay(0);
//         // TODO: Do not support MT4 for now
//     }
//
//     private async Task ProcessMt5(int serviceId, List<string> groups)
//     {
//         await Task.Delay(0);
//         // TODO: Do not support MT5 for now
//     }
//
//     private int GetTransId(long serviceId, string group)
//         => _transIdDict.TryGetValue($"{serviceId}_{group}", out var transId) ? transId : 0;
//
//     private void SetTransId(long serviceId, string group, int transId)
//         => _transIdDict[$"{serviceId}_{group}"] = transId;
//
//     private async Task<List<TradeServiceSimpleOptions>> GetTradeAccountGroups()
//     {
//         var items = await _tenantDbContext.TradeServices
//             .Select(x => new { x.Id, x.Platform, x.Configuration })
//             .ToListAsync();
//
//         return items.Select(x =>
//         {
//             var configString = string.IsNullOrEmpty(x.Configuration) ? "{}" : x.Configuration;
//             var config = JsonConvert.DeserializeObject<TradeServiceOptions>(configString);
//             return new TradeServiceSimpleOptions
//             {
//                 Id = x.Id,
//                 Platform = (PlatformTypes)x.Platform,
//                 Configuration = config is { Groups: not null } ? config : new TradeServiceOptions(),
//                 Groups = config is { Groups: not null } ? config.Groups : new List<string>()
//             };
//         }).ToList();
//     }
// }
//
// public class TradeServiceSimpleOptions
// {
//     public int Id { get; set; }
//     public PlatformTypes Platform { get; set; }
//     public TradeServiceOptions Configuration { get; set; } = new();
//     public List<string> Groups { get; set; } = new();
// }

