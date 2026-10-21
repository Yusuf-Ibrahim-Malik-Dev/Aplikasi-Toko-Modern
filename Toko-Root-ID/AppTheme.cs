using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Toko_Root_ID
{
    /// <summary>
    /// Pusat tema aplikasi: menyimpan palet warna, preset, gaya tombol/input/tabel,
    /// serta membaca dan menyimpan pilihan tema supaya dipakai konsisten di seluruh fitur.
    /// </summary>
    internal static class AppTheme
    {
        // Nama folder/file konfigurasi tema yang diletakkan di folder AppData milik pengguna.
        private const string FolderName = "TokoRootID";
        private const string ThemeFile = "theme.ini";
        private const string BackupFile = "theme.ini.bak";
        private const string TempFile = "theme.ini.tmp";
        private const string LegacyFile = "theme.txt";

        // Lokasi penyimpanan konfigurasi tema di %AppData%\TokoRootID; tidak ditaruh di folder program.
        private static readonly string Folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FolderName);

        private static readonly string FilePath = Path.Combine(Folder, ThemeFile);
        private static readonly string BackupPath = Path.Combine(Folder, BackupFile);
        private static readonly string TempPath = Path.Combine(Folder, TempFile);
        private static readonly string LegacyPath = Path.Combine(Folder, LegacyFile);

        // Warna default: Accent adalah warna utama; DefaultSidebarBase adalah dasar warna sidebar.
        // Color.FromArgb menerima komponen Red, Green, Blue dengan rentang 0 sampai 255.
        private static readonly Color DefaultAccent = Color.FromArgb(45, 110, 220);
        private static readonly Color DefaultSidebarBase = Color.FromArgb(18, 25, 43);

        // ==== PALET GLOBAL ====
        // Nilai-nilai ini dibaca template masing-masing fitur. Setter private menjaga perubahan lewat API tema.
        public static Color Accent { get; private set; } = DefaultAccent; // Warna utama tombol, ikon, dan menu aktif.
        public static Color Sidebar { get; private set; } = Color.FromArgb(24, 39, 75); // Warna panel navigasi.
        public static Color Background { get; private set; } = Color.FromArgb(246, 248, 252); // Latar halaman.
        public static Color Surface { get; private set; } = Color.White; // Latar kartu/panel/komponen.
        public static Color Text { get; private set; } = Color.FromArgb(31, 41, 61); // Teks utama.
        public static Color Muted { get; private set; } = Color.FromArgb(115, 126, 146); // Teks keterangan yang lebih redup.
        public static bool IsDarkMode { get; private set; } = false; // Status apakah palet gelap sedang dipakai.

        // Warna semantik: Border=garis, Success=berhasil, Warning=peringatan, Danger=aksi berisiko,
        // Hover=warna saat diarahkan mouse, Selected=warna area/baris yang sedang dipilih.
        public static Color Border { get; private set; } = Color.FromArgb(224, 229, 238);
        public static Color Success { get; private set; } = Color.FromArgb(34, 160, 90); // Hijau: status/aksi berhasil.
        public static Color Warning { get; private set; } = Color.FromArgb(230, 160, 40); // Oranye: peringatan.
        public static Color Danger { get; private set; } = Color.FromArgb(210, 60, 70); // Merah: hapus/batal/keluar.
        public static Color Hover { get; private set; } = Color.FromArgb(238, 242, 250);
        public static Color Selected { get; private set; } = Color.FromArgb(220, 232, 250);

        // Tanda identitas ringkas untuk membandingkan kombinasi warna aksen dan mode gelap.
        public static string Signature => Accent.ToArgb() + "|" + IsDarkMode;

        // Event diberitahukan setelah tema berubah agar komponen yang berlangganan dapat menggambar ulang.
        public static event EventHandler ThemeChanged;

        // ==== DEBOUNCE EVENT ====
        // Timer merapatkan notifikasi perubahan tema yang berdekatan agar event tidak ditembak berkali-kali.
        private static Timer _debounce;
        private static EventArgs _pendingArgs;
        private static EventHandler _pendingHandler;

        // Static constructor dipanggil satu kali saat AppTheme pertama kali digunakan; muat tema tersimpan.
        static AppTheme() { Load(); }

        // =========================================================
        // PRESET (class-based, tidak pakai tuple)
        // =========================================================
        // Satu pilihan preset: Name adalah nama yang tampil di UI, Color adalah warna aksennya.
        public sealed class ThemePreset
        {
            public string Name { get; private set; }
            public Color Color { get; private set; }
            public ThemePreset(string name, Color color)
            {
                Name = name;
                Color = color;
            }
        }

        // Daftar pilihan warna siap pakai pada halaman Pengaturan Tema.
        // Tambah satu ThemePreset di array ini jika ingin menambah pilihan warna.
        public static readonly ThemePreset[] Presets =
        {
            new ThemePreset("Ocean Blue",    Color.FromArgb(45, 110, 220)),
            new ThemePreset("Emerald",       Color.FromArgb(16, 155, 112)),
            new ThemePreset("Royal Purple",  Color.FromArgb(124, 78, 210)),
            new ThemePreset("Sunset Orange", Color.FromArgb(230, 126, 34)),
            new ThemePreset("Rose Pink",     Color.FromArgb(219, 70, 120)),
            new ThemePreset("Midnight",      Color.FromArgb(52, 73, 94)),
            new ThemePreset("Teal",          Color.FromArgb(0, 150, 170)),
            new ThemePreset("Ruby Red",      Color.FromArgb(190, 55, 55)),
        };

        // =========================================================
        // PUBLIC API
        // =========================================================

        // Mengganti warna aksen dan menghitung ulang palet terkait. Fungsi ini juga menyimpan pilihan pengguna.
        public static void SetAccent(Color color)
        {
            // Hindari proses simpan/notifikasi jika warna yang dipilih sama dengan warna saat ini.
            if (Accent.ToArgb() == color.ToArgb()) return;
            Accent = color; // Warna utama untuk tombol, ikon, dan menu aktif.
            Sidebar = Blend(DefaultSidebarBase, color, 0.30); // Campurkan warna aksen ke warna dasar sidebar.
            ApplyPalette();
            Save();
            RaiseChanged();
        }

        // Mengaktifkan atau menonaktifkan mode gelap dan memperbarui seluruh warna palet.
        public static void SetDarkMode(bool enabled)
        {
            if (IsDarkMode == enabled) return; // Tidak perlu kerja jika statusnya tidak berubah.
            IsDarkMode = enabled;
            ApplyPalette();
            Save();
            RaiseChanged();
        }

        // Membalik status mode gelap dari kondisi saat ini.
        public static void ToggleDarkMode() { SetDarkMode(!IsDarkMode); }

        // Mengembalikan tema ke warna aksen default dan mode terang.
        public static void Reset()
        {
            Accent = DefaultAccent;
            Sidebar = Blend(DefaultSidebarBase, Accent, 0.30);
            IsDarkMode = false;
            ApplyPalette();
            Save();
            RaiseChanged();
        }

        // Mencari preset berdasarkan nama lalu menerapkan warna yang ditemukan.
        public static void ApplyPreset(string presetName)
        {
            Color color;
            if (TryGetPreset(presetName, out color))
                SetAccent(color);
        }

        // Mencari nama preset tanpa membedakan huruf besar/kecil. Return false jika tidak ditemukan.
        // Nilai warna dikembalikan melalui parameter out.
        public static bool TryGetPreset(string name, out Color color)
        {
            color = DefaultAccent;
            if (string.IsNullOrWhiteSpace(name)) return false;
            foreach (var p in Presets)
            {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    color = p.Color;
                    return true;
                }
            }
            return false;
        }

        // =========================================================
        // COLOR HELPERS
        // =========================================================

        // Mencerahkan warna c dengan mencampurkannya ke putih. amount 0=warna asli, 1=putih.
        public static Color Tint(Color c, double amount)
        {
            amount = Clamp01(amount);
            return Color.FromArgb(
                Clamp((int)(c.R + (255 - c.R) * amount)),
                Clamp((int)(c.G + (255 - c.G) * amount)),
                Clamp((int)(c.B + (255 - c.B) * amount)));
        }

        // Menggelapkan warna c. amount 0=warna asli, mendekati 1=hitam.
        public static Color Shade(Color c, double amount)
        {
            amount = Clamp01(amount);
            return Color.FromArgb(
                Clamp((int)(c.R * (1 - amount))),
                Clamp((int)(c.G * (1 - amount))),
                Clamp((int)(c.B * (1 - amount))));
        }

        // Mencampur dua warna secara proporsional; t=0 menghasilkan a, t=1 menghasilkan b.
        public static Color Blend(Color a, Color b, double t)
        {
            t = Clamp01(t);
            return Color.FromArgb(
                Clamp((int)(a.R * (1 - t) + b.R * t)),
                Clamp((int)(a.G * (1 - t) + b.G * t)),
                Clamp((int)(a.B * (1 - t) + b.B * t)));
        }

        // Memilih warna teks gelap atau putih agar teks tetap mudah dibaca di atas background.
        public static Color ContrastText(Color background)
        {
            double luma = Luma(background);
            return luma > 150 ? Color.FromArgb(31, 41, 61) : Color.White;
        }

        // Helper untuk menggolongkan warna berdasarkan kecerahan yang dihitung di Luma().
        public static bool IsLight(Color c) { return Luma(c) > 150; }
        public static bool IsDark(Color c) { return !IsLight(c); }

        // Menghitung perkiraan kecerahan mata manusia dari komponen RGB.
        private static double Luma(Color c)
        {
            return 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
        }

        // =========================================================
        // STYLE HELPERS
        // =========================================================

        // Jenis tombol standar. Template memilih salah satu sesuai fungsi tombolnya.
        public enum ButtonStyle { Primary, Secondary, Danger, Success, Ghost, Warning }

        // Satu pintu untuk tampilan tombol, termasuk warna normal, hover, dan saat ditekan.
        public static void StyleButton(Button btn, ButtonStyle style)
        {
            if (btn == null) return; // Abaikan referensi kosong agar tidak terjadi NullReferenceException.
            btn.FlatStyle = FlatStyle.Flat; // Pakai gaya datar agar tampilan dapat dikustomisasi.
            btn.FlatAppearance.BorderSize = 0; // Hilangkan border default.
            btn.Cursor = Cursors.Hand; // Kursor tangan menandakan tombol bisa diklik.
            btn.UseVisualStyleBackColor = false; // Gunakan BackColor yang ditentukan tema.

            // Pilih warna latar (back) dan teks (fore) berdasarkan jenis tombol.
            Color back, fore;
            switch (style)
            {
                // Primary: aksi utama, memakai warna aksen global.
                case ButtonStyle.Primary: back = Accent; fore = ContrastText(Accent); break;
                // Secondary: aksi pendamping, lebih netral dan memiliki border.
                case ButtonStyle.Secondary:
                    back = Surface; fore = Text;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = Border;
                    break;
                // Danger untuk hapus/keluar atau aksi yang perlu perhatian.
                case ButtonStyle.Danger: back = Danger; fore = Color.White; break;
                // Success untuk aksi berhasil/konfirmasi positif.
                case ButtonStyle.Success: back = Success; fore = Color.White; break;
                // Warning untuk aksi yang membutuhkan perhatian.
                case ButtonStyle.Warning: back = Warning; fore = Color.White; break;
                // Ghost transparan: tombol tampak ringan tanpa bidang warna.
                case ButtonStyle.Ghost: back = Color.Transparent; fore = Accent; break;
                default: back = Accent; fore = Color.White; break;
            }
            // Terapkan warna utama dan dua state interaksi mouse.
            btn.BackColor = back;
            btn.ForeColor = fore;
            btn.FlatAppearance.MouseOverBackColor = Blend(back, Color.White, 0.15); // Sedikit lebih terang saat hover.
            btn.FlatAppearance.MouseDownBackColor = Shade(back, 0.15); // Sedikit lebih gelap saat ditekan.
        }

        // Overload default param — untuk kompatibilitas pemanggil lama
        public static void StyleButton(Button btn)
        {
            StyleButton(btn, ButtonStyle.Primary);
        }

        // Gaya dasar TextBox: bidang input, teks, dan border mengikuti palet aktif.
        public static void StyleTextBox(TextBox t)
        {
            if (t == null) return;
            t.BackColor = Surface; // Warna kotak input.
            t.ForeColor = Text; // Warna teks yang diketik.
            t.BorderStyle = BorderStyle.FixedSingle; // Border tipis standar.
        }

        // Gaya dropdown ComboBox supaya selaras dengan TextBox.
        public static void StyleComboBox(ComboBox cb)
        {
            if (cb == null) return;
            cb.BackColor = Surface; // Latar dropdown.
            cb.ForeColor = Text; // Warna teks pilihan.
            cb.FlatStyle = FlatStyle.Flat; // Gaya visual dropdown yang lebih sederhana.
        }

        // Gaya standar tabel data (DataGridView), termasuk header, baris biasa, baris selang-seling, dan seleksi.
        public static void StyleDataGridView(DataGridView dgv)
        {
            if (dgv == null) return;
            dgv.BackgroundColor = Background; // Latar area tabel di luar sel.
            dgv.GridColor = Border; // Warna garis pemisah antar sel.
            dgv.BorderStyle = BorderStyle.None; // Hilangkan bingkai luar default.
            dgv.EnableHeadersVisualStyles = false; // Izinkan warna header dikendalikan tema.

            // HEADER KOLOM: latar permukaan, teks tebal, dan warna seleksi tetap seragam.
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Surface;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Surface;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;

            // ISI TABEL: warna normal dan warna saat baris dipilih.
            dgv.DefaultCellStyle.BackColor = Surface;
            dgv.DefaultCellStyle.ForeColor = Text;
            dgv.DefaultCellStyle.SelectionBackColor = Selected;
            dgv.DefaultCellStyle.SelectionForeColor = Text;

            // Baris selang-seling memakai warna latar agar tabel lebih mudah dipindai.
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Background;
            dgv.RowHeadersVisible = false; // Sembunyikan kolom nomor/baris bawaan di sisi kiri.
        }

        // Mengatur warna dasar form, lalu meneruskan styling ke semua kontrol turunannya.
        public static void StyleForm(Form form)
        {
            if (form == null) return;
            form.BackColor = Background; // Warna latar jendela.
            form.ForeColor = Text; // Warna teks standar yang diwariskan kontrol.
            ApplyToChildren(form); // Terapkan warna pada semua kontrol turunan.
        }

        // Menerapkan palet pada kontrol berdasarkan jenis kontrol dan Tag opsional.
        // Tag "primary", "danger", "secondary", "muted", "accent", "surface", atau "background"
        // dapat dipakai untuk meminta variasi gaya tertentu dari Designer.
        public static void ApplyToChildren(Control parent)
        {
            if (parent == null) return;
            foreach (Control c in parent.Controls)
            {
                // Tag membantu mempertahankan gaya khusus tanpa harus memeriksa nama tiap kontrol.
                string tag = c.Tag as string;
                switch (c)
                {
                    case Panel p:
                        if (tag == "surface") p.BackColor = Surface; // Tag surface: panel seperti kartu.
                        else if (tag == "background") p.BackColor = Background; // Tag background: panel latar halaman.
                        break;
                    case Label l:
                        if (tag == "muted") l.ForeColor = Muted; // Tag muted: teks sekunder.
                        else if (tag == "accent") l.ForeColor = Accent; // Tag accent: teks yang menonjol.
                        else l.ForeColor = Text;
                        break;
                    case TextBox t: StyleTextBox(t); break;
                    case ComboBox cb: StyleComboBox(cb); break;
                    case DataGridView dgv: StyleDataGridView(dgv); break;
                    case Button b:
                        if (tag == "primary") StyleButton(b, ButtonStyle.Primary); // Tombol utama.
                        else if (tag == "danger") StyleButton(b, ButtonStyle.Danger); // Tombol berisiko.
                        else if (tag == "secondary") StyleButton(b, ButtonStyle.Secondary); // Tombol pendamping.
                        break;
                }
                if (c.HasChildren) ApplyToChildren(c);
            }
        }

        // =========================================================
        // INTERNAL
        // =========================================================

        // Mengisi ulang seluruh warna semantik berdasarkan mode terang/gelap.
        // Template membaca properti warna ini, sehingga perubahan di sini berlaku ke semua fitur.
        private static void ApplyPalette()
        {
            if (IsDarkMode)
            {
                Background = Color.FromArgb(18, 22, 32); // Palet dasar mode gelap.
                Surface = Color.FromArgb(28, 34, 48); // Kartu sedikit lebih terang daripada latar.
                Text = Color.FromArgb(232, 236, 245);
                Muted = Color.FromArgb(148, 158, 176);
                Border = Color.FromArgb(52, 60, 78);
                Hover = Color.FromArgb(40, 48, 66);
                Selected = Blend(Accent, Surface, 0.65);
            }
            else
            {
                Background = Color.FromArgb(246, 248, 252); // Palet dasar mode terang.
                Surface = Color.White; // Kartu putih di atas latar abu muda.
                Text = Color.FromArgb(31, 41, 61);
                Muted = Color.FromArgb(115, 126, 146);
                Border = Color.FromArgb(224, 229, 238);
                Hover = Color.FromArgb(238, 242, 250);
                Selected = Blend(Accent, Color.White, 0.80);
            }
            Sidebar = Blend(DefaultSidebarBase, Accent, 0.30);
        }

        // Menunda notifikasi ThemeChanged sebentar agar perubahan warna beruntun digabung menjadi satu event.
        private static void RaiseChanged()
        {
            if (_debounce == null)
            {
                _debounce = new Timer { Interval = 50 };
                _debounce.Tick += (s, e) =>
                {
                    _debounce.Stop();
                    // Ambil handler/eventArgs terbaru, kosongkan referensi sementara, lalu panggil subscriber.
                    var h = _pendingHandler;
                    var a = _pendingArgs;
                    _pendingHandler = null;
                    _pendingArgs = null;
                    if (h != null) h(null, a ?? EventArgs.Empty);
                };
            }

            _pendingHandler = ThemeChanged;
            _pendingArgs = EventArgs.Empty;

            _debounce.Stop();
            _debounce.Start();
        }

        // =========================================================
        // PERSISTENCE
        // =========================================================

        // Simpan warna aksen dan mode gelap ke file konfigurasi pengguna.
        // File sementara digunakan lebih dulu supaya file utama tidak rusak jika penulisan terputus.
        private static void Save()
        {
            try
            {
                if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);

                var sb = new StringBuilder();
                sb.AppendLine("# TokoRootID theme – auto generated");
                sb.AppendLine("# " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine("AccentR=" + Accent.R);
                sb.AppendLine("AccentG=" + Accent.G);
                sb.AppendLine("AccentB=" + Accent.B);
                sb.AppendLine("DarkMode=" + (IsDarkMode ? "1" : "0"));

                // Tulis ke file sementara sebelum mengganti file tema yang sedang dipakai.
                File.WriteAllText(TempPath, sb.ToString());

                // Jika file lama ada, buat backup lalu coba mengganti file secara atomik.
                if (File.Exists(FilePath))
                {
                    try
                    {
                        File.Copy(FilePath, BackupPath, true);
                        File.Replace(TempPath, FilePath, null);
                    }
                    catch
                    {
                        // Fallback untuk file system yang tidak mendukung File.Replace.
                        File.Copy(TempPath, FilePath, true);
                        File.Delete(TempPath);
                    }
                }
                else
                {
                    File.Move(TempPath, FilePath);
                }
            }
            catch { }
        }

        // Urutan pemuatan: konfigurasi utama, backup jika file utama gagal, lalu format lama theme.txt.
        private static void Load()
        {
            if (TryLoadIni(FilePath)) return;
            if (TryLoadIni(BackupPath)) return;
            TryLoadLegacy(LegacyPath);
        }

        // Membaca format key=value. Return true jika minimal ada pengaturan yang berhasil dibaca.
        private static bool TryLoadIni(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;

                int r = DefaultAccent.R, g = DefaultAccent.G, b = DefaultAccent.B;
                bool dark = false;
                bool anyRead = false;

                // Baca satu baris demi satu baris; baris kosong dan komentar (#) diabaikan.
                foreach (var raw in File.ReadAllLines(path))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    // Pisahkan nama pengaturan di kiri tanda '=' dan nilai di kanannya.
                    string key = line.Substring(0, eq).Trim();
                    string val = line.Substring(eq + 1).Trim();

                    if (key == "AccentR") { int rr; if (int.TryParse(val, out rr)) { r = rr; anyRead = true; } }
                    else if (key == "AccentG") { int gg; if (int.TryParse(val, out gg)) { g = gg; anyRead = true; } }
                    else if (key == "AccentB") { int bb; if (int.TryParse(val, out bb)) { b = bb; anyRead = true; } }
                    else if (key == "DarkMode")
                    {
                        dark = val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase);
                        anyRead = true;
                    }
                }

                if (!anyRead) return false;

                // Pastikan RGB tidak keluar rentang 0..255 sebelum membentuk warna baru.
                Accent = Color.FromArgb(Clamp(r), Clamp(g), Clamp(b));
                IsDarkMode = dark;
                ApplyPalette();
                return true;
            }
            catch { return false; }
        }

        // Kompatibilitas untuk file tema versi lama dengan format "R,G,B".
        // Setelah terbaca, preferensi lama dipindahkan ke format INI melalui Save().
        private static void TryLoadLegacy(string path)
        {
            try
            {
                if (!File.Exists(path)) return;
                var parts = File.ReadAllText(path).Split(',');
                if (parts.Length != 3) return;

                int r, g, b;
                if (int.TryParse(parts[0], out r) &&
                    int.TryParse(parts[1], out g) &&
                    int.TryParse(parts[2], out b))
                {
                    Accent = Color.FromArgb(Clamp(r), Clamp(g), Clamp(b));
                    Sidebar = Blend(DefaultSidebarBase, Accent, 0.30);
                    IsDarkMode = false;
                    ApplyPalette();
                    Save();
                }
            }
            catch { }
        }

        // =========================================================
        // UTILS
        // =========================================================

        // Batasi bilangan warna RGB ke 0..255.
        private static int Clamp(int n) { return Math.Max(0, Math.Min(255, n)); }
        // Batasi angka proporsi warna ke 0..1.
        private static double Clamp01(double v) { return v < 0 ? 0 : (v > 1 ? 1 : v); }
    }
}