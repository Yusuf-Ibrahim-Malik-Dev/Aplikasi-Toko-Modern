using System;
using System.Collections.Generic;
using System.Data;
using CommonClass;

namespace DAAccess
{
    /// <summary>Query dashboard dan laporan monitoring toko.</summary>
    public class DALaporan
    {
        public ClassDashboard GetDashboard()
        {
            ClassDashboard d = new ClassDashboard();
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                DataTable dt = conn.ExecQuery(@"
SELECT
 (SELECT COUNT(1) FROM dbo.tbBarang) AS jumlah_produk,
 (SELECT ISNULL(SUM(stok),0) FROM dbo.tbBarang) AS total_stok,
 (SELECT COUNT(1) FROM dbo.tbBarang WHERE ISNULL(stok,0)>0 AND ISNULL(stok,0)<=ISNULL(stok_min,0)) AS stok_menipis,
 (SELECT COUNT(1) FROM dbo.tbBarang WHERE ISNULL(stok,0)<=0) AS stok_habis,
 (SELECT COUNT(1) FROM dbo.tbFaktur WHERE CAST(tanggal AS DATE)=CAST(GETDATE() AS DATE) AND ISNULL(status,'Posted')<>N'Cancelled') AS transaksi_hari_ini,
 (SELECT ISNULL(SUM(total),0) FROM dbo.tbFaktur WHERE CAST(tanggal AS DATE)=CAST(GETDATE() AS DATE) AND ISNULL(status,'Posted')<>N'Cancelled') AS omzet_hari_ini,
 (SELECT COUNT(1) FROM dbo.tbFaktur WHERE tanggal>=DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1) AND ISNULL(status,'Posted')<>N'Cancelled') AS transaksi_bulan_ini,
 (SELECT ISNULL(SUM(total),0) FROM dbo.tbFaktur WHERE tanggal>=DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1) AND ISNULL(status,'Posted')<>N'Cancelled') AS omzet_bulan_ini,
 (SELECT ISNULL(SUM(nominal-terbayar),0) FROM dbo.tbPiutang WHERE nominal>terbayar) AS total_piutang,
 (SELECT ISNULL(SUM(nominal-terbayar),0) FROM dbo.tbHutang WHERE nominal>terbayar) AS total_hutang,
 (SELECT ISNULL(SUM(nominal),0) FROM dbo.tbKas WHERE CAST(tanggal AS DATE)=CAST(GETDATE() AS DATE) AND tipe=N'Masuk') AS kas_masuk_hari_ini,
 (SELECT ISNULL(SUM(nominal),0) FROM dbo.tbKas WHERE CAST(tanggal AS DATE)=CAST(GETDATE() AS DATE) AND tipe=N'Keluar') AS kas_keluar_hari_ini,
 (SELECT ISNULL(SUM(laba_kotor),0) FROM dbo.vwPenjualanDetail WHERE tanggal>=DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1)) AS laba_bulan_ini");
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    d.jumlah_produk = Convert.ToInt32(r["jumlah_produk"]);
                    d.total_stok = Convert.ToDecimal(r["total_stok"]);
                    d.stok_menipis = Convert.ToInt32(r["stok_menipis"]);
                    d.stok_habis = Convert.ToInt32(r["stok_habis"]);
                    d.transaksi_hari_ini = Convert.ToInt32(r["transaksi_hari_ini"]);
                    d.omzet_hari_ini = Convert.ToDecimal(r["omzet_hari_ini"]);
                    d.transaksi_bulan_ini = Convert.ToInt32(r["transaksi_bulan_ini"]);
                    d.omzet_bulan_ini = Convert.ToDecimal(r["omzet_bulan_ini"]);
                    d.total_piutang = Convert.ToDecimal(r["total_piutang"]);
                    d.total_hutang = Convert.ToDecimal(r["total_hutang"]);
                    d.kas_masuk_hari_ini = Convert.ToDecimal(r["kas_masuk_hari_ini"]);
                    d.kas_keluar_hari_ini = Convert.ToDecimal(r["kas_keluar_hari_ini"]);
                    d.laba_bulan_ini = Convert.ToDecimal(r["laba_bulan_ini"]);
                }
            }
            return d;
        }

        /// <summary>Omzet 7 hari terakhir, hari tanpa transaksi tetap ditampilkan dengan nilai 0.</summary>
        public List<ClassChartPoint> GetPenjualan7Hari()
        {
            List<ClassChartPoint> list = new List<ClassChartPoint>();
            DataTable dt;
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                dt = conn.ExecQuery(@"
SELECT CAST(tanggal AS DATE) AS tanggal,ISNULL(SUM(total),0) AS omzet
FROM dbo.tbFaktur
WHERE tanggal>=DATEADD(DAY,-6,CAST(GETDATE() AS DATE)) AND ISNULL(status,'Posted')<>N'Cancelled'
GROUP BY CAST(tanggal AS DATE)");
            }
            DateTime start = DateTime.Today.AddDays(-6);
            for (int i = 0; i < 7; i++)
            {
                DateTime hari = start.AddDays(i);
                decimal nilai = 0m;
                foreach (DataRow r in dt.Rows)
                {
                    if (Convert.ToDateTime(r["tanggal"]).Date == hari)
                    {
                        nilai = Convert.ToDecimal(r["omzet"]);
                        break;
                    }
                }
                list.Add(new ClassChartPoint(hari.ToString("ddd"), nilai));
            }
            return list;
        }

        /// <summary>Omzet 12 bulan berjalan untuk grafik tren jangka panjang.</summary>
        public List<ClassChartPoint> GetPenjualan12Bulan()
        {
            List<ClassChartPoint> list = new List<ClassChartPoint>();
            DataTable dt;
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                dt = conn.ExecQuery(@"
SELECT YEAR(tanggal) AS tahun,MONTH(tanggal) AS bulan,ISNULL(SUM(total),0) AS omzet
FROM dbo.tbFaktur
WHERE tanggal>=DATEADD(MONTH,-11,DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1)) AND ISNULL(status,'Posted')<>N'Cancelled'
GROUP BY YEAR(tanggal),MONTH(tanggal)");
            }
            DateTime start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11);
            for (int i = 0; i < 12; i++)
            {
                DateTime bulan = start.AddMonths(i);
                decimal nilai = 0m;
                foreach (DataRow r in dt.Rows)
                {
                    if (Convert.ToInt32(r["tahun"]) == bulan.Year && Convert.ToInt32(r["bulan"]) == bulan.Month)
                    {
                        nilai = Convert.ToDecimal(r["omzet"]);
                        break;
                    }
                }
                list.Add(new ClassChartPoint(bulan.ToString("MMM"), nilai));
            }
            return list;
        }

        public DataTable GetProdukTerlaris(int top = 5)
        {
            return Query(@"
SELECT TOP (@0) kodebarang,ISNULL(namabarang,kodebarang) AS namabarang,
       CAST(qty_terjual AS DECIMAL(18,3)) AS qty_terjual,omzet,laba_kotor
FROM dbo.vwProdukTerlaris ORDER BY qty_terjual DESC", new object[] { top });
        }

        public DataTable GetStokKritis(int top = 8)
        {
            return Query(@"
SELECT TOP (@0) kodebarang,namabarang,satuan,hargasatuan,stok,stok_min,status_stok
FROM dbo.vwReportBarang WHERE status_stok<>N'AMAN' ORDER BY stok ASC,namabarang", new object[] { top });
        }

        public DataTable GetTransaksiTerbaru(int top = 8)
        {
            return Query(@"
SELECT TOP (@0) nofaktur,tanggal,ISNULL(pelanggan,N'Umum') AS pelanggan,
       ISNULL(metode_bayar,N'-') AS metode_bayar,total
FROM dbo.vwReportPenjualan ORDER BY tanggal DESC,nofaktur DESC", new object[] { top });
        }

        public DataTable GetLaporanPenjualan(DateTime mulai, DateTime akhir, string keyword = null)
        {
            return Query(@"
SELECT nofaktur,tanggal,ISNULL(pelanggan,N'Umum') AS pelanggan,ISNULL(metode_bayar,N'-') AS metode_bayar,
       diskon,total,dibayar,kembali,ISNULL(kasir,N'-') AS kasir,status
FROM dbo.vwReportPenjualan
WHERE CAST(tanggal AS DATE) BETWEEN @0 AND @1
  AND (@2 IS NULL OR @2=N'' OR nofaktur LIKE N'%'+@2+N'%' OR ISNULL(pelanggan,N'') LIKE N'%'+@2+N'%')
ORDER BY tanggal DESC,nofaktur DESC", new object[] { mulai.Date, akhir.Date, keyword });
        }

        public DataTable GetLaporanPembelian(DateTime mulai, DateTime akhir)
        {
            return Query(@"
SELECT nomor,tanggal,ISNULL(supplier,N'Umum') AS supplier,status,subtotal,diskon,total,dibayar,sisa_pembayaran
FROM dbo.vwReportPembelian WHERE CAST(tanggal AS DATE) BETWEEN @0 AND @1
ORDER BY tanggal DESC,nomor DESC", new object[] { mulai.Date, akhir.Date });
        }

        public DataTable GetLaporanStok(bool hanyaKritis)
        {
            return Query(@"
SELECT kodebarang,namabarang,satuan,hargasatuan,harga_beli,stok,stok_min,status_stok
FROM dbo.vwReportBarang WHERE (@0=0 OR status_stok<>N'AMAN') ORDER BY namabarang",
                new object[] { hanyaKritis ? 1 : 0 });
        }

        public DataTable GetLaporanKas(DateTime mulai, DateTime akhir)
        {
            return Query(@"
SELECT tanggal,tipe,kategori,nominal,ISNULL(metode_bayar,N'-') AS metode_bayar,
       ISNULL(referensi,N'-') AS referensi,ISNULL(keterangan,N'') AS keterangan
FROM dbo.tbKas WHERE CAST(tanggal AS DATE) BETWEEN @0 AND @1 ORDER BY tanggal DESC,kas_id DESC",
                new object[] { mulai.Date, akhir.Date });
        }

        public DataTable GetLaporanHarian(DateTime mulai, DateTime akhir)
        {
            return Query(@"
SELECT tanggal,jumlah_transaksi,omzet,diskon FROM dbo.vwLaporanHarianPenjualan
WHERE tanggal BETWEEN @0 AND @1 ORDER BY tanggal DESC", new object[] { mulai.Date, akhir.Date });
        }

        public DataTable GetLaporanLaba(DateTime mulai, DateTime akhir)
        {
            return Query(@"
SELECT CAST(d.tanggal AS DATE) AS tanggal,COUNT(DISTINCT d.nofaktur) AS transaksi,
       SUM(d.total) AS penjualan,SUM(d.harga_beli*d.qty) AS hpp,SUM(d.laba_kotor) AS laba_kotor
FROM dbo.vwPenjualanDetail d
INNER JOIN dbo.tbFaktur f ON f.nofaktur=RTRIM(d.nofaktur)
WHERE CAST(d.tanggal AS DATE) BETWEEN @0 AND @1 AND ISNULL(f.status,'Posted')<>N'Cancelled'
GROUP BY CAST(d.tanggal AS DATE) ORDER BY CAST(d.tanggal AS DATE) DESC",
                new object[] { mulai.Date, akhir.Date });
        }

        public DataTable GetLaporanProduk(DateTime mulai, DateTime akhir)
        {
            return Query(@"
SELECT d.kodebarang,ISNULL(d.namabarang,d.kodebarang) AS namabarang,
       SUM(d.qty) AS qty_terjual,SUM(d.total) AS omzet,SUM(d.laba_kotor) AS laba_kotor
FROM dbo.vwPenjualanDetail d
INNER JOIN dbo.tbFaktur f ON f.nofaktur=RTRIM(d.nofaktur)
WHERE CAST(d.tanggal AS DATE) BETWEEN @0 AND @1 AND ISNULL(f.status,'Posted')<>N'Cancelled'
GROUP BY d.kodebarang,d.namabarang ORDER BY qty_terjual DESC",
                new object[] { mulai.Date, akhir.Date });
        }

        public DataTable GetLaporanHutang()
        {
            return Query("SELECT * FROM dbo.vwReportHutang ORDER BY jatuh_tempo,hutang_id DESC");
        }

        public DataTable GetLaporanPiutang()
        {
            return Query("SELECT * FROM dbo.vwReportPiutang ORDER BY jatuh_tempo,piutang_id DESC");
        }

        private DataTable Query(string sql, object[] par = null)
        {
            using (var conn = new DbConnection(dbConfiguration.ConnectionString))
            {
                return conn.ExecQuery(sql, par);
            }
        }
    }
}
