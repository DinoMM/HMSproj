using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;
using UniComponents;

namespace RecepciaModul.ViewModels
{
    public class KasaViewModel : AObservableViewModel<DBLayer.Models.PokladnicnyDoklad>
    {
        public List<DBLayer.Models.Kasa> ZoznamKas = new();

        public DBLayer.Models.Kasa? AktlKasa { get; set; } = null;

        private readonly DBContext _db;
        private readonly UserService _userService;
        private readonly IJSRuntime _jsRuntime;

        public KasaViewModel(DBContext db, UserService userService, IJSRuntime jsRuntime)
        {
            _db = db;
            _userService = userService;
            _jsRuntime = jsRuntime;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(DBLayer.Models.PokladnicnyDoklad.ROLE_R_POKLDOKL) && _userService.IsLoggedUserInRoles(DBLayer.Models.Kasa.ROLE_R_KASA);
        }
        public bool ValidateUserCRU()
        {
            return _userService.IsLoggedUserInRoles(DBLayer.Models.PokladnicnyDoklad.ROLE_CRU_POKLDOKL);
        }
        public bool ValidateUserD()
        {
            return _userService.IsLoggedUserInRoles(DBLayer.Models.PokladnicnyDoklad.ROLE_D_POKLDOKL);
        }
        public bool ValidateUserKasaR()
        {
            return _userService.IsLoggedUserInRoles(DBLayer.Models.Kasa.ROLE_R_KASA);
        }

        public bool ValidateUserKasaCRUD()
        {
            return _userService.IsLoggedUserInRoles(DBLayer.Models.Kasa.ROLE_CRUD_KASA);
        }

        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(() =>
            {
                NacitajKasy();
                ZoznamPoloziek = new(_db.PokladnicneDoklady
                    .Include(x => x.KasaX)
                    .Include(x => x.HostX)
                    .OrderByDescending(x => x.Vznik)
                    .ToList());
            });
        }

        public void NacitajKasy()
        {
            ZoznamKas.Clear();
            ZoznamKas.AddRange(_db.Kasy
                .Include(x => x.ActualUserX)
                .Include(x => x.DodavatelX)
                .ToList());
        }

        public override bool MoznoVymazat(DBLayer.Models.PokladnicnyDoklad item)
        {
            return !item.Spracovana;
        }

        public override void Vymazat(DBLayer.Models.PokladnicnyDoklad item)
        {
            var itemy = _db.ItemyPokladDokladu.Where(x => x.Skupina == item.ID).ToList();
            foreach (var ytem in itemy)
            {
                _db.ItemyPokladDokladu.Remove(ytem);
            }
            base.Vymazat(item);
            _db.PokladnicneDoklady.Remove(item);
            _db.SaveChanges();
        }

    }
}