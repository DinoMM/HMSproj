using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Dodavatel
    {
        /// <summary>
        /// povolene role pre zobrazenie dodavatelov
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_R_DODAVATELIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.Nakupca, RolesOwn.Skladnik };

        /// <summary>
        /// povolene role pre upravu dodavatelov
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CRUD_DODAVATELIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Nakupca };

    }
}
