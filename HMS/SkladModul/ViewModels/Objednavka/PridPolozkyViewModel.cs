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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using OBJ = DBLayer.Models.Objednavka;

namespace SkladModul.ViewModels.Objednavka
{
    public partial class PridPolozkyViewModel : ObservableObject
    {
        [ObservableProperty]
        OBJ objednavka = new();
        [ObservableProperty]
        ObservableCollection<PolozkaSkladuObjednavky> zoznamObjednavky;
        [ObservableProperty]
        PolozkaSkladuObjednavky novaPoloz;
        [ObservableProperty]
        private bool uprava = true;

        readonly DBContext _db;
        public bool Existujuca { get; set; } = false;
        public bool Zmena { get; set; } = false;
        private List<PolozkaSkladuObjednavky> ZoznamObjednavkySave = new();
        private List<PolozkaSkladuObjednavky> ZoznamObjednavkyNaVymazanie = new();

        public PridPolozkyViewModel(DBContext db)
        {
            _db = db;
            zoznamObjednavky = new();
            novaPoloz = new();
        }

        public void ResetOBJ()
        {
            Objednavka = new();
        }

        public bool IsOBJValidna(bool ajId = true)  //kontrola ci objednavka ma vsetky povinne polia vyplnene, na zaklade ID sa zisti aj ci je uz objednavka v systeme
        {
            return OBJ.JeObjednavkaOK(Objednavka, ajId);
        }

        public void LoadZoznamObjednavky()      //treba pred tym nastavit Existujuca
        {
            if (Existujuca)
            {
                var obj = _db.Objednavky.FirstOrDefault(x => x.ID == Objednavka.ID);
                if (obj != null)
                {
                    obj.SetFromObjednavka(Objednavka);
                    Objednavka = obj;
                }

                var list = _db.PolozkySkladuObjednavky.Include(x => x.PolozkaSkladuX).Where(x => x.Objednavka == Objednavka.ID).ToList();
                foreach (var item in list)
                {
                    ZoznamObjednavky.Add(item.Clon());  //klon hodnoty aby sa nemenila databaza

                }
                ZoznamObjednavkySave.AddRange(ZoznamObjednavky);
            }
        }

        [RelayCommand]
        private void VyhladajPolozku(ChangeEventArgs param)     //vyhlada polozku na zaklade inputu a ked najde, nastavuje novupolozku
        {
            var res = _db.PolozkySkladu.FirstOrDefault(x => x.ID == (string)param.Value);
            if (res == null)
            {
                NovaPoloz = new();
                Uprava = true;
                return;
            }
            Uprava = false;
            NovaPoloz = new();      //naslo polozku tak nacitame info
            NovaPoloz.SetZPolozkySkladu(res);

        }

        [RelayCommand]
        private void PridatPolozku()
        {
            Zmena = true;
            Uprava = true;
            if (!Existujuca)
            {      //ak neexistuje objednavka tak vytvorime ID automaticky po pridani prvej polozky
                Objednavka.ID = OBJ.DajNoveID(_db);
                _db.Objednavky.Add(Objednavka);
                Existujuca = true;
            }

            NovaPoloz.Objednavka = Objednavka.ID;
            NovaPoloz.ObjednavkaX = Objednavka;

            //V tomto bode by mala polozka obsahovat vsetko okrem upresneneho nazvu, ceny a mnozstva
            ZoznamObjednavky.Add(NovaPoloz);
            NovaPoloz = new();
        }


        [RelayCommand]
        private void Vymazat(PolozkaSkladuObjednavky poloz)
        {
            ZoznamObjednavky.Remove(poloz);
            ZoznamObjednavkyNaVymazanie.Add(poloz);
            Zmena = true;
        }

        [RelayCommand]
        private void Uloz()
        {

            //if (!Zmena)
            //{
            //    return;
            //}
            //Zmena je true
            //kontrola hodnot, uprava na default ak sa najdu, treba posudit uzivatelom
            bool trebaCheck = false;
            trebaCheck = ZoznamObjednavky.Count == 0;
            foreach (var item in ZoznamObjednavky)
            {
                if (string.IsNullOrWhiteSpace(item.Nazov))
                {
                    item.Nazov = item.PolozkaSkladuX.Nazov;
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
            //hodnoty su v poriadku, mozeme ulozit
            var obj = _db.Objednavky.FirstOrDefault(x => x.ID == Objednavka.ID);
            if (obj == null)    //ak neexistuje objednávka tak pridame, inak nie je potrebne nic v nej menit
            {
                _db.Add(Objednavka);
            }

            foreach (var item in ZoznamObjednavky)
            {
                var poloz = _db.PolozkySkladuObjednavky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                if (poloz != null) //ak existuje tak ho nahradime, inak len pridáme
                {
                    poloz.SetZPolozkySkladuObjednavky(item);
                }
                else
                {
                    _db.Add(item);
                }
            }
            foreach (var item in ZoznamObjednavkyNaVymazanie)   //prejdenie zoznamu vymazanych ak nahodou bola polozka pred tym v databazi
            {
                var poloz = _db.PolozkySkladuObjednavky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                if (poloz != null) //ak existuje tak ho vymazeme, inak nic
                {
                    _db.Remove(poloz);
                }

            }
            ZoznamObjednavkyNaVymazanie.Clear();

            Zmena = false;      //oznacenie ze vsetko je ulozene v db
            _db.SaveChanges();

        }

        public bool VratDoPovodnehoStavu()  //true - zoznam bude prazdny po vrateni zmien, false - zoznam bude obsahovat hodnoty po vrateni zmien
        {
            bool prazdny = false;
            if (ZoznamObjednavkySave.Count == 0)
            {
                prazdny = true;
            }
            ZoznamObjednavky.Clear();
            ZoznamObjednavky = new(ZoznamObjednavkySave);
            return prazdny;
        }

        [RelayCommand]
        private void OdstranCeluObjednavku()
        {
            if (ZoznamObjednavky.Count != 0)
            {      //ak ma objednavka polozky
                return;
            }

            foreach (var item in ZoznamObjednavkyNaVymazanie)   //prejdenie zoznamu vymazanych ak nahodou bola polozka pred tym v databazi
            {
                var poloz = _db.PolozkySkladuObjednavky.FirstOrDefault(x => x.ID == item.ID); //ak exituje item v databaze
                if (poloz != null) //ak existuje tak ho vymazeme, inak nic
                {
                    _db.Remove(poloz);
                }

            }
            ZoznamObjednavkyNaVymazanie.Clear();

            var obj = _db.Objednavky.FirstOrDefault(x => x.ID == Objednavka.ID);
            if (obj != null)    //ak existuje objednávka tak mazeme
            {
                _db.Remove(obj);
            }
            _db.Remove(Objednavka);
            Objednavka = new();
            Existujuca = false;

            _db.SaveChanges();
        }

        public void VycistiHodnotyForce()
        {
            ZoznamObjednavky.Clear();
            ZoznamObjednavkySave.Clear();
            ZoznamObjednavkyNaVymazanie.Clear();
            Objednavka = new();
            NovaPoloz = new();
            Uprava = true;
            Existujuca = false;
            Zmena = false;

        }

        public bool IsZoznamEmpty()
        {
            return ZoznamObjednavky.Count == 0;
        }
    }
}
