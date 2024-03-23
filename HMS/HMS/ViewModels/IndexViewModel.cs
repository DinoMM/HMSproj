
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
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
        private ObservableCollection<(string, string)> modulesList;     //(nazov modulu, hyperlink)

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
            modulesList = new ObservableCollection<(string, string)>();
            modulesList.Add(("Objednávky", "/Objednavka"));
            modulesList.Add(("Sklad", "/Sklad"));

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
