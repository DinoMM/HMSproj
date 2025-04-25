using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using UniComponents;

namespace UctovnyModul.ViewModels
{
    public partial class CRUFakturaViewModel : ObservableEntitySaverVM<Faktura>
    {
        public bool NotEditable
        {
            get => EntitySaver.Exist && EntitySaver.Original.Spracovana != null;
        }

        public ObservableCollectionOwn<PohJednotka>? OwnObservedCollectionPohJ { get; set; }
        public int TotalFak { get; set; } = 0;


        #region Polia na vyplnenie
        public decimal Cenaz { get; set; } = 0;
        public decimal Dphz { get; set; } = 0;
        public decimal Cenadphz { get; set; } = 0;

        public decimal CenaAll { get; set; } = 0;
        public decimal DphAll { get; set; } = 0;
        public decimal CenadphAll { get; set; } = 0;
        #endregion
        private List<Dodavatel> dodavatelia = new();

        private readonly DBContext _db;
        private readonly UserService _userService;

        public CRUFakturaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;

            EntitySaver = new EntitySaver<Faktura>(_db,
                _db.Faktury,
                (Faktura x) => _db.Faktury.FirstOrDefault(y => x.ID == y.ID && x.Skupina == y.Skupina));

            OwnObservedCollectionPohJ = new(nacitajZoznamyAsync: async () =>
            {
                if (string.IsNullOrEmpty(Entity.Skupina) || Entity.SkupinaX == null)
                {
                    OwnObservedCollectionPohJ.ZoznamPoloziek = new();
                    RefreshSumy();
                    return;
                }
                OwnObservedCollectionPohJ.ZoznamPoloziek = new(await PohJednotka.GetDbSet(Entity.SkupinaX.GetType(), _db)
                    .Include(x => x.SkupinaX)
                    .Where(x => x.Skupina == Entity.Skupina)
                    .ToListAsync());
                RefreshSumy();
            },
            moznoVymazat: (item) =>
            {
                return false; /*nemozno vymazat lebo sa len natahuju udaje*/
            },
            vymazat: (item) =>
            {
                //if (_db.Set<PohJednotka>().Any(x => x.ID == item.ID))
                //{
                //    _db.Remove(item);
                //    _db.SaveChanges();
                //}
            });
            OwnObservedCollectionPohJ.Initialization();

        }

        public bool ValidateUserD()
        {
            return _userService.IsLoggedUserInRoles(Faktura.ROLE_CRUD_FAKTURY);
        }

        public async Task<ValidationResult> Save()
        {
            if (!EntitySaver.Exist && string.IsNullOrEmpty(Entity.ID))
            {
                Entity.ID = Faktura.DajNoveID(_db.Faktury);
            }
            if (!string.IsNullOrEmpty(Entity.ID) && !string.IsNullOrEmpty(Entity.Skupina))
            {
                if (Entity.Spracovana != null)
                {
                    await OwnObservedCollectionPohJ.NacitajZoznamy();
                    Entity.CenaBezDPH = Cenaz;
                    Entity.CenaSDPH = Cenadphz;
                }
                else
                {
                    Entity.CenaBezDPH = 0;
                    Entity.CenaSDPH = 0;
                }
            }

            bool firstTime = !EntitySaver.Exist;
            var res = base.Save();

            if (res == ValidationResult.Success)
            {
                if (firstTime)
                {
                    TotalFak++;
                }
                if (TotalFak > 1)
                {
                    await LoadFromAll(lenSpracovane: true);
                    RefreshSumyAll(OwnObservedCollectionPohJ.ZoznamPoloziek);
                }
            }
            return res;
        }

        public async Task LoadFromAll(bool lenSpracovane = false)
        {
            if (string.IsNullOrEmpty(Entity.ID))
            {
                return;
            }
            await OwnObservedCollectionPohJ.NacitajAndSilentCollection(methodAsync: async () =>
            {
                List<PohSkup?> listSkup;
                if (lenSpracovane)
                {
                    listSkup = _db.Faktury
                       .Include(x => x.SkupinaX)
                       .Where(x => x.ID == Entity.ID && x.Spracovana == true)
                       .Select(x => x.SkupinaX)
                       .ToList();
                }
                else
                {
                    listSkup = _db.Faktury
                       .Include(x => x.SkupinaX)
                       .Where(x => x.ID == Entity.ID)
                       .Select(x => x.SkupinaX)
                       .ToList();
                }

                List<PohJednotka> listPohJedn = new();
                foreach (var item in listSkup)
                {
                    if (string.IsNullOrEmpty(item?.ID))
                    {
                        continue;
                    }
                    listPohJedn.AddRange(await PohJednotka.GetDbSet(item.GetType(), _db)
                        .Include(x => x.SkupinaX)
                        .Where(x => x.Skupina == item.ID)
                        .ToListAsync());
                }

                OwnObservedCollectionPohJ.ZoznamPoloziek = new(listPohJedn);
            });
            RefreshSumy();
            await OwnObservedCollectionPohJ.ManualNotifyColection();
        }

        public async Task<List<PohSkup>> GetSkupiny()
        {
            List<PohSkup> list = new();
            list.AddRange(await _db.Set<PohSkup>().Where(x => x.Spracovana).ToListAsync());
            foreach (var item in list.ToList())
            {
                if (await _db.Faktury.AnyAsync(x => x.Skupina == item.ID && x.Skupina != Entity.Skupina))
                {
                    list.Remove(item);
                }
            }
            return list.OrderByDescending(x => x.ID).ToList();
        }

        public async Task<List<Dodavatel>> GetDod()
        {
            if (dodavatelia.Count > 0)
            {
                return dodavatelia;
            }
            dodavatelia.AddRange(await _db.Dodavatelia.OrderBy(x => x.ICO).ToListAsync());
            return dodavatelia;
        }

        public void RefreshSumy()  //volat po zmenene poloziek
        {
            Cenaz = (decimal)OwnObservedCollectionPohJ.ZoznamPoloziek.Sum(x => x.CelkovaCena);
            Cenadphz = (decimal)OwnObservedCollectionPohJ.ZoznamPoloziek.Sum(x => x.CelkovaCenaDPH);
            Dphz = Cenaz != 0 ? ((Cenadphz * 100) / Cenaz) - 100 : 0;
        }

        public void RefreshSumyAll(IList<PohJednotka> list)  //volat po zmenene poloziek
        {
            CenaAll = (decimal)list.Sum(x => x.CelkovaCena);
            CenadphAll = (decimal)list.Sum(x => x.CelkovaCenaDPH);
            DphAll = CenaAll != 0 ? ((CenadphAll * 100) / CenaAll) - 100 : 0;
        }

    }
}