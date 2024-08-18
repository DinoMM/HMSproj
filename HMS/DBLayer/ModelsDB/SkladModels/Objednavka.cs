using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Models
{
    public partial class Objednavka : ICloneable
    {
        
        public static string DajNoveID(DBContext db)
        {
            int adder = 1;
            string newID;
            do
            {
                int cislo;
                if (db.Objednavky.Count() != 0)
                {
                    cislo = int.Parse(db.Objednavky.DefaultIfEmpty().Max(x => x != null ? x.ID : "1") ?? "1") + adder;
                }
                else
                {
                    cislo = 1;
                }
                //moznost pridat prefix
                newID = cislo.ToString("D7");
                ++adder;

            } while (db.Objednavky.FirstOrDefault(d => d.ID == newID) != null);
            return newID;
        }

        public static void NastavStavZPrijemok(Prijemka prijemka, in DBContext db)
        {
            if (string.IsNullOrEmpty(prijemka.Objednavka) || string.IsNullOrEmpty(prijemka.ID)) //prazdne pole objednavky, ID
            {
                return;
            }

            var foundedObjednavka = db.Objednavky.FirstOrDefault(x => x.ID == prijemka.Objednavka && x.Stav == StavOBJ.Schvalena || x.Stav == StavOBJ.Ukoncena);
            if (foundedObjednavka == null)
            {           //ak sa nenasla schvalena/ukoncena objednavka
                return;
            }
            #region nacitanie poloziek z objednavky
            var polozkyZObjednavky = db.PolozkySkladuObjednavky.Where(x => x.Objednavka == prijemka.Objednavka).ToList();   //polozky z objednavky

            var celkoveMnozstvaZObjednavky = ZosumarizujPolozkyObjednavky(foundedObjednavka, in db);
            #endregion
            #region nacitanie poloziek zo SCHVALENYCH prijemok
            var prijemkySRovnakouObjednavkou = db.Prijemky.Where(x => x.Objednavka == prijemka.Objednavka && x.Spracovana == true).ToList();  //vsetky schvalene prijemky s rovnakou objednavkou, obdobie moze presahovat
            if (prijemkySRovnakouObjednavkou.Count == 0)
            {  //ak nie su ziadne prijemky s rovnakou objednavkou
                if (foundedObjednavka.Stav == StavOBJ.Ukoncena)
                {
                    foundedObjednavka.Stav = StavOBJ.Schvalena;
                    db.SaveChanges();
                }
                return;
            }

            List<PolozkaSkladu> celkoveMnozstvaZPrijemok = new();   //finalny list bude obsahovat vsetky schvalene polozky z prijemok
            foreach (var item in prijemkySRovnakouObjednavkou) // prejde vsetky prijemky s rovnakou objednavkou
            {
                var celkoveMnozstvaZPrijemky = Prijemka.ZosumarizujPolozkyPrijemky(item, in db); //ziska vsetky polozky z prijemky a zosumarizuje ich
                celkoveMnozstvaZPrijemok.AddRange(celkoveMnozstvaZPrijemky); //tento list prida na koniec finalneho listu
            }
            celkoveMnozstvaZPrijemok = PolozkaSkladu.ZosumarizujListPoloziek(in celkoveMnozstvaZPrijemok); //zosumarizuje finalny list
            #endregion
            #region porovnanie poloziek z objednavky a prijemok
            foreach (var item in celkoveMnozstvaZObjednavky)
            {
                if (celkoveMnozstvaZPrijemok.FirstOrDefault(x => (x.ID == item.ID && x.Mnozstvo < item.Mnozstvo)) != null) //ak polozky z prijemok, ktora by mala menej mnozstva ako polozky z objednavka
                {
                    if (foundedObjednavka.Stav == StavOBJ.Ukoncena)
                    {
                        foundedObjednavka.Stav = StavOBJ.Schvalena;
                        db.SaveChanges();
                    }
                    return; //ak sa najde jedna tak netreba hladat dalej
                }
                if (celkoveMnozstvaZPrijemok.RemoveAll(x => x.ID == item.ID) != 1) //vymaze polozku z finalneho listu a musi sa vzdy vymazat jedna polozka -> Ciel je mat prazdny finalny list
                {
                    if (foundedObjednavka.Stav == StavOBJ.Ukoncena)
                    {
                        foundedObjednavka.Stav = StavOBJ.Schvalena;
                        db.SaveChanges();
                    }
                    return;
                }
            }
            #endregion
            if (foundedObjednavka.Stav == StavOBJ.Schvalena)
            {
                foundedObjednavka.Stav = StavOBJ.Ukoncena;
                db.SaveChanges();
            }
        }

        public static List<PolozkaSkladu> ZosumarizujPolozkyObjednavky(Objednavka objednavka, in DBContext db)
        {
            var polozkyZItem = db.PolozkySkladuObjednavky.Where(x => x.Objednavka == objednavka.ID).ToList(); //ziska vsetky polozky z objednavky

            return PolozkaSkladu.ZosumarizujListPoloziek(in polozkyZItem);
        }
    }
}
