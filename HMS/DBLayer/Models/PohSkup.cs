using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public abstract class PohSkup
    {
        [Key]
        public string ID { get; set; } = default!;
        public DateTime Vznik { get; set; } = DateTime.Today;
        public string? Poznamka { get; set; } = default!;
        public bool Spracovana { get; set; } = false;


        public static string DajNoveID(DbSet<PohSkup> dbset)
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
                //moznost pridat prefix
                newID = cislo.ToString("D9");
                ++adder;

            } while (dbset.FirstOrDefault(d => d.ID == newID) != null);
            return newID;
        }
    }
}
