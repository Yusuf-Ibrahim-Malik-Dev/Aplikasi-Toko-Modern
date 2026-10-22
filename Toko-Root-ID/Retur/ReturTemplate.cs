using System;
using System.Drawing;
using System.Windows.Forms;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Retur
{
    /// <summary>Template modul retur penjualan dan retur pembelian.</summary>
    internal sealed class ReturTemplate
    {
        internal sealed class ReturPane
        {
            public ComboBox DocumentCombo;
            public Button LoadButton;
            public TextBox ReasonBox;
            public Button SaveButton;
            public DataGridView ItemsGrid;
            public DataGridView HistoryGrid;
            public Label Info;
        }

        internal sealed class ReturView
        {
            public UiKit.PageView Page;
            public TabControl Tabs;
            public ReturPane Sales;
            public ReturPane Purchase;
        }

        public static ReturView Build(Form form)
        {
            var view = new ReturView();
            view.Page = UiKit.Page(form, "Retur Barang",
                "Proses pengembalian barang dari pelanggan atau ke supplier dengan penyesuaian stok otomatis.");
            view.Tabs = UiKit.Tabs();
            view.Page.Body.Controls.Add(view.Tabs);

            view.Sales = BuildTab(view, "Retur Penjualan", "Nomor faktur penjualan");
            view.Purchase = BuildTab(view, "Retur Pembelian", "Nomor pembelian");
            return view;
        }

        private static ReturPane BuildTab(ReturView view, string title, string documentLabel)
        {
            TabPage tab = UiKit.Tab(title);
            view.Tabs.TabPages.Add(tab);

            var pane = new ReturPane();

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Background
            };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            tab.Controls.Add(shell);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(header, 0, 0);

            header.Controls.Add(UiKit.FieldLabel(documentLabel, 0, 4, 260));
            pane.DocumentCombo = UiKit.Combo(0, 22, 280);
            header.Controls.Add(pane.DocumentCombo);

            pane.LoadButton = UiKit.Button("Muat Barang", null, AppTheme.ButtonStyle.Primary);
            pane.LoadButton.Location = new Point(290, 19);
            pane.LoadButton.Size = new Size(118, 34);
            header.Controls.Add(pane.LoadButton);

            header.Controls.Add(UiKit.FieldLabel("Alasan retur", 424, 4, 260));
            pane.ReasonBox = UiKit.Input(424, 22, 320);
            header.Controls.Add(pane.ReasonBox);

            pane.SaveButton = UiKit.Button("SIMPAN RETUR", null, AppTheme.ButtonStyle.Success);
            pane.SaveButton.Location = new Point(758, 19);
            pane.SaveButton.Size = new Size(158, 34);
            header.Controls.Add(pane.SaveButton);

            // Barang yang diretur.
            var itemsCard = UiKit.Card(0, 0, 200, 200);
            itemsCard.Dock = DockStyle.Fill;
            itemsCard.Margin = new Padding(0, 6, 0, 10);
            itemsCard.Padding = new Padding(12);
            shell.Controls.Add(itemsCard, 0, 1);

            var itemsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppTheme.Surface
            };
            itemsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            itemsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            itemsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            itemsCard.Controls.Add(itemsLayout);

            itemsLayout.Controls.Add(new Label
            {
                Text = "Isi kolom RETUR sesuai jumlah yang dikembalikan (0 bila tidak diretur).",
                Dock = DockStyle.Fill,
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                BackColor = AppTheme.Surface
            }, 0, 0);

            pane.ItemsGrid = UiKit.Grid();
            pane.ItemsGrid.ReadOnly = false;
            pane.ItemsGrid.EditMode = DataGridViewEditMode.EditOnEnter;
            pane.ItemsGrid.AutoGenerateColumns = false;
            pane.ItemsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "kodebarang", HeaderText = "Kode", DataPropertyName = "kodebarang", FillWeight = 65, ReadOnly = true });
            pane.ItemsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "namabarang", HeaderText = "Nama Barang", DataPropertyName = "namabarang", FillWeight = 180, ReadOnly = true });
            pane.ItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "qty",
                HeaderText = "Qty",
                DataPropertyName = "qty",
                FillWeight = 55,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.###", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            pane.ItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "harga",
                HeaderText = "Harga",
                DataPropertyName = "harga",
                FillWeight = 80,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            pane.ItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "retur",
                HeaderText = "RETUR",
                DataPropertyName = "retur",
                FillWeight = 65,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "0.###",
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    BackColor = Color.FromArgb(255, 249, 224)
                }
            });
            pane.ItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "subtotal",
                HeaderText = "Nilai Retur",
                DataPropertyName = "subtotal",
                FillWeight = 90,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            itemsLayout.Controls.Add(pane.ItemsGrid, 0, 1);

            pane.Info = new Label
            {
                Text = "Pilih dokumen lalu tekan Muat Barang.",
                Dock = DockStyle.Fill,
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                BackColor = AppTheme.Surface
            };
            itemsLayout.Controls.Add(pane.Info, 0, 2);

            // Riwayat retur.
            var historyCard = UiKit.Card(0, 0, 200, 200);
            historyCard.Dock = DockStyle.Fill;
            historyCard.Margin = new Padding(0);
            historyCard.Padding = new Padding(12);
            shell.Controls.Add(historyCard, 0, 2);

            var historyLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.Surface
            };
            historyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            historyLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            historyCard.Controls.Add(historyLayout);

            historyLayout.Controls.Add(new Label
            {
                Text = "Riwayat " + title.ToLowerInvariant() + " (90 hari terakhir)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppTheme.Text,
                BackColor = AppTheme.Surface
            }, 0, 0);

            pane.HistoryGrid = UiKit.Grid();
            historyLayout.Controls.Add(pane.HistoryGrid, 0, 1);

            return pane;
        }
    }
}
