using BuildingTemplates;
using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace RecepciaModul.ViewModels
{
    public partial class HostiaViewModel : ObservableObject, I_ValidationVM, I_RD_TableVM<Host>
    {
        [ObservableProperty]
        ObservableCollection<Host> zoznamHosti = new();

        public bool NacitavaniePoloziek { get; private set; } = true;

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;

        public HostiaViewModel(DBContext db, UserService userService, DataContext dbw)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Host.ROLE_R_HOSTIA);
        }
        public bool ValidateUserCRU()
        {
            return _userService.IsLoggedUserInRoles(Host.ROLE_CRU_HOSTIA);
            
        }
        public bool ValidateUserD()
        {
            return _userService.IsLoggedUserInRoles(Host.ROLE_D_HOSTIA);
        }

        public async Task NacitajZoznamy()
        {
            ZoznamHosti = new( _db.Hostia.OrderBy(x => x.Surname).ToList());
            foreach (var item in ZoznamHosti)
            {
                item.GuestZ = await _dbw.Users.FirstOrDefaultAsync(x => x.Id == item.Guest);
            }
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(Host item)
        {
            if (_db.HostConReservations.Any(x => x.Host == item.ID /*&& x.ReservationZ.DepartureDate <= DateTime.Today.AddDays(-360)*/)) {
                return false;
            }
            return true;
        }

        public void Vymazat(Host item)
        {
            ZoznamHosti.Remove(item);
            _db.HostConFlags.RemoveRange(_db.HostConFlags.Where(x => x.Host == item.ID));
            _db.HostConReservations.RemoveRange(_db.HostConReservations.Where(x => x.Host == item.ID));
            _db.Hostia.Remove(item);
            _db.SaveChanges();
        }
    }
}