using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OBJ = DBLayer.Models.Objednavka;

namespace SkladModul.ViewModels.Objednavka
{
    public partial class ObjednavkaViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<OBJ> zoznamObjednavok = new();
        private DateTime posledneNacitanyDatum = DateTime.Today;

        private readonly DBContext _db;

        public ObjednavkaViewModel(DBContext db)
        {
            _db = db;
        }

        [RelayCommand]
        private void NacitajDalsie() //nacita dalsi pocet kusov z databazy a prida ich na koniec sekvencie
        {
            var newDalsie = _db.Objednavky
                .Include(x => x.DodavatelX)
                .Include(x => x.OdberatelX)
                .Where(x => posledneNacitanyDatum >= x.DatumVznik)
                .OrderBy(x => x.DatumVznik)
                .Take(10);

            if (newDalsie.Any())
            {
                posledneNacitanyDatum = newDalsie.Last().DatumVznik;
            }

            foreach (var item in newDalsie)
            {
                ZoznamObjednavok.Add(item);
            }
        }
    }
}
