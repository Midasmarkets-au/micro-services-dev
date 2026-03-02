namespace Bacera.Gateway.Vendor.Long77Pay;

public class Long77PayVAResponseModel
{
    public bool IsSuccess => Code == 200;
    public int Code { get; set; }
    public string? Message { get; set; }
    public string? SystemOrderCode { get; set; }
    public string? PartnerOrderCode { get; set; }
    public long? Amount { get; set; }
    public string? PaymentUrl { get; set; }
    public string? PaymentId { get; set; }
    public BankAccountInfo? BankAccount { get; set; }

    public class BankAccountInfo
    {
        public string BankCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNo { get; set; } = string.Empty;
        public string BankAccountName { get; set; } = string.Empty;
    }

    public static Long77PayVAResponseModel FromDynamic(dynamic response)
    {
        // Use SelectToken for all properties to ensure compatibility with JObject
        var codeToken = response.SelectToken("code");
        return new Long77PayVAResponseModel
        {
            Code = codeToken != null ? (int)codeToken : -1,
            Message = (string)response.SelectToken("msg") ?? null,
            SystemOrderCode = (string)response.SelectToken("data.system_order_code") ?? null,
            PartnerOrderCode = (string)response.SelectToken("data.partner_order_code") ?? null,
            Amount = response.SelectToken("data.amount") != null ? (long)response.SelectToken("data.amount") : null,
            PaymentUrl = (string)response.SelectToken("data.payment_url") ?? null,
            PaymentId = (string)response.SelectToken("data.payment_id") ?? null,
            BankAccount = response.SelectToken("data.bank_account") != null
                ? new BankAccountInfo
                {
                    BankCode = (string)response.SelectToken("data.bank_account.bank_code") ?? string.Empty,
                    BankName = (string)response.SelectToken("data.bank_account.bank_name") ?? string.Empty,
                    BankAccountNo = (string)response.SelectToken("data.bank_account.bank_account_no") ?? string.Empty,
                    BankAccountName = (string)response.SelectToken("data.bank_account.bank_account_name") ?? string.Empty
                }
                : null
        };
    }
}

