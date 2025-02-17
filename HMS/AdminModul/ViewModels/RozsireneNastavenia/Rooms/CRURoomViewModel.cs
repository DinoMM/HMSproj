using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using DBLayer.Models.HSKModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using UniComponents;

namespace AdminModul.ViewModels
{
    public class CRURoomViewModel : ObservableEntitySaverVM<Room>
    {
        public EntitySaver<RoomInfo> EntitySaver_RoomInfo { get; set; }

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;

        public CRURoomViewModel(DBContext db, UserService userService, DataContext dbw)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;

            EntitySaver = new EntitySaver<Room>(_dbw,
                _dbw.HRooms,
                (Room x) => _dbw.HRooms.FirstOrDefault(y => x.RoomNumber == y.RoomNumber));

            EntitySaver_RoomInfo = new EntitySaver<RoomInfo>(_db, _db.RoomInfos,
                (RoomInfo x) => _db.RoomInfos.FirstOrDefault(y => x.ID_Room == y.ID_Room));

            TrackingList.Add(EntitySaver_RoomInfo);
        }

        public override ValidationResult Save()
        {
            EntitySaver_RoomInfo.Input.LastUpdate = DateTime.Now;
            if (!EntitySaver.Exist)
            {
                EntitySaver_RoomInfo.Input.ID_Room = Entity.RoomNumber;
            }
            return base.Save();
        }

    }
}