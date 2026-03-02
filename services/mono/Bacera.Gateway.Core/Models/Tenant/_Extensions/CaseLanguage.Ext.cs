namespace Bacera.Gateway;

public partial class CaseLanguage
{
    public static CaseLanguage Build(long caseId, string language, string content) => new()
    {
        CaseId = caseId,
        Language = language,
        Content = content
    };
}