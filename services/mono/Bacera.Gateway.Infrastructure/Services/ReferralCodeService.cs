using Bacera.Gateway.Connection;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Services;

public class ReferralCodeService(TenantDbConnection dbConnection, ILogger<ReferralCodeService> logger)
{
    public async Task<List<AccountTypes>?> GetAccountTypesInReferralCodeAsync(string referralCode)
    {
        var (_, accountTypesRaw) = await dbConnection.FirstOrDefaultAsync<(string?, string?)>
        ($"""
          SELECT rrc."Code" as "Code",
                 COALESCE(
                     (SELECT jsonb_agg(cast(elem ->> 'accountType' as int))
                      FROM jsonb_array_elements(rrc."Schema") as elem),
                     '[]'::jsonb
                 ) as "AccountType"
          FROM (
              SELECT rc."Code" as "Code",
                     CASE
                         WHEN rc."ServiceType" = 200 OR rc."ServiceType" = 300
                             THEN cast(rc."Summary"::jsonb ->> 'schema' AS jsonb)
                         ELSE cast(rc."Summary"::jsonb ->> 'allowAccountTypes' AS jsonb)
                     END as "Schema"
              FROM core."_ReferralCode" rc
              WHERE "ServiceType" IN (200, 300, 400) AND rc."Code" = '{referralCode}'
              ORDER BY rc."Id" DESC
          ) as rrc;
          """);
        if (accountTypesRaw == null) return null;

        var items = Utils.JsonDeserializeObjectWithDefault<List<int>>(accountTypesRaw);
        return items.Select(x => (AccountTypes)x).ToList();
    }

    public async Task<bool> SetAgentDefaultClientReferralCodeAsync(long agentAccountId, string code)
    {
        try
        {
            await dbConnection.ExecuteAsync($"""
                                             UPDATE core."_ReferralCode" SET "IsDefault" = 0 WHERE "ServiceType" = {(int)ReferralServiceTypes.Client} 
                                                                                               AND "AccountId" = {agentAccountId};
                                             UPDATE core."_ReferralCode" SET "IsDefault" = 1 WHERE "ServiceType" = {(int)ReferralServiceTypes.Client} 
                                                                                               AND "AccountId" = {agentAccountId} 
                                                                                               AND "Code" = '{code}';
                                             """);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to set default referral code for agent account {agentAccountId}", agentAccountId);
            return false;
        }

        return true;
    }
}