// using Bacera.Gateway.Core.Types;
// using Bacera.Gateway.TradingData.Models;
// using Newtonsoft.Json;
// using Microsoft.EntityFrameworkCore;

// namespace Bacera.Gateway.TradingData;

// public partial class TradingDataImporter
// {
//     private readonly CentralDbContext _centralCtx;
//     private readonly string _tenantDbConnectionStringTemplate;

//     private SemaphoreSlim _semaphore;
//     private readonly int _threadCount;
//     private readonly Options? _options;
//     private const int DefaultThreadCount = 24;
//     private const int MinAccountBatchSize = 10;
//     private const int MaxAccountBatchSize = 100;

//     public TradingDataImporter(string centralDbConnectionString, string tenantDbConnectionStringStringTemplate,
//         Options? options = null)
//     {
//         _options = options;
//         _semaphore = new SemaphoreSlim(DefaultThreadCount);
//         _threadCount = _options?.Thread ?? DefaultThreadCount;
//         _tenantDbConnectionStringTemplate = tenantDbConnectionStringStringTemplate;
//         _centralCtx = new CentralDbContext(getCentralDbContextOption(centralDbConnectionString));
//     }

//     /// <summary>
//     /// Import Trade data from MT4 server, from the last import time to now.
//     /// </summary>
//     /// <param name="from"></param>
//     public async Task<List<ImportResult>> ImportSinceAsync(DateTime? from = null)
//     {
//         _semaphore = new SemaphoreSlim(_threadCount);
//         var tenants = await getTenantsByOptions();
//         var results = new List<ImportResult>();
//         foreach (var tenant in tenants)
//         {
//             onTenantChanged(new TenantChangedEventArgs(tenant.Name, tenant.Id, tenant.DatabaseName));

//             var tenantDbContext = new TenantDbContext(getTenantDbContextOption(tenant.DatabaseName));
//             var tradeServices = await getMt4TradeServices(tenantDbContext);
//             foreach (var tradeService in tradeServices)
//             {
//                 var result = await tradeServerImportByDateTime(tenant, tenantDbContext, tradeService, from);
//                 results.Add(result);
//             }
//         }

//         return results;
//     }

//     /// <summary>
//     /// Import trade data by foreach all trade accounts
//     /// </summary>
//     public async Task<List<ImportResult>> Import()
//     {
//         _semaphore = new SemaphoreSlim(_threadCount);
//         var tenants = await getTenantsByOptions();
//         var results = new List<ImportResult>();

//         foreach (var tenant in tenants)
//         {
//             onTenantChanged(new TenantChangedEventArgs(tenant.Name, tenant.Id, tenant.DatabaseName));

//             var tenantDbContext = new TenantDbContext(getTenantDbContextOption(tenant.DatabaseName));

//             var tradeServices = await getTradeServices(tenantDbContext);
//             foreach (var tradeService in tradeServices)
//             {
//                 var result = await tradeServerImport(tenant, tradeService, tenantDbContext);
//                 results.Add(result);
//             }

//             // Demo Accounts
//             var demoTradeServices = await getDemoTradeServices(tenantDbContext);
//             foreach (var tradeService in demoTradeServices)
//             {
//                 var resultDemo = await demoTradeServerImport(tenant, tenantDbContext, tradeService);
//                 results.Add(resultDemo);
//             }
//         }

//         return results;
//     }

//     private async Task<ImportResult> tradeServerImportByDateTime(Tenant tenant, TenantDbContext tenantDbContext,
//         TradeService tradeService, DateTime? from)
//     {
//         var result = ImportResult.Create(tenant.Id, tenant.Name, tenant.DatabaseName, tradeService.Id,
//             tradeService.Name);

//         var traderServiceOptions = tradeService.GetOptions<TradeServiceOptions>();

//         var eventArgs = new TradeServerChangedEventArgs(
//             Enum.GetName(typeof(PlatformTypes), tradeService.Platform) ?? "Unknown Platform", tradeService.Id,
//             100000000,
//             tradeService.Name);

//         if (!traderServiceOptions.IsDatabaseValidated())
//         {
//             const string message = "No database configuration found or validated";
//             eventArgs.TotalAccount = 0;
//             eventArgs.AdditionalInfo += message;
//             onTradeServerChanged(eventArgs);
//             result.Note = message;
//             return result;
//         }

