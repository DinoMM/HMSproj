using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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


        /// <summary>
        /// Skontroluje mnozstvo poloziek na sklade.
        /// </summary>
        /// <param name="polozky"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static ValidationResult ValidateMnozstvo(in List<ItemPokladDokladu> polozky, in DBContext db)
        {
            var listRelevant = polozky.Where(x => x.UniConItemPoklDokladuX is PolozkaSkladuConItemPoklDokladu).ToList();
            if (listRelevant == null || listRelevant.Count == 0)
            {
                return ValidationResult.Success;
            }

            List<(DateTime?, Sklad, PolozkaSkladu)> listValidator = new();
            foreach (var item in listRelevant)
            {
                (DateTime?, Sklad, PolozkaSkladu) newitem = new();
                newitem.Item1 = item.Obdobie;
                newitem.Item2 = ((PolozkaSkladuConItemPoklDokladu)item.UniConItemPoklDokladuX).PolozkaSkladuMnozstvaX.SkladX;
                newitem.Item3 = ((PolozkaSkladuConItemPoklDokladu)item.UniConItemPoklDokladuX).PolozkaSkladuMnozstvaX.PolozkaSkladuX;
                newitem.Item3.Cena = item.Cena;
                newitem.Item3.DPH = item.DPH;
                newitem.Item3.Mnozstvo = item.Mnozstvo;
                listValidator.Add(newitem);
            }
            var listL = listValidator.GroupBy(x => new { x.Item1, x.Item2 })
                .Select(group => new
                {
                    Obdobie = group.Key.Item1,
                    Sklad = group.Key.Item2,
                    Mnozstvo = group.Sum(x => x.Item3.Mnozstvo),
                    Cena = group.Sum(x => x.Item3.Cena * x.Item3.Mnozstvo) / group.Sum(x => x.Item3.Mnozstvo),
                    DPH = group.Sum(x => (decimal)x.Item3.DPH * (decimal)x.Item3.Mnozstvo) / (decimal)group.Sum(x => x.Item3.Mnozstvo),
                    PolozkaSkladu = group.Select(x => x.Item3).First()
                })
                .ToList();


            foreach (var item in listL)     //poprehadzovanie
            {
                item.PolozkaSkladu.Cena = item.Cena;
                item.PolozkaSkladu.DPH = item.DPH;
                item.PolozkaSkladu.Mnozstvo = item.Mnozstvo;
            }

            foreach (var item in listL)     //pontrola polozka po polozke
            {
                if (Sklad.MoznoVydať(new List<PrijemkaPolozka> {
                    new PrijemkaPolozka {
                        PolozkaSkladu = item.PolozkaSkladu.ID,
                        Mnozstvo = item.Mnozstvo,
                        Cena = item.Cena,
                        DPH = item.DPH
                    }},
                    item.Sklad,
                    item.Obdobie.Value,
                    in db).Count != 0)
                {
                    return new ValidationResult($"Nemožno vydať viacej ako je na sklade: {item.Sklad} - {item.PolozkaSkladu.ID}");
                }
            }
            return ValidationResult.Success;
        }
    }
}
