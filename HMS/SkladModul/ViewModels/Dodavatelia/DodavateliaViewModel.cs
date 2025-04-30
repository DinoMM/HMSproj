using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UniComponents;
using Ddodavatel = DBLayer.Models.Dodavatel;

namespace SkladModul.ViewModels.Dodavatelia
{
    public partial class DodavateliaViewModel : AObservableViewModel<Ddodavatel>
    {
        public ObservableCollection<Ddodavatel> ZoznamDodavatelov { get => ZoznamPoloziek; set => ZoznamPoloziek = value; }
        private List<(Dodavatel, bool)> moznoVymazat = new();

        private readonly DBContext _db;
        private readonly UserService _userService;

        public DodavateliaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Ddodavatel.ROLE_R_DODAVATELIA);
        }

        public bool ValidateUserCRUD()
        {
            return _userService.IsLoggedUserInRoles(Ddodavatel.ROLE_CRUD_DODAVATELIA);
        }

        protected override async Task NacitajZoznamyAsync()
        {
            var token = CancellationTokenSource.Token;
            ZoznamDodavatelov = new(await _db.Dodavatelia.OrderBy(x => x.Nazov).ToListAsync(token));

            moznoVymazat.Clear();
            foreach (var dod in ZoznamDodavatelov)
            {
                var res = await _db.Objednavky.AnyAsync(x => x.Dodavatel == dod.ICO || x.Odberatel == dod.ICO, token) 
                    || await _db.Kasy.AnyAsync(x => x.Dodavatel == dod.ICO, token)
                    || await _db.Faktury.AnyAsync(x => x.OdKoho == dod.ICO, token);

                moznoVymazat.Add((dod, !res));
            }
        }

        public override bool MoznoVymazat(Ddodavatel dod)
        {
            return moznoVymazat.FirstOrDefault(x => x.Item1.ICO == dod.ICO).Item2;
        }

        public override void Vymazat(Ddodavatel dod)
        {
            base.Vymazat(dod);
            _db.Dodavatelia.Remove(dod);
            _db.SaveChanges();
        }
    }
}