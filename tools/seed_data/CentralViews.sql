create view "_PermissionRoleAccessView"("Role", "Action", "RoleId", "PermissionId") as
SELECT r."Name" AS "Role",
       p."Action",
       r."Id"   AS "RoleId",
       p."Id"   AS "PermissionId"
FROM auth."_PermissionRoleAccess" pra
         JOIN auth."_Role" r ON pra."RoleId" = r."Id"
         JOIN auth."_Permission" p ON pra."PermissionId" = p."Id"
ORDER BY p."Id";

alter table "_PermissionRoleAccessView"
    owner to bcrpro;

create view "_PermissionUserAccessView"("Email", "TenantId", "PartyId", "Action", "UserId", "PermissionId") as
SELECT u."Email",
       u."TenantId",
       u."PartyId",
       p."Action",
       u."Id" AS "UserId",
       p."Id" AS "PermissionId"
FROM auth."_PermissionUserAccess" pua
         JOIN auth."_User" u ON pua."UserId" = u."Id"
         JOIN auth."_Permission" p ON pua."PermissionId" = p."Id"
ORDER BY p."Id";

alter table "_PermissionUserAccessView"
    owner to bcrpro;

