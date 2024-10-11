using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Kasa : ICloneable
    {
        [Key]
        [Required(ErrorMessage = "Povinné pole.")]
        public string ID { get; set; }

        [ForeignKey("DodavatelX")]
        [Required(ErrorMessage = "Povinné pole.")]
        public string Dodavatel { get; set; } = default!;
        public Dodavatel DodavatelX { get; set; }

        [ForeignKey("ActualUserX")]
        public string? ActualUser { get; set; } = default!;
        public IdentityUserOwn? ActualUserX { get; set; }

        [HideFromInput]
        [Column(TypeName = "decimal(12, 2)")]
        public double HotovostStav { get; set; } = 0;


        public object Clone()
        {
            return new Kasa
            {
                ID = this.ID,
                Dodavatel = this.Dodavatel,
                DodavatelX = this.DodavatelX,
                ActualUser = this.ActualUser,
                ActualUserX = this.ActualUserX
            };
        }

        public Kasa Clon()
        {
            return (Kasa)Clone();
        }

        public void SetFrom(Kasa other)
        {
            this.ID = other.ID;
            this.Dodavatel = other.Dodavatel;
            this.DodavatelX = other.DodavatelX;
            this.ActualUser = other.ActualUser;
            this.ActualUserX = other.ActualUserX;
        }
    }
}
