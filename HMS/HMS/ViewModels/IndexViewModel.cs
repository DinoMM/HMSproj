
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
using DBLayer.Models;
using Microsoft.AspNetCore.Components;
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
        private ObservableCollection<(string, string, List<RolesOwn>)> modulesList;     //(nazov modulu, hyperlink, list povolenych roli)

        public InfoModal CloseAppModal = new();


        public IndexViewModel(Navigator navigator, NavigationManager NavManager, UniComponents.Services.IAppLifeCycleService AppLifecycleService, ObjectHolder objectHolder, DBContext db)
        {
            navigator.InitializeNavManazer(NavManager); //inicializacia navigatora

            AppLifecycleService.Destroying += OnAppDestroying;

            modulesList = new ObservableCollection<(string, string, List<RolesOwn>)>();
            modulesList.Add(("Objednávky", "/Objednavka", DBLayer.Models.Objednavka.POVOLENEROLE));
            modulesList.Add(("Skladové hospodárstvo", "/Sklad", DBLayer.Models.Sklad.ROLE_R_SKLAD));
            modulesList.Add(("Používatelia", "/Pouzivatelia", new() { RolesOwn.Admin }));
            modulesList.Add(("Správa profesií", "/SpravaRoli", IdentityUserOwn.ROLE_CRUD_ROLI));
            modulesList.Add(("Zamestnanci", "/Zam", IdentityUserOwn.ROLE_R_ZAMESTNANCI));
            modulesList.Add(("Organizácie", "/Dodavatelia", Dodavatel.ROLE_R_DODAVATELIA));
            modulesList.Add(("Sklady", "/Sklady", Sklad.ROLE_R_SKLAD));
            modulesList.Add(("Faktúry", "/Faktury", Faktura.ROLE_R_FAKTURY));
            modulesList.Add(("Rezervácie", "/Rezervacia", Rezervation.ROLE_R_REZERVACIA));
            modulesList.Add(("Hostia", "/Hostia", Host.ROLE_R_HOSTIA));
            modulesList.Add(("Pokladňa", "/Kasa", DBLayer.Models.Kasa.ROLE_R_KASA.Concat(DBLayer.Models.PokladnicnyDoklad.ROLE_R_POKLDOKL).ToList()));
            modulesList.Add(("Položky dokladu", "/UniConItemy", UniConItemPoklDokladu.ROLE_R_POLOZKY));
            modulesList.Add(("Zmenáreň", "/Zmenaren", ZmenaMeny.ROLE_CR_ZMENAREN));


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
        }

        private void OnAppDestroying()
        {
            Debug.WriteLine("******************Applikacia sa zatvára.*******************"); //pridanie app eventu
        }
    }
}