//         DateTime fromDt;
//         if (from.HasValue)
//         {
//             fromDt = from.Value;
//         }
//         else
//         {
//             var syncedOn = await getTradeServiceSyncOn(tenantDbContext, tradeService.Id);
//             fromDt = syncedOn ?? DateTime.UtcNow.AddHours(-1);
//         }

//         fromDt = fromDt.AddMinutes(-10);
//         var now = DateTime.UtcNow;
//         var dataConvertor = new MetaTradeDataConverter(traderServiceOptions.Database!.ConnectionString);
//         var count = await dataConvertor.CountTransactionByRangeAsync(fromDt, now,
//             traderServiceOptions.Database.TradeTableName);
//         eventArgs.TotalAccount = count;
//         eventArgs.AdditionalInfo += $" from {fromDt}";
//         result.TotalRecords = count;
//         result.Note += $" from {fromDt}";

//         onTradeServerChanged(eventArgs);

//         var taskId = -1;
//         var batchSize = 1000;

//         var tasks = new List<Task>();
//         var page = 0;
//         var accountIds = new List<long>();
//         while (true)
//         {
//             var trans = await dataConvertor.GetTransactionsByRangeAsync(fromDt, now, page, batchSize,
//                 traderServiceOptions.Database.TradeTableName);
//             page++;
//             if (!trans.Any())
//                 break;

//             var transByLogin = trans.GroupBy(x => x.AccountNumber)
//                 .Select(x => new { AccountNumber = x.Key, Transactions = x.ToList() });

//             foreach (var item in transByLogin)
//             {
//                 taskId++;

//                 var account = tenantDbContext.TradeAccounts
//                     .FirstOrDefault(
//                         x => x.AccountNumber == item.AccountNumber && x.ServiceId == tradeService.Id);
//                 if (account == null)
//                     continue;
//                 accountIds.Add(account.Id);

//                 var transactions = item.Transactions.Select(x =>
//                 {
//                     x.ServiceId = tradeService.Id;
//                     x.TradeAccountId = account.Id;
//                     return x;
//                 }).ToList();

//                 if (tasks.Count >= _threadCount)
//                 {
//                     var finishedTask = await Task.WhenAny(tasks);
//                     tasks.Remove(finishedTask);
//                 }

//                 await _semaphore.WaitAsync();
//                 var id = taskId;
//                 var task = Task.Run(async () =>
//                 {
//                     try
//                     {
//                         var (inserted, updated) = await batchTransactionProcessing(id, tenant.DatabaseName,
//                             traderServiceOptions,
//                             account, transactions, tradeService);
//                         result.Inserted += inserted;
//                         result.Updated += updated;
//                     }
//                     finally
//                     {
//                         _semaphore.Release();
//                     }
//                 });
//                 tasks.Add(task);
//             }
//         }

//         await Task.WhenAll(tasks);
//         dataConvertor.Dispose();
//         await setTradeServiceSyncOn(tenantDbContext, tradeService.Id, now);
//         await updateAccountStatues(tenantDbContext, accountIds.Distinct().ToList(), now);
//         return result;
//     }

//     private async Task<ImportResult> tradeServerImport(Tenant tenant, TradeService tradeService,
//         TenantDbContext tenantDbContext)
//     {
//         var traderServiceOptions = tradeService.GetOptions<TradeServiceOptions>();

//         var eventArgs = new TradeServerChangedEventArgs(
//             Enum.GetName(typeof(PlatformTypes), tradeService.Platform) ?? "Unknown Platform", tradeService.Id,
//             0,
//             tradeService.Name);

//         var result = ImportResult.Create(tenant.Id, tenant.Name, tenant.DatabaseName, tradeService.Id,
//             tradeService.Name);

//         if (!traderServiceOptions.IsDatabaseValidated())
//         {
//             var message = "No database configuration found or validated";
//             eventArgs.AdditionalInfo += message;
//             onTradeServerChanged(eventArgs);
//             result.Note = message;
//             return result;
//         }

//         var accountCount = await getAccountCount(tenantDbContext, tradeService.Id, _options);
//         eventArgs.TotalAccount = accountCount;

//         onTradeServerChanged(eventArgs);

//         if (accountCount == 0)
//         {
//             result.Note = "Not account found.";
//             return result;
//         }

