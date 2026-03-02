using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class RebateSchemaBundle
{
    [NotMapped]
    public Dictionary<string, decimal> Items
    {
        get
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, decimal>>(Data)
                       ?? new Dictionary<string, decimal>();
            }
            catch (Exception)
            {
                return new Dictionary<string, decimal>();
            }
        }
        set => Data = JsonConvert.SerializeObject(value); // !! don't change capitalization
    }

    public class CreateSpec
    {
        public string Name { get; set; } = null!;
        public string? Note { get; set; }

        public RebateRuleTemplateBundleTypes Type { get; set; }
        public Dictionary<string, decimal> Items { get; set; } = null!;
    }
}