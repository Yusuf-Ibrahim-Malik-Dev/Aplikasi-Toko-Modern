using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Toko_Root_ID;

namespace Toko_Root_ID.Login
{
    /// <summary>
    /// Seluruh rancangan visual, ukuran, warna, font, posisi, dan dekorasi
    /// layar login diletakkan di template ini. Form hanya menghubungkan event
    /// dan menjalankan logika autentikasi.
    /// </summary>
    internal static class LoginTemplate
    {
        // Wadah referensi kontrol yang dibutuhkan form untuk menangani event dan login.
        // Desainnya dibuat di bawah, tetapi form tetap dapat mengakses TextBox/tombol melalui objek ini.
        internal sealed class LoginView
        {
            public Panel BrandPanel { get; internal set; }
            public Panel FormPanel { get; internal set; }
            public TextBox Username { get; internal set; }
            public TextBox Password { get; internal set; }
            public Button LoginButton { get; internal set; }
            public Button ExitButton { get; internal set; }
            public CheckBox ShowPassword { get; internal set; }
            public Label ErrorLabel { get; internal set; }
        }

        // Titik masuk untuk merakit seluruh tampilan login dan mengembalikan referensi kontrolnya.
        // legacyControls adalah kontrol lama dari Designer yang disembunyikan agar tidak bertumpuk.
        public static LoginView Build(Form form, Control[] legacyControls)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            // SuspendLayout mencegah WinForms menghitung ulang posisi berulang kali saat UI disusun.
            form.SuspendLayout();
            // Pengaturan jendela utama: judul, posisi awal, ukuran tetap, dan tema dasar.
            form.Text = "Root-ID | Login";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.ClientSize = new Size(880, 520);
            form.MinimumSize = new Size(880, 520);
            form.MaximumSize = new Size(880, 520);
            form.BackColor = AppTheme.Surface;
            form.Font = new Font("Segoe UI", 9F);
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;

            // Sembunyikan kontrol lama yang sudah diganti dengan tampilan modern.
            // Kontrol tidak dihapus dari Designer sehingga event/kode lama tetap bisa dipelihara.
            if (legacyControls != null)
            {
                foreach (Control control in legacyControls)
                    if (control != null) control.Visible = false;
            }

            // Buat sisi branding dan sisi input. Referensi TextBox/tombol disimpan ke LoginView.
            var view = new LoginView();
            view.BrandPanel = BuildBrandPanel();
            view.FormPanel = BuildFormPanel(view);
            // Tambahkan panel konten terlebih dahulu, lalu panel kiri branding.
            form.Controls.Add(view.FormPanel);
            form.Controls.Add(view.BrandPanel);
            // Aktifkan kembali layout dan hitung posisi akhir semua kontrol.
            form.ResumeLayout(true);
            return view;
        }

