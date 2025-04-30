using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Dodavatel : ICloneable
    {
        [Key]
        public string ICO { get; set; } = default!;
        public string Nazov { get; set; } = default!;
        public string Obec { get; set; } = default!;
        public string Adresa { get; set; } = default!;
        public string Iban { get; set; } = default!;
        public string DIC { get; set; } = default!;
        public string IC_DPH { get; set; } = default!;
        public string Poznámka { get; set; } = default!;

        public object Clone()
        {
            return new Dodavatel
            {
                ICO = this.ICO,
                Nazov = this.Nazov,
                Obec = this.Obec,
                Adresa = this.Adresa,
                Iban = this.Iban,
                DIC = this.DIC,
                IC_DPH = this.IC_DPH,
                Poznámka = this.Poznámka
            };
        }

        public Dodavatel Clon()
        {
            return (Dodavatel)this.Clone();
        }
    }
}
