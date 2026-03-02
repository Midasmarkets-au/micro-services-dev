namespace Bacera.Gateway.Web.Request;

[Serializable]
public class ProfileRequestModel
{
    public string? Timezone { get; set; }
    public string? Language { get; set; }
}