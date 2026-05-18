namespace Bacera.Gateway;

public partial class AccountPaymentMethodAccess
{
    public sealed class ExtraInfoModel
    {
        public bool IsDisplay { get; set; }
    }

    public ExtraInfoModel GetExtraInfo() =>
        Utils.JsonDeserializeObjectWithDefault<ExtraInfoModel>(ExtraInfo);

    public void SetExtraInfo(ExtraInfoModel model) =>
        ExtraInfo = Utils.JsonSerializeObject(model);

    public bool IsDisplay => GetExtraInfo().IsDisplay;
}
