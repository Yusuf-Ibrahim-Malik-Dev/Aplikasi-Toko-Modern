/* Toko-Root-ID - INSTALLER SQL LENGKAP ERP/POS
   Jalankan pada SQL Server Management Studio. Pastikan database dbToko sudah ada dan backup sebelum migrasi.
   Target SQL Server 2016 SP1 atau lebih baru. Skrip terdiri dari Wave 1, Wave 2-6, Views dan Stored Procedures.
*/
USE dbToko;
GO


/* ================= SOURCE: wave1_foundation_auth_rbac.sql ================= */
-- ============================================================================
-- DATABASE SCHEMA: Toko-Root-ID ERP/POS System
-- Wave 1: Foundation, Auth & RBAC (Role-Based Access Control)
-- ============================================================================

-- ============================================
-- TABEL: tbRole (Daftar Role Pengguna)
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbRole' AND xtype='U')
BEGIN
    CREATE TABLE tbRole (
        role_id INT IDENTITY(1,1) PRIMARY KEY,
        role_name NVARCHAR(50) NOT NULL UNIQUE,
        role_description NVARCHAR(255) NULL,
        is_active BIT NOT NULL DEFAULT 1,
        created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        created_by INT NULL,
        updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT CK_tbRole_role_name CHECK (LEN(LTRIM(RTRIM(role_name))) > 0)
    );

    -- Insert roles dasar
    INSERT INTO tbRole (role_name, role_description, is_active, created_at) VALUES
    ('Owner', 'Pemilik toko - akses penuh', 1, GETDATE()),
    ('Admin', 'Administrator - akses semua modul', 1, GETDATE()),
    ('Manager', 'Manager toko - akses penjualan, inventory, laporan', 1, GETDATE()),
    ('Kasir', 'Kasir - akses POS dan transaksi penjualan', 1, GETDATE()),
    ('Gudang', 'Petugas gudang - akses inventory masuk/keluar', 1, GETDATE());
END
GO

