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

namespace SkladModul.ViewModels.Sklad
{
    public partial class PrijemPolozViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<Pprijemka> zoznamPrijemok = new();
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
            //string shortOb = Ssklad.ShortFromObdobie(Obdobie);
          /*  var najd = _db.Prijemky.Where(x => x.Vznik >= Obdobie).ToList();
            foreach (var item in najd)
            {
                var existuje = _db.PrijemkyPolozky.Include(x => x.PolozkaSkladuMnozstvaX)
                    .FirstOrDefault(x => x.PolozkaSkladuMnozstvaX.Sklad == Sklad.ID && x.Skupina == item.ID);
                if (existuje != null)
                {
                    ZoznamPrijemok.Add(item);
                }
            }*/
            

        }
    }
}
