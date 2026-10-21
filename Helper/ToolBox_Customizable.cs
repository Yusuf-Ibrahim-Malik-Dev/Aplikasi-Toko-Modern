using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Helper
{
    public static class ToolBoxs
    {
        // ==========================================
        // THEME
        // ==========================================

        public static class Theme
        {
            public static Color Primary { get; set; } =
                Color.FromArgb(37, 99, 235);

            public static Color Background { get; set; } =
                Color.FromArgb(248, 250, 252);

            public static Color Surface { get; set; } =
                Color.White;

            public static Color Header { get; set; } =
                Color.FromArgb(30, 41, 59);

            public static Color Text { get; set; } =
                Color.FromArgb(51, 65, 85);

            public static Color Border { get; set; } =
                Color.FromArgb(226, 232, 240);

            public static Color AlternateRow { get; set; } =
                Color.FromArgb(241, 245, 249);
        }

        // ==========================================
        // DATAGRIDVIEW
        // ==========================================

        public static void Grid(
            DataGridView grid,
            int? width = null,
            int? height = null,
            int headerHeight = 42,
            int rowHeight = 36,
            float fontSize = 9.5F,
            float headerFontSize = 10F,
            Color? backgroundColor = null,
            Color? headerColor = null,
            Color? headerTextColor = null,
            Color? textColor = null,
            Color? borderColor = null,
            Color? alternateRowColor = null,
            Color? selectionBackColor = null,
            Color? selectionTextColor = null,
            DataGridViewAutoSizeColumnsMode columnSizing = DataGridViewAutoSizeColumnsMode.Fill)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            grid.SuspendLayout();

            try
            {
                if (width.HasValue) grid.Width = width.Value;
                if (height.HasValue) grid.Height = height.Value;

                grid.BackgroundColor = backgroundColor ?? Theme.Surface;
                grid.BorderStyle = BorderStyle.None;
                grid.GridColor = borderColor ?? Theme.Border;

                grid.EnableHeadersVisualStyles = false;

                grid.ColumnHeadersDefaultCellStyle.BackColor =
                    headerColor ?? Theme.Header;

                grid.ColumnHeadersDefaultCellStyle.ForeColor =
                    headerTextColor ?? Color.White;

                grid.ColumnHeadersDefaultCellStyle.Font =
                    new Font("Segoe UI", headerFontSize, FontStyle.Bold);

                grid.ColumnHeadersHeight = headerHeight;

                grid.DefaultCellStyle.Font =
                    new Font("Segoe UI", fontSize);

                grid.DefaultCellStyle.ForeColor = textColor ?? Theme.Text;

                grid.DefaultCellStyle.SelectionBackColor =
                    selectionBackColor ?? Color.FromArgb(219, 234, 254);

                grid.DefaultCellStyle.SelectionForeColor =
                    selectionTextColor ?? Color.FromArgb(30, 64, 175);

                grid.AlternatingRowsDefaultCellStyle.BackColor =
                    alternateRowColor ?? Theme.AlternateRow;

                grid.AutoSizeColumnsMode = columnSizing;

                grid.RowTemplate.Height = rowHeight;
                grid.RowHeadersVisible = false;
                grid.AllowUserToAddRows = false;
                grid.AllowUserToDeleteRows = false;
                grid.AllowUserToResizeRows = false;
                grid.ReadOnly = true;
                grid.MultiSelect = false;

                grid.SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect;
            }
            finally
            {
                grid.ResumeLayout();
            }
        }

        // ==========================================
        // GRID DATA: STORED PROCEDURE / VIEW / TABLE
        // ==========================================

        public static void GridData(
            DataGridView grid,
            string connectionString,
            string sql,
            CommandType commandType = CommandType.Text,
            params SqlParameter[] parameters)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            using (SqlConnection connection =
                new SqlConnection(connectionString))
            using (SqlCommand command =
                new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter =
                new SqlDataAdapter(command))
            {
                command.CommandType = commandType;
                command.CommandTimeout = 60;

                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                DataTable table = new DataTable();

                adapter.Fill(table);

                grid.DataSource = null;
                grid.DataSource = table;

                Grid(grid);
            }
        }

        // ==========================================
        // TEXTBOX
        // ==========================================

        public static void TextBox(
            TextBox textBox,
            int? width = null,
            int? height = null,
            float fontSize = 10F,
            Color? textColor = null,
            Color? backgroundColor = null,
            BorderStyle borderStyle = BorderStyle.FixedSingle)
        {
            if (textBox == null)
                throw new ArgumentNullException(nameof(textBox));

            if (width.HasValue) textBox.Width = width.Value;
            if (height.HasValue) textBox.Height = height.Value;
            textBox.Font = new Font("Segoe UI", fontSize);
            textBox.ForeColor = textColor ?? Theme.Text;
            textBox.BackColor = backgroundColor ?? Color.White;
            textBox.BorderStyle = borderStyle;
        }

        // ==========================================
        // RICHTEXTBOX
        // ==========================================

        public static void RichTextBox(
            RichTextBox textBox,
            int? width = null,
            int? height = null,
            float fontSize = 10F,
            Color? textColor = null,
            Color? backgroundColor = null,
            BorderStyle borderStyle = BorderStyle.FixedSingle)
        {
            if (textBox == null)
                throw new ArgumentNullException(nameof(textBox));

            if (width.HasValue) textBox.Width = width.Value;
            if (height.HasValue) textBox.Height = height.Value;
            textBox.Font = new Font("Segoe UI", fontSize);
            textBox.ForeColor = textColor ?? Theme.Text;
            textBox.BackColor = backgroundColor ?? Color.White;
            textBox.BorderStyle = borderStyle;
        }

        // ==========================================
        // CHECKBOX
        // ==========================================

        public static void CheckBox(
            CheckBox checkBox,
            float fontSize = 9.5F,
            Color? textColor = null,
            Color? backgroundColor = null,
            bool autoSize = true)
        {
            if (checkBox == null)
                throw new ArgumentNullException(nameof(checkBox));

            checkBox.Font = new Font("Segoe UI", fontSize);
            checkBox.ForeColor = textColor ?? Theme.Text;
            checkBox.BackColor = backgroundColor ?? Color.Transparent;
            checkBox.AutoSize = autoSize;
            checkBox.Cursor = Cursors.Hand;
        }

        // ==========================================
        // RADIOBUTTON
        // ==========================================

        public static void RadioButton(
            RadioButton radioButton,
            float fontSize = 9.5F,
            Color? textColor = null,
            Color? backgroundColor = null,
            bool autoSize = true)
        {
            if (radioButton == null)
                throw new ArgumentNullException(nameof(radioButton));

            radioButton.Font = new Font("Segoe UI", fontSize);
            radioButton.ForeColor = textColor ?? Theme.Text;
            radioButton.BackColor = backgroundColor ?? Color.Transparent;
            radioButton.AutoSize = autoSize;
            radioButton.Cursor = Cursors.Hand;
        }

        // ==========================================
        // COMBOBOX
        // ==========================================

        public static void ComboBox(
            ComboBox comboBox,
            int? width = null,
            int? height = null,
            float fontSize = 10F,
            Color? textColor = null,
            Color? backgroundColor = null,
            ComboBoxStyle dropDownStyle = ComboBoxStyle.DropDownList)
        {
            if (comboBox == null)
                throw new ArgumentNullException(nameof(comboBox));

            if (width.HasValue) comboBox.Width = width.Value;
            if (height.HasValue) comboBox.Height = height.Value;
            comboBox.Font = new Font("Segoe UI", fontSize);
            comboBox.ForeColor = textColor ?? Theme.Text;
            comboBox.BackColor = backgroundColor ?? Color.White;
            comboBox.DropDownStyle = dropDownStyle;
        }

        // ==========================================
        // BUTTON
        // ==========================================

        public static void Button(
            Button button,
            int? width = null,
            int? height = null,
            float fontSize = 9.5F,
            Color? textColor = null,
            Color? backgroundColor = null,
            Color? borderColor = null,
            int borderSize = 0,
            FontStyle fontStyle = FontStyle.Bold)
        {
            if (button == null)
                throw new ArgumentNullException(nameof(button));

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = borderSize;
            button.FlatAppearance.BorderColor = borderColor ?? Theme.Primary;
            if (width.HasValue) button.Width = width.Value;
            if (height.HasValue) button.Height = height.Value;
            button.BackColor = backgroundColor ?? Theme.Primary;
            button.ForeColor = textColor ?? Color.White;
            button.Font = new Font("Segoe UI", fontSize, fontStyle);

            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        // ==========================================
        // LABEL
        // ==========================================

        public static void Label(
            Label label,
            int? width = null,
            int? height = null,
            float fontSize = 9.5F,
            Color? textColor = null,
            Color? backgroundColor = null,
            FontStyle fontStyle = FontStyle.Regular,
            ContentAlignment textAlign = ContentAlignment.MiddleLeft,
            bool autoSize = true)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));

            if (width.HasValue) label.Width = width.Value;
            if (height.HasValue) label.Height = height.Value;
            label.Font = new Font("Segoe UI", fontSize, fontStyle);
            label.ForeColor = textColor ?? Theme.Text;
            label.BackColor = backgroundColor ?? Color.Transparent;
            label.TextAlign = textAlign;
            label.AutoSize = autoSize;
        }

        // ==========================================
        // DATE TIME PICKER
        // ==========================================

        public static void DatePicker(
            DateTimePicker picker,
            int? width = null,
            int? height = null,
            float fontSize = 10F,
            Color? textColor = null,
            Color? backgroundColor = null,
            string customFormat = "dd MMMM yyyy")
        {
            if (picker == null)
                throw new ArgumentNullException(nameof(picker));

            if (width.HasValue) picker.Width = width.Value;
            if (height.HasValue) picker.Height = height.Value;
            picker.Font = new Font("Segoe UI", fontSize);
            if (textColor.HasValue) picker.ForeColor = textColor.Value;
            if (backgroundColor.HasValue) picker.CalendarMonthBackground = backgroundColor.Value;
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = customFormat;
        }

        // ==========================================
        // NUMERIC UP DOWN
        // ==========================================

        public static void Numeric(
            NumericUpDown numeric,
            int? width = null,
            int? height = null,
            float fontSize = 10F,
            Color? textColor = null,
            Color? backgroundColor = null,
            BorderStyle borderStyle = BorderStyle.FixedSingle,
            HorizontalAlignment textAlign = HorizontalAlignment.Right)
        {
            if (numeric == null)
                throw new ArgumentNullException(nameof(numeric));

            if (width.HasValue) numeric.Width = width.Value;
            if (height.HasValue) numeric.Height = height.Value;
            numeric.Font = new Font("Segoe UI", fontSize);
            numeric.ForeColor = textColor ?? Theme.Text;
            numeric.BackColor = backgroundColor ?? Color.White;
            numeric.BorderStyle = borderStyle;
            numeric.TextAlign = textAlign;
        }

        // ==========================================
        // PANEL
        // ==========================================

        public static void Panel(
            Panel panel,
            int? width = null,
            int? height = null,
            Color? backgroundColor = null)
        {
            if (panel == null)
                throw new ArgumentNullException(nameof(panel));

            if (width.HasValue) panel.Width = width.Value;
            if (height.HasValue) panel.Height = height.Value;
            panel.BackColor = backgroundColor ?? Theme.Surface;
        }

        // ==========================================
        // GROUPBOX
        // ==========================================

        public static void GroupBox(
            GroupBox groupBox,
            int? width = null,
            int? height = null,
            float fontSize = 10F,
            Color? textColor = null,
            Color? backgroundColor = null,
            FontStyle fontStyle = FontStyle.Bold)
        {
            if (groupBox == null)
                throw new ArgumentNullException(nameof(groupBox));

            if (width.HasValue) groupBox.Width = width.Value;
            if (height.HasValue) groupBox.Height = height.Value;
            groupBox.Font = new Font("Segoe UI", fontSize, fontStyle);
            groupBox.ForeColor = textColor ?? Theme.Text;
            groupBox.BackColor = backgroundColor ?? Color.Transparent;
        }

        // ==========================================
        // FORM
        // ==========================================

        public static void Form(
            Form form,
            int? width = null,
            int? height = null,
            float fontSize = 9.5F,
            Color? backgroundColor = null,
            FormStartPosition startPosition = FormStartPosition.CenterScreen)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            if (width.HasValue) form.Width = width.Value;
            if (height.HasValue) form.Height = height.Value;
            form.Font = new Font("Segoe UI", fontSize);
            form.BackColor = backgroundColor ?? Theme.Background;
            form.StartPosition = startPosition;
        }
    }
}
