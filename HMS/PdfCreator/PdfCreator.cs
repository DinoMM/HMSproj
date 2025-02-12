using DBLayer.Models;
using System.Text;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

namespace PdfCreator
{


    public abstract class PdfCreator
    {
        /// <summary>
        /// Celá cesta k súboru PDF
        /// </summary>
        public string FullPathPDF { get; set; } = "";
        /// <summary>
        /// Celá cesta k priečinku, kde sa nachádza súbor PDF
        /// </summary>
        public string FolderPath { get; set; } = "";
        /// <summary>
        /// Názov priečinku, kde sa nachádza súbor PDF
        /// </summary>
        public string FolderName { get; set; } = "";
        /// <summary>
        /// Názov súboru PDF
        /// </summary>
        public string FileName { get; set; } = "";



        public PdfCreator()
        {
            FolderName = "PDFfiles";
            FolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName);
            System.IO.Directory.CreateDirectory(FolderPath); // vytvori adresar ak neexistuje
        }
        /// <summary>
        /// Otvorí PDF súbor
        /// </summary>
        public void OpenPDF()
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = FullPathPDF,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Skontroluje, či sa text zmestí na riadok, vracia text, ktorý sa zmestí a text, ktorý sa nezmestí.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="textToWrote"></param>
        /// <param name="fontSize"></param>
        /// <param name="tmp"></param>
        /// <param name="fontR"></param>
        /// <param name="maxDistance">Hranica, po ktorú chceme aby sa text zmestil.</param>
        /// <returns> <para>(true - vložený text sa zmestí, string bude "", string bude "") </para><para> (false - vložený text sa nezmestí, string bude obsahovať tú časť čo sa zmestila, string bude obsahovať tú časť čo sa nezmestila)</para></returns>
        protected (bool, string, string) WillFit(in PdfPageBuilder page, string textToWrote, int fontSize, PdfPoint point, PdfDocumentBuilder.AddedFont fontR, int maxDistance)
        {
            PdfPoint tmp = point.Translate(0, 0);
            var velkostTextu = page.MeasureText(textToWrote, fontSize, tmp, fontR);
            var textNaRiadku = velkostTextu.TakeWhile(d => d.EndBaseLine.X <= tmp.X + maxDistance);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var s in textNaRiadku)
            {
                stringBuilder.Append(s.Value);
            }

            textToWrote = textToWrote.Remove(0, stringBuilder.ToString().Length);

            return (textToWrote == "", stringBuilder.ToString(), textToWrote);
        }

        /// <summary>
        /// Vypíše blok textu, ktorý sa má zmestiť na vyhradenú časť.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="textToWrote"></param>
        /// <param name="fontSize"></param>
        /// <param name="point">Bod, od ktorého sa bude písat</param>
        /// <param name="fontR"></param>
        /// <param name="maxRows">Maximálny počet riadkov, ktoré sa majú vypísať</param>
        /// <param name="enterSpacing">Veľkost medzi enterami</param>
        /// <param name="MaxDistance1">Prvá maximálna vzdialenosť v prvom riadku</param>
        /// <param name="MaxDistance2">Maximálna vzdialenosť pre ostatné riadky, ktoré následujú po prvom riadku.</param>
        /// <returns><para>(true zmestilo sa do bloku, int pocet riadkov ktoré ešte zostávajú z MaxRows, string text ktorý sa nezmestil do bloku )</para></returns>
        protected (bool, int, string) WriteInBlock(in PdfPageBuilder page, string textToWrote, int fontSize, PdfPoint point, PdfDocumentBuilder.AddedFont fontR, int maxRows, int enterSpacing, int MaxDistance1, int MaxDistance2)
        {
            PdfPoint tmp = point.Translate(0, 0);
            var res = WillFit(in page, textToWrote, fontSize, tmp, fontR, MaxDistance1);
            bool isFit = res.Item1;
            string leftOver = "";
            bool fliped = false;
            do
            {
                page.AddText(res.Item2, fontSize, tmp, fontR);
                if (fliped)
                {
                    tmp = tmp.Translate(0, -enterSpacing);        //skocenie o riadok
                }
                else
                {
                    tmp = tmp.Translate(-(MaxDistance2 - MaxDistance1), -enterSpacing);        //skocenie o riadok a trochu vzad
                    fliped = true;
                }

                isFit = res.Item1;
                leftOver = res.Item3;
                res = WillFit(in page, res.Item3, fontSize, tmp, fontR, MaxDistance2);
                --maxRows;
            } while (maxRows > 0 && !isFit);
            return (isFit, maxRows, leftOver);
        }

        protected (double, (double, string)?) MaxLength<T>(IList<T> list, (string Header, Func<T, string> CellValue) setting, in PdfPageBuilder page, int fontSize, PdfPoint point, PdfDocumentBuilder.AddedFont fontR)
        {
            PdfPoint tmp = point.Translate(0, 0);
            var velkostTextu = page.MeasureText(setting.Header, fontSize, tmp, fontR);
            double maxHeader = velkostTextu?.LastOrDefault()?.EndBaseLine.X ?? 0.0;
            maxHeader = maxHeader != 0.0 ? maxHeader - tmp.X : 0.0;
            double max = 0.0;
            foreach (var item in list)
            {
                var max2 = page.MeasureText(setting.CellValue(item), fontSize, tmp, fontR)?.LastOrDefault()?.EndBaseLine.X ?? 0.0;
                max2 = max2 != 0.0 ? max2 - tmp.X : 0.0;
                if (max2 > max)
                {
                    max = max2;
                }
                if (list.IndexOf(item) == list.Count - 1)
                {
                    max += fontSize;
                }
            }
            if (maxHeader > max + 30)       //odchylka 30
            {
                var textNaRiadku = page.MeasureText(setting.Header, fontSize, tmp, fontR).TakeWhile(d => d.EndBaseLine.X <= tmp.X + max + 30);

                StringBuilder stringBuilder = new StringBuilder();
                foreach (var s in textNaRiadku)
                {
                    stringBuilder.Append(s.Value);
                }
                return (maxHeader, (max + 30, stringBuilder.ToString()));
            }
            else
            {
                return (max, null);
            }


        }

    }
}
