using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Prijemka : PohSkup
    {
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

            return PolozkaSkladu.ZosumarizujListPoloziek(in polozkyZItem);
        }
    }
}