//         var taskId = -1;
//         var currentIndex = 0;
//         var batchSize = calculateBatchSize(accountCount);
//         var tasks = new List<Task>();
//         while (true)
//         {
//             var accountIds = await getAccountIdList(tenantDbContext, tradeService.Id, currentIndex, batchSize,
//                 _options);
//             if (!accountIds.Any())
//                 break;

//             taskId++;
//             currentIndex += batchSize;

//             if (tasks.Count >= _threadCount)
//             {
//                 var finishedTask = await Task.WhenAny(tasks);
//                 tasks.Remove(finishedTask);
//             }

//             await _semaphore.WaitAsync();
//             var id = taskId;
//             var task = Task.Run(async () =>
//             {
//                 try
//                 {
//                     var (total, inserted, updated) = await batchProcessing(id, tenant.DatabaseName,
//                         traderServiceOptions, accountIds,
//                         tradeService);
//                     result.TotalRecords += total;
//                     result.Inserted += inserted;
//                     result.Updated += updated;
//                 }
//                 finally
//                 {
//                     _semaphore.Release();
//                 }
//             });
//             tasks.Add(task);
//         }

//         await Task.WhenAll(tasks);
//         return result;
//     }

//     private async Task<ImportResult> demoTradeServerImport(Tenant tenant, TenantDbContext tenantDbContext,
//         TradeService tradeService)
//     {
//         var traderServiceOptions = tradeService.GetOptions<TradeServiceOptions>();
//         var result = ImportResult.Create(tenant.Id, tenant.Name, tenant.DatabaseName, tradeService.Id,
//             tradeService.Name);
//         var eventArgs = new TradeServerChangedEventArgs(
//             Enum.GetName(typeof(PlatformTypes), tradeService.Platform) ?? "Unknown Platform", tradeService.Id,
//             0,
//             tradeService.Name);

//         if (!traderServiceOptions.IsDatabaseValidated())
//         {
//             var message = "No database configuration found or validated";
//             eventArgs.AdditionalInfo += message;
//             onTradeServerChanged(eventArgs);
//             result.Note = message;
//             return result;
//         }

//         var accountCount = await getDemoAccountCount(tenantDbContext, tradeService.Id);

//         onTradeServerChanged(eventArgs);

//         if (accountCount == 0)
//         {
//             result.Note = "Not account found.";
//             return result;
//         }

//         var taskId = -1;
//         var currentIndex = 0;
//         var demoTasks = new List<Task>();
//         var batchSize = calculateBatchSize(accountCount);

//         while (true)
//         {
//             var accountIds = await getDemoAccountIdList(tenantDbContext, tradeService.Id, currentIndex,
//                 batchSize,
//                 _options);
//             if (!accountIds.Any())
//                 break;

//             taskId++;
//             currentIndex += batchSize;

//             if (demoTasks.Count >= _threadCount)
//             {
//                 var finishedTask = await Task.WhenAny(demoTasks);
//                 demoTasks.Remove(finishedTask);
//             }

//             await _semaphore.WaitAsync();
//             var id = taskId;
//             var demoTask = Task.Run(async () =>
//             {
//                 try
//                 {
//                     var (batchTotal, _, _) = await demoBatchProcessing(id, tenant.DatabaseName, traderServiceOptions,
//                         accountIds,
//                         tradeService);
//                     result.TotalRecords += batchTotal;
//                 }
//                 finally
//                 {
//                     _semaphore.Release();
//                 }
//             });
//             demoTasks.Add(demoTask);
//         }

//         await Task.WhenAll(demoTasks);
//         return result;
//     }

//     public int GetThreadCount() => _threadCount;

//     private async Task<List<Tenant>> getTenantsByOptions() =>
//         await _centralCtx.Tenants
//             .Where(x => _options == null || _options.TenantId == null || x.Id.Equals(_options.TenantId))
//             .OrderBy(x => x.Id)
//             .ToListAsync();

//     private async Task<DateTime?> getTradeServiceSyncOn(TenantDbContext tenantDbContext, long serviceId)
//     {
//         var item = await tenantDbContext.Supplements
//             .Where(x => x.Type == (short)SupplementTypes.TradeServiceSyncedOn)
//             .Where(x => x.RowId == serviceId)
//             .SingleOrDefaultAsync();

//         if (item == null) return null;
//         var timeStamp = JsonConvert.DeserializeObject<Supplement.TradeServiceSyncOn>(item.Data);
//         return timeStamp?.SyncedOn;
//     }

