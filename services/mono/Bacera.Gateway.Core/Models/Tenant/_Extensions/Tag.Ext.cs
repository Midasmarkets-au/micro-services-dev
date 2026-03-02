namespace Bacera.Gateway;

public partial class Tag
{
    public bool IsEmpty() => Id == 0;

    public static Tag Build(string type, string name)
        => new() { Type = type, Name = name };

    public sealed class BasicModel
    {
        public long Id { get; set; }
        public string Type { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public sealed class TypeBasicModel
    {
        public string Name { get; set; } = null!;
        public bool Enabled { get; set; }
    }

    public sealed class QuerySpec
    {
        private string _type = null!;

        public string Type
        {
            get => _type.ToLower();
            set => _type = value;
        }

        public long RowId { get; set; }
    }

    public sealed class CreateOrUpdateSpec
    {
        private string _type = null!;

        public string Type
        {
            get => _type.ToLower();
            set => _type = value;
        }

        public string Name { get; set; } = null!;
        public long RowId { get; set; }
        public bool Enabled { get; set; }
    }
}

public static class TagExtension
{
    public static IQueryable<Tag.BasicModel> ToBasicModels(this IQueryable<Tag> queryable)
        => queryable.Select(x => new Tag.BasicModel
        {
            Id = x.Id,
            Type = x.Type,
            Name = x.Name
        });
}