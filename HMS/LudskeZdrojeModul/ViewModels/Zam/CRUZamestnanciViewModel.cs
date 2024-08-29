using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LudskeZdrojeModul.ViewModels.Zamestnanci
{
    public partial class CRUZamestnanciViewModel : ObservableObject
    {
        #region polia na vyplnenie

        [Required(ErrorMessage = "Povinné pole")]
        [MinLength(2, ErrorMessage = "Minimálne 2 znaky.")]
        [MaxLength(64, ErrorMessage = "Maximálne 64 znakov.")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Povinné pole")]
        [MinLength(2, ErrorMessage = "Minimálne 2 znaky.")]
        [MaxLength(64, ErrorMessage = "Maximálne 64 znakov.")]
        public string Surname { get; set; } = default!;

        [MaxLength(128, ErrorMessage = "Maximálne 128 znakov.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Neplatná emailová adresa.")]
        public string Email { get; set; } = "";

        [RegularExpression(@"^\+?[0-9\s\-()]{7,15}$", ErrorMessage = "Neplatné telefónne èíslo.")]
        public string PhoneNumber { get; set; } = "";

        [RegularExpression(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]{1,30}$", ErrorMessage = "Neplatné IBAN èíslo.")]
        public string IBAN { get; set; } = "";

        [MaxLength(256, ErrorMessage = "Maximálne 256 znakov.")]
        public string Adresa { get; set; } = "";

        #endregion

        public IdentityUserOwn? User { get; set; } = default!;

        public List<IdentityRole> RoleUsera { get; set; } = new();
        public List<IdentityRole> RoleUseraPred { get; set; } = new();
        public List<RolesOwn> RoleVsetkyDef { get; set; } = new();
        public List<IdentityRole> RoleVsetky { get; set; } = new();
        [ObservableProperty]
        private bool checkBoxRole = false;
        [ObservableProperty]
        private bool savedRole = true;


        private readonly DBContext _db;
        private readonly UserService _userService;

        public CRUZamestnanciViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(IdentityUserOwn.ROLE_CRUD_ZAMESTNANCI);
        }

        public void SetProp(IdentityUserOwn user)
        {
            Name = user.Name;
            Surname = user.Surname;
            Email = user.Email ?? "";
            PhoneNumber = user.PhoneNumber ?? "";
            IBAN = user.IBAN ?? "";
            Adresa = user.Adresa ?? "";
            User = user;

        }

        public async Task NacitajZoznamy()
        {
            RoleVsetkyDef = Enum.GetValues(typeof(RolesOwn)).Cast<RolesOwn>().ToList();
            RoleVsetky = new(_db.Roles.ToList());


            var first = RoleVsetky.FirstOrDefault(x => x.Name == RolesOwn.None.ToString());
            RoleVsetky.Remove(first);
            RoleVsetky = RoleVsetky.OrderBy(x => x.Name).ToList();
            var list = new List<IdentityRole>() { first };
            list.AddRange(RoleVsetky);
            RoleVsetky = list;
        }

        public async Task SetUserRole()
        {
            if (User is not null)
            {
                var listUR = _db.UserRoles.Where(x => x.UserId == User.Id).ToList();
                foreach (var item in listUR)
                {
                    var pridan = _db.Roles.FirstOrDefault(x => x.Id == item.RoleId);
                    RoleUsera.Add(pridan);
                    RoleUseraPred.Add(pridan);
                }
                CheckBoxRole = RoleUsera.Exists(x => x.Name == RolesOwn.None.ToString());
            }
        }

        public bool IsDefaultRole(IdentityRole role)
        {
            if (string.IsNullOrEmpty(role.Name))
            {
                return false;
            }
            return Enum.TryParse<RolesOwn>(role.Name, out _);
        }

        public void VyberRolu(IdentityRole rola)  //ak neni tak sa prida, ak je tak sa odobere
        {
            var found = RoleUsera.FirstOrDefault(x => x.Id == rola.Id);
            if (found is null)
            {
                RoleUsera.Add(rola);
                return;

            }
            RoleUsera.Remove(found);
            return;
        }

        public async Task<bool> Uloz()
        {
            if (User is null)      //ak nema id -> vytvarame noveho zamestnanca
            {
                User = new();
                User.Name = Name;
                User.Surname = Surname;
                User.Email = Email;
                User.PhoneNumber = PhoneNumber;
                User.IBAN = IBAN;
                User.Adresa = Adresa;

                if (!await _userService.CreateNewUserNoPSWDAsync(User))
                {
                    return false;
                }

                await _db.Entry(User).ReloadAsync();

                foreach (var item in RoleUsera)
                {
                    await _userService.AddRoleToUser(User.Id, item.Name);
                }
                return true;
            }
            else
            {
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

                foreach (var item in RoleUseraPred)        //najskor vymazeme vsetky zaznamy
                {
                    await _userService.RemoveRoleFromUser(User.Id, item.Name);
                }
                foreach (var item in RoleUsera)     //pridame nove
                {
                    await _userService.AddRoleToUser(User.Id, item.Name);
                }
                return true;
            }

        }


    }
}