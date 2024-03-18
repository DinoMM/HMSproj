using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class PolozkaSkladu : ICloneable
    {
        [Key]
        public string ID { get; set; } = default!;
        [Required]
        [StringLength(128,MinimumLength = 3,ErrorMessage = "Nazov musi byť v rozmedzi 3 - 128 znakov" )]
        public string Nazov { get; set; } = default!;
        [Required]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Merná jednotka musi byť v rozmedzi 1 - 32 znakov")]
        public string MernaJednotka { get; set; } = default!;
        [NotMapped]
        public double Mnozstvo { get; set; } = 0;
        [Range(0.0, double.MaxValue, ErrorMessage = "Len kladné hodnoty")]
        public double Cena { get; set; } = 0;

        [NotMapped]
        public double CelkovaCena { get => (double)Mnozstvo * Cena; }
        [NotMapped]
        public double CenaDPH { get => (Cena * 120) / 100;  }
        [NotMapped]
        public double CelkovaCenaDPH { get => (double)Mnozstvo * CenaDPH; }

        public object Clone()       //deep copy
        {
            PolozkaSkladu clone = new PolozkaSkladu();
            clone.Nazov = Nazov;
            clone.ID = ID;
            clone.MernaJednotka = MernaJednotka;
            clone.Mnozstvo = Mnozstvo;
            clone.Cena = Cena;

            return clone;
        }
        public PolozkaSkladu Clon() {
            return (PolozkaSkladu)Clone();
        }

        public static string DajNoveID(DBContext db)
        {
            int adder = 1;
            string newID;
            do
            {
                int cislo;
                if (db.PolozkySkladu.Count() != 0)
                {
                    cislo = int.Parse(db.PolozkySkladu.Last().ID) + adder;
                }
                else
                {
                    cislo = 1;
                }
                //moznost pridat prefix
                newID = cislo.ToString("D7");
                ++adder;

            } while (db.PolozkySkladu.FirstOrDefault(d => d.ID == newID) != null);
            return newID;
        }
    }
}
