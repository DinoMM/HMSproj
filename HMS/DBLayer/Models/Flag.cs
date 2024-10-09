using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class Flag
    {
        [Key]
        public string ID { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public double? NumericValue { get; set; } = null;
        public string? StringValue { get; set; } = null;
        public DateTime? DateValue { get; set; } = null;
    }
}
