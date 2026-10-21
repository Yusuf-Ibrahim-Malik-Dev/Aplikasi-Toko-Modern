
using System;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DAAccess
{
    public class dbConfiguration
    {
        // =========================================
        // KONFIGURASI KONEKSI DATABASE
        // =========================================
        private static readonly string _connString =
            @"Data Source=Mr-Yusuf-I_M-PC;Initial Catalog=dbToko;User ID=sa;Password=system1994;Connect Timeout=10";

        // =========================================
        // FILE CONNECTION STRING TERENKRIPSI
        // =========================================
        private static readonly string _filePath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Toko_Root_ID",
                "dbconnection.dat");

        private static readonly object _lockObject = new object();
        private static string _decryptedConnectionString;

        // =========================================
        // CONNECTION STRING
        // =========================================
        public static string ConnectionString
        {
            get
            {
                return GetConnectionString();
            }
        }

        // =========================================
        // MEMBUAT KONEKSI SQL SERVER
        // =========================================
        public static SqlConnection createConection()
        {
            return new SqlConnection(GetConnectionString());
        }

        // =========================================
        // ENKRIPSI CONNECTION STRING
        // =========================================
        public static void EncryptConnectionString()
        {
            lock (_lockObject)
            {
                byte[] plainBytes = null;
                byte[] encryptedBytes = null;

                try
                {
                    plainBytes = Encoding.UTF8.GetBytes(_connString);

                    encryptedBytes = ProtectedData.Protect(
                        plainBytes,
                        null,
                        DataProtectionScope.CurrentUser);

                    string directory = Path.GetDirectoryName(_filePath);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    File.WriteAllText(
                        _filePath,
                        Convert.ToBase64String(encryptedBytes));

                    _decryptedConnectionString = _connString;
                }
                finally
                {
                    if (plainBytes != null)
                        Array.Clear(plainBytes, 0, plainBytes.Length);

                    if (encryptedBytes != null)
                        Array.Clear(encryptedBytes, 0, encryptedBytes.Length);
                }
            }
        }

        // =========================================
        // DEKRIPSI CONNECTION STRING
        // =========================================
        private static string GetConnectionString()
        {
            if (!string.IsNullOrEmpty(_decryptedConnectionString))
                return _decryptedConnectionString;

            lock (_lockObject)
            {
                if (!string.IsNullOrEmpty(_decryptedConnectionString))
                    return _decryptedConnectionString;

                // Jika file terenkripsi belum ada,
                // buat otomatis dari _connString.
                if (!File.Exists(_filePath))
                {
                    EncryptConnectionString();
                    return _decryptedConnectionString;
                }

                byte[] encryptedBytes = null;
                byte[] plainBytes = null;

                try
                {
                    string encryptedText =
                        File.ReadAllText(_filePath).Trim();

                    encryptedBytes =
                        Convert.FromBase64String(encryptedText);

                    plainBytes = ProtectedData.Unprotect(
                        encryptedBytes,
                        null,
                        DataProtectionScope.CurrentUser);

                    _decryptedConnectionString =
                        Encoding.UTF8.GetString(plainBytes);

                    return _decryptedConnectionString;
                }
                catch (CryptographicException ex)
                {
                    throw new InvalidOperationException(
                        "Gagal mendekripsi koneksi database. " +
                        "Pastikan aplikasi berjalan menggunakan akun " +
                        "Windows yang sama saat enkripsi dilakukan.",
                        ex);
                }
                finally
                {
                    if (plainBytes != null)
                        Array.Clear(plainBytes, 0, plainBytes.Length);

                    if (encryptedBytes != null)
                        Array.Clear(encryptedBytes, 0, encryptedBytes.Length);
                }
            }
        }
    }
}
