namespace Bacera.Gateway.Web.DTO;

public partial class LegacyUser
{
    public uint Id { get; set; }

    public uint Login { get; set; }

    public string Email { get; set; } = null!;

    public string ClientGroup { get; set; } = null!;

    public string Mt4Group { get; set; } = null!;

    public string Password { get; set; } = null!;

    public uint? StatusId { get; set; }

    public string PhonePass { get; set; } = null!;

    public string InvestorPass { get; set; } = null!;

    public int SalesAccount { get; set; }

    public int IbAccount { get; set; }

    public bool DocumentVerify { get; set; }

    public bool DepositVerify { get; set; }

    public bool VerifyWithdraw { get; set; }

    public bool OnMt4 { get; set; }

    public bool Enable { get; set; }

    public uint? TermsId { get; set; }

    public uint? AccountTypeId { get; set; }

    public bool TractionFlag { get; set; }

    public bool AllowIps { get; set; }

    public string? Currency { get; set; }

    public string? Lang { get; set; }

    public bool? ElectronicConsent { get; set; }

    public bool? ChinaConsent { get; set; }

    public bool? Subscription { get; set; }

    public int? Leverage { get; set; }

    public string Mt4Location { get; set; } = null!;

    public uint MaxAllowedLeverageId { get; set; }

    public bool MarkAsDelete { get; set; }

    public string OpenAt { get; set; } = null!;

    public uint? AssociatedAccountId { get; set; }

    public bool NewConfirm { get; set; }

    public string? ApiToken { get; set; }

    public uint RateLimit { get; set; }

    public string? Properties { get; set; }

    public string? RememberToken { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? JurisdictionConsent { get; set; }
}
