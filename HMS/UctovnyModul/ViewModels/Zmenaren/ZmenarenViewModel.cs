using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using currencyapi;
using Azure;
using System.Diagnostics;

namespace UctovnyModul.ViewModels
{
    public partial class ZmenarenViewModel : ObservableObject
    {
        public bool NacitavaniePoloziek { get; private set; } = true;
        public bool ChybaPriNacitavani { get; private set; } = false;

        private Currencyapi currencyapi;
        public List<Currency>? ZoznamCurrencies { get; set; } = new();
        public List<ZmenaMeny> ZoznamTransakcii { get; set; } = new();

        public ZmenaMeny NovaTransakcia { get; set; } = new();

        private readonly DBContext _db;
        private readonly UserService _userService;

        public ZmenarenViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(ZmenaMeny.ROLE_CR_ZMENAREN);
        }

        public bool ValidateUserD()
        {
            return _userService.IsLoggedUserInRoles(ZmenaMeny.ROLE_D_ZMENAREN);
        }

        public async Task NacitajZoznamy()
        {
            ZoznamTransakcii.AddRange(await _db.ZmenyMien
                .OrderByDescending(x => x.ID)
                .ToListAsync());

            try
            {
                currencyapi = new Currencyapi(Environment.GetEnvironmentVariable("API_KEY_CURRENCYAPI") ?? ""); //nastavenie API kluca

                ZoznamCurrencies = Currency.GetListOfCurrenciesFromJsonString(currencyapi.Currencies()); //ziskanie vsetkych mien
                if (ZoznamCurrencies == null)
                {
                    throw new InvalidOperationException();
                }
            }
            catch (Exception ex)
            {
                ChybaPriNacitavani = true;
            }

            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(ZmenaMeny item)
        {
            return true;
        }

        public void Vymazat(ZmenaMeny item)
        {
            var found = _db.ZmenyMien.FirstOrDefault(x => x.ID == item.ID);
            if (found == null)
            {
                return;
            }
            ZoznamTransakcii.Remove(item);
            _db.ZmenyMien.Remove(found);
            _db.SaveChanges();
        }

        public bool SpracujNovuTransakciu()
        {
            NovaTransakcia.Vznik = DateTime.Now;
            _db.ZmenyMien.Add(NovaTransakcia);
            _db.SaveChanges();
            ZoznamTransakcii.Add(NovaTransakcia);
            NovaTransakcia = new();
            return true;
        }

        public CurrencyApiResponse? GetCurrencyApiResponse()
        {
            return CurrencyApiResponse.GetCurrencyApiResponse(currencyapi.Latest(NovaTransakcia.MenaZ, NovaTransakcia.MenaDO)); //ziska aktualny kurz
        }

        public bool CheckIfOk()
        {
            return !string.IsNullOrEmpty(NovaTransakcia.MenaZ)
                && NovaTransakcia.SumaZ > 0;
        }
    }
}