/* Toko-Root-ID ERP/POS extension, Waves 2-6. Idempotent SQL Server migration.
   Run after wave1_foundation_auth_rbac.sql against database dbToko. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.tbKategori','U') IS NULL CREATE TABLE dbo.tbKategori (kategori_id INT IDENTITY PRIMARY KEY, nama NVARCHAR(100) NOT NULL UNIQUE, keterangan NVARCHAR(255) NULL, is_active BIT NOT NULL DEFAULT 1, created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.tbMerek','U') IS NULL CREATE TABLE dbo.tbMerek (merek_id INT IDENTITY PRIMARY KEY, nama NVARCHAR(100) NOT NULL UNIQUE, is_active BIT NOT NULL DEFAULT 1, created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.tbSatuan','U') IS NULL CREATE TABLE dbo.tbSatuan (satuan_id INT IDENTITY PRIMARY KEY, nama NVARCHAR(50) NOT NULL UNIQUE, is_active BIT NOT NULL DEFAULT 1);
IF OBJECT_ID('dbo.tbSupplier','U') IS NULL CREATE TABLE dbo.tbSupplier (supplier_id INT IDENTITY PRIMARY KEY, kode NVARCHAR(30) NULL UNIQUE, nama NVARCHAR(150) NOT NULL, telepon NVARCHAR(30) NULL, email NVARCHAR(150) NULL, alamat NVARCHAR(500) NULL, saldo_hutang DECIMAL(18,2) NOT NULL DEFAULT 0, is_active BIT NOT NULL DEFAULT 1, created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.tbPelanggan','U') IS NULL CREATE TABLE dbo.tbPelanggan (pelanggan_id INT IDENTITY PRIMARY KEY, kode NVARCHAR(30) NULL UNIQUE, nama NVARCHAR(150) NOT NULL, telepon NVARCHAR(30) NULL, email NVARCHAR(150) NULL, alamat NVARCHAR(500) NULL, saldo_piutang DECIMAL(18,2) NOT NULL DEFAULT 0, is_active BIT NOT NULL DEFAULT 1, created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.tbGudang','U') IS NULL CREATE TABLE dbo.tbGudang (gudang_id INT IDENTITY PRIMARY KEY, kode NVARCHAR(30) NOT NULL UNIQUE, nama NVARCHAR(100) NOT NULL, alamat NVARCHAR(300) NULL, is_active BIT NOT NULL DEFAULT 1);

/* Extend legacy product table without renaming its existing columns. */
IF OBJECT_ID('dbo.tbBarang','U') IS NOT NULL BEGIN
 IF COL_LENGTH('dbo.tbBarang','kategori_id') IS NULL ALTER TABLE dbo.tbBarang ADD kategori_id INT NULL;
 IF COL_LENGTH('dbo.tbBarang','merek_id') IS NULL ALTER TABLE dbo.tbBarang ADD merek_id INT NULL;
 IF COL_LENGTH('dbo.tbBarang','satuan_id') IS NULL ALTER TABLE dbo.tbBarang ADD satuan_id INT NULL;
 IF COL_LENGTH('dbo.tbBarang','barcode') IS NULL ALTER TABLE dbo.tbBarang ADD barcode NVARCHAR(100) NULL;
 IF COL_LENGTH('dbo.tbBarang','stok_min') IS NULL ALTER TABLE dbo.tbBarang ADD stok_min DECIMAL(18,3) NOT NULL CONSTRAINT DF_tbBarang_stok_min DEFAULT 0;
 IF COL_LENGTH('dbo.tbBarang','harga_beli') IS NULL ALTER TABLE dbo.tbBarang ADD harga_beli DECIMAL(18,2) NOT NULL CONSTRAINT DF_tbBarang_harga_beli DEFAULT 0;
END;

