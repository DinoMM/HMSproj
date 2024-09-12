using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class HostConReservation
    {
        [ForeignKey("HostX")]
        public long Host { get; set; }
        public Host HostX { get; set; }
        [IsForeignKeyRezervation]
        public long Reservation { get; set; }       //foreign key, druha databaza/context
        [NotMapped]
        public Rezervation? ReservationZ { get; set; }
    }

   
}
