using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class SkladUzivatel
    {
        [ForeignKey("SkladX")]
        public string Sklad { get; set; } = default!;
        public Sklad SkladX { get; set; }
        [ForeignKey("UzivatelX")]
        public string Uzivatel { get; set; } = default!;
        public IdentityUserOwn UzivatelX { get; set; }

        
    }
}
