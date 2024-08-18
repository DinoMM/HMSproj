using DBLayer.Migrations;
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
       

        public static string DajNoveID(DBContext db)
        {
            int adder = 1;
            string newID;
            do
            {
                int cislo;
                if (db.PolozkySkladu.Count() != 0)
                {
                    cislo = int.Parse(db.PolozkySkladu.DefaultIfEmpty().Max(x => x != null ? x.ID : "1") ?? "1") + adder;
                }
                else
                {
                    cislo = 1;
                }
                //moznost pridat prefix
                newID = cislo.ToString("D7");
                ++adder;

            } while (db.PolozkySkladu.FirstOrDefault(d => d.ID == newID) != null);
            return newID;
        }
    }
}
