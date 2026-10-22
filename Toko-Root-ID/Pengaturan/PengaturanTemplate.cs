using System;
using System.Drawing;
using System.Windows.Forms;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Pengaturan
{
    /// <summary>Template halaman pengaturan: profil toko, tampilan tema, dan informasi aplikasi.</summary>
    internal sealed class PengaturanTemplate
    {
        internal sealed class PengaturanView
        {
            public UiKit.PageView Page;
            public TabControl Tabs;

            public TextBox NameBox;
            public TextBox AddressBox;
            public TextBox PhoneBox;
            public TextBox EmailBox;
            public TextBox FooterBox;
            public Button SaveButton;
            public Label StatusLabel;

            public Label PreviewName;
            public Label PreviewAddress;
            public Label PreviewPhone;
            public Label PreviewFooter;

            public Button DarkModeButton;
            public Button ResetThemeButton;
            public Button[] PresetButtons;
            public Label ThemeStatus;

            public Label AboutLabel;
            public Label InfoLabel;
            public Button RefreshInfoButton;
        }

        public static PengaturanView Build(Form form)
        {
            var view = new PengaturanView();
            view.Page = UiKit.Page(form, "Pengaturan",
                "Sesuaikan identitas toko pada struk dan tampilan aplikasi.");
            view.Tabs = UiKit.Tabs();
            view.Page.Body.Controls.Add(view.Tabs);

            BuildProfile(view);
            BuildAppearance(view);
            BuildAbout(view);
            return view;
        }

        // =========================================================
        // PROFIL TOKO
        // =========================================================
        private static void BuildProfile(PengaturanView view)
        {
            TabPage tab = UiKit.Tab("Profil Toko");
            view.Tabs.TabPages.Add(tab);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(body);

            var editor = UiKit.Card(0, 0, 200, 300);
            editor.Dock = DockStyle.Fill;
            editor.Margin = new Padding(0, 6, 16, 0);
            editor.Padding = new Padding(18);
            body.Controls.Add(editor, 0, 0);

            editor.Controls.Add(UiKit.Section("Identitas Toko", 0, 0, 320));

            view.NameBox = UiKit.Input(0, 0, 420);
            UiKit.AddField(editor, "Nama toko", view.NameBox, 0, 40, 420);

            view.AddressBox = UiKit.Input(0, 0, 420);
            view.AddressBox.Multiline = true;
            view.AddressBox.Height = 60;
            UiKit.AddField(editor, "Alamat", view.AddressBox, 0, 88, 420);
            view.AddressBox.Height = 60;

            view.PhoneBox = UiKit.Input(0, 0, 420);
            UiKit.AddField(editor, "Telepon", view.PhoneBox, 0, 176, 420);

            view.EmailBox = UiKit.Input(0, 0, 420);
            UiKit.AddField(editor, "Email", view.EmailBox, 0, 224, 420);

            view.FooterBox = UiKit.Input(0, 0, 420);
            UiKit.AddField(editor, "Teks penutup struk", view.FooterBox, 0, 272, 420);

            view.SaveButton = UiKit.Button("SIMPAN PROFIL", null, AppTheme.ButtonStyle.Success);
            view.SaveButton.Location = new Point(0, 330);
            view.SaveButton.Size = new Size(180, 40);
            editor.Controls.Add(view.SaveButton);

            view.StatusLabel = new Label
            {
                Text = "Perubahan profil langsung dipakai pada struk berikutnya.",
                Location = new Point(192, 336),
                Size = new Size(360, 28),
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            editor.Controls.Add(view.StatusLabel);

            // Pratinjau struk.
            var preview = UiKit.Card(0, 0, 400, 300);
            preview.Dock = DockStyle.Fill;
            preview.Margin = new Padding(0, 6, 0, 0);
            preview.Padding = new Padding(18);
            body.Controls.Add(preview, 1, 0);

            preview.Controls.Add(UiKit.Section("Pratinjau Struk", 0, 0, 300));

            var paper = new Panel
            {
                Location = new Point(0, 38),
                Size = new Size(330, 420),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            preview.Controls.Add(paper);

            view.PreviewName = PreviewLabel("Nama Toko", 14, 16, 300, new Font("Consolas", 10F, FontStyle.Bold), AppTheme.Text);
            view.PreviewAddress = PreviewLabel("Alamat toko", 14, 40, 300, new Font("Consolas", 8F), AppTheme.Text);
            view.PreviewPhone = PreviewLabel("Telepon", 14, 58, 300, new Font("Consolas", 8F), AppTheme.Text);
            view.PreviewFooter = PreviewLabel("Terima kasih", 14, 250, 300, new Font("Consolas", 8F), AppTheme.Text);
            paper.Controls.Add(view.PreviewName);
            paper.Controls.Add(view.PreviewAddress);
            paper.Controls.Add(view.PreviewPhone);
            paper.Controls.Add(view.PreviewFooter);

            paper.Controls.Add(new Label
            {
                Text = "--------------------------------",
                Location = new Point(14, 78),
                Size = new Size(300, 16),
                Font = new Font("Consolas", 8F),
                ForeColor = AppTheme.Text
            });
            paper.Controls.Add(new Label
            {
                Text = "No: FK00001\nKasir: Kasir\n\nContoh Barang\n1 x 25.000        25.000\n\n--------------------------------\nTOTAL             25.000\nBayar             50.000\nKembali           25.000",
                Location = new Point(14, 96),
                Size = new Size(300, 146),
                Font = new Font("Consolas", 8F),
                ForeColor = AppTheme.Text
            });
        }

        private static Label PreviewLabel(string text, int x, int y, int width, Font font, Color color)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 18),
                Font = font,
                ForeColor = color,
                BackColor = Color.Transparent
            };
        }

        // =========================================================
        // TAMPILAN
        // =========================================================
        private static void BuildAppearance(PengaturanView view)
        {
            TabPage tab = UiKit.Tab("Tampilan");
            view.Tabs.TabPages.Add(tab);

            var card = UiKit.Card(0, 0, 200, 300);
            card.Dock = DockStyle.Top;
            card.Height = 470;
            card.Margin = new Padding(0, 6, 0, 0);
            card.Padding = new Padding(20);
            tab.Controls.Add(card);

            card.Controls.Add(UiKit.Section("Mode Gelap", 0, 0, 320));
            card.Controls.Add(new Label
            {
                Text = "Mode gelap membuat seluruh halaman, kartu, dan tabel memakai palet gelap.",
                Location = new Point(0, 30),
                Size = new Size(560, 22),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppTheme.Muted,
                BackColor = Color.Transparent
            });

            view.DarkModeButton = UiKit.Button("Aktifkan Mode Gelap", null, AppTheme.ButtonStyle.Secondary);
            view.DarkModeButton.Location = new Point(0, 60);
            view.DarkModeButton.Size = new Size(200, 38);
            card.Controls.Add(view.DarkModeButton);

            card.Controls.Add(UiKit.Section("Warna Aksen", 0, 120, 320));
            card.Controls.Add(new Label
            {
                Text = "Warna aksen dipakai pada menu aktif, tombol utama, ikon, dan grafik.",
                Location = new Point(0, 150),
                Size = new Size(580, 22),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppTheme.Muted,
                BackColor = Color.Transparent
            });

            var swatches = new FlowLayoutPanel
            {
                Location = new Point(0, 180),
                Size = new Size(600, 160),
                BackColor = Color.Transparent,
                WrapContents = true
            };
            card.Controls.Add(swatches);

            view.PresetButtons = new Button[AppTheme.Presets.Length];
            for (int i = 0; i < AppTheme.Presets.Length; i++)
            {
                AppTheme.ThemePreset preset = AppTheme.Presets[i];
                var button = new Button
                {
                    Text = preset.Name,
                    Tag = preset.Name,
                    Size = new Size(136, 36),
                    Margin = new Padding(0, 0, 8, 8),
                    BackColor = preset.Color,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                button.FlatAppearance.BorderSize = 0;
                view.PresetButtons[i] = button;
                swatches.Controls.Add(button);
            }

            view.ResetThemeButton = UiKit.Button("Reset ke Tema Default", null, AppTheme.ButtonStyle.Secondary);
            view.ResetThemeButton.Location = new Point(0, 360);
            view.ResetThemeButton.Size = new Size(210, 38);
            card.Controls.Add(view.ResetThemeButton);

            view.ThemeStatus = new Label
            {
                Text = "",
                Location = new Point(226, 366),
                Size = new Size(420, 26),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = AppTheme.Muted,
                BackColor = Color.Transparent
            };
            card.Controls.Add(view.ThemeStatus);
        }

        // =========================================================
        // TENTANG
        // =========================================================
        private static void BuildAbout(PengaturanView view)
        {
            TabPage tab = UiKit.Tab("Tentang Aplikasi");
            view.Tabs.TabPages.Add(tab);

            var card = UiKit.Card(0, 0, 200, 300);
            card.Dock = DockStyle.Top;
            card.Height = 420;
            card.Margin = new Padding(0, 6, 0, 0);
            card.Padding = new Padding(20);
            tab.Controls.Add(card);

            card.Controls.Add(UiKit.Section("Toko Root-ID", 0, 0, 320));
            view.AboutLabel = new Label
            {
                Text = "Aplikasi kasir dan manajemen toko berbasis Windows Forms.\n" +
                       "Modul: Kasir, Pembelian, Master Data, Inventory, Keuangan, Retur, Laporan, dan Pengaturan.",
                Location = new Point(0, 34),
                Size = new Size(680, 56),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = AppTheme.Text,
                BackColor = Color.Transparent
            };
            card.Controls.Add(view.AboutLabel);

            view.InfoLabel = new Label
            {
                Text = "Memuat informasi...",
                Location = new Point(0, 104),
                Size = new Size(680, 220),
                Font = new Font("Consolas", 9F),
                ForeColor = AppTheme.Muted,
                BackColor = Color.Transparent
            };
            card.Controls.Add(view.InfoLabel);

            view.RefreshInfoButton = UiKit.Button("Muat Ulang Informasi", null, AppTheme.ButtonStyle.Secondary);
            view.RefreshInfoButton.Location = new Point(0, 340);
            view.RefreshInfoButton.Size = new Size(190, 38);
            card.Controls.Add(view.RefreshInfoButton);
        }
    }
}
