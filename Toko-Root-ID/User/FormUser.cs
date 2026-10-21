
using System;
using System.Windows.Forms;
using CommonClass;
using DAAccess;
using Helper;

namespace Toko_Root_ID
{
    public partial class FormUser : Form
    {
        public ClassUser obj = new ClassUser();
        public string status;

        private string _existingPasswordHash = string.Empty;
        private bool _isEdit = false;

        public FormUser()
        {
            InitializeComponent();
            Toko_Root_ID.User.UserTemplate.Apply(this);
            LoadRole();
            SetupGridUser();
            LoadDataUser();
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            FormUtama host = Tag as FormUtama;
            if (host != null) { host.NavigateDaftarPengguna(); return; }
            grdUser yusuf3 = new grdUser();
            yusuf3.Show();
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Handler kosong lama dipertahankan.
        }

        private void FormUser_Activated(object sender, EventArgs e)
        {
            // Tambahkan proses jika diperlukan.
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show(
                    "Nama wajib diisi.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtNama.Focus();
                return;
            }

            // Password wajib untuk user baru.
            if (!_isEdit && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show(
                    "Password wajib diisi.",
                    "Validasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtPassword.Focus();
                return;
            }

            try
            {
                ClassUser data = bindFormToObject();

                // Expired otomatis 5 tahun dari sekarang.
                data.expireddate = DateTime.Now.AddYears(5).ToString("yyyy-MM-dd");

                // Hash password baru sebelum disimpan.
                if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    data.Password = PasswordHelper.HashPassword(
                        txtPassword.Text);
                }
                else if (_isEdit &&
                         !string.IsNullOrEmpty(_existingPasswordHash))
                {
                    // Password tidak diganti: pertahankan hash lama.
                    data.Password = _existingPasswordHash;
                }
                else
                {
                    MessageBox.Show(
                        "Password belum tersedia.",
                        "Validasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                using (DAUser proses = new DAUser())
                {
                    string hasil = proses.SubmitUser(data);

                    MessageBox.Show(
                        hasil,
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                SetupGridUser();
                LoadDataUser();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal menyimpan data user.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private ClassUser bindFormToObject()
        {
            ClassUser data = new ClassUser();

            // Ambil NIK yang sudah diisi pada form.
            string nik = txtNik.Text.Trim();

            // Cek apakah NIK tersebut sudah terdaftar.
            if (!string.IsNullOrWhiteSpace(nik))
            {
                using (DAUser da = new DAUser())
                {
                    ClassUser existingUser = da.GetDataByUserId(nik);

                    if (existingUser != null &&
                        !string.IsNullOrWhiteSpace(existingUser.nik))
                    {
                        // Data ditemukan: gunakan NIK yang sudah ada.
                        data.nik = existingUser.nik;
                    }
                    else
                    {
                        // Data belum ditemukan: generate NIK baru.
                        using (DAPermission permission = new DAPermission())
                        {
                            data.nik = permission.GenerateNik();
                        }
                    }
                }
            }
            else
            {
                // NIK kosong: generate NIK baru.
                using (DAPermission permission = new DAPermission())
                {
                    data.nik = permission.GenerateNik();
                }
            }

            txtNik.Text = data.nik;

            data.nama = txtNama.Text.Trim();
            data.expireddate = null;

            // Mengambil role_id dari ComboBox.
            if (comboBox1.SelectedValue == null)
            {
                throw new InvalidOperationException(
                    "Silakan pilih role terlebih dahulu.");
            }

            data.role_id = Convert.ToInt32(comboBox1.SelectedValue);

            return data;
        }



        private void txtNik_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter)
                return;

            e.Handled = true;

            try
            {
                string kode = txtNik.Text.Trim();

                if (string.IsNullOrWhiteSpace(kode))
                {
                    MessageBox.Show(
                        "NIK wajib diisi.",
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    txtNik.Focus();
                    return;
                }

                using (DAUser da = new DAUser())
                {
                    ClassUser data = da.GetDataByUserId(kode);

                    if (data == null || string.IsNullOrEmpty(data.nik))
                    {
                        _isEdit = false;
                        _existingPasswordHash = string.Empty;

                        txtNama.Clear();
                        txtPassword.Clear();

                        MessageBox.Show(
                            "User belum terdaftar. Silakan isi data user baru.",
                            "Info",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        txtNama.Focus();
                    }
                    else
                    {
                        _isEdit = true;

                        txtNama.Text = data.nama;
                        _existingPasswordHash = data.Password;
                        txtPassword.Clear();

                        txtPassword.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal mengambil data user.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadRole()
        {
            try
            {
                using (DAPermission da = new DAPermission())
                {
                    comboBox1.DataSource = da.GetDataRole();
                    comboBox1.DisplayMember = "role_name";
                    comboBox1.ValueMember = "role_id";
                    comboBox1.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal memuat data role.\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void SetupGridUser()
        {
            dgvUser.AutoGenerateColumns = false;
            dgvUser.Columns.Clear();

            dgvUser.AllowUserToAddRows = false;
            dgvUser.AllowUserToDeleteRows = false;
            dgvUser.ReadOnly = true;
            dgvUser.MultiSelect = false;
            dgvUser.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUser.RowHeadersVisible = false;
            dgvUser.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "nik",
                HeaderText = "NIK",
                DataPropertyName = "nik",
                FillWeight = 25
            });

            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "nama",
                HeaderText = "Nama User",
                DataPropertyName = "nama",
                FillWeight = 35
            });

            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "role_name",
                HeaderText = "Role",
                DataPropertyName = "role_name",
                FillWeight = 20
            });

            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "role_id",
                HeaderText = "Role ID",
                DataPropertyName = "role_id",
                Visible = false
            });

            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "is_active",
                HeaderText = "Status",
                DataPropertyName = "is_active",
                FillWeight = 15
            });

            dgvUser.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "btnEdit",
                HeaderText = "Edit",
                Text = "Edit",
                UseColumnTextForButtonValue = true,
                FillWeight = 12
            });

            dgvUser.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "btnDelete",
                HeaderText = "Delete",
                Text = "Delete",
                UseColumnTextForButtonValue = true,
                FillWeight = 12
            });

