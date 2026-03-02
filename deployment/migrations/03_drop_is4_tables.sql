-- =============================================================================
-- Step 9 (FINAL): Drop IdentityServer4 tables
-- Run ONLY after all steps (3-8) are complete and verified in production
-- =============================================================================

-- Uncomment to execute:

-- BEGIN;

-- DROP TABLE IF EXISTS identity."ClientClaims"                  CASCADE;
-- DROP TABLE IF EXISTS identity."ClientCorsOrigins"             CASCADE;
-- DROP TABLE IF EXISTS identity."ClientGrantTypes"              CASCADE;
-- DROP TABLE IF EXISTS identity."ClientIdPRestrictions"         CASCADE;
-- DROP TABLE IF EXISTS identity."ClientPostLogoutRedirectUris"  CASCADE;
-- DROP TABLE IF EXISTS identity."ClientProperties"              CASCADE;
-- DROP TABLE IF EXISTS identity."ClientRedirectUris"            CASCADE;
-- DROP TABLE IF EXISTS identity."ClientScopes"                  CASCADE;
-- DROP TABLE IF EXISTS identity."ClientSecrets"                 CASCADE;
-- DROP TABLE IF EXISTS identity."Clients"                       CASCADE;

-- DROP TABLE IF EXISTS identity."ApiResourceClaims"             CASCADE;
-- DROP TABLE IF EXISTS identity."ApiResourceProperties"         CASCADE;
-- DROP TABLE IF EXISTS identity."ApiResourceScopes"             CASCADE;
-- DROP TABLE IF EXISTS identity."ApiResourceSecrets"            CASCADE;
-- DROP TABLE IF EXISTS identity."ApiResources"                  CASCADE;

-- DROP TABLE IF EXISTS identity."ApiScopeClaims"                CASCADE;
-- DROP TABLE IF EXISTS identity."ApiScopeProperties"            CASCADE;
-- DROP TABLE IF EXISTS identity."ApiScopes"                     CASCADE;

-- DROP TABLE IF EXISTS identity."IdentityResourceClaims"        CASCADE;
-- DROP TABLE IF EXISTS identity."IdentityResourceProperties"    CASCADE;
-- DROP TABLE IF EXISTS identity."IdentityResources"             CASCADE;

-- DROP TABLE IF EXISTS identity."PersistedGrants"               CASCADE;
-- DROP TABLE IF EXISTS identity."DeviceCodes"                   CASCADE;

-- COMMIT;
