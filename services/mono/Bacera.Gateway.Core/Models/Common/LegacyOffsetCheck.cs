namespace Bacera.Gateway;

public class LegacyOffsetCheck
{
    public long Id { get; set; }
    public bool IsActive { get; set; } = true;
    public string Name { get; set; } = null!;
    public List<long> AccountNumbers { get; set; } = new();
}