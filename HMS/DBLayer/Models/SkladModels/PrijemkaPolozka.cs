using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class PrijemkaPolozka : PohJednotka
    {
        [ForeignKey("PolozkaSkladuMnozstvaX")]
        public long PolozkaSkladuMnozstva { get; set; }
        public PolozkaSkladuMnozstvo PolozkaSkladuMnozstvaX { get; set; }
    }
}
