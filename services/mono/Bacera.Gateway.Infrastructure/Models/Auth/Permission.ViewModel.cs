// ReSharper disable once CheckNamespace
namespace Bacera.Gateway;

using M = Permission;
public partial class Permission
{
    public sealed class BasicModel
    {
        public long Id { get; set; }
        public bool Auth { get; set; }
        public string Action { get; set; } = null!;
        public string Method { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Key { get; set; } = null!;
    }
}

public static class PermissionViewModelExt
{
    public static IQueryable<M.BasicModel> ToBasicModels(this IQueryable<Permission> query)
        => query.Select(x => new M.BasicModel
        {
            Id = x.Id,
            Auth = x.Auth,
            Action = x.Action,
            Method = x.Method,
            Category = x.Category,
            Key = x.Key,
        });
}