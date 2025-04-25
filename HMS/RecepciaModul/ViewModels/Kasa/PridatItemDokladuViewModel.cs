using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Xml.Xsl;
using UniComponents;
using Kkasa = DBLayer.Models.Kasa;
using PpokladnicnyDoklad = DBLayer.Models.PokladnicnyDoklad;

namespace RecepciaModul.ViewModels
{
    public partial class PridatItemDokladuViewModel : ObservableObject
    {
        public ItemPokladDokladu ItemPokladDokladuInput { get; set; } = new();
        public UniConItemPoklDokladu UniConItemPoklDokladuInput { get; set; } = new PolozkaSkladuConItemPoklDokladu();

        public List<DBLayer.Models.UniConItemPoklDokladu> ZoznamUniConItems { get; set; } = new();
        public List<object> ZoznamUniConItemsObjekty { get; set; } = new();

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
            return !string.IsNullOrEmpty(ItemPokladDokladuInput.UniConItemPoklDokladuX?.GetID());
        }

        public void SetItem(ItemPokladDokladu item)
        {
            ItemPokladDokladuInput = item.Clon();
        }

        public bool Ulozit()
        {
            return true;
        }

        public void PripravZoznam()
        {
            ZoznamUniConItemsObjekty.Clear();
            foreach (var item in ZoznamUniConItems)
            {
                if (item.GetType() != UniConItemPoklDokladuInput.GetType())
                {
                    continue;
                }

                switch (item)
                {
                    case PolozkaSkladuConItemPoklDokladu ytem:
                        if (!ZoznamUniConItemsObjekty.Any(x => ((PolozkaSkladu)x).ID == ytem.PolozkaSkladuMnozstvaX.PolozkaSkladu))
                        {
                            ZoznamUniConItemsObjekty.Add(ytem.PolozkaSkladuMnozstvaX.PolozkaSkladuX);
                        }
                        break;
                    case ReservationConItemPoklDokladu ytem:
                        if (_db.ItemyPokladDokladu
                            .Include(x => x.UniConItemPoklDokladuX)
                            .Any(x => x.UniConItemPoklDokladuX is ReservationConItemPoklDokladu
                            && ((ReservationConItemPoklDokladu)x.UniConItemPoklDokladuX).Reservation == ytem.Reservation))
                        {
                            continue;
                        }
                        ZoznamUniConItemsObjekty.Add(ytem.Reservation.ToString());
                        break;
                    default: break;
                }
            }

        }

        public void SpracujUniCon(object item)
        {
            switch (UniConItemPoklDokladuInput)
            {
                case PolozkaSkladuConItemPoklDokladu ytem:
                    var result = _db.PolozkySkladuConItemPoklDokladu
                        .Include(x => x.PolozkaSkladuMnozstvaX)
                        .FirstOrDefault(x => x.PolozkaSkladuMnozstvaX.PolozkaSkladu == ((PolozkaSkladu)item).ID);
                    if (result == null)
                    {
                        return;
                    }
                    UniConItemPoklDokladuInput = result;
                    break;
                case ReservationConItemPoklDokladu ytem:
                    var resultb = _db.ReservationConItemyPoklDokladu
                        .FirstOrDefault(x => x.Reservation == long.Parse((string)item));
                    if (resultb == null)
                    {
                        return;
                    }
                    UniConItemPoklDokladuInput = resultb;
                    break;
                default: break;
            }

            ItemPokladDokladuInput.Nazov = UniConItemPoklDokladuInput.GetNameUni();
            ItemPokladDokladuInput.Cena = (double)UniConItemPoklDokladuInput.GetCenaUni();
            ItemPokladDokladuInput.DPH = UniConItemPoklDokladuInput.GetDPHUni();
            ItemPokladDokladuInput.Mnozstvo = 1;
            ItemPokladDokladuInput.UniConItemPoklDokladu = UniConItemPoklDokladuInput.ID;
            ItemPokladDokladuInput.UniConItemPoklDokladuX = UniConItemPoklDokladuInput;
        }

        public List<string> GetColNames()
        {
            switch (UniConItemPoklDokladuInput)
            {
                case PolozkaSkladuConItemPoklDokladu item:
                    return new List<string> { "ID", "Názov", "Merná jednotka" };
                case ReservationConItemPoklDokladu item:
                    return new List<string> { "ID" };

                default: break;
            }
            return new List<string> { "ID" };      //chyba
        }
        public List<string> TypyList()
        {
            List<string> list = new();
            list.Add(new ReservationConItemPoklDokladu().GetTypeUni());
            list.Add(new PolozkaSkladuConItemPoklDokladu().GetTypeUni());
            return list;
        }

