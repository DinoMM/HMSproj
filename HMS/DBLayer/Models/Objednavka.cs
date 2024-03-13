using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class Objednavka : ICloneable
    {
        [Key]
        public string ID { get; set; } = default!;

        [ForeignKey("DodavatelX")]
        public string Dodavatel { get; set; } = default!;
        public Dodavatel DodavatelX { get; set; }
        [ForeignKey("OdberatelX")]
        public string Odberatel { get; set; } = default!;
        public Dodavatel OdberatelX { get; set; }

        public string? Popis { get; set; }
        public DateTime DatumVznik { get; set; } = DateTime.Today;





        public object Clone()
        {
            var clon = new Objednavka();
            clon.ID = ID;
            clon.Dodavatel = Dodavatel;
            clon.Odberatel = Odberatel;
            clon.Popis = Popis;
            clon.DatumVznik = DatumVznik;

            clon.DodavatelX = DodavatelX;
            clon.OdberatelX = OdberatelX;
            return clon;
        }
        public Objednavka Clon()
        {
            return (Objednavka)Clone();
        }

        public static string DajNoveID(DBContext db)
        {
            int adder = 1;
            string newID;
            do
            {
                int cislo;
                if (db.Objednavky.Count() != 0)
                {
                    cislo = int.Parse(db.Objednavky.DefaultIfEmpty().Max(x => x != null ? x.ID : "1") ?? "1") + adder;
                }
                else {
                    cislo = 1;
                }
                //moznost pridat prefix
                newID = cislo.ToString("D7");
                ++adder;

            } while (db.Objednavky.FirstOrDefault(d => d.ID == newID) != null);
            return newID;
        }

        public static bool JeObjednavkaOK(Objednavka obj, bool ajID = true)
        {
            if (string.IsNullOrEmpty(obj.ID) && ajID)
            {
                return false;
            }
            if (string.IsNullOrEmpty(obj.Dodavatel) ||
                string.IsNullOrEmpty(obj.Dodavatel))
            {
                return false;
            }
            return true;

            //DodavatelX;
            //OdberatelX;
        }
    }
}
