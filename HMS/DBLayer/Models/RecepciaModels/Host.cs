using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace DBLayer.Models
{
    public partial class Host : ICloneable
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

        private string _address = "";
        [Column(TypeName = "nvarchar(max)")]
        public string Address
        {
            get => DataEncryptor.Unprotect(_address);
            set => _address = DataEncryptor.Protect(value);
        }

        private string _birthNumber = "";
        [Column(TypeName = "nvarchar(256)")]
        public string BirthNumber
        {
            get => DataEncryptor.Unprotect(_birthNumber);
            set => _birthNumber = DataEncryptor.Protect(value);
        }

        private string _passport = "";
        [Column(TypeName = "nvarchar(256)")]
        public string Passport
        {
            get => DataEncryptor.Unprotect(_passport);
            set => _passport = DataEncryptor.Protect(value);
        }

        private string _citizenID = "";
        [Column(TypeName = "nvarchar(256)")]
        public string CitizenID
        {
            get => DataEncryptor.Unprotect(_citizenID);
            set => _citizenID = DataEncryptor.Protect(value);
        }
        /// <summary>
        /// false - male, true - female
        /// </summary>
        [BooleanStringValues("Muž", "Žena")]
        public bool Sex { get; set; } = false;

        [Column(TypeName = "nvarchar(128)")]
        public string Nationality { get; set; } = "";

        [Column(TypeName = "nvarchar(256)")]
        public string Note { get; set; } = "";

        public DateTime BirthDate { get; set; } = DateTime.Today;


        [IsForeignKeyUserWebNull]
        public string? Guest { get; set; }
        [NotMapped]
        public IdentityUserWebOwn? GuestZ { get; set; }

        [NotMapped]
        public PokladnicnyDoklad? PokladnicnyDokladZ { get; set; }

        public object Clone()
        {
            var newHost = new Host()
            {
                ID = this.ID,
                Name = this.Name,
                Surname = this.Surname,
                Address = this.Address,
                BirthNumber = this.BirthNumber,
                Passport = this.Passport,
                CitizenID = this.CitizenID,
                BirthDate = this.BirthDate,
                Guest = this.Guest,
                Sex = this.Sex,
                Nationality = this.Nationality,
                Note = this.Note,
                PokladnicnyDokladZ = this.PokladnicnyDokladZ
                //GuestZ = this.GuestZ.
            };
            return newHost;
        }
        public Host Clon()
        {
            return (Host)Clone();
        }
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
