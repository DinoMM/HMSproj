using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Migrations;
using DBLayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ssklad = DBLayer.Models.Sklad;
using Pprijemka = DBLayer.Models.Prijemka;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using UniComponents;
using System.Xml;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace SkladModul.ViewModels.Sklad
{
    public partial class ModifPrijemkaViewModel : ObservableObject
    {


        [ObservableProperty]
        Pprijemka polozka = new();

        public DateTime Obdobie { get; set; }
        public Ssklad Sklad { get; set; }
        public bool Saved { get; set; } = true;
        public double CelkovaSuma { get; set; } = 0;
        public double CelkovaSumaDPH { get; set; } = 0;

        public bool AktualneObdobie { get; set; } = true;       //true - mozno vytvorit novu prijemku
        public List<DruhPohybu> DruhyPohybov { get; set; } = new();

        public List<DBLayer.Models.Objednavka> ZoznamSpracObjednavok = new();
        public bool ObsahujePolozky { get; set; } = false;

        readonly DBContext _db;

        public ModifPrijemkaViewModel(DBContext db)
        {
            _db = db;
        }

        public void SetProp(Ssklad sk, string ob, Pprijemka? pr = null)
        {
            Sklad = sk;
            Obdobie = Ssklad.DateFromShortForm(ob);     //bez kontroly

            var actualObdobieSkladu = SkladObdobie.GetActualObdobieFromSklad(Sklad, _db); //ziska aktualne obdobie pre sklad
            AktualneObdobie = actualObdobieSkladu == Obdobie;  //ak neni aktualne obdobie (vtedy je v minulosti a prijemku nemozno vytvorit)

            if (pr != null) //ak mame prijemku, nastavime ju
            {
                Polozka = pr;
                if (!string.IsNullOrEmpty(Polozka.ID))
                {
                    Saved = true;
                }
                ObsahujePolozky = _db.PrijemkyPolozky.Any(x => x.Skupina == Polozka.ID);
            }
            else //pre novu prijemku nastavime obdobie
            {
                Polozka.Obdobie = SkladObdobie.GetActualObdobieFromSklad(Sklad, _db);
            }
            if (DruhyPohybov.Count == 0)
            {
                DruhyPohybov.AddRange(_db.DruhyPohybov
                    .Where(x => (x.MD.StartsWith("1") || x.MD.StartsWith("3")) && x.MD.Length <= 3)
                    .ToList());     //vybratie podla MD zacinajuceho na 1**
                if (string.IsNullOrEmpty(Polozka.DruhPohybu))
                {
                    Polozka.DruhPohybu = DruhyPohybov.FirstOrDefault()?.ID ?? "";
                    Polozka.DruhPohybuX = DruhyPohybov.FirstOrDefault();
                }
            }
        }
        public async void Uloz()
        {
            if (string.IsNullOrEmpty(Polozka.ID))
            {     //ak nema ID
                Polozka.ID = PohSkup.DajNoveID(_db.Prijemky, _db);
                _db.Prijemky.Add(Polozka);
                Polozka.Spracovana = false;     //pre istotu, nemoze byt nikdy spracovana ked sa vytvori a je prazdna
            }
            if (string.IsNullOrEmpty(Polozka.Sklad))
            {
                Polozka.Sklad = Sklad.ID;
                Polozka.SkladX = Sklad;
            }
            Saved = true;
            _db.SaveChanges();
        }
        public void NeulozZmeny()
        {
            if (!string.IsNullOrEmpty(Polozka.ID))
            {
                _db.Entry(Polozka).State = EntityState.Unchanged;
                _db.SaveChanges();
            }
        }
        public void VycistiSa()
        {
            Polozka = new();
            Saved = false;
            Sklad = new();
            Obdobie = default!;
            CelkovaSuma = 0;
            CelkovaSumaDPH = 0;
            DruhyPohybov.Clear();
        }

        public async Task SpracujPrijemku(IModal zoznamprazdnymodal)
        {
            if (!Polozka.Spracovana)
            {
                if (!ObsahujePolozky)
                {
                    await zoznamprazdnymodal.OpenModal();
                    return;
                }
                #region kontrola, ci je mozne pridat vsetky polozky do skladu

                var polozky = _db.PrijemkyPolozky.Where(x => x.Skupina == Polozka.ID).ToList();  //polozky prijemky

                var nemoznoDodat = Ssklad.MoznoDodat(polozky, Sklad, _db); //ak su polozky spravne, tak ich pridame do skladu

                if (nemoznoDodat.Count != 0) //ak sa nepodarilo pridat vsetky polozky
                {
                    return;
                }

                #endregion

                Polozka.Spracovana = true;
                _db.SaveChanges();

                #region kontrola vsetkych prijemok, ktore maju rovnaku objednavku ci nesplnaju pocty pre ukoncenie objednavky automaticky
                DBLayer.Models.Objednavka.NastavStavZPrijemok(Polozka, in _db);
                #endregion

            }
        }
        public void CelkovaCenaCalc()
        {

            if (string.IsNullOrEmpty(Polozka.ID))
            {
                return;
            }
            var listPoloziek = _db.PrijemkyPolozky.Where(x => x.Skupina == Polozka.ID);
            CelkovaSuma = 0.0;
            CelkovaSumaDPH = 0.0;
            foreach (var item in listPoloziek)
            {
                CelkovaSuma += item.CelkovaCena;
                CelkovaSumaDPH += item.CelkovaCenaDPH;
            }
            return;
        }

        public async Task NacitajZObjednavky(IModal succmodal, IModal notfoundModal, IModal someExcluded)
        {
            Uloz();     //automaticke ulozenie
            if (!Saved)
            {   //ak sa nepodarilo ulozit
                return;
            }
            if (ObsahujePolozky)
            {
                return;
            }
            if (string.IsNullOrEmpty(Polozka.Objednavka))
            { //ak je prazdne pole
                await notfoundModal.OpenModal();
                return;
            }
            var foundedOBJ = _db.Objednavky.FirstOrDefault(x => x.ID == Polozka.Objednavka);
            if (foundedOBJ == null)
            {           //ak sa nenasla objednavka
                await notfoundModal.OpenModal();
                return;
            }

            if (foundedOBJ.Stav == StavOBJ.Vytvorena || foundedOBJ.Stav == StavOBJ.Neschvalena || foundedOBJ.Stav == StavOBJ.Ukoncena)
            {   //len schvalene/aktivne  objednavky
                await notfoundModal.OpenModal();
                return;
            }

            bool vsetkyPridane = true;
            var zoznamObjednavky = _db.PolozkySkladuObjednavky.Where(x => x.Objednavka == foundedOBJ.ID).ToList();
            foreach (var item in zoznamObjednavky)
            {
                if (_db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.PolozkaSkladu && x.Sklad == Polozka.Sklad) != null)
                {
                    //ak naslo vazbu s polozkou a skladom, tak pridame, inak nepridame
                    var nova = new DBLayer.Models.PrijemkaPolozka()
                    {
                        Cena = item.Cena,
                        Mnozstvo = item.Mnozstvo,
                        Nazov = item.Nazov,
                        PolozkaSkladu = item.PolozkaSkladu,
                        Skupina = Polozka.ID
                    };
                    _db.PrijemkyPolozky.Add(nova);
                }
                else
                {
                    vsetkyPridane = false;
                }
            }
            if (!vsetkyPridane)
            {
                await someExcluded.OpenModal(true);
            }
            await succmodal.OpenModal(true);
            _db.SaveChanges();
        }

        public async Task NacitajSpracovaneObjednavky()
        {
            ZoznamSpracObjednavok.Clear();
            ZoznamSpracObjednavok.AddRange(await _db.Objednavky
                .Where(x => x.Stav == StavOBJ.Objednana 
                || x.Stav == StavOBJ.Schvalena)
                .OrderByDescending(x => x.ID)
                .ToListAsync());
        }

    }
}
