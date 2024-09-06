using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class HostConReservation
    {
        public static List<ValidationResult> ValidateCon(in HostConReservation con)
        {

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(con, null, null);

            Validator.TryValidateObject(con, validationContext, validationResults, true);
            return validationResults;
        }
        public static bool ValidateConReservation(in HostConReservation host)
        {
            var valRes = ValidateCon(in host);
            return !valRes.Any(x => x.ErrorMessage == "Rezervačné ID je potrebné." || x.ErrorMessage == "Zlý formát pri kontrolovaní rezervačného ID." || x.ErrorMessage == "Priradená rezervácia neexistuje.");
        }
    }
}
