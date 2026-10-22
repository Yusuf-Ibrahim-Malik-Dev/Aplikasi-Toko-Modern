using System;
using System.Data;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Helper;
using Toko_Root_ID.Inventory;
using Toko_Root_ID.Keuangan;
using Toko_Root_ID.Laporan;
using Toko_Root_ID.Master;
using Toko_Root_ID.MenuUtama;
using Toko_Root_ID.Pembelian;
using Toko_Root_ID.Pengaturan;
using Toko_Root_ID.Penjualan;
using Toko_Root_ID.Retur;

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

        // =========================================================
        // DATA DASHBOARD
        // =========================================================
        internal MenuUtamaTemplate.DashboardData LoadDashboardData()
        {
            var data = new MenuUtamaTemplate.DashboardData();
            try
            {
                var laporan = new DALaporan();
                data.Summary = laporan.GetDashboard();
                data.Sales7 = laporan.GetPenjualan7Hari();

                DataTable top = laporan.GetProdukTerlaris(5);
                foreach (DataRow row in top.Rows)
                {
                    data.TopProducts.Add(new ClassChartPoint(
                        Convert.ToString(row["namabarang"]), Convert.ToDecimal(row["qty_terjual"])));
                }

                data.Status = "Diperbarui " + DateTime.Now.ToString("HH:mm") + " WIB";
            }
            catch
            {
                data.Error = true;
                data.Status = "Database belum tersambung — monitoring belum tersedia";
            }
            return data;
        }

        // =========================================================
        // NAVIGASI MODUL
        // =========================================================
        public void NavigateDashboard() { _template.ShowDashboard(); }

        public void NavigatePOS() { if (BolehBuka("Kasir (POS)")) _template.ShowEmbedded(new frmPOS(), "Kasir (POS)"); }
        public void NavigatePembelian() { if (BolehBuka("Pembelian")) _template.ShowEmbedded(new frmPembelian(), "Pembelian"); }
        public void NavigateRetur() { if (BolehBuka("Retur Barang")) _template.ShowEmbedded(new frmRetur(), "Retur Barang"); }
        public void NavigateInventory() { if (BolehBuka("Inventory")) _template.ShowEmbedded(new frmInventory(), "Inventory"); }
        public void NavigateMaster() { if (BolehBuka("Master Data")) _template.ShowEmbedded(new frmMaster(), "Master Data"); }
        public void NavigateKeuangan() { if (BolehBuka("Keuangan")) _template.ShowEmbedded(new frmKeuangan(), "Keuangan"); }
        public void NavigateLaporan() { if (BolehBuka("Laporan dan Grafik")) _template.ShowEmbedded(new frmLaporan(), "Laporan dan Grafik"); }
        public void NavigatePengaturan() { if (BolehBuka("Pengaturan Toko")) _template.ShowEmbedded(new frmPengaturan(), "Pengaturan Toko"); }

        // Menu lama tetap tersedia untuk kompatibilitas.
        public void NavigateTambahBarang() { _template.ShowEmbedded(new frmBarang(), "Tambah Barang"); }
        public void NavigateDataBarang() { _template.ShowEmbedded(new grdBarang(), "Data Barang"); }
        public void NavigatePenjualan() { _template.ShowEmbedded(new frmPenjualan(), "Penjualan Klasik"); }
        public void NavigatePengguna() { if (BolehBuka("Pengguna")) _template.ShowEmbedded(new FormUser(), "Pengguna"); }
        public void NavigateDaftarPengguna() { _template.ShowEmbedded(new grdUser(), "Pengguna"); }

        /// <summary>Dipanggil form pengaturan setelah tema diubah agar seluruh UI dibangun ulang.</summary>
        public void RebuildAndOpenPengaturan()
        {
            // Bangun ulang setelah event klik selesai agar form pemanggil tidak
            // dibuang di tengah handler-nya sendiri.
            if (!IsHandleCreated)
            {
                RebuildPengaturanSekarang();
                return;
            }
            BeginInvoke(new Action(RebuildPengaturanSekarang));
        }

        private void RebuildPengaturanSekarang()
        {
            var ukuran = Size;
            _template.Build();
            if (WindowState == FormWindowState.Normal) Size = ukuran;
            ApplyCurrentRole();
            _template.ShowEmbedded(new frmPengaturan(), "Pengaturan Toko");
        }

        private bool BolehBuka(string menu)
        {
            if (_template.CanOpen(menu)) return true;
            MessageBox.Show(this, "Menu " + menu + " tidak tersedia untuk role Anda.",
                "Akses dibatasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        // =========================================================
        // ROLE-BASED MENU CONTROL
        // =========================================================
        private void ApplyCurrentRole()
        {
            if (SessionManager.IsOwner() || SessionManager.IsAdmin()) _template.ApplyRole("Owner");
            else if (SessionManager.IsManager()) _template.ApplyRole("Manager");
            else if (SessionManager.IsKasir()) _template.ApplyRole("Kasir");
            else if (SessionManager.IsGudang()) _template.ApplyRole("Gudang");
        }

        internal void EnableAllMenus()
        {
            _template.ApplyRole("Owner");
        }

        internal void EnableManagerMenus()
        {
            _template.ApplyRole("Manager");
        }

        internal void EnableKasirMenus()
        {
            _template.ApplyRole("Kasir");
        }

        internal void EnableGudangMenus()
        {
            _template.ApplyRole("Gudang");
        }

        // =========================================================
        // EVENT MENU LAMA
        // =========================================================
        private void button1_Click(object sender, EventArgs e) { NavigateTambahBarang(); }
        private void button2_Click(object sender, EventArgs e) { NavigateDataBarang(); }
        private void button3_Click(object sender, EventArgs e) { NavigatePenjualan(); }
        private void btnRegistrasi_Click(object sender, EventArgs e) { NavigatePengguna(); }
    }
}
