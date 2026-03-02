using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class AccountingService
{
    public async Task<Tuple<bool, Payment>> PaymentExecuteAsync(long paymentId, long serviceId = 0,
        string? reference = null, object? response = null)
    {
        var payment = await _tenantDbContext.Payments
            .ScopeCanExecute()
            .FirstOrDefaultAsync(x => x.Id == paymentId);
        if (payment == null)
        {
            return Tuple.Create(false, new Payment());
        }

        if (serviceId != 0) payment.PaymentServiceId = serviceId;
        payment.Status = (short)PaymentStatusTypes.Executing;
        payment.UpdatedOn = DateTime.UtcNow;

        _tenantDbContext.Payments.Update(payment);
        await _tenantDbContext.SaveChangesAsync();

        if (string.IsNullOrEmpty(reference)) return Tuple.Create(true, payment);

        var supplement = await _tenantDbContext.Supplements
            .Where(x => x.RowId == payment.Id)
            .Where(x => x.Type == (short)SupplementTypes.Payment)
            .FirstOrDefaultAsync() ?? new Supplement
        {
            Type = (int)SupplementTypes.Payment,
            RowId = payment.Id,
        };
        var data = Supplement.PaymentSupplement.FromJson(supplement.Data);
        data.Reference = reference;
        if (response != null)
            data.Response = Utils.JsonSerializeObject(response);

        supplement.Data = data.ToJson();
        supplement.UpdatedOn = DateTime.UtcNow;
        _tenantDbContext.Supplements.Update(supplement);
        await _tenantDbContext.SaveChangesAsync();

        return Tuple.Create(true, payment);
    }

    public async Task<Result<List<Payment>, Payment.Criteria>> PaymentQueryAsync(Payment.Criteria criteria)
    {
        var items = await _tenantDbContext.Payments
            .PagedFilterBy(criteria)
            .ToListAsync();
        return Result<List<Payment>, Payment.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<Payment.ClientResponseModel>, Payment.Criteria>> PaymentQueryAsync(long partyId,
        Payment.Criteria criteria)
    {
        criteria.PartyId = partyId;
        var items = await _tenantDbContext.Payments
            .PagedFilterBy(criteria)
            .ToClientResponseModel()
            .ToListAsync();
        return Result<List<Payment.ClientResponseModel>, Payment.Criteria>.Of(items, criteria);
    }

    public async Task<Payment> PaymentGetAsync(long paymentId) => await _tenantDbContext.Payments
        .Include(x => x.PaymentMethod)
        .FirstOrDefaultAsync(x => x.Id == paymentId) ?? new Payment();

    public async Task<Tuple<int, Payment>> PaymentCancelAsync(long paymentId, string? activity = null)
        => await PaymentChangeStatusAsync(paymentId, PaymentStatusTypes.Cancelled, activity);

    public async Task<Tuple<int, Payment>> PaymentFailAsync(long paymentId, string? activity = null)
        => await PaymentChangeStatusAsync(paymentId, PaymentStatusTypes.Failed, activity);

    public async Task<Tuple<int, Payment>> PaymentCompleteAsync(long paymentId, string? activity = null)
        => await PaymentChangeStatusAsync(paymentId, PaymentStatusTypes.Completed, activity);

    /// <summary>
    /// Change payment status
    /// </summary>
    /// <param name="paymentId"></param>
    /// <param name="targetStatus"></param>
    /// <param name="activity">activity for record</param>
    /// <returns></returns>
    private async Task<Tuple<int, Payment>> PaymentChangeStatusAsync(long paymentId, PaymentStatusTypes targetStatus,
        string? activity = null)
    {
        if (targetStatus == PaymentStatusTypes.Pending) return Tuple.Create(-1, new Payment());

        var query = _tenantDbContext.Payments.AsQueryable();
        query = targetStatus switch
        {
            PaymentStatusTypes.Executing => query.ScopeCanExecute(),
            PaymentStatusTypes.Completed => query.ScopeCanComplete(),
            PaymentStatusTypes.Cancelled => query.ScopeCanCancel(),
            PaymentStatusTypes.Failed => query.ScopeCanFail(),
            _ => query
        };

        var payment = await query.FirstOrDefaultAsync(x => x.Id == paymentId);
        if (payment == null) return Tuple.Create(-2, new Payment());

        payment.UpdatedOn = DateTime.UtcNow;
        payment.Status = (short)targetStatus;

        _tenantDbContext.Payments.Update(payment);
        await _tenantDbContext.SaveChangesAsync();

        if (string.IsNullOrEmpty(activity)) return Tuple.Create(1, payment);

        var supplement =
            await _tenantDbContext.Supplements.FirstOrDefaultAsync(x =>
                x.RowId == payment.Id && x.Type == (short)SupplementTypes.Payment)
            ?? Supplement.Build(SupplementTypes.Payment, payment.Id);
        var data = Supplement.PaymentSupplement.FromJson(supplement.Data);
        data.AddActivity(activity);
        supplement.Data = data.ToJson();
        supplement.UpdatedOn = DateTime.UtcNow;
        if (supplement.Id > 0) _tenantDbContext.Supplements.Update(supplement);
        else await _tenantDbContext.Supplements.AddAsync(supplement);
        await _tenantDbContext.SaveChangesAsync();
        return Tuple.Create(1, payment);
    }
}