//     private async Task setTradeServiceSyncOn(TenantDbContext tenantDbContext, long serviceId, DateTime syncedOn)
//     {
//         var item = await tenantDbContext.Supplements.Where(x => x.Type == (short)SupplementTypes.TradeServiceSyncedOn)
//             .Where(x => x.RowId == serviceId).SingleOrDefaultAsync();
//         if (item == null)
//         {
//             var supplement = Supplement.Build(SupplementTypes.TradeServiceSyncedOn, serviceId,
//                 JsonConvert.SerializeObject(new Supplement.TradeServiceSyncOn { SyncedOn = syncedOn }));
//             tenantDbContext.Supplements.Add(supplement);
//         }
//         else
//         {
//             item.Data = JsonConvert.SerializeObject(new Supplement.TradeServiceSyncOn { SyncedOn = syncedOn });
//             item.UpdatedOn = DateTime.UtcNow;
//             tenantDbContext.Supplements.Update(item);
//         }

//         await tenantDbContext.SaveChangesAsync();
//     }

//     private static DbContextOptions<CentralDbContext> getCentralDbContextOption(string centralDbConnection)
//     {
//         var options = new DbContextOptionsBuilder<CentralDbContext>();
//         options.UseNpgsql(centralDbConnection, optionsBuilder =>
//         {
//             // optionsBuilder.EnableRetryOnFailure(3);
//             optionsBuilder.CommandTimeout(10);
//         });
//         options.EnableDetailedErrors();
//         return options.Options;
//     }

//     private DbContextOptions<TenantDbContext> getTenantDbContextOption(string tenantDatabaseName)
//     {
//         var options = new DbContextOptionsBuilder<TenantDbContext>();
//         var connectionString = _tenantDbConnectionStringTemplate.Replace("{{DATABASE}}", tenantDatabaseName);
//         options.UseNpgsql(connectionString, optionsBuilder =>
//         {
//             // optionsBuilder.EnableRetryOnFailure(3);
//             optionsBuilder.CommandTimeout(10);
//         });
//         options.EnableDetailedErrors();
//         return options.Options;
//     }

//     private async Task<List<TradeService>> getTradeServices(TenantDbContext tenantDbContext) =>
//         await tenantDbContext.TradeServices
//             .Where(x => _options == null
//                         || _options.TradeServiceId == null || x.Id.Equals(_options.TradeServiceId))
//             .Where(x => x.Platform == (short)PlatformTypes.MetaTrader4
//                         || x.Platform == (short)PlatformTypes.MetaTrader5)
//             .OrderBy(x => x.Id)
//             .ToListAsync();

//     private async Task<List<TradeService>> getMt4TradeServices(TenantDbContext tenantDbContext) =>
//         await tenantDbContext.TradeServices
//             .Where(x => _options == null
//                         || _options.TradeServiceId == null || x.Id.Equals(_options.TradeServiceId))
//             .Where(x => x.Platform == (short)PlatformTypes.MetaTrader4)
//             .OrderBy(x => x.Id)
//             .ToListAsync();

//     private static async Task<int> getAccountCount(TenantDbContext tenantDbContext, int tradeServiceId,
//         Options? options = null)
//     {
//         var query = getAccountFetchQuery(tenantDbContext, tradeServiceId, options);
//         return await query.CountAsync();
//     }

//     private static async Task<List<Tuple<long, short>>> getAccountIdList(TenantDbContext tenantDbContext,
//         int tradeServiceId,
//         int currentIndex, int pageSize, Options? options = null)
//     {
//         var query = getAccountFetchQuery(tenantDbContext, tradeServiceId, options);
//         return await query
//             // .Include(x => x.TradeAccountStatus)
//             .OrderBy(x => x.Id)
//             .Skip(currentIndex)
//             .Take(pageSize)
//             .AsNoTracking()
//             .Select(x => Tuple.Create(x.Id, x.IdNavigation.Status))
//             .ToListAsync();
//     }

//     private static IQueryable<TradeAccount> getAccountFetchQuery(TenantDbContext tenantDbContext, int tradeServiceId,
//         Options? options)
//     {
//         var query = tenantDbContext.TradeAccounts
//             .Where(x => x.ServiceId.Equals(tradeServiceId));

