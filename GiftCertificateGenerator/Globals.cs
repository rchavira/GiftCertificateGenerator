using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GiftCertificateGenerator
{
    public class Globals
    {
        GiftCertificates _db;

        public GiftCertificates DB { get { if(_db == null) Load(); return _db; } set { SaveChanges(); } }

        public Globals() { Load(); }

        public void Load()
        {
            _db = new GiftCertificates();
            if (System.IO.File.Exists("giftcertificates.xml"))
                _db.ReadXml("giftcertificates.xml", System.Data.XmlReadMode.Auto);
        }

        public void SaveChanges()
        {
            _db.WriteXml("giftcertificates.xml", System.Data.XmlWriteMode.WriteSchema);
        }

        public PointF getPoint(string location)
        {
            float x = float.Parse(location.Split(',')[0]);
            float y = float.Parse(location.Split(',')[1]);
            return new PointF(x, y);
        }

        public string setPoint(PointF p)
        {
            return p.X + "," + p.Y;
        }

    }

    public enum fieldTypes { GCNumber, Expiration, From, To, Amount, Memo, QR }

    public class Fields
    {
        public fieldTypes FieldType;
        public string text;
        public PointF location;
        public SizeF size;
        public bool selected;

        public bool HitTest(PointF loc)
        {
            RectangleF rect = new RectangleF(location, size);
            return rect.Contains(loc);
        }

        public void updateSize(SizeF sz)
        {
            size = sz;
        }
    }
}
