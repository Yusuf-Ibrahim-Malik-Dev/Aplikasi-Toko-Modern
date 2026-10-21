/* Toko-Root-ID - Query tambahan Views & Stored Procedures
   Database: dbToko | SQL Server 2016+
   Urutan: jalankan wave1_foundation_auth_rbac.sql (bila belum), wave2_6_erp_pos.sql,
   lalu file ini. Skrip aman dijalankan ulang untuk CREATE/ALTER VIEW/PROCEDURE.
*/
USE dbToko;
GO

/* ========================= VIEWS ========================= */
CREATE OR ALTER VIEW dbo.vwReportBarang AS
SELECT b.kodebarang, b.namabarang, b.satuan, b.hargasatuan,
       CAST(ISNULL(b.stok, 0) AS DECIMAL(18,3)) AS stok,
       CAST(ISNULL(b.stok_min, 0) AS DECIMAL(18,3)) AS stok_min,
       CAST(ISNULL(b.harga_beli, 0) AS DECIMAL(18,2)) AS harga_beli,
       CASE WHEN ISNULL(b.stok,0) <= ISNULL(b.stok_min,0) THEN N'MENIPIS' ELSE N'AMAN' END AS status_stok
FROM dbo.tbBarang b;
GO

CREATE OR ALTER VIEW dbo.vwReportPembelian AS
SELECT p.pembelian_id, p.nomor, p.tanggal, s.nama AS supplier,
       p.status, p.subtotal, p.diskon, p.total, p.dibayar,
       CASE WHEN p.total > p.dibayar THEN p.total-p.dibayar ELSE 0 END AS sisa_pembayaran
FROM dbo.tbPembelian p LEFT JOIN dbo.tbSupplier s ON s.supplier_id=p.supplier_id;
GO

CREATE OR ALTER VIEW dbo.vwReportHutang AS
SELECT h.hutang_id, s.nama AS supplier, h.tanggal, h.jatuh_tempo, h.nominal,
       h.terbayar, CASE WHEN h.nominal>h.terbayar THEN h.nominal-h.terbayar ELSE 0 END AS sisa,
       h.status FROM dbo.tbHutang h LEFT JOIN dbo.tbSupplier s ON s.supplier_id=h.supplier_id;
GO

CREATE OR ALTER VIEW dbo.vwReportPiutang AS
SELECT p.piutang_id, c.nama AS pelanggan, p.nomor_penjualan, p.tanggal, p.jatuh_tempo,
       p.nominal, p.terbayar, CASE WHEN p.nominal>p.terbayar THEN p.nominal-p.terbayar ELSE 0 END AS sisa,
       p.status FROM dbo.tbPiutang p LEFT JOIN dbo.tbPelanggan c ON c.pelanggan_id=p.pelanggan_id;
GO

CREATE OR ALTER VIEW dbo.vwReportKasHarian AS
SELECT CAST(tanggal AS DATE) AS tanggal,
       SUM(CASE WHEN tipe=N'Masuk' THEN nominal ELSE 0 END) AS kas_masuk,
       SUM(CASE WHEN tipe=N'Keluar' THEN nominal ELSE 0 END) AS kas_keluar,
       SUM(CASE WHEN tipe=N'Masuk' THEN nominal ELSE -nominal END) AS saldo_bersih
FROM dbo.tbKas GROUP BY CAST(tanggal AS DATE);
GO

CREATE OR ALTER VIEW dbo.vwReportMutasiStok AS
SELECT m.mutasi_id,m.tanggal,m.kodebarang,b.namabarang,m.gudang_id,g.nama AS gudang,
       m.tipe,m.qty,m.referensi,m.catatan,m.created_by
FROM dbo.tbMutasiStok m LEFT JOIN dbo.tbBarang b ON b.kodebarang=m.kodebarang
LEFT JOIN dbo.tbGudang g ON g.gudang_id=m.gudang_id;
GO