//         if (options is { AccountNumbers: not null } && options.AccountNumbers.Any())
//             query = query.Where(x => options.AccountNumbers.Contains(x.AccountNumber));

//         if (options is { AccountIds: not null } && options.AccountIds.Any())
//             query = query.Where(x => options.AccountIds.Contains(x.Id));

//         return query;
//     }

//     private static async Task updateAccountStatues(TenantDbContext tenantDbContext,
//         ICollection<long> accountIds, DateTime now)
//     {
//         var accounts = await tenantDbContext.TradeAccounts
//             .Where(x => accountIds.Contains(x.Id))
//             .ToListAsync();
//         foreach (var account in accounts)
//         {
//             account.LastSyncedOn = now;
//             account.UpdatedOn = now;
//             tenantDbContext.TradeAccounts.Update(account);
//         }

//         await tenantDbContext.SaveChangesAsync();
//     }

//     private async Task<Tuple<int, int>> batchTransactionProcessing(int taskId, string tenantDatabaseName,
//         TradeServiceOptions traderServiceOptions, TradeAccount account,
//         ICollection<TradeTransaction> transactions, TradeService tradeService)
//     {
//         await using var tenantDbContext = new TenantDbContext(getTenantDbContextOption(tenantDatabaseName));

//         var dataConverter = new MetaTradeDataConverter(traderServiceOptions.Database!.ConnectionString);
//         var userTable = traderServiceOptions.Database!.UserTableName;

//         var i = 0;
//         var status = await dataConverter.GetStatusAsync(account.AccountNumber, userTable);
//         dataConverter.Dispose();

//         var eventArgs = new AccountProcessEventArgs(tradeService.Id, account.AccountNumber, i, transactions.Count,
//             taskId);
//         // Login not found on the server
//         if (status == null)
//             return Tuple.Create(0, 0);

//         //onAccountProcessStarted(eventArgs);

//         var updateCount = 0;
//         var insertCount = 0;

//         var counter = 0;
//         foreach (var trans in transactions)
//         {
//             var (inserted, updated) = await createOrUpdateTransaction(tenantDbContext, trans);
//             if (inserted) insertCount++;
//             else if (updated) updateCount++;
//             counter++;
//             if (counter % 100 == 0)
//             {
//                 onProgressChanged(new ProgressChangedEventArgs(eventArgs, $"Trans {counter} of {transactions.Count} "));
//             }

//             i++;
//         }

//         await tenantDbContext.SaveChangesAsync();

//         await updateAccountStatus(tenantDbContext, account, status);

//         if (transactions.Count > 10)
//             onProgressChanged(new ProgressChangedEventArgs(eventArgs,
//                 $"Trans {transactions.Count} saved"));

//         onTransactionProcessed(new TransactionProcessedEventArgs
//             { ServiceId = tradeService.Id, Updated = updateCount, Inserted = insertCount });
//         onAccountProcessCompleted(
//             new AccountProcessEventArgs(tradeService.Id, account.AccountNumber, i, transactions.Count,
//                 taskId));
//         return Tuple.Create(insertCount, updateCount);
//     }

//     private async Task<Tuple<int, int, int>> batchProcessing(int taskId, string tenantDatabaseName,
//         TradeServiceOptions traderServiceOptions,
//         ICollection<Tuple<long, short>> accountIds, TradeService tradeService)
//     {
//         await using var tenantDbContext = new TenantDbContext(getTenantDbContextOption(tenantDatabaseName));
//         var dataConverter = new MetaTradeDataConverter(traderServiceOptions.Database!.ConnectionString);

//         var userTable = traderServiceOptions.Database.UserTableName;
//         var tradeTable = traderServiceOptions.Database.TradeTableName;


//         var ids = accountIds
//             .Where(a => a.Item2 == (short)AccountStatusTypes.Activate)
//             .Select(a => a.Item1).ToList();

//         var inactivatedCount = accountIds.Count(x => x.Item2 != (short)AccountStatusTypes.Activate);

//         onAccountProcessStarted(new AccountProcessEventArgs(tradeService.Id, 0,
//             inactivatedCount, accountIds.Count, taskId));

//         var accounts = await tenantDbContext.TradeAccounts
//             .Where(x => ids.Contains(x.Id))
//             .ToListAsync();


//         var i = inactivatedCount;
//         onAccountProcessCompleted(
//             new AccountProcessEventArgs(tradeService.Id, 0, i, accountIds.Count,
//                 taskId));

