namespace Bacera.Gateway;

public partial class Permission
{
    public class CreateSpec
    {
        public bool Auth { get; set; }
        public string Action { get; set; } = null!;
        public string Method { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Key { get; set; } = null!;
    }
}