using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using DBLayer.Models;
using System.Collections.ObjectModel;

namespace PdfCreator.Models
{
    public class UzavierkaPDF : PdfCreator
    {
        private List<PolozkaSkladu> zoznamPoloziek = new(); //zoznam poloziek

        private List<PolozkaSkladuMnozstvo> zoznamPoloziekSkladuMnozstva = new(); //zoznam poloziek s mnozstvami na zaciatku obdobia
        private List<PolozkaSkladu> zoznamAktualnehoMnozstva = new(); //zoznam aktualneho mnozstva po zohladneni prijmov a vydajov
        private List<PolozkaSkladu> zoznamPrijateho = new(); //zoznam prijatych mnozstiev
        private List<PolozkaSkladu> zoznamPrijatehoZPrijemok = new(); //zoznam prijatych mnozstiev len z prijemok
        private List<PolozkaSkladu> zoznamVydateho = new(); //zoznam vydatych mnozstiev
        private List<PolozkaSkladu> zoznamPrevodiekZoSkladu = new(); //zoznam vydatych mnozstiev z prevodiek z tohto skladu
        private DateTime Obdobie;
        private Sklad SkladOb;
        private double CenaAktSkladu = 0.0;
        private string TotSAktualSklad = "";
        public UzavierkaPDF(List<PolozkaSkladu> zoznamPoloziek, List<PolozkaSkladuMnozstvo> zoznamPoloziekSkladuMnozstva, List<PolozkaSkladu> zoznamAktualnehoMnozstva, List<PolozkaSkladu> zoznamPrijateho, List<PolozkaSkladu> zoznamPrijatehoZPrijemok, List<PolozkaSkladu> zoznamVydateho, List<PolozkaSkladu> zoznamPrevodiekZoSkladu, DateTime obdobie, Sklad sklad, double cenaAktSkladu, string totalSumAktualSklad) : base()
        {
            FileName = "UzavierkaSkladu.pdf";
            FullPathPDF = System.IO.Path.Combine(FolderPath, FileName);    //vysledna cesta
            this.zoznamPoloziek = zoznamPoloziek;
            this.zoznamPoloziekSkladuMnozstva = zoznamPoloziekSkladuMnozstva;
            this.zoznamAktualnehoMnozstva = zoznamAktualnehoMnozstva;
            this.zoznamPrijateho = zoznamPrijateho;
            this.zoznamPrijatehoZPrijemok = zoznamPrijatehoZPrijemok;
            this.zoznamVydateho = zoznamVydateho;
            this.zoznamPrevodiekZoSkladu = zoznamPrevodiekZoSkladu;
            Obdobie = obdobie;
            SkladOb = sklad;
            CenaAktSkladu = cenaAktSkladu;
            TotSAktualSklad = totalSumAktualSklad;
        }

