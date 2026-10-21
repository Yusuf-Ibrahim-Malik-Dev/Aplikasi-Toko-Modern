using System;
using System.Drawing;
using System.Windows.Forms;

namespace Toko_Root_ID.Grid
{
    /// <summary>Rancangan visual daftar barang dan tabel data.</summary>
    /// <remarks>Gunakan file ini untuk tampilan daftar barang; event pemilihan/refresh/print tetap ada di form.</remarks>
    internal static class grdBarangTemplate
    {
        // Titik masuk template: panggil setelah InitializeComponent() agar semua kontrol Designer sudah tersedia.
        public static void Apply(Form form)
        {
            // Jangan diam-diam memproses null; jika pemanggil salah, tampilkan error yang jelas.
            if (form == null) throw new ArgumentNullException(nameof(form));
            form.Text = "Root-ID | Daftar Barang"; // Judul jendela daftar barang.
            ApplyVisuals(form); // Gaya bersama untuk semua kontrol daftar.
            // Tombol refresh/print dan tombol navigasi biasanya sekunder; aksi utama memakai Accent.
            StyleNamedButton(form, "btnRefresh", AppTheme.ButtonStyle.Secondary); // Muat ulang data.
            StyleNamedButton(form, "btnPrint", AppTheme.ButtonStyle.Secondary); // Cetak/preview.
            StyleNamedButton(form, "button1", AppTheme.ButtonStyle.Primary); // Tombol aksi utama dari Designer.
            StyleNamedButton(form, "button2", AppTheme.ButtonStyle.Secondary); // Tombol aksi pendamping dari Designer.
        }

        // Mengubah gaya tombol berdasarkan Name dari Designer. Pencarian rekursif (true)
        // berarti tombol yang berada di dalam Panel/GroupBox juga bisa ditemukan.
        private static void StyleNamedButton(Form form, string name, AppTheme.ButtonStyle style)
        {
            Control[] found = form.Controls.Find(name, true);
            // Pastikan kontrol ditemukan dan benar-benar Button sebelum di-cast.
            if (found.Length > 0 && found[0] is Button) AppTheme.StyleButton((Button)found[0], style);
        }

        // Menetapkan gaya umum form lebih dulu, lalu merapikan tiap jenis kontrol di dalamnya.
        private static void ApplyVisuals(Form form)
        {
            AppTheme.StyleForm(form); // Terapkan palet global dan warna standar ke kontrol turunan.
            form.Font = new Font("Segoe UI", 9F, FontStyle.Regular); // Font dasar form.
            form.StartPosition = FormStartPosition.CenterScreen; // Form dibuka di tengah layar.
            form.BackColor = AppTheme.Background; // Warna bidang belakang jendela.
            StyleTree(form); // Terapkan gaya spesifik ke tombol, label, input, tabel, dan panel.
        }

        // Menelusuri seluruh kontrol hingga ke kontrol bersarang. Cara ini membuat
        // styling konsisten walaupun tombol atau input berada di dalam beberapa panel.
        private static void StyleTree(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                // Tombol diberi warna berdasarkan nama atau teks yang menunjukkan tujuannya.
                if (control is Button)
                {
                    Button button = (Button)control;
                    string key = ((button.Name ?? string.Empty) + " " + (button.Text ?? string.Empty)).ToLowerInvariant();
                    // Aksi destruktif/keluar memakai warna merah agar mudah dikenali.
                    if (key.Contains("delete") || key.Contains("hapus") || key.Contains("close") || key.Contains("keluar"))
                        AppTheme.StyleButton(button, AppTheme.ButtonStyle.Danger);
                    // Aksi bantuan atau navigasi memakai gaya sekunder yang lebih netral.
                    else if (key.Contains("refresh") || key.Contains("print") || key.Contains("view") || key.Contains("kembali") || key.Contains("dashboard") || key.Contains("button2"))
                        AppTheme.StyleButton(button, AppTheme.ButtonStyle.Secondary);
                    // Aksi utama seperti simpan/update memakai warna aksen.
                    else if (key.Contains("update") || key.Contains("simpan") || key.Contains("save") || key.Contains("ok"))
                        AppTheme.StyleButton(button, AppTheme.ButtonStyle.Primary);
                    else
                        AppTheme.StyleButton(button, AppTheme.ButtonStyle.Primary);
                    button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                // Label berukuran kecil dianggap sebagai teks pendukung; label lain menjadi teks utama.
                else if (control is Label)
                {
                    Label label = (Label)control;
                    label.Font = new Font("Segoe UI", label.Font.Size, label.Font.Style);
                    label.ForeColor = label.Font.Size <= 8.5F ? AppTheme.Muted : AppTheme.Text;
                }
                // Samakan ukuran font input teks supaya konsisten dengan gaya aplikasi.
                else if (control is TextBox)
                {
                    ((TextBox)control).Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
                // Dropdown menggunakan font yang sama dengan TextBox.
                else if (control is ComboBox)
                {
                    ((ComboBox)control).Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
                // Tabel data menggunakan warna tema, baris lebih nyaman dibaca, dan seleksi satu baris.
                else if (control is DataGridView)
                {
                    DataGridView grid = (DataGridView)control;
                    AppTheme.StyleDataGridView(grid);
                    grid.RowTemplate.Height = 30; // Tinggi baris dalam pixel.
                    grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Seleksi satu baris penuh.
                    grid.MultiSelect = false; // Satu baris saja dapat dipilih setiap waktu.
                    grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Kolom memenuhi lebar tabel.
                }
                // SplitContainer membagi area menjadi dua; warna panel dan garis pemisah diselaraskan.
                else if (control is SplitContainer)
                {
                    SplitContainer split = (SplitContainer)control;
                    split.BackColor = AppTheme.Border;
                    split.Panel1.BackColor = AppTheme.Surface;
                    split.Panel2.BackColor = AppTheme.Background;
                    split.SplitterWidth = Math.Max(split.SplitterWidth, 5); // Tebal pembagi minimal 5 pixel.
                }
                // Panel biasa dijadikan bidang/kartu dengan warna Surface.
                else if (control is Panel)
                {
                    ((Panel)control).BackColor = AppTheme.Surface;
                }

                // Jika kontrol memiliki anak, ulangi proses untuk semua kontrol di dalamnya.
                if (control.HasChildren) StyleTree(control);
            }
        }
    }
}
