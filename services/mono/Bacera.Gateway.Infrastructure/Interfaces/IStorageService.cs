namespace Bacera.Gateway;

public interface IStorageService
{
    Task<Medium> UploadFileAndSaveMediaAsync(Stream fileStream,
        string fileName = "",
        string extension = "",
        // bool isPublic = false,
        string type = "unknown",
        long rowId = 0,
        string contentType = "",
        long tenantId = 0,
        long partyId = 0,
        bool saveMedia = true
    );

    Task<Medium> UploadPublicFileAndSaveMediaAsync(Stream fileStream,
        string fileName = "",
        string extension = "",
        string type = "unknown",
        long rowId = 0,
        string contentType = "",
        long tenantId = 0,
        long partyId = 0,
        bool saveMedia = true
    );


    /// <returns>(result: bool, keyName: string, file full path in S3)</returns>
    Task<(bool, string)> UploadFileAsync(Stream fileStream,
        string fileDirPath = "",
        string fileName = "",
        string extension = "",
        string contentType = "",
        long tenantId = 0,
        long partyId = 0,
        bool changeFileName = true
    );

    Task<(bool, string)> UploadPublicFileAsync(Stream fileStream,
        string fileDirPath = "",
        string fileName = "",
        string extension = "",
        string contentType = "",
        long tenantId = 0,
        long partyId = 0,
        bool changeFileName = true
    );

    Task<MediumStream> GetObjectAsync(long id);
    Task<MediumStream> GetObjectByGuidAsync(string guid);
    Task<Stream?> GetObjectByFilenameAsync(string filename);
    Task<MediumStream> GetObjectByGuidAndPartyIdAsync(long partyId, string type, string guid);
    Task<MediumStream> GetObjectByGuidAndPartyIdAsync(long partyId, string guid);
    Task<bool> MarkAsDeletedByGuidAndPartyIdAsync(long partyId, string guid);
    Task<bool> MarkAsDeletedAsync(string guid);
}