using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CommonClass;

namespace DAAccess
{
    /// <summary>Data access inventory: stok, mutasi, opname, dan transfer gudang.</summary>
    public class DAInventory
    {
        public DataTable GetMutasi(DateTime mulai, DateTime akhir, string tipe = null, string keyword = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT m.mutasi_id,m.tanggal,m.kodebarang,ISNULL(b.namabarang,m.kodebarang) AS namabarang,
       ISNULL(g.nama,N'-') AS gudang,m.tipe,m.qty,m.referensi,m.catatan,ISNULL(m.created_by,N'-') AS created_by
FROM dbo.tbMutasiStok m
LEFT JOIN dbo.tbBarang b ON b.kodebarang=m.kodebarang
LEFT JOIN dbo.tbGudang g ON g.gudang_id=m.gudang_id
WHERE CAST(m.tanggal AS DATE) BETWEEN @0 AND @1
  AND (@2 IS NULL OR @2=N'' OR m.tipe=@2)
  AND (@3 IS NULL OR @3=N'' OR m.kodebarang LIKE N'%'+@3+N'%' OR ISNULL(b.namabarang,N'') LIKE N'%'+@3+N'%')
ORDER BY m.tanggal DESC,m.mutasi_id DESC", new object[] { mulai.Date, akhir.Date, tipe, keyword });
            }
        }

        public DataTable GetOpname(DateTime mulai, DateTime akhir)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT o.opname_id,o.nomor,o.tanggal,ISNULL(g.nama,N'-') AS gudang,o.status,o.catatan,ISNULL(o.created_by,N'-') AS created_by,
       (SELECT COUNT(1) FROM dbo.tbOpnameStokDetail d WHERE d.opname_id=o.opname_id) AS jumlah_item
FROM dbo.tbOpnameStok o LEFT JOIN dbo.tbGudang g ON g.gudang_id=o.gudang_id
WHERE CAST(o.tanggal AS DATE) BETWEEN @0 AND @1 ORDER BY o.tanggal DESC,o.opname_id DESC",
                    new object[] { mulai.Date, akhir.Date });
            }
        }

        public DataTable GetTransfer(DateTime mulai, DateTime akhir)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT t.transfer_id,t.nomor,t.tanggal,g1.nama AS dari_gudang,g2.nama AS ke_gudang,t.status,t.catatan,
       (SELECT COUNT(1) FROM dbo.tbTransferGudangDetail d WHERE d.transfer_id=t.transfer_id) AS jumlah_item
FROM dbo.tbTransferGudang t
LEFT JOIN dbo.tbGudang g1 ON g1.gudang_id=t.dari_gudang
LEFT JOIN dbo.tbGudang g2 ON g2.gudang_id=t.ke_gudang
WHERE CAST(t.tanggal AS DATE) BETWEEN @0 AND @1 ORDER BY t.tanggal DESC,t.transfer_id DESC",
                    new object[] { mulai.Date, akhir.Date });
            }
        }

        public ClassSimpanHasil SimpanOpname(List<ClassOpnameItem> items, int? gudangId, string catatan, string user)
        {
            if (items == null || items.Count == 0) throw new InvalidOperationException("Belum ada barang yang dihitung.");
            ClassSimpanHasil hasil = new ClassSimpanHasil();
            using (SqlConnection conn = dbConfiguration.createConection())
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string nomor = GenerateNomor(conn, tx, "OP", "tbOpnameStok");
                        long opnameId;
                        using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbOpnameStok (nomor,gudang_id,status,catatan,created_by)
VALUES (@nomor,@gudang,N'Posted',@catatan,@user); SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@nomor", nomor);
                            cmd.Parameters.AddWithValue("@gudang", (object)gudangId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@catatan", (object)catatan ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@user", (object)user ?? DBNull.Value);
                            opnameId = Convert.ToInt64(cmd.ExecuteScalar());
                        }

                        foreach (ClassOpnameItem item in items)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbOpnameStokDetail (opname_id,kodebarang,stok_sistem,stok_fisik)
