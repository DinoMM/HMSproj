using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Ssklad = DBLayer.Models.Sklad;

namespace SkladModul.ViewModels.Sklady
{
    public partial class SkladyViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<Ssklad> zoznamSkladov = new();

        public bool NacitavaniePoloziek { get; private set; } = true;

        private readonly DBContext _db;
        private readonly UserService _userService;

        public SkladyViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Ssklad.ROLE_R_SKLAD);
        }
        public bool ValidateUserCRUD()
        {
            return _userService.IsLoggedUserInRoles(Ssklad.ROLE_CRUD_SKLAD);
        }

        public async Task NacitajZoznamy()
        {
            ZoznamSkladov = new(await _db.Sklady.OrderBy(x => x.ID).ToListAsync());
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(Ssklad sklad)
        {
            if (_db.PolozkaSkladuMnozstva.Any(x => x.Sklad == sklad.ID))
            {
                return false;
            }
            if (_db.SkladUzivatelia.Any(x => x.Sklad == sklad.ID))
            {
                return false;
            }
            if (_db.SkladObdobi.Any(x => x.Sklad == sklad.ID))
            {
                return false;
            }
            if (_db.Vydajky.Any(x => x.Sklad == sklad.ID || x.SkladDo == sklad.ID))
            {
                return false;
            }
            if (_db.Prijemky.Any(x => x.Sklad == sklad.ID))
            {
                return false;
            }
            return true;
        }

        public void Vymazat(Ssklad sklad)
        {
            ZoznamSkladov.Remove(sklad);
            _db.Sklady.Remove(sklad);
            _db.SaveChanges();
        }
    }
}