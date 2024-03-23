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

        DBContext _db;

        public ModifPrijemkaViewModel(DBContext db)
        {
            _db = db;
        }

        public void SetProp(Ssklad sk, string ob, Pprijemka? pr = null)
        {
            Sklad = sk;
            Obdobie = Ssklad.DateFromShortForm(ob);
            if (pr != null)
            {
                Polozka = pr;
                if (!string.IsNullOrEmpty(Polozka.ID))
                {
                    Saved = true;
                }
            }

        }
        public async void Uloz()
        {

            if (string.IsNullOrEmpty(Polozka.ID))
            {     //ak nema ID
                Polozka.ID = PohSkup.DajNoveID(_db.Prijemky);
                _db.Prijemky.Add(Polozka);
                Polozka.Spracovana = false;     //pre istotu, nemoze byt nikdy spracovana ked sa vytvori a je prazdna
            }
            if (string.IsNullOrEmpty(Polozka.Sklad))
            {
                Polozka.Sklad = Sklad.ID;
                Polozka.SkladX = Sklad;
            }

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
        }

        public async Task SpracujPrijemku(IModal zoznampradnymodal)
        {
            if (!Polozka.Spracovana)
            {
                if (!ObsahujePolozky())
                {
                    await zoznampradnymodal.OpenModal();
                    return;
                }

                var polozky = _db.PrijemkyPolozky.Where(x => x.Skupina == Polozka.ID).ToList();  //polozky prijemky
                foreach (var item in polozky)       //pridanie poloziek do skladu
                {
                    var found = _db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.PolozkaSkladu && x.Sklad == Polozka.Sklad);
                    if (found != null)
                    {
                        found.Mnozstvo += item.Mnozstvo;
                    }
                    else
                    {
                        Debug.WriteLine("Chybna polozka v zozname pri spracovani prijemky");
                    }
                }

                Polozka.Spracovana = true;
                _db.SaveChanges();
            }
        }

        public double CelkovaCenaCalc()
        {

            if (string.IsNullOrEmpty(Polozka.ID))
            {
                return CelkovaSuma;
            }
            var listPoloziek = _db.PrijemkyPolozky.Where(x => x.Skupina == Polozka.ID);
            CelkovaSuma = 0.0;
            foreach (var item in listPoloziek)
            {
                CelkovaSuma += item.CelkovaCena;
            }
            return CelkovaSuma;
        }

        public bool ObsahujePolozky()
        {
            if (string.IsNullOrEmpty(Polozka.ID))
            {
                return false;
            }
            return _db.PrijemkyPolozky.FirstOrDefault(x => x.Skupina == Polozka.ID) != null;
        }

        public async Task NacitajZObjednavky(IModal succmodal, IModal notfoundModal, IModal someExcluded)
        {
            if (ObsahujePolozky())
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

            if (foundedOBJ.Stav == StavOBJ.Vytvorena || foundedOBJ.Stav == StavOBJ.Neschvalena || foundedOBJ.Stav == StavOBJ.Ukoncena) {   //len aktivne objednavky
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

    }
}
