using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bacera.Gateway.Core.Models.Tenant
{
    [Table("contract_specs", Schema = "cms")]
    public sealed class ContractSpec
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("site")]
        [MaxLength(255)]
        public string Site { get; set; } = string.Empty;

        [Column("category")]
        [MaxLength(255)]
        public string Category { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(255)]
        public string? Description { get; set; }

        [Column("symbol")]
        [MaxLength(255)]
        public string Symbol { get; set; } = string.Empty;

        [Column("contract_size")]
        public int? ContractSize { get; set; }

        [Column("contract_unit")]
        [MaxLength(255)]
        public string? ContractUnit { get; set; }

        [Column("trading_start_time")]
        public TimeSpan? TradingStartTime { get; set; }

        [Column("trading_end_time")]
        public TimeSpan? TradingEndTime { get; set; }

        [Column("trading_start_weekday")]
        [MaxLength(255)]
        public string? TradingStartWeekday { get; set; }

        [Column("trading_end_weekday")]
        [MaxLength(255)]
        public string? TradingEndWeekday { get; set; }

        [Column("break_start_time")]
        public TimeSpan? BreakStartTime { get; set; }

        [Column("break_end_time")]
        public TimeSpan? BreakEndTime { get; set; }

        [Column("more_break_start_time")]
        public TimeSpan? MoreBreakStartTime { get; set; }

        [Column("more_break_end_time")]
        public TimeSpan? MoreBreakEndTime { get; set; }

        [Column("margin_requirements", TypeName = "json")]
        public string? MarginRequirements { get; set; }

        [Column("commission")]
        public int? Commission { get; set; }

        [Column("rollover_time")]
        public TimeSpan? RolloverTime { get; set; }

        [Column("comment")]
        [MaxLength(255)]
        public string? Comment { get; set; }

        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        [Column("operator_info", TypeName = "json")]
        public string? OperatorInfo { get; set; }

        [Column("description_langs", TypeName = "json")]
        public string? DescriptionLangs { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}


