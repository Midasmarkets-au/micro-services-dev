using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Bacera.Gateway.Auth
{
    public partial class User : IdentityUser<long>, IHasOptionalEmail, IHasLanguage
    {
        [Key] public long Uid { get; set; }
        [Key] public long PartyId { get; set; }
        [Key] public long TenantId { get; set; }
        public long ReferrerPartyId { get; set; }
        [StringLength(128)] public string NativeName { get; set; } = string.Empty;
        [StringLength(64)] public string FirstName { get; set; } = string.Empty;
        [StringLength(64)] public string LastName { get; set; } = string.Empty;
        [StringLength(20)] public string Language { get; set; } = string.Empty;
        [StringLength(255)] public string Avatar { get; set; } = string.Empty;
        [StringLength(30)] public string TimeZone { get; set; } = string.Empty;
        [StringLength(20)] public string ReferCode { get; set; } = string.Empty;
        [StringLength(10)] public string CountryCode { get; set; } = string.Empty;
        [StringLength(10)] public string Currency { get; set; } = string.Empty;
        [StringLength(10)] public string CCC { get; set; } = string.Empty;

        public DateOnly Birthday { get; set; } = default;
        public int Gender { get; set; } = 0;
        public short Status { get; set; }
        [StringLength(32)] public string Citizen { get; set; } = string.Empty;
        [StringLength(512)] public string Address { get; set; } = string.Empty;
        public int IdType { get; set; } = 0;
        [StringLength(128)] public string IdNumber { get; set; } = string.Empty;
        [StringLength(128)] public string IdIssuer { get; set; } = string.Empty;
        public DateOnly IdIssuedOn { get; set; } = default;
        public DateOnly IdExpireOn { get; set; } = default;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginOn { get; set; }
        [StringLength(64)] public string RegisteredIp { get; set; } = string.Empty;
        [StringLength(64)] public string LastLoginIp { get; set; } = string.Empty;
        public string ReferPath { get; set; } = string.Empty;
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }

    public class ApplicationRole : IdentityRole<long>
    {
        public virtual ICollection<UserRole>? UserRoles { get; set; }

        // public virtual ICollection<PermissionRoleAccess> PermissionRoleAccesses { get; set; } = new List<PermissionRoleAccess>();
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();


        //public virtual ICollection<RoleClaim>? RoleClaims { get; set; }

        public sealed class BasicModel
        {
            public long Id { get; set; }
            public string? Name { get; set; }
        }
    }

    public class RoleClaim : IdentityRoleClaim<long>
    {
        //public virtual UserRole? Role { get; set; }
        //public virtual UserClaim? Claim { get; set; }
        //public virtual Role Role { get; set; }
    }

    public class UserRole : IdentityUserRole<long>
    {
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
        public virtual ApplicationRole ApplicationRole { get; set; } = null!;
    }

    public class Role : IdentityRole<long>
    {
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class UserClaim : IdentityUserClaim<long>
    {
        //public virtual User User { get; set; }
    }
}