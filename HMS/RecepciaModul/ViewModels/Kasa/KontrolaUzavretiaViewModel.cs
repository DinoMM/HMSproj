using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using PpDoklad = DBLayer.Models.PokladnicnyDoklad;
using Kkasa = DBLayer.Models.Kasa;
namespace RecepciaModul.ViewModels
{
    public partial class KontrolaUzavretiaViewModel : ObservableObject
    {
        public List<Kkasa> ZoznamKas = new();
        #region vstupy
        [Required(ErrorMessage = "Povinné pole.")]
        public DateTime DatumOd { get; set; }
        [Required(ErrorMessage = "Povinné pole.")]
        [DateNotSoonerThan("DatumOd", ErrorMessage = "Nesmie by skorej ako dátum od.")]
        public DateTime DatumDo { get; set; }

        [Required(ErrorMessage = "Povinné pole.")]
        public string VybranaKasa { get; set; }
        public Kkasa VybranaKasaX { get; set; }
        #endregion
        #region vystupy
        public decimal VyslednaSuma { get; set; } = 0;
        public decimal VyslednaSumaDPH { get; set; }
        public decimal DPH { get; set; } = 0;
        public int Pocet { get; set; } = 0;

        public decimal VyslednaSumaNesprac { get; set; }
        public decimal VyslednaSumaDPHNesprac { get; set; }
        public decimal DPHNesprac { get; set; } = 0;
        public int PocetNesprac { get; set; } = 0;
        #endregion
        public bool NacitavaniePoloziek { get; private set; } = true;

        private readonly DBContext _db;
        private readonly UserService _userService;

        public KontrolaUzavretiaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
            DatumOd = StartOfTheDay(DateTime.Now);
            DatumDo = EndOfTheDay(DateTime.Now);
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(PpDoklad.ROLE_R_POKLDOKL);
        }

        public async Task NacitajZoznamy()
        {
            ZoznamKas.AddRange(_db.Kasy
                .Include(x => x.DodavatelX)
                .Include(x => x.ActualUserX)
                .ToList());

            NacitavaniePoloziek = false;
        }

        public DateTime StartOfTheDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }
        public DateTime EndOfTheDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }

        public void SetKasa(Kkasa nova)
        {
            VybranaKasa = nova.ID;
            VybranaKasaX = nova;
        }

        public bool Vypocitaj()
        {
            DatumOd = StartOfTheDay(DatumOd);
            DatumDo = EndOfTheDay(DatumDo);

            //nacitanie dokladov
            var doklady = _db.PokladnicneDoklady
                .Include(x => x.KasaX)
                .Where(x =>
                x.Vznik >= DatumOd
                && x.Vznik <= DatumDo)
                .ToList();
            var dokladySchvalene = doklady.Where(x => x.Spracovana && x.Kasa == VybranaKasa).ToList();
            var dokladyNeSchvalene = doklady.Where(x => !x.Spracovana && string.IsNullOrEmpty(x.Kasa)).ToList();

            //schvalene
            var itemySchvalene = new List<ItemPokladDokladu>();
            foreach (var item in dokladySchvalene)
            {
                itemySchvalene.AddRange(_db.ItemyPokladDokladu
                    .Where(x => x.Skupina == item.ID)
                    .ToList());
            }
            //var sumPoloziekSchvalene = PolozkaSkladu.ZosumarizujListPoloziek(itemySchvalene);

            //neschvalene
            var itemyNeSchvalene = new List<ItemPokladDokladu>();
            foreach (var item in dokladyNeSchvalene)
            {
                itemyNeSchvalene.AddRange(_db.ItemyPokladDokladu
                    .Where(x => x.Skupina == item.ID)
                    .ToList());
            }
            //var sumPoloziekNeSchvalene = PolozkaSkladu.ZosumarizujListPoloziek(itemySchvalene);

            //sumarizacia
            VyslednaSuma = (decimal)itemySchvalene.Sum(x => x.CelkovaCena);
            VyslednaSumaDPH = (decimal)itemySchvalene.Sum(x => x.CelkovaCenaDPH);
            if (VyslednaSuma != 0)
            {
                DPH = ((VyslednaSumaDPH - VyslednaSuma) / VyslednaSuma) * 100;
            }
            else
            {
                DPH = 0;
            }
            Pocet = dokladySchvalene.Count;

            VyslednaSumaNesprac = (decimal)itemyNeSchvalene.Sum(x => x.CelkovaCena);
            VyslednaSumaDPHNesprac = (decimal)itemyNeSchvalene.Sum(x => x.CelkovaCenaDPH);
            if (VyslednaSumaNesprac != 0)
            {
                DPHNesprac = ((VyslednaSumaDPHNesprac - VyslednaSumaNesprac) / VyslednaSumaNesprac) * 100;
            }
            else
            {
                DPHNesprac = 0;
            }
            PocetNesprac = dokladyNeSchvalene.Count;

            return true;
        }

    }
}