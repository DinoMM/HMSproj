using BuildingTemplates;
using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace UctovnyModul.ViewModels.Faktury
{
    public partial class FakturyViewModel : ObservableObject, I_ValidationVM, I_RD_TableVM<Faktura>
    {
        [ObservableProperty]
        ObservableCollection<Faktura> zoznamFaktur = new();

        public bool NacitavaniePoloziek { get; private set; } = true;

        private readonly DBContext _db;
        private readonly UserService _userService;

        

        public FakturyViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Faktura.ROLE_R_FAKTURY);
        }

        public bool ValidateUserCRU()
        {
            return _userService.IsLoggedUserInRoles(Faktura.ROLE_CRUD_FAKTURY);
        }

        public async Task NacitajZoznamy()
        {
            ZoznamFaktur = new(await _db.Faktury
                .Include(x => x.SkupinaX)
                .Include(x => x.OdKohoX)
                .OrderBy(x => x.Vystavenie)
                .ToListAsync());
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(Faktura item)
        {
            return false;
        }

        public void Vymazat(Faktura item)
        {


        }

        
    }
}