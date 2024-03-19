using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class Dodavatel
    {
        [Key]
        public string ICO { get; set; } = default!;
        public string Nazov { get; set; } = default!;
        public string Obec { get; set; } = default!;
        public string Adresa { get; set; } = default!;
        public string Iban { get; set; } = default!;

    }
}
