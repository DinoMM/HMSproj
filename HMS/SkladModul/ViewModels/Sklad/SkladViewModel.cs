using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBLayer;
using PolozkaS = DBLayer.Models.PolozkaSkladu;
using Ssklad = DBLayer.Models.Sklad;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using DBLayer.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace SkladModul.ViewModels.Sklad
{
    public partial class SkladViewModel : ObservableObject
    {
        public string Obdobie { get; set; }
        public List<Ssklad> Sklady { get; set; }
        public Ssklad Sklad { get; set; }
        public bool NacitaneMnozstvo { get; set; } = false;
        public bool AktualneObdobie { get; set; } = false;
        public bool NacitavaniePoloziek { get; private set; } = true;

        [ObservableProperty]
        ObservableCollection<PolozkaS> zoznamPoloziekSkladu = new();


        readonly DBContext _db;
        readonly UserService _userService;
        readonly Blazored.SessionStorage.ISessionStorageService _sessionStorage;

        public SkladViewModel(DBContext db, UserService userService, Blazored.SessionStorage.ISessionStorageService sessionStorage)
        {
            _db = db;
            _userService = userService;
            _sessionStorage = sessionStorage;

            if (_userService.LoggedUser == null)
            {
                Debug.WriteLine("Neni prihlaseny uzivatel");
                return;
            }
            if (true)
            {
                var skladuziv = _db.SkladUzivatelia.Include(x => x.SkladX).Where(x => x.Uzivatel == _userService.LoggedUser.Id).ToList();

                if (skladuziv.Count > 0)
                {
                    Sklady = skladuziv.Select(x => x.SkladX).ToList();
                    Sklad = Sklady.FirstOrDefault();
                    //Obdobie = Sklad.ShortformObdobie();
                    var obd = _db.SkladObdobi.Include(x => x.SkladX)
                        .Where(x => x.Sklad == Sklad.ID)
                        .OrderByDescending(x => x.Obdobie)
                        .FirstOrDefault()?.Obdobie;
                    if (obd.HasValue)
                    {
                        Obdobie = Ssklad.ShortFromObdobie(obd.Value);
                    }



                    if (Ssklad.ZMENAPOLOZIEKROLE.Contains(_userService.LoggedUserRole))
                    {
                        if (Sklady.FirstOrDefault(x => x.ID == "ALL") == null)  //prida moznost pre zobrazenie vsetkych skladovych poloziek
                        {
                            Sklady.Add(new Ssklad() { ID = "ALL", Nazov = "Zobrazenie všetkých položiek"/*, Obdobie = DateTime.Today*/ });
                        }
                    }
                    CheckIsLastObdobie();     //kontrola ci je obdobie aktualne
                }
                else
                {
                    Debug.WriteLine("Ziadne sklady!");
                }
            }

        }

        public void SetProp(Ssklad sk, string ob)
        {
            Sklad = sk;
            Obdobie = ob;
            CheckIsLastObdobie();     //kontrola ci je obdobie aktualne
        }

        public List<string> GetObdobia()
        {
            var list = _db.SkladObdobi.Include(x => x.SkladX)
                .Where(x => x.Sklad == Sklad.ID)
                .OrderByDescending(x => x.Obdobie)
                .Select(x => Ssklad.ShortFromObdobie(x.Obdobie))
                .ToList();
            list.Reverse();
            return list;
        }
        public async Task SetSklad(string ID)
        {
            var found = Sklady.FirstOrDefault(x => x.ID == ID);
            if (found != null)
            {
                if (Sklad.ID != found.ID)
                {
                    Sklad = found;
                    //Obdobie = Sklad.ShortformObdobie();
                    var obd = _db.SkladObdobi.Include(x => x.SkladX)
                        .Where(x => x.Sklad == Sklad.ID)
                        .OrderByDescending(x => x.Obdobie)
                        .FirstOrDefault()?.Obdobie;
                    if (obd.HasValue)
                    {
                        Obdobie = Ssklad.ShortFromObdobie(obd.Value);
                    }

                    ZoznamPoloziekSkladu.Clear();
                    await LoadPolozky();
                    NacitaneMnozstvo = false;
                }
            }
        }

        public async Task LoadPolozky()
        {
            if (ZoznamPoloziekSkladu.Count == 0)
            {

                if (Sklad.ID == "ALL")      //pri zobrazeni vsetkych poloziek pri mode ALL
                {
                    var zozn = _db.PolozkySkladu.ToList();
                    foreach (var item in zozn)
                    {
                        ZoznamPoloziekSkladu.Add(item);
                    }
                    return;
                }

                //pridanie relevantnych poloziek skladu podla SKLADU
                var zoz = _db.PolozkaSkladuMnozstva.Include(x => x.PolozkaSkladuX)
                    .Include(x => x.SkladX)
                    .Where(x => x.Sklad == Sklad.ID).ToList();
                //.ForEachAsync(x => ZoznamPoloziekSkladu.Add(x.PolozkaSkladuX));
                foreach (var item in zoz)
                {
                    ZoznamPoloziekSkladu.Add(item.PolozkaSkladuX);
                }
                await _sessionStorage.SetItemAsync("SkladPolozkyLoaded", true);
                NacitavaniePoloziek = false;        //nacitanie poloziek skladu je dokoncene
            }
            else // ak zoznam obsahuje polozky
            {
                if (!(await _sessionStorage.GetItemAsync<bool>("SkladPolozkyLoaded")))   //ak je nastavene ze chceme aktualizovat polozky
                {
                    await AktualizujPolozky();
                }
            }
        }
        public async Task AktualizujPolozky()
        {
            NacitavaniePoloziek = true;
            ZoznamPoloziekSkladu.Clear();
            await LoadPolozky();
        }

        public void VymazPolozku(PolozkaS poloz)
        {
            _db.PolozkySkladu.Remove(poloz);
            ZoznamPoloziekSkladu.Remove(poloz);
            _db.SaveChanges();
        }

        public bool MoznoVymazat(PolozkaS poloz)
        {
            var activcon = _db.PolozkaSkladuMnozstva.Where(x => x.PolozkaSkladu == poloz.ID);
            foreach (var item in activcon)
            {
                if (item.Mnozstvo != 0)
                {
                    return false;
                }
            }
            var activObj = _db.PolozkySkladuObjednavky.Include(x => x.ObjednavkaX).Where(x => x.PolozkaSkladu == poloz.ID);
            foreach (var item in activObj)
            {
                if (item.ObjednavkaX.Stav != DBLayer.Models.StavOBJ.Ukoncena)
                {
                    return false;
                }
            }
            return true;
        }

        public void LoadMnozstvo()
        {
            if (ZoznamPoloziekSkladu.Count != 0)
            {
                ClearNumZoznam();
                foreach (var item in ZoznamPoloziekSkladu)
                {
                    var founded = _db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.ID && x.Sklad == Sklad.ID);
                    if (founded != null)
                    {
                        item.Mnozstvo = founded.Mnozstvo;
                    }
                    else
                    {
                        Debug.WriteLine("Pri načitavani množstva nebolo možné nájsť položku skladu");
                    }
                }
                NacitaneMnozstvo = true;
            }
        }
        public void CheckIsLastObdobie()
        {
            var obdob = GetObdobia();
            foreach (var item in obdob)
            {
                if (Ssklad.DateFromShortForm(item) > Ssklad.DateFromShortForm(Obdobie))
                {
                    AktualneObdobie = false;
                    return;
                }
            }
            AktualneObdobie = true;
        }

        public void ClearNumZoznam()
        {
            foreach (var item in ZoznamPoloziekSkladu)
            {
                item.Mnozstvo = 0;
            }
        }
    }
}
