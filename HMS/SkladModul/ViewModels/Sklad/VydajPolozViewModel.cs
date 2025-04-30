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
using UniComponents;
using Ssklad = DBLayer.Models.Sklad;
using Vvydajka = DBLayer.Models.Vydajka;

namespace SkladModul.ViewModels.Sklad
{
    public class VydajPolozViewModel : AObservableViewModel<Vvydajka>
    {
        public DateTime Obdobie { get; set; } = new();
        public Ssklad Sklad { get; set; } = new();
        public bool IsObdobieActual { get; set; } = false;

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

        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(async () =>
            {
                var token = CancellationTokenSource.Token;

                ZoznamPoloziek = new(await _db.Vydajky
                    .Include(x => x.SkladX)
                    .Include(x => x.DruhPohybuX)
                    .Where(x => x.Obdobie >= Obdobie && x.Sklad == Sklad.ID)
                    .OrderByDescending(x => x.Vznik)
                    .ToListAsync(token));

                token.ThrowIfCancellationRequested();
                IsObdobieActual = SkladObdobie.IsObdobieActual(Sklad, Obdobie, in _db);
            });
        }

        public override bool MoznoVymazat(Vvydajka polozka)
        {
            return !polozka.Spracovana;
        }

        public override void Vymazat(Vvydajka poloz)
        {
            base.Vymazat(poloz);
            _db.Vydajky.Remove(poloz);
            _db.SaveChanges();
        }
    }
}
