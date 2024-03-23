using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBLayer;
using DBLayer.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniComponents;

using OBJ = DBLayer.Models.Objednavka;

namespace SkladModul.ViewModels.Objednavka
{
    public partial class AddObjednavViewModel : ObservableObject
    {
        [ObservableProperty]
        private string icoDod = default!;

        [ObservableProperty]
        private string nazovDod = default!;

        [ObservableProperty]
        private string icoOdo = default!;

        [ObservableProperty]
        private string nazovOdo = default!;

        [ObservableProperty]
        private string popis = default!;

        [ObservableProperty]
        StavOBJ stav = StavOBJ.Vytvorena;


        //private readonly PridPolozkyViewModel _PDViewModel;
        private readonly ObjectHolder _objectHolder;
        private readonly DBContext _db;
        private readonly UserService _userService;

        public bool CorrectDod { get; set; } = false;
        public bool CorrectOdo { get; set; } = false;

        public OBJ objednavka { get; set; }


        public AddObjednavViewModel(DBContext db, ObjectHolder objectHolder, UserService userService)
        {
            _db = db;
            _userService = userService;
            _objectHolder = objectHolder;
            objednavka = new();
        }

        [RelayCommand]
        private void ZmenaDod(ChangeEventArgs param)
        {
            string vlozeneIco = (string)param.Value!;
            var najDod = _db.Dodavatelia.FirstOrDefault(x => x.ICO == vlozeneIco);
            if (najDod == null)
            {
                CorrectDod = false;
                NazovDod = default!;
                return;
            }
            NazovDod = najDod.Nazov;
            CorrectDod = true;

            objednavka.Dodavatel = najDod.ICO;
            objednavka.DodavatelX = najDod;
        }

        [RelayCommand]
        private void ZmenaOdo(ChangeEventArgs param)
        {
            string vlozeneIco = (string)param.Value!;
            var najDod = _db.Dodavatelia.FirstOrDefault(x => x.ICO == vlozeneIco);
            if (najDod == null)
            {
                CorrectOdo = false;
                NazovOdo = default!;
                return;
            }
            NazovOdo = najDod.Nazov;
            CorrectOdo = true;

            objednavka.Odberatel = najDod.ICO;
            objednavka.OdberatelX = najDod;
        }

        [RelayCommand]
        private void NastavObjednavku()
        {
            if (Locked())
            {
                _objectHolder.Add(objednavka);
                return;
            }

            objednavka.Stav = Stav;
            objednavka.Popis = Popis;
            bool nova = false;
            if (string.IsNullOrEmpty(objednavka.ID))    //ak neni ID tak vygenerujeme
            {
                objednavka.ID = OBJ.DajNoveID(_db);
                nova = true;
            }

            if (string.IsNullOrEmpty(objednavka.Tvorca))
            {
                objednavka.Tvorca = _userService.LoggedUser.Id;
            }

            if (objednavka.TvorcaX == null)
            {
                var pouz = _db.Users.FirstOrDefault(x => x.Id == objednavka.Tvorca);
                if (pouz != null)
                {
                    objednavka.TvorcaX = pouz;
                }
            }

            if (nova)
            {
                _db.Objednavky.Add(objednavka);
            }
            else
            {
                var obj = _db.Objednavky.FirstOrDefault(x => x.ID == objednavka.ID);
                if (obj != null)
                {
                    obj.SetFromObjednavka(objednavka);
                }
            }
            _db.SaveChanges();

            _objectHolder.Add(objednavka);
        }

        public void SetExistObjednavka(OBJ obj)
        {
            objednavka = obj;
            IcoDod = objednavka.Dodavatel;
            NazovDod = objednavka.DodavatelX.Nazov;
            IcoDod = objednavka.Odberatel;
            NazovOdo = objednavka.OdberatelX.Nazov;
            Stav = objednavka.Stav;
            Popis = objednavka.Popis ?? "";
            CorrectDod = true;
            CorrectOdo = true;
        }

        public bool Exist()
        {
            return !string.IsNullOrEmpty(objednavka?.ID);
        }

        public bool Locked()
        {
            switch (objednavka.Stav)
            {
                case StavOBJ.Vytvorena:
                case StavOBJ.Neschvalena:
                    return false;
                case StavOBJ.Ukoncena:
                    return true;
                default:
                    if (_userService.LoggedUserRole == RolesOwn.Admin ||
                        _userService.LoggedUserRole == RolesOwn.Riaditel)
                    {
                        return false;
                    }
                    return true;
            }

        }
        public bool PozriZmeny()
        {
            if (objednavka.Popis != null)
            {
                return _db.Entry(objednavka).State == EntityState.Modified ||
                   !objednavka.Popis.Equals(Popis) ||
                    objednavka.Stav != Stav;
            }
            else
            {
                return _db.Entry(objednavka).State == EntityState.Modified ||
                  !(string.IsNullOrEmpty(objednavka.Popis) && string.IsNullOrEmpty(Popis)) ||
                   objednavka.Stav != Stav;
            }
        }
        [RelayCommand]
        private void Uloz()
        {
            objednavka.Popis = Popis;
            objednavka.Stav = Stav;
            _db.SaveChanges();
        }
        [RelayCommand]
        private void Neuloz()
        {
            if (!string.IsNullOrEmpty(objednavka.ID))
            {
                _db.Entry(objednavka).State = EntityState.Unchanged;
                _db.SaveChanges();
            }
        }

    }
}
