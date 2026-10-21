
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Helper
{
    public static class ToolBox
    {
        // =========================================================
        // THEME
        // Pusat pengaturan warna dan font aplikasi.
        // Ubah nilai di sini untuk mengganti tema secara global.
        // =========================================================
        public static class Theme
        {
            public static Color Primary = Color.FromArgb(37, 99, 235);
            public static Color PrimaryHover = Color.FromArgb(29, 78, 216);
            public static Color Success = Color.FromArgb(22, 163, 74);
            public static Color Danger = Color.FromArgb(220, 38, 38);
            public static Color Warning = Color.FromArgb(217, 119, 6);

            public static Color Background = Color.FromArgb(248, 250, 252);
            public static Color Surface = Color.White;
            public static Color Header = Color.FromArgb(30, 41, 59);
            public static Color Text = Color.FromArgb(51, 65, 85);
            public static Color MutedText = Color.FromArgb(100, 116, 139);
            public static Color Border = Color.FromArgb(226, 232, 240);
            public static Color AlternateRow = Color.FromArgb(241, 245, 249);

            public static string FontName = "Segoe UI";
            public static float FontSize = 9.5F;
            public static float InputFontSize = 10F;
        }

        // =========================================================
        // UTILITAS INTERNAL
        // =========================================================

        // Memastikan kontrol tidak null sebelum diatur.
        private static void Check(object control, string name)
        {
            if (control == null)
                throw new ArgumentNullException(name);
        }

        // Membuat font berdasarkan tema global.
        private static Font CreateFont(
            float size,
            FontStyle style = FontStyle.Regular)
        {
            return new Font(Theme.FontName, size, style);
        }

        // Mengubah ukuran hanya jika nilainya lebih dari nol.
        private static void SetSize(Control control, int width, int height)
        {
            if (width > 0) control.Width = width;
            if (height > 0) control.Height = height;
        }

        // =========================================================
        // FORM
        // =========================================================
        public static void Form(Form form)
        {
            Form(form, Theme.Background, Theme.Text, Theme.FontSize);
        }

        public static void Form(
            Form form,
            Color background,
            Color foreground,
            float fontSize = 9.5F)
        {
            Check(form, nameof(form));

            form.Font = CreateFont(fontSize);
            form.BackColor = background;
            form.ForeColor = foreground;
            form.StartPosition = FormStartPosition.CenterScreen;
        }

        // =========================================================
        // LABEL
        // =========================================================
        public static void Label(Label label)
        {
            Label(label, Theme.Text, Theme.FontSize);
        }

        public static void Label(
            Label label,
            Color foreColor,
            float fontSize = 9.5F,
            bool autoSize = true)
        {
            Check(label, nameof(label));

            label.Font = CreateFont(fontSize);
            label.ForeColor = foreColor;
            label.BackColor = Color.Transparent;
            label.AutoSize = autoSize;
        }

        // =========================================================
        // BUTTON
        // Bisa mengatur warna, font, tinggi, dan lebar.
        // =========================================================
        public static void Button(Button button)
        {
            Button(button, Theme.Primary, Color.White, 0, 0);
        }

        public static void Button(
            Button button,
            Color backColor,
            Color foreColor,
            int width = 0,
            int height = 0,
            float fontSize = 9.5F)
        {
            Check(button, nameof(button));

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Theme.PrimaryHover;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = CreateFont(fontSize, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;

            SetSize(button, width, height);
        }

        // =========================================================
        // TEXTBOX
        // =========================================================
        public static void TextBox(TextBox textBox)
        {
            TextBox(textBox, Theme.Surface, Theme.Text, 0, 0);
        }

        public static void TextBox(
            TextBox textBox,
            Color backColor,
            Color foreColor,
            int width = 0,
            int height = 0,
            float fontSize = 10F)
        {
            Check(textBox, nameof(textBox));

            textBox.Font = CreateFont(fontSize);
            textBox.BackColor = backColor;
            textBox.ForeColor = foreColor;
            textBox.BorderStyle = BorderStyle.FixedSingle;

            SetSize(textBox, width, height);
        }

        // =========================================================
        // RICHTEXTBOX
        // =========================================================
        public static void RichTextBox(RichTextBox textBox)
        {
            Check(textBox, nameof(textBox));

            textBox.Font = CreateFont(Theme.InputFontSize);
            textBox.ForeColor = Theme.Text;
            textBox.BackColor = Theme.Surface;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        // =========================================================
        // MASKEDTEXTBOX
        // =========================================================
        public static void MaskedTextBox(MaskedTextBox textBox)
        {
            Check(textBox, nameof(textBox));

            textBox.Font = CreateFont(Theme.InputFontSize);
            textBox.ForeColor = Theme.Text;
            textBox.BackColor = Theme.Surface;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        // =========================================================
        // CHECKBOX
        // =========================================================
        public static void CheckBox(CheckBox checkBox)
        {
            Check(checkBox, nameof(checkBox));

            checkBox.Font = CreateFont(Theme.FontSize);
            checkBox.ForeColor = Theme.Text;
            checkBox.BackColor = Color.Transparent;
            checkBox.AutoSize = true;
            checkBox.Cursor = Cursors.Hand;
        }

        // =========================================================
        // RADIOBUTTON
        // =========================================================
        public static void RadioButton(RadioButton radioButton)
        {
            Check(radioButton, nameof(radioButton));

            radioButton.Font = CreateFont(Theme.FontSize);
            radioButton.ForeColor = Theme.Text;
            radioButton.BackColor = Color.Transparent;
            radioButton.AutoSize = true;
            radioButton.Cursor = Cursors.Hand;
        }

        // =========================================================
        // COMBOBOX
        // =========================================================
        public static void ComboBox(ComboBox comboBox)
        {
            Check(comboBox, nameof(comboBox));

            comboBox.Font = CreateFont(Theme.InputFontSize);
            comboBox.ForeColor = Theme.Text;
            comboBox.BackColor = Theme.Surface;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public static void ComboBox(
            ComboBox comboBox,
            ComboBoxStyle style,
            Color backColor,
            Color foreColor)
        {
            Check(comboBox, nameof(comboBox));

            comboBox.Font = CreateFont(Theme.InputFontSize);
            comboBox.ForeColor = foreColor;
            comboBox.BackColor = backColor;
            comboBox.DropDownStyle = style;
        }

        // =========================================================
        // LISTBOX DAN CHECKEDLISTBOX
        // =========================================================
        public static void ListBox(ListBox listBox)
        {
            Check(listBox, nameof(listBox));

            listBox.Font = CreateFont(Theme.InputFontSize);
            listBox.ForeColor = Theme.Text;
            listBox.BackColor = Theme.Surface;
            listBox.BorderStyle = BorderStyle.FixedSingle;
        }

        public static void CheckedListBox(CheckedListBox list)
        {
            Check(list, nameof(list));

            list.Font = CreateFont(Theme.InputFontSize);
            list.ForeColor = Theme.Text;
            list.BackColor = Theme.Surface;
            list.BorderStyle = BorderStyle.FixedSingle;
            list.CheckOnClick = true;
        }

        // =========================================================
        // LINKLABEL
        // =========================================================
        public static void LinkLabel(LinkLabel link)
        {
            Check(link, nameof(link));

            link.Font = CreateFont(Theme.FontSize);
            link.LinkColor = Theme.Primary;
            link.ActiveLinkColor = Theme.PrimaryHover;
            link.VisitedLinkColor = Theme.Primary;
            link.BackColor = Color.Transparent;
            link.AutoSize = true;
            link.Cursor = Cursors.Hand;
        }

        // =========================================================
        // DATETIMEPICKER
        // =========================================================
        public static void DatePicker(DateTimePicker picker)
        {
            DatePicker(picker, "dd MMMM yyyy");
        }

        public static void DatePicker(DateTimePicker picker, string format)
        {
            Check(picker, nameof(picker));

            picker.Font = CreateFont(Theme.InputFontSize);
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = format;
        }

        // =========================================================
        // MONTHCALENDAR
        // =========================================================
        public static void MonthCalendar(MonthCalendar calendar)
        {
            Check(calendar, nameof(calendar));

            calendar.BackColor = Theme.Surface;
            calendar.ForeColor = Theme.Text;
            calendar.TitleBackColor = Theme.Header;
            calendar.TitleForeColor = Color.White;
            calendar.TrailingForeColor = Theme.MutedText;
        }

        // =========================================================
        // NUMERICUPDOWN
        // =========================================================
        public static void Numeric(NumericUpDown numeric)
        {
            Check(numeric, nameof(numeric));

            numeric.Font = CreateFont(Theme.InputFontSize);
            numeric.ForeColor = Theme.Text;
            numeric.BackColor = Theme.Surface;
            numeric.BorderStyle = BorderStyle.FixedSingle;
            numeric.TextAlign = HorizontalAlignment.Right;
        }

        // =========================================================
        // TRACKBAR DAN PROGRESSBAR
        // =========================================================
        public static void TrackBar(TrackBar trackBar)
        {
            Check(trackBar, nameof(trackBar));

            trackBar.BackColor = Theme.Surface;
            trackBar.ForeColor = Theme.Primary;
        }

        public static void ProgressBar(ProgressBar progressBar)
        {
            Check(progressBar, nameof(progressBar));

            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
        }

        // =========================================================
        // PICTUREBOX
        // =========================================================
        public static void PictureBox(PictureBox pictureBox)
        {
            Check(pictureBox, nameof(pictureBox));

            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.BackColor = Theme.Surface;
            pictureBox.BorderStyle = BorderStyle.None;
        }

        // =========================================================
        // PANEL DAN GROUPBOX
        // =========================================================
        public static void Panel(Panel panel)
        {
            Panel(panel, Theme.Surface);
        }

        public static void Panel(Panel panel, Color backColor)
        {
            Check(panel, nameof(panel));
            panel.BackColor = backColor;
            panel.BorderStyle = BorderStyle.None;
        }

        public static void GroupBox(GroupBox groupBox)
        {
            Check(groupBox, nameof(groupBox));

            groupBox.Font = CreateFont(11F, FontStyle.Bold);
            groupBox.ForeColor = Theme.Text;
            groupBox.BackColor = Color.Transparent;
        }

        // =========================================================
        // TABCONTROL DAN TABPAGE
        // =========================================================
        public static void TabControl(TabControl tabs)
        {
            Check(tabs, nameof(tabs));

            tabs.Font = CreateFont(Theme.FontSize);
            tabs.BackColor = Theme.Surface;
            tabs.ForeColor = Theme.Text;
        }

        public static void TabPage(TabPage page)
        {
            Check(page, nameof(page));

            page.BackColor = Theme.Surface;
            page.ForeColor = Theme.Text;
            page.Font = CreateFont(Theme.FontSize);
        }

        // =========================================================
        // TREEVIEW
        // =========================================================
        public static void TreeView(TreeView tree)
        {
            Check(tree, nameof(tree));

            tree.Font = CreateFont(Theme.FontSize);
            tree.ForeColor = Theme.Text;
            tree.BackColor = Theme.Surface;
            tree.BorderStyle = BorderStyle.FixedSingle;
            tree.HideSelection = false;
            tree.FullRowSelect = true;
        }

        // =========================================================
        // LISTVIEW
        // =========================================================
        public static void ListView(ListView list)
        {
            Check(list, nameof(list));

            list.Font = CreateFont(Theme.FontSize);
            list.ForeColor = Theme.Text;
            list.BackColor = Theme.Surface;
            list.BorderStyle = BorderStyle.FixedSingle;
            list.FullRowSelect = true;
            list.HideSelection = false;
            list.GridLines = true;
            list.MultiSelect = false;
        }

        // =========================================================
        // SPLITCONTAINER
        // =========================================================
        public static void SplitContainer(SplitContainer split)
        {
            Check(split, nameof(split));

            split.BackColor = Theme.Border;
            split.Panel1.BackColor = Theme.Surface;
            split.Panel2.BackColor = Theme.Surface;
        }

        // =========================================================
        // FLOWLAYOUTPANEL
        // =========================================================
        public static void FlowLayout(FlowLayoutPanel flow)
        {
            Check(flow, nameof(flow));

            flow.BackColor = Theme.Background;
            flow.WrapContents = true;
            flow.AutoScroll = true;
        }

        // =========================================================
        // TABLELAYOUTPANEL
        // =========================================================
        public static void TableLayout(TableLayoutPanel table)
        {
            Check(table, nameof(table));

            table.BackColor = Theme.Background;
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
        }


        // =========================================================
        // DATAGRIDVIEW
        // =========================================================
        public static void Grid(DataGridView grid)
        {
            Grid(grid, Theme.Header, Theme.Surface, Theme.Text, Theme.Border);
        }

        public static void Grid(
            DataGridView grid,
            Color headerColor,
            Color rowColor,
            Color textColor,
            Color borderColor)
        {
            Check(grid, nameof(grid));

            grid.SuspendLayout();

            try
            {
                grid.BackgroundColor = rowColor;
                grid.BorderStyle = BorderStyle.None;
                grid.GridColor = borderColor;
                grid.EnableHeadersVisualStyles = false;

                grid.ColumnHeadersDefaultCellStyle.BackColor = headerColor;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.ColumnHeadersDefaultCellStyle.Font =
                    CreateFont(10F, FontStyle.Bold);
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerColor;
                grid.ColumnHeadersHeight = 42;

                grid.DefaultCellStyle.Font = CreateFont(Theme.FontSize);
                grid.DefaultCellStyle.ForeColor = textColor;
                grid.DefaultCellStyle.BackColor = rowColor;
                grid.DefaultCellStyle.SelectionBackColor =
                    Color.FromArgb(219, 234, 254);
                grid.DefaultCellStyle.SelectionForeColor =
                    Color.FromArgb(30, 64, 175);

                grid.AlternatingRowsDefaultCellStyle.BackColor =
                    Theme.AlternateRow;

                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                grid.RowTemplate.Height = 36;
                grid.RowHeadersVisible = false;
                grid.AllowUserToAddRows = false;
                grid.AllowUserToDeleteRows = false;
                grid.AllowUserToResizeRows = false;
                grid.ReadOnly = true;
                grid.MultiSelect = false;
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
            finally
            {
                grid.ResumeLayout();
            }
        }

        // Memuat data SQL Server ke DataGridView.
        public static void GridData(
            DataGridView grid,
            string connectionString,
            string sql,
            CommandType commandType = CommandType.Text,
            params SqlParameter[] parameters)
        {
            Check(grid, nameof(grid));

            using (SqlConnection connection =
                new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.CommandType = commandType;
                command.CommandTimeout = 60;

                if (parameters != null)
                {
                    foreach (SqlParameter parameter in parameters)
                    {
                        if (parameter != null)
                            command.Parameters.Add(parameter);
                    }
                }

                DataTable table = new DataTable();
                adapter.Fill(table);

                grid.DataSource = null;
                grid.DataSource = table;

                Grid(grid);
            }
        }

        // =========================================================
        // MENUS & TOOLBARS
        // =========================================================
        public static void MenuStrip(MenuStrip menu)
        {
            Check(menu, nameof(menu));
            menu.BackColor = Theme.Header;
            menu.ForeColor = Color.White;
            menu.Font = CreateFont(Theme.FontSize);
        }

        public static void ContextMenu(ContextMenuStrip menu)
        {
            Check(menu, nameof(menu));
            menu.BackColor = Theme.Surface;
            menu.ForeColor = Theme.Text;
            menu.Font = CreateFont(Theme.FontSize);
        }

        public static void ToolStrip(ToolStrip strip)
        {
            Check(strip, nameof(strip));
            strip.BackColor = Theme.Surface;
            strip.ForeColor = Theme.Text;
            strip.Font = CreateFont(Theme.FontSize);
            strip.GripStyle = ToolStripGripStyle.Hidden;
        }

        public static void StatusStrip(StatusStrip strip)
        {
            Check(strip, nameof(strip));
            strip.BackColor = Theme.Header;
            strip.ForeColor = Color.White;
            strip.Font = CreateFont(Theme.FontSize);
            strip.SizingGrip = false;
        }

        public static void BindingNavigator(BindingNavigator navigator)
        {
            Check(navigator, nameof(navigator));
            navigator.BackColor = Theme.Surface;
            navigator.ForeColor = Theme.Text;
            navigator.Font = CreateFont(Theme.FontSize);
            navigator.GripStyle = ToolStripGripStyle.Hidden;
        }

        // =========================================================
        // SCROLLBARS
        // =========================================================
        public static void HScrollBar(HScrollBar bar)
        {
            Check(bar, nameof(bar));
            bar.BackColor = Theme.Background;
        }

        public static void VScrollBar(VScrollBar bar)
        {
            Check(bar, nameof(bar));
            bar.BackColor = Theme.Background;
        }

        // =========================================================
        // COMPONENTS
        // =========================================================
        public static void Timer(Timer timer, int interval = 1000)
        {
            Check(timer, nameof(timer));

            if (interval < 1)
                throw new ArgumentOutOfRangeException(nameof(interval));

            timer.Interval = interval;
        }

        public static void ImageList(
            ImageList images,
            int width = 24,
            int height = 24)
        {
            Check(images, nameof(images));

            images.ImageSize = new Size(
                Math.Max(1, width),
                Math.Max(1, height));

            images.ColorDepth = ColorDepth.Depth32Bit;
        }

        public static void ToolTip(
            ToolTip toolTip,
            int initialDelay = 500,
            int reshowDelay = 100,
            int autoPopDelay = 5000)
        {
            Check(toolTip, nameof(toolTip));

            toolTip.InitialDelay = initialDelay;
            toolTip.ReshowDelay = reshowDelay;
            toolTip.AutoPopDelay = autoPopDelay;
            toolTip.ShowAlways = true;
        }

        public static void NotifyIcon(
            NotifyIcon icon,
            Icon applicationIcon,
            string text)
        {
            Check(icon, nameof(icon));

            if (applicationIcon != null)
                icon.Icon = applicationIcon;

            if (!string.IsNullOrWhiteSpace(text))
                icon.Text = text.Length > 63 ? text.Substring(0, 63) : text;
        }

        public static void ErrorProvider(
            ErrorProvider provider,
            ContainerControl container)
        {
            Check(provider, nameof(provider));

            if (container != null)
                provider.ContainerControl = container;

            provider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        }

        public static void HelpProvider(HelpProvider provider)
        {
            Check(provider, nameof(provider));
        }

        public static void BackgroundWorker(
            BackgroundWorker worker,
            bool reportProgress = true,
            bool allowCancellation = true)
        {
            Check(worker, nameof(worker));

            worker.WorkerReportsProgress = reportProgress;
            worker.WorkerSupportsCancellation = allowCancellation;
        }

        // =========================================================
        // DATA COMPONENTS
        // =========================================================
        public static void BindingSource(
            BindingSource source,
            object dataSource,
            string dataMember = null)
        {
            Check(source, nameof(source));

            source.DataSource = dataSource;

            if (!string.IsNullOrWhiteSpace(dataMember))
                source.DataMember = dataMember;
        }

        public static DataTable CreateDataTable(string tableName = null)
        {
            DataTable table = new DataTable();

            if (!string.IsNullOrWhiteSpace(tableName))
                table.TableName = tableName;

            return table;
        }

        public static DataSet CreateDataSet(string dataSetName = null)
        {
            DataSet dataSet = new DataSet();

            if (!string.IsNullOrWhiteSpace(dataSetName))
                dataSet.DataSetName = dataSetName;

            return dataSet;
        }

        public static DataView CreateDataView(
            DataTable table,
            string rowFilter = null,
            string sort = null)
        {
            Check(table, nameof(table));

            DataView view = new DataView(table);

            if (!string.IsNullOrWhiteSpace(rowFilter))
                view.RowFilter = rowFilter;

            if (!string.IsNullOrWhiteSpace(sort))
                view.Sort = sort;

            return view;
        }

        // =========================================================
        // DIALOGS
        // =========================================================
        public static DialogResult OpenFile(
            OpenFileDialog dialog,
            IWin32Window owner = null)
        {
            Check(dialog, nameof(dialog));

            return owner == null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
        }

        public static DialogResult SaveFile(
            SaveFileDialog dialog,
            IWin32Window owner = null)
        {
            Check(dialog, nameof(dialog));

            return owner == null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
        }

        public static DialogResult SelectFolder(
            FolderBrowserDialog dialog,
            IWin32Window owner = null)
        {
            Check(dialog, nameof(dialog));

            return owner == null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
        }

        public static DialogResult SelectColor(
            ColorDialog dialog,
            IWin32Window owner = null)
        {
            Check(dialog, nameof(dialog));

            return owner == null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
        }

        public static DialogResult SelectFont(
            FontDialog dialog,
            IWin32Window owner = null)
        {
            Check(dialog, nameof(dialog));

            return owner == null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
        }

        public static DialogResult Print(
            PrintDialog dialog,
            IWin32Window owner = null)
        {
            Check(dialog, nameof(dialog));

            return owner == null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
        }

        public static DialogResult PreviewPrint(
            PrintPreviewDialog dialog,
            IWin32Window owner = null)
        {
            Check(dialog, nameof(dialog));

            dialog.StartPosition = FormStartPosition.CenterParent;

            return owner == null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
        }

        public static DialogResult SetupPage(
            PageSetupDialog dialog,
            IWin32Window owner = null)
        {
            Check(dialog, nameof(dialog));

            return owner == null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
        }

        // =========================================================
        // PRINTDOCUMENT
        // =========================================================
        public static void PrintDocument(
            PrintDocument document,
            string documentName = null)
        {
            Check(document, nameof(document));

            if (!string.IsNullOrWhiteSpace(documentName))
                document.DocumentName = documentName;

            document.OriginAtMargins = true;
        }

        // =========================================================
        // GENERIC CONTROL
        // Untuk UserControl atau kontrol tambahan.
        // =========================================================
        public static void Control(
            Control control,
            Color backColor,
            Color foreColor,
            float fontSize = 9.5F,
            int width = 0,
            int height = 0)
        {
            Check(control, nameof(control));

            control.BackColor = backColor;
            control.ForeColor = foreColor;
            control.Font = CreateFont(fontSize);

            SetSize(control, width, height);
        }

    }
}
