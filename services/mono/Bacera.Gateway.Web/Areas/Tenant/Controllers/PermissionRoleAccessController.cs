
using Bacera.Gateway.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

/// <summary>
/// Permission Role Access Management - SuperAdmin Only
/// </summary>
[Tags("Tenant/Permission Role Access")]
[Route("api/" + VersionTypes.V1 + "/[Area]/permission-role-access")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = UserRoleTypesString.SuperAdmin)]
public class PermissionRoleAccessController(AuthDbContext authDbContext) : TenantBaseController
{
    /// <summary>
    /// Get permission role access list with pagination
    /// </summary>
    /// <param name="criteria"></param>
    /// <returns>Paginated list of permission role access</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<PermissionRoleAccessItem>, PermissionRoleAccessCriteria>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] PermissionRoleAccessCriteria? criteria)
    {
        criteria ??= new PermissionRoleAccessCriteria();
        
        List<PermissionRoleAccessItem> items;
        
        // If TestUrl is provided, use regex matching mode (PostgreSQL ~)
        if (!string.IsNullOrWhiteSpace(criteria.TestUrl))
        {
            // Use raw SQL for regex matching: WHERE '{testUrl}' ~ "Action"
            var sql = $"""
                SELECT r."Id" as "RoleId", r."Name" as "RoleName", p."Id" as "PermissionId", p."Action"
                FROM auth."_Permission" p
                INNER JOIN auth."_PermissionRoleAccess" pra ON p."Id" = pra."PermissionId"
                INNER JOIN auth."_Role" r ON pra."RoleId" = r."Id"
                WHERE p."Category" = 'API' AND p."Auth" = true
                AND @testUrl ~ p."Action"
                """;
            
            if (criteria.RoleId.HasValue)
            {
                sql += " AND r.\"Id\" = @roleId";
            }
            
            sql += " ORDER BY r.\"Id\", p.\"Id\"";
            
            var allItems = await authDbContext.Database
                .SqlQueryRaw<PermissionRoleAccessItem>(sql, 
                    new Npgsql.NpgsqlParameter("@testUrl", criteria.TestUrl),
                    new Npgsql.NpgsqlParameter("@roleId", criteria.RoleId ?? (object)DBNull.Value))
                .ToListAsync();
            
            criteria.Total = allItems.Count;
            criteria.Page = criteria.Page < 1 ? 1 : criteria.Page;
            criteria.Size = criteria.Size < 1 ? 20 : criteria.Size;
            criteria.Size = criteria.Size > 100 ? 100 : criteria.Size;
            criteria.PageCount = (int)Math.Ceiling(criteria.Total / (decimal)criteria.Size);
            criteria.HasMore = criteria.PageCount > criteria.Page;
            
            items = allItems
                .Skip((criteria.Page - 1) * criteria.Size)
                .Take(criteria.Size)
                .ToList();
        }
        else
        {
            // Standard LINQ query with LIKE filter
            var query = from p in authDbContext.Permissions
                where p.Category == "API" && p.Auth == true
                from r in p.ApplicationRoles
                select new PermissionRoleAccessItem
                {
                    RoleId = r.Id,
                    RoleName = r.Name ?? string.Empty,
                    PermissionId = p.Id,
                    Action = p.Action
                };

            // Apply action filter (LIKE/fuzzy match)
            if (!string.IsNullOrWhiteSpace(criteria.Action))
            {
                query = query.Where(x => x.Action.Contains(criteria.Action));
            }

            // Apply RoleId filter
            if (criteria.RoleId.HasValue)
            {
                query = query.Where(x => x.RoleId == criteria.RoleId.Value);
            }

            // Get total count
            criteria.Total = await query.CountAsync();

            // Apply sorting
            query = query.OrderBy(x => x.RoleId).ThenBy(x => x.PermissionId);

            // Apply pagination
            criteria.Page = criteria.Page < 1 ? 1 : criteria.Page;
            criteria.Size = criteria.Size < 1 ? 20 : criteria.Size;
            criteria.Size = criteria.Size > 100 ? 100 : criteria.Size;
            criteria.PageCount = (int)Math.Ceiling(criteria.Total / (decimal)criteria.Size);
            criteria.HasMore = criteria.PageCount > criteria.Page;

            items = await query
                .Skip((criteria.Page - 1) * criteria.Size)
                .Take(criteria.Size)
                .ToListAsync();
        }

        return Ok(Result<List<PermissionRoleAccessItem>, PermissionRoleAccessCriteria>.Of(items, criteria));
    }

    /// <summary>
    /// Add permission to role
    /// </summary>
    /// <param name="spec">RoleId and PermissionId</param>
    /// <returns>Success or error message</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] PermissionRoleAccessSpec spec)
    {
        // Check if role exists
        var role = await authDbContext.ApplicationRoles
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == spec.RoleId);

