namespace Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;

public static class CacheKeys
{
    public static string GetAccountTenantIdHashKey() => "AccountToTenantIdHashKey";
    public static string GetAccountNumberToAccountHashKey() => "AccountNumberToAccountHashKey";
    public static string GetServiceIdToNameHashKey() => "ServiceIdToNameHashKey";
    public static string GetAccountIdToAccountHashKey() => "AccountIdToAccountHashKey";
    public static string GetAccountUidToAccountHashKey() => "AccountUidToAccountHashKey";
    public static string GetTradeToQueueHashKey() => "TradeToQueueHashKey";
    public static string GetReleaseDisabledKey() => "ReleaseRebateDisabledKey";

    public static string GetBlackedUserNameHashKey() => "BlackedUserNameHashKey";
    public static string GetBlackedUserPhoneHashKey() => "BlackedUserPhoneHashKey";
    public static string GetBlackedUserEmailHashKey() => "BlackedUserEmailHashKey";
    public static string GetBlackedUserIdNumberHashKey() => "BlackedUserIdNumberHashKey";

    public static string GetBlackedIpHashKey() => "IpHashKey";
    public static string GetAccountCheckHashKey() => "account_check_hash_key";
    public static string GetCentralTradeServiceKey() => "central_trade_service_key";
    public static string GetTenantTradeServiceKey(long tenantId) => $"central_trade_service_key_tid:{tenantId}";

    public static string GetPartyIdToPartyHashKey(long tenantId) => $"partyId_Party_hkey_tid:{tenantId}";
    public static string GetPartyAccountUidsHashKey(long tenantId) => $"party_account_uids_hkey_tid:{tenantId}";
    public static string GetAccountUidToIdHashKey(long tenantId) => $"account_uid_to_id_hkey_tid:{tenantId}";

    public static string GetPartyIdByAccountIdHashKey(long tenantId) => $"party_id_by_account_id_hkey_tid:{tenantId}";

    public static string GetPartyExtraRelationAccountKey(long tenantId, long partyId, long accountId) =>
        $"party_extra_account_tid:{tenantId}_pid:{partyId}_aid:{accountId}";
    public static string GetPartyIdByWalletIdHashKey(long tenantId) => $"party_id_by_wallet_id_hkey_tid:{tenantId}";

    public const string EventKeyToIdCache = "eventKeyToId";

    public static string GetPaymentMethodKey(long tenantId) => $"payment_method_key_tid:{tenantId}";
    public static string GetPaymentMethodHashKey(long tenantId) => $"payment_method_hash_key_tid:{tenantId}";

    public static string GetPermissionKey() => "permission_key";
    public static string GetRolesKey() => "user_roles";
    public static string GetConfigHashKey(long tenantId) => $"config_hkey_tid:{tenantId}";
    public static string GetAllTagKey(long tenantId) => $"all_tag_tid:{tenantId}";
    public static string GetConfigurationAllKey(long tenantId) => $"configuration_all_tid:{tenantId}";
    public static string GetUserLockedHashKey(long tenantId) => $"user_locked_hkey_tid:{tenantId}";
    public static string CryptoWalletPaymentHKey(string address, long amount) => $"crypto_wallet_payment_key_{address}_{amount}";
    public static string CryptoWalletKey(string address) => $"crypto_wallet_hkey_addr:{address}";
    public static string GetSendBatchEmailContentHKey(long tenantId) => $"send_batch_email_content_hkey_tid{tenantId}";
    public static string GetSendBatchEmailsHKey(long tenantId) => $"send_batch_emails_hkey_tid{tenantId}";
    public static string GetSendBatchEmailFailedHKey(long tenantId) => $"send_batch_email_failed_hkey_tid{tenantId}";

    public static string GetSendBatchEmailHasSentHKey(string topicKey) =>
        $"send_batch_email_has_sent_hkey_topic_{topicKey}";
    public static string GetWsOnlineAdminHKey() => "bcrweb_app_ws_realtime_log_hkey";

    public static string UserTokenInvalidKey(long tenantId, long partyId) => $"user_token_invalid_key_tid{tenantId}_pid:{partyId}";

    public static List<string> GetResetCacheKeys(List<long> tenantIds)
    {
        var results = new List<string>();
        results.AddRange(tenantIds.Select(GetPaymentMethodKey));
        results.AddRange(tenantIds.Select(GetTenantTradeServiceKey));
        results.AddRange(tenantIds.Select(GetConfigurationAllKey));
        results.AddRange(tenantIds.Select(GetConfigHashKey));
        results.AddRange(tenantIds.Select(GetPartyIdToPartyHashKey));
        results.AddRange(tenantIds.Select(GetUserLockedHashKey));
        results.AddRange(tenantIds.Select(GetAllTagKey));
        results.Add(GetCentralTradeServiceKey());
        results.Add(GetPermissionKey());
        results.Add(GetWsOnlineAdminHKey());
        results.Add(GetRolesKey());
        results.Add(EventKeyToIdCache);
        return results;
    }
}

public static class DistributedLockKeys
{
    public static string GetUpdatePaymentMethodKey(long tenantId) => $"update_payment_method_hkey_tid:{tenantId}";
    public static string GetCalculateRebateKey(long tenantId) => $"calculate_rebate_lock_key_tid:{tenantId}";

    public static string GetProcessTransactionKey(long tenantId, long transactionId) =>
        $"complete_transaction_lock_key_tid:{tenantId}_transid:{transactionId}";

    public static string GetProcessRebateKey(long tenantId, long rebateId) =>
        $"complete_rebate_lock_key_tid:{tenantId}_rebateid:{rebateId}";

    public static string GetProcessDepositKey(long tenantId, long depositId) =>
        $"approve_withdrawal_lock_key_tid:{tenantId}_depositid:{depositId}";

    public static string GetProcessMatterKey(long tenantId, long matterId) =>
        $"process_matter_lock_key_tid:{tenantId}_matterid:{matterId}";
}