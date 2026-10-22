using System;
using System.Data;
using CommonClass;

namespace DAAccess
{
    /// <summary>
    /// Data access untuk master data toko: barang, kategori, merek, satuan,
    /// supplier, pelanggan, profil toko, dan daftar role.
    /// Semua query memakai parameter (@0, @1, ...) agar aman dari SQL injection.
    /// </summary>
    public class DAMaster
    {
        // =========================================================
        // BARANG
        // =========================================================
        public DataTable GetBarang(string keyword = null, bool hanyaMenipis = false)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT b.kodebarang, b.namabarang, b.kategori_id, k.nama AS kategori,
       b.merek_id, m.nama AS merek, b.satuan_id, s.nama AS satuan,
       b.barcode, ISNULL(b.harga_beli,0) AS harga_beli, b.hargasatuan,
       CAST(ISNULL(b.stok,0) AS DECIMAL(18,3)) AS stok,
       CAST(ISNULL(b.stok_min,0) AS DECIMAL(18,3)) AS stok_min,
       CASE WHEN ISNULL(b.stok,0) <= 0 THEN N'HABIS'
            WHEN ISNULL(b.stok,0) <= ISNULL(b.stok_min,0) THEN N'MENIPIS'
            ELSE N'AMAN' END AS status_stok
FROM dbo.tbBarang b
LEFT JOIN dbo.tbKategori k ON k.kategori_id = b.kategori_id
LEFT JOIN dbo.tbMerek m ON m.merek_id = b.merek_id
LEFT JOIN dbo.tbSatuan s ON s.satuan_id = b.satuan_id
WHERE (@0 IS NULL OR @0 = N'' OR b.kodebarang LIKE N'%'+@0+N'%'
       OR b.namabarang LIKE N'%'+@0+N'%' OR ISNULL(b.barcode,N'') LIKE N'%'+@0+N'%')
  AND (@1 = 0 OR ISNULL(b.stok,0) <= ISNULL(b.stok_min,0))
ORDER BY b.namabarang", new object[] { keyword, hanyaMenipis ? 1 : 0 });
            }
        }

        public string SaveBarang(string kode, string nama, int? kategoriId, int? merekId,
            int? satuanId, string satuanNama, string barcode, decimal hargaBeli,
            decimal hargaJual, decimal stok, decimal stokMin)
        {
            if (string.IsNullOrWhiteSpace(kode)) throw new ArgumentException("Kode barang wajib diisi.");
            if (string.IsNullOrWhiteSpace(nama)) throw new ArgumentException("Nama barang wajib diisi.");
            if (hargaJual < 0 || hargaBeli < 0) throw new ArgumentException("Harga tidak boleh negatif.");
            if (stok < 0 || stokMin < 0) throw new ArgumentException("Stok tidak boleh negatif.");

            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                int affected = conn.ExecSqlNonQuery(@"
IF EXISTS (SELECT 1 FROM dbo.tbBarang WHERE kodebarang=@0)
BEGIN
    UPDATE dbo.tbBarang SET namabarang=@1, kategori_id=@2, merek_id=@3, satuan_id=@4,
        satuan=@5, barcode=@6, harga_beli=@7, hargasatuan=@8, stok=@9, stok_min=@10
    WHERE kodebarang=@0;
END
ELSE
BEGIN
    INSERT INTO dbo.tbBarang (kodebarang,namabarang,kategori_id,merek_id,satuan_id,satuan,
        barcode,harga_beli,hargasatuan,stok,stok_min)
    VALUES (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10);
END", new object[]
                {
                    kode.Trim(), nama.Trim(), kategoriId, merekId, satuanId,
                    string.IsNullOrWhiteSpace(satuanNama) ? "pcs" : satuanNama.Trim(),
                    string.IsNullOrWhiteSpace(barcode) ? (object)DBNull.Value : barcode.Trim(),
                    hargaBeli, hargaJual, stok, stokMin
                });

                return affected > 0 ? "Data barang berhasil disimpan." : "Data barang tidak berubah.";
            }
        }

        public string DeleteBarang(string kode)
        {
            if (string.IsNullOrWhiteSpace(kode)) throw new ArgumentException("Kode barang wajib diisi.");
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                object dipakai = conn.ExecSqlScalar(
                    "SELECT COUNT(1) FROM dbo.tblsifaktur WHERE kodebarang=@0", new object[] { kode.Trim() });
                if (Convert.ToInt32(dipakai) > 0)
                    throw new InvalidOperationException("Barang sudah dipakai pada transaksi penjualan sehingga tidak boleh dihapus.");

                conn.ExecSqlNonQuery("DELETE FROM dbo.tbBarang WHERE kodebarang=@0", new object[] { kode.Trim() });
                return "Barang berhasil dihapus.";
            }
        }

        // =========================================================
        // KATEGORI / MEREK / SATUAN
        // =========================================================
        public DataTable GetKategori()
        {
            return Select("SELECT kategori_id,nama,keterangan,is_active,created_at FROM dbo.tbKategori ORDER BY nama");
        }

        public string SaveKategori(int id, string nama, string keterangan, bool aktif)
        {
            if (string.IsNullOrWhiteSpace(nama)) throw new ArgumentException("Nama kategori wajib diisi.");
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                if (id > 0)
                    conn.ExecSqlNonQuery("UPDATE dbo.tbKategori SET nama=@1,keterangan=@2,is_active=@3 WHERE kategori_id=@0",
                        new object[] { id, nama.Trim(), (object)keterangan ?? DBNull.Value, aktif ? 1 : 0 });
                else
                    conn.ExecSqlNonQuery("INSERT INTO dbo.tbKategori (nama,keterangan,is_active) VALUES (@0,@1,@2)",
                        new object[] { nama.Trim(), (object)keterangan ?? DBNull.Value, aktif ? 1 : 0 });
                return "Kategori berhasil disimpan.";
            }
        }

        public string DeleteKategori(int id)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                object dipakai = conn.ExecSqlScalar("SELECT COUNT(1) FROM dbo.tbBarang WHERE kategori_id=@0", new object[] { id });
                if (Convert.ToInt32(dipakai) > 0)
                    throw new InvalidOperationException("Kategori masih dipakai oleh barang.");
                conn.ExecSqlNonQuery("DELETE FROM dbo.tbKategori WHERE kategori_id=@0", new object[] { id });
                return "Kategori berhasil dihapus.";
            }
        }

        public DataTable GetMerek()
        {
            return Select("SELECT merek_id,nama,is_active,created_at FROM dbo.tbMerek ORDER BY nama");
        }

        public string SaveMerek(int id, string nama, bool aktif)
        {
            if (string.IsNullOrWhiteSpace(nama)) throw new ArgumentException("Nama merek wajib diisi.");
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                if (id > 0)
                    conn.ExecSqlNonQuery("UPDATE dbo.tbMerek SET nama=@1,is_active=@2 WHERE merek_id=@0",
                        new object[] { id, nama.Trim(), aktif ? 1 : 0 });
                else
                    conn.ExecSqlNonQuery("INSERT INTO dbo.tbMerek (nama,is_active) VALUES (@0,@1)",
                        new object[] { nama.Trim(), aktif ? 1 : 0 });
                return "Merek berhasil disimpan.";
            }
        }

        public string DeleteMerek(int id)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                object dipakai = conn.ExecSqlScalar("SELECT COUNT(1) FROM dbo.tbBarang WHERE merek_id=@0", new object[] { id });
                if (Convert.ToInt32(dipakai) > 0)
                    throw new InvalidOperationException("Merek masih dipakai oleh barang.");
                conn.ExecSqlNonQuery("DELETE FROM dbo.tbMerek WHERE merek_id=@0", new object[] { id });
                return "Merek berhasil dihapus.";
            }
        }

        public DataTable GetSatuan()
        {
            return Select("SELECT satuan_id,nama,is_active FROM dbo.tbSatuan ORDER BY nama");
        }

        public string SaveSatuan(int id, string nama, bool aktif)
        {
            if (string.IsNullOrWhiteSpace(nama)) throw new ArgumentException("Nama satuan wajib diisi.");
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                if (id > 0)
                    conn.ExecSqlNonQuery("UPDATE dbo.tbSatuan SET nama=@1,is_active=@2 WHERE satuan_id=@0",
                        new object[] { id, nama.Trim(), aktif ? 1 : 0 });
                else
                    conn.ExecSqlNonQuery("INSERT INTO dbo.tbSatuan (nama,is_active) VALUES (@0,@1)",
                        new object[] { nama.Trim(), aktif ? 1 : 0 });
                return "Satuan berhasil disimpan.";
            }
        }

        public string DeleteSatuan(int id)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                object dipakai = conn.ExecSqlScalar("SELECT COUNT(1) FROM dbo.tbBarang WHERE satuan_id=@0", new object[] { id });
                if (Convert.ToInt32(dipakai) > 0)
                    throw new InvalidOperationException("Satuan masih dipakai oleh barang.");
                conn.ExecSqlNonQuery("DELETE FROM dbo.tbSatuan WHERE satuan_id=@0", new object[] { id });
                return "Satuan berhasil dihapus.";
            }
        }

        // =========================================================
        // SUPPLIER
        // =========================================================
        public DataTable GetSupplier(string keyword = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT supplier_id,kode,nama,telepon,email,alamat,ISNULL(saldo_hutang,0) AS saldo_hutang,is_active
FROM dbo.tbSupplier
WHERE (@0 IS NULL OR @0=N'' OR nama LIKE N'%'+@0+N'%' OR ISNULL(kode,N'') LIKE N'%'+@0+N'%')
ORDER BY nama", new object[] { keyword });
            }
        }

        public string SaveSupplier(int id, string kode, string nama, string telepon, string email, string alamat, bool aktif)
        {
            if (string.IsNullOrWhiteSpace(nama)) throw new ArgumentException("Nama supplier wajib diisi.");
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                object[] par =
                {
                    id, Kosong(kode), nama.Trim(), Kosong(telepon), Kosong(email), Kosong(alamat), aktif ? 1 : 0
                };
                if (id > 0)
                    conn.ExecSqlNonQuery(@"UPDATE dbo.tbSupplier SET kode=@1,nama=@2,telepon=@3,email=@4,alamat=@5,is_active=@6 WHERE supplier_id=@0", par);
                else
                    conn.ExecSqlNonQuery(@"INSERT INTO dbo.tbSupplier (kode,nama,telepon,email,alamat,is_active) VALUES (@1,@2,@3,@4,@5,@6)", par);
                return "Supplier berhasil disimpan.";
            }
        }

        public string DeleteSupplier(int id)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                object dipakai = conn.ExecSqlScalar("SELECT COUNT(1) FROM dbo.tbPembelian WHERE supplier_id=@0", new object[] { id });
                if (Convert.ToInt32(dipakai) > 0)
                    throw new InvalidOperationException("Supplier sudah dipakai pada transaksi pembelian.");
                conn.ExecSqlNonQuery("DELETE FROM dbo.tbSupplier WHERE supplier_id=@0", new object[] { id });
                return "Supplier berhasil dihapus.";
            }
        }

        // =========================================================
        // PELANGGAN
        // =========================================================
        public DataTable GetPelanggan(string keyword = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT pelanggan_id,kode,nama,telepon,email,alamat,ISNULL(saldo_piutang,0) AS saldo_piutang,is_active
FROM dbo.tbPelanggan
WHERE (@0 IS NULL OR @0=N'' OR nama LIKE N'%'+@0+N'%' OR ISNULL(kode,N'') LIKE N'%'+@0+N'%')
ORDER BY nama", new object[] { keyword });
            }
        }

        public string SavePelanggan(int id, string kode, string nama, string telepon, string email, string alamat, bool aktif)
        {
            if (string.IsNullOrWhiteSpace(nama)) throw new ArgumentException("Nama pelanggan wajib diisi.");
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                object[] par =
                {
                    id, Kosong(kode), nama.Trim(), Kosong(telepon), Kosong(email), Kosong(alamat), aktif ? 1 : 0
                };
                if (id > 0)
                    conn.ExecSqlNonQuery(@"UPDATE dbo.tbPelanggan SET kode=@1,nama=@2,telepon=@3,email=@4,alamat=@5,is_active=@6 WHERE pelanggan_id=@0", par);
                else
                    conn.ExecSqlNonQuery(@"INSERT INTO dbo.tbPelanggan (kode,nama,telepon,email,alamat,is_active) VALUES (@1,@2,@3,@4,@5,@6)", par);
                return "Pelanggan berhasil disimpan.";
            }
        }

        public string DeletePelanggan(int id)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                object dipakai = conn.ExecSqlScalar("SELECT COUNT(1) FROM dbo.tbFaktur WHERE pelanggan_id=@0", new object[] { id });
                if (Convert.ToInt32(dipakai) > 0)
                    throw new InvalidOperationException("Pelanggan sudah memiliki transaksi penjualan.");
                conn.ExecSqlNonQuery("DELETE FROM dbo.tbPelanggan WHERE pelanggan_id=@0", new object[] { id });
                return "Pelanggan berhasil dihapus.";
            }
        }

        // =========================================================
        // PROFIL TOKO / ROLE / LOOKUP
        // =========================================================
        public ClassProfilToko GetProfil()
        {
            ClassProfilToko profil = new ClassProfilToko
            {
                nama_toko = "Toko Root-ID",
                alamat = string.Empty,
                telepon = string.Empty,
                email = string.Empty,
                footer_struk = "Terima kasih telah berbelanja"
            };
            try
            {
                DataTable dt = Select("SELECT TOP 1 nama_toko,alamat,telepon,email,footer_struk FROM dbo.tbProfilToko WHERE profil_id=1");
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    profil.nama_toko = Convert.ToString(r["nama_toko"]);
                    profil.alamat = Convert.ToString(r["alamat"]);
                    profil.telepon = Convert.ToString(r["telepon"]);
                    profil.email = Convert.ToString(r["email"]);
                    profil.footer_struk = Convert.ToString(r["footer_struk"]);
                }
            }
            catch { }
            return profil;
        }

        public string SaveProfil(ClassProfilToko profil)
        {
            if (profil == null || string.IsNullOrWhiteSpace(profil.nama_toko))
                throw new ArgumentException("Nama toko wajib diisi.");
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                conn.ExecSqlNonQuery(@"
IF EXISTS (SELECT 1 FROM dbo.tbProfilToko WHERE profil_id=1)
    UPDATE dbo.tbProfilToko SET nama_toko=@0,alamat=@1,telepon=@2,email=@3,footer_struk=@4 WHERE profil_id=1
ELSE
    INSERT INTO dbo.tbProfilToko (profil_id,nama_toko,alamat,telepon,email,footer_struk) VALUES (1,@0,@1,@2,@3,@4)",
                    new object[] { profil.nama_toko.Trim(), Kosong(profil.alamat), Kosong(profil.telepon), Kosong(profil.email), Kosong(profil.footer_struk) });
                return "Profil toko berhasil disimpan.";
            }
        }

        public DataTable GetRole()
        {
            return Select("SELECT role_id,role_name FROM dbo.tbRole WHERE is_active=1 ORDER BY role_name");
        }

        public DataTable GetGudang()
        {
            return Select("SELECT gudang_id,kode,nama,alamat FROM dbo.tbGudang WHERE is_active=1 ORDER BY nama");
        }

        // =========================================================
        // HELPER
        // =========================================================
        private DataTable Select(string sql)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(sql);
            }
        }

        private static object Kosong(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }
    }
}
