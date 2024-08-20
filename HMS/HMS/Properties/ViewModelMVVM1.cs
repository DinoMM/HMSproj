using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace $rootnamespace$
{
    public partial class $projectname$ : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<TYPE> zoznamViewModel = new();

        private readonly DBContext _db;
        private readonly UserService _userService;

        public $projectname$(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return list.Contains(_userService.LoggedUserRole);
        }
    }
}