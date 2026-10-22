using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CommonClass;

namespace DAAccess
{
    /// <summary>
    /// Data access retur penjualan dan retur pembelian. Setiap retur mengembalikan
    /// atau mengurangi stok, mencatat mutasi, dan mencatat kas pengembalian dana
    /// dalam satu transaksi database.
    /// </summary>
    public class DARetur
    {
        public DataTable GetReturJual(DateTime mulai, DateTime akhir, string keyword = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT r.retur_id,r.nomor,r.tanggal,ISNULL(r.nomor_penjualan,N'-') AS nomor_penjualan,
       ISNULL(r.alasan,N'') AS alasan,r.total,r.status,
       (SELECT COUNT(1) FROM dbo.tbReturJualDetail d WHERE d.retur_id=r.retur_id) AS jumlah_item
FROM dbo.tbReturJual r
WHERE CAST(r.tanggal AS DATE) BETWEEN @0 AND @1
  AND (@2 IS NULL OR @2=N'' OR r.nomor LIKE N'%'+@2+N'%' OR ISNULL(r.nomor_penjualan,N'') LIKE N'%'+@2+N'%')
ORDER BY r.tanggal DESC,r.retur_id DESC", new object[] { mulai.Date, akhir.Date, keyword });
            }
        }

        public DataTable GetReturBeli(DateTime mulai, DateTime akhir, string keyword = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT r.retur_id,r.nomor,r.tanggal,ISNULL(p.nomor,N'-') AS nomor_pembelian,
       ISNULL(r.alasan,N'') AS alasan,r.total,r.status,
       (SELECT COUNT(1) FROM dbo.tbReturBeliDetail d WHERE d.retur_id=r.retur_id) AS jumlah_item
FROM dbo.tbReturBeli r
LEFT JOIN dbo.tbPembelian p ON p.pembelian_id=r.pembelian_id
WHERE CAST(r.tanggal AS DATE) BETWEEN @0 AND @1
  AND (@2 IS NULL OR @2=N'' OR r.nomor LIKE N'%'+@2+N'%' OR ISNULL(p.nomor,N'') LIKE N'%'+@2+N'%')
ORDER BY r.tanggal DESC,r.retur_id DESC", new object[] { mulai.Date, akhir.Date, keyword });
            }
        }

        /// <summary>Barang pada satu faktur penjualan untuk dipilih saat retur.</summary>
        public DataTable GetBarangPenjualan(string nofaktur)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT d.kodebarang,ISNULL(b.namabarang,d.namabarang) AS namabarang,
       CAST(d.jumlahbeli AS DECIMAL(18,3)) AS qty,
       d.hargasatuan AS harga,d.diskon,d.total
FROM dbo.tblsifaktur d
LEFT JOIN dbo.tbBarang b ON b.kodebarang=d.kodebarang
WHERE RTRIM(d.nofaktur)=@0 ORDER BY d.kodebarang", new object[] { nofaktur });
            }
        }

        public DataTable GetBarangPembelian(long pembelianId)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT d.kodebarang,ISNULL(b.namabarang,d.kodebarang) AS namabarang,
       CAST(d.qty AS DECIMAL(18,3)) AS qty,d.harga_beli AS harga,d.diskon,d.total
FROM dbo.tbIsiPembelian d
LEFT JOIN dbo.tbBarang b ON b.kodebarang=d.kodebarang
WHERE d.pembelian_id=@0 ORDER BY d.detail_id", new object[] { pembelianId });
            }
        }

        /// <summary>Qty yang sudah pernah diretur untuk satu faktur penjualan, per barang.</summary>
        public DataTable GetQtyReturJual(string nomorPenjualan)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT d.kodebarang,CAST(SUM(d.qty) AS DECIMAL(18,3)) AS qty
FROM dbo.tbReturJualDetail d
INNER JOIN dbo.tbReturJual r ON r.retur_id=d.retur_id
WHERE r.nomor_penjualan=@0
GROUP BY d.kodebarang", new object[] { nomorPenjualan });
            }
        }

        /// <summary>Qty yang sudah pernah diretur untuk satu dokumen pembelian, per barang.</summary>
        public DataTable GetQtyReturBeli(long pembelianId)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT d.kodebarang,CAST(SUM(d.qty) AS DECIMAL(18,3)) AS qty
