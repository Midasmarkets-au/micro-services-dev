namespace Bacera.Gateway;

public enum UserRoleTypes
{
    System = 1,
    SuperAdmin = 2,
    TenantAdmin = 10,
    Supervisor = 12,
    DemoAuAdmin = 13,
    Dealing = 15,
    ShowContact = 16,
    Compliance = 20,
    KycOfficer = 21,
    DepositAdmin = 30,
    EventAdmin = 60,
    Admin = 99,
    Sales = 100,
    Rep = 110,
    Broker = 200,
    IB = 300,
    Wholesale = 310,
    Client = 400,
    EventShop = 501,
    MLM = 510,
    Guest = 1000,
}

public static class UserRoleTypesString
{
    public const string Guest = "Guest";
    public const string Compliance = "Compliance";
    public const string KycOfficer = "KycOfficer";
    public const string Client = "Client";
    public const string Wholesale = "Wholesale";
    public const string Sales = "Sales";
    public const string Rep = "Rep";
    public const string IB = "IB";
    public const string Dealing = "Dealing";
    public const string Supervisor = "Supervisor";
    public const string Broker = "Broker";
    public const string ShowContact = "ShowContact";
    public const string DepositAdmin = "DepositAdmin";
    public const string AccountAdmin = "AccountAdmin";
    public const string DemoAuAdmin = "DemoAuAdmin";
    public const string SuperAdmin = "SuperAdmin";
    public const string TenantAdmin = "TenantAdmin";
    public const string Admin = "Admin";
    public const string EventShop = "EventShop";
    public const string EventAdmin = "EventAdmin";
    public const string MLM = "MLM";
    public const string ClientOrTenantAdmin = Client + "," + TenantAdmin;
    public const string AllClient = Client + "," + Guest + "," + TenantAdmin;
}

public static class UserRoleTypesExtensions
{
    public static List<string> GetAllRoleString() =>
    [
        UserRoleTypesString.Guest,
        UserRoleTypesString.Compliance,
        UserRoleTypesString.KycOfficer,
        UserRoleTypesString.Client,
        UserRoleTypesString.Wholesale,
        UserRoleTypesString.Sales,
        UserRoleTypesString.Rep,
        UserRoleTypesString.IB,
        UserRoleTypesString.Dealing,
        UserRoleTypesString.Broker,
        UserRoleTypesString.DemoAuAdmin,
        UserRoleTypesString.ShowContact,
        UserRoleTypesString.Supervisor,
        UserRoleTypesString.DepositAdmin,
        UserRoleTypesString.AccountAdmin,
        UserRoleTypesString.SuperAdmin,
        UserRoleTypesString.TenantAdmin,
        UserRoleTypesString.Admin,
        UserRoleTypesString.EventShop,
        UserRoleTypesString.EventAdmin,
        UserRoleTypesString.MLM,
    ];
    public static string GetDescription(this UserRoleTypes value)
    {
        return value switch
        {
            UserRoleTypes.SuperAdmin => "SuperAdmin",
            UserRoleTypes.TenantAdmin => "TenantAdmin",
            UserRoleTypes.DemoAuAdmin => "DemoAuAdmin",
            UserRoleTypes.Supervisor => "Supervisor",
            UserRoleTypes.Broker => "Broker",
            UserRoleTypes.Wholesale => "Wholesale",
            UserRoleTypes.Client => "Client",
            UserRoleTypes.Sales => "Sales",
            UserRoleTypes.IB => "IB",
            UserRoleTypes.ShowContact => "ShowContact",
            UserRoleTypes.Dealing => "Dealing",
            UserRoleTypes.Guest => "Guest",
            UserRoleTypes.Rep => "Rep",
            UserRoleTypes.DepositAdmin => "DepositAdmin",
            UserRoleTypes.Compliance => "Compliance",
            UserRoleTypes.KycOfficer => "KycOfficer",
            UserRoleTypes.Admin => "Admin",
            UserRoleTypes.EventShop => "EventShop",
            UserRoleTypes.EventAdmin => "EventAdmin",
            UserRoleTypes.MLM => "MLM",
            _ => "Unknown"
        };
    }
}