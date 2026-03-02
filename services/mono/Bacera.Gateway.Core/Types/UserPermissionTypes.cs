using System.Xml;

namespace Bacera.Gateway
{
    public static class UserPermissionTypes
    {
        public const string RoleRead = "role.read";
        public const string RoleWrite = "role.write";
        public const string RoleAll = "role.all";

        public const string UserRead = "user.read";
        public const string UserWrite = "user.write";
        public const string UserAll = "user.all";

        public const string TenantRead = "tenant.read";
        public const string TenantWrite = "tenant.write";
        public const string TenantAll = "tenant.all";

        public const string PermissionRead = "permission.read";
        public const string PermissionWrite = "permission.write";
        public const string PermissionAll = "permission.all";

        public const string DomainRead = "domain.read";
        public const string DomainWrite = "domain.write";
        public const string DomainAll = "domain.all";

        public const string AccountRead = "account.read";
        public const string AccountWrite = "account.write";
        public const string AccountAll = "account.all";

        public const string ApplicationWholesaleDisabled = "application.wholesale.disabled";

        public static readonly string[] All =
        {
            RoleRead,
            RoleWrite,
            RoleAll,
            UserRead,
            UserWrite,
            UserAll,
            TenantRead,
            TenantWrite,
            TenantAll,
            PermissionRead,
            PermissionWrite,
            PermissionAll,
            DomainRead,
            DomainWrite,
            DomainAll,
            AccountRead,
            AccountWrite,
            AccountAll
        };
    }
}