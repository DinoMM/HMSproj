using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBLayer;
using PolozkaS = DBLayer.Models.PolozkaSkladu;
using Ssklad = DBLayer.Models.Sklad;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using DBLayer.Models;

namespace SkladModul.ViewModels.Sklad
{
    public partial class SkladViewModel : ObservableObject
    {
        public string Obdobie { get; set; }
        public List<Ssklad> Sklady { get; set; }
        public Ssklad Sklad { get; set; }

        [ObservableProperty]
        ObservableCollection<PolozkaS> zoznamPoloziekSkladu = new();


        DBContext _db;
        UserService _userService;

        public SkladViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;

            if (_userService.LoggedUser == null)
            {
                Debug.WriteLine("Neni prihlaseny uzivatel");
                return;
            }
            if (true)
            {
                var skladuziv = _db.SkladUzivatelia.Include(x => x.SkladX).Where(x => x.Uzivatel == _userService.LoggedUser.Id).ToList();

                if (skladuziv.Count > 0)
                {
                    Sklady = skladuziv.Select(x => x.SkladX).ToList();
                    Sklad = Sklady.FirstOrDefault();
                    Obdobie = Sklad.ShortformObdobie();
                }
                else
                {
                    Debug.WriteLine("Ziadne sklady!");
                }
            }

        }

        public void SetProp(Ssklad sk, string ob)
        {
            Sklad = sk;
            Obdobie = ob;
        }

        public List<string> GetObdobia()
        {
            var list = new List<string>() { Sklad.ShortformObdobie() };
            return list;
        }
        public void SetSklad(string ID)
        {
            var found = Sklady.FirstOrDefault(x => x.ID == ID);
            if (found != null)
            {
                Sklad = found;
                Obdobie = Sklad.ShortformObdobie();
            }
        }

        public async Task LoadPolozky()
        {
            if (ZoznamPoloziekSkladu.Count == 0)
            {
                await _db.PolozkySkladu.ForEachAsync(x => ZoznamPoloziekSkladu.Add(x));
            }
        }

        public void VymazPolozku(PolozkaS poloz)
        {
            _db.PolozkySkladu.Remove(poloz);
            ZoznamPoloziekSkladu.Remove(poloz);
            _db.SaveChanges();
        }

        public bool MoznoVymazat(PolozkaS poloz)
        {
            var activcon = _db.PolozkaSkladuMnozstva.Where(x => x.PolozkaSkladu == poloz.ID);
            foreach (var item in activcon)
            {
                if (item.Mnozstvo != 0)
                {
                    return false;
                }
            }
            var activObj = _db.PolozkySkladuObjednavky.Include(x => x.ObjednavkaX).Where(x => x.PolozkaSkladu == poloz.ID);
            foreach (var item in activObj)
            {
                if (item.ObjednavkaX.Stav != DBLayer.Models.StavOBJ.Ukoncena)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
