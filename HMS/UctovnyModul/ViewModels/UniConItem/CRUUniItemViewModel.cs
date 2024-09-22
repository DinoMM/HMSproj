using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using UniComponents;
using UniItem = DBLayer.Models.UniConItemPoklDokladu;

namespace UctovnyModul.ViewModels
{
    public partial class CRUUniItemViewModel : ObservableObject
    {
        public UniItem UniItemInput { get; set; } = new PolozkaSkladuConItemPoklDokladu();
        private bool exist = false;

        public bool LoadingZoznam { get; set; } = false;
        public List<object> Polozky { get; set; } = new List<object>();

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;

        public CRUUniItemViewModel(DBContext db, UserService userService, DataContext dbw)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(UniItem.ROLE_CRUD_POLOZKY);
        }

        public void SetItem(UniItem item)
        {
            UniItemInput = item.Clon();
            exist = true;
        }
        public bool Existuje()
        {
            return exist;
        }

        public System.Reflection.PropertyInfo[] GetProperties()
        {
            //return obj.GetType().GetProperties().Where(p => p.CanWrite);
            var prop = UniItemInput.GetType().GetProperties();
            var propRet = new List<System.Reflection.PropertyInfo>();

            foreach (var item in prop)
            {
                if (true)
                {
                    if (item.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute), false).FirstOrDefault() != null)
                    {
                        continue;
                    }
                }

                if (true)
                {
                    if (item.GetCustomAttributes(typeof(HideFromInputAttribute), false).FirstOrDefault() != null)
                    {
                        continue;
                    }
                }

