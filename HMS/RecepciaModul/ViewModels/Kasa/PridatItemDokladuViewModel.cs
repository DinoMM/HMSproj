using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Kkasa = DBLayer.Models.Kasa;
using PpokladnicnyDoklad = DBLayer.Models.PokladnicnyDoklad;

namespace RecepciaModul.ViewModels
{
    public partial class PridatItemDokladuViewModel : ObservableObject
    {
        public ItemPokladDokladu ItemPokladDokladuInput { get; set; } = new();

        public List<DBLayer.Models.UniConItemPoklDokladu> ZoznamUniConItems { get; set; }

        private readonly DBContext _db;
        private readonly UserService _userService;

        public PridatItemDokladuViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(PpokladnicnyDoklad.ROLE_CRU_POKLDOKL);
        }

        public bool Existuje()
        {
            return ItemPokladDokladuInput.ID != 0;
        }

        public void SetItem(ItemPokladDokladu item)
        {
            ItemPokladDokladuInput = item.Clon();
        }

        public bool Ulozit()
        {
            return true;
        }
    }
}