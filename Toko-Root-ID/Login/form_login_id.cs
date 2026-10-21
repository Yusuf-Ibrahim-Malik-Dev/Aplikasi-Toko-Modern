using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Helper;
using Toko_Root_ID.Login;

namespace Toko_Root_ID
{
    public partial class form_login_id : Form
    {
        // Kontrol visual dibuat dan ditata sepenuhnya oleh LoginTemplate.
        private LoginTemplate.LoginView _loginView;
        private TextBox _txtUsernameModern;
        private TextBox _txtPasswordModern;
        private Button _btnLoginModern;
        private CheckBox _chkShowPassword;
        private Label _lblError;
        private bool _isLoggingIn = false;

        public form_login_id()
        {
            InitializeComponent();

            _loginView = LoginTemplate.Build(this, new Control[]
            {
                label1, label2, label3, panel1, panel2,
                txtUsername, txtPassword, btnLogin, btnCancel, button1
            });

            _txtUsernameModern = _loginView.Username;
            _txtPasswordModern = _loginView.Password;
            _btnLoginModern = _loginView.LoginButton;
            _chkShowPassword = _loginView.ShowPassword;
            _lblError = _loginView.ErrorLabel;

            // Event dan alur aplikasi tetap menjadi tanggung jawab form.
            _txtUsernameModern.MaxLength = 64;
            _txtUsernameModern.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    _txtPasswordModern.Focus();
                }
            };

            _txtPasswordModern.UseSystemPasswordChar = true;
            _txtPasswordModern.MaxLength = 128;
            _txtPasswordModern.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    DoLogin();
                }
            };

            _chkShowPassword.CheckedChanged += (s, e) =>
                _txtPasswordModern.UseSystemPasswordChar = !_chkShowPassword.Checked;
            _btnLoginModern.Click += (s, e) => DoLogin();
            _loginView.ExitButton.Click += (s, e) => BtnCancel_Click(s, e);

            KeyPreview = true;
            ActiveControl = _txtUsernameModern;
        }

        private async void DoLogin()
        {

            if (_isLoggingIn) return;

            string username = _txtUsernameModern.Text.Trim();
            string password = _txtPasswordModern.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Username wajib diisi.");
                _txtUsernameModern.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                ShowError("Password wajib diisi.");
                _txtPasswordModern.Focus();
                return;
            }

            HideError();
            _isLoggingIn = true;
            _btnLoginModern.Enabled = false;
            _btnLoginModern.Text = "MEMPROSES...";
            Cursor = Cursors.WaitCursor;

            try
            {
                string hasil = null;
                await Task.Run(() =>
                {
                    ClassUser obj = new ClassUser { nik = username, nama = username, Password = password };
                    using (DAUser proses = new DAUser())
                        hasil = proses.CheckLogin(obj);
                });

                if (hasil == "1")
                {
                    // Login successful
                    _txtPasswordModern.Clear();

                    // Set role-based start menu
                    var myform = new FormUtama();

                    // Set menu based on role
                    if (SessionManager.IsOwner() || SessionManager.IsAdmin())
                    {
                        // Full access - enable all menu items
                        myform.EnableAllMenus();
                    }
                    else if (SessionManager.IsManager())
                    {
                        // Manager access - limited menus
                        myform.EnableManagerMenus();
                    }
                    else if (SessionManager.IsKasir())
                    {
                        // Kasir access - only POS and reports
                        myform.EnableKasirMenus();
                    }
                    else if (SessionManager.IsGudang())
                    {
                        // Gudang access - only inventory
                        myform.EnableGudangMenus();
                    }

                    myform.Show();
                    myform.BringToFront();
                    myform.Activate();
                    Hide();
                }
                else if (hasil == "-1")
                {
                    ShowError("Akun Anda tidak aktif. Hubungi administrator.");
                    _txtPasswordModern.Clear();
                    _txtPasswordModern.Focus();
                }
                else
                {
                    ShowError("Username atau password salah.");
                    _txtPasswordModern.Clear();
                    _txtPasswordModern.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError("Error: " + ex.Message);
            }
            finally
            {
                _isLoggingIn = false;
                _btnLoginModern.Enabled = true;
                _btnLoginModern.Text = "LOGIN";
                Cursor = Cursors.Default;
            }
        }

        private void ShowError(string msg)
        {
            if (_lblError == null) return;
            _lblError.Text = "\u26A0  " + msg;
            _lblError.Visible = true;
        }

        private void HideError()
        {
            if (_lblError != null) _lblError.Visible = false;
        }

        // =========================================================
        // DESIGNER COMPAT STUBS
        // Kontrol lama di-hide, handler tetap ada agar .Designer.cs tidak error
        // =========================================================
        private void btnLogin_Click(object sender, EventArgs e) { DoLogin(); }
        private void btnCancel_Click(object sender, EventArgs e) { BtnCancel_Click(sender, e); }
        private void button1_Click(object sender, EventArgs e)
        {
#if DEBUG
            var myform = new FormUtama();
            myform.Show();
            Hide();
#endif
        }

        // Handler untuk tombol exit baru
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            var konfirmasi = MessageBox.Show(
                "Benarkah Anda ingin keluar dari aplikasi Root-ID-1994?",
                "Konfirmasi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (konfirmasi == DialogResult.Yes) Application.Exit();
        }

        // =========================================================
        // DESIGNER COMPAT (event lama)
        // =========================================================
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                DoLogin();
            }
        }

        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                if (_txtPasswordModern != null) _txtPasswordModern.Focus();
            }
        }

        private void txtLevel_SelectedIndexChanged(object sender, EventArgs e) { /* tidak dipakai */ }
    }
}
