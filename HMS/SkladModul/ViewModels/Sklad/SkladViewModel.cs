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
using DBLayer.Migrations;
using static System.Net.Mime.MediaTypeNames;
using UniComponents;


namespace SkladModul.ViewModels.Sklad
{
    public class SkladViewModel : AObservableViewModel<PolozkaS>
    {
        public string Obdobie { get; set; }
        public List<Ssklad> Sklady { get; set; } = new();
        public Ssklad Sklad { get; set; } = new();
        public bool NacitaneMnozstvo { get; set; } = false;
        public bool AktualneObdobie { get; set; } = false;
        //public bool NacitavaniePoloziek { get; private set; } = true;

        //[ObservableProperty]
        //ObservableCollection<PolozkaS> zoznamPoloziekSkladu = new();

        IdentityUserOwn? actualUser = null;


        readonly DBContext _db;
        readonly UserService _userService;
        readonly Blazored.SessionStorage.ISessionStorageService _sessionStorage;

        public SkladViewModel(DBContext db, UserService userService, Blazored.SessionStorage.ISessionStorageService sessionStorage)
        {
            _db = db;
            _userService = userService;
            _sessionStorage = sessionStorage;
        }

        public void SetProp(Ssklad sk, string ob)
        {
            if (Sklady.Any(x => x.ID == sk.ID))
            {
                Sklad = sk;
                Obdobie = ob;
                CheckIsObdobieActual();     //kontrola ci je obdobie aktualne
            }
        }

