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
        [Key]
        public string ID { get; set; } = default!;
        public string Nazov { get; set; } = default!;
        //public DateTime Obdobie { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); //vrati vzdy prveho dany mesiac

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

        

    }
}