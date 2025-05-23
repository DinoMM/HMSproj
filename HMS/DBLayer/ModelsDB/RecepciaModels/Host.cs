﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DBLayer.Models
{
    public partial class Host
    {
        /// <summary>
        /// povolenie pre čítanie zoznamu hostí
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_R_HOSTIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel , RolesOwn.RCVeduci ,RolesOwn.Recepcny };
        /// <summary>
        /// povolenie pre upravu hostí
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CRU_HOSTIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.Recepcny, RolesOwn.RCVeduci };
        /// <summary>
        /// povolenie pre upravu hostí
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_D_HOSTIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.RCVeduci };
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

        public static List<PokladnicnyDoklad> GetActivePokladnicneDoklady(Host host, in DBContext db)
        {
            return db.PokladnicneDoklady
                .Include(x => x.KasaX)
                .Include(x => x.HostX)
                .Where(x => !x.Spracovana && x.Host == host.ID)
                .OrderByDescending(x => x.Vznik)
                .ToList();
        }

    }
}
