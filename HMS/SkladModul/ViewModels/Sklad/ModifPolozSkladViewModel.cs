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

        public PolozkaS Polozka = new();
        public bool Saved { get; set; } = false;

        readonly DBContext _db;
        readonly Blazored.SessionStorage.ISessionStorageService _sessionStorage;
        public ModifPolozSkladViewModel(DBContext db, Blazored.SessionStorage.ISessionStorageService sessionStorage)
        {
            _db = db;

            zoznamSkladov = new(_db.Sklady.ToList());
            _sessionStorage = sessionStorage;

        }

        public void SetExist(PolozkaS poloz)
        {
            Polozka = poloz;
            var activcon = _db.PolozkaSkladuMnozstva.Include(x => x.SkladX).Where(x => x.PolozkaSkladu == Polozka.ID);
            foreach (var item in activcon)
            {
                ZoznamOnacenych.Add(item.SkladX);
            }
            Saved = true;
        }

        public void VyberSklad(DBLayer.Models.Sklad poloz)
        {
            if (!ZoznamOnacenych.Remove(poloz))
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


                List<PolozkaSkladuMnozstvo> activcon = _db.PolozkaSkladuMnozstva.Include(x => x.SkladX).Where(x => x.PolozkaSkladu == Polozka.ID).ToList();
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
    }
}
