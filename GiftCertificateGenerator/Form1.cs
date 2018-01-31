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
    public partial class Form1 : Form
    {
        Globals g;
        bool newRecord = true;

        Int64 myID = -1;

        bool dirty;

        public Form1(Globals glbl)
        {
            g = glbl;
            InitializeComponent();
            dateTimePicker1.Value = DateTime.Now.AddMonths(12);
            loadServices();
            dirty = true;
        }

        void loadServices()
        {
            comboBox1.Items.Clear();
            foreach (DataRow dr in g.DB.Tables["ServiceList"].Rows)
                comboBox1.Items.Add((string)dr["Service"]);            
        }

        public Form1(Globals glbl, Int64 gcid)
        {
            g = glbl;
            InitializeComponent();
            newRecord = false;
            myID = gcid;            
            LoadRecord();
        }

        void LoadRecord()
        {
            GiftCertificates.GiftCertificatesRow dr = (GiftCertificates.GiftCertificatesRow)g.DB._GiftCertificates.Select("GiftCertificateID=" + myID.ToString())[0];
            textBox1.Text = dr.GiftCertificateNumber;
            textBox2.Text = dr.ToText;
            checkBox1.Checked = dr.HideFrom;
            textBox3.Text = dr.FromText;
            textBox4.Text = dr.Memo;
            if (dr.ServiceAmount != "")
            {
                comboBox1.Text = dr.ServiceAmount;
                radioButton2.Checked = true;                
            }
            else
            {
                numericUpDown1.Value = dr.Amount;
                radioButton1.Checked = true;
            }
            updateAmount();
            textBox5.Text = dr.Notes;
            dateTimePicker1.Value = dr.ExpirationDate;
            dirty = false;
        }

        void updateAmount()
        {
            numericUpDown1.Enabled = radioButton1.Checked;
            comboBox1.Enabled = radioButton2.Checked;
            dirty = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) => updateAmount();

        private void radioButton2_CheckedChanged(object sender, EventArgs e) => updateAmount();

        void SaveRecord()
        {
            //save record if new
            if (newRecord)
            {
                GiftCertificates.GiftCertificatesRow dr = g.DB._GiftCertificates.AddGiftCertificatesRow(DateTime.Now, textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, numericUpDown1.Value, comboBox1.Text, checkBox1.Checked, dateTimePicker1.Value);
                myID = dr.GiftCertificateID;
                g.SaveChanges();
                newRecord = false;
            }
            else
            {
                //update existing record
                GiftCertificates.GiftCertificatesRow dr = (GiftCertificates.GiftCertificatesRow)g.DB._GiftCertificates.Select("GiftCertificateID=" + myID.ToString())[0];
                dr.GiftCertificateNumber = textBox1.Text;
                dr.ToText = textBox2.Text;
                dr.HideFrom = checkBox1.Checked;
                dr.FromText = textBox3.Text;
                dr.Memo = textBox4.Text;
                if (radioButton2.Checked)
                    dr.ServiceAmount = comboBox1.Text;                    
                else
                    dr.Amount = numericUpDown1.Value;
                dr.Notes = textBox5.Text;
                dr.ExpirationDate = dateTimePicker1.Value;
                dr.AcceptChanges();
                g.DB._GiftCertificates.AcceptChanges();
                g.DB.AcceptChanges();
                g.SaveChanges();
            }
            dirty = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveRecord();
            using (PrintCertificate pf = new PrintCertificate(g, myID))
            {
                pf.ShowDialog();
            }
        }

        private void radioButton1_Click(object sender, EventArgs e) => updateAmount();

        private void radioButton2_Click(object sender, EventArgs e) => updateAmount();

        private void button2_Click(object sender, EventArgs e)
        {
            if (dirty)
            {
                switch (MessageBox.Show("Save Changes?", "Save", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes: SaveRecord(); Close(); break;
                    case DialogResult.No: Close(); break;
                    case DialogResult.Cancel: break;
                }
            }
            else Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            dirty = true;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            dirty = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dirty = true;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dirty = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dirty = true;
        }
    }
}
