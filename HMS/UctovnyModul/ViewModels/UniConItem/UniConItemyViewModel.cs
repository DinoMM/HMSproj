using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace UctovnyModul.ViewModels
{
    public partial class UniConItemyViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<UniConItemPoklDokladu> zoznamPoloziek = new();

        public bool NacitavaniePoloziek { get; private set; } = true;

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;

        public UniConItemyViewModel(DBContext db, UserService userService, DataContext dbw)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(UniConItemPoklDokladu.ROLE_R_POLOZKY);
        }
        public bool ValidateUserCRUD()
        {
            return _userService.IsLoggedUserInRoles(UniConItemPoklDokladu.ROLE_CRUD_POLOZKY);
        }

        public async Task NacitajZoznamy()
        {
            ZoznamPoloziek = new(await _db.UniConItemyPoklDokladu
                .OrderByDescending(x => x.ID)
                .Include(x => ((PolozkaSkladuConItemPoklDokladu)x).PolozkaSkladuX)
                .ToListAsync());
            var polozkyRes = ZoznamPoloziek.Where(x => x is ReservationConItemPoklDokladu).Select(x => ((ReservationConItemPoklDokladu)x)).ToList();
            foreach (var item in polozkyRes)
            {
                var found = await _dbw.Rezervations
                    .Include(x => x.Room)
                    .Include(x => x.Coupon)
                    .Include(x => x.Guest)
                    .FirstOrDefaultAsync(x => x.Id == item.Reservation);
                if (found == null)
                {
                    continue;
                }
                item.ReservationZ = found;
            }
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(UniConItemPoklDokladu item)
        {
            return false;
        }

        public void Vymazat(UniConItemPoklDokladu item)
        {


        }
    }
}