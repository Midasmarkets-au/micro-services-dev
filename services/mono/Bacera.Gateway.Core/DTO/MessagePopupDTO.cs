using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bacera.Gateway;

public class MessagePopupDTO
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    // should be in (info, success, warning, error)
    public string Level { get; set; } = "info";

    public bool IsLevelValid() => Level is "info" or "success" or "warning" or "error";

    public static MessagePopupDTO BuildInfo(string title, string text) => Build(title, text, "info");
    public static MessagePopupDTO BuildSuccess(string title, string text) => Build(title, text, "success");
    public static MessagePopupDTO BuildWarning(string title, string text) => Build(title, text, "warning");
    public static MessagePopupDTO BuildError(string title, string text) => Build(title, text, "error");

    private static MessagePopupDTO Build(string title, string text, string level)
        => new()
        {
            Title = title,
            Text = text,
            Level = level,
        };

    public EventNotice ToEventNotice() => EventNotice.Build("__MESSAGE_POPUP__", 0, message: ToJson());

    public string ToJson() => Utils.JsonSerializeObject(this);
    public static MessagePopupDTO Parse(string json) => Utils.JsonDeserializeObjectWithDefault<MessagePopupDTO>(json);
}

public class CreateMessagePopupSpec
{
    public long TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Level { get; set; } = "info";
    public string Role { get; set; } = UserRoleTypesString.Dealing;
    public long PartyId { get; set; }
    
    

    public bool IsValid()
    {
        if (UserRoleTypesExtensions.GetAllRoleString().All(x => !x.Equals(Role, StringComparison.CurrentCultureIgnoreCase)))
            return false;

        if (Level is not "info" and not "success" and not "warning" and not "error")
            return false;

        return true;
    }

    public MessagePopupDTO ToMessagePopupDTO() => new()
    {
        Title = Title,
        Text = Text,
        Level = Level,
    };
}