        // Membuat panel kiri yang berisi logo, slogan, daftar fitur, dan dekorasi gradient.
        private static Panel BuildBrandPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Left, // Panel menempel di kiri jendela.
                Width = 400, // Lebar kolom branding dalam pixel.
                BackColor = AppTheme.Accent // Warna fallback sebelum gambar gradient digambar.
            };

            // Paint dipanggil ulang saat panel perlu digambar. Semua dekorasi vektor diletakkan di sini.
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Gradient diagonal dari warna aksen ke warna aksen yang lebih gelap.
                using (var brush = new LinearGradientBrush(panel.ClientRectangle,
                    AppTheme.Accent, AppTheme.Shade(AppTheme.Accent, 0.45), 45f))
                    g.FillRectangle(brush, panel.ClientRectangle);
                // Lingkaran putih transparan sebagai ornamen dekoratif pada panel kiri.
                using (var blob = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                {
                    g.FillEllipse(blob, -80, -80, 280, 280);
                    g.FillEllipse(blob, panel.Width - 150, panel.Height - 200, 300, 300);
                    g.FillEllipse(blob, panel.Width - 100, -120, 220, 220);
                }
                // Garis tipis transparan di bawah panel untuk menambah detail visual.
                using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1.5f))
                    g.DrawLine(pen, 40, panel.Height - 90, 120, panel.Height - 90);
            };

            // Logo sederhana berbentuk kotak. Ubah Text/Font/Location untuk mengganti tampilannya.
            panel.Controls.Add(new Label
            {
                Text = "T", Location = new Point(40, 50), Size = new Size(56, 56),
                BackColor = Color.White, ForeColor = AppTheme.Accent,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            });
            // Nama merek yang sejajar dengan kotak logo.
            panel.Controls.Add(new Label
            {
                Text = "TOKO MODERN", Location = new Point(108, 60), Size = new Size(260, 26),
                ForeColor = Color.White, Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                BackColor = Color.Transparent
            });
            // Subjudul kecil di bawah nama merek.
            panel.Controls.Add(new Label
            {
                Text = "STORE MANAGEMENT", Location = new Point(108, 84), Size = new Size(260, 18),
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), BackColor = Color.Transparent
            });
            // Slogan utama di panel branding. Tanda \n membuat teks turun ke baris berikutnya.
            panel.Controls.Add(new Label
            {
                Text = "Kelola toko lebih\nmodern, cepat,\ndan terpercaya.",
                Location = new Point(40, 210), Size = new Size(330, 120),
                ForeColor = Color.White, Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                BackColor = Color.Transparent
            });
            // Tiap fitur terdiri dari ikon cek dan teks; angka terakhir adalah posisi vertikal (Y).
            AddFeature(panel, "✓", "Real-time monitoring stok", 350);
            AddFeature(panel, "✓", "Transaksi aman & cepat", 382);
            AddFeature(panel, "✓", "Laporan otomatis", 414);
            // Informasi versi aplikasi di bagian bawah. Anchor membuat jarak dari bawah tetap terjaga.
            panel.Controls.Add(new Label
            {
                Text = "●  v2.0.0  •  Desktop", Location = new Point(40, panel.Height - 40),
                Size = new Size(300, 20), ForeColor = Color.FromArgb(200, 255, 255, 255),
                Font = new Font("Segoe UI", 8F), BackColor = Color.Transparent,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            });
            return panel;
        }

        // Helper agar daftar fitur memakai format ikon + teks yang sama.
        // y mengatur jarak vertikal; icon dan text menentukan isi baris fitur.
        private static void AddFeature(Panel panel, string icon, string text, int y)
        {
            panel.Controls.Add(new Label
            {
                Text = icon, Location = new Point(40, y), Size = new Size(20, 22),
                ForeColor = Color.FromArgb(230, 255, 255, 255),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), BackColor = Color.Transparent
            });
            panel.Controls.Add(new Label
            {
                Text = text, Location = new Point(66, y + 2), Size = new Size(300, 22),
                ForeColor = Color.FromArgb(230, 255, 255, 255),
                Font = new Font("Segoe UI", 9.5F), BackColor = Color.Transparent
            });
        }

        // Membuat panel kanan berisi judul, input username/password, checkbox, pesan error, dan tombol.
        // Kontrol penting disimpan di LoginView supaya form dapat memasang event dan membaca input.
        private static Panel BuildFormPanel(LoginView view)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill, // Mengisi sisa jendela selain panel branding di kiri.
                BackColor = AppTheme.Surface, // Warna permukaan utama.
                Padding = new Padding(56, 70, 56, 40) // Ruang dalam: kiri, atas, kanan, bawah.
            };
            // Judul halaman login.
            panel.Controls.Add(new Label
            {
                Text = "Selamat Datang", Location = new Point(56, 80), Size = new Size(400, 40),
                ForeColor = AppTheme.Text, Font = new Font("Segoe UI", 24F, FontStyle.Bold)
            });
            // Teks penjelas singkat tepat di bawah judul.
            panel.Controls.Add(new Label
            {
                Text = "Masuk untuk melanjutkan ke dashboard Anda",
                Location = new Point(58, 122), Size = new Size(400, 22),
                ForeColor = AppTheme.Muted, Font = new Font("Segoe UI", 9.5F)
            });

            // Label dan input username. Ubah koordinat Y untuk memindahkan satu baris input.
            AddFieldLabel(panel, "USERNAME", 170);
            TextBox usernameTextBox;
            var usernameWrapper = MakeModernTextBox(out usernameTextBox, 196);
            view.Username = usernameTextBox;
            panel.Controls.Add(usernameWrapper);

            // Baris password berada di bawah username dengan pola ukuran yang sama.
            AddFieldLabel(panel, "PASSWORD", 258);
            TextBox passwordTextBox;
            var passwordWrapper = MakeModernTextBox(out passwordTextBox, 284);
            view.Password = passwordTextBox;
            panel.Controls.Add(passwordWrapper);

            // Checkbox ini referensinya dikirim ke form untuk menangani event CheckedChanged.
            view.ShowPassword = new CheckBox
            {
                Text = "Lihat password", Location = new Point(58, 340), Size = new Size(140, 22),
                ForeColor = AppTheme.Muted, Font = new Font("Segoe UI", 8.5F), Cursor = Cursors.Hand
            };
            panel.Controls.Add(view.ShowPassword);
            // Label pesan error disembunyikan sampai validasi/login gagal.
            view.ErrorLabel = new Label
            {
                Text = "", Location = new Point(58, 368), Size = new Size(400, 20),
                ForeColor = AppTheme.Danger, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Visible = false
            };
            panel.Controls.Add(view.ErrorLabel);
            // Tombol login. Event Click-nya dipasang oleh form agar logika autentikasi tetap di form.
            view.LoginButton = new Button
            {
                Text = "LOGIN", Location = new Point(56, 398), Size = new Size(430, 46),
                BackColor = AppTheme.Accent, ForeColor = AppTheme.ContrastText(AppTheme.Accent),
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            // Hilangkan bingkai default dan berikan warna saat mouse hover/diklik.
            view.LoginButton.FlatAppearance.BorderSize = 0;
            view.LoginButton.FlatAppearance.MouseOverBackColor = AppTheme.Tint(AppTheme.Accent, 0.10);
            view.LoginButton.FlatAppearance.MouseDownBackColor = AppTheme.Shade(AppTheme.Accent, 0.10);
            panel.Controls.Add(view.LoginButton);

            // Tombol keluar dibuat bergaya link agar tidak bersaing secara visual dengan LOGIN.
            view.ExitButton = new Button
            {
                Text = "Keluar dari aplikasi", Location = new Point(56, 452), Size = new Size(430, 26),
                BackColor = Color.Transparent, ForeColor = AppTheme.Muted,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F), Cursor = Cursors.Hand
            };
            view.ExitButton.FlatAppearance.BorderSize = 0;
            view.ExitButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            panel.Controls.Add(view.ExitButton);
            return panel;
        }

        // Helper pembuat label USERNAME/PASSWORD agar font dan warnanya selalu seragam.
        // parent adalah panel tujuan, text adalah tulisan, dan y adalah posisi vertikalnya.
        private static void AddFieldLabel(Control parent, string text, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text, Location = new Point(58, y), Size = new Size(400, 18),
                ForeColor = AppTheme.Muted, Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            });
        }

        // TextBox dibungkus Panel karena TextBox standar tidak mudah diberi border modern custom.
        // Parameter out mengembalikan TextBox aslinya, sedangkan return mengembalikan panel pembungkus.
        private static Panel MakeModernTextBox(out TextBox textBox, int y)
        {
            var wrapper = new Panel
            {
                Location = new Point(56, y), // Posisi X tetap; Y diberikan pemanggil untuk username/password.
                Size = new Size(430, 44), // Lebar dan tinggi kotak input.
                BackColor = AppTheme.Background, // Warna saat input tidak fokus.
                Padding = new Padding(14, 10, 14, 8) // Ruang di dalam kotak di sekitar TextBox.
            };
            // Menggambar border tipis panel pembungkus. Event Paint membuat garis muncul saat repaint.
            wrapper.Paint += (s, e) =>
            {
                using (var pen = new Pen(AppTheme.Border, 1.2f))
                    e.Graphics.DrawRectangle(pen, 0, 0, wrapper.Width - 1, wrapper.Height - 1);
            };
            // Buat TextBox tanpa border bawaan karena border dibuat oleh wrapper Panel.
            textBox = new TextBox
            {
                BorderStyle = BorderStyle.None, Dock = DockStyle.Fill,
                BackColor = AppTheme.Background, ForeColor = AppTheme.Text,
                Font = new Font("Segoe UI", 11F), TextAlign = HorizontalAlignment.Left
            };
            // Saat cursor masuk ke input, ubah latar dan ganti border ke warna aksen (focus state).
            textBox.Enter += (s, e) =>
            {
                wrapper.BackColor = AppTheme.Surface;
                wrapper.Paint += PaintAccentBorder;
                wrapper.Invalidate();
            };
            // Saat input kehilangan fokus, kembalikan warna normal dan lepaskan handler border aksen.
            textBox.Leave += (s, e) =>
            {
                wrapper.BackColor = AppTheme.Background;
                wrapper.Paint -= PaintAccentBorder;
                wrapper.Invalidate();
            };
            // Masukkan TextBox ke wrapper; return wrapper supaya seluruh kotak bisa ditambahkan ke form.
            wrapper.Controls.Add(textBox);
            return wrapper;
        }

        // Menggambar border fokus yang lebih tebal dan memakai warna aksen tema.
        // sender adalah panel wrapper yang sedang digambar.
        private static void PaintAccentBorder(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            using (var pen = new Pen(AppTheme.Accent, 1.8f))
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        }
    }
}
