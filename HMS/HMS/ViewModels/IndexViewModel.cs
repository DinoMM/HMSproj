
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
using DBLayer.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.Diagnostics;
using UniComponents;

namespace HMS.ViewModels
{
    public partial class IndexViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<(string, string, List<RolesOwn>)> modulesList;     //(nazov modulu, hyperlink, list povolenych roli)

        public InfoModal CloseAppModal = new();
        

        public IndexViewModel(Navigator navigator, NavigationManager NavManager, HMS.Components.Services.IAppLifeCycleService AppLifecycleService)
        {
            navigator.InitializeNavManazer(NavManager); //inicializacia navigatora

            AppLifecycleService.Destroying += OnAppDestroying;

            modulesList = new ObservableCollection<(string, string, List<RolesOwn>)>();
            modulesList.Add(("Objednávky", "/Objednavka", DBLayer.Models.Objednavka.POVOLENEROLE));
            modulesList.Add(("Skladové hospodárstvo", "/Sklad", DBLayer.Models.Sklad.ROLE_R_SKLAD));
            modulesList.Add(("Používatelia", "/Pouzivatelia", new() { RolesOwn.Admin }));
            modulesList.Add(("Správa profesií", "/SpravaRoli", IdentityUserOwn.ROLE_CRUD_ROLI));
            modulesList.Add(("Zamestnanci", "/Zam", IdentityUserOwn.ROLE_R_ZAMESTNANCI));
            modulesList.Add(("Organizácie", "/Dodavatelia", IdentityUserOwn.ROLE_R_ZAMESTNANCI));
            modulesList.Add(("Sklady", "/Sklady", Sklad.ROLE_R_SKLAD));
            modulesList.Add(("Faktúry", "/Faktury", Faktura.ROLE_R_FAKTURY));
            modulesList.Add(("Rezervácie", "/Rezervacia", Rezervation.ROLE_R_REZERVACIA));

        }

        private void OnAppDestroying()
        {
            Debug.WriteLine("******************Applikacia sa zatvára.*******************"); //pridanie app eventu
        }
    }
}
