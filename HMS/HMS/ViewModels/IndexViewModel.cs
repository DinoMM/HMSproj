
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
using DBLayer.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using UniComponents;

namespace HMS.ViewModels
{
    public partial class IndexViewModel : ObservableObject
    {
        //[ObservableProperty]
        //private int myProperty;

        [ObservableProperty]
        private ObservableCollection<(string, string, List<RolesOwn>)> modulesList;     //(nazov modulu, hyperlink, list povolenych roli)


        //private readonly DBContext _db;
        private readonly UserService _userService;

        public InfoModal InfoModal { get; set; }

        public IndexViewModel(UserService userService)
        {
            //_db = database;
            _userService = userService;


            if (userService.LoggedUser == null)
            {       //kontrola prihlasenia
                //vyhodit prihlasovacie okno
                Debug.WriteLine("Uzivatel nie je prihlaseny");
            }
            modulesList = new ObservableCollection<(string, string, List<RolesOwn>)>();
            modulesList.Add(("Objednávky", "/Objednavka", DBLayer.Models.Objednavka.POVOLENEROLE));
            modulesList.Add(("Skladové hospodárstvo", "/Sklad", DBLayer.Models.Sklad.ROLE_R_SKLAD));
            modulesList.Add(("Používatelia", "/Pouzivatelia", new() { RolesOwn.Admin }));
            modulesList.Add(("Správa profesií", "/SpravaRoli", IdentityUserOwn.ROLE_CRUD_ROLI));
            modulesList.Add(("Zamestnanci", "/Zam", IdentityUserOwn.ROLE_R_ZAMESTNANCI));
            modulesList.Add(("Organizácie", "/Dodavatelia", IdentityUserOwn.ROLE_R_ZAMESTNANCI));
            modulesList.Add(("Sklady", "/Sklady", Sklad.ROLE_R_SKLAD));


        }

        [RelayCommand]
        private void Navigate(string link)
        {


        }

        //[RelayCommand(CanExecute = nameof(IsSomeMethodExcutable))]
        [RelayCommand]
        private async Task OpenInfoModal()
        {
            await InfoModal.OpenModal();
        }
    }
}
