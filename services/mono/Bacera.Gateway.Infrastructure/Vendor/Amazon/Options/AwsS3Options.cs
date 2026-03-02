using Amazon;

namespace Bacera.Gateway.Vendor.Amazon.Options;

public class AwsS3Options : AwsOptions
{
    public string BucketName { get; set; } = null!;
    public string PublicBucketName { get; set; } = null!;

    public static AwsS3Options Of(string key, string secret, string region, string bucketName, string publicBucketName)
        => new()
        {
            AccessKey = key, SecretKey = secret, BucketName = bucketName, PublicBucketName = publicBucketName,
            Region = RegionEndpoint.GetBySystemName(region)
        };
}