                var propType = item.PropertyType;
                if (propType == typeof(string) || propType == typeof(int) || propType == typeof(bool) || propType == typeof(double) || propType == typeof(long) || propType == typeof(DateTime) || propType == typeof(decimal))
                {
                    propRet.Add(item);
                    continue;
                }
            }
            return propRet.ToArray();
        }

        public bool IsKeyTypeAttribute(PropertyInfo prop)
        {
            if (IsForeignKeyAttribute(prop))
            {
                return true;
            }

            if (IsKeyAttribute(prop))
            {
                return true;
            }

            return false;
        }

        public bool IsKeyAttribute(PropertyInfo prop)
        {
            return prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), false).FirstOrDefault() != null;
        }

        public bool IsForeignKeyAttribute(PropertyInfo prop)
        {
            return prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute), false).FirstOrDefault() != null
              || prop.GetCustomAttributes(typeof(IsForeignKeyRezervationAttribute), false).FirstOrDefault() != null;
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
            UniItem newItem = new ReservationConItemPoklDokladu();
            if (typ == new ReservationConItemPoklDokladu().GetTypeUni())
            {
                newItem = new ReservationConItemPoklDokladu();
            }
            else if (typ == new PolozkaSkladuConItemPoklDokladu().GetTypeUni())
            {
                newItem = new PolozkaSkladuConItemPoklDokladu();
            }

            if (UniItemInput.GetType() == newItem.GetType())
            {
                return;
            }

            UniItemInput = newItem;
        }

        public async Task LoadPolozky()
        {
            var checktypeitem = Polozky.FirstOrDefault(); //kontrola
            if (checktypeitem != null)
            {
                if (checktypeitem.GetType() == UniItemInput.GetType())
                {
                    return;
                }
            }
            LoadingZoznam = true;
            await Task.Run(() =>
                {
                    Polozky.Clear();
                    switch (UniItemInput)
                    {
                        case PolozkaSkladuConItemPoklDokladu item:
                            //var lista = _db.PolozkySkladu.ToList();
                            var lista = _db.PolozkaSkladuMnozstva
                            .Include(x => x.PolozkaSkladuX)
                            .Include(x => x.SkladX)
                            .ToList();
                            var listaa = _db.PolozkySkladuConItemPoklDokladu
                                .Include(x => x.PolozkaSkladuMnozstvaX)
                                .Select(x => x.PolozkaSkladuMnozstvaX)
                                .ToList();
                            foreach (var ytem in listaa)
                            {
                                lista.Remove(ytem);
                            }
                            foreach (var ytem in lista.Select(x => x.PolozkaSkladuX).Distinct())
                            {
                                Polozky.Add(ytem);
                            }
                            break;
                        case ReservationConItemPoklDokladu item:
                            var listb = _dbw.Rezervations.Where(x =>
                            x.Status != ReservationStatus.Stornovana.ToString()
                            && x.Status != ReservationStatus.Checked_IN.ToString()
                            && x.Status != ReservationStatus.Checked_OUT.ToString()
                            && x.Status != ReservationStatus.Blokovana.ToString()
                            && x.Status != ReservationStatus.SchvalenaZaplatena.ToString())
                                .ToList();
                            foreach (var ytem in listb)
                            {
                                if (!_db.ReservationConItemyPoklDokladu.Any(x => x.Reservation == ytem.Id))
                                {
                                    Polozky.Add(ytem);
                                }
                            }
                            break;

                        default: break;
                    }
                }
                );
            LoadingZoznam = false;
        }

        public List<string> GetColNames()
        {
            switch (UniItemInput)
            {
                case PolozkaSkladuConItemPoklDokladu item:
                    return new List<string> { "ID", "Názov", "MernaJednotka", "Cena" };
                case ReservationConItemPoklDokladu item:
                    return new List<string> { "ID", "Príchod", "Odchod", "Poèet plán. hostí", "Celková suma", "Status" };

                default: break;
            }
            return new List<string> { "ID" };      //chyba
        }

        public void SpracujNovuPolozku(object polozka)
        {
            switch (UniItemInput)
            {
                case PolozkaSkladuConItemPoklDokladu item:
                    item.PolozkaSkladuMnozstvaX.PolozkaSkladu = ((PolozkaSkladu)polozka).ID;
                    item.PredajnaCena = (decimal)((PolozkaSkladu)polozka).Cena;
                    break;
                case ReservationConItemPoklDokladu item:
                    item.Reservation = ((Rezervation)polozka).Id;
                    break;

                default: break;
            }
        }

        public bool Ulozit()
        {
            switch (UniItemInput)       //kontrola FK
            {
                case PolozkaSkladuConItemPoklDokladu item:
                    if (string.IsNullOrEmpty(item.PolozkaSkladuMnozstvaX.PolozkaSkladu)
                        || string.IsNullOrEmpty(item.PolozkaSkladuMnozstvaX.Sklad)
                        )
                    {
                        return false;
                    }
                    break;
                case ReservationConItemPoklDokladu item:
                    if (item.Reservation == 0)
                    {
                        return false;
                    }
                    break;
                default: return false;
            }

            if (!Existuje())
            {
                switch (UniItemInput)       //najdenie potrebnych FK
                {
                    case PolozkaSkladuConItemPoklDokladu item:
                        var founded = _db.PolozkaSkladuMnozstva.FirstOrDefault(x =>
                        x.PolozkaSkladu == item.PolozkaSkladuMnozstvaX.PolozkaSkladu
                        && x.Sklad == item.PolozkaSkladuMnozstvaX.Sklad);

                        if (founded == null)
                        {
                            return false;
                        }
                        item.PolozkaSkladuMnozstva = founded.ID;
                        item.PolozkaSkladuMnozstvaX = founded;
                        break;
                    default: break;
                }

                _db.UniConItemyPoklDokladu.Add(UniItemInput);
            }
            else
            {
                var found = _db.UniConItemyPoklDokladu.FirstOrDefault(x => x.ID == UniItemInput.ID);
                if (found == null)
                {
                    return false;
                }
                found.SetFrom(UniItemInput);
            }


            _db.SaveChanges();
            exist = true;
            return true;
        }

        public List<Sklad> GetSklady()
        {
            switch (UniItemInput)
            {
                case PolozkaSkladuConItemPoklDokladu item:
                    var list = _db.PolozkaSkladuMnozstva
                        .Include(x => x.SkladX)
                        .Where(x => x.PolozkaSkladu == item.PolozkaSkladuMnozstvaX.PolozkaSkladu)
                        .ToList();
                    var listt = _db.PolozkySkladuConItemPoklDokladu
                        .Include(x => x.PolozkaSkladuMnozstvaX)
                        .Where(x => x.PolozkaSkladuMnozstvaX.PolozkaSkladu == item.PolozkaSkladuMnozstvaX.PolozkaSkladu)
                        .ToList();
                    var listtt = new List<Sklad>();
                    foreach (var ytem in list)
                    {
                        if (!listt.Any(x => x.PolozkaSkladuMnozstvaX.PolozkaSkladu == item.PolozkaSkladuMnozstvaX.PolozkaSkladu &&
                        ytem.Sklad == x.PolozkaSkladuMnozstvaX.Sklad))
                        {
                            listtt.Add(ytem.SkladX);
                        }
                    }
                    return listtt.Distinct().ToList();

                default: return new();
            }
        }

        public PropertyInfo GetIdIfKey(PropertyInfo property)
        {
            if (IsForeignKeyAttribute(property))       //frkey ID moze byt ine
            {
                return new StringPropertyInfo(property.Name, UniItemInput.GetID());
            }
            return property;
        }

        public string GetName(PropertyInfo property)
        {
            if (IsForeignKeyAttribute(property))
            {
                return UniItemInput.GetTypeUni();
            }
            if (IsKeyAttribute(property))
            {
                return "ID Spojenia";
            }
            #region pre DisplayAndValueAttribute property, ak existuje, treba doplnit switch keby sa pridavali dalsie
            var attributeDisplay = property.GetCustomAttributes(typeof(DBLayer.DisplayAndValueAttribute<string, PolozkaSkladuConItemPoklDokladu>), false).FirstOrDefault() as DBLayer.DisplayAndValueAttribute<string, PolozkaSkladuConItemPoklDokladu>;
            if (attributeDisplay != null)
            {
                return attributeDisplay.Display;
            }
            #endregion
            return property.Name;
        }
        public void SpracujSklad(Sklad sk)
        {
            switch (UniItemInput)
            {
                case PolozkaSkladuConItemPoklDokladu item:
                    item.PolozkaSkladuMnozstvaX.Sklad = sk.ID;
                    break;
                default: break;
            }
        }
    }
}