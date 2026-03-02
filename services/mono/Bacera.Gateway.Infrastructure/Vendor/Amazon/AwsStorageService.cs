using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Bacera.Gateway.Vendor.Amazon.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Bacera.Gateway.Vendor.Amazon;

public class AwsStorageService : IStorageService
{
    private readonly AwsS3Options _options;
    private readonly AmazonS3Client _client;
    private readonly TenantDbContext _tenantDbContext;
    private readonly ILogger<AwsStorageService> _logger;

    public AwsStorageService(IOptions<AwsS3Options> options, TenantDbContext tenantDbContext,
        ILogger<AwsStorageService>? logger = null)
    {
        _options = options.Value;
        _tenantDbContext = tenantDbContext;
        _logger = logger ?? new NullLogger<AwsStorageService>();
        _client = new AmazonS3Client(_options.AccessKey, _options.SecretKey,
            region: _options.Region);
    }


    public async Task<Medium> UploadFileAndSaveMediaAsync(Stream fileStream,
        string fileName = "",
        string extension = "",
        string type = "unknown",
        long rowId = 0,
        string contentType = "",
        long tenantId = 0,
        long partyId = 0,
        bool saveMedia = true) =>
        await PutFileAndSaveMediaAsync(_options.BucketName, fileStream, fileName, extension, false, type, rowId,
            contentType,
            tenantId,
            partyId,
            saveMedia);

    public async Task<Medium> UploadPublicFileAndSaveMediaAsync(Stream fileStream,
        string fileName = "",
        string extension = "",
        string type = "unknown",
        long rowId = 0,
        string contentType = "",
        long tenantId = 0,
        long partyId = 0,
        bool saveMedia = true) =>
        await PutFileAndSaveMediaAsync(_options.PublicBucketName, fileStream, fileName, extension, true, type, rowId,
            contentType, tenantId, partyId, saveMedia);

    private async Task<Medium> PutFileAndSaveMediaAsync(string bucketName
        , Stream fileStream
        , string fileName = ""
        , string extension = ""
        , bool isPublic = false
        , string type = "unknown"
        , long rowId = 0
        , string contentType = ""
        , long tenantId = 0
        , long partyId = 0
        , bool saveMedia = true
    )
    {
        var length = fileStream.Length;

        var (result, keyName) = await PutFileAsync(bucketName
            , fileStream
            , $"t_{tenantId}/{DateTime.UtcNow:yy}/{DateTime.UtcNow:MM}/"
            , fileName
            , extension
            , isPublic
            , contentType
            , tenantId
            , partyId
        );

        if (!result) return new Medium();

        var media = new Medium
        {
            Guid = Guid.NewGuid().ToString(),
            FileName = fileName.Length <= 256 ? fileName : fileName[..256],
            Type = type,
            RowId = rowId,
            ContentType = contentType,
            TenantId = tenantId,
            PartyId = partyId,
            Length = length,
            Url = $"https://{bucketName}.s3.amazonaws.com/{keyName}"
        };
        if (!saveMedia) return media;
        await _tenantDbContext.Media.AddAsync(media);
        await _tenantDbContext.SaveChangesAsync();
        return media;
    }

    public async Task<(bool, string)> UploadFileAsync(Stream fileStream,
        string fileDirPath = "",
        string fileName = "",
        string extension = "",
        string contentType = "",
        long tenantId = 0,
        long partyId = 0,
        bool changeFileName = true) =>
        await PutFileAsync(_options.BucketName, fileStream, fileDirPath, fileName, extension, false, contentType,
            tenantId, partyId, changeFileName);

    public async Task<(bool, string)> UploadPublicFileAsync(Stream fileStream,
        string fileDirPath = "",
        string fileName = "",
        string extension = "",
        string contentType = "",
        long tenantId = 0,
        long partyId = 0,
        bool changeFileName = true) =>
        await PutFileAsync(_options.BucketName, fileStream, fileDirPath, fileName, extension, true, contentType,
            tenantId, partyId, changeFileName);

    private async Task<(bool, string)> PutFileAsync(string bucketName, Stream fileStream,
        string fileDirPath = "",
        string fileName = "",
        string extension = "",
        bool isPublic = false,
        string contentType = "",
        long tenantId = 0,
        long partyId = 0,
        bool changeFileName = true
    )
    {
        var prefix = string.IsNullOrEmpty(fileDirPath)
            ? $"t_{tenantId}/{DateTime.UtcNow:yy}/{DateTime.UtcNow:MM}/"
            : $"{fileDirPath}";
        var keyName = prefix + fileName;
        if (changeFileName)
        {
            keyName = prefix + GenerateFileName(fileName, extension);
        }


        fileStream.Position = 0;

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = keyName,
            InputStream = fileStream,
            ContentType = contentType,
        };
        // AWS recommends bucket policies over ACLs
        // if (isPublic) request.CannedACL = S3CannedACL.PublicRead;
        try
        {
            var response = await _client.PutObjectAsync(request);
            if (response.HttpStatusCode != HttpStatusCode.OK) return (false, "");

            _logger.LogInformation("[Party: {PartyId}] Uploaded file {FileName} to {BucketName} with key {KeyName}",
                partyId, fileName, bucketName, keyName);
        }
        catch (AmazonS3Exception e)
        {
            _logger.LogError(e, "Error encountered on server. Message:'{Message}' when writing an object", e.Message);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unknown encountered on server. Message:'{Message}' when writing an object", e.Message);
            throw;
        }

