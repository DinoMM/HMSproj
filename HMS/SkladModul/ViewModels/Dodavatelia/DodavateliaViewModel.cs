using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UniComponents;
using Ddodavatel = DBLayer.Models.Dodavatel;

namespace SkladModul.ViewModels.Dodavatelia
{
    public partial class DodavateliaViewModel
    {
        public ObservableCollection<Ddodavatel> ZoznamDodavatelov { get; set; } = new();

        public bool NacitavaniePoloziek { get; set; } = true;

        private readonly DBContext _db;
        private readonly UserService _userService;

        [CopyProperties]
        public ComplexTable<DBLayer.Models.Dodavatel>? Complextable { get; set; }

        public DodavateliaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Ddodavatel.ROLE_R_DODAVATELIA);
        }

        public bool ValidateUserCRUD()
        {
            return _userService.IsLoggedUserInRoles(Ddodavatel.ROLE_CRUD_DODAVATELIA);
        }

        public async Task NacitajZoznamy()
        {
            NacitavaniePoloziek = true;
            ZoznamDodavatelov.CollectionChanged -= OnCollectionChanged;
            ZoznamDodavatelov = new(await _db.Dodavatelia.OrderBy(x => x.Nazov).ToListAsync());
            ZoznamDodavatelov.CollectionChanged += OnCollectionChanged;
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(Ddodavatel dod)
        {
            var found = _db.Objednavky.Any(x => x.Dodavatel == dod.ICO || x.Odberatel == dod.ICO);    //nesmie byt v objednavkach
            if (found)
            {
                return false;
            }
            return true;
        }

        public async Task Vymazat(Ddodavatel dod)
        {
            ZoznamDodavatelov.Remove(dod);
            _db.Dodavatelia.Remove(dod);
            await _db.SaveChangesAsync();
        }

        private async void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (Complextable != null)
            {
                await Complextable.OnItemsChange();
            }
        }
    }
}