-- ============================================
-- TABEL: tbPermission (Daftar Permission/Akses)
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbPermission' AND xtype='U')
BEGIN
    CREATE TABLE tbPermission (
        permission_id INT IDENTITY(1,1) PRIMARY KEY,
        permission_code NVARCHAR(50) NOT NULL UNIQUE,
        permission_name NVARCHAR(100) NOT NULL,
        permission_category NVARCHAR(50) NOT NULL, -- e.g., 'Master', 'Transaksi', 'Laporan', 'User'
        is_active BIT NOT NULL DEFAULT 1,
        created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT CK_tbPermission_permission_code CHECK (LEN(LTRIM(RTRIM(permission_code))) > 0)
    );

    -- Insert permissions dasar per kategori
    INSERT INTO tbPermission (permission_code, permission_name, permission_category, is_active, created_at) VALUES
    -- Master Data
    ('MASTER_BARANG_VIEW', 'Lihat Daftar Barang', 'Master', 1, GETDATE()),
    ('MASTER_BARANG_CREATE', 'Tambah Barang', 'Master', 1, GETDATE()),
    ('MASTER_BARANG_EDIT', 'Edit Barang', 'Master', 1, GETDATE()),
    ('MASTER_BARANG_DELETE', 'Hapus Barang', 'Master', 1, GETDATE()),
    ('MASTER_KATEGORI_VIEW', 'Lihat Kategori', 'Master', 1, GETDATE()),
    ('MASTER_KATEGORI_MANAGE', 'Kelola Kategori', 'Master', 1, GETDATE()),
    ('MASTER_MEREK_VIEW', 'Lihat Merek', 'Master', 1, GETDATE()),
    ('MASTER_MEREK_MANAGE', 'Kelola Merek', 'Master', 1, GETDATE()),
    ('MASTER_SATUAN_VIEW', 'Lihat Satuan', 'Master', 1, GETDATE()),
    ('MASTER_SATUAN_MANAGE', 'Kelola Satuan', 'Master', 1, GETDATE()),
    ('MASTER_PEMERIKSAAN_VIEW', 'Lihat Pemeriksaan', 'Master', 1, GETDATE()),
    ('MASTER_PEMERIKSAAN_MANAGE', 'Kelola Pemeriksaan', 'Master', 1, GETDATE()),
    
    -- Transaksi
    ('TRANSAKSI_PENJUALAN_VIEW', 'Lihat Penjualan', 'Transaksi', 1, GETDATE()),
    ('TRANSAKSI_PENJUALAN_CREATE', 'Buat Penjualan', 'Transaksi', 1, GETDATE()),
    ('TRANSAKSI_PENJUALAN_RETUR', 'Retur Penjualan', 'Transaksi', 1, GETDATE()),
    ('TRANSAKSI_PEMBELIAN_VIEW', 'Lihat Pembelian', 'Transaksi', 1, GETDATE()),
    ('TRANSAKSI_PEMBELIAN_CREATE', 'Buat Pembelian', 'Transaksi', 1, GETDATE()),
    ('TRANSAKSI_PEMBELIAN_RETUR', 'Retur Pembelian', 'Transaksi', 1, GETDATE()),
    
    -- Inventory
    ('INVENTORY_STOK_VIEW', 'Lihat Stok', 'Inventory', 1, GETDATE()),
    ('INVENTORY_MUTASI_VIEW', 'Lihat Mutasi Stok', 'Inventory', 1, GETDATE()),
    ('INVENTORY_OPNAME_VIEW', 'Lihat Opname', 'Inventory', 1, GETDATE()),
    ('INVENTORY_OPNAME_CREATE', 'Buat Opname', 'Inventory', 1, GETDATE()),
    
    -- Keuangan
    ('KEUANGAN_KAS_VIEW', 'Lihat Arus Kas', 'Keuangan', 1, GETDATE()),
    ('KEUANGAN_HUTANG_VIEW', 'Lihat Hutang', 'Keuangan', 1, GETDATE()),
    ('KEUANGAN_PIUTANG_VIEW', 'Lihat Piutang', 'Keuangan', 1, GETDATE()),
    ('KEUANGAN_LAPORAN_VIEW', 'Lihat Laporan Keuangan', 'Keuangan', 1, GETDATE()),
    
    -- HR
    ('HR_KARYAWAN_VIEW', 'Lihat Karyawan', 'HR', 1, GETDATE()),
    ('HR_KARYAWAN_MANAGE', 'Kelola Karyawan', 'HR', 1, GETDATE()),
    ('HR_ABSENISI_VIEW', 'Lihat Absensi', 'HR', 1, GETDATE()),
    ('HR_GAJI_VIEW', 'Lihat Gaji', 'HR', 1, GETDATE()),
    
    -- Laporan
    ('LAPORAN_PENJUALAN_VIEW', 'Lihat Laporan Penjualan', 'Laporan', 1, GETDATE()),
    ('LAPORAN_PEMBELIAN_VIEW', 'Lihat Laporan Pembelian', 'Laporan', 1, GETDATE()),
    ('LAPORAN_STOK_VIEW', 'Lihat Laporan Stok', 'Laporan', 1, GETDATE()),
    ('LAPORAN_PROFIT_VIEW', 'Lihat Laporan Profit', 'Laporan', 1, GETDATE()),
    ('LAPORAN_DASHBOARD_VIEW', 'Lihat Dashboard', 'Laporan', 1, GETDATE()),
    
    -- User & Auth
    ('USER_VIEW', 'Lihat Daftar User', 'User', 1, GETDATE()),
    ('USER_CREATE', 'Tambah User', 'User', 1, GETDATE()),
    ('USER_EDIT', 'Edit User', 'User', 1, GETDATE()),
    ('USER_DELETE', 'Hapus User', 'User', 1, GETDATE()),
    ('ROLE_MANAGE', 'Kelola Role', 'User', 1, GETDATE()),
    ('PERMISSION_MANAGE', 'Kelola Permission', 'User', 1, GETDATE());
END
GO

