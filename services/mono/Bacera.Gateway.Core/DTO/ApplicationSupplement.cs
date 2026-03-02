using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Bacera.Gateway;

public class ApplicationSupplement : IApplicationSupplement
{
    public AccountRoleTypes Role { get; set; } = AccountRoleTypes.Client;
    public long? RepAccountId { get; set; }
    public long? SalesAccountId { get; set; }
    public long? AgentAccountId { get; set; }
    public long? AccountNumber { get; set; }
    public string? ReferCode { get; set; } = "";
    [MaxLength(60)] public string? SalesSelfGroup { get; set; } = "";
    [MaxLength(60)] public string? SalesDividedGroup { get; set; } = "";
    [MaxLength(60)] public string? AgentSelfGroup { get; set; } = "";
    public int? Leverage { get; set; }
    public PlatformTypes? Platform { get; set; }
    public int? ServiceId { get; set; }
    public int? CurrencyId { get; set; }
    public CurrencyTypes CurrencyIdType => (CurrencyTypes)(CurrencyId ?? -1);

    public AccountTypes? AccountType { get; set; }
    public FundTypes FundType { get; set; }
    public string ToJson() => Utils.JsonSerializeObject(this);

    private bool IsAgentAccountValid() => AgentAccountId != null && AgentAccountId != 0;

    public long ParentAccountId => Role switch
    {
        AccountRoleTypes.Sales => RepAccountId,
        AccountRoleTypes.Agent => IsAgentAccountValid() ? AgentAccountId : SalesAccountId,
        AccountRoleTypes.Client => IsAgentAccountValid() ? AgentAccountId : SalesAccountId,
        AccountRoleTypes.Rep => 0,
        _ => 0
    } ?? 0;

    public static ApplicationSupplement Build(
        AccountRoleTypes role,
        FundTypes fundType,
        CurrencyTypes? currency = null,
        AccountTypes? accountType = null,
        PlatformTypes? platform = null,
        int? serviceId = null,
        int? leverage = null,
        long? agentAccountId = null,
        long? salesAccountId = null,
        string? referCode = null,
        string? agentSelfGroup = null,
        string? salesSelfGroup = null
    )
        => new()
        {
            Role = role,
            FundType = fundType,
            ReferCode = referCode,
            Leverage = leverage,
            Platform = platform,
            ServiceId = serviceId,
            AgentAccountId = agentAccountId,
            SalesAccountId = salesAccountId,
            CurrencyId = (int)(currency ?? CurrencyTypes.Invalid),
            AccountType = accountType,
            AgentSelfGroup = agentSelfGroup,
            SalesSelfGroup = salesSelfGroup
        };

    /**
     * Merge from other supplement. Current supplement will be overwritten by other supplement.
     */
    public ApplicationSupplement Merge(ApplicationSupplement? other)
    {
        if (other == null) return this;
        ReferCode = other.ReferCode ?? ReferCode ?? "";
        FundType = other.FundType;
        Role = other.Role != AccountRoleTypes.Unknown ? other.Role : Role;
        Leverage = other.Leverage ?? Leverage;
        Platform = other.Platform ?? Platform;
        ServiceId = other.ServiceId ?? ServiceId;
        CurrencyId = other.CurrencyId ?? CurrencyId;
        AccountType = other.AccountType ?? AccountType;
        RepAccountId = other.RepAccountId ?? RepAccountId;
        AgentAccountId = other.AgentAccountId ?? AgentAccountId;
        SalesAccountId = other.SalesAccountId ?? SalesAccountId;
        AgentSelfGroup = other.AgentSelfGroup ?? AgentSelfGroup;
        SalesSelfGroup = other.SalesSelfGroup ?? SalesSelfGroup;
        SalesDividedGroup = other.SalesDividedGroup ?? SalesDividedGroup;
        return this;
    }
}

public class ApplicationSupplementValidator : AbstractValidator<ApplicationSupplement>
{
    public ApplicationSupplementValidator()
    {
        RuleFor(x => x.Role).NotEqual(AccountRoleTypes.Unknown);
        RuleFor(x => x.SalesSelfGroup)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(60)
            .When(x => x.Role == AccountRoleTypes.Sales);
        RuleFor(x => x.AgentSelfGroup)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(60)
            .When(x => x.Role == AccountRoleTypes.Agent);
        RuleFor(x => x.FundType).Must(t => (int)t > 0);
        RuleFor(x => x.Platform)
            .Must(x => Enum.IsDefined(typeof(PlatformTypes), x!))
            .When(x => x.Platform.HasValue);

        RuleFor(x => x.CurrencyId)
            .Must(x => Enum.IsDefined(typeof(CurrencyTypes), x!))
            .When(x => x.CurrencyId.HasValue);
    }
}