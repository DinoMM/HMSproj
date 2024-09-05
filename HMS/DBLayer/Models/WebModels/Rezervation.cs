using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBLayer.Models
{
    public partial class Rezervation
    {
        [Key]
        public long Id { get; set; }                    //id rezervacie
        //public int RoomNumber { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public int NumberGuest { get; set; }            //počet hosti
        public decimal CelkovaSuma { get; set; }        //celkova suma, ktora sa ma vypočitat pri vytvoreni rezervacie

        [Column(TypeName = "nvarchar(450)")]
        public string GuestId { get; set; }             //id Usera

        [ForeignKey("Room")]
        public string RoomNumber { get; set; }                  //id miestnosti

        public string? CouponId { get; set; }

        public IdentityUserWebOwn Guest { get; set; }
        
        public Room Room { get; set; }

        public Coupon? Coupon { get; set; }

        public string Status { get; set; } = ReservationStatus.VytvorenaRucne.ToString();

        public void setFromReservation(Rezervation res)
        {
            Id = res.Id;
            RoomNumber = res.RoomNumber;
            Room = res.Room;
            CelkovaSuma = res.CelkovaSuma;
            ArrivalDate = res.ArrivalDate;
            DepartureDate = res.DepartureDate;
            NumberGuest = res.NumberGuest;
            GuestId = res.GuestId;
            Guest = res.Guest;
        }


    }
    public enum ReservationStatus
    {
        VytvorenaRucne,
        VytvorenaWeb,
        SchvalenaNezaplatena,
        SchvalenaZaplatena,
        Stornovana

    }


}
