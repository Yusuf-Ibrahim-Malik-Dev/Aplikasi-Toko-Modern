using System;
using System.Collections.Generic;

namespace CommonClass
{
    public class ClassSupplier
    {
        public int supplier_id { get; set; }
        public string kode { get; set; }
        public string nama { get; set; }
        public string telepon { get; set; }
        public string email { get; set; }
        public string alamat { get; set; }
        public decimal saldo_hutang { get; set; }
        public bool is_active { get; set; } = true;
    }

    public class ClassPelanggan
    {
        public int pelanggan_id { get; set; }
        public string kode { get; set; }
        public string nama { get; set; }
        public string telepon { get; set; }
        public string email { get; set; }
        public string alamat { get; set; }
        public decimal saldo_piutang { get; set; }
        public bool is_active { get; set; } = true;
    }

    public class ClassKategori
    {
        public int kategori_id { get; set; }
        public string nama { get; set; }
        public string keterangan { get; set; }
        public bool is_active { get; set; } = true;
    }

    public class ClassMerek
    {
        public int merek_id { get; set; }
        public string nama { get; set; }
        public bool is_active { get; set; } = true;
    }

    public class ClassSatuan
    {
        public int satuan_id { get; set; }
        public string nama { get; set; }
        public bool is_active { get; set; } = true;
    }

    public class ClassProfilToko
    {
        public string nama_toko { get; set; }
        public string alamat { get; set; }
        public string telepon { get; set; }
        public string email { get; set; }
        public string footer_struk { get; set; }
    }

    public class ClassKas
    {
        public long kas_id { get; set; }
        public DateTime tanggal { get; set; }
        public string tipe { get; set; }       // Masuk / Keluar
        public string kategori { get; set; }
        public decimal nominal { get; set; }
        public string metode_bayar { get; set; }
        public string referensi { get; set; }
        public string keterangan { get; set; }
        public string created_by { get; set; }

        public ClassKas()
        {
            tanggal = DateTime.Now;
            tipe = "Masuk";
            metode_bayar = "Tunai";
        }
    }

    public class ClassOpnameItem
    {
        public string kodebarang { get; set; }
        public string namabarang { get; set; }
        public decimal stok_sistem { get; set; }
        public decimal stok_fisik { get; set; }
        public decimal selisih { get { return stok_fisik - stok_sistem; } }
    }

    public class ClassTransferItem
    {
        public string kodebarang { get; set; }
        public string namabarang { get; set; }
        public decimal qty { get; set; }
    }

    public class ClassReturItem
    {
        public string kodebarang { get; set; }
        public string namabarang { get; set; }
        public decimal qty { get; set; }
        public decimal harga { get; set; }
        public decimal total { get { return qty * harga; } }
    }

    /// <summary>Ringkasan angka untuk dashboard dan kartu monitoring.</summary>
    public class ClassDashboard
    {
        public int jumlah_produk { get; set; }
        public decimal total_stok { get; set; }
        public int stok_menipis { get; set; }
        public int stok_habis { get; set; }
        public int transaksi_hari_ini { get; set; }
        public decimal omzet_hari_ini { get; set; }
        public int transaksi_bulan_ini { get; set; }
        public decimal omzet_bulan_ini { get; set; }
        public decimal total_piutang { get; set; }
        public decimal total_hutang { get; set; }
        public decimal kas_masuk_hari_ini { get; set; }
        public decimal kas_keluar_hari_ini { get; set; }
        public decimal laba_bulan_ini { get; set; }
    }

    public class ClassChartPoint
    {
        public string label { get; set; }
        public decimal value { get; set; }

        public ClassChartPoint() { }
        public ClassChartPoint(string label, decimal value)
        {
            this.label = label;
            this.value = value;
        }
    }
}
