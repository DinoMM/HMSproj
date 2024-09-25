using DBLayer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Content;

namespace PdfCreator.Models
{
    public class BlocekPDF : PdfCreator
    {
        public BlocekPDF() : base()
        {
            FileName = "PokladnicnyBlok.pdf";
            FullPathPDF = System.IO.Path.Combine(FolderPath, FileName);    //vysledna cesta
        }

        /// <summary>
        /// Použitá stiahnutá knižnica z NuGet -> PdfPig -> https://www.nuget.org/packages/PdfPig/#readme-body-tab
        /// + stiahnuté 2 fonty Montserrat (.ttf) (lebo diakritika) -> https://www.fontspace.com/collection/best-of-mo-peterson-design-cw3d9qd
        /// </summary>
        /// 
        public void GenerujPdf(PokladnicnyDoklad doklad, List<ItemPokladDokladu> zoznam)       //vygeneruje pdf
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
            #region Content
            int Sx = 30;                //pomocne Pointy -> pre Ohranicenie, riadkovanie, text
            int Ex = 565;
            int Sy = 42;
            int Ey = 800;
            int Riad = 28;
            int TSx = 35;
            int TEx = 560;
            int TSy = 47;
            int TEy = 788;

            int fontSize = 12;


            string datumVystavenia = (DateTime.Now).ToString("dd-MM-yyyy HH:mm:ss");  // datum


            page.DrawRectangle(new PdfPoint(Sx, Sy), 535, 758);     //HlavneBoundary

            page.AddText("Pokladničný doklad", fontSize + 4, new PdfPoint(TEx / 2 - 50, TEy - 18), fontB);
            //////////////////////////
            page.DrawRectangle(new PdfPoint(Sx + 10, Ey - 17 - Riad), 515, -150);       //stvorec v strede

            PdfPoint actual = new(Sx + 13, Ey - 15 - Riad - Riad);        //zaciatok
            PdfPoint actRiadok = actual.Translate(0, 0);
            PdfPoint start = actual.Translate(0, 0);

            var endp = page.AddText("Číslo dokladu: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(doklad.ID, fontSize, actual, fontR);
            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok

            endp = page.AddText("Vytvorené na pokladni: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(doklad.Kasa, fontSize, actual, fontR);
            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok

            endp = page.AddText("Typ platby: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(!doklad.TypPlatby ? "Platobná karta" : "Hotovosť", fontSize, actual, fontR);
            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok

            endp = page.AddText("Dátum vytvorenia: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(datumVystavenia, fontSize, actual, fontR);
            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok

            ///udaje o organizacii
            actRiadok = actual = start.Translate(280, 0);
            endp = page.AddText("IČO: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(doklad.KasaX.Dodavatel, fontSize, actual, fontR);
            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok

            endp = page.AddText("Názov: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(WillFit(in page, doklad.KasaX.DodavatelX.Nazov, fontSize, actual, fontR, 185).Item2, fontSize, actual, fontR);
            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok

            endp = page.AddText("Obec: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(WillFit(in page, doklad.KasaX.DodavatelX.Obec, fontSize, actual, fontR, 185).Item2, fontSize, actual, fontR);
            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok

            endp = page.AddText("Adresa: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            WriteInBlock(page, doklad.KasaX.DodavatelX.Adresa, fontSize, actual, fontR, 2, Riad, 185, 220);

            //////// Tabulka

            string hlavicka = "                 Názov položky                        Množstvo         Cena s DPH            DPH        Celková cena s DPH";

            fontSize = fontSize - 2;

            var globalRiadok = actRiadok = actual = new(Sx + 13, Ey - 15 - Riad - Riad - (Riad * 5)); ;

            endp = page.AddText(hlavicka, fontSize, actual, fontR);

            globalRiadok = actRiadok = actual = globalRiadok.Translate(0, -Riad);        //skocenie o riadok cez Global

            double CelkovaSuma = 0.0;
            double CelkovaSumaDPH = 0.0;
            foreach (var pol in zoznam)
            {
                CelkovaSuma += pol.CelkovaCena;
                CelkovaSumaDPH += pol.CelkovaCenaDPH;
            }

            int enter = 1;              //pocitanie enterov
            foreach (var pol in zoznam)
            {
                PdfPoint tmp = actual.Translate(0, 0);
                string nazov = pol.Nazov;                             //1
                List<string> pridavaneRiadky = new List<string>();
                double fictionaryY = tmp.Y;
                do
                {               //prejdenie nazvu pre korektne ulozenie do pdfka
                    var velkostTextu = page.MeasureText(nazov, fontSize, tmp, fontR);
                    var textNaRiadku = velkostTextu.TakeWhile(d => d.EndBaseLine.X <= tmp.X + 180);

                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var s in textNaRiadku)
                    {
                        stringBuilder.Append(s.Value);
                    }

                    nazov = nazov.Remove(0, stringBuilder.ToString().Length);
                    pridavaneRiadky.Add(stringBuilder.ToString());

                    if (nazov == "")
                    {
                        break;
                    }
                    fictionaryY -= Riad;
                    if (fictionaryY <= TSy)       //ak sme presli pod hranicu stranky tak sa vyvori nova stranka (nastane nekonecna slucka ak bude text dlhy cez stranu)
                    {
                        page = builder.AddPage(PageSize.A4);
                        page.DrawRectangle(new PdfPoint(Sx, Sy), 535, 758);     //HlavneBoundary
                        globalRiadok = actRiadok = actual = tmp = new PdfPoint(35, 788);
                        fictionaryY = tmp.Y;
                    }
                } while (true);

                for (int i = 0; i < pridavaneRiadky.Count; ++i)                 //vypis do pdf nazov
                {
                    page.AddText(pridavaneRiadky[i], fontSize, tmp, fontR);
                    if (i != pridavaneRiadky.Count - 1)
                    {
                        tmp = tmp.Translate(0, -Riad);
                    }
                }

                enter = pridavaneRiadky.Count;          //kolko enterov sa ma rezervovat pre nazov

                actual = actual.Translate(190, 0);                      //vypis ostatnych stlpcov
                page.AddText(pol.Mnozstvo.ToString("F2"), fontSize, actual, fontR);                //2

                actual = actual.Translate(70, 0);
                page.AddText(pol.CenaDPH.ToString("F2") + "€", fontSize, actual, fontR);                    //3

                actual = actual.Translate(85, 0);
                page.AddText(pol.DPH.ToString("F1") + "%", fontSize, actual, fontR);                     //4

                actual = actual.Translate(70, 0);
                page.AddText(pol.CelkovaCenaDPH.ToString("F2") + "€", fontSize, actual, fontR);                  //5

                globalRiadok = actRiadok = actual = globalRiadok.Translate(0, -Riad * enter);        //skocenie o riadok cez Global

                page.DrawLine(actual.Translate(-4, +Riad - 3), actual.Translate(TEx - TSx - 11, +Riad - 3));      //vykreslenie ciary
            }
            /////////////////////////////////
            page.AddText("Suma celkom", fontSize, actual.Translate(200 + 50 + 20, 0), fontR);               //vypis celkovej sumy
            
            page.AddText(CelkovaSumaDPH.ToString("F2") + "€", fontSize, actual.Translate(200 + 50 + 70 + 95, 0), fontR);
            /////////////////////////////////

            #endregion
            #region Build
            byte[] documentBytes = builder.Build();         //skonvertovanie do byte

            System.IO.File.WriteAllBytes(FullPathPDF, documentBytes);         //vysledne pdf
            #endregion
        }
    }
}