IF OBJECT_ID('dbo.tbPembelian','U') IS NULL CREATE TABLE dbo.tbPembelian (pembelian_id BIGINT IDENTITY PRIMARY KEY, nomor NVARCHAR(40) NOT NULL UNIQUE, tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), supplier_id INT NULL REFERENCES dbo.tbSupplier(supplier_id), status NVARCHAR(20) NOT NULL DEFAULT 'Draft', subtotal DECIMAL(18,2) NOT NULL DEFAULT 0, diskon DECIMAL(18,2) NOT NULL DEFAULT 0, total DECIMAL(18,2) NOT NULL DEFAULT 0, dibayar DECIMAL(18,2) NOT NULL DEFAULT 0, catatan NVARCHAR(500) NULL, created_by NVARCHAR(100) NULL, CHECK(status IN ('Draft','Ordered','Received','Partial','Cancelled')));
IF OBJECT_ID('dbo.tbIsiPembelian','U') IS NULL CREATE TABLE dbo.tbIsiPembelian (detail_id BIGINT IDENTITY PRIMARY KEY, pembelian_id BIGINT NOT NULL REFERENCES dbo.tbPembelian(pembelian_id), kodebarang NVARCHAR(50) NOT NULL, qty DECIMAL(18,3) NOT NULL CHECK(qty>0), harga_beli DECIMAL(18,2) NOT NULL CHECK(harga_beli>=0), diskon DECIMAL(18,2) NOT NULL DEFAULT 0, total AS CONVERT(DECIMAL(18,2),(qty*harga_beli)-diskon) PERSISTED);
IF OBJECT_ID('dbo.tbReturJual','U') IS NULL CREATE TABLE dbo.tbReturJual (retur_id BIGINT IDENTITY PRIMARY KEY, nomor NVARCHAR(40) NOT NULL UNIQUE, tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), nomor_penjualan NVARCHAR(40) NULL, alasan NVARCHAR(300) NULL, total DECIMAL(18,2) NOT NULL DEFAULT 0, status NVARCHAR(20) NOT NULL DEFAULT 'Posted');
IF OBJECT_ID('dbo.tbReturBeli','U') IS NULL CREATE TABLE dbo.tbReturBeli (retur_id BIGINT IDENTITY PRIMARY KEY, nomor NVARCHAR(40) NOT NULL UNIQUE, tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), pembelian_id BIGINT NULL REFERENCES dbo.tbPembelian(pembelian_id), alasan NVARCHAR(300) NULL, total DECIMAL(18,2) NOT NULL DEFAULT 0, status NVARCHAR(20) NOT NULL DEFAULT 'Posted');
IF OBJECT_ID('dbo.tbReturJualDetail','U') IS NULL CREATE TABLE dbo.tbReturJualDetail (detail_id BIGINT IDENTITY PRIMARY KEY, retur_id BIGINT NOT NULL REFERENCES dbo.tbReturJual(retur_id), kodebarang NVARCHAR(50) NOT NULL, qty DECIMAL(18,3) NOT NULL CHECK(qty>0), harga DECIMAL(18,2) NOT NULL CHECK(harga>=0));
IF OBJECT_ID('dbo.tbReturBeliDetail','U') IS NULL CREATE TABLE dbo.tbReturBeliDetail (detail_id BIGINT IDENTITY PRIMARY KEY, retur_id BIGINT NOT NULL REFERENCES dbo.tbReturBeli(retur_id), kodebarang NVARCHAR(50) NOT NULL, qty DECIMAL(18,3) NOT NULL CHECK(qty>0), harga DECIMAL(18,2) NOT NULL CHECK(harga>=0));

