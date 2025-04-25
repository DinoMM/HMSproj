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
        public DateTime Vznik { get; set; } = DateTime.Now;
        public string? Poznamka { get; set; } = default!;
        public bool Spracovana { get; set; } = false;


        public static string DajNoveID<T>(DbSet<T> dbset, DBContext db) where T : PohSkup
        {
            string newID;
            int cislo;
            if (dbset.Count() != 0)
            {
                cislo = int.Parse(dbset.DefaultIfEmpty().Max(x => x != null ? x.ID : "1") ?? "1") + 1;
            }
            else
            {
                cislo = 1;
            }

            do
            {
                newID = cislo.ToString("D9");
                ++cislo;

            } while (db.Find<PohSkup>(newID) != null);
            return newID;
        }

        public virtual string GetDisplayName()
        {
            return this.GetType().Name;
        }
    }
}
