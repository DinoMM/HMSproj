using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
using DBLayer.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ssklad = DBLayer.Models.Sklad;
using Pprijemka = DBLayer.Models.Prijemka;
using Vvydajka = DBLayer.Models.Vydajka;
using UniComponents;

namespace SkladModul.ViewModels.Sklad
{
    public partial class ModifPolozPohJednotkaViewModel : ObservableObject
    {
        [ObservableProperty]
        PohSkup pohSkupina;
        [ObservableProperty]
        ObservableCollection<PohJednotka> zoznamPohSkupiny;
        public PohJednotka NovaPoloz;
        [ObservableProperty]
        private bool uprava = true;
        public Type TypeOfPohSkupina { get; set; } = typeof(PohSkup);
        public Type TypeOfPohJednotka { get; set; } = typeof(PohJednotka);


        readonly DBContext _db;
        readonly UserService _userService;
        public bool Existujuca { get; set; } = false;
        public bool Zmena { get; set; } = false;
        public bool Deleted = false;
        private List<PohJednotka> zoznamPohSkupinySave = new();
        private List<PohJednotka> zoznamPohSkupinyNaVymazanie = new();

        public ModifPolozPohJednotkaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
            zoznamPohSkupiny = new();
        }

        public bool IsSkupValidna(bool ajId = true)  // ci ma ID
        {
            return !string.IsNullOrEmpty(PohSkupina.ID);
        }

        public void LoadzoznamPohSkupiny()      //treba pred tym nastavit Existujuca
        {
            if (Existujuca)
            {
                _db.Entry(PohSkupina).State = EntityState.Unchanged; //oznacenie ze v objednavke nerobime ziadnu zmenu pre pripad nejakej nejastnosti
                List<PrijemkaPolozka>? list;
                if (TypeOfPohSkupina == typeof(Pprijemka))
                {
                    list = _db.PrijemkyPolozky.Include(x => x.PolozkaSkladuX).Where(x => x.Skupina == PohSkupina.ID).ToList();
                }
                else  //Vydajka
                {
                    list = _db.VydajkyPolozky.Include(x => x.PolozkaSkladuX).Where(x => x.Skupina == PohSkupina.ID).ToList();
                }

                foreach (var item in list)
                {
                    ZoznamPohSkupiny.Add(item.Clon());  //klon hodnoty aby sa nemenila databaza
                }
                zoznamPohSkupinySave.AddRange(ZoznamPohSkupiny);
            }
        }

