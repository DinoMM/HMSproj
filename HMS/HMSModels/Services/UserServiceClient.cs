using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSModels.Services
{
    public class UserServiceClient
    {
        public IdentityUserOwn? LoggedUser { get; set; } = default!;
        public List<RolesOwn> LoggedUserRoles { get; set; } = new();
        public string JwtToken { get; set; } = string.Empty;

        /// <summary>
        /// Spustí sa po úspešnom prohlásení uživateľa
        /// </summary>
        public Action? OnLogin { get; set; }

        /// <summary>
        /// Spustí sa pred odhlásením používateľa
        /// </summary>
        public Action? OnLogOut { get; set; }

        public void LogInUser(IdentityUserOwn user, List<RolesOwn> roles, string token)
        {
            LoggedUser = user;
            LoggedUserRoles = roles.ToList();

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("JWT token je prázdny");
            }
            JwtToken = token;
        }

        public void LogOutUser()
        {
            if (OnLogOut != null)
            {
                OnLogOut();
            }
            JwtToken = string.Empty;
            LoggedUser = null;
            LoggedUserRoles.Clear();
        }

        public bool IsUserLogged()
        {
            return LoggedUser != null;
        }

        public bool IsUserInRole(RolesOwn role)
        {
            return LoggedUserRoles.Contains(role);
        }

        public bool IsLoggedUserInRoles(params RolesOwn[] roles)
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
        Udrzbar,
        RCVeduci
    };
}
