using Bacera.Gateway;
using Microsoft.EntityFrameworkCore;

namespace Cleaner;

public class GroupToPath
{
    private readonly TenantDbContext _tenantDbContext;

    public GroupToPath(TenantDbContext tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }


    // public async Task<bool> BuildAgentGroupHierarchyPathFromRoot(Account? rootAccount = null)
    // {
    //     rootAccount ??= new Account();
    //     var idx = 0;
    //     // any top ip account doesn't belong to any agent group
    //     var topIbAccounts = rootAccount.IsEmpty()
    //         ? await _tenantDbContext.Accounts
    //             .Where(x => x.Role == (short)AccountRoleTypes.Agent)
    //             .Where(x => x.Groups.All(g => g.Type != (int)AccountGroupTypes.Agent))
    //             .ToListAsync()
    //         : new List<Account> { rootAccount };
    //
    //     var queue = new Queue<Account>();
    //     var visitedAccountIds = new HashSet<long>();
    //     foreach (var account in topIbAccounts)
    //     {
    //         if (string.IsNullOrEmpty(account.ReferPath))
    //         {
    //             account.ReferPath = "." + account.Uid;
    //             _tenantDbContext.Accounts.Update(account);
    //         }
    //
    //         visitedAccountIds.Add(account.Id);
    //         queue.Enqueue(account);
    //     }
    //
    //     await _tenantDbContext.SaveChangesAsync();
    //
    //     while (queue.Any())
    //     {
    //         var account = queue.Dequeue();
    //         var childAccounts = await _tenantDbContext.Groups
    //             .Where(g => g.OwnerAccountId == account.Id)
    //             .SelectMany(g => g.Accounts)
    //             .Where(a => a.Role == (short)AccountRoleTypes.Agent || a.Role == (short)AccountRoleTypes.Client)
    //             .ToListAsync();
    //
    //         foreach (var childAccount in childAccounts
    //                      .Where(childAccount => !visitedAccountIds.Contains(childAccount.Id)))
    //         {
    //             idx++;
    //             visitedAccountIds.Add(account.Id);
    //             childAccount.ReferPath = account.ReferPath + "." + childAccount.Uid;
    //             _tenantDbContext.Accounts.Update(childAccount);
    //
    //             if (idx % 100 == 0)
    //                 Console.WriteLine($"{DateTime.Now:G} " + idx);
    //
    //             if (childAccount.Role == (short)AccountRoleTypes.Client)
    //                 continue;
    //
    //             queue.Enqueue(childAccount);
    //         }
    //
    //         await _tenantDbContext.SaveChangesAsync();
    //     }
    //
    //     return true;
    // }
}