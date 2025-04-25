using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DBLayer.Models.HSKModels
{
    //[INotifyPropertyChanged]
    public partial class RoomInfo : ObservableValidator, ICloneable
    {
        [Key]
        public string ID_Room { get; set; }

        [ObservableProperty]
        private RoomStatus status = RoomStatus.NutnaKontrola;

        [ObservableProperty]
        private DateTime lastUpdate = DateTime.Now;

        [ObservableProperty]
        private string poznamka = "";

        /// <summary>
        /// Predajné DPH
        /// </summary>
        [ObservableProperty]
        [property: Column(TypeName = "decimal(9, 3)")]
        [DecimalNonNegative(ErrorMessage = "Len kladné hodnoty.")]
        private decimal dPH = 23;

        [ObservableProperty]
        [property: Column(TypeName = "decimal(9, 3)")]
        [DecimalNonNegative(ErrorMessage = "Len kladné hodnoty.")]
        private decimal ubytovaciPoplatok = 0;

        [NotMapped]
        public Room? RoomZ { get; set; }

        [NotMapped]
        public bool NeedKontrola => LastUpdate.Date.AddDays(2) < DateTime.Now.Date;

        public object Clone()
        {
            return new RoomInfo
            {
                ID_Room = ID_Room,
                Status = Status,
                LastUpdate = LastUpdate,
                Poznamka = Poznamka,
                RoomZ = RoomZ,
                DPH = DPH,
                UbytovaciPoplatok = UbytovaciPoplatok
            };
        }
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
