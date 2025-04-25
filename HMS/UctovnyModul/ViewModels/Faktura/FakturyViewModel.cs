using BuildingTemplates;
using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using UniComponents;

namespace UctovnyModul.ViewModels
{
    public partial class FakturyViewModel : AObservableViewModel<Faktura>
    {
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

        protected override async Task NacitajZoznamyAsync()
        {
            ZoznamPoloziek = new(await _db.Faktury
                .Include(x => x.SkupinaX)
                .Include(x => x.OdKohoX)
                .OrderByDescending(x => x.Vystavenie)
                .ToListAsync());
        }

        public override bool MoznoVymazat(Faktura item)
        {
            return item.Spracovana == null;
        }

        public override void Vymazat(Faktura item)
        {
            base.Vymazat(item);
            _db.Faktury.Remove(item);
            _db.SaveChanges();
        }

        
    }
}