/* Penjualan lama berbeda-beda antar versi database. View ini mendeteksi tbFaktur.
   Jika tabel/kolom penjualan lama tidak ada, view mengembalikan schema kosong, bukan gagal build. */
IF OBJECT_ID(N'dbo.vwReportPenjualan',N'V') IS NOT NULL DROP VIEW dbo.vwReportPenjualan;
IF OBJECT_ID(N'dbo.tbFaktur',N'U') IS NOT NULL
   AND COL_LENGTH('dbo.tbFaktur','nofaktur') IS NOT NULL
   AND COL_LENGTH('dbo.tbFaktur','tanggal') IS NOT NULL
   AND COL_LENGTH('dbo.tbFaktur','total') IS NOT NULL
    EXEC(N'CREATE VIEW dbo.vwReportPenjualan AS SELECT nofaktur, tanggal, total FROM dbo.tbFaktur');
ELSE
    EXEC(N'CREATE VIEW dbo.vwReportPenjualan AS SELECT CAST(NULL AS NVARCHAR(40)) AS nofaktur, CAST(NULL AS DATETIME) AS tanggal, CAST(NULL AS DECIMAL(18,2)) AS total WHERE 1=0');
GO

/* ========================= STORED PROCEDURES ========================= */
CREATE OR ALTER PROCEDURE dbo.usp_ProdukCari
    @kata NVARCHAR(100)=NULL,
    @stokMenipis BIT=0
AS
BEGIN
 SET NOCOUNT ON;
 SELECT * FROM dbo.vwReportBarang
 WHERE (@kata IS NULL OR kodebarang LIKE N'%'+@kata+N'%' OR namabarang LIKE N'%'+@kata+N'%' OR ISNULL((SELECT TOP 1 barcode FROM dbo.tbBarang x WHERE x.kodebarang=vwReportBarang.kodebarang),N'') LIKE N'%'+@kata+N'%')
   AND (@stokMenipis=0 OR status_stok=N'MENIPIS')
 ORDER BY namabarang;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DashboardRingkasan
    @tanggalMulai DATE=NULL,
    @tanggalAkhir DATE=NULL
AS
BEGIN
 SET NOCOUNT ON;
 IF @tanggalMulai IS NULL SET @tanggalMulai=CONVERT(DATE,GETDATE());
 IF @tanggalAkhir IS NULL SET @tanggalAkhir=@tanggalMulai;
 SELECT
   (SELECT COUNT_BIG(*) FROM dbo.tbBarang) AS jumlah_produk,
   (SELECT COUNT_BIG(*) FROM dbo.vwReportBarang WHERE status_stok=N'MENIPIS') AS stok_menipis,
   (SELECT COUNT_BIG(*) FROM dbo.tbPembelian WHERE tanggal>=@tanggalMulai AND tanggal<DATEADD(DAY,1,@tanggalAkhir)) AS jumlah_pembelian,
   (SELECT ISNULL(SUM(total),0) FROM dbo.tbPembelian WHERE tanggal>=@tanggalMulai AND tanggal<DATEADD(DAY,1,@tanggalAkhir) AND status<>N'Cancelled') AS total_pembelian,
   (SELECT ISNULL(SUM(nominal),0) FROM dbo.tbKas WHERE tanggal>=@tanggalMulai AND tanggal<DATEADD(DAY,1,@tanggalAkhir) AND tipe=N'Masuk') AS kas_masuk,
   (SELECT ISNULL(SUM(nominal),0) FROM dbo.tbKas WHERE tanggal>=@tanggalMulai AND tanggal<DATEADD(DAY,1,@tanggalAkhir) AND tipe=N'Keluar') AS kas_keluar,
   (SELECT ISNULL(SUM(nominal-terbayar),0) FROM dbo.tbHutang WHERE nominal>terbayar) AS total_hutang,
   (SELECT ISNULL(SUM(nominal-terbayar),0) FROM dbo.tbPiutang WHERE nominal>terbayar) AS total_piutang,
   (SELECT COUNT_BIG(*) FROM dbo.vwReportPenjualan WHERE tanggal>=@tanggalMulai AND tanggal<DATEADD(DAY,1,@tanggalAkhir)) AS jumlah_transaksi,
   (SELECT ISNULL(SUM(total),0) FROM dbo.vwReportPenjualan WHERE tanggal>=@tanggalMulai AND tanggal<DATEADD(DAY,1,@tanggalAkhir)) AS omzet;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReportPenjualan
 @tanggalMulai DATE=NULL, @tanggalAkhir DATE=NULL
