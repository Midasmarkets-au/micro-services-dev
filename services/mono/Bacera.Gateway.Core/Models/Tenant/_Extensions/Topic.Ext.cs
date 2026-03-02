using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

partial class Topic
{
    public Topic()
    {
        AdditionalInformation = string.Empty;
    }
    
    private static readonly Hashids Hashids = new(HashIdSaltTypes.Wallet, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.Wallet]);
    public static string HashEncode(long id) => Hashids.EncodeLong(id);

    public sealed class CreateSpec
    {
        [MinLength(3)] public string Title { get; set; } = null!;
        public TopicTypes Type { get; set; }

        [RegularExpression(LanguageTypes.AllLanguageRegEx)]
        [Required]
        public string Language { get; set; } = null!;

        [MinLength(3)] public string Content { get; set; } = null!;
        public string Author { get; set; } = string.Empty;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public sealed class UpdateSpec
    {
        [MinLength(3)] public string? Title { get; set; }
        public TopicTypes Type { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public sealed class ResponseModel
    {
        public int Id { get; set; }
        public short Type { get; set; }

        public string Title { get; set; } = null!;

        public DateTime EffectiveFrom { get; set; }

        public DateTime EffectiveTo { get; set; }


        public Dictionary<string, TopicContent.ResponseModel> Contents { get; set; } = new();

        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsEmpty() => Id == 0;
    }
}

public static class TopicExtension
{
    public static Topic Build(this Topic.CreateSpec me)
    {
        return new Topic
        {
            Title = me.Title,
            Type = (short)me.Type,
            EffectiveFrom = me.EffectiveFrom ?? DateTime.MinValue,
            EffectiveTo = me.EffectiveTo ?? DateTime.MinValue,
            TopicContents =
            {
                new TopicContent
                {
                    Language = me.Language,
                    Title = me.Title,
                    Author = me.Author,
                    Content = me.Content
                }
            }
        };
    }

    public static IQueryable<Topic.ResponseModel> ToResponseModels(this IQueryable<Topic> query,
        string? language = null)
        => query.Select(x => new Topic.ResponseModel
        {
            Id = x.Id,
            Type = x.Type,
            Title = x.Title,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            Contents = language == null
                ? x.TopicContents
                    .ToLanguageDictionary()
                : x.TopicContents
                    .Where(l => l.Language.ToUpper().Equals(language.ToUpper()))
                    .ToLanguageDictionary(),
            UpdatedOn = x.UpdatedOn,
            CreatedOn = x.CreatedOn
        });
}