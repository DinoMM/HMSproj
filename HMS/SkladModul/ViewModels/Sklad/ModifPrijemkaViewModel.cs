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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using UniComponents;

namespace SkladModul.ViewModels.Sklad
{
    public partial class ModifPrijemkaViewModel : ObservableObject
    {


        [ObservableProperty]
        Pprijemka polozka = new();

        public DateTime Obdobie { get; set; }
        public Ssklad Sklad { get; set; }
        public bool Saved { get; set; } = false;

        DBContext _db;

        public ModifPrijemkaViewModel(DBContext db)
        {
            _db = db;
        }

        public void SetProp(Ssklad sk, string ob, Pprijemka? pr = null)
        {
            Sklad = sk;
            Obdobie = Ssklad.DateFromShortForm(ob);
            if (pr != null)
            {
                Polozka = pr;
                if (!string.IsNullOrEmpty(Polozka.ID)) {
                    Saved = true;
                }
            }

        }

        public void Uloz() {
            if (string.IsNullOrEmpty(Polozka.ID)) {     //ak nema ID
                Polozka.ID = PohSkup.DajNoveID(_db.Prijemky);
                _db.Prijemky.Add(Polozka);
            }

            _db.SaveChanges();

        }

    }
}
