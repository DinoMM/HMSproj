using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using UniComponents;
using Ssklad = DBLayer.Models.Sklad;

namespace SkladModul.ViewModels.Sklady
{
    public class SkladyViewModel : AObservableViewModel<Ssklad>
    {
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
        protected override async Task NacitajZoznamyAsync()
        {
            ZoznamPoloziek = new(await _db.Sklady.OrderBy(x => x.ID).ToListAsync());
        }

        public override bool MoznoVymazat(Ssklad sklad)
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

        public override void Vymazat(Ssklad sklad)
        {
            base.Vymazat(sklad);
            _db.Sklady.Remove(sklad);
            _db.SaveChanges();
        }
    }
}