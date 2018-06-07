/*
 * Author : Vincent Erbrich - CPNV
 * Project : TPI 2018 Exam - CateringEasy
 * Project Manager : Pascal Benzonana
 * First Expert : Alain Roy
 * Second Expert : Laurent Ruchat
 * Creation date : 22/05/2018
 */

using System.Collections.Generic;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Diagnostics;

namespace CateringEasy
{
    /*
     * This class manages the invoice generation and printing
     */ 
    class Invoicer
    {
        /*
         * This method generates a pdf file according to the parameters and saves it in the C:\CateringEasyInvoices folder.
         * It uses the PdfSharp library to draw the document graphics and save them into a pdf file.
         * The document generation code was adapted from the hello world sample on the PdfSharp.net website : http://www.pdfsharp.net/wiki/PDFsharpFirstSteps.ashx
         */
        public void NewInvoice(List<string> lines, float total, string currency, string filename)
        {
            PdfDocument pdf = new PdfDocument();
            PdfPage page = pdf.AddPage();
            XFont font = new XFont("Seorge UI Semibold", 15, XFontStyle.Regular);
            XGraphics gfx = XGraphics.FromPdfPage(page);
            int y = 0;
            foreach (string str in lines)
            {
                if (y > page.Height)
                {
                    page = pdf.AddPage();
                }
                gfx.DrawString(str + currency, font, XBrushes.Black, new XRect(20, y, page.Width, page.Height), XStringFormat.TopCenter);
                y = y + 30;
            }
            font = new XFont("Seorge UI", 26, XFontStyle.Bold);
            gfx.DrawString("Total :" + total.ToString(), font, XBrushes.Black, new XRect(20, y, page.Width, page.Height), XStringFormat.TopCenter);
            pdf.Save("C:\\CateringEasyInvoices\\" + filename + ".pdf");
            SendToPrinter("C:\\CateringEasyInvoices\\" + filename + ".pdf");
        }
        /*
         * This method prints the pdf file in the parameters on the default Windows printer using the System.Diagnostics library.
         * This exact code was found on the StackOverFlow website https://stackoverflow.com/questions/17448465/send-pdf-file-to-a-printer-print-pdf. It was written by Johan Hjalmarsson.
         */
        private void SendToPrinter(string filename)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.Verb = "print";
            info.FileName = filename;
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;

            Process p = new Process();
            p.StartInfo = info;
            p.Start();

            p.WaitForInputIdle();
            System.Threading.Thread.Sleep(3000);
            if (false == p.CloseMainWindow())
                p.Kill();
        }
    }
}
