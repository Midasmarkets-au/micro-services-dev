namespace Bacera.Gateway;

partial class Transaction
{
    public class CreateBetweenAccountSpec
    {
        public long TargetTradeAccountUid { get; set; }
        public long SourceTradeAccountUid { get; set; }
        public long Amount { get; set; }
        /// <summary>
        /// Email verification code (required for trade account to trade account transfer)
        /// </summary>
        public string? VerificationCode { get; set; }
    }
    
    public class CreateBetweenTradeAccountSpec
    {
        public long TargetTradeAccountUid { get; set; }
        public long SourceTradeAccountUid { get; set; }
        public long Amount { get; set; }
        /// <summary>
        /// Email verification code (required for trade account to trade account transfer)
        /// </summary>
        public string? VerificationCode { get; set; }
    }

}