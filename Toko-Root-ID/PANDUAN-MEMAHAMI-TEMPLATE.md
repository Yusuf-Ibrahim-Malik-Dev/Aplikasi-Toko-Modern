# Panduan Memahami File Template Toko Root-ID

Dokumen ini menjelaskan fungsi file template yang mengatur tampilan aplikasi. Komentar di dalam file `.cs` juga menjelaskan tiap bagian kode yang penting.

## Pembagian tanggung jawab

- `AppTheme.cs`: palet warna global, gaya tombol/input/tabel, mode terang/gelap, dan penyimpanan pilihan tema.
- `Login/LoginTemplate.cs`: seluruh rancangan layar login modern, termasuk panel branding, input, tombol, dekorasi gradient, dan efek fokus.
- `MenuUtama/MenuUtamaTemplate.cs`: kerangka dashboard, sidebar, header, kartu metrik, grafik contoh, akses cepat, halaman tema, dan navigasi ke form fitur.
- `Barang/BarangTemplate.cs`: gaya visual form barang.
- `Penjualan/PenjualanTemplate.cs`: gaya visual layar kasir dan tabel detail transaksi.
- `Grid/grdBarangTemplate.cs` dan `Grid/grdUserTemplate.cs`: gaya visual tabel daftar barang/pengguna.
- `User/UserTemplate.cs`: gaya visual form pengguna dan penyamaran input password.
- `Report/ReportBarangTemplate.cs` dan `Report/ReportTransaksiTemplate.cs`: pengaturan jendela dan Report Viewer laporan.

## Arti properti tampilan yang sering dijumpai

| Kode | Artinya | Efek saat diubah |
|---|---|---|
| `Text` | Tulisan yang tampil di label/tombol/judul jendela | Mengubah kata-kata yang terlihat pengguna |
| `Location = new Point(x, y)` | Posisi kontrol: `x` dari kiri, `y` dari atas, dalam pixel | Memindahkan kontrol |
| `Size = new Size(width, height)` | Lebar dan tinggi kontrol, dalam pixel | Mengubah ukuran kontrol |
| `Font = new Font("Segoe UI", 10F, ...)` | Jenis font, ukuran, dan gaya huruf | Mengubah rupa dan ukuran teks |
| `BackColor` | Warna bidang/latar kontrol | Mengubah warna latar |
| `ForeColor` | Warna teks atau ikon | Mengubah warna tulisan |
| `Padding` | Ruang di dalam tepi kontrol | Membuat isi kontrol lebih berjarak dari tepi |
| `Dock` | Cara kontrol menempel atau memenuhi sisi parent | Misalnya sidebar tetap di kiri, header di atas, konten mengisi sisa ruang |
| `Anchor` | Sisi yang dijaga jaraknya ketika ukuran parent berubah | Membantu kontrol tetap berada di sisi tertentu |
| `FlatAppearance` | Warna/border tombol saat normal, hover, atau ditekan | Mengatur detail tampilan interaksi tombol |
| `Paint` | Event untuk menggambar elemen custom | Dipakai untuk gradient, garis, lingkaran, dan grafik custom |

## Cara mengganti warna utama

Buka `AppTheme.cs` dan ubah nilai RGB pada `DefaultAccent`. RGB adalah tiga angka dari 0 sampai 255: merah, hijau, biru. Untuk mengubah palet terang/gelap, sesuaikan nilai di method `ApplyPalette()`.

Jika warna baru ingin muncul sebagai opsi di halaman **Pengaturan Tema**, tambahkan satu item `new ThemePreset("Nama Tema", Color.FromArgb(R, G, B))` ke array `Presets`.

## Cara mengganti gaya tombol

Di template fitur, cari pemanggilan seperti:

```csharp
StyleNamedButton(form, "btnSave", AppTheme.ButtonStyle.Primary);
```

- `"btnSave"` adalah properti `Name` tombol pada WinForms Designer; namanya harus cocok dengan kontrol di form.
- `Primary` berarti aksi utama memakai warna aksen.
- `Secondary` berarti tombol pendamping yang lebih netral.
- `Danger` berarti aksi yang perlu perhatian, misalnya hapus atau keluar.
- `Success`, `Warning`, dan `Ghost` juga tersedia di `AppTheme.ButtonStyle`.

## Cara memindahkan kontrol

Untuk UI yang dirakit dari kode seperti login/dashboard, ubah nilai `Location` dan `Size` langsung di template tersebut. Contoh `new Point(56, 196)` berarti posisi 56 pixel dari kiri parent dan 196 pixel dari atas parent. Jika kontrol menggunakan `Dock`, nilai `Location` biasanya tidak menjadi penentu posisi akhir karena tata letak ditentukan oleh `Dock`.

## Hal yang jangan dipindahkan ke template

Template bertanggung jawab pada tampilan. Validasi, login, query database, perhitungan harga/stok, transaksi, dan event bisnis tetap ada di file form atau class service/data access. Template boleh membentuk kontrol dan mengatur gaya; form tetap menghubungkan event tombol ke logika bisnis.

> Catatan: grafik dashboard saat ini berisi angka contoh visual di `DrawSalesChart()`. Komentar di kode menandai bagian tersebut agar tidak disalahartikan sebagai laporan penjualan aktual.
