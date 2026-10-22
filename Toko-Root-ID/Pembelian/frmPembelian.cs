using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Helper;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Pembelian
{
    /// <summary>Form pembelian: entri penerimaan barang, hutang supplier, dan riwayat.</summary>
    public class frmPembelian : Form
    {
        private readonly PembelianTemplate.PembelianView _view;
        private readonly DAPembelian _pembelian = new DAPembelian();
        private readonly DAMaster _master = new DAMaster();

        private DataTable _cart;
        private DataTable _products;
        private DataTable _lastDetail;
        private string _lastNomor;
        private string _lastSupplier;
        private decimal _lastSubtotal;
        private decimal _lastDiskon;
        private decimal _lastTotal;
        private decimal _lastDibayar;
        private decimal _lastSisa;
        private bool _sedangSimpan;

        public frmPembelian()
        {
            _view = PembelianTemplate.Build(this);
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleBuffered = true;

            SiapkanKeranjang();
            PasangEvent();
            MuatSupplier();
            MuatProduk(string.Empty);
            NomorBaru();
            MuatRiwayat();
        }

        // =========================================================
        // PERSIAPAN
        // =========================================================
        private void SiapkanKeranjang()
        {
            _cart = new DataTable("KeranjangPembelian");
            _cart.Columns.Add("kodebarang", typeof(string));
            _cart.Columns.Add("namabarang", typeof(string));
            _cart.Columns.Add("qty", typeof(decimal));
            _cart.Columns.Add("harga_beli", typeof(decimal));
            _cart.Columns.Add("diskon", typeof(decimal));
            _cart.Columns.Add("subtotal", typeof(decimal));

            _view.CartGrid.AutoGenerateColumns = false;
            Bind("kodebarang", "kodebarang");
            Bind("namabarang", "namabarang");
            Bind("qty", "qty");
            Bind("harga_beli", "harga_beli");
            Bind("diskon", "diskon");
            Bind("subtotal", "subtotal");
            _view.CartGrid.DataSource = _cart;
            HitungTotal();
        }

        private void Bind(string column, string property)
        {
            if (_view.CartGrid.Columns[column] != null)
                _view.CartGrid.Columns[column].DataPropertyName = property;
        }

        private void PasangEvent()
        {
            _view.ProductRefresh.Click += delegate { MuatProduk(string.Empty); };
            _view.ProductSearch.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                MuatProduk(UiKit.SearchText(_view.ProductSearch));
            };
            _view.ProductGrid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) TambahBaris(e.RowIndex);
            };
            _view.ProductGrid.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter || _view.ProductGrid.CurrentRow == null) return;
                e.SuppressKeyPress = true;
                TambahBaris(_view.ProductGrid.CurrentRow.Index);
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
                int index = _view.CartGrid.CurrentRow.Index;
                if (index >= 0 && index < _cart.Rows.Count) _cart.Rows.RemoveAt(index);
                HitungTotal();
            };
            _view.CartGrid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e)
            {
                e.ThrowException = false;
                SetStatus("Nilai tidak valid. Masukkan angka tanpa huruf.");
            };

            _view.DiscountBox.TextChanged += delegate { HitungTotal(); };
            _view.PaidBox.TextChanged += delegate { HitungTotal(); };
            _view.SaveButton.Click += delegate { Simpan(); };
            _view.ClearButton.Click += delegate { Kosongkan(true); };
            _view.PrintButton.Click += delegate { Cetak(); };

            _view.HistoryRefresh.Click += delegate { MuatRiwayat(); };
            _view.HistorySearch.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                MuatRiwayat();
            };
            _view.HistoryGrid.CellClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) MuatDetailRiwayat(e.RowIndex);
            };
            _view.ShowDetailButton.Click += delegate
            {
                if (_view.HistoryGrid.CurrentRow != null)
                    MuatDetailRiwayat(_view.HistoryGrid.CurrentRow.Index);
            };
        }

        // =========================================================
        // DATA PRODUK & SUPPLIER
        // =========================================================
        private void MuatSupplier()
        {
            try
            {
                UiKit.FillCombo(_view.SupplierCombo, _master.GetSupplier(), "supplier_id", "nama");
                if (_view.SupplierCombo.Items.Count > 0) _view.SupplierCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void MuatProduk(string keyword)
        {
            try
            {
                _products = _pembelian.GetProdukCari(keyword);
                _view.ProductGrid.DataSource = _products;
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void NomorBaru()
        {
            try
            {
                _view.NomorBox.Text = _pembelian.GenerateNomor();
            }
            catch
            {
                _view.NomorBox.Text = "PB" + DateTime.Now.ToString("yyyyMMdd") + "-0001";
            }
        }

        private void TambahBaris(int rowIndex)
        {
            if (_products == null || rowIndex < 0 || rowIndex >= _products.Rows.Count) return;
            DataRow produk = _products.Rows[rowIndex];
            string kode = Convert.ToString(produk["kodebarang"]);

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
                existing["qty"] = Angka(existing["qty"]) + 1m;
                existing["subtotal"] = (Angka(existing["harga_beli"]) * Angka(existing["qty"])) - Angka(existing["diskon"]);
            }
            else
            {
                DataRow baru = _cart.NewRow();
                baru["kodebarang"] = kode;
                baru["namabarang"] = Convert.ToString(produk["namabarang"]);
                baru["qty"] = 1m;
                baru["harga_beli"] = Angka(produk["harga_beli"]);
                baru["diskon"] = 0m;
                baru["subtotal"] = Angka(produk["harga_beli"]);
                _cart.Rows.Add(baru);
            }

            _view.CartGrid.Refresh();
            HitungTotal();
            SetStatus("Ditambahkan: " + produk["namabarang"]);
        }

        private void HitungUlangBaris(int index)
        {
            if (index < 0 || index >= _cart.Rows.Count) return;
            DataRow row = _cart.Rows[index];
            decimal qty = Angka(row["qty"]);
            decimal harga = Angka(row["harga_beli"]);
            decimal diskon = Angka(row["diskon"]);

            if (qty <= 0) { qty = 1m; row["qty"] = qty; }
            if (harga < 0) { harga = 0m; row["harga_beli"] = harga; }
            if (diskon < 0) { diskon = 0m; row["diskon"] = diskon; }
            if (diskon > harga * qty) { diskon = harga * qty; row["diskon"] = diskon; }

            row["subtotal"] = (harga * qty) - diskon;
            HitungTotal();
        }

        private void HitungTotal()
        {
            decimal subtotal = 0m;
            foreach (DataRow row in _cart.Rows) subtotal += Angka(row["subtotal"]);

            decimal diskon = UiKit.ToDecimal(_view.DiscountBox);
            if (diskon < 0) diskon = 0m;
            if (diskon > subtotal) diskon = subtotal;

            decimal total = subtotal - diskon;
            decimal dibayar = UiKit.ToDecimal(_view.PaidBox);
            if (dibayar > total) dibayar = total;
            decimal sisa = Math.Max(0m, total - dibayar);

            _view.SubtotalLabel.Text = UiKit.Rupiah(subtotal);
            _view.TotalLabel.Text = UiKit.Rupiah(total);
            _view.SisaLabel.Text = UiKit.Rupiah(sisa);
            _view.SisaLabel.ForeColor = sisa > 0 ? AppTheme.Danger : AppTheme.Success;
            _view.SaveButton.Enabled = _cart.Rows.Count > 0;
            _view.PrintButton.Enabled = !string.IsNullOrEmpty(_lastNomor);
        }

        // =========================================================
        // SIMPAN
        // =========================================================
        private void Simpan()
        {
            if (_sedangSimpan) return;
            if (_cart.Rows.Count == 0)
            {
                UiKit.ShowInfo(this, "Daftar pembelian masih kosong.");
                return;
            }

            int supplierId = 0;
            if (_view.SupplierCombo.SelectedIndex > 0 && _view.SupplierCombo.SelectedValue != null)
                int.TryParse(Convert.ToString(_view.SupplierCombo.SelectedValue), out supplierId);

            var items = new List<ClassPembelianDetail>();
            decimal subtotal = 0m;
            foreach (DataRow row in _cart.Rows)
            {
                decimal qty = Angka(row["qty"]);
                if (qty <= 0)
                {
                    UiKit.ShowInfo(this, "Qty pembelian harus lebih dari nol.");
                    return;
                }
                items.Add(new ClassPembelianDetail
                {
                    kodebarang = Convert.ToString(row["kodebarang"]),
                    namabarang = Convert.ToString(row["namabarang"]),
                    qty = qty,
                    harga_beli = Angka(row["harga_beli"]),
                    diskon = Angka(row["diskon"])
                });
                subtotal += Angka(row["subtotal"]);
            }

            decimal diskonHeader = Math.Min(UiKit.ToDecimal(_view.DiscountBox), subtotal);
            decimal total = subtotal - diskonHeader;
            decimal dibayar = Math.Min(UiKit.ToDecimal(_view.PaidBox), total);
            string status = Convert.ToString(_view.StatusCombo.SelectedItem);

            _sedangSimpan = true;
            _view.SaveButton.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                var header = new ClassPembelian
                {
                    nomor = _view.NomorBox.Text,
                    tanggal = _view.DatePicker.Value,
                    supplier_id = supplierId > 0 ? (int?)supplierId : null,
                    subtotal = subtotal,
                    diskon = diskonHeader,
                    total = total,
                    dibayar = dibayar,
                    status = status,
                    catatan = _view.NoteBox.Text.Trim(),
                    created_by = NamaUser()
                };

                ClassSimpanHasil hasil = _pembelian.SimpanPembelian(header, items);

                _lastNomor = hasil.nomor;
                _lastSupplier = supplierId > 0 ? Convert.ToString(_view.SupplierCombo.Text) : "Umum";
                _lastSubtotal = subtotal;
                _lastDiskon = diskonHeader;
                _lastTotal = total;
                _lastDibayar = dibayar;
                _lastSisa = total - dibayar;
                _lastDetail = new DAPembelian().GetDetailPembelian(PembelianId(hasil.nomor));

                DialogResult jawab = MessageBox.Show(this,
                    hasil.pesan + "\nStatus stok: " + status +
                    "\nSisa hutang: " + UiKit.Rupiah(Math.Max(0m, total - dibayar)) +
                    "\n\nCetak bukti pembelian sekarang?",
                    "Pembelian Tersimpan", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                Kosongkan(false);
                NomorBaru();
                MuatProduk(string.Empty);
                MuatRiwayat();
                SetStatus("Pembelian " + hasil.nomor + " tersimpan.");

                if (jawab == DialogResult.Yes) Cetak();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
                SetStatus("Gagal menyimpan pembelian: " + ex.Message);
            }
            finally
            {
                _sedangSimpan = false;
                Cursor = Cursors.Default;
                HitungTotal();
            }
        }

        private long PembelianId(string nomor)
        {
            try
            {
                DataTable dt = _pembelian.GetRiwayatPembelian(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), nomor);
                foreach (DataRow row in dt.Rows)
                {
                    if (string.Equals(Convert.ToString(row["nomor"]), nomor, StringComparison.OrdinalIgnoreCase))
                        return Convert.ToInt64(row["pembelian_id"]);
                }
            }
            catch { }
            return 0;
        }

        private void Kosongkan(bool konfirmasi)
        {
            if (konfirmasi && _cart.Rows.Count > 0 &&
                UiKit.Confirm(this, "Kosongkan daftar pembelian?") != DialogResult.Yes) return;

            _cart.Rows.Clear();
            _view.DiscountBox.Text = "0";
            _view.PaidBox.Text = "0";
            _view.NoteBox.Clear();
            HitungTotal();
            SetStatus("Daftar pembelian kosong.");
        }

        private void Cetak()
        {
            if (string.IsNullOrEmpty(_lastNomor) || _lastDetail == null)
            {
                UiKit.ShowInfo(this, "Belum ada pembelian yang bisa dicetak pada sesi ini.");
                return;
            }
            try
            {
                ClassProfilToko profil = _master.GetProfil();
                ReportStrukTemplate.PreviewPembelian(this, profil.nama_toko, profil.alamat, profil.telepon,
                    _lastNomor, DateTime.Now, _lastSupplier, _lastDetail, _lastSubtotal,
                    _lastDiskon, _lastTotal, _lastDibayar, _lastSisa);
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        // =========================================================
        // RIWAYAT
        // =========================================================
        private void MuatRiwayat()
        {
            try
            {
                DataTable dt = _pembelian.GetRiwayatPembelian(_view.StartPicker.Value.Date,
                    _view.EndPicker.Value.Date, UiKit.SearchText(_view.HistorySearch));
                _view.HistoryGrid.DataSource = dt;
                FormatRiwayat();

                if (dt.Rows.Count > 0) MuatDetailRiwayat(0);
                else { _view.DetailGrid.DataSource = null; _view.DetailLabel.Text = "Detail pembelian"; }
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void FormatRiwayat()
        {
            DataGridView grid = _view.HistoryGrid;
            if (grid.Columns.Count == 0) return;
            Header(grid, "nomor", "Nomor", 120);
            Header(grid, "tanggal", "Tanggal", 110);
            Header(grid, "supplier", "Supplier", 150);
            Header(grid, "status", "Status", 80);
            Header(grid, "total", "Total", 90);
            Header(grid, "dibayar", "Dibayar", 90);
            Header(grid, "sisa", "Sisa", 90);
            Header(grid, "created_by", "Oleh", 80);
            if (grid.Columns["pembelian_id"] != null) grid.Columns["pembelian_id"].Visible = false;
            if (grid.Columns["subtotal"] != null) grid.Columns["subtotal"].Visible = false;
            if (grid.Columns["diskon"] != null) grid.Columns["diskon"].Visible = false;
            foreach (string column in new[] { "total", "dibayar", "sisa" })
                if (grid.Columns[column] != null) grid.Columns[column].DefaultCellStyle.Format = "N0";
            if (grid.Columns["tanggal"] != null) grid.Columns["tanggal"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        private void MuatDetailRiwayat(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _view.HistoryGrid.Rows.Count) return;
            DataRowView row = _view.HistoryGrid.Rows[rowIndex].DataBoundItem as DataRowView;
            if (row == null) return;
            try
            {
                long id = Convert.ToInt64(row["pembelian_id"]);
                _view.DetailGrid.DataSource = _pembelian.GetDetailPembelian(id);
                _view.DetailLabel.Text = "Detail pembelian " + Convert.ToString(row["nomor"]) +
                    " - " + Convert.ToString(row["supplier"]);
                FormatDetail();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void FormatDetail()
        {
            DataGridView grid = _view.DetailGrid;
            if (grid.Columns.Count == 0) return;
            if (grid.Columns["kodebarang"] != null) grid.Columns["kodebarang"].HeaderText = "Kode";
            if (grid.Columns["namabarang"] != null) grid.Columns["namabarang"].HeaderText = "Nama Barang";
            if (grid.Columns["qty"] != null) grid.Columns["qty"].DefaultCellStyle.Format = "0.###";
            foreach (string column in new[] { "harga_beli", "diskon", "total" })
                if (grid.Columns[column] != null) grid.Columns[column].DefaultCellStyle.Format = "N0";
        }

        private static void Header(DataGridView grid, string column, string text, int weight)
        {
            if (grid.Columns[column] == null) return;
            grid.Columns[column].HeaderText = text;
            grid.Columns[column].FillWeight = weight;
        }

        private static string NamaUser()
        {
            try
            {
                if (SessionManager.IsLoggedIn && SessionManager.CurrentUser != null)
                    return string.IsNullOrWhiteSpace(SessionManager.CurrentUser.nama)
                        ? SessionManager.CurrentUser.nik : SessionManager.CurrentUser.nama;
            }
            catch { }
            return Environment.UserName;
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
