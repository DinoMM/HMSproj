using DBLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

namespace PdfCreator.Models
{
    public class ObjednavkaPDF : PdfCreator
    {
        public ObjednavkaPDF() : base()
        {
            FileName = "Objednavka.pdf";
            FullPathPDF = System.IO.Path.Combine(FolderPath, FileName);    //vysledna cesta
        }

        /// <summary>
        /// Použitá stiahnutá knižnica z NuGet -> PdfPig -> https://www.nuget.org/packages/PdfPig/#readme-body-tab
        /// + stiahnuté 2 fonty Montserrat (.ttf) (lebo diakritika) -> https://www.fontspace.com/collection/best-of-mo-peterson-design-cw3d9qd
        /// </summary>
        /// <param name="objednavka"></param>
        public void GenerujPdf(Objednavka objednavka, Dodavatel dodavatel, Dodavatel odberatel, List<PolozkaSkladuObjednavky> zoznamObjednavky)       //vygeneruje pdf z objednavky
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

            string datumVystavenia = (DateOnly.FromDateTime(objednavka.DatumVznik)).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);  // datum


            page.DrawRectangle(new PdfPoint(Sx, Sy), 535, 758);     //HlavneBoundary


            page.AddText("Objednávka", fontSize + 4, new PdfPoint(TEx / 2 - 35, TEy - 18), fontB);
            /////////////////////////////////
            page.DrawRectangle(new PdfPoint(Sx + 3, Ey - 200 - Riad - 10), Ex / 2 - 20, 200 - 3);       //prvy stvorcek DODAVATEL
            PdfPoint start = new PdfPoint(Sx + 3, Ey - 200 - Riad - 10);
            double width = Ex / 2 - 20;
            double height = 200 - 3;

            PdfPoint actual = start.Translate(5, height - fontSize - 4);        //zaciatok
            PdfPoint actRiadok = actual.Translate(0, 0);

