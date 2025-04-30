using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Collections.ObjectModel;
using UniComponents;

namespace RecepciaModul.ViewModels
{
    public class HostFlagyViewModel : AObservableViewModel<HostFlag>
    {
        List<(HostFlag, bool)> moznoVymazatList = new();

        public ObservableCollection<Host> ZoznamHostiFlagy { get; set; } = new();
        public List<Host> OdstraniFlaguHostList { get; set; } = new();
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

        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(async () =>
            {
                var token = CancellationTokenSource.Token;

                ZoznamPoloziek = new(await _db.HostFlags
                    .OrderBy(x => x.ID)
                    .ToListAsync(token));

                foreach (var item in ZoznamPoloziek)
                {
                    moznoVymazatList.Add((item, ValidateUserCUD() && !await _db.HostConFlags.AnyAsync(x => x.HostFlag == item.ID, token)));
                }
            });
        }

        public override bool MoznoVymazat(HostFlag item)
        {
            return moznoVymazatList.FirstOrDefault(x => x.Item1 == item).Item2;
        }

        public override void Vymazat(HostFlag item)
        {
            var found = _db.HostFlags.FirstOrDefault(x => x.ID == item.ID);
            if (found != null)
            {
                base.Vymazat(item);
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

                ZoznamPoloziek.Add(EditFlag);
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
            return !Existuje && ZoznamPoloziek.Any(x => x.ID == EditFlag.ID);
        }

        public async Task LoadHostiaZFlagy(HostFlag flaga)
        {
            Nacitavanie = true;
            ZoznamHostiFlagy.Clear();
            OdstraniFlaguHostList.Clear();
            await Task.Run(() =>
            {
                _db.HostConFlags.Include(x => x.HostX)
                .Where(x => x.HostFlag == flaga.ID)
                .ForEachAsync(x => ZoznamHostiFlagy.Add(x.HostX));
            });
            Nacitavanie = false;
        }

        public async Task OdstranHostiZFlagy()
        {
            Nacitavanie = true;
            await Task.Run(() =>
            {
                var canSave = false;
                foreach (var item in OdstraniFlaguHostList)
                {
                    var found = _db.HostConFlags.FirstOrDefault(x => x.Host == item.ID);
                    if (found != null)
                    {
                        _db.HostConFlags.Remove(found);
                        canSave = true;
                    }
                }
                if (canSave)
                {
                    _db.SaveChanges();
                }
                ZoznamHostiFlagy.Clear();
                OdstraniFlaguHostList.Clear();
            });
            Nacitavanie = false;
        }
    }
}