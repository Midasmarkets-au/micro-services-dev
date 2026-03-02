namespace Bacera.Gateway
{
    public enum PaymentStatusTypes : short
    {
        Pending = 0,
        Executing = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        Rejected = 5,
    }
}