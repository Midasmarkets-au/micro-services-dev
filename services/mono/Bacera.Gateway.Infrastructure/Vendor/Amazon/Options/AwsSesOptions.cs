using Amazon;

namespace Bacera.Gateway.Vendor.Amazon.Options;

public class AwsSesOptions : AwsOptions
{
    public string FromAddress { get; set; } = null!;
    public string FromDisplayName { get; set; } = null!;

    public static AwsSesOptions Of(string key, string secret, string region, string from, string fromDisplayName = "")
        => new()
        {
            AccessKey = key,
            SecretKey = secret,
            FromAddress = from,
            Region = RegionEndpoint.GetBySystemName(region),
            FromDisplayName = !string.IsNullOrEmpty(fromDisplayName) ? fromDisplayName : from,
        };
}