using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace AdminModul.ViewModels.Pouzivatelia
{
    public partial class ZmenaPouzivatelaViewModel : ObservableObject
    {
        #region polia na vyplnenie
        public string UserName { get; set; } = default!;

        [Required(ErrorMessage = "Povinn� pole")]
        [MinLength(2, ErrorMessage = "Minim�lne 2 znaky.")]
        [MaxLength(64, ErrorMessage = "Maxim�lne 64 znakov.")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Povinn� pole")]
        [MinLength(2, ErrorMessage = "Minim�lne 2 znaky.")]
        [MaxLength(64, ErrorMessage = "Maxim�lne 64 znakov.")]
        public string Surname { get; set; } = default!;

        [MinLength(5, ErrorMessage = "Minim�lne 5 znakov.")]
        [MaxLength(128, ErrorMessage = "Maxim�lne 128 znakov.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Neplatn� emailov� adresa.")]
        public string Email { get; set; } = default!;

        [RegularExpression(@"^\+?[0-9\s\-()]{7,15}$", ErrorMessage = "Neplatn� telef�nne ��slo.")]
        public string PhoneNumber { get; set; } = default!;

        [RegularExpression(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]{1,30}$", ErrorMessage = "Neplatn� IBAN ��slo.")]
        public string IBAN { get; set; } = default!;

        [MaxLength(256, ErrorMessage = "Maxim�lne 256 znakov.")]
        public string Adresa { get; set; } = default!;

        #endregion

        public IdentityUserOwn User { get; set; } = default!;

        private readonly DBContext _db;
        private readonly UserService _userService;

        public ZmenaPouzivatelaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.LoggedUserRole == _userService.LoggedUserRole; ;
        }
        public void SetUser(IdentityUserOwn user)
        {
            User = user;
            UserName = user.UserName ?? "";
            Name = user.Name;
            Surname = user.Surname;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            //IBAN = user.IBAN;
            //Adresa = user.Adresa;

        }

        public async Task<bool> Uloz()
        {
            User.UserName = UserName;
            User.Name = Name;
            User.Surname = Surname;
            User.Email = Email;
            User.PhoneNumber = PhoneNumber;
            //User.IBAN = IBAN;
            //User.Adresa = Adresa;

            return true;
        }
    }
}