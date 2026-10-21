using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DAAccess;
using Toko_Root_ID;

namespace Toko_Root_ID.MenuUtama
{
    /// <summary>
    /// Semua komposisi visual menu utama, dashboard, navigasi, kartu statistik,
    /// chart, dan pengaturan tema berada di template ini. FormUtama hanya menjadi
    /// host untuk navigasi dan sumber data dashboard.
    /// </summary>
    internal sealed class MenuUtamaTemplate
    {
        // DTO sederhana: wadah data yang disediakan oleh FormUtama untuk ditampilkan dashboard.
        // Tipe ini tidak mengakses database sendiri; pemuatan datanya berada di _owner.
        internal sealed class DashboardData
        {
            public int ItemCount;
            public decimal TotalStock;
            public string Status;
        }

        // Referensi form utama dipakai untuk memuat data dan menjadi pemilik halaman.
        private readonly FormUtama _owner;
        // Referensi kontrol visual disimpan agar bisa diganti/diubah ketika pengguna berpindah halaman.
        private Panel _sidebar;
        private Panel _header;
        private Panel _content;
        private Label _pageTitle;
        private Button _activeNav;
        private Form _embeddedForm;

        // Constructor menerima FormUtama yang menjadi host tampilan menu utama.
        internal MenuUtamaTemplate(FormUtama owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            _owner = owner;
        }

        // Merakit kerangka aplikasi: sidebar kiri, header atas, dan area konten di kanan.
        // Dipanggil dari FormUtama setelah form siap dibuat.
        internal void Build()
        {
            // Tunda layout saat banyak kontrol ditambahkan agar rendering lebih efisien.
            _owner.SuspendLayout();
            _owner.Controls.Clear();
            _owner.Text = "Toko Modern | Sistem Penjualan"; // Judul di title bar.
            _owner.Name = "FormUtama"; // Nama internal form, berguna saat kontrol dicari lewat kode.
            _owner.StartPosition = FormStartPosition.CenterScreen; // Awalnya tampil di tengah layar.
            _owner.MinimumSize = new Size(1050, 680); // Ukuran terkecil agar sidebar dan konten tetap muat.
            _owner.Size = new Size(1280, 820); // Ukuran awal jendela dalam pixel.
            _owner.BackColor = AppTheme.Background; // Warna dasar jendela dari tema global.
            _owner.Font = new Font("Segoe UI", 9F); // Font default untuk kontrol yang tidak memiliki font sendiri.

            // SIDEBAR: menu navigasi tetap di sisi kiri jendela.
            _sidebar = new Panel
            {
                Name = "sidebar", // Nama kontrol panel navigasi.
                Dock = DockStyle.Left, // Tempel di sisi kiri form.
                Width = 230, // Lebar sidebar dalam pixel.
                BackColor = AppTheme.Sidebar, // Warna sidebar khusus dari tema.
                Padding = new Padding(14, 18, 14, 14) // Ruang dalam: kiri, atas, kanan, bawah.
            };
            // Identitas merek di area paling atas sidebar.
            _sidebar.Controls.Add(new Label
            {
                Text = "T",
                Location = new Point(14, 18),
                Size = new Size(42, 42),
                BackColor = AppTheme.Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            });
            _sidebar.Controls.Add(new Label
            {
                Text = "TOKO MODERN",
                Location = new Point(66, 20),
                Size = new Size(145, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold)
            });
            _sidebar.Controls.Add(new Label
            {
                Text = "STORE MANAGEMENT",
                Location = new Point(67, 44),
                Size = new Size(145, 18),
                ForeColor = Color.FromArgb(174, 188, 211),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold)
            });
            _sidebar.Controls.Add(new Label
            {
                Text = "MENU UTAMA",
                Location = new Point(16, 78),
                Size = new Size(180, 18),
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            });

            // Daftar navigasi: parameter adalah judul, simbol, posisi Y, aksi, dan status terpilih awal.
            AddNav("Dashboard", "▦", 105, ShowDashboard, true);
            AddNav("Tambah Barang", "+", 153, delegate { ShowEmbedded(new frmBarang(), "Tambah Barang"); }, false);
            AddNav("Data Barang", "▤", 201, delegate { ShowEmbedded(new grdBarang(), "Data Barang"); }, false);
            AddNav("Penjualan", "↔", 249, delegate { ShowEmbedded(new frmPenjualan(), "Penjualan"); }, false);
            AddNav("Pengguna", "♙", 297, delegate { ShowEmbedded(new FormUser(), "Pengguna"); }, false);
            AddNav("Pengaturan Tema", "⚙", 345, ShowThemeSettings, false);
            // Label footer yang ditempel ke bawah sidebar.
            _sidebar.Controls.Add(new Label
            {
                Text = "TOKO MODERN • DESKTOP",
                Location = new Point(14, 0),
                Height = 25,
                Dock = DockStyle.Bottom,
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8F)
            });

