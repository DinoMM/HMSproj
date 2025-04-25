using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using PdfCreator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfCreator;

using OBJ = DBLayer.Models.Objednavka;
using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;
using UniComponents;
using System.Collections.Specialized;

namespace SkladModul.ViewModels.Objednavka
{
    public class ObjednavkaViewModel : AObservableViewModel<OBJ>
    {
        private DateTime posledneNacitanyDatum = DateTime.Today;
        private List<PolozkaSkladu> polozkySkladov = new();
        public bool PdfLoading { get; set; } = false;

        public bool Loading { get => Nacitavanie; set => Nacitavanie = value; }

        private readonly DBContext _db;
        private readonly UserService _userService;


        public ObjednavkaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        //[RelayCommand]
        //private void NacitajDalsie() //nacita dalsi pocet kusov z databazy a prida ich na koniec sekvencie
        //{
        //    var newDalsie = _db.Objednavky
        //        .Include(x => x.DodavatelX)
        //        .Include(x => x.OdberatelX)
        //        .Include(x => x.TvorcaX)
        //        .Where(x => posledneNacitanyDatum >= x.DatumVznik)
        //        .OrderByDescending(x => x.DatumVznik)
        //        .Take(10)
        //        .ToList();

        //    if (newDalsie.Any())
        //    {
        //        posledneNacitanyDatum = newDalsie.Last().DatumVznik;
        //    }

        //    foreach (var item in newDalsie)
        //    {
        //        if (!ZoznamObjednavok.Contains(item))
        //        {
        //            ZoznamObjednavok.Add(item);
        //        }
        //    }
        //}
        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(() =>
            {
                ZoznamPoloziek = new(_db.Objednavky
               .Include(x => x.DodavatelX)
               .Include(x => x.OdberatelX)
               .Include(x => x.TvorcaX)
               .OrderByDescending(x => x.ID)
               .ToList());
            });
        }
        public override bool MoznoVymazat(OBJ polozka)
        {
            return polozka.Stav == DBLayer.Models.StavOBJ.Vytvorena ||
                   polozka.Stav == DBLayer.Models.StavOBJ.Neschvalena ||
                   _userService.LoggedUserRole == DBLayer.RolesOwn.Admin ||
                   _userService.LoggedUserRole == DBLayer.RolesOwn.Riaditel
                   &&
                   polozka.Stav != DBLayer.Models.StavOBJ.Ukoncena;
        }

        public override void Vymazat(OBJ poloz)
        {
            base.Vymazat(poloz);
            _db.Objednavky.Remove(poloz);
            _db.SaveChanges();
        }

        public async Task VytvorPDF(OBJ poloz) //vygeneruje a otvori objednavku v pdf v default nastavenom programe
        {
            PdfLoading = true;
            ObjednavkaPDF creator = new ObjednavkaPDF();

            var polozkyZObjednavky = _db.PolozkySkladuObjednavky.Include(x => x.PolozkaSkladuX).Where(x => x.Objednavka == poloz.ID).ToList();   //polozky z objednavky
            await Task.Run(() =>
            {
                creator.GenerujPdf(poloz, poloz.DodavatelX, poloz.OdberatelX, polozkyZObjednavky);
                creator.OpenPDF();

            });
            PdfLoading = false;
        }

        /// <summary>
        /// Duplikuje objednavku spolu s jej polozkami
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task Duplicate(OBJ item)
        {
            Nacitavanie = true;
            await Task.Run(async () =>
            {
                var found = _db.Objednavky.FirstOrDefault(x => x.ID == item.ID);    //kontrola existencie
                if (found != null)
                {
                    var zozn = await _db.PolozkySkladuObjednavky
                    .Where(x => x.Objednavka == found.ID)
                    .ToArrayAsync();    //ziska vsetky polozky objednavky

                    var newOrder = new OBJ()    //vytvorenie novej objednavky
                    {
                        Dodavatel = found.Dodavatel,
                        Odberatel = found.Odberatel,
                        Stav = DBLayer.Models.StavOBJ.Vytvorena,
                        Tvorca = _userService.LoggedUser.Id,
                    };
                    newOrder.ID = DBLayer.Models.Objednavka.DajNoveID(_db);
                    _db.Objednavky.Add(newOrder);   //pridanie

                    var newItems = new List<PolozkaSkladuObjednavky>(); //prejdenie zoznamu a pridavanie poloziek
                    foreach (var item in zozn)
                    {
                        var newItem = new PolozkaSkladuObjednavky()
                        {
                            Objednavka = newOrder.ID,
                            PolozkaSkladu = item.PolozkaSkladu,
                            Mnozstvo = item.Mnozstvo,
                            Cena = item.Cena,
                            DPH = item.DPH,
                            Nazov = item.Nazov,
                        };
                        _db.PolozkySkladuObjednavky.Add(newItem); //pridanie
                    }
                    _db.SaveChanges();
                    ZoznamPoloziek.Insert(0, newOrder); //update zoznamu
                }
            });
            Nacitavanie = false;
        }

        public async Task<List<PolozkaSkladu>> GetPolozkaItems()
        {
            if (polozkySkladov.Count == 0)
            {
                await Nacitaj(methodAsync: async () =>
                {
                    polozkySkladov = await _db.PolozkySkladu
                    .OrderBy(x => x.ID)
                    .ToListAsync();
                });
            }
            return polozkySkladov;
        }

        public async Task<List<OBJ>> GetObjednavkaItems(PolozkaSkladu polozka)
        {
            var objednavky = new List<OBJ>();
            await Nacitaj(methodAsync: async () =>
                {
                    objednavky = await _db.PolozkySkladuObjednavky
                    .Include(x => x.ObjednavkaX)
                    .ThenInclude(x => x.TvorcaX)
                    .Include(x => x.ObjednavkaX)
                    .ThenInclude(x => x.DodavatelX)
                    .Include(x => x.ObjednavkaX)
                    .ThenInclude(x => x.OdberatelX)
                    .Where(x => x.PolozkaSkladu == polozka.ID)
                    .Select(x => x.ObjednavkaX)
                    .Distinct()
                    .OrderByDescending(x => x.ID)
                    .ToListAsync();
                });
            return objednavky;
        }
    }
}
