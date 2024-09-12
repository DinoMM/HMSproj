using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public class ReservationConItemPoklDokladu : UniConItemPoklDokladu
    {
        [IsForeignKeyRezervation]
        public long Reservation { get; set; }

        [NotMapped]
        public Rezervation ReservationZ { get; set; }
    }
}
