using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Laporan
{
    /// <summary>Form laporan dan monitoring: KPI, grafik penjualan, produk terlaris, dan ekspor.</summary>
    public class frmLaporan : Form
    {
        private readonly LaporanTemplate.LaporanView _view;
        private readonly DALaporan _laporan = new DALaporan();

        private DataTable _report;
        private DataTable _printData;
        private int _printRow;
        private int _printPage = 1;
        private string _printTitle = "LAPORAN";

        public frmLaporan()
        {
            _view = LaporanTemplate.Build(this);
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleBuffered = true;

            PasangEvent();
            MuatDashboard();
            MuatGrafik();
            MuatLaporan();
        }

        private void PasangEvent()
        {
            _view.ChartRefresh.Click += delegate { MuatDashboard(); MuatGrafik(); };
            _view.TrendCombo.SelectedIndexChanged += delegate { MuatGrafik(); };

            _view.ShowButton.Click += delegate { MuatLaporan(); };
            _view.SearchBox.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                MuatLaporan();
            };
            _view.ReportCombo.SelectedIndexChanged += delegate { MuatLaporan(); };
            _view.ExportButton.Click += delegate { EksporCsv(); };
            _view.PrintButton.Click += delegate { Cetak(); };
        }

        // =========================================================
        // MONITORING
        // =========================================================
        private void MuatDashboard()
        {
            try
            {
                ClassDashboard data = _laporan.GetDashboard();
                _view.CardOmzet.Value = UiKit.Rupiah(data.omzet_hari_ini);
                _view.CardTransaksi.Value = data.transaksi_hari_ini.ToString("N0");
                _view.CardLaba.Value = UiKit.Rupiah(data.laba_bulan_ini);
                _view.CardStok.Value = (data.stok_menipis + data.stok_habis).ToString("N0");

                _view.StockDonut.Aman = Math.Max(0, data.jumlah_produk - data.stok_menipis - data.stok_habis);
                _view.StockDonut.Menipis = data.stok_menipis;
                _view.StockDonut.Habis = data.stok_habis;
                _view.StockDonut.Caption = "Total barang";
                _view.StockDonut.Invalidate();

                _view.LegendAman.Text = "Aman " + _view.StockDonut.Aman;
                _view.LegendMenipis.Text = "Menipis " + _view.StockDonut.Menipis;
                _view.LegendHabis.Text = "Habis " + _view.StockDonut.Habis;
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void MuatGrafik()
        {
            try
            {
                bool bulanan = _view.TrendCombo.SelectedIndex == 1;
                List<ClassChartPoint> tren = bulanan
                    ? _laporan.GetPenjualan12Bulan()
                    : _laporan.GetPenjualan7Hari();
                _view.BarChart.SetData(tren);

                DataTable top = _laporan.GetProdukTerlaris(5);
                var poin = new List<ClassChartPoint>();
                foreach (DataRow row in top.Rows)
                    poin.Add(new ClassChartPoint(Convert.ToString(row["namabarang"]), Decimal(row["qty_terjual"])));
                _view.TopProducts.SetData(poin);

                DataTable terbaru = _laporan.GetTransaksiTerbaru(8);
                _view.RecentGrid.DataSource = terbaru;
                FormatTerbaru();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void FormatTerbaru()
        {
            DataGridView grid = _view.RecentGrid;
            if (grid.Columns.Count == 0) return;
            Header(grid, "nofaktur", "No. Faktur", 90);
            Header(grid, "tanggal", "Tanggal", 110);
            Header(grid, "pelanggan", "Pelanggan", 130);
            Header(grid, "metode_bayar", "Metode", 80);
            Header(grid, "total", "Total", 100);
            if (grid.Columns["tanggal"] != null) grid.Columns["tanggal"].DefaultCellStyle.Format = "dd/MM HH:mm";
            if (grid.Columns["total"] != null) grid.Columns["total"].DefaultCellStyle.Format = "N0";
        }

        // =========================================================
        // LAPORAN
        // =========================================================
        private void MuatLaporan()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                DateTime mulai = _view.StartPicker.Value.Date;
                DateTime akhir = _view.EndPicker.Value.Date;
                string keyword = UiKit.SearchText(_view.SearchBox);
                string jenis = Convert.ToString(_view.ReportCombo.SelectedItem);

                switch (jenis)
                {
                    case "Laba Harian": _report = _laporan.GetLaporanLaba(mulai, akhir); break;
                    case "Produk Terjual": _report = _laporan.GetLaporanProduk(mulai, akhir); break;
                    case "Pembelian": _report = _laporan.GetLaporanPembelian(mulai, akhir); break;
                    case "Stok Barang": _report = _laporan.GetLaporanStok(false); break;
                    case "Kas": _report = _laporan.GetLaporanKas(mulai, akhir); break;
                    case "Hutang": _report = _laporan.GetLaporanHutang(); break;
                    case "Piutang": _report = _laporan.GetLaporanPiutang(); break;
                    default: _report = _laporan.GetLaporanPenjualan(mulai, akhir, keyword); break;
                }

                _view.ReportGrid.DataSource = _report;
                FormatLaporan();
                _view.SummaryLabel.Text = _report.Rows.Count.ToString("N0") + " baris data - " + jenis +
                    " (" + mulai.ToString("dd MMM yyyy") + " s/d " + akhir.ToString("dd MMM yyyy") + ")";
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void FormatLaporan()
        {
            DataGridView grid = _view.ReportGrid;
            if (grid.Columns.Count == 0) return;

            foreach (string column in new[]
            {
                "total", "subtotal", "diskon", "dibayar", "kembali", "omzet", "laba_kotor", "hpp",
                "nominal", "terbayar", "sisa", "sisa_pembayaran", "harga_beli", "hargasatuan"
            })
            {
                if (grid.Columns[column] != null)
                    grid.Columns[column].DefaultCellStyle.Format = "N0";
            }

            foreach (string column in new[] { "tanggal", "jatuh_tempo" })
            {
                if (grid.Columns[column] != null && grid.Columns[column].ValueType != typeof(string))
                    grid.Columns[column].DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            foreach (string column in new[] { "jumlah_transaksi", "transaksi", "qty_terjual", "qty" })
            {
                if (grid.Columns[column] != null)
                    grid.Columns[column].DefaultCellStyle.Format = "N0";
            }

            foreach (string column in new[] { "stok", "stok_min" })
            {
                if (grid.Columns[column] != null)
                    grid.Columns[column].DefaultCellStyle.Format = "0.###";
            }

            if (grid.Columns["nofaktur"] != null) grid.Columns["nofaktur"].HeaderText = "No. Faktur";
            if (grid.Columns["nomor"] != null) grid.Columns["nomor"].HeaderText = "Nomor";
            if (grid.Columns["kodebarang"] != null) grid.Columns["kodebarang"].HeaderText = "Kode";
            if (grid.Columns["namabarang"] != null) grid.Columns["namabarang"].HeaderText = "Nama Barang";
        }

        // =========================================================
        // EKSPOR & CETAK
        // =========================================================
        private void EksporCsv()
        {
            if (_report == null || _report.Rows.Count == 0)
            {
                UiKit.ShowInfo(this, "Tampilkan laporan terlebih dahulu sebelum mengekspor.");
                return;
            }

            using (var dialog = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = "Laporan_" + Convert.ToString(_view.ReportCombo.SelectedItem).Replace(' ', '_') +
                           "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                var sb = new StringBuilder();
                for (int c = 0; c < _report.Columns.Count; c++)
                {
                    if (c > 0) sb.Append(';');
                    sb.Append(Escape(_report.Columns[c].ColumnName));
                }
                sb.AppendLine();
                foreach (DataRow row in _report.Rows)
                {
                    for (int c = 0; c < _report.Columns.Count; c++)
                    {
                        if (c > 0) sb.Append(';');
                        sb.Append(Escape(Convert.ToString(row[c])));
                    }
                    sb.AppendLine();
                }
                File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(true));
                UiKit.ShowInfo(this, "CSV berhasil disimpan:\n" + dialog.FileName);
            }
        }

        private static string Escape(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private void Cetak()
        {
            if (_report == null || _report.Rows.Count == 0)
            {
                UiKit.ShowInfo(this, "Tidak ada data untuk dicetak.");
                return;
            }

            _printData = _report;
            _printRow = 0;
            _printTitle = "LAPORAN " + Convert.ToString(_view.ReportCombo.SelectedItem).ToUpperInvariant();

            var doc = new PrintDocument();
            doc.DocumentName = _printTitle;
            doc.DefaultPageSettings.Landscape = true;
            doc.BeginPrint += delegate { _printRow = 0; _printPage = 1; };
            doc.PrintPage += PrintPage;

            using (var preview = new PrintPreviewDialog { Document = doc, Width = 1000, Height = 720 })
                preview.ShowDialog(this);
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;
            float width = e.MarginBounds.Width;

            using (var titleFont = new Font("Segoe UI", 13F, FontStyle.Bold))
            using (var smallFont = new Font("Segoe UI", 8F))
            using (var headFont = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var bodyFont = new Font("Segoe UI", 8F))
            using (var line = new Pen(Color.Gray))
            {
                e.Graphics.DrawString(_printTitle, titleFont, Brushes.Black, x, y); y += 28;
                e.Graphics.DrawString("Periode: " + _view.StartPicker.Value.ToString("dd-MM-yyyy") +
                    " s/d " + _view.EndPicker.Value.ToString("dd-MM-yyyy") +
                    "    Dicetak: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm"), smallFont, Brushes.Black, x, y);
                y += 22;

                int count = _printData.Columns.Count;
                float cellWidth = Math.Max(50f, width / Math.Max(1, count));
                for (int c = 0; c < count; c++)
                    e.Graphics.DrawString(_printData.Columns[c].ColumnName, headFont, Brushes.Black,
                        new RectangleF(x + c * cellWidth, y, cellWidth - 4, 16));
                y += 17;
                e.Graphics.DrawLine(line, x, y, x + width, y);
                y += 4;

                while (_printRow < _printData.Rows.Count && y < e.MarginBounds.Bottom - 20)
                {
                    for (int c = 0; c < count; c++)
                    {
                        string value = Convert.ToString(_printData.Rows[_printRow][c]);
                        if (value.Length > 26) value = value.Substring(0, 23) + "...";
                        e.Graphics.DrawString(value, bodyFont, Brushes.Black,
                            new RectangleF(x + c * cellWidth, y, cellWidth - 4, 16));
                    }
                    y += 17;
                    _printRow++;
                }

                e.Graphics.DrawString("Halaman " + _printPage, smallFont, Brushes.Black, x, e.MarginBounds.Bottom - 14);
                e.HasMorePages = _printRow < _printData.Rows.Count;
                if (e.HasMorePages) _printPage++;
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

        private static decimal Decimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            decimal hasil;
            return decimal.TryParse(Convert.ToString(value), out hasil) ? hasil : 0m;
        }
    }
}
