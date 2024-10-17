using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class ZmenaMeny
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        public DateTime Vznik { get; set; } = DateTime.Now;

        [Column(TypeName = "nvarchar(16)")]
        public string MenaZ { get; set; } = "";

        [Column(TypeName = "nvarchar(16)")]
        public string MenaDO { get; set; } = "EUR";

        [Column(TypeName = "decimal(18, 3)")]
        public decimal Kurz { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SumaZ { get; set; } = 0;


        [NotMapped]
        public decimal SumaDO => Math.Round(SumaZ * Kurz, 2);

    }
}
