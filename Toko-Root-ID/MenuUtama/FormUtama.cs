using System;
using System.Linq;
using System.Windows.Forms;
using DAAccess;
using Toko_Root_ID.MenuUtama;

namespace Toko_Root_ID
{
    public partial class FormUtama : Form
    {
        private readonly MenuUtamaTemplate _template;

        public FormUtama()
        {
            InitializeComponent();
            _template = new MenuUtamaTemplate(this);
            _template.Build();
            _template.ShowDashboard();
        }

        // Pengambilan data dashboard tetap berada di form; komposisi visualnya ada di template.
        internal MenuUtamaTemplate.DashboardData LoadDashboardData()
        {
            MenuUtamaTemplate.DashboardData data = new MenuUtamaTemplate.DashboardData();
            data.ItemCount = 0;
            data.TotalStock = 0;
            data.Status = "Data diperbarui dari database";
            try
            {
                var items = new DABarang().BindList();
                data.ItemCount = items.Count;
                data.TotalStock = items.Sum(x => x.stok);
            }
            catch
            {
                data.Status = "Database belum tersambung — angka monitoring belum tersedia";
            }
            return data;
        }

        // Public navigation APIs dipakai tombol-tombol pada form fitur.
        public void NavigateDashboard() { _template.ShowDashboard(); }
        public void NavigateTambahBarang() { _template.ShowEmbedded(new frmBarang(), "Tambah Barang"); }
        public void NavigateDataBarang() { _template.ShowEmbedded(new grdBarang(), "Data Barang"); }
        public void NavigatePenjualan() { _template.ShowEmbedded(new frmPenjualan(), "Penjualan"); }
        public void NavigatePengguna() { _template.ShowEmbedded(new FormUser(), "Pengguna"); }
        public void NavigateDaftarPengguna() { _template.ShowEmbedded(new grdUser(), "Daftar Pengguna"); }

        // =========================================================
        // ROLE-BASED MENU CONTROL
        // =========================================================
        internal void EnableAllMenus()
        {
            // Owner/Admin - all access
            this.button1.Enabled = true; // Tambah Barang
            this.button2.Enabled = true; // Data Barang
            this.button3.Enabled = true; // Penjualan
            this.btnRegistrasi.Enabled = true; // Pengguna
        }

        internal void EnableManagerMenus()
        {
            // Manager - all access except user management
            this.button1.Enabled = true; // Tambah Barang
            this.button2.Enabled = true; // Data Barang
            this.button3.Enabled = true; // Penjualan
            this.btnRegistrasi.Enabled = false; // Pengguna - disabled for Manager
        }

        internal void EnableKasirMenus()
        {
            // Kasir - only POS
            this.button1.Enabled = false; // Tambah Barang
            this.button2.Enabled = false; // Data Barang
            this.button3.Enabled = true; // Penjualan (POS)
            this.btnRegistrasi.Enabled = false; // Pengguna
        }

        internal void EnableGudangMenus()
        {
            // Gudang - only inventory
            this.button1.Enabled = true; // Tambah Barang (inventory input)
            this.button2.Enabled = true; // Data Barang (stok view)
            this.button3.Enabled = false; // Penjualan
            this.btnRegistrasi.Enabled = false; // Pengguna
        }

        // =========================================================
        // BUTTON EVENT HANDLERS
        // =========================================================
        private void button1_Click(object sender, EventArgs e) { NavigateTambahBarang(); }
        private void button2_Click(object sender, EventArgs e) { NavigateDataBarang(); }
        private void button3_Click(object sender, EventArgs e) { NavigatePenjualan(); }
        private void btnRegistrasi_Click(object sender, EventArgs e) { NavigatePengguna(); }
    }
}