        if (role == null)
            return BadRequest(Result.Error("Role not found"));

        // Check if permission exists
        var permission = await authDbContext.Permissions.FindAsync(spec.PermissionId);
        if (permission == null)
            return BadRequest(Result.Error("Permission not found"));

        // Check if already exists
        if (role.Permissions.Any(x => x.Id == spec.PermissionId))
            return BadRequest(Result.Error("Permission already assigned to this role"));

        // Add permission to role
        role.Permissions.Add(permission);
        await authDbContext.SaveChangesAsync();

        return Ok(Result.Success("Permission added to role successfully"));
    }

    /// <summary>
    /// Batch add permissions to role
    /// </summary>
    /// <param name="spec">RoleId and list of PermissionIds</param>
    /// <returns>Success or error message with details</returns>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BatchAdd([FromBody] PermissionRoleAccessBatchSpec spec)
    {
        if (spec.PermissionIds == null || spec.PermissionIds.Count == 0)
            return BadRequest(Result.Error("PermissionIds cannot be empty"));

        // Check if role exists
        var role = await authDbContext.ApplicationRoles
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == spec.RoleId);

        if (role == null)
            return BadRequest(Result.Error("Role not found"));

        // Get all permissions that exist
        var permissions = await authDbContext.Permissions
            .Where(x => spec.PermissionIds.Contains(x.Id))
            .ToListAsync();

        var existingPermissionIds = role.Permissions.Select(x => x.Id).ToHashSet();
        var addedIds = new List<long>();
        var skippedIds = new List<long>();
        var notFoundIds = new List<long>();

        foreach (var permissionId in spec.PermissionIds)
        {
            var permission = permissions.FirstOrDefault(x => x.Id == permissionId);
            
            if (permission == null)
            {
                notFoundIds.Add(permissionId);
                continue;
            }

            if (existingPermissionIds.Contains(permissionId))
            {
                skippedIds.Add(permissionId);
                continue;
            }

            role.Permissions.Add(permission);
            existingPermissionIds.Add(permissionId);
            addedIds.Add(permissionId);
        }

        if (addedIds.Count > 0)
        {
            await authDbContext.SaveChangesAsync();
        }

        return Ok(Result.Success("Batch add completed", new
        {
            Added = addedIds,
            AddedCount = addedIds.Count,
            Skipped = skippedIds,
            SkippedCount = skippedIds.Count,
            NotFound = notFoundIds,
            NotFoundCount = notFoundIds.Count
        }));
    }

    /// <summary>
    /// Remove permission from role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Success or error message</returns>
    [HttpDelete]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromQuery] long roleId, [FromQuery] long permissionId)
    {
        // Check if role exists with the permission
        var role = await authDbContext.ApplicationRoles
            .Include(x => x.Permissions.Where(p => p.Id == permissionId))
            .FirstOrDefaultAsync(x => x.Id == roleId);

        if (role == null)
            return NotFound(Result.Error("Role not found"));

        var permission = role.Permissions.FirstOrDefault(x => x.Id == permissionId);
        if (permission == null)
            return NotFound(Result.Error("Permission not assigned to this role"));

        // Remove permission from role
        role.Permissions.Remove(permission);
        await authDbContext.SaveChangesAsync();

        return Ok(Result.Success("Permission removed from role successfully"));
    }

    #region Models

    public class PermissionRoleAccessItem
    {
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public long PermissionId { get; set; }
        public string Action { get; set; } = string.Empty;
    }

    public class PermissionRoleAccessCriteria
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
        public int Total { get; set; }
        public int PageCount { get; set; }
        public bool HasMore { get; set; }
        
        /// <summary>
        /// Filter by action (LIKE/fuzzy match). Use this to search actions containing a keyword.
        /// Example: action=verification
        /// </summary>
        public string? Action { get; set; }
        
        /// <summary>
        /// Test URL for regex matching. Use this to find which permissions match a specific URL.
        /// Uses PostgreSQL regex: WHERE '{testUrl}' ~ "Action"
        /// Example: testUrl=/api/v1/tenant/account/10187/parent-accounts
        /// </summary>
        public string? TestUrl { get; set; }
        
        /// <summary>
        /// Filter by RoleId
        /// </summary>
        public long? RoleId { get; set; }
    }

    public class PermissionRoleAccessSpec
    {
        public long RoleId { get; set; }
        public long PermissionId { get; set; }
    }

    public class PermissionRoleAccessBatchSpec
    {
        public long RoleId { get; set; }
        public List<long> PermissionIds { get; set; } = new();
    }

    #endregion
}
