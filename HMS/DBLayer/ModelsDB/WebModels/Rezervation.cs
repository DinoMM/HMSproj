using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBLayer.Models
{
    public partial class Rezervation
    {
        /// <summary>
        /// povolene role, pre zobrazovanie rezervácií 
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_R_REZERVACIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.UdalostnyPlanovac, RolesOwn.HKVeduci, RolesOwn.FBVeduci, RolesOwn.Recepcny, RolesOwn.RCVeduci };

        /// <summary>
        /// povolene role, pre menenie rezervácií 
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CRUD_REZERVACIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.Recepcny, RolesOwn.RCVeduci };

        /// <summary>
        /// povolene role, pre menenie rezervácií 
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_SPECIAL_REZERVACIA { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.RCVeduci };

        /// <summary>
        /// povolene role, pre zobrazovanie housekeepingu 
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_R_HSK { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.UdalostnyPlanovac, RolesOwn.HKVeduci, RolesOwn.Udrzbar};

        /// <summary>
        /// povolene role, pre správu housekeepingu 
        /// </summary>
        [NotMapped]
        public static List<RolesOwn> ROLE_CUD_HSK { get; private set; } = new() { RolesOwn.Admin, RolesOwn.Riaditel, RolesOwn.HKVeduci};


        /// <summary>
        /// Vrati IdentityUserOwn z rezervacie (naposledy upravil) alebo null
        /// </summary>
        /// <param name="res"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IdentityUserOwn? GetRecentChangedUser(Rezervation res, in DBContext db)
        {
            return db.Users.FirstOrDefault(x => x.Id == res.RecentChangesUser);
        }
    }


}
