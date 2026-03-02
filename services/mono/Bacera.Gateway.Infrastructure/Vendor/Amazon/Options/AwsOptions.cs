using Amazon;
using Amazon.Internal;

namespace Bacera.Gateway.Vendor.Amazon.Options;

public class AwsOptions
{
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public RegionEndpoint Region { get; set; } = RegionEndpoint.USWest1;
}