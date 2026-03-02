using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

partial class AccountingService
{
    public async Task SetPaymentServiceAccessAsync(long partyId, PaymentService.Accesses access)
    {
        var item = await _tenantDbContext.PaymentServiceAccesses
            .Where(x => x.PartyId == partyId)
            .Where(x => x.CurrencyId == (int)access.CurrencyId)
            .Where(x => x.FundType == (int)access.FundType)
            .Where(x => x.PaymentServiceId == access.PaymentServiceId)
            .SingleOrDefaultAsync();
        if (item != null)
        {
            item.CanDeposit = access.CanDeposit ? (short)1 : (short)0;
            item.CanWithdraw = access.CanWithdraw ? (short)1 : (short)0;
            _tenantDbContext.PaymentServiceAccesses.Update(item);
            await _tenantDbContext.SaveChangesAsync();
            return;
        }

        item = new PaymentServiceAccess
        {
            PartyId = partyId,
            FundType = (int)access.FundType,
            CurrencyId = (int)access.CurrencyId,
            PaymentServiceId = access.PaymentServiceId,
            CanDeposit = access.CanDeposit ? (short)1 : (short)0,
            CanWithdraw = access.CanWithdraw ? (short)1 : (short)0,
        };
        await _tenantDbContext.PaymentServiceAccesses.AddAsync(item);
        await _tenantDbContext.SaveChangesAsync();
    }

    public async Task<PaymentService.AccessResponseModel> GetPaymentServiceAccessForClientAsync(long partyId,
        FundTypes? fundType = null, CurrencyTypes? currencyId = null)
    {
        var items = await _tenantDbContext.PaymentServiceAccesses
            .Where(x => x.PartyId == partyId)
            .Where(x => fundType == null || x.FundType == (int)fundType.Value)
            .Where(x => currencyId == null || x.CurrencyId == (int)currencyId.Value)
            .Include(x => x.PaymentService)
            .ToListAsync();

        var result = new PaymentService.AccessResponseModel
        {
            Deposit = items
                .Where(x => x.CanDeposit == 1)
                .Select(x => x.PaymentService)
                .Where(x => x.CanDeposit == 1)
                .Where(x => x.IsActivated == 1)
                .AsQueryable()
                .ToResponse()
                .ToList(),

            Withdrawal = items
                .Where(x => x.CanWithdraw == 1)
                .Select(x => x.PaymentService)
                .Where(x => x.CanWithdraw == 1)
                .Where(x => x.IsActivated == 1)
                .AsQueryable()
                .ToResponse()
                .ToList(),
        };
        return result;
    }

    public async Task<PaymentService.AccessResponseModel> GetPaymentServiceAccessForTenantAsync(long partyId,
        FundTypes? fundType = null, CurrencyTypes? currencyId = null)
    {
        var items = await _tenantDbContext.PaymentServiceAccesses
            .Where(x => x.PartyId == partyId)
            .Where(x => fundType == null || x.FundType == (int)fundType.Value)
            .Where(x => currencyId == null || x.CurrencyId == (int)currencyId.Value)
            .Include(x => x.PaymentService)
            .ToListAsync();

        var result = new PaymentService.AccessResponseModel
        {
            Deposit = items
                .Where(x => x.CanDeposit == 1)
                .Select(x => x.PaymentService)
                .Where(x => x.CanDeposit == 1)
                .AsQueryable()
                .ToResponse()
                .ToList(),

            Withdrawal = items
                .Where(x => x.CanWithdraw == 1)
                .Select(x => x.PaymentService)
                .Where(x => x.CanWithdraw == 1)
                .AsQueryable()
                .ToResponse()
                .ToList(),
        };
        return result;
    }
}