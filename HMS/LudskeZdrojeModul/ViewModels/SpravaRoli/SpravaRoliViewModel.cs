using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Data;
using UniComponents;

namespace LudskeZdrojeModul.ViewModels.SpravaRoli
{
    public class SpravaRoliViewModel : AObservableViewModel<IdentityRole>
    {
        private List<(IdentityRole, bool)> MoznoVymazatList = new();

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

        protected override async Task NacitajZoznamyAsync()
        {
            ZoznamPoloziek = new(await _db.Roles.OrderBy(x => x.Name).ToListAsync());
            foreach (var item in ZoznamPoloziek)
            {
                MoznoVymazatList.Add((item, !_db.UserRoles.Any(x => x.RoleId == item.Id)));
            }
        }

        public override bool MoznoVymazat(IdentityRole role)
        {
            if (IsDefaultRole(role))
            {
                return false;
            }

            return MoznoVymazatList.FirstOrDefault(x => x.Item1 == role).Item2;
        }

        public override void Vymazat(IdentityRole role)
        {
            base.Vymazat(role);
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