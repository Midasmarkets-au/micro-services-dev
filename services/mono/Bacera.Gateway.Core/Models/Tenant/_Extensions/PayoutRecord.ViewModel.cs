using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

using M = PayoutRecord;

public partial class PayoutRecord
{
    public sealed class TenantPageModel
    {
        public long Id { get; set; }
        public string PaymentMethodName { get; set; } = "";
        public string BatchUid { get; set; } = "";
        public string Status { get; set; } = "";
        public string BankName { get; set; } = "";
        public string BankCode { get; set; } = "";
        public string BranchName { get; set; } = "";
        public string AccountName { get; set; } = "";
        public string BankNumber { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
    public sealed class InfoModel
    {
        public List<object> RequestHistory { get; set; } = [];
    }
}

public static class PayoutRecordViewModelExtension
{
    public static IQueryable<M.TenantPageModel> ToTenantPageModel(this IQueryable<M> q) =>
        q.Select(x => new M.TenantPageModel
        {
            Id = x.Id,
            PaymentMethodName = x.PaymentMethod.Name,
            BatchUid = x.BatchUid,
            Status = Enum.GetName((PayoutRecordStatusTypes)x.Status)!,
            BankName = x.BankName,
            BankCode = x.BankCode,
            BranchName = x.BranchName,
            AccountName = x.AccountName,
            BankNumber = x.BankNumber,
            Currency = x.Currency,
            Amount = x.Amount,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn
        });
}