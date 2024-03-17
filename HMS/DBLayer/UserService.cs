using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;


namespace DBLayer
{
    public class UserService    //vytverene AI
    {
        private readonly UserManager<IdentityUserOwn> _userManager;

        public IdentityUserOwn? LoggedUser { get; set; } = default!;
        public RolesOwn LoggedUserRole { get; set; } = RolesOwn.None;


        public UserService(UserManager<IdentityUserOwn> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> CreateNewUserAsync(IdentityUserOwn user, string password, RolesOwn assignedRole = RolesOwn.None)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) {
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
            if (!Enum.TryParse((await _userManager.GetRolesAsync(user)).FirstOrDefault(), out role)) //ziskanie roly usera
            {
                return false;
            }

            LoggedUser = user;
            LoggedUserRole = role;
            return true;
        }

        public void LogOutUser() {
            LoggedUser = default;
            LoggedUserRole = RolesOwn.None;
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
