using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway.Auth;

partial class User
{
    public class TenantUpdateSpec
    {
        [StringLength(128)] public string NativeName { get; set; } = string.Empty;
        [StringLength(64)] public string FirstName { get; set; } = string.Empty;
        [StringLength(64)] public string LastName { get; set; } = string.Empty;
        [StringLength(20)] public string Language { get; set; } = string.Empty;

        public DateOnly Birthday { get; set; } = default;
        public int Gender { get; set; } = 0;
        [StringLength(32)] public string Citizen { get; set; } = string.Empty;
        [StringLength(512)] public string Address { get; set; } = string.Empty;
        public int IdType { get; set; } = 0;
        [StringLength(128)] public string IdNumber { get; set; } = string.Empty;
        [StringLength(128)] public string IdIssuer { get; set; } = string.Empty;
        [StringLength(20)] public string ReferCode { get; set; } = string.Empty;
        [StringLength(20)] public string PhoneNumber { get; set; } = string.Empty;
        [StringLength(5)] public string CCC { get; set; } = string.Empty;
        public DateOnly? IdIssuedOn { get; set; }
        public DateOnly? IdExpireOn { get; set; }
        [StringLength(256)] public string Email { get; set; } = string.Empty;
    }

    public sealed class TenantUpdateStatusSpec
    {
        public PartyStatusTypes Status { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}