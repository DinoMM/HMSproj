using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using UniComponents;

namespace LudskeZdrojeModul.ViewModels.Zamestnanci
{
    public class ZamViewModel : AObservableViewModel<IdentityUserOwn>
    {
        ObservableCollection<(IdentityUserOwn, List<string>)> ZoznamPouzivatelovRoli = new();

        private readonly DBContext _db;
        private readonly UserService _userService;

        public ZamViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(IdentityUserOwn.ROLE_R_ZAMESTNANCI);
        }

        public bool ValidateUserCRUD()
        {
            return _userService.IsLoggedUserInRoles(IdentityUserOwn.ROLE_CRUD_ZAMESTNANCI);
        }

        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(async () =>
            {
                var token = CancellationTokenSource.Token;

                ZoznamPoloziek = new(await _db.Users.OrderBy(x => x.Surname).ToListAsync(token));
                foreach (var item in ZoznamPoloziek)    //pridanie zoznamov roli
                {
                    token.ThrowIfCancellationRequested();
                    ZoznamPouzivatelovRoli.Add((item, await _userService.GetRolesFromUser(item)));
                }
            });
        }

        public override bool MoznoVymazat(IdentityUserOwn user)
        {
            if (user.UserName == _userService.LoggedUser.UserName) //isty ako prihlaseny nemoze
            {
                return false;
            }
            var roleUser = ZoznamPouzivatelovRoli.FirstOrDefault(x => x.Item1 == user).Item2;
            if (roleUser.Count > 0) //nemoze mat ziadne role
            {
                return false;
            }

            return true;
        }

        public async Task VymazatAsync(IdentityUserOwn user)
        {
            base.Vymazat(user);
            ZoznamPouzivatelovRoli.Remove(ZoznamPouzivatelovRoli.FirstOrDefault(x => x.Item1 == user));
            await _userService.DeleteUser(user);
        }

        public string GetRoleString(IdentityUserOwn user)
        {
            var found = ZoznamPouzivatelovRoli.FirstOrDefault(x => x.Item1 == user);
            if (found == default)
            {
                return "";
            }
            string rolesStr = "";
            for (int i = 0; i < found.Item2.Count; ++i)
            {
                if (i == found.Item2.Count - 1)
                {
                    rolesStr += found.Item2[i];
                }
                else
                {
                    rolesStr += found.Item2[i] + ", ";
                }
            }
            return rolesStr;
        }
    }
}