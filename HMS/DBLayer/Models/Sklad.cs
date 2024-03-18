using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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


        public DateTime GetActual() {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        public string ShortformObdobie() {
            return Obdobie.ToString("MMyy");
        }

        public void NextObdobie() {
            Obdobie.AddMonths(1);
        }

    }
}
