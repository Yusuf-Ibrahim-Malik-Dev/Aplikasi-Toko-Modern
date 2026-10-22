using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Helper;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Penjualan
{
    /// <summary>
    /// Layar kasir modern. Semua komposisi visual berada di POSTemplate,
    /// form ini hanya menangani data produk, keranjang, pembayaran, dan struk.
    /// </summary>
    public class frmPOS : Form
    {
        private readonly POSTemplate.PosView _view;
        private readonly DAPenjualan _penjualan = new DAPenjualan();
        private readonly DAMaster _master = new DAMaster();

        private DataTable _cart;
        private DataTable _products;
        private DataTable _lastDetail;
        private Decimal _lastSubtotal;
        private Decimal _lastDiskon;
        private Decimal _lastTotal;
        private Decimal _lastDibayar;
        private Decimal _lastKembali;
        private string _lastNomor;
        private string _kasir;
        private bool _placeholder = true;
        private bool _sedangSimpan;

        public frmPOS()
        {
            _kasir = AmbilNamaKasir();

            // Template membangun seluruh kontrol dan mengembalikan referensinya.
            _view = POSTemplate.Build(this, _kasir);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;

            SiapkanKeranjang();
            SiapkanGridProduk();
            PasangEvent();
            MuatPelanggan();
            MuatProduk(string.Empty);
        }

        // =========================================================
        // PERSIAPAN
        // =========================================================
        private static string AmbilNamaKasir()
        {
            try
            {
                if (SessionManager.IsLoggedIn && SessionManager.CurrentUser != null)
                {
                    string nama = SessionManager.CurrentUser.nama;
                    if (!string.IsNullOrWhiteSpace(nama)) return nama;
                    return SessionManager.CurrentUser.nik;
                }
            }
            catch { }
            return Environment.UserName;
        }

        private void SiapkanKeranjang()
        {
            _cart = new DataTable("Keranjang");
            _cart.Columns.Add("kodebarang", typeof(string));
            _cart.Columns.Add("namabarang", typeof(string));
            _cart.Columns.Add("hargasatuan", typeof(decimal));
            _cart.Columns.Add("qty", typeof(decimal));
            _cart.Columns.Add("diskon", typeof(decimal));
            _cart.Columns.Add("subtotal", typeof(decimal));

            _view.CartGrid.AutoGenerateColumns = false;
            Bind("kodebarang", "kodebarang");
            Bind("namabarang", "namabarang");
            Bind("hargasatuan", "hargasatuan");
            Bind("qty", "qty");
            Bind("diskon", "diskon");
            Bind("subtotal", "subtotal");
            _view.CartGrid.DataSource = _cart;

            HitungTotal();
        }

        private void Bind(string columnName, string property)
        {
            if (_view.CartGrid.Columns[columnName] != null)
                _view.CartGrid.Columns[columnName].DataPropertyName = property;
        }

        private void SiapkanGridProduk()
        {
            DataGridView grid = _view.ProductGrid;
            grid.AutoGenerateColumns = false;
            grid.Columns.Clear();
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "kodebarang", HeaderText = "Kode", DataPropertyName = "kodebarang", FillWeight = 70 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "namabarang", HeaderText = "Nama Barang", DataPropertyName = "namabarang", FillWeight = 170 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "satuan", HeaderText = "Satuan", DataPropertyName = "satuan", FillWeight = 55 });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "hargasatuan",
                HeaderText = "Harga",
                DataPropertyName = "hargasatuan",
                FillWeight = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "stok",
                HeaderText = "Stok",
                DataPropertyName = "stok",
                FillWeight = 55,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.###", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "status_stok", HeaderText = "Status", DataPropertyName = "status_stok", FillWeight = 65 });
        }

        private void PasangEvent()
        {
            _view.SearchButton.Click += delegate { CariProduk(true); };
            _view.RefreshButton.Click += delegate
            {
                _view.SearchBox.Text = string.Empty;
                _placeholder = false;
                MuatProduk(string.Empty);
            };
            _view.SearchBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    CariProduk(true);
                }
            };
            _view.SearchBox.Enter += delegate
            {
                if (!_placeholder) return;
                _placeholder = false;
                _view.SearchBox.Text = string.Empty;
                _view.SearchBox.ForeColor = AppTheme.Text;
            };
            _view.SearchBox.Leave += delegate
            {
                if (_view.SearchBox.Text.Trim().Length != 0) return;
                _placeholder = true;
                _view.SearchBox.ForeColor = AppTheme.Muted;
                _view.SearchBox.Text = "Scan barcode / ketik kode atau nama barang...";
            };

            _view.ProductGrid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) TambahDariGrid(e.RowIndex);
            };
            _view.ProductGrid.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                if (_view.ProductGrid.CurrentRow == null) return;
                e.SuppressKeyPress = true;
                TambahDariGrid(_view.ProductGrid.CurrentRow.Index);
            };

            _view.CartGrid.CellEndEdit += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                HitungUlangBaris(e.RowIndex);
            };
            _view.CartGrid.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Delete || _view.CartGrid.CurrentRow == null) return;
                e.SuppressKeyPress = true;
                HapusBaris(_view.CartGrid.CurrentRow.Index);
            };
            _view.CartGrid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e)
            {
                e.ThrowException = false;
                SetStatus("Nilai tidak valid. Masukkan angka tanpa huruf.");
            };

            _view.DiscountInput.TextChanged += delegate { HitungTotal(); };
            _view.PaidInput.TextChanged += delegate { HitungTotal(); };
            _view.PaymentCombo.SelectedIndexChanged += delegate { HitungTotal(); };
            _view.SaveButton.Click += delegate { SimpanTransaksi(); };
            _view.ClearButton.Click += delegate { KosongkanKeranjang(true); };
            _view.PrintButton.Click += delegate { CetakStrukTerakhir(); };

            AcceptButton = _view.SaveButton;
        }

        // =========================================================
        // PRODUK
        // =========================================================
        private void MuatProduk(string keyword)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _products = _penjualan.GetProdukCari(keyword);
                _view.ProductGrid.DataSource = _products;
                SetStatus(_products.Rows.Count + " barang ditemukan.");
            }
            catch (Exception ex)
            {
                SetStatus("Gagal memuat barang: " + ex.Message);
                UiKit.ShowError(this, ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void CariProduk(bool tambahJikaTunggal)
        {
            string keyword = _placeholder ? string.Empty : _view.SearchBox.Text.Trim();
            MuatProduk(keyword);
            if (!tambahJikaTunggal) return;

            // Barcode / kode yang diketik persis langsung masuk keranjang.
            if (_products == null || keyword.Length == 0) return;
            foreach (DataRow row in _products.Rows)
            {
                string kode = Convert.ToString(row["kodebarang"]);
                string barcode = Convert.ToString(row["barcode"]);
                if (string.Equals(kode, keyword, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(barcode, keyword, StringComparison.OrdinalIgnoreCase))
                {
                    TambahKeKeranjang(row);
                    _view.SearchBox.Clear();
                    return;
                }
            }
        }

        private void TambahDariGrid(int rowIndex)
        {
            if (_products == null || rowIndex < 0 || rowIndex >= _products.Rows.Count) return;
            TambahKeKeranjang(_products.Rows[rowIndex]);
        }

        private void TambahKeKeranjang(DataRow produk)
        {
            string kode = Convert.ToString(produk["kodebarang"]);
            decimal stok = Angka(produk["stok"]);
            decimal harga = Angka(produk["hargasatuan"]);

            if (stok <= 0)
            {
                UiKit.ShowInfo(this, "Stok " + produk["namabarang"] + " habis. Lakukan pembelian terlebih dahulu.");
                SetStatus("Stok habis: " + produk["namabarang"]);
                return;
            }

            DataRow existing = null;
            foreach (DataRow row in _cart.Rows)
            {
                if (string.Equals(Convert.ToString(row["kodebarang"]), kode, StringComparison.OrdinalIgnoreCase))
                {
                    existing = row;
                    break;
                }
            }

            if (existing != null)
            {
                decimal qtyBaru = Angka(existing["qty"]) + 1m;
                if (qtyBaru > stok)
                {
                    UiKit.ShowInfo(this, "Stok tidak mencukupi. Tersedia " + stok.ToString("0.###") + " unit.");
                    return;
                }
                existing["qty"] = qtyBaru;
                existing["subtotal"] = (Angka(existing["hargasatuan"]) * qtyBaru) - Angka(existing["diskon"]);
            }
            else
            {
                DataRow baru = _cart.NewRow();
                baru["kodebarang"] = kode;
                baru["namabarang"] = Convert.ToString(produk["namabarang"]);
                baru["hargasatuan"] = harga;
                baru["qty"] = 1m;
                baru["diskon"] = 0m;
                baru["subtotal"] = harga;
                _cart.Rows.Add(baru);
            }

            _view.CartGrid.Refresh();
            HitungTotal();
            SetStatus("Ditambahkan: " + produk["namabarang"]);
        }

        private void HitungUlangBaris(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _cart.Rows.Count) return;
            DataRow row = _cart.Rows[rowIndex];
            decimal qty = Angka(row["qty"]);
            decimal harga = Angka(row["hargasatuan"]);
            decimal diskon = Angka(row["diskon"]);

            if (qty <= 0)
            {
                qty = 1m;
                row["qty"] = qty;
            }
            if (diskon < 0) { diskon = 0m; row["diskon"] = diskon; }
            if (diskon > harga * qty) { diskon = harga * qty; row["diskon"] = diskon; }

            decimal stok = StokProduk(Convert.ToString(row["kodebarang"]));
            if (stok >= 0 && qty > stok)
            {
                qty = stok;
                row["qty"] = qty;
                SetStatus("Jumlah disesuaikan dengan stok tersedia (" + stok.ToString("0.###") + ").");
            }

            row["subtotal"] = (harga * qty) - diskon;
            HitungTotal();
        }

        private decimal StokProduk(string kode)
        {
            if (_products == null) return -1m;
            foreach (DataRow row in _products.Rows)
            {
                if (string.Equals(Convert.ToString(row["kodebarang"]), kode, StringComparison.OrdinalIgnoreCase))
                    return Angka(row["stok"]);
            }
            return -1m;
        }

        private void HapusBaris(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _cart.Rows.Count) return;
            string nama = Convert.ToString(_cart.Rows[rowIndex]["namabarang"]);
            _cart.Rows.RemoveAt(rowIndex);
            HitungTotal();
            SetStatus("Dihapus dari keranjang: " + nama);
        }

        // =========================================================
        // TOTAL & PEMBAYARAN
        // =========================================================
        private void HitungTotal()
        {
            decimal subtotal = 0m;
            foreach (DataRow row in _cart.Rows)
                subtotal += Angka(row["subtotal"]);

            decimal diskon = UiKit.ToDecimal(_view.DiscountInput);
            if (diskon < 0) diskon = 0m;
            if (diskon > subtotal) diskon = subtotal;

            decimal total = subtotal - diskon;
            decimal dibayar = UiKit.ToDecimal(_view.PaidInput);
            bool kredit = string.Equals(Convert.ToString(_view.PaymentCombo.SelectedItem), "Kredit", StringComparison.OrdinalIgnoreCase);
            decimal kembali = kredit ? 0m : Math.Max(0m, dibayar - total);

            _view.SubtotalLabel.Text = UiKit.Rupiah(subtotal);
            _view.TotalLabel.Text = UiKit.Rupiah(total);
            _view.ChangeLabel.Text = UiKit.Rupiah(kembali);
            _view.ChangeLabel.ForeColor = (!kredit && dibayar > 0 && dibayar < total) ? Color.Firebrick : AppTheme.Success;
            _view.SaveButton.Enabled = _cart.Rows.Count > 0 && total > 0;
            _view.PrintButton.Enabled = !string.IsNullOrEmpty(_lastNomor);
        }

        private void MuatPelanggan()
        {
            try
            {
                DataTable dt = _master.GetPelanggan();
                DataTable data = new DataTable();
                data.Columns.Add("pelanggan_id", typeof(int));
                data.Columns.Add("nama", typeof(string));
                data.Rows.Add(0, "Umum / Tanpa nama");
                foreach (DataRow row in dt.Rows)
                    data.Rows.Add(Convert.ToInt32(row["pelanggan_id"]), Convert.ToString(row["nama"]));

                _view.CustomerCombo.DataSource = data;
                _view.CustomerCombo.DisplayMember = "nama";
                _view.CustomerCombo.ValueMember = "pelanggan_id";
                _view.CustomerCombo.SelectedIndex = 0;
            }
            catch
            {
                // Data pelanggan opsional; transaksi tunai tetap bisa dilakukan.
                _view.CustomerCombo.Items.Clear();
                _view.CustomerCombo.Items.Add("Umum / Tanpa nama");
                _view.CustomerCombo.SelectedIndex = 0;
            }
        }

        // =========================================================
        // SIMPAN & CETAK
        // =========================================================
        private void SimpanTransaksi()
        {
            if (_sedangSimpan) return;
            if (_cart.Rows.Count == 0)
            {
                UiKit.ShowInfo(this, "Keranjang masih kosong.");
                return;
            }

            decimal subtotal = 0m;
            var items = new List<ClassPenjualanDetail>();
            foreach (DataRow row in _cart.Rows)
            {
                decimal qty = Angka(row["qty"]);
                decimal harga = Angka(row["hargasatuan"]);
                if (qty <= 0)
                {
                    UiKit.ShowInfo(this, "Jumlah barang harus lebih dari nol.");
                    return;
                }

                items.Add(new ClassPenjualanDetail
                {
                    kodebarang = Convert.ToString(row["kodebarang"]),
                    namabarang = Convert.ToString(row["namabarang"]),
                    qty = qty,
                    hargasatuan = harga,
                    diskon = Angka(row["diskon"])
                });
                subtotal += Angka(row["subtotal"]);
            }

            decimal diskonHeader = Math.Min(UiKit.ToDecimal(_view.DiscountInput), subtotal);
            decimal total = subtotal - diskonHeader;
            string metode = Convert.ToString(_view.PaymentCombo.SelectedItem);
            bool kredit = string.Equals(metode, "Kredit", StringComparison.OrdinalIgnoreCase);
            decimal dibayar = UiKit.ToDecimal(_view.PaidInput);

            if (total <= 0)
            {
                UiKit.ShowInfo(this, "Total transaksi harus lebih dari nol.");
                return;
            }
            if (!kredit && dibayar < total)
            {
                UiKit.ShowInfo(this, "Uang dibayar masih kurang dari total.\nTotal " + UiKit.Rupiah(total) + ".");
                _view.PaidInput.Focus();
                return;
            }

            int pelangganId = 0;
            if (_view.CustomerCombo.SelectedValue != null)
                int.TryParse(Convert.ToString(_view.CustomerCombo.SelectedValue), out pelangganId);
            if (kredit && pelangganId <= 0)
            {
                UiKit.ShowInfo(this, "Transaksi kredit wajib memilih pelanggan.");
                _view.CustomerCombo.Focus();
                return;
            }
            if (kredit && dibayar <= 0) dibayar = 0m;

            if (!kredit && dibayar > total) dibayar = total;

            _sedangSimpan = true;
            _view.SaveButton.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var header = new ClassPenjualan
                {
                    tanggal = DateTime.Now,
                    pelanggan_id = pelangganId > 0 ? (int?)pelangganId : null,
                    subtotal = subtotal,
                    diskon = diskonHeader,
                    total = total,
                    dibayar = dibayar,
                    kembali = Math.Max(0m, dibayar - total),
                    metode_bayar = metode,
                    kasir = _kasir,
                    catatan = _view.NoteInput.Text.Trim(),
                    status = "Posted"
                };

                ClassSimpanHasil hasil = _penjualan.SimpanPenjualan(header, items);

                // Simpan snapshot untuk cetak struk sebelum keranjang dibersihkan.
                _lastNomor = hasil.nomor;
                _lastSubtotal = subtotal;
                _lastDiskon = diskonHeader;
                _lastTotal = total;
                _lastDibayar = dibayar;
                _lastKembali = Math.Max(0m, dibayar - total);
                _lastDetail = _penjualan.GetDetailPenjualan(hasil.nomor);

                string pesan = hasil.pesan + "\nKembalian: " + UiKit.Rupiah(_lastKembali) +
                               "\n\nCetak struk sekarang?";
                DialogResult jawab = MessageBox.Show(this, pesan, "Transaksi Berhasil",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                KosongkanKeranjang(false);
                MuatProduk(string.Empty);
                SetStatus("Transaksi " + hasil.nomor + " tersimpan.");

                if (jawab == DialogResult.Yes) CetakStruk(_lastNomor, _lastDetail,
                    _lastSubtotal, _lastDiskon, _lastTotal, _lastDibayar, _lastKembali);
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
                SetStatus("Transaksi gagal: " + ex.Message);
            }
            finally
            {
                _sedangSimpan = false;
                Cursor = Cursors.Default;
                HitungTotal();
            }
        }

        private void KosongkanKeranjang(bool konfirmasi)
        {
            if (_cart.Rows.Count > 0 && konfirmasi)
            {
                if (UiKit.Confirm(this, "Kosongkan seluruh keranjang?") != DialogResult.Yes) return;
            }

            _cart.Rows.Clear();
            _view.DiscountInput.Text = "0";
            _view.PaidInput.Text = "0";
            _view.NoteInput.Clear();
            _view.PaymentCombo.SelectedIndex = 0;
            if (_view.CustomerCombo.Items.Count > 0) _view.CustomerCombo.SelectedIndex = 0;
            HitungTotal();
            SetStatus("Keranjang kosong. Siap melayani transaksi baru.");
            _view.SearchBox.Focus();
            _placeholder = false;
            _view.SearchBox.Clear();
            _view.SearchBox.ForeColor = AppTheme.Text;
        }

        private void CetakStrukTerakhir()
        {
            if (string.IsNullOrEmpty(_lastNomor) || _lastDetail == null)
            {
                UiKit.ShowInfo(this, "Belum ada transaksi yang bisa dicetak pada sesi ini.");
                return;
            }
            CetakStruk(_lastNomor, _lastDetail, _lastSubtotal, _lastDiskon,
                _lastTotal, _lastDibayar, _lastKembali);
        }

        private void CetakStruk(string nomor, DataTable detail, decimal subtotal,
            decimal diskon, decimal total, decimal dibayar, decimal kembali)
        {
            try
            {
                ClassProfilToko profil = _master.GetProfil();
                ReportStrukTemplate.Preview(this, profil.nama_toko, profil.alamat, profil.telepon,
                    nomor, DateTime.Now, _kasir, detail, subtotal, diskon, total,
                    dibayar, kembali, profil.footer_struk);
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void SetStatus(string message)
        {
            if (_view.StatusLabel != null) _view.StatusLabel.Text = message;
        }

        private static decimal Angka(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            decimal hasil;
            return decimal.TryParse(Convert.ToString(value), out hasil) ? hasil : 0m;
        }
    }
}
