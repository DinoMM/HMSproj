using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBLayer.Models
{
    public partial class Rezervation : ICloneable
    {
        [Key]
        public long Id { get; set; }                    //id rezervacie
                                                        //public int RoomNumber { get; set; }
        public DateTime ArrivalDate { get; set; } = DateTime.Today;

        [DateNotSoonerThan("ArrivalDate", ErrorMessage = "Dátum odchodu nesmie byť skorej ako dátum príchodu.")]
        public DateTime DepartureDate { get; set; } = DateTime.Today.AddDays(1);

        [Range(0, int.MaxValue, ErrorMessage = "Len kladné hodnoty.")]
        public int NumberGuest { get; set; }            //počet hosti

        [Range(0, int.MaxValue, ErrorMessage = "Len kladné hodnoty.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CelkovaSuma { get; set; }        //celkova suma, ktora sa ma vypočitat pri vytvoreni rezervacie

        [Column(TypeName = "nvarchar(450)")]
        [ForeignKey("Guest")]
        public string? GuestId { get; set; }             //id Usera

        [ForeignKey("Room")]
        [RoomExist(ErrorMessage = "Izba neexistuje")]
        [IsRoomFree("Id", "ArrivalDate", "DepartureDate", ErrorMessage = "Táto izba je obsadená v tomto období.")]
        public string RoomNumber { get; set; }                  //id miestnosti

        [ForeignKey("Coupon")]
        public string? CouponId { get; set; }

        public IdentityUserWebOwn? Guest { get; set; }
        public Room Room { get; set; }

        public Coupon? Coupon { get; set; }

        public string Status { get; set; } = ReservationStatus.VytvorenaRucne.ToString();

        public string? RecentChangesUser { get; set; }

        [NotMapped]
        public IdentityUserOwn? RecentChangesUserZ { get; set; }

        [NotMapped]
        [Range(0.0, double.MaxValue, ErrorMessage = "Len kladné hodnoty.")]
        public double UbytovaciPoplatok { get; set; } = 0.0;
        [NotMapped]
        public double CelkovaSumaDPH { get => (double)(CelkovaSuma * (100 + DPH)) / 100; }

        /// <summary>
        /// [NotMapped]
        /// </summary>
        [NotMapped]
        [DecimalNonNegative(ErrorMessage = "Len kladné hodnoty.")]
        public decimal DPH { get; set; } = 23;


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
            Status = res.Status;
            CouponId = res.CouponId;
            Coupon = res.Coupon;
            RecentChangesUser = res.RecentChangesUser;
            RecentChangesUserZ = res.RecentChangesUserZ;
            DPH = res.DPH;
            UbytovaciPoplatok = res.UbytovaciPoplatok;
        }

        public object Clone()
        {
            Rezervation clone = new Rezervation();
            clone.Id = this.Id;
            clone.ArrivalDate = this.ArrivalDate;
            clone.DepartureDate = this.DepartureDate;
            clone.NumberGuest = this.NumberGuest;
            clone.CelkovaSuma = this.CelkovaSuma;
            clone.GuestId = this.GuestId;
            clone.RoomNumber = this.RoomNumber;
            clone.CouponId = this.CouponId;
            clone.Guest = this.Guest;
            clone.Room = this.Room;
            clone.Coupon = this.Coupon;
            clone.Status = this.Status;
            clone.RecentChangesUser = this.RecentChangesUser;
            clone.RecentChangesUserZ = this.RecentChangesUserZ;
            clone.DPH = this.DPH;
            clone.UbytovaciPoplatok = this.UbytovaciPoplatok;
            return clone;
        }
        public Rezervation Clon()
        {
            return (Rezervation)Clone();
        }

        public string GetRecentChangedUserName()
        {
            return RecentChangesUserZ != null ? RecentChangesUserZ.Name + " " + RecentChangesUserZ.Surname : "-";
        }

        public HSKStatus GetHSKStatus(DateTime date)
        {
            if (Status == ReservationStatus.Blokovana.ToString())
            {
                return HSKStatus.Blokovana;
            }
            if (Status != ReservationStatus.Checked_IN.ToString() && Status != ReservationStatus.Checked_OUT.ToString() && ArrivalDate.Date == date.Date)
            {
                return HSKStatus.Prichod;
            }
            if (Status == ReservationStatus.Checked_IN.ToString() && DepartureDate.Date == date.Date)
            {
                return HSKStatus.Odchod;
            }
            if (Status == ReservationStatus.Checked_IN.ToString())
            {
                return HSKStatus.Obsadena;
            }
            return HSKStatus.Neobsadena;
        }
    }
    public enum ReservationStatus
    {
        VytvorenaRucne,
        VytvorenaWeb,
        SchvalenaNezaplatena,
        SchvalenaZaplatena,
        Stornovana,
        Blokovana,
        Checked_IN,
        Checked_OUT
    }

    public enum HSKStatus
    {
        [Display(Name = "Neobsadená")]
        Neobsadena,
        [Display(Name = "Príchod")]
        Prichod,
        [Display(Name = "Odchod")]
        Odchod,
        [Display(Name = "Obsadená")]
        Obsadena,
        [Display(Name = "Blokovaná")]
        Blokovana
    }

    public class DateNotSoonerThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateNotSoonerThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)  //pomoc od AI
        {
            var ContextMemberNames = new List<string>() { validationContext.MemberName ?? "" };
            var currentValue = (DateTime?)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
            {
                return new ValidationResult($"Unknown property: {_comparisonProperty}", ContextMemberNames);
            }

            var comparisonValue = (DateTime?)property.GetValue(validationContext.ObjectInstance);

            if (currentValue.HasValue && comparisonValue.HasValue && currentValue < comparisonValue)
            {
                return new ValidationResult(ErrorMessage ?? $"The date cannot be sooner than {_comparisonProperty}.", ContextMemberNames);
            }

            return ValidationResult.Success;
        }
    }

    public class RoomExistAttribute : ValidationAttribute      //vlastny atribut pre kontrolu volnej izby. Pomoc od AI
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var ContextMemberNames = new List<string>() { validationContext.MemberName ?? "" };
            if (value == null)
            {
                return new ValidationResult("Povinné pole.", ContextMemberNames);
            }

            if (!(value is string roomID))
            {
                return new ValidationResult("Zlý formát pri kontrolovaní rezervačného ID.", ContextMemberNames);
            }

            var dbContext = (DataContext)validationContext.GetService(typeof(DataContext));
            if (dbContext == null)
            {
                throw new InvalidOperationException("DataContext context neni dostupný.");
            }
            #region kontrola existencie izby
            if (!dbContext.HRooms.Any(u => u.RoomNumber == roomID)) //kontrola existencie
            {
                return new ValidationResult("Priradená izba neexistuje.", ContextMemberNames);
            }
            #endregion
            return ValidationResult.Success;
        }
    }

    public class IsRoomFreeAttribute : ValidationAttribute      //vlastny atribut pre kontrolu volnej izby. Pomoc od AI
    {
        private readonly string _datePropertyStart;
        private readonly string _datePropertyEnd;
        private readonly string _keyproperty;

        public IsRoomFreeAttribute(string keyProperty, string datePropertyStart, string datePropertyEnd)
        {
            _keyproperty = keyProperty;
            _datePropertyStart = datePropertyStart;
            _datePropertyEnd = datePropertyEnd;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var ContextMemberNames = new List<string>() { validationContext.MemberName ?? "" };
            if (value == null)
            {
                return new ValidationResult("Povinné pole.");
            }

            if (!(value is string roomID))
            {
                return new ValidationResult("Zlý formát pri kontrolovaní ID izby .", ContextMemberNames);
            }
            #region ziskanie datumov
            var property = validationContext.ObjectType.GetProperty(_datePropertyStart);

            if (property == null)
            {
                return new ValidationResult($"Unknown property: {_datePropertyStart}", ContextMemberNames);
            }
            var property2 = validationContext.ObjectType.GetProperty(_datePropertyEnd);

            if (property2 == null)
            {
                return new ValidationResult($"Unknown property: {_datePropertyEnd}", ContextMemberNames);
            }
            var property3 = validationContext.ObjectType.GetProperty(_keyproperty);

            if (property3 == null)
            {
                return new ValidationResult($"Unknown property: {_keyproperty}", ContextMemberNames);
            }

            var dateValueStart = (DateTime?)property.GetValue(validationContext.ObjectInstance);
            var dateValueEnd = (DateTime?)property2.GetValue(validationContext.ObjectInstance);
            var keyID = (long?)property3.GetValue(validationContext.ObjectInstance);
            #endregion

            var dbContext = (DataContext)validationContext.GetService(typeof(DataContext));
            if (dbContext == null)
            {
                throw new InvalidOperationException("DataContext context neni dostupný.");
            }
            #region kontrola volnej izby
            if (!dateValueStart.HasValue || !dateValueEnd.HasValue)
            {
                return new ValidationResult(ErrorMessage ?? $"Null.", ContextMemberNames);
            }
            //if (dbContext.Rezervations.Any(x =>
            // x.RoomNumber == roomID
            // && x.Id != keyID
            // && (x.ArrivalDate < dateValueEnd && dateValueStart < x.DepartureDate)
            // && x.Status != ReservationStatus.Stornovana.ToString()
            if (dbContext.Rezervations.Any(x =>
             x.RoomNumber == roomID
             && x.Id != keyID
             && (x.ArrivalDate < dateValueEnd && dateValueStart < x.DepartureDate)
             && x.Status != ReservationStatus.Stornovana.ToString()
             || (
                x.RoomNumber == roomID
                && x.Id != keyID
                && x.Status == ReservationStatus.Blokovana.ToString()
                && (x.ArrivalDate == dateValueEnd || dateValueStart == x.DepartureDate))
             ))
            {
                return new ValidationResult(ErrorMessage ?? $"V danom čase je táto izba obsadená.", ContextMemberNames);
            }
            //if (dbContext.Rezervations.Any(x =>
            // x.RoomNumber == roomID
            // && x.Id != keyID
            // && x.Status == ReservationStatus.Blokovana.ToString()
            // && (x.ArrivalDate == dateValueEnd || dateValueStart == x.DepartureDate)
            // ))
            //{
            //    return new ValidationResult( $"V danom čase je táto izba blokovana.", ContextMemberNames);
            //}
            #endregion

            return ValidationResult.Success;
        }
    }

}
