using System;
using System.Drawing;
using System.Windows.Forms;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Inventory
{
    /// <summary>Template modul inventory: mutasi stok, opname, transfer gudang, stok kritis.</summary>
    internal sealed class InventoryTemplate
    {
        internal sealed class InventoryView
        {
            public UiKit.PageView Page;
            public TabControl Tabs;

            public DateTimePicker StartPicker;
            public DateTimePicker EndPicker;
            public ComboBox TypeCombo;
            public TextBox SearchBox;
            public Button RefreshButton;
            public DataGridView MutasiGrid;

            public ComboBox WarehouseCombo;
            public TextBox OpnameNote;
            public Button OpnameRefresh;
            public Button FillSystemButton;
            public Button OpnameSave;
            public DataGridView OpnameGrid;
            public Label OpnameStatus;

            public ComboBox FromWarehouse;
            public ComboBox ToWarehouse;
            public TextBox TransferSearch;
            public Button TransferRefresh;
            public DataGridView TransferProductGrid;
            public DataGridView TransferCart;
            public Button TransferSave;
            public Label TransferStatus;

            public DataGridView CriticalGrid;
            public Button CriticalRefresh;
            public Label CriticalSummary;
        }

        public static InventoryView Build(Form form)
        {
            var view = new InventoryView();
            view.Page = UiKit.Page(form, "Inventory",
                "Pantau pergerakan stok, lakukan stok opname, dan pindahkan barang antar gudang.");
            view.Tabs = UiKit.Tabs();
            view.Page.Body.Controls.Add(view.Tabs);

            BuildMutasi(view);
            BuildOpname(view);
            BuildTransfer(view);
            BuildCritical(view);
            return view;
        }

        private static void BuildMutasi(InventoryView view)
        {
            TabPage tab = UiKit.Tab("Mutasi Stok");
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.Background
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(shell);

            var filter = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(filter, 0, 0);

            filter.Controls.Add(UiKit.FieldLabel("Dari", 0, 4, 80));
            view.StartPicker = UiKit.Date(0, 22, 130);
            view.StartPicker.Value = DateTime.Today.AddDays(-30);
            filter.Controls.Add(view.StartPicker);

            filter.Controls.Add(UiKit.FieldLabel("Sampai", 142, 4, 80));
            view.EndPicker = UiKit.Date(142, 22, 130);
            filter.Controls.Add(view.EndPicker);

            filter.Controls.Add(UiKit.FieldLabel("Tipe", 286, 4, 80));
            view.TypeCombo = UiKit.Combo(286, 22, 140);
            view.TypeCombo.Items.AddRange(new object[] { "Semua", "Masuk", "Keluar", "Penyesuaian", "Transfer" });
            view.TypeCombo.SelectedIndex = 0;
            filter.Controls.Add(view.TypeCombo);

            view.SearchBox = UiKit.Input(440, 22, 250);
            UiKit.SetPlaceholder(view.SearchBox, "Cari kode / nama barang...");
            filter.Controls.Add(view.SearchBox);

            view.RefreshButton = UiKit.Button("Tampilkan", null, AppTheme.ButtonStyle.Primary);
            view.RefreshButton.Location = new Point(700, 19);
            view.RefreshButton.Size = new Size(110, 34);
            filter.Controls.Add(view.RefreshButton);

            var card = UiKit.Card(0, 0, 200, 200);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 6, 0, 0);
            card.Padding = new Padding(12);
            shell.Controls.Add(card, 0, 1);

            view.MutasiGrid = UiKit.Grid();
            card.Controls.Add(view.MutasiGrid);
        }

        private static void BuildOpname(InventoryView view)
        {
            TabPage tab = UiKit.Tab("Stok Opname");
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Background
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tab.Controls.Add(shell);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(header, 0, 0);

            header.Controls.Add(UiKit.FieldLabel("Gudang", 0, 4, 200));
            view.WarehouseCombo = UiKit.Combo(0, 22, 220);
            header.Controls.Add(view.WarehouseCombo);

            header.Controls.Add(UiKit.FieldLabel("Catatan opname", 236, 4, 260));
            view.OpnameNote = UiKit.Input(236, 22, 320);
            header.Controls.Add(view.OpnameNote);

            view.FillSystemButton = UiKit.Button("Isi = Stok Sistem", null, AppTheme.ButtonStyle.Secondary);
            view.FillSystemButton.Location = new Point(572, 19);
            view.FillSystemButton.Size = new Size(150, 34);
            header.Controls.Add(view.FillSystemButton);

            view.OpnameRefresh = UiKit.Button("Muat Barang", null, AppTheme.ButtonStyle.Secondary);
            view.OpnameRefresh.Location = new Point(730, 19);
            view.OpnameRefresh.Size = new Size(118, 34);
            header.Controls.Add(view.OpnameRefresh);

            var card = UiKit.Card(0, 0, 200, 200);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 6, 0, 10);
            card.Padding = new Padding(12);
            shell.Controls.Add(card, 0, 1);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = AppTheme.Surface };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            card.Controls.Add(layout);

            layout.Controls.Add(new Label
            {
                Text = "Masukkan jumlah fisik hasil hitung. Selisih otomatis menyesuaikan stok sistem.",
                Dock = DockStyle.Fill,
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                BackColor = AppTheme.Surface
            }, 0, 0);

            view.OpnameGrid = UiKit.Grid();
            view.OpnameGrid.ReadOnly = false;
            view.OpnameGrid.EditMode = DataGridViewEditMode.EditOnEnter;
            view.OpnameGrid.AutoGenerateColumns = false;
            view.OpnameGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "kodebarang", HeaderText = "Kode", DataPropertyName = "kodebarang", FillWeight = 70, ReadOnly = true });
            view.OpnameGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "namabarang", HeaderText = "Nama Barang", DataPropertyName = "namabarang", FillWeight = 180, ReadOnly = true });
            view.OpnameGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "satuan", HeaderText = "Satuan", DataPropertyName = "satuan", FillWeight = 60, ReadOnly = true });
            view.OpnameGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "stok",
                HeaderText = "Stok Sistem",
                DataPropertyName = "stok",
                FillWeight = 80,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.###", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            view.OpnameGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "fisik",
                HeaderText = "Stok Fisik",
                DataPropertyName = "fisik",
                FillWeight = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.###", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            view.OpnameGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "selisih",
                HeaderText = "Selisih",
                DataPropertyName = "selisih",
                FillWeight = 70,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.###", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            layout.Controls.Add(view.OpnameGrid, 0, 1);

            var footer = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(footer, 0, 2);

            view.OpnameStatus = new Label
            {
                Dock = DockStyle.Left,
                Width = 520,
                Text = "Belum ada penyesuaian.",
                ForeColor = AppTheme.Muted,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F),
                BackColor = AppTheme.Background
            };
            footer.Controls.Add(view.OpnameStatus);

            view.OpnameSave = UiKit.Button("SIMPAN OPNAME", null, AppTheme.ButtonStyle.Success);
            view.OpnameSave.Dock = DockStyle.Right;
            view.OpnameSave.Width = 170;
            view.OpnameSave.Height = 36;
            footer.Controls.Add(view.OpnameSave);
        }

        private static void BuildTransfer(InventoryView view)
        {
            TabPage tab = UiKit.Tab("Transfer Gudang");
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.Background
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(shell);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(header, 0, 0);

            header.Controls.Add(UiKit.FieldLabel("Gudang asal", 0, 4, 200));
            view.FromWarehouse = UiKit.Combo(0, 22, 220);
            header.Controls.Add(view.FromWarehouse);

            header.Controls.Add(UiKit.FieldLabel("Gudang tujuan", 236, 4, 200));
            view.ToWarehouse = UiKit.Combo(236, 22, 220);
            header.Controls.Add(view.ToWarehouse);

            view.TransferSave = UiKit.Button("SIMPAN TRANSFER", null, AppTheme.ButtonStyle.Success);
            view.TransferSave.Location = new Point(472, 19);
            view.TransferSave.Size = new Size(170, 34);
            header.Controls.Add(view.TransferSave);

            view.TransferStatus = new Label
            {
                Text = "",
                Location = new Point(656, 22),
                Size = new Size(400, 30),
                ForeColor = AppTheme.Muted,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = AppTheme.Background
            };
            header.Controls.Add(view.TransferStatus);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.Controls.Add(body, 0, 1);

            var productCard = UiKit.Card(0, 0, 200, 200);
            productCard.Dock = DockStyle.Fill;
            productCard.Margin = new Padding(0, 6, 14, 0);
            productCard.Padding = new Padding(12);
            body.Controls.Add(productCard, 0, 0);

            var productLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = AppTheme.Surface };
            productLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            productLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            productCard.Controls.Add(productLayout);

            var toolbar = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Surface };
            view.TransferSearch = UiKit.Input(0, 4, 300);
            UiKit.SetPlaceholder(view.TransferSearch, "Cari barang...");
            toolbar.Controls.Add(view.TransferSearch);
            view.TransferRefresh = UiKit.Button("Cari", null, AppTheme.ButtonStyle.Secondary);
            view.TransferRefresh.Location = new Point(308, 2);
            view.TransferRefresh.Size = new Size(92, 32);
            toolbar.Controls.Add(view.TransferRefresh);
            productLayout.Controls.Add(toolbar, 0, 0);

            view.TransferProductGrid = UiKit.Grid();
            view.TransferProductGrid.AutoGenerateColumns = false;
            view.TransferProductGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "kodebarang", HeaderText = "Kode", DataPropertyName = "kodebarang", FillWeight = 70 });
            view.TransferProductGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "namabarang", HeaderText = "Nama Barang", DataPropertyName = "namabarang", FillWeight = 180 });
            view.TransferProductGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "satuan", HeaderText = "Satuan", DataPropertyName = "satuan", FillWeight = 60 });
            view.TransferProductGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "stok",
                HeaderText = "Stok",
                DataPropertyName = "stok",
                FillWeight = 60,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.###", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            productLayout.Controls.Add(view.TransferProductGrid, 0, 1);

            var cartCard = UiKit.Card(0, 0, 420, 200);
            cartCard.Dock = DockStyle.Fill;
            cartCard.Margin = new Padding(0, 6, 0, 0);
            cartCard.Padding = new Padding(12);
            body.Controls.Add(cartCard, 1, 0);

            var cartLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = AppTheme.Surface };
            cartLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            cartLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            cartCard.Controls.Add(cartLayout);

            cartLayout.Controls.Add(new Label
            {
                Text = "BARANG YANG DIPINDAHKAN",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppTheme.Text,
                BackColor = AppTheme.Surface
            }, 0, 0);

            view.TransferCart = UiKit.Grid();
            view.TransferCart.ReadOnly = false;
            view.TransferCart.EditMode = DataGridViewEditMode.EditOnEnter;
            view.TransferCart.AutoGenerateColumns = false;
            view.TransferCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "kodebarang", HeaderText = "Kode", FillWeight = 70, ReadOnly = true });
            view.TransferCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "namabarang", HeaderText = "Barang", FillWeight = 180, ReadOnly = true });
            view.TransferCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "qty",
                HeaderText = "Qty",
                FillWeight = 60,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.###", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            cartLayout.Controls.Add(view.TransferCart, 0, 1);
        }

        private static void BuildCritical(InventoryView view)
        {
            TabPage tab = UiKit.Tab("Stok Kritis");
            view.Tabs.TabPages.Add(tab);

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.Background
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(shell);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(header, 0, 0);

            view.CriticalSummary = new Label
            {
                Dock = DockStyle.Left,
                Width = 520,
                Text = "Barang dengan stok habis atau di bawah batas minimum.",
                ForeColor = AppTheme.Muted,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F),
                BackColor = AppTheme.Background
            };
            header.Controls.Add(view.CriticalSummary);

            view.CriticalRefresh = UiKit.Button("Muat Ulang", null, AppTheme.ButtonStyle.Secondary);
            view.CriticalRefresh.Dock = DockStyle.Right;
            view.CriticalRefresh.Width = 120;
            view.CriticalRefresh.Height = 34;
            header.Controls.Add(view.CriticalRefresh);

            var card = UiKit.Card(0, 0, 200, 200);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 6, 0, 0);
            card.Padding = new Padding(12);
            shell.Controls.Add(card, 0, 1);

            view.CriticalGrid = UiKit.Grid();
            card.Controls.Add(view.CriticalGrid);
        }
    }
}
