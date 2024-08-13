using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class Vydajka : PohSkup
    {
        [ForeignKey("SkladX")]
        public string Sklad { get; set; } = default!;
        public Sklad SkladX { get; set; }

        [ForeignKey("SkladDoX")]
        public string? SkladDo { get; set; } = default!;
        public Sklad? SkladDoX { get; set; }
        public DateTime Obdobie { get; set; } = SkladObdobie.GetSeassonFromToday();

        public static List<PolozkaSkladu> ZosumarizujPolozkyVydajky(Vydajka vydajka, in DBContext db)
        {
            var polozkyZItem = db.VydajkyPolozky.Where(x => x.Skupina == vydajka.ID).ToList(); //ziska vsetky polozky z vydajky

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
