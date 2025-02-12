using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using DBLayer.Models.HSKModels;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using PdfCreator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using UniComponents;

namespace HSKModul.ViewModels
{
    public class HousekeepingViewModel : AObservableViewModel<Rezervation>
    {
        public List<Room> VsetkyIzby { get; private set; } = new();
        public List<HostConReservation> RelevantHostia { get; private set; } = new();
        public DateTime AktDen { get; set; } = DateTime.Now;
        public bool NacitavaniePDF { get; set; } = false;
        public bool Zobrazenie { get; set; } = false;   //false aktuality, true izby

        private List<Rezervation> ZobrazenieHideList = new();

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

        protected override async Task NacitajZoznamyAsync()
        {
            await Task.Run(async () =>
            {
                ZoznamPoloziek = new(_dbw.Rezervations
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
                foreach (var item in ZoznamPoloziek)
                {
                    RelevantHostia.AddRange(_db.HostConReservations
                    .Include(y => y.HostX)
                    .Where(y => y.Reservation == item.Id)
                    .ToList());
                }

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

                foreach (var item in VsetkyIzby)    //pre zobrazenie vsetkych izieb
                {
                    var founds = ZoznamPoloziek.Where(x => x.RoomNumber == item.RoomNumber);
                    if (founds.Count() > 0)
                    {
                        ZobrazenieHideList.AddRange(founds);
                    }
                    else
                    {
                        ZobrazenieHideList.Add(new Rezervation() { ArrivalDate = new DateTime(), DepartureDate = new DateTime(), RoomNumber = item.RoomNumber, Room = item });    //maketa
                    }
                }
            });
        }

        public async Task NacitajPDF()
        {
            NacitavaniePDF = true;
            await Task.Run(() =>
            {
                List<Rezervation> list = ComplexTable.Markers.Count > 0 ?
                                        ComplexTable.Markers
                                        .OrderBy(x => x.RoomNumber)
                                        .ToList() :
                                        ComplexTable.ActualList
                                        .OrderBy(x => x.RoomNumber)
                                        .ToList();       //bud vyklikany alebo zobrazeny zoznam
                var creator = new HskZobrazeniePDF();
                creator.GenerujPdf(list, ComplexTable.GetWriteableOnlySettings());
                creator.OpenPDF();
            });
            NacitavaniePDF = false;
        }

        public async Task ZmenaZobrazenie()
        {
            Zobrazenie = !Zobrazenie;
            await Nacitaj(methodAsync: async () =>
                await SilentCollection(methodAsync: async () =>
                {
                    await Task.Run(() =>
                    {
                        List<Rezervation> tmp = new();
                        tmp.AddRange(ZoznamPoloziek);
                        ZoznamPoloziek = new(ZobrazenieHideList);
                        ZobrazenieHideList = tmp;
                    });
                })
            ); 
            await ManualNotifyColection();
        }

    }
}
