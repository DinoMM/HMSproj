using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;
using Kkasa = DBLayer.Models.Kasa;
using PpokladnicnyDoklad = DBLayer.Models.PokladnicnyDoklad;

namespace RecepciaModul.ViewModels
{
    public partial class PokladnicnyDokladViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<DBLayer.Models.ItemPokladDokladu> zoznamPoloziek = new();

        public bool NacitavaniePoloziek { get; private set; } = true;

        public Kkasa AktKasa { get; set; } = new();

        public PpokladnicnyDoklad PoklDokladInput { get; set; } = new();

        private bool existuje = false;

        private readonly DBContext _db;
        private readonly DBContext _dbw;
        private readonly UserService _userService;
        private readonly IJSRuntime _jsRuntime;

        public PokladnicnyDokladViewModel(DBContext db, UserService userService, DBContext dbw, IJSRuntime jsRuntime)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;
            _jsRuntime = jsRuntime;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(PpokladnicnyDoklad.ROLE_R_POKLDOKL);
        }

        public bool ValidateUserCRU()
        {
            return _userService.IsLoggedUserInRoles(PpokladnicnyDoklad.ROLE_CRU_POKLDOKL);
        }

        public bool ValidateUserD()
        {
            return _userService.IsLoggedUserInRoles(PpokladnicnyDoklad.ROLE_D_POKLDOKL);
        }

        public async Task NacitajZoznamy()
        {
            //nacitat zoznam poloziek
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(ItemPokladDokladu item)
        {
            return false;
        }

        public void Vymazat(ItemPokladDokladu item)
        {

        }

        public void SetPoklDokl(PpokladnicnyDoklad item)
        {
            PoklDokladInput = item.Clon();
            existuje = true;
        }

        public void PocessUniConItem(DBLayer.Models.UniConItemPoklDokladu item)
        {
            if (!Existuje())
            {
               
            }

        }

        public bool Existuje()
        {
            return existuje;
        }
    }
}