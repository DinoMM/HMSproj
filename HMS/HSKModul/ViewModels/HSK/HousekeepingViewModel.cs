using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using DBLayer.Models.HSKModels;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using UniComponents;

namespace HSKModul.ViewModels
{
    public partial class HousekeepingViewModel : ObservableObject
    {
        public List<Rezervation> RelevantRezervacie { get; private set; } = new();
        public List<Room> VsetkyIzby { get; private set; } = new();
        public List<HostConReservation> RelevantHostia { get; private set; } = new();
        public DateTime AktDen { get; set; } = DateTime.Now;

        public bool NacitavaniePoloziek { get; private set; } = true;

        private readonly DBContext _db;
        private readonly UserService _userService;
        private readonly DataContext _dbw;
        public HousekeepingViewModel(DBContext db, UserService userService, DataContext dbw)
        {
            _db = db;
            _userService = userService;
            _dbw = dbw;
        }

        public bool ValidateUser()
        {
            return _userService.IsLoggedUserInRoles(Rezervation.ROLE_R_HSK);
        }
        
        public async Task NacitajZoznamy()
        {
            NacitavaniePoloziek = true;

            RelevantRezervacie.Clear();
            RelevantRezervacie.AddRange(_dbw.Rezervations
                .Include(x => x.Guest)
                .Include(x => x.Room)
                .Include(x => x.Coupon)
                .Where(x =>
                    (x.DepartureDate.Date == AktDen.Date
                    || x.ArrivalDate.Date == AktDen.Date
                    || x.Status == ReservationStatus.Checked_IN.ToString())
                    && x.Status != ReservationStatus.Stornovana.ToString()
                    )
                    .OrderBy(x => x.RoomNumber)
                    .ToList()
                );

            RelevantHostia.Clear();
            RelevantRezervacie.ForEach(x =>
            RelevantHostia.AddRange(_db.HostConReservations
            .Include(y => y.HostX)
            .Where(y => y.Reservation == x.Id)
            .ToList())
            );

            VsetkyIzby.Clear();
            VsetkyIzby.AddRange(_dbw.HRooms
                .ToList());

            bool needsave = false;
            VsetkyIzby.ForEach(x =>
            {
                var found = _db.RoomInfos.FirstOrDefault(y => y.ID_Room == x.RoomNumber);
                if (found != null)  //ak existuje tak pridame 99% sanca
                {
                    x.RoomInfo = found;
                }
                else // ak neexistuje tak vytvorime, len pri nenaplnenej databaze izbami
                {
                    var newRoomInfo = new RoomInfo() { ID_Room = x.RoomNumber };
                    _db.RoomInfos.Add(newRoomInfo);
                    x.RoomInfo = newRoomInfo;
                    needsave = true;
                }
            });
            if (needsave)
            {
                await _db.SaveChangesAsync();
            }
            NacitavaniePoloziek = false;
        }
        
    }
}