FROM dbo.tbReturBeliDetail d
INNER JOIN dbo.tbReturBeli r ON r.retur_id=d.retur_id
WHERE r.pembelian_id=@0
GROUP BY d.kodebarang", new object[] { pembelianId });
            }
        }

        public string GenerateNomor()
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                DataTable dt = conn.ExecQuery("EXEC dbo.usp_GenerateNomor @0,@1,@2",
                    new object[] { "RTJ", "tbReturJual", "nomor" });
                if (dt.Rows.Count > 0) return Convert.ToString(dt.Rows[0]["nomor"]);
                return "RTJ" + DateTime.Now.ToString("yyyyMMdd") + "-0001";
            }
        }

        public ClassSimpanHasil SimpanReturJual(string nomorPenjualan, DateTime tanggal, string alasan,
            List<ClassReturItem> items, string user)
        {
            if (string.IsNullOrWhiteSpace(nomorPenjualan))
                throw new InvalidOperationException("Nomor penjualan wajib dipilih.");
            if (items == null || items.Count == 0)
                throw new InvalidOperationException("Belum ada barang yang diretur.");

            ClassSimpanHasil hasil = new ClassSimpanHasil();
            using (SqlConnection conn = dbConfiguration.createConection())
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        decimal total = 0m;
                        foreach (ClassReturItem item in items)
                        {
                            if (item.qty <= 0) throw new InvalidOperationException("Qty retur harus lebih dari nol.");
                            if (item.harga < 0) throw new InvalidOperationException("Harga retur tidak valid.");

                            decimal terjual;
                            using (SqlCommand cmd = new SqlCommand(
                                "SELECT ISNULL(SUM(jumlahbeli),0) FROM dbo.tblsifaktur WHERE RTRIM(nofaktur)=@no AND kodebarang=@kode", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@no", nomorPenjualan);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                terjual = Convert.ToDecimal(cmd.ExecuteScalar());
                            }

                            decimal sudah;
                            using (SqlCommand cmd = new SqlCommand(@"
SELECT ISNULL(SUM(d.qty),0) FROM dbo.tbReturJualDetail d
INNER JOIN dbo.tbReturJual r ON r.retur_id=d.retur_id
WHERE r.nomor_penjualan=@no AND d.kodebarang=@kode", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@no", nomorPenjualan);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                sudah = Convert.ToDecimal(cmd.ExecuteScalar());
                            }

                            if (item.qty + sudah > terjual)
                                throw new InvalidOperationException("Qty retur " + item.namabarang +
                                    " melebihi sisa yang bisa diretur. Terjual " + terjual.ToString("0.###") +
                                    ", sudah diretur " + sudah.ToString("0.###") + ".");
                            total += item.total;
                        }

                        string nomor = GenerateNomor(conn, tx, "RTJ", "tbReturJual");
                        long returId;
                        using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbReturJual (nomor,tanggal,nomor_penjualan,alasan,total,status)
VALUES (@nomor,@tgl,@no,@alasan,@total,N'Posted');
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@nomor", nomor);
                            cmd.Parameters.AddWithValue("@tgl", tanggal == default(DateTime) ? DateTime.Now : tanggal);
                            cmd.Parameters.AddWithValue("@no", nomorPenjualan);
                            cmd.Parameters.AddWithValue("@alasan", string.IsNullOrWhiteSpace(alasan) ? (object)DBNull.Value : alasan.Trim());
                            cmd.Parameters.AddWithValue("@total", total);
                            returId = Convert.ToInt64(cmd.ExecuteScalar());
                        }

                        foreach (ClassReturItem item in items)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbReturJualDetail (retur_id,kodebarang,qty,harga) VALUES (@id,@kode,@qty,@harga);", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", returId);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                cmd.Parameters.AddWithValue("@qty", item.qty);
                                cmd.Parameters.AddWithValue("@harga", item.harga);
                                cmd.ExecuteNonQuery();
                            }
                            using (SqlCommand cmd = new SqlCommand(
                                "UPDATE dbo.tbBarang SET stok=ISNULL(stok,0)+@qty WHERE kodebarang=@kode", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@qty", item.qty);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                cmd.ExecuteNonQuery();
                            }
                            InsertMutasi(conn, tx, item.kodebarang, "Masuk", item.qty, nomor,
                                "Retur penjualan " + item.namabarang, user);
                        }

                        if (total > 0)
                            InsertKas(conn, tx, "Keluar", "Retur Penjualan", total, nomor,
                                "Pengembalian dana retur " + nomor, user);

                        tx.Commit();
                        hasil.sukses = true;
                        hasil.nomor = nomor;
                        hasil.pesan = "Retur penjualan " + nomor + " berhasil disimpan.";
                        return hasil;
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        public ClassSimpanHasil SimpanReturBeli(long pembelianId, DateTime tanggal, string alasan,
            List<ClassReturItem> items, string user)
        {
            if (pembelianId <= 0)
                throw new InvalidOperationException("Nomor pembelian wajib dipilih.");
            if (items == null || items.Count == 0)
                throw new InvalidOperationException("Belum ada barang yang diretur.");

            ClassSimpanHasil hasil = new ClassSimpanHasil();
            using (SqlConnection conn = dbConfiguration.createConection())
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        decimal total = 0m;
                        foreach (ClassReturItem item in items)
                        {
                            if (item.qty <= 0) throw new InvalidOperationException("Qty retur harus lebih dari nol.");
                            decimal dibeli;
                            using (SqlCommand cmd = new SqlCommand(
                                "SELECT ISNULL(SUM(qty),0) FROM dbo.tbIsiPembelian WHERE pembelian_id=@id AND kodebarang=@kode", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", pembelianId);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                dibeli = Convert.ToDecimal(cmd.ExecuteScalar());
                            }
                            if (item.qty > dibeli)
                                throw new InvalidOperationException("Qty retur " + item.namabarang +
                                    " melebihi jumlah pembelian (" + dibeli.ToString("0.###") + ").");

                            decimal sudah;
                            using (SqlCommand cmd = new SqlCommand(@"
SELECT ISNULL(SUM(d.qty),0) FROM dbo.tbReturBeliDetail d
INNER JOIN dbo.tbReturBeli r ON r.retur_id=d.retur_id
WHERE r.pembelian_id=@id AND d.kodebarang=@kode", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", pembelianId);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                sudah = Convert.ToDecimal(cmd.ExecuteScalar());
                            }
                            if (item.qty + sudah > dibeli)
                                throw new InvalidOperationException("Qty retur " + item.namabarang +
                                    " melebihi sisa yang bisa diretur. Dibeli " + dibeli.ToString("0.###") +
                                    ", sudah diretur " + sudah.ToString("0.###") + ".");

                            decimal stok;
                            using (SqlCommand cmd = new SqlCommand(
                                "SELECT CAST(ISNULL(stok,0) AS DECIMAL(18,3)) FROM dbo.tbBarang WITH (UPDLOCK,HOLDLOCK) WHERE kodebarang=@kode", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                object nilai = cmd.ExecuteScalar();
                                if (nilai == null) throw new InvalidOperationException("Barang " + item.kodebarang + " tidak ditemukan.");
                                stok = Convert.ToDecimal(nilai);
                            }
                            if (stok < item.qty)
                                throw new InvalidOperationException("Stok " + item.namabarang + " tidak cukup untuk diretur ke supplier.");

                            total += item.total;
                        }

                        string nomor = GenerateNomor(conn, tx, "RTB", "tbReturBeli");
                        long returId;
                        using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbReturBeli (nomor,tanggal,pembelian_id,alasan,total,status)
VALUES (@nomor,@tgl,@id,@alasan,@total,N'Posted');
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@nomor", nomor);
                            cmd.Parameters.AddWithValue("@tgl", tanggal == default(DateTime) ? DateTime.Now : tanggal);
                            cmd.Parameters.AddWithValue("@id", pembelianId);
                            cmd.Parameters.AddWithValue("@alasan", string.IsNullOrWhiteSpace(alasan) ? (object)DBNull.Value : alasan.Trim());
                            cmd.Parameters.AddWithValue("@total", total);
                            returId = Convert.ToInt64(cmd.ExecuteScalar());
                        }

                        foreach (ClassReturItem item in items)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbReturBeliDetail (retur_id,kodebarang,qty,harga) VALUES (@id,@kode,@qty,@harga);", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", returId);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                cmd.Parameters.AddWithValue("@qty", item.qty);
                                cmd.Parameters.AddWithValue("@harga", item.harga);
                                cmd.ExecuteNonQuery();
                            }
                            using (SqlCommand cmd = new SqlCommand(
                                "UPDATE dbo.tbBarang SET stok=stok-@qty WHERE kodebarang=@kode", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@qty", item.qty);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                cmd.ExecuteNonQuery();
                            }
                            InsertMutasi(conn, tx, item.kodebarang, "Keluar", -item.qty, nomor,
                                "Retur pembelian " + item.namabarang, user);
                        }

                        if (total > 0)
                            InsertKas(conn, tx, "Masuk", "Retur Pembelian", total, nomor,
                                "Pengembalian dana retur pembelian " + nomor, user);

                        tx.Commit();
                        hasil.sukses = true;
                        hasil.nomor = nomor;
                        hasil.pesan = "Retur pembelian " + nomor + " berhasil disimpan.";
                        return hasil;
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        private static string GenerateNomor(SqlConnection conn, SqlTransaction tx, string prefix, string table)
        {
            string like = prefix + DateTime.Now.ToString("yyyyMMdd") + "-%";
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(MAX(nomor),'') FROM dbo." + table + " WITH (UPDLOCK,HOLDLOCK) WHERE nomor LIKE @like", conn, tx))
            {
                cmd.Parameters.AddWithValue("@like", like);
                string last = Convert.ToString(cmd.ExecuteScalar());
                int nomor = 0;
                if (!string.IsNullOrWhiteSpace(last))
                {
                    int idx = last.LastIndexOf('-');
                    if (idx >= 0) int.TryParse(last.Substring(idx + 1), out nomor);
                }
                return prefix + DateTime.Now.ToString("yyyyMMdd") + "-" + (nomor + 1).ToString("0000");
            }
        }

        private static void InsertMutasi(SqlConnection conn, SqlTransaction tx, string kode,
            string tipe, decimal qty, string referensi, string catatan, string user)
        {
            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbMutasiStok (kodebarang,gudang_id,tipe,qty,referensi,catatan,created_by)
VALUES (@kode,(SELECT TOP 1 gudang_id FROM dbo.tbGudang ORDER BY gudang_id),@tipe,@qty,@ref,@catatan,@user);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@kode", kode);
                cmd.Parameters.AddWithValue("@tipe", tipe);
                cmd.Parameters.AddWithValue("@qty", qty);
                cmd.Parameters.AddWithValue("@ref", referensi);
                cmd.Parameters.AddWithValue("@catatan", catatan);
                cmd.Parameters.AddWithValue("@user", (object)user ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertKas(SqlConnection conn, SqlTransaction tx, string tipe, string kategori,
            decimal nominal, string referensi, string keterangan, string user)
        {
            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbKas (tipe,kategori,nominal,metode_bayar,referensi,keterangan,created_by)
VALUES (@tipe,@kategori,@nominal,N'Tunai',@ref,@ket,@user);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@tipe", tipe);
                cmd.Parameters.AddWithValue("@kategori", kategori);
                cmd.Parameters.AddWithValue("@nominal", nominal);
                cmd.Parameters.AddWithValue("@ref", referensi);
                cmd.Parameters.AddWithValue("@ket", keterangan);
                cmd.Parameters.AddWithValue("@user", (object)user ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
