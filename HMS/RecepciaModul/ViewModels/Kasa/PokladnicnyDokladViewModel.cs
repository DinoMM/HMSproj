using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using PdfCreator.Models;
using PdfSharp.Pdf.Advanced;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Kkasa = DBLayer.Models.Kasa;
using PpokladnicnyDoklad = DBLayer.Models.PokladnicnyDoklad;

namespace RecepciaModul.ViewModels
{
    public partial class PokladnicnyDokladViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<DBLayer.Models.ItemPokladDokladu> zoznamPoloziek = new();

        public List<DBLayer.Models.UniConItemPoklDokladu> ZoznamUniConItems { get; set; } = new();

        public bool NacitavaniePoloziek { get; private set; } = true;
        public bool PDFLoading { get; private set; } = false;

        public Kkasa? AktKasa { get; set; } = null;

        public PpokladnicnyDoklad PoklDokladInput { get; set; } = new();

        public UniConItemPoklDokladu? UniConItem { get; set; }
        public double PlatbaOdHostaInput { get; set; } = 0.0;

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

            ZoznamUniConItems.Clear();
            ZoznamUniConItems.AddRange(_db.PolozkySkladuConItemPoklDokladu
                .Include(x => x.PolozkaSkladuMnozstvaX)
                .ThenInclude(x => x.PolozkaSkladuX)
                .Include(x => x.PolozkaSkladuMnozstvaX)
                .ThenInclude(x => x.SkladX)
                .ToList());
            ZoznamUniConItems.AddRange(_db.ReservationConItemyPoklDokladu.ToList());

            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(ItemPokladDokladu item)
        {
            return true;
        }

        public void Vymazat(ItemPokladDokladu item)
        {
            ZoznamPoloziek.Remove(item);
        }

        public bool SetPoklDokl(PpokladnicnyDoklad item)
        {
            PoklDokladInput = item.Clon();
            existuje = true;
            return UniConItemPoklDokladu.JeItemOnlyOnePD(item, in _db);
        }
        public bool SetPoklDokl(Host item)
        {
            if (item.PokladnicnyDokladZ != null)
            {
                return SetPoklDokl(item.PokladnicnyDokladZ);
            }
            PoklDokladInput.Host = item.ID;
            PoklDokladInput.HostX = item;
            return false;
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

        public async Task<bool> Ulozit(bool asComponent = false)
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
                //found.Host = PoklDokladInput.Host;
                //found.HostX = PoklDokladInput.HostX;
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

            if (asComponent)
            {
                await _sessionStorage.SetItemAsync("PDChanged", true);
            }
            _db.SaveChanges();
            existuje = true;
            return true;
        }

