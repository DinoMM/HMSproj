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
using Vvydajka = DBLayer.Models.Vydajka;
using Microsoft.EntityFrameworkCore;

namespace SkladModul.ViewModels.Sklad
{
    public partial class UzavierkaViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<PolozkaSkladu> zoznamPoloziek = new();

        private List<PolozkaSkladuMnozstvo> zoznamPoloziekSkladuMnozstva = new();
        private List<PohSkup> zoznamPrijemok = new();
        private List<PohSkup> zoznamVydajok = new();

        public DateTime Obdobie { get; set; }
        public Ssklad Sklad { get; set; }

        DBContext _db;

        public UzavierkaViewModel(DBContext db)
        {
            _db = db;
        }

        
        public string GetAktualneMnozstvo(PolozkaSkladu poloz)
        {
            return zoznamPoloziekSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == poloz.ID).Mnozstvo.ToString("F3");
        }
        public string GetPrijateMnozstvo(PolozkaSkladu poloz)
        {
            double mnozstvo = 0;
            foreach (var item in zoznamPrijemok)
            {
                var found = _db.PrijemkyPolozky.Where(x => x.Skupina == item.ID && x.PolozkaSkladu == poloz.ID).ToList();
                foreach (var prid in found)
                {
                    mnozstvo += prid.Mnozstvo;
                }
            }
            return mnozstvo.ToString("F3");
        }
        public string GetVydateMnozstvo(PolozkaSkladu poloz)
        {
            double mnozstvo = 0;
            foreach (var item in zoznamVydajok)
            {
                var found = _db.VydajkyPolozky.Where(x => x.Skupina == item.ID && x.PolozkaSkladu == poloz.ID).ToList();
                foreach (var prid in found)
                {
                    mnozstvo += prid.Mnozstvo;
                }
            }
            return mnozstvo.ToString("F3");
        }

        public void SetProp(Ssklad sk, string ob)
        {
            Sklad = sk;
            Obdobie = Ssklad.DateFromShortForm(ob);
        }

        public void LoadZoznamy()
        {
            zoznamPoloziekSkladuMnozstva = new(_db.PolozkaSkladuMnozstva.Include(x => x.PolozkaSkladuX).Include(x => x.SkladX).Where(x => x.Sklad == Sklad.ID).ToList());
            ZoznamPoloziek = new(zoznamPoloziekSkladuMnozstva.Select(x => x.PolozkaSkladuX));
            zoznamPrijemok = new(_db.Prijemky.Where(x => x.Sklad == Sklad.ID && x.Vznik >= Obdobie && x.Spracovana).ToList()); //vybera od aktualneho mesiaca dalej!
            zoznamVydajok = new(_db.Vydajky.Where(x => x.Sklad == Sklad.ID && x.Vznik >= Obdobie && x.Spracovana).ToList());    //vybera od aktualneho mesiaca dalej!

            var zozPrevodiekSem = _db.Vydajky.Where(x => x.SkladDo == Sklad.ID && x.Vznik >= Obdobie && x.Spracovana).ToList(); //vybera od aktualneho mesiaca dalej!
            foreach (var item in zozPrevodiekSem)
            {
                zoznamPrijemok.Add(item);
            }
        }

    }
}
