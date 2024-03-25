using DBLayer.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class Sklad
    {
        [Key]
        public string ID { get; set; } = default!;
        public string Nazov { get; set; } = default!;
        public DateTime Obdobie { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); //vrati vzdy prveho dany mesiac


        /// <summary>
        /// povolene role, aj keby bol uzivatel pripojeny ku skladu tak musi mat prislusnu rolu inak len na zobrazenie
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> POVOLENEROLE { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.Nakupca, RolesOwn.Skladnik, };
        /// <summary>
        /// povolene role pre pridavanie a mazanie poloziek, zobrazovanie celeho skladu poloziek
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ZMENAPOLOZIEKROLE { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Nakupca };
        /// <summary>
        /// povolenie pre prijem,vydaj,uzavretie skladu, nakupca by sa nemal starat o tieto veci
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> SKLADOVEPOHYBYROLE { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Skladnik };

        public DateTime GetActual()
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        public string ShortformObdobie()
        {
            return Obdobie.ToString("MMyy");
        }
        public static string ShortFromObdobie(DateTime date)
        {
            return date.ToString("MMyy");
        }
        public static DateTime DateFromShortForm(string miniObd)
        {
            DateTime date = DateTime.ParseExact(miniObd, "MMyy", System.Globalization.CultureInfo.InvariantCulture);
            date = new DateTime(date.Year, date.Month, 1);
            return date;
        }

        public void NextObdobie()
        {
            Obdobie.AddMonths(1);
        }

        public static List<string> MoznoVydať(IEnumerable<PrijemkaPolozka> zoznamPoloziek, Sklad skld, in DBContext db)
        {
            var listNemozno = new List<string>();
            foreach (var item in zoznamPoloziek)       //pridanie poloziek do skladu
            {
                var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.PolozkaSkladu && x.Sklad == skld.ID);
                if (found != null)
                {
                    if (found.Mnozstvo - item.Mnozstvo < 0)
                    {
                        listNemozno.Add(item.PolozkaSkladu);
                    }
                }
                else
                {
                    Debug.WriteLine("Chybna polozka v zozname pri spracovani vydajky");
                }
            }
            return listNemozno;
        }
        public static List<string> MoznoDodat(IEnumerable<PrijemkaPolozka> zoznamPoloziek, Sklad skladdo, in DBContext db)
        {
            var listNemozno = new List<string>();
            foreach (var item in zoznamPoloziek)       //pridanie poloziek do skladu
            {
                var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.PolozkaSkladu && x.Sklad == skladdo.ID);
                if (found == null)
                {
                    listNemozno.Add(item.PolozkaSkladu);
                }
            }
            return listNemozno;
        }

        

    }
}