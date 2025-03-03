using BuildingTemplates;
using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using UniComponents;

namespace RecepciaModul.ViewModels
{
    public class HostiaViewModel : AObservableViewModel<Host>, I_ValidationVM, I_RD_TableVM<Host>
    {
        List<(Host, bool)> moznoVymazatList = new();
        public List<DBLayer.Models.Kasa> ZoznamKas { get; set; } = new();

        public bool NacitavaniePoloziek { get => Nacitavanie; set => Nacitavanie = value; }

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

        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(async () =>
            {
                ZoznamPoloziek = new(_db.Hostia.OrderBy(x => x.Surname).ToList());
                foreach (var item in ZoznamPoloziek)
                {
                    item.GuestZ = await _dbw.Users.FirstOrDefaultAsync(x => x.Id == item.Guest);

                    item.PokladnicnyDokladZ = Host.GetActivePokladnicneDoklady(item, in _db).FirstOrDefault();  //ziskanie prveho aktivneho pokladnicneho dokladu

                    moznoVymazatList.Add((item, ValidateUserD() && !_db.HostConReservations.Any(x => x.Host == item.ID /*|| x.ReservationZ.DepartureDate <= DateTime.Today.AddDays(-360)*/)));
                }

                ZoznamKas.Clear();
                ZoznamKas.AddRange(await _db.Kasy
                .Include(x => x.ActualUserX)
                .Include(x => x.DodavatelX)
                .OrderBy(x => x.ID)
                .ToListAsync());         //nacitanie zoznamu kas

            });
        }

        public override bool MoznoVymazat(Host item)
        {
            return moznoVymazatList.FirstOrDefault(x => x.Item1 == item).Item2;
        }

        public override void Vymazat(Host item)
        {
            base.Vymazat(item);
            _db.HostConFlags.RemoveRange(_db.HostConFlags.Where(x => x.Host == item.ID));
            _db.HostConReservations.RemoveRange(_db.HostConReservations.Where(x => x.Host == item.ID));
            _db.Hostia.Remove(item);
            _db.SaveChanges();
        }

        public async Task RefreshMaPokladnicnyBlok()
        {
            Nacitavanie = true;
            await Task.Run(() =>
            {
                foreach (var item in ZoznamPoloziek)
                {
                    item.PokladnicnyDokladZ = Host.GetActivePokladnicneDoklady(item, in _db).FirstOrDefault();
                }
            });
            Nacitavanie = false;
        }
    }
}