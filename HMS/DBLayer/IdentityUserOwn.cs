using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer
{
    
     public class IdentityUserOwn : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(64)")]
        public string Name { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(64)")]
        public string Surname { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(64)")]
        public string? IBAN { get; set; } = default!;

        [PersonalData]
        [Column(TypeName = "nvarchar(64)")]
        public string? Adresa { get; set; } = default!;
    }
}
