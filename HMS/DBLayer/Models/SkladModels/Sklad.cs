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
using System.Transactions;

namespace DBLayer.Models
{
    public class Sklad
    {
        [Key]
        public string ID { get; set; } = default!;
        public string Nazov { get; set; } = default!;
        //public DateTime Obdobie { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); //vrati vzdy prveho dany mesiac


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

        //public string ShortformObdobie()
        //{
        //    return Obdobie.ToString("MMyy");
        //}
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

        /// <summary>
        /// Prejde zoznam prijímacích položiek a zistí, či je možné ich prijať.
        /// </summary>
        /// <param name="zoznamPoloziek">Zoznam prijímacích položiek.</param>
        /// <param name="skld">Sklad, z ktorého sa má vydať.</param>
        /// <param name="db">Databázový kontext.</param>
        /// <returns>Zoznam položiek, ktoré nie je možné vydať.</returns>
        public static List<string> MoznoVydať(IEnumerable<PrijemkaPolozka> zoznamPoloziek, Sklad skld, in DBContext db)
        {
            var listNemozno = new List<string>();

            #region nacitanie poloziek zo SCHVALENYCH prijemok
            List<PolozkaSkladu> celkoveMnozstvaZPrijemok = GetPoctyZPrijemok(skld, in db);
            #endregion
            #region nacitanie poloziek zo SCHVALENYCH vydajok
            List<PolozkaSkladu> celkoveMnozstvaZVydajok = GetPoctyZVydajok(skld, in db);
            #endregion

            foreach (var item in zoznamPoloziek)       //prejdenie poloziek zo zoznamu
            {
                var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.PolozkaSkladu && x.Sklad == skld.ID); //najde polozku na sklade
                if (found != null)
                {
                    double pridan = celkoveMnozstvaZPrijemok.FirstOrDefault(x => x.ID == item.PolozkaSkladu)?.Mnozstvo ?? 0;
                    double odobrat = celkoveMnozstvaZVydajok.FirstOrDefault(x => x.ID == item.PolozkaSkladu)?.Mnozstvo ?? 0;
                    if (found.Mnozstvo + pridan - odobrat - item.Mnozstvo < 0) //ak je mnozstvo na sklade mensie ako mnozstvo ktore chceme vydat
                    {
                        listNemozno.Add(item.PolozkaSkladu);
                    }
                }
                else
                {
                    listNemozno.Add(item.PolozkaSkladu);
                }
            }
            return listNemozno;
        }
        /// <summary>
        /// Prejde zoznam prijímacích položiek a zistí, či je možné ich prijať/spracovať do skladu. 
        /// </summary>
        /// <param name="zoznamPoloziek">Zoznam prijímacích položiek.</param>
        /// <param name="skladdo">Sklad, do ktorého sa majú položky pridať.</param>
        /// <param name="db">Databázový kontext.</param>
        /// <param name="procedure">Voliteľná procedúra, ktorá sa vykoná pre každú položku, ktorú je možné pridať.</param>
        /// <returns>Zoznam položiek, ktoré nie je možné pridať.</returns>
        public static List<string> SpracovatPolozky(IEnumerable<PrijemkaPolozka> zoznamPoloziek, Sklad skladdo, in DBContext db, Action<PolozkaSkladuMnozstvo, PrijemkaPolozka>? procedure)
        {
            var listNemozno = new List<string>();
            var Rollback = false;
            using (var transaction = db.Database.BeginTransaction())
            {
                foreach (var item in zoznamPoloziek)       //prejdenie poloziek zo zoznamu
                {
                    var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.PolozkaSkladu && x.Sklad == skladdo.ID);
                    if (found == null)
                    {
                        listNemozno.Add(item.PolozkaSkladu);
                        Rollback = true;
                    }
                    else
                    {
                        if (procedure is not null)
                        {
                            procedure(found, item);
                        }
                    }
                }
                if (Rollback) //ak sa nemozu pridat vsetky polozky
                {
                    transaction.Rollback();
                }
                else
                {
                    db.SaveChanges();
                    transaction.Commit();
                }
            }
            return listNemozno;
        }

        /// <summary>
        /// Prejde zoznam položiek a zistí, či majú spojenie so skladom. 
        /// </summary>
        /// <param name="zoznamPoloziek">Zoznam prijímacích položiek.</param>
        /// <param name="skladdo">Sklad, do ktorého sa majú položky pridať.</param>
        /// <param name="db">Databázový kontext.</param>
        /// <param name="procedure">Voliteľná procedúra, ktorá sa vykoná pre každú položku, ktorú je možné pridať.</param>
        /// <returns>Zoznam položiek, ktoré nie je možné pridať.</returns>
        public static List<string> MoznoDodat(IEnumerable<PrijemkaPolozka> zoznamPoloziek, Sklad skladdo, in DBContext db)
        {
            var listNemozno = new List<string>();
            foreach (var item in zoznamPoloziek)       //prejdenie poloziek do skladu
            {
                var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.PolozkaSkladu && x.Sklad == skladdo.ID);
                if (found == null)
                {
                    listNemozno.Add(item.PolozkaSkladu);
                }
            }
            return listNemozno;
        }


        /// <summary>
        /// Načíta množstvo do položiek a zistí, či majú spojenie so skladom.
        /// </summary>
        /// <param name="zoznamPoloziek">Zoznam položiek, ktoré sa majú načítať a uloži do nich zrátané množstvá.</param>
        /// <param name="skladdo">Sklad, ku ktorému sa majú položky priradiť.</param>
        /// <param name="db">Databázový kontext.</param>
        /// <returns>Zoznam položiek, ktoré nemajú spojenie so skladom.</returns>
        public static List<PolozkaSkladu> LoadMnozstvoPoloziek(IEnumerable<PolozkaSkladu> zoznamPoloziek, Sklad skladdo, in DBContext db)
        {
            var listNemozno = new List<PolozkaSkladu>();

            #region nacitanie poloziek zo SCHVALENYCH prijemok
            List<PolozkaSkladu> celkoveMnozstvaZPrijemok = GetPoctyZPrijemok(skladdo, in db);
            #endregion
            #region nacitanie poloziek zo SCHVALENYCH vydajok
            List<PolozkaSkladu> celkoveMnozstvaZVydajok = GetPoctyZVydajok(skladdo, in db);
            #endregion

            foreach (var item in zoznamPoloziek)       //prejdenie poloziek zo zoznamu
            {
                var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.ID && x.Sklad == skladdo.ID);
                if (found != null)
                {
                    double pridan = celkoveMnozstvaZPrijemok.FirstOrDefault(x => x.ID == item.ID)?.Mnozstvo ?? 0;
                    double odobrat = celkoveMnozstvaZVydajok.FirstOrDefault(x => x.ID == item.ID)?.Mnozstvo ?? 0;
                    item.Mnozstvo = found.Mnozstvo + pridan - odobrat;
                }
                else
                {
                    listNemozno.Add(item);
                }
            }
            return listNemozno;
        }

        public static List<PolozkaSkladu> GetPoctyZPrijemok(Sklad skladdo, in DBContext db)
        {
            //TODO: pridat obdobie
            var prijemky = db.Prijemky.Where(x => x.Spracovana == true && x.Sklad == skladdo.ID).ToList();  //vsetky schvalene prijemky

            List<PolozkaSkladu> celkoveMnozstvaZPrijemok = new();   //finalny list bude obsahovat vsetky schvalene polozky z prijemok
            foreach (var ytem in prijemky) // prejde vsetky prijemky
            {
                celkoveMnozstvaZPrijemok.AddRange(Prijemka.ZosumarizujPolozkyPrijemky(ytem, in db)); //tento list prida na koniec finalneho listu
            }

            return celkoveMnozstvaZPrijemok.GroupBy(x => x.ID) //spoji duplikaty do unikatneho listu - FINALNY LIST
                .Select(group => new PolozkaSkladu()
                {
                    ID = group.Key,
                    Mnozstvo = group.Sum(item => item.Mnozstvo)
                })
                .ToList();
        }
        public static List<PolozkaSkladu> GetPoctyZVydajok(Sklad skladdo, in DBContext db)
        {
            //TODO: pridat obdobie
            var vydajky = db.Vydajky.Where(x => x.Spracovana == true && x.Sklad == skladdo.ID).ToList();

            List<PolozkaSkladu> celkoveMnozstvaZVydajok = new();   //finalny list bude obsahovat vsetky schvalene polozky z vydajok
            foreach (var ytem in vydajky) // prejde vsetky vydajky
            {
                celkoveMnozstvaZVydajok.AddRange(Vydajka.ZosumarizujPolozkyVydajky(ytem, in db)); //tento list prida na koniec finalneho listu
            }
            return celkoveMnozstvaZVydajok.GroupBy(x => x.ID) //spoji duplikaty do unikatneho listu - FINALNY LIST
                .Select(group => new PolozkaSkladu()
                {
                    ID = group.Key,
                    Mnozstvo = group.Sum(item => item.Mnozstvo)
                })
                .ToList();

        }
        public static bool ExistujePouzitie(PolozkaSkladu polozka, in DBContext db)
        {
            if (db.PrijemkyPolozky.Any(x => x.PolozkaSkladu == polozka.ID)) {
                return false;
            }
            if (db.VydajkyPolozky.Any(x => x.PolozkaSkladu == polozka.ID))
            {
                return false;
            }
            if (db.PolozkySkladuObjednavky.Any(x => x.PolozkaSkladu == polozka.ID))
            {
                return false;
            }
            return true;
        }
    }
}