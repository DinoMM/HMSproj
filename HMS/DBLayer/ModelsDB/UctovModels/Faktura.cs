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
    public partial class Faktura
    {
        /// <summary>
        /// Role na zobrazenie faktur
        /// </summary>
        public static List<RolesOwn> ROLE_R_FAKTURY { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Uctovnik, RolesOwn.Riaditel };
        /// <summary>
        /// Role na menenie faktur
        /// </summary>
        public static List<RolesOwn> ROLE_CRUD_FAKTURY { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Uctovnik, RolesOwn.Riaditel };


        public static string DajNoveID(DbSet<Faktura> dbset)
        {
            int adder = 1;
            string newID = "";
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
                //newID = "FA";   //prefix // nepouzivat
                newID = newID + cislo.ToString("D8");
                ++adder;

            } while (dbset.FirstOrDefault(d => d.ID == newID) != null);
            return  newID;
        }

    }
}
