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
    public partial class Sklad
    {
        /// <summary>
        /// povolene role, aj keby bol uzivatel pripojeny ku skladu tak musi mat prislusnu rolu inak len na zobrazenie
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_R_SKLAD { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.Nakupca, RolesOwn.Skladnik, };
        /// <summary>
        /// povolene role pre pridavanie a mazanie poloziek, zobrazovanie celeho skladu poloziek
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ZMENAPOLOZIEKROLE { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Nakupca, RolesOwn.Riaditel };
        /// <summary>
        /// povolenie pre prijem,vydaj,uzavretie skladu, nakupca by sa nemal starat o tieto veci
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> SKLADOVEPOHYBYROLE { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Skladnik };
        /// <summary>
        /// povolenie pre zobrazenie uzavretie skladu
        /// </summary>
        /// [NotMapped]
        public static List<RolesOwn> ROLE_R_SKLADOVEPOHYBY { get; private set; } = new() { RolesOwn.Riaditel, RolesOwn.Nakupca };
        /// <summary>
        /// povolenie pre prijem,vydaj,uzavretie skladu, nakupca by sa nemal starat o tieto veci
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CRUD_SKLAD { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Nakupca };

        /// <summary>
        /// Prejde zoznam prijímacích položiek a zistí, či je možné ich prijať.
        /// </summary>
        /// <param name="zoznamPoloziek">Zoznam prijímacích položiek.</param>
        /// <param name="skld">Sklad, z ktorého sa má vydať.</param>
        /// <param name="obdobie">Obdobie, pre ktoré sa kontroluje možnosť vydať položky.</param>
        /// <param name="db">Databázový kontext.</param>
        /// <returns>Zoznam položiek, ktoré nie je možné vydať.</returns>
        public static List<string> MoznoVydať(IEnumerable<PrijemkaPolozka> zoznamPoloziek, Sklad skld, DateTime obdobie, in DBContext db)
        {
            var listNemozno = new List<string>();

            #region nacitanie poloziek zo SCHVALENYCH prijemok
            List<PolozkaSkladu> celkoveMnozstvaZPrijemok = GetPoctyZPrijemok(skld, obdobie, in db);
            #endregion
            #region nacitanie poloziek zo SCHVALENYCH vydajok
            List<PolozkaSkladu> celkoveMnozstvaZVydajok = GetPoctyZVydajok(skld, obdobie, in db);
            #endregion
            #region nacitanie poloziek zo SCHVALENYCH prevodiek
            List<PolozkaSkladu> celkoveMnozstvaZPrevodiek = GetPoctyZPrevodiek(skld, obdobie, in db);
            #endregion
            foreach (var item in zoznamPoloziek)       //prejdenie poloziek zo zoznamu
            {
                var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.PolozkaSkladu && x.Sklad == skld.ID); //najde polozku na sklade
                if (found != null)
                {
                    double pridan = celkoveMnozstvaZPrijemok.FirstOrDefault(x => x.ID == item.PolozkaSkladu)?.Mnozstvo ?? 0;
                    double odobrat = celkoveMnozstvaZVydajok.FirstOrDefault(x => x.ID == item.PolozkaSkladu)?.Mnozstvo ?? 0;
                    double prevod = celkoveMnozstvaZPrevodiek.FirstOrDefault(x => x.ID == item.PolozkaSkladu)?.Mnozstvo ?? 0;
                    if (found.Mnozstvo + pridan + prevod - odobrat - item.Mnozstvo < 0) //ak je mnozstvo na sklade mensie ako mnozstvo ktore chceme vydat
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
        /// Načíta zratane množstvo do zoznamPoloziek a zistí, či majú spojenie so skladom.
        /// </summary>
        /// <param name="zoznamPoloziek">Zoznam položiek, ktoré sa majú načítať a uloži do nich zrátané množstvá.</param>
        /// <param name="skladdo">Sklad, ku ktorému sa majú položky priradiť.</param>
        /// <param name="db">Databázový kontext.</param>
        /// <returns>Zoznam položiek, ktoré nemajú spojenie so skladom.</returns>
        public static List<PolozkaSkladu> LoadMnozstvoPoloziek(IEnumerable<PolozkaSkladu> zoznamPoloziek, Sklad skladDo, in DBContext db)
        {
            var listNemozno = new List<PolozkaSkladu>();
            var aktualneObdobie = SkladObdobie.GetActualObdobieFromSklad(skladDo, in db);

            #region nacitanie poloziek zo SCHVALENYCH prijemok
            List<PolozkaSkladu> celkoveMnozstvaZPrijemok = GetPoctyZPrijemok(skladDo, aktualneObdobie, in db);
            #endregion
            #region nacitanie poloziek zo SCHVALENYCH vydajok
            List<PolozkaSkladu> celkoveMnozstvaZVydajok = GetPoctyZVydajok(skladDo, aktualneObdobie, in db);
            #endregion
            #region nacitanie poloziek zo SCHVALENYCH prevodiek
            List<PolozkaSkladu> celkoveMnozstvaZPrevodiek = GetPoctyZPrevodiek(skladDo, aktualneObdobie, in db);
            #endregion

            foreach (var item in zoznamPoloziek)       //prejdenie poloziek zo zoznamu
            {
                var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.ID && x.Sklad == skladDo.ID);
                if (found != null)
                {
                    double pridan = celkoveMnozstvaZPrijemok.FirstOrDefault(x => x.ID == item.ID)?.Mnozstvo ?? 0;
                    double odobrat = celkoveMnozstvaZVydajok.FirstOrDefault(x => x.ID == item.ID)?.Mnozstvo ?? 0;
                    double prevod = celkoveMnozstvaZPrevodiek.FirstOrDefault(x => x.ID == item.ID)?.Mnozstvo ?? 0;
                    item.Mnozstvo = found.Mnozstvo + pridan + prevod - odobrat;
                }
                else
                {
                    listNemozno.Add(item);
                }
            }
            return listNemozno;
        }

        public static List<PolozkaSkladu> GetAktualneMnozstva(Sklad sklad, in DBContext db)
        {
            var polozky = db.PolozkaSkladuMnozstva.Where(x => x.Sklad == sklad.ID).ToList();
            List<PolozkaSkladu> list = new();
            foreach (var item in polozky)
            {
                list.Add(item.PolozkaSkladuX.Clon());
            }
            LoadMnozstvoPoloziek(list, sklad, in db);
            return list;
        }

        public static List<PolozkaSkladu> LoadMnozstvoZaObdobie(IEnumerable<PolozkaSkladu> zoznamPoloziek, Sklad sklad, DateTime obdobie, in DBContext db)
        {
            var listNemozno = new List<PolozkaSkladu>();

            #region nacitanie poloziek zo SCHVALENYCH prijemok
            List<PolozkaSkladu> celkoveMnozstvaZPrijemok = GetPoctyZPrijemok(sklad, obdobie, in db);
            #endregion
            #region nacitanie poloziek zo SCHVALENYCH vydajok
            List<PolozkaSkladu> celkoveMnozstvaZVydajok = GetPoctyZVydajok(sklad, obdobie, in db);
            #endregion
            #region nacitanie poloziek zo SCHVALENYCH prevodiek
            List<PolozkaSkladu> celkoveMnozstvaZPrevodiek = GetPoctyZPrevodiek(sklad, obdobie, in db);
            #endregion

            foreach (var item in zoznamPoloziek)       //prejdenie poloziek zo zoznamu
            {
                var found = db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.ID && x.Sklad == sklad.ID);
                if (found != null)
                {
                    double pridan = celkoveMnozstvaZPrijemok.FirstOrDefault(x => x.ID == item.ID)?.Mnozstvo ?? 0;
                    double odobrat = celkoveMnozstvaZVydajok.FirstOrDefault(x => x.ID == item.ID)?.Mnozstvo ?? 0;
                    double prevod = celkoveMnozstvaZPrevodiek.FirstOrDefault(x => x.ID == item.ID)?.Mnozstvo ?? 0;
                    item.Mnozstvo = pridan + prevod - odobrat;
                }
                else
                {
                    listNemozno.Add(item);
                }
            }
            return listNemozno;
        }


        public static List<PolozkaSkladu> GetPoctyZPrijemok(Sklad skladDo, DateTime obdobie, in DBContext db)
        {
            var prijemky = db.Prijemky.Where(x => x.Spracovana == true && x.Sklad == skladDo.ID && x.Obdobie == obdobie).ToList();  //vsetky schvalene prijemky

            List<PolozkaSkladu> celkoveMnozstvaZPrijemok = new();   //finalny list bude obsahovat vsetky schvalene polozky z prijemok
            foreach (var ytem in prijemky) // prejde vsetky prijemky
            {
                celkoveMnozstvaZPrijemok.AddRange(Prijemka.ZosumarizujPolozkyPrijemky(ytem, in db)); //tento list prida na koniec finalneho listu
            }

            //return celkoveMnozstvaZPrijemok.GroupBy(x => x.ID) //spoji duplikaty do unikatneho listu - FINALNY LIST
            //    .Select(group => new PolozkaSkladu()
            //    {
            //        ID = group.Key,
            //        Mnozstvo = group.Sum(item => item.Mnozstvo)
            //    })
            //    .ToList();
            return PolozkaSkladu.ZosumarizujListPoloziek(in celkoveMnozstvaZPrijemok);
        }
        public static List<PolozkaSkladu> GetPoctyZVydajok(Sklad skladDo, DateTime obdobie, in DBContext db)
        {
            var vydajky = db.Vydajky.Where(x => x.Spracovana == true && x.Sklad == skladDo.ID && x.Obdobie == obdobie).ToList();

            List<PolozkaSkladu> celkoveMnozstvaZVydajok = new();   //finalny list bude obsahovat vsetky schvalene polozky z vydajok
            foreach (var ytem in vydajky) // prejde vsetky vydajky
            {
                celkoveMnozstvaZVydajok.AddRange(Vydajka.ZosumarizujPolozkyVydajky(ytem, in db)); //tento list prida na koniec finalneho listu
            }
            return PolozkaSkladu.ZosumarizujListPoloziek(in celkoveMnozstvaZVydajok);
        }

        public static List<PolozkaSkladu> GetPoctyZPrevodiek(Sklad skladDo, DateTime obdobie, in DBContext db)
        {
            var prevodky = db.Vydajky.Where(x => x.Spracovana == true && x.SkladDo == skladDo.ID && x.ObdobieDo == obdobie).ToList();  //vsetky schvalene prevodky

            List<PolozkaSkladu> celkoveMnozstvaZPrevodky = new();   //finalny list bude obsahovat vsetky schvalene polozky z predoky
            foreach (var ytem in prevodky) // prejde vsetky prevodky
            {
                celkoveMnozstvaZPrevodky.AddRange(Vydajka.ZosumarizujPolozkyVydajky(ytem, in db)); //tento list prida na koniec finalneho listu
            }

            return PolozkaSkladu.ZosumarizujListPoloziek(in celkoveMnozstvaZPrevodky);
        }

        public static List<PolozkaSkladu> GetPoctyZPrevodiekZoSkladu(Sklad skladZ, DateTime obdobie, in DBContext db)
        {
            var prevodky = db.Vydajky.Where(x => x.Spracovana == true && x.Sklad == skladZ.ID && x.Obdobie == obdobie && !string.IsNullOrEmpty(x.SkladDo)).ToList();  //vsetky schvalene prevodky

            List<PolozkaSkladu> celkoveMnozstvaZPrevodky = new();   //finalny list bude obsahovat vsetky schvalene polozky z predoky
            foreach (var ytem in prevodky) // prejde vsetky prevodky
            {
                celkoveMnozstvaZPrevodky.AddRange(Vydajka.ZosumarizujPolozkyVydajky(ytem, in db)); //tento list prida na koniec finalneho listu
            }

            return PolozkaSkladu.ZosumarizujListPoloziek(in celkoveMnozstvaZPrevodky);
        }

        public static bool ExistujePouzitie(PolozkaSkladu polozka, in DBContext db)
        {
            if (db.PrijemkyPolozky.Any(x => x.PolozkaSkladu == polozka.ID))
            {
                return true;
            }
            if (db.VydajkyPolozky.Any(x => x.PolozkaSkladu == polozka.ID))
            {
                return true;
            }
            if (db.PolozkySkladuObjednavky.Any(x => x.PolozkaSkladu == polozka.ID))
            {
                return true;
            }
            return false;
        }


    }
}