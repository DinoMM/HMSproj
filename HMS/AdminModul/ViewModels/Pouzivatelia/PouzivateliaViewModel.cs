using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniComponents;

namespace AdminModul.ViewModels.Pouzivatelia
{
    public class PouzivateliaViewModel : AObservableViewModel<IdentityUserOwn>
    {
        ObservableCollection<(IdentityUserOwn, List<string>)> ZoznamPouzivatelovRoli = new();

        private readonly DBContext _db;
        readonly UserService _userService;
        public PouzivateliaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }
        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(async () =>
            {
                ZoznamPoloziek = new(_db.Users.OrderBy(x => x.Surname).ToList());
                foreach (var item in ZoznamPoloziek)    //pridanie zoznamov roli
                {
                    ZoznamPouzivatelovRoli.Add((item, await _userService.GetRolesFromUser(item)));
                }
            });
        }

        public bool ValidateUser()
        {
            return _userService.LoggedUserRole == RolesOwn.Admin;
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

        public override bool MoznoVymazat(IdentityUserOwn user)
        {
            if (user.UserName == _userService.LoggedUser.UserName) //isty ako prihlaseny nemoze
            {
                return false;
            }
            var roleUser = ZoznamPouzivatelovRoli.FirstOrDefault(x => x.Item1 == user).Item2;
            if (roleUser.Count > 0) //nemoze mat ziadne role - ked ma rolu NONE tak by nemal mat ani prepojenia k inym tabulkam
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


    }
}
