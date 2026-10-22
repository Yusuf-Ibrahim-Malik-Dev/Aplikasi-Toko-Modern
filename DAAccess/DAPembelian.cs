using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CommonClass;

namespace DAAccess
{
    /// <summary>Data access transaksi pembelian, penerimaan barang, hutang, dan kas keluar.</summary>
    public class DAPembelian
    {
        public string GenerateNomor()
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                DataTable dt = conn.ExecQuery("EXEC dbo.usp_GenerateNomor @0,@1,@2",
                    new object[] { "PB", "tbPembelian", "nomor" });
                if (dt.Rows.Count > 0) return Convert.ToString(dt.Rows[0]["nomor"]);
                return "PB" + DateTime.Now.ToString("yyyyMMdd") + "-0001";
            }
        }

        public DataTable GetProdukCari(string keyword)
        {
            return new DAPenjualan().GetProdukCari(keyword);
        }

        public DataTable GetRiwayatPembelian(DateTime mulai, DateTime akhir, string keyword = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT p.pembelian_id,p.nomor,p.tanggal,ISNULL(s.nama,N'Umum') AS supplier,p.status,
       p.subtotal,p.diskon,p.total,p.dibayar,(p.total-p.dibayar) AS sisa,ISNULL(p.created_by,N'-') AS created_by
FROM dbo.tbPembelian p LEFT JOIN dbo.tbSupplier s ON s.supplier_id=p.supplier_id
WHERE CAST(p.tanggal AS DATE) BETWEEN @0 AND @1
  AND (@2 IS NULL OR @2=N'' OR p.nomor LIKE N'%'+@2+N'%' OR ISNULL(s.nama,N'') LIKE N'%'+@2+N'%')
ORDER BY p.tanggal DESC,p.pembelian_id DESC", new object[] { mulai.Date, akhir.Date, keyword });
            }
        }

        public DataTable GetDetailPembelian(long pembelianId)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT d.kodebarang,b.namabarang,d.qty,d.harga_beli,d.diskon,d.total
