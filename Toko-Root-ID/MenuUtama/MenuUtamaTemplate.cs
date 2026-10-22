using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CommonClass;
using Toko_Root_ID;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.MenuUtama
{
    /// <summary>
    /// Komposisi visual menu utama: sidebar bergrup, header, dashboard monitoring,
    /// dan host untuk form fitur. FormUtama hanya menyediakan navigasi dan data.
    /// </summary>
    internal sealed class MenuUtamaTemplate
    {
        /// <summary>Data dashboard yang dimuat FormUtama dari DALaporan.</summary>
        internal sealed class DashboardData
        {
            public ClassDashboard Summary = new ClassDashboard();
            public List<ClassChartPoint> Sales7 = new List<ClassChartPoint>();
            public List<ClassChartPoint> TopProducts = new List<ClassChartPoint>();
            public string Status = "Data diperbarui dari database";
            public bool Error;

            public decimal OmzetHariIni { get { return Summary == null ? 0m : Summary.omzet_hari_ini; } }
            public decimal OmzetBulanIni { get { return Summary == null ? 0m : Summary.omzet_bulan_ini; } }
            public int TransaksiHariIni { get { return Summary == null ? 0 : Summary.transaksi_hari_ini; } }
            public int JumlahProduk { get { return Summary == null ? 0 : Summary.jumlah_produk; } }
            public decimal TotalStok { get { return Summary == null ? 0m : Summary.total_stok; } }
            public int StokMenipis { get { return Summary == null ? 0 : Summary.stok_menipis; } }
            public int StokHabis { get { return Summary == null ? 0 : Summary.stok_habis; } }
            public decimal Piutang { get { return Summary == null ? 0m : Summary.total_piutang; } }
            public decimal Hutang { get { return Summary == null ? 0m : Summary.total_hutang; } }
            public decimal LabaBulanIni { get { return Summary == null ? 0m : Summary.laba_bulan_ini; } }
        }

        private readonly FormUtama _owner;
        private Panel _sidebar;
        private Panel _header;
        private Panel _content;
        private Panel _navPanel;
        private Label _pageTitle;
        private Button _activeNav;
        private Form _embeddedForm;
        private string _currentPage = "Dashboard";

        private readonly List<Control> _navOrder = new List<Control>();
        private readonly Dictionary<string, Button> _navButtons = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<Button>> _navGroups = new Dictionary<string, List<Button>>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> _allowedMenus;

        internal MenuUtamaTemplate(FormUtama owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            _owner = owner;
        }

        // =========================================================
        // KERANGKA APLIKASI
        // =========================================================
        internal void Build()
        {
            _owner.SuspendLayout();
            _owner.Controls.Clear();
            _owner.Text = "Toko Modern | Sistem Toko";
            _owner.Name = "FormUtama";
            _owner.StartPosition = FormStartPosition.CenterScreen;
            _owner.MinimumSize = new Size(1100, 700);
            _owner.Size = new Size(1320, 840);
            _owner.BackColor = AppTheme.Background;
            _owner.Font = new Font("Segoe UI", 9F);

            // SIDEBAR
            _sidebar = new Panel
            {
                Name = "sidebar",
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = AppTheme.Sidebar
            };

            _navPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppTheme.Sidebar,
                Padding = new Padding(10, 4, 10, 8)
            };

            var brand = new Panel { Dock = DockStyle.Top, Height = 104, BackColor = AppTheme.Sidebar };
            brand.Controls.Add(new Label
            {
                Text = "T",
                Location = new Point(14, 20),
                Size = new Size(42, 42),
                BackColor = AppTheme.Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            });
            brand.Controls.Add(new Label
            {
                Text = "TOKO MODERN",
                Location = new Point(66, 21),
                Size = new Size(160, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold)
            });
            brand.Controls.Add(new Label
            {
                Text = "STORE MANAGEMENT",
                Location = new Point(67, 45),
                Size = new Size(160, 18),
                ForeColor = Color.FromArgb(174, 188, 211),
                Font = new Font("Segoe UI", 7F, FontStyle.Bold)
            });

            var footer = new Label
            {
                Text = "  TOKO MODERN • DESKTOP",
                Dock = DockStyle.Bottom,
                Height = 34,
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8F)
            };

            _sidebar.Controls.Add(_navPanel);
            _sidebar.Controls.Add(footer);
            _sidebar.Controls.Add(brand);

            BuildNavigation();

            // PANEL KANAN: header + konten
            var right = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            _header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = AppTheme.Surface,
                Padding = new Padding(28, 12, 28, 10)
            };

            _pageTitle = new Label
            {
                Text = "Dashboard",
                Location = new Point(28, 16),
                Size = new Size(560, 30),
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = AppTheme.Text
            };
            _header.Controls.Add(_pageTitle);
            _header.Controls.Add(new Label
            {
                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Dock = DockStyle.Right,
                Width = 250,
                ForeColor = AppTheme.Muted,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F)
            });

            _content = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = AppTheme.Background,
                Padding = new Padding(24, 20, 24, 20)
            };

            right.Controls.Add(_content);
            right.Controls.Add(_header);
            _owner.Controls.Add(right);
            _owner.Controls.Add(_sidebar);
            _owner.ResumeLayout(true);
        }

        private void BuildNavigation()
        {
            _navOrder.Clear();
            _navButtons.Clear();
            _navGroups.Clear();
            _navPanel.Controls.Clear();

            AddGroup("UTAMA");
            AddNav("UTAMA", "Dashboard", "▦", delegate { _owner.NavigateDashboard(); }, true);

            AddGroup("TRANSAKSI");
            AddNav("TRANSAKSI", "Kasir (POS)", "▣", delegate { _owner.NavigatePOS(); }, false);
            AddNav("TRANSAKSI", "Pembelian", "⇩", delegate { _owner.NavigatePembelian(); }, false);
            AddNav("TRANSAKSI", "Retur Barang", "↺", delegate { _owner.NavigateRetur(); }, false);

            AddGroup("GUDANG");
            AddNav("GUDANG", "Inventory", "▤", delegate { _owner.NavigateInventory(); }, false);
            AddNav("GUDANG", "Master Data", "▥", delegate { _owner.NavigateMaster(); }, false);

            AddGroup("KEUANGAN");
            AddNav("KEUANGAN", "Keuangan", "Rp", delegate { _owner.NavigateKeuangan(); }, false);
            AddNav("KEUANGAN", "Laporan dan Grafik", "◔", delegate { _owner.NavigateLaporan(); }, false);

            AddGroup("SISTEM");
            AddNav("SISTEM", "Pengguna", "♙", delegate { _owner.NavigatePengguna(); }, false);
            AddNav("SISTEM", "Pengaturan Toko", "⚙", delegate { _owner.NavigatePengaturan(); }, false);

            RelayoutNav();
        }

        private void AddGroup(string title)
        {
            var label = new Label
            {
                Text = title,
                Height = 22,
                Width = 198,
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                Tag = "group:" + title
            };
            _navGroups[title] = new List<Button>();
            _navOrder.Add(label);
            _navPanel.Controls.Add(label);
        }

        private void AddNav(string group, string title, string glyph, Action action, bool selected)
        {
            var button = new Button
            {
                Text = "  " + glyph + "     " + title,
                Tag = title,
                Width = 198,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5F, selected ? FontStyle.Bold : FontStyle.Regular),
                BackColor = selected ? AppTheme.Accent : AppTheme.Sidebar,
                ForeColor = selected ? Color.White : Color.FromArgb(210, 220, 236),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = AppTheme.Tint(AppTheme.Sidebar, .15);
            button.Click += delegate
            {
                SetActiveNavigation(title);
                action();
            };

            _navOrder.Add(button);
            _navPanel.Controls.Add(button);
            _navButtons[title] = button;
            if (_navGroups.ContainsKey(group)) _navGroups[group].Add(button);
            if (selected) _activeNav = button;
        }

        private void RelayoutNav()
        {
            int y = 4;
            foreach (Control control in _navOrder)
            {
                string group = Convert.ToString(control.Tag);
                if (control is Label && group != null && group.StartsWith("group:", StringComparison.Ordinal))
                {
                    string name = group.Substring(6);
                    bool any = false;
                    if (_navGroups.ContainsKey(name))
                        foreach (Button nav in _navGroups[name]) any |= nav.Visible;

                    control.Visible = any;
                    if (any)
                    {
                        control.Location = new Point(4, y);
                        y += 26;
                    }
                }
                else
                {
                    if (!control.Visible) continue;
                    control.Location = new Point(4, y);
                    y += 44;
                }
            }
            _navPanel.AutoScrollMinSize = new Size(0, y + 8);
        }

        // =========================================================
        // HAK AKSES MENU
        // =========================================================
        /// <summary>Menampilkan menu sesuai role: owner/admin, manager, kasir, atau gudang.</summary>
        internal void ApplyRole(string role)
        {
            string value = (role ?? string.Empty).Trim().ToLowerInvariant();
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            allowed.Add("Dashboard");
            allowed.Add("Retur Barang");

            if (value == "kasir")
            {
                allowed.Add("Kasir (POS)");
            }
            else if (value == "gudang")
            {
                allowed.Add("Pembelian");
                allowed.Add("Inventory");
                allowed.Add("Master Data");
            }
            else if (value == "manager")
            {
                allowed.Add("Kasir (POS)");
                allowed.Add("Pembelian");
                allowed.Add("Inventory");
                allowed.Add("Master Data");
                allowed.Add("Keuangan");
                allowed.Add("Laporan dan Grafik");
            }
            else
            {
                // owner, admin, atau mode tanpa login: seluruh menu.
                foreach (string title in _navButtons.Keys) allowed.Add(title);
            }

            foreach (KeyValuePair<string, Button> pair in _navButtons)
                pair.Value.Visible = allowed.Contains(pair.Key);

            _allowedMenus = allowed;
            RelayoutNav();

            if (_currentPage == "Dashboard" || (!string.IsNullOrEmpty(_currentPage) && !allowed.Contains(_currentPage) && _currentPage != "Pengaturan Tema"))
                ShowDashboard();
        }

        /// <summary>True bila role saat ini boleh membuka menu dengan judul tersebut.</summary>
        internal bool CanOpen(string title)
        {
            return _allowedMenus == null || _allowedMenus.Contains(title);
        }

        // =========================================================
        // NAVIGASI HALAMAN
        // =========================================================
        internal void ShowEmbedded(Form child, string title)
        {
            if (_pageTitle == null || _content == null) return;
            _currentPage = title;
            SetActiveNavigation(title);
            _pageTitle.Text = title;
            _content.SuspendLayout();
            _content.Controls.Clear();
            if (_embeddedForm != null)
            {
                _embeddedForm.Dispose();
                _embeddedForm = null;
            }

            _embeddedForm = child;
            child.Tag = _owner;
            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;
            child.Visible = true;
            _content.Padding = new Padding(0);
            _content.Controls.Add(child);
            child.Show();
            _content.ResumeLayout(true);
        }

        private void ResetContent()
        {
            if (_embeddedForm != null)
            {
                _embeddedForm.Dispose();
                _embeddedForm = null;
            }
            _content.Controls.Clear();
            _content.Padding = new Padding(24, 20, 24, 20);
        }

        // =========================================================
        // DASHBOARD
        // =========================================================
        internal void ShowDashboard()
        {
            if (_pageTitle == null || _content == null) return;
            _currentPage = "Dashboard";
            SetActiveNavigation("Dashboard");
            _pageTitle.Text = "Dashboard";
            ResetContent();

            DashboardData data = _owner.LoadDashboardData();

            _content.SuspendLayout();
            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = AppTheme.Background,
                Margin = new Padding(0)
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            _content.Controls.Add(page);

            // Judul kecil + status pemuatan data.
            var intro = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            intro.Controls.Add(new Label
            {
                Text = "Ringkasan Toko",
                Location = new Point(2, 0),
                Size = new Size(360, 28),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = AppTheme.Text
            });
            intro.Controls.Add(new Label
            {
                Text = "Stok, penjualan, dan keuangan dalam satu tampilan.",
                Location = new Point(4, 29),
                Size = new Size(560, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppTheme.Muted
            });
            intro.Controls.Add(new Label
            {
                Text = data.Status,
                Dock = DockStyle.Right,
                Width = 400,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = data.Error ? AppTheme.Danger : AppTheme.Muted
            });
            page.Controls.Add(intro, 0, 0);

            // Kartu KPI.
            var metrics = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = AppTheme.Background,
                Margin = new Padding(0)
            };
            for (int i = 0; i < 4; i++) metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            metrics.Controls.Add(Metric("OMZET HARI INI", "Rp " + data.OmzetHariIni.ToString("N0"),
                data.TransaksiHariIni.ToString("N0") + " transaksi", "Rp", AppTheme.Accent), 0, 0);
            metrics.Controls.Add(Metric("OMZET BULAN INI", "Rp " + data.OmzetBulanIni.ToString("N0"),
                "Laba kotor Rp " + data.LabaBulanIni.ToString("N0"), "↗", AppTheme.Success), 1, 0);
            metrics.Controls.Add(Metric("PRODUK & STOK", data.JumlahProduk.ToString("N0") + " produk",
                data.TotalStok.ToString("N0") + " unit tersedia", "▤", Color.FromArgb(124, 78, 210)), 2, 0);
            metrics.Controls.Add(Metric("PERLU PERHATIAN", (data.StokMenipis + data.StokHabis).ToString("N0") + " barang",
                data.StokHabis.ToString("N0") + " habis • " + data.StokMenipis.ToString("N0") + " menipis", "!", AppTheme.Warning), 3, 0);
            page.Controls.Add(metrics, 0, 1);

            // Baris grafik: tren 7 hari + komposisi stok.
            var chartsRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background,
                Margin = new Padding(0, 4, 0, 4)
            };
            chartsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            chartsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var bar = new BarChartControl { Dock = DockStyle.Fill, ValuePrefix = "Rp", Currency = true, Accent = AppTheme.Accent };
            bar.SetData(data.Sales7);
            chartsRow.Controls.Add(ChartCard("Tren Penjualan 7 Hari", "Omzet harian dari transaksi posted", bar), 0, 0);

            var donut = new DonutChartControl
            {
                Dock = DockStyle.Fill,
                Aman = Math.Max(0, data.JumlahProduk - data.StokMenipis - data.StokHabis),
                Menipis = data.StokMenipis,
                Habis = data.StokHabis,
                Caption = "Komposisi stok"
            };
            chartsRow.Controls.Add(ChartCard("Status Persediaan", "Aman, menipis, dan habis", donut), 1, 0);
            page.Controls.Add(chartsRow, 0, 2);

            // Baris bawah: produk terlaris + ringkasan keuangan/aksi cepat.
            var bottomRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background,
                Margin = new Padding(0, 4, 0, 0)
            };
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var top = new HBarChartControl { Dock = DockStyle.Fill, Accent = Color.FromArgb(124, 78, 210), ValueSuffix = " pcs" };
            top.SetData(data.TopProducts);
            bottomRow.Controls.Add(ChartCard("Produk Terlaris", "Berdasarkan qty terjual", top), 0, 0);
            bottomRow.Controls.Add(FinanceCard(data), 1, 0);
            page.Controls.Add(bottomRow, 0, 3);

            _content.ResumeLayout(true);
        }

        private static StatCard Metric(string title, string value, string subtitle, string glyph, Color accent)
        {
            return new StatCard
            {
                Dock = DockStyle.Fill,
                Title = title,
                Value = value,
                Subtitle = subtitle,
                Glyph = glyph,
                Accent = accent,
                Margin = new Padding(4)
            };
        }

        private Panel ChartCard(string title, string subtitle, Control body)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(14, 10, 14, 12),
                Margin = new Padding(4)
            };
            var head = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = AppTheme.Surface };
            head.Controls.Add(new Label
            {
                Text = title,
                Location = new Point(0, 0),
                Size = new Size(360, 22),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = AppTheme.Text
            });
            head.Controls.Add(new Label
            {
                Text = subtitle,
                Location = new Point(1, 22),
                Size = new Size(420, 18),
                Font = new Font("Segoe UI", 8F),
                ForeColor = AppTheme.Muted
            });

            body.Dock = DockStyle.Fill;
            card.Controls.Add(body);
            card.Controls.Add(head);
            return card;
        }

        private Panel FinanceCard(DashboardData data)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(14, 10, 14, 12),
                Margin = new Padding(4)
            };
            card.Controls.Add(new Label
            {
                Text = "Keuangan dan Aksi Cepat",
                Location = new Point(14, 10),
                Size = new Size(320, 22),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = AppTheme.Text
            });

            var lines = new TableLayoutPanel
            {
                Location = new Point(14, 38),
                Size = new Size(330, 104),
                ColumnCount = 2,
                RowCount = 3,
                BackColor = AppTheme.Surface,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            card.Resize += delegate
            {
                lines.Width = Math.Max(220, card.ClientSize.Width - 28);
            };
            lines.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            lines.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            for (int i = 0; i < 3; i++) lines.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3F));
            AddFinanceLine(lines, 0, "Piutang pelanggan", "Rp " + data.Piutang.ToString("N0"), AppTheme.Warning);
            AddFinanceLine(lines, 1, "Hutang supplier", "Rp " + data.Hutang.ToString("N0"), AppTheme.Danger);
            AddFinanceLine(lines, 2, "Laba kotor bulan ini", "Rp " + data.LabaBulanIni.ToString("N0"), AppTheme.Success);
            card.Controls.Add(lines);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.Surface
            };
            if (CanOpen("Kasir (POS)"))
                actions.Controls.Add(QuickButton("Buka Kasir", delegate { _owner.NavigatePOS(); }));
            if (CanOpen("Pembelian"))
                actions.Controls.Add(QuickButton("Pembelian", delegate { _owner.NavigatePembelian(); }));
            if (CanOpen("Laporan dan Grafik"))
                actions.Controls.Add(QuickButton("Laporan", delegate { _owner.NavigateLaporan(); }));
            if (actions.Controls.Count == 0)
                actions.Visible = false;
            card.Controls.Add(actions);

            return card;
        }

        private static void AddFinanceLine(TableLayoutPanel table, int row, string label, string value, Color color)
        {
            table.Controls.Add(new Label
            {
                Text = "●  " + label,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = color
            }, 0, row);
            table.Controls.Add(new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppTheme.Text
            }, 1, row);
        }

        private static Button QuickButton(string text, Action action)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(102, 34),
                BackColor = AppTheme.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 4, 6, 0)
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += delegate { action(); };
            return button;
        }

        // Halaman tema lama diarahkan ke modul pengaturan yang baru.
        internal void ShowThemeSettings()
        {
            _owner.NavigatePengaturan();
        }

        private void SetActiveNavigation(string title)
        {
            if (_navButtons.ContainsKey(title)) _activeNav = _navButtons[title];
            foreach (Button nav in _navButtons.Values)
            {
                bool selected = string.Equals(Convert.ToString(nav.Tag), title, StringComparison.OrdinalIgnoreCase);
                nav.BackColor = selected ? AppTheme.Accent : AppTheme.Sidebar;
                nav.ForeColor = selected ? Color.White : Color.FromArgb(210, 220, 236);
                nav.Font = new Font("Segoe UI", 9.5F, selected ? FontStyle.Bold : FontStyle.Regular);
            }
        }
    }
}