//         if (inactivatedCount == accountIds.Count)
//             return Tuple.Create(0, 0, 0);

//         var total = 0;
//         var batchInserted = 0;
//         var batchUpdated = 0;
//         foreach (var account in accounts)
//         {
//             var status = await dataConverter.GetStatusAsync(account.AccountNumber, userTable);

//             // Login not found on the server
//             if (status == null)
//             {
//                 onAccountProcessStarted(new AccountProcessEventArgs(tradeService.Id, account.AccountNumber,
//                     i, accountIds.Count, taskId));

//                 // Set Account status to Inactivated;
//                 await inactivateAccountAsync(tenantDbContext, account);

//                 onAccountRemoved(new AccountRemovedEventArgs(tradeService.Id, account.AccountNumber, taskId));

//                 i++;
//                 onAccountProcessCompleted(new AccountProcessEventArgs(tradeService.Id, account.AccountNumber,
//                     i, accountIds.Count, taskId,
//                     $" Login not found on {Enum.GetName(typeof(PlatformTypes), tradeService.Platform)} Server [{tradeService.Id}]"));
//                 continue;
//             }

//             var eventArgs = new AccountProcessEventArgs(tradeService.Id, account.AccountNumber, i, accountIds.Count,
//                 taskId);

//             onAccountProcessStarted(eventArgs);

//             var now = DateTime.UtcNow;

//             var transPage = 0;
//             var transPageSize = 100;

//             var updateCount = 0;
//             var insertCount = 0;

//             var savedCount = 0;
//             while (true)
//             {
//                 var msg = "Fetching trans ";
//                 if (transPage > 0)
//                     msg += $"p:{transPage + 1} ";
//                 if (savedCount > 0)
//                     msg += " " + savedCount + " saved";

//                 onProgressChanged(new ProgressChangedEventArgs(eventArgs, msg));
//                 var transactions =
//                     await dataConverter.GetTransactionsAsync(account, now, transPage, transPageSize, tradeTable);
//                 if (!transactions.Any())
//                     break;

//                 onProgressChanged(new ProgressChangedEventArgs(eventArgs,
//                     $"Fetched {transactions.Count} trans"));

//                 total += transactions.Count;
//                 transPage++;
//                 var counter = 0;
//                 foreach (var trans in transactions)
//                 {
//                     var (inserted, updated) = await createOrUpdateTransaction(tenantDbContext, trans);
//                     if (inserted) insertCount++;
//                     else if (updated) updateCount++;
//                     counter++;
//                     if (counter % 100 == 0)
//                     {
//                         onProgressChanged(new ProgressChangedEventArgs(eventArgs,
//                             $"Trans {counter} of {transactions.Count} " +
//                             (transPage > 1 ? "(" + (transPage + 1) + ")" : "")));
//                     }
//                 }

//                 if (transactions.Count > 10)
//                     onProgressChanged(new ProgressChangedEventArgs(eventArgs,
//                         $"Trans {transactions.Count} processing"));

//                 savedCount = await tenantDbContext.SaveChangesAsync();
//             }

//             onTransactionProcessed(new TransactionProcessedEventArgs
//                 { ServiceId = tradeService.Id, Updated = updateCount, Inserted = insertCount });

//             await updateAccountStatus(tenantDbContext, account, status);

//             await updateAccountLastSync(tenantDbContext, account.Id, now);

//             await tenantDbContext.SaveChangesAsync();

//             i++;
//             onAccountProcessCompleted(
//                 new AccountProcessEventArgs(tradeService.Id, account.AccountNumber, i, accountIds.Count,
//                     taskId));
//             batchInserted += insertCount;
//             batchUpdated += updateCount;
//         }

//         dataConverter.Dispose();
//         //return removedAccountList;
//         return Tuple.Create(total, batchInserted, batchUpdated);
//     }

//     private static async Task<Tuple<bool, bool>> createOrUpdateTransaction(TenantDbContext tenantDbContext,
//         TradeTransaction fetchedTransaction)
//     {
//         var inserted = false;
//         var updated = false;
//         var exists = await tenantDbContext.TradeTransactions
//             .Where(x => x.ServiceId.Equals(fetchedTransaction.ServiceId))
//             .Where(x => x.Ticket.Equals(fetchedTransaction.Ticket))
//             .Select(x => new TradeTransaction
//                 { Id = x.Id, CreatedOn = x.CreatedOn, Status = x.Status, TimeStamp = x.TimeStamp })
//             .AsNoTracking()
//             .FirstOrDefaultAsync();