IF OBJECT_ID('dbo.tbMutasiStok','U') IS NULL CREATE TABLE dbo.tbMutasiStok (mutasi_id BIGINT IDENTITY PRIMARY KEY, tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), kodebarang NVARCHAR(50) NOT NULL, gudang_id INT NULL REFERENCES dbo.tbGudang(gudang_id), tipe NVARCHAR(20) NOT NULL, qty DECIMAL(18,3) NOT NULL, referensi NVARCHAR(60) NULL, catatan NVARCHAR(300) NULL, created_by NVARCHAR(100) NULL, CHECK(tipe IN ('Masuk','Keluar','Penyesuaian','Transfer')));
IF OBJECT_ID('dbo.tbOpnameStok','U') IS NULL CREATE TABLE dbo.tbOpnameStok (opname_id BIGINT IDENTITY PRIMARY KEY, nomor NVARCHAR(40) NOT NULL UNIQUE, tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), gudang_id INT NULL REFERENCES dbo.tbGudang(gudang_id), status NVARCHAR(20) NOT NULL DEFAULT 'Draft', catatan NVARCHAR(300) NULL, created_by NVARCHAR(100) NULL);
IF OBJECT_ID('dbo.tbOpnameStokDetail','U') IS NULL CREATE TABLE dbo.tbOpnameStokDetail (detail_id BIGINT IDENTITY PRIMARY KEY, opname_id BIGINT NOT NULL REFERENCES dbo.tbOpnameStok(opname_id), kodebarang NVARCHAR(50) NOT NULL, stok_sistem DECIMAL(18,3) NOT NULL, stok_fisik DECIMAL(18,3) NOT NULL, selisih AS (stok_fisik-stok_sistem) PERSISTED);
IF OBJECT_ID('dbo.tbTransferGudang','U') IS NULL CREATE TABLE dbo.tbTransferGudang (transfer_id BIGINT IDENTITY PRIMARY KEY, nomor NVARCHAR(40) NOT NULL UNIQUE, tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), dari_gudang INT NOT NULL REFERENCES dbo.tbGudang(gudang_id), ke_gudang INT NOT NULL REFERENCES dbo.tbGudang(gudang_id), status NVARCHAR(20) NOT NULL DEFAULT 'Draft', catatan NVARCHAR(300) NULL, CHECK(dari_gudang<>ke_gudang));
IF OBJECT_ID('dbo.tbTransferGudangDetail','U') IS NULL CREATE TABLE dbo.tbTransferGudangDetail (detail_id BIGINT IDENTITY PRIMARY KEY, transfer_id BIGINT NOT NULL REFERENCES dbo.tbTransferGudang(transfer_id), kodebarang NVARCHAR(50) NOT NULL, qty DECIMAL(18,3) NOT NULL CHECK(qty>0));

IF OBJECT_ID('dbo.tbKas','U') IS NULL CREATE TABLE dbo.tbKas (kas_id BIGINT IDENTITY PRIMARY KEY, tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), tipe NVARCHAR(10) NOT NULL, kategori NVARCHAR(100) NOT NULL, nominal DECIMAL(18,2) NOT NULL CHECK(nominal>0), metode_bayar NVARCHAR(30) NULL, referensi NVARCHAR(60) NULL, keterangan NVARCHAR(300) NULL, created_by NVARCHAR(100) NULL, CHECK(tipe IN ('Masuk','Keluar')));
IF OBJECT_ID('dbo.tbHutang','U') IS NULL CREATE TABLE dbo.tbHutang (hutang_id BIGINT IDENTITY PRIMARY KEY, supplier_id INT NULL REFERENCES dbo.tbSupplier(supplier_id), pembelian_id BIGINT NULL REFERENCES dbo.tbPembelian(pembelian_id), tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), jatuh_tempo DATE NULL, nominal DECIMAL(18,2) NOT NULL CHECK(nominal>=0), terbayar DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK(terbayar>=0), status NVARCHAR(20) NOT NULL DEFAULT 'Belum Lunas');
IF OBJECT_ID('dbo.tbPembayaranHutang','U') IS NULL CREATE TABLE dbo.tbPembayaranHutang (pembayaran_id BIGINT IDENTITY PRIMARY KEY, hutang_id BIGINT NOT NULL REFERENCES dbo.tbHutang(hutang_id), tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), nominal DECIMAL(18,2) NOT NULL CHECK(nominal>0), metode NVARCHAR(30) NULL, referensi NVARCHAR(60) NULL);
IF OBJECT_ID('dbo.tbPiutang','U') IS NULL CREATE TABLE dbo.tbPiutang (piutang_id BIGINT IDENTITY PRIMARY KEY, pelanggan_id INT NULL REFERENCES dbo.tbPelanggan(pelanggan_id), nomor_penjualan NVARCHAR(40) NULL, tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), jatuh_tempo DATE NULL, nominal DECIMAL(18,2) NOT NULL CHECK(nominal>=0), terbayar DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK(terbayar>=0), status NVARCHAR(20) NOT NULL DEFAULT 'Belum Lunas');
IF OBJECT_ID('dbo.tbPembayaranPiutang','U') IS NULL CREATE TABLE dbo.tbPembayaranPiutang (pembayaran_id BIGINT IDENTITY PRIMARY KEY, piutang_id BIGINT NOT NULL REFERENCES dbo.tbPiutang(piutang_id), tanggal DATETIME2 NOT NULL DEFAULT SYSDATETIME(), nominal DECIMAL(18,2) NOT NULL CHECK(nominal>0), metode NVARCHAR(30) NULL, referensi NVARCHAR(60) NULL);

