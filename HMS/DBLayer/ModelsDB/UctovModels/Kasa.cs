using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Kasa
    {
        /// <summary>
        /// povolenie pre čítanie kasy
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_R_KASA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.RCVeduci, RolesOwn.Recepcny, RolesOwn.Uctovnik };

        /// <summary>
        /// povolenie pre update kasy, pridavanie kas
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CRUD_KASA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel };


    }
}
