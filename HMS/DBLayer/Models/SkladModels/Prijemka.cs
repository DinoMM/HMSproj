using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class Prijemka : PohSkup
    {
        public string Objednavka { get; set; } = default!;
        public string DodaciID { get; set; } = default!;
        public string FakturaID { get; set; } = default!;
    }
}
