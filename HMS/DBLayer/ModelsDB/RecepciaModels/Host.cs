using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Host
    {
        /// <summary>
        /// povolenie pre čítanie zoznamu hostí
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_R_HOSTIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel ,RolesOwn.Recepcny };
        /// <summary>
        /// povolenie pre upravu hostí
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CRUD_HOSTIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.Recepcny };
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
