using DBLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Content;
using System.ComponentModel.DataAnnotations;

namespace PdfCreator.Models
{
    public class HskZobrazeniePDF : PdfCreator
    {
        public HskZobrazeniePDF()
        {
            FileName = "HSK_Zobrazenie.pdf";
            FullPathPDF = System.IO.Path.Combine(FolderPath, FileName);    //vysledna cesta
        }

        /// <summary>
        /// Použitá stiahnutá knižnica z NuGet -> PdfPig -> https://www.nuget.org/packages/PdfPig/#readme-body-tab
        /// + stiahnuté 2 fonty Montserrat (.ttf) (lebo diakritika) -> https://www.fontspace.com/collection/best-of-mo-peterson-design-cw3d9qd
        /// </summary>
        /// 
        public void GenerujPdf(List<Rezervation> zoznam, List<(string Header, Func<Rezervation, string> CellValue)> settings)       //vygeneruje pdf
        {
            PdfDocumentBuilder builder = new PdfDocumentBuilder();              //vytvorenie buildera, ktory to na konci vsetko prevedie to reality
            PdfPageBuilder page = builder.AddPage(PageSize.A4, isPortrait: false);                 //takto sa prida stránka do buildera

            #region basic
            string fontPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MontserratRegular.ttf");    //vlastny font potrebujeme lebo klasicke nepodporuju naše znaky(ľščťžýáí..)
            byte[] fontBites = System.IO.File.ReadAllBytes(fontPath);                       //font sa precita zo subora do byte []
            PdfDocumentBuilder.AddedFont fontR = builder.AddTrueTypeFont(fontBites);            //nasledne mozeme vyuzit tento font
            fontPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MontserratBold.ttf");      //Bold
            fontBites = System.IO.File.ReadAllBytes(fontPath);
            PdfDocumentBuilder.AddedFont fontB = builder.AddTrueTypeFont(fontBites);
            #endregion
            #region Content
            int Sx = 15;                //pomocne Pointy -> pre Ohranicenie, riadkovanie, text
            int Ex = 827;
            int Sy = 15;
            int Ey = 595;
            int Riad = 20;

            var midLine = new PdfPoint(Sx + 10, Ey - Riad).Translate(Ex / 2 - (20), -5);
            bool twoSides = false;
            int Sx2 = 420;


            int fontSize = 11;

            int strana = 1;
            ////////////////////
            string datumVystavenia = (DateTime.Now).ToString("dd.MM.yyyy HH:mm:ss");  // datum

            page.AddText("Stav Izieb", fontSize + 4, new PdfPoint(Ex / 2 - 20, Ey - 18), fontB);
            page.AddText(datumVystavenia, fontSize, new PdfPoint(Ex / 1.2, Ey - 18), fontR);
            page.AddText($"Strana {strana}", fontSize, new PdfPoint(Ex - 45, Sy - fontSize), fontR);
            //////////////////////////
            PdfPoint actual = new(Sx + 10, Ey - Riad);
            PdfPoint actRiadok = actual.Translate(0, 0);
            PdfPoint start = actual.Translate(0, 0);
            IReadOnlyList<Letter> endp;

            ////////////////////
            page.DrawRectangle(new PdfPoint(Sx, Sy), 812, 555);     //HlavneBoundary
            actRiadok = actual = start = actRiadok.Translate(0, -Riad);        //skocenie o riadok
            ////////////////////

            double summax = 0.0;
            List<double> maxList = new();
            for (int i = 0; i < settings.Count; ++i)
            {
                var res = MaxLength<Rezervation>(zoznam, settings[i], page, fontSize, actual, fontR);
                if (res.Item2.HasValue)
                {
                    summax += res.Item2.Value.Item1;
                    maxList.Add(res.Item2.Value.Item1);
                    settings[i] = (res.Item2.Value.Item2, settings[i].CellValue);
                }
                else
                {
                    summax += res.Item1;
                    maxList.Add(res.Item1);
                }

            }

            if (summax < midLine.X) // rozhodnutie ak sa ma prepolit alebo nie
            {
                twoSides = true;
                page.DrawLine(midLine, new PdfPoint(Sx + 10, Ey - Riad).Translate(Ex / 2 - (20), -559));  //ciara pod nazvom stred
            }

            var spaceToSpare = twoSides ?
                ((842 / 2) - Sx - summax) / settings.Count :
                (842 - (Sx + (842 - Ex)) - summax) / settings.Count; //urci este moznu medzeru medzi jednotlivymi headrami
            PdfPoint start2;
            var actual2 = start2 = new PdfPoint(Sx2, actual.Y);
            for (int i = 0; i < settings.Count; ++i)    //vypisanie
            {
                page.AddText(settings[i].Header, fontSize, actual, fontB);
                actual = actual.Translate(maxList[i] + spaceToSpare, 0);
                if (twoSides)
                {
                    page.AddText(settings[i].Header, fontSize, actual2, fontB);
                    actual2 = actual2.Translate(maxList[i] + spaceToSpare, 0);
                }
            }
            bool swtch = false;
            actual = actRiadok = actRiadok.Translate(0, -Riad);
            actual2 = start2.Translate(0, -Riad);
            foreach (var item in zoznam)
            {

                for (int i = 0; i < settings.Count; ++i)    //vypisanie
                {
                    page.AddText(settings[i].CellValue(item), fontSize, actual, fontR);
                    actual = actual.Translate(maxList[i] + spaceToSpare, 0);
                }
                actual = actRiadok = actRiadok.Translate(0, -Riad);

                if (actual.Y <= Sy && zoznam.Last() != item)
                {
                    if (!swtch && twoSides)
                    {
                        swtch = true;
                        actual = actRiadok = actual2.Translate(0, 0);
                        continue;
                    }
                    swtch = false;
                    // pridavanie dalsej stranky
                    page = builder.AddPage(PageSize.A4, isPortrait: false);
                    page.AddText("Stav Izieb", fontSize + 4, new PdfPoint(Ex / 2 - 20, Ey - 18), fontB);
                    page.AddText(datumVystavenia, fontSize, new PdfPoint(Ex / 1.2, Ey - 18), fontR);
                    page.DrawRectangle(new PdfPoint(Sx, Sy), 812, 555);
                    page.AddText($"Strana {++strana}", fontSize, new PdfPoint(Ex - 45, Sy - fontSize), fontR);

                    if (twoSides)
                    {
                        page.DrawLine(midLine, new PdfPoint(Sx + 10, Ey - Riad).Translate(Ex / 2 - (20), -559));  //ciara pod nazvom stred
                    }
                    actual = actRiadok = start.Translate(0, 0);
                    actual2 = start2.Translate(0, 0);
                    for (int i = 0; i < settings.Count; ++i)    //vypisanie hlavicky
                    {
                        page.AddText(settings[i].Header, fontSize, actual, fontB);
                        actual = actual.Translate(maxList[i] + spaceToSpare, 0);
                        if (twoSides)
                        {
                            page.AddText(settings[i].Header, fontSize, actual2, fontB);
                            actual2 = actual2.Translate(maxList[i] + spaceToSpare, 0);
                        }
                    }
                    actual = actRiadok = actRiadok.Translate(0, -Riad);
                    actual2 = start2.Translate(0, -Riad);
                }
            }

            #endregion
            #region Build
            byte[] documentBytes = builder.Build();         //skonvertovanie do byte

            System.IO.File.WriteAllBytes(FullPathPDF, documentBytes);         //vysledne pdf
            #endregion
        }
    }
}
