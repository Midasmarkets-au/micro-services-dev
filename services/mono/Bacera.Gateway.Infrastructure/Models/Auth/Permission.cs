using System;
using System.Collections.Generic;
using Bacera.Gateway.Auth;

namespace Bacera.Gateway;

public partial class Permission
{
    public long Id { get; set; }

    public string Action { get; set; } = null!;

    public string Method { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Key { get; set; } = null!;

    public bool Auth { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<ApplicationRole> ApplicationRoles { get; set; } = new List<ApplicationRole>();
}