IF OBJECT_ID('dbo.tbKaryawan','U') IS NULL CREATE TABLE dbo.tbKaryawan (karyawan_id INT IDENTITY PRIMARY KEY, nik NVARCHAR(30) NOT NULL UNIQUE, nama NVARCHAR(150) NOT NULL, jabatan NVARCHAR(100) NULL, telepon NVARCHAR(30) NULL, alamat NVARCHAR(500) NULL, tanggal_masuk DATE NULL, gaji_pokok DECIMAL(18,2) NOT NULL DEFAULT 0, is_active BIT NOT NULL DEFAULT 1);
IF OBJECT_ID('dbo.tbAbsensi','U') IS NULL CREATE TABLE dbo.tbAbsensi (absensi_id BIGINT IDENTITY PRIMARY KEY, karyawan_id INT NOT NULL REFERENCES dbo.tbKaryawan(karyawan_id), tanggal DATE NOT NULL, jam_masuk TIME NULL, jam_keluar TIME NULL, status NVARCHAR(20) NOT NULL DEFAULT 'Hadir', keterangan NVARCHAR(300) NULL, CONSTRAINT UQ_tbAbsensi_karyawan_tanggal UNIQUE(karyawan_id,tanggal));
IF OBJECT_ID('dbo.tbGaji','U') IS NULL CREATE TABLE dbo.tbGaji (gaji_id BIGINT IDENTITY PRIMARY KEY, karyawan_id INT NOT NULL REFERENCES dbo.tbKaryawan(karyawan_id), periode CHAR(7) NOT NULL, gaji_pokok DECIMAL(18,2) NOT NULL DEFAULT 0, tunjangan DECIMAL(18,2) NOT NULL DEFAULT 0, potongan DECIMAL(18,2) NOT NULL DEFAULT 0, total AS (gaji_pokok+tunjangan-potongan) PERSISTED, tanggal_bayar DATE NULL, status NVARCHAR(20) NOT NULL DEFAULT 'Draft', CONSTRAINT UQ_tbGaji_karyawan_periode UNIQUE(karyawan_id,periode));
IF OBJECT_ID('dbo.tbPengaturan','U') IS NULL CREATE TABLE dbo.tbPengaturan (pengaturan_key NVARCHAR(100) PRIMARY KEY, pengaturan_value NVARCHAR(1000) NULL, updated_at DATETIME2 NOT NULL DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.tbProfilToko','U') IS NULL CREATE TABLE dbo.tbProfilToko (profil_id INT NOT NULL PRIMARY KEY CHECK(profil_id=1), nama_toko NVARCHAR(150) NOT NULL, alamat NVARCHAR(500) NULL, telepon NVARCHAR(30) NULL, email NVARCHAR(150) NULL, footer_struk NVARCHAR(300) NULL);

/* Standard role names requested by the project; keep legacy roles intact. */
IF OBJECT_ID('dbo.tbRole','U') IS NOT NULL BEGIN
 IF NOT EXISTS(SELECT 1 FROM dbo.tbRole WHERE role_name='Supervisor') INSERT dbo.tbRole(role_name,role_description,is_active) VALUES('Supervisor','Supervisor toko',1);
END;
/* Add common permissions safely; role assignment is intentionally explicit. */
IF OBJECT_ID('dbo.tbPermission','U') IS NOT NULL BEGIN
 INSERT dbo.tbPermission(permission_code,permission_name,permission_category)
 SELECT v.code,v.name,v.cat FROM (VALUES
 ('POS_ACCESS','Akses POS','Transaksi'),('PEMBELIAN_MANAGE','Kelola pembelian','Transaksi'),('SUPPLIER_MANAGE','Kelola supplier','Master'),('PELANGGAN_MANAGE','Kelola pelanggan','Master'),('INVENTORY_MANAGE','Kelola inventory','Inventory'),('FINANCE_MANAGE','Kelola keuangan','Keuangan'),('HR_MANAGE','Kelola HR','HR'),('SETTINGS_MANAGE','Kelola pengaturan','Sistem'),('BACKUP_MANAGE','Backup dan restore','Sistem'),('DASHBOARD_VIEW','Lihat dashboard','Laporan')) v(code,name,cat)
 WHERE NOT EXISTS(SELECT 1 FROM dbo.tbPermission p WHERE p.permission_code=v.code);
END;

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.tbMutasiStok') AND name='IX_tbMutasiStok_barang_tanggal') CREATE INDEX IX_tbMutasiStok_barang_tanggal ON dbo.tbMutasiStok(kodebarang,tanggal DESC);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.tbKas') AND name='IX_tbKas_tanggal') CREATE INDEX IX_tbKas_tanggal ON dbo.tbKas(tanggal DESC);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.tbHutang') AND name='IX_tbHutang_status') CREATE INDEX IX_tbHutang_status ON dbo.tbHutang(status,jatuh_tempo);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.tbPiutang') AND name='IX_tbPiutang_status') CREATE INDEX IX_tbPiutang_status ON dbo.tbPiutang(status,jatuh_tempo);

