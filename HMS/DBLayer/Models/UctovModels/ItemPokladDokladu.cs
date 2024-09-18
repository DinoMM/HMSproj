using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class ItemPokladDokladu : PohJednotka
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        [ForeignKey("SkupinaX")]
        public string Skupina { get; set; }
        public PohSkup SkupinaX { get; set; }
        public string? Nazov { get; set; }
        public double Mnozstvo { get; set; }
        public double Cena { get; set; }
        [NotMapped]
        public double CelkovaCena { get => (double)Mnozstvo * Cena; }
        [NotMapped]
        public double CenaDPH { get => (Cena * (double)(100 + DPH)) / 100; }
        [NotMapped]
        public double CelkovaCenaDPH { get => (double)Mnozstvo * CenaDPH; }



        [ForeignKey("UniConItemPoklDokladuX")]
        public long UniConItemPoklDokladu { get; set; }
        public UniConItemPoklDokladu UniConItemPoklDokladuX { get; set; }

        [Column(TypeName = "decimal(9, 3)")]
        [DecimalNonNegative(ErrorMessage = "Len kladné hodnoty.")]
        public decimal DPH { get; set; } = 20;



        public object Clone()
        {
            var clone = new ItemPokladDokladu();
            clone.ID = this.ID;
            clone.Skupina = this.Skupina;
            clone.SkupinaX = this.SkupinaX;
            clone.Nazov = this.Nazov;
            clone.Mnozstvo = this.Mnozstvo;
            clone.Cena = this.Cena;
            clone.UniConItemPoklDokladu = this.UniConItemPoklDokladu;
            clone.UniConItemPoklDokladuX = this.UniConItemPoklDokladuX;
            clone.DPH = this.DPH;
            return clone;
        }

        public ItemPokladDokladu Clon()
        {
            return (ItemPokladDokladu)this.Clone();
        }

        public void SetFrom(ItemPokladDokladu item)
        {
            this.ID = item.ID;
            this.Skupina = item.Skupina;
            this.SkupinaX = item.SkupinaX;
            this.Nazov = item.Nazov;
            this.Mnozstvo = item.Mnozstvo;
            this.Cena = item.Cena;
            this.UniConItemPoklDokladu = item.UniConItemPoklDokladu;
            this.UniConItemPoklDokladuX = item.UniConItemPoklDokladuX;
            this.DPH = item.DPH;
        }
    }
}