AS
BEGIN
 SET NOCOUNT ON;
 SELECT nofaktur,tanggal,total FROM dbo.vwReportPenjualan
 WHERE (@tanggalMulai IS NULL OR tanggal>=@tanggalMulai)
   AND (@tanggalAkhir IS NULL OR tanggal<DATEADD(DAY,1,@tanggalAkhir))
 ORDER BY tanggal DESC,nofaktur DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReportPembelian
 @tanggalMulai DATE=NULL, @tanggalAkhir DATE=NULL, @supplierId INT=NULL
AS
BEGIN
 SET NOCOUNT ON;
 SELECT * FROM dbo.vwReportPembelian
 WHERE (@tanggalMulai IS NULL OR tanggal>=@tanggalMulai)
   AND (@tanggalAkhir IS NULL OR tanggal<DATEADD(DAY,1,@tanggalAkhir))
   AND (@supplierId IS NULL OR EXISTS(SELECT 1 FROM dbo.tbPembelian p WHERE p.pembelian_id=vwReportPembelian.pembelian_id AND p.supplier_id=@supplierId))
 ORDER BY tanggal DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReportStokKritis
AS
BEGIN
 SET NOCOUNT ON;
 SELECT * FROM dbo.vwReportBarang WHERE status_stok=N'MENIPIS' ORDER BY stok ASC,namabarang;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReportArusKas
 @tanggalMulai DATE=NULL, @tanggalAkhir DATE=NULL
AS
BEGIN
 SET NOCOUNT ON;
 SELECT CAST(tanggal AS DATE) AS tanggal, tipe, kategori, nominal, metode_bayar, referensi, keterangan, created_by
 FROM dbo.tbKas WHERE (@tanggalMulai IS NULL OR tanggal>=@tanggalMulai)
 AND (@tanggalAkhir IS NULL OR tanggal<DATEADD(DAY,1,@tanggalAkhir)) ORDER BY tanggal DESC,kas_id DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ProdukSimpan
 @kodebarang NVARCHAR(50), @namabarang NVARCHAR(150), @satuan NVARCHAR(50),
 @hargasatuan DECIMAL(18,2), @stok DECIMAL(18,3)=0, @stok_min DECIMAL(18,3)=0,
 @harga_beli DECIMAL(18,2)=0, @barcode NVARCHAR(100)=NULL
AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM dbo.tbBarang WHERE kodebarang=@kodebarang)
  UPDATE dbo.tbBarang SET namabarang=@namabarang,satuan=@satuan,hargasatuan=@hargasatuan,stok=@stok,stok_min=@stok_min,harga_beli=@harga_beli,barcode=@barcode WHERE kodebarang=@kodebarang;
 ELSE
  INSERT dbo.tbBarang(kodebarang,namabarang,satuan,hargasatuan,stok,stok_min,harga_beli,barcode)
  VALUES(@kodebarang,@namabarang,@satuan,@hargasatuan,@stok,@stok_min,@harga_beli,@barcode);
 SELECT N'OK' AS status, @kodebarang AS kodebarang;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_KasCatat
 @tipe NVARCHAR(10), @kategori NVARCHAR(100), @nominal DECIMAL(18,2),
 @metode_bayar NVARCHAR(30)=NULL, @referensi NVARCHAR(60)=NULL,
 @keterangan NVARCHAR(300)=NULL, @created_by NVARCHAR(100)=NULL
