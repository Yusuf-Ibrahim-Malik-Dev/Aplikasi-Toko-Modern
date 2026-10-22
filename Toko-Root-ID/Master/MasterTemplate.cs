using System;
using System.Drawing;
using System.Windows.Forms;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Master
{
    /// <summary>
    /// Template modul master data: satu halaman dengan tab barang, kategori,
    /// merek, satuan, supplier, dan pelanggan. Form frmMaster hanya mengisi data.
    /// </summary>
    internal sealed class MasterTemplate
    {
        internal sealed class SimpleEditor
        {
            public DataGridView Grid;
            public TextBox Search;
            public Button Refresh;
            public Button New;
            public Button Save;
            public Button Delete;
            public TextBox[] Fields;
            public CheckBox Active;
            public Panel Editor;
        }

        internal sealed class MasterView
        {
            public UiKit.PageView Page;
            public TabControl Tabs;

            public DataGridView BarangGrid;
            public TextBox BarangSearch;
            public Button BarangRefresh;
            public Button BarangNew;
            public Button BarangSave;
            public Button BarangDelete;
            public TextBox Kode;
            public TextBox Nama;
            public TextBox Barcode;
            public TextBox HargaBeli;
            public TextBox HargaJual;
            public TextBox Stok;
            public TextBox StokMin;
            public ComboBox Kategori;
            public ComboBox Merek;
            public ComboBox Satuan;

            public SimpleEditor KategoriTab;
            public SimpleEditor MerekTab;
            public SimpleEditor SatuanTab;
            public SimpleEditor SupplierTab;
            public SimpleEditor PelangganTab;
        }

        public static MasterView Build(Form form)
        {
            var view = new MasterView();
            view.Page = UiKit.Page(form, "Master Data",
                "Kelola barang, kategori, merek, satuan, supplier, dan pelanggan toko.");

            Button refreshAll = UiKit.Button("Muat Ulang Semua", null, AppTheme.ButtonStyle.Secondary);
            refreshAll.Size = new Size(150, 34);
            refreshAll.Margin = new Padding(0, 0, 0, 0);
            view.Page.Actions.Controls.Add(refreshAll);

            view.Tabs = UiKit.Tabs();
            view.Page.Body.Controls.Add(view.Tabs);

            BuildBarangTab(view);
            view.KategoriTab = BuildSimpleTab(view, "Kategori", "Cari kategori...",
                new[] { "Nama kategori", "Keterangan" }, true);
            view.MerekTab = BuildSimpleTab(view, "Merek", "Cari merek...",
                new[] { "Nama merek" }, true);
            view.SatuanTab = BuildSimpleTab(view, "Satuan", "Cari satuan...",
                new[] { "Nama satuan" }, true);
            view.SupplierTab = BuildSimpleTab(view, "Supplier", "Cari supplier...",
                new[] { "Kode", "Nama supplier", "Telepon", "Email", "Alamat" }, true);
            view.PelangganTab = BuildSimpleTab(view, "Pelanggan", "Cari pelanggan...",
                new[] { "Kode", "Nama pelanggan", "Telepon", "Email", "Alamat" }, true);

            return view;
        }

        // =========================================================
        // TAB BARANG
        // =========================================================
        private static void BuildBarangTab(MasterView view)
        {
            TabPage tab = UiKit.Tab("Barang");
            view.Tabs.TabPages.Add(tab);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 392));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(layout);

            view.BarangNew = UiKit.Button("Barang Baru", null, AppTheme.ButtonStyle.Secondary);
            view.BarangRefresh = UiKit.Button("Muat Ulang", null, AppTheme.ButtonStyle.Secondary);
            view.BarangGrid = UiKit.Grid();
            Panel toolbar = Toolbar(out view.BarangSearch, "Cari kode / nama / barcode...",
                view.BarangNew, view.BarangRefresh);
            layout.Controls.Add(UiKit.ListCard(toolbar, view.BarangGrid, 16), 0, 0);

            // Panel editor barang di sisi kanan.
            var editor = UiKit.Card(0, 0, 392, 300);
            editor.Dock = DockStyle.Fill;
            editor.Margin = new Padding(0);
            editor.Padding = new Padding(16);
            editor.AutoScroll = true;
            layout.Controls.Add(editor, 1, 0);

            editor.Controls.Add(UiKit.Section("Form Barang", 0, 0, 300));

            view.Kode = UiKit.Input(0, 0, 330);
            UiKit.AddField(editor, "Kode barang", view.Kode, 0, 34, 330);
            view.Nama = UiKit.Input(0, 0, 330);
            UiKit.AddField(editor, "Nama barang", view.Nama, 0, 80, 330);
            view.Barcode = UiKit.Input(0, 0, 330);
            UiKit.AddField(editor, "Barcode", view.Barcode, 0, 126, 330);

            view.Kategori = UiKit.Combo(0, 0, 330);
            UiKit.AddField(editor, "Kategori", view.Kategori, 0, 172, 330);
            view.Merek = UiKit.Combo(0, 0, 330);
            UiKit.AddField(editor, "Merek", view.Merek, 0, 218, 330);
            view.Satuan = UiKit.Combo(0, 0, 330);
            UiKit.AddField(editor, "Satuan", view.Satuan, 0, 264, 330);

            view.HargaBeli = UiKit.Input(0, 0, 330, "0");
            view.HargaBeli.TextAlign = HorizontalAlignment.Right;
            UiKit.AddField(editor, "Harga beli", view.HargaBeli, 0, 310, 330);
            view.HargaJual = UiKit.Input(0, 0, 330, "0");
            view.HargaJual.TextAlign = HorizontalAlignment.Right;
            UiKit.AddField(editor, "Harga jual", view.HargaJual, 0, 356, 330);
            view.Stok = UiKit.Input(0, 0, 154, "0");
            view.Stok.TextAlign = HorizontalAlignment.Right;
            UiKit.AddField(editor, "Stok", view.Stok, 0, 402, 154);
            view.StokMin = UiKit.Input(0, 0, 154, "0");
            view.StokMin.TextAlign = HorizontalAlignment.Right;
            UiKit.AddField(editor, "Stok minimum", view.StokMin, 176, 402, 154);

            view.BarangSave = UiKit.Button("Simpan", null, AppTheme.ButtonStyle.Primary);
            view.BarangSave.Location = new Point(0, 456);
            view.BarangSave.Size = new Size(110, 38);
            editor.Controls.Add(view.BarangSave);

            view.BarangDelete = UiKit.Button("Hapus", null, AppTheme.ButtonStyle.Danger);
            view.BarangDelete.Location = new Point(118, 456);
            view.BarangDelete.Size = new Size(100, 38);
            editor.Controls.Add(view.BarangDelete);

            Button reset = UiKit.Button("Bersihkan", null, AppTheme.ButtonStyle.Secondary);
            reset.Location = new Point(226, 456);
            reset.Size = new Size(104, 38);
            reset.Click += delegate { ClearBarang(editor); };
            editor.Controls.Add(reset);
        }

        private static void ClearBarang(Control editor)
        {
            foreach (Control control in editor.Controls)
            {
                TextBox box = control as TextBox;
                if (box != null && box.Name != "search") box.Clear();
                ComboBox combo = control as ComboBox;
                if (combo != null) combo.SelectedIndex = -1;
            }
        }

        // =========================================================
        // TAB SEDERHANA
        // =========================================================
        private static SimpleEditor BuildSimpleTab(MasterView view, string title, string hint,
            string[] labels, bool withActive)
        {
            TabPage tab = UiKit.Tab(title);
            view.Tabs.TabPages.Add(tab);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(layout);

            var item = new SimpleEditor();
            item.New = UiKit.Button("Baru", null, AppTheme.ButtonStyle.Secondary);
            item.Refresh = UiKit.Button("Muat Ulang", null, AppTheme.ButtonStyle.Secondary);
            item.Grid = UiKit.Grid();
            Panel toolbar = Toolbar(out item.Search, hint, item.New, item.Refresh);
            layout.Controls.Add(UiKit.ListCard(toolbar, item.Grid, 16), 0, 0);

            var editor = UiKit.Card(0, 0, 360, 300);
            editor.Dock = DockStyle.Fill;
            editor.Margin = new Padding(0);
            editor.Padding = new Padding(16);
            editor.AutoScroll = true;
            layout.Controls.Add(editor, 1, 0);
            item.Editor = editor;

            editor.Controls.Add(UiKit.Section("Form " + title, 0, 0, 280));

            item.Fields = new TextBox[labels.Length];
            int y = 36;
            for (int i = 0; i < labels.Length; i++)
            {
                item.Fields[i] = UiKit.Input(0, 0, 300);
                UiKit.AddField(editor, labels[i], item.Fields[i], 0, y, 300);
                y += 52;
            }

            if (withActive)
            {
                item.Active = new CheckBox
                {
                    Text = "Aktif",
                    Location = new Point(0, y + 2),
                    Size = new Size(200, 24),
                    Checked = true,
                    ForeColor = AppTheme.Text,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9.5F)
                };
                editor.Controls.Add(item.Active);
            }

            int buttonTop = y + (withActive ? 34 : 8);
            item.Save = UiKit.Button("Simpan", null, AppTheme.ButtonStyle.Primary);
            item.Save.Location = new Point(0, buttonTop);
            item.Save.Size = new Size(100, 38);
            editor.Controls.Add(item.Save);

            item.Delete = UiKit.Button("Hapus", null, AppTheme.ButtonStyle.Danger);
            item.Delete.Location = new Point(108, buttonTop);
            item.Delete.Size = new Size(92, 38);
            editor.Controls.Add(item.Delete);

            Button clear = UiKit.Button("Bersihkan", null, AppTheme.ButtonStyle.Secondary);
            clear.Location = new Point(208, buttonTop);
            clear.Size = new Size(96, 38);
            clear.Click += delegate
            {
                foreach (TextBox field in item.Fields) field.Clear();
                if (item.Active != null) item.Active.Checked = true;
                item.Grid.ClearSelection();
            };
            editor.Controls.Add(clear);

            return item;
        }

        private static Panel Toolbar(out TextBox search, string hint, params Button[] buttons)
        {
            var bar = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Surface };
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.Surface,
                Padding = new Padding(0, 6, 0, 0)
            };
            search = UiKit.Input(0, 0, 250);
            UiKit.SetPlaceholder(search, hint);
            search.Margin = new Padding(0, 0, 8, 0);
            flow.Controls.Add(search);
            foreach (Button button in buttons)
            {
                button.Size = new Size(108, 30);
                button.Margin = new Padding(0, 0, 6, 0);
                flow.Controls.Add(button);
            }
            bar.Controls.Add(flow);
            return bar;
        }
    }
}
