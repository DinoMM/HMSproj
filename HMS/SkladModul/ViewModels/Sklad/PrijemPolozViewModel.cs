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
using UniComponents;
using System.Drawing;

namespace SkladModul.ViewModels.Sklad
{
    public class PrijemPolozViewModel : AObservableViewModel<PohSkup>
    {
        public DateTime Obdobie { get; set; } = new();
        public Ssklad Sklad { get; set; } = new();

        public bool IsObdobieActual { get; set; } = false;

        private List<(PohSkup, bool)> MoznoVymazatList = new();

        readonly DBContext _db;
        readonly UserService _userService;

        public PrijemPolozViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public void SetProp(Ssklad sk, string ob)
        {
            Sklad = sk;
            Obdobie = Ssklad.DateFromShortForm(ob);
        }

        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(() =>
            {
                var prijemky = _db.Prijemky
                    .Include(x => x.SkladX)
                    .Include(x => x.DruhPohybuX)
                    .Where(x => x.Obdobie >= Obdobie && x.Sklad == Sklad.ID)
                    .ToList();     //zoznam prijemok
                var prevodky = _db.Vydajky.Include(x => x.SkladX).Include(x => x.SkladDoX).Where(x => x.ObdobieDo >= Obdobie && x.SkladDo == Sklad.ID)
                    .ToList();      //zoznam prevodiek urcene pre aktualny sklad, ignorujeme obdobie kedze mozu mat rozdielne obdobia

                List<PohSkup> spojPrAPre = new(prijemky); //spojenie zoznamov
                spojPrAPre.AddRange(prevodky);//spojenie zoznamov
                ZoznamPoloziek = new(spojPrAPre.OrderByDescending(x => x.Vznik).ToList()); //zoradenie podla datumu

               IsObdobieActual = SkladObdobie.IsObdobieActual(Sklad, Obdobie, in _db);

                MoznoVymazatList = new();
                foreach (var item in ZoznamPoloziek)
                {
                    MoznoVymazatList.Add((item, MoznoVymazat(item, DBLayer.Models.Prijemka.TOTAL_MAZANIE_PRIJEMOK.Contains(_userService.LoggedUserRole))));
                }
            });
        }

        /// <summary>
        /// Vymazanie prijemky s overenim
        /// </summary>
        /// <param name="poloz">vymazavana prijemka</param>
        /// <param name="povolenie">true - najvyššie povolenie</param>
        private bool MoznoVymazat(PohSkup polozka, bool povolenie)
        {
            if (polozka is not Prijemka)
            {
                return false;
            }
            if (!povolenie)
            {
                if (_db.PrijemkyPolozky.Any(x => x.Skupina == polozka.ID))    //ak sa v prijemke nachadzaju polozky
                {
                    return false;
                }
            }
            return true;
        }

        public override bool MoznoVymazat(PohSkup polozka)
        {
            return MoznoVymazatList.FirstOrDefault(x => x.Item1 == polozka).Item2;
        }

        public override void Vymazat(PohSkup poloz)
        {
            base.Vymazat(poloz);
            _db.Prijemky.Remove((Prijemka)poloz);
            _db.SaveChanges();
            DBLayer.Models.Objednavka.NastavStavZPrijemok((Prijemka)poloz, _db); //nastavenie stavu objednavky
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
