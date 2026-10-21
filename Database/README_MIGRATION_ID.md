# Panduan migrasi Toko-Root-ID

## Urutan aman
1. Buat backup database `dbToko` yang sedang dipakai terlebih dahulu.
2. Buka database `dbToko` di SQL Server Management Studio (SSMS).
3. Jalankan `wave1_foundation_auth_rbac.sql`.
4. Jalankan `wave2_6_erp_pos.sql`.
5. Verifikasi tabel, foreign key, dan data aplikasi sebelum dipakai.
6. Jalankan `backup_dbToko_updated.sql` setelah mengubah path backup ke folder yang bisa ditulis oleh layanan SQL Server. File hasilnya bernama `dbToko_ERP_POS_Updated.bak`.

## Catatan penting
- File `.bak` adalah format backup native SQL Server. Tidak aman untuk mengubah byte file `.bak` secara langsung atau mengganti ekstensi file SQL menjadi `.bak`. Backup baru harus dibuat oleh SQL Server setelah migrasi benar-benar diterapkan.
- Migrasi ini menambah fondasi tabel Wave 2-6 dan mempertahankan tabel lama. Form WinForms untuk semua modul (pembelian, retur, finance, HR, gudang, dan laporan ERP) masih memerlukan integrasi UI/DAAccess bertahap sebelum setiap fitur dapat dianggap siap produksi.
- Uji di salinan database dahulu. Jangan jalankan migrasi langsung pada database produksi tanpa backup yang tervalidasi.