        public async Task<ValidationResult> Predat(bool asComponent = false)
        {
            if (!JeNastavenaKasa())
            {
                return new ValidationResult("Nie je nastaven� kasa.");
            }
            if (await Ulozit())
            {
                var foundedPd = _db.PokladnicneDoklady.FirstOrDefault(x => x.ID == PoklDokladInput.ID);
                if (foundedPd == null)
                {
                    return new ValidationResult("Pokladni�n� doklad neexistuje");
                }
                #region spracovanie jednotlivych poloziek pred spracovanim, musi byt vsetko ok
                foreach (var item in ZoznamPoloziek)
                {
                    var result = UniConItemPoklDokladu.SpracujItemPD(item, in _db, in _dbw);
                    if (result != ValidationResult.Success)
                    {
                        _dbw.ClearPendingChanges();
                        _db.ClearPendingChanges();
                        return result;
                    }
                }
                #endregion
                #region kontrola, �i m��eme vyda� mno�stvo zo skladu
                var resmnoz = PolozkaSkladuConItemPoklDokladu.ValidateMnozstvo(ZoznamPoloziek.ToList(), in _db);
                if (resmnoz != ValidationResult.Success)
                {
                    _db.ClearPendingChanges();
                    _dbw.ClearPendingChanges();
                    return resmnoz;
                }
                #endregion
                PoklDokladInput.Kasa = AktKasa?.ID;
                PoklDokladInput.KasaX = AktKasa;
                PoklDokladInput.Spracovana = true;
                PoklDokladInput.Vznik = DateTime.Now;


                foundedPd.Kasa = AktKasa?.ID;
                foundedPd.KasaX = AktKasa;
                foundedPd.Spracovana = true;
                foundedPd.Vznik = DateTime.Now;
                _db.SaveChanges();
                _dbw.SaveChanges();
                #region kontrola priradenia datumu lebo niekedy nefunguje ked sa mnozstva aktualne == odoberanie (ked pred tym sa nezhodovali pocty a aktualizuju sa na dobre, tak vtedy nastavuje null.)
                foreach (var item in ZoznamPoloziek)
                {
                    if (item.UniConItemPoklDokladuX is PolozkaSkladuConItemPoklDokladu)
                    {
                        var founchknull = _db.ItemyPokladDokladu
                            .FirstOrDefault(x => x.ID == item.ID);
                        if (founchknull == null)
                        {
                            throw new Exception("pokladnicny item sa nenasiel pri predaji.");
                        }
                        if (founchknull.Obdobie == null)
                        {
                            var fndd = _db.PolozkySkladuConItemPoklDokladu
                                .Include(x => x.PolozkaSkladuMnozstvaX)
                                .ThenInclude(x => x.SkladX)
                                .FirstOrDefault(x => x.ID == founchknull.UniConItemPoklDokladu);
                            if (fndd == null)
                            {
                                throw new Exception("PolozkySkladuConItemPoklDokladu sa nenasiel pri predaji.");
                            }
                            founchknull.Obdobie = SkladObdobie.GetActualObdobieFromSklad(fndd.PolozkaSkladuMnozstvaX.SkladX, in _db);
                            _db.SaveChanges();
                        }
                    }
                }
                #endregion
                if (asComponent)
                {
                    await _sessionStorage.SetItemAsync("PDSold", true);
                }
                return ValidationResult.Success;
            }
            return new ValidationResult("Ulo�enie dokladu neprebehlo �spe�ne, skontrolujte doklad.");
        }

        public bool JeNastavenaKasa()
        {
            return AktKasa != null;
        }

        public void PridatItemDokladu(ItemPokladDokladu item)
        {
            item.Skupina = PoklDokladInput.ID;
            ZoznamPoloziek.Add(item);
        }

        public void ZmenitItemDokladu(ItemPokladDokladu original, ItemPokladDokladu novy)
        {
            ZoznamPoloziek.Remove(original);
            PridatItemDokladu(novy);
        }

        public bool KontrolaOnlyOne(ItemPokladDokladu item)
        {
            return item.UniConItemPoklDokladuX.JeItemOnlyOneTyp();
        }

        public bool MoznoUlozitOnlyOne(ItemPokladDokladu item)
        {
            if (ZoznamPoloziek.Count == 0)
            {
                return true;
            }

            if (ZoznamPoloziek.Count > 1)
            {
                return false;
            }

            if (ZoznamPoloziek[0].UniConItemPoklDokladuX.GetType() == item.UniConItemPoklDokladuX.GetType())
            {
                return true;
            }
            return false;
        }

        public async Task CreatePdf()
        {
            PDFLoading = true;
            BlocekPDF creator = new BlocekPDF();

            if (PoklDokladInput.KasaX!.DodavatelX == null)
            {
                PoklDokladInput.KasaX!.DodavatelX = _db.Kasy
                    .Include(x => x.DodavatelX)
                    .Include(x => x.ActualUserX)
                    .FirstOrDefault(x => x.ID == PoklDokladInput.Kasa).DodavatelX;
            }

            await Task.Run(() =>
            {
                creator.GenerujPdf(PoklDokladInput, ZoznamPoloziek.ToList());
                creator.OpenPDF();

            });

            PDFLoading = false;
        }

        public double GetSum()
        {
            return ZoznamPoloziek.Sum(x => x.CelkovaCenaDPH);
        }

        public bool Mapolozky()
        {
            return ZoznamPoloziek.Count != 0;
        }
    }
}