﻿using DBLayer.Models.HSKModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DBLayer.Models
{
    public partial class Room : ICloneable
    {

        [Key]
        [Required(ErrorMessage = "Nutné pole")]
        public string RoomNumber { get; set; }        //id miestnosti
        public string RoomCategory { get; set; }        //kategoria

        [Range(0.0, int.MaxValue, ErrorMessage = "Len kladné hodnoty.")]
        public int MaxNumberOfGuest { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Len kladné hodnoty.")]
        [Column(TypeName = "decimal(18, 2)")]
        public double Cost { get; set; }
        //[NotMapped]
        //public string[] RoomIds { get; set; }
        //[NotMapped]
        //public string[] Describe { get; set; }
        //[NotMapped]
        //public string[] Furnishing { get; set; }
        //[NotMapped]
        //public string[] Bathroom { get; set; }
        //[NotMapped]
        //public string[] Services { get; set; }
        //[NotMapped]
        //public string[] Photos { get; set; }

        [NotMapped]
        public RoomInfo? RoomInfo { get; set; }

        public object Clone()
        {
            return new Room
            {
                RoomNumber = RoomNumber,
                RoomCategory = RoomCategory,
                MaxNumberOfGuest = MaxNumberOfGuest,
                Cost = Cost,
                RoomInfo = RoomInfo
            };
        }

        //cena za den

        public void setFromOtherRoom(Room room) {
            RoomNumber = room.RoomNumber;
            RoomCategory = room.RoomCategory;
            //Describe = room.Describe;
            //Furnishing = room.Furnishing;
            //Bathroom = room.Bathroom;
            //Services = room.Services;
            //Photos = room.Photos;
            MaxNumberOfGuest = room.MaxNumberOfGuest;
            Cost = room.Cost;
            RoomInfo = room.RoomInfo;
        }

    }
}
