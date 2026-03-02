namespace Bacera.Gateway;

public class TwoFactorAuthSetting
{
    public bool LoginCodeEnabled { get; set; }
    public bool WalletToWalletTransfer { get; set; } = true;
    public bool WalletToTradeAccount { get; set; } = true;
    public bool TradeAccountToTradeAccount { get; set; } = true;
    public bool Withdrawal { get; set; } = true;
}

public class TwoFactorAuthTransactionSetting
{
    public bool WalletToWalletTransfer { get; set; } = true;
    public bool WalletToTradeAccount { get; set; } = true;
    public bool TradeAccountToTradeAccount { get; set; } = true;
    public bool Withdrawal { get; set; } = true;
}