-- ============================================
-- TABEL: tbRolePermission (Hubungan Role-Permission)
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbRolePermission' AND xtype='U')
BEGIN
    CREATE TABLE tbRolePermission (
        role_permission_id INT IDENTITY(1,1) PRIMARY KEY,
        role_id INT NOT NULL,
        permission_id INT NOT NULL,
        granted_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        granted_by INT NULL,
        CONSTRAINT FK_tbRolePermission_role FOREIGN KEY (role_id) REFERENCES tbRole(role_id),
        CONSTRAINT FK_tbRolePermission_permission FOREIGN KEY (permission_id) REFERENCES tbPermission(permission_id),
        CONSTRAINT UQ_tbRolePermission_role_permission UNIQUE (role_id, permission_id)
    );
END
GO

-- ============================================
-- UPDATE/ALTER TABEL tbUser
-- ============================================
-- Cek apakah tabel tbUser sudah ada
IF OBJECT_ID('dbo.tbUser', 'U') IS NOT NULL
BEGIN
    -- Tambahkan kolom role_id jika belum ada
    IF COL_LENGTH('tbUser', 'role_id') IS NULL
    BEGIN
        ALTER TABLE tbUser ADD role_id INT NULL;
        ALTER TABLE tbUser ADD CONSTRAINT FK_tbUser_role FOREIGN KEY (role_id) REFERENCES tbRole(role_id);
        PRINT 'Added role_id column to tbUser';
    END
    
    -- Tambahkan kolom is_active jika belum ada
    IF COL_LENGTH('tbUser', 'is_active') IS NULL
    BEGIN
        ALTER TABLE tbUser ADD is_active BIT NOT NULL DEFAULT 1;
        PRINT 'Added is_active column to tbUser';
    END
    
    -- Tambahkan kolom PasswordHash jika belum ada (untuk keamanan)
    -- Format: 96 karakter (32 salt + 64 PBKDF2 hash dalam bentuk hex)
    IF COL_LENGTH('tbUser', 'PasswordHash') IS NULL
    BEGIN
        ALTER TABLE tbUser ADD PasswordHash NVARCHAR(96) NULL;
        PRINT 'Added PasswordHash column to tbUser';
    END
    
    -- Tambahkan kolom Salt jika belum ada
    IF COL_LENGTH('tbUser', 'Salt') IS NULL
    BEGIN
        ALTER TABLE tbUser ADD Salt NVARCHAR(32) NULL;
        PRINT 'Added Salt column to tbUser';
    END
END
GO

-- ============================================
-- MEMBERIKAN PERKEZIINAN KE ROLE AWAL
-- ============================================
IF EXISTS (SELECT * FROM tbRolePermission) 
BEGIN
    PRINT 'Role permissions already assigned, skipping initial assignment.';
END
ELSE
BEGIN
    -- Owner: Semua permission
    INSERT INTO tbRolePermission (role_id, permission_id, granted_at, granted_by)
    SELECT r.role_id, p.permission_id, GETDATE(), 1
    FROM tbRole r CROSS JOIN tbPermission p
    WHERE r.role_name = 'Owner';

    -- Admin: Semua permission kecuali Owner exclusive
    INSERT INTO tbRolePermission (role_id, permission_id, granted_at, granted_by)
    SELECT r.role_id, p.permission_id, GETDATE(), 1
    FROM tbRole r CROSS JOIN tbPermission p
    WHERE r.role_name = 'Admin';

    -- Manager: Akses Transaksi, Inventory, Laporan, Master (view)
    INSERT INTO tbRolePermission (role_id, permission_id, granted_at, granted_by)
    SELECT r.role_id, p.permission_id, GETDATE(), 1
    FROM tbRole r, tbPermission p
    WHERE r.role_name = 'Manager' 
    AND (
        p.permission_category IN ('Transaksi', 'Inventory', 'Laporan')
        OR (p.permission_category = 'Master' AND p.permission_name LIKE '%VIEW%')
    );

    -- Kasir: Akses POS (penjualan)
    INSERT INTO tbRolePermission (role_id, permission_id, granted_at, granted_by)
    SELECT r.role_id, p.permission_id, GETDATE(), 1
    FROM tbRole r, tbPermission p
    WHERE r.role_name = 'Kasir' 
    AND p.permission_category = 'Transaksi' 
    AND p.permission_code IN ('TRANSAKSI_PENJUALAN_VIEW', 'TRANSAKSI_PENJUALAN_CREATE');

    -- Gudang: Akses Inventory
    INSERT INTO tbRolePermission (role_id, permission_id, granted_at, granted_by)
    SELECT r.role_id, p.permission_id, GETDATE(), 1
    FROM tbRole r, tbPermission p
    WHERE r.role_name = 'Gudang'
    AND p.permission_category = 'Inventory';
