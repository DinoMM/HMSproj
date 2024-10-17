using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class ZmenaMeny
    {
        /// <summary>
        /// povolenie pre mazanie transakcii
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CR_ZMENAREN { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.RCVeduci, RolesOwn.Uctovnik, RolesOwn.Recepcny };
        /// <summary>
        /// povolenie pre mazanie transakcii
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_D_ZMENAREN { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.RCVeduci, RolesOwn.Uctovnik };
    }
}
