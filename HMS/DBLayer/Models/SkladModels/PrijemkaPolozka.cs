using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class PrijemkaPolozka : PohJednotka
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        [ForeignKey("SkupinaX")]
        public string Skupina { get; set; } = default!;
        public PohSkup SkupinaX { get; set; }
        public string? Nazov { get; set; } = default!;
        [Column(TypeName = "decimal(18, 4)")]
        public double Mnozstvo { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public double Cena { get; set; }
        [NotMapped]
        public double CelkovaCena { get => (double)Mnozstvo * Cena; }
        [NotMapped]
        public double CenaDPH { get => (Cena * (double)(100 + DPH)) / 100; }
        [NotMapped]
        public double CelkovaCenaDPH { get => (double)Mnozstvo * CenaDPH; }
        [NotMapped]
        public decimal DPH { get; set; } = 20;


        //Pridane


        [ForeignKey("PolozkaSkladuX")]
        public string PolozkaSkladu { get; set; }
        public PolozkaSkladu PolozkaSkladuX { get; set; }

        public void SetZPolozSkladuMnozstva(PolozkaSkladu polozka)
        {
            PolozkaSkladu = polozka.ID;
            PolozkaSkladuX = polozka;

            Nazov = polozka.Nazov;
            Mnozstvo = polozka.Mnozstvo;
            Cena = polozka.Cena;
            DPH = polozka.DPH;
        }

        public object Clone() {
            var novy = new PrijemkaPolozka();
            novy.ID = ID;
            novy.Cena = Cena;
            novy.Mnozstvo = Mnozstvo;
            novy.Nazov = Nazov;
            novy.PolozkaSkladu = PolozkaSkladu;
            novy.Skupina = Skupina;
            novy.DPH = DPH;

            novy.SkupinaX = SkupinaX;
            novy.PolozkaSkladuX = PolozkaSkladuX;
            return novy;
        }
        public PrijemkaPolozka Clon() {
            return (PrijemkaPolozka)Clone();
        }
    }
}
