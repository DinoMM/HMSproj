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

        /// <summary>
        /// true - hotovosť, false - platobná karta
        /// </summary>
        [BooleanStringValues("Platobná karta", "Hotovosť")]
        public bool TypPlatby { get; set; } = false;

        [ForeignKey("HostX")]
        public long? Host { get; set; } = default!;
        public Host? HostX { get; set; }

        [NotMapped]
        public string GetMenoHosta { get => HostX != null ? HostX.Name + " " + HostX.Surname : "-"; }
        [NotMapped]
        public string GetMenoHostaBezPoml { get => HostX != null ? HostX.Name + " " + HostX.Surname : ""; }


        public string GetTypPlatby()
        {
            return TypPlatby ? "Hotovosť" : "Platobná karta";
        }

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
                TypPlatby = this.TypPlatby,
                Host = this.Host,
                HostX = this.HostX
            };
        }
        public PokladnicnyDoklad Clon()
        {
            return (PokladnicnyDoklad)this.Clone();
        }

        public override string GetDisplayName()
        {
            return "Poklad. doklad";
        }
    }
}
