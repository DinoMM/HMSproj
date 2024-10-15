using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class PokladnicnyDoklad : PohSkup
    {
        /// <summary>
        /// povolenie pre čítanie dokladov
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_R_POKLDOKL { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.RCVeduci, RolesOwn.Recepcny, RolesOwn.Uctovnik };

        /// <summary>
        /// povolenie pre update dokladov bez delete
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CRU_POKLDOKL { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.RCVeduci, RolesOwn.Recepcny};

        /// <summary>
        /// povolenie pre update dokladov
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_D_POKLDOKL { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.RCVeduci };

        /// <summary>
        /// Prejde v databazi itemy poklad dokladu a ak je prazdny vymaze ho -> true
        /// </summary>
        /// <param name="doklad"></param>
        /// <param name="db"></param>
        public static bool VymazAkPrazdnyZoznamPoloziek(PokladnicnyDoklad doklad, in DBContext db)
        {
            if (!string.IsNullOrEmpty(doklad.ID) && !doklad.Spracovana)
            {
                if (db.ItemyPokladDokladu.FirstOrDefault(x => x.Skupina == doklad.ID) == null)
                {
                    var found = db.PokladnicneDoklady.FirstOrDefault(x => x.ID == doklad.ID);
                    if (found != null)
                    {
                        db.PokladnicneDoklady.Remove(found);
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
