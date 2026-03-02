using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

using M = AuthCode;

public partial class AuthCode
{
    public static class EventLabel
    {
        public static readonly string ResetPassword = "Reset Password";
        public static readonly string Login = "Login";
        public static readonly string TwoFactor = "TwoFactor";
        public static readonly string TwoFactorEnable = "TwoFactorEnable";
        public static readonly string TwoFactorDisable = "TwoFactorDisable";
        public static readonly string PaperVerification = "PaperVerification";
        public static readonly string ParentViewEmail = "ParentViewEmail";
        public static readonly string WalletToWalletTransfer = "WalletToWalletTransfer";
        public static readonly string Withdrawal = "Withdrawal";
        public static readonly string WalletToTradeAccount = "WalletToTradeAccount";
        public static readonly string TradeAccountToTradeAccount = "TradeAccountToTradeAccount";

        /// <summary>
        /// Normalizes the event type string to the correct EventLabel constant (case-insensitive)
        /// </summary>
        /// <param name="eventType">The event type string to normalize</param>
        /// <param name="defaultValue">The default value to return if no match is found</param>
        /// <returns>The matching EventLabel constant or the default value</returns>
        public static string Normalize(string? eventType, string? defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                return defaultValue ?? WalletToWalletTransfer;

            return eventType.Trim().ToLowerInvariant() switch
            {
                "resetpassword" or "reset password" => ResetPassword,
                "login" => Login,
                "twofactor" => TwoFactor,
                "twofactorenable" => TwoFactorEnable,
                "twofactordisable" => TwoFactorDisable,
                "paperverification" => PaperVerification,
                "parentviewemail" => ParentViewEmail,
                "wallettowalletransfer" or "wallet to wallet transfer" => WalletToWalletTransfer,
                "withdrawal" => Withdrawal,
                "transfer" or "wallettotradeaccount" or "wallet to trade account" => WalletToTradeAccount,
                "tradeaccounttotradeaccount" or "trade account to trade account" => TradeAccountToTradeAccount,
                _ => defaultValue ?? eventType
            };
        }
    }

    public static M Build(long partyId, string eventLabel, AuthCodeMethodTypes method, string methodValue,
        long codeLen = 6,
        long expireMinutes = 10) => new()
    {
        PartyId = partyId,
        Status = (short)AuthCodeStatusTypes.Valid,
        Event = eventLabel,
        Method = (short)method,
        MethodValue = methodValue,
        // Code = new Random().Next(100000, 999999).ToString(),
        Code = new Random().Next((int)Math.Pow(10, codeLen - 1), (int)Math.Pow(10, codeLen) - 1).ToString(),
        CreatedOn = DateTime.UtcNow,
        ExpireOn = DateTime.UtcNow.AddMinutes(expireMinutes + 1)
    };
}