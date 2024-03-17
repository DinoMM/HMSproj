using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBLayer;
using Polozka = DBLayer.Models.PolozkaSkladu;
using CommunityToolkit.Mvvm.Input;

namespace SkladModul.ViewModels.Sklad
{
    public partial class SkladViewModel : ObservableObject
    {
        public string Obdobie {  get; set; }
        public string Sklad {  get; set; }

        DBContext _db;
        UserService _userService;

        public SkladViewModel(DBContext db, UserService userService) {
            _db = db;
            _userService = userService;
        }

        public List<string> GetObdobia() {
            var list = new List<string>() { "0001", "0002" };
            Obdobie = "0001";
            return list;
        }
        public List<string> GetSklady()
        {
            var list = new List<string>() { "SKLU", "AWER" };
            Sklad = "SKLU";
            return list;
        }
    }
}
