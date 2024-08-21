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
        [Required(ErrorMessage = "Povinn� pole")]
        [MinLength(6, ErrorMessage = "Minim�lne 6 znakov.")]
        [MaxLength(128, ErrorMessage = "Maxim�lne 128 znakov.")]
        [RegularExpression("^[a-z0-9]+$", ErrorMessage = "M��e obsahova� iba mal� p�smen� a ��slice.")]
        [UniqueUserName]
        public string UserName { get; set; } = default!;

        [Required(ErrorMessage = "Povinn� pole")]
        [MinLength(2, ErrorMessage = "Minim�lne 2 znaky.")]
        [MaxLength(64, ErrorMessage = "Maxim�lne 64 znakov.")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Povinn� pole")]
        [MinLength(2, ErrorMessage = "Minim�lne 2 znaky.")]
        [MaxLength(64, ErrorMessage = "Maxim�lne 64 znakov.")]
        public string Surname { get; set; } = default!;

        [Required(ErrorMessage = "Povinn� pole")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{6,}$", ErrorMessage = "Heslo mus� obsahova� 6 znakov, 1x ve�k� p�smeno, 1x mal� p�smeno a 1x ��slicu")]
        public string Heslo { get; set; } = default!;

        [Required(ErrorMessage = "Povinn� pole")]
        [Compare("Heslo", ErrorMessage = "Hesl� sa nezhoduj�")]
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
                return new ValidationResult("UserName u� existuje.");
            }

            return ValidationResult.Success;
        }
    }

}