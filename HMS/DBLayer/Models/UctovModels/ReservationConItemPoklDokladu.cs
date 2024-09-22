using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class ReservationConItemPoklDokladu : UniConItemPoklDokladu
    {
        [IsForeignKeyRezervation]
        public long Reservation { get; set; }

        [NotMapped]
        public Rezervation ReservationZ { get; set; }


        public override string GetID()
        {
            return Reservation.ToString();
        }

        public override string GetTypeUni()
        {
            return "Rezervácia";
        }

        public override string GetNameUni()
        {
            return $"Rezervacia {ReservationZ.Room.RoomCategory}" + (ReservationZ.Coupon != null ? $", Zľava {ReservationZ.Coupon.Discount}%" : "");
        }

        public override decimal GetCenaUni()
        {
            return ReservationZ.CelkovaSuma;
        }

        public override object Clone()
        {
            var clone = new ReservationConItemPoklDokladu
            {
                ID = this.ID,
                Reservation = this.Reservation,
                ReservationZ = this.ReservationZ
            };
            return clone;
        }

        public override UniConItemPoklDokladu Clon()
        {
            return (ReservationConItemPoklDokladu)Clone();
        }

        public override void SetFrom(UniConItemPoklDokladu item)
        {
            if (item is ReservationConItemPoklDokladu ytem)
            {
                this.ID = ytem.ID;
                this.Reservation = ytem.Reservation;
                this.ReservationZ = ytem.ReservationZ;
            }
            else
            {
                throw new InvalidCastException("Nemožno skopírovat z ineho typu ako ReservationConItemPoklDokladu");
            }
        }

        public override bool JeItemOnlyOneTyp()
        {
            return true;
        }

        public override decimal GetDPHUni()
        {
            return 10;
        }
    }
}
