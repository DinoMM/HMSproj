using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace HMS.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        #region polia na vyplnenie
        [Required(ErrorMessage = "Povinné pole")]
        public string UserName { get; set; } = "";
        [Required(ErrorMessage = "Povinné pole")]
        public string Password { get; set; } = "";
        #endregion

        private readonly UserService _userService;

        public LoginViewModel(UserService userService)
        {
            _userService = userService;
        }

        public bool IsLoggedIn()
        {
            return _userService.LoggedUser != null;
        }

        public async Task<bool> Login()
        {
            return await _userService.LogInUserAsync(UserName, Password);
        }

    }
}