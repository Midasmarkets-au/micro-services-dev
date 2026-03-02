using Amazon.Runtime;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Core.Utility;
using Bacera.Gateway.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Bacera.Gateway.Vendor.ChinaPay;

public class ChinaPay
{
    public static async Task<object> TestAsync(HttpClient client, string config, ILogger logger)
    {
        var options = ChinaPayOptions.FromJson(config);

        var request = new RequestClient
        {
            Amount = 147,
            PaymentNumber = Payment.GenerateNumber(),
            AccountUid = 32245523,
            NativeName = "zhen yang",
            PhoneNumber = "13242323444",
            Options = options,
            Logger = logger,
            Client = client
        };

        var response = await request.RequestAsync();
        return response;
    }

    public sealed class RequestClient
    {
        public long Amount { get; set; }
        public long AccountUid { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public ILogger Logger { get; set; } = null!;
        public ChinaPayOptions Options { get; set; } = null!;
        public HttpClient Client { get; set; } = null!;

        private Dictionary<string, object> BuildData() => new()
        {
            { "payType", Options.PayType },
            { "tradeNo", PaymentNumber },
            { "userId", AccountUid.ToString() },
            { "userName", NativeName },
            { "userPhone", PhoneNumber },
            { "symbol", Options.Symbol },
            { "price", $"{Amount:0.00}" },
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
            { "notifyUrl", Options.CallbackUri }, // Add callback URL for notifications
        };

        private Dictionary<string, string> BuildBasicRequestData() => new()
        {
            { "appId", Options.AppId },
            { "method", Options.InterfaceName },
            { "version", Options.Version },
            { "charset", Options.Charset },
            { "sign", "" },
            { "data", "" },
        };

        public async Task<DepositCreatedResponseModel> RequestAsync()
        {
            var form = BuildData();
            Logger.LogInformation("ChinaPay_Requesting_ChinaPay_with_data: {data}", form);

            var data = JsonConvert.SerializeObject(form);

            var encodedEncrypted = EncryptJava(data, Options.ServerPublicKey, Options.Charset);
            Logger.LogInformation("ChinaPay_Encrypted_data: {encrypted}", encodedEncrypted);

            var signed = RSASignJava(encodedEncrypted, Options.AppPrivateKey, "MD5", Options.Charset);
            Logger.LogInformation("ChinaPay_Sign: {signed}", signed);

            var requestData = BuildBasicRequestData();
            requestData["sign"] = signed;
            requestData["data"] = encodedEncrypted;
            Logger.LogInformation("Request data: {requestData}", requestData);

            var query = requestData
                .Select(x => $"{x.Key}={x.Value}")
                .Aggregate((x, y) => $"{x}&{y}");
            Logger.LogInformation("ChinaPay_Query: {query}", query);

            var endPoint = $"{Options.ApiDomain}?{query}";
            Logger.LogInformation("ChinaPay_Endpoint: {endPoint}", endPoint);

            var response = await Client.GetAsync(endPoint);
            var content = await response.Content.ReadAsStringAsync();
            Logger.LogInformation("ChinaPay_Response: {content}", content);

            var body = Utils.JsonDeserializeDynamic(content);
            int code = body.code;
            string message = body.message ?? string.Empty;
            if (code != 0) return DepositCreatedResponseModel.Fail(message, true);

            string redirectUrl = body.body?.url ?? string.Empty;
            var result = new DepositCreatedResponseModel
            {
                IsSuccess = true,
                RedirectUrl = redirectUrl,
                Action = PaymentResponseActionTypes.Redirect,
                PaymentNumber = PaymentNumber,
            };
            return result;
        }
    }

    public static string EncryptJava(string data, string publicKeyJava, string encoding = "UTF-8")
    {
        if (string.IsNullOrEmpty(encoding))
        {
            encoding = "UTF-8";
        }

        var rsa = new RSACryptoServiceProvider();
        rsa.FromPublicKeyJavaString(publicKeyJava);

        const int size = 117;
        int len;
        var dataBytes = Encoding.GetEncoding(encoding).GetBytes(data);
        var bytesLen = dataBytes.Length;
        if (bytesLen % size == 0)
        {
            len = bytesLen / size;
        }
        else
        {
            len = bytesLen / size + 1;
        }

        var result = new List<byte>();
        for (var i = 0; i < len; i++)
        {
            var buff = new List<byte>();
            var start = size * i;
            for (var j = 0; j < size; j++)
            {
                buff.Add(dataBytes[j + start]);
                if (start + j + 1 >= bytesLen)
                {
                    break;
                }
            }

            result.AddRange(rsa.Encrypt(buff.ToArray(), false));
        }

        var base64Encoded = Convert.ToBase64String(result.ToArray());

        var urlEncoded = base64Encoded
            .Select(x => System.Web.HttpUtility.UrlEncode(x.ToString(), Encoding.GetEncoding(encoding)))
            .Select(x => x.Length > 1 ? x.ToUpper() : x)
            .Aggregate((x, y) => x + y);

        return urlEncoded;
    }

    private static string RSASignJava(string data, string privateKeyJava, string hashAlgorithm = "MD5",
        string encoding = "UTF-8")
    {
        var rsa = new RSACryptoServiceProvider();
        rsa.FromPrivateKeyJavaString(privateKeyJava); //加载私钥

        var dataBytes = Encoding.GetEncoding(encoding).GetBytes(data);
        var hashByteSignature = rsa.SignData(dataBytes, hashAlgorithm);
        var base64Encoded = Convert.ToBase64String(hashByteSignature);
        var urlEncoded = base64Encoded
            .Select(x => System.Web.HttpUtility.UrlEncode(x.ToString(), Encoding.GetEncoding(encoding)))
            .Aggregate((x, y) => x + y);

        return urlEncoded;
    }

    public static bool VerifySign(string data, string serverPublicKey, string sign, string charset = "UTF-8",
        string hashAlgorithm = "MD5")
    {
        sign = System.Web.HttpUtility.UrlDecode(sign, Encoding.GetEncoding(charset));

        var rsa = new RSACryptoServiceProvider();
        //导入公钥，准备验证签名
        rsa.FromPublicKeyJavaString(serverPublicKey);
        //返回数据验证结果
        var bytes = Encoding.GetEncoding(charset).GetBytes(data);
        var rgbSignature = Convert.FromBase64String(sign);

        return rsa.VerifyData(bytes, "MD5", rgbSignature);
    }

    public static string DecryptJava(string privateKeyJava, string data, string encoding = "UTF-8")
    {
        data = System.Web.HttpUtility.UrlDecode(data, Encoding.GetEncoding(encoding));
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        rsa.FromPrivateKeyJavaString(privateKeyJava);
        int size = 128;
        byte[] dataBytes = Convert.FromBase64String(data);
        int bytesLen = dataBytes.Length;
        int len;
        if (bytesLen % size == 0)
        {
            len = bytesLen / size;
        }
        else
        {
            len = bytesLen / size + 1;
        }

        List<byte> result = new List<byte>();
        for (int i = 0; i < len; i++)
        {
            List<byte> buff = new List<byte>();
            int start = i * size;
            for (int j = 0; j < size; j++)
            {
                buff.Add(dataBytes[start + j]);
                if (start + j + 1 >= bytesLen)
                {
                    break;
                }
            }

            result.AddRange(rsa.Decrypt(buff.ToArray(), false));
        }


        return Encoding.GetEncoding(encoding).GetString(result.ToArray());
    }
}

