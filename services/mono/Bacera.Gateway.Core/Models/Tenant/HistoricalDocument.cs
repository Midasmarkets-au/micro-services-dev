using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bacera.Gateway.Core.Models.Tenant
{
    [Table("historical_documents", Schema = "cms")]
    public partial class HistoricalDocument
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("link")]
        [MaxLength(255)] // optional: PostgreSQL VARCHAR length not defined, can adjust
        public string? Link { get; set; }

        [Column("language")]
        [MaxLength(100)]
        public string? Language { get; set; }

        [Column("site")]
        [MaxLength(255)]
        public string? Site { get; set; }

        [Column("operator_info", TypeName = "json")]
        public string? OperatorInfo { get; set; } // store raw JSON string

        [Column("comment")]
        [MaxLength(500)]
        public string? Comment { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Document Document { get; set; } = null!;
    }
}