            var endp = page.AddText("Dodávateľ: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;

            int finalEnters = 4;
            int maxEnters = WriteInBlock(in page, dodavatel.Nazov, fontSize, actual, fontR, 2, Riad, 180, 250).Item2;
            finalEnters = (finalEnters - 1) + maxEnters;
            for (int i = 0; i < 2 - maxEnters; ++i)
            {
                actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok
            }

            endp = page.AddText("IČO: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(dodavatel.ICO, fontSize, actual, fontR);

            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok

            endp = page.AddText("Adresa: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(
                WillFit(in page, dodavatel.Obec + ",", fontSize, actual, fontR, 200).Item2,
                fontSize, actual, fontR);
            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok
            maxEnters = WriteInBlock(in page, dodavatel.Adresa, fontSize, actual, fontR, finalEnters, Riad, 250, 250).Item2;


            PdfPoint globalRiadok = actRiadok.Translate(0, 0);
            /////////////////////////////////
            page.DrawRectangle(new PdfPoint(Sx + 6 + Ex / 2 - 20, Ey - 200 - Riad - 10), Ex / 2 - 20, 200 - 3);  //druhy stvorcek ODBERATEL

            start = new PdfPoint(Sx + 6 + Ex / 2 - 20, Ey - 200 - Riad - 10);
            width = Ex / 2 - 20;
            height = 200 - 3;

            actual = start.Translate(5, height - fontSize - 4);        //zaciatok
            actRiadok = actual.Translate(0, 0);

            endp = page.AddText("Odberateľ: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            finalEnters = 4;
            maxEnters = WriteInBlock(in page, odberatel.Nazov, fontSize, actual, fontR, 2, Riad, 180, 250).Item2;
            finalEnters = (finalEnters - 1) + maxEnters;
            for (int i = 0; i < 2 - maxEnters; ++i)
            {
                actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok
            }

            endp = page.AddText("IČO: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(odberatel.ICO, fontSize, actual, fontR);

            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok

            endp = page.AddText("Adresa: ", fontSize, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(
                WillFit(in page, odberatel.Obec + ",", fontSize, actual, fontR, 200).Item2,
                fontSize, actual, fontR);
            actRiadok = actual = actRiadok.Translate(0, -Riad);        //skocenie o riadok
            maxEnters = WriteInBlock(in page, odberatel.Adresa, fontSize, actual, fontR, finalEnters, Riad, 250, 250).Item2;


            /////////////////////////////////
            globalRiadok = actRiadok = actual = new PdfPoint(38, 531); //set pre datum vytvorenia       
            page.DrawRectangle(new PdfPoint(Sx + 3, Ey - 250 - Riad - 10), TEx - TSx + 2, 50 - 3);       //treti stvorcek

            endp = page.AddText($"Dátum vytvorenia:    ", fontSize + 1, actual, fontR);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(datumVystavenia, fontSize + 2, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText("               Číslo objednávky:    ", fontSize + 1, actual, fontR);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText(objednavka.ID, fontSize + 2, actual, fontB);

            globalRiadok = actRiadok = actual = globalRiadok.Translate(0, -Riad);        //skocenie o riadok cez Global
            globalRiadok = actRiadok = actual = globalRiadok.Translate(0, -Riad);        //skocenie o riadok cez Global

            ////////////////////////////////////////
            page.DrawRectangle(new PdfPoint(Sx + 3, Ey - 300 - Riad - 10), TEx - TSx + 2, 50 - 3);       //stvrty stvorcek
            endp = page.AddText($"Vytvoril: ", fontSize + 1, actual, fontR);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText($"{(objednavka.TvorcaX.Name.Length > 0 ? objednavka.TvorcaX.Name.FirstOrDefault() + "." : "")}{objednavka.TvorcaX.Surname}  ", fontSize + 1, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText($"Email: ", fontSize + 1, actual, fontR);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText($"{objednavka.TvorcaX.Email}   ", fontSize + 1, actual, fontB);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText($"Tel.: ", fontSize + 1, actual, fontR);
            actual = endp[^1].EndBaseLine;
            endp = page.AddText($"{objednavka.TvorcaX.PhoneNumber}", fontSize + 1, actual, fontB);
            globalRiadok = actRiadok = actual = globalRiadok.Translate(0, -Riad);        //skocenie o riadok cez Global
            /////////////////////////////////
            endp = page.AddText($"Odberateľ si objednáva následovný tovar/služby:", fontSize, actual, fontR);

            globalRiadok = actRiadok = actual = globalRiadok.Translate(0, -Riad);        //skocenie o riadok cez Global
            /////////////////////////////////
            page.DrawRectangle(new PdfPoint(Sx + 3, Ey - 250 - (4 * Riad) - Riad - 5), TEx - TSx + 2, 40 - 3); // piaty stvorcek

            string hlavicka = "                 Názov položky                      Množstvo      MJ      Cena za MJ     Celková cena    Celková cena s DPH";

            fontSize = fontSize - 2;

            endp = page.AddText(hlavicka, fontSize, actual, fontR);

            globalRiadok = actRiadok = actual = globalRiadok.Translate(0, -Riad);        //skocenie o riadok cez Global

            double CelkovaSuma = 0.0;
            double CelkovaSumaDPH = 0.0;
            foreach (var pol in zoznamObjednavky)
            {
                CelkovaSuma += pol.CelkovaCena;
                CelkovaSumaDPH += pol.CelkovaCenaDPH;
            }

            int enter = 1;              //pocitanie enterov
            foreach (var pol in zoznamObjednavky)
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

                actual = actual.Translate(200, 0);                      //vypis ostatnych stlpcov
                page.AddText(pol.Mnozstvo.ToString("F3"), fontSize, actual, fontR);                //2

                actual = actual.Translate(50, 0);
                page.AddText(pol.PolozkaSkladuX.MernaJednotka, fontSize, actual, fontR);                           //3

                actual = actual.Translate(50, 0);
                page.AddText(pol.Cena.ToString("F3"), fontSize, actual, fontR);                    //4

                actual = actual.Translate(70, 0);
                page.AddText(pol.CelkovaCena.ToString("F3"), fontSize, actual, fontR);                     //5

                actual = actual.Translate(90, 0);
                page.AddText(pol.CelkovaCenaDPH.ToString("F3"), fontSize, actual, fontR);                  //6

                globalRiadok = actRiadok = actual = globalRiadok.Translate(0, -Riad * enter);        //skocenie o riadok cez Global

                page.DrawLine(actual.Translate(-4, +Riad - 3), actual.Translate(TEx - TSx, +Riad - 3));      //vykreslenie ciary
            }
            /////////////////////////////////
            page.AddText("Suma celkom", fontSize, actual.Translate(200 + 50 + 20, 0), fontR);               //vypis celkovej sumy
            page.AddText(CelkovaSuma.ToString("F3"), fontSize, actual.Translate(200 + 50 + 50 + 70, 0), fontR);
            page.AddText(CelkovaSumaDPH.ToString("F3"), fontSize, actual.Translate(200 + 50 + 50 + 70 + 90, 0), fontR);
            /////////////////////////////////

            byte[] documentBytes = builder.Build();         //skonvertovanie do byte

            System.IO.File.WriteAllBytes(FullPathPDF, documentBytes);         //vysledne pdf
        }
    }
}
