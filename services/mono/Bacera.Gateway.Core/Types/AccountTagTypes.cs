namespace Bacera.Gateway;

public class AccountTagTypes
{
    public const string DailyConfirmEmail = "DailyConfirmEmail";
    public const string DefaultAgentAccount = "DefaultAgentAccount";
    public const string DefaultSalesAccount = "DefaultSalesAccount";
    public const string NewRebateTest = "NewRebateTest";
    public const string Test = "Test";
    public const string SwapFree = "SwapFree";
    public const string AddPips = "AddPips";
    public const string AddCommission = "AddCommission";
    public const string AutoCreate = "AutoCreate";
    public const string AutoCreateConfirmed = "AutoCreateConfirmed";


    public static readonly string[] All =
    {
        DailyConfirmEmail,
        NewRebateTest,
        Test,
        SwapFree,
        AddPips,
        AddCommission,
        DefaultAgentAccount,
        DefaultSalesAccount,
        AutoCreate,
        AutoCreateConfirmed
    };
}