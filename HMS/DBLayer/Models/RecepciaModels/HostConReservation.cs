using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class HostConReservation
    {
        [ForeignKey("HostX")]
        public long Host { get; set; }
        public Host HostX { get; set; }
        [IsForeignKeyRezervation]
        public long Reservation { get; set; }       //foreign key, druha databaza/context
        [NotMapped]
        public Rezervation? ReservationZ { get; set; }
    }

    public class IsForeignKeyRezervationAttribute : ValidationAttribute      //vlastny atribut pre kontrolu existujucej rezervacie. Pomoc od AI
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Rezervačné ID je potrebné.");
            }

            if (!(value is long resId))
            {
                return new ValidationResult("Zlý formát pri kontrolovaní rezervačného ID.");
            }

            var dbContext = (DataContext)validationContext.GetService(typeof(DataContext));
            if (dbContext == null)
            {
                throw new InvalidOperationException("DataContext context neni dostopný.");
            }

            if (dbContext.Rezervations.Any(u => u.Id == resId)) //kontrola existencie
            {
                return ValidationResult.Success;
                
            }
            return new ValidationResult("Priradená rezervácia neexistuje.");
        }
    }
}
