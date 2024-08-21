using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace AdminModul.ViewModels.Pouzivatelia
{
    public partial class AddPouzivatelViewModel : ObservableObject
    {
        #region polia na vyplnenie
        [Required(ErrorMessage = "PovinnÈ pole")]
        [MinLength(6, ErrorMessage = "Minim·lne 6 znakov.")]
        [MaxLength(128, ErrorMessage = "Maxim·lne 128 znakov.")]
        [RegularExpression("^[a-z0-9]+$", ErrorMessage = "MÙûe obsahovaù iba malÈ pÌsmen· a ËÌslice.")]
        [UniqueUserName]
        public string UserName { get; set; } = default!;

        [Required(ErrorMessage = "PovinnÈ pole")]
        [MinLength(2, ErrorMessage = "Minim·lne 2 znaky.")]
        [MaxLength(64, ErrorMessage = "Maxim·lne 64 znakov.")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "PovinnÈ pole")]
        [MinLength(2, ErrorMessage = "Minim·lne 2 znaky.")]
        [MaxLength(64, ErrorMessage = "Maxim·lne 64 znakov.")]
        public string Surname { get; set; } = default!;

        [Required(ErrorMessage = "PovinnÈ pole")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{6,}$", ErrorMessage = "Heslo musÌ obsahovaù 6 znakov, 1x veækÈ pÌsmeno, 1x malÈ pÌsmeno a 1x ËÌslicu")]
        public string Heslo { get; set; } = default!;

        [Required(ErrorMessage = "PovinnÈ pole")]
        [Compare("Heslo", ErrorMessage = "Hesl· sa nezhoduj˙")]
        public string HesloRovnake { get; set; } = default!;
        #endregion

        private readonly DBContext _db;
        private readonly UserService _userService;

        public AddPouzivatelViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.LoggedUserRole == _userService.LoggedUserRole;
        }

        public async Task<bool> Vytvorit()
        {
            return await _userService.CreateNewUserAsync(new IdentityUserOwn { UserName = UserName, Name = Name, Surname = Surname }, Heslo, RolesOwn.None);
        }
    }

    public class UniqueUserNameAttribute : ValidationAttribute      //vlastny atribut pre kontrolu existujuceho UserName. Pomoc od AI
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dbContext = (DBContext)validationContext.GetService(typeof(DBContext));
            var userName = value as string;

            if (dbContext.Users.Any(u => u.UserName == userName))
            {
                return new ValidationResult("UserName uû existuje.");
            }

            return ValidationResult.Success;
        }
    }

}