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
        public double Mnozstvo { get; set; }
        public double Cena { get; set; }
        [NotMapped]
        public double CelkovaCena { get => (double)Mnozstvo * Cena; }
        [NotMapped]
        public double CenaDPH { get => (Cena * 120) / 100; }
        [NotMapped]
        public double CelkovaCenaDPH { get => (double)Mnozstvo * CenaDPH; }


        //Pridane


        [ForeignKey("PolozkaSkladuMnozstvaX")]
        public long PolozkaSkladuMnozstva { get; set; }
        public PolozkaSkladuMnozstvo PolozkaSkladuMnozstvaX { get; set; }

        public void SetZPolozSkladuMnozstva(PolozkaSkladuMnozstvo polozka)
        {
            PolozkaSkladuMnozstva = polozka.ID;
            PolozkaSkladuMnozstvaX = polozka;

            Nazov = polozka.PolozkaSkladuX.Nazov;
            Mnozstvo = 0;
            Cena = polozka.PolozkaSkladuX.Cena;
        }
    }
}
