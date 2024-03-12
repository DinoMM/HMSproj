using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer
{
    public class DbInitializeService
    {
        readonly UserManager<IdentityUserOwn> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;

        private bool _done = false;

        public DbInitializeService(UserManager<IdentityUserOwn> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task TryMustHaveValues()
        {
            if (!_done)
            {
                foreach (var roleName in Enum.GetValues(typeof(RolesOwn)))  //ini aby boli pristupne vsetky role
                {
                    var roleStr = ((RolesOwn)roleName).ToString();
                    var roleExist = await _roleManager.RoleExistsAsync(roleStr);
                    if (!roleExist)
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleStr));
                    }
                }
                var res = await _userManager.FindByNameAsync("admin");  //pridanie admina
                if (res == null)
                {
                    var admin = new IdentityUserOwn() { UserName = "admin", Email = "admin@admin.com", Name = "Admin", Surname = "Admin" };
                    await _userManager.CreateAsync(admin, "Heslo123");
                    await _userManager.AddToRoleAsync(admin, RolesOwn.Admin.ToString());
                }

                _done = true;
            }
        }

    }
}
