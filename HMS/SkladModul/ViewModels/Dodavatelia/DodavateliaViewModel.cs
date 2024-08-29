using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

using Ddodavatel = DBLayer.Models.Dodavatel;

namespace SkladModul.ViewModels.Dodavatelia
{
    public partial class DodavateliaViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<Ddodavatel> zoznamDodavatelov = new();

        public bool NacitavaniePoloziek { get; set; } = true;

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

        public async Task NacitajZoznamy()
        {
            ZoznamDodavatelov = new(await _db.Dodavatelia.OrderBy(x => x.Nazov).ToListAsync());
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(Ddodavatel dod)
        {
            var found = _db.Objednavky.Any(x => x.Dodavatel == dod.ICO || x.Odberatel == dod.ICO);    //nesmie byt v objednavkach
            if (found)
            {
                return false;
            }
            return true;
        }

        public async Task Vymazat(Ddodavatel dod)
        {
            ZoznamDodavatelov.Remove(dod);
            _db.Dodavatelia.Remove(dod);
            await _db.SaveChangesAsync();
        }
    }
}