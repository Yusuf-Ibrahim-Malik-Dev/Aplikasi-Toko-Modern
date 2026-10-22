using System;
using System.Drawing;
using System.Windows.Forms;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Pembelian
{
    /// <summary>Template modul pembelian: entri penerimaan barang dan riwayat pembelian.</summary>
    internal sealed class PembelianTemplate
    {
        internal sealed class PembelianView
        {
            public UiKit.PageView Page;
            public TabControl Tabs;

            // Entri pembelian
            public TextBox NomorBox;
            public DateTimePicker DatePicker;
            public ComboBox SupplierCombo;
            public ComboBox StatusCombo;
            public TextBox NoteBox;
            public TextBox ProductSearch;
            public Button ProductRefresh;
            public DataGridView ProductGrid;
            public DataGridView CartGrid;
            public Label SubtotalLabel;
            public TextBox DiscountBox;
            public Label TotalLabel;
            public TextBox PaidBox;
            public Label SisaLabel;
            public Button SaveButton;
            public Button ClearButton;
            public Button PrintButton;
            public Label StatusLabel;

            // Riwayat
            public DateTimePicker StartPicker;
            public DateTimePicker EndPicker;
            public TextBox HistorySearch;
            public Button HistoryRefresh;
            public Button ShowDetailButton;
            public DataGridView HistoryGrid;
            public DataGridView DetailGrid;
            public Label DetailLabel;
        }

        public static PembelianView Build(Form form)
        {
            var view = new PembelianView();
            view.Page = UiKit.Page(form, "Pembelian",
                "Catat penerimaan barang dari supplier, pantau hutang, dan cetak bukti pembelian.");
            view.Tabs = UiKit.Tabs();
            view.Page.Body.Controls.Add(view.Tabs);

            BuildEntryTab(view);
            BuildHistoryTab(view);
            return view;
        }

        // =========================================================
        // TAB ENTRI PEMBELIAN
        // =========================================================
        private static void BuildEntryTab(PembelianView view)
        {
            TabPage tab = UiKit.Tab("Pembelian Baru");
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.Background
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(shell);

            var header = UiKit.Card(0, 0, 200, 88);
            header.Dock = DockStyle.Fill;
            header.Margin = new Padding(0, 0, 0, 12);
            header.Padding = new Padding(14, 8, 14, 8);
            shell.Controls.Add(header, 0, 0);

            var fields = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.Surface
            };
            header.Controls.Add(fields);

            view.NomorBox = UiKit.Input(0, 0, 150);
            view.NomorBox.ReadOnly = true;
            view.NomorBox.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            fields.Controls.Add(FieldBlock("No. pembelian", view.NomorBox, 158));

            view.DatePicker = UiKit.Date(0, 0, 130);
            fields.Controls.Add(FieldBlock("Tanggal", view.DatePicker, 140));

            view.SupplierCombo = UiKit.Combo(0, 0, 240);
            fields.Controls.Add(FieldBlock("Supplier", view.SupplierCombo, 250));

            view.StatusCombo = UiKit.Combo(0, 0, 140);
            view.StatusCombo.Items.AddRange(new object[] { "Received", "Draft" });
            view.StatusCombo.SelectedIndex = 0;
            fields.Controls.Add(FieldBlock("Status stok", view.StatusCombo, 150));

            view.NoteBox = UiKit.Input(0, 0, 250);
            fields.Controls.Add(FieldBlock("Catatan", view.NoteBox, 260));

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 470));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.Controls.Add(body, 0, 1);

            // Panel produk.
            var productCard = UiKit.Card(0, 0, 200, 200);
            productCard.Dock = DockStyle.Fill;
            productCard.Margin = new Padding(0, 0, 14, 0);
            productCard.Padding = new Padding(12);
            body.Controls.Add(productCard, 0, 0);

            var productLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Surface
            };
            productLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            productLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            productLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            productCard.Controls.Add(productLayout);

            var productToolbar = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Surface };
            view.ProductSearch = UiKit.Input(0, 0, 300);
            view.ProductSearch.Font = new Font("Segoe UI", 10F);
            UiKit.SetPlaceholder(view.ProductSearch, "Cari kode / nama / barcode...");
            view.ProductSearch.Location = new Point(0, 6);
            view.ProductSearch.Width = 300;
            productToolbar.Controls.Add(view.ProductSearch);
            view.ProductRefresh = UiKit.Button("Muat Ulang", null, AppTheme.ButtonStyle.Secondary);
            view.ProductRefresh.Location = new Point(308, 4);
            view.ProductRefresh.Size = new Size(108, 32);
            productToolbar.Controls.Add(view.ProductRefresh);
            productLayout.Controls.Add(productToolbar, 0, 0);

            view.ProductGrid = UiKit.Grid();
            view.ProductGrid.AutoGenerateColumns = false;
            AddProductColumns(view.ProductGrid);
            productLayout.Controls.Add(view.ProductGrid, 0, 1);

            productLayout.Controls.Add(new Label
            {
                Text = "Klik dua kali barang untuk menambahkannya ke daftar pembelian.",
                Dock = DockStyle.Fill,
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Surface
            }, 0, 2);

            // Panel keranjang pembelian.
            var cartCard = UiKit.Card(0, 0, 470, 200);
            cartCard.Dock = DockStyle.Fill;
            cartCard.Margin = new Padding(0);
            cartCard.Padding = new Padding(12);
            body.Controls.Add(cartCard, 1, 0);

            var cartLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Surface
            };
            cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            cartLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 262));
            cartCard.Controls.Add(cartLayout);

            cartLayout.Controls.Add(new Label
            {
                Text = "DAFTAR PEMBELIAN",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppTheme.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Surface
            }, 0, 0);

            view.CartGrid = UiKit.Grid();
            view.CartGrid.ReadOnly = false;
            view.CartGrid.EditMode = DataGridViewEditMode.EditOnEnter;
            view.CartGrid.AutoGenerateColumns = false;
            AddCartColumns(view.CartGrid);
            cartLayout.Controls.Add(view.CartGrid, 0, 1);

            var summary = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Surface };
            cartLayout.Controls.Add(summary, 0, 2);

            summary.Controls.Add(UiKit.FieldLabel("Subtotal", 0, 4, 120));
            view.SubtotalLabel = ValueLabel("Rp 0", 140, 0, AppTheme.Text);
            summary.Controls.Add(view.SubtotalLabel);

            summary.Controls.Add(UiKit.FieldLabel("Diskon (Rp)", 0, 36, 120));
            view.DiscountBox = UiKit.Input(140, 32, 120, "0");
            view.DiscountBox.TextAlign = HorizontalAlignment.Right;
            summary.Controls.Add(view.DiscountBox);

            summary.Controls.Add(UiKit.FieldLabel("TOTAL", 0, 70, 120));
            view.TotalLabel = new Label
            {
                Text = "Rp 0",
                Location = new Point(120, 62),
                Size = new Size(310, 34),
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = AppTheme.Accent,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };
            summary.Controls.Add(view.TotalLabel);

            summary.Controls.Add(UiKit.FieldLabel("Dibayar", 0, 104, 120));
            view.PaidBox = UiKit.Input(140, 100, 120, "0");
            view.PaidBox.TextAlign = HorizontalAlignment.Right;
            summary.Controls.Add(view.PaidBox);

            summary.Controls.Add(UiKit.FieldLabel("Sisa hutang", 270, 104, 100));
            view.SisaLabel = ValueLabel("Rp 0", 250, 100, AppTheme.Danger);
            summary.Controls.Add(view.SisaLabel);

            view.SaveButton = UiKit.Button("SIMPAN PEMBELIAN", null, AppTheme.ButtonStyle.Success);
            view.SaveButton.Location = new Point(0, 142);
            view.SaveButton.Size = new Size(190, 42);
            summary.Controls.Add(view.SaveButton);

            view.ClearButton = UiKit.Button("Kosongkan", null, AppTheme.ButtonStyle.Secondary);
            view.ClearButton.Location = new Point(198, 142);
            view.ClearButton.Size = new Size(110, 42);
            summary.Controls.Add(view.ClearButton);

            view.PrintButton = UiKit.Button("Cetak", null, AppTheme.ButtonStyle.Ghost);
            view.PrintButton.Location = new Point(316, 142);
            view.PrintButton.Size = new Size(120, 42);
            summary.Controls.Add(view.PrintButton);

            view.StatusLabel = new Label
            {
                Text = "Siap mencatat pembelian.",
                Location = new Point(0, 194),
                Size = new Size(440, 22),
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Surface
            };
            summary.Controls.Add(view.StatusLabel);
        }

        // =========================================================
        // TAB RIWAYAT
        // =========================================================
        private static void BuildHistoryTab(PembelianView view)
        {
            TabPage tab = UiKit.Tab("Riwayat");
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Background
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            tab.Controls.Add(shell);

            var filterBar = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(filterBar, 0, 0);

            filterBar.Controls.Add(UiKit.FieldLabel("Dari tanggal", 0, 2, 110));
            view.StartPicker = UiKit.Date(0, 20, 130);
            view.StartPicker.Value = DateTime.Today.AddDays(-30);
            filterBar.Controls.Add(view.StartPicker);

            filterBar.Controls.Add(UiKit.FieldLabel("Sampai", 142, 2, 90));
            view.EndPicker = UiKit.Date(142, 20, 130);
            filterBar.Controls.Add(view.EndPicker);

            view.HistorySearch = UiKit.Input(286, 20, 230);
            UiKit.SetPlaceholder(view.HistorySearch, "Cari nomor / supplier...");
            filterBar.Controls.Add(view.HistorySearch);

            view.HistoryRefresh = UiKit.Button("Tampilkan", null, AppTheme.ButtonStyle.Primary);
            view.HistoryRefresh.Location = new Point(524, 17);
            view.HistoryRefresh.Size = new Size(106, 34);
            filterBar.Controls.Add(view.HistoryRefresh);

            view.ShowDetailButton = UiKit.Button("Lihat Detail", null, AppTheme.ButtonStyle.Secondary);
            view.ShowDetailButton.Location = new Point(638, 17);
            view.ShowDetailButton.Size = new Size(112, 34);
            filterBar.Controls.Add(view.ShowDetailButton);

            var historyCard = UiKit.Card(0, 0, 200, 200);
            historyCard.Dock = DockStyle.Fill;
            historyCard.Margin = new Padding(0, 6, 0, 12);
            historyCard.Padding = new Padding(12);
            shell.Controls.Add(historyCard, 0, 1);

            view.HistoryGrid = UiKit.Grid();
            historyCard.Controls.Add(view.HistoryGrid);

            var detailCard = UiKit.Card(0, 0, 200, 200);
            detailCard.Dock = DockStyle.Fill;
            detailCard.Margin = new Padding(0);
            detailCard.Padding = new Padding(12);
            shell.Controls.Add(detailCard, 0, 2);

            var detailLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.Surface
            };
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            detailCard.Controls.Add(detailLayout);

            view.DetailLabel = new Label
            {
                Text = "Detail pembelian",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppTheme.Text,
                BackColor = AppTheme.Surface
            };
            detailLayout.Controls.Add(view.DetailLabel, 0, 0);

            view.DetailGrid = UiKit.Grid();
            detailLayout.Controls.Add(view.DetailGrid, 0, 1);
        }

        // =========================================================
        // KOMPONEN
        // =========================================================
        private static Panel FieldBlock(string label, Control input, int width)
        {
            var panel = new Panel { Size = new Size(width, 62), BackColor = AppTheme.Surface, Margin = new Padding(0, 0, 10, 0) };
            panel.Controls.Add(UiKit.FieldLabel(label, 0, 2, width));
            input.Location = new Point(0, 22);
            input.Width = width - 4;
            panel.Controls.Add(input);
            return panel;
        }

        private static Label ValueLabel(string text, int x, int y, Color color)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(300, 28),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };
        }

        private static void AddProductColumns(DataGridView grid)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "kodebarang", HeaderText = "Kode", DataPropertyName = "kodebarang", FillWeight = 70 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "namabarang", HeaderText = "Nama Barang", DataPropertyName = "namabarang", FillWeight = 170 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "satuan", HeaderText = "Satuan", DataPropertyName = "satuan", FillWeight = 60 });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "harga_beli",
                HeaderText = "H. Beli",
                DataPropertyName = "harga_beli",
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
        }

        private static void AddCartColumns(DataGridView grid)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "kodebarang", HeaderText = "Kode", FillWeight = 65, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "namabarang", HeaderText = "Barang", FillWeight = 130, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "qty",
                HeaderText = "Qty",
                FillWeight = 45,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.###", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "harga_beli",
                HeaderText = "Harga",
                FillWeight = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "diskon",
                HeaderText = "Disk",
                FillWeight = 50,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "subtotal",
                HeaderText = "Subtotal",
                FillWeight = 85,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
        }
    }
}
