
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Helper
{
    public static class PasswordHelper
    {
        // PASSWORD HASH
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 210000;

        // AES ENCRYPTION
        private const int AesKeySize = 32; // 256 bit
        private const int AesIvSize = 16;  // 128 bit

        /*
         * HASH PASSWORD
         * Output: 96 karakter hexadecimal.
         * 32 karakter salt + 64 karakter hash.
         */
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException(
                    "Password tidak boleh kosong.", "password");

            byte[] salt = new byte[SaltSize];
            byte[] hash = null;

            try
            {
                using (RandomNumberGenerator rng =
                    RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                hash = DeriveHash(password, salt, Iterations);

                return ToHex(salt) + ToHex(hash);
            }
            finally
            {
                Array.Clear(salt, 0, salt.Length);

                if (hash != null)
                    Array.Clear(hash, 0, hash.Length);
            }
        }

        /*
         * VERIFY PASSWORD
         * Dipakai saat login.
         */
        public static bool VerifyPassword(
            string password,
            string storedHash)
        {
            if (string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(storedHash))
                return false;

            byte[] salt = null;
            byte[] expectedHash = null;
            byte[] actualHash = null;

            try
            {
                if (storedHash.Length != 96)
                    return false;

                salt = FromHex(storedHash.Substring(0, 32));
                expectedHash = FromHex(storedHash.Substring(32, 64));

                if (salt.Length != SaltSize ||
                    expectedHash.Length != HashSize)
                    return false;

                actualHash = DeriveHash(password, salt, Iterations);

                int difference = 0;

                for (int i = 0; i < HashSize; i++)
                    difference |= expectedHash[i] ^ actualHash[i];

                return difference == 0;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            finally
            {
                if (salt != null)
                    Array.Clear(salt, 0, salt.Length);

                if (expectedHash != null)
                    Array.Clear(expectedHash, 0, expectedHash.Length);

                if (actualHash != null)
                    Array.Clear(actualHash, 0, actualHash.Length);
            }
        }

        /*
         * AES-256 ENCRYPT
         * plaintext bisa berupa teks biasa.
         * encryptionKey harus berupa kunci rahasia 32 byte.
         *
         * Output: Base64 dari IV + ciphertext.
         */
        public static string Encrypt(
            string plainText,
            string encryptionKey)
        {
            if (plainText == null)
                throw new ArgumentNullException("plainText");

            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentException(
                    "Kunci enkripsi tidak boleh kosong.",
                    "encryptionKey");

            byte[] key = DeriveAesKey(encryptionKey);
            byte[] iv = new byte[AesIvSize];
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = null;

            try
            {
                using (RandomNumberGenerator rng =
                    RandomNumberGenerator.Create())
                {
                    rng.GetBytes(iv);
                }

                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = iv;

                    using (ICryptoTransform encryptor =
                        aes.CreateEncryptor())
                    {
                        cipherBytes = encryptor.TransformFinalBlock(
                            plainBytes, 0, plainBytes.Length);
                    }
                }

                byte[] output = new byte[iv.Length + cipherBytes.Length];

                Buffer.BlockCopy(iv, 0, output, 0, iv.Length);
                Buffer.BlockCopy(
                    cipherBytes, 0, output, iv.Length, cipherBytes.Length);

                return Convert.ToBase64String(output);
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
                Array.Clear(iv, 0, iv.Length);
                Array.Clear(plainBytes, 0, plainBytes.Length);

                if (cipherBytes != null)
                    Array.Clear(cipherBytes, 0, cipherBytes.Length);
            }
        }

        /*
         * AES-256 DECRYPT
         * Gunakan kunci yang sama dengan Encrypt().
         */
        public static string Decrypt(
            string cipherText,
            string encryptionKey)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentException(
                    "Ciphertext tidak boleh kosong.", "cipherText");

            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentException(
                    "Kunci dekripsi tidak boleh kosong.",
                    "encryptionKey");

            byte[] input = Convert.FromBase64String(cipherText);
            byte[] key = DeriveAesKey(encryptionKey);
            byte[] iv = null;
            byte[] encryptedBytes = null;
            byte[] plainBytes = null;

            try
            {
                if (input.Length <= AesIvSize)
                    throw new CryptographicException(
                        "Data enkripsi tidak valid.");

                iv = new byte[AesIvSize];
                Buffer.BlockCopy(input, 0, iv, 0, AesIvSize);

                int cipherLength = input.Length - AesIvSize;
                encryptedBytes = new byte[cipherLength];

                Buffer.BlockCopy(
                    input, AesIvSize, encryptedBytes, 0, cipherLength);

                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = key;
                    aes.IV = iv;

                    using (ICryptoTransform decryptor =
                        aes.CreateDecryptor())
                    {
                        plainBytes = decryptor.TransformFinalBlock(
                            encryptedBytes, 0, encryptedBytes.Length);
                    }
                }

                return Encoding.UTF8.GetString(plainBytes);
            }
            finally
            {
                Array.Clear(input, 0, input.Length);
                Array.Clear(key, 0, key.Length);

                if (iv != null)
                    Array.Clear(iv, 0, iv.Length);

                if (encryptedBytes != null)
                    Array.Clear(encryptedBytes, 0, encryptedBytes.Length);

                if (plainBytes != null)
                    Array.Clear(plainBytes, 0, plainBytes.Length);
            }
        }

        /*
         * Membentuk hash PBKDF2-HMAC-SHA256 secara manual,
         * agar kompatibel dengan .NET Framework lama.
         */
        private static byte[] DeriveHash(
            string password,
            byte[] salt,
            int iterations)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] blockInput = new byte[salt.Length + 4];
            byte[] u = null;
            byte[] result = new byte[HashSize];
            byte[] t = null;

            try
            {
                Buffer.BlockCopy(salt, 0, blockInput, 0, salt.Length);
                blockInput[blockInput.Length - 1] = 1;

                using (HMACSHA256 hmac = new HMACSHA256(passwordBytes))
                {
                    u = hmac.ComputeHash(blockInput);
                    t = new byte[HashSize];
                    Buffer.BlockCopy(u, 0, t, 0, HashSize);

                    for (int i = 1; i < iterations; i++)
                    {
                        byte[] nextU = hmac.ComputeHash(u);

                        Array.Clear(u, 0, u.Length);
                        u = nextU;

                        for (int j = 0; j < HashSize; j++)
                            t[j] ^= u[j];
                    }

                    Buffer.BlockCopy(t, 0, result, 0, HashSize);
                }

                return result;
            }
            catch
            {
                Array.Clear(result, 0, result.Length);
                throw;
            }
            finally
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
                Array.Clear(blockInput, 0, blockInput.Length);

                if (u != null)
                    Array.Clear(u, 0, u.Length);

                if (t != null)
                    Array.Clear(t, 0, t.Length);
            }
        }

        /*
         * Membuat kunci AES 256-bit dari secret aplikasi.
         * Kunci ini harus disimpan terpisah dari ciphertext.
         */
        private static byte[] DeriveAesKey(string encryptionKey)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(
                    Encoding.UTF8.GetBytes(encryptionKey));
            }
        }

        private static string ToHex(byte[] bytes)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString("X2"));

            return result.ToString();
        }

        private static byte[] FromHex(string hex)
        {
            if (hex == null || hex.Length % 2 != 0)
                throw new FormatException(
                    "Format hexadecimal tidak valid.");

            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(
                    hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
    }
}
