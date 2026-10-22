using System;
using System.Collections.Generic;

namespace CommonClass
{
    /// <summary>Header transaksi pembelian dari supplier.</summary>
    public class ClassPembelian
    {
        public long pembelian_id { get; set; }
        public string nomor { get; set; }
        public DateTime tanggal { get; set; }
        public int? supplier_id { get; set; }
        public string supplier { get; set; }
        public string status { get; set; }
        public decimal subtotal { get; set; }
        public decimal diskon { get; set; }
        public decimal total { get; set; }
        public decimal dibayar { get; set; }
        public string catatan { get; set; }
        public string created_by { get; set; }

        public ClassPembelian()
        {
            tanggal = DateTime.Now;
            status = "Received";
        }
    }

    /// <summary>Satu baris barang pada transaksi pembelian.</summary>
    public class ClassPembelianDetail
    {
        public string kodebarang { get; set; }
        public string namabarang { get; set; }
        public decimal qty { get; set; }
        public decimal harga_beli { get; set; }
        public decimal diskon { get; set; }
        public decimal total { get { return (harga_beli * qty) - diskon; } }
    }
}
