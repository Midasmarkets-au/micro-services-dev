using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

public partial class SalesRebateSchema
{
    public sealed class CreateSpec
    {
        [Required] public SalesRebateSchemaSalesTypes SalesType { get; set; }
        [Required] public SalesRebateSchemaScheduleTypes Schedule { get; set; }
        [Required] public long SalesAccountUid { get; set; }
        [Required] public long RebateAccountUid { get; set; }
        [Required] public int Rebate { get; set; }
        public int AlphaRebate { get; set; }
        public int ProRebate { get; set; }
        public string ExcludeAccount { get; set; } = string.Empty;
        public string ExcludeSymbol { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
    
    public sealed class UpdateSpec
    {
        public SalesRebateSchemaSalesTypes SalesType { get; set; }
        public SalesRebateSchemaScheduleTypes Schedule { get; set; }
        public int Rebate { get; set; }
        public int AlphaRebate { get; set; }
        public int ProRebate { get; set; }
        public string ExcludeAccount { get; set; } = string.Empty;
        public string ExcludeSymbol { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}