IF OBJECT_ID('dbo.vwRingkasanKas','V') IS NOT NULL DROP VIEW dbo.vwRingkasanKas;
EXEC('CREATE VIEW dbo.vwRingkasanKas AS SELECT CAST(tanggal AS DATE) tanggal, SUM(CASE WHEN tipe=''Masuk'' THEN nominal ELSE 0 END) kas_masuk, SUM(CASE WHEN tipe=''Keluar'' THEN nominal ELSE 0 END) kas_keluar, SUM(CASE WHEN tipe=''Masuk'' THEN nominal ELSE -nominal END) saldo_harian FROM dbo.tbKas GROUP BY CAST(tanggal AS DATE)');
IF OBJECT_ID('dbo.vwHutangTerbuka','V') IS NOT NULL DROP VIEW dbo.vwHutangTerbuka;
EXEC('CREATE VIEW dbo.vwHutangTerbuka AS SELECT h.hutang_id,h.supplier_id,s.nama nama_supplier,h.tanggal,h.jatuh_tempo,h.nominal,h.terbayar,(h.nominal-h.terbayar) sisa,h.status FROM dbo.tbHutang h LEFT JOIN dbo.tbSupplier s ON s.supplier_id=h.supplier_id WHERE h.nominal>h.terbayar');
IF OBJECT_ID('dbo.vwPiutangTerbuka','V') IS NOT NULL DROP VIEW dbo.vwPiutangTerbuka;
EXEC('CREATE VIEW dbo.vwPiutangTerbuka AS SELECT p.piutang_id,p.pelanggan_id,c.nama nama_pelanggan,p.nomor_penjualan,p.tanggal,p.jatuh_tempo,p.nominal,p.terbayar,(p.nominal-p.terbayar) sisa,p.status FROM dbo.tbPiutang p LEFT JOIN dbo.tbPelanggan c ON c.pelanggan_id=p.pelanggan_id WHERE p.nominal>p.terbayar');

COMMIT TRANSACTION;
PRINT 'Wave 2-6 schema migration completed. Review application integration before production use.';
GO
