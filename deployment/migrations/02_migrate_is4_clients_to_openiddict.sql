-- =============================================================================
-- Step 2B: Migrate IdentityServer4 Clients → OpenIddict Applications
-- Database: portal_central  |  Schema: identity
-- Run AFTER: 01_openiddict_create_tables.sql
-- Safe to re-run: INSERT ... ON CONFLICT DO NOTHING
-- =============================================================================

BEGIN;

-- ---------------------------------------------------------------------------
-- IS4 Clients → OpenIddictApplications
-- Grant type mapping:
--   password           → gt:password
--   refresh_token      → gt:refresh_token
--   client_credentials → gt:client_credentials
--   authorization_code → gt:authorization_code
-- Scope mapping:  "api" → scp:api,  "openid" → scp:openid, etc.
-- ---------------------------------------------------------------------------
INSERT INTO identity."OpenIddictApplications" (
    "Id",
    "ClientId",
    "ClientSecret",
    "DisplayName",
    "Permissions",
    "RedirectUris",
    "PostLogoutRedirectUris",
    "CorsOrigins",
    "Requirements",
    "Type",
    "ConsentType",
    "Properties",
    "Settings"
)
SELECT
    gen_random_uuid()::text                          AS "Id",
    c."ClientId"                                     AS "ClientId",

    -- First active secret (IS4 already stores it hashed)
    (SELECT cs."Value"
     FROM identity."ClientSecrets" cs
     WHERE cs."ClientId" = c."Id"
     ORDER BY cs."Created" DESC LIMIT 1)             AS "ClientSecret",

    COALESCE(c."ClientName", c."ClientId")           AS "DisplayName",

    -- Permissions = grant types + scopes + standard endpoints
    (SELECT jsonb_agg(DISTINCT perm)
     FROM (
         SELECT CASE cgt."GrantType"
             WHEN 'password'           THEN 'gt:password'
             WHEN 'refresh_token'      THEN 'gt:refresh_token'
             WHEN 'client_credentials' THEN 'gt:client_credentials'
             WHEN 'authorization_code' THEN 'gt:authorization_code'
             WHEN 'implicit'           THEN 'gt:implicit'
             ELSE 'gt:' || cgt."GrantType"
         END AS perm
         FROM identity."ClientGrantTypes" cgt WHERE cgt."ClientId" = c."Id"
         UNION ALL
         SELECT CASE cs2."Scope"
             WHEN 'openid'         THEN 'scp:openid'
             WHEN 'profile'        THEN 'scp:profile'
             WHEN 'offline_access' THEN 'scp:offline_access'
             ELSE 'scp:' || cs2."Scope"
         END
         FROM identity."ClientScopes" cs2 WHERE cs2."ClientId" = c."Id"
         UNION ALL SELECT 'ept:token'
         UNION ALL SELECT 'ept:introspection'
         UNION ALL SELECT 'ept:revocation'
     ) t(perm))                                      AS "Permissions",

    (SELECT jsonb_agg(r."RedirectUri")
     FROM identity."ClientRedirectUris" r
     WHERE r."ClientId" = c."Id")                    AS "RedirectUris",

    (SELECT jsonb_agg(p."PostLogoutRedirectUri")
     FROM identity."ClientPostLogoutRedirectUris" p
     WHERE p."ClientId" = c."Id")                    AS "PostLogoutRedirectUris",

    (SELECT jsonb_agg(o."Origin")
     FROM identity."ClientCorsOrigins" o
     WHERE o."ClientId" = c."Id")                    AS "CorsOrigins",

    CASE WHEN c."RequirePkce"
         THEN '["ft:pkce"]'::jsonb
         ELSE '[]'::jsonb
    END                                              AS "Requirements",

    CASE WHEN c."RequireClientSecret"
         THEN 'confidential'
         ELSE 'public'
    END                                              AS "Type",

    'implicit'                                       AS "ConsentType",

    jsonb_build_object(
        'is4_access_token_lifetime',     c."AccessTokenLifetime",
        'is4_sliding_refresh_lifetime',  c."SlidingRefreshTokenLifetime",
        'is4_allow_offline_access',      c."AllowOfflineAccess",
        'is4_migrated_at',               now()
    )                                                AS "Properties",

    jsonb_build_object(
        'token_lifetimes', jsonb_build_object(
            'access_token',  c."AccessTokenLifetime"::text,
            'refresh_token', c."SlidingRefreshTokenLifetime"::text
        )
    )                                                AS "Settings"

FROM identity."Clients" c
WHERE c."Enabled" = true
ON CONFLICT ("ClientId") DO NOTHING;

-- ---------------------------------------------------------------------------
-- IS4 ApiScopes → OpenIddictScopes
-- ---------------------------------------------------------------------------
INSERT INTO identity."OpenIddictScopes" (
    "Id",
    "Name",
    "DisplayName",
    "Description",
    "Resources",
    "Properties"
)
SELECT
    gen_random_uuid()::text                          AS "Id",
    s."Name"                                         AS "Name",
    COALESCE(s."DisplayName", s."Name")              AS "DisplayName",
    s."Description"                                  AS "Description",
    (SELECT jsonb_agg(ar."Name")
     FROM identity."ApiResourceScopes" ars
     JOIN identity."ApiResources" ar ON ar."Id" = ars."ApiResourceId"
     WHERE ars."Scope" = s."Name")                   AS "Resources",
    jsonb_build_object('is4_migrated_at', now())     AS "Properties"
FROM identity."ApiScopes" s
WHERE s."Enabled" = true
ON CONFLICT ("Name") DO NOTHING;

COMMIT;

-- =============================================================================
-- Verification (run manually after migration)
-- =============================================================================
-- SELECT "ClientId", "Type", jsonb_array_length("Permissions") AS perm_count
-- FROM identity."OpenIddictApplications";
--
-- SELECT "Name", "Resources" FROM identity."OpenIddictScopes";
--
-- SELECT
--   (SELECT COUNT(*) FROM identity."Clients" WHERE "Enabled" = true) AS is4_clients,
--   (SELECT COUNT(*) FROM identity."OpenIddictApplications")          AS oidc_apps;