//         if (exists?.TimeStamp == fetchedTransaction.TimeStamp)
//             return Tuple.Create(false, false);

//         if (exists != null)
//         {
//             fetchedTransaction.Id = exists.Id;
//             fetchedTransaction.UpdatedOn = DateTime.UtcNow;
//             fetchedTransaction.CreatedOn = DateTime.SpecifyKind(exists.CreatedOn, DateTimeKind.Utc);
//             fetchedTransaction.Status = exists.Status;
//             var existingEntity = tenantDbContext.TradeTransactions.Local.SingleOrDefault(x => x.Id == exists.Id);
//             if (existingEntity != null)
//             {
//                 tenantDbContext.TradeTransactions.Entry(existingEntity).CurrentValues.SetValues(fetchedTransaction);
//             }
//             else
//             {
//                 //tenantDbContext.TradeTransactions.Update(fetchedTransaction);
//                 tenantDbContext.TradeTransactions.Attach(fetchedTransaction);
//                 tenantDbContext.Entry(fetchedTransaction).State = EntityState.Modified;
//             }

//             updated = true;
//         }
//         else
//         {
//             fetchedTransaction.UpdatedOn = fetchedTransaction.CreatedOn = DateTime.UtcNow;
//             if (fetchedTransaction.RebateUnqualified())
//                 fetchedTransaction.Status = (short)TradeStatusType.RebateUnqualified;
//             await tenantDbContext.TradeTransactions.AddAsync(fetchedTransaction);
//             inserted = true;
//         }

//         return Tuple.Create(inserted, updated);
//     }

//     private async Task<int> getDemoAccountCount(TenantDbContext tenantDbContext, int tradeServiceId)
//     {
//         var query = tenantDbContext.TradeDemoAccounts
//             .Where(x => x.ServiceId.Equals(tradeServiceId));

//         if (_options is { AccountNumbers: not null } && _options.AccountNumbers.Any())
//             query = query.Where(x => _options.AccountNumbers.Contains(x.AccountNumber));
//         if (_options is { AccountIds: not null } && _options.AccountIds.Any())
//             query = query.Where(x => _options.AccountIds.Contains(x.Id));

//         return await query.CountAsync();
//     }

//     private async Task<List<TradeService>> getDemoTradeServices(TenantDbContext tenantDbContext) =>
//         await tenantDbContext.TradeServices
//             .Where(x => _options == null
//                         || _options.TradeServiceId == null || x.Id.Equals(_options.TradeServiceId))
//             .Where(x => x.Platform == (short)PlatformTypes.MetaTrader4Demo
//                         || x.Platform == (short)PlatformTypes.MetaTrader5Demo)
//             .OrderBy(x => x.Id)
//             .ToListAsync();

//     private static async Task<List<long>> getDemoAccountIdList(TenantDbContext tenantDbContext,
//         int tradeServiceId,
//         int currentIndex, int pageSize, Options? options = null)
//     {
//         var query = tenantDbContext.TradeDemoAccounts
//             .Where(x => x.ServiceId.Equals(tradeServiceId));

//         if (options is { AccountNumbers: not null } && options.AccountNumbers.Any())
//             query = query.Where(x => options.AccountNumbers.Contains(x.AccountNumber));
//         if (options is { AccountIds: not null } && options.AccountIds.Any())
//             query = query.Where(x => options.AccountIds.Contains(x.Id));

//         return await query
//             // .Include(x => x.TradeAccountStatus)
//             .OrderBy(x => x.Id)
//             .Skip(currentIndex)
//             .Take(pageSize)
//             .AsNoTracking()
//             .Select(x => x.Id)
//             .ToListAsync();
//     }

//     private async Task<Tuple<int, int, int>> demoBatchProcessing(int taskId, string tenantDatabaseName,
//         TradeServiceOptions traderServiceOptions,
//         ICollection<long> accountIds, TradeService tradeService)
//     {
//         var tenantDbContext = new TenantDbContext(getTenantDbContextOption(tenantDatabaseName));
//         var dataConverter = new MetaTradeDataConverter(traderServiceOptions.Database!.ConnectionString);
//         var userTable = traderServiceOptions.Database.UserTableName;
//         var accounts = await tenantDbContext.TradeDemoAccounts
//             .Where(x => accountIds.Contains(x.Id))
//             .ToListAsync();

