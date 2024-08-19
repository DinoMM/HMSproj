using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminModul.ViewModels.Pouzivatelia
{
    public partial class PouzivateliaViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<IdentityUserOwn> zoznamPouzivatelov = new();

        [ObservableProperty]
        ObservableCollection<(IdentityUserOwn, List<RolesOwn>)> zoznamPouzivatelovRoli = new();

        public bool NacitavaniePoloziek = true;

        private readonly DBContext _db;
        readonly UserService _userService;
        public PouzivateliaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;

        }

        public async Task NacitajZoznamy()
        {
            ZoznamPouzivatelov = new(_db.Users.ToList());
            foreach (var item in ZoznamPouzivatelov)    //pridanie zoznamov roli
            {
                ZoznamPouzivatelovRoli.Add((item, await _userService.GetRolesFromUser(item)));
            }
            NacitavaniePoloziek = false;
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
                    rolesStr += found.Item2[i].ToString();
                }
                else
                {
                    rolesStr += found.Item2[i].ToString() + ", ";
                }
            }
            return rolesStr;
        }

        public bool MoznoVymazat(IdentityUserOwn user)
        {
            if (user.UserName != _userService.LoggedUser.UserName) //isty ako prihlaseny nemoze
            {
                return false;
            }
            if (ZoznamPouzivatelovRoli.FirstOrDefault(x => x.Item1 == user).Item2.Count != 0) //nemoze mat ziadne role - ked nema role tak by nemal amt ani prepojenia k inym tabulkam
            {
                return false;
            }

            return true;
        }

        public void Vymazat(IdentityUserOwn user)
        {
            var found = _db.Users.FirstOrDefault(x => x.UserName == user.UserName);
            if (found != null)
            {
                ZoznamPouzivatelov.Remove(user);
                ZoznamPouzivatelovRoli.Remove(ZoznamPouzivatelovRoli.FirstOrDefault(x => x.Item1 == user));
                _db.Users.Remove(found);
                _db.SaveChanges();
            }
            return;
        }


    }
}
