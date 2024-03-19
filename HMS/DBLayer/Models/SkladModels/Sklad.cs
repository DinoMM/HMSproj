using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        /// povolene role pre pridavanie a mazanie poloziek
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ZMENAPOLOZIEKROLE { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Nakupca };
        /// <summary>
        /// povolenie pre prijem,vydaj,uzavretie skladu, nakupca by sa nemal starat o tieto veci
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> SKLADOVEPOHYBYROLE { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Skladnik };


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
