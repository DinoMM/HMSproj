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
        public DateTime Obdobie { get; set; } = SkladObdobie.GetSeassonFromToday();

        /// <summary>
        /// povolenie pre mazanie prijemky
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> TOTAL_MAZANIE_PRIJEMOK { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Uctovnik };

        public static void PrijatNaSklad(IEnumerable<PrijemkaPolozka> polozky, Sklad skladDo, in DBContext db)
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

        public static List<PolozkaSkladu> ZosumarizujPolozkyPrijemky(Prijemka prijemka, in DBContext db)
        {
            var polozkyZItem = db.PrijemkyPolozky.Where(x => x.Skupina == prijemka.ID).ToList(); //ziska vsetky polozky z prijemky

            return polozkyZItem.GroupBy(x => x.PolozkaSkladu) //spoji duplikaty do unikatneho listu
            .Select(group => new PolozkaSkladu()
            {
                ID = group.Key,
                Mnozstvo = group.Sum(x => x.Mnozstvo)
            })
            .ToList();
        }


    }
}