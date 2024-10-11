using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;

namespace UctovnyModul.ViewModels
{
    public partial class UniConItemyViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<UniConItemPoklDokladu> zoznamPoloziek = new();
        List<UniConItemPoklDokladu> zoznamPoloziekAll = new();

        public bool NacitavaniePoloziek { get; private set; } = true;

        Type typSpojenia = typeof(PolozkaSkladuConItemPoklDokladu);

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
            zoznamPoloziekAll = new(await _db.UniConItemyPoklDokladu
                .OrderByDescending(x => x.ID)
                .Include(x => ((PolozkaSkladuConItemPoklDokladu)x).PolozkaSkladuMnozstvaX)
                .ThenInclude(x => x.PolozkaSkladuX)
                .ToListAsync());
            var polozkyRes = zoznamPoloziekAll.Where(x => x is ReservationConItemPoklDokladu)
                .Select(x => ((ReservationConItemPoklDokladu)x))
                .ToList();
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
            ZoznamPoloziek.Clear();
            foreach (var item in zoznamPoloziekAll)
            {
                if (item.GetType() == typSpojenia)
                {
                    ZoznamPoloziek.Add(item);
                }
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

        public List<string> TypyList()
        {
            List<string> list = new();
            list.Add(new ReservationConItemPoklDokladu().GetTypeUni());
            list.Add(new PolozkaSkladuConItemPoklDokladu().GetTypeUni());
            return list;
        }

        public string GetTypNazov()
        {
            if (typSpojenia == typeof(ReservationConItemPoklDokladu))
            {
                return new ReservationConItemPoklDokladu().GetTypeUni();
            }
            return new PolozkaSkladuConItemPoklDokladu().GetTypeUni();
        }

        public void SpravujZmenuTypu(string typ)
        {
            if (!TypyList().Contains(typ))
            {
                return;
            }

            if (typ == new ReservationConItemPoklDokladu().GetTypeUni())
            {
                typSpojenia = typeof(ReservationConItemPoklDokladu);
            }
            else if (typ == new PolozkaSkladuConItemPoklDokladu().GetTypeUni())
            {
                typSpojenia = typeof(PolozkaSkladuConItemPoklDokladu);
            }

            ZoznamPoloziek.Clear();
            foreach (var item in zoznamPoloziekAll)
            {
                if (item.GetType() == typSpojenia)
                {
                    ZoznamPoloziek.Add(item);
                }
            }

        }

        public string GetHeaderTable()
        {
            if (typSpojenia == typeof(ReservationConItemPoklDokladu))
            {
                return "Rezervácia";
            }
            return "Sklad";
        }

        public string GetValTable(UniConItemPoklDokladu item)
        {
            if (item is ReservationConItemPoklDokladu ytem)
            {
                return ytem.ReservationZ?.Guest?.Email ?? ytem.ReservationZ?.RoomNumber ?? "-";
            }
            return ((PolozkaSkladuConItemPoklDokladu)item).PolozkaSkladuMnozstvaX.Sklad;
        }

    }
}