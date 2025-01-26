using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using DBLayer.Models.HSKModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using UniComponents;

namespace HSKModul.ViewModels
{
    public partial class CRURoomCompViewModel : ObservableEntitySaverVM<Rezervation>
    {
        public EntitySaver<RoomInfo> EntitySaver_RoomInfo { get; set; }

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;

        public CRURoomCompViewModel(DBContext db, UserService userService, DataContext dbw)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;

            EntitySaver = new EntitySaver<Rezervation>(_dbw,
                _dbw.Rezervations,
                (Rezervation x) => _dbw.Rezervations.FirstOrDefault(y => x.Id == y.Id));

            EntitySaver_RoomInfo = new EntitySaver<RoomInfo>( _db, _db.RoomInfos,
                (RoomInfo x) => _db.RoomInfos.FirstOrDefault(y => x.ID_Room == y.ID_Room));

            TrackingList.Add(EntitySaver_RoomInfo);
        }

        public bool ValidateUser()
        {
            return true;
        }

    }
}