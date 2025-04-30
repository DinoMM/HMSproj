using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using UniComponents;

namespace AdminModul.ViewModels
{
    public class RoomsViewModel : AObservableViewModel<Room>
    {
        private List<(Room, bool)> moznoVymazatList = new();

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;

        public RoomsViewModel(DBContext db, UserService userService, DataContext dbw)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(new List<RolesOwn>() { RolesOwn.Admin });
        }

        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(async () =>
            {
                var token = CancellationTokenSource.Token;

                ZoznamPoloziek = new(await _dbw.HRooms
                    .ToListAsync(token));

                var list = await _db.RoomInfos.ToListAsync(token);
                foreach (var item in ZoznamPoloziek)
                {
                    token.ThrowIfCancellationRequested();
                    var found = list.FirstOrDefault(x => x.ID_Room == item.RoomNumber);
                    if (found != null)
                    {
                        item.RoomInfo = found;
                    }
                    else
                    {
                        throw new ArgumentException("Room info neexistuje!");
                    }

                    moznoVymazatList.Add((item, !_dbw.Rezervations.Any(x => x.RoomNumber == item.RoomNumber))); //jednoducha kontrola, ak by sa chcelo vyhrat tak by sa dalo spravit to ze ked je ukoncena rezervacia po dlhsiom case tak by bolo mozne vymazat
                }

            });
        }


        public override bool MoznoVymazat(Room item)
        {
            return moznoVymazatList.Find(x => x.Item1 == item).Item2;
        }

        public override void Vymazat(Room item)
        {
            base.Vymazat(item);
            _db.RoomInfos.Remove(item.RoomInfo);
            _dbw.HRooms.Remove(item);
            _db.SaveChanges();
            _dbw.SaveChanges();
        }
    }
}