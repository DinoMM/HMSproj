using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ssklad = DBLayer.Models.Sklad;
using Pprijemka = DBLayer.Models.Prijemka;
using CommunityToolkit.Mvvm.Input;

namespace SkladModul.ViewModels.Sklad
{
    public partial class PrijemPolozViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<PohSkup> zoznamPrijemok = new();
        public DateTime Obdobie { get; set; }
        public Ssklad Sklad { get; set; }

        DBContext _db;

        public PrijemPolozViewModel(DBContext db)
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
            ZoznamPrijemok = new(_db.Prijemky.Include(x => x.SkladX)        
                .Where(x => x.Vznik >= Obdobie && x.Sklad == Sklad.ID)
                .ToList());     //zoznam prijemok
            var prevodky = _db.Vydajky.Include(x => x.SkladX).Include(x => x.SkladDoX).Where(x => x.Vznik >= Obdobie && x.SkladDo == Sklad.ID)
                .ToList();      //zoznam vydajok urcene pre aktualny sklad
            foreach (var item in prevodky)  //pridanie prevodiek do zoznamu
            {
                ZoznamPrijemok.Add(item);
            }
            ZoznamPrijemok.OrderByDescending(x => x.Vznik);     //utriedenie
        }

        [RelayCommand]
        private void Vymaz(Pprijemka poloz)
        {
            ZoznamPrijemok.Remove(poloz);
            _db.Prijemky.Remove(poloz);
            _db.SaveChanges();

        }
    }
}