        public void NacitatDostupneSklady()
        {
            if (actualUser != null && _userService.IsThisUserLoggedIn(actualUser))  //ak neni ziaden actualUser alebo actualUser neni prihlaseny -> nacitat sklady
            {
                return;
            }
            actualUser = _userService.LoggedUser;
            #region nacitanie typ skladov
            var skladuziv = _db.SkladUzivatelia.Include(x => x.SkladX).Where(x => x.Uzivatel == actualUser.Id).ToList();

            if (skladuziv.Count > 0 || _userService.IsLoggedUserInRoles(Ssklad.ZMENAPOLOZIEKROLE))
            {
                if (_userService.IsLoggedUserInRoles(Ssklad.ZMENAPOLOZIEKROLE))
                {
                    Sklady = _db.Sklady.ToList();
                }
                else
                {
                    Sklady = skladuziv.Select(x => x.SkladX).ToList();
                }
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
            #endregion
        }

        /// <summary>
        /// Vráti všetky obdobia pre dany sklad, zoradene
        /// </summary>
        /// <returns></returns>
        public List<string> GetObdobiaDes()
        {
            if (Sklad.ID == "ALL")
            {
                var listOdb = new List<string>() { "AKTU" };
                listOdb.AddRange(ObdobiaPreAll().Select(x => Ssklad.ShortFromObdobie(x)).ToList());
                return listOdb;
            }

            var list = _db.SkladObdobi.Include(x => x.SkladX)
                .Where(x => x.Sklad == Sklad.ID)
                .OrderByDescending(x => x.Obdobie)
                .Select(x => Ssklad.ShortFromObdobie(x.Obdobie))
                .ToList();
            return list;
        }
        private List<DateTime> ObdobiaPreAll()
        {
            var skladyBezAll = Sklady.Where(x => x.ID != "ALL").ToList(); //odstranime sklad ALL z listu
            List<DateTime> obdSkladu = new();
            foreach (var item in skladyBezAll)
            {
                obdSkladu.AddRange(SkladObdobie.GetObdobiaPo(item, new DateTime(), in _db));
            }
            obdSkladu = obdSkladu.Distinct().OrderByDescending(x => x).ToList();
            return obdSkladu;
        }
        public async Task SetSklad(string ID)
        {
            var found = Sklady.FirstOrDefault(x => x.ID == ID);
            if (found != null)
            {
                if (Sklad.ID != found.ID)
                {
                    Sklad = found;
                    if (Sklad.ID != "ALL")  //pre jednotlivy sklad
                    {
                        Obdobie = Ssklad.ShortFromObdobie(SkladObdobie.GetActualObdobieFromSklad(Sklad, in _db));
                    }
                    else //pre vsetky sklady
                    {
                        Obdobie = "AKTU";
                    }
                    //ZoznamPoloziek.Clear();
                    await NacitajZoznamy();
                    OnCollectionChanged(this, new(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
                }
            }
        }
        protected override async Task NacitajZoznamyAsync()
        {
            //if (ZoznamPoloziek.Count == 0)
            //{

                if (Sklad.ID == "ALL")      //pri zobrazeni vsetkych poloziek pri mode ALL
                {
                    ZoznamPoloziek = new(_db.PolozkySkladu.ToList());
                    return;
                }

                //pridanie relevantnych poloziek skladu podla SKLADU
                var zoz = _db.PolozkaSkladuMnozstva.Include(x => x.PolozkaSkladuX)
                    .Include(x => x.SkladX)
                    .Where(x => x.Sklad == Sklad.ID).ToList();
                //.ForEachAsync(x => ZoznamPoloziekSkladu.Add(x.PolozkaSkladuX));
                ZoznamPoloziek = new();
                foreach (var item in zoz)
                {
                    ZoznamPoloziek.Add(item.PolozkaSkladuX);
                }
                await _sessionStorage.SetItemAsync("SkladPolozkyLoaded", true);
            //}
            //else // ak zoznam obsahuje polozky
            //{
            //    if (!(await _sessionStorage.GetItemAsync<bool>("SkladPolozkyLoaded")))   //ak je nastavene ze chceme aktualizovat polozky
            //    {
            //        await AktualizujPolozky();
            //    }
            //}
        }

        //public async Task LoadPolozky()
        //{
        //    if (ZoznamPoloziekSkladu.Count == 0)
        //    {

        //        if (Sklad.ID == "ALL")      //pri zobrazeni vsetkych poloziek pri mode ALL
        //        {
        //            var zozn = _db.PolozkySkladu.ToList();
        //            foreach (var item in zozn)
        //            {
        //                ZoznamPoloziekSkladu.Add(item);
        //            }
        //            NacitavaniePoloziek = false;        //nacitanie poloziek skladu je dokoncene
        //            return;
        //        }

        //        //pridanie relevantnych poloziek skladu podla SKLADU
        //        var zoz = _db.PolozkaSkladuMnozstva.Include(x => x.PolozkaSkladuX)
        //            .Include(x => x.SkladX)
        //            .Where(x => x.Sklad == Sklad.ID).ToList();
        //        //.ForEachAsync(x => ZoznamPoloziekSkladu.Add(x.PolozkaSkladuX));
        //        foreach (var item in zoz)
        //        {
        //            ZoznamPoloziekSkladu.Add(item.PolozkaSkladuX);
        //        }
        //        await _sessionStorage.SetItemAsync("SkladPolozkyLoaded", true);
        //        NacitavaniePoloziek = false;        //nacitanie poloziek skladu je dokoncene
        //    }
        //    else // ak zoznam obsahuje polozky
        //    {
        //        if (!(await _sessionStorage.GetItemAsync<bool>("SkladPolozkyLoaded")))   //ak je nastavene ze chceme aktualizovat polozky
        //        {
        //            await AktualizujPolozky();
        //        }
        //    }
        //}
        public async Task AktualizujPolozky()
        {
            //NacitavaniePoloziek = true;
            //ZoznamPoloziek.Clear();
            await NacitajZoznamy();
        }

        public void VymazPolozku(PolozkaS poloz) //Natvrdo vymaze polozku zo skladu, pozor na integritu dat
        {
            base.Vymazat(poloz);
            _db.PolozkySkladu.Remove(poloz);
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
            if (ZoznamPoloziek.Count != 0)
            {
                ZoznamPoloziek.CollectionChanged -= OnCollectionChanged;
                ClearNumZoznam();
                if (Sklad.ID != "ALL")  //pre urcity sklad nacita mnozstvo
                {
                    if (CheckIsObdobieActual()) //ak je aktualne obdobie
                    {
                        var nemozno = Ssklad.LoadMnozstvoPoloziek(ZoznamPoloziek, Sklad, in _db); //pre existujuci sklad
                        if (nemozno.Count != 0)
                        {
                            Debug.WriteLine("Nemožno načítat množstvo niektorých položiek");
                        }
                    }
                    else // inak sa dopočítavame k hodnotám podla období
                    {
                        var obdSkladu = SkladObdobie.GetObdobiaPo(Sklad, Ssklad.DateFromShortForm(Obdobie), in _db);

                        List<PolozkaS> zoznamPoloziekObd = new();
                        foreach (var item in ZoznamPoloziek)
                        {
                            zoznamPoloziekObd.Add(item.Clon());
                        }

                        obdSkladu.RemoveAt(0);  //vyhodíme aktuálne obdobie
                        PolozkaSkladuMnozstvo.NastavZaciatocMnozstva(ZoznamPoloziek, Sklad, in _db);
                        foreach (var item in obdSkladu)
                        {
                            Ssklad.LoadMnozstvoZaObdobie(zoznamPoloziekObd, Sklad, item, in _db);
                            var prijem = Ssklad.GetPoctyZPrijemok(Sklad, item, in _db);
                            PolozkaS.SpracListySpolu(ZoznamPoloziek, in zoznamPoloziekObd, (x, y) =>
                            {
                                x.Mnozstvo -= y.Mnozstvo;
                                var prijPoloz = prijem.FirstOrDefault(z => z.ID == x.ID);
                                if (prijPoloz != null)
                                {
                                    x.Cena = x.Mnozstvo != 0 ? ((x.Cena * (x.Mnozstvo + prijPoloz.Mnozstvo) - (prijPoloz.Cena * prijPoloz.Mnozstvo)) / x.Mnozstvo) : prijPoloz.Cena;
                                }
                            });
                            PolozkaS.ResetMnozstva(zoznamPoloziekObd);
                        }
                    }
                }
                else //pre vsetky sklady nacita vsetky polozky - MOZE TRVAT DLHO
                {
                    var skladyBezAll = Sklady.Where(x => x.ID != "ALL").ToList(); //odstranime sklad ALL z listu

                    List<PolozkaS> zoznamPoloziekObd = new();
                    foreach (var item in ZoznamPoloziek)
                    {
                        zoznamPoloziekObd.Add(item.Clon());
                    }

                    if (Obdobie != "AKTU")  //pre urcite obdobie v ALL vypocitavame postupnym zrátavaním
                    {
                        List<DateTime> obdSkladu = ObdobiaPreAll();
                        obdSkladu = obdSkladu.Where(x => x >= Ssklad.DateFromShortForm(Obdobie)).ToList();  //vsetky mozne obdobia

                        foreach (var item in skladyBezAll)      //aktualne mnozstvo v skladoch
                        {
                            Ssklad.LoadMnozstvoPoloziek(zoznamPoloziekObd, item, in _db);
                            PolozkaS.SpracListySpolu(ZoznamPoloziek, in zoznamPoloziekObd, (x, y) => x.Mnozstvo += y.Mnozstvo);
                            PolozkaS.ResetMnozstva(zoznamPoloziekObd);
                        }
                        foreach (var item in skladyBezAll)  //pre vsetky sklady prejdeme vsetky obdobia
                        {
                            foreach (var ytem in obdSkladu)
                            {

                                Ssklad.LoadMnozstvoZaObdobie(zoznamPoloziekObd, item, ytem, in _db);
                                PolozkaS.SpracListySpolu(ZoznamPoloziek, in zoznamPoloziekObd, (x, y) => x.Mnozstvo -= y.Mnozstvo);
                                PolozkaS.ResetMnozstva(zoznamPoloziekObd);
                            }
                        }
                    }
                    else  //pre aktualne obdobie zratame vsetky sklady v akuálnom období
                    {
                        foreach (var item in skladyBezAll)  //aktualne mnozstvo v skladoch
                        {
                            Ssklad.LoadMnozstvoPoloziek(zoznamPoloziekObd, item, in _db);
                            PolozkaS.SpracListySpolu(ZoznamPoloziek, in zoznamPoloziekObd, (x, y) => x.Mnozstvo += y.Mnozstvo);
                            PolozkaS.ResetMnozstva(zoznamPoloziekObd);
                        }
                    }
                }
                ZoznamPoloziek.CollectionChanged += OnCollectionChanged;
                NacitaneMnozstvo = true;
            }
        }
        /// <summary>
        /// Nastavi aktualne obdobie 'AktualneObdobie' ak je aktualne
        /// </summary>
        public bool CheckIsObdobieActual()
        {
            if (Sklad.ID == "ALL")
            {
                AktualneObdobie = Obdobie == "AKTU";    //pre AKTU je pri ALL aktualne obdobie vsetkych skladov
                return AktualneObdobie;
            }

            if (SkladObdobie.GetActualObdobieFromSklad(Sklad, in _db) == Ssklad.DateFromShortForm(Obdobie)) // ak je obdobie aktualne
            {
                AktualneObdobie = true;
            }
            else
            {
                AktualneObdobie = false;
            }
            return AktualneObdobie;

        }

        public void ClearNumZoznam()
        {
            foreach (var item in ZoznamPoloziek)
            {
                item.Mnozstvo = 0;
            }
        }
    }
}
