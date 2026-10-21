
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Helper;

namespace Toko_Root_ID
{
    public partial class frmBarang : Form
    {
        public ClassBarang obj = new ClassBarang();
        public string status;

        public frmBarang()
        {
            InitializeComponent();
            Toko_Root_ID.Barang.BarangTemplate.Apply(this);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FormUtama host = Tag as FormUtama;
            if (host != null) { host.NavigateDashboard(); return; }
            FormUtama form = new FormUtama();
            form.Show();
            Close();
        }

        private void frmBarang_Activated(object sender, EventArgs e)
        {
            if (status == "update")
            {
                bindObjectToForm(obj);
                txtKodeBarang.Enabled = false;
            }
            else if (status == "new")
            {
                txtKodeBarang.Enabled = true;
                txtKodeBarang.Focus();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SimpanBarang("Barang berhasil disimpan.");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Event lama dipertahankan.
            // Proses update menggunakan btnUpdate_Click_1.
        }

        private void btnUpdate_Click_1(object sender, EventArgs e)
        {
            SimpanBarang("Barang berhasil diperbarui.");
        }

        private void SimpanBarang(string pesanSukses)
        {
            ClassBarang barang = bindFormToObject();

            if (barang == null)
            {
                return;
            }

            try
            {
                DABarang proses = new DABarang();
                string hasil = proses.submitBarang(barang);

                MessageBox.Show(
                    string.IsNullOrWhiteSpace(hasil) ? pesanSukses : hasil,
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal menyimpan data barang.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private ClassBarang bindFormToObject()
        {
            decimal harga;

            if (string.IsNullOrWhiteSpace(txtKodeBarang.Text))
            {
                MessageBox.Show(
                    "Kode barang wajib diisi.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtKodeBarang.Focus();
                return null;
            }

            if (string.IsNullOrWhiteSpace(txtNamaBarang.Text))
            {
                MessageBox.Show(
                    "Nama barang wajib diisi.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtNamaBarang.Focus();
                return null;
            }

            if (!decimal.TryParse(
                txtHargaSatuan.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out harga))
            {
                // Mencoba format angka dengan titik sebagai desimal.
                if (!decimal.TryParse(
                    txtHargaSatuan.Text,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out harga))
                {
                    MessageBox.Show(
                        "Harga satuan harus berupa angka yang valid.",
                        "Validasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtHargaSatuan.Focus();
                    return null;
                }
            }

            ClassBarang barang = new ClassBarang();
            barang.kodebarang = txtKodeBarang.Text.Trim();
            barang.namabarang = txtNamaBarang.Text.Trim();
            barang.satuan = txtSatuan.Text.Trim();
            barang.hargasatuan = harga;
            barang.stok = Convert.ToDecimal(txtStok.Text.Trim());

            return barang;
        }

        protected void bindObjectToForm(ClassBarang barang)
        {
            if (barang == null)
            {
                return;
            }

            txtKodeBarang.Text = barang.kodebarang;
            txtNamaBarang.Text = barang.namabarang;
            txtSatuan.Text = barang.satuan;
            txtHargaSatuan.Text = barang.hargasatuan.ToString(
                CultureInfo.CurrentCulture);
        }

        private void txtKodeBarang_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter)
            {
                return;
            }

            e.Handled = true;

            try
            {
                string kode = txtKodeBarang.Text.Trim();
                DAPenjualan da = new DAPenjualan();

                ClassBarang barang = da.getdatabarangbykode(kode);

                if (barang == null ||
                    string.IsNullOrWhiteSpace(barang.kodebarang))
                {
                    MessageBox.Show(
                        "Kode barang belum terdaftar!",
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    txtKodeBarang.Focus();
                    return;
                }

                txtNamaBarang.Text = barang.namabarang;
                txtHargaSatuan.Text = barang.hargasatuan.ToString(
                    CultureInfo.CurrentCulture);
                txtSatuan.Text = barang.satuan;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal mengambil data barang.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Apakah Anda yakin ingin keluar dari aplikasi Root-ID-1994?",
                "Konfirmasi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string kode = txtKodeBarang.Text.Trim();

            if (string.IsNullOrWhiteSpace(kode))
            {
                MessageBox.Show(
                    "Masukkan kode barang terlebih dahulu.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtKodeBarang.Focus();
                return;
            }

            DialogResult konfirmasi = MessageBox.Show(
                "Apakah Anda yakin ingin menghapus barang ini?",
                "Konfirmasi Hapus",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (konfirmasi != DialogResult.Yes)
            {
                return;
            }

            try
            {
                DABarang da = new DABarang();
                ClassBarang barang = da.getdeletebarang(kode);

                if (barang == null ||
                    string.IsNullOrWhiteSpace(barang.kodebarang))
                {
                    MessageBox.Show(
                        "Kode barang belum terdaftar atau barang tidak ditemukan.",
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    txtKodeBarang.Focus();
                    return;
                }

                MessageBox.Show(
                    "Barang berhasil diproses.",
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                bindObjectToForm(barang);
                txtKodeBarang.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal menghapus barang.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            FormUtama host = Tag as FormUtama;
            if (host != null) { host.NavigateDataBarang(); return; }
            grdBarang form = new grdBarang();
            form.Show();
            Close();
        }

        private void frmBarang_Load(object sender, EventArgs e)
        {
            if (txtSatuan.Items.Count == 0)
            {
                txtSatuan.Items.Add("pcs");
                txtSatuan.Items.Add("set");
                txtSatuan.Items.Add("unit");
                txtSatuan.Items.Add("liter");
                txtSatuan.Items.Add("kilo");
                txtSatuan.Items.Add("gram");
            }
        }
    }
}
