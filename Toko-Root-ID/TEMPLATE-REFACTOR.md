# Template Refactor — Root-ID

## Prinsip pemisahan

- `Login/LoginTemplate.cs` membangun seluruh UI login (panel branding, field, tombol, error state, dan focus border). `form_login_id.cs` menangani autentikasi dan event.
- `MenuUtama/MenuUtamaTemplate.cs` membangun shell aplikasi, sidebar berkelompok, dashboard, kartu metrik, chart, aksi cepat, dan pengaturan warna. `FormUtama.cs` menyimpan navigasi publik, mengambil data dashboard, dan menjaga hak akses role.
- `UI/UiKit.cs` menyediakan komponen halaman bersama (`Page`, `Tabs`, `Card`, `Grid`, `FillCombo`) supaya modul baru tidak menduplikasi kode tata letak.
- `UI/Charts.cs` menyediakan kontrol grafik custom yang mengikuti tema (`StatCard`, `BarChartControl`, `HBarChartControl`, `DonutChartControl`).
- `Penjualan/POSTemplate.cs` menjadi tempat tema visual kasir baru (`frmPOS`); `Penjualan/PenjualanTemplate.cs` tetap menjadi tema kasir/tabel klasik.
- `Pembelian/PembelianTemplate.cs` menjadi tempat tema visual pembelian, riwayat, dan struk pembelian.
- `Master/MasterTemplate.cs`, `Inventory/InventoryTemplate.cs`, `Keuangan/KeuanganTemplate.cs`, `Laporan/LaporanTemplate.cs`, `Retur/ReturTemplate.cs`, dan `Pengaturan/PengaturanTemplate.cs` menjadi tempat tema visual modul masing-masing.
- `Barang/BarangTemplate.cs` menjadi tempat tema visual untuk form barang lama.
- `Grid/grdBarangTemplate.cs` dan `Grid/grdUserTemplate.cs` menjadi tempat tema visual untuk tabel daftar barang/pengguna.
- `User/UserTemplate.cs` menjadi tempat tema visual untuk form pengguna.
- `Report/ReportBarangTemplate.cs` dan `Report/ReportTransaksiTemplate.cs` menjadi tempat tema visual untuk jendela laporan.

Semua template fitur dipanggil setelah `InitializeComponent()`, sehingga aturan warna, font, tombol, input, split container, dan grid diterapkan dari template. Kode-behind form tetap menangani event, validasi, proses database, dan alur bisnis.

## Modul baru dan pola pemanggilan

Modul baru (`POS`, `Pembelian`, `Master`, `Inventory`, `Keuangan`, `Laporan`, `Retur`, `Pengaturan`) dibangun sepenuhnya dari kode. Form memanggil template di konstruktor:

```csharp
public frmMaster()
{
    InitializeComponent();
    _view = MasterTemplate.Build(this);
}
```

`POSTemplate.Build(this, namaKasir)` menerima nama kasir karena header struk membutuhkannya. Template lain memakai `Build(this)`; template legacy memakai `Apply(this)`.

Template memakai `UiKit.Page(this, ...)` untuk membuat header, area aksi, dan badan halaman, lalu mengisi `page.Body` dengan `Tabs`, `Card`, `Grid`, dan kontrol input. Dengan pola ini, Designer tidak perlu memuat kontrol baru dan tema gelap/terang tetap konsisten.

## Hak akses role

`MenuUtamaTemplate.CanOpen(namaMenu)` adalah satu-satunya sumber kebenaran untuk menu yang boleh dibuka. `FormUtama` memanggilnya lewat `BolehBuka()` sebelum membuat form, sehingga menu yang disembunyikan untuk role Kasir juga tidak bisa dibuka lewat pemanggilan method langsung. Aksi cepat di dashboard memakai pengecekan yang sama.

## Catatan WinForms Designer

Untuk form legacy Barang, Penjualan, Grid, Pengguna, dan Laporan, `*.Designer.cs` tetap membuat control tree serta posisi dasar supaya nama field yang dipakai alur bisnis lama tetap kompatibel. Template fitur menerapkan tema visual saat runtime setelah control tersebut dibuat. Shell/dashboard, login, dan modul baru yang dibuat secara dinamis sudah dikomposisikan langsung oleh template masing-masing.
