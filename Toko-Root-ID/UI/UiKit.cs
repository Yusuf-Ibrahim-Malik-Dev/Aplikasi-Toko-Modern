using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Toko_Root_ID.UI
{
    /// <summary>
    /// Kumpulan helper tampilan agar seluruh modul baru memakai bahasa visual
    /// yang sama: kartu membulat, judul, input, tombol, grid, dan format rupiah.
    /// </summary>
    internal static class UiKit
    {
        public static readonly Font FontTitle = new Font("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font FontSection = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font FontBody = new Font("Segoe UI", 9.5F);
        public static readonly Font FontSmall = new Font("Segoe UI", 8.5F);
        public static readonly Font FontValue = new Font("Segoe UI", 16F, FontStyle.Bold);

        public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("id-ID");

        public static string Rupiah(decimal value)
        {
            return "Rp " + value.ToString("N0", Culture);
        }

        public static string Angka(decimal value)
        {
            return value.ToString("N0", Culture);
        }

        public static decimal ToDecimal(string text)
        {
            decimal value;
            if (decimal.TryParse(text, NumberStyles.Number, Culture, out value)) return value;
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value)) return value;
            return 0m;
        }

        public static decimal ToDecimal(TextBox textBox)
        {
            return textBox == null ? 0m : ToDecimal(textBox.Text);
        }

        public static int ToInt(string text)
        {
            int value;
            return int.TryParse(text, out value) ? value : 0;
        }

        /// <summary>Menampilkan teks petunjuk pada kotak pencarian yang otomatis hilang saat diketik.</summary>
        public static void SetPlaceholder(TextBox box, string text)
        {
            if (box == null) return;
            bool showing = false;

            Action show = delegate
            {
                if (box.Text.Trim().Length != 0) return;
                showing = true;
                box.Tag = "placeholder";
                box.Text = text;
                box.ForeColor = AppTheme.Muted;
            };

            box.GotFocus += delegate
            {
                if (!showing) return;
                showing = false;
                box.Tag = null;
                box.Text = string.Empty;
                box.ForeColor = AppTheme.Text;
            };
            box.LostFocus += delegate
            {
                if (box.Text.Trim().Length != 0) return;
                show();
            };
            show();
        }

        /// <summary>Nilai pencarian tanpa teks petunjuk.</summary>
        public static string SearchText(TextBox box)
        {
            if (box == null) return string.Empty;
            return Convert.ToString(box.Tag) == "placeholder" ? string.Empty : box.Text.Trim();
        }

        public static Label Title(string text, int x, int y, int width = 640)        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 34),
                Font = FontTitle,
                ForeColor = AppTheme.Text,
                BackColor = Color.Transparent
            };
        }

        public static Label Subtitle(string text, int x, int y, int width = 760)        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 22),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = AppTheme.Muted,
                BackColor = Color.Transparent
            };
        }

        public static Label FieldLabel(string text, int x, int y, int width = 220)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = AppTheme.Muted,
                BackColor = Color.Transparent
            };
        }

        public static Label Section(string text, int x, int y, int width = 300)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 24),
                Font = FontSection,
                ForeColor = AppTheme.Text,
                BackColor = Color.Transparent
            };
        }

        public static RoundedPanel Card(int x, int y, int width, int height)
        {
            return new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = AppTheme.Surface,
                BorderColor = AppTheme.Border,
                Radius = 12
            };
        }

        public static TextBox Input(int x, int y, int width, string text = "")
        {
            var box = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 28),
                Text = text,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.Text
            };
            return box;
        }

        public static ComboBox Combo(int x, int y, int width)
        {
            var combo = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 28),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.Text,
                FlatStyle = FlatStyle.Flat
            };
            return combo;
        }

        public static DateTimePicker Date(int x, int y, int width)
        {
            return new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(width, 28),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd MMM yyyy"
            };
        }

        public static NumericUpDown Numeric(int x, int y, int width, decimal max = 1000000000m, int decimals = 0)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(width, 28),
                Font = new Font("Segoe UI", 10F),
                Maximum = max,
                DecimalPlaces = decimals,
                ThousandsSeparator = true,
                TextAlign = HorizontalAlignment.Right,
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.Text,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        public static Button Button(string text, EventHandler click, AppTheme.ButtonStyle style = AppTheme.ButtonStyle.Primary)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(120, 34),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            AppTheme.StyleButton(button, style);
            if (click != null) button.Click += click;
            return button;
        }

        public static DataGridView Grid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = AppTheme.Surface,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = true,
                Font = FontBody
            };
            AppTheme.StyleDataGridView(grid);
            grid.ColumnHeadersHeight = 40;
            grid.RowTemplate.Height = 34;
            return grid;
        }

        public static void AddField(Control parent, string label, Control input, int x, int y, int width)
        {
            parent.Controls.Add(FieldLabel(label, x, y, width));
            input.Location = new Point(x, y + 20);
            input.Width = width;
            parent.Controls.Add(input);
        }

        /// <summary>Kerangka halaman standar: judul, subjudul, area aksi, dan badan halaman.</summary>
        internal sealed class PageView
        {
            public Panel Header;
            public Panel Body;
            public Label Title;
            public Label Subtitle;
            public FlowLayoutPanel Actions;
        }

        public static PageView Page(Form form, string title, string subtitle = null)
        {
            form.SuspendLayout();
            form.Controls.Clear();
            form.BackColor = AppTheme.Background;
            form.ForeColor = AppTheme.Text;
            form.Font = new Font("Segoe UI", 9F);
            form.MinimumSize = new Size(1000, 640);

            var view = new PageView();
            var root = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(20) };
            form.Controls.Add(root);

            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = AppTheme.Background };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(shell);

            view.Header = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            view.Title = Title(title, 0, 0, 760);
            view.Subtitle = Subtitle(subtitle ?? string.Empty, 0, 36, 820);
            view.Actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 400,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = AppTheme.Background,
                Padding = new Padding(0, 8, 0, 0)
            };
            view.Header.Controls.Add(view.Actions);
            view.Header.Controls.Add(view.Subtitle);
            view.Header.Controls.Add(view.Title);
            shell.Controls.Add(view.Header, 0, 0);

            view.Body = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
            shell.Controls.Add(view.Body, 0, 1);

            form.ResumeLayout(true);
            return view;
        }

        /// <summary>TabControl dengan gaya yang selaras tema untuk modul berisi beberapa halaman.</summary>
        public static TabControl Tabs()
        {
            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(150, 34),
                Padding = new Point(14, 6),
                BackColor = AppTheme.Background
            };
            return tabs;
        }

        public static TabPage Tab(string text)
        {
            return new TabPage(text)
            {
                BackColor = AppTheme.Background,
                Padding = new Padding(0, 10, 0, 0),
                UseVisualStyleBackColor = false
            };
        }

        /// <summary>Kartu berisi toolbar di atas dan grid mengisi sisa ruang.</summary>
        public static RoundedPanel ListCard(Control toolbar, Control grid, int rightMargin = 0)
        {
            var card = Card(0, 0, 200, 200);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 0, rightMargin, 0);
            card.Padding = new Padding(12);
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.Surface
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            toolbar.Dock = DockStyle.Fill;
            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(grid, 0, 1);
            card.Controls.Add(layout);
            return card;
        }

        public static Panel ListToolbar(out TextBox search, out Button refresh, string placeholder = "Cari data...")
        {
            var bar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = AppTheme.Surface };
            search = Input(14, 11, 320, "");
            search.ForeColor = AppTheme.Text;
            search.Tag = placeholder;
            refresh = Button("Muat Ulang", null, AppTheme.ButtonStyle.Secondary);
            refresh.Size = new Size(110, 32);
            refresh.Location = new Point(346, 10);
            bar.Controls.Add(search);
            bar.Controls.Add(refresh);
            return bar;
        }

        public static void ShowError(IWin32Window owner, Exception ex)
        {
            MessageBox.Show(owner, ex.Message, "Terjadi Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowInfo(IWin32Window owner, string message)
        {
            MessageBox.Show(owner, message, "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static DialogResult Confirm(IWin32Window owner, string message)
        {
            return MessageBox.Show(owner, message, "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public static void FillCombo(ComboBox combo, System.Data.DataTable table, string valueMember, string displayMember, bool withEmpty = true)
        {
            combo.DataSource = null;
            System.Data.DataTable data = table == null ? new System.Data.DataTable() : table.Copy();
            if (withEmpty)
                data.Rows.InsertAt(data.NewRow(), 0);
            combo.DataSource = data;
            combo.DisplayMember = displayMember;
            combo.ValueMember = valueMember;
            combo.SelectedIndex = -1;
        }
    }

    /// <summary>Panel kartu dengan sudut membulat dan garis tepi halus.</summary>
    internal class RoundedPanel : Panel
    {
        private int _radius = 12;
        public int Radius
        {
            get { return _radius; }
            set { _radius = Math.Max(2, value); Invalidate(); }
        }
        public Color BorderColor { get; set; }

        public RoundedPanel()
        {
            DoubleBuffered = true;
            BorderColor = AppTheme.Border;
            BackColor = AppTheme.Surface;
            Padding = new Padding(1);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            using (GraphicsPath path = RoundPath(ClientRectangle, _radius))
                Region = new Region(path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = RoundPath(new Rectangle(0, 0, Width - 1, Height - 1), _radius))
            using (var pen = new Pen(BorderColor))
                e.Graphics.DrawPath(pen, path);
        }

        private static GraphicsPath RoundPath(Rectangle bounds, int radius)
        {
            int d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            var path = new GraphicsPath();
            if (d <= 0) { path.AddRectangle(bounds); return path; }
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
