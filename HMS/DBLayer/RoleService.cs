using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer
{
    public class RoleService
    {
        readonly RoleManager<IdentityRole> _roleManager;

        public RoleService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<bool> CreateNewRole(string newRole)
        {
            var roleExist = await _roleManager.RoleExistsAsync(newRole);
            if (!roleExist)
            {
                var res = await _roleManager.CreateAsync(new IdentityRole(newRole));
                return res.Succeeded;
            }
            return false;
        }

        public async Task<bool> UpdateRole(IdentityRole role, string newName)
        {
            var roleExist = await _roleManager.RoleExistsAsync(newName);
            if (roleExist)
            {
                return false;
            }

            var currRole = await _roleManager.FindByIdAsync(role.Id);
            if (currRole is null)
            {
                return false;
            }

            currRole.Name = newName;
            var res = await _roleManager.UpdateAsync(currRole);
            return res.Succeeded;
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
