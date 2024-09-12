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
        [Key]
        public string ID { get; set; }

        [ForeignKey("DodavatelX")]
        public string Dodavatel { get; set; } = default!;
        public Dodavatel DodavatelX { get; set; }

        [ForeignKey("ActualUserX")]
        public string? ActualUser { get; set; } = default!;
        public IdentityUserOwn? ActualUserX { get; set; }
    }
}
