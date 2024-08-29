using DBLayer.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer
{
    public class DbInitializeService
    {
        readonly UserManager<IdentityUserOwn> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;
        readonly DBContext _db;

        private bool _done = false;

        public DbInitializeService(UserManager<IdentityUserOwn> userManager, RoleManager<IdentityRole> roleManager, DBContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        public async Task TryMustHaveValues()
        {
            if (!_done)
            {
                #region pridavanie roli
                foreach (var roleName in Enum.GetValues(typeof(RolesOwn)))  //ini aby boli pristupne vsetky role
                {
                    var roleStr = ((RolesOwn)roleName).ToString();
                    var roleExist = await _roleManager.RoleExistsAsync(roleStr);
                    if (!roleExist)
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleStr));
                    }
                }
                #endregion
                #region basic uzivatelia
                var res = await _userManager.FindByNameAsync("admin");  //pridanie admina
                if (res == null)
                {
                    var admin = new IdentityUserOwn() { UserName = "admin", Email = "admin@admin.com", Name = "Admin", Surname = "Admin" };
                    await _userManager.CreateAsync(admin, "Heslo123");
                    await _userManager.AddToRoleAsync(admin, RolesOwn.Admin.ToString());
                }
                #endregion
                #region pridanie dodavatelov
                var dod = _db.Dodavatelia.FirstOrDefault(x => x.ICO == "123456"); //pridanie dodavatelov
                if (dod == null)
                {
                    var prevadzka = new Dodavatel() { ICO = "123456", Nazov = "HotelX", Obec = "Piešťany", Adresa = "Hurbanova 37, 921 01", Iban = "SK1247885698" };
                    _db.Dodavatelia.Add(prevadzka);
                }
                dod = _db.Dodavatelia.FirstOrDefault(x => x.ICO == "111222");
                if (dod == null)
                {
                    var prevadzka = new Dodavatel() { ICO = "111222", Nazov = "FastStore s.r.o.", Obec = "Trnava", Adresa = "Radlínska 453, 917 01", Iban = "SK347744698" };
                    _db.Dodavatelia.Add(prevadzka);
                }
                dod = _db.Dodavatelia.FirstOrDefault(x => x.ICO == "333444");
                if (dod == null)
                {
                    var prevadzka = new Dodavatel() { ICO = "333444", Nazov = "UniTrans s.r.o.", Obec = "Bratislava", Adresa = "Sedmáková 845, 811 03", Iban = "SK567777748" };
                    _db.Dodavatelia.Add(prevadzka);
                }
                #endregion
                
                #region pridanie poloziek skladu
                PolozkaSkladu? pol = _db.PolozkySkladu.FirstOrDefault(x => x.Nazov == "Toaletný papier");
                if (pol == null)
                {
                    pol = new PolozkaSkladu() { ID = PolozkaSkladu.DajNoveID(_db), Cena = 0.79, MernaJednotka = "KS", Nazov = "Toaletný papier" };
                    pol = _db.PolozkySkladu.Add(pol).Entity;
                }
                #endregion
                #region pridanie skladov
                var skl = _db.Sklady.FirstOrDefault(x => x.ID == "HKS");
                if (skl == null)
                {
                    var polozka = new Sklad() { ID = "HKS", Nazov = "House keeping sklad" };
                    _db.Sklady.Add(polozka);
                }
                skl = _db.Sklady.FirstOrDefault(x => x.ID == "KS");
                if (skl == null)
                {
                    var polozka = new Sklad() { ID = "KS", Nazov = "Kitchen sklad" };
                    _db.Sklady.Add(polozka);
                }
                #endregion
                #region priradenie skladu uzivatelovi admin
                var admn = _db.Users.FirstOrDefault(x => x.UserName == "admin");
                var skuz = _db.SkladUzivatelia.FirstOrDefault(x => x.Sklad == "HKS" && x.Uzivatel == admn.Id);
                if (skuz == null)
                {
                    var polozka = new SkladUzivatel() { Sklad = "HKS", Uzivatel = admn.Id };
                    _db.SkladUzivatelia.Add(polozka);
                }
                skuz = _db.SkladUzivatelia.FirstOrDefault(x => x.Sklad == "KS" && x.Uzivatel == admn.Id);
                if (skuz == null)
                {
                    var polozka = new SkladUzivatel() { Sklad = "KS", Uzivatel = admn.Id };
                    _db.SkladUzivatelia.Add(polozka);
                }
                #endregion
                #region pridanie polozky skladu mnozstva
                var polsm = _db.PolozkaSkladuMnozstva.FirstOrDefault(x => x.Sklad == "HKS" && x.PolozkaSkladu == pol.ID);
                if (polsm == null)
                {
                    var polozka = new PolozkaSkladuMnozstvo() { Sklad = "HKS", PolozkaSkladu = pol.ID };
                    _db.PolozkaSkladuMnozstva.Add(polozka);
                }
                #endregion

                /*var otrt = _db.Prijemky.FirstOrDefault(x => x.ID == "000000001");
                //if (otrt == null)
                //{
                //    var polozka = new Prijemka() { ID = "000000001", Sklad = "HKS", Poznamka = "oo", DodaciID = "0", FakturaID = "0", Objednavka = "0" };
                //    _db.Prijemky.Add(polozka);
                //}
                //var eere = _db.PrijemkyPolozky.FirstOrDefault(x => x.Skupina == "000000001");
                //if (eere == null)
                //{
                //    var polozka = new PrijemkaPolozka() { Nazov = "eee", Skupina = "000000001", PolozkaSkladu = "0000001" };
                //    _db.PrijemkyPolozky.Add(polozka);
                //}
                //var vydjrk = _db.Vydajky.FirstOrDefault(x => x.Sklad == "HKS");
                //if (vydjrk == null)
                //{
                //    var polozka = new Vydajka() { ID = "000000002", Sklad = "HKS", Poznamka = "uu" };
                //    _db.Vydajky.Add(polozka);
                //}
                //var vydajpolre = _db.VydajkyPolozky.FirstOrDefault(x => x.Skupina == "000000002");
                //if (vydajpolre == null)
                //{
                //    var polozka = new PrijemkaPolozka() { Nazov = "uuu", Skupina = "000000002", PolozkaSkladu = "0000001" };
                //    _db.VydajkyPolozky.Add(polozka);
                //}*/

                /*#region pridanie skladovych obdobi v aktualny mesiac
                //var skldobd = _db.SkladObdobi.FirstOrDefault(x => x.Obdobie == SkladObdobie.GetActualSeason() && x.Sklad == "HKS");
                //if (skldobd == null)
                //{
                //    var polozka = new SkladObdobie() { Obdobie = SkladObdobie.GetActualSeason(), Sklad = "HKS" };
                //    _db.SkladObdobi.Add(polozka);
                //}
                //skldobd = _db.SkladObdobi.FirstOrDefault(x => x.Obdobie == SkladObdobie.GetActualSeason() && x.Sklad == "KS");
                //if (skldobd == null)
                //{
                //    var polozka = new SkladObdobie() { Obdobie = SkladObdobie.GetActualSeason(), Sklad = "KS" };
                //    _db.SkladObdobi.Add(polozka);
                //}
                #endregion*/

                #region vymazanie default user validatora
                _userManager.UserValidators.RemoveAt(0);    //mam vytvoreny vlastny a default som nevedel dat inak prec. Predpokladam, ze default validator sa vytvara vzdy ako prvy. Robim to pre to lebo default validator ma automaticky nastavene ze UserName nesmie byt null aj ked to DB umoznuje tak som si vytvoril vlastny
                #endregion
                await _db.SaveChangesAsync();
                _done = true;
            }
        }

    }
}