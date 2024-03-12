using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
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
    public partial class PridPolozkyViewModel : ObservableObject
    {
        [ObservableProperty]
        OBJ objednavka = new();
        [ObservableProperty]
        ObservableCollection<PolozkaSkladuObjednavky> zoznamObjednavky;

        readonly DBContext _db;
        public bool Exitujuca { get; set; } = false;
        public PridPolozkyViewModel(DBContext db)
        {
            _db = db;
            zoznamObjednavky = new();
        }

        public void ResetOBJ()
        {
            Objednavka = new();
        }

        public bool IsOBJValidna(bool ajId = true)
        {
            return OBJ.JeObjednavkaOK(Objednavka, ajId);
        }

        public void LoadZoznamObjednavky()      //treba pred tym nastavit Existujuca
        {
            if (!Exitujuca)
            {
                ZoznamObjednavky = new(
                    _db.PolozkySkladuObjednavky.Include(x => x.PolozkaSkladuX).Where(x => x.Objednavka == Objednavka.ID));
            }
        }
    }
}
