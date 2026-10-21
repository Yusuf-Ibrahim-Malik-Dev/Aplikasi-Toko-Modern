using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System;

namespace Helper
{
    public static class StringTextHelper
    {
        // =========================================
        // LOWERCASE & UPPERCASE
        // =========================================

        // Semua huruf menjadi kecil
        public static string ToLower(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.ToLowerInvariant();
        }

        // Semua huruf menjadi besar
        public static string ToUpper(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.ToUpperInvariant();
        }

        // Huruf pertama setiap kata menjadi besar
        public static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            TextInfo textInfo = CultureInfo
                .GetCultureInfo("id-ID").TextInfo;

            return textInfo.ToTitleCase(
                value.Trim().ToLowerInvariant()
            );
        }

        // Hanya huruf pertama string menjadi besar
        public static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            value = value.Trim();

            if (value.Length == 0)
                return string.Empty;

            return char.ToUpperInvariant(value[0])
                + value.Substring(1);
        }

        // =========================================
        // TRIM & SPASI
        // =========================================

        // Hapus spasi di awal dan akhir
        public static string Trim(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        // Hapus spasi di awal saja
        public static string TrimStart(string value)
        {
            return value == null ? string.Empty : value.TrimStart();
        }

        // Hapus spasi di akhir saja
        public static string TrimEnd(string value)
        {
            return value == null ? string.Empty : value.TrimEnd();
        }

        // Gabungkan spasi berulang menjadi satu
        public static string NormalizeSpaces(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return Regex.Replace(value.Trim(), @"\s+", " ");
        }

        // Hapus seluruh spasi
        public static string RemoveSpaces(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : Regex.Replace(value, @"\s+", "");
        }

        // =========================================
        // VALIDASI STRING
        // =========================================

        // Cek null, kosong, atau hanya spasi
        public static bool IsEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        // Cek string berisi teks
        public static bool HasValue(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        // Mengembalikan default jika string kosong
        public static string DefaultIfEmpty(
            string value,
            string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value)
                ? defaultValue
                : value;
        }

        // =========================================
        // PENCARIAN & PERBANDINGAN
        // =========================================

        // Membandingkan dua string tanpa membedakan kapital
        public static bool EqualsIgnoreCase(
            string first,
            string second)
        {
            return string.Equals(
                first,
                second,
                StringComparison.OrdinalIgnoreCase
            );
        }

        // Cek apakah teks mengandung kata tertentu
        public static bool ContainsIgnoreCase(
            string value,
            string search)
        {
            if (value == null || search == null)
                return false;

            return value.IndexOf(
                search,
                StringComparison.OrdinalIgnoreCase
            ) >= 0;
        }

        // =========================================
        // POTONG & BATAS KARAKTER
        // =========================================

        // Batasi panjang string
        public static string Truncate(
            string value,
            int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (maxLength <= 0)
                return string.Empty;

            return value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength);
        }

        // Ambil sejumlah karakter dari depan
        public static string Left(string value, int length)
        {
            if (string.IsNullOrEmpty(value) || length <= 0)
                return string.Empty;

            return value.Substring(
                0,
                Math.Min(length, value.Length)
            );
        }

        // Ambil sejumlah karakter dari belakang
        public static string Right(string value, int length)
        {
            if (string.IsNullOrEmpty(value) || length <= 0)
                return string.Empty;

            length = Math.Min(length, value.Length);

            return value.Substring(value.Length - length);
        }

        // =========================================
        // FORMAT TANGGAL
        // =========================================

        // Format DateTime menjadi string
        public static string FormatDate(
            DateTime date,
            string format = "yyyy-MM-dd")
        {
            return date.ToString(
                format,
                CultureInfo.InvariantCulture
            );
        }

        // Konversi string ke DateTime
        public static bool TryParseDate(
            string value,
            out DateTime date)
        {
            return DateTime.TryParse(
                value,
                CultureInfo.GetCultureInfo("id-ID"),
                DateTimeStyles.None,
                out date
            );
        }

        // Tanggal sekarang + jumlah tahun
        public static string ExpirationDate(int years)
        {
            return DateTime.Now
                .AddYears(years)
                .ToString("yyyy-MM-dd");
        }

        // =========================================
        // FORMAT ANGKA
        // =========================================

        // Konversi angka menjadi string format Indonesia
        public static string FormatNumber(decimal value)
        {
            return value.ToString(
                "N0",
                CultureInfo.GetCultureInfo("id-ID")
            );
        }

        // Konversi string menjadi integer dengan aman
        public static int ToInt(
            string value,
            int defaultValue = 0)
        {
            int result;

            return int.TryParse(value, out result)
                ? result
                : defaultValue;
        }

        // Konversi string menjadi decimal dengan aman
        public static decimal ToDecimal(
            string value,
            decimal defaultValue = 0)
        {
            decimal result;

            return decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.GetCultureInfo("id-ID"),
                out result
            )
                ? result
                : defaultValue;
        }

        // =========================================
        // PEMBERSIHAN KARAKTER
        // =========================================

        // Hanya izinkan huruf dan angka
        public static string AlphaNumeric(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return Regex.Replace(value, @"[^a-zA-Z0-9]", "");
        }

        // Hanya izinkan angka
        public static string DigitsOnly(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return Regex.Replace(value, @"[^0-9]", "");
        }

        // Hapus karakter kontrol
        public static string RemoveControlCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return Regex.Replace(value, @"[\x00-\x1F\x7F]", "");
        }

        // =========================================
        // MASKING DATA
        // =========================================

        // Contoh: 081234567890 menjadi 0812******90
        public static string Mask(
            string value,
            int visibleStart = 4,
            int visibleEnd = 2,
            char maskChar = '*')
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (visibleStart < 0 || visibleEnd < 0)
                return value;

            if (visibleStart + visibleEnd >= value.Length)
                return value;

            int maskLength =
                value.Length - visibleStart - visibleEnd;

            return value.Substring(0, visibleStart)
                + new string(maskChar, maskLength)
                + value.Substring(value.Length - visibleEnd);
        }


        // =========================================
        // VALIDASI SIMBOL, EMOJI, DAN KARAKTER
        // =========================================

        // Cek apakah string mengandung simbol
        public static bool HasSymbol(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (char.IsSymbol(c) || char.IsPunctuation(c))
                    return true;
            }

            return false;
        }

        // Cek apakah string mengandung emoji
        // Mendeteksi banyak emoji umum melalui surrogate pair,
        // variation selector, dan karakter emoji tertentu.
        public static bool HasEmoji(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                int code = char.ConvertToUtf32(value, i);

                if (char.IsHighSurrogate(value[i]))
                    i++;

                if (
                    (code >= 0x1F000 && code <= 0x1FAFF) ||
                    (code >= 0x2600 && code <= 0x27BF) ||
                    (code >= 0x1F1E6 && code <= 0x1F1FF) ||
                    code == 0xFE0F ||
                    code == 0x200D
                )
                {
                    return true;
                }
            }

            return false;
        }

        // Hanya boleh huruf dan spasi
        public static bool IsLettersAndSpacesOnly(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            foreach (char c in value)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                    return false;
            }

            return true;
        }

        // Hanya boleh huruf dan angka
        public static bool IsAlphaNumericOnly(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            foreach (char c in value)
            {
                if (!char.IsLetterOrDigit(c))
                    return false;
            }

            return true;
        }

        // Hanya boleh angka
        public static bool IsDigitsOnly(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        // Cek apakah string mengandung karakter selain huruf dan angka
        public static bool HasSpecialCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (!char.IsLetterOrDigit(c) &&
                    !char.IsWhiteSpace(c))
                {
                    return true;
                }
            }

            return false;
        }

        // Hapus karakter selain huruf, angka, dan spasi
        public static string RemoveSpecialCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            StringBuilder result = new StringBuilder();

            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c) ||
                    char.IsWhiteSpace(c))
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        // Hanya boleh karakter yang ditentukan
        public static bool ContainsOnlyAllowedCharacters(
            string value,
            string allowedSymbols)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c) ||
                    char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (allowedSymbols != null &&
                    allowedSymbols.IndexOf(c) >= 0)
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
