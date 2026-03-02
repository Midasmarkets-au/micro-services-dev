using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

public class DebugEmailRequest
{
    [Required] public string To { get; set; } = null!;
    [Required] public string Title { get; set; } = null!;
    public string Language { get; set; } = LanguageTypes.English;
    public object Model { get; set; } = new();

    public static DebugEmailRequest Build() => new() { To = "", Title = "" };
}