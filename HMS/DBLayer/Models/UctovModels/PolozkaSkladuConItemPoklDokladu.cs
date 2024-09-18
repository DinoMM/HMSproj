using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models.UctovModels
{
    public partial class PolozkaSkladuConItemPoklDokladu : UniConItemPoklDokladu
    {
        [ForeignKey("PolozkaSkladuX")]
        public string PolozkaSkladu { get; set; }
        public PolozkaSkladu PolozkaSkladuX { get; set; }

        public decimal PredajnaCena { get; set; } = 0.0M;
        public decimal PredajneDPH { get; set; } = 20;
    }
}