AS
BEGIN
 SET NOCOUNT ON;
 IF @tipe NOT IN(N'Masuk',N'Keluar') THROW 50001,'Tipe kas harus Masuk atau Keluar.',1;
 IF @nominal<=0 THROW 50002,'Nominal harus lebih besar dari nol.',1;
 INSERT dbo.tbKas(tipe,kategori,nominal,metode_bayar,referensi,keterangan,created_by)
 VALUES(@tipe,@kategori,@nominal,@metode_bayar,@referensi,@keterangan,@created_by);
 SELECT SCOPE_IDENTITY() AS kas_id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_PembayaranHutangCatat
 @hutang_id BIGINT, @nominal DECIMAL(18,2), @metode NVARCHAR(30)=NULL, @referensi NVARCHAR(60)=NULL
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON; BEGIN TRAN;
 IF @nominal<=0 THROW 50003,'Nominal pembayaran harus lebih besar dari nol.',1;
 IF NOT EXISTS(SELECT 1 FROM dbo.tbHutang WITH(UPDLOCK,HOLDLOCK) WHERE hutang_id=@hutang_id) THROW 50004,'Data hutang tidak ditemukan.',1;
 IF @nominal>(SELECT nominal-terbayar FROM dbo.tbHutang WHERE hutang_id=@hutang_id) THROW 50005,'Pembayaran melebihi sisa hutang.',1;
 INSERT dbo.tbPembayaranHutang(hutang_id,nominal,metode,referensi) VALUES(@hutang_id,@nominal,@metode,@referensi);
 UPDATE dbo.tbHutang SET terbayar=terbayar+@nominal,status=CASE WHEN terbayar+@nominal>=nominal THEN N'Lunas' ELSE N'Sebagian' END WHERE hutang_id=@hutang_id;
 INSERT dbo.tbKas(tipe,kategori,nominal,metode_bayar,referensi,keterangan) VALUES(N'Keluar',N'Pembayaran Hutang',@nominal,@metode,@referensi,CONCAT(N'Hutang ID ',@hutang_id));
 COMMIT; SELECT @hutang_id AS hutang_id,N'OK' AS status;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_PembayaranPiutangCatat
 @piutang_id BIGINT, @nominal DECIMAL(18,2), @metode NVARCHAR(30)=NULL, @referensi NVARCHAR(60)=NULL
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON; BEGIN TRAN;
 IF @nominal<=0 THROW 50006,'Nominal pembayaran harus lebih besar dari nol.',1;
 IF NOT EXISTS(SELECT 1 FROM dbo.tbPiutang WITH(UPDLOCK,HOLDLOCK) WHERE piutang_id=@piutang_id) THROW 50007,'Data piutang tidak ditemukan.',1;
 IF @nominal>(SELECT nominal-terbayar FROM dbo.tbPiutang WHERE piutang_id=@piutang_id) THROW 50008,'Pembayaran melebihi sisa piutang.',1;
 INSERT dbo.tbPembayaranPiutang(piutang_id,nominal,metode,referensi) VALUES(@piutang_id,@nominal,@metode,@referensi);
 UPDATE dbo.tbPiutang SET terbayar=terbayar+@nominal,status=CASE WHEN terbayar+@nominal>=nominal THEN N'Lunas' ELSE N'Sebagian' END WHERE piutang_id=@piutang_id;
 INSERT dbo.tbKas(tipe,kategori,nominal,metode_bayar,referensi,keterangan) VALUES(N'Masuk',N'Pembayaran Piutang',@nominal,@metode,@referensi,CONCAT(N'Piutang ID ',@piutang_id));
 COMMIT; SELECT @piutang_id AS piutang_id,N'OK' AS status;
END;
GO

PRINT N'Views dan stored procedures ERP/POS berhasil dibuat.';
GO
