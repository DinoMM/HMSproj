using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class PolozkaSkladuMnozstvo
    {
        /// <summary>
        /// Nastaví množstvá pre aktuálne obdobie položky skladu v priloženom zozname.
        /// </summary>
        /// <param name="zoznamPoloziek"></param>
        /// <param name="sklad"></param>
        /// <param name="db"></param>
        public static void NastavZaciatocMnozstva(IEnumerable<PolozkaSkladu> zoznamPoloziek, Sklad sklad, in DBContext db)
        {
            foreach (var item in zoznamPoloziek)
            {
                var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.ID && x.Sklad == sklad.ID);
                if (found != null)
                {
                    item.Mnozstvo = found.Mnozstvo;
                    item.Cena = found.Cena;
                }
            }
        }
    }
}