using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bacera.Gateway;

partial class Configuration : IEntity
{
    public class CreateSpec
    {
        public string Key { get; set; } = null!;

        // public string Value { get; set; } = null!;
        public object Value { get; set; } = new();
        public string Name { get; set; } = null!;
        public string DataFormat { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
    }

    public static class CategoryTypes
    {
        public static string Public = "public";
        public static string Account = "account";
        public static string Party = "party";
    }


    private static object? ParseJson(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject(json);
        }
        catch
        {
            return null; // or handle the error as needed
        }
    }

    private static bool ValidateJson(string strInput)
    {
        if (string.IsNullOrWhiteSpace(strInput))
        {
            return false;
        }

        strInput = strInput.Trim();
        if ((!strInput.StartsWith("{") || !strInput.EndsWith("}")) && //For object
            (!strInput.StartsWith("[") || !strInput.EndsWith("]"))) return false; //For array
        try
        {
            JsonConvert.DeserializeObject(strInput);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

