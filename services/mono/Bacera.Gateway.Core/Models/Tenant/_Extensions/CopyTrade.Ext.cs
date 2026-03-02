namespace Bacera.Gateway;

partial class CopyTrade
{
    public sealed class CreateSpec
    {
        public long Source { get; set; }
        public long Target { get; set; }
        public string Mode { get; set; } = null!;
        public int? Value { get; set; }
    }
}