        public void GenerujPdf()
        {
            PdfDocumentBuilder builder = new PdfDocumentBuilder();              //vytvorenie buildera, ktory to na konci vsetko prevedie to reality
            PdfPageBuilder page = builder.AddPage(PageSize.A4, isPortrait: false);

            #region basic
            string fontPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MontserratRegular.ttf");    //vlastny font potrebujeme lebo klasicke nepodporuju naše znaky(ľščťžýáí..)
            byte[] fontBites = System.IO.File.ReadAllBytes(fontPath);                       //font sa precita zo subora do byte []
            PdfDocumentBuilder.AddedFont fontR = builder.AddTrueTypeFont(fontBites);            //nasledne mozeme vyuzit tento font
            fontPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MontserratBold.ttf");      //Bold
            fontBites = System.IO.File.ReadAllBytes(fontPath);
            PdfDocumentBuilder.AddedFont fontB = builder.AddTrueTypeFont(fontBites);
            #endregion
            #region points
            int Sx = 25;                //pomocne Pointy -> pre Ohranicenie, riadkovanie, text
            int Ex = 817;
            int Sy = 30;
            int Ey = 565;
            int Riad = 25;


            int fontSize = 12;
            #endregion

            #region Content
            page.DrawRectangle(new PdfPoint(Sx, Sy), 792, 535);     //HlavneBoundary

            int strana = 1;
            PdfPoint actual;
            ///////////////Hlavicka///////////
            {
                double imx = ((Sx + Ex) * 0.5) - 150;
                double imy = ((Sy + Ey) * 0.75) + Riad * 4 - 5;
                var endp = page.AddText($"Inventúrny súpis obdobia: {Sklad.ShortFromObdobie(Obdobie)}", fontSize + 4, new PdfPoint(imx, imy), fontB);
                actual = endp[^1].EndBaseLine;
                endp = page.AddText($"Sklad: ", fontSize + 2, actual.Translate(100, 0), fontR);
                actual = endp[^1].EndBaseLine;
                page.AddText($"{SkladOb.ID}", fontSize + 2, actual, fontB);
                endp = page.AddText($"Cena aktuálneho skladu: ", fontSize + 2, new PdfPoint(imx + 25, imy - Riad), fontR);
                actual = endp[^1].EndBaseLine;
                page.AddText($"{TotSAktualSklad} €", fontSize + 2, actual, fontB);
                endp = page.AddText($"Cena prijatých položiek: ", fontSize + 2, new PdfPoint(imx + 25, imy - Riad * 2), fontR);
                actual = endp[^1].EndBaseLine;
                page.AddText($"{GetTotalSumPrijemky()} €", fontSize + 2, actual, fontB);
                endp = page.AddText($"Cena vydaných položiek: ", fontSize + 2, new PdfPoint(imx + 25, imy - Riad * 3), fontR);
                actual = endp[^1].EndBaseLine;
                page.AddText($"{GetTotalSumVydajky()} €", fontSize + 2, actual, fontB);

                page.AddText($"Dátum vytvorenia: {DateTime.Today.ToString("dd.MM.yyyy")}", fontSize + 1, actual.Translate(145, -(Riad / 2)), fontR);
                ////Strana papiera dole
                page.AddText($"Strana {strana}", fontSize, new PdfPoint(Ex - 45, Sy - 18), fontR);
                ////

            }
            page.DrawLine(new PdfPoint(Sx, (Sy + Ey) * 0.75), new PdfPoint(Ex, (Sy + Ey) * 0.75), 2); //hlavna ciara    
            ///////////////Tabulka////////////
            ////////////////////////////Hlavicka Tabulky////////////////////////
            var start = actual = new PdfPoint(Sx + 3, (Sy + Ey) * 0.75 - Riad);
            var TableStarts = new PdfPoint[5];
            TableStarts[0] = actual = page.AddText("        ID", fontSize - 1, actual, fontB)[^1].EndBaseLine.Translate(38, 0);
            page.DrawLine(actual.Translate(-1, Riad), actual.Translate(-1, -392));
            TableStarts[1] = actual = page.AddText("               Názov položky", fontSize, actual, fontB)[^1].EndBaseLine.Translate(82, 0);
            page.DrawLine(actual.Translate(-1, Riad), actual.Translate(-1, -392));
            TableStarts[2] = actual = page.AddText("  MJ", fontSize, actual, fontB)[^1].EndBaseLine.Translate(13, 0);   //4
            page.DrawLine(actual.Translate(-1, Riad), actual.Translate(-1, -392));
            TableStarts[3] = actual = page.AddText("    Cena", fontSize, actual, fontB)[^1].EndBaseLine.Translate(25, 0);
            page.DrawLine(actual.Translate(-1, Riad), actual.Translate(-1, -392));
            TableStarts[4] = actual = page.AddText("   Aktuálne Množstvo", fontSize, actual, fontB)[^1].EndBaseLine.Translate(17, 0);
            page.DrawLine(actual.Translate(-1, Riad), actual.Translate(-1, -392));
            actual = page.AddText("Kontrola", fontSize, actual.Translate(75, 0), fontB)[^1].EndBaseLine;

            actual = start;
            page.DrawLine(actual.Translate(-3, -2), actual.Translate(Ex - Sx - 3, -2));
            ////////////////////////////Telo Tabulky////////////////////////
            start = actual = actual.Translate(0, -Riad);
            var lineStart = actual.Translate(-3, -2);
            var lineEnd = actual.Translate(Ex - Sx - 3, -2);
            double maxNazov = TableStarts[1].X - TableStarts[0].X;
            double maxMJ = TableStarts[2].X - TableStarts[1].X;
            
            
            foreach (var item in zoznamPoloziek)
            {
                actual = page.AddText(item.ID, fontSize, actual, fontR)[^1].EndBaseLine;
                TableStarts[0] = page.AddText(WillFit(page, item.Nazov, fontSize, TableStarts[0], fontR, (int)maxNazov).Item2, fontSize, TableStarts[0].Translate(0, -Riad), fontR)[0].StartBaseLine;
                TableStarts[1] = page.AddText(WillFit(page, item.MernaJednotka, fontSize, TableStarts[1], fontR, (int)maxMJ).Item2, fontSize, TableStarts[1].Translate(0, -Riad), fontR)[0].StartBaseLine;
                TableStarts[2] = page.AddText(GetAktualnaCena(item), fontSize, TableStarts[2].Translate(0, -Riad), fontR)[0].StartBaseLine;
                TableStarts[3] = page.AddText(GetAktualneMnozstvo(item), fontSize, TableStarts[3].Translate(0, -Riad), fontR)[0].StartBaseLine;

                page.DrawLine(lineStart, lineEnd);

                start = actual = start.Translate(0, -Riad);
                lineStart = lineStart.Translate(0, -Riad);
                lineEnd = lineEnd.Translate(0, -Riad);

                if (actual.Y <= Sy && zoznamPoloziek.Last() != item)     //ak sa vypise strana a mame este polozky tak vytvarame novu stranu
                {
                    page = builder.AddPage(PageSize.A4, isPortrait: false);
                    page.DrawRectangle(new PdfPoint(Sx, Sy), 792, 535, 1);     //HlavneBoundary
                    ////Strana papiera dole
                    ++strana;
                    page.AddText($"Strana {strana}", fontSize, new PdfPoint(Ex - 45, Sy - 18), fontR);
                    ////
                    ////////////////////////////Hlavicka Tabulky Nova strana////////////////////////
                    start = actual = new PdfPoint(Sx + 3, Ey - Riad);
                    TableStarts[0] = actual = page.AddText("        ID", fontSize - 1, actual, fontB)[^1].EndBaseLine.Translate(38, 0);
                    page.DrawLine(actual.Translate(-1, Riad), actual.Translate(-1, -511));
                    TableStarts[1] = actual = page.AddText("               Názov položky", fontSize, actual, fontB)[^1].EndBaseLine.Translate(82, 0);
                    page.DrawLine(actual.Translate(-1, Riad), actual.Translate(-1, -511));
                    TableStarts[2] = actual = page.AddText("  MJ", fontSize, actual, fontB)[^1].EndBaseLine.Translate(13, 0);   //4
                    page.DrawLine(actual.Translate(-1, Riad), actual.Translate(-1, -511));
                    TableStarts[3] = actual = page.AddText("    Cena", fontSize, actual, fontB)[^1].EndBaseLine.Translate(25, 0);
                    page.DrawLine(actual.Translate(-1, Riad), actual.Translate(-1, -511));
                    TableStarts[4] = actual = page.AddText("   Aktuálne Množstvo", fontSize, actual, fontB)[^1].EndBaseLine.Translate(17, 0);
                    page.DrawLine(actual.Translate(-1, Riad), actual.Translate(-1, -511));
                    actual = page.AddText("Kontrola", fontSize, actual.Translate(75, 0), fontB)[^1].EndBaseLine;

                    actual = start;
                    page.DrawLine(actual.Translate(-3, -2), actual.Translate(Ex - Sx - 3, -2));
                    ///////
                    start = actual = actual.Translate(0, -Riad);
                    lineStart = actual.Translate(-3, -2);
                    lineEnd = actual.Translate(Ex - Sx - 3, -2);
                }
            }
            #endregion
            #region Build
            byte[] documentBytes = builder.Build();         //skonvertovanie do byte

            System.IO.File.WriteAllBytes(FullPathPDF, documentBytes);         //vysledne pdf
            #endregion
        }
        
        public string GetAktualneMnozstvo(PolozkaSkladu poloz)
        {
            return zoznamAktualnehoMnozstva.FirstOrDefault(x => x.ID == poloz.ID)?.Mnozstvo.ToString("F3") ?? 0.ToString("F3");
        }
        
        public string GetTotalSumPrijemky()
        {
            return zoznamPrijatehoZPrijemok.Sum(x => x.CelkovaCena).ToString("F3");
        }

        public string GetTotalSumVydajky()
        {
            var sum1 = zoznamVydateho.Sum(x => x.CelkovaCena);
            var sum2 = zoznamPrevodiekZoSkladu.Sum(x => x.CelkovaCena);
            return (zoznamVydateho.Sum(x => x.CelkovaCena) - zoznamPrevodiekZoSkladu.Sum(x => x.CelkovaCena)).ToString("F3");
        }
        public string GetAktualnaCena(PolozkaSkladu poloz)
        {
            return zoznamAktualnehoMnozstva.FirstOrDefault(x => x.ID == poloz.ID)?.Cena.ToString("F3") ?? 0.ToString("F3");
        }
    }
}
