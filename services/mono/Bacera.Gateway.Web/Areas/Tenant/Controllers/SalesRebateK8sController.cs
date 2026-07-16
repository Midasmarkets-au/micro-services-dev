using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Area("Tenant")]
[Tags("Tenant/Sales Rebate K8s")]
[Route("api/" + VersionTypes.V1 + "/[Area]/sales-rebate-k8s")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class SalesRebateK8sController(TenantDbContext tenantDbContext, IHttpClientFactory httpClientFactory, IStorageService storageService) : TenantBaseController
{
    public class Criteria : Bacera.Criteria
    {
        public DateTime? From           { get; set; }
        public DateTime? To             { get; set; }
        public long?     SalesAccountId { get; set; }
    }

    public class ItemCriteria : Bacera.Criteria
    {
        public string?  Filter         { get; set; } // "all" | "included" | "excluded"
        // Stats over the full item set (filter-independent) for the summary bar:
        public int      IncludedCount  { get; set; }
        public int      ExcludedCount  { get; set; }
        public decimal  IncludedAmount { get; set; }
    }

    public class SummaryViewModel
    {
        public long     Id                  { get; set; }
        public long     SalesAccountId      { get; set; }
        public long     SalesAccountUid     { get; set; }
        public DateTime PeriodStart         { get; set; }
        public DateTime PeriodEnd           { get; set; }
        public short    ScheduleType        { get; set; }
        public decimal  TotalAmount         { get; set; }
        public int      TradeCount          { get; set; }
        public short    Status              { get; set; }
        public long?    WalletTransactionId { get; set; }
        public DateTime CreatedOn           { get; set; }
    }

    public class ItemViewModel
    {
        public long     Id                 { get; set; }
        public long     Ticket             { get; set; }
        public long     TradeAccountNumber { get; set; }
        public int      TradeAccountCurrencyId { get; set; }
        public string   Symbol             { get; set; } = "";
        public int      Volume             { get; set; }
        public string   RebateType         { get; set; } = "";
        public decimal  RebateBase         { get; set; }
        public decimal  Amount             { get; set; }
        public DateTime ClosedOn           { get; set; }
        public bool     Excluded           { get; set; }
    }

    /// <summary>
    /// List SalesRebate K8s summary records (one per period per sales account).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<SummaryViewModel>, Criteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] Criteria? criteria)
    {
        criteria ??= new Criteria();

        var query =
            from r in tenantDbContext.SalesRebateK8s
            join a in tenantDbContext.Accounts on r.SalesAccountId equals a.Id
            select new { r, SalesAccountUid = a.Uid };

        if (criteria.From != null)
            query = query.Where(x => x.r.PeriodStart >= criteria.From);
        if (criteria.To != null)
            query = query.Where(x => x.r.PeriodStart < criteria.To);
        if (criteria.SalesAccountId != null)
            query = query.Where(x => x.r.SalesAccountId == criteria.SalesAccountId);

        criteria.Total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.r.PeriodStart)
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .Select(x => new SummaryViewModel
            {
                Id                  = x.r.Id,
                SalesAccountId      = x.r.SalesAccountId,
                SalesAccountUid     = x.SalesAccountUid,
                PeriodStart         = x.r.PeriodStart,
                PeriodEnd           = x.r.PeriodEnd,
                ScheduleType        = x.r.ScheduleType,
                TotalAmount         = x.r.TotalAmount,
                TradeCount          = x.r.TradeCount,
                Status              = x.r.Status,
                WalletTransactionId = x.r.WalletTransactionId,
                CreatedOn           = x.r.CreatedOn,
            })
            .ToListAsync();

        return Ok(Result<List<SummaryViewModel>, Criteria>.Of(items, criteria));
    }

    /// <summary>
    /// List detail items for a SalesRebate K8s summary record.
    /// </summary>
    [HttpGet("{id:long}/items")]
    [ProducesResponseType(typeof(Result<List<ItemViewModel>, ItemCriteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Items(long id, [FromQuery] ItemCriteria? criteria)
    {
        criteria ??= new ItemCriteria();
        if (criteria.Page < 1) criteria.Page = 1;
        if (criteria.Size < 1) criteria.Size = 20;

        var baseQuery = tenantDbContext.SalesRebateItemK8s.Where(x => x.SalesRebateId == id);

        // Filter-independent stats for the summary bar.
        criteria.IncludedCount  = await baseQuery.CountAsync(x => !x.Excluded);
        criteria.ExcludedCount  = await baseQuery.CountAsync(x => x.Excluded);
        criteria.IncludedAmount = await baseQuery.Where(x => !x.Excluded)
                                                 .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        var query = criteria.Filter switch
        {
            "included" => baseQuery.Where(x => !x.Excluded),
            "excluded" => baseQuery.Where(x => x.Excluded),
            _          => baseQuery,
        };

        criteria.Total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.ClosedOn)
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .Select(x => new ItemViewModel
            {
                Id                 = x.Id,
                Ticket             = x.Ticket,
                TradeAccountNumber = x.TradeAccountNumber,
                TradeAccountCurrencyId = x.TradeAccountCurrencyId,
                Symbol             = x.Symbol,
                Volume             = x.Volume,
                RebateType         = x.RebateType,
                RebateBase         = x.RebateBase,
                Amount             = x.Amount,
                ClosedOn           = x.ClosedOn,
                Excluded           = x.Excluded,
            })
            .ToListAsync();

        return Ok(Result<List<ItemViewModel>, ItemCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Re-calculate a single un-released SalesRebate K8s summary — re-reads the
    /// period's trades (e.g. after MT ticket data was updated) and regenerates the
    /// record. Delegates to the Rust scheduler's force-recalc, scoped to this sales
    /// account. Blocked once the record has been released.
    /// </summary>
    [HttpPost("{id:long}/recalculate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Recalculate(long id)
    {
        var record = await tenantDbContext.SalesRebateK8s
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (record == null)
            return NotFound();
        if (record.Status != 0)
            return BadRequest(Result.Error(ResultMessage.Common.ActionNotAllow));

        var schedulerHttp = (Environment.GetEnvironmentVariable("SCHEDULER_HTTP_URL")
                             ?? "http://scheduler:9004").TrimEnd('/');
        var payload = new
        {
            schedule_type    = record.ScheduleType,
            period_start     = DateTime.SpecifyKind(record.PeriodStart, DateTimeKind.Utc).ToString("o"),
            period_end       = DateTime.SpecifyKind(record.PeriodEnd, DateTimeKind.Utc).ToString("o"),
            sales_account_id = record.SalesAccountId,
        };

        var http = httpClientFactory.CreateClient();
        var resp = await http.PostAsJsonAsync($"{schedulerHttp}/trigger/sales-rebate-force", payload);
        if (!resp.IsSuccessStatusCode)
            return StatusCode(StatusCodes.Status502BadGateway, Result.Error(ResultMessage.Common.ActionFail));

        return Ok(new { status = "triggered" });
    }

    /// <summary>
    /// Download the detail CSV report for a summary (generated to S3 by the scheduler
    /// when the summary is created / regenerated). 404 if it hasn't been generated yet.
    /// </summary>
    [HttpGet("{id:long}/report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadReport(long id)
    {
        var record = await tenantDbContext.SalesRebateK8s
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
        if (record == null)
            return NotFound();

        // Deterministic key (matches the scheduler upload); summary id is globally unique.
        var key = $"sales-rebate-k8s-report/{id}.csv";
        var stream = await storageService.GetObjectByFilenameAsync(key);
        if (stream == null)
            return NotFound(Result.Error(ResultMessage.Common.RecordNotFound));

        return File(stream, "text/csv", $"sales-rebate-{id}.csv");
    }

    /// <summary>
    /// (Re)generate the summary's detail CSV report and overwrite its S3 object.
    /// Delegates to the Rust scheduler (which owns the report generation).
    /// </summary>
    [HttpPost("{id:long}/report/regenerate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegenerateReport(long id)
    {
        var record = await tenantDbContext.SalesRebateK8s
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
        if (record == null)
            return NotFound();

        var schedulerHttp = (Environment.GetEnvironmentVariable("SCHEDULER_HTTP_URL")
                             ?? "http://scheduler:9004").TrimEnd('/');
        var http = httpClientFactory.CreateClient();
        var resp = await http.PostAsJsonAsync(
            $"{schedulerHttp}/trigger/sales-rebate-report",
            new { summary_id = id });
        if (!resp.IsSuccessStatusCode)
            return StatusCode(StatusCodes.Status502BadGateway, Result.Error(ResultMessage.Common.ActionFail));

        return Ok(new { status = "triggered" });
    }

    /// <summary>
    /// Release a SalesRebate K8s record — credits total_amount to the sales account wallet.
    /// Idempotent: safe to call multiple times.
    /// </summary>
    [HttpPut("{id:long}/release")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Release(long id)
    {
        var record = await tenantDbContext.SalesRebateK8s
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (record == null)
            return NotFound();
        if (record.Status == 1)
            return Ok(new { message = "Already released", walletTransactionId = record.WalletTransactionId });

        var connStr = tenantDbContext.Database.GetConnectionString()!;
        await using var conn = new Npgsql.NpgsqlConnection(connStr);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            // 1. Load payee (target) account — RebateAccountId, which differs from
            //    SalesAccountId for override schemas (upline collects on downline volume).
            var payeeAccountId = record.RebateAccountId ?? record.SalesAccountId;
            long partyId; int currencyId; int fundType; long? walletId;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"SELECT ""PartyId"", ""CurrencyId"", ""FundType"", ""WalletId""
                                    FROM trd.""_Account"" WHERE ""Id"" = @id";
                cmd.Parameters.AddWithValue("@id", payeeAccountId);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return BadRequest(new { error = "Rebate target account not found" });
                partyId    = reader.GetInt64(0);
                currencyId = reader.GetInt32(1);
                fundType   = reader.GetInt32(2);
                walletId   = reader.IsDBNull(3) ? null : (long?)reader.GetInt64(3);
            }

            // 2. Resolve wallet id
            if (walletId == null)
            {
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"SELECT ""Id"" FROM acct.""_Wallet""
                                        WHERE ""PartyId"" = @p AND ""CurrencyId"" = @c AND ""FundType"" = @f
                                        LIMIT 1";
                    cmd.Parameters.AddWithValue("@p", partyId);
                    cmd.Parameters.AddWithValue("@c", currencyId);
                    cmd.Parameters.AddWithValue("@f", fundType);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result == null) return BadRequest(new { error = "Wallet not found for sales account" });
                    walletId = Convert.ToInt64(result);
                }

                await using (var upd = conn.CreateCommand())
                {
                    upd.Transaction = tx;
                    upd.CommandText = @"UPDATE trd.""_Account"" SET ""WalletId"" = @wid WHERE ""Id"" = @aid";
                    upd.Parameters.AddWithValue("@wid", walletId);
                    upd.Parameters.AddWithValue("@aid", payeeAccountId);
                    await upd.ExecuteNonQueryAsync();
                }
            }

            // 3. Lock wallet + get prev balance
            // _Wallet.Balance and wallet_transaction_k8s.{prev_balance,amount} both use the
            // legacy raw scale: real_dollars * 1,000,000 (same as the regular Rebate release
            // path in db/rebate.rs — keep wallet_transaction_k8s in that scale everywhere).
            long prevBalanceRaw;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"SELECT ""Balance"" FROM acct.""_Wallet"" WHERE ""Id"" = @wid FOR UPDATE";
                cmd.Parameters.AddWithValue("@wid", walletId!);
                var result = await cmd.ExecuteScalarAsync();
                if (result == null) return BadRequest(new { error = "Wallet row not found" });
                prevBalanceRaw = Convert.ToInt64(result);
            }

            // 4. Idempotency check
            long? existingWtId = null;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "SELECT id FROM acct.wallet_transaction_k8s WHERE matter_id = @mid LIMIT 1";
                cmd.Parameters.AddWithValue("@mid", id);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null) existingWtId = Convert.ToInt64(result);
            }

            long wtId;
            if (existingWtId.HasValue)
            {
                wtId = existingWtId.Value;
            }
            else
            {
                var now = DateTime.UtcNow;

                // 5. Convert real-dollar TotalAmount to raw scale and update _Wallet.Balance
                long amountRaw = (long)Math.Round(record.TotalAmount * 1_000_000m);
                long newRawBalance = prevBalanceRaw + amountRaw;
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"UPDATE acct.""_Wallet"" SET ""Balance"" = @nb WHERE ""Id"" = @wid";
                    cmd.Parameters.AddWithValue("@nb", newRawBalance);
                    cmd.Parameters.AddWithValue("@wid", walletId!);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 6. Insert wallet_transaction_k8s — NUMERIC(20,4) columns, raw legacy scale
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"INSERT INTO acct.wallet_transaction_k8s
                        (wallet_id, matter_id, prev_balance, amount, created_on, updated_on)
                        VALUES (@wid, @mid, @pb, @amt, @now, @now)
                        RETURNING id";
                    cmd.Parameters.AddWithValue("@wid", walletId!);
                    cmd.Parameters.AddWithValue("@mid", id);
                    cmd.Parameters.AddWithValue("@pb",  prevBalanceRaw);
                    cmd.Parameters.AddWithValue("@amt", amountRaw);
                    cmd.Parameters.AddWithValue("@now", now);
                    var result = await cmd.ExecuteScalarAsync();
                    wtId = Convert.ToInt64(result!);
                }
            }

            // 7. Update sales_rebate_k8s status
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"UPDATE trd.sales_rebate_k8s
                    SET status = 1, wallet_transaction_id = @wtid
                    WHERE id = @id AND created_on = @con";
                cmd.Parameters.AddWithValue("@wtid", wtId);
                cmd.Parameters.AddWithValue("@id",   id);
                cmd.Parameters.AddWithValue("@con",  record.CreatedOn);
                await cmd.ExecuteNonQueryAsync();
            }

            // 8. Audit: write activity_k8s if matter_id exists
            if (record.MatterId.HasValue)
            {
                var auditNow = DateTime.UtcNow;
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"UPDATE core.matter_k8s SET state_id = 1, stated_on = @now WHERE id = @mid";
                    cmd.Parameters.AddWithValue("@now", auditNow);
                    cmd.Parameters.AddWithValue("@mid", record.MatterId.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"INSERT INTO core.activity_k8s
                        (matter_id, party_id, performed_on, on_state_id, action_id, to_state_id, data)
                        VALUES (@mid, 1, @now, 0, 0, 1, @data)";
                    cmd.Parameters.AddWithValue("@mid",  record.MatterId.Value);
                    cmd.Parameters.AddWithValue("@now",  auditNow);
                    cmd.Parameters.AddWithValue("@data", $"SalesRebate released manually: wt_id={wtId}");
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return Ok(new { walletTransactionId = wtId });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
