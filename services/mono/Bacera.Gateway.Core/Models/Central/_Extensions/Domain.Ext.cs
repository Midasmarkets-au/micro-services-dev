using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Bacera.Gateway;

partial class Domain
{
    public sealed class CreateSpec
    {
        [Required] public long TenantId { get; set; }

        [Required, MinLength(3), MaxLength(64)]
        public string DomainName { get; set; } = null!;

        public bool IsValid() => DomainExt.IsDomainName(DomainName);
    }

    public sealed class UpdateSpec
    {
        [Required] public long TenantId { get; set; }

        [Required, MinLength(3), MaxLength(64)]
        public string DomainName { get; set; } = null!;

        public bool IsValid() => DomainExt.IsDomainName(DomainName);
    }

    public static Domain Build(long tenantId, string domainName) =>
        new() { TenantId = tenantId, DomainName = domainName };
}

public static class DomainExt
{
    public static bool IsDomainName(string domainName) =>
        Regex.IsMatch(domainName, @"^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$");
}