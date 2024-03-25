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
        public DateTime Obdobie { get; set; } = GetActualSeason();
        [ForeignKey("SkladX")]
        public string Sklad { get; set; } = default!;
        public Sklad SkladX { get; set; }

        public static DateTime GetActualSeason()
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }
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

    }

}
