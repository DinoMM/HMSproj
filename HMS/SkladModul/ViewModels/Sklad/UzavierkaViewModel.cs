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
using PdfCreator.Models;
using System.Drawing;

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
        private List<PolozkaSkladu> zoznamPredaneho = new(); //zoznam predanych mnozstiev

        private double DiffMedziVydatymAPrijatym = 0.0;

        public bool NacitavaniePoloziek = true;
        public bool PdfLoading { get; set; } = false;

        public DateTime Obdobie { get; set; }
        public Ssklad Sklad { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }

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
        public string GetAktualnaCena(PolozkaSkladu poloz)
        {
            return zoznamAktualnehoMnozstva.FirstOrDefault(x => x.ID == poloz.ID)?.Cena.ToString("F3") ?? 0.ToString("F3");
        }
        public string GetAktualnaCenaDPH(PolozkaSkladu poloz)
        {
            return zoznamAktualnehoMnozstva.FirstOrDefault(x => x.ID == poloz.ID)?.CenaDPH.ToString("F3") ?? 0.ToString("F3");
        }
        public string GetPrijateMnozstvo(PolozkaSkladu poloz)
        {
            return zoznamPrijateho.FirstOrDefault(x => x.ID == poloz.ID)?.Mnozstvo.ToString("F3") ?? 0.ToString("F3");
        }
        public string GetVydateMnozstvo(PolozkaSkladu poloz)
        {
            return zoznamVydateho.FirstOrDefault(x => x.ID == poloz.ID)?.Mnozstvo.ToString("F3") ?? 0.ToString("F3");
        }
        public string GetPredaneMnozstvo(PolozkaSkladu poloz)
        {
            return zoznamPredaneho.FirstOrDefault(x => x.ID == poloz.ID)?.Mnozstvo.ToString("F3") ?? 0.ToString("F3");
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
            return (zoznamVydateho.Sum(x => x.CelkovaCena) - zoznamPrevodiekZoSkladu.Sum(x => x.CelkovaCena)).ToString("F3");
        }

        public string GetTotalSumAktual()
        {
            return zoznamAktualnehoMnozstva.Sum(x => x.CelkovaCena).ToString("F4");
        }

        public string GetTotalDiff()
        {
            return DiffMedziVydatymAPrijatym.ToString("F3");
        }

        public void SetProp(Ssklad sk, string ob)
        {
            Sklad = sk;
            Obdobie = Ssklad.DateFromShortForm(ob);
        }

        public async Task LoadZoznamy()       //pred tymto treba spravit kontrolu obdobia
        {
            CancellationTokenSource = new CancellationTokenSource();
            try
            {
                var token = CancellationTokenSource.Token;
                await Task.Run(() =>
                {
                    zoznamPoloziekSkladuMnozstva = new(_db.PolozkaSkladuMnozstva.Include(x => x.PolozkaSkladuX).Include(x => x.SkladX).Where(x => x.Sklad == Sklad.ID).ToList());
                    token.ThrowIfCancellationRequested();

                    ZoznamPoloziek = new(zoznamPoloziekSkladuMnozstva.Select(x => x.PolozkaSkladuX).ToList());
                    foreach (var item in ZoznamPoloziek)
                    {
                        zoznamAktualnehoMnozstva.Add(item.Clon());
                    }
                    token.ThrowIfCancellationRequested();

                    //TODO kontrola spracovania faktury
                    zoznamPrijateho = Ssklad.GetPoctyZPrijemok(Sklad, Obdobie, in _db);
                    token.ThrowIfCancellationRequested();
                    zoznamVydateho = Ssklad.GetPoctyZVydajok(Sklad, Obdobie, in _db);
                    token.ThrowIfCancellationRequested();
                    zoznamPredaneho = Ssklad.GetPoctyZPredaja(Sklad, Obdobie, in _db);
                    token.ThrowIfCancellationRequested();
                    Ssklad.LoadMnozstvoPoloziek(zoznamAktualnehoMnozstva, Sklad, in _db); //aktualne obdobie
                    token.ThrowIfCancellationRequested();

                    foreach (var item in zoznamPrijateho)
                    {
                        zoznamPrijatehoZPrijemok.Add(item.Clon());
                    }

                    token.ThrowIfCancellationRequested();
                    var zoznamPrevodiek = Ssklad.GetPoctyZPrevodiek(Sklad, Obdobie, in _db);    //pripocitanie prevodiek do prijateho mnozstva
                    zoznamPrijateho.AddRange(zoznamPrevodiek);
                    zoznamPrijateho = PolozkaSkladu.ZosumarizujListPoloziek(in zoznamPrijateho);
                    token.ThrowIfCancellationRequested();

                    zoznamPrevodiekZoSkladu = Ssklad.GetPoctyZPrevodiekZoSkladu(Sklad, Obdobie, in _db);    //pripocitanie prevodiek zo skladu do prijateho mnozstva
                    token.ThrowIfCancellationRequested();

                    #region vypocitavanie ceny aktualneho skladu
                    DiffMedziVydatymAPrijatym = 0.0;
                    var ZozPrijat = new List<PolozkaSkladu>();
                    //var ZozVydat = new List<PolozkaSkladu>();
                    //var ZozPrevod = new List<PolozkaSkladu>();
                    foreach (var item in ZoznamPoloziek)
                    {
                        var pridItem = item.Clon();
                        pridItem.Cena = 0.0;
                        pridItem.Mnozstvo = 0.0;
                        ZozPrijat.Add(pridItem);
                        //var pridItem2 = pridItem.Clon();
                        //ZozPrijat.Add(pridItem2);
                        //var pridItem3 = pridItem2.Clon();
                        //ZozPrijat.Add(pridItem);
                    }
                    token.ThrowIfCancellationRequested();
                    var vsetkyObdobia = _db.SkladObdobi.Include(x => x.SkladX).Where(x => x.Sklad == Sklad.ID).OrderBy(x => x.Obdobie).ToList();
                    token.ThrowIfCancellationRequested();
                    foreach (var item in vsetkyObdobia)
                    {
                        token.ThrowIfCancellationRequested();
                        if (item.Obdobie <= Obdobie)
                        {
                            var prijate = Ssklad.GetPoctyZPrijemok(Sklad, item.Obdobie, in _db);
                            var vydateAll = Ssklad.GetPoctyZVydajok(Sklad, item.Obdobie, in _db);
                            var vydatePrevodky = Ssklad.GetPoctyZPrevodiekZoSkladu(Sklad, item.Obdobie, in _db);
                            double sumaVydate = vydateAll.Sum(x => x.CelkovaCena) - vydatePrevodky.Sum(x => x.CelkovaCena);
                            var predanePD = Ssklad.GetPoctyZPredaja(Sklad, item.Obdobie, in _db);

                            DiffMedziVydatymAPrijatym += sumaVydate + predanePD.Sum(x => x.CelkovaCena) - prijate.Sum(x => x.CelkovaCena);


                            ZozPrijat.AddRange(prijate);
                            ZozPrijat = PolozkaSkladu.ZosumarizujListPoloziek(in ZozPrijat, true);
                            PolozkaSkladu.SpracListySpolu(ZozPrijat, in vydateAll, (x, y) => x.Mnozstvo -= y.Mnozstvo);

                            //ZozVydat.AddRange(vydateAll);
                            //ZozVydat = PolozkaSkladu.ZosumarizujListPoloziek(in ZozVydat);
                            //ZozPrevod.AddRange(vydatePrevodky);
                            //ZozPrevod = PolozkaSkladu.ZosumarizujListPoloziek(in ZozPrevod);
                        }
                    }
                    foreach (var item in zoznamAktualnehoMnozstva)
                    {
                        token.ThrowIfCancellationRequested();
                        double num = ZozPrijat.FirstOrDefault(x => x.ID == item.ID)?.Cena ?? 0.0;
                        item.Cena = double.IsNaN(num) ? 0.0 : num;
                    }

                    #endregion
                    NacitavaniePoloziek = false;
                }, token);
            }
            catch (TaskCanceledException e) { }
            catch (OperationCanceledException ex) { }
            

        }

        public bool CheckActualObdobie()
        {
            return SkladObdobie.GetActualObdobieFromSklad(Sklad, in _db) == Obdobie;
        }

        public bool MoznoUzavrietObdobie()
        {
            if (_uservice.IsLoggedUserInRoles(Ssklad.ROLE_R_SKLADOVEPOHYBY))
            {
                return false;
            }
            return SkladObdobie.MoznoUzavrietObdobie(Sklad, Obdobie, in _db, in _uservice);
        }

        public bool UzavrietObdobie()
        {
            return SkladObdobie.UzavrietObdobie(Sklad, Obdobie, zoznamAktualnehoMnozstva, in _db, in _uservice);
        }

        public async Task VytvorPDF() //vygeneruje a otvori objednavku v pdf v default nastavenom programe
        {
            PdfLoading = true;
            await Task.Run(() =>
            {
                UzavierkaPDF creator = new UzavierkaPDF(ZoznamPoloziek.ToList(), zoznamPoloziekSkladuMnozstva, zoznamAktualnehoMnozstva, zoznamPrijateho, zoznamPrijatehoZPrijemok, zoznamVydateho, zoznamPrevodiekZoSkladu, Obdobie, Sklad, DiffMedziVydatymAPrijatym, GetTotalSumAktual());
                creator.GenerujPdf();
                creator.OpenPDF();

            });
            PdfLoading = false;
        }

    }
}