        [RelayCommand]
        private void VyhladajPolozku(ChangeEventArgs param)     //vyhlada polozku na zaklade inputu a ked najde, nastavuje novupolozku
        {
            if (TypeOfPohJednotka == typeof(DBLayer.Models.PrijemkaPolozka))
            {
                PolozkaSkladuMnozstvo? najd;
                if (TypeOfPohSkupina == typeof(Pprijemka))
                {
                    najd = _db.PolozkaSkladuMnozstva.Include(x => x.PolozkaSkladuX)
                       .FirstOrDefault(x => x.PolozkaSkladu == (string)param.Value &&
                       x.Sklad == ((Pprijemka)PohSkupina).Sklad);
                }
                else //vydajka
                {
                    najd = _db.PolozkaSkladuMnozstva.Include(x => x.PolozkaSkladuX)
                       .FirstOrDefault(x => x.PolozkaSkladu == (string)param.Value &&
                       x.Sklad == ((Vvydajka)PohSkupina).Sklad);
                }

                if (najd == null)
                {
                    NovaPoloz = (PohJednotka)Activator.CreateInstance(TypeOfPohJednotka);
                    Uprava = true;
                    return;
                }

                if (TypeOfPohSkupina == typeof(Vvydajka))
                {
                    if (!string.IsNullOrEmpty(((Vvydajka)PohSkupina).SkladDo))  //ak je vydajka nastavena ako prevodka, musi to byt platny SkladDo
                    {
                        if (_db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.Sklad == ((Vvydajka)PohSkupina).SkladDo && x.PolozkaSkladu == najd.PolozkaSkladu) == null)
                        { //ak mozno priradit polozku do zoznamu
                            NovaPoloz = (PohJednotka)Activator.CreateInstance(TypeOfPohJednotka);
                            Uprava = true;
                            return;
                        }
                    }
                }

                Uprava = false;
                NovaPoloz = (PohJednotka)Activator.CreateInstance(TypeOfPohJednotka);       //naslo polozku tak nacitame info
                ((DBLayer.Models.PrijemkaPolozka)NovaPoloz).SetZPolozSkladuMnozstva(najd.PolozkaSkladuX);
            }
        }

        [RelayCommand]
        private void PridatPolozku()
        {
            Zmena = true;
            Uprava = true;
            NovaPoloz.Skupina = PohSkupina.ID;
            NovaPoloz.SkupinaX = PohSkupina;

            //V tomto bode by mala polozka obsahovat vsetko okrem ...
            ZoznamPohSkupiny.Add(NovaPoloz);
            NovaPoloz = (PohJednotka)Activator.CreateInstance(TypeOfPohJednotka);
        }


        [RelayCommand]
        private void Vymazat(PohJednotka poloz)
        {
            ZoznamPohSkupiny.Remove(poloz);
            zoznamPohSkupinyNaVymazanie.Add(poloz);
            Zmena = true;
        }

        public async Task Uloz(IModal notresmodal)
        {
            if (!Zmena)
            {
                return;
            }
            //Zmena je true
            //kontrola hodnot, uprava na default ak sa najdu, treba posudit uzivatelom
            bool trebaCheck = false;
            trebaCheck = ZoznamPohSkupiny.Count == 0;
            foreach (var item in ZoznamPohSkupiny)
            {
                if (string.IsNullOrWhiteSpace(item.Nazov))
                {
                    //item.Nazov = item.PolozkaSkladuX.Nazov;
                    trebaCheck = true;
                }
                if (item.Cena < 0)
                {
                    item.Cena = 0;
                    trebaCheck = true;
                }
                if (item.Mnozstvo <= 0)
                {
                    item.Mnozstvo = 0;
                    trebaCheck = true;
                }
            }
            if (TypeOfPohSkupina == typeof(Vvydajka)) {     //kontrola mnozstva
                var listnemozno = Ssklad.MoznoVydať(ZoznamPohSkupiny.Cast<PrijemkaPolozka>(), ((Vvydajka)PohSkupina).SkladX, _db);
                if (listnemozno.Count != 0)     //ak su polozky, ktore nemozny vydat zo skladu
                {
                    //modal nie je mozne vydat zo skladu
                    var vypis = "Nemožno vydať viacej ako je na sklade:<br>";
                    foreach (var item in listnemozno)
                    {
                        vypis += item + "<br>";
                    }
                    notresmodal.UpdateText(vypis);
                    await notresmodal.OpenModal(true);
                    return;
                }
            }

            if (trebaCheck)
            {
                return;
            }

            foreach (var item in ZoznamPohSkupiny)
            {
                PrijemkaPolozka? poloz;
                if (TypeOfPohSkupina == typeof(Pprijemka))
                {
                    poloz = _db.PrijemkyPolozky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                }
                else  //vydajka
                {
                    poloz = _db.VydajkyPolozky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                }
                if (poloz != null) //ak existuje tak ho nahradime, inak len pridáme
                {
                    PolozkaSkladu doc = ((PrijemkaPolozka)item).PolozkaSkladuX;
                    doc.Mnozstvo = item.Mnozstvo;
                    poloz.SetZPolozSkladuMnozstva(doc);
                }
                else
                {
                    if (TypeOfPohSkupina == typeof(Pprijemka))
                    {
                        _db.PrijemkyPolozky.Add((PrijemkaPolozka)item);
                    }
                    else
                    {
                        _db.VydajkyPolozky.Add((PrijemkaPolozka)item);
                    }
                }
            }
            foreach (var item in zoznamPohSkupinyNaVymazanie)   //prejdenie zoznamu vymazanych ak nahodou bola polozka pred tym v databazi
            {
                PrijemkaPolozka? poloz;
                if (TypeOfPohSkupina == typeof(Pprijemka))
                {
                    poloz = _db.PrijemkyPolozky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                    if (poloz != null) //ak existuje tak ho vymazeme, inak nic
                    {
                        _db.PrijemkyPolozky.Remove(poloz);
                    }
                }
                else //vydajka
                {
                    poloz = _db.VydajkyPolozky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                    if (poloz != null) //ak existuje tak ho vymazeme, inak nic
                    {
                        _db.VydajkyPolozky.Remove(poloz);
                    }
                }
            }
            zoznamPohSkupinyNaVymazanie.Clear();
            zoznamPohSkupinySave = new(ZoznamPohSkupiny);

            Zmena = false;      //oznacenie ze vsetko je ulozene v db
            _db.SaveChanges();
        }

        public bool VratDoPovodnehoStavu()  //true - zoznam bude prazdny po vrateni zmien, false - zoznam bude obsahovat hodnoty po vrateni zmien
        {
            bool prazdny = false;
            if (zoznamPohSkupinySave.Count == 0)
            {
                prazdny = true;
            }
            ZoznamPohSkupiny.Clear();
            ZoznamPohSkupiny = new(zoznamPohSkupinySave);
            return prazdny;
        }

        [RelayCommand]
        private void OdstranCeluSkupinu()
        {
            if (ZoznamPohSkupiny.Count != 0)
            {      //ak ma objednavka polozky
                Deleted = false;
                return;
            }

            foreach (var item in zoznamPohSkupinyNaVymazanie)   //prejdenie zoznamu vymazanych ak nahodou bola polozka pred tym v databazi
            {
                PrijemkaPolozka? poloz;
                if (TypeOfPohSkupina == typeof(Pprijemka))
                {
                    poloz = _db.PrijemkyPolozky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                    if (poloz != null) //ak existuje tak ho vymazeme, inak nic
                    {
                        _db.PrijemkyPolozky.Remove(poloz);
                    }
                }
                else //vydajka
                {
                    poloz = _db.VydajkyPolozky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                    if (poloz != null) //ak existuje tak ho vymazeme, inak nic
                    {
                        _db.VydajkyPolozky.Remove(poloz);
                    }
                }

            }
            zoznamPohSkupinyNaVymazanie.Clear();

            PohSkup? skp;
            if (TypeOfPohSkupina == typeof(Pprijemka))
            {
                skp = _db.Prijemky.FirstOrDefault(x => x.ID == PohSkupina.ID);
            }
            else //vydajka
            {
                skp = _db.Vydajky.FirstOrDefault(x => x.ID == PohSkupina.ID);
            }
            if (skp != null)    //ak existuje objednávka tak mazeme
            {
                _db.Remove(skp);
            }
            PohSkupina = (PohSkup)Activator.CreateInstance(TypeOfPohSkupina);
            Existujuca = false;

            _db.SaveChanges();
            Deleted = true;
        }

        public void VycistiHodnotyForce()
        {
            ZoznamPohSkupiny.Clear();
            zoznamPohSkupinySave.Clear();
            zoznamPohSkupinyNaVymazanie.Clear();
            PohSkupina = (PohSkup)Activator.CreateInstance(TypeOfPohSkupina);
            NovaPoloz = (PohJednotka)Activator.CreateInstance(TypeOfPohJednotka);
            Uprava = true;
            Existujuca = false;
            Zmena = false;

        }

        public bool IsZoznamEmpty()
        {
            return ZoznamPohSkupiny.Count == 0;
        }

    }
}