END
GO

-- ============================================
-- STORED PROCEDURE: proc_login (Update untuk RBAC)
-- Password verification dilakukan di sisi aplikasi (PasswordHelper)
-- ============================================
IF OBJECT_ID('dbo.proc_login', 'P') IS NOT NULL
    DROP PROCEDURE dbo.proc_login;
GO

CREATE PROCEDURE dbo.proc_login
    @username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- Cek user yang aktif dengan role yang terhubung
    -- Verifikasi password dilakukan di sisi aplikasi (PasswordHelper)
    SELECT 
        u.nik,
        u.nama,
        u.PasswordHash,
        u.Salt,
        u.role_id,
        r.role_name,
        u.is_active,
        u.expireddate
    FROM tbUser u
    INNER JOIN tbRole r ON u.role_id = r.role_id
    WHERE (u.nama = @username OR u.nik = @username)
        ;
END
GO

-- ============================================
-- STORED PROCEDURE: proc_getAllUser (Update untuk role)
-- ============================================
IF EXISTS (SELECT * FROM sysobjects WHERE name='proc_getAllUser' AND xtype='P')
BEGIN
    DROP PROCEDURE dbo.proc_getAllUser;
END
GO

CREATE PROCEDURE dbo.proc_getAllUser
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.nik,
        u.nama,
        u.expireddate,
        u.is_active,
        u.role_id,
        r.role_name
    FROM tbUser u
    LEFT JOIN tbRole r ON u.role_id = r.role_id;
END
GO

-- ============================================
-- STORED PROCEDURE: proc_userinsertupdate
-- Password hashing dilakukan di sisi aplikasi (PasswordHelper)
-- ============================================
IF OBJECT_ID('dbo.proc_userinsertupdate', 'P') IS NOT NULL
    DROP PROCEDURE dbo.proc_userinsertupdate;
GO

CREATE PROCEDURE dbo.proc_userinsertupdate
    @nik NVARCHAR(50) = NULL,
    @nama NVARCHAR(100) = NULL,
    @passwordHash NVARCHAR(96) = NULL,
    @expireddate DATETIME2 = NULL,
    @role_id INT = NULL,
    @is_active BIT = 1,
    @action NVARCHAR(10) = 'INSERT'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @pesan NVARCHAR(255) = '';

    IF @action = 'INSERT'
    BEGIN
        -- Cek nik sudah ada
        IF EXISTS (SELECT 1 FROM tbUser WHERE nik = @nik)
        BEGIN
            SET @pesan = 'NIK sudah ada. Silakan gunakan NIK lain.';
            SELECT @pesan AS pesan;
            RETURN;
        END

        INSERT INTO tbUser (
            nik, nama, PasswordHash, expireddate, role_id, is_active
        ) VALUES (
            @nik, @nama, @passwordHash, @expireddate, @role_id, @is_active
        );

        SET @pesan = 'User berhasil ditambahkan.';
    END
    ELSE IF @action = 'UPDATE'
    BEGIN
        UPDATE tbUser 
        SET 
            nama = @nama,
            expireddate = @expireddate,
            role_id = @role_id,
            is_active = @is_active,
            PasswordHash = @passwordHash
        WHERE nik = @nik;

        SET @pesan = 'User berhasil diupdate.';
    END

    SELECT @pesan AS pesan;
END
GO

-- ============================================
-- STORED PROCEDURE: proc_getdatauserbykode
-- ============================================
IF OBJECT_ID('dbo.proc_getdatauserbykode', 'P') IS NOT NULL
    DROP PROCEDURE dbo.proc_getdatauserbykode;
GO

CREATE PROCEDURE dbo.proc_getdatauserbykode
    @nik NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.nik,
        u.nama,
        u.expireddate,
        u.is_active,
        u.role_id,
        r.role_name
    FROM tbUser u
    LEFT JOIN tbRole r ON u.role_id = r.role_id
    WHERE u.nik = @nik;
