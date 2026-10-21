
using System;
using System.Globalization;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Helper;

namespace Toko_Root_ID
{
    public partial class frmPenjualan : Form
    {
        private bool sedangMemproses;

        public frmPenjualan()
        {
            InitializeComponent();
            Toko_Root_ID.Penjualan.PenjualanTemplate.Apply(this);
        }

        private void label7_Click(object sender, EventArgs e)
        {
            // Event dipertahankan untuk kompatibilitas Designer.
        }

        private void frmPenjualan_Load(object sender, EventArgs e)
        {
            persiapan();
        }

        private void persiapan()
        {
            txtTanggal.Text = DateTime.Now.ToString("dd/MM/yyyy");

            txttotalbelanja.Text = "0";
            txtDisplay.Text = "0";
            textBox11.Text = "";
            txttotalharga.Text = "";

            txtDisplay.Enabled = false;
            txtnamabarang.Enabled = false;
            txtharga.Enabled = false;
            txttotalharga.Enabled = false;
            txttotalbelanja.Enabled = false;
            textBox11.Enabled = false;

            btnSimpan.Enabled = false;
            btnPrint.Enabled = false;

            txtnofaktur.Text = CreateFakturNumber();
            txtkodebarang.Focus();
        }

        private string CreateFakturNumber()
        {
            DAPenjualan proses = new DAPenjualan();
            return proses.generatenofaktur();
        }

        private void txtkodebarang_KeyPress(
            object sender,
            KeyPressEventArgs e)
        {
            CariBarangSaatEnter(e);
        }

        private void txtkodebarang_KeyPress_1(
            object sender,
            KeyPressEventArgs e)
        {
            CariBarangSaatEnter(e);
        }

        private void CariBarangSaatEnter(KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter)
            {
                return;
            }

            e.Handled = true;

            string kode = txtkodebarang.Text.Trim();

