using System.Text.Json.Serialization;
using Bacera.Gateway.Core.Types;
using HashidsNet;
using Microsoft.AspNetCore.Http;

namespace Bacera.Gateway;

using M = Medium;

public partial class Medium
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.Wallet, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.Wallet]);

    public string HashId => HashEncode(Id);

    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();

    public ClientPageModel ToClientPageModel()
        => new()
        {
            Id = Id,
            Guid = Guid,
            Type = Type,
            FileName = FileName,
            ContentType = ContentType,
            CreatedOn = CreatedOn
        };
    
    public class ResponseModel
    {
        public long Id { get; set; }
        public string Guid { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
    }

    public sealed class ClientPageModel
    {
        [JsonIgnore] public long Id { get; set; }
        public string HashId => HashEncode(Id);
        public string Guid { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }

        public M ToModel() => new()
        {
            Id = HashDecode(HashId),
            Guid = Guid,
            Type = Type,
            FileName = FileName,
            ContentType = ContentType,
            CreatedOn = CreatedOn
        };
    }

    public sealed class ChunkInfo
    {
        public string? FieldId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long ChunkIndex { get; set; }
        public long ChunkSize { get; set; }
        public long TotalChunks { get; set; }
        public long TotalSize { get; set; }
        public IFormFile? File { get; set; }
    }
}

public static class MediumExtensions
{
    public static IQueryable<M.ResponseModel> ToResponseModel(this IQueryable<M> query)
        => query.Select(x => new M.ResponseModel
        {
            Id = x.Id,
            Guid = x.Guid,
            Url = x.Url,
            Type = x.Type,
            FileName = x.FileName,
            ContentType = x.ContentType,
            Context = x.Context
        });

    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            Guid = x.Guid,
            Type = x.Type,
            FileName = x.FileName,
            ContentType = x.ContentType,
            CreatedOn = x.CreatedOn
        });
}