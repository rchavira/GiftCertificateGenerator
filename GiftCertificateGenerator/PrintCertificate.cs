using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.QrCodeNet;

namespace GiftCertificateGenerator
{
    public partial class PrintCertificate : Form
    {
        Int64 id;
        DateTime dAdded;
        string gcNum;
        string to;
        string from;
        string memo;
        string amt;
        DateTime exp;

        PointF GCNumL;
        PointF ExpL;
        PointF FromL;
        PointF ToL;
        PointF AmtL;
        PointF MemoL;
        PointF QRL;
        bool printQR;
        double FontSize;
        SizeF docSize;
        
        public PrintCertificate(Globals g, Int64 gcid)
        {
            id = gcid;
            InitializeComponent();
            GiftCertificates.GiftCertificatesRow dr = (GiftCertificates.GiftCertificatesRow)  g.DB._GiftCertificates.Select("GiftCertificateID=" + id.ToString())[0];
            dAdded = dr.DateAdded;
            gcNum = dr.GiftCertificateNumber;
            to = dr.ToText;
            if (!dr.HideFrom)
                from = dr.FromText;
            else
                from = "";
            memo = dr.Memo;
            if (dr.IsServiceAmountNull() || dr.ServiceAmount == "")
                amt = dr.Amount.ToString("$0.00");
            else
                amt = dr.ServiceAmount;
            exp = dr.ExpirationDate;

            if (g.DB.CertificateConfig.Rows.Count > 0)
            {
                GiftCertificates.CertificateConfigRow cr = (GiftCertificates.CertificateConfigRow) g.DB.CertificateConfig.Select()[0];

                GCNumL = g.getPoint(cr.GCLocation);
                ExpL = g.getPoint(cr.ExpirationLocation);
                FromL = g.getPoint(cr.FromLocation);
                ToL = g.getPoint(cr.ToLocation);
                AmtL = g.getPoint(cr.AmountLocation);
                MemoL = g.getPoint(cr.MemoLocation);
                QRL = g.getPoint(cr.QRLocation);
                printQR = cr.PrintQR;
                FontSize = cr.FontSize;
                docSize = new SizeF(g.getPoint(cr.DocSize));

                printDocument1.DefaultPageSettings.Landscape = true;
                printDocument1.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("GiftCertificate", (int)docSize.Height, (int)docSize.Width);                
                printPreviewControl1.Document = printDocument1;
            }
            foreach (string s in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                comboBox1.Items.Add(s);

            if(Properties.Settings.Default.DefaultPrinter != "")
                if (comboBox1.Items.Contains(Properties.Settings.Default.DefaultPrinter))
                    comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Properties.Settings.Default.DefaultPrinter);
        }
        
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Font fnt = new Font("Arial", (float)FontSize, FontStyle.Regular);
            e.Graphics.DrawString(gcNum, fnt, Brushes.Black, GCNumL);
            e.Graphics.DrawString(exp.ToShortDateString(), fnt, Brushes.Black, ExpL);
            e.Graphics.DrawString(from, fnt, Brushes.Black, FromL);
            e.Graphics.DrawString(to, fnt, Brushes.Black, ToL);
            e.Graphics.DrawString(amt, fnt, Brushes.Black, AmtL);
            e.Graphics.DrawString(memo, fnt, Brushes.Black, MemoL);
            if (printQR)
            {
                //draw qr... for later
                Gma.QrCodeNet.Encoding.QrEncoder qrenc = new Gma.QrCodeNet.Encoding.QrEncoder();
                Gma.QrCodeNet.Encoding.QrCode qr = qrenc.Encode(GCNumL + "|" + id.ToString() + "|" + exp.ToShortDateString());
                Bitmap bmp = new Bitmap(qr.Matrix.Width, qr.Matrix.Height);
                for(int x = 0; x < qr.Matrix.Width; x++)
                    for(int y = 0; y < qr.Matrix.Height; y++)
                        bmp.SetPixel(x, y, qr.Matrix.InternalArray[x, y] ? Color.Black : Color.White);

                //e.Graphics.DrawString(gcNum, fnt, Brushes.Black, QRL);
                e.Graphics.DrawImage(bmp, new RectangleF(QRL, new Size(bmp.Width * 2, bmp.Height * 2)));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex > -1)
            {
                printDocument1.PrinterSettings.PrinterName = (string)comboBox1.SelectedItem;
                printDocument1.Print();

                //save default printer for this pc
                Properties.Settings.Default.DefaultPrinter = printDocument1.PrinterSettings.PrinterName;
                Properties.Settings.Default.Save();
                Close();
            }
        }

        private void button1_Enter(object sender, EventArgs e)
        {
            label1.Visible = true;
        }

        private void button1_Leave(object sender, EventArgs e)
        {
            label1.Visible = false;
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            label1.Visible = true;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            label1.Visible = false;
        }
    }
}
