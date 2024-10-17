using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;

namespace RecepciaModul.ViewModels
{
    public partial class CRUHostViewModel : ObservableObject
    {

        #region polia na vyplnenie
        public Host HostInput { get; set; } = new();
        #endregion

        public Host Host { get; set; } = new();
        private bool existuje = false;
        private bool BolVarovany = false;
        public bool NacitavanieZoznamu { get; private set; } = false;
        public bool NacitavaniePoloziek { get; set; } = true;       //pre zistenie ze sme nacitali Host

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;
        private readonly IJSRuntime _JSRuntime;

        public CRUHostViewModel(DBContext db, UserService userService, DataContext dbw, IJSRuntime jSRuntime)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;
            _JSRuntime = jSRuntime;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Host.ROLE_R_HOSTIA);
        }
        public bool ValidateUserCRU()
        {
            return _userService.IsLoggedUserInRoles(Host.ROLE_CRU_HOSTIA);
        }

        public void SetProp(Host host)
        {
            Host = host;
            HostInput = host.Clon();    //bez instancie GuestZ
            HostInput.GuestZ = host.GuestZ;
            existuje = true;

        }

        public void ClearProp()
        {
            Host = new();
            HostInput = new();
            existuje = false;
        }

        public async Task<bool> Uloz()
        {
            if (!Existuje())
            {
                _db.Hostia.Add(HostInput);
                Host = HostInput;
                HostInput = Host.Clon();
            }
            else
            {
                //Host.ID = HostInput.ID;
                Host.Name = HostInput.Name;
                Host.Surname = HostInput.Surname;
                Host.Address = HostInput.Address;
                Host.BirthNumber = HostInput.BirthNumber;
                Host.Passport = HostInput.Passport;
                Host.CitizenID = HostInput.CitizenID;
                Host.BirthDate = HostInput.BirthDate;
                Host.Guest = HostInput.Guest;
                Host.GuestZ = HostInput.GuestZ;
            }
            await _db.SaveChangesAsync();
            existuje = true;
            return true;
        }

        public bool Existuje()
        {
            return existuje;
        }

        public async Task<List<IdentityUserWebOwn>> GetListWebUsers()
        {
            NacitavanieZoznamu = true;
            var list = await _dbw.Users.ToListAsync();
            NacitavanieZoznamu = false;
            return list;
        }

        public void SetNewWebUser(IdentityUserWebOwn user)
        {
            if (!string.IsNullOrEmpty(user.UserName))
            {
                HostInput.Guest = user.Id;
                HostInput.GuestZ = user;
            }
            else
            {
                HostInput.Guest = null;
                HostInput.GuestZ = null;
            }
        }

        /// <summary>
        /// Upozoni 1x uzivatela o urcitom varovani, ktoremu ma venovat pozornost
        /// </summary>
        /// <param name="button">Id tlacidla, ktore zmeni farbu po najdeni upozonenia</param>
        /// <param name="ids">Vsetky idcka elementov: aktualne 3</param>
        /// <returns></returns>
        public async Task<bool> PozriVarovania(string button, List<string> ids)
        {
            if (BolVarovany)
            {
                BolVarovany = false;
                return false;
            }

            bool vysledok = false;
            #region info1

            if (DateTime.Today <= HostInput.BirthDate.AddYears(18))   //je dospeli?
            {
                if (DateTime.Today <= HostInput.BirthDate.AddYears(12)) //dieta?
                {
                    if (DateTime.Today <= HostInput.BirthDate.AddYears(5)) //moc mlade
                    {
                        if (DateTime.Today <= HostInput.BirthDate.AddYears(2)) //batola
                        {
                            await _JSRuntime.InvokeVoidAsync("addInfoPopOverUntilClick", ids[0], "Do 2 rokov.", "right");
                        }
                        else
                        {
                            await _JSRuntime.InvokeVoidAsync("addInfoPopOverUntilClick", ids[0], "Do 5 rokov.", "right");
                        }
                    }
                    else
                    {
                        await _JSRuntime.InvokeVoidAsync("addInfoPopOverUntilClick", ids[0], "Do 12 rokov.", "right");
                    }
                }
                else
                {
                    await _JSRuntime.InvokeVoidAsync("addInfoPopOverUntilClick", ids[0], "Nemá 18 rokov.", "right");
                }
                //vysledok = true;
            }
            #endregion
            #region warning2
            if (!string.IsNullOrEmpty(HostInput.CitizenID) && _db.Hostia.Any(x => x.CitizenID == HostInput.CitizenID && x.ID != HostInput.ID))
            {
                await _JSRuntime.InvokeVoidAsync("addWarningPopOverUntilClick", ids[1], "Èíslo obèianského už existuje v systéme", "left");
                vysledok = true;
            }
            #endregion
            #region warning3
            if (!string.IsNullOrEmpty(HostInput.Passport) && _db.Hostia.Any(x => x.Passport == HostInput.Passport && x.ID != HostInput.ID))
            {
                await _JSRuntime.InvokeVoidAsync("addWarningPopOverUntilClick", ids[2], "Èíslo pasu už existuje v systéme", "left");
                vysledok = true;
            }
            #endregion
            if (vysledok)
            {
                await _JSRuntime.InvokeVoidAsync("addStyleClassUntilClick", button, "btn-warning");
            }
            BolVarovany = vysledok;
            return vysledok;
        }

    }
}