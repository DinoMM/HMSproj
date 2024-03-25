using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class Prijemka : PohSkup
    {
        public string? Objednavka { get; set; } = default!;
        public string? DodaciID { get; set; } = default!;
        public string? FakturaID { get; set; } = default!;
        [ForeignKey("SkladX")]
        public string Sklad { get; set; } = default!;
        public Sklad SkladX { get; set; }


        public static void PrijatNaSklad(IEnumerable<PrijemkaPolozka> polozky, Sklad skladDo, DBContext db)
        {
            foreach (var item in polozky)
            {
                var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.PolozkaSkladu && x.Sklad == skladDo.ID);
                if (found != null)
                {
                    found.Mnozstvo += item.Mnozstvo;
                }
            }
            db.SaveChanges();
        }
    }
}