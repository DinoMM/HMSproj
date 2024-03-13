using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class PolozkaSkladuObjednavky : ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]       //pridava autoincrement
        public long ID { get; set; }
        [ForeignKey("ObjednavkaX")]
        public string Objednavka { get; set; }
        public Objednavka ObjednavkaX { get; set; }
        [ForeignKey("PolozkaSkladuX")]
        public string PolozkaSkladu { get; set; }
        public PolozkaSkladu PolozkaSkladuX { get; set; }
        public string? Nazov { get; set; }      //mozno zmenit nazov ak treba
        public double Mnozstvo { get; set; }
        public double Cena { get; set; }

        [NotMapped]
        public double CelkovaCena { get => (double)Mnozstvo * Cena; }
        [NotMapped]
        public double CenaDPH { get => (Cena * 120) / 100; }
        [NotMapped]
        public double CelkovaCenaDPH { get => (double)Mnozstvo * CenaDPH; }
        
        public object Clone()       //deep copy
        {
            PolozkaSkladuObjednavky clone = new PolozkaSkladuObjednavky();
            clone.ID = ID;
            clone.Objednavka = Objednavka;
            clone.ObjednavkaX = ObjednavkaX;
            clone.PolozkaSkladu = PolozkaSkladu;
            clone.PolozkaSkladuX = PolozkaSkladuX;
            clone.Nazov = Nazov;
            clone.Mnozstvo = Mnozstvo;
            clone.Cena = Cena;

            return clone;
        }
        public PolozkaSkladuObjednavky Clon()
        {
            return (PolozkaSkladuObjednavky)Clone();
        }

        public void SetZPolozkySkladu(PolozkaSkladu polozka)
        {
            PolozkaSkladuX = polozka;
            PolozkaSkladu = polozka.ID;
            Nazov = polozka.Nazov;
            Cena = polozka.Cena;
        }

    }
}
