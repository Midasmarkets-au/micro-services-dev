using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

using M = Address;

public partial class Address
{
    public sealed class UpdateSpec
    {
        public string Name { get; set; } = "";
        public string CCC { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Country { get; set; } = "";
        public string Content { get; set; } = "{}";
    }
    
    public sealed class UpdateSpecV2
    {
        public string Name { get; set; } = "";
        public string CCC { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Country { get; set; } = "";
        public string Content { get; set; } = "{}";
    }

    public sealed class CreateSpec
    {
        public string Name { get; set; } = "";
        public string CCC { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Country { get; set; } = "";
        public string Content { get; set; } = "{}";
    }
    
    public sealed class CreateSpecV2
    {
        public string Name { get; set; } = "";
        public string CCC { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Country { get; set; } = "";
        public string Content { get; set; } = "{}";
    }
}