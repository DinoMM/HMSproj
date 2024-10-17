using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace RecepciaModul.ViewModels
{
    public partial class HostFlagyViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<HostFlag> zoznamFlagov = new();

        public bool NacitavaniePoloziek { get; private set; } = true;

        public HostFlag EditFlag { get; set; } = new() { DateValue = DateTime.Now };
        public bool Existuje { get; set; } = false;

        private readonly DBContext _db;
        private readonly UserService _userService;

        public HostFlagyViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Host.ROLE_R_HOSTIA);
        }

        public bool ValidateUserCUD()
        {
            return _userService.IsLoggedUserInRoles(Host.ROLE_D_HOSTIA);
        }

        public async Task NacitajZoznamy()
        {
            _db.HostFlags
                .OrderBy(x => x.ID)
                .ToList()
                .ForEach(x => ZoznamFlagov.Add(x));
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(HostFlag item)
        {
            return _db.HostConFlags.Any(x => x.HostFlag == item.ID);
        }

        public void Vymazat(HostFlag item)
        {
            var found = _db.HostFlags.FirstOrDefault(x => x.ID == item.ID);
            if (found != null)
            {
                ZoznamFlagov.Remove(item);
                _db.HostFlags.Remove(found);
                _db.SaveChanges();
            }
        }
        public bool Ulozit()
        {
            var found = _db.HostFlags.FirstOrDefault(x => x.ID == EditFlag.ID);
            if (found == null)
            {
                _db.HostFlags.Add(EditFlag);
                Existuje = true;

                ZoznamFlagov.Add(EditFlag);
            }
            else
            {
                found.SetFromOther(EditFlag);
            }
            _db.SaveChanges();
            return true;
        }

        public bool KontrolaExistencieID()
        {
            return !Existuje && ZoznamFlagov.Any(x => x.ID == EditFlag.ID);
        }
    }
}