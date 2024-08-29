using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace $rootnamespace$
{
    public partial class $itemname$ : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<###> zoznamViewModel = new();

        public bool NacitavaniePoloziek { get; private set; } = true;

private readonly DBContext _db;
        private readonly UserService _userService;

        public $itemname$(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(###);
        }

public async Task NacitajZoznamy()
{
###
    NacitavaniePoloziek = false;
}

public bool MoznoVymazat(### item)
{
    
}

public void Vymazat(### item)
{
    

}
    }
}