            dgvUser.CellContentClick -= dgvUser_CellContentClick;
            dgvUser.CellContentClick += dgvUser_CellContentClick;
        }

        private void dgvUser_CellContentClick(
                object sender,
                DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = dgvUser.Rows[e.RowIndex];

            string nik = Convert.ToString(row.Cells["nik"].Value);
            string nama = Convert.ToString(row.Cells["nama"].Value);


            // EDIT
            if (dgvUser.Columns[e.ColumnIndex].Name == "btnEdit")
            {
                txtNik.Text = nik;
                txtNama.Text = nama;
                comboBox1.SelectedValue = Convert.ToInt32(row.Cells["role_id"].Value);

                _isEdit = true;
                txtNik.ReadOnly = true;

                // Jika ada ComboBox role:
                // cmbRole.SelectedValue = row.Cells["role_id"].Value;

                // Jika ada CheckBox status:
                // chkAktif.Checked = Convert.ToBoolean(
                //     row.Cells["is_active"].Value);

                txtNama.Focus();
                return;
            }

            // DELETE
            if (dgvUser.Columns[e.ColumnIndex].Name == "btnDelete")
            {
                DialogResult result = MessageBox.Show(
                    "Hapus user ini?\n\nNIK: " + nik +
                    "\nNama: " + nama,
                    "Konfirmasi Hapus",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;

                try
                {
                    using (DAUser da = new DAUser())
                    {
                        da.DeleteUser(nik);
                    }

                    LoadDataUser();

                    MessageBox.Show(
                        "User berhasil dihapus.",
                        "Informasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Gagal menghapus user:\n" + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void LoadDataUser()
        {
            try
            {
                using (DAPermission da = new DAPermission())
                {
                    // Mengambil data user dan menampilkannya ke grid.
                    dgvUser.DataSource = da.GetAllUser();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal mengambil data user:\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

    }
}
