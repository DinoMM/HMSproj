using BuildingTemplates;
using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace RecepciaModul.ViewModels
{
    public partial class RezervaciaViewModel : ObservableObject, I_ValidationVM, I_RD_TableVM<Rezervacia>
    {
        [ObservableProperty]
        ObservableCollection<Rezervation> zoznamRezervacii = new();
        [ObservableProperty]
        ObservableCollection<Room> zoznamIzieb = new();
        public List<DateTime> ZoznamDatumovNaZobrazenie { get; private set; } = new();

        #region polia na vyplnenie
        public DateTime DatumOd { get; set; } = DateTime.Today;
        public DateTime DatumDo { get; set; } = DateTime.Today.AddDays(1);
        #endregion

        public bool NacitavaniePoloziek { get; private set; } = true;

        #region farby
        public readonly string HEX_GRAY = "#6c757d";
        public readonly string HEX_GREEN = "#28a745";
        public readonly string HEX_YELLOW = "#ffc107";
        public readonly string HEX_RED = "#dc3545";
        public readonly string HEX_BLUE = "#007bff";
        public readonly string HEX_BLACK = "#000000";
        #endregion

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;

        public RezervaciaViewModel(DBContext db, UserService userService, DataContext dbw)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;

            ZoznamDatumovNaZobrazenie = GetRozmedzieDatumov(-3, DateTime.Today, 10);     //zakladne rozmedzie datumov na zobrazenie
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Rezervation.ROLE_R_REZERVACIA);
        }
        public bool ValidateUserCRUD()
        {
            throw new NotImplementedException();
        }

        public async Task NacitajZoznamy()
        {
            NacitavaniePoloziek = true;
            ZoznamRezervacii = new(await _dbw.Rezervations
                .Include(x => x.Guest)
                .Include(x => x.Room)
                .Where(x => x.DepartureDate >= ZoznamDatumovNaZobrazenie[0]
                && x.ArrivalDate <= ZoznamDatumovNaZobrazenie.Last())
                .OrderBy(x => x.ArrivalDate)
                .ToListAsync()
                );  //naèítané rezervacie v rozmedzi datumov
            ZoznamIzieb = new(await _dbw.HRooms.ToListAsync());  //naèítanie všetkých izieb

            NacitavaniePoloziek = false;
        }

        public bool MoznoVymazat(Rezervacia item)
        {
            return false;
        }

        public void Vymazat(Rezervacia item)
        {


        }

        private List<DateTime> GetRozmedzieDatumov(int predDate, DateTime date, int poDate)
        {
            List<DateTime> dates = new List<DateTime>();

            for (int i = predDate; i <= poDate; i++)
            {
                dates.Add(date.AddDays(i));
            }

            return dates;
        }
        private List<DateTime> GetRozmedzieDatumov(DateTime dateOd, DateTime dateDo)
        {
            List<DateTime> dates = new List<DateTime>();

            while (dateOd <= dateDo)
            {
                dates.Add(dateOd);
                dateOd = dateOd.AddDays(1);
            }

            return dates;
        }

        public (string, int) GetPoziciaRes(Rezervation res)
        {
            int start = 1;
            int end = ZoznamDatumovNaZobrazenie.Count * 2 + 1;
            int fin = 0;    // 0 - ziaden datum, 1 len odchod, 2 len prichod, 3 oba najdene
            for (int i = 1; i <= ZoznamDatumovNaZobrazenie.Count; i++)
            {
                if (res.ArrivalDate == ZoznamDatumovNaZobrazenie[i - 1])
                {
                    start = (i * 2);
                    fin += 2;
                }
                if (res.DepartureDate == ZoznamDatumovNaZobrazenie[i - 1])
                {
                    end = (i * 2);
                    fin += 1;
                }
                if (fin == 3)
                {
                    return (start + "/" + end, fin);
                }
            }
            if (fin == 2)   //ak sa chytil den prichodu len tak den odchodu je dalej ako rozsah
            {
                return ((start + "/" + (end + 1)), fin);
            }
            if (fin == 1)
            {
                return (start + "/" + end, fin);
            }
            if (res.ArrivalDate < ZoznamDatumovNaZobrazenie[0] && ZoznamDatumovNaZobrazenie.Last() < res.DepartureDate)
            {
                return (start + "/" + end, 4);      //ak je rezvacia cez cely rozsah
            }
            return ("0/0", fin);
        }

        public string GetHexFarbuRes(Rezervation res)
        {
            if (res.DepartureDate <= DateTime.Today)
            {
                return HEX_RED;
            }
            if (res.ArrivalDate <= DateTime.Today && DateTime.Today <= res.DepartureDate)
            {
                return HEX_GREEN;
            }
            if (res.ArrivalDate == DateTime.Today.AddDays(1))
            {
                return HEX_YELLOW;
            }
            if (DateTime.Today > res.ArrivalDate)
            {
                return HEX_BLUE;
            }
            return HEX_BLUE;
        }

        public string ResName(Rezervation res)
        {
            string nospaceSurname = res.Guest.Surname.Replace(" ", string.Empty);
            nospaceSurname = char.ToUpper(nospaceSurname[0]) + nospaceSurname.Substring(1);
            return char.ToUpper(res.Guest.Name[0]) + "." + nospaceSurname;
        }

        public async Task ChangeDate()
        {
            var pocDni = (DatumDo - DatumOd).Days;
            if (DatumOd >= DatumDo || pocDni > 121)
            {
                return;
            }
            ZoznamDatumovNaZobrazenie = GetRozmedzieDatumov(DatumOd, DatumDo);
            await NacitajZoznamy();
        }

    }
}