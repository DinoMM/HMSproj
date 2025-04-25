using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using UniComponents;

namespace AdminModul.ViewModels.Pouzivatelia
{
    public partial class ZmenaPouzivatelaViewModel : ObservableObject
    {
        #region polia na vyplnenie
        public string UserName { get; set; } = default!;

        [Required(ErrorMessage = "PovinnÈ pole")]
        [MinLength(2, ErrorMessage = "Minim·lne 2 znaky.")]
        [MaxLength(64, ErrorMessage = "Maxim·lne 64 znakov.")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "PovinnÈ pole")]
        [MinLength(2, ErrorMessage = "Minim·lne 2 znaky.")]
        [MaxLength(64, ErrorMessage = "Maxim·lne 64 znakov.")]
        public string Surname { get; set; } = default!;

        [MaxLength(128, ErrorMessage = "Maxim·lne 128 znakov.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Neplatn· emailov· adresa.")]
        public string Email { get; set; } = "";

        [RegularExpression(@"^\+?[0-9\s\-()]{7,15}$", ErrorMessage = "NeplatnÈ telefÛnne ËÌslo.")]
        public string PhoneNumber { get; set; } = "";

        [RegularExpression(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]{1,30}$", ErrorMessage = "NeplatnÈ IBAN ËÌslo.")]
        public string IBAN { get; set; } = "";

        [MaxLength(256, ErrorMessage = "Maxim·lne 256 znakov.")]
        public string Adresa { get; set; } = "";

        #endregion

        public IdentityUserOwn User { get; set; } = default!;

        public List<RolesOwn> RoleUsera { get; set; } = new();
        public List<RolesOwn> RoleVsetky { get; set; } = new();

        public object? MiniViewModel { get; set; } = null;

        [ObservableProperty]
        private bool checkBoxRole = false;

        [ObservableProperty]
        private bool savedRole = true;

        [ObservableProperty]
        private bool checkLock = false;

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
            Email = user.Email ?? string.Empty;
            PhoneNumber = user.PhoneNumber ?? "";
            IBAN = user.IBAN ?? "";
            Adresa = user.Adresa ?? "";


            RoleVsetky = Enum.GetValues(typeof(RolesOwn)).Cast<RolesOwn>().ToList();
            var first = RoleVsetky.FirstOrDefault();
            RoleVsetky.Remove(first);
            RoleVsetky = RoleVsetky.OrderBy(x => x.ToString()).ToList();
            var list = new List<RolesOwn>() { first };
            list.AddRange(RoleVsetky);
            RoleVsetky = list;
        }

        public async Task SetUserRole()
        {
            RoleUsera = new(await _userService.GetRolesOwnFromUser(User));
            CheckBoxRole = RoleUsera.Contains(RolesOwn.None);
        }

        public async Task<bool> Uloz()
        {
            //User.UserName = UserName;
            User.Name = Name;
            User.Surname = Surname;
            User.Email = Email;
            User.PhoneNumber = PhoneNumber;
            User.IBAN = IBAN;
            User.Adresa = Adresa;
            if (!await _userService.UpdateUser(User))
            {
                return false;
            }
            await _db.Entry(User).ReloadAsync();

            //var zozOzn = _db.UserRoles.Where(x => x.UserId == User.Id).ToList();
            //var zozRole = _db.Roles.ToList();
            //foreach (var item in zozOzn)        //najskor vymazeme vsetky zaznamy
            //{
            //    var role = zozRole.FirstOrDefault(x => x.Id == item.RoleId).Name;
            //    await _userService.RemoveRoleFromUser(User.Id, (RolesOwn)Enum.Parse(typeof(RolesOwn), role));
            //}

            var zozUsRol = await _userService.GetRolesOwnFromUser(User);
            foreach (var item in zozUsRol)        //najskor vymazeme vsetky zaznamy
            {
                await _userService.RemoveRoleFromUser(User.Id, item);
            }
            foreach (var item in RoleUsera)        //potom pridame role
            {
                await _userService.AddRoleToUser(User.Id, item);
            }
            SavedRole = true;
            return true;
        }

