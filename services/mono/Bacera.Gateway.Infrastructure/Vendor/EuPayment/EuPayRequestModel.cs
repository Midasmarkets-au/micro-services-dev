using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.EuPayment;


public class EuPayRequestModel
{
    [Required] public string BillingLastName { get; set; } = null!;
    [Required] public string BillingFirstName { get; set; } = null!;
    [Required] public string BillingAddress { get; set; } = null!;
    [Required] public string BillingCity { get; set; } = null!;
    [Required] public string BillingState { get; set; } = null!;
    [Required] public string BillingZipcode { get; set; } = null!;
    [Required] public string BillingCountry { get; set; } = null!;
    [Required] public string BillingPhone { get; set; } = null!;
    [Required] public string CcNumber { get; set; } = null!;
    [Required] public string CcMonth { get; set; } = null!;
    [Required] public string CcYear { get; set; } = null!;
    [Required] public string CcCvc { get; set; } = null!;

    public static bool TryParse(string json, out EuPayRequestModel error)
    {
        try
        {
            error = JsonConvert.DeserializeObject<EuPayRequestModel>(json)!;
            return true;
        }
        catch
        {
            error = new EuPayRequestModel();
            return false;
        }
    }

    public EuPayRequestModel GetDesensitizedData()
        => new()
        {
            BillingLastName = BillingLastName,
            BillingFirstName = BillingFirstName,
            BillingAddress = BillingAddress,
            BillingCity = BillingCity,
            BillingState = BillingState,
            BillingZipcode = BillingZipcode,
            BillingCountry = BillingCountry,
            BillingPhone = BillingPhone
        };
}