
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
using DBLayer.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecepciaModul;
using System.Collections.ObjectModel;
using System.Diagnostics;
using UniComponents;
using UniComponents.Services;

namespace HMS.ViewModels
{
    public partial class IndexViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<(string, string, List<RolesOwn>, (string bgcoloricon, string biicon))> modulesList;     //(nazov modulu, hyperlink, list povolenych roli, (css farba icony, typ icony bi-))

        public InfoModal CloseAppModal = new();

        public bool IsConnectionChecked { get; set; } = false;

        public double ScrollPosition { get; set; } = 0;

        private IJSRuntime jsRuntime;

        public IndexViewModel(Navigator navigator, NavigationManager NavManager, UniComponents.Services.IAppLifeCycleService AppLifecycleService, ObjectHolder objectHolder, DBContext db, IJSRuntime jsRuntime)
        {
            navigator.InitializeNavManazer(NavManager); //inicializacia navigatora

            AppLifecycleService.Destroying += OnAppDestroying;

            modulesList = new ObservableCollection<(string, string, List<RolesOwn>, (string, string))>();
            modulesList.Add(("Objednávky", "/Objednavka", DBLayer.Models.Objednavka.POVOLENEROLE, ("orange", "bi-cart4")));
            modulesList.Add(("Skladové hospodárstvo", "/Sklad", DBLayer.Models.Sklad.ROLE_R_SKLAD, ("orange", "bi-box-seam")));
            modulesList.Add(("Používatelia", "/Pouzivatelia", new() { RolesOwn.Admin }, ("black", "bi-person-gear")));
            modulesList.Add(("Správa profesií", "/SpravaRoli", IdentityUserOwn.ROLE_CRUD_ROLI, ("brown", "bi-award")));
            modulesList.Add(("Zamestnanci", "/Zam", IdentityUserOwn.ROLE_R_ZAMESTNANCI, ("brown", "bi-people")));
            modulesList.Add(("Organizácie", "/Dodavatelia", Dodavatel.ROLE_R_DODAVATELIA, ("orange", "bi-shop")));
            modulesList.Add(("Sklady", "/Sklady", Sklad.ROLE_R_SKLAD, ("orange", "bi-boxes")));
            modulesList.Add(("Faktúry", "/Faktury", Faktura.ROLE_R_FAKTURY, ("green", "bi-file-text")));
            modulesList.Add(("Rezervácie", "/Rezervacia", Rezervation.ROLE_R_REZERVACIA, ("DarkTurquoise", "bi-calendar3-range")));
            modulesList.Add(("Hostia", "/Hostia", Host.ROLE_R_HOSTIA, ("DarkTurquoise", "bi-person-arms-up")));
            modulesList.Add(("Pokladňa", "/Kasa", DBLayer.Models.Kasa.ROLE_R_KASA.Concat(DBLayer.Models.PokladnicnyDoklad.ROLE_R_POKLDOKL).ToList(), ("DarkTurquoise", "bi-receipt-cutoff")));
            modulesList.Add(("Položky dokladu", "/UniConItemy", UniConItemPoklDokladu.ROLE_R_POLOZKY, ("DarkTurquoise", "bi-box2")));
            modulesList.Add(("Zmenáreň", "/Zmenaren", ZmenaMeny.ROLE_CR_ZMENAREN, ("DarkTurquoise", "bi-currency-exchange")));
            modulesList.Add(("Housekeeping", "/Housekeeping", Rezervation.ROLE_R_HSK, ("purple", "bi-house")));
            modulesList.Add(("Správa izieb", "/Rooms", new List<RolesOwn>() { RolesOwn.Admin }, ("black", "bi-house-gear")));


            if (!CompVyberKasa.checkWhenAppClose)        //zaistenie, ze uzivatel je odpojeny od kasy
            {
                CompVyberKasa.checkWhenAppClose = true;
                var ensureKasaLogOff = () =>
                {
                    var found = objectHolder.Remove<DBLayer.Models.Kasa>();
                    if (found != null)
                    {
                        found.ActualUser = null;
                        db.SaveChanges();
                    }

                };

                AppLifecycleService.Destroying += ensureKasaLogOff;
                AppLifecycleService.LogOff += ensureKasaLogOff;
            }

            this.jsRuntime = jsRuntime;
        }

        private void OnAppDestroying()
        {
            Debug.WriteLine("******************Applikacia sa zatvára.*******************"); //pridanie app eventu
        }

        public async Task SetScroll(string id)
        {
            await jsRuntime.InvokeVoidAsync("setScrollPosition", id, ScrollPosition); //nastavi scrollbar na poziciu pre odchodom zo stranky
        }

        public async Task GetScroll(string id)
        {
            ScrollPosition = await jsRuntime.InvokeAsync<double>("getScrollPosition", id);
        }
    }
}
