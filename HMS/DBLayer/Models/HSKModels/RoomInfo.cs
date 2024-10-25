using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models.HSKModels
{
    public class RoomInfo
    {
        [Key]
        public string ID_Room { get; set; }

        public RoomStatus Status { get; set; } = RoomStatus.NutnaKontrola;

        public DateTime LastUpdate { get; set; } = DateTime.Now;

        public string Poznamka { get; set; } = "";

        [NotMapped]
        public Room? RoomZ { get; set; }

        [NotMapped]
        public bool NeedKontrola => LastUpdate.Date.AddDays(2) < DateTime.Now.Date;
    }

    public enum RoomStatus
    {
        [Display(Name = "Uprataná")]
        Upratana,
        [Display(Name = "Neuprataná")]
        Neupratana,
        [Display(Name = "Nutná kontrola")]
        NutnaKontrola

    }
}
