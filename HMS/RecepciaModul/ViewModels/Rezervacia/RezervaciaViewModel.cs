using BuildingTemplates;
using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using UniComponents;

namespace RecepciaModul.ViewModels
{
    public partial class RezervaciaViewModel : ObservableObject, I_ValidationVM, I_RD_TableVM<Rezervation>
    {
        [ObservableProperty]
        ObservableCollection<Rezervation> zoznamRezervacii = new();
        [ObservableProperty]
        ObservableCollection<Room> zoznamIzieb = new();
        public List<Rezervation> ZoznamNespracRezervacii { get; set; } = new();
        public ObservableCollection<Rezervation> ZoznamVsetkychRezervacii { get; set; } = new();
        public List<DateTime> ZoznamDatumovNaZobrazenie { get; private set; } = new();

        #region polia na vyplnenie
        public DateTime DatumOd { get; set; } = DateTime.Today;
        public DateTime DatumDo { get; set; } = DateTime.Today.AddDays(1);
        #endregion

        public bool NacitavaniePoloziek { get; private set; } = true;
        public bool SmerNacitavaniaAll { get; set; } = true;

        #region farby
        public readonly string HEX_GRAY = "#6c757d";
        public readonly string HEX_GREEN = "#28a745";
        public readonly string HEX_YELLOW = "#ffc107";
        public readonly string HEX_DARK_YELLOW = "#b38600";
        public readonly string HEX_RED = "#dc3545";
        public readonly string HEX_BLUE = "#007bff";
        public readonly string HEX_BLACK = "#000000";
        public readonly string HEX_PURPLE = "#ac00e6";
        #endregion

        public List<IdentityUserWebOwn> ZoznamWebGuest { get; private set; } = new();
        public List<Host> ZoznamHosti { get; private set; } = new();
        public List<Room> ZoznamIziebList { get; private set; } = new();

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;
        private readonly Blazored.SessionStorage.ISessionStorageService _sessionStorage;
        private readonly ObjectHolder _objectHolder;

        public RezervaciaViewModel(DBContext db, UserService userService, DataContext dbw, Blazored.SessionStorage.ISessionStorageService sessionStorage, ObjectHolder objectHolder)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;

