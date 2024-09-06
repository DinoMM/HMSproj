using DBLayer.Models.RecepciaModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class RoomConFlag
    {
        [IsForeignKeyRoom]
        public string Room { get; set; }
        [NotMapped]
        public Room? RoomZ { get; set; }
        [ForeignKey("RoomFlagX")]
        public string RoomFlag { get; set; }
        public RoomFlag RoomFlagX { get; set; }
    }
    public class IsForeignKeyRoomAttribute : ValidationAttribute      //vlastny atribut pre kontrolu existujucej rezervacie. Pomoc od AI
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Room ID je potrebné.");
            }

            if (!(value is string roomId))
            {
                return new ValidationResult("Zlý formát pri kontrolovaní room ID.");
            }

            var dbContext = (DataContext)validationContext.GetService(typeof(DataContext));
            if (dbContext == null)
            {
                throw new InvalidOperationException("DataContext context neni dostopný.");
            }

            if (dbContext.HRooms.Any(u => u.RoomNumber == roomId)) //kontrola existencie
            {
                return ValidationResult.Success;

            }
            return new ValidationResult("Priradený room neexistuje.");
        }
    }
}
