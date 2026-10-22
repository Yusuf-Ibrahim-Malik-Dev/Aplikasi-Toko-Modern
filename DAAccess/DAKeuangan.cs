using System;
using System.Data;
using System.Data.SqlClient;
using CommonClass;

namespace DAAccess
{
    /// <summary>Data access modul keuangan: kas, hutang supplier, dan piutang pelanggan.</summary>
    public class DAKeuangan
    {
        // =========================================================
        // KAS
        // =========================================================
        public DataTable GetKas(DateTime mulai, DateTime akhir, string tipe = null, string keyword = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT kas_id,tanggal,tipe,kategori,nominal,ISNULL(metode_bayar,N'-') AS metode_bayar,
       ISNULL(referensi,N'-') AS referensi,ISNULL(keterangan,N'') AS keterangan,ISNULL(created_by,N'-') AS created_by
FROM dbo.tbKas
WHERE CAST(tanggal AS DATE) BETWEEN @0 AND @1
  AND (@2 IS NULL OR @2=N'' OR tipe=@2)
  AND (@3 IS NULL OR @3=N'' OR kategori LIKE N'%'+@3+N'%' OR ISNULL(keterangan,N'') LIKE N'%'+@3+N'%')
ORDER BY tanggal DESC,kas_id DESC", new object[] { mulai.Date, akhir.Date, tipe, keyword });
            }
        }

