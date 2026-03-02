using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public class HostingBackgroundJob
{
    public string Name { get; set; } = null!;
    [Required] public string Command { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime LastExecuteOn { get; set; }
    public BackgroundJobStatusTypes Status { get; set; }
    public string Value { get; set; } = string.Empty;

    public static List<HostingBackgroundJob>? ToHostingBackgroundJobList(string itemString)
    {
        List<HostingBackgroundJob>? result;
        try
        {
            result = JsonConvert.DeserializeObject<List<HostingBackgroundJob>>(itemString);
        }
        catch
        {
            result = new List<HostingBackgroundJob>();
        }

        return result;
    }
}