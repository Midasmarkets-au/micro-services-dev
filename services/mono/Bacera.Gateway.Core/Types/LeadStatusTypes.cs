namespace Bacera.Gateway.Core.Types;

public enum LeadStatusTypes
{
    UserNotRegistered = 0,
    UserRegistered = 10,
    UserVerifying = 20,
    UserVerificationRejected = 26,
    UserVerificationUnderReview = 27,
    UserVerificationApproved = 28,
    AccountApplicationCreated = 30, // tenant approves verification, both agent and client
    AccountApplicationApproved = 31, // tenant approves client's account application
    AccountApplicationRejected = 35,
    TradeAccountCreated = 40, // tenant opens MT5 account for client
    AgentAccountCreated = 50,
    DemoAccountCreated = 60,
}

public static class LeadStatusDescriptions
{
    public const string UserNotRegistered = "Created";
    public const string UserRegistered = "User registered";
    public const string UserVerifying = "User is verifying";
    public const string UserVerificationSubmitted = "User verification submitted";
    public const string UserVerificationRejected = "User verification rejected";
    public const string UserVerificationUnderReview = "User verification under review";
    public const string UserVerificationApproved = "User verification approved";
    public const string AccountApplicationCreated = "Account application created";
    public const string AccountApplicationApproved = "Account application approved";
    public const string AccountApplicationRejected = "Account application rejected";
    public const string TradeAccountCreated = "Trade account created";
    public const string AgentAccountCreated = "Agent account created";
    public const string DemoAccountCreated = "Demo account created";

    public static Dictionary<LeadStatusTypes, string> Dictionary { get; } = new()
    {
        { LeadStatusTypes.UserNotRegistered, UserNotRegistered },
        { LeadStatusTypes.UserRegistered, UserRegistered },
        { LeadStatusTypes.UserVerifying, UserVerifying },
        { LeadStatusTypes.UserVerificationRejected, UserVerificationRejected },
        { LeadStatusTypes.UserVerificationUnderReview, UserVerificationUnderReview },
        { LeadStatusTypes.UserVerificationApproved, UserVerificationApproved },
        { LeadStatusTypes.AccountApplicationCreated, AccountApplicationCreated },
        { LeadStatusTypes.AccountApplicationApproved, AccountApplicationApproved },
        { LeadStatusTypes.AccountApplicationRejected, AccountApplicationRejected },
        { LeadStatusTypes.TradeAccountCreated, TradeAccountCreated },
        { LeadStatusTypes.AgentAccountCreated, AgentAccountCreated },
        { LeadStatusTypes.DemoAccountCreated, DemoAccountCreated },
    };

    public static readonly string[] All =
    {
        UserNotRegistered,
        UserRegistered,
        UserVerifying,
        UserVerificationRejected,
        UserVerificationUnderReview,
        UserVerificationApproved,
        AccountApplicationCreated,
        AccountApplicationApproved,
        AccountApplicationRejected,
        TradeAccountCreated,
        AgentAccountCreated,
        UserVerificationSubmitted,
        DemoAccountCreated
    };
}