        return (true, keyName);
    }

    public async Task<MediumStream> GetObjectAsync(long id)
    {
        var item = await _tenantDbContext.Media
            .Where(x => x.Id.Equals(id)).FirstOrDefaultAsync();
        // return item == null ? MediumStream.Empty() : MediumStream.Create(item, await GetObject(item.Url));
        if (item == null) return MediumStream.Empty();
        var obj = await TryGetObject(item.Url);
        return obj == null ? MediumStream.Empty() : MediumStream.Create(item, obj);
    }

    public async Task<MediumStream> GetObjectByGuidAsync(string guid)
    {
        var item = await _tenantDbContext.Media
            .Where(x => x.Guid.Equals(guid))
            .FirstOrDefaultAsync();
        if (item == null) return MediumStream.Empty();
        var obj = await TryGetObject(item.Url);
        return obj == null ? MediumStream.Empty() : MediumStream.Create(item, obj);
    }

    public async Task<Stream?> GetObjectByFilenameAsync(string filename)
    {
        return await TryGetObject(filename);
    }

    public async Task<MediumStream> GetObjectByGuidAndPartyIdAsync(long partyId, string type, string guid)
    {
        var item = await _tenantDbContext.Media
            .Where(x => type == "public" || x.PartyId.Equals(partyId))
            .Where(x => x.Guid.Equals(guid))
            .Where(x => x.DeletedOn == null)
            .FirstOrDefaultAsync();
        if (item == null) return MediumStream.Empty();
        var obj = await TryGetObject(item.Url);
        return obj == null ? MediumStream.Empty() : MediumStream.Create(item, obj);
    }

    public async Task<MediumStream> GetObjectByGuidAndPartyIdAsync(long partyId, string guid)
    {
        var item = await _tenantDbContext.Media
            .Where(x => x.Type == "public" || x.PartyId.Equals(partyId))
            .Where(x => x.Guid.Equals(guid))
            .Where(x => x.DeletedOn == null)
            .FirstOrDefaultAsync();
        if (item == null) return MediumStream.Empty();
        var obj = await TryGetObject(item.Url);
        return obj == null ? MediumStream.Empty() : MediumStream.Create(item, obj);
    }

    public async Task<bool> MarkAsDeletedByGuidAndPartyIdAsync(long partyId, string guid)
    {
        var item = await _tenantDbContext.Media
            .Where(x => x.PartyId.Equals(partyId))
            .Where(x => x.Guid.Equals(guid))
            .Where(x => x.DeletedOn == null)
            .FirstOrDefaultAsync();
        if (item == null) return false;
        item.DeletedOn = DateTime.UtcNow;
        await _tenantDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAsDeletedAsync(string guid)
    {
        var item = await _tenantDbContext.Media
            .Where(x => x.Guid.Equals(guid))
            .Where(x => x.DeletedOn == null)
            .FirstOrDefaultAsync();
        if (item == null) return false;
        item.DeletedOn = DateTime.UtcNow;
        await _tenantDbContext.SaveChangesAsync();
        return true;
    }

    private async Task<Stream?> TryGetObject(string url)
    {
        var key = GetObjectKeyFromUrl(url);
        try
        {
            var memoryStream = new MemoryStream();
            var response =
                await _client.GetObjectAsync(_options.BucketName, key);
            if (response.HttpStatusCode != HttpStatusCode.OK || response.ResponseStream == null)
            {
                _logger.LogWarning("Get S3 object error for key {Key}", key);
                return null;
            }

            await using (var responseStream = response.ResponseStream)
            {
                await responseStream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (AmazonS3Exception e)
        {
            if (e.Message == "The specified key does not exist.")
            {
                _logger.LogInformation("The specified key does not exist. Url: {Url}, Key: {Key}", url, key);
                return null;
            }

            _logger.LogWarning(e,
                "Error encountered on server. Message:'{Message}' when getting an object for key {Key}"
                , e.Message, key);
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Unknown encountered on server. Message:'{Message}' when getting an object for key {Key}",
                e.Message, key);
            return null;
        }
    }

    private static string GenerateFileName(string fileName, string extension)
        => (Path.GetFileNameWithoutExtension(fileName)
            + "_"
            + Guid.NewGuid().ToString().Split("-").First().ToLower()
            + extension).ToLower();

    private string GetObjectKeyFromUrl(string url)
        //url decode
        => WebUtility.UrlDecode(url.Replace($"https://{_options.BucketName}.s3.amazonaws.com/", ""));

    private string GetChunkKey(string fileId, long chunkIndex)
        => $"upload:chunk:{fileId}:{chunkIndex}";
}