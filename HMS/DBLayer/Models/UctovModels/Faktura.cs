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
    public partial class Faktura
    {
        [Key]
        public string ID { get; set; } = default!;
        [ForeignKey("SkupinaX")]
        public string Skupina { get; set; } = default!;
        public PohSkup SkupinaX { get; set; }
        public DateTime Vystavenie { get; set; } = DateTime.Today;
        public DateTime Splatnost { get; set; } = DateTime.Today.AddDays(15);
        [ForeignKey("OdKohoX")]
        public string OdKoho { get; set; } = default!;
        public Dodavatel OdKohoX { get; set; }
        public bool Spracovana { get; set; } = false;
        [Column(TypeName = "decimal(18, 3)")]
        public double CenaBezDPH { get; set; } = default!;
        [NotMapped]
        public double CenaDPH { get => (CenaBezDPH * 120) / 100; }
        


    }
}