VALUES (@id,@kode,@sistem,@fisik);
UPDATE dbo.tbBarang SET stok=@fisik WHERE kodebarang=@kode;", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", opnameId);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                cmd.Parameters.AddWithValue("@sistem", item.stok_sistem);
                                cmd.Parameters.AddWithValue("@fisik", item.stok_fisik);
                                cmd.ExecuteNonQuery();
                            }
                            if (item.selisih != 0)
                            {
                                using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbMutasiStok (kodebarang,gudang_id,tipe,qty,referensi,catatan,created_by)
VALUES (@kode,@gudang,N'Penyesuaian',@qty,@ref,N'Opname stok',@user);", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                    cmd.Parameters.AddWithValue("@gudang", (object)gudangId ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@qty", item.selisih);
                                    cmd.Parameters.AddWithValue("@ref", nomor);
                                    cmd.Parameters.AddWithValue("@user", (object)user ?? DBNull.Value);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();
                        hasil.sukses = true;
                        hasil.nomor = nomor;
                        hasil.pesan = "Opname " + nomor + " berhasil disimpan.";
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

        public ClassSimpanHasil SimpanTransfer(int dariGudang, int keGudang, List<ClassTransferItem> items,
            string catatan, string user)
        {
            if (dariGudang <= 0 || keGudang <= 0) throw new InvalidOperationException("Gudang asal dan tujuan wajib dipilih.");
            if (dariGudang == keGudang) throw new InvalidOperationException("Gudang asal dan tujuan tidak boleh sama.");
            if (items == null || items.Count == 0) throw new InvalidOperationException("Detail transfer masih kosong.");

            ClassSimpanHasil hasil = new ClassSimpanHasil();
            using (SqlConnection conn = dbConfiguration.createConection())
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string nomor = GenerateNomor(conn, tx, "TR", "tbTransferGudang");
                        long transferId;
                        using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbTransferGudang (nomor,dari_gudang,ke_gudang,status,catatan)
VALUES (@nomor,@dari,@ke,N'Posted',@catatan); SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@nomor", nomor);
                            cmd.Parameters.AddWithValue("@dari", dariGudang);
                            cmd.Parameters.AddWithValue("@ke", keGudang);
                            cmd.Parameters.AddWithValue("@catatan", (object)catatan ?? DBNull.Value);
                            transferId = Convert.ToInt64(cmd.ExecuteScalar());
                        }

                        foreach (ClassTransferItem item in items)
                        {
                            if (item.qty <= 0) throw new InvalidOperationException("Qty transfer harus lebih dari nol.");
                            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbTransferGudangDetail (transfer_id,kodebarang,qty) VALUES (@id,@kode,@qty);", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", transferId);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                cmd.Parameters.AddWithValue("@qty", item.qty);
                                cmd.ExecuteNonQuery();
                            }
                            InsertMutasi(conn, tx, item.kodebarang, dariGudang, "Transfer", -item.qty, nomor,
                                "Transfer keluar " + item.namabarang, user);
                            InsertMutasi(conn, tx, item.kodebarang, keGudang, "Transfer", item.qty, nomor,
                                "Transfer masuk " + item.namabarang, user);
                        }

                        tx.Commit();
                        hasil.sukses = true;
                        hasil.nomor = nomor;
                        hasil.pesan = "Transfer " + nomor + " berhasil disimpan.";
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
            string tanggal = DateTime.Now.ToString("yyyyMMdd");
            string like = prefix + tanggal + "-%";
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
                return prefix + tanggal + "-" + (nomor + 1).ToString("0000");
            }
        }

        private static void InsertMutasi(SqlConnection conn, SqlTransaction tx, string kode, int gudangId,
            string tipe, decimal qty, string referensi, string catatan, string user)
        {
            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbMutasiStok (kodebarang,gudang_id,tipe,qty,referensi,catatan,created_by)
VALUES (@kode,@gudang,@tipe,@qty,@ref,@catatan,@user);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@kode", kode);
                cmd.Parameters.AddWithValue("@gudang", gudangId);
                cmd.Parameters.AddWithValue("@tipe", tipe);
                cmd.Parameters.AddWithValue("@qty", qty);
                cmd.Parameters.AddWithValue("@ref", (object)referensi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@catatan", (object)catatan ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@user", (object)user ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