        public string SimpanKas(ClassKas kas)
        {
            if (kas == null) throw new ArgumentNullException("kas");
            if (kas.nominal <= 0) throw new ArgumentException("Nominal kas harus lebih dari nol.");
            if (kas.tipe != "Masuk" && kas.tipe != "Keluar") throw new ArgumentException("Tipe kas tidak valid.");
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                conn.ExecSqlNonQuery(@"
INSERT INTO dbo.tbKas (tanggal,tipe,kategori,nominal,metode_bayar,referensi,keterangan,created_by)
VALUES (@0,@1,@2,@3,@4,@5,@6,@7)", new object[]
                {
                    kas.tanggal == default(DateTime) ? DateTime.Now : kas.tanggal,
                    kas.tipe, kas.kategori, kas.nominal,
                    string.IsNullOrWhiteSpace(kas.metode_bayar) ? "Tunai" : kas.metode_bayar,
                    (object)kas.referensi ?? DBNull.Value,
                    (object)kas.keterangan ?? DBNull.Value,
                    (object)kas.created_by ?? DBNull.Value
                });
                return "Transaksi kas berhasil disimpan.";
            }
        }

        // =========================================================
        // HUTANG
        // =========================================================
        public DataTable GetHutang(bool hanyaTerbuka = false, string keyword = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT h.hutang_id,ISNULL(s.nama,N'Umum') AS supplier,h.tanggal,h.jatuh_tempo,h.nominal,h.terbayar,
       (h.nominal-h.terbayar) AS sisa,h.status
FROM dbo.tbHutang h LEFT JOIN dbo.tbSupplier s ON s.supplier_id=h.supplier_id
WHERE (@0=0 OR h.nominal>h.terbayar)
  AND (@1 IS NULL OR @1=N'' OR ISNULL(s.nama,N'') LIKE N'%'+@1+N'%')
ORDER BY h.jatuh_tempo,h.hutang_id DESC", new object[] { hanyaTerbuka ? 1 : 0, keyword });
            }
        }

        public string BayarHutang(long hutangId, decimal nominal, string metode, string referensi)
        {
            if (nominal <= 0) throw new ArgumentException("Nominal pembayaran harus lebih dari nol.");
            using (SqlConnection conn = dbConfiguration.createConection())
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        decimal sisa;
                        int supplierId;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT (nominal-terbayar),ISNULL(supplier_id,0) FROM dbo.tbHutang WITH (UPDLOCK,HOLDLOCK) WHERE hutang_id=@id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", hutangId);
                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {
                                if (!rd.Read()) throw new InvalidOperationException("Data hutang tidak ditemukan.");
                                sisa = Convert.ToDecimal(rd[0]);
                                supplierId = Convert.ToInt32(rd[1]);
                            }
                        }
                        if (nominal > sisa) throw new InvalidOperationException("Pembayaran melebihi sisa hutang.");

                        Exec(conn, tx, "INSERT INTO dbo.tbPembayaranHutang (hutang_id,nominal,metode,referensi) VALUES (@id,@nom,@metode,@ref)",
                            new SqlParameter("@id", hutangId), new SqlParameter("@nom", nominal),
                            new SqlParameter("@metode", (object)metode ?? DBNull.Value),
                            new SqlParameter("@ref", (object)referensi ?? DBNull.Value));
                        Exec(conn, tx, @"UPDATE dbo.tbHutang SET terbayar=terbayar+@nom,
status=CASE WHEN terbayar+@nom>=nominal THEN N'Lunas' ELSE N'Sebagian' END WHERE hutang_id=@id",
                            new SqlParameter("@nom", nominal), new SqlParameter("@id", hutangId));
                        if (supplierId > 0)
                            Exec(conn, tx, "UPDATE dbo.tbSupplier SET saldo_hutang=CASE WHEN saldo_hutang>=@nom THEN saldo_hutang-@nom ELSE 0 END WHERE supplier_id=@id",
                                new SqlParameter("@nom", nominal), new SqlParameter("@id", supplierId));
                        Exec(conn, tx, @"INSERT INTO dbo.tbKas (tipe,kategori,nominal,metode_bayar,referensi,keterangan)
VALUES (N'Keluar',N'Pembayaran Hutang',@nom,@metode,@ref,N'Pembayaran hutang')",
                            new SqlParameter("@nom", nominal), new SqlParameter("@metode", (object)metode ?? DBNull.Value),
                            new SqlParameter("@ref", (object)referensi ?? DBNull.Value));
                        tx.Commit();
                        return "Pembayaran hutang berhasil disimpan.";
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        // =========================================================
        // PIUTANG
        // =========================================================
        public DataTable GetPiutang(bool hanyaTerbuka = false, string keyword = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT p.piutang_id,ISNULL(c.nama,N'Umum') AS pelanggan,ISNULL(p.nomor_penjualan,N'-') AS nomor_penjualan,
       p.tanggal,p.jatuh_tempo,p.nominal,p.terbayar,(p.nominal-p.terbayar) AS sisa,p.status
FROM dbo.tbPiutang p LEFT JOIN dbo.tbPelanggan c ON c.pelanggan_id=p.pelanggan_id
WHERE (@0=0 OR p.nominal>p.terbayar)
  AND (@1 IS NULL OR @1=N'' OR ISNULL(c.nama,N'') LIKE N'%'+@1+N'%' OR ISNULL(p.nomor_penjualan,N'') LIKE N'%'+@1+N'%')
ORDER BY p.jatuh_tempo,p.piutang_id DESC", new object[] { hanyaTerbuka ? 1 : 0, keyword });
            }
        }

        public string BayarPiutang(long piutangId, decimal nominal, string metode, string referensi)
        {
            if (nominal <= 0) throw new ArgumentException("Nominal pembayaran harus lebih dari nol.");
            using (SqlConnection conn = dbConfiguration.createConection())
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        decimal sisa;
                        int pelangganId;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT (nominal-terbayar),ISNULL(pelanggan_id,0) FROM dbo.tbPiutang WITH (UPDLOCK,HOLDLOCK) WHERE piutang_id=@id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", piutangId);
                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {
                                if (!rd.Read()) throw new InvalidOperationException("Data piutang tidak ditemukan.");
                                sisa = Convert.ToDecimal(rd[0]);
                                pelangganId = Convert.ToInt32(rd[1]);
                            }
                        }
                        if (nominal > sisa) throw new InvalidOperationException("Pembayaran melebihi sisa piutang.");

                        Exec(conn, tx, "INSERT INTO dbo.tbPembayaranPiutang (piutang_id,nominal,metode,referensi) VALUES (@id,@nom,@metode,@ref)",
                            new SqlParameter("@id", piutangId), new SqlParameter("@nom", nominal),
                            new SqlParameter("@metode", (object)metode ?? DBNull.Value),
                            new SqlParameter("@ref", (object)referensi ?? DBNull.Value));
                        Exec(conn, tx, @"UPDATE dbo.tbPiutang SET terbayar=terbayar+@nom,
status=CASE WHEN terbayar+@nom>=nominal THEN N'Lunas' ELSE N'Sebagian' END WHERE piutang_id=@id",
                            new SqlParameter("@nom", nominal), new SqlParameter("@id", piutangId));
                        if (pelangganId > 0)
                            Exec(conn, tx, "UPDATE dbo.tbPelanggan SET saldo_piutang=CASE WHEN saldo_piutang>=@nom THEN saldo_piutang-@nom ELSE 0 END WHERE pelanggan_id=@id",
                                new SqlParameter("@nom", nominal), new SqlParameter("@id", pelangganId));
                        Exec(conn, tx, @"INSERT INTO dbo.tbKas (tipe,kategori,nominal,metode_bayar,referensi,keterangan)
VALUES (N'Masuk',N'Pembayaran Piutang',@nom,@metode,@ref,N'Pembayaran piutang')",
                            new SqlParameter("@nom", nominal), new SqlParameter("@metode", (object)metode ?? DBNull.Value),
                            new SqlParameter("@ref", (object)referensi ?? DBNull.Value));
                        tx.Commit();
                        return "Pembayaran piutang berhasil disimpan.";
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        private static void Exec(SqlConnection conn, SqlTransaction tx, string sql, params SqlParameter[] par)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                if (par != null) cmd.Parameters.AddRange(par);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
