using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
using DBLayer.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkladModul.ViewModels.Objednavka
{
    public partial class AddObjednavViewModel : ObservableObject
    {
        [ObservableProperty]
        private string ico = default!;

        [ObservableProperty]
        private string nazovDodavatela = default!;

        [ObservableProperty]
        private string popis = default!;

        [ObservableProperty]
        Dodavatel odberatel = new();


        private readonly PridPolozkyViewModel _PDViewModel;
        private readonly DBContext _db;
        public bool CorrectDod { get; set; } = false;
        public bool CorrectOdo { get; set; } = false;

        private DBLayer.Models.Objednavka? pridObj;


        public AddObjednavViewModel(DBContext db, PridPolozkyViewModel PDViewModel)
        {
            _PDViewModel = PDViewModel;
            _db = db;
            _PDViewModel.ResetOBJ();
            pridObj = new();
        }

        [RelayCommand]
        private void ZmenaDod(ChangeEventArgs param)
        {
            Ico = (string)param.Value!;
            List<Dodavatel> list = new() { new Dodavatel() { ICO = "123", Nazov = "EEJO" } };
            var najDod = list.FirstOrDefault(x => x.ICO == Ico); //_db.Dodavatelia
            if (najDod is null)
            {
                CorrectDod = false;
                NazovDodavatela = default!;
                return;
            }
            NazovDodavatela = najDod.Nazov;
            CorrectDod = true;

            pridObj.Dodavatel = najDod.Nazov;
            pridObj.DodavatelX = najDod;
        }

        [RelayCommand]
        private void ZmenaOdo(ChangeEventArgs param)
        {
            Odberatel.ICO = (string)param.Value!;
            List<Dodavatel> list = new() { new Dodavatel() { ICO = "123", Nazov = "EEJO" } };
            var najDod = list.FirstOrDefault(x => x.ICO == Odberatel.ICO); //_db.Dodavatelia
            if (najDod is null)
            {
                CorrectOdo = false;
                Odberatel.Nazov = default!;
                return;
            }
            Odberatel.Nazov = najDod.Nazov;
            CorrectOdo = true;

            pridObj.Odberatel = najDod.Nazov;
            pridObj.OdberatelX = najDod;
        }

        [RelayCommand]
        private void NastavObjednavku() {
            _PDViewModel.Objednavka.Dodavatel = pridObj.Dodavatel;
            _PDViewModel.Objednavka.DodavatelX = pridObj.DodavatelX;
            _PDViewModel.Objednavka.Odberatel = pridObj.Odberatel;
            _PDViewModel.Objednavka.OdberatelX = pridObj.OdberatelX;
            _PDViewModel.Objednavka.Popis = Popis;
        }


    }
}
