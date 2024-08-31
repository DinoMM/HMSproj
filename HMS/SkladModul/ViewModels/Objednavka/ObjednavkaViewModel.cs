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

namespace SkladModul.ViewModels.Objednavka
{
    public partial class ObjednavkaViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<OBJ> zoznamObjednavok = new();
        private DateTime posledneNacitanyDatum = DateTime.Today;

        public bool PdfLoading { get; set; } = false;

        private readonly DBContext _db;
        private readonly PridPolozkyViewModel _polozkyViewModel;

        public ObjednavkaViewModel(DBContext db, PridPolozkyViewModel polozkyViewModel)
        {
            _db = db;
            _polozkyViewModel = polozkyViewModel;
        }

        [RelayCommand]
        private void NacitajDalsie() //nacita dalsi pocet kusov z databazy a prida ich na koniec sekvencie
        {
            var newDalsie = _db.Objednavky
                .Include(x => x.DodavatelX)
                .Include(x => x.OdberatelX)
                .Include(x => x.TvorcaX)
                .Where(x => posledneNacitanyDatum >= x.DatumVznik)
                .OrderBy(x => x.DatumVznik)
                .Take(10);

            if (newDalsie.Any())
            {
                posledneNacitanyDatum = newDalsie.Last().DatumVznik;
            }

            foreach (var item in newDalsie)
            {
                if (!ZoznamObjednavok.Contains(item))
                {
                    ZoznamObjednavok.Add(item);
                }
            }
        }

        [RelayCommand]
        private void Vymazat(OBJ poloz)
        {
            ZoznamObjednavok.Remove(poloz);
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

    }
}
