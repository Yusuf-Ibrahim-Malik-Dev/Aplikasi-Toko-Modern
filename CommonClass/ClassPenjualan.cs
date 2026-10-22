using System;
using System.Collections.Generic;

namespace CommonClass
{
    /// <summary>Header transaksi penjualan (kasir/POS).</summary>
    public class ClassPenjualan
    {
        public string nofaktur { get; set; }
        public DateTime tanggal { get; set; }
        public int? pelanggan_id { get; set; }
        public decimal subtotal { get; set; }
        public decimal diskon { get; set; }
        public decimal total { get; set; }
        public decimal dibayar { get; set; }
        public decimal kembali { get; set; }
        public string metode_bayar { get; set; }
        public string kasir { get; set; }
        public string catatan { get; set; }
        public string status { get; set; }

        public ClassPenjualan()
        {
            tanggal = DateTime.Now;
            metode_bayar = "Tunai";
            status = "Posted";
        }
    }

    /// <summary>Satu baris barang pada transaksi penjualan.</summary>
    public class ClassPenjualanDetail
    {
        public string nofaktur { get; set; }
        public string kodebarang { get; set; }
        public string namabarang { get; set; }
        public decimal qty { get; set; }
        public decimal hargasatuan { get; set; }
        public decimal diskon { get; set; }
        public decimal harga_beli { get; set; }
        public decimal total { get { return (hargasatuan * qty) - diskon; } }
    }

    /// <summary>Hasil simpan transaksi: nomor dokumen dan pesan.</summary>
    public class ClassSimpanHasil
    {
        public bool sukses { get; set; }
        public string nomor { get; set; }
        public string pesan { get; set; }
        public List<string> peringatan { get; set; }

        public ClassSimpanHasil()
        {
            peringatan = new List<string>();
        }
    }
}