        public void SpravujZmenuTypu(string typ)
        {
            if (!TypyList().Contains(typ))
            {
                return;
            }
            UniConItemPoklDokladu newItem = new ReservationConItemPoklDokladu();
            if (typ == new ReservationConItemPoklDokladu().GetTypeUni())
            {
                newItem = new ReservationConItemPoklDokladu();
            }
            else if (typ == new PolozkaSkladuConItemPoklDokladu().GetTypeUni())
            {
                newItem = new PolozkaSkladuConItemPoklDokladu();
            }

            if (UniConItemPoklDokladuInput.GetType() == newItem.GetType())
            {
                return;
            }
            
            UniConItemPoklDokladuInput = newItem;
        }

        public List<Sklad> GetSklady()
        {
            switch (UniConItemPoklDokladuInput)
            {
                case PolozkaSkladuConItemPoklDokladu item:
                    return _db.PolozkySkladuConItemPoklDokladu
                        .Include(x => x.PolozkaSkladuMnozstvaX)
                        .ThenInclude(x => x.SkladX)
                        .Where(x => x.PolozkaSkladuMnozstvaX.PolozkaSkladu == item.PolozkaSkladuMnozstvaX.PolozkaSkladu)
                        .Select(x => x.PolozkaSkladuMnozstvaX.SkladX)
                        .Distinct()
                        .ToList();
                default: return new();
            }
        }
        public void SpracujSklad(Sklad sk)
        {
            switch (UniConItemPoklDokladuInput)
            {
                case PolozkaSkladuConItemPoklDokladu item:
                    var unicon = _db.PolozkySkladuConItemPoklDokladu
                        .Include(x => x.PolozkaSkladuMnozstvaX)
                        .ThenInclude(x => x.PolozkaSkladuX)
                        .Include(x => x.PolozkaSkladuMnozstvaX)
                        .ThenInclude(x => x.SkladX)
                        .FirstOrDefault(x => x.PolozkaSkladuMnozstvaX.PolozkaSkladu == item.PolozkaSkladuMnozstvaX.PolozkaSkladu &&
                        x.PolozkaSkladuMnozstvaX.Sklad == sk.ID);
                    if (unicon == null)
                    {
                        break;
                    }
                    UniConItemPoklDokladuInput = unicon;
                    ItemPokladDokladuInput.Nazov = UniConItemPoklDokladuInput.GetNameUni();
                    ItemPokladDokladuInput.Cena = (double)UniConItemPoklDokladuInput.GetCenaUni();
                    ItemPokladDokladuInput.DPH = UniConItemPoklDokladuInput.GetDPHUni();
                    ItemPokladDokladuInput.Mnozstvo = 1;
                    ItemPokladDokladuInput.UniConItemPoklDokladu = UniConItemPoklDokladuInput.ID;
                    ItemPokladDokladuInput.UniConItemPoklDokladuX = UniConItemPoklDokladuInput;
                    break;
                default: break;
            }
        }


        public List<TableColumnTemplate<object>> TableSettings = new();


        public void SetSettings()
        {
            switch (UniConItemPoklDokladuInput)
            {
                case PolozkaSkladuConItemPoklDokladu item:
                    //new List<string> { "ID", "Názov", "Merná jednotka" };
                    TableSettings = new()
                    {
                         new () { ID_Prop = "id", HeaderValue = "ID", CellValue = (item) => ((PolozkaSkladu)item).ID, CellConvert = ((item) => ((PolozkaSkladu)item).ID, typeof(string)) },
                         new () { ID_Prop = "nazov", HeaderValue = "Názov", CellValue = (item) => ((PolozkaSkladu)item).Nazov, CellConvert = ((item) => ((PolozkaSkladu)item).Nazov, typeof(string)) },
                         new () { ID_Prop = "mj", HeaderValue = "Merná jednotka", CellValue = (item) => ((PolozkaSkladu)item).MernaJednotka, CellConvert = ((item) => ((PolozkaSkladu)item).MernaJednotka, typeof(string)) }
                    };
                      break;
                case ReservationConItemPoklDokladu item:
                    TableSettings = new()
                    {
                         new () { ID_Prop = "id", HeaderValue = "ID", CellValue = (item) => ((string)item), CellConvert = ((item) => ((PolozkaSkladu)item), typeof(string)) }
                    };
                    break;

                default: break;
            }
            return ;      
        }


    }
}