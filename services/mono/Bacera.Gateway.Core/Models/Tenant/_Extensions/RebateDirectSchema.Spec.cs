namespace Bacera.Gateway;

partial class RebateDirectSchema
{
    public class CreateSpec
    {
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public List<RebateDirectSchemaItem.CreateSpec> Items { get; set; } = null!;
    }

    public class UpdateSpec
    {
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }

    public class ResponseModel
    {
        public long Id { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByPartyName { get; set; } = string.Empty;

        public DateTime? ConfirmedOn { get; set; }
        public long? ConfirmedBy { get; set; }
        public string ConfirmedByPartyName { get; set; } = string.Empty;
    }
}