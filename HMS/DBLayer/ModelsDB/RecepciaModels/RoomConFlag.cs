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
        public static List<ValidationResult> ValidateCon(in RoomConFlag con)
        {

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(con, null, null);

            Validator.TryValidateObject(con, validationContext, validationResults, true);
            return validationResults;
        }
        public static bool ValidateConReservation(in RoomConFlag host)
        {
            var valRes = ValidateCon(in host);
            return !valRes.Any(x => x.ErrorMessage == "Room ID je potrebné." || x.ErrorMessage == "Zlý formát pri kontrolovaní room ID." || x.ErrorMessage == "Priradený room neexistuje.");
        }
    }
}
