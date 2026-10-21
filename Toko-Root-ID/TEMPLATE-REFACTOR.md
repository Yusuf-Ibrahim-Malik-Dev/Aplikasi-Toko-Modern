# Template Refactor — Root-ID

## Prinsip pemisahan

- `Login/LoginTemplate.cs` membangun seluruh UI login (panel branding, field, tombol, error state, dan focus border). `form_login_id.cs` menangani autentikasi dan event.
- `MenuUtama/MenuUtamaTemplate.cs` membangun shell aplikasi, sidebar/navigasi, dashboard, kartu metrik, chart, dan pengaturan warna. `FormUtama.cs` menyimpan navigasi publik dan mengambil data dashboard.
- `Barang/BarangTemplate.cs` menjadi tempat tema visual untuk form barang.
- `Penjualan/PenjualanTemplate.cs` menjadi tempat tema visual untuk kasir/penjualan.
- `Grid/grdBarangTemplate.cs` dan `Grid/grdUserTemplate.cs` menjadi tempat tema visual untuk tabel daftar barang/pengguna.
- `User/UserTemplate.cs` menjadi tempat tema visual untuk form pengguna.
- `Report/ReportBarangTemplate.cs` dan `Report/ReportTransaksiTemplate.cs` menjadi tempat tema visual untuk jendela laporan.

Semua template fitur dipanggil setelah `InitializeComponent()`, sehingga aturan warna, font, tombol, input, split container, dan grid diterapkan dari template. Kode-behind form tetap menangani event, validasi, proses database, dan alur bisnis.

## Catatan WinForms Designer

Untuk form legacy Barang, Penjualan, Grid, Pengguna, dan Laporan, `*.Designer.cs` tetap membuat control tree serta posisi dasar supaya nama field yang dipakai alur bisnis lama tetap kompatibel. Template fitur menerapkan tema visual saat runtime setelah control tersebut dibuat. Shell/dashboard dan login yang dibuat secara dinamis sudah dikomposisikan langsung oleh template masing-masing.
