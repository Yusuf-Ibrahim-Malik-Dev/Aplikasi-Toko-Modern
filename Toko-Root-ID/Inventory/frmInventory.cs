using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Helper;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Inventory
{
    /// <summary>Form inventory: mutasi stok, stok opname, transfer gudang, dan stok kritis.</summary>
    public class frmInventory : Form
    {
        private readonly InventoryTemplate.InventoryView _view;
        private readonly DAInventory _inventory = new DAInventory();
        private readonly DAMaster _master = new DAMaster();
        private readonly DALaporan _laporan = new DALaporan();

        private DataTable _opname;
        private DataTable _transferCart;
        private DataTable _transferProducts;

        public frmInventory()
        {
            _view = InventoryTemplate.Build(this);
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleBuffered = true;

            SiapkanTransferCart();
            PasangEvent();
            MuatGudang();
            MuatMutasi();
            MuatOpname();
            MuatProdukTransfer(string.Empty);
            MuatKritis();
        }

        // =========================================================
        // PERSIAPAN
        // =========================================================
        private void SiapkanTransferCart()
        {
            _transferCart = new DataTable("TransferCart");
            _transferCart.Columns.Add("kodebarang", typeof(string));
            _transferCart.Columns.Add("namabarang", typeof(string));
            _transferCart.Columns.Add("qty", typeof(decimal));

            _view.TransferCart.AutoGenerateColumns = false;
            BindCart("kodebarang", "kodebarang");
            BindCart("namabarang", "namabarang");
            BindCart("qty", "qty");
            _view.TransferCart.DataSource = _transferCart;
        }

        private void BindCart(string column, string property)
        {
            if (_view.TransferCart.Columns[column] != null)
                _view.TransferCart.Columns[column].DataPropertyName = property;
        }

        private void PasangEvent()
        {
            _view.RefreshButton.Click += delegate { MuatMutasi(); };
            _view.SearchBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                MuatMutasi();
            };

            _view.OpnameRefresh.Click += delegate { MuatOpname(); };
            _view.FillSystemButton.Click += delegate { IsiFisikSamaDenganSistem(); };
            _view.OpnameSave.Click += delegate { SimpanOpname(); };
            _view.OpnameGrid.CellEndEdit += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) HitungSelisih(e.RowIndex);
            };
            _view.OpnameGrid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e)
            {
                e.ThrowException = false;
            };

            _view.TransferRefresh.Click += delegate { MuatProdukTransfer(UiKit.SearchText(_view.TransferSearch)); };
            _view.TransferSearch.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                MuatProdukTransfer(UiKit.SearchText(_view.TransferSearch));
            };
            _view.TransferProductGrid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) TambahTransfer(e.RowIndex);
            };
            _view.TransferProductGrid.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter || _view.TransferProductGrid.CurrentRow == null) return;
                e.SuppressKeyPress = true;
                TambahTransfer(_view.TransferProductGrid.CurrentRow.Index);
            };
            _view.TransferCart.CellEndEdit += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) ValidasiQtyTransfer(e.RowIndex);
            };
            _view.TransferCart.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e)
            {
                e.ThrowException = false;
            };
            _view.TransferSave.Click += delegate { SimpanTransfer(); };

            _view.CriticalRefresh.Click += delegate { MuatKritis(); };
        }

        // =========================================================
        // MUTASI
        // =========================================================
        private void MuatMutasi()
        {
            try
            {
                string tipe = Convert.ToString(_view.TypeCombo.SelectedItem);
                if (string.Equals(tipe, "Semua", StringComparison.OrdinalIgnoreCase)) tipe = null;
                DataTable dt = _inventory.GetMutasi(_view.StartPicker.Value.Date,
                    _view.EndPicker.Value.Date, tipe, UiKit.SearchText(_view.SearchBox));
                _view.MutasiGrid.DataSource = dt;
                FormatMutasi();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void FormatMutasi()
        {
            DataGridView grid = _view.MutasiGrid;
            if (grid.Columns.Count == 0) return;
            Header(grid, "tanggal", "Tanggal", 110);
            Header(grid, "kodebarang", "Kode", 70);
            Header(grid, "namabarang", "Nama Barang", 170);
            Header(grid, "gudang", "Gudang", 90);
            Header(grid, "tipe", "Tipe", 80);
            Header(grid, "qty", "Qty", 60);
            Header(grid, "referensi", "Referensi", 110);
            Header(grid, "catatan", "Catatan", 180);
            Header(grid, "created_by", "Oleh", 80);
            if (grid.Columns["mutasi_id"] != null) grid.Columns["mutasi_id"].Visible = false;
            if (grid.Columns["tanggal"] != null) grid.Columns["tanggal"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (grid.Columns["qty"] != null) grid.Columns["qty"].DefaultCellStyle.Format = "0.###";
        }

        // =========================================================
        // OPNAME
        // =========================================================
        private void MuatGudang()
        {
            try
            {
                DataTable dt = _master.GetGudang();
                UiKit.FillCombo(_view.WarehouseCombo, dt, "gudang_id", "nama");
                if (_view.WarehouseCombo.Items.Count > 0) _view.WarehouseCombo.SelectedIndex = 0;
                UiKit.FillCombo(_view.FromWarehouse, dt, "gudang_id", "nama");
                if (_view.FromWarehouse.Items.Count > 0) _view.FromWarehouse.SelectedIndex = 0;
                UiKit.FillCombo(_view.ToWarehouse, dt, "gudang_id", "nama");
                if (_view.ToWarehouse.Items.Count > 1) _view.ToWarehouse.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void MuatOpname()
        {
            try
            {
                DataTable src = _master.GetBarang();
                _opname = new DataTable("Opname");
                _opname.Columns.Add("kodebarang", typeof(string));
                _opname.Columns.Add("namabarang", typeof(string));
                _opname.Columns.Add("satuan", typeof(string));
                _opname.Columns.Add("stok", typeof(decimal));
                _opname.Columns.Add("fisik", typeof(decimal));
                _opname.Columns.Add("selisih", typeof(decimal));

                foreach (DataRow row in src.Rows)
                {
                    DataRow baru = _opname.NewRow();
                    baru["kodebarang"] = Convert.ToString(row["kodebarang"]);
                    baru["namabarang"] = Convert.ToString(row["namabarang"]);
                    baru["satuan"] = Convert.ToString(row["satuan"]);
                    baru["stok"] = Angka(row["stok"]);
                    baru["fisik"] = Angka(row["stok"]);
                    baru["selisih"] = 0m;
                    _opname.Rows.Add(baru);
                }

                _view.OpnameGrid.DataSource = _opname;
                _view.OpnameStatus.Text = _opname.Rows.Count + " barang dimuat. Ubah kolom stok fisik bila ada selisih.";
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void IsiFisikSamaDenganSistem()
        {
            if (_opname == null) return;
            foreach (DataRow row in _opname.Rows)
            {
                row["fisik"] = Angka(row["stok"]);
                row["selisih"] = 0m;
            }
            _view.OpnameStatus.Text = "Semua stok fisik disamakan dengan stok sistem.";
        }

        private void HitungSelisih(int index)
        {
            if (_opname == null || index < 0 || index >= _opname.Rows.Count) return;
            DataRow row = _opname.Rows[index];
            decimal fisik = Angka(row["fisik"]);
            if (fisik < 0) { fisik = 0m; row["fisik"] = fisik; }
            row["selisih"] = fisik - Angka(row["stok"]);
        }

        private void SimpanOpname()
        {
            if (_opname == null) return;
            var items = new List<ClassOpnameItem>();
            foreach (DataRow row in _opname.Rows)
            {
                HitungSelisih(_opname.Rows.IndexOf(row));
                decimal selisih = Angka(row["selisih"]);
                if (selisih == 0) continue;
                items.Add(new ClassOpnameItem
                {
                    kodebarang = Convert.ToString(row["kodebarang"]),
                    namabarang = Convert.ToString(row["namabarang"]),
                    stok_sistem = Angka(row["stok"]),
                    stok_fisik = Angka(row["fisik"])
                });
            }

            if (items.Count == 0)
            {
                UiKit.ShowInfo(this, "Tidak ada selisih stok. Ubah kolom stok fisik terlebih dahulu.");
                return;
            }
            if (UiKit.Confirm(this, "Simpan opname untuk " + items.Count + " barang dengan selisih stok?") != DialogResult.Yes)
                return;

            try
            {
                ClassSimpanHasil hasil = _inventory.SimpanOpname(items, GudangId(_view.WarehouseCombo),
                    _view.OpnameNote.Text.Trim(), NamaUser());
                UiKit.ShowInfo(this, hasil.pesan);
                _view.OpnameNote.Clear();
                MuatOpname();
                MuatMutasi();
                MuatKritis();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        // =========================================================
        // TRANSFER
        // =========================================================
        private void MuatProdukTransfer(string keyword)
        {
            try
            {
                _transferProducts = _master.GetBarang(keyword);
                _view.TransferProductGrid.DataSource = _transferProducts;
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void TambahTransfer(int rowIndex)
        {
            if (_transferProducts == null || rowIndex < 0 || rowIndex >= _transferProducts.Rows.Count) return;
            DataRow produk = _transferProducts.Rows[rowIndex];
            string kode = Convert.ToString(produk["kodebarang"]);
            decimal stok = Angka(produk["stok"]);

            DataRow existing = null;
            foreach (DataRow row in _transferCart.Rows)
            {
                if (string.Equals(Convert.ToString(row["kodebarang"]), kode, StringComparison.OrdinalIgnoreCase))
                {
                    existing = row;
                    break;
                }
            }

            if (existing != null)
            {
                decimal qty = Angka(existing["qty"]) + 1m;
                if (qty > stok)
                {
                    UiKit.ShowInfo(this, "Stok tidak mencukupi. Tersedia " + stok.ToString("0.###") + ".");
                    return;
                }
                existing["qty"] = qty;
            }
            else
            {
                if (stok <= 0)
                {
                    UiKit.ShowInfo(this, "Stok barang ini habis.");
                    return;
                }
                DataRow baru = _transferCart.NewRow();
                baru["kodebarang"] = kode;
                baru["namabarang"] = Convert.ToString(produk["namabarang"]);
                baru["qty"] = 1m;
                _transferCart.Rows.Add(baru);
            }

            _view.TransferCart.Refresh();
            _view.TransferStatus.Text = _transferCart.Rows.Count + " barang dalam daftar transfer.";
        }

        private void ValidasiQtyTransfer(int index)
        {
            if (index < 0 || index >= _transferCart.Rows.Count) return;
            DataRow row = _transferCart.Rows[index];
            decimal qty = Angka(row["qty"]);
            if (qty <= 0) { qty = 1m; row["qty"] = qty; }

            decimal stok = StokTransfer(Convert.ToString(row["kodebarang"]));
            if (stok >= 0 && qty > stok)
            {
                row["qty"] = stok;
                _view.TransferStatus.Text = "Qty disesuaikan dengan stok tersedia (" + stok.ToString("0.###") + ").";
            }
        }

        private decimal StokTransfer(string kode)
        {
            if (_transferProducts == null) return -1m;
            foreach (DataRow row in _transferProducts.Rows)
            {
                if (string.Equals(Convert.ToString(row["kodebarang"]), kode, StringComparison.OrdinalIgnoreCase))
                    return Angka(row["stok"]);
            }
            return -1m;
        }

        private void SimpanTransfer()
        {
            int dari = GudangId(_view.FromWarehouse) ?? 0;
            int ke = GudangId(_view.ToWarehouse) ?? 0;
            if (dari <= 0 || ke <= 0)
            {
                UiKit.ShowInfo(this, "Pilih gudang asal dan gudang tujuan.");
                return;
            }
            if (dari == ke)
            {
                UiKit.ShowInfo(this, "Gudang asal dan tujuan tidak boleh sama.");
                return;
            }
            if (_transferCart.Rows.Count == 0)
            {
                UiKit.ShowInfo(this, "Belum ada barang yang dipindahkan.");
                return;
            }

            var items = new List<ClassTransferItem>();
            foreach (DataRow row in _transferCart.Rows)
            {
                decimal qty = Angka(row["qty"]);
                if (qty <= 0) continue;
                items.Add(new ClassTransferItem
                {
                    kodebarang = Convert.ToString(row["kodebarang"]),
                    namabarang = Convert.ToString(row["namabarang"]),
                    qty = qty
                });
            }
            if (items.Count == 0)
            {
                UiKit.ShowInfo(this, "Qty transfer harus lebih dari nol.");
                return;
            }

            try
            {
                ClassSimpanHasil hasil = _inventory.SimpanTransfer(dari, ke, items, null, NamaUser());
                UiKit.ShowInfo(this, hasil.pesan);
                _transferCart.Rows.Clear();
                _view.TransferStatus.Text = "Transfer tersimpan.";
                MuatProdukTransfer(string.Empty);
                MuatMutasi();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        // =========================================================
        // STOK KRITIS
        // =========================================================
        private void MuatKritis()
        {
            try
            {
                DataTable dt = _laporan.GetStokKritis(100);
                _view.CriticalGrid.DataSource = dt;
                _view.CriticalSummary.Text = dt.Rows.Count == 0
                    ? "Semua stok dalam kondisi aman."
                    : dt.Rows.Count + " barang perlu segera diisi ulang.";
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        // =========================================================
        // HELPER
        // =========================================================
        private static int? GudangId(ComboBox combo)
        {
            if (combo.SelectedIndex < 0 || combo.SelectedValue == null) return null;
            int id;
            return int.TryParse(Convert.ToString(combo.SelectedValue), out id) && id > 0 ? (int?)id : null;
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

        private static void Header(DataGridView grid, string column, string text, int weight)
        {
            if (grid.Columns[column] == null) return;
            grid.Columns[column].HeaderText = text;
            grid.Columns[column].FillWeight = weight;
        }

        private static decimal Angka(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            decimal hasil;
            return decimal.TryParse(Convert.ToString(value), out hasil) ? hasil : 0m;
        }
    }
}
