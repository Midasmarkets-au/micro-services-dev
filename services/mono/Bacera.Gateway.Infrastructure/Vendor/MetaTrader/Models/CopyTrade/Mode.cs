namespace Bacera.Gateway.Vendor.MetaTrader;

public class Mode
{
    public Mode(string mode, int? value = null)
    {
        Name = mode;
        Value = new[] { ModeTypes.FixedVolume }.Contains(mode) == false ? null : value;
    }

    public string Name { get; set; }
    public int? Value { get; set; }
}