//         var i = 0;
//         foreach (var account in accounts)
//         {
//             i++;
//             var status = await dataConverter.GetStatusAsync(account.AccountNumber, userTable);

//             // Login not found on the server
//             if (status == null)
//             {
//                 onAccountProcessStarted(new AccountProcessEventArgs(tradeService.Id, account.AccountNumber,
//                     i, accounts.Count, taskId));
//                 onAccountProcessCompleted(new AccountProcessEventArgs(tradeService.Id, account.AccountNumber,
//                     i, accounts.Count, taskId,
//                     $" Login not found on {Enum.GetName(typeof(PlatformTypes), tradeService.Platform)} Server [{tradeService.Id}]"));
//                 continue;
//             }

//             onAccountProcessStarted(new AccountProcessEventArgs(tradeService.Id, account.AccountNumber, i,
//                 accounts.Count, taskId));

//             account.UpdatedOn = DateTime.UtcNow;
//             account.Leverage = status.Leverage;
//             account.Balance = status.Balance;
//             account.CurrencyId = (int)CurrencyTypes.USD;
//             if (Enum.TryParse(status.Currency, out CurrencyTypes currencyId))
//             {
//                 account.CurrencyId = (int)currencyId;
//             }

//             tenantDbContext.TradeDemoAccounts.Update(account);
//             await tenantDbContext.SaveChangesAsync();

//             onAccountProcessCompleted(new AccountProcessEventArgs(tradeService.Id, account.AccountNumber, i,
//                 accounts.Count, taskId));
//         }

//         dataConverter.Dispose();
//         //return removedAccountList;
//         return Tuple.Create(accounts.Count, 0, 0);
//     }

//     private int calculateBatchSize(int accountCount)
//         => Math.Min(
//             MaxAccountBatchSize,
//             Math.Max(MinAccountBatchSize,
//                 (int)Math.Round(accountCount / (decimal)_threadCount, 0)
//             )
//         );

//     private static async Task inactivateAccountAsync(TenantDbContext tenantDbContext, TradeAccount account)
//     {
//         var accountEntryStatus = await tenantDbContext.Accounts.SingleAsync(x => x.Id == account.Id);
//         accountEntryStatus.Status = (short)AccountStatusTypes.Inactivated;
//         accountEntryStatus.UpdatedOn = DateTime.UtcNow;
//         tenantDbContext.Accounts.Update(accountEntryStatus);
//         await tenantDbContext.SaveChangesAsync();
//     }

//     private static async Task updateAccountStatus(TenantDbContext tenantDbContext, TradeAccount account,
//         TradeAccountStatus status)
//     {
//         status.Id = account.Id;
//         status.UpdatedOn = DateTime.UtcNow;
//         var statusEntry = await tenantDbContext.TradeAccountStatuses
//             .Select(x => new { Id = x.Id, x.ModifiedOn })
//             .SingleOrDefaultAsync(x => x.Id == status.Id);

//         if (statusEntry != null && statusEntry.ModifiedOn < status.ModifiedOn)
//         {
//             tenantDbContext.TradeAccountStatuses.Update(status);
//             await tenantDbContext.SaveChangesAsync();
//             return;
//         }

//         if (statusEntry == null)
//         {
//             await tenantDbContext.TradeAccountStatuses.AddAsync(status);
//             await tenantDbContext.SaveChangesAsync();
//         }
//     }

//     private static async Task updateAccountLastSync(TenantDbContext tenantDbContext, long accountId, DateTime now)
//     {
//         var accountEntry = await tenantDbContext.TradeAccounts.SingleAsync(x => x.Id == accountId);
//         accountEntry.LastSyncedOn = now;
//         tenantDbContext.TradeAccounts.Update(accountEntry);
//         await tenantDbContext.SaveChangesAsync();
//     }

//     [Serializable]
//     public class Options
//     {
//         public long? TenantId { get; set; }
//         public int? TradeServiceId { get; set; }
//         public IEnumerable<long>? AccountNumbers { get; set; }
//         public IEnumerable<long>? AccountIds { get; set; }
//         public int? Thread { get; set; }
//     }
// }