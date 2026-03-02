namespace Bacera.Gateway;

public enum AuditTypes
{
    Unknown = 0,
    User = 10,
    Account = 20,
    TradeAccount = 21,
    Wallet = 22,
    PaymentService = 23,
    TradeAccountBalance = 24,
    ExternalTradeAccount = 100,
    ExchangeRate = 120,
    RebateClientRule = 130,
    RebateDirectSchema = 131,
    RebateDirectSchemaItem = 132,
    RebateAgentRule = 133,
    RebateDirectRule = 134,
    Configuration = 160,
}