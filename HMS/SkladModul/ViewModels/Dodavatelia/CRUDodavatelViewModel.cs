using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace SkladModul.ViewModels.Dodavatelia
{
    public partial class CRUDodavatelViewModel : ObservableObject
    {

        #region polia na vyplnenie

        [Required(ErrorMessage = "PovinnÈ pole")]
        [RegularExpression(@"^\d+$", ErrorMessage = "MusÌ skladaù len z ËÌsiel.")]
        [MinLength(8, ErrorMessage = "Minim·lne 8 znaky.")]
        [MaxLength(8, ErrorMessage = "Maxim·lne 8 znakov.")]
        [UniqueICO]
        public string ICO { get; set; } = "";

        [Required(ErrorMessage = "PovinnÈ pole")]
        [MinLength(3, ErrorMessage = "Minim·lne 3 znaky.")]
        [MaxLength(128, ErrorMessage = "Maxim·lne 128 znakov.")]
        public string Nazov { get; set; } = "";

        [Required(ErrorMessage = "PovinnÈ pole")]
        [MinLength(3, ErrorMessage = "Minim·lne 3 znaky.")]
        [MaxLength(128, ErrorMessage = "Maxim·lne 128 znakov.")]
        public string Obec { get; set; } = "";

        [Required(ErrorMessage = "PovinnÈ pole")]
        [MinLength(3, ErrorMessage = "Minim·lne 3 znaky.")]
        [MaxLength(256, ErrorMessage = "Maxim·lne 256 znakov.")]
        public string Adresa { get; set; } = "";

        [RegularExpression(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]{1,30}$", ErrorMessage = "NeplatnÈ IBAN ËÌslo.")]
        public string IBAN { get; set; } = "";

        #endregion

        public Dodavatel Dod { get; set; } = new();

        private readonly DBContext _db;
        private readonly UserService _userService;

        public CRUDodavatelViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Dodavatel.ROLE_CRUD_DODAVATELIA);
        }

        public void SetProp(Dodavatel dod)
        {
            ICO = dod.ICO;
            Nazov = dod.Nazov;
            Obec = dod.Obec;
            Adresa = dod.Adresa;
            IBAN = dod.Iban;
            Dod = dod;
        }

        public async Task<bool> Uloz()
        {
            if (!Existuje())
            {
                Dod.ICO = ICO;
                Dod.Nazov = Nazov;
                Dod.Obec = Obec;
                Dod.Adresa = Adresa;
                Dod.Iban = IBAN;
                _db.Dodavatelia.Add(Dod);
            }
            else
            {
                Dod.Nazov = Nazov;
                Dod.Obec = Obec;
                Dod.Adresa = Adresa;
                Dod.Iban = IBAN;
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public bool Existuje()
        {
            return !string.IsNullOrEmpty(Dod.ICO);
        }
    }

    public class UniqueICOAttribute : ValidationAttribute      //vlastny atribut pre kontrolu existujuceho ICO. Pomoc od AI
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dbContext = (DBContext)validationContext.GetService(typeof(DBContext));
            var ICO = value as string;

            if (dbContext.Dodavatelia.Any(u => u.ICO == ICO))
            {
                return new ValidationResult("I»O uû existuje.");
            }

            return ValidationResult.Success;
        }
    }
}