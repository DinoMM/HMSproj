using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Vydajka : PohSkup
    {
        [ForeignKey("SkladX")]
        public string Sklad { get; set; } = default!;
        public Sklad SkladX { get; set; }

        [ForeignKey("SkladDoX")]
        public string? SkladDo { get; set; } = default!;
        public Sklad? SkladDoX { get; set; }
        public DateTime Obdobie { get; set; } = SkladObdobie.GetSeassonFromToday();
        public DateTime? ObdobieDo { get; set; } = SkladObdobie.GetSeassonFromToday();

        [ForeignKey("DruhPohybuX")]
        public string? DruhPohybu { get; set; } = default!;
        public DruhPohybu? DruhPohybuX { get; set; }

        public override string GetDisplayName()
        {
            return "Výdajka";
        }
    }
}
