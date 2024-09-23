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

        public object Clone()
        {
            return new Dodavatel
            {
                ICO = this.ICO,
                Nazov = this.Nazov,
                Obec = this.Obec,
                Adresa = this.Adresa,
                Iban = this.Iban
            };
        }

        public Dodavatel Clon()
        {
            return (Dodavatel)this.Clone();
        }
    }
}
