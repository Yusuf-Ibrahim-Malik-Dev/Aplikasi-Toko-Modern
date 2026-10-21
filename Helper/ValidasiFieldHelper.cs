using System;
using System.Windows.Forms;

namespace Helper
{
    public static class ValidasiFieldHelper
    {
        // =========================================
        // 1. VALIDASI STRING KOSONG
        // =========================================
        public static bool ValidateRequired(
            string value,
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show(
                    fieldName + " tidak boleh kosong",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return false;
            }

            return true;
        }

        // =========================================
        // 2. VALIDASI TEXTBOX
        // =========================================
        public static bool ValidateTextBox(
            TextBox textBox,
            string fieldName)
        {
            if (textBox == null)
            {
                MessageBox.Show(
                    "Field " + fieldName + " tidak ditemukan.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return false;
            }

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show(
                    fieldName + " tidak boleh kosong",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox.Focus();
                return false;
            }

            return true;
        }

        // =========================================
        // 3. VALIDASI COMBOBOX
        // =========================================
        public static bool ValidateComboBox(
            ComboBox comboBox,
            string fieldName)
        {
            if (comboBox == null)
            {
                MessageBox.Show(
                    "Field " + fieldName + " tidak ditemukan.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return false;
            }

            if (comboBox.SelectedIndex < 0 ||
                string.IsNullOrWhiteSpace(comboBox.Text))
            {
                MessageBox.Show(
                    fieldName + " tidak boleh kosong",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                comboBox.Focus();
                return false;
            }

            return true;
        }

        // =========================================
        // 4. VALIDASI PANJANG MINIMUM
        // =========================================
        public static bool ValidateMinLength(
            TextBox textBox,
            string fieldName,
            int minimum)
        {
            if (!ValidateTextBox(textBox, fieldName))
                return false;

            if (textBox.Text.Trim().Length < minimum)
            {
                MessageBox.Show(
                    fieldName + " minimal " +
                    minimum + " karakter.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox.Focus();
                return false;
            }

            return true;
        }

        // =========================================
        // 5. VALIDASI PANJANG MAKSIMUM
        // =========================================
        public static bool ValidateMaxLength(
            TextBox textBox,
            string fieldName,
            int maximum)
        {
            if (!ValidateTextBox(textBox, fieldName))
                return false;

            if (textBox.Text.Length > maximum)
            {
                MessageBox.Show(
                    fieldName + " maksimal " +
                    maximum + " karakter.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox.Focus();
                return false;
            }

            return true;
        }

        // =========================================
        // 6. VALIDASI ANGKA SAJA
        // =========================================
        public static bool ValidateDigitsOnly(
            TextBox textBox,
            string fieldName)
        {
            if (!ValidateTextBox(textBox, fieldName))
                return false;

            foreach (char c in textBox.Text)
            {
                if (c < '0' || c > '9')
                {
                    MessageBox.Show(
                        fieldName + " hanya boleh berisi angka.",
                        "Validasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    textBox.Focus();
                    return false;
                }
            }

            return true;
        }

        // =========================================
        // 7. VALIDASI HURUF SAJA
        // =========================================
        public static bool ValidateLettersOnly(
            TextBox textBox,
            string fieldName)
        {
            if (!ValidateTextBox(textBox, fieldName))
                return false;

            foreach (char c in textBox.Text)
            {
                if (!char.IsLetter(c) &&
                    !char.IsWhiteSpace(c))
                {
                    MessageBox.Show(
                        fieldName +
                        " hanya boleh berisi huruf dan spasi.",
                        "Validasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    textBox.Focus();
                    return false;
                }
            }

            return true;
        }

        // =========================================
        // 8. VALIDASI HURUF DAN ANGKA
        // =========================================
        public static bool ValidateAlphaNumeric(
            TextBox textBox,
            string fieldName)
        {
            if (!ValidateTextBox(textBox, fieldName))
                return false;

            foreach (char c in textBox.Text)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    MessageBox.Show(
                        fieldName +
                        " hanya boleh berisi huruf dan angka.",
                        "Validasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    textBox.Focus();
                    return false;
                }
            }

            return true;
        }

        // =========================================
        // 9. VALIDASI EMAIL
        // =========================================
        public static bool ValidateEmail(
            TextBox textBox,
            string fieldName)
        {
            if (!ValidateTextBox(textBox, fieldName))
                return false;

            try
            {
                System.Net.Mail.MailAddress email =
                    new System.Net.Mail.MailAddress(
                        textBox.Text.Trim()
                    );

                if (email.Address != textBox.Text.Trim())
                    throw new FormatException();

                return true;
            }
            catch
            {
                MessageBox.Show(
                    fieldName + " tidak valid.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textBox.Focus();
                return false;
            }
        }

        // =========================================
        // 10. VALIDASI CHECKBOX
        // =========================================
        public static bool ValidateCheckBox(
            CheckBox checkBox,
            string fieldName)
        {
            if (checkBox == null || !checkBox.Checked)
            {
                MessageBox.Show(
                    fieldName + " harus dicentang.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                if (checkBox != null)
                    checkBox.Focus();

                return false;
            }

            return true;
        }
    }
}