        public async Task<bool> PromoteToPouzivatel(string meno, string heslo, string hesloZnova, IModal modal)
        {
            var errString = "";
            if (_db.Users.Any(u => u.UserName == meno))     //kontrola username, musim tu lebo v niûöom kode to nefungovalo
            {
                errString = "PouûÌvateæ uû existuje. <br>";
            }
            if (string.IsNullOrEmpty(meno))
            {
                errString += "UserName je povinnÈ pole. <br>";
            }
            if (meno.Length < 6)
            {
                errString += "UserName musÌ maù minim·lne 6 znakov. <br>";
            }
            if (meno.Length > 128)
            {
                errString += "UserName musÌ maù maxim·lne 128 znakov. <br>";
            }
            if (!Regex.IsMatch(meno, "^[a-z0-9]+$"))
            {
                errString += "UserName mÙûe obsahovaù iba malÈ pÌsmen· a ËÌslice. <br>";
            }
            {
                var viewModel = new AddPouzivatelViewModel(null, null)      //dalej postup na kontrolu hesiel, beriem si instanciu z AddPouzivatelViewModel kde to otestujem programovo, pomoc od AI
                {
                    Heslo = heslo,
                    HesloRovnake = hesloZnova
                };

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(viewModel);

                bool isValid = Validator.TryValidateObject(viewModel, validationContext, validationResults, true);

                if (!isValid)
                {

                    foreach (var validationResult in validationResults)
                    {
                        if (validationResult.MemberNames.Contains("Heslo") || validationResult.MemberNames.Contains("HesloRovnake"))     //kontrolujeme len hesla, kedze ostatne fieldy budu davat errory
                            errString += validationResult.ErrorMessage + "<br>";
                    }

                }
            }
            if (string.IsNullOrEmpty(errString))    //ak sme nedostali ziadne error spr·vy tak nemenim text v modali, ukladame usera
            {
                var existUser = _db.Users.Local.FirstOrDefault(x => x.Id == User.Id);
                if (existUser != null)
                {
                    _db.Entry(existUser).State = EntityState.Detached;
                }
                _db.Entry(User).State = EntityState.Modified;


                User.UserName = meno;
                UserName = meno;

                await _db.SaveChangesAsync();

                await _userService.ChangePassword(User.Id, heslo, meno);

                await _db.Entry(User).ReloadAsync();

                return true;
            }
            modal.UpdateText(errString);
            return false;
        }

        public void VyberRolu(RolesOwn rola)  //ak neni tak sa prida, ak je tak sa odobere
        {
            if (!RoleUsera.Remove(rola))
            {
                RoleUsera.Add(rola);
                CheckBoxRole = true;
                return;
            }
            CheckBoxRole = false;
            return;
        }

        public DBContext GetDB()
        {
            return _db;
        }

        public async Task<string?> ZmenitHeslo(string heslo, string hesloZnova)
        {
            string errString = "";
            {
                var viewModel = new AddPouzivatelViewModel(null, null)      //dalej postup na kontrolu hesiel, beriem si instanciu z AddPouzivatelViewModel kde to otestujem programovo, pomoc od AI
                {
                    Heslo = heslo,
                    HesloRovnake = hesloZnova
                };

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(viewModel);

                bool isValid = Validator.TryValidateObject(viewModel, validationContext, validationResults, true);

                if (!isValid)
                {

                    foreach (var validationResult in validationResults)
                    {
                        if (validationResult.MemberNames.Contains("Heslo") || validationResult.MemberNames.Contains("HesloRovnake"))     //kontrolujeme len hesla, kedze ostatne fieldy budu davat errory
                            errString += validationResult.ErrorMessage + "<br>";
                    }

                }
            }
            if (string.IsNullOrEmpty(errString))    //ak sme nedostali ziadne error spr·vy tak nemenim text v modali, ukladame usera
            {
                if (string.IsNullOrEmpty(User.UserName))
                {
                    errString += "username neni priradenÈ.";
                    return errString;
                }
                if (!await _userService.ChangePassword(User.Id, heslo, User.UserName))
                {
                    errString += "Nepodarilo sa zmeniù heslo.";
                }

                await _db.Entry(User).ReloadAsync();
            }

            return errString;
        }

    }
}