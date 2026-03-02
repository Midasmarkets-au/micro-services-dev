namespace Bacera.Gateway;

partial class CentralAccount
{
    public static CentralAccount Build(long tenantId, int serviceId, long tradeAccountId, long accountNumber)
        => new()
        {
            TenantId = tenantId,
            ServiceId = serviceId,
            AccountNumber = accountNumber,
            AccountId = tradeAccountId,
            CreatedOn = DateTime.UtcNow
        };
}