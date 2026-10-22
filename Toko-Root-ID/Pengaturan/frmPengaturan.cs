using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Pengaturan
{
    /// <summary>Form pengaturan profil toko, tema aplikasi, dan informasi sistem.</summary>
    public class frmPengaturan : Form
    {
        private readonly PengaturanTemplate.PengaturanView _view;
        private readonly DAMaster _master = new DAMaster();
        private readonly DALaporan _laporan = new DALaporan();

        public frmPengaturan()
        {
            _view = PengaturanTemplate.Build(this);
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleBuffered = true;

            PasangEvent();
            MuatProfil();
            MuatInfo();
            PerbaruiStatusTema();
        }

        private void PasangEvent()
        {
            _view.NameBox.TextChanged += delegate { PerbaruiPratinjau(); };
            _view.AddressBox.TextChanged += delegate { PerbaruiPratinjau(); };
            _view.PhoneBox.TextChanged += delegate { PerbaruiPratinjau(); };
            _view.FooterBox.TextChanged += delegate { PerbaruiPratinjau(); };
            _view.SaveButton.Click += delegate { SimpanProfil(); };

            _view.DarkModeButton.Click += delegate
            {
                AppTheme.SetDarkMode(!AppTheme.IsDarkMode);
                TerapkanTema();
            };
            _view.ResetThemeButton.Click += delegate
            {
                AppTheme.Reset();
                TerapkanTema();
            };

            foreach (Button button in _view.PresetButtons)
            {
                Button preset = button;
                preset.Click += delegate
                {
                    AppTheme.ApplyPreset(Convert.ToString(preset.Tag));
                    TerapkanTema();
                };
            }

            _view.RefreshInfoButton.Click += delegate { MuatInfo(); };
        }

        // =========================================================
        // PROFIL
        // =========================================================
        private void MuatProfil()
        {
            try
            {
                ClassProfilToko profil = _master.GetProfil();
                _view.NameBox.Text = profil.nama_toko;
                _view.AddressBox.Text = profil.alamat;
                _view.PhoneBox.Text = profil.telepon;
                _view.EmailBox.Text = profil.email;
                _view.FooterBox.Text = profil.footer_struk;
                PerbaruiPratinjau();
            }
            catch (Exception ex)
            {
                _view.StatusLabel.Text = "Gagal memuat profil: " + ex.Message;
            }
        }

        private void PerbaruiPratinjau()
        {
            _view.PreviewName.Text = string.IsNullOrWhiteSpace(_view.NameBox.Text) ? "NAMA TOKO" : _view.NameBox.Text;
            _view.PreviewAddress.Text = _view.AddressBox.Text.Replace("\r\n", " ").Replace("\n", " ");
            _view.PreviewPhone.Text = _view.PhoneBox.Text;
            _view.PreviewFooter.Text = string.IsNullOrWhiteSpace(_view.FooterBox.Text)
                ? "Terima kasih telah berbelanja" : _view.FooterBox.Text;
        }

        private void SimpanProfil()
        {
            try
            {
                var profil = new ClassProfilToko
                {
                    nama_toko = _view.NameBox.Text.Trim(),
                    alamat = _view.AddressBox.Text.Trim(),
                    telepon = _view.PhoneBox.Text.Trim(),
                    email = _view.EmailBox.Text.Trim(),
                    footer_struk = _view.FooterBox.Text.Trim()
                };
                UiKit.ShowInfo(this, _master.SaveProfil(profil));
                _view.StatusLabel.Text = "Profil tersimpan pada " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + ".";
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        // =========================================================
        // TEMA
        // =========================================================
        private void TerapkanTema()
        {
            FormUtama owner = Tag as FormUtama;
            if (owner != null)
            {
                owner.RebuildAndOpenPengaturan();
                return;
            }
            PerbaruiStatusTema();
            _view.ThemeStatus.Text = "Tema disimpan. Buka ulang halaman untuk melihat seluruh perubahan.";
        }

        private void PerbaruiStatusTema()
        {
            _view.DarkModeButton.Text = AppTheme.IsDarkMode ? "Matikan Mode Gelap" : "Aktifkan Mode Gelap";
            _view.ThemeStatus.Text = "Mode " + (AppTheme.IsDarkMode ? "gelap" : "terang") +
                " aktif. Warna aksen: RGB(" + AppTheme.Accent.R + ", " + AppTheme.Accent.G + ", " + AppTheme.Accent.B + ").";
        }

        // =========================================================
        // TENTANG
        // =========================================================
        private void MuatInfo()
        {
            try
            {
                ClassDashboard dash = _laporan.GetDashboard();
                DataTable roles = _master.GetRole();
                DataTable gudang = _master.GetGudang();

                _view.InfoLabel.Text =
                    "Versi aplikasi   : 1.0 (Store Edition)\n" +
                    "Framework        : .NET Framework 4.5.2\n" +
                    "Database         : dbToko (SQL Server)\n" +
                    "Jumlah produk    : " + dash.jumlah_produk.ToString("N0") + "\n" +
                    "Total stok       : " + dash.total_stok.ToString("N0") + " unit\n" +
                    "Role aktif       : " + roles.Rows.Count + "\n" +
                    "Gudang aktif     : " + gudang.Rows.Count + "\n" +
                    "Transaksi hari ini: " + dash.transaksi_hari_ini.ToString("N0") + "\n" +
                    "Omzet hari ini   : " + UiKit.Rupiah(dash.omzet_hari_ini) + "\n" +
                    "Tema aktif       : " + (AppTheme.IsDarkMode ? "Gelap" : "Terang") + "\n" +
                    "Status koneksi   : Terhubung";
            }
            catch (Exception ex)
            {
                _view.InfoLabel.Text = "Database belum tersambung.\n" + ex.Message;
            }
        }
    }
}
