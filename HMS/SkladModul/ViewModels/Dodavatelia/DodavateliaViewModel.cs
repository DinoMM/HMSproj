using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace SkladModul.ViewModels.Dodavatelia
{
    public partial class DodavateliaViewModel : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<Dodavatel> zoznamDodavatelov = new();

        private readonly DBContext _db;
        private readonly UserService _userService;

        public DodavateliaViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(DBLayer.Models.Dodavatel.POVOLENEROLE);
        }
    }
}