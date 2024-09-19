using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
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

                var propType = item.PropertyType;
                if (propType == typeof(string) || propType == typeof(int) || propType == typeof(bool) || propType == typeof(double) || propType == typeof(long) || propType == typeof(DateTime) || propType == typeof(decimal))
                {
                    propRet.Add(item);
                    continue;
                }
            }
            return propRet.ToArray();
        }

        public Expression<Func<T, object>> GetPropertyExpression<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var conversion = Expression.Convert(property, typeof(object));
            return Expression.Lambda<Func<T, object>>(conversion, parameter);
        }

        public bool IsKeyTypeAttribute(PropertyInfo prop)
        {
            if (prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute), false).FirstOrDefault() != null)
            {
                return true;
            }

            if (prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), false).FirstOrDefault() != null)
            {
                return true;
            }

            if (prop.GetCustomAttributes(typeof(IsForeignKeyRezervationAttribute), false).FirstOrDefault() != null)
            {
                return true;
            }
            return false;
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
                            var lista = _db.PolozkySkladu.ToList();
                            foreach (var ytem in lista)
                            {
                                if (!_db.PolozkySkladuConItemPoklDokladu.Any(x => x.PolozkaSkladu == ytem.ID))
                                {
                                    Polozky.Add(ytem);
                                }
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
                    ((PolozkaSkladuConItemPoklDokladu)UniItemInput).PolozkaSkladu = ((PolozkaSkladu)polozka).ID;
                    ((PolozkaSkladuConItemPoklDokladu)UniItemInput).PredajnaCena = (decimal)((PolozkaSkladu)polozka).Cena;
                    break;
                case ReservationConItemPoklDokladu item:
                    ((ReservationConItemPoklDokladu)UniItemInput).Reservation = ((Rezervation)polozka).Id;
                    break;

                default: break;
            }
        }

        public bool Ulozit()
        {
            switch (UniItemInput)       //kontrola FK
            {
                case PolozkaSkladuConItemPoklDokladu item:
                    if (string.IsNullOrEmpty(((PolozkaSkladuConItemPoklDokladu)UniItemInput).PolozkaSkladu))
                    {
                        return false;
                    }
                    break;
                case ReservationConItemPoklDokladu item:
                    if (((ReservationConItemPoklDokladu)UniItemInput).Reservation == 0)
                    {
                        return false;
                    }
                    break;
                default: return false;
            }

            if (!Existuje())
            {
                _db.Add(UniItemInput);
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
                    return _db.PolozkaSkladuMnozstva.Include(x => x.SkladX).Where(x => x.ID == UniItemInput.ID).Select(x => x.SkladX).Distinct().ToList();
                    break;

                default: return new();
            }
        }

        public void SpracujSklad(Sklad sk)
        {
                
        }
    }
}