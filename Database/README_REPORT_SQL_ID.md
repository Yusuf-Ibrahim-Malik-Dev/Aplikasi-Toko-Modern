# Toko-Root-ID — perbaikan report dan query SQL

## Query database
1. Backup database `dbToko` sebelum menjalankan migrasi.
2. Buka `Database/ERP_POS_COMPLETE_INSTALL.sql` di SQL Server Management Studio.
3. Pilih database `dbToko`, lalu Execute. Target script adalah SQL Server 2016 SP1 atau lebih baru.
4. Periksa tab Messages. Jika skema database lama berbeda, sesuaikan kolom/tabel yang disebutkan pada pesan error.

File modular tersedia jika ingin menjalankan bertahap: `wave1_foundation_auth_rbac.sql`, `wave2_6_erp_pos.sql`, lalu `ERP_POS_Views_Procedures.sql`.

## Report tanpa Crystal Reports
- Project utama tidak lagi mereferensikan CrystalDecisions, Microsoft ReportViewer, atau FlashControl.
- Form `Report_Barang` dan `Report_Transaksi` memakai DataGridView, Print Preview/PrintDocument, dan export CSV.
- `ReportStrukTemplate.Preview(...)` menyediakan template struk tanpa Crystal; pemanggilannya dapat diintegrasikan dari proses pembayaran POS.
- Laporan barang membaca `dbo.vwReportBarang`; laporan transaksi membaca `dbo.vwReportPenjualan`. View penjualan dibuat dari `tbFaktur` jika kolom `nofaktur`, `tanggal`, dan `total` tersedia; jika tidak, view kosong dibuat agar aplikasi tetap bisa dibangun dan query report tidak gagal karena objek hilang.

## Catatan penting
- SQL dibuat idempotent sebisa mungkin, tetapi tetap jalankan di database salinan/cadangan lebih dahulu.
- Source tidak dapat dikompilasi/diuji terhadap SQL Server pada lingkungan ini karena toolchain Visual Studio/MSBuild dan instance SQL Server tidak tersedia. Lakukan Rebuild Solution dan uji login, cetak struk, report, transaksi serta backup pada mesin pengembangan.
