using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.ViewModels.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway;

public sealed partial class AccountingService
{
    public async Task<Result<List<RefundViewModel>, Refund.Criteria>> RefundQueryAsync(Refund.Criteria criteria,
        bool hideEmail = false)
    {
        var results = await _tenantDbContext.Refunds
            .PagedFilterBy(criteria)
            .ToResponseModel(hideEmail)
            .ToListAsync();

        var partyIds = results.Select(x => x.PartyId).Distinct().ToList();
        var comments = await _tenantDbContext.Comments
            .Where(x => partyIds.Contains(x.PartyId) && x.Type == (int)CommentTypes.User)
            .Select(x => new { x.PartyId, x.Content })
            .ToListAsync();
        results.ForEach(x => x.User.HasComment = comments.Any(c => c.PartyId == x.PartyId));
        return Result<List<RefundViewModel>, Refund.Criteria>.Of(results, criteria);
    }

    public async Task<Refund> RefundCreateAsync(Refund.CreateSpec spec, long operatorPartyId = 1)
    {
        var targetModel = spec.TargetType switch
        {
            RefundTargetTypes.Wallet => await _tenantDbContext.Wallets
                .Where(x => x.Id == spec.TargetId)
                .Select(x => new
                { x.PartyId, CurrencyId = (CurrencyTypes)x.CurrencyId, FundType = (FundTypes)x.FundType })
                .FirstOrDefaultAsync(),

            // RefundTargetTypes.TradeAccount => await _tenantDbContext.Accounts
            //     .Where(x => x.Id == spec.TargetId)
            //     .Select(x => new
            //         { x.PartyId, CurrencyId = (CurrencyTypes)x.CurrencyId, FundType = (FundTypes)x.FundType })
            //     .FirstOrDefaultAsync(),

            _ => null
        };

        if (targetModel == null)
        {
            _logger.LogWarning("Refund: {Id} create failed by {PartyId}, Target not found", spec.TargetId,
                operatorPartyId);
            return new Refund();
        }

        // 汇率转换逻辑：先转换货币，再×1000000存储
        long finalAmount;

        if (spec.TransferCurrencyId == (int)targetModel.CurrencyId)
        {
            // 相同货币：amount=1代表0.01，存储为0.01×1000000=10000
            finalAmount = spec.Amount.ToScaledFromCents();
        }
        else if (spec.TransferCurrencyId == 841 && (int)targetModel.CurrencyId == 840) // USC → USD
        {
            // 0.01 USC = 0.0001 USD，存储为0.0001×1000000=100
            finalAmount = spec.Amount * 100;
        }
        else if (spec.TransferCurrencyId == 840 && (int)targetModel.CurrencyId == 841) // USD → USC
        {
            // 0.01 USD = 1 USC，存储为1×1000000=1000000
            finalAmount = (spec.Amount * 100).ToScaledFromCents();
        }
        else
        {
            throw new NotSupportedException($"Currency conversion from {spec.TransferCurrencyId} to {(int)targetModel.CurrencyId} is not supported");
        }

        var refund = Refund.Build(targetModel.PartyId, spec.TargetType, spec.TargetId, finalAmount,
            targetModel.FundType, targetModel.CurrencyId, spec.Comment);
        refund.IdNavigation.AddActivity(operatorPartyId, ActionTypes.RefundCreate);
        await _tenantDbContext.Refunds.AddAsync(refund);
        await _tenantDbContext.SaveChangesAsync();

        switch (spec.TargetType)
        {
            case RefundTargetTypes.Wallet:
                await WalletChangeBalanceAsync(refund.PartyId, (FundTypes)refund.FundType, refund.Id, refund.Amount,
                    (CurrencyTypes)refund.CurrencyId, operatorPartyId);
                break;
            case RefundTargetTypes.TradeAccount:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _logger.LogInformation("Refund: {Id} created by {PartyId}", refund.Id, operatorPartyId);
        return refund;
    }

    public async Task<bool> RefundCompleteAsync(long id, long operatorPartyId)
    {
        var model = await _tenantDbContext.Refunds
            .Include(x => x.IdNavigation)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (model == null)
        {
            _logger.LogWarning("Refund: {Id} complete failed by {PartyId}, Refund not found", id,
                operatorPartyId);
            return false;
        }

        var result = await TransitAsync(model, ActionTypes.RefundCompleted, operatorPartyId);
        if (!result.Item1)
        {
            _logger.LogWarning("Refund: {Id} complete failed by {PartyId}", model.Id, operatorPartyId);
            return false;
        }

        _logger.LogInformation("Refund: {Id} complete by {PartyId}", model.Id, operatorPartyId);
        return true;
    }
}