using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBLayer
{
    public class IdentityUserWebOwn : IdentityUser
    {
        [ProtectedPersonalData]
        [Column(TypeName = "nvarchar(64)")]
        public string Name { get; set; }

        [ProtectedPersonalData]
        [Column(TypeName = "nvarchar(64)")]
        public string Surname { get; set; }

    }
}
