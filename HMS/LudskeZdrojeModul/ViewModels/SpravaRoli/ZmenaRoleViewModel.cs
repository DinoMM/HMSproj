using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace LudskeZdrojeModul.ViewModels.SpravaRoli
{
    public partial class ZmenaRoleViewModel : ObservableObject
    {
        #region polia na vyplnenie
        [Required]
        [MinLength(3, ErrorMessage = "Profesia musÌ maù minim·lne 3 znaky.")]
        [MaxLength(128, ErrorMessage = "Profesia musÌ maù maxim·lne 128 znakov.")]
        public string RoleName { get; set; } = default!;
        #endregion

        public IdentityRole Role { get; set; } = default!;


        private readonly DBContext _db;
        private readonly UserService _userService;
        private readonly RoleService _roleService;

        public ZmenaRoleViewModel(DBContext db, UserService userService, RoleService roleService)
        {
            _db = db;
            _userService = userService;
            _roleService = roleService;
        }
        

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(IdentityUserOwn.ROLE_CRUD_ROLI);

        }

        public void SetRole(IdentityRole role)
        {
            Role = role;
            RoleName = role.Name ?? "";
        }

        public async Task<bool> Uloz()
        {
            var res = await _roleService.UpdateRole(Role, RoleName);
            if (res)
            {
                await _db.Entry(Role).ReloadAsync();
            }
            return res;
        }


    }
}