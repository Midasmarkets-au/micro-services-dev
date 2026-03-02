using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Vendor.MonetixPay;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway.Vendor.Monetix;

public class Monetix
{
    public static async Task<object> TestAsync(string config, ILogger logger)
    {
        var options = MonetixOptions.FromJson(config);
        var client = new RequestClient
        {
            Amount = 8114000,
            PaymentNumber = Payment.GenerateNumber(),
            AccountUid = 325150104,
            Email = "dfdadf@bacera.com",
            FirstName = "aafa",
            LastName = "dong",
            CurrencyId = CurrencyTypes.KHR,
            Options = options
        };

        var response = await client.RequestAsync(true);
        return response;
    }

    public sealed class RequestClient
    {
        public decimal Amount { get; set; }
        public long AccountUid { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CurrencyTypes CurrencyId { get; set; } = CurrencyTypes.KHR;
        public MonetixOptions Options { get; set; } = new();
        public HttpClient Client { get; set; } = null!;

        private decimal ActualPaymentAmount => CurrencyId switch
        {
            CurrencyTypes.MYR or CurrencyTypes.THB or CurrencyTypes.KHR => Math.Ceiling(Amount / 100m) * 100,
            CurrencyTypes.VND => Math.Ceiling(Amount / 100m),
            _ => Amount
        };

        private Dictionary<string, object> BuildForm() =>
            new()
            {
                { "project_id", Options.ProjectId },
                { "payment_amount", $"{ActualPaymentAmount}" },
                { "payment_id", PaymentNumber },
                { "payment_currency", $"{Enum.GetName(CurrencyId)}" },
                { "customer_id", AccountUid.ToString() },
                { "customer_first_name", FirstName },
                { "customer_last_name", LastName },
                { "customer_email", Email },
            };

        private string GetSignature(Dictionary<string, object> form)
        {
            // url += Options.MerchantSecret;
            var baseItemList = form
                .Select(x => $"{x.Key}:{x.Value}")
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();
            // var baseString = string.Join(";", baseItemList);
            var baseString = FlattenDictionary(form);
            var signature = Utils.HmacSha512(baseString, Options.SecretKeyForSignature);
            return signature;
        }

        private async Task<string?> GetRequestEndPoint()
        {
            var urlToRequestDomain = $"https://{Options.Login}:{Options.Password}@{Options.Server}";

            // use my header to create a http client
            using var handler = new HttpClientHandler();
            
            var request = new HttpRequestMessage(HttpMethod.Get, urlToRequestDomain);
            request.Headers.Authorization = null;
            request.Headers.Remove("Authorization");

            if (Client.DefaultRequestHeaders.Contains("Authorization"))
            {
                Client.DefaultRequestHeaders.Remove("Authorization");
            }

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Options.Login}:{Options.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var endPointResponse = await Client.SendAsync(request);
            if (!endPointResponse.IsSuccessStatusCode) return null;

            var endPoint = await endPointResponse.Content.ReadAsStringAsync();
            endPoint = endPoint.Trim().Trim('\n', '\r');
            return endPoint;
        }

        public async Task<DepositCreatedResponseModel> RequestAsync(bool isTest = false)
        {
            var endPoint = await GetRequestEndPoint();

            if (string.IsNullOrEmpty(endPoint))
                return new DepositCreatedResponseModel { IsSuccess = false, Message = "Failed to get request endpoint", };

            var form = BuildForm();
            var signature = GetSignature(form);
            form.Add("signature", signature);
            var query = string.Join("&", form.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString((string)x.Value)}"));
            var param = $"/payment?{query}";
            var encryptedParam = Utils.AesEncrypt(param, Options.SecretKeyForAesEncrypt);

            var url = $"https://{endPoint}/{Options.ProjectId}/{encryptedParam}";

            if (isTest)
            {
                var dd = await Client.GetAsync(url);
                var ddBody = await dd.Content.ReadAsStringAsync();
                return new DepositCreatedResponseModel();
            }

            return new DepositCreatedResponseModel
            {
                IsSuccess = true,
                PaymentNumber = PaymentNumber,
                Action = PaymentResponseActionTypes.Redirect,
                RedirectUrl = url,
            }; 
        }
    }

    public static bool VerifySignatureFromCb(string bodyString, string signature, string secretKey)
    {
        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(bodyString, new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None // Prevent date parsing
        })!;
        dict.Remove("signature");
        var baseString = FlattenDictionary(dict);
        var expectedSignature = Utils.HmacSha512(baseString, secretKey);
        return signature == expectedSignature;
    }

    private static string FlattenDictionary(Dictionary<string, object> dictionary)
    {
        var result = new List<string>();
        Helper(dictionary, "", result);
        return result.OrderBy(x => x, StringComparer.Ordinal).Aggregate((x, y) => $"{x};{y}");

        void Helper(Dictionary<string, object> dict, string prefix, List<string> res)
        {
            foreach (var kvp in dict)
            {
                var newPrefix = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}:{kvp.Key}";
                switch (kvp.Value)
                {
                    case JObject nestedObject:
                        Helper(nestedObject.ToObject<Dictionary<string, object>>()!, newPrefix, res);
                        break;
                    case JArray array:
                    {
                        for (var i = 0; i < array.Count; i++)
                        {
                            if (array[i] is JObject obj)
                            {
                                Helper(obj.ToObject<Dictionary<string, object>>()!, $"{newPrefix}[{i}]", res);
                            }
                            else
                            {
                                res.Add($"{newPrefix}[{i}]:{array[i]}");
                            }
                        }

                        break;
                    }
                    default:
                    {
                        if (DateTime.TryParseExact(kvp.Value.ToString()
                                , "yyyy-MM-ddTHH:mm:ssK"
                                , null
                                , DateTimeStyles.AdjustToUniversal
                                , out var dt))
                        {
                            var formattedDate = dt.ToString("yyyy-MM-ddTHH:mm:ss+0000", CultureInfo.InvariantCulture);
                            res.Add($"{newPrefix}:{formattedDate}");
                        }
                        else
                        {
                            res.Add($"{newPrefix}:{kvp.Value}");
                        }

                        break;
                    }
                }
            }
        }
    }
}