using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Host
    {
        public static List<ValidationResult> ValidateHost(in Host host) {

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(host, null, null);

            Validator.TryValidateObject(host, validationContext, validationResults, true);
            return validationResults;
        }
        public static bool ValidateHostGuest(in Host host)
        {
            var valRes =  ValidateHost(in host);
            return !valRes.Any(x => x.ErrorMessage == "Zlý formát pri kontrolovaní Guest ID." || x.ErrorMessage == "Priradený guest neexistuje.");
        }
    }
}
