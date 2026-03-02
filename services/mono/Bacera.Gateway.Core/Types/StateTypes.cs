namespace Bacera.Gateway;

public enum StateTypes
{
    Initialed = 0,
    TransferCreated = 200,
    TransferCanceled = 205,
    TransferFailed = 206,
    TransferAwaitingApproval = 210,
    TransferRejected = 215,
    TransferApproved = 220,
    TransferCompleted = 250,
    DepositCreated = 300,
    DepositCanceled = 305,
    DepositFailed = 306,
    DepositPaymentCompleted = 310,
    DepositCallbackTimeOut = 315,
    DepositCentralApproved = 320,
    DepositCentralRejected = 325,
    DepositTenantApproved = 330,
    DepositTenantRejected = 335,
    DepositCallbackCompleted = 345,
    DepositCompleted = 350,
    WithdrawalCreated = 400,
    WithdrawalCanceled = 405,
    WithdrawalFailed = 406,
    WithdrawalCentralApproved = 410,
    WithdrawalCentralRejected = 415,
    WithdrawalTenantApproved = 420,
    WithdrawalTenantRejected = 425,
    WithdrawalPaymentCompleted = 430,
    WithdrawalCompleted = 450,
    RebateCreated = 500,
    RebateCanceled = 505,
    RebateOnHold = 510,
    RebateReleased = 520,
    RebateCompleted = 550,
    RebateTradeClosedLessThanOneMinute = 590,
    RefundCreated = 600,
    RefundCompleted = 650,
    WalletAdjustCreated = 700,
    WalletAdjustCompleted = 750,
}
// 345,1345,310,345
// 
// 

public static class StageTypes
{
    public static List<int> Padding => new()
    {
        (int)StateTypes.Initialed,
        (int)StateTypes.TransferCreated,
        (int)StateTypes.DepositCreated,
        (int)StateTypes.WithdrawalCreated,
        (int)StateTypes.RebateCreated,
        (int)StateTypes.RefundCreated,
        (int)StateTypes.WalletAdjustCreated,
    };

    public static List<int> Canceled => new()
    {
        (int)StateTypes.TransferCanceled,
        (int)StateTypes.TransferRejected,
        (int)StateTypes.DepositCanceled,
        (int)StateTypes.DepositTenantRejected,
        (int)StateTypes.WithdrawalCanceled,
        (int)StateTypes.WithdrawalCentralRejected,
        (int)StateTypes.WithdrawalTenantRejected,
        (int)StateTypes.RebateCanceled,
        (int)StateTypes.WithdrawalTenantRejected,
    };

    public static List<int> Processing => new()
    {
        (int)StateTypes.TransferAwaitingApproval,
        (int)StateTypes.TransferApproved,
        (int)StateTypes.DepositPaymentCompleted,
        (int)StateTypes.DepositCentralApproved,
        (int)StateTypes.DepositCentralRejected,
        (int)StateTypes.DepositTenantApproved,
        (int)StateTypes.DepositTenantRejected,

        (int)StateTypes.WithdrawalCentralApproved,
        (int)StateTypes.WithdrawalCentralRejected,
        (int)StateTypes.WithdrawalTenantApproved,
        (int)StateTypes.WithdrawalTenantRejected,
        (int)StateTypes.WithdrawalPaymentCompleted,

        (int)StateTypes.RebateOnHold,
        (int)StateTypes.RebateReleased,
    };

    public static List<int> Completed => new()
    {
        (int)StateTypes.TransferCompleted,
        (int)StateTypes.DepositCompleted,
        (int)StateTypes.WithdrawalCompleted,
        (int)StateTypes.WithdrawalCreated,
        (int)StateTypes.RebateCompleted,
        (int)StateTypes.RefundCompleted,
        (int)StateTypes.WalletAdjustCompleted,
    };


    public static HashSet<int> CanDeleteDeposit => new()
    {
        (int)StateTypes.DepositCreated,
        (int)StateTypes.DepositPaymentCompleted,
        (int)StateTypes.DepositCanceled,
    };
}