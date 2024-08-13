using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class SkladObdobie
    {
        [Key]
        public DateTime Obdobie { get; set; } = GetSeassonFromToday();
        [ForeignKey("SkladX")]
        public string Sklad { get; set; } = default!;
        public Sklad SkladX { get; set; }

        public static DateTime GetSeasonFromDate(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }
        /// <summary>
        /// Test, či dátum 'date' sa nachádza v mesiaci a roku priloženého 'dateMonth'
        /// </summary>
        /// <param name="date"></param>
        /// <param name="dateMonth"></param>
        /// <returns></returns>
        public static bool IsDateInMonth(DateTime date, DateTime dateMonth)
        {
            return date >= dateMonth && date <= dateMonth.AddMonths(1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Vrati datetime na zaklade dnesneho datumu zaokruhleny na prvy den v mesiaci</returns>
        public static DateTime GetSeassonFromToday()
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }


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


    }

}
