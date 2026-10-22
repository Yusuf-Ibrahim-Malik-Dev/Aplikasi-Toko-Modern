
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CommonClass;

namespace DAAccess
{
    public class DAPenjualan
    {
        // =========================================================
        // GENERATE NOMOR FAKTUR
        // Stored Procedure: proc_generatefakturno
        // =========================================================
        public string generatenofaktur()
        {
            string msg = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    conn.ExecReader("proc_generatefakturno");

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("hasil");
                    }
                }

                return msg;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // INSERT TEMPORARY ISI FAKTUR
        // Stored Procedure: proc_inserttempisifaktur
        // =========================================================
        public string insterttemp(ClassTempIsiFaktur obj)
        {
            string keluaran = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[6]
                    {
                        obj.nofaktur,
                        obj.kodebarang,
                        obj.namabarang,
                        obj.hargasatuan,
                        obj.jumlahbeli,
                        obj.total
                    };

                    conn.ExecReader(
                        "proc_inserttempisifaktur @0, @1, @2, @3, @4, @5",
                        par
                    );

                    while (conn.HasRow())
                    {
                        keluaran = conn.GetFieldValue("hasil");
                    }
                }

                return keluaran;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // GET DATA BARANG BERDASARKAN KODE
        // Stored Procedure: proc_getdatabykode
        // =========================================================
        public ClassBarang getdatabarangbykode(string kodebarang)
        {
            ClassBarang data = new ClassBarang();

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[1]
                    {
                        kodebarang
                    };

                    conn.ExecReader(
                        "proc_getdatabykode @0",
                        par
                    );

                    while (conn.HasRow())
                    {
                        data.kodebarang =
                            conn.GetFieldValue("kodebarang");

                        data.namabarang =
                            conn.GetFieldValue("namabarang");

                        data.hargasatuan =
                            conn.GetFieldValue<decimal>("hargasatuan");

                        data.satuan =
                            conn.GetFieldValue("satuan");
                    }
                }

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // SUBMIT FAKTUR
        // Stored Procedure: proc_insertfaktur
        // =========================================================
        public string submitfaktur(ClassFaktur obj)
        {
            string msg = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[2]
                    {
                        obj.nofaktur,
                        obj.total
                    };

                    conn.ExecReader(
                        "proc_insertfaktur @0, @1",
                        par
                    );

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("hasil");
                    }
                }

                return msg;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // SUBMIT ISI FAKTUR
        // Stored Procedure: proc_insertisifaktur
        // =========================================================
        public string submitisifaktur(ClassIsiFaktur obj)
        {
            string msg = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[6]
                    {
                        obj.nofaktur,
                        obj.kodebarang,
                        obj.namabarang,
                        obj.hargasatuan,
                        obj.jumlah,
                        obj.total
                    };

                    conn.ExecReader(
                        "proc_insertisifaktur @0, @1, @2, @3, @4, @5",
                        par
                    );

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("hasil");
                    }
                }

                return msg;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // DELETE TEMPORARY FAKTUR
        // Stored Procedure: proc_deleteFakturTemporary
        // =========================================================
        public string deletetemporary()
        {
            string msg = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    conn.ExecReader("proc_deleteFakturTemporary");

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("hasil");
                    }
                }

                return msg;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // GET ISI GRID FAKTUR
        // Stored Procedure: proc_getisigrid
        // =========================================================
        public List<ClassTempIsiFaktur> getisigrid(string nofaktur)
        {
            List<ClassTempIsiFaktur> list =
                new List<ClassTempIsiFaktur>();

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[1]
                    {
                        nofaktur
                    };

                    conn.ExecReader(
                        "proc_getisigrid @0",
                        par
                    );

                    while (conn.HasRow())
                    {
                        ClassTempIsiFaktur data =
                            new ClassTempIsiFaktur();

                        data.nofaktur =
                            conn.GetFieldValue("nofaktur");

                        data.kodebarang =
                            conn.GetFieldValue("kodebarang");

                        data.namabarang =
                            conn.GetFieldValue("namabarang");

                        data.hargasatuan =
                            conn.GetFieldValue<decimal>("hargasatuan");

                        data.jumlahbeli =
                            conn.GetFieldValue<short>("jumlahbeli");

                        data.total =
                            conn.GetFieldValue<decimal>("total");

                        list.Add(data);
                    }
                }

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // SUBMIT FAKTUR JUMLAH PEMBAYARAN
        // Stored Procedure: proc_insertfakturjumlah
        // =========================================================
        public string submitfakturjumlah(ClassIsiFakturJumlah obj)
        {
            string msg = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[3]
                    {
                        obj.nofaktur,
                        obj.jumlahuang,
                        obj.kembali
                    };

                    conn.ExecReader(
                        "exec proc_insertfakturjumlah @0, @1, @2",
                        par
                    );

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("hasil");
                    }
                }

                return msg;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // MODUL POS MODERN (TRANSAKSI ATOMIK)
        // =========================================================
        public DataTable GetProdukCari(string keyword)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT b.kodebarang,b.namabarang,ISNULL(b.barcode,N'') AS barcode,
       ISNULL(s.nama,b.satuan) AS satuan,ISNULL(b.harga_beli,0) AS harga_beli,
       b.hargasatuan,CAST(ISNULL(b.stok,0) AS DECIMAL(18,3)) AS stok,
       CASE WHEN ISNULL(b.stok,0)<=0 THEN N'HABIS'
            WHEN ISNULL(b.stok,0)<=ISNULL(b.stok_min,0) THEN N'MENIPIS' ELSE N'AMAN' END AS status_stok
