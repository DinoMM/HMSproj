using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ssklad = DBLayer.Models.Sklad;
using Vvydajka = DBLayer.Models.Vydajka;

namespace SkladModul.ViewModels.Sklad
{
    public partial class VydajPolozViewModel : ObservableObject 
    {
        [ObservableProperty]
        ObservableCollection<Vvydajka> zoznamVydajok = new();
        public DateTime Obdobie { get; set; }
        public Ssklad Sklad { get; set; }

        DBContext _db;

        public VydajPolozViewModel(DBContext db)
        {
            _db = db;
        }

        public void SetProp(Ssklad sk, string ob)
        {
            Sklad = sk;
            Obdobie = Ssklad.DateFromShortForm(ob);
        }

        public void LoadZoznam()
        {
            ZoznamVydajok = new(_db.Vydajky.Include(x => x.SkladX).Where(x => x.Obdobie >= Obdobie && x.Sklad == Sklad.ID)
                .OrderByDescending(x => x.Vznik));
        }

        [RelayCommand]
        private void Vymaz(Vvydajka poloz)
        {
            ZoznamVydajok.Remove(poloz);
            _db.Vydajky.Remove(poloz);
            _db.SaveChanges();
        }
    }
}
