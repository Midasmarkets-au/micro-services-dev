namespace Bacera.Gateway.Services;

public class ApplicationForWholesaleCreatedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.ApplicationForWholesaleCreated;
}

public class ApplicationForWholesaleApprovedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.ApplicationForWholesaleApproved;
}

public class ApplicationForWholesaleRejectedViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.ApplicationForWholesaleRejected;
}

public class ApplicationForWholesaleEvidenceNeededViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } = EmailTemplateTypes.ApplicationForWholesaleEvidenceNeeded;
}

public class ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededViewModel : EmailViewModel
{
    public override string TemplateTitle { get; } =
        EmailTemplateTypes.ApplicationForWholesaleEvidenceForSophisticatedInvestorNeeded;
}