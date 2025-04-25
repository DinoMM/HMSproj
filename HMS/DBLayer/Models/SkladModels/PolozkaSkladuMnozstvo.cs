using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    [Index(nameof(Active), Name = "INDX_PolozkaSkladuMnozstva_Active")]
    public partial class PolozkaSkladuMnozstvo : ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        [ForeignKey("PolozkaSkladuX")]
        public string PolozkaSkladu { get; set; } = default!;
        public PolozkaSkladu PolozkaSkladuX { get; set; }

        [ForeignKey("SkladX")]
        public string Sklad { get; set; } = default!;
        public Sklad SkladX { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Len kladné hodnoty")]
        public double Mnozstvo { get; set; } = 0.0;

        [Range(0.0, double.MaxValue, ErrorMessage = "Len kladné hodnoty")]
        [Column(TypeName = "decimal(18, 3)")]
        public double Cena { get; set; } = 0;

        /// <summary>
        /// Urcuje, ci sa ma polozka zobrazovat pri vyhladavani
        /// </summary>
        public bool Active { get; set; } = true;

        public object Clone()
        {
            return new PolozkaSkladuMnozstvo { 
             ID = this.ID,
             PolozkaSkladu = this.PolozkaSkladu,
             PolozkaSkladuX = this.PolozkaSkladuX,
             Sklad = this.Sklad,
             SkladX = this.SkladX,
             Mnozstvo = this.Mnozstvo,
             Cena = this.Cena,
             Active = this.Active
            };
        }
        public PolozkaSkladuMnozstvo Clon()
        {
            return (PolozkaSkladuMnozstvo)Clone();
        }
    }
}