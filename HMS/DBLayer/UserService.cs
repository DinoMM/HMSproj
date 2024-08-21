using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;


namespace DBLayer
{
    public class UserService    //pomoc AI
    {
        private readonly UserManager<IdentityUserOwn> _userManager;

        public IdentityUserOwn? LoggedUser { get; set; } = default!;
        public RolesOwn LoggedUserRole { get; set; } = RolesOwn.None;


        public UserService(UserManager<IdentityUserOwn> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Vytvorí nového používateľa s daným heslom a priradí mu rolu
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="assignedRole"></param>
        /// <returns></returns>
        public async Task<bool> CreateNewUserAsync(IdentityUserOwn user, string password, RolesOwn assignedRole = RolesOwn.None)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return false;
            }
            var res = _userManager.AddToRoleAsync(user, assignedRole.ToString());
            return result.Succeeded;
        }

        public async Task<bool> CreateNewUserNoPSWDAsync(IdentityUserOwn user, RolesOwn assignedRole = RolesOwn.None)
        {
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return false;
            }
            var res = _userManager.AddToRoleAsync(user, assignedRole.ToString());
            return result.Succeeded;
        }

        public async Task<bool> LogInUserAsync(string name, string password)
        {
            var user = await _userManager.FindByNameAsync(name);//ziskanie usera
            if (user is null)
            {
                return false;
            }
            var succed = await _userManager.CheckPasswordAsync(user, password); //kontrola hesla
            if (!succed)
            {
                return false;
            }
            RolesOwn role;
            if (!Enum.TryParse((await _userManager.GetRolesAsync(user)).FirstOrDefault(), out role)) //ziskanie roly usera, prvej roly
            {
                return false;
            }
            //TODO pridat fukctionalitu pre viacero rolii
            LoggedUser = user;
            LoggedUserRole = role;
            return true;
        }

        public void LogOutUser()
        {
            LoggedUser = default;
            LoggedUserRole = RolesOwn.None;
        }

        public async Task<List<RolesOwn>> GetRolesFromUser(IdentityUserOwn user)
        {
            var list = await _userManager.GetRolesAsync(user);
            RolesOwn rola = RolesOwn.None;
            List<RolesOwn> roles = new();
            foreach (var item in list)
            {
                if (!Enum.TryParse(item, out rola))
                {
                    Debug.WriteLine("Chyba pri ziskavani roly");
                    break;
                }
                roles.Add(rola);
            }
            return roles;
        }

        public async Task<bool> ChangePassword(string userId, string password, string username)
        {
            var userfound = await _userManager.FindByIdAsync(userId);
            if (userfound != null)
            {
                await _userManager.RemovePasswordAsync(userfound);
                await _userManager.AddPasswordAsync(userfound, password);
                await _userManager.SetUserNameAsync(userfound, username);
                return true;
            }
            return false;
        }

        public async Task<bool> AddRoleToUser(string userId, RolesOwn roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            return (await _userManager.AddToRoleAsync(user, roleName.ToString())).Succeeded;
        }

        public async Task<bool> RemoveRoleFromUser(string userId, RolesOwn role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            return (await _userManager.RemoveFromRoleAsync(user, role.ToString())).Succeeded;
        }


    }
    public enum RolesOwn
    {
        None,
        Admin,
        Skladnik,
        Recepcny,
        Bezpecnostnik,
        FBVeduci,
        HKVeduci,
        Nakupca,
        Personalista,
        Riaditel,
        Uctovnik,
        UdalostnyPlanovac,
        Udrzbar
    };
}
