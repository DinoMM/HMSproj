using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;

namespace RecepciaModul.ViewModels
{
    public partial class KasaViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<DBLayer.Models.PokladnicnyDoklad> zoznamBlockov = new();

        public bool NacitavaniePoloziek { get; private set; } = true;

        public DBLayer.Models.Kasa? AktualKasa { get; set; } = null;



        private readonly DBContext _db;
        private readonly UserService _userService;
        private readonly IJSRuntime _jsRuntime;

        public KasaViewModel(DBContext db, UserService userService, IJSRuntime jsRuntime)
        {
            _db = db;
            _userService = userService;
            _jsRuntime = jsRuntime;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(DBLayer.Models.PokladnicnyDoklad.ROLE_R_POKLDOKL) && _userService.IsLoggedUserInRoles(DBLayer.Models.Kasa.ROLE_R_KASA);
        }

        public async Task NacitajZoznamy()
        {

            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(DBLayer.Models.PokladnicnyDoklad item)
        {
            return false;
        }

        public void Vymazat(DBLayer.Models.PokladnicnyDoklad item)
        {


        }
    }
}