using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class PolozkaSkladuConItemPoklDokladu : UniConItemPoklDokladu
    {
        [ForeignKey("PolozkaSkladuX")]
        public string PolozkaSkladu { get; set; }
        public PolozkaSkladu PolozkaSkladuX { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        [DecimalNonNegative]
        public decimal PredajnaCena { get; set; } = 0.0M;
        [Column(TypeName = "decimal(9, 3)")]
        [DecimalNonNegative]
        public decimal PredajneDPH { get; set; } = 20;




        public override string GetID()
        {
            return PolozkaSkladu;
        }

        public override string GetTypeUni()
        {
            return "Položka skladu";
        }

        public override string GetNameUni()
        {
            return PolozkaSkladuX.Nazov;
        }

        public override decimal GetCenaUni()
        {
            return PredajnaCena;
        }

        public override object Clone()
        {
            var clone = new PolozkaSkladuConItemPoklDokladu
            {
                ID = this.ID,
                PolozkaSkladu = this.PolozkaSkladu,
                PolozkaSkladuX = this.PolozkaSkladuX,
                PredajnaCena = this.PredajnaCena,
                PredajneDPH = this.PredajneDPH
            };
            return clone;
        }

        public override UniConItemPoklDokladu Clon()
        {
            return (PolozkaSkladuConItemPoklDokladu)Clone();
        }

        public override void SetFrom(UniConItemPoklDokladu item)
        {
            if (item is PolozkaSkladuConItemPoklDokladu ytem)
            {
                this.ID = ytem.ID;
                this.PolozkaSkladu = ytem.PolozkaSkladu;
                this.PolozkaSkladuX = ytem.PolozkaSkladuX;
                this.PredajnaCena = ytem.PredajnaCena;
                this.PredajneDPH = ytem.PredajneDPH;
            }
            else
            {
                throw new InvalidCastException("Nemožno skopírovat z ineho typu ako PolozkaSkladuConItemPoklDokladu");
            }
        }
    }
}
