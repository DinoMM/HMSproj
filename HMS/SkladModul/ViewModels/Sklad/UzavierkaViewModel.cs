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
        private ObservableCollection<PolozkaSkladu> zoznamPoloziek = new(); //zoznam poloziek

        private List<PolozkaSkladuMnozstvo> zoznamPoloziekSkladuMnozstva = new(); //zoznam poloziek s mnozstvami na zaciatku obdobia
        private List<PolozkaSkladu> zoznamAktualnehoMnozstva = new(); //zoznam aktualneho mnozstva po zohladneni prijmov a vydajov
        private List<PolozkaSkladu> zoznamPrijateho = new(); //zoznam prijatych mnozstiev
        private List<PolozkaSkladu> zoznamPrijatehoZPrijemok = new(); //zoznam prijatych mnozstiev len z prijemok
        private List<PolozkaSkladu> zoznamVydateho = new(); //zoznam vydatych mnozstiev
        private List<PolozkaSkladu> zoznamPrevodiekZoSkladu = new(); //zoznam vydatych mnozstiev z prevodiek z tohto skladu
        

        public bool NacitavaniePoloziek = true;

        public DateTime Obdobie { get; set; }
        public Ssklad Sklad { get; set; }

        readonly DBContext _db;
        readonly UserService _uservice;

        public UzavierkaViewModel(DBContext db, UserService uservice)
        {
            _db = db;
            _uservice = uservice;
        }

        public string GetMnozstvoZaciatok(PolozkaSkladu poloz)
        {
            return zoznamPoloziekSkladuMnozstva.FirstOrDefault(x => x.PolozkaSkladu == poloz.ID)?.Mnozstvo.ToString("F3") ?? 0.ToString("F3");
        }
        public string GetAktualneMnozstvo(PolozkaSkladu poloz)
        {
            return zoznamAktualnehoMnozstva.FirstOrDefault(x => x.ID == poloz.ID)?.Mnozstvo.ToString("F3") ?? 0.ToString("F3");
        }
        public string GetPrijateMnozstvo(PolozkaSkladu poloz)
        {
            return zoznamPrijateho.FirstOrDefault(x => x.ID == poloz.ID)?.Mnozstvo.ToString("F3") ?? 0.ToString("F3");
        }
        public string GetVydateMnozstvo(PolozkaSkladu poloz)
        {
            return zoznamVydateho.FirstOrDefault(x => x.ID == poloz.ID)?.Mnozstvo.ToString("F3") ?? 0.ToString("F3");
        }
        public PolozkaSkladu GetPolozkaPrijemky(PolozkaSkladu poloz)
        {
            return zoznamPrijatehoZPrijemok.FirstOrDefault(x => x.ID == poloz.ID) ?? new();
        }
        public string GetTotalSumPrijemky()
        {
            return zoznamPrijatehoZPrijemok.Sum(x => x.CelkovaCena).ToString("F3");
        }
        public string GetTotalSumVydajky()
        {
            var sum1 = zoznamVydateho.Sum(x => x.CelkovaCena);
            var sum2 = zoznamPrevodiekZoSkladu.Sum(x => x.CelkovaCena);
            return (zoznamVydateho.Sum(x => x.CelkovaCena) - zoznamPrevodiekZoSkladu.Sum(x => x.CelkovaCena)).ToString("F3");
        }

        public void SetProp(Ssklad sk, string ob)
        {
            Sklad = sk;
            Obdobie = Ssklad.DateFromShortForm(ob);
        }

        public void LoadZoznamy()       //pred tymto treba spravit kontrolu obdobia
        {
            zoznamPoloziekSkladuMnozstva = new(_db.PolozkaSkladuMnozstva.Include(x => x.PolozkaSkladuX).Include(x => x.SkladX).Where(x => x.Sklad == Sklad.ID).ToList());

            ZoznamPoloziek = new(zoznamPoloziekSkladuMnozstva.Select(x => x.PolozkaSkladuX).ToList());
            foreach (var item in ZoznamPoloziek)
            {
                zoznamAktualnehoMnozstva.Add(item.Clon());
            }
            //TODO kontrola spracovania faktury
            zoznamPrijateho = Ssklad.GetPoctyZPrijemok(Sklad, Obdobie, in _db);
            zoznamVydateho = Ssklad.GetPoctyZVydajok(Sklad, Obdobie, in _db);
            Ssklad.LoadMnozstvoPoloziek(zoznamAktualnehoMnozstva, Sklad, in _db); //aktualne obdobie

            foreach (var item in zoznamPrijateho)
            {
                zoznamPrijatehoZPrijemok.Add(item.Clon());
            }

            var zoznamPrevodiek = Ssklad.GetPoctyZPrevodiek(Sklad, Obdobie, in _db);    //pripocitanie prevodiek do prijateho mnozstva
            zoznamPrijateho.AddRange(zoznamPrevodiek);
            zoznamPrijateho = PolozkaSkladu.ZosumarizujListPoloziek(in zoznamPrijateho);

            zoznamPrevodiekZoSkladu = Ssklad.GetPoctyZPrevodiekZoSkladu(Sklad, Obdobie, in _db);    //pripocitanie prevodiek zo skladu do prijateho mnozstva

            NacitavaniePoloziek = false;

        }

        public bool CheckActualObdobie()
        {
            return SkladObdobie.GetActualObdobieFromSklad(Sklad, in _db) == Obdobie;
        }

        public bool MoznoUzavrietObdobie()
        {
            if (_uservice.IsLoggedUserInRoles(Ssklad.ROLE_R_SKLADOVEPOHYBY)) {
                return false;
            }
            return SkladObdobie.MoznoUzavrietObdobie(Sklad, Obdobie, in _db, in _uservice);
        }

        public void UzavrietObdobie()
        {
            SkladObdobie.UzavrietObdobie(Sklad, Obdobie, in _db, in _uservice);
        }

    }
}
