using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniComponents;
using PolozkaS = DBLayer.Models.PolozkaSkladu;

namespace SkladModul.ViewModels.Sklad
{
    public partial class ModifPolozSkladViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<DBLayer.Models.Sklad> zoznamSkladov = new();
        [ObservableProperty]
        ObservableCollection<DBLayer.Models.Sklad> zoznamOnacenych = new();

        public List<MoznoVymazatActive> MoznoZmenitActive { get; set; } = new();

        public PolozkaS Polozka = new();
        public bool Saved { get; set; } = false;

        readonly DBContext _db;
        readonly Blazored.SessionStorage.ISessionStorageService _sessionStorage;
        public ModifPolozSkladViewModel(DBContext db, Blazored.SessionStorage.ISessionStorageService sessionStorage)
        {
            _db = db;

            zoznamSkladov = new(_db.Sklady.ToList());
            _sessionStorage = sessionStorage;

            foreach (var item in zoznamSkladov)
            {
                MoznoZmenitActive.Add(new MoznoVymazatActive() { Sklad = item });
            }

        }

        public void SetExist(PolozkaS poloz)
        {
            Polozka = poloz;
            var activcon = _db.PolozkaSkladuMnozstva
                .Include(x => x.SkladX)
                .Include(x => x.PolozkaSkladuX)
                .Where(x => x.PolozkaSkladu == Polozka.ID)
                .ToList();
            foreach (var item in activcon)
            {
                ZoznamOnacenych.Add(item.SkladX);
                var res = MoznoZmenitActive.FirstOrDefault(x => x.Sklad.ID == item.SkladX.ID);

                res.MoznoZmenit = !ExistujePouzitie(item.PolozkaSkladuX, item.SkladX);
                res.Active = item.Active;

            }
            Saved = true;
        }

        public void VyberSklad(DBLayer.Models.Sklad poloz)
        {
            if (MoznoZmenitActive.FirstOrDefault(x => x.Sklad.ID == poloz.ID).MoznoZmenit && !ZoznamOnacenych.Remove(poloz))
            {
                ZoznamOnacenych.Add(poloz);
            }
        }

        public async Task Uloz(ApproveModal apprModal)
        {
            if (!Saved)
            {
                if (string.IsNullOrEmpty(Polozka.ID))
                {
                    Polozka.ID = PolozkaS.DajNoveID(_db);
                    _db.PolozkySkladu.Add(Polozka);
                }


                List<PolozkaSkladuMnozstvo> activcon = _db.PolozkaSkladuMnozstva
                    .Include(x => x.SkladX)
                    .Where(x => x.PolozkaSkladu == Polozka.ID)
                    .ToList();
                foreach (var item in activcon)  //nahodenie activ
                {
                    var found = MoznoZmenitActive.FirstOrDefault(x => x.Sklad.ID == item.Sklad);
                    if (found != null)
                    {
                        item.Active = found.Active;
                    }
                }

                foreach (var item in ZoznamOnacenych)
                {
                    PolozkaSkladuMnozstvo? najd = null;
                    var newPolozMnoz = new PolozkaSkladuMnozstvo() { Sklad = item.ID, PolozkaSkladu = Polozka.ID };
                    if ((najd = activcon.FirstOrDefault(x => x.Sklad == item.ID)) == null)
                    {
                        _db.PolozkaSkladuMnozstva.Add(newPolozMnoz);
                        continue;
                    }
                    activcon.Remove(najd);
                }

                if (activcon.Count > 0)
                {
                    var ans = await apprModal.OpenModal(true);
                    if (ans)
                    {
                        foreach (var item in activcon)
                        {
                            _db.PolozkaSkladuMnozstva.Remove(item);
                        }
                    }
                    else
                    {
                        foreach (var item in activcon)
                        {
                            ZoznamOnacenych.Add(item.SkladX);
                        }
                    }
                }
                await _sessionStorage.SetItemAsync("SkladPolozkyLoaded", false);    //chceme aktualizaciu poloziek
                _db.SaveChanges();


                for (int i = 0; i < MoznoZmenitActive.Count; ++i)
                {
                    MoznoZmenitActive[i].MoznoZmenit = true;
                    MoznoZmenitActive[i].Active = MoznoZmenitActive[i].Active;
                }
                foreach (var item in ZoznamOnacenych)
                {
                    var fond = MoznoZmenitActive.FirstOrDefault(x => x.Sklad.ID == item.ID);
                    fond.MoznoZmenit = false;
                    fond.Active = fond.Active;

                }

                // var activconn = _db.PolozkaSkladuMnozstva
                //.Include(x => x.SkladX)
                //.Include(x => x.PolozkaSkladuX)
                //.Where(x => x.PolozkaSkladu == Polozka.ID)
                //.ToList();
                // foreach (var item in activconn)
                // {
                //     var res = MoznoZmenitActive.FirstOrDefault(x => x.sklad.ID == item.SkladX.ID);
                //     var found = !ExistujePouzitie(item.PolozkaSkladuX);
                //     res = (res.sklad, found, item.Active);
                // }
            }
        }

        public async Task VratZmeny()
        {
            if (!string.IsNullOrEmpty(Polozka.ID))
            {
                await _sessionStorage.SetItemAsync("SkladPolozkyLoaded", false); //chceme aktualizaciu poloziek
                _db.Entry(Polozka).State = EntityState.Unchanged;
                _db.SaveChanges();
            }
        }

        public bool Existuje()
        {
            return !string.IsNullOrEmpty(Polozka.ID);
        }

        public bool ExistujePouzitie(PolozkaS polozka, DBLayer.Models.Sklad sklad)
        {
            if (_db.PrijemkyPolozky.Include(x => x.SkupinaX).Any(x => ((Prijemka)x.SkupinaX).Sklad == sklad.ID && x.PolozkaSkladu == polozka.ID))
            {
                return true;
            }
            if (_db.VydajkyPolozky.Include(x => x.SkupinaX).Any(x => ((Vydajka)x.SkupinaX).Sklad == sklad.ID && x.PolozkaSkladu == polozka.ID))
            {
                return true;
            }
           //toto dako opravit
            //if (_db.PolozkySkladuConItemPoklDokladu.Include(x => x.PolozkaSkladuMnozstvaX).Any(x => x.PolozkaSkladuMnozstvaX.Sklad == sklad.ID && x.PolozkaSkladuMnozstvaX.PolozkaSkladu == polozka.ID))
            //{
            //    return true;
            //}
            return false;
        }

        public class MoznoVymazatActive
        {
            public DBLayer.Models.Sklad Sklad { get; set; }
            public bool MoznoZmenit { get; set; } = true;
            public bool Active { get; set; } = true;
        }
    }
}
