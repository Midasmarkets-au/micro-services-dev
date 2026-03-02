using Bacera.Gateway;
using Microsoft.EntityFrameworkCore;

namespace Cleaner;

public class BuildGroupFromPath
{
    private readonly TenantDbContext _tenantDbContext;

    public BuildGroupFromPath(TenantDbContext tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }

    public async Task BuildRepGroup()
    {
        Console.WriteLine(
            "=====================================================Build Rep group start=====================================================");
        var repAccountGroups = await _tenantDbContext.Accounts
            .Where(x => x.Role == (short)AccountRoleTypes.Rep)
            .Select(x => new Group
            {
                Type = (int)AccountGroupTypes.Rep,
                OwnerAccountId = x.Id,
                Name = "Rep-" + x.Uid,
                Description = ""
            })
            .ToListAsync();
        foreach (var group in repAccountGroups)
        {
            Console.WriteLine(group.Name);
        }

        Console.WriteLine(
            "=====================================================Build Rep group End=====================================================");
        // if (true)
        // {
        //     return;
        // }

        await _tenantDbContext.Groups.AddRangeAsync(repAccountGroups);
        await _tenantDbContext.SaveChangesAsync();
    }

    public async Task BuildSalesGroup()
    {
        Console.WriteLine(
            "=====================================================Build sales group start=====================================================");

        var accounts = await _tenantDbContext.Accounts
            // .Where(x => x.IsClosed != 1)
            .Where(x => x.Role == (short)AccountRoleTypes.Sales)
            .ToListAsync();

        var defaultRepGroup = await _tenantDbContext.Groups
            .Where(x => x.Type == (int)AccountGroupTypes.Rep)
            .Include(x => x.OwnerAccount)
            .FirstOrDefaultAsync(x => x.OwnerAccount.Uid == 51011101);

        if (defaultRepGroup == null)
            throw new Exception("Default rep group not found");

        foreach (var account in accounts)
        {
            Console.WriteLine(account.ReferPath);

            var uids = GetUidsTreeFromReferPath(account.ReferPath);
            switch (uids.Count)
            {
                case 0:
                    throw new FormatException(
                        $"Sales refer path error: uid count is {uids.Count}"
                    );
                case > 2:
                    throw new FormatException(
                        $"Sales refer path error: the uid count exceeds 2 which is {uids.Count}"
                    );
                case 1:
                    var uid = uids.First();
                    if (uid != account.Uid)
                        throw new FormatException(
                            $"Sales refer path error: no rep uid and the uid is not the sales account itself"
                        );
                    defaultRepGroup.Accounts.Add(account);
                    _tenantDbContext.Groups.Update(defaultRepGroup);
                    await _tenantDbContext.SaveChangesAsync();
                    continue;
                case 2:
                    if (uids.Last() != account.Uid)
                        throw new FormatException(
                            $"Sales refer path error: Rep Uid exists but the selfUid is not the sales account itself"
                        );
                    var repGroup = await _tenantDbContext.Groups
                        .Where(x => x.OwnerAccount.Uid == uids.First())
                        .Where(x => x.Type == (int)AccountGroupTypes.Rep)
                        .SingleOrDefaultAsync();
                    if (repGroup == null)
                        throw new Exception($"Rep group of ownerAccountUid:{uids.First()} not found");

                    repGroup.Accounts.Add(account);
                    _tenantDbContext.Groups.Update(repGroup);
                    await _tenantDbContext.SaveChangesAsync();

                    var salesGroup = new Group
                    {
                        OwnerAccountId = account.Id,
                        Type = (int)AccountGroupTypes.Sales,
                        Name = account.Code,
                        Description = ""
                    };

                    _tenantDbContext.Groups.Add(salesGroup);
                    await _tenantDbContext.SaveChangesAsync();
                    break;
            }
        }

        Console.WriteLine(
            "=====================================================Build sales group end=====================================================");
    }

