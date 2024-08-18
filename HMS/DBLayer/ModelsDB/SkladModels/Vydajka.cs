using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Vydajka : PohSkup
    {
        public static List<PolozkaSkladu> ZosumarizujPolozkyVydajky(Vydajka vydajka, in DBContext db)
        {
            var polozkyZItem = db.VydajkyPolozky.Where(x => x.Skupina == vydajka.ID).ToList(); //ziska vsetky polozky z vydajky

            return PolozkaSkladu.ZosumarizujListPoloziek(in polozkyZItem);
        }
    }
}
