using System;
using System.Data;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Helper;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Keuangan
{
    /// <summary>Form keuangan: kas, pembayaran hutang supplier, dan penerimaan piutang.</summary>
    public class frmKeuangan : Form
    {
        private readonly KeuanganTemplate.KeuanganView _view;
        private readonly DAKeuangan _keuangan = new DAKeuangan();

        public frmKeuangan()
        {
            _view = KeuanganTemplate.Build(this);
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleBuffered = true;

            PasangEvent();
            MuatKas();
            MuatHutang();
            MuatPiutang();
        }

        private void PasangEvent()
        {
            _view.RefreshButton.Click += delegate { MuatKas(); };
            _view.SearchBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                MuatKas();
            };
            _view.KasSave.Click += delegate { SimpanKas(); };

            _view.HutangRefresh.Click += delegate { MuatHutang(); };
            _view.HutangSearch.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                MuatHutang();
            };
            _view.HutangGrid.CellClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                DataRowView row = _view.HutangGrid.Rows[e.RowIndex].DataBoundItem as DataRowView;
                if (row == null) return;
                _view.HutangPayAmount.Text = Convert.ToString(row["sisa"]);
                _view.HutangInfo.Text = "Hutang " + Convert.ToString(row["supplier"]) +
                    " - sisa " + UiKit.Rupiah(Angka(row["sisa"]));
            };
            _view.HutangPay.Click += delegate { BayarHutang(); };

            _view.PiutangRefresh.Click += delegate { MuatPiutang(); };
            _view.PiutangSearch.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                MuatPiutang();
            };
            _view.PiutangGrid.CellClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                DataRowView row = _view.PiutangGrid.Rows[e.RowIndex].DataBoundItem as DataRowView;
                if (row == null) return;
                _view.PiutangPayAmount.Text = Convert.ToString(row["sisa"]);
                _view.PiutangInfo.Text = "Piutang " + Convert.ToString(row["pelanggan"]) +
                    " - sisa " + UiKit.Rupiah(Angka(row["sisa"]));
            };
            _view.PiutangPay.Click += delegate { BayarPiutang(); };
        }

        // =========================================================
        // KAS
        // =========================================================
        private void MuatKas()
        {
            try
            {
                string tipe = Convert.ToString(_view.TypeCombo.SelectedItem);
                if (string.Equals(tipe, "Semua", StringComparison.OrdinalIgnoreCase)) tipe = null;

                DataTable dt = _keuangan.GetKas(_view.StartPicker.Value.Date, _view.EndPicker.Value.Date,
                    tipe, UiKit.SearchText(_view.SearchBox));
                _view.KasGrid.DataSource = dt;
                FormatKas();

                decimal masuk = 0m, keluar = 0m;
                foreach (DataRow row in dt.Rows)
                {
                    if (string.Equals(Convert.ToString(row["tipe"]), "Masuk", StringComparison.OrdinalIgnoreCase))
                        masuk += Angka(row["nominal"]);
                    else
                        keluar += Angka(row["nominal"]);
                }

                _view.SummaryIn.Text = UiKit.Rupiah(masuk);
                _view.SummaryOut.Text = UiKit.Rupiah(keluar);
                _view.SummarySaldo.Text = UiKit.Rupiah(masuk - keluar);
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void FormatKas()
        {
            DataGridView grid = _view.KasGrid;
            if (grid.Columns.Count == 0) return;
            Header(grid, "tanggal", "Tanggal", 100);
            Header(grid, "tipe", "Tipe", 70);
            Header(grid, "kategori", "Kategori", 130);
            Header(grid, "nominal", "Nominal", 100);
            Header(grid, "metode_bayar", "Metode", 80);
            Header(grid, "referensi", "Referensi", 110);
            Header(grid, "keterangan", "Keterangan", 190);
            Header(grid, "created_by", "Oleh", 80);
            if (grid.Columns["kas_id"] != null) grid.Columns["kas_id"].Visible = false;
            if (grid.Columns["tanggal"] != null) grid.Columns["tanggal"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (grid.Columns["nominal"] != null) grid.Columns["nominal"].DefaultCellStyle.Format = "N0";
        }

        private void SimpanKas()
        {
            try
            {
                var kas = new ClassKas
                {
                    tanggal = _view.KasDate.Value,
                    tipe = Convert.ToString(_view.KasType.SelectedItem),
                    kategori = _view.KasCategory.Text.Trim(),
                    nominal = UiKit.ToDecimal(_view.KasNominal),
                    metode_bayar = Convert.ToString(_view.KasMethod.SelectedItem),
                    keterangan = _view.KasNote.Text.Trim(),
                    created_by = NamaUser()
                };
                UiKit.ShowInfo(this, _keuangan.SimpanKas(kas));
                _view.KasNominal.Text = "0";
                _view.KasNote.Clear();
                MuatKas();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        // =========================================================
        // HUTANG
        // =========================================================
        private void MuatHutang()
        {
            try
            {
                DataTable dt = _keuangan.GetHutang(true, UiKit.SearchText(_view.HutangSearch));
                _view.HutangGrid.DataSource = dt;
                FormatHutang(dt);
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void FormatHutang(DataTable dt)
        {
            DataGridView grid = _view.HutangGrid;
            if (grid.Columns.Count == 0) return;
            Header(grid, "supplier", "Supplier", 150);
            Header(grid, "tanggal", "Tanggal", 100);
            Header(grid, "jatuh_tempo", "Jatuh Tempo", 100);
            Header(grid, "nominal", "Nominal", 100);
            Header(grid, "terbayar", "Terbayar", 100);
            Header(grid, "sisa", "Sisa", 100);
            Header(grid, "status", "Status", 90);
            if (grid.Columns["hutang_id"] != null) grid.Columns["hutang_id"].Visible = false;
            foreach (string column in new[] { "nominal", "terbayar", "sisa" })
                if (grid.Columns[column] != null) grid.Columns[column].DefaultCellStyle.Format = "N0";
            foreach (string column in new[] { "tanggal", "jatuh_tempo" })
                if (grid.Columns[column] != null) grid.Columns[column].DefaultCellStyle.Format = "dd/MM/yyyy";

            decimal total = 0m;
            foreach (DataRow row in dt.Rows) total += Angka(row["sisa"]);
            _view.HutangInfo.Text = dt.Rows.Count + " hutang terbuka, total sisa " + UiKit.Rupiah(total) + ".";
        }

        private void BayarHutang()
        {
            if (_view.HutangGrid.CurrentRow == null)
            {
                UiKit.ShowInfo(this, "Pilih baris hutang terlebih dahulu.");
                return;
            }
            DataRowView row = _view.HutangGrid.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) return;

            decimal nominal = UiKit.ToDecimal(_view.HutangPayAmount);
            decimal sisa = Angka(row["sisa"]);
            if (nominal <= 0 || nominal > sisa)
            {
                UiKit.ShowInfo(this, "Nominal pembayaran harus lebih dari nol dan tidak melebihi sisa hutang.");
                return;
            }

            try
            {
                UiKit.ShowInfo(this, _keuangan.BayarHutang(Convert.ToInt64(row["hutang_id"]), nominal,
                    Convert.ToString(_view.HutangPayMethod.SelectedItem), null));
                _view.HutangPayAmount.Text = "0";
                MuatHutang();
                MuatKas();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        // =========================================================
        // PIUTANG
        // =========================================================
        private void MuatPiutang()
        {
            try
            {
                DataTable dt = _keuangan.GetPiutang(true, UiKit.SearchText(_view.PiutangSearch));
                _view.PiutangGrid.DataSource = dt;
                FormatPiutang(dt);
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void FormatPiutang(DataTable dt)
        {
            DataGridView grid = _view.PiutangGrid;
            if (grid.Columns.Count == 0) return;
            Header(grid, "pelanggan", "Pelanggan", 140);
            Header(grid, "nomor_penjualan", "No. Jual", 100);
            Header(grid, "tanggal", "Tanggal", 100);
            Header(grid, "jatuh_tempo", "Jatuh Tempo", 100);
            Header(grid, "nominal", "Nominal", 100);
            Header(grid, "terbayar", "Terbayar", 100);
            Header(grid, "sisa", "Sisa", 100);
            Header(grid, "status", "Status", 90);
            if (grid.Columns["piutang_id"] != null) grid.Columns["piutang_id"].Visible = false;
            foreach (string column in new[] { "nominal", "terbayar", "sisa" })
                if (grid.Columns[column] != null) grid.Columns[column].DefaultCellStyle.Format = "N0";
            foreach (string column in new[] { "tanggal", "jatuh_tempo" })
                if (grid.Columns[column] != null) grid.Columns[column].DefaultCellStyle.Format = "dd/MM/yyyy";

            decimal total = 0m;
            foreach (DataRow row in dt.Rows) total += Angka(row["sisa"]);
            _view.PiutangInfo.Text = dt.Rows.Count + " piutang terbuka, total sisa " + UiKit.Rupiah(total) + ".";
        }

        private void BayarPiutang()
        {
            if (_view.PiutangGrid.CurrentRow == null)
            {
                UiKit.ShowInfo(this, "Pilih baris piutang terlebih dahulu.");
                return;
            }
            DataRowView row = _view.PiutangGrid.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) return;

            decimal nominal = UiKit.ToDecimal(_view.PiutangPayAmount);
            decimal sisa = Angka(row["sisa"]);
            if (nominal <= 0 || nominal > sisa)
            {
                UiKit.ShowInfo(this, "Nominal penerimaan harus lebih dari nol dan tidak melebihi sisa piutang.");
                return;
            }

            try
            {
                UiKit.ShowInfo(this, _keuangan.BayarPiutang(Convert.ToInt64(row["piutang_id"]), nominal,
                    Convert.ToString(_view.PiutangPayMethod.SelectedItem), null));
                _view.PiutangPayAmount.Text = "0";
                MuatPiutang();
                MuatKas();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        // =========================================================
        // HELPER
        // =========================================================
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

        private static decimal Angka(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            decimal hasil;
            return decimal.TryParse(Convert.ToString(value), out hasil) ? hasil : 0m;
        }
    }
}