END
GO

-- ============================================
-- STORED PROCEDURE: proc_deleteuser
-- ============================================
IF OBJECT_ID('dbo.proc_deleteuser', 'P') IS NOT NULL
    DROP PROCEDURE dbo.proc_deleteuser;
GO

CREATE PROCEDURE dbo.proc_deleteuser
    @nik NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @nama NVARCHAR(100);

    SELECT @nama = nama FROM tbUser WHERE nik = @nik;

    DELETE FROM tbUser WHERE nik = @nik;

    SELECT @nama AS nama;
END
GO

-- ============================================
-- VIEW: vwActiveUsers (Daftar user aktif)
-- ============================================
IF OBJECT_ID('dbo.vwActiveUsers', 'V') IS NOT NULL
    DROP VIEW dbo.vwActiveUsers;
GO

CREATE VIEW dbo.vwActiveUsers
AS
SELECT 
    u.nik,
    u.nama,
    u.expireddate,
    u.is_active,
    u.role_id,
    r.role_name
FROM tbUser u
INNER JOIN tbRole r ON u.role_id = r.role_id
WHERE u.is_active = 1;
GO

-- ============================================
-- VIEW: vwUserRole (Gabungkan user dengan role untuk permission check)
-- ============================================
IF OBJECT_ID('dbo.vwUserRole', 'V') IS NOT NULL
    DROP VIEW dbo.vwUserRole;
GO

CREATE VIEW dbo.vwUserRole
AS
SELECT 
    r.role_id,
    r.role_name,
    p.permission_id,
    p.permission_code,
    p.permission_name,
    p.permission_category
FROM tbRole r
CROSS JOIN tbPermission p
WHERE r.is_active = 1 AND p.is_active = 1;
GO

-- ============================================
-- INDEX untuk kinerja
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbUser_nama' AND object_id = OBJECT_ID('dbo.tbUser'))
    CREATE NON CLUSTERED INDEX IX_tbUser_nama ON tbUser(nama);
    
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbUser_nik' AND object_id = OBJECT_ID('dbo.tbUser'))
    CREATE NON CLUSTERED INDEX IX_tbUser_nik ON tbUser(nik);
    
IF COL_LENGTH('tbUser', 'role_id') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbUser_role_id' AND object_id = OBJECT_ID('dbo.tbUser'))
        CREATE NON CLUSTERED INDEX IX_tbUser_role_id ON tbUser(role_id);
END

IF COL_LENGTH('tbUser', 'is_active') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbUser_is_active' AND object_id = OBJECT_ID('dbo.tbUser'))
        CREATE NON CLUSTERED INDEX IX_tbUser_is_active ON tbUser(is_active);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbRole_role_name' AND object_id = OBJECT_ID('dbo.tbRole'))
    CREATE NON CLUSTERED INDEX IX_tbRole_role_name ON tbRole(role_name);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbRolePermission_role_id' AND object_id = OBJECT_ID('dbo.tbRolePermission'))
    CREATE NON CLUSTERED INDEX IX_tbRolePermission_role_id ON tbRolePermission(role_id);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbRolePermission_permission_id' AND object_id = OBJECT_ID('dbo.tbRolePermission'))
    CREATE NON CLUSTERED INDEX IX_tbRolePermission_permission_id ON tbRolePermission(permission_id);
GO

-- ============================================
-- KOMENTAR AKHIR
-- ============================================
PRINT '';
PRINT 'Wave 1: Foundation, Auth & RBAC - Database schema successfully created/updated.';
PRINT '';
PRINT 'Langkah selanjutnya:';
PRINT '1. Run this SQL script against dbToko database';
PRINT '2. Create default admin user with hashed password';
PRINT '3. Update DAUser.cs to set SessionManager.CurrentUser on login';
PRINT '4. Update Login form to use session management and permission-based navigation';
PRINT '';



/* ================= SOURCE: wave2_6_erp_pos.sql ================= */
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




/* ================= SOURCE: ERP_POS_Views_Procedures.sql ================= */
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


