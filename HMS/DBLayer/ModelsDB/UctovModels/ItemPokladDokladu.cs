using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DBLayer.Models
{
    public partial class ItemPokladDokladu : PohJednotka
    {
        /// <summary>
        /// Najde prvy pokladnicny doklad v itemoch pokladnicneho dokladu, PREDPOKLADA sa ze tento con moze byt len jeden na jeden pokladnicny doklad (nie viac PD)
        /// </summary>
        /// <param name="con"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static PokladnicnyDoklad? GetPD_UniCon_Unique(UniConItemPoklDokladu con, in DBContext db)
        {
            var found = db.ItemyPokladDokladu.Include(x => x.SkupinaX).FirstOrDefault(x => x.UniConItemPoklDokladu == con.ID
            && x.SkupinaX is PokladnicnyDoklad
            );

            if (found != null)
            {
                return (PokladnicnyDoklad)found.SkupinaX;
            }
            return null;
        }


        /// <summary>
        /// Vytvori novu instanciu bez ID. Inicializuje na hodnoty z UniConu. FK kluce treba nastavit manualne pri ukladani do DB
        /// </summary>
        /// <param name="con">Con, z ktorého sa podľa jeho typu nahodia údaje do novej inštancie</param>
        /// <param name="dbw">Pre hladanie poloziek z druhej databaze, ktore niesu nacitane</param>
        /// <returns></returns>
        public static ItemPokladDokladu? CreateInstanceFromUniCon(UniConItemPoklDokladu con, PohSkup Skupina, in DataContext dbw)
        {
            var newItemPD = new ItemPokladDokladu();
            switch (con)
            {
                case ReservationConItemPoklDokladu item:
                    var founded = dbw.Rezervations
                        .Include(x => x.Room)
                        .Include(x => x.Coupon)
                        .FirstOrDefault(x => x.Id == item.Reservation);     //bez instancie guest
                    if (founded == null)
                    {
                        return null;
                    }
                    item.ReservationZ = founded;
                    //newItemPD.ID
                    newItemPD.Cena = (double)item.ReservationZ.CelkovaSuma;
                    newItemPD.Mnozstvo = 1;
                    newItemPD.Skupina = Skupina.ID;
                    newItemPD.SkupinaX = Skupina;
                    newItemPD.UniConItemPoklDokladu = item.ID;
                    newItemPD.UniConItemPoklDokladuX = item;
                    newItemPD.Nazov = item.GetNameUni();
                    newItemPD.DPH = item.ReservationZ.DPH;
                    break;

                default: return null;
            }
            return newItemPD;
        }
    }
}
