using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using UniComponents;

namespace UctovnyModul.ViewModels
{
    public class UniConItemyViewModel : AObservableViewModel<UniConItemPoklDokladu>
    {
        List<UniConItemPoklDokladu> zoznamPoloziekAll = new();

        List<(UniConItemPoklDokladu, bool)> moznoVymazatList = new();

        Type typSpojenia { get; set; } = typeof(PolozkaSkladuConItemPoklDokladu);

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

        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(async () =>
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
                ZoznamPoloziek = new();
                foreach (var item in zoznamPoloziekAll)
                {
                    if (item.GetType() == typSpojenia)
                    {
                        ZoznamPoloziek.Add(item);
                    }
                }

                foreach (var item in zoznamPoloziekAll)
                {
                    moznoVymazatList.Add((item, !_db.ItemyPokladDokladu.Any(x => x.UniConItemPoklDokladu == item.ID)));
                }
            });
        }

        public override bool MoznoVymazat(UniConItemPoklDokladu item)
        {
            return moznoVymazatList.FirstOrDefault(x => x.Item1 == item).Item2;
        }

        public override void Vymazat(UniConItemPoklDokladu item)
        {
            base.Vymazat(item);
            _db.UniConItemyPoklDokladu.Remove(item);
            _db.SaveChanges();
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

        public async Task SpravujZmenuTypu(string typ)
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

            ZoznamPoloziek.CollectionChanged -= OnCollectionChanged;
            Nacitavanie = true;
            await Task.Run(() => {
                ZoznamPoloziek.Clear();
                foreach (var item in zoznamPoloziekAll)
                {
                    if (item.GetType() == typSpojenia)
                    {
                        ZoznamPoloziek.Add(item);
                    }
                }
                OnCollectionChanged(this, new(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
            });
            ZoznamPoloziek.CollectionChanged += OnCollectionChanged;
            Nacitavanie = false;
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