            // Panel kanan menampung header dan area konten yang berganti sesuai navigasi.
            Panel right = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            // HEADER berisi judul halaman dan tanggal hari ini.
            _header = new Panel
            {
                Dock = DockStyle.Top, // Header menempel di bagian atas panel kanan.
                Height = 76, // Tinggi header dalam pixel.
                BackColor = AppTheme.Surface, // Warna permukaan header.
                Padding = new Padding(28, 12, 28, 10) // Jarak konten header dari tepi panel.
            };
            _pageTitle = new Label
            {
                Text = "Dashboard",
                Location = new Point(28, 15),
                Size = new Size(450, 28),
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = AppTheme.Text
            };
            _header.Controls.Add(_pageTitle);
            _header.Controls.Add(new Label
            {
                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Dock = DockStyle.Right,
                Width = 240,
                ForeColor = AppTheme.Muted,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F)
            });
            // CONTENT menjadi tempat dashboard atau form fitur yang sedang dibuka.
            _content = new Panel
            {
                Dock = DockStyle.Fill, // Isi ruang yang tersisa di bawah header.
                AutoScroll = true, // Tampilkan scroll jika isi halaman lebih panjang dari area.
                BackColor = AppTheme.Background, // Warna belakang area konten.
                Padding = new Padding(28, 24, 28, 24) // Margin dalam konten dashboard.
            };
            right.Controls.Add(_content);
            right.Controls.Add(_header);
            // Tambahkan panel kanan dan sidebar ke form, lalu jalankan layout final.
            _owner.Controls.Add(right);
            _owner.Controls.Add(_sidebar);
            _owner.ResumeLayout(true);
        }

        // Membuat satu tombol navigasi. Action adalah method yang dijalankan saat tombol diklik.
        // Tag menyimpan judul aslinya agar item aktif dapat dicari tanpa membaca teks tombol.
        private void AddNav(string title, string glyph, int top, Action action, bool selected)
        {
            Button button = new Button
            {
                Text = "  " + glyph + "     " + title,
                Tag = title,
                Location = new Point(8, top), // X/Y posisi tombol di dalam sidebar.
                Size = new Size(202, 42), // Lebar/tinggi tombol navigasi dalam pixel.
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5F, selected ? FontStyle.Bold : FontStyle.Regular),
                BackColor = selected ? AppTheme.Accent : AppTheme.Sidebar,
                ForeColor = selected ? Color.White : Color.FromArgb(210, 220, 236),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = AppTheme.Tint(AppTheme.Sidebar, .15);
            // Saat navigasi diklik, reset gaya item sebelumnya, tandai tombol ini sebagai aktif,
            // lalu jalankan action halaman yang dipilih.
            button.Click += delegate
            {
                if (_activeNav != null)
                {
                    _activeNav.BackColor = AppTheme.Sidebar;
                    _activeNav.ForeColor = Color.FromArgb(210, 220, 236);
                    _activeNav.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
                }
                _activeNav = button;
                button.BackColor = AppTheme.Accent;
                button.ForeColor = Color.White;
                button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                action();
            };
            _sidebar.Controls.Add(button);
            if (selected) _activeNav = button;
        }

        // Menampilkan form fitur (Barang, Penjualan, Pengguna, dan lain-lain) di dalam area konten.
        // Form anak dibuat non-top-level dan tanpa border agar terlihat seperti halaman dashboard.
        internal void ShowEmbedded(Form child, string title)
        {
            SetActiveNavigation(title == "Daftar Pengguna" ? "Pengguna" : title);
            _pageTitle.Text = title;
            _content.SuspendLayout();
            _content.Controls.Clear();
            // Tutup dan lepaskan halaman fitur sebelumnya agar kontrolnya tidak menumpuk.
            if (_embeddedForm != null)
            {
                _embeddedForm.Dispose();
                _embeddedForm = null;
            }
            // Simpan halaman aktif. Tag dipakai beberapa form untuk mengetahui parent FormUtama.
            _embeddedForm = child;
            child.Tag = _owner;
            child.TopLevel = false; // Form menjadi kontrol anak, bukan jendela terpisah.
            child.FormBorderStyle = FormBorderStyle.None; // Hilangkan bingkai jendela di dalam konten.
            child.Dock = DockStyle.Fill; // Isi seluruh area konten dengan form anak.
            child.Visible = true;
            _content.Padding = new Padding(0);
            _content.Controls.Add(child);
            child.Show();
            _content.ResumeLayout(true);
        }

        // Membersihkan halaman saat kembali ke dashboard/pengaturan tema.
        private void ResetContent()
        {
            if (_embeddedForm != null)
            {
                _embeddedForm.Dispose();
                _embeddedForm = null;
            }
            _content.Controls.Clear();
            _content.Padding = new Padding(28, 24, 28, 24);
        }

        // Menggambar ulang halaman ringkasan utama beserta metrik, grafik, inventori, dan shortcut.
        internal void ShowDashboard()
        {
            // Jika Build() belum selesai, kontrol dashboard belum tersedia; hentikan dengan aman.
            if (_pageTitle == null || _content == null) return;
            SetActiveNavigation("Dashboard");
            _pageTitle.Text = "Dashboard";
            ResetContent();
            _content.SuspendLayout();
            _content.Controls.Add(new Label
            {
                Text = "Monitoring Toko",
                Location = new Point(28, 18),
                Size = new Size(600, 34),
                Font = new Font("Segoe UI", 19, FontStyle.Bold),
                ForeColor = AppTheme.Text
            });
            _content.Controls.Add(new Label
            {
                Text = "Pantau stok, penjualan, dan aktivitas bisnis dalam satu tampilan.",
                Location = new Point(30, 54),
                Size = new Size(750, 24),
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 10F)
            });

            // Minta data aktual kepada FormUtama; template hanya menampilkan nilai yang diterima.
            DashboardData data = _owner.LoadDashboardData();
            // Empat kartu KPI. Nilai diambil dari DashboardData atau placeholder jika belum dihubungkan.
            AddMetric("JENIS BARANG", data.ItemCount.ToString("N0"), "Produk terdaftar", "▤", 28, 100, Color.FromArgb(45, 110, 220));
            AddMetric("TOTAL STOK", data.TotalStock.ToString("N0"), "Unit tersedia*", "▦", 292, 100, Color.FromArgb(16, 155, 112));
            AddMetric("BARANG TERJUAL", "—", "Total unit terjual", "↗", 556, 100, Color.FromArgb(124, 78, 210));
            AddMetric("OMZET HARI INI", "—", "Total transaksi hari ini", "Rp", 820, 100, Color.FromArgb(230, 126, 34));

            // Kartu grafik penjualan; event Paint menggambar garis dan batang secara manual.
            Panel chart = new Panel
            {
                Location = new Point(28, 225),
                Size = new Size(550, 285),
                BackColor = AppTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };
            chart.Controls.Add(new Label
            {
                Text = "Tren Penjualan",
                Location = new Point(20, 16),
                Size = new Size(260, 25),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = AppTheme.Text
            });
            // Catatan bahwa grafik saat ini masih contoh visual dan dapat disambungkan ke data asli.
            chart.Controls.Add(new Label
            {
                Text = "Visualisasi siap dihubungkan dengan data transaksi",
                Location = new Point(20, 43),
                Size = new Size(460, 20),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = AppTheme.Muted
            });
            // Daftarkan fungsi penggambar yang dipanggil ulang saat panel perlu dirender.
            chart.Paint += DrawSalesChart;
            _content.Controls.Add(chart);

            // Kartu inventori berisi jumlah barang, total stok, dan indikator stok yang perlu dicek.
            Panel stockPanel = new Panel
            {
                Location = new Point(596, 225),
                Size = new Size(510, 285),
                BackColor = AppTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };
            stockPanel.Controls.Add(new Label
            {
                Text = "Status Inventori",
                Location = new Point(20, 16),
                Size = new Size(260, 25),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = AppTheme.Text
            });
            AddInventoryLine(stockPanel, "Total jenis barang", data.ItemCount.ToString("N0"), 58, AppTheme.Accent);
            AddInventoryLine(stockPanel, "Total stok terdata", data.TotalStock.ToString("N0"), 105, Color.FromArgb(16, 155, 112));
            AddInventoryLine(stockPanel, "Stok menipis", "Perlu konfigurasi batas minimum", 152, Color.FromArgb(230, 126, 34));
            AddInventoryLine(stockPanel, "Barang habis", "Perlu validasi data stok", 199, Color.FromArgb(219, 70, 70));
            _content.Controls.Add(stockPanel);

            _content.ResumeLayout(true);
        }

        // Helper pembuat kartu KPI pada dashboard.
        // title/value/subtitle adalah teks; glyph ikon; x/y posisi kartu; color warna aksen kartu.
        private void AddMetric(string title, string value, string subtitle, string glyph, int x, int y, Color color)
        {
            Panel card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(248, 108),
                BackColor = AppTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };
            // Garis aksen tipis di bagian atas kartu membedakan jenis metrik.
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 4, BackColor = color });
            card.Controls.Add(new Label
            {
                Text = glyph,
                Location = new Point(15, 18),
                Size = new Size(40, 40),
                BackColor = AppTheme.Tint(color, .87),
                ForeColor = color,
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            });
            card.Controls.Add(new Label
            {
                Text = title,
                Location = new Point(65, 15),
                Size = new Size(170, 18),
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            });
            card.Controls.Add(new Label
            {
                Text = value,
                Location = new Point(65, 36),
                Size = new Size(170, 30),
                ForeColor = AppTheme.Text,
                Font = new Font("Segoe UI", 18, FontStyle.Bold)
            });
            card.Controls.Add(new Label
            {
                Text = subtitle,
                Location = new Point(15, 78),
                Size = new Size(220, 18),
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 8F)
            });
            _content.Controls.Add(card);
        }

        // Menambahkan satu baris informasi persediaan: penanda warna, label kiri, dan nilai rata kanan.
        private void AddInventoryLine(Panel panel, string label, string value, int y, Color color)
        {
            panel.Controls.Add(new Panel { Location = new Point(20, y + 3), Size = new Size(5, 27), BackColor = color });
            panel.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(36, y),
                Size = new Size(210, 30),
                ForeColor = AppTheme.Text,
                Font = new Font("Segoe UI", 9.5F),
                TextAlign = ContentAlignment.MiddleLeft
            });
            panel.Controls.Add(new Label
            {
                Text = value,
                Location = new Point(250, y),
                Size = new Size(235, 30),
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            });
        }

        // Membuat tombol pintasan. Action menentukan form/halaman yang dibuka ketika diklik.
        private void AddQuickButton(Panel panel, string text, int x, int y, Action action)
        {
            Button button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(190, 38),
                BackColor = AppTheme.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            // Jalankan callback yang dikirim saat tombol shortcut dibuat.
            button.Click += delegate { action(); };
            panel.Controls.Add(button);
        }

        // Menggambar grafik contoh secara manual tanpa komponen chart tambahan.
        // Angka values saat ini adalah contoh visual; gantikan dengan data transaksi ketika tersedia.
        private void DrawSalesChart(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Batas area plot: kiri/atas adalah titik awal, width/height adalah ukuran grafik.
            int left = 45, top = 95, width = 460, height = 145;
            using (Pen gridPen = new Pen(Color.FromArgb(230, 234, 242)))
            using (SolidBrush barBrush = new SolidBrush(AppTheme.Tint(AppTheme.Accent, .22)))
            using (SolidBrush accentBrush = new SolidBrush(AppTheme.Accent))
            using (SolidBrush textBrush = new SolidBrush(AppTheme.Muted))
            using (Font font = new Font("Segoe UI", 8F))
            {
                for (int i = 0; i < 4; i++)
                    graphics.DrawLine(gridPen, left, top + i * height / 3, left + width, top + i * height / 3);
                // Data contoh grafik dan label hari yang dipasangkan berdasarkan indeks array.
                // Tinggi batang adalah angka contoh relatif (0-100), bukan angka penjualan dari database.
                int[] values = { 38, 58, 45, 78, 62, 88, 70 };
                string[] days = { "Sen", "Sel", "Rab", "Kam", "Jum", "Sab", "Min" };
                // barWidth mengatur lebar batang; gap adalah jarak antarbatang.
                int barWidth = 35, gap = 27;
                for (int i = 0; i < values.Length; i++)
                {
                    int barHeight = values[i] * height / 100;
                    int x = left + 10 + i * (barWidth + gap);
                    graphics.FillRectangle(i == values.Length - 1 ? accentBrush : barBrush,
                        x, top + height - barHeight, barWidth, barHeight);
                    graphics.DrawString(days[i], font, textBrush, x - 2, top + height + 8);
                }
            }
        }

        // Halaman personalisasi warna. Daftar preset diambil dari AppTheme agar konsisten global.
        internal void ShowThemeSettings()
        {
            SetActiveNavigation("Pengaturan Tema");
            ResetContent();
            _pageTitle.Text = "Pengaturan Tema";
            _content.Controls.Add(new Label
            {
                Text = "Personalisasi tampilan",
                Location = new Point(28, 22),
                Size = new Size(500, 34), // Judul halaman tema.
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = AppTheme.Text
            });
            _content.Controls.Add(new Label
            {
                Text = "Pilih warna aksen untuk menu aktif, tombol, ikon, dan kartu dashboard.",
                Location = new Point(30, 60),
                Size = new Size(700, 24),
                ForeColor = AppTheme.Muted,
                Font = new Font("Segoe UI", 10F)
            });
            Panel card = new Panel
            {
                Location = new Point(28, 110),
                Size = new Size(660, 355),
                BackColor = AppTheme.Surface,
                BorderStyle = BorderStyle.FixedSingle
            };
            card.Controls.Add(new Label
            {
                Text = "PILIH PALET WARNA",
                Location = new Point(22, 18),
                Size = new Size(260, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppTheme.Text
            });
            Panel preview = new Panel { Location = new Point(22, 55), Size = new Size(110, 80), BackColor = AppTheme.Accent };
            preview.Controls.Add(new Label
            {
                Text = "PREVIEW",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            });
            card.Controls.Add(preview);

            // Buat tombol swatch dari seluruh preset. Dua kolom dirapikan memakai indeks array.
            for (int i = 0; i < AppTheme.Presets.Length; i++)
            {
                AppTheme.ThemePreset preset = AppTheme.Presets[i];
                Color picked = preset.Color;
                // Membagi swatch jadi dua kolom: sisa bagi menentukan kolom, pembagian bulat menentukan baris.
                int column = i % 2, row = i / 2;
                Button swatch = new Button
                {
                    Text = preset.Name,
                    Location = new Point(160 + column * 220, 55 + row * 48), // Letak tombol warna berdasarkan baris/kolom.
                    Size = new Size(195, 36),
                    BackColor = picked,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                swatch.FlatAppearance.BorderSize = 0;
                // Setelah warna dipilih, ubah tema global dan bangun ulang UI agar warna langsung terlihat.
                swatch.Click += delegate { AppTheme.SetAccent(picked); Build(); ShowThemeSettings(); };
                card.Controls.Add(swatch);
            }

            // Tombol untuk membuka dialog pemilih warna bawaan Windows.
            Button custom = new Button
            {
                Text = "Pilih warna sendiri...",
                Location = new Point(22, 270),
                Size = new Size(205, 42),
                BackColor = AppTheme.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            custom.FlatAppearance.BorderSize = 0;
            custom.Click += delegate
            {
                // using memastikan ColorDialog dibersihkan setelah pengguna menutup dialog.
                using (ColorDialog dialog = new ColorDialog())
                {
                    dialog.Color = AppTheme.Accent;
                    dialog.FullOpen = true;
                    if (dialog.ShowDialog(_owner) == DialogResult.OK)
                    {
                        AppTheme.SetAccent(dialog.Color);
                        Build();
                        ShowThemeSettings();
                    }
                }
            };
            card.Controls.Add(custom);

            // Kembalikan warna aksen ke biru default aplikasi.
            Button reset = new Button
            {
                Text = "Reset tema",
                Location = new Point(242, 270),
                Size = new Size(150, 42),
                BackColor = AppTheme.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            reset.FlatAppearance.BorderSize = 0;
            reset.Click += delegate { AppTheme.SetAccent(Color.FromArgb(45, 110, 220)); Build(); ShowThemeSettings(); };
            card.Controls.Add(reset);
            _content.Controls.Add(card);
        }

        // Menyamakan tampilan navigasi aktif dengan halaman yang sedang dibuka.
        // Judul dibandingkan dengan Tag tombol, bukan dengan string tampilan yang mungkin berisi ikon.
        private void SetActiveNavigation(string title)
        {
            if (_sidebar == null) return;
            foreach (Button nav in _sidebar.Controls.OfType<Button>())
            {
                // Perbandingan tanpa membedakan huruf besar/kecil untuk menghindari salah pilih menu.
                bool selected = string.Equals(Convert.ToString(nav.Tag), title, StringComparison.OrdinalIgnoreCase);
                // Operator ?: memilih warna aktif atau nonaktif berdasarkan nilai selected.
                nav.BackColor = selected ? AppTheme.Accent : AppTheme.Sidebar;
                nav.ForeColor = selected ? Color.White : Color.FromArgb(210, 220, 236);
                nav.Font = new Font("Segoe UI", 9.5F, selected ? FontStyle.Bold : FontStyle.Regular);
                if (selected) _activeNav = nav;
            }
        }
    }
}
