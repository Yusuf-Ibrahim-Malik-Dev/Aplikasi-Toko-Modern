/* Jalankan SETELAH wave1_foundation_auth_rbac.sql dan wave2_6_erp_pos.sql sukses.
   SQL Server harus memiliki izin tulis ke folder tujuan. Sesuaikan path sesuai server. */
USE master;
GO
BACKUP DATABASE [dbToko]
TO DISK = N'C:\SQLBackups\dbToko_ERP_POS_Updated.bak'
WITH COPY_ONLY, INIT, CHECKSUM, STATS = 10;
GO
RESTORE VERIFYONLY
FROM DISK = N'C:\SQLBackups\dbToko_ERP_POS_Updated.bak'
WITH CHECKSUM;
GO
