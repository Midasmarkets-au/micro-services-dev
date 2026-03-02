namespace Bacera.Gateway;

public class IpSetting
{
    public string Ip { get; set; } = null!;
    public IpSettingTypes Type { get; set; }
    public string? Token { get; set; }
    public string Description { get; set; } = null!;
}