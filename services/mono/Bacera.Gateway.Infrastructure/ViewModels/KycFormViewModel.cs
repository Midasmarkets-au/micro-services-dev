using FluentValidation;

namespace Bacera.Gateway;

public class KycFormViewModel
{
    public string AccountNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Citizen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PriorName { get; set; } = string.Empty;
    public DateOnly Birthday { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Occupation { get; set; } = string.Empty;
    public string AnnualIncome { get; set; } = string.Empty;
    public string NetWorth { get; set; } = string.Empty;
    public string SourceOfFunds { get; set; } = string.Empty;

    public string Bg1 { get; set; } = string.Empty;
    public string Bg2 { get; set; } = string.Empty;
    public string Exp1 { get; set; } = string.Empty;
    public string Exp2 { get; set; } = string.Empty;
    public string Exp3 { get; set; } = string.Empty;
    public string Exp4 { get; set; } = string.Empty;
    public string Exp5 { get; set; } = string.Empty;

    public string? Exp1Employer { get; set; } = string.Empty;
    public string? Exp1Position { get; set; } = string.Empty;
    public string? Exp1Remuneration { get; set; } = string.Empty;

    public string? Exp2More { get; set; } = string.Empty;
    public string? Exp3More { get; set; } = string.Empty;
    public string? Exp4More { get; set; } = string.Empty;
    public string? Exp5More { get; set; } = string.Empty;

    public string? BusinessName { get; set; } = string.Empty;
    public string? BusinessNumber { get; set; } = string.Empty;
    public string? BusinessAddress { get; set; } = string.Empty;
    public string? BusinessCity { get; set; } = string.Empty;
    public string? BusinessState { get; set; } = string.Empty;
    public string? BusinessZipCode { get; set; } = string.Empty;
    public string? BusinessCountry { get; set; } = string.Empty;
    public string? BusinessCompanyPurposeBetweenUs { get; set; } = string.Empty;
    public string? BusinessServiceRequireReason { get; set; } = string.Empty;
    public string? BusinessIndustry { get; set; } = string.Empty;
    public string? BusinessClientIndustry1 { get; set; } = string.Empty;
    public string? BusinessClientIndustry3 { get; set; } = string.Empty;
    public string? BusinessService { get; set; } = string.Empty;

    public string? IdType { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public DateOnly? IdIssueOn { get; set; }
    public DateOnly? IdExpireOn { get; set; }
    
    public string? ElectronicOrTraditional { get; set; } = string.Empty;
    public string? ElectronicOption { get; set; } = string.Empty;
    public string? ElectronicSourceOption { get; set; } = string.Empty;
    public string? TraditionalOption { get; set; } = string.Empty;
    public string? ComplianceNote { get; set; } = string.Empty;

    public long StaffPartyId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string StaffPosition { get; set; } = string.Empty;
    public DateOnly? StaffSignedOn { get; set; }
    public int StaffSignatureId { get; set; }
    public string StaffSignature { get; set; } = string.Empty;

    public string? ComplianceName { get; set; } = string.Empty;
    public string? CompliancePosition { get; set; } = string.Empty;
    public DateOnly? ComplianceSignedOn { get; set; }
    public int? ComplianceSignatureId { get; set; }
    public string? ComplianceSignature { get; set; } = string.Empty;
}

public class KycFormViewModelSignValidator : AbstractValidator<KycFormViewModel>
{
    public KycFormViewModelSignValidator()
    {
        RuleFor(x => x.Exp1Employer).NotEmpty().When(x => x.Exp1=="0");
        RuleFor(x => x.Exp1Position).NotEmpty().When(x => x.Exp1=="0");
        RuleFor(x => x.Exp1Remuneration).NotEmpty().When(x => x.Exp1=="0");

        RuleFor(x => x.Exp2More).NotEmpty().When(x => x.Exp2=="0");
        RuleFor(x => x.Exp3More).NotEmpty().When(x => x.Exp3=="0");
        RuleFor(x => x.Exp4More).NotEmpty().When(x => x.Exp4=="0");
        RuleFor(x => x.Exp5More).NotEmpty().When(x => x.Exp5=="0");
    }
}

public class KycFormViewModelFinalizeValidator : AbstractValidator<KycFormViewModel>
{
    public KycFormViewModelFinalizeValidator()
    {
        RuleFor(x => x.ComplianceName).NotEmpty();
        RuleFor(x => x.CompliancePosition).NotEmpty();
        RuleFor(x => x.ComplianceSignedOn).NotNull();
        RuleFor(x => x.ComplianceSignature).NotEmpty();
    }
}