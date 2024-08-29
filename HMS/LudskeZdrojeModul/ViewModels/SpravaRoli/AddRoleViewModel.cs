using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace LudskeZdrojeModul.ViewModels.SpravaRoli
{
    public partial class AddRoleViewModel : ObservableObject
    {
        #region polia na vyplnenie
        [Required]
        [MinLength(3, ErrorMessage = "Profesia musÌ maù minim·lne 3 znaky.")]
        [MaxLength(128, ErrorMessage = "Profesia musÌ maù maxim·lne 128 znakov.")]
        public string RoleName { get; set; } = default!;
        #endregion

        private readonly DBContext _db;
        private readonly UserService _userService;
        private readonly RoleService _roleService;

        public AddRoleViewModel(DBContext db, UserService userService, RoleService roleService)
        {
            _db = db;
            _userService = userService;
            _roleService = roleService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(IdentityUserOwn.ROLE_CRUD_ROLI);
        }

        public async Task<bool> Vytvorit()
        {
            return await _roleService.CreateNewRole(RoleName);
        }

    }
}