using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DBLayer.Models
{
    public partial class PolozkaSkladuConItemPoklDokladu : UniConItemPoklDokladu
    {
        [DisplayAndValue<string, PolozkaSkladuConItemPoklDokladu>("ID", getid: true)]
        [ForeignKey("PolozkaSkladuMnozstvaX")]
        public long PolozkaSkladuMnozstva { get; set; }
        public PolozkaSkladuMnozstvo PolozkaSkladuMnozstvaX { get; set; } = new();

        [DisplayAndValue<string, PolozkaSkladuConItemPoklDokladu>("Predajná cena")]
        [Column(TypeName = "decimal(18, 3)")]
        [DecimalNonNegative]
        public decimal PredajnaCena { get; set; } = 0.0M;

        [DisplayAndValue<string, PolozkaSkladuConItemPoklDokladu>("Predajné DPH")]
        [Column(TypeName = "decimal(9, 3)")]
        [DecimalNonNegative]
        public decimal PredajneDPH { get; set; } = 20;




        public override string GetID()
        {
            return PolozkaSkladuMnozstvaX.PolozkaSkladu;
        }

        public override string GetTypeUni()
        {
            return "Položka skladu";
        }

        public override string GetNameUni()
        {
            return PolozkaSkladuMnozstvaX.PolozkaSkladuX.Nazov;
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
                PolozkaSkladuMnozstva = this.PolozkaSkladuMnozstva,
                PolozkaSkladuMnozstvaX = this.PolozkaSkladuMnozstvaX,
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
                this.PolozkaSkladuMnozstva = ytem.PolozkaSkladuMnozstva;
                this.PolozkaSkladuMnozstvaX = ytem.PolozkaSkladuMnozstvaX;
                this.PredajnaCena = ytem.PredajnaCena;
                this.PredajneDPH = ytem.PredajneDPH;
            }
            else
            {
                throw new InvalidCastException("Nemožno skopírovat z ineho typu ako PolozkaSkladuConItemPoklDokladu");
            }
        }

        public override bool JeItemOnlyOneTyp()
        {
            return false;
        }

        public override decimal GetDPHUni()
        {
            return PredajneDPH;
        }
    }
}
