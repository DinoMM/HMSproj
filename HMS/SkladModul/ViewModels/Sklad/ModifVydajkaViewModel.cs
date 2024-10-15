using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniComponents;
using Ssklad = DBLayer.Models.Sklad;
using Vvydajka = DBLayer.Models.Vydajka;

namespace SkladModul.ViewModels.Sklad
{
    public partial class ModifVydajkaViewModel : ObservableObject
    {

        [ObservableProperty]
        Vvydajka polozka = new();

        public DateTime Obdobie { get; set; }
        public Ssklad Sklad { get; set; }
        public bool Saved { get; set; } = true;
        public List<Ssklad> Sklady { get; set; } = new();
        public bool Readonly { get; set; } = false;
        public bool AktualneObdobie { get; set; } = true;       //true - mozno vytvorit novu vydajku
        public List<DruhPohybu> DruhyPohybov { get; set; } = new();

        readonly DBContext _db;

        public ModifVydajkaViewModel(DBContext db)
        {
            _db = db;
        }

        public void SetProp(Ssklad sk, string ob, Vvydajka? pr = null)
        {
            Readonly = false;
            Sklad = sk;
            Obdobie = Ssklad.DateFromShortForm(ob); //bez kontroly

            var actualObdobieSkladu = SkladObdobie.GetActualObdobieFromSklad(Sklad, _db); //ziska aktualne obdobie pre sklad
            AktualneObdobie = actualObdobieSkladu == Obdobie;  //ak neni aktualne obdobie (vtedy je v minulosti a vydajku nemozno vytvorit)

            if (pr != null)
            {
                Polozka = pr;
                if (!string.IsNullOrEmpty(Polozka.ID))
                {
                    Saved = true;
                    Readonly = Polozka.Sklad != Sklad.ID;       //ak aktualny sklad sa nerovna so skladom s polozkou tak pravdepodobne ide o prevodku
                }

            }
            else //pre novu vydajku nastavime obdobie
            {
                Polozka.Obdobie = SkladObdobie.GetActualObdobieFromSklad(Sklad, _db);
            }
            if (DruhyPohybov.Count == 0)
            {
                DruhyPohybov.AddRange(_db.DruhyPohybov
                    .Where(x => x.MD.StartsWith("5") && x.MD.Length <= 3)
                    .ToList());     //vybratie podla MD zacinajuceho na 5**
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
                Polozka.ID = PohSkup.DajNoveID(_db.Vydajky, _db);
                _db.Vydajky.Add(Polozka);
                Polozka.Spracovana = false;     //pre istotu, nemoze byt nikdy spracovana ked sa vytvori a je prazdna
            }
            if (string.IsNullOrEmpty(Polozka.Sklad))
            {
                Polozka.Sklad = Sklad.ID;
                Polozka.SkladX = Sklad;
            }

            _db.SaveChanges();
        }
        public void IniSklady()
        {
            if (!Readonly)
            {
                Sklady = new(_db.Sklady.Where(x => x.ID != Sklad.ID));    //musi prebehnut po tom co sa nacita Sklad
            }
            else
            {
                Sklady = new(_db.Sklady.Where(x => x.ID == Polozka.SkladDo));
            }
        }

        public async Task<bool> Kontrola(IModal modaldoskladu, IModal modalzoskladu) //skontroluje ci pole v SkladDo je validne: true -> skladDo je null alebo existujuce | false -> skladDo neexistuje
        {
            if (!string.IsNullOrEmpty(Polozka.SkladDo))
            {
                var skl = _db.Sklady.FirstOrDefault(x => x.ID == Polozka.SkladDo);
                if (skl == null)
                {
                    return false;
                }

                var polozky = _db.VydajkyPolozky.Where(x => x.Skupina == Polozka.ID).ToList();  //polozky vydajky
                var listnemozno = Ssklad.MoznoVydať(polozky, Polozka.SkladX, Obdobie, _db);
                if (listnemozno.Count != 0)     //ak su polozky, ktore nemozno vydat zo skladu
                {
                    //modal nie je mozne vydat zo skladu
                    var vypis = "Nemožno vydať viacej ako je na sklade:<br>";
                    foreach (var item in listnemozno)
                    {
                        vypis += item + "<br>";
                    }
                    modaldoskladu.UpdateText(vypis);
                    await modaldoskladu.OpenModal();
                    return false;
                }
                listnemozno = Ssklad.MoznoDodat(polozky, skl, _db);
                if (listnemozno.Count != 0)     //ak su polozky ktore nemozno dodat do skladu
                {
                    //modal najdene polozky, ktore druhy sklad neprijme
                    var vypis = "Nemožno vydať položky do [" + skl.ID + "]:<br>";
                    foreach (var item in listnemozno)
                    {
                        vypis += item + "<br>";
                    }
                    modalzoskladu.UpdateText(vypis);
                    await modalzoskladu.OpenModal();
                    return false;
                }
                //OK

                Polozka.SkladDo = skl.ID;
                Polozka.SkladDoX = skl;
                Polozka.ObdobieDo = SkladObdobie.GetActualObdobieFromSklad(skl, _db);
                return true;
            }
            Polozka.SkladDo = null;
            Polozka.SkladDoX = null;
            Polozka.ObdobieDo = null;
            return true;
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
            Sklady.Clear();
        }

        public async Task SpracujVydajku(IModal zoznamprazdnymodal, IModal modaldoskladu)
        {
            if (!Polozka.Spracovana)
            {
                if (!ObsahujePolozky())
                {
                    await zoznamprazdnymodal.OpenModal();
                    return;
                }

                var polozky = _db.VydajkyPolozky.Where(x => x.Skupina == Polozka.ID).ToList();  //polozky vydajky
                var listnemozno = Ssklad.MoznoVydať(polozky, Polozka.SkladX, Obdobie, _db);
                if (listnemozno.Count != 0)     //ak su polozky, ktore nemozno vydat zo skladu
                {
                    //modal nie je mozne vydat zo skladu
                    var vypis = "Nemožno vydať viacej ako je na sklade:<br>";
                    foreach (var item in listnemozno)
                    {
                        vypis += item + "<br>";
                    }
                    modaldoskladu.UpdateText(vypis);
                    await modaldoskladu.OpenModal();
                    return;
                }

                //foreach (var item in polozky)       //odobranie poloziek zo skladu ZO
                //{
                //    var found = _db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == item.PolozkaSkladu && x.Sklad == Polozka.Sklad);
                //    if (found != null)
                //    {
                //        found.Mnozstvo -= item.Mnozstvo;
                //    }
                //    else
                //    {
                //        Debug.WriteLine("Chybna polozka v zozname pri spracovani prijemky");
                //    }
                //}

                //if (!string.IsNullOrEmpty(Polozka.SkladDo)) //prevodka - pripocitanie do skladu
                //{
                //    DBLayer.Models.Prijemka.PrijatNaSklad(polozky, Polozka.SkladDoX, _db);
                //}


                Polozka.Spracovana = true;
                _db.SaveChanges();
            }

        }




        public bool ObsahujePolozky()
        {
            if (string.IsNullOrEmpty(Polozka.ID))
            {
                return false;
            }
            return _db.VydajkyPolozky.FirstOrDefault(x => x.Skupina == Polozka.ID) != null;
        }

    }
}
