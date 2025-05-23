﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace DBLayer.Models
{
    public abstract class UniConItemPoklDokladu : ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }


        public abstract string GetID();
        public abstract string GetTypeUni();
        public abstract string GetNameUni();
        public abstract decimal GetCenaUni();
        public abstract decimal GetDPHUni();

        public abstract object Clone();
        public abstract UniConItemPoklDokladu Clon();
        public abstract void SetFrom(UniConItemPoklDokladu item);
        public abstract bool JeItemOnlyOneTyp();



        /// <summary>
        /// povolene role pre zobrazenie poloziek dokladov
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_R_POLOZKY { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.RCVeduci, RolesOwn.Uctovnik, RolesOwn.Recepcny };

        /// <summary>
        /// povolene role pre upravu dokladov
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CRUD_POLOZKY { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.Nakupca, RolesOwn.RCVeduci };

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

        /// <summary>
        /// Spracuje item pokladničného dokladu, podľa osobitného spracovania
        /// </summary>
        /// <param name="itemPD"></param>
        /// <param name="db"></param>
        /// <param name="dbw"></param>
        /// <returns></returns>
        public static ValidationResult SpracujItemPD(ItemPokladDokladu itemPD, in DBContext db, in DataContext dbw)
        {
            var founded = db.UniConItemyPoklDokladu.FirstOrDefault(x => x.ID == itemPD.UniConItemPoklDokladu);
            if (founded == null)
            {
                return new ValidationResult($"Zvolená položka nemá spojenie. ({itemPD.UniConItemPoklDokladu})");
            }
            switch (founded)
            {
                case ReservationConItemPoklDokladu item:
                    var foundedRes = dbw.Rezervations
                        .FirstOrDefault(x => x.Id == item.Reservation);
                    if (founded == null)
                    {
                        return new ValidationResult($"Rezervácia sa nenašla. ({itemPD.UniConItemPoklDokladu})");
                    }
                    if (foundedRes.Status == ReservationStatus.SchvalenaNezaplatena.ToString())
                    {
                        foundedRes.Status = ReservationStatus.SchvalenaZaplatena.ToString();
                    }
                    else
                    {
                        return new ValidationResult($"Rezervácia nie je SchvalenaNezaplatena. ({foundedRes.Id})");
                    }
                    break;
                case PolozkaSkladuConItemPoklDokladu item:
                    var foundedItem = db.PolozkaSkladuMnozstva
                        .FirstOrDefault(x => x.ID == item.PolozkaSkladuMnozstva);
                    if (foundedItem == null)
                    {
                        return new ValidationResult($"Položka skladu sa nenašla. ({itemPD.UniConItemPoklDokladu})");
                    }
                    //if (Sklad.MoznoVydať(new List<PrijemkaPolozka> {
                    //    new PrijemkaPolozka { PolozkaSkladu = foundedItem.PolozkaSkladu, Mnozstvo = itemPD.Mnozstvo } },
                    //    foundedItem.SkladX,
                    //    SkladObdobie.GetActualObdobieFromSklad(foundedItem.SkladX, in db),
                    //    in db).Count != 0)
                    //{
                    //    return new ValidationResult($"Nemožno vydať viacej ako je na sklade. ({foundedItem.PolozkaSkladu})");
                    //}
                    itemPD.Obdobie = SkladObdobie.GetActualObdobieFromSklad(foundedItem.SkladX, in db);     //nastavenie aktualneho obdobia
                    break;


                default: return new ValidationResult($"Nie je naimplementovaná kontrola v UniCon. ({itemPD.UniConItemPoklDokladu})"); 
            }
            return ValidationResult.Success;
        }

        /// <summary>
        /// Vráti true ak by v pokladničnom doklade mala byť len jedna položka (podľa typu Uniconu)
        /// </summary>
        /// <param name="doklad"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool JeItemOnlyOnePD(PokladnicnyDoklad doklad, in DBContext db)
        {
            var itemsPD = db.ItemyPokladDokladu
                .Include(x => x.UniConItemPoklDokladuX)
                .Where(x => x.Skupina == doklad.ID)
                .ToList();
            if (itemsPD.Count == 1)
            {
                if (
                    itemsPD.FirstOrDefault().UniConItemPoklDokladuX is ReservationConItemPoklDokladu
                    )
                {
                    return true;
                }
            }
            return false;
        }



    }
}
