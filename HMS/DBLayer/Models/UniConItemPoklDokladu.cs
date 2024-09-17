using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DBLayer.Models
{
    public abstract class UniConItemPoklDokladu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }



        /// <summary>
        /// Ak existuje v UniCone, tak vrati inštanciu, inak vytvorí novú a AJ uloží do DB
        /// </summary>
        /// <param name="ObjectD">Tento objekt musí byť v databázy uložený a byť jedným z dedených typov UniCon</param>
        /// <param name="db"></param>
        /// <param name="dbw"></param>
        /// <returns></returns>
        public static UniConItemPoklDokladu? EnsureCreated(object ObjectUni, in DBContext db, in DataContext dbw)
        {
            UniConItemPoklDokladu? con = null;
            try
            {
                switch (ObjectUni)
                {
                    case Rezervation item:
                        con = db.ReservationConItemyPoklDokladu.SingleOrDefault(x => x.Reservation == item.Id);
                        if (con == null)    // ak neexistuje con tak vytvorime novy
                        {
                            var newCon = new ReservationConItemPoklDokladu
                            {
                                //ID
                                Reservation = item.Id,
                                ReservationZ = item
                            };
                            db.ReservationConItemyPoklDokladu.Add(newCon);
                            db.SaveChanges();
                            con = newCon;
                        }
                        break;
                    default: return null;
                }
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine($"Našlo sa viacero unikátnych spojení v UniConItemPoklDokladu: {e.Message}");
                return null;
            }
            return con;
        }

        public static bool SpracujItemPD(ItemPokladDokladu itemPD, in DBContext db, in DataContext dbw)
        {
            var founded = db.UniConItemyPoklDokladu.FirstOrDefault(x => x.ID == itemPD.UniConItemPoklDokladu);
            if (founded == null)
            {
                return false;
            }
            switch (founded)
            {
                case ReservationConItemPoklDokladu item:
                    var foundedRes = dbw.Rezervations
                        .FirstOrDefault(x => x.Id == item.Reservation);
                    if (founded == null)
                    {
                        return false;
                    }
                    if (foundedRes.Status == ReservationStatus.SchvalenaNezaplatena.ToString())
                    {
                        foundedRes.Status = ReservationStatus.SchvalenaZaplatena.ToString();
                    }
                    else {
                        return false;
                    }
                    break;
                default: return true;
            }
            return true;
        }
    }
}
