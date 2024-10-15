using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel;
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

        public List<RolesOwn> LoggedUserRoles { get; set; } = new();


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

        public async Task<bool> CreateNewUserNoPSWDAsync(IdentityUserOwn user)
        {
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> LogInUserAsync(string name, string password)
        {
            var user = await _userManager.FindByNameAsync(name);//ziskanie usera
            if (user is null)
            {
                LoggedUser = null;
                return false;
            }
            var succed = await _userManager.CheckPasswordAsync(user, password); //kontrola hesla
            if (!succed)
            {
                LoggedUser = null;
                return false;
            }
            RolesOwn role;
            if (!Enum.TryParse((await _userManager.GetRolesAsync(user)).FirstOrDefault(), out role)) //ziskanie roly usera, prvej roly
            {
                role = RolesOwn.None;
            }
            
            var rolesUser = await _userManager.GetRolesAsync(user); // ziskanie vsetkych roli usera
            foreach (var item in rolesUser)
            {
                RolesOwn rrole;
                if (!Enum.TryParse(item, out rrole)) //ziskanie roly usera
                {
                    continue;
                }
                LoggedUserRoles.Add(rrole);
            }

            LoggedUser = user;
            LoggedUserRole = role;
            return true;
        }

        public void LogOutUser()
        {
            LoggedUser = null;
            LoggedUserRole = RolesOwn.None;
            LoggedUserRoles.Clear();
        }

        public async Task<List<RolesOwn>> GetRolesOwnFromUser(IdentityUserOwn user)
        {
            var list = await GetRolesFromUser(user);
            RolesOwn rola = RolesOwn.None;
            List<RolesOwn> roles = new();
            foreach (var item in list)
            {
                if (!Enum.TryParse(item, out rola))
                {
                    continue;   //pre ine role sa nic nedeje
                }
                roles.Add(rola);
            }
            return roles;
        }
        public async Task<List<string>> GetRolesFromUser(IdentityUserOwn user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
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
            return await AddRoleToUser(userId, roleName.ToString());
        }
        public async Task<bool> AddRoleToUser(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(user); //kontrola existencie
            if (roles.Contains(roleName))
            {
                return false;
            }

            return (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;   //rola musi existovat
        }

        public async Task<bool> RemoveRoleFromUser(string userId, RolesOwn role)
        {
            return await RemoveRoleFromUser(userId, role.ToString());
        }
        public async Task<bool> RemoveRoleFromUser(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            return (await _userManager.RemoveFromRoleAsync(user, role)).Succeeded;
        }

        public bool IsLoggedUserInRole(RolesOwn role)
        {
            return LoggedUserRoles.Contains(role);
        }

        public bool IsLoggedUserInRoles(IList<RolesOwn> roles)
        {
            foreach (var item in roles)
            {
                if (LoggedUserRoles.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Updatne usera
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUser(IdentityUserOwn user)
        {
            //var found = await _userManager.FindByIdAsync(user.Id);
            //if (found is null)
            //{
            //    return false;
            //}
            //found.UserName = user.UserName;
            //found.Name = user.Name;
            //found.Surname = user.Surname;
            //found.Email = user.Email;
            //found.PhoneNumber = user.PhoneNumber;
            //found.IBAN = user.IBAN;
            //found.Adresa = user.Adresa;
            //found.RodneCislo = user.RodneCislo;
            //found.ObcianskyID = user.ObcianskyID;
            //found.Sex = user.Sex;
            //found.Nationality = user.Nationality;
            return (await _userManager.UpdateAsync(user)).Succeeded;
        }

        public async Task<bool> DeleteUser(IdentityUserOwn user)
        {
            var found = await _userManager.FindByIdAsync(user.Id);
            if (found is null)
            {
                return false;
            }
            return (await _userManager.DeleteAsync(found)).Succeeded;
        }

        public bool IsThisUserLoggedIn(IdentityUserOwn user)
        {
            if (LoggedUser is null)
            {
                return false;
            }
            return LoggedUser.Id == user.Id;
        }

        public async Task<IdentityUserOwn?> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

    }

}
