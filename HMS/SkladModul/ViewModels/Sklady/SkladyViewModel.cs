using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Migrations;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using UniComponents;
using Ssklad = DBLayer.Models.Sklad;

namespace SkladModul.ViewModels.Sklady
{
    public class SkladyViewModel : AObservableViewModel<Ssklad>
    {
        private List<(Ssklad, bool)> moznoVymazat = new();

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
            var token = CancellationTokenSource.Token;
            ZoznamPoloziek = new(await _db.Sklady.OrderBy(x => x.ID).ToListAsync(token));
            foreach (var sklad in ZoznamPoloziek)
            {
                var res =
                    await _db.PolozkaSkladuMnozstva.AnyAsync(x => x.Sklad == sklad.ID, token)
                    || await _db.SkladUzivatelia.AnyAsync(x => x.Sklad == sklad.ID, token)
                    || await _db.SkladObdobi.AnyAsync(x => x.Sklad == sklad.ID, token)
                    || await _db.Vydajky.AnyAsync(x => x.Sklad == sklad.ID || x.SkladDo == sklad.ID, token)
                    || await _db.Prijemky.AnyAsync(x => x.Sklad == sklad.ID, token);

                moznoVymazat.Add((sklad, !res));
            }
        }

        public override bool MoznoVymazat(Ssklad sklad)
        {
            return moznoVymazat.FirstOrDefault(x => x.Item1.ID == sklad.ID).Item2;
        }

        public override void Vymazat(Ssklad sklad)
        {
            base.Vymazat(sklad);
            _db.Sklady.Remove(sklad);
            _db.SaveChanges();
        }
    }
}