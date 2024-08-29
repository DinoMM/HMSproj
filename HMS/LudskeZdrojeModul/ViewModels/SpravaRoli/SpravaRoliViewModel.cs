using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Data;

namespace LudskeZdrojeModul.ViewModels.SpravaRoli
{
    public partial class SpravaRoliViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<IdentityRole> zoznamRoli = new();

        public bool NacitavaniePoloziek { get; private set; } = true;

        private readonly DBContext _db;
        private readonly UserService _userService;

        public SpravaRoliViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(IdentityUserOwn.ROLE_CRUD_ROLI);
        }

        public async Task NacitajZoznamy()
        {
            ZoznamRoli = new(await _db.Roles.OrderBy(x => x.Name).ToListAsync());
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(IdentityRole role)
        {
            if (IsDefaultRole(role)) {
                return false;
            }

            var found = _db.UserRoles.FirstOrDefault(x => x.RoleId == role.Id);
            return found is null;
        }

        public void Vymazat(IdentityRole role)
        {
            ZoznamRoli.Remove(role);
            _db.Roles.Remove(role);
            _db.SaveChanges();

        }

        public bool IsDefaultRole(IdentityRole rola)    //kontrola ci sa jedna o defaultnu rolu
        {
            var roleVsetkyEnum = Enum.GetValues(typeof(RolesOwn)).Cast<RolesOwn>().ToList();    //nemozno vymazat role, ktore su z enumu
            foreach (var item in roleVsetkyEnum)
            {
                if (rola.Name == item.ToString())
                {
                    return true;
                }
            }
            return false;
        }
    }
}