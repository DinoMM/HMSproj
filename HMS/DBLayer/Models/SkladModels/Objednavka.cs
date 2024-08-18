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
        [Key]
        public string ID { get; set; } = default!;

        [ForeignKey("DodavatelX")]
        public string Dodavatel { get; set; } = default!;
        public Dodavatel DodavatelX { get; set; }
        [ForeignKey("OdberatelX")]
        public string Odberatel { get; set; } = default!;
        public Dodavatel OdberatelX { get; set; }
        [ForeignKey("TvorcaX")]
        public string Tvorca { get; set; } = default!;
        public IdentityUserOwn TvorcaX { get; set; }

        public string? Popis { get; set; }
        public DateTime DatumVznik { get; set; } = DateTime.Today;
        public StavOBJ Stav { get; set; } = StavOBJ.Vytvorena;



        public object Clone()
        {
            var clon = new Objednavka();
            clon.ID = ID;
            clon.Dodavatel = Dodavatel;
            clon.Odberatel = Odberatel;
            clon.Tvorca = Tvorca;
            clon.Popis = Popis;
            clon.DatumVznik = DatumVznik;
            clon.Stav = Stav;

            clon.DodavatelX = DodavatelX;
            clon.OdberatelX = OdberatelX;
            clon.TvorcaX = TvorcaX;
            return clon;
        }
        public Objednavka Clon()
        {
            return (Objednavka)Clone();
        }

        public void SetFromObjednavka(Objednavka obj)
        {
            ID = obj.ID;
            Dodavatel = obj.Dodavatel;
            Odberatel = obj.Odberatel;
            Popis = obj.Popis;
            DatumVznik = obj.DatumVznik;
            Stav = obj.Stav;
            Tvorca = obj.Tvorca;

            DodavatelX = obj.DodavatelX;
            OdberatelX = obj.OdberatelX;
            TvorcaX = obj.TvorcaX;
        }

        public static bool JeObjednavkaOK(Objednavka obj, bool ajID = true)
        {
            if (string.IsNullOrEmpty(obj.ID) && ajID)
            {
                return false;
            }
            if (string.IsNullOrEmpty(obj.Dodavatel) ||
                string.IsNullOrEmpty(obj.Odberatel) ||
                string.IsNullOrEmpty(obj.Tvorca))
            {
                return false;
            }
            return true;

            //DodavatelX;
            //OdberatelX;
        }
    }

    /// <summary>
    /// Jednotlive fazy pri existencii objednavok
    /// </summary>
    public enum StavOBJ
    {
        Vytvorena,
        Schvalena,
        Neschvalena,
        Objednana,
        Ukoncena
    }
}
