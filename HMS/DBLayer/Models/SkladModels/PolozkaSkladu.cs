﻿using DBLayer.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class PolozkaSkladu : ICloneable
    {
        [Key]
        public string ID { get; set; } = default!;
        [Required(ErrorMessage = "Nutné pole")]
        [StringLength(128, MinimumLength = 3, ErrorMessage = "Nazov musi byť v rozmedzi 3 - 128 znakov")]
        public string Nazov { get; set; } = default!;
        [Required(ErrorMessage = "Nutné pole")]
        [StringLength(32, MinimumLength = 1, ErrorMessage = "Merná jednotka musi byť v rozmedzi 1 - 32 znakov")]
        public string MernaJednotka { get; set; } = default!;

        /// <summary>
        /// Sluzi ako predloha dph, nemala by sa vyuzivat pri vypoctoch
        /// </summary>
        [Column(TypeName = "decimal(9, 3)")]
        [DecimalNonNegative(ErrorMessage = "Len kladné hodnoty.")]
        public decimal DPH_mask { get; set; } = 23;
       
        [NotMapped]
        public double Mnozstvo { get; set; } = 0;

        [NotMapped]
        [Range(0.0, double.MaxValue, ErrorMessage = "Len kladné hodnoty")]
        public double Cena { get; set; } = 0;
        
        [NotMapped]
        public double CelkovaCena { get => (double)Mnozstvo * Cena; }
        /// <summary>
        /// Pouzita je DPH nie DPH_mask
        /// </summary>
        [NotMapped]
        public double CenaDPH { get => (Cena * (double)(100 + DPH)) / 100; }
        /// <summary>
        /// Pouzita je DPH nie DPH_mask
        /// </summary>
        [NotMapped]
        public double CelkovaCenaDPH { get => (double)Mnozstvo * CenaDPH; }
        /// <summary>
        /// DPH v percentach, [NotMapped], ulozene dph je v DPH_mask
        /// </summary>
        [NotMapped]
        [DecimalNonNegative(ErrorMessage = "Len kladné hodnoty.")]
        public decimal DPH { get; set; } = 23;

        /// <summary>
        /// [NotMapped]
        /// </summary>
        [NotMapped]
        public bool ActiveNM { get; set; } = true;

        public object Clone()       //deep copy
        {
            PolozkaSkladu clone = new PolozkaSkladu();
            clone.Nazov = Nazov;
            clone.ID = ID;
            clone.MernaJednotka = MernaJednotka;
            clone.Mnozstvo = Mnozstvo;
            clone.Cena = Cena;
            clone.DPH = DPH;
            clone.DPH_mask = DPH_mask;

            return clone;
        }
        public PolozkaSkladu Clon()
        {
            return (PolozkaSkladu)Clone();
        }

        /// <summary>
        /// Nastavi DPH na hodnotu z DPH_mask
        /// </summary>
        public void SetDPHFromMask()
        {
            DPH = DPH_mask;
        }
        public static List<PolozkaSkladu> ZosumarizujListPoloziek(in List<PolozkaSkladuObjednavky> polozky)
        {
            return polozky.GroupBy(x =>
            x.PolozkaSkladu) //spoji duplikaty do unikatneho listu
            .Select(group =>
            {
                var totalMnozstvo = group.Sum(x => x.Mnozstvo);
                return new PolozkaSkladu()
                {
                    ID = group.Key,
                    //Nazov = group.First().Nazov,
                    //MernaJednotka = group.First().MernaJednotka,
                    Mnozstvo = totalMnozstvo,
                    Cena = totalMnozstvo != 0 ? group.Sum(x => x.Cena * x.Mnozstvo) / totalMnozstvo : 0,
                    DPH = totalMnozstvo != 0 ? group.Sum(x => (decimal)x.DPH * (decimal)x.Mnozstvo) / (decimal)totalMnozstvo : 0
                };
            })

            //    group => new PolozkaSkladu()
            //{
            //    ID = group.Key,
            //    //Nazov = group.First().Nazov,
            //    //MernaJednotka = group.First().MernaJednotka,
            //    Mnozstvo = group.Sum(x => x.Mnozstvo),
            //    Cena = group.Sum(x => x.Cena * x.Mnozstvo) / group.Sum(x => x.Mnozstvo),
            //    DPH = group.Sum(x => (decimal)x.DPH * (decimal)x.Mnozstvo) / (decimal)group.Sum(x => x.Mnozstvo)
            //})
            .ToList();
        }

        public static List<PolozkaSkladu> ZosumarizujListPoloziek(in List<PolozkaSkladu> polozky, bool checkNan = false)
        {
            var list = polozky.GroupBy(x => x.ID) //spoji duplikaty do unikatneho listu
            .Select(group =>
            {
                var totalMnozstvo = group.Sum(x => x.Mnozstvo);
                return new PolozkaSkladu()
                {
                    ID = group.Key,
                    //Nazov = group.First().Nazov,
                    //MernaJednotka = group.First().MernaJednotka,
                    Mnozstvo = totalMnozstvo,
                    Cena = totalMnozstvo != 0 ? group.Sum(x => x.Cena * x.Mnozstvo) / totalMnozstvo : 0,
                    DPH = totalMnozstvo != 0 ? group.Sum(x => (decimal)x.DPH * (decimal)x.Mnozstvo) / (decimal)totalMnozstvo : 0
                };
            })
            .ToList();
            if (checkNan)
            {
                foreach (var item in list)
                {
                    item.Cena = double.IsNaN(item.Cena) ? 0.0 : item.Cena;
                }
            }
            return list;
        }

        public static List<PolozkaSkladu> ZosumarizujListPoloziek(in List<PrijemkaPolozka> polozky)
        {
            return polozky.GroupBy(x => x.PolozkaSkladu) //spoji duplikaty do unikatneho listu
            .Select(group =>
            {
                var totalMnozstvo = group.Sum(x => x.Mnozstvo);
                return new PolozkaSkladu()
                {
                    ID = group.Key,
                    //Nazov = group.First().Nazov,
                    //MernaJednotka = group.First().MernaJednotka,
                    Mnozstvo = totalMnozstvo,
                    Cena = totalMnozstvo != 0 ? group.Sum(x => x.Cena * x.Mnozstvo) / totalMnozstvo : 0,
                    DPH = totalMnozstvo != 0 ? group.Sum(x => (decimal)x.DPH * (decimal)x.Mnozstvo) / (decimal)totalMnozstvo : 0
                };
            })
            //    group => new PolozkaSkladu()
            //{
            //    ID = group.Key,
            //    //Nazov = group.First().Nazov,
            //    //MernaJednotka = group.First().MernaJednotka,
            //    Mnozstvo = group.Sum(x => x.Mnozstvo),
            //    Cena = group.Sum(x => x.Cena * x.Mnozstvo) / group.Sum(x => x.Mnozstvo),
            //    DPH = group.Sum(x => (decimal)x.DPH * (decimal)x.Mnozstvo) / (decimal)group.Sum(x => x.Mnozstvo)
            //})
            .ToList();
        }

        public static List<PolozkaSkladu> ZosumarizujListPoloziek(in List<ItemPokladDokladu> polozky)
        {
            return polozky.GroupBy(x => ((PolozkaSkladuConItemPoklDokladu)x.UniConItemPoklDokladuX).PolozkaSkladuMnozstvaX.PolozkaSkladu) //spoji duplikaty do unikatneho listu
            .Select(group => new PolozkaSkladu()
            {
                ID = group.Key,
                Mnozstvo = group.Sum(x => x.Mnozstvo),
                Cena = group.Sum(x => x.Cena * x.Mnozstvo) / group.Sum(x => x.Mnozstvo),
                DPH = group.Sum(x => (decimal)x.DPH * (decimal)x.Mnozstvo) / (decimal)group.Sum(x => x.Mnozstvo)
            })
            .ToList();
        }

        public static void SpracListySpolu(IEnumerable<PolozkaSkladu> listDo, in List<PolozkaSkladu> listZ, Action<PolozkaSkladu, PolozkaSkladu> procedure)
        {
            foreach (var item in listDo)       //prejdenie poloziek zo zoznamu
            {
                var found = listZ.FirstOrDefault(x => x.ID == item.ID);
                if (found != null)
                {
                    procedure(item, found);
                }
            }
        }
        public static void ResetMnozstva(IEnumerable<PolozkaSkladu> list)
        {
            foreach (var item in list)
            {
                item.Mnozstvo = 0;
            }
        }


    }
}
