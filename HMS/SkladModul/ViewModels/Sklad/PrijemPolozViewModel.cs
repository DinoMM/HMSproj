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
        public bool NacitavaniePoloziek { get; set; } = true;

        readonly DBContext _db;

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
            NacitavaniePoloziek = true;
            var prijemky = _db.Prijemky.Include(x => x.SkladX)
                .Where(x => x.Obdobie >= Obdobie && x.Sklad == Sklad.ID)
                .ToList();     //zoznam prijemok
            var prevodky = _db.Vydajky.Include(x => x.SkladX).Include(x => x.SkladDoX).Where(x => x.ObdobieDo >= Obdobie && x.SkladDo == Sklad.ID)
                .ToList();      //zoznam prevodiek urcene pre aktualny sklad, ignorujeme obdobie kedze mozu mat rozdielne obdobia

            List<PohSkup> spojPrAPre = new(prijemky); //spojenie zoznamov
            spojPrAPre.AddRange(prevodky);//spojenie zoznamov
            spojPrAPre = spojPrAPre.OrderByDescending(x =>  x.Vznik).ToList(); //zoradenie podla datumu
            foreach (var item in spojPrAPre)  //pridanie prevodiek do zoznamu
            {
                ZoznamPrijemok.Add(item);
            }
            NacitavaniePoloziek = false;
        }

        /// <summary>
        /// Vymazanie prijemky s overenim
        /// </summary>
        /// <param name="poloz">vymazavana prijemka</param>
        /// <param name="povolenie">true - najvyššie povolenie</param>
        public bool Vymaz(Pprijemka poloz, bool povolenie)
        {
            if (!povolenie)
            {
                if (_db.PrijemkyPolozky.Any(x => x.Skupina == poloz.ID))    //ak sa v prijemke nachadzaju polozky
                {
                    return false;
                }
            }

            ZoznamPrijemok.Remove(poloz);
            _db.Prijemky.Remove(poloz);
            _db.SaveChanges();
            DBLayer.Models.Objednavka.NastavStavZPrijemok(poloz, _db); //nastavenie stavu objednavky
            return true;
        }

        public string GetObdobie(PohSkup polozka)
        {
            if (polozka is Pprijemka)
            {
                return Ssklad.ShortFromObdobie(((Pprijemka)polozka).Obdobie);
            }
            else
            {
                return Ssklad.ShortFromObdobie(((Vydajka)polozka).Obdobie);
            }




        }
    }
}