FROM dbo.tbIsiPembelian d LEFT JOIN dbo.tbBarang b ON b.kodebarang=d.kodebarang
WHERE d.pembelian_id=@0 ORDER BY d.detail_id", new object[] { pembelianId });
            }
        }

        public ClassSimpanHasil SimpanPembelian(ClassPembelian header, List<ClassPembelianDetail> items)
        {
            if (header == null) throw new ArgumentNullException("header");
            if (items == null || items.Count == 0) throw new InvalidOperationException("Detail pembelian masih kosong.");

            ClassSimpanHasil hasil = new ClassSimpanHasil();
            using (SqlConnection conn = dbConfiguration.createConection())
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string nomor = string.IsNullOrWhiteSpace(header.nomor) ? GenerateNomor(conn, tx) : header.nomor;
                        decimal subtotal = 0m;
                        foreach (ClassPembelianDetail item in items)
                        {
                            if (item.qty <= 0) throw new InvalidOperationException("Qty pembelian harus lebih dari nol.");
                            if (item.harga_beli < 0 || item.diskon < 0) throw new InvalidOperationException("Harga/diskon tidak valid.");
                            subtotal += item.total;
                        }
                        if (header.diskon < 0 || header.diskon > subtotal)
                            throw new InvalidOperationException("Diskon pembelian tidak valid.");
                        decimal total = subtotal - header.diskon;
                        if (total < 0) throw new InvalidOperationException("Total pembelian tidak valid.");
                        decimal dibayar = Math.Max(0, Math.Min(header.dibayar, total));
                        string status = string.IsNullOrWhiteSpace(header.status) ? "Received" : header.status;

                        long pembelianId;
                        using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbPembelian (nomor,tanggal,supplier_id,status,subtotal,diskon,total,dibayar,catatan,created_by)
VALUES (@nomor,@tgl,@supplier,@status,@subtotal,@diskon,@total,@bayar,@catatan,@user);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@nomor", nomor);
                            cmd.Parameters.AddWithValue("@tgl", header.tanggal == default(DateTime) ? DateTime.Now : header.tanggal);
                            cmd.Parameters.AddWithValue("@supplier", (object)header.supplier_id ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@status", status);
                            cmd.Parameters.AddWithValue("@subtotal", subtotal);
                            cmd.Parameters.AddWithValue("@diskon", header.diskon);
                            cmd.Parameters.AddWithValue("@total", total);
                            cmd.Parameters.AddWithValue("@bayar", dibayar);
                            cmd.Parameters.AddWithValue("@catatan", (object)header.catatan ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@user", (object)header.created_by ?? DBNull.Value);
                            pembelianId = Convert.ToInt64(cmd.ExecuteScalar());
                        }

                        foreach (ClassPembelianDetail item in items)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbIsiPembelian (pembelian_id,kodebarang,qty,harga_beli,diskon)
VALUES (@id,@kode,@qty,@harga,@diskon);", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", pembelianId);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                cmd.Parameters.AddWithValue("@qty", item.qty);
                                cmd.Parameters.AddWithValue("@harga", item.harga_beli);
                                cmd.Parameters.AddWithValue("@diskon", item.diskon);
                                cmd.ExecuteNonQuery();
                            }

                            if (status == "Received")
                            {
                                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.tbBarang SET stok=ISNULL(stok,0)+@qty,harga_beli=@harga WHERE kodebarang=@kode;", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@qty", item.qty);
                                    cmd.Parameters.AddWithValue("@harga", item.harga_beli);
                                    cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                    if (cmd.ExecuteNonQuery() == 0)
                                        throw new InvalidOperationException("Barang " + item.kodebarang + " tidak ditemukan.");
                                }
                                InsertMutasi(conn, tx, item.kodebarang, "Masuk", item.qty, nomor,
                                    "Pembelian " + item.namabarang, header.created_by);
                            }
                        }

                        if (dibayar > 0)
                            InsertKas(conn, tx, "Keluar", "Pembelian", dibayar, "Tunai", nomor,
                                "Pembayaran pembelian " + nomor, header.created_by);

                        decimal sisa = total - dibayar;
                        if (sisa > 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbHutang (supplier_id,pembelian_id,tanggal,jatuh_tempo,nominal,terbayar,status)
VALUES (@supplier,@id,@tgl,DATEADD(DAY,30,@tgl),@nominal,0,N'Belum Lunas');
IF @supplier IS NOT NULL UPDATE dbo.tbSupplier SET saldo_hutang=ISNULL(saldo_hutang,0)+@nominal WHERE supplier_id=@supplier;", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@supplier", (object)header.supplier_id ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@id", pembelianId);
                                cmd.Parameters.AddWithValue("@tgl", header.tanggal == default(DateTime) ? DateTime.Now : header.tanggal);
                                cmd.Parameters.AddWithValue("@nominal", sisa);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        hasil.sukses = true;
                        hasil.nomor = nomor;
                        hasil.pesan = "Pembelian " + nomor + " berhasil disimpan.";
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

        private static string GenerateNomor(SqlConnection conn, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(MAX(nomor),'') FROM dbo.tbPembelian WITH (UPDLOCK,HOLDLOCK) WHERE nomor LIKE 'PB'+CONVERT(CHAR(8),GETDATE(),112)+'%'", conn, tx))
            {
                string last = Convert.ToString(cmd.ExecuteScalar());
                int nomor = 0;
                if (!string.IsNullOrWhiteSpace(last))
                {
                    int idx = last.LastIndexOf('-');
                    if (idx >= 0) int.TryParse(last.Substring(idx + 1), out nomor);
                }
                return "PB" + DateTime.Now.ToString("yyyyMMdd") + "-" + (nomor + 1).ToString("0000");
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
                cmd.Parameters.AddWithValue("@ref", (object)referensi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@catatan", (object)catatan ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@user", (object)user ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertKas(SqlConnection conn, SqlTransaction tx, string tipe, string kategori,
            decimal nominal, string metode, string referensi, string keterangan, string user)
        {
            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbKas (tipe,kategori,nominal,metode_bayar,referensi,keterangan,created_by)
VALUES (@tipe,@kategori,@nominal,@metode,@ref,@ket,@user);", conn, tx))
            {
                cmd.Parameters.AddWithValue("@tipe", tipe);
                cmd.Parameters.AddWithValue("@kategori", kategori);
                cmd.Parameters.AddWithValue("@nominal", nominal);
                cmd.Parameters.AddWithValue("@metode", (object)metode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ref", (object)referensi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ket", (object)keterangan ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@user", (object)user ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
