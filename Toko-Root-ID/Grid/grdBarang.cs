using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAAccess;
using Helper;


namespace Toko_Root_ID
{
    public partial class grdBarang : Form
    {
        public grdBarang()
        {
            InitializeComponent();
            Toko_Root_ID.Grid.grdBarangTemplate.Apply(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormUtama host = Tag as FormUtama;
            if (host != null) { host.NavigateDashboard(); return; }
            FormUtama yusuf4 = new FormUtama();
            yusuf4.Show();
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormUtama host = Tag as FormUtama;
            if (host != null) { host.NavigateTambahBarang(); return; }
            frmBarang yusuf5 = new frmBarang();
            yusuf5.Show();
            Close();
        }

        private void grdBarang_Load(object sender, EventArgs e)
        {
            isigrid();
        }

        protected void isigrid()
        {
            DABarang list = new DABarang();
            dataGridView1.DataSource = list.BindList();
            dataGridView1.Columns["hargasatuan"].DefaultCellStyle.Format = "C0";
            dataGridView1.Columns["hargasatuan"].DefaultCellStyle.FormatProvider =
                new System.Globalization.CultureInfo("id-ID");
            ToolBox.Grid(dataGridView1);
        }

        private void grdBarang_Activated(object sender, EventArgs e)
        {

        }

        private void gridBarang_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void gridBarang_DoubleClick(object sender, EventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            DABarang list = new DABarang();
            dataGridView1.DataSource = list.BindList();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            Report_Barang yusuf9 = new Report_Barang();
            yusuf9.Show();
        }
    }
}
