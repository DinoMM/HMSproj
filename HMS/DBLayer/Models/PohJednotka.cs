﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public interface PohJednotka
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
        public decimal DPH { get; set; }
        [NotMapped]
        public double CelkovaCena { get => (double)Mnozstvo * Cena; }
        [NotMapped]
        public double CenaDPH { get => (Cena * (double)(100 + DPH)) / 100; }
        [NotMapped]
        public double CelkovaCenaDPH { get => (double)Mnozstvo * CenaDPH; }

        public object Clone();


        public static IQueryable<PohJednotka> GetDbSet(Type typ, in DBContext db)
        {
            switch (typ)
            {
                case Type t when t == typeof(Prijemka):
                    return db.PrijemkyPolozky.Cast<PohJednotka>();
                case Type t when t == typeof(Vydajka):
                    return db.VydajkyPolozky.Cast<PohJednotka>();
                case Type t when t == typeof(PokladnicnyDoklad):
                    return db.ItemyPokladDokladu.Cast<PohJednotka>();
                default:
                    throw new ArgumentException("Neznama trieda PohJednotky");
            }
        }

        //public static string DajNoveID(DbSet<PohJednotka> dbset)
        //{
        //    int adder = 1;
        //    string newID;
        //    do
        //    {
        //        int cislo;
        //        if (dbset.Count() != 0)
        //        {
        //            cislo = int.Parse(dbset.DefaultIfEmpty().Max(x => x != null ? x.ID : "1") ?? "1") + adder;
        //        }
        //        else
        //        {
        //            cislo = 1;
        //        }
        //        //moznost pridat prefix
        //        newID = cislo.ToString("D9");
        //        ++adder;

        //    } while (dbset.FirstOrDefault(d => d.ID == newID) != null);
        //    return newID;
        //}
    }
}
