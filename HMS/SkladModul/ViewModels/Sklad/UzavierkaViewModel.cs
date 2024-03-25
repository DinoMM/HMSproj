using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkladModul.ViewModels.Sklad
{
    public class UzavierkaViewModel : ObservableObject
    {
        DBContext _db;

        public UzavierkaViewModel(DBContext db)
        {
            _db = db;
        }
    }
}
