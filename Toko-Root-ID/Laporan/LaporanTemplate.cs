using System;
using System.Drawing;
using System.Windows.Forms;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Laporan
{
    /// <summary>
    /// Template modul laporan: tab monitoring berisi kartu KPI dan chart,
    /// serta tab laporan berisi filter, grid, ekspor, dan cetak.
    /// </summary>
    internal sealed class LaporanTemplate
    {
        internal sealed class LaporanView
        {
            public UiKit.PageView Page;
            public TabControl Tabs;

            public StatCard CardOmzet;
            public StatCard CardTransaksi;
            public StatCard CardLaba;
            public StatCard CardStok;
            public ComboBox TrendCombo;
            public Button ChartRefresh;
            public BarChartControl BarChart;
            public HBarChartControl TopProducts;
            public DonutChartControl StockDonut;
            public Label LegendAman;
            public Label LegendMenipis;
            public Label LegendHabis;
            public DataGridView RecentGrid;

            public DateTimePicker StartPicker;
            public DateTimePicker EndPicker;
            public ComboBox ReportCombo;
            public TextBox SearchBox;
            public Button ShowButton;
            public DataGridView ReportGrid;
            public Label SummaryLabel;
            public Button ExportButton;
            public Button PrintButton;
        }

        public static LaporanView Build(Form form)
        {
            var view = new LaporanView();
            view.Page = UiKit.Page(form, "Laporan dan Monitoring",
                "Pantau performa toko lewat grafik dan cetak laporan operasional.");

            view.ExportButton = UiKit.Button("Ekspor CSV", null, AppTheme.ButtonStyle.Secondary);
            view.ExportButton.Size = new Size(118, 34);
            view.Page.Actions.Controls.Add(view.ExportButton);

            view.PrintButton = UiKit.Button("Cetak Laporan", null, AppTheme.ButtonStyle.Primary);
            view.PrintButton.Size = new Size(132, 34);
            view.Page.Actions.Controls.Add(view.PrintButton);

            view.Tabs = UiKit.Tabs();
            view.Page.Body.Controls.Add(view.Tabs);

            BuildMonitoring(view);
            BuildReport(view);
            return view;
        }

        // =========================================================
        // TAB MONITORING
        // =========================================================
        private static void BuildMonitoring(LaporanView view)
        {
            TabPage tab = UiKit.Tab("Monitoring");
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Background
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            tab.Controls.Add(shell);

            // Baris kartu KPI.
            var cards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.Background,
                Padding = new Padding(0)
            };
            shell.Controls.Add(cards, 0, 0);

            view.CardOmzet = NewCard("Omzet Hari Ini", "Rp 0", "Penjualan hari ini", "Rp", AppTheme.Accent);
            view.CardTransaksi = NewCard("Transaksi Hari Ini", "0", "Struk terbit hari ini", "▤", Color.FromArgb(16, 155, 112));
            view.CardLaba = NewCard("Laba Bulan Ini", "Rp 0", "Laba kotor bulan berjalan", "↗", Color.FromArgb(124, 78, 210));
            view.CardStok = NewCard("Stok Kritis", "0", "Menipis atau habis", "!", Color.FromArgb(230, 126, 34));
            cards.Controls.Add(view.CardOmzet);
            cards.Controls.Add(view.CardTransaksi);
            cards.Controls.Add(view.CardLaba);
            cards.Controls.Add(view.CardStok);

            // Baris grafik.
            var charts = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background
            };
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            charts.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.Controls.Add(charts, 0, 1);

            var trendCard = UiKit.Card(0, 0, 200, 200);
            trendCard.Dock = DockStyle.Fill;
            trendCard.Margin = new Padding(0, 6, 14, 10);
            trendCard.Padding = new Padding(14, 10, 14, 10);
            charts.Controls.Add(trendCard, 0, 0);

            var trendHeader = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = AppTheme.Surface };
            trendCard.Controls.Add(trendHeader);
            trendHeader.Controls.Add(new Label
            {
                Text = "Tren Omzet",
                Dock = DockStyle.Left,
                Width = 220,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = AppTheme.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Surface
            });
            view.TrendCombo = UiKit.Combo(0, 0, 150);
            view.TrendCombo.Dock = DockStyle.Right;
            view.TrendCombo.Width = 150;
            view.TrendCombo.Items.AddRange(new object[] { "7 hari terakhir", "12 bulan terakhir" });
            view.TrendCombo.SelectedIndex = 0;
            trendHeader.Controls.Add(view.TrendCombo);

            view.BarChart = new BarChartControl { Dock = DockStyle.Fill, Accent = AppTheme.Accent };
            trendCard.Controls.Add(view.BarChart);
            view.BarChart.BringToFront();

            var topCard = UiKit.Card(0, 0, 200, 200);
            topCard.Dock = DockStyle.Fill;
            topCard.Margin = new Padding(0, 6, 0, 10);
            topCard.Padding = new Padding(14, 10, 14, 10);
            charts.Controls.Add(topCard, 1, 0);

            topCard.Controls.Add(new Label
            {
                Text = "Produk Terlaris",
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = AppTheme.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Surface
            });

            view.TopProducts = new HBarChartControl { Dock = DockStyle.Fill, Accent = AppTheme.Accent, ValueSuffix = " pcs" };
            topCard.Controls.Add(view.TopProducts);
            view.TopProducts.BringToFront();

            // Baris donut dan transaksi terbaru.
            var bottom = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background
            };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.Controls.Add(bottom, 0, 2);

            var stockCard = UiKit.Card(0, 0, 330, 200);
            stockCard.Dock = DockStyle.Fill;
            stockCard.Margin = new Padding(0, 0, 14, 0);
            stockCard.Padding = new Padding(14, 10, 14, 10);
            bottom.Controls.Add(stockCard, 0, 0);

            stockCard.Controls.Add(new Label
            {
                Text = "Status Stok",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = AppTheme.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Surface
            });

            var legend = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.Surface
            };
            view.LegendAman = LegendLabel("Aman 0", AppTheme.Success);
            view.LegendMenipis = LegendLabel("Menipis 0", AppTheme.Warning);
            view.LegendHabis = LegendLabel("Habis 0", AppTheme.Danger);
            legend.Controls.Add(view.LegendAman);
            legend.Controls.Add(view.LegendMenipis);
            legend.Controls.Add(view.LegendHabis);
            stockCard.Controls.Add(legend);

            view.StockDonut = new DonutChartControl { Dock = DockStyle.Fill, Caption = "Total barang" };
            stockCard.Controls.Add(view.StockDonut);
            view.StockDonut.BringToFront();

            var recentCard = UiKit.Card(0, 0, 200, 200);
            recentCard.Dock = DockStyle.Fill;
            recentCard.Margin = new Padding(0);
            recentCard.Padding = new Padding(14, 10, 14, 10);
            bottom.Controls.Add(recentCard, 1, 0);

            var recentLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = AppTheme.Surface };
            recentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            recentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            recentCard.Controls.Add(recentLayout);

            var recentHeader = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Surface };
            recentLayout.Controls.Add(recentHeader, 0, 0);
            recentHeader.Controls.Add(new Label
            {
                Text = "Transaksi Terbaru",
                Dock = DockStyle.Left,
                Width = 220,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = AppTheme.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Surface
            });
            view.ChartRefresh = UiKit.Button("Muat Ulang", null, AppTheme.ButtonStyle.Secondary);
            view.ChartRefresh.Dock = DockStyle.Right;
            view.ChartRefresh.Width = 112;
            view.ChartRefresh.Height = 28;
            recentHeader.Controls.Add(view.ChartRefresh);

            view.RecentGrid = UiKit.Grid();
            recentLayout.Controls.Add(view.RecentGrid, 0, 1);
        }

        private static StatCard NewCard(string title, string value, string subtitle, string glyph, Color accent)
        {
            return new StatCard
            {
                Title = title,
                Value = value,
                Subtitle = subtitle,
                Glyph = glyph,
                Accent = accent,
                Width = 250,
                Height = 104,
                Margin = new Padding(0, 0, 12, 0)
            };
        }

        private static Label LegendLabel(string text, Color color)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Width = 100,
                Height = 22,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
        }

        // =========================================================
        // TAB LAPORAN
        // =========================================================
        private static void BuildReport(LaporanView view)
        {
            TabPage tab = UiKit.Tab("Laporan");
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Background
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            tab.Controls.Add(shell);

            var filter = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(filter, 0, 0);

            filter.Controls.Add(UiKit.FieldLabel("Dari", 0, 2, 80));
            view.StartPicker = UiKit.Date(0, 20, 130);
            view.StartPicker.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            filter.Controls.Add(view.StartPicker);

            filter.Controls.Add(UiKit.FieldLabel("Sampai", 142, 2, 80));
            view.EndPicker = UiKit.Date(142, 20, 130);
            filter.Controls.Add(view.EndPicker);

            filter.Controls.Add(UiKit.FieldLabel("Jenis laporan", 286, 2, 140));
            view.ReportCombo = UiKit.Combo(286, 20, 180);
            view.ReportCombo.Items.AddRange(new object[]
            {
                "Penjualan", "Laba Harian", "Produk Terjual", "Pembelian",
                "Stok Barang", "Kas", "Hutang", "Piutang"
            });
            view.ReportCombo.SelectedIndex = 0;
            filter.Controls.Add(view.ReportCombo);

            view.SearchBox = UiKit.Input(478, 20, 220);
            UiKit.SetPlaceholder(view.SearchBox, "Cari nomor / pelanggan...");
            filter.Controls.Add(view.SearchBox);

            view.ShowButton = UiKit.Button("Tampilkan", null, AppTheme.ButtonStyle.Primary);
            view.ShowButton.Location = new Point(708, 17);
            view.ShowButton.Size = new Size(106, 34);
            filter.Controls.Add(view.ShowButton);

            var card = UiKit.Card(0, 0, 200, 200);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 6, 0, 4);
            card.Padding = new Padding(12);
            shell.Controls.Add(card, 0, 1);

            view.ReportGrid = UiKit.Grid();
            card.Controls.Add(view.ReportGrid);

            view.SummaryLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Pilih periode lalu tekan Tampilkan.",
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Background
            };
            shell.Controls.Add(view.SummaryLabel, 0, 2);
        }
    }
}