FROM dbo.tbBarang b
LEFT JOIN dbo.tbSatuan s ON s.satuan_id=b.satuan_id
WHERE (@0 IS NULL OR @0=N'' OR b.kodebarang LIKE N'%'+@0+N'%'
       OR b.namabarang LIKE N'%'+@0+N'%' OR ISNULL(b.barcode,N'') LIKE N'%'+@0+N'%')
ORDER BY b.namabarang", new object[] { keyword });
            }
        }

        /// <summary>
        /// Menyimpan penjualan, detail, pembayaran, mutasi stok, kas, dan piutang
        /// dalam satu transaksi database. Jika satu langkah gagal, semua dibatalkan.
        /// </summary>
        public ClassSimpanHasil SimpanPenjualan(ClassPenjualan header, List<ClassPenjualanDetail> items)
        {
            if (header == null) throw new ArgumentNullException("header");
            if (items == null || items.Count == 0) throw new InvalidOperationException("Keranjang masih kosong.");

            ClassSimpanHasil hasil = new ClassSimpanHasil();
            using (SqlConnection conn = dbConfiguration.createConection())
            {
                conn.Open();
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        string nofaktur = GenerateFakturNo(conn, tx);
                        decimal subtotal = 0m;
                        foreach (ClassPenjualanDetail item in items)
                        {
                            if (item.qty <= 0) throw new InvalidOperationException("Jumlah barang harus lebih dari nol.");
                            if (item.hargasatuan < 0 || item.diskon < 0) throw new InvalidOperationException("Harga/diskon tidak valid.");
                            subtotal += item.total;
                        }
                        if (header.diskon < 0 || header.diskon > subtotal)
                            throw new InvalidOperationException("Diskon transaksi tidak valid.");
                        decimal total = subtotal - header.diskon;
                        if (total <= 0) throw new InvalidOperationException("Total transaksi harus lebih dari nol.");
                        bool kredit = string.Equals(header.metode_bayar, "Kredit", StringComparison.OrdinalIgnoreCase);
                        if (!kredit && header.dibayar < total)
                            throw new InvalidOperationException("Jumlah pembayaran kurang dari total transaksi.");
                        if (kredit && header.pelanggan_id.GetValueOrDefault() <= 0)
                            throw new InvalidOperationException("Pelanggan wajib dipilih untuk transaksi kredit.");
                        decimal dibayar = header.dibayar;
                        if (dibayar > total && !kredit) dibayar = total;

                        using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbFaktur (nofaktur,tanggal,total,diskon,dibayar,kembali,metode_bayar,pelanggan_id,kasir,catatan,status)
VALUES (@no,@tgl,@total,@diskon,@bayar,@kembali,@metode,@pelanggan,@kasir,@catatan,N'Posted');", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@no", nofaktur);
                            cmd.Parameters.AddWithValue("@tgl", header.tanggal == default(DateTime) ? DateTime.Now : header.tanggal);
                            cmd.Parameters.AddWithValue("@total", total);
                            cmd.Parameters.AddWithValue("@diskon", header.diskon);
                            cmd.Parameters.AddWithValue("@bayar", dibayar);
                            cmd.Parameters.AddWithValue("@kembali", Math.Max(0, dibayar - total));
                            cmd.Parameters.AddWithValue("@metode", (object)header.metode_bayar ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@pelanggan", (object)header.pelanggan_id ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@kasir", (object)header.kasir ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@catatan", (object)header.catatan ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        foreach (ClassPenjualanDetail item in items)
                        {
                            decimal stokSekarang;
                            decimal hargaBeli;
                            using (SqlCommand cek = new SqlCommand(
                                "SELECT CAST(ISNULL(stok,0) AS DECIMAL(18,3)),ISNULL(harga_beli,0) FROM dbo.tbBarang WITH (UPDLOCK,HOLDLOCK) WHERE kodebarang=@kode", conn, tx))
                            {
                                cek.Parameters.AddWithValue("@kode", item.kodebarang);
                                using (SqlDataReader rd = cek.ExecuteReader())
                                {
                                    if (!rd.Read())
                                        throw new InvalidOperationException("Barang " + item.kodebarang + " tidak ditemukan.");
                                    stokSekarang = Convert.ToDecimal(rd[0]);
                                    hargaBeli = Convert.ToDecimal(rd[1]);
                                }
                            }
                            if (stokSekarang < item.qty)
                                throw new InvalidOperationException("Stok " + item.namabarang + " tidak cukup. Sisa " + stokSekarang.ToString("0.###") + ".");

                            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tblsifaktur (nofaktur,kodebarang,namabarang,hargasatuan,jumlahbeli,total,diskon,harga_beli)
VALUES (@no,@kode,LEFT(@nama,50),@harga,@qty,@total,@diskon,@beli);", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@no", nofaktur);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                cmd.Parameters.AddWithValue("@nama", item.namabarang ?? item.kodebarang);
                                cmd.Parameters.AddWithValue("@harga", item.hargasatuan);
                                cmd.Parameters.AddWithValue("@qty", item.qty);
                                cmd.Parameters.AddWithValue("@total", item.total);
                                cmd.Parameters.AddWithValue("@diskon", item.diskon);
                                cmd.Parameters.AddWithValue("@beli", hargaBeli);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                "UPDATE dbo.tbBarang SET stok=stok-@qty WHERE kodebarang=@kode", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@qty", item.qty);
                                cmd.Parameters.AddWithValue("@kode", item.kodebarang);
                                cmd.ExecuteNonQuery();
                            }

                            InsertMutasi(conn, tx, item.kodebarang, "Keluar", -item.qty, nofaktur,
                                "Penjualan " + item.namabarang, header.kasir);
                        }

                        if (dibayar > 0)
                        {
                            InsertKas(conn, tx, "Masuk", "Penjualan", dibayar, header.metode_bayar,
                                nofaktur, "Pembayaran penjualan " + nofaktur, header.kasir);
                        }

                        decimal sisa = total - dibayar;
                        if (sisa > 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.tbPiutang (pelanggan_id,nomor_penjualan,tanggal,jatuh_tempo,nominal,terbayar,status)
VALUES (@pelanggan,@no,@tgl,DATEADD(DAY,30,@tgl),@nominal,0,N'Belum Lunas');
UPDATE dbo.tbPelanggan SET saldo_piutang=ISNULL(saldo_piutang,0)+@nominal WHERE pelanggan_id=@pelanggan;", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@pelanggan", (object)header.pelanggan_id ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@no", nofaktur);
                                cmd.Parameters.AddWithValue("@tgl", header.tanggal == default(DateTime) ? DateTime.Now : header.tanggal);
                                cmd.Parameters.AddWithValue("@nominal", sisa);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        hasil.sukses = true;
                        hasil.nomor = nofaktur;
                        hasil.pesan = "Transaksi " + nofaktur + " berhasil disimpan.";
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

        public DataTable GetRiwayatPenjualan(DateTime mulai, DateTime akhir, string keyword = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT f.nofaktur,f.tanggal,ISNULL(c.nama,N'Umum') AS pelanggan,
       ISNULL(f.metode_bayar,N'-') AS metode_bayar,f.total,f.dibayar,f.kembali,
       ISNULL(f.kasir,N'-') AS kasir,f.status
FROM dbo.tbFaktur f LEFT JOIN dbo.tbPelanggan c ON c.pelanggan_id=f.pelanggan_id
WHERE CAST(f.tanggal AS DATE) BETWEEN @0 AND @1
  AND (@2 IS NULL OR @2=N'' OR f.nofaktur LIKE N'%'+@2+N'%' OR ISNULL(c.nama,N'') LIKE N'%'+@2+N'%')
ORDER BY f.tanggal DESC,f.nofaktur DESC", new object[] { mulai.Date, akhir.Date, keyword });
            }
        }

        public DataTable GetDetailPenjualan(string nofaktur)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(@"
SELECT RTRIM(d.nofaktur) AS nofaktur,d.kodebarang,b.namabarang,d.hargasatuan,
       d.jumlahbeli AS qty,d.diskon,d.total,(d.hargasatuan-d.harga_beli)*d.jumlahbeli-d.diskon AS laba_kotor
FROM dbo.tblsifaktur d LEFT JOIN dbo.tbBarang b ON b.kodebarang=d.kodebarang
WHERE RTRIM(d.nofaktur)=@0 ORDER BY d.kodebarang", new object[] { nofaktur });
            }
        }

        // =========================================================
        // HELPER PRIVATE
        // =========================================================
        private static string GenerateFakturNo(SqlConnection conn, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(MAX(nofaktur),'FK00000') FROM dbo.tbFaktur WITH (UPDLOCK,HOLDLOCK)", conn, tx))
            {
                string last = Convert.ToString(cmd.ExecuteScalar());
                int nomor;
                if (!int.TryParse(last.Length >= 5 ? last.Substring(last.Length - 5) : "0", out nomor))
                    nomor = 0;
                nomor++;
                if (nomor > 99999) throw new InvalidOperationException("Nomor faktur sudah mencapai batas maksimum.");
                return "FK" + nomor.ToString("00000");
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
