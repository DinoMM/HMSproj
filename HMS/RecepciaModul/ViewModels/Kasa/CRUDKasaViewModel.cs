using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Kkasa = DBLayer.Models.Kasa;
namespace RecepciaModul.ViewModels
{
    public partial class CRUDKasaViewModel : ObservableObject
    {
        public List<Kkasa> ZoznamKas { get; private set; } = new();
        public List<Dodavatel> ZoznamOrganizacii { get; private set; } = new();

        public Kkasa KasaInput { get; set; } = new();
        private bool exist = false;

        public bool NacitavaniePoloziek { get; private set; } = true;

        private readonly DBContext _db;
        private readonly UserService _userService;

        public CRUDKasaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Kkasa.ROLE_CRUD_KASA);
        }

        public async Task NacitajZoznamy()
        {
            ZoznamKas = new(await _db.Kasy
                .Include(x => x.DodavatelX)
                .Include(x => x.ActualUserX)
                .ToListAsync());
            ZoznamOrganizacii = new(await _db.Dodavatelia.ToListAsync());
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(Kkasa item)
        {
            if (_db.PokladnicneDoklady.Any(x => x.Kasa == item.ID) || ZoznamKas.Any(x => x.ID == item.ID && x.ActualUser != null))
            {
                return false;
            }
            return true;
        }

        public bool Vymazat(Kkasa item)
        {
            var found = _db.Kasy.FirstOrDefault(x => x.ID == item.ID);
            if (found == null)
            {
                return false;
            }
            _db.Kasy.Remove(found);
            ZoznamKas.Remove(item);
            _db.SaveChanges();
            return true;
        }

        public bool Existuje()
        {
            return exist;
        }

        public void SetKasa(Kkasa nova)
        {
            KasaInput = nova.Clon();
            exist = !string.IsNullOrEmpty(nova.ID);
        }

        public bool Ulozit()
        {
            if (!Existuje())
            {
                if (_db.Kasy.Any(x => x.ID == KasaInput.ID))
                {
                    return false;
                }
                _db.Kasy.Add(KasaInput);
                ZoznamKas.Add(KasaInput);
            }
            else
            {
                var found = _db.Kasy.FirstOrDefault(x => x.ID == KasaInput.ID);
                if (found == null)
                {
                    return false;
                }
                found.SetFrom(KasaInput);
            }

            _db.SaveChanges();
            exist = true;
            return true;
        }
    }
}