            ZoznamDatumovNaZobrazenie = GetRozmedzieDatumov(-3, DateTime.Today, 10);     //zakladne rozmedzie datumov na zobrazenie
            _sessionStorage = sessionStorage;
            _objectHolder = objectHolder;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Rezervation.ROLE_R_REZERVACIA);
        }
        public bool ValidateUserCRU()
        {
            throw new NotImplementedException();
        }

        public async Task NacitajZoznamy()
        {
            NacitavaniePoloziek = true;
            await NacitajZoznamRezervacie();    //naèítanie rezervácií
            await NacitajIzbyRezervacie();

            NacitavaniePoloziek = false;
        }

        private async Task NacitajZoznamRezervacie()
        {
            ZoznamRezervacii = new(await _dbw.Rezervations
                    .Include(x => x.Guest)
                    .Include(x => x.Room)
                    .Include(x => x.Coupon)
                    .Where(x => x.DepartureDate >= ZoznamDatumovNaZobrazenie[0]
                    && x.ArrivalDate <= ZoznamDatumovNaZobrazenie.Last()
                    && x.Status != ReservationStatus.Stornovana.ToString())
                    .OrderBy(x => x.ArrivalDate)
                    .ToListAsync()
                    );  //naèítané rezervacie v rozmedzi datumov
            foreach (var item in ZoznamRezervacii)
            {
                item.RecentChangesUserZ = item.RecentChangesUserZ ?? Rezervation.GetRecentChangedUser(item, in _db); //nacita recent userov
            }
        }
        public async Task NacitajZoznamRezervacieAll(bool refresh = false)
        {
            if (refresh)
            {
                ZoznamVsetkychRezervacii.Clear();
            }
            if (ZoznamVsetkychRezervacii.Count == 0)
            {
                if (SmerNacitavaniaAll)
                {
                    var yesterday = DateTime.Today.AddDays(-1);
                    (await _dbw.Rezervations
                            .Include(x => x.Guest)
                            .Include(x => x.Room)
                            .Include(x => x.Coupon)
                            .Where(x => x.ArrivalDate >= yesterday)
                            .OrderBy(x => x.ArrivalDate)
                            .ToListAsync()
                            )
                            .ForEach(x => ZoznamVsetkychRezervacii.Add(x));  //naèítané rezervacie v rozmedzi datumov
                }
                else
                {
                    (await _dbw.Rezervations
                            .Include(x => x.Guest)
                            .Include(x => x.Room)
                            .Include(x => x.Coupon)
                            .Where(x => x.ArrivalDate <= DateTime.Today)
                            .OrderByDescending(x => x.DepartureDate)
                            .ToListAsync()
                            )
                            .ForEach(x => ZoznamVsetkychRezervacii.Add(x));  //naèítané rezervacie v rozmedzi datumov
                }
                foreach (var item in ZoznamVsetkychRezervacii) //nacita recent userov
                {
                    item.RecentChangesUserZ = item.RecentChangesUserZ ?? Rezervation.GetRecentChangedUser(item, in _db); 
                }
                //ZoznamVsetkychRezervacii.ForEach(x => x.RecentChangesUserZ = x.RecentChangesUserZ ?? Rezervation.GetRecentChangedUser(x, in _db)); //nacita recent userov
            }

        }
        private async Task NacitajIzbyRezervacie()
        {
            ZoznamIzieb = new(await _dbw.HRooms.ToListAsync());  //naèítanie všetkých izieb
            ZoznamIziebList = new(ZoznamIzieb.ToList());
        }

        public bool MoznoVymazat(Rezervation item)
        {
            return false;
        }

        public void Vymazat(Rezervation item)
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
            int fin = 0;    // 0 - ziaden datum, 1 len odchod, 2 len prichod, 3 oba najdene, 4 cez cely rozsah
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
                    if (res.Status != ReservationStatus.Blokovana.ToString())
                    {
                        return (start + "/" + end, fin);
                    }
                    else
                    {
                        return (GetPoziciaResBlokovana(start, end), fin);
                    }
                }
            }

            if (fin == 2)   //ak sa chytil den prichodu len tak den odchodu je dalej ako rozsah
            {
                if (res.Status != ReservationStatus.Blokovana.ToString())
                {
                    return ((start + "/" + (end + 1)), fin);
                }
                else
                {
                    return (GetPoziciaResBlokovana(start, end), fin);
                }
            }
            if (fin == 1)
            {
                if (res.Status != ReservationStatus.Blokovana.ToString())
                {
                    return (start + "/" + end, fin);
                }
                else
                {
                    return (GetPoziciaResBlokovana(start + 1, end), fin);
                }
            }
            if (res.ArrivalDate < ZoznamDatumovNaZobrazenie[0] && ZoznamDatumovNaZobrazenie.Last() < res.DepartureDate)
            {
                return (start + "/" + end, 4);      //ak je rezvacia cez cely rozsah
            }
            return ("0/0", fin);
        }

        private string GetPoziciaResBlokovana(int start, int end)
        {
            start = (start - 1) == 0 ? 1 : (start - 1);
            end = (end + 1) == (ZoznamDatumovNaZobrazenie.Count * 2 + 2) ? ZoznamDatumovNaZobrazenie.Count * 2 + 1 : (end + 1);
            return (start + "/" + end);
        }

        public string GetHexFarbuRes(Rezervation res)
        {
            if (res.Status == ReservationStatus.Blokovana.ToString())
            {
                return HEX_GRAY;
            }
            if (res.Status == ReservationStatus.VytvorenaWeb.ToString() || res.Status == ReservationStatus.VytvorenaRucne.ToString())
            {
                return HEX_PURPLE;
            }
            if (res.Status == ReservationStatus.Checked_OUT.ToString())
            {
                return HEX_RED;
            }
            if (res.DepartureDate == DateTime.Today)
            {
                return HEX_DARK_YELLOW;
            }
            if (res.Status == ReservationStatus.Checked_IN.ToString())
            {
                return HEX_GREEN;
            }
            if (res.ArrivalDate == DateTime.Today)
            {
                return HEX_YELLOW;
            }


            if (DateTime.Today > res.ArrivalDate)
            {
                return HEX_BLUE;
            }
            if (DateTime.Today < res.DepartureDate)
            {
                return HEX_BLUE;
            }
            return HEX_BLUE;
        }

        public string ResName(Rezervation res)
        {
            if (res.Status == ReservationStatus.Blokovana.ToString())
            {
                return "Blokácia";
            }
            if (res.Guest == null)
            {
                return "Rezervácia";
            }
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

        public async Task SpracujZmeny()
        {
            if (await _sessionStorage.GetItemAsync<bool>("RezervationChanged"))
            {
                await _sessionStorage.SetItemAsync("RezervationChanged", false);
                await NacitajZoznamRezervacie();
                ZoznamNespracRezervacii.Clear();
                ZoznamVsetkychRezervacii.Clear();
            }
        }

        public async Task NacitajPotrebneZoznamy()
        {
            NacitavaniePoloziek = true;
            if (ZoznamWebGuest.Count == 0)
            {
                ZoznamWebGuest.AddRange(await _dbw.Users.OrderBy(x => x.Email).ToListAsync());
            }
            if (ZoznamHosti.Count == 0)
            {
                ZoznamHosti.AddRange(await _db.Hostia.OrderBy(x => x.Surname).ToListAsync());
                await Task.Run(() => {
                    foreach (var item in ZoznamHosti)   //nacita k hostom ich web ucty ak maju
                    {
                        if (!string.IsNullOrEmpty(item.Guest)) {
                            item.GuestZ = ZoznamWebGuest.FirstOrDefault(x => x.Id == item.Guest);
                        }
                        //nenacitava pokladnicne bloky dalej
                    }
                });
            }

            NacitavaniePoloziek = false;
        }

        public void ClearChangesDB()
        {
            _db.ClearPendingChanges();
            _dbw.ClearPendingChanges();
        }

        public async Task NacitajNespracovaneRezervacie(bool refresh)
        {
            if (refresh)
            {
                ZoznamNespracRezervacii.Clear();
            }
            if (ZoznamNespracRezervacii.Count == 0)
            {
                ZoznamNespracRezervacii.AddRange(await _dbw.Rezervations
                    .Include(x => x.Guest)
                    .Include(x => x.Room)
                    .Include(x => x.Coupon)
                    .Where(x => x.Status == ReservationStatus.VytvorenaWeb.ToString() || x.Status == ReservationStatus.VytvorenaRucne.ToString())
                    .OrderBy(x => x.ArrivalDate)
                    .ToListAsync());

                ZoznamNespracRezervacii.ForEach(x => x.RecentChangesUserZ = x.RecentChangesUserZ ?? Rezervation.GetRecentChangedUser(x, in _db)); //nacita recent userov
            }
        }

        public async Task ZmenSmerNacitavania()
        {
            SmerNacitavaniaAll = !SmerNacitavaniaAll;
            await NacitajZoznamRezervacieAll(true);
        }

        //    public async ValueTask<ItemsProviderResult<Rezervation>> LoadReservations(
        //ItemsProviderRequest request)
        //    {
        //        var cntTotal = _dbw.Rezervations.Count();
        //        // Calculate the number of reservations to fetch
        //        var numRezervations = Math.Min(request.Count, cntTotal - request.StartIndex);

        //        // Fetch the reservations from the database
        //        var yesterday = DateTime.Today.AddDays(-1);
        //        var rezervations = await _dbw.Rezervations
        //            .Where(x => x.ArrivalDate >= yesterday)
        //            .OrderBy(r => r.ArrivalDate) // Adjust the ordering as needed
        //            .Skip(request.StartIndex)
        //            .Take(numRezervations)
        //            .ToListAsync(request.CancellationToken);


        //        // Return the result
        //        return new ItemsProviderResult<Rezervation>(rezervations, cntTotal);
        //    }
    }
}