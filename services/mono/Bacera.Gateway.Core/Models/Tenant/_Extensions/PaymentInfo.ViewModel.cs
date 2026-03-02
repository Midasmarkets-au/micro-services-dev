namespace Bacera.Gateway;

using M = PaymentInfo;

partial class PaymentInfo
{
    public sealed class ClientPageModel
    {
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]

        public long Id { get; set; }

        public string HashId => HashEncode(Id);
        public PaymentPlatformTypes Platform { get; set; }

        public string Name { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string InfoJson { get; set; } = string.Empty;

        public dynamic Info => Utils.JsonDeserializeDynamic(InfoJson);

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}

public static class PaymentInfoViewModelExt
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> source) => source.Select(x => new M.ClientPageModel
    {
        Id = x.Id,
        Platform = (PaymentPlatformTypes)x.PaymentPlatform,
        Name = x.Name,
        InfoJson = x.Info,
        CreatedOn = x.CreatedOn,
        UpdatedOn = x.UpdatedOn
    });
}