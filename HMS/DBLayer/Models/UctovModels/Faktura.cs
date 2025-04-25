using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DBLayer.Models
{
    public partial class Faktura : ObservableValidator, ICloneable
    {
        [Key]
        [ObservableProperty]
        private string iD = default!;
        [Required(ErrorMessage = "Nutné pole.")]
        [ForeignKey("SkupinaX")]
        public string? Skupina { get; set; } = "";
        public PohSkup? SkupinaX { get; set; }
        /// <summary>
        /// Kedy vznikla faktura
        /// </summary>
        [Required(ErrorMessage = "Nutné pole.")]
        [ObservableProperty]
        private DateTime vystavenie = DateTime.Now;
        /// <summary>
        /// Kedy bol tovar/sluzby dodane
        /// </summary>
        [Required(ErrorMessage = "Nutné pole.")]
        [ObservableProperty]
        private DateTime dodanie = DateTime.Now;
        /// <summary>
        /// Splatnost faktury, default +14 dni do 23:59:59
        /// </summary>
        [Required(ErrorMessage = "Nutné pole.")]
        [DateNotSoonerThan("Vystavenie", ErrorMessage = "Nemôže byť skorej ako dátum vystavenia")]
        public DateTime Splatnost { get => _splatnost; set => _splatnost = value.Date.AddHours(23).AddMinutes(59).AddSeconds(59); }
        private DateTime _splatnost = DateTime.Today.AddDays(14).AddHours(23).AddMinutes(59).AddSeconds(59);

        [ForeignKey("OdKohoX")]
        [Required(ErrorMessage = "Nutné pole.")]
        public string OdKoho { get; set; } = "";
        public Dodavatel OdKohoX { get; set; }

        /// <summary>
        /// Null - nespracovaná, True - spracovaná, False - stornovaná
        /// </summary>
        [ObservableProperty]
        private bool? spracovana = null;   
        [Column(TypeName = "decimal(18, 3)")]
        [DecimalNonNegative]
        [ObservableProperty]
        private decimal cenaBezDPH = default!;
        [Column(TypeName = "decimal(18, 3)")]
        [DecimalNonNegative]
        [ObservableProperty]
        private decimal cenaSDPH = default!;

        #region pre koho
        /// <summary>
        /// Nazov: PO - nazov firmy, FO - meno a priezvisko
        /// </summary>
        [Required(ErrorMessage = "Nutné pole.")]
        [ObservableProperty]
        private string nazov = default!;
        /// <summary>
        /// ICO: PO - Ico firmy, FO - nema byt vyplnene
        /// </summary>
        [ObservableProperty]
        private string iCO = "";
        /// <summary>
        /// DIC: PO - DIC firmy, FO - nema byt vyplnene
        /// </summary>
        [ObservableProperty]
        private string dIC = "";
        /// <summary>
        /// IC_DPH: PO - IC_DPH firmy, FO - nema byt vyplnene
        /// </summary>
        [ObservableProperty]
        private string iC_DPH = "";
        /// <summary>
        /// Adresa: PO - Adresa firmy, FO - Adresa / pobyt osoby
        /// </summary>
        [Required(ErrorMessage = "Nutné pole.")]
        [ObservableProperty]
        private string adresa = "";
        /// <summary>
        /// Email: PO - Email firmy, FO - Email osoby
        /// </summary>
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Neplatná emailová adresa.")]
        [ObservableProperty]
        private string email = "";
        /// <summary>
        /// Telefon: PO - Telefon firmy, FO - Telefon osoby
        /// </summary>
        [RegularExpression(@"^\+?[0-9\s\-()]{7,15}$", ErrorMessage = "Neplatné telefónne číslo.")]
        [ObservableProperty]
        private string telefon = "";
        #endregion

        #region uhrada
        [ObservableProperty]
        private FormaUhrady formaUhrady = FormaUhrady.Prevodom;
        
        [FormaUhradyCheck]
        [RegularExpression(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]{1,30}$", ErrorMessage = "Neplatné IBAN číslo.")]
        [ObservableProperty]
        private string iBAN = "";
        [ObservableProperty]
        private string variabilnySymbol = "";
        [ObservableProperty]
        private string konstantnySymbol = "";
        [ObservableProperty]
        private string specifickySymbol = "";

        /// <summary>
        /// Odkaz na objednavku, nemusi byt prepojene so evidenciou objednavok
        /// </summary>
        [ObservableProperty]
        private string cisloObjednavky = "";
        #endregion

        /// <summary>
        /// DPH v percentach, [NotMapped] vypocitane z cien
        /// </summary>
        [NotMapped]
        public decimal DPH { get => CenaBezDPH != 0 ? ((CenaSDPH * 100 ) / CenaBezDPH) - 100 : 0; }

        /// <summary>
        /// [NotMapped]
        /// </summary>
        [NotMapped]

        public Dodavatel? Odberatel { get; set; } = null;

        public string StavSpracovania()
        {
            if (Spracovana == null)
            {
                return "Nespracovaná";
            }
            if (Spracovana == true)
            {
                return "Spracovaná";
            }
            else
            {
                return "Stornovaná";
            }
        }

        public bool? GetBoolStavSpracovania(string stavsprac)
        {
            switch (stavsprac)
            {
                case "Nespracovaná":
                    return null;
                case "Spracovaná":
                    return true;
                case "Stornovaná":
                    return false;
                default:
                    return null;
            }
        }
        public List<string> GetListStavSpracovania()
        {
            return new List<string>() { "Nespracovaná", "Spracovaná", "Stornovaná" };
        }

        public object Clone()
        {
            var clone = new Faktura();
            clone.ID = this.ID;
            clone.Skupina = this.Skupina;
            clone.SkupinaX = this.SkupinaX;
            clone.Vystavenie = this.Vystavenie;
            clone.Dodanie = this.Dodanie;
            clone.Splatnost = this.Splatnost;
            clone.OdKoho = this.OdKoho;
            clone.OdKohoX = this.OdKohoX;
            clone.Spracovana = this.Spracovana;
            clone.CenaBezDPH = this.CenaBezDPH;
            clone.CenaSDPH = this.CenaSDPH;
            clone.Nazov = this.Nazov;
            clone.ICO = this.ICO;
            clone.DIC = this.DIC;
            clone.IC_DPH = this.IC_DPH;
            clone.Adresa = this.Adresa;
            clone.Email = this.Email;
            clone.Telefon = this.Telefon;
            clone.FormaUhrady = this.FormaUhrady;
            clone.IBAN = this.IBAN;
            clone.VariabilnySymbol = this.VariabilnySymbol;
            clone.KonstantnySymbol = this.KonstantnySymbol;
            clone.SpecifickySymbol = this.SpecifickySymbol;
            clone.CisloObjednavky = this.CisloObjednavky;

            return clone;
        }

        public Faktura Clon()
        {
            return (Faktura)this.Clone();
        }

    }
    public enum FormaUhrady
    {
        [Display(Name = "Prevod")]
        Prevodom,
        [Display(Name = "Hotovosť")]
        Hotovost,
        [Display(Name = "Platba Kartou")]
        PlatbaKartou,
        [Display(Name = "Dobierka")]
        Dobierka
    }

    public class FormaUhradyCheckAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) 
        {
            var ContextMemberNames = new List<string>() { validationContext.MemberName ?? "" };
           
            var faktura = validationContext.ObjectInstance as Faktura;
            if (faktura == null)
            {
                return ValidationResult.Success;
            }
            
            if (faktura.FormaUhrady == FormaUhrady.Hotovost || !string.IsNullOrEmpty(faktura.IBAN))
            {
                return ValidationResult.Success;
            }
            
            return new ValidationResult(ErrorMessage ?? "Nutné pole v prípade platby na účet", ContextMemberNames);
        }
    }

}
