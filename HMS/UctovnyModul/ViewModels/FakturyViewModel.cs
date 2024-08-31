using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace UctovnyModul.ViewModels.Faktury
{
    public partial class FakturyViewModel : ObservableObject
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

        public async Task NacitajZoznamy()
        {
            //ZoznamFaktur = new(_db.)
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