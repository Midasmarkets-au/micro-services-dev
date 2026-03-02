//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Bacera.Gateway.Web.Areas.Sales.Controllers;

//using M = Group;

//public class GroupController : SalesBaseController
//{
//    private readonly TenantDbContext _tenantDbContext;

//    public GroupController(TenantDbContext tenantDbContext
//    )
//    {
//        _tenantDbContext = tenantDbContext;
//    }

//    [HttpGet("o-{accountId:long}")]
//    public async Task<IActionResult> List(long accountId, [FromQuery] M.Criteria? criteria)
//    {
//        criteria ??= new M.Criteria();
//        criteria.AccountId = accountId;
//        criteria.PartyId = GetPartyId();
//        var items = await _tenantDbContext.Groups
//            .FilterBy(criteria)
//            .ToListAsync();
//        return Ok(Result<List<Group>, M.Criteria>.Of(items, criteria));
//    }

//    [HttpGet("o-{accountId:long}/{id:long}")]
//    public async Task<IActionResult> Get(long accountId, long id)
//    {
//        var item = await _tenantDbContext.Groups
//            .Where(x => x.AccountId == accountId)
//            .Where(x => x.Account.PartyId == GetPartyId())
//            .Where(x => x.Id == id)
//            .FirstOrDefaultAsync();
//        return item == null ? NotFound() : Ok(item);
//    }

//    [HttpDelete("o-{accountId:long}/{id:long}")]
//    public async Task<IActionResult> Delete(long accountId, long id)
//    {
//        var item = await _tenantDbContext.Groups
//            .Where(x => x.Account.PartyId == GetPartyId())
//            .Where(x => x.AccountId == accountId)
//            .Where(x => x.Id == id)
//            .FirstOrDefaultAsync();
//        if (item == null) return NotFound();

//        _tenantDbContext.Groups.Remove(item);
//        await _tenantDbContext.SaveChangesAsync();
//        return NoContent();
//    }

//    [HttpGet("o-{accountId:long}/{id:long}/account")]
//    public async Task<IActionResult> Accounts(long accountId, long id)
//    {
//        if (await IsAccountExistsForCurrentParty(accountId) == false) return NotFound();
//        var items = await _tenantDbContext.Accounts
//            .Where(x => x.AccountGroups.Any(y => y.GroupId == id))
//            .Where(x => x.SalesAccount != null && x.SalesAccount.PartyId == GetPartyId())
//            .OrderByDescending(x => x.Id)
//            .ToListAsync();

//        return Ok(Result<List<Account>>.Of(items));
//    }

//    [HttpPost("o-{accountId:long}")]
//    public async Task<IActionResult> Create(long accountId, [FromBody] M.CreateSpec spec)
//    {
//        if (await IsAccountExistsForCurrentParty(accountId) == false) return NotFound();
//        var group = new Group
//        {
//            Name = spec.Name,
//            AccountId = accountId,
//            Description = spec.Description,
//        };

//        _tenantDbContext.Groups.Add(group);
//        await _tenantDbContext.SaveChangesAsync();

//        var accountIds = _tenantDbContext.Accounts
//            .Where(x => x.SalesAccount != null && x.SalesAccount.PartyId == GetPartyId())
//            .Where(x => spec.AccountIds.Contains(x.Id))
//            .Select(x => x.Id)
//            .ToList();

//        var accountGroups = accountIds
//            .Distinct()
//            .Select(x => new AccountGroup
//            {
//                AccountId = x,
//                GroupId = group.Id,
//            }).ToList();

//        // avoid error when adding account which not exist
//        foreach (var accountGroup in accountGroups)
//        {
//            try
//            {
//                await _tenantDbContext.AccountGroups.AddAsync(accountGroup);
//                await _tenantDbContext.SaveChangesAsync();
//            }
//            catch (Exception)
//            {
//                //ignore
//            }
//        }

//        return Ok(group);
//    }

//    [HttpPost("o-{accountId:long}/{id:long}/account")]
//    public async Task<IActionResult> Append(long accountId, long id, [FromBody] M.UpdateAccountSpec spec)
//    {
//        if (await IsAccountExistsForCurrentParty(accountId) == false) return NotFound();
//        if (id != spec.Id) return BadRequest(Result.Error(ResultMessage.Group.IdNotMatch));

//        var group = await _tenantDbContext.Groups
//            .Where(x => x.Account.PartyId == GetPartyId())
//            .Where(x => x.AccountId == accountId)
//            .Where(x => x.Id == id)
//            .FirstOrDefaultAsync();
//        if (group == null) return NotFound();

//        var accountIds = _tenantDbContext.Accounts
//            .Where(x => x.SalesAccount != null && x.SalesAccount.PartyId == GetPartyId())
//            .Where(x => spec.AccountIds.Contains(x.Id))
//            .Select(x => x.Id)
//            .ToList();

//        var accountGroups = accountIds
//            .Distinct()
//            .Select(x => new AccountGroup
//            {
//                AccountId = x,
//                GroupId = id,
//            }).ToList();

//        // avoid error when adding account which not exist
//        var newIds = new List<long>();
//        foreach (var accountGroup in accountGroups)
//        {
//            try
//            {
//                await _tenantDbContext.AccountGroups.AddAsync(accountGroup);
//                await _tenantDbContext.SaveChangesAsync();
//                newIds.Add(accountGroup.AccountId);
//            }
//            catch (Exception)
//            {
//                //ignore
//            }
//        }

//        return newIds.Count > 0
//            ? Ok(Result<List<long>>.Of(newIds))
//            : BadRequest(Result.Error(ResultMessage.Group.NoAccountAdded));
//    }

//    [HttpDelete("o-{accountId:long}/{id:long}/account/{subaccountId:long}")]
//    public async Task<IActionResult> Remove(long id, long accountId, long subaccountId)
//    {
//        var accountGroup = await _tenantDbContext.AccountGroups
//            .Where(x => x.Account.PartyId == GetPartyId())
//            .Where(x => x.Group.AccountId == accountId)
//            .FirstOrDefaultAsync(x => x.GroupId == id && x.AccountId == subaccountId);
//        if (accountGroup == null) return NotFound();
//        _tenantDbContext.AccountGroups.Remove(accountGroup);
//        await _tenantDbContext.SaveChangesAsync();
//        return NoContent();
//    }

//    private async Task<bool> IsAccountExistsForCurrentParty(long accountId) =>
//        await _tenantDbContext.Accounts
//            .Where(x => x.Id == accountId)
//            .Where(x => x.PartyId == GetPartyId())
//            .AnyAsync();
//}