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
            ZoznamDodavatelov = new(await _db.Dodavatelia.OrderBy(x => x.Nazov).ToListAsync());
        }

        public override bool MoznoVymazat(Ddodavatel dod)
        {
            var found = _db.Objednavky.Any(x => x.Dodavatel == dod.ICO || x.Odberatel == dod.ICO);    //nesmie byt v objednavkach
            if (found)
            {
                return false;
            }
            return true;
        }

        public override void Vymazat(Ddodavatel dod)
        {
            base.Vymazat(dod);
            _db.Dodavatelia.Remove(dod);
            _db.SaveChanges();
        }
    }
}