    public async Task BuildAgentGroup()
    {
        Console.WriteLine(
            "=====================================================Build Agent group start=====================================================");
        var accounts = await _tenantDbContext.Accounts
            .Where(x => x.SalesAccountId != null)
            .Where(x => x.Role == (short)AccountRoleTypes.Agent)
            .OrderBy(x => x.ReferPath.Length)
            .ToListAsync();

        foreach (var account in accounts)
        {
            Console.WriteLine(account.ReferPath);

            var uids = GetUidsTreeFromReferPath(account.ReferPath);
            if (uids.Count < 3)
                throw new FormatException(
                    $"Agent refer path error: uid count is {uids.Count}"
                );
            var repUid = uids[0];
            var salesUid = uids[1];
            var selfUid = uids[2];
            if (selfUid != account.Uid)
                throw new FormatException(
                    $"Agent refer path error: selfUid is not the agent account itself"
                );

            var repGroup = await _tenantDbContext.Groups
                .Where(x => x.OwnerAccount.Uid == repUid)
                .Where(x => x.Type == (int)AccountGroupTypes.Rep)
                .SingleOrDefaultAsync();
            if (repGroup == null)
                throw new Exception($"Rep group of ownerAccountUid:{repUid} not found");
            repGroup.Accounts.Add(account);
            _tenantDbContext.Groups.Update(repGroup);
            await _tenantDbContext.SaveChangesAsync();

            var salesGroup = await _tenantDbContext.Groups
                .Where(x => x.OwnerAccount.Uid == salesUid)
                .Where(x => x.Type == (int)AccountGroupTypes.Sales)
                .SingleOrDefaultAsync();
            if (salesGroup == null)
                throw new Exception($"Sales group of ownerAccountUid:{salesUid} not found");
            salesGroup.Accounts.Add(account);
            _tenantDbContext.Groups.Update(salesGroup);
            await _tenantDbContext.SaveChangesAsync();

            var isTopIb = uids.Count == 3;
            if (isTopIb)
            {
                var agentSelfGroup = new Group
                {
                    OwnerAccountId = account.Id,
                    Type = (int)AccountGroupTypes.Agent,
                    Name = account.Code,
                    Description = ""
                };

                _tenantDbContext.Groups.Add(agentSelfGroup);
                await _tenantDbContext.SaveChangesAsync();
                continue;
            }

            var agentUid = uids[^2];
            var agentGroup = await _tenantDbContext.Groups
                .Where(x => x.OwnerAccount.Uid == agentUid)
                .Where(x => x.Type == (int)AccountGroupTypes.Agent)
                .SingleOrDefaultAsync();
            if (agentGroup == null)
                throw new Exception($"Agent group of ownerAccountUid:{agentUid} not found");

            agentGroup.Accounts.Add(account);
            _tenantDbContext.Groups.Update(agentGroup);
            await _tenantDbContext.SaveChangesAsync();
        }

        Console.WriteLine(
            "=====================================================Build Agent group end=====================================================");
    }

    public async Task AssignClientToGroups()
    {
        Console.WriteLine(
            "=====================================================Assign Client group start=====================================================");
        var accounts = await _tenantDbContext.Accounts
            .Where(x => x.SalesAccountId != null)
            .Where(x => x.Role == (short)AccountRoleTypes.Client)
            .OrderBy(x => x.ReferPath.Length)
            .ToListAsync();

        foreach (var account in accounts)
        {
            Console.WriteLine(account.ReferPath);

            var uids = GetUidsTreeFromReferPath(account.ReferPath);
            if (uids.Count < 3)
                throw new FormatException(
                    $"Client refer path error: uid count is {uids.Count}"
                );

            var repUid = uids[0];
            var salesUid = uids[1];
            var selfUid = uids.Last();
            if (selfUid != account.Uid)
                throw new FormatException(
                    $"Client refer path error: selfUid is not the client account itself"
                );

            var repGroup = await _tenantDbContext.Groups
                .Where(x => x.OwnerAccount.Uid == repUid)
                .Where(x => x.Type == (int)AccountGroupTypes.Rep)
                .SingleOrDefaultAsync();
            if (repGroup == null)
                throw new Exception($"Rep group of ownerAccountUid:{repUid} not found");
            repGroup.Accounts.Add(account);
            _tenantDbContext.Groups.Update(repGroup);
            await _tenantDbContext.SaveChangesAsync();

            var salesGroup = await _tenantDbContext.Groups
                .Where(x => x.OwnerAccount.Uid == salesUid)
                .Where(x => x.Type == (int)AccountGroupTypes.Sales)
                .SingleOrDefaultAsync();
            if (salesGroup == null)
                throw new Exception($"Sales group of ownerAccountUid:{salesUid} not found");
            salesGroup.Accounts.Add(account);
            _tenantDbContext.Groups.Update(salesGroup);
            await _tenantDbContext.SaveChangesAsync();

            var agentUid = uids[^2];
            var agentGroup = await _tenantDbContext.Groups
                .Where(x => x.OwnerAccount.Uid == agentUid)
                .Where(x => x.Type == (int)AccountGroupTypes.Agent)
                .SingleOrDefaultAsync();
            if (agentGroup == null)
                throw new Exception($"Agent group of ownerAccountUid:{agentUid} not found");

            agentGroup.Accounts.Add(account);
            _tenantDbContext.Groups.Update(agentGroup);
            await _tenantDbContext.SaveChangesAsync();
        }

        Console.WriteLine(
            "=====================================================Assign Client group end=====================================================");
    }

    private static List<long> GetUidsTreeFromReferPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        var parts = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

