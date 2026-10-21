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