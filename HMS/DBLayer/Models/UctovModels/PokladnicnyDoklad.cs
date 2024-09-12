using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class PokladnicnyDoklad : PohSkup, ICloneable
    {
        [ForeignKey("KasaX")]
        public string? Kasa { get; set; } = default!;
        public Kasa? KasaX { get; set; }

        [BooleanStringValues("Platobná karta", "Hotovosť")]
        public bool TypPlatby { get; set; } = false;

        public object Clone()
        {
            return new PokladnicnyDoklad
            {
                ID = this.ID,
                Vznik = this.Vznik,
                Poznamka = this.Poznamka,
                Spracovana = this.Spracovana,
                Kasa = this.Kasa,
                KasaX = this.KasaX,
                TypPlatby = this.TypPlatby
            };
        }
        public PokladnicnyDoklad Clon()
        {
            return (PokladnicnyDoklad)this.Clone();
        }
    }
}
