using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class Faktura
    {
        [Key]
        public string ID { get; set; } = default!;
        [ForeignKey("SkupinaX")]
        public string Skupina { get; set; } = default!;
        public PohSkup SkupinaX { get; set; }
        public DateTime Vystavenie { get; set; }
        public DateTime Splatnost { get; set; }
        public Dodavatel OdKoho { get; set; }
        [NotMapped]
        public double Cena { get; set; }
        [NotMapped]
        public double CenaDPH { get => (Cena * 120) / 100; }
        

        public static string DajNoveID(DbSet<Faktura> dbset)
        {
            int adder = 1;
            string newID;
            do
            {
                int cislo;
                if (dbset.Count() != 0)
                {
                    cislo = int.Parse(dbset.DefaultIfEmpty().Max(x => x != null ? x.ID : "1") ?? "1") + adder;
                }
                else
                {
                    cislo = 1;
                }
                newID = "FA";   //prefix
                newID = newID + cislo.ToString("D8");
                ++adder;

            } while (dbset.FirstOrDefault(d => d.ID == newID) != null);
            return  newID;
        }

    }
}
