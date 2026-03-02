using OpenIddict.Validation.AspNetCore;
﻿using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

using M = Audit;

[Tags("Tenant/Audit")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class AuditController(AuthDbContext authDbContext, TenantDbContext tenantDbContext) : TenantBaseController
{
    /// <summary>
    /// Audit pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<AuditResponseModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        var items = await tenantDbContext.Audits
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();
        await FulfillUsersAsync(items);
        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Audit of account balance change pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns></returns>
    [HttpGet("account-change-balance")]
    [ProducesResponseType(typeof(Result<List<AuditAccountBalanceViewModel>, M.Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AccountChangeBalance([FromQuery] M.Criteria? criteria)
    {
        criteria ??= new M.Criteria();
        criteria.Type = AuditTypes.TradeAccountBalance;

        if (criteria.AccountNumber != null)
        {
            criteria.RowId = await tenantDbContext.Accounts
                .Where(x => x.AccountNumber == criteria.AccountNumber)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }

        var items = await tenantDbContext.Audits
            .PagedFilterBy(criteria)
            .ToTenantResponseModel()
            .ToListAsync();

        await FulfillUsersAndAccountsAsync(items);
        return Ok(Result.Of(items, criteria));
    }

    /// <summary>
    /// Audit of High Dollar Value
    /// </summary>
    /// <returns></returns>
    [HttpGet("high-dollar/latest")]
    [ProducesResponseType(typeof(AuditResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> LastHighValueAudit()
    {
        var key = Enum.GetName(typeof(ConfigurationTypes), ConfigurationTypes.HighDollarValue) ?? string.Empty;
        var rowId = await tenantDbContext.Configurations
            .Where(x => x.Category == nameof(Public))
            .Where(x => x.Key == key && x.RowId == 0)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        var criteria = new M.Criteria
        {
            Type = AuditTypes.Configuration,
            RowId = rowId,
            Page = 1,
            Size = 1,
        };
        var items = await tenantDbContext.Audits
            .PagedFilterBy(criteria)
            .OrderByDescending(x => x.Id)
            .ToResponseModel()
            .ToListAsync();

        await FulfillUsersAsync(items);
        return Ok(items.FirstOrDefault());
    }

    /// <summary>
    /// Get Audit Details
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(AuditResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Details(long id)
    {
        var item = await tenantDbContext.Audits
            .ToResponseModel()
            .FirstOrDefaultAsync(x => x.Id == id);
        return item == null ? NotFound() : Ok(await GetAuditUserAsync(item));
    }

    private async Task<AuditResponseModel> GetAuditUserAsync(AuditResponseModel audit)
    {
        var items = await FulfillUsersAsync(new List<AuditResponseModel> { audit });
        return items.Select(x => x.SetValue(x.Data)).First();
    }

    private async Task<ICollection<AuditResponseModel>> FulfillUsersAsync(ICollection<AuditResponseModel> audits)
    {
        var tenantId = GetTenantId();
        var users = await authDbContext.Users
            .Where(x => audits.Select(a => a.PartyId).Contains(x.PartyId) && x.TenantId == tenantId)
            .ToUserInfo()
            .ToListAsync();

        foreach (var audit in audits)
        {
            var user = users.FirstOrDefault(x => x.PartyId == audit.PartyId);
            audit.SetValue(audit.Data);
            if (user != null) audit.SetUser(user);
        }

        return audits;
    }

    private async Task FulfillUsersAndAccountsAsync(ICollection<AuditAccountBalanceViewModel> audits)
    {
        var accounts = await tenantDbContext.Accounts
            .Where(x => audits.Select(a => a.AccountId).Contains(x.Id))
            .ToTenantBasicViewModel()
            .ToListAsync();

        foreach (var audit in audits)
        {
            var account = accounts.FirstOrDefault(x => x.Id == audit.AccountId);
            if (account != null) audit.Account = account;
        }


        var partyIds = audits.Select(x => x.PartyId).ToList();
        var partyIdsFromAccount = audits.Select(x => x.Account.PartyId).ToList();
        var combinedPartyIds = partyIds.Concat(partyIdsFromAccount).Distinct().ToList();

        var users = await tenantDbContext.Parties
            .Where(x => combinedPartyIds.Contains(x.Id))
            .ToTenantBasicViewModel()
            .ToListAsync();

        foreach (var audit in audits)
        {
            var user = users.FirstOrDefault(x => x.PartyId == audit.PartyId);
            if (user != null) audit.User = user;

            var accountUser = users.FirstOrDefault(x => x.PartyId == audit.Account.PartyId);
            if (accountUser != null) audit.Account.User = accountUser;
        }
    }
}