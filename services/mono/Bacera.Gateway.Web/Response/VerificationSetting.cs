using Bacera.Gateway.DTO;

namespace Bacera.Gateway.Web.Response;

public class VerificationSetting
{
    public string[] Settings { get; set; } = [];
    public VerificationDTO Data { get; set; } = new();
}

public class VerificationJpSetting
{
    public string[] Settings { get; set; } = [];
    public VerificationJpClientDTO Data { get; set; } = new();
}

public class VerificationSettingV2
{
    public string[] Settings { get; set; } = [];
    public List<BCRLegalDocument> Documents { get; set; } = [];
    public VerificationDTOV2? Data { get; set; } = new();
}

public class VerificationJpSettingV2
{
    public string[] Settings { get; set; } = [];
    public List<BCRLegalDocument> Documents { get; set; } = [];
    public VerificationDTOV2? Data { get; set; } = new();
}