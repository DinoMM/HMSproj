using DBLayer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

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
        public void GenerujPdf(HostConReservation con)       //vygeneruje pdf z objednavky
        {

            PdfDocumentBuilder builder = new PdfDocumentBuilder();              //vytvorenie buildera, ktory to na konci vsetko prevedie to reality
            PdfPageBuilder page = builder.AddPage(PageSize.A4);                 //takto sa prida stránka do buildera

            #region basic
            string fontPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MontserratRegular.ttf");    //vlastny font potrebujeme lebo klasicke nepodporuju naše znaky(ľščťžýáí..)
            byte[] fontBites = System.IO.File.ReadAllBytes(fontPath);                       //font sa precita zo subora do byte []
            PdfDocumentBuilder.AddedFont fontR = builder.AddTrueTypeFont(fontBites);            //nasledne mozeme vyuzit tento font
            fontPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MontserratBold.ttf");      //Bold
            fontBites = System.IO.File.ReadAllBytes(fontPath);
            PdfDocumentBuilder.AddedFont fontB = builder.AddTrueTypeFont(fontBites);
            #endregion

            int Sx = 30;                //pomocne Pointy -> pre Ohranicenie, riadkovanie, text
            int Ex = 565;
            int Sy = 42;
            int Ey = 800;
            int Riad = 30;
            int TSx = 35;
            int TEx = 560;
            int TSy = 47;
            int TEy = 788;

            int fontSize = 12;

            string datumVystavenia = (DateOnly.FromDateTime(DateTime.Today)).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);  // datum


            page.DrawRectangle(new PdfPoint(Sx, Sy), 535, 758);     //HlavneBoundary


            page.AddText("Potvrdenie o ubytovaní", fontSize + 4, new PdfPoint(TEx / 2 - 35, TEy - 18), fontB);
            /////////////////////////////////
            page.DrawRectangle(new PdfPoint(Sx + 3, Ey - 200 - Riad - 10), Ex / 2 - 20, 200 - 3);       //prvy stvorcek DODAVATEL
            PdfPoint start = new PdfPoint(Sx + 3, Ey - 200 - Riad - 10);
            double width = Ex / 2 - 20;
            double height = 200 - 3;






            #region Build
            byte[] documentBytes = builder.Build();         //skonvertovanie do byte

            System.IO.File.WriteAllBytes(FullPathPDF, documentBytes);         //vysledne pdf
            #endregion
        }

    }
}
