using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
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
        public bool NoDbChange { get; set; } = false;
        public bool NacitavanieZoznamu { get; private set; } = false;

        private readonly DBContext _db;
        private readonly DataContext _dbw;
        private readonly UserService _userService;

        public CRUHostViewModel(DBContext db, UserService userService, DataContext dbw)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Host.ROLE_R_HOSTIA);
        }
        public bool ValidateUserCRUD()
        {
            return _userService.IsLoggedUserInRoles(Host.ROLE_CRUD_HOSTIA);
        }

        public void SetProp(Host host)
        {
            Host = host;
            HostInput = host.Clon();    //bez instancie GuestZ
            HostInput.GuestZ = host.GuestZ;
            existuje = true;

        }

        public void ClearProp() {
            Host = new();
            HostInput = new();
            existuje = false;
        }

        public async Task<bool> Uloz()
        {
            if (!Existuje())
            {
                _db.Hostia.Add(HostInput);
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
            //if (!NoDbChange)
            //{
            await _db.SaveChangesAsync();

            //}
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

    }
}