using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bacera.Gateway.Core.Models.Tenant
{
    [Table("documents", Schema = "cms" )]
    public partial class Document
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("title")]
        [MaxLength(255)]
        public string? Title { get; set; }

        [Column("site")]
        [MaxLength(255)]
        public string? Site { get; set; }

        [Column("link")]
        [MaxLength(255)]
        public string? Link { get; set; }

        [Column("languages", TypeName = "json")]
        public string? Languages { get; set; } // JSON string storing language package

        [Column("operator_info", TypeName = "json")]
        public string? OperatorInfo { get; set; } // JSON string storing update info

        [Column("comment")]
        [MaxLength(500)]
        public string? Comment { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property for related HistoricalDocuments
        public ICollection<HistoricalDocument> HistoricalDocuments { get; set; } = new List<HistoricalDocument>();
    }
}
