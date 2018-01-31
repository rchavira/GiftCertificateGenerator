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
    public partial class MainMenu : Form
    {
        Globals g = new Globals();

        public MainMenu()
        {
            InitializeComponent();
            LoadCertificates();
        }

        void LoadCertificates()
        {
            listView1.Items.Clear();
            foreach (DataRow dr in g.DB.Tables["GiftCertificates"].Rows)
            {
                ListViewItem li = new ListViewItem();
                li.Text = ((DateTime)dr["DateAdded"]).ToShortDateString(); //0
                li.SubItems.Add((string)dr["FromText"]);  //1
                if (dr["Amount"] != DBNull.Value)
                    li.SubItems.Add(((decimal)dr["Amount"]).ToString("$0.00"));  //2
                else
                    li.SubItems.Add((string)dr["ServiceAmount"]);  //2

                li.SubItems.Add(((Int64)dr["GiftCertificateID"]).ToString());  //3
                listView1.Items.Insert(0, li);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (Form1 f = new Form1(g))
            {
                Hide();
                f.ShowDialog();
                LoadCertificates();
                Show();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (Configuration cf = new Configuration(g))
            {
                Hide();
                cf.ShowDialog();
                Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                using (Form1 f = new Form1(g, long.Parse(listView1.SelectedItems[0].SubItems[3].Text)))
                {
                    Hide();
                    f.ShowDialog();
                    LoadCertificates();
                    Show();
                }
            }
        }
    }
}
