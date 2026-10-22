using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DAAccess;
using Toko_Root_ID.UI;

namespace Toko_Root_ID.Master
{
    /// <summary>Form master data: barang, kategori, merek, satuan, supplier, dan pelanggan.</summary>
    public class frmMaster : Form
    {
        private readonly DAMaster _master = new DAMaster();
        private readonly MasterTemplate.MasterView _view;

        public frmMaster()
        {
            _view = MasterTemplate.Build(this);
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleBuffered = true;

            WireBarang();
            WireSimpleTabs();
            LoadAll();
        }

        // =========================================================
        // PEMUATAN DATA
        // =========================================================
        private void LoadAll()
        {
            LoadLookups();
            LoadBarang();
            LoadKategori();
            LoadMerek();
            LoadSatuan();
            LoadSupplier();
            LoadPelanggan();
        }

        private void LoadLookups()
        {
            try
            {
                UiKit.FillCombo(_view.Kategori, _master.GetKategori(), "kategori_id", "nama");
                _view.Kategori.SelectedIndex = 0;
                UiKit.FillCombo(_view.Merek, _master.GetMerek(), "merek_id", "nama");
                _view.Merek.SelectedIndex = 0;
                UiKit.FillCombo(_view.Satuan, _master.GetSatuan(), "satuan_id", "nama");
                _view.Satuan.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void LoadBarang()
        {
            try
            {
                DataTable data = _master.GetBarang(UiKit.SearchText(_view.BarangSearch));
                _view.BarangGrid.DataSource = data;
                FormatBarangGrid();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void FormatBarangGrid()
        {
            DataGridView grid = _view.BarangGrid;
            if (grid.Columns.Count == 0) return;
            SetHeader(grid, "kodebarang", "Kode", 70);
            SetHeader(grid, "namabarang", "Nama Barang", 170);
            SetHeader(grid, "kategori", "Kategori", 100);
            SetHeader(grid, "merek", "Merek", 90);
            SetHeader(grid, "satuan", "Satuan", 60);
            SetHeader(grid, "harga_beli", "H. Beli", 80);
            SetHeader(grid, "hargasatuan", "H. Jual", 80);
            SetHeader(grid, "stok", "Stok", 55);
            SetHeader(grid, "stok_min", "Min", 50);
            SetHeader(grid, "status_stok", "Status", 70);
            Hide(grid, "kategori_id");
            Hide(grid, "merek_id");
            Hide(grid, "satuan_id");
            Hide(grid, "barcode");
            Money(grid, "harga_beli");
            Money(grid, "hargasatuan");
            Number(grid, "stok");
            Number(grid, "stok_min");
        }

        private static void SetHeader(DataGridView grid, string column, string text, int width)
        {
            if (grid.Columns[column] == null) return;
            grid.Columns[column].HeaderText = text;
            grid.Columns[column].FillWeight = width;
        }

        private static void Hide(DataGridView grid, string column)
        {
            if (grid.Columns[column] != null) grid.Columns[column].Visible = false;
        }

        private static void Money(DataGridView grid, string column)
        {
            if (grid.Columns[column] != null)
                grid.Columns[column].DefaultCellStyle.Format = "N0";
        }

        private static void Number(DataGridView grid, string column)
        {
            if (grid.Columns[column] != null)
                grid.Columns[column].DefaultCellStyle.Format = "0.###";
        }

        private void LoadKategori()
        {
            BindSimple(_view.KategoriTab, _master.GetKategori(), "kategori_id",
                new[] { "nama", "keterangan" }, "nama");
        }

        private void LoadMerek()
        {
            BindSimple(_view.MerekTab, _master.GetMerek(), "merek_id",
                new[] { "nama" }, "nama");
        }

        private void LoadSatuan()
        {
            BindSimple(_view.SatuanTab, _master.GetSatuan(), "satuan_id",
                new[] { "nama" }, "nama");
        }

        private void LoadSupplier()
        {
            BindSimple(_view.SupplierTab, _master.GetSupplier(UiKit.SearchText(_view.SupplierTab.Search)),
                "supplier_id", new[] { "kode", "nama", "telepon", "email", "alamat" }, "nama");
        }

        private void LoadPelanggan()
        {
            BindSimple(_view.PelangganTab, _master.GetPelanggan(UiKit.SearchText(_view.PelangganTab.Search)),
                "pelanggan_id", new[] { "kode", "nama", "telepon", "email", "alamat" }, "nama");
        }

        private void BindSimple(MasterTemplate.SimpleEditor editor, DataTable data,
            string idColumn, string[] fieldColumns, string nameColumn)
        {
            editor.Grid.DataSource = data;
            if (editor.Grid.Columns.Count == 0) return;

            SetHeader(editor.Grid, "kode", "Kode", 70);
            SetHeader(editor.Grid, "nama", "Nama", 160);
            SetHeader(editor.Grid, "keterangan", "Keterangan", 160);
            SetHeader(editor.Grid, "telepon", "Telepon", 90);
            SetHeader(editor.Grid, "email", "Email", 130);
            SetHeader(editor.Grid, "alamat", "Alamat", 170);
            SetHeader(editor.Grid, "saldo_hutang", "Hutang", 90);
            SetHeader(editor.Grid, "saldo_piutang", "Piutang", 90);
            SetHeader(editor.Grid, "is_active", "Aktif", 55);
            SetHeader(editor.Grid, "created_at", "Dibuat", 90);
            Hide(editor.Grid, idColumn);
            Money(editor.Grid, "saldo_hutang");
            Money(editor.Grid, "saldo_piutang");
            if (editor.Grid.Columns["is_active"] != null)
                editor.Grid.Columns["is_active"].DefaultCellStyle.Format = "0";

            // Simpan referensi kolom pada Tag agar pengisian editor tidak perlu menebak.
            editor.Grid.Tag = fieldColumns;
            editor.Grid.CellClick -= GridCellClick;
            editor.Grid.CellClick += GridCellClick;
        }

        private void GridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridView grid = (DataGridView)sender;
            DataRowView row = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (row == null) return;

            MasterTemplate.SimpleEditor editor = FindEditor(grid);
            if (editor == null) return;
            string[] fields = grid.Tag as string[];
            if (fields == null) return;

            for (int i = 0; i < editor.Fields.Length && i < fields.Length; i++)
                editor.Fields[i].Text = Convert.ToString(row[fields[i]]);

            if (editor.Active != null && row.Row.Table.Columns.Contains("is_active"))
                editor.Active.Checked = Convert.ToInt32(row["is_active"]) == 1;
        }

        private MasterTemplate.SimpleEditor FindEditor(DataGridView grid)
        {
            if (grid == _view.KategoriTab.Grid) return _view.KategoriTab;
            if (grid == _view.MerekTab.Grid) return _view.MerekTab;
            if (grid == _view.SatuanTab.Grid) return _view.SatuanTab;
            if (grid == _view.SupplierTab.Grid) return _view.SupplierTab;
            if (grid == _view.PelangganTab.Grid) return _view.PelangganTab;
            return null;
        }

        // =========================================================
        // EVENT BARANG
        // =========================================================
        private void WireBarang()
        {
            _view.BarangRefresh.Click += delegate { LoadBarang(); LoadLookups(); };
            _view.BarangNew.Click += delegate { ClearBarangEditor(); };
            _view.BarangSave.Click += delegate { SaveBarang(); };
            _view.BarangDelete.Click += delegate { DeleteBarang(); };

            _view.BarangSearch.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                LoadBarang();
            };

            _view.BarangGrid.CellClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                DataRowView row = _view.BarangGrid.Rows[e.RowIndex].DataBoundItem as DataRowView;
                if (row == null) return;
                _view.Kode.Text = Convert.ToString(row["kodebarang"]);
                _view.Nama.Text = Convert.ToString(row["namabarang"]);
                _view.Barcode.Text = Convert.ToString(row["barcode"]);
                _view.HargaBeli.Text = Convert.ToString(row["harga_beli"]);
                _view.HargaJual.Text = Convert.ToString(row["hargasatuan"]);
                _view.Stok.Text = Convert.ToString(row["stok"]);
                _view.StokMin.Text = Convert.ToString(row["stok_min"]);
                SelectCombo(_view.Kategori, "kategori_id", row["kategori_id"]);
                SelectCombo(_view.Merek, "merek_id", row["merek_id"]);
                SelectCombo(_view.Satuan, "satuan_id", row["satuan_id"]);
            };
        }

        private static void SelectCombo(ComboBox combo, string valueMember, object value)
        {
            if (value == null || value == DBNull.Value) { combo.SelectedIndex = 0; return; }
            try { combo.SelectedValue = Convert.ToInt32(value); }
            catch { combo.SelectedIndex = 0; }
        }

        private void ClearBarangEditor()
        {
            _view.Kode.Clear();
            _view.Nama.Clear();
            _view.Barcode.Clear();
            _view.HargaBeli.Text = "0";
            _view.HargaJual.Text = "0";
            _view.Stok.Text = "0";
            _view.StokMin.Text = "0";
            _view.Kategori.SelectedIndex = 0;
            _view.Merek.SelectedIndex = 0;
            _view.Satuan.SelectedIndex = 0;
            _view.BarangGrid.ClearSelection();
            _view.Kode.Focus();
        }

        private void SaveBarang()
        {
            try
            {
                int? kategoriId = ComboId(_view.Kategori);
                int? merekId = ComboId(_view.Merek);
                int? satuanId = ComboId(_view.Satuan);
                string satuanNama = satuanId.HasValue ? Convert.ToString(_view.Satuan.Text) : null;

                string pesan = _master.SaveBarang(_view.Kode.Text.Trim(), _view.Nama.Text.Trim(),
                    kategoriId, merekId, satuanId, satuanNama, _view.Barcode.Text.Trim(),
                    UiKit.ToDecimal(_view.HargaBeli), UiKit.ToDecimal(_view.HargaJual),
                    UiKit.ToDecimal(_view.Stok), UiKit.ToDecimal(_view.StokMin));

                UiKit.ShowInfo(this, pesan);
                LoadBarang();
                LoadLookups();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private void DeleteBarang()
        {
            string kode = _view.Kode.Text.Trim();
            if (kode.Length == 0)
            {
                UiKit.ShowInfo(this, "Pilih barang yang akan dihapus.");
                return;
            }
            if (UiKit.Confirm(this, "Hapus barang " + kode + "?") != DialogResult.Yes) return;
            try
            {
                UiKit.ShowInfo(this, _master.DeleteBarang(kode));
                ClearBarangEditor();
                LoadBarang();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private static int? ComboId(ComboBox combo)
        {
            if (combo.SelectedIndex <= 0 || combo.SelectedValue == null) return null;
            int id;
            return int.TryParse(Convert.ToString(combo.SelectedValue), out id) && id > 0 ? (int?)id : null;
        }

        // =========================================================
        // EVENT TAB SEDERHANA
        // =========================================================
        private void WireSimpleTabs()
        {
            // Kategori
            WireSearch(_view.KategoriTab.Search, LoadKategori);
            _view.KategoriTab.Refresh.Click += delegate { LoadKategori(); };
            _view.KategoriTab.New.Click += delegate { ClearSimple(_view.KategoriTab); };
            _view.KategoriTab.Save.Click += delegate
            {
                try
                {
                    int id = SelectedId(_view.KategoriTab, "kategori_id");
                    UiKit.ShowInfo(this, _master.SaveKategori(id, _view.KategoriTab.Fields[0].Text,
                        _view.KategoriTab.Fields[1].Text, Active(_view.KategoriTab)));
                    LoadKategori();
                    LoadLookups();
                }
                catch (Exception ex) { UiKit.ShowError(this, ex); }
            };
            _view.KategoriTab.Delete.Click += delegate { DeleteSimple(_view.KategoriTab, "kategori_id", _master.DeleteKategori, LoadKategori); };

            // Merek
            WireSearch(_view.MerekTab.Search, LoadMerek);
            _view.MerekTab.Refresh.Click += delegate { LoadMerek(); };
            _view.MerekTab.New.Click += delegate { ClearSimple(_view.MerekTab); };
            _view.MerekTab.Save.Click += delegate
            {
                try
                {
                    int id = SelectedId(_view.MerekTab, "merek_id");
                    UiKit.ShowInfo(this, _master.SaveMerek(id, _view.MerekTab.Fields[0].Text, Active(_view.MerekTab)));
                    LoadMerek();
                    LoadLookups();
                }
                catch (Exception ex) { UiKit.ShowError(this, ex); }
            };
            _view.MerekTab.Delete.Click += delegate { DeleteSimple(_view.MerekTab, "merek_id", _master.DeleteMerek, LoadMerek); };

            // Satuan
            WireSearch(_view.SatuanTab.Search, LoadSatuan);
            _view.SatuanTab.Refresh.Click += delegate { LoadSatuan(); };
            _view.SatuanTab.New.Click += delegate { ClearSimple(_view.SatuanTab); };
            _view.SatuanTab.Save.Click += delegate
            {
                try
                {
                    int id = SelectedId(_view.SatuanTab, "satuan_id");
                    UiKit.ShowInfo(this, _master.SaveSatuan(id, _view.SatuanTab.Fields[0].Text, Active(_view.SatuanTab)));
                    LoadSatuan();
                    LoadLookups();
                }
                catch (Exception ex) { UiKit.ShowError(this, ex); }
            };
            _view.SatuanTab.Delete.Click += delegate { DeleteSimple(_view.SatuanTab, "satuan_id", _master.DeleteSatuan, LoadSatuan); };

            // Supplier
            WireSearch(_view.SupplierTab.Search, LoadSupplier);
            _view.SupplierTab.Refresh.Click += delegate { LoadSupplier(); };
            _view.SupplierTab.New.Click += delegate { ClearSimple(_view.SupplierTab); };
            _view.SupplierTab.Save.Click += delegate
            {
                try
                {
                    TextBox[] f = _view.SupplierTab.Fields;
                    int id = SelectedId(_view.SupplierTab, "supplier_id");
                    UiKit.ShowInfo(this, _master.SaveSupplier(id, f[0].Text, f[1].Text, f[2].Text,
                        f[3].Text, f[4].Text, Active(_view.SupplierTab)));
                    LoadSupplier();
                }
                catch (Exception ex) { UiKit.ShowError(this, ex); }
            };
            _view.SupplierTab.Delete.Click += delegate { DeleteSimple(_view.SupplierTab, "supplier_id", _master.DeleteSupplier, LoadSupplier); };

            // Pelanggan
            WireSearch(_view.PelangganTab.Search, LoadPelanggan);
            _view.PelangganTab.Refresh.Click += delegate { LoadPelanggan(); };
            _view.PelangganTab.New.Click += delegate { ClearSimple(_view.PelangganTab); };
            _view.PelangganTab.Save.Click += delegate
            {
                try
                {
                    TextBox[] f = _view.PelangganTab.Fields;
                    int id = SelectedId(_view.PelangganTab, "pelanggan_id");
                    UiKit.ShowInfo(this, _master.SavePelanggan(id, f[0].Text, f[1].Text, f[2].Text,
                        f[3].Text, f[4].Text, Active(_view.PelangganTab)));
                    LoadPelanggan();
                }
                catch (Exception ex) { UiKit.ShowError(this, ex); }
            };
            _view.PelangganTab.Delete.Click += delegate { DeleteSimple(_view.PelangganTab, "pelanggan_id", _master.DeletePelanggan, LoadPelanggan); };
        }

        private static void WireSearch(TextBox box, Action reload)
        {
            box.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                reload();
            };
        }

        private static bool Active(MasterTemplate.SimpleEditor editor)
        {
            return editor.Active == null || editor.Active.Checked;
        }

        private static int SelectedId(MasterTemplate.SimpleEditor editor, string idColumn)
        {
            if (editor.Grid.CurrentRow == null) return 0;
            DataRowView row = editor.Grid.CurrentRow.DataBoundItem as DataRowView;
            if (row == null || !row.Row.Table.Columns.Contains(idColumn)) return 0;
            return Convert.ToInt32(row[idColumn]);
        }

        private void DeleteSimple(MasterTemplate.SimpleEditor editor, string idColumn,
            Func<int, string> delete, Action reload)
        {
            int id = SelectedId(editor, idColumn);
            if (id <= 0)
            {
                UiKit.ShowInfo(this, "Pilih data yang akan dihapus.");
                return;
            }
            if (UiKit.Confirm(this, "Hapus data terpilih?") != DialogResult.Yes) return;
            try
            {
                UiKit.ShowInfo(this, delete(id));
                ClearSimple(editor);
                reload();
            }
            catch (Exception ex)
            {
                UiKit.ShowError(this, ex);
            }
        }

        private static void ClearSimple(MasterTemplate.SimpleEditor editor)
        {
            foreach (TextBox field in editor.Fields) field.Clear();
            if (editor.Active != null) editor.Active.Checked = true;
            editor.Grid.ClearSelection();
        }
    }
}
