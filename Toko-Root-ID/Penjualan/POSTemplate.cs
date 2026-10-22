using System;
using System.Drawing;
using System.Windows.Forms;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Penjualan
{
    /// <summary>
    /// Template layar kasir modern: katalog produk di kiri, keranjang dan pembayaran di kanan.
    /// Form frmPOS hanya menangani data, validasi, dan proses simpan.
    /// </summary>
    internal sealed class POSTemplate
    {
        internal sealed class PosView
        {
            public TextBox SearchBox;
            public Button SearchButton;
            public Button RefreshButton;
            public DataGridView ProductGrid;
            public DataGridView CartGrid;
            public Label SubtotalLabel;
            public TextBox DiscountInput;
            public Label TotalLabel;
            public TextBox PaidInput;
            public Label ChangeLabel;
            public ComboBox PaymentCombo;
            public ComboBox CustomerCombo;
            public TextBox NoteInput;
            public Button SaveButton;
            public Button ClearButton;
            public Button PrintButton;
            public Label StatusLabel;
            public Label TodayLabel;
            public Label CashierLabel;
        }

        public static PosView Build(Form form, string cashierName)
        {
            form.SuspendLayout();
            form.Controls.Clear();
            form.Text = "Toko Root-ID | Kasir";
            form.BackColor = AppTheme.Background;
            form.ForeColor = AppTheme.Text;
            form.Font = new Font("Segoe UI", 9F);
            form.MinimumSize = new Size(1100, 680);

            var view = new PosView();

            var root = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(20) };
            form.Controls.Add(root);

            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = AppTheme.Background };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(shell);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(0) };
            shell.Controls.Add(header, 0, 0);

            var title = UiKit.Title("Kasir / Point of Sale", 0, 4, 520);
            var subtitle = UiKit.Subtitle("Cari barang, susun keranjang, lalu proses pembayaran dan cetak struk.", 0, 40, 720);
            view.TodayLabel = new Label
            {
                Location = new Point(0, 8),
                Size = new Size(340, 24),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppTheme.Accent,
                BackColor = Color.Transparent
            };
            view.CashierLabel = new Label
            {
                Location = new Point(0, 34),
                Size = new Size(340, 20),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = AppTheme.Muted,
                BackColor = Color.Transparent
            };
            var meta = new Panel { Dock = DockStyle.Right, Width = 350, BackColor = Color.Transparent };
            meta.Controls.Add(view.TodayLabel);
            meta.Controls.Add(view.CashierLabel);
            header.Controls.Add(meta);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background,
                Padding = new Padding(0, 4, 0, 0)
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 470));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.Controls.Add(body, 0, 1);

            // ---------------- PANEL PRODUK ----------------
            var productCard = UiKit.Card(0, 0, 200, 200);
            productCard.Dock = DockStyle.Fill;
            productCard.Margin = new Padding(0, 0, 14, 0);
            productCard.Padding = new Padding(14);
            body.Controls.Add(productCard, 0, 0);

            var productLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = AppTheme.Surface };
            productLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            productLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            productLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            productCard.Controls.Add(productLayout);

            var searchBar = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Surface };
            view.SearchBox = UiKit.Input(0, 8, 360);
            view.SearchBox.Font = new Font("Segoe UI", 11F);
            view.SearchBox.ForeColor = AppTheme.Muted;
            view.SearchBox.Text = "Scan barcode / ketik kode atau nama barang...";
            view.SearchButton = UiKit.Button("Cari", null, AppTheme.ButtonStyle.Primary);
            view.SearchButton.Size = new Size(84, 32);
            view.SearchButton.Location = new Point(368, 6);
            view.RefreshButton = UiKit.Button("Muat Ulang", null, AppTheme.ButtonStyle.Secondary);
            view.RefreshButton.Size = new Size(104, 32);
            view.RefreshButton.Location = new Point(458, 6);
            searchBar.Controls.Add(view.SearchBox);
            searchBar.Controls.Add(view.SearchButton);
            searchBar.Controls.Add(view.RefreshButton);
            productLayout.Controls.Add(searchBar, 0, 0);

            view.ProductGrid = UiKit.Grid();
            view.ProductGrid.AutoGenerateColumns = true;
            productLayout.Controls.Add(view.ProductGrid, 0, 1);

            var productHint = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Klik dua kali barang atau tekan Enter untuk menambahkan ke keranjang.",
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Surface
            };
            productLayout.Controls.Add(productHint, 0, 2);

            // ---------------- PANEL KERANJANG ----------------
            var cartCard = UiKit.Card(0, 0, 430, 200);
            cartCard.Dock = DockStyle.Fill;
            cartCard.Margin = new Padding(0);
            cartCard.Padding = new Padding(14);
            body.Controls.Add(cartCard, 1, 0);

            var cartLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = AppTheme.Surface };
            cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            cartLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 322));
            cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            cartCard.Controls.Add(cartLayout);

            cartLayout.Controls.Add(new Label
            {
                Text = "KERANJANG TRANSAKSI",
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

            var payment = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Surface };
            cartLayout.Controls.Add(payment, 0, 2);

            payment.Controls.Add(UiKit.FieldLabel("Subtotal", 2, 6, 130));
            view.SubtotalLabel = ValueLabel("Rp 0", 130, 2);
            payment.Controls.Add(view.SubtotalLabel);

            payment.Controls.Add(UiKit.FieldLabel("Diskon (Rp)", 2, 40, 130));
            view.DiscountInput = UiKit.Input(130, 36, 120, "0");
            view.DiscountInput.TextAlign = HorizontalAlignment.Right;
            payment.Controls.Add(view.DiscountInput);

            payment.Controls.Add(UiKit.FieldLabel("TOTAL", 2, 76, 130));
            view.TotalLabel = new Label
            {
                Text = "Rp 0",
                Location = new Point(118, 68),
                Size = new Size(290, 34),
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = AppTheme.Accent,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };
            payment.Controls.Add(view.TotalLabel);

            payment.Controls.Add(UiKit.FieldLabel("Metode bayar", 2, 114, 130));
            view.PaymentCombo = UiKit.Combo(130, 110, 140);
            view.PaymentCombo.Items.AddRange(new object[] { "Tunai", "Transfer", "Kartu", "QRIS", "Kredit" });
            view.PaymentCombo.SelectedIndex = 0;
            payment.Controls.Add(view.PaymentCombo);

            payment.Controls.Add(UiKit.FieldLabel("Pelanggan", 2, 148, 130));
            view.CustomerCombo = UiKit.Combo(130, 144, 290);
            payment.Controls.Add(view.CustomerCombo);

            payment.Controls.Add(UiKit.FieldLabel("Uang dibayar", 2, 182, 130));
            view.PaidInput = UiKit.Input(130, 178, 120, "0");
            view.PaidInput.TextAlign = HorizontalAlignment.Right;
            payment.Controls.Add(view.PaidInput);

            payment.Controls.Add(UiKit.FieldLabel("Kembali", 258, 182, 80));
            view.ChangeLabel = new Label
            {
                Text = "Rp 0",
                Location = new Point(330, 178),
                Size = new Size(90, 28),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppTheme.Success,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };
            payment.Controls.Add(view.ChangeLabel);

            payment.Controls.Add(UiKit.FieldLabel("Catatan", 2, 214, 130));
            view.NoteInput = UiKit.Input(130, 210, 290);
            payment.Controls.Add(view.NoteInput);

            view.SaveButton = UiKit.Button("PROSES BAYAR", null, AppTheme.ButtonStyle.Success);
            view.SaveButton.Location = new Point(2, 250);
            view.SaveButton.Size = new Size(170, 42);
            payment.Controls.Add(view.SaveButton);

            view.ClearButton = UiKit.Button("Kosongkan", null, AppTheme.ButtonStyle.Secondary);
            view.ClearButton.Location = new Point(180, 250);
            view.ClearButton.Size = new Size(110, 42);
            payment.Controls.Add(view.ClearButton);

            view.PrintButton = UiKit.Button("Cetak", null, AppTheme.ButtonStyle.Ghost);
            view.PrintButton.Location = new Point(298, 250);
            view.PrintButton.Size = new Size(122, 42);
            payment.Controls.Add(view.PrintButton);

            view.StatusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Siap melayani transaksi.",
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Surface
            };
            cartLayout.Controls.Add(view.StatusLabel, 0, 3);

            view.TodayLabel.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            view.CashierLabel.Text = "Kasir: " + (string.IsNullOrWhiteSpace(cashierName) ? "-" : cashierName);

            form.ResumeLayout(true);
            return view;
        }

        private static Label ValueLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(300, 26),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppTheme.Text,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };
        }

        private static void AddCartColumns(DataGridView grid)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "kodebarang", HeaderText = "Kode", FillWeight = 70, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "namabarang", HeaderText = "Barang", FillWeight = 140, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "hargasatuan",
                HeaderText = "Harga",
                FillWeight = 80,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "qty",
                HeaderText = "Qty",
                FillWeight = 50,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "diskon",
                HeaderText = "Disk",
                FillWeight = 55,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "subtotal",
                HeaderText = "Subtotal",
                FillWeight = 90,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
        }
    }
}
