using System.Text;
using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

partial class Party
{
    public sealed class TenantBasicUserPageModel
    {
        public long PartyId { get; set; }
        public string Avatar { get; set; } = "";
        public string Email { get; set; } = "";

        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        public string NativeName { get; set; } = "";

        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]

        public string FirstName { get; set; } = "";

        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]

        public string LastName { get; set; } = "";

        public string DisplayName => string.IsNullOrWhiteSpace(NativeName) ? $"{FirstName} {LastName}" : NativeName;
    }
}