using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class SkladObdobie
    {
        /// <summary>
        /// povolene role, ltpre mozu ozavriet obdobie a nasledne začat nové
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLEUZAVRETIAOBDOBIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.Uctovnik };

        /// <summary>
        /// Získa aktuálne obdobie pre daný sklad. Vytvorí obdobie ak neexistuje
        /// </summary>
        /// <param name="sklad">Sklad, pre ktorý sa má získať aktuálne obdobie.</param>
        /// <param name="db">DBContext objekt pre prístup k databáze.</param>
        /// <returns>Aktuálne obdobie pre daný sklad.</returns>
        public static DateTime GetActualObdobieFromSklad(Sklad sklad, in DBContext db)
        {
            var actualDate = db.SkladObdobi.Where(x => x.Sklad == sklad.ID)
                .OrderByDescending(x => x.Obdobie)
                .FirstOrDefault()?.Obdobie;
            if (actualDate is null)
            {
                CreateObdobie(sklad, GetSeassonFromToday(), in db);
                return GetActualObdobieFromSklad(sklad, in db);     //POZOR rekurzia
            }
            return actualDate.Value;
        }


        /// <summary>
        /// Vytvorí nové obdobie pre daný sklad ak neexistuje.
        /// </summary>
        /// <param name="sklad">Sklad, pre ktorý sa má vytvoriť obdobie.</param>
        /// <param name="obdobie">Dátum obdobia.</param>
        /// <param name="db">DBContext objekt pre prístup k databáze.</param>
        public static void CreateObdobie(Sklad sklad, DateTime obdobie, in DBContext db)
        {
            var obd = new SkladObdobie()
            {
                Obdobie = obdobie,
                Sklad = sklad.ID,
                SkladX = sklad
            };
            if (db.SkladObdobi.Any(x => x.Sklad == sklad.ID && x.Obdobie == obdobie)) //pre istotu, ak by uz existovalo
            {
                return;
            }
            db.SkladObdobi.Add(obd);
            db.SaveChanges();
        }

        public static bool UzavrietObdobie(Sklad sklad, DateTime obdobie, List<PolozkaSkladu> zoznam, in DBContext db, in UserService uservice)
        {
            #region kontrola
            if (!MoznoUzavrietObdobie(sklad, obdobie, in db, in uservice)) //kontrola ci je mozne uzavriet obdobie
            {
                return false;
            }
            #endregion
            #region aktualizacia poloziek mnozstva a ceny
            var listActual = DBLayer.Models.Sklad.GetAktualneMnozstva(sklad, in db); //ziskanie aktualnych mnozstiev
            var listNoveMnozstvo = db.PolozkaSkladuMnozstva.Where(x => x.Sklad == sklad.ID).ToList();

            foreach (var item in listNoveMnozstvo)
            {
                item.Mnozstvo = listActual.FirstOrDefault(x => x.ID == item.PolozkaSkladu)?.Mnozstvo ?? 0; //nastavenie mnozstva
                if (item.Mnozstvo < 0)      //pre istotu kontrola
                {
                    db.ClearPendingChanges();
                    return false;
                }
                item.Cena = zoznam.FirstOrDefault(x => x.ID == item.PolozkaSkladu)?.Cena ?? 0; //nastavenie ceny
            }
            db.SaveChanges();

            #endregion
            #region pridanie noveho obdobia
            CreateObdobie(sklad, GetNextObdobie(sklad, obdobie, in db), in db); //vytvorenie noveho obdobia podla určeneho pravidla
            #endregion
            return true;
        }

        public static bool MoznoUzavrietObdobie(Sklad sklad, DateTime obdobie, in DBContext db, in UserService uservice)
        {
            if (!ROLEUZAVRETIAOBDOBIA.Contains(uservice.LoggedUserRole)) //kontrola opravneni
            {
                return false;
            }

            if (SkladObdobie.GetActualObdobieFromSklad(sklad, in db) != obdobie)    // kontrola ci je to aktualne obdobie
            {
                return false;
            }

            return VsetkyPrijemkyObdobiaSpracovane(sklad, obdobie, in db) && VsetkyVydajkyObdobiaSpracovane(sklad, obdobie, in db); // kontrola ci su všetky prijemky a vydajky spracovane v obdobii
        }

        public static bool VsetkyPrijemkyObdobiaSpracovane(Sklad sklad, DateTime obdobie, in DBContext db)
        {
            return !db.Prijemky.Any(x => x.Sklad == sklad.ID && x.Obdobie == obdobie && x.Spracovana == false);
        }
        public static bool VsetkyVydajkyObdobiaSpracovane(Sklad sklad, DateTime obdobie, in DBContext db)
        {
            return !db.Vydajky.Any(x => x.Sklad == sklad.ID && x.Obdobie == obdobie && x.Spracovana == false);
        }


        /// <summary>
        /// Získa nasledujúce obdobie pre daný sklad podľa určenyhc pravidiel.
        /// </summary>
        /// <param name="sklad">Sklad, pre ktorý sa má získať nasledujúce obdobie.</param>
        /// <param name="obdobie">obdobie, najlepšie aktuálne.</param>
        /// <param name="db">DBContext objekt pre prístup k databáze.</param>
        /// <returns>Nasledujúce obdobie pre daný sklad.</returns>
        private static DateTime GetNextObdobie(Sklad sklad, DateTime obdobie, in DBContext db)
        {
            obdobie = new DateTime(obdobie.Year, obdobie.Month, 1); // nastavenie na prvy den aktualneho mesiaca

            if (SkladObdobie.GetActualObdobieFromSklad(sklad, in db) == obdobie) // ak je to aktualne obdobie, tak sa posunie o jeden mesiac
            {
                obdobie = obdobie.AddMonths(1);
                obdobie = new DateTime(obdobie.Year, obdobie.Month, 1);
            }
            return obdobie;
        }

        /// <summary>
        /// Získa všetky obdobia pre daný sklad, zoradené od najbližšieho(aktuálneho) po staršie.
        /// </summary>
        /// <param name="sklad"></param>
        /// <param name="obdobiePo"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static List<DateTime> GetObdobiaPo(Sklad sklad, DateTime obdobiePo, in DBContext db)
        {
            return db.SkladObdobi.Where(x => x.Sklad == sklad.ID && x.Obdobie >= obdobiePo)
              .OrderByDescending(x => x.Obdobie)
              .Select(x => x.Obdobie).ToList();
        }

        public static bool IsObdobieActual(Sklad sklad, DateTime obdobie, in DBContext db)
        {
            var actualDate = db.SkladObdobi.Where(x => x.Sklad == sklad.ID)
                .OrderByDescending(x => x.Obdobie)
                .FirstOrDefault()?.Obdobie;
            return actualDate == obdobie;
        }
    }

}
