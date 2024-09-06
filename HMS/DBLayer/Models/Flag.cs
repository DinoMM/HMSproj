using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class Flag
    {
        [Key]
        public string ID { get; set; }
        public double? NumericValue { get; set; } = null;
        public string? StringValue { get; set; } = null;
        public DateTime? DateValue { get; set; } = null;
    }
}
