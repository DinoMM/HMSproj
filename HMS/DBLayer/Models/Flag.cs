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
        [Required(ErrorMessage = "Povinné pole.")]
        public string ID { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public double? NumericValue { get; set; } = null;
        public string? StringValue { get; set; } = null;
        public DateTime? DateValue { get; set; } = null;

        public virtual object Clone()
        {
            return new Flag
            {
                ID = this.ID,
                NumericValue = this.NumericValue,
                StringValue = this.StringValue,
                DateValue = this.DateValue
            };
        }

        public virtual Flag Clon()
        {
            return (Flag)Clone();
        }

        public virtual void SetFromOther(Flag other)
        {
            this.ID = other.ID;
            this.NumericValue = other.NumericValue;
            this.StringValue = other.StringValue;
            this.DateValue = other.DateValue;
        }



    }
}