            if (string.IsNullOrWhiteSpace(kode))
            {
                MessageBox.Show(
                    "Masukkan kode barang terlebih dahulu.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtkodebarang.Focus();
                return;
            }

            try
            {
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

                    txtkodebarang.Focus();
                    return;
                }

                txtnamabarang.Text = barang.namabarang;
                txtharga.Text = barang.hargasatuan.ToString(
                    CultureInfo.CurrentCulture);

                txtjumlahbeli.Focus();
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

        private void txtjumlahbeli_KeyPress(
            object sender,
            KeyPressEventArgs e)
        {
            HitungSubtotalSaatEnter(e);
        }

        private void txtjumlahbeli_KeyPress_1(
            object sender,
            KeyPressEventArgs e)
        {
            HitungSubtotalSaatEnter(e);
        }

        private void HitungSubtotalSaatEnter(KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter)
            {
                return;
            }

            e.Handled = true;

            decimal harga;
            int jumlah;

            if (!decimal.TryParse(
                txtharga.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out harga))
            {
                MessageBox.Show(
                    "Harga barang tidak valid.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!int.TryParse(txtjumlahbeli.Text, out jumlah) ||
                jumlah <= 0)
            {
                MessageBox.Show(
                    "Jumlah beli harus berupa bilangan bulat lebih dari 0.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtjumlahbeli.Focus();
                return;
            }

            decimal total = harga * jumlah;

            txttotalharga.Text = total.ToString(
                CultureInfo.CurrentCulture);

            btnOK.Focus();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            TambahBarangKeKeranjang();
        }

        private void btnOK_Click_1(object sender, EventArgs e)
        {
            TambahBarangKeKeranjang();
        }

        private void TambahBarangKeKeranjang()
        {
            if (string.IsNullOrWhiteSpace(txtkodebarang.Text) ||
                string.IsNullOrWhiteSpace(txtnamabarang.Text))
            {
                MessageBox.Show(
                    "Pilih barang yang valid terlebih dahulu.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtkodebarang.Focus();
                return;
            }

            decimal harga;
            decimal total;
            int jumlah;

            if (!decimal.TryParse(
                txtharga.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out harga) ||
                !int.TryParse(txtjumlahbeli.Text, out jumlah) ||
                jumlah <= 0 ||
                !decimal.TryParse(
                    txttotalharga.Text,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out total))
            {
                MessageBox.Show(
                    "Periksa kembali harga, jumlah beli, dan total harga.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            ClassTempIsiFaktur barang = new ClassTempIsiFaktur();
            barang.nofaktur = txtnofaktur.Text;
            barang.kodebarang = txtkodebarang.Text.Trim();
            barang.namabarang = txtnamabarang.Text;
            barang.hargasatuan = harga;
            barang.jumlahbeli = jumlah;
            barang.total = total;

            try
            {
                DAPenjualan da = new DAPenjualan();
                string hasil = da.insterttemp(barang);

                if (hasil != "1")
                {
                    MessageBox.Show(
                        hasil,
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                hitungTotal(total);
                fillgrid();
                clearscreen();

                btnSimpan.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal menambahkan barang ke keranjang.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void hitungTotal(decimal subtotal)
        {
            decimal totalBelanja;

            if (!decimal.TryParse(
                txttotalbelanja.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out totalBelanja))
            {
                totalBelanja = 0;
            }

            totalBelanja += subtotal;

            txttotalbelanja.Text = totalBelanja.ToString(
                CultureInfo.CurrentCulture);

            txtDisplay.Text = txttotalbelanja.Text;
        }

        private void clearscreen()
        {
            txtkodebarang.Clear();
            txtnamabarang.Clear();
            txtharga.Clear();
            txtjumlahbeli.Clear();
            txttotalharga.Clear();

            txtkodebarang.Focus();
        }

        private void fillgrid()
        {
            string nofaktur = txtnofaktur.Text;
            DAPenjualan proses = new DAPenjualan();

            dgvIsiFaktur.DataSource = proses.getisigrid(nofaktur);
            ToolBox.Grid(dgvIsiFaktur);
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            // Simpan transaksi tanpa pembayaran jika memang alur
            // aplikasi menggunakan tombol ini untuk menyimpan faktur.
            SimpanTransaksi();
        }

        private void SimpanTransaksi()
        {
            if (sedangMemproses)
            {
                return;
            }

            if (dgvIsiFaktur.Rows.Count == 0 ||
                (dgvIsiFaktur.Rows.Count == 1 &&
                 dgvIsiFaktur.Rows[0].IsNewRow))
            {
                MessageBox.Show(
                    "Keranjang masih kosong.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            sedangMemproses = true;

            try
            {
                simpanisi();
                simpanfaktur();

                MessageBox.Show(
                    "Proses penyimpanan transaksi telah dijalankan.",
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                bersihtemporary();
                bersihlayar();
                persiapan();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal menyimpan transaksi.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                sedangMemproses = false;
            }
        }

        private void simpanisi()
        {
            DAPenjualan aksi = new DAPenjualan();

            foreach (DataGridViewRow row in dgvIsiFaktur.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                ClassIsiFaktur isiFaktur = new ClassIsiFaktur();

                isiFaktur.nofaktur = txtnofaktur.Text;
                isiFaktur.kodebarang = Convert.ToString(
                    row.Cells["kodebarang"].Value);

                isiFaktur.namabarang = Convert.ToString(
                    row.Cells["namabarang"].Value);

                isiFaktur.hargasatuan = Convert.ToDecimal(
                    row.Cells["hargasatuan"].Value);

                isiFaktur.jumlah = Convert.ToInt16(
                    row.Cells["jumlahbeli"].Value);

                isiFaktur.total = Convert.ToDecimal(
                    row.Cells["total"].Value);

                string hasil = aksi.submitisifaktur(isiFaktur);

                if (hasil != "1")
                {
                    throw new InvalidOperationException(
                        "Gagal menyimpan detail faktur: " + hasil);
                }
            }
        }

        private void simpanfaktur()
        {
            DAPenjualan aksi = new DAPenjualan();

            ClassFaktur faktur = new ClassFaktur();
            faktur.nofaktur = txtnofaktur.Text;
            faktur.total = Convert.ToDecimal(txttotalbelanja.Text);

            string hasil = aksi.submitfaktur(faktur);

            if (hasil != "1")
            {
                throw new InvalidOperationException(
                    "Gagal menyimpan faktur: " + hasil);
            }
        }

        private void bersihtemporary()
        {
            DAPenjualan da = new DAPenjualan();
            string hasil = da.deletetemporary();

            if (hasil != "1")
            {
                MessageBox.Show(
                    hasil,
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void bersihlayar()
        {
            dgvIsiFaktur.DataSource = null;

            txttotalbelanja.Text = "0";
            txtDisplay.Text = "0";
            textBox10.Clear();
            textBox11.Clear();

            btnSimpan.Enabled = false;
            btnPrint.Enabled = false;

            txtkodebarang.Focus();
        }

        private void textBox10_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter)
            {
                return;
            }

            e.Handled = true;

            decimal totalBelanja;
            decimal uangBayar;

            if (!decimal.TryParse(
                txttotalbelanja.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out totalBelanja) ||
                !decimal.TryParse(
                    textBox10.Text,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out uangBayar))
            {
                MessageBox.Show(
                    "Masukkan jumlah uang pembayaran yang valid.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                textBox10.Focus();
                return;
            }

            if (uangBayar < totalBelanja)
            {
                MessageBox.Show(
                    "Uang pembayaran kurang dari total belanja.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                textBox10.Focus();
                return;
            }

            decimal kembali = uangBayar - totalBelanja;

            textBox11.Text = kembali.ToString(
                CultureInfo.CurrentCulture);
        }

        private void btnSimpan_Click_1(object sender, EventArgs e)
        {
            if (sedangMemproses)
            {
                return;
            }

            decimal uangBayar;
            decimal totalBelanja;
            decimal kembali;

            if (dgvIsiFaktur.Rows.Count == 0 ||
                (dgvIsiFaktur.Rows.Count == 1 &&
                 dgvIsiFaktur.Rows[0].IsNewRow))
            {
                MessageBox.Show(
                    "Keranjang masih kosong.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!decimal.TryParse(
                textBox10.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out uangBayar) ||
                !decimal.TryParse(
                    txttotalbelanja.Text,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out totalBelanja))
            {
                MessageBox.Show(
                    "Masukkan uang pembayaran yang valid.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                textBox10.Focus();
                return;
            }

            if (uangBayar < totalBelanja)
            {
                MessageBox.Show(
                    "Uang pembayaran tidak mencukupi.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            kembali = uangBayar - totalBelanja;
            textBox11.Text = kembali.ToString(
                CultureInfo.CurrentCulture);

            try
            {
                ClassIsiFakturJumlah pembayaran =
                    new ClassIsiFakturJumlah();

                pembayaran.nofaktur = txtnofaktur.Text;
                pembayaran.jumlahuang = uangBayar;
                pembayaran.kembali = kembali;

                DAPenjualan da = new DAPenjualan();
                string hasil = da.submitfakturjumlah(pembayaran);

                if (hasil != "1")
                {
                    MessageBox.Show(
                        hasil,
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                SimpanTransaksi();

                //Report_Transaksi report = new Report_Transaksi();
                //report.Show();

                btnPrint.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal memproses pembayaran.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Apakah Anda yakin ingin membersihkan transaksi sementara?",
                "Konfirmasi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                bersihtemporary();
                bersihlayar();
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            //Report_Transaksi report = new Report_Transaksi();
            //report.Show();
        }
    }
}
