namespace Bacera.Gateway.Vendor.TronProCrypto;

using Newtonsoft.Json;
using System.Collections.Generic;

public class TronProCryptoApiResponse
{
    [JsonProperty("contractMap")] public Dictionary<string, bool> ContractMap { get; set; } = new();

    [JsonProperty("tokenInfo")] public TokenInfo TokenInfo { get; set; } = new();

    [JsonProperty("page_size")] public int PageSize { get; set; }

    [JsonProperty("code")] public int Code { get; set; }

    [JsonProperty("data")] public List<TransactionData> Data { get; set; } = [];

    public static bool TryParse(string json, out TronProCryptoApiResponse result)
    {
        try
        {
            result = JsonConvert.DeserializeObject<TronProCryptoApiResponse>(json)!;
            return true;
        }
        catch
        {
            result = new TronProCryptoApiResponse();
            return false;
        }
    }
}

public class TokenInfo
{
    [JsonProperty("tokenId")] public string TokenId { get; set; } = string.Empty;

    [JsonProperty("tokenAbbr")] public string TokenAbbr { get; set; } = string.Empty;

    [JsonProperty("tokenName")] public string TokenName { get; set; } = string.Empty;

    [JsonProperty("tokenDecimal")] public int TokenDecimal { get; set; }

    [JsonProperty("tokenCanShow")] public int TokenCanShow { get; set; }

    [JsonProperty("tokenType")] public string TokenType { get; set; } = string.Empty;

    [JsonProperty("tokenLogo")] public string TokenLogo { get; set; } = string.Empty;

    [JsonProperty("tokenLevel")] public string TokenLevel { get; set; } = string.Empty;

    [JsonProperty("issuerAddr")] public string IssuerAddr { get; set; } = string.Empty;

    [JsonProperty("vip")] public bool Vip { get; set; } = false;
}

public class TransactionData
{
    [JsonProperty("amount")] public string Amount { get; set; } = string.Empty;

    [JsonProperty("status")] public int Status { get; set; }

    [JsonProperty("approval_amount")] public string ApprovalAmount { get; set; } = string.Empty;

    [JsonProperty("block_timestamp")] public long BlockTimestamp { get; set; }

    [JsonProperty("block")] public int Block { get; set; }

    [JsonProperty("from")] public string From { get; set; } = string.Empty;

    [JsonProperty("to")] public string To { get; set; } = string.Empty;

    [JsonProperty("hash")] public string Hash { get; set; } = string.Empty;

    [JsonProperty("contract_address")] public string ContractAddress { get; set; } = string.Empty;

    [JsonProperty("confirmed")] public int Confirmed { get; set; }

    [JsonProperty("contract_type")] public string ContractType { get; set; } = string.Empty;

    [JsonProperty("contractType")] public int ContractTypeNumber { get; set; }

    [JsonProperty("revert")] public int Revert { get; set; }

    [JsonProperty("contract_ret")] public string ContractRet { get; set; } = string.Empty;

    [JsonProperty("final_result")] public string FinalResult { get; set; } = string.Empty;

    [JsonProperty("event_type")] public string EventType { get; set; } = string.Empty;

    [JsonProperty("issue_address")] public string IssueAddress { get; set; } = string.Empty;

    [JsonProperty("decimals")] public int Decimals { get; set; }

    [JsonProperty("token_name")] public string TokenName { get; set; } = string.Empty;

    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("direction")] public int Direction { get; set; }
}