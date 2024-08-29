using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Ssklad = DBLayer.Models.Sklad;

namespace SkladModul.ViewModels.Sklady
{
    
    public partial class CRUSkladViewModel : ObservableObject
    {
        #region polia na vyplnenie

        [Required(ErrorMessage = "Povinné pole")]
        [MinLength(2, ErrorMessage = "Minimálne 2 znaky.")]
        [MaxLength(5, ErrorMessage = "Maximálne 5 znakov.")]
        [UniqueSkladId]
        public string ID { get; set; } = default!;

        [Required(ErrorMessage = "Povinné pole")]
        [MinLength(3, ErrorMessage = "Minimálne 3 znaky.")]
        [MaxLength(64, ErrorMessage = "Maximálne 64 znakov.")]
        public string Nazov { get; set; } = default!;

        #endregion
        public Ssklad SkladAkt { get; set; } = new();

        private readonly DBContext _db;
        private readonly UserService _userService;

        public CRUSkladViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Ssklad.ROLE_CRUD_SKLAD);
        }

        public void SetProp(Ssklad sklad)
        {
            ID = sklad.ID;
            Nazov = sklad.Nazov;
            SkladAkt = sklad;
        }

        public async Task<bool> Uloz()
        {
            if (!Existuje())
            {
                SkladAkt.ID = ID;
                SkladAkt.Nazov = Nazov;
                _db.Sklady.Add(SkladAkt);
            }
            else
            {
                SkladAkt.Nazov = Nazov;
            }
            await _db.SaveChangesAsync();
            return true;
        }

        public bool Existuje()
        {
            return !string.IsNullOrEmpty(SkladAkt.ID);
        }
    }
    public class UniqueSkladIdAttribute : ValidationAttribute      //vlastny atribut pre kontrolu existujuceho ICO. Pomoc od AI
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dbContext = (DBContext)validationContext.GetService(typeof(DBContext));
            var Id = value as string;

            if (dbContext.Sklady.Any(u => u.ID == Id))
            {
                return new ValidationResult("Sklad už existuje.");
            }

            return ValidationResult.Success;
        }
    }
}