        var uids = new List<long>();
        foreach (var part in parts)
        {
            if (long.TryParse(part, out var uid))
                uids.Add(uid);
            else
                throw new FormatException($"The part '{part}' is not a valid long number.");
        }

        return uids;
    }

    private async Task<bool> RelationShipPreCheck()
    {
        var accountQuery = _tenantDbContext.Accounts;
        var accounts = await accountQuery
            .Where(x => x.Role == (int)AccountRoleTypes.Rep)
            .ToListAsync();
        foreach (var account in accounts)
        {
            var uids = GetUidsTreeFromReferPath(account.ReferPath);
            if (uids.Count == 1) continue;
            Console.WriteLine("Rep account refer path error accountId: =>" + account.Id);
            return false;
        }

        var salesAccounts = await accountQuery
            .Where(x => x.Role == (int)AccountRoleTypes.Sales)
            .ToListAsync();

        var groupCount = salesAccounts
            .GroupBy(x => x.Code)
            .Select(x => new
            {
                Code = x.Key,
                Count = x.Count()
            });
        foreach (var group in groupCount)
        {
            if (group.Count == 1) continue;
            Console.WriteLine("Sales code duplicated code: => " + group.Code);
            return false;
        }

        var repUids = new List<long>();
        foreach (var account in salesAccounts)
        {
            var uids = GetUidsTreeFromReferPath(account.ReferPath);
            if (uids.Count == 2)
            {
                repUids.Add(uids.First());
                continue;
            }

            Console.WriteLine("Sales account refer path error accountId: => " + account.Id);
            return false;
        }

        var repUidsForCheck = await accountQuery
            .Where(x => repUids.Contains(x.Uid))
            .Select(x => x.Uid)
            .ToListAsync();
        foreach (var ruid in repUids)
        {
            if (repUidsForCheck.Contains(ruid)) continue;

            Console.WriteLine("RepUid of sales not exists, repUid: => " + ruid);
            return false;
        }

        var agentAccounts = await accountQuery
            .Where(x => x.SalesAccountId != null)
            .Where(x => x.Role == (int)AccountRoleTypes.Agent || x.Role == (int)AccountRoleTypes.Client)
            .ToListAsync();

        groupCount = agentAccounts
            .Where(x => x.Role == (int)AccountRoleTypes.Agent)
            .GroupBy(x => x.Group)
            .Select(x => new
            {
                Code = x.Key,
                Count = x.Count()
            });

        foreach (var group in groupCount)
        {
            if (group.Count == 1) continue;
            Console.WriteLine("Agent group duplicated: => " + group.Code);
            return false;
        }

        repUids = new List<long>();
        var salesUids = new List<long>();
        var agentUids = new List<long>();
        foreach (var account in agentAccounts)
        {
            var uids = GetUidsTreeFromReferPath(account.ReferPath);
            if (uids.Count < 3)
            {
                Console.WriteLine("Agent account refer path error AccountId: => " + account.Id + " path: => " +
                                  account.ReferPath);
                return false;
            }

            for (int i = 0; i < uids.Count - 1; i++)
            {
                if (i == 0)
                {
                    repUids.Add(uids[i]);
                    continue;
                }

                if (i == 1)
                {
                    salesUids.Add(uids[i]);
                    continue;
                }

                if (i == 2)
                {
                    agentUids.Add(uids[i]);
                    continue;
                }
            }
        }

        repUidsForCheck = await accountQuery
            .Where(x => repUids.Contains(x.Uid))
            .Select(x => x.Uid)
            .ToListAsync();

        foreach (var ruid in repUids)
        {
            if (repUidsForCheck.Contains(ruid)) continue;

            Console.WriteLine("RepUid of agent and client not exists, RepUid: => " + ruid);
            return false;
        }

        var salesUidsForCheck = await accountQuery
            .Where(x => salesUids.Contains(x.Uid))
            .Select(x => x.Uid)
            .ToListAsync();

        foreach (var ruid in salesUidsForCheck)
        {
            if (salesUidsForCheck.Contains(ruid)) continue;

            Console.WriteLine("SalesUid of agent and client not exists SalesUid: => " + ruid);
            return false;
        }

        var agentUidsForCheck = await accountQuery
            .Where(x => agentUids.Contains(x.Uid))
            .Select(x => x.Uid)
            .ToListAsync();

        foreach (var ruid in agentUidsForCheck)
        {
            if (agentUidsForCheck.Contains(ruid)) continue;

            Console.WriteLine("Agent of agent and client not exists agentUid: => " + ruid);
            return false;
        }

        return true;
    }

    public async Task<(bool, string)> Run()
    {
        // if (!await relationShipPreCheck()) return (false, "BuildFailed");

        // await BuildRepGroup();
        await BuildSalesGroup();
        // await BuildAgentGroup();
        // await AssignClientToGroups();
        return (true, "BuildFinished");
    }
}