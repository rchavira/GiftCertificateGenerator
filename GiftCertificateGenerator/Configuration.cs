using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GiftCertificateGenerator
{
    public partial class Configuration : Form
    {
        Globals g;

        bool printQR;
        double FontSize;
        SizeF docSize;
        SizeF offset;

        List<Fields> fieldList;

        bool initialized = false;

        public Configuration(Globals glbl)
        {
            g = glbl;
            InitializeComponent();
            fieldList = new List<Fields>();

            if (g.DB.CertificateConfig.Rows.Count > 0)
            {
                GiftCertificates.CertificateConfigRow dr = (GiftCertificates.CertificateConfigRow) g.DB.CertificateConfig.Select()[0];
                fieldList.Add(new Fields() { FieldType = fieldTypes.GCNumber, location = g.getPoint(dr.GCLocation), selected = false, text = "GC###" });
                fieldList.Add(new Fields() { FieldType = fieldTypes.Expiration, location = g.getPoint(dr.ExpirationLocation), selected = false, text = DateTime.Now.AddMonths(12).ToShortDateString() });
                fieldList.Add(new Fields() { FieldType = fieldTypes.From, location = g.getPoint(dr.FromLocation), selected = false, text = "GIFT FROM ..." });
                fieldList.Add(new Fields() { FieldType = fieldTypes.To, location = g.getPoint(dr.ToLocation), selected = false, text = "GIVEN TO ..." });
                fieldList.Add(new Fields() { FieldType = fieldTypes.Amount, location = g.getPoint(dr.AmountLocation), selected = false, text = "$$$.00" });
                fieldList.Add(new Fields() { FieldType = fieldTypes.Memo, location = g.getPoint(dr.MemoLocation), selected = false, text = "MEMO NOTES ..." });
                fieldList.Add(new Fields() { FieldType = fieldTypes.QR, location = g.getPoint(dr.QRLocation), selected = false, text = "QRID | EXPDATE | GCNUMBER" });
                printQR = dr.PrintQR;
                FontSize = dr.FontSize;
                docSize = new SizeF(g.getPoint(dr.DocSize));
            }
            else
            {
                fieldList.Add(new Fields() { FieldType = fieldTypes.GCNumber, location = g.getPoint("800,300"), selected = false, text = "GC###" });
                fieldList.Add(new Fields() { FieldType = fieldTypes.Expiration, location = g.getPoint("700,310"), selected = false, text = DateTime.Now.AddMonths(12).ToShortDateString() });
                fieldList.Add(new Fields() { FieldType = fieldTypes.From, location = g.getPoint("650,320"), selected = false, text = "GIFT FROM ..." });
                fieldList.Add(new Fields() { FieldType = fieldTypes.To, location = g.getPoint("650,330"), selected = false, text = "GIVEN TO ..." });
                fieldList.Add(new Fields() { FieldType = fieldTypes.Amount, location = g.getPoint("675,340"), selected = false, text = "$$$.00" });
                fieldList.Add(new Fields() { FieldType = fieldTypes.Memo, location = g.getPoint("200,310"), selected = false, text = "MEMO NOTES ..." });
                fieldList.Add(new Fields() { FieldType = fieldTypes.QR, location = g.getPoint("200,100"), selected = false, text = "QRID | EXPDATE | GCNUMBER" });
                printQR = false;
                FontSize = 8;
                docSize = new SizeF(g.getPoint("1000,500"));
            }
            updateSize();
            checkBox1.Checked = printQR;
            numericUpDown1.Value = (decimal) FontSize;
            pictureBox1.Size = new Size((int)docSize.Width, (int)docSize.Height);
            initialized = true;
            pictureBox1.Refresh();
        }

        void updateSize()
        {
            Width = (int)docSize.Width;
            Height = (int)docSize.Height + 63;
            numericUpDown2.Value = (decimal)docSize.Width;
            numericUpDown3.Value = (decimal)docSize.Height;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (initialized)
            {
                e.Graphics.Clear(Color.White);
                Font fnt = new Font("Arial", (float)FontSize, FontStyle.Regular);

                foreach (Fields f in fieldList)
                {
                    if (f.FieldType != fieldTypes.QR)
                    {
                        f.updateSize(e.Graphics.MeasureString(f.text, fnt));
                        e.Graphics.DrawString(f.text, fnt, (f.selected ? Brushes.Blue : Brushes.Black), f.location);
                    }
                }

                if (printQR)
                {
                    foreach (Fields f in fieldList)
                    {
                        if (f.FieldType == fieldTypes.QR)
                        {
                            Gma.QrCodeNet.Encoding.QrEncoder qrenc = new Gma.QrCodeNet.Encoding.QrEncoder();
                            Gma.QrCodeNet.Encoding.QrCode qr = qrenc.Encode(f.text);
                            Bitmap bmp = new Bitmap(qr.Matrix.Width, qr.Matrix.Height);
                            for (int x = 0; x < qr.Matrix.Width; x++)
                                for (int y = 0; y < qr.Matrix.Height; y++)
                                    bmp.SetPixel(x, y, qr.Matrix.InternalArray[x, y] ? (f.selected ? Color.Blue : Color.Black) : Color.White);
                           
                            f.updateSize(new SizeF(bmp.Width * 2, bmp.Height * 2));
                            e.Graphics.DrawImage(bmp, new RectangleF(f.location, f.size));
                        }
                    }

                }
            }
        }

        string getLocation(fieldTypes ftype)
        {
            string result = "";
            foreach (Fields f in fieldList)
            {
                if (ftype == f.FieldType)
                    result = g.setPoint(f.location);
            }
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            g.DB.CertificateConfig.Rows.Clear();
            g.DB.CertificateConfig.Clear();

            while (g.DB.CertificateConfig.Rows.Count > 0)
                g.DB.CertificateConfig.Rows.RemoveAt(0);

            g.DB.CertificateConfig.AddCertificateConfigRow(getLocation(fieldTypes.GCNumber), getLocation(fieldTypes.Expiration), getLocation(fieldTypes.From), getLocation(fieldTypes.To), getLocation(fieldTypes.Amount), getLocation(fieldTypes.Memo),printQR, getLocation(fieldTypes.QR),FontSize,docSize.Width + "," + docSize.Height);
            g.DB.CertificateConfig.AcceptChanges();
            g.DB.AcceptChanges();
            g.SaveChanges();
            Close();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (Fields f in fieldList)
            {
                f.selected = false;
                if (f.HitTest(e.Location))
                {
                    offset = new SizeF(f.location.X - e.Location.X , f.location.Y - e.Location.Y);
                    f.selected = true;
                }
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            foreach(Fields f in fieldList)
            {
                if (f.selected)
                {
                    f.location = PointF.Add(e.Location, offset);
                    f.selected = false;
                }
            }
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            foreach (Fields f in fieldList)
            {
                if (f.selected)
                {
                    f.location = PointF.Add(e.Location, offset);                    
                }
            }
            pictureBox1.Refresh();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            printQR = checkBox1.Checked;
            pictureBox1.Refresh();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            FontSize = (double)numericUpDown1.Value;
            pictureBox1.Refresh();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            docSize.Width = (int)numericUpDown2.Value;
            updateSize();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            docSize.Height = (int)numericUpDown3.Value;
            updateSize();
        }
    }
}
