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
        [ForeignKey("SkladX")]
        public string Sklad { get; set; } = default!;
        public Sklad SkladX { get; set; }
        [ForeignKey("SkladDoX")]
        public string? SkladDo { get; set; } = default!;
        public Sklad? SkladDoX { get; set; }
    }
}
