using DBLayer.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using PdfDocument = PdfSharp.Pdf.PdfDocument;

namespace PdfCreator.Models
{
    public class PotvrdenieOUbytovaniPDF : PdfCreator
    {

        public PotvrdenieOUbytovaniPDF()
        {
            FileName = "Potvrdenie_o_ubytovani.pdf";
            FullPathPDF = System.IO.Path.Combine(FolderPath, FileName);    //vysledna cesta
        }

        /// <summary>
        /// Použitá stiahnutá knižnica z NuGet -> PdfPig -> https://www.nuget.org/packages/PdfPig/#readme-body-tab
        /// + stiahnuté 2 fonty Montserrat (.ttf) (lebo diakritika) -> https://www.fontspace.com/collection/best-of-mo-peterson-design-cw3d9qd
        /// </summary>
        /// <param name="objednavka"></param>
        public void GenerujPdf(HostConReservation con, Dodavatel organizacia)       //vygeneruje pdf z objednavky
        {
            int fontSize = 12;
            var originalsablona = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registraciaform.pdf");

            PdfDocument outputDocument = new PdfDocument();     //vysledne pdf
            PdfDocument inputDocument = PdfReader.Open(originalsablona, PdfDocumentOpenMode.Import);  //nacitanie sablony

            PdfPage page = inputDocument.Pages[0];      //pridanie strany cez copy
            var newpage = outputDocument.AddPage(page);
            XGraphics gfx = XGraphics.FromPdfPage(newpage);

            XFont font = new XFont("Arial", fontSize);

            // datum

            string datumVystavenia = (DateOnly.FromDateTime(DateTime.Today)).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);

            string text = datumVystavenia;
            gfx.DrawString("Dátum vystavenia: " + text, font, XBrushes.Black, new XPoint(370, 98));

            // tabulka

            int x = 235;
            int y = 50;

            text = con.ReservationZ?.RoomNumber ?? "";
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75, 130));

            text = con.HostX.Surname + " " + con.HostX.Name;
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75, 130 + (y * 1)));

            text = con.HostX.Address;
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75, 130 + (y * 2) - 1));

            text = con.HostX.Nationality;
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75, 130 + (y * 3) - 1));

            text = con.HostX.Sex ? "Žena" : "Muž";
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75, 130 + (y * 4) - 1));

            text = con.HostX.GuestZ?.PhoneNumber ?? "";
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75, 130 + (y * 5) - 4));

            ////////

            text = con.HostX.BirthDate.ToString("dd.MM.yyyy");
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75 + x, 130 + (y * 0)));

            text = con.HostX.CitizenID;
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75 + x, 130 + (y * 1)));

            text = con.HostX.Passport;
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75 + x, 130 + (y * 2) - 1));

            text = con.HostX.BirthNumber;
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75 + x, 130 + (y * 3) - 1));

            //volne policko

            text = con.HostX.GuestZ?.Email ?? "";
            text = text.Length <= 30 ? text : text.Substring(0, 35);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(75 + x, 130 + (y * 5) - 4));


            ////////////////Organizacia
            text = organizacia.Nazov;
            text = text.Length <= 50 ? text : text.Substring(0, 50);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(245, 639));

            text = organizacia.Obec;
            text = text.Length <= 50 ? text : text.Substring(0, 50);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(125, 666));

            text = organizacia.Adresa;
            text = text.Length <= 50 ? text : text.Substring(0, 50);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(135, 692));

            text = organizacia.ICO;
            text = text.Length <= 50 ? text : text.Substring(0, 50);
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(115, 718));


            outputDocument.Save(FullPathPDF);

           


        }

    }
}
