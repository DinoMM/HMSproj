using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Host
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        [Required(ErrorMessage = "Povinné pole.")]
        [Column(TypeName = "nvarchar(128)")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Povinné pole.")]
        [Column(TypeName = "nvarchar(128)")]
        public string Surname { get; set; } = "";

        [Column(TypeName = "nvarchar(192)")]
        public string Address { get; set; } = "";

        [Column(TypeName = "nvarchar(32)")]
        public string BirthNumber { get; set; } = "";

        [Column(TypeName = "nvarchar(32)")]
        public string Passport { get; set; } = "";

        [Column(TypeName = "nvarchar(32)")]
        public string CitizenID { get; set; } = "";

        public DateTime BirthDate { get; set; } = DateTime.Today;


        [IsForeignKeyUserWebNull]
        public string? Guest { get; set; }
        [NotMapped]
        public IdentityUserWebOwn? GuestX { get; set; }

    }
    public class IsForeignKeyUserWebNullAttribute : ValidationAttribute      //vlastny atribut pre kontrolu existujucej rezervacie. Pomoc od AI
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)  //moze byt prazdny ako null
            {
                return ValidationResult.Success;
            }

            if (!(value is string guestId))
            {
                return new ValidationResult("Zlý formát pri kontrolovaní Guest ID.");
            }

            var dbContext = (DataContext)validationContext.GetService(typeof(DataContext));
            if (dbContext == null)
            {
                throw new InvalidOperationException("DataContext context neni dostopný.");
            }
            if (string.IsNullOrEmpty(guestId))  //moze byt prazdny ako null
            {
                return ValidationResult.Success;
            }

            if (dbContext.Users.Any(u => u.Id == guestId)) //kontrola existencie
            {
                return ValidationResult.Success;

            }
            return new ValidationResult("Priradený guest neexistuje.");
        }
    }
}
