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

namespace Toko_Root_ID
{
    public partial class grdUser : Form
    {
        public grdUser()
        {
            InitializeComponent();
            Toko_Root_ID.Grid.grdUserTemplate.Apply(this);
        }

        private void btnUser_Click(object sender, EventArgs e)
        {
            FormUtama host = Tag as FormUtama;
            if (host != null) { host.NavigatePengguna(); return; }
            FormUser yusuf1 = new FormUser();
            yusuf1.Show();
            Close();
        }

        private void btnKembali_Click(object sender, EventArgs e)
        {
            FormUtama host = Tag as FormUtama;
            if (host != null) { host.NavigateDashboard(); return; }
            FormUtama yusuf2 = new FormUtama();
            yusuf2.Show();
            Close();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            FormUtama host = Tag as FormUtama;
            if (host != null) { host.NavigateDaftarPengguna(); return; }
            grdUser yusuf3 = new grdUser();
            yusuf3.Show();
            Close();
        }

        private void grdUser_Load(object sender, EventArgs e)
        {
            isigrid();
        }

        private void isigrid()
        {
            DAUser list = new DAUser();
            dataGridView1.DataSource = list.BindList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Report_Barang yusuf4 = new Report_Barang();
            yusuf4.Show();
        }
    }
}
