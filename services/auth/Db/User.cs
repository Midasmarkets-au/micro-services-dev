using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway.Auth.Db;

/// <summary>
/// Mirrors auth."_User" — ASP.NET Identity table with custom fields.
/// </summary>
public class User
{
    [Column("Id")]
    public long Id { get; set; }

    [Column("Uid")]
    public long Uid { get; set; }

    [Column("PartyId")]
    public long PartyId { get; set; }

    [Column("TenantId")]
    public long TenantId { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("NormalizedEmail")]
    public string? NormalizedEmail { get; set; }

    [Column("EmailConfirmed")]
    public bool EmailConfirmed { get; set; }

    [Column("PasswordHash")]
    public string? PasswordHash { get; set; }

    [Column("Status")]
    public short Status { get; set; }

    [Column("LockoutEnabled")]
    public bool LockoutEnabled { get; set; }

    [Column("LockoutEnd")]
    public DateTimeOffset? LockoutEnd { get; set; }

    [Column("TwoFactorEnabled")]
    public bool TwoFactorEnabled { get; set; }

    [Column("NativeName")]
    public string NativeName { get; set; } = string.Empty;

    [Column("FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [Column("LastName")]
    public string LastName { get; set; } = string.Empty;

    [Column("Language")]
    public string Language { get; set; } = string.Empty;

    [Column("LastLoginOn")]
    public DateTime? LastLoginOn { get; set; }

    [Column("LastLoginIp")]
    public string LastLoginIp { get; set; } = string.Empty;
}
