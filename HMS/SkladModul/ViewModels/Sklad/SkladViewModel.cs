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
                    Sklad = Sklady.FirstOrDefault()!;
                    Obdobie = Ssklad.ShortFromObdobie(SkladObdobie.GetActualObdobieFromSklad(Sklad, _db)); //ziska aktualne obdobie, ak neexistuje tak sa vytvori
                    AktualneObdobie = true; //mali by sme mat urcite aktualne obdobie po vytvoreni ViewModela

                    if (Ssklad.ZMENAPOLOZIEKROLE.Contains(_userService.LoggedUserRole))
                    {
                        if (Sklady.FirstOrDefault(x => x.ID == "ALL") == null)  //prida moznost pre zobrazenie vsetkych skladovych poloziek
                        {
                            Sklady.Add(new Ssklad() { ID = "ALL", Nazov = "Zobrazenie všetkých položiek"/*, Obdobie = DateTime.Today*/ });
                        }
                    }
                    /*CheckIsObdobieActual();*/     //kontrola ci je obdobie aktualne, nastavenie priznakov

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
            CheckIsObdobieActual();     //kontrola ci je obdobie aktualne
        }

        /// <summary>
        /// Vráti všetky obdobia pre dany sklad, zoradene
        /// </summary>
        /// <returns></returns>
        public List<string> GetObdobiaAsc()
        {
            if (Sklad.ID == "ALL")
            {
                return new List<string> { Ssklad.ShortFromObdobie(DateTime.Today) };
            }

            var list = _db.SkladObdobi.Include(x => x.SkladX)
                .Where(x => x.Sklad == Sklad.ID)
                .OrderBy(x => x.Obdobie)
                .Select(x => Ssklad.ShortFromObdobie(x.Obdobie))
                .ToList();
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
                    if (Sklad.ID != "ALL")
                    {
                        Obdobie = Ssklad.ShortFromObdobie(SkladObdobie.GetActualObdobieFromSklad(Sklad, in _db));
                    }
                    else
                    {
                        Obdobie = Ssklad.ShortFromObdobie(DateTime.Today);
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

        public void VymazPolozku(PolozkaS poloz) //Natvrdo vymaze polozku zo skladu, pozor na integritu dat
        {
            _db.PolozkySkladu.Remove(poloz);
            ZoznamPoloziekSkladu.Remove(poloz);
            _db.SaveChanges();
        }
        /// <summary>
        /// Ak existuje pouzitie polozky alebo ma mnozstvo stale na sklade.
        /// </summary>
        /// <param name="poloz"></param>
        /// <returns></returns>
        public bool MoznoVymazat(PolozkaS poloz)
        {
            var found = _db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == poloz.ID);
            if (found is not null && found.Mnozstvo > 0)
            {
                return false;
            }
            if (Ssklad.ExistujePouzitie(poloz, in _db))
            {
                return false;
            }
            return true;
        }

        public void LoadMnozstvo()
        {
            if (ZoznamPoloziekSkladu.Count != 0)
            {
                ClearNumZoznam();
                if (Sklad.ID != "ALL")  //pre vsetky sklady nacita vsetky polozky
                {
                    var nemozno = Ssklad.LoadMnozstvoPoloziek(ZoznamPoloziekSkladu, Sklad, in _db); //pre existujuci sklad
                    if (nemozno.Count != 0)
                    {
                        Debug.WriteLine("Nemožno načítat množstvo niektorých položiek");
                    }
                }
                else //pre vsetky sklady nacita vsetky polozky - MOZE TRVAT DLHO
                {
                    var skladyBezAll = Sklady.Where(x => x.ID != "ALL").ToList(); //odstranime sklad ALL z listu

                    var zoznamPoloziekSkladuSum = new List<PolozkaS>();
                    foreach (var item in ZoznamPoloziekSkladu) { //pridavanie poloziek do zoznamu, musime spravit klony
                        zoznamPoloziekSkladuSum.Add(item.Clon());
                    }

                    foreach (var item in skladyBezAll) //zosumuje vsetky sklady
                    {
                        Ssklad.LoadMnozstvoPoloziek(ZoznamPoloziekSkladu, item, in _db); //pre sklad
                        foreach (var ytem in ZoznamPoloziekSkladu) //zosumuje mnozstva
                        {
                            zoznamPoloziekSkladuSum.FirstOrDefault(x => x.ID == ytem.ID).Mnozstvo += ytem.Mnozstvo;
                        }
                        ClearNumZoznam(); //vycistenie zoznamu
                    }
                    foreach (var ytem in ZoznamPoloziekSkladu)//pridanie spoctenych mnozstiev do originalneho zoznamu
                    {
                        ytem.Mnozstvo = zoznamPoloziekSkladuSum.FirstOrDefault(x => x.ID == ytem.ID).Mnozstvo;
                    }
                }

                NacitaneMnozstvo = true;
            }
        }
        /// <summary>
        /// Nastavi aktualne obdobie 'AktualneObdobie' ak je aktualne
        /// </summary>
        public void CheckIsObdobieActual()
        {
            if (Sklad.ID == "ALL")
            {
                return;
            }

            if (SkladObdobie.GetActualObdobieFromSklad(Sklad, in _db) == Ssklad.DateFromShortForm(Obdobie)) // ak je obdobie aktualne
            {
                AktualneObdobie = true;
            }
            else
            {
                AktualneObdobie = false;
            }

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
