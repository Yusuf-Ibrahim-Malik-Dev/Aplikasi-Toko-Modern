using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Helper;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Retur
{
    /// <summary>
    /// Form retur penjualan dan pembelian. Qty retur dibatasi jumlah yang masih
    /// tersisa pada dokumen asal, stok serta kas disesuaikan otomatis oleh DARetur.
    /// </summary>
    public class frmRetur : Form
    {
        private readonly ReturTemplate.ReturView _view;
        private readonly DARetur _retur = new DARetur();
        private readonly DAPenjualan _penjualan = new DAPenjualan();
        private readonly DAPembelian _pembelian = new DAPembelian();

        private DataTable _salesItems;
        private DataTable _purchaseItems;

        public frmRetur()
        {
            _view = ReturTemplate.Build(this);
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleBuffered = true;

            PasangEvent();
            MuatDokumenPenjualan();
            MuatDokumenPembelian();
            MuatRiwayat();
        }

        // =========================================================
        // PERSIAPAN
        // =========================================================
        private void PasangEvent()
        {
            _view.Sales.LoadButton.Click += delegate { MuatBarangPenjualan(); };
            _view.Sales.DocumentCombo.SelectedIndexChanged += delegate { KosongkanBarang(true); };
            _view.Sales.SaveButton.Click += delegate { SimpanRetur(true); };
            _view.Sales.ItemsGrid.CellEndEdit += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) HitungBaris(_salesItems, e.RowIndex);
            };
            _view.Sales.ItemsGrid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e)
            {
                e.ThrowException = false;
            };

            _view.Purchase.LoadButton.Click += delegate { MuatBarangPembelian(); };
            _view.Purchase.DocumentCombo.SelectedIndexChanged += delegate { KosongkanBarang(false); };
            _view.Purchase.SaveButton.Click += delegate { SimpanRetur(false); };
            _view.Purchase.ItemsGrid.CellEndEdit += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) HitungBaris(_purchaseItems, e.RowIndex);
            };
            _view.Purchase.ItemsGrid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e)
            {
                e.ThrowException = false;
            };
        }

        private void KosongkanBarang(bool penjualan)
        {
            ReturTemplate.ReturPane pane = penjualan ? _view.Sales : _view.Purchase;
            if (penjualan) _salesItems = null; else _purchaseItems = null;
            pane.ItemsGrid.DataSource = null;
            pane.Info.Text = "Pilih dokumen lalu tekan Muat Barang.";
            pane.Info.ForeColor = AppTheme.Muted;
        }

        private void MuatDokumenPenjualan()
        {
            try
            {
                DateTime akhir = DateTime.Today;
                DataTable src = _penjualan.GetRiwayatPenjualan(akhir.AddDays(-90), akhir);
                var dt = new DataTable("DokumenJual");
                dt.Columns.Add("id", typeof(string));
                dt.Columns.Add("teks", typeof(string));
                foreach (DataRow row in src.Rows)
                {
                    string no = Convert.ToString(row["nofaktur"]);
                    DataRow baru = dt.NewRow();
                    baru["id"] = no;
                    baru["teks"] = no + "  •  " + Convert.ToDateTime(row["tanggal"]).ToString("dd/MM/yyyy") +
                                   "  •  " + Convert.ToString(row["pelanggan"]) +
                                   "  •  " + Angka(row["total"]).ToString("N0");
                    dt.Rows.Add(baru);
                }

                DataTable current = _view.Sales.DocumentCombo.DataSource as DataTable;
                string selected = SelectedId(_view.Sales.DocumentCombo);
                _view.Sales.DocumentCombo.DataSource = null;
                _view.Sales.DocumentCombo.DataSource = dt;
                _view.Sales.DocumentCombo.DisplayMember = "teks";
                _view.Sales.DocumentCombo.ValueMember = "id";
                if (!string.IsNullOrEmpty(selected)) PilihId(_view.Sales.DocumentCombo, selected);
                else if (_view.Sales.DocumentCombo.Items.Count > 0) _view.Sales.DocumentCombo.SelectedIndex = 0;

                if (current == null && dt.Rows.Count == 0)
                    _view.Sales.Info.Text = "Belum ada faktur penjualan dalam 90 hari terakhir.";
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void MuatDokumenPembelian()
        {
            try
            {
                DateTime akhir = DateTime.Today;
                DataTable src = _pembelian.GetRiwayatPembelian(akhir.AddDays(-90), akhir);
                var dt = new DataTable("DokumenBeli");
                dt.Columns.Add("id", typeof(string));
                dt.Columns.Add("teks", typeof(string));
                foreach (DataRow row in src.Rows)
                {
                    string id = Convert.ToString(row["pembelian_id"]);
                    DataRow baru = dt.NewRow();
                    baru["id"] = id;
                    baru["teks"] = Convert.ToString(row["nomor"]) +
                                   "  •  " + Convert.ToDateTime(row["tanggal"]).ToString("dd/MM/yyyy") +
                                   "  •  " + Convert.ToString(row["supplier"]) +
                                   "  •  " + Angka(row["total"]).ToString("N0");
                    dt.Rows.Add(baru);
                }

                string selected = SelectedId(_view.Purchase.DocumentCombo);
                _view.Purchase.DocumentCombo.DataSource = null;
                _view.Purchase.DocumentCombo.DataSource = dt;
                _view.Purchase.DocumentCombo.DisplayMember = "teks";
                _view.Purchase.DocumentCombo.ValueMember = "id";
                if (!string.IsNullOrEmpty(selected)) PilihId(_view.Purchase.DocumentCombo, selected);
                else if (_view.Purchase.DocumentCombo.Items.Count > 0) _view.Purchase.DocumentCombo.SelectedIndex = 0;

                _view.Purchase.Info.Text = "Pilih dokumen lalu tekan Muat Barang.";
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        // =========================================================
        // MUAT BARANG
        // =========================================================
        private void MuatBarangPenjualan()
        {
            string no = SelectedId(_view.Sales.DocumentCombo);
            if (string.IsNullOrEmpty(no))
            {
                UiKit.ShowInfo(this, "Pilih faktur penjualan terlebih dahulu.");
                return;
            }

            try
            {
                DataTable src = _retur.GetBarangPenjualan(no);
                DataTable sudah = _retur.GetQtyReturJual(no);
                _salesItems = BuatTabelBarang(src, sudah);
                _view.Sales.ItemsGrid.DataSource = _salesItems;
                UpdateInfo(_view.Sales, no);
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void MuatBarangPembelian()
        {
            long id;
            if (!long.TryParse(SelectedId(_view.Purchase.DocumentCombo), out id) || id <= 0)
            {
                UiKit.ShowInfo(this, "Pilih dokumen pembelian terlebih dahulu.");
                return;
            }

            try
            {
                DataTable src = _retur.GetBarangPembelian(id);
                DataTable sudah = _retur.GetQtyReturBeli(id);
                _purchaseItems = BuatTabelBarang(src, sudah);
                _view.Purchase.ItemsGrid.DataSource = _purchaseItems;
                UpdateInfo(_view.Purchase, SelectedId(_view.Purchase.DocumentCombo));
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        /// <summary>Menyusun tabel barang yang bisa diretur beserta sisa qty.</summary>
        private static DataTable BuatTabelBarang(DataTable src, DataTable sudah)
        {
            var dt = new DataTable("BarangRetur");
            dt.Columns.Add("kodebarang", typeof(string));
            dt.Columns.Add("namabarang", typeof(string));
            dt.Columns.Add("qty", typeof(decimal));
            dt.Columns.Add("harga", typeof(decimal));
            dt.Columns.Add("retur", typeof(decimal));
            dt.Columns.Add("subtotal", typeof(decimal));
            dt.Columns.Add("sisa", typeof(decimal));

            foreach (DataRow row in src.Rows)
            {
                string kode = Convert.ToString(row["kodebarang"]);
                decimal qty = Angka(row["qty"]);
                decimal total = Angka(row["total"]);
                decimal harga = qty > 0 && total > 0 ? total / qty : Angka(row["harga"]);
                decimal pernah = Angka(CariQty(sudah, kode));
                decimal sisa = qty - pernah;
                if (sisa < 0) sisa = 0;

                DataRow baru = dt.NewRow();
                baru["kodebarang"] = kode;
                baru["namabarang"] = Convert.ToString(row["namabarang"]);
                baru["qty"] = qty;
                baru["harga"] = Math.Round(harga, 2);
                baru["retur"] = 0m;
                baru["subtotal"] = 0m;
                baru["sisa"] = sisa;
                dt.Rows.Add(baru);
            }
            return dt;
        }

        // =========================================================
        // SIMPAN
        // =========================================================
        private void SimpanRetur(bool penjualan)
        {
            ReturTemplate.ReturPane pane = penjualan ? _view.Sales : _view.Purchase;
            DataTable items = penjualan ? _salesItems : _purchaseItems;
            if (items == null || items.Rows.Count == 0)
            {
                UiKit.ShowInfo(this, "Muat barang dari dokumen terlebih dahulu.");
                return;
            }

            string dokumen = SelectedId(pane.DocumentCombo);
            var daftar = new List<ClassReturItem>();
            foreach (DataRow row in items.Rows)
            {
                decimal qty = Angka(row["retur"]);
                if (qty <= 0) continue;
                decimal sisa = Angka(row["sisa"]);
                if (qty > sisa)
                {
                    UiKit.ShowInfo(this, "Qty retur " + Convert.ToString(row["namabarang"]) +
                        " melebihi sisa yang bisa diretur (" + sisa.ToString("0.###") + ").");
                    return;
                }
                daftar.Add(new ClassReturItem
                {
                    kodebarang = Convert.ToString(row["kodebarang"]),
                    namabarang = Convert.ToString(row["namabarang"]),
                    qty = qty,
                    harga = Angka(row["harga"])
                });
            }

            if (daftar.Count == 0)
            {
                UiKit.ShowInfo(this, "Isi kolom RETUR lebih dari nol pada barang yang dikembalikan.");
                return;
            }

            decimal total = 0m;
            foreach (ClassReturItem item in daftar) total += item.total;

            if (UiKit.Confirm(this, "Simpan " + (penjualan ? "retur penjualan" : "retur pembelian") +
                " senilai " + total.ToString("N0") + " untuk " + daftar.Count + " barang?") != DialogResult.Yes)
                return;

            try
            {
                ClassSimpanHasil hasil = penjualan
                    ? _retur.SimpanReturJual(dokumen, DateTime.Now, pane.ReasonBox.Text.Trim(), daftar, NamaUser())
                    : _retur.SimpanReturBeli(Convert.ToInt64(dokumen), DateTime.Now,
                        pane.ReasonBox.Text.Trim(), daftar, NamaUser());

                UiKit.ShowInfo(this, hasil.pesan);
                pane.ReasonBox.Clear();
                if (penjualan) _salesItems = null; else _purchaseItems = null;
                pane.ItemsGrid.DataSource = null;
                pane.Info.Text = "Retur tersimpan. Pilih dokumen lain untuk retur berikutnya.";
                pane.Info.ForeColor = AppTheme.Success;

                MuatRiwayat();
                MuatDokumenPenjualan();
                MuatDokumenPembelian();
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
                DateTime akhir = DateTime.Today;
                DataTable jual = _retur.GetReturJual(akhir.AddDays(-90), akhir);
                DataTable beli = _retur.GetReturBeli(akhir.AddDays(-90), akhir);

                _view.Sales.HistoryGrid.DataSource = jual;
                _view.Purchase.HistoryGrid.DataSource = beli;

                FormatRiwayat(_view.Sales.HistoryGrid, true);
                FormatRiwayat(_view.Purchase.HistoryGrid, false);
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private static void FormatRiwayat(DataGridView grid, bool penjualan)
        {
            if (grid.Columns.Count == 0) return;
            if (grid.Columns["retur_id"] != null) grid.Columns["retur_id"].Visible = false;
            if (grid.Columns["nomor"] != null)
            {
                grid.Columns["nomor"].HeaderText = "No. Retur";
                grid.Columns["nomor"].FillWeight = 80;
            }
            if (grid.Columns["tanggal"] != null)
            {
                grid.Columns["tanggal"].HeaderText = "Tanggal";
                grid.Columns["tanggal"].FillWeight = 85;
                grid.Columns["tanggal"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
            string dokumen = penjualan ? "nomor_penjualan" : "nomor_pembelian";
            if (grid.Columns[dokumen] != null)
            {
                grid.Columns[dokumen].HeaderText = penjualan ? "Faktur Jual" : "No. Pembelian";
                grid.Columns[dokumen].FillWeight = 80;
            }
            if (grid.Columns["alasan"] != null)
            {
                grid.Columns["alasan"].HeaderText = "Alasan";
                grid.Columns["alasan"].FillWeight = 160;
            }
            if (grid.Columns["jumlah_item"] != null)
            {
                grid.Columns["jumlah_item"].HeaderText = "Item";
                grid.Columns["jumlah_item"].FillWeight = 45;
            }
            if (grid.Columns["total"] != null)
            {
                grid.Columns["total"].HeaderText = "Nilai Retur";
                grid.Columns["total"].FillWeight = 90;
                grid.Columns["total"].DefaultCellStyle.Format = "N0";
            }
            if (grid.Columns["status"] != null)
            {
                grid.Columns["status"].HeaderText = "Status";
                grid.Columns["status"].FillWeight = 60;
            }
        }

        // =========================================================
        // HELPER
        // =========================================================
        private static void HitungBaris(DataTable items, int index)
        {
            if (items == null || index < 0 || index >= items.Rows.Count) return;
            DataRow row = items.Rows[index];
            decimal sisa = Angka(row["sisa"]);
            decimal retur = Angka(row["retur"]);
            if (retur < 0) retur = 0;
            if (retur > sisa) retur = sisa;
            row["retur"] = retur;
            row["subtotal"] = retur * Angka(row["harga"]);
        }

        private static void UpdateInfo(ReturTemplate.ReturPane pane, string dokumen)
        {
            decimal sisa = 0m;
            if (pane.ItemsGrid.DataSource is DataTable)
            {
                foreach (DataRow row in ((DataTable)pane.ItemsGrid.DataSource).Rows)
                {
                    sisa += Angka(row["sisa"]);
                }
            }
            pane.Info.Text = "Dokumen " + dokumen + " dimuat. Sisa qty yang bisa diretur: " +
                sisa.ToString("0.###") + ". Isi kolom RETUR bila ada pengembalian.";
            pane.Info.ForeColor = AppTheme.Muted;
        }

        private static string SelectedId(ComboBox combo)
        {
            if (combo.SelectedValue == null) return string.Empty;
            if (combo.SelectedValue is DataRowView)
                return Convert.ToString(((DataRowView)combo.SelectedValue).Row["id"]);
            return Convert.ToString(combo.SelectedValue);
        }

        private static void PilihId(ComboBox combo, string id)
        {
            try
            {
                for (int i = 0; i < combo.Items.Count; i++)
                {
                    DataRowView item = combo.Items[i] as DataRowView;
                    if (item != null && string.Equals(Convert.ToString(item.Row["id"]), id, StringComparison.OrdinalIgnoreCase))
                    {
                        combo.SelectedIndex = i;
                        return;
                    }
                }
                if (combo.Items.Count > 0) combo.SelectedIndex = 0;
            }
            catch { }
        }

        private static object CariQty(DataTable table, string kode)
        {
            if (table == null) return 0m;
            foreach (DataRow row in table.Rows)
            {
                if (string.Equals(Convert.ToString(row["kodebarang"]), kode, StringComparison.OrdinalIgnoreCase))
                    return row["qty"];
            }
            return 0m;
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

        private static decimal Angka(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            decimal hasil;
            return decimal.TryParse(Convert.ToString(value), out hasil) ? hasil : 0m;
        }
    }
}
