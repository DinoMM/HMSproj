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

        public bool IsOBJValidna(bool ajId = true)  //kontrola ci objednavka ma vsetky povinne polia vyplnene, na zaklade ID sa zisti aj ci je uz objednavka v systeme
        {
            return !string.IsNullOrEmpty(PohSkupina.ID);
        }

        public void LoadzoznamPohSkupiny()      //treba pred tym nastavit Existujuca
        {
            if (Existujuca)
            {
                _db.Entry(PohSkupina).State = EntityState.Unchanged; //oznacenie ze v objednavke nerobime ziadnu zmenu pre pripad nejakej nejastnosti
                var list = _db.PrijemkyPolozky.Include(x => x.PolozkaSkladuX).Where(x => x.Skupina == PohSkupina.ID).ToList();
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
                var res = _db.PolozkaSkladuMnozstva.Include(x => x.PolozkaSkladuX)
                    .FirstOrDefault(x => x.PolozkaSkladu == (string)param.Value &&
                    x.Sklad == ((Pprijemka)PohSkupina).Sklad);
                if (res == null)
                {
                    NovaPoloz = (PohJednotka)Activator.CreateInstance(TypeOfPohJednotka);
                    Uprava = true;
                    return;
                }
                Uprava = false;
                NovaPoloz = (PohJednotka)Activator.CreateInstance(TypeOfPohJednotka);       //naslo polozku tak nacitame info
                ((DBLayer.Models.PrijemkaPolozka)NovaPoloz).SetZPolozSkladuMnozstva(res.PolozkaSkladuX);
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

        [RelayCommand]
        private void Uloz()
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
            if (trebaCheck)
            {
                return;
            }

            foreach (var item in ZoznamPohSkupiny)
            {
                var poloz = _db.PrijemkyPolozky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                if (poloz != null) //ak existuje tak ho nahradime, inak len pridáme
                {
                    PolozkaSkladu doc = ((PrijemkaPolozka)item).PolozkaSkladuX;
                    doc.Mnozstvo = item.Mnozstvo;
                    poloz.SetZPolozSkladuMnozstva(doc);
                }
                else
                {
                    _db.PrijemkyPolozky.Add((PrijemkaPolozka)item);
                }
            }
            foreach (var item in zoznamPohSkupinyNaVymazanie)   //prejdenie zoznamu vymazanych ak nahodou bola polozka pred tym v databazi
            {
                var poloz = _db.PrijemkyPolozky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                if (poloz != null) //ak existuje tak ho vymazeme, inak nic
                {
                    _db.PrijemkyPolozky.Remove(poloz);
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
                var poloz = _db.PrijemkyPolozky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                if (poloz != null) //ak existuje tak ho vymazeme, inak nic
                {
                    _db.Remove(poloz);
                }

            }
            zoznamPohSkupinyNaVymazanie.Clear();

            var obj = _db.Prijemky.FirstOrDefault(x => x.ID == PohSkupina.ID);
            if (obj != null)    //ak existuje objednávka tak mazeme
            {
                _db.Remove(obj);
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
