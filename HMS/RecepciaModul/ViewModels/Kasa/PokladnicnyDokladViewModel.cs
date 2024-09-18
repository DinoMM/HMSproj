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

        public Kkasa? AktKasa { get; set; } = null;

        public PpokladnicnyDoklad PoklDokladInput { get; set; } = new();

        public UniConItemPoklDokladu? UniConItem { get; set; }

        private bool existuje = false;

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;
        private readonly IJSRuntime _jsRuntime;
        private readonly Blazored.SessionStorage.ISessionStorageService _sessionStorage;

        public PokladnicnyDokladViewModel(DBContext db, UserService userService, DataContext dbw, IJSRuntime jsRuntime, Blazored.SessionStorage.ISessionStorageService sessionStorage)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;
            _jsRuntime = jsRuntime;
            _sessionStorage = sessionStorage;
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
            if (Existuje())
            {
                ZoznamPoloziek = new(await _db.ItemyPokladDokladu
                    .Include(x => x.SkupinaX)
                    .Include(x => x.UniConItemPoklDokladuX)
                    .Where(x => x.Skupina == PoklDokladInput.ID)
                    .ToListAsync());
            }
            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(ItemPokladDokladu item)
        {
            return false;
        }

        public void Vymazat(ItemPokladDokladu item)
        {

        }

        public bool SetPoklDokl(PpokladnicnyDoklad item)
        {
            PoklDokladInput = item.Clon();
            existuje = true;
            return UniConItemPoklDokladu.JeItemOnlyOnePD(item, in _db);
        }

        /// <summary>
        /// pri conexii, kde moze byt polozka LEN jedna na 1 PD v celom systeme
        /// </summary>
        /// <param name="con"></param>
        public void PocessUNIQUE_UniConItem(DBLayer.Models.UniConItemPoklDokladu con)
        {
            if (!Existuje()) //ak neexistuje
            {
                #region existencia con
                var foundcon = _db.UniConItemyPoklDokladu.FirstOrDefault(x => x.ID == con.ID);
                if (foundcon == null)    //musi existovat instancia
                {
                    return;
                }
                #endregion
                #region existencia pokladnicneho dokladu, nahodenie prvej polozky
                var foundPoklDok = ItemPokladDokladu.GetPD_UniCon_Unique(foundcon, _db);
                if (foundPoklDok == null)   //ak sa nenasiel, tak vytvorime novy, bez ulozenia do DB
                {
                    var itemPD = ItemPokladDokladu.CreateInstanceFromUniCon(foundcon, PoklDokladInput, in _dbw);
                    if (itemPD == null)
                    {
                        return;
                    }
                    ZoznamPoloziek.Add(itemPD); //pridanie do zoznamu na zobrazenie
                }
                else
                {
                    SetPoklDokl(foundPoklDok);
                    //dalej sa predpoklada ze prebehne NacitajZoznamy()
                }
                #endregion
            }
            //ak existuje zo zakladu, tak sa ignoruje vstup Unicon
        }

        public bool Existuje()
        {
            return existuje;
        }

        public async Task<bool> Ulozit()
        {
            if (UniConItem != null)
            {
                PocessUNIQUE_UniConItem(UniConItem);
                await NacitajZoznamy();
            }

            #region ulozenie pokladnicneho dokladu
            if (!Existuje())
            {
                PoklDokladInput.ID = PohSkup.DajNoveID<PpokladnicnyDoklad>(_db.PokladnicneDoklady, _db);
                _db.PokladnicneDoklady.Add(PoklDokladInput);
            }
            else
            {
                var found = _db.PokladnicneDoklady.FirstOrDefault(x => x.ID == PoklDokladInput.ID);
                if (found == null)
                {
                    return false;
                }
                found.Spracovana = PoklDokladInput.Spracovana;
                found.Vznik = PoklDokladInput.Vznik;
                found.Poznamka = PoklDokladInput.Poznamka;
                found.TypPlatby = PoklDokladInput.TypPlatby;
                found.Kasa = PoklDokladInput.Kasa;
                found.KasaX = PoklDokladInput.KasaX;
            }
            #endregion

            #region ulozenie zoznamu poloziek
            var zoznamSave = new List<ItemPokladDokladu>(ZoznamPoloziek.ToList());
            var zoznamDB = _db.ItemyPokladDokladu.Where(x => x.Skupina == PoklDokladInput.ID).ToList();
            foreach (var item in zoznamDB)
            {
                var found = zoznamSave.FirstOrDefault(x => x.ID == item.ID);
                if (found == null)
                {
                    _db.Remove(item);
                }
                else
                {
                    item.SetFrom(found);    //update
                    zoznamSave.Remove(found);
                }
            }
            foreach (var item in zoznamSave)
            {
                _db.ItemyPokladDokladu.Add(item);
            }
            #endregion


            _db.SaveChanges();
            return true;
        }

        public async Task<bool> Predat()
        {
            if (!JeNastavenaKasa())
            {
                return false;
            }
            if (await Ulozit())
            {
                foreach (var item in ZoznamPoloziek)
                {
                    if (!UniConItemPoklDokladu.SpracujItemPD(item, in _db, in _dbw))
                    {
                        _dbw.ClearPendingChanges();
                        _db.ClearPendingChanges();
                        return false;
                    }
                }
                PoklDokladInput.Kasa = AktKasa?.ID;
                PoklDokladInput.KasaX = AktKasa;
                PoklDokladInput.Spracovana = true;
                PoklDokladInput.Vznik = DateTime.Now;

                var foundedPd = _db.PokladnicneDoklady.FirstOrDefault(x => x.ID == PoklDokladInput.ID);
                if (foundedPd == null)
                {
                    return false;
                }
                foundedPd.Kasa = AktKasa?.ID;
                foundedPd.KasaX = AktKasa;
                foundedPd.Spracovana = true;
                foundedPd.Vznik = DateTime.Now;
                _db.SaveChanges();
                _dbw.SaveChanges();
                await _sessionStorage.SetItemAsync("UniconChanged", true);
                return true;
            }
            return false;
        }

        public bool JeNastavenaKasa()
        {
            return AktKasa != null;
        }
    }
}