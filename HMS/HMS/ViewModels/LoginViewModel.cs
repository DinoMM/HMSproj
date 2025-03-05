using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using HMSModels.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Platform;
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
        private readonly UserServiceClient _userServiceClient;
        private readonly HMSModels.HttpClientService<object> _httpClient;

        public LoginViewModel(UserService userService, HMSModels.HttpClientService<object> httpClient, UserServiceClient userServiceClient)
        {
            _userService = userService;
            _httpClient = httpClient;
            _userServiceClient = userServiceClient;
        }

        public bool IsLoggedIn()
        {
            return _userService.LoggedUser != null;
        }

        public async Task<bool> Login()
        {
            //return await _userService.LogInUserAsync(UserName, Password);
            var res = await _httpClient.LoginAsync(username: UserName, password: Password);
            var storage = new MauiStorageManager.MauiStorageManager();
            if (res == null)
            {
                storage.RemoveDataLocalySecure("JWT_TOKEN");
                return false;
            }
            else
            {
                //TODO vymazat
                await _userService.LogInUserAsync(UserName, Password);

                _userServiceClient.LogInUser(res.User, res.Roles, res.Token);
                await storage.SaveDataLocalySecure("JWT_TOKEN", res.Token);
                _userServiceClient.OnLogOut = () =>
                {
                    var manstorage = new MauiStorageManager.MauiStorageManager();
                    manstorage.RemoveDataLocalySecure("JWT_TOKEN");
                }; // asi netreba
                return true;
            }
        }

    }
}