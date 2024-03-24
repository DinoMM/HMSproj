using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class Vydajka : PohSkup
    {
        public string Sklad { get; set; } = default!;
        public DateTime Obdobie { get; set; } = default!;
        [ForeignKey("Sklad, Obdobie")]
        public Sklad SkladX { get; set; }
        
        public string? SkladDo { get; set; } = default!;
        public DateTime? ObdobieDo { get; set; } = default!;
        [ForeignKey("SkladDo, ObdobieDo")]
        public Sklad? SkladDoX { get; set; }

    }
}
