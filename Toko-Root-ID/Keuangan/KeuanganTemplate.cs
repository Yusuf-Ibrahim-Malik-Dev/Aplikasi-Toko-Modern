using System;
using System.Drawing;
using System.Windows.Forms;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Keuangan
{
    /// <summary>Template modul keuangan: kas harian, hutang supplier, dan piutang pelanggan.</summary>
    internal sealed class KeuanganTemplate
    {
        internal sealed class KeuanganView
        {
            public UiKit.PageView Page;
            public TabControl Tabs;

            public DateTimePicker StartPicker;
            public DateTimePicker EndPicker;
            public ComboBox TypeCombo;
            public TextBox SearchBox;
            public Button RefreshButton;
            public DataGridView KasGrid;
            public Label SummaryIn;
            public Label SummaryOut;
            public Label SummarySaldo;

            public DateTimePicker KasDate;
            public ComboBox KasType;
            public TextBox KasCategory;
            public TextBox KasNominal;
            public ComboBox KasMethod;
            public TextBox KasNote;
            public Button KasSave;

            public DataGridView HutangGrid;
            public TextBox HutangSearch;
            public Button HutangRefresh;
            public TextBox HutangPayAmount;
            public ComboBox HutangPayMethod;
            public Button HutangPay;
            public Label HutangInfo;

            public DataGridView PiutangGrid;
            public TextBox PiutangSearch;
            public Button PiutangRefresh;
            public TextBox PiutangPayAmount;
            public ComboBox PiutangPayMethod;
            public Button PiutangPay;
            public Label PiutangInfo;
        }

        public static KeuanganView Build(Form form)
        {
            var view = new KeuanganView();
            view.Page = UiKit.Page(form, "Keuangan",
                "Catat kas masuk/keluar, pantau hutang supplier, dan tagih piutang pelanggan.");
            view.Tabs = UiKit.Tabs();
            view.Page.Body.Controls.Add(view.Tabs);

            BuildKas(view);
            BuildDebt(view, "Hutang Supplier");
            BuildReceivable(view);
            return view;
        }

        // =========================================================
        // KAS
        // =========================================================
        private static void BuildKas(KeuanganView view)
        {
            TabPage tab = UiKit.Tab("Kas");
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.Background
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(shell);

            var filter = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(filter, 0, 0);

            filter.Controls.Add(UiKit.FieldLabel("Dari", 0, 2, 80));
            view.StartPicker = UiKit.Date(0, 20, 130);
            view.StartPicker.Value = DateTime.Today.AddDays(-30);
            filter.Controls.Add(view.StartPicker);

            filter.Controls.Add(UiKit.FieldLabel("Sampai", 142, 2, 80));
            view.EndPicker = UiKit.Date(142, 20, 130);
            filter.Controls.Add(view.EndPicker);

            filter.Controls.Add(UiKit.FieldLabel("Tipe", 286, 2, 80));
            view.TypeCombo = UiKit.Combo(286, 20, 120);
            view.TypeCombo.Items.AddRange(new object[] { "Semua", "Masuk", "Keluar" });
            view.TypeCombo.SelectedIndex = 0;
            filter.Controls.Add(view.TypeCombo);

            view.SearchBox = UiKit.Input(418, 20, 220);
            UiKit.SetPlaceholder(view.SearchBox, "Cari kategori / keterangan...");
            filter.Controls.Add(view.SearchBox);

            view.RefreshButton = UiKit.Button("Tampilkan", null, AppTheme.ButtonStyle.Primary);
            view.RefreshButton.Location = new Point(648, 17);
            view.RefreshButton.Size = new Size(104, 34);
            filter.Controls.Add(view.RefreshButton);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.Controls.Add(body, 0, 1);

            var listCard = UiKit.Card(0, 0, 200, 200);
            listCard.Dock = DockStyle.Fill;
            listCard.Margin = new Padding(0, 6, 14, 0);
            listCard.Padding = new Padding(12);
            body.Controls.Add(listCard, 0, 0);

            var listLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = AppTheme.Surface };
            listLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            listLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            listCard.Controls.Add(listLayout);

            var summary = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.Surface
            };
            listLayout.Controls.Add(summary, 0, 0);

            summary.Controls.Add(SummaryBlock("KAS MASUK", out view.SummaryIn, AppTheme.Success));
            summary.Controls.Add(SummaryBlock("KAS KELUAR", out view.SummaryOut, AppTheme.Danger));
            summary.Controls.Add(SummaryBlock("SALDO PERIODE", out view.SummarySaldo, AppTheme.Accent));

            view.KasGrid = UiKit.Grid();
            listLayout.Controls.Add(view.KasGrid, 0, 1);

            var editor = UiKit.Card(0, 0, 340, 200);
            editor.Dock = DockStyle.Fill;
            editor.Margin = new Padding(0, 6, 0, 0);
            editor.Padding = new Padding(16);
            editor.AutoScroll = true;
            body.Controls.Add(editor, 1, 0);

            editor.Controls.Add(UiKit.Section("Catat Kas", 0, 0, 260));

            view.KasDate = UiKit.Date(0, 0, 280);
            UiKit.AddField(editor, "Tanggal", view.KasDate, 0, 36, 280);

            view.KasType = UiKit.Combo(0, 0, 280);
            view.KasType.Items.AddRange(new object[] { "Masuk", "Keluar" });
            view.KasType.SelectedIndex = 0;
            UiKit.AddField(editor, "Tipe", view.KasType, 0, 84, 280);

            view.KasCategory = UiKit.Input(0, 0, 280);
            view.KasCategory.Text = "Penjualan";
            UiKit.AddField(editor, "Kategori", view.KasCategory, 0, 132, 280);

            view.KasNominal = UiKit.Input(0, 0, 280, "0");
            view.KasNominal.TextAlign = HorizontalAlignment.Right;
            UiKit.AddField(editor, "Nominal (Rp)", view.KasNominal, 0, 180, 280);

            view.KasMethod = UiKit.Combo(0, 0, 280);
            view.KasMethod.Items.AddRange(new object[] { "Tunai", "Transfer", "Kartu", "QRIS" });
            view.KasMethod.SelectedIndex = 0;
            UiKit.AddField(editor, "Metode", view.KasMethod, 0, 228, 280);

            view.KasNote = UiKit.Input(0, 0, 280);
            UiKit.AddField(editor, "Keterangan", view.KasNote, 0, 276, 280);

            view.KasSave = UiKit.Button("SIMPAN KAS", null, AppTheme.ButtonStyle.Success);
            view.KasSave.Location = new Point(0, 332);
            view.KasSave.Size = new Size(280, 40);
            editor.Controls.Add(view.KasSave);
        }

        private static Panel SummaryBlock(string label, out Label value, Color color)
        {
            var panel = new Panel { Size = new Size(164, 52), BackColor = AppTheme.Surface, Margin = new Padding(0, 0, 8, 0) };
            panel.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(0, 0),
                Size = new Size(164, 18),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = AppTheme.Muted,
                BackColor = Color.Transparent
            });
            value = new Label
            {
                Text = "Rp 0",
                Location = new Point(0, 18),
                Size = new Size(164, 28),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = color,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(value);
            return panel;
        }

        // =========================================================
        // HUTANG
        // =========================================================
        private static void BuildDebt(KeuanganView view, string title)
        {
            TabPage tab = UiKit.Tab(title);
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = AppTheme.Background };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            tab.Controls.Add(shell);

            var filter = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(filter, 0, 0);

            view.HutangSearch = UiKit.Input(0, 8, 280);
            UiKit.SetPlaceholder(view.HutangSearch, "Cari supplier...");
            filter.Controls.Add(view.HutangSearch);

            view.HutangRefresh = UiKit.Button("Tampilkan", null, AppTheme.ButtonStyle.Primary);
            view.HutangRefresh.Location = new Point(290, 5);
            view.HutangRefresh.Size = new Size(104, 34);
            filter.Controls.Add(view.HutangRefresh);

            var card = UiKit.Card(0, 0, 200, 200);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 4, 0, 10);
            card.Padding = new Padding(12);
            shell.Controls.Add(card, 0, 1);

            var listLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = AppTheme.Surface };
            listLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            listLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            card.Controls.Add(listLayout);

            view.HutangInfo = new Label
            {
                Text = "Pilih baris hutang, lalu isi nominal pembayaran.",
                Dock = DockStyle.Fill,
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                BackColor = AppTheme.Surface
            };
            listLayout.Controls.Add(view.HutangInfo, 0, 0);

            view.HutangGrid = UiKit.Grid();
            listLayout.Controls.Add(view.HutangGrid, 0, 1);

            var payBar = UiKit.Card(0, 0, 200, 76);
            payBar.Dock = DockStyle.Fill;
            payBar.Margin = new Padding(0);
            payBar.Padding = new Padding(14, 8, 14, 8);
            shell.Controls.Add(payBar, 0, 2);

            payBar.Controls.Add(UiKit.FieldLabel("Nominal bayar", 0, 2, 130));
            view.HutangPayAmount = UiKit.Input(0, 20, 160, "0");
            view.HutangPayAmount.TextAlign = HorizontalAlignment.Right;
            payBar.Controls.Add(view.HutangPayAmount);

            payBar.Controls.Add(UiKit.FieldLabel("Metode", 176, 2, 100));
            view.HutangPayMethod = UiKit.Combo(176, 20, 140);
            view.HutangPayMethod.Items.AddRange(new object[] { "Tunai", "Transfer", "Kartu", "QRIS" });
            view.HutangPayMethod.SelectedIndex = 0;
            payBar.Controls.Add(view.HutangPayMethod);

            view.HutangPay = UiKit.Button("BAYAR HUTANG", null, AppTheme.ButtonStyle.Success);
            view.HutangPay.Location = new Point(332, 18);
            view.HutangPay.Size = new Size(160, 34);
            payBar.Controls.Add(view.HutangPay);
        }

        // =========================================================
        // PIUTANG
        // =========================================================
        private static void BuildReceivable(KeuanganView view)
        {
            TabPage tab = UiKit.Tab("Piutang Pelanggan");
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = AppTheme.Background };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            tab.Controls.Add(shell);

            var filter = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(filter, 0, 0);

            view.PiutangSearch = UiKit.Input(0, 8, 280);
            UiKit.SetPlaceholder(view.PiutangSearch, "Cari pelanggan / nomor penjualan...");
            filter.Controls.Add(view.PiutangSearch);

            view.PiutangRefresh = UiKit.Button("Tampilkan", null, AppTheme.ButtonStyle.Primary);
            view.PiutangRefresh.Location = new Point(290, 5);
            view.PiutangRefresh.Size = new Size(104, 34);
            filter.Controls.Add(view.PiutangRefresh);

            var card = UiKit.Card(0, 0, 200, 200);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 4, 0, 10);
            card.Padding = new Padding(12);
            shell.Controls.Add(card, 0, 1);

            var listLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = AppTheme.Surface };
            listLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            listLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            card.Controls.Add(listLayout);

            view.PiutangInfo = new Label
            {
                Text = "Pilih baris piutang, lalu isi nominal pembayaran.",
                Dock = DockStyle.Fill,
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                BackColor = AppTheme.Surface
            };
            listLayout.Controls.Add(view.PiutangInfo, 0, 0);

            view.PiutangGrid = UiKit.Grid();
            listLayout.Controls.Add(view.PiutangGrid, 0, 1);

            var payBar = UiKit.Card(0, 0, 200, 76);
            payBar.Dock = DockStyle.Fill;
            payBar.Margin = new Padding(0);
            payBar.Padding = new Padding(14, 8, 14, 8);
            shell.Controls.Add(payBar, 0, 2);

            payBar.Controls.Add(UiKit.FieldLabel("Nominal bayar", 0, 2, 130));
            view.PiutangPayAmount = UiKit.Input(0, 20, 160, "0");
            view.PiutangPayAmount.TextAlign = HorizontalAlignment.Right;
            payBar.Controls.Add(view.PiutangPayAmount);

            payBar.Controls.Add(UiKit.FieldLabel("Metode", 176, 2, 100));
            view.PiutangPayMethod = UiKit.Combo(176, 20, 140);
            view.PiutangPayMethod.Items.AddRange(new object[] { "Tunai", "Transfer", "Kartu", "QRIS" });
            view.PiutangPayMethod.SelectedIndex = 0;
            payBar.Controls.Add(view.PiutangPayMethod);

            view.PiutangPay = UiKit.Button("TERIMA PIUTANG", null, AppTheme.ButtonStyle.Success);
            view.PiutangPay.Location = new Point(332, 18);
            view.PiutangPay.Size = new Size(160, 34);
            payBar.Controls.Add(view.PiutangPay);
        }
    }
}
