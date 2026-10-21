using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Toko_Root_ID
{
    /// <summary>Template struk thermal/standard printer tanpa Crystal Reports.</summary>
    public static class ReportStrukTemplate
    {
        public static void Preview(IWin32Window owner, string namaToko, string alamat, string telepon,
            string nomorFaktur, DateTime tanggal, string kasir, DataTable items,
            decimal subtotal, decimal diskon, decimal total, decimal dibayar, decimal kembalian,
            string footer = "Terima kasih telah berbelanja")
        {
            var doc = new PrintDocument();
            doc.DocumentName = "Struk " + nomorFaktur;
            doc.DefaultPageSettings.Margins = new Margins(12, 12, 12, 12);
            doc.DefaultPageSettings.PaperSize = new PaperSize("Struk 80mm", 315, 900);
            int row = 0;
            doc.PrintPage += delegate(object sender, PrintPageEventArgs e)
            {
                float x = e.MarginBounds.Left, y = e.MarginBounds.Top;
                float width = e.MarginBounds.Width;
                using (var title = new Font("Consolas", 10, FontStyle.Bold))
                using (var normal = new Font("Consolas", 8))
                using (var bold = new Font("Consolas", 8, FontStyle.Bold))
                {
                    DrawCenter(e.Graphics, namaToko ?? "TOKO", title, Brushes.Black, x, y, width); y += 18;
                    if (!String.IsNullOrWhiteSpace(alamat)) { DrawCenter(e.Graphics, alamat, normal, Brushes.Black, x, y, width); y += 14; }
                    if (!String.IsNullOrWhiteSpace(telepon)) { DrawCenter(e.Graphics, telepon, normal, Brushes.Black, x, y, width); y += 14; }
                    e.Graphics.DrawString(new string('-', 32), normal, Brushes.Black, x, y); y += 14;
                    e.Graphics.DrawString("No: " + nomorFaktur, normal, Brushes.Black, x, y); y += 14;
                    e.Graphics.DrawString(tanggal.ToString("dd-MM-yyyy HH:mm"), normal, Brushes.Black, x, y); y += 14;
                    e.Graphics.DrawString("Kasir: " + (kasir ?? "-"), normal, Brushes.Black, x, y); y += 14;
                    e.Graphics.DrawString(new string('-', 32), normal, Brushes.Black, x, y); y += 14;
                    if (items != null)
                    {
                        while (row < items.Rows.Count && y < e.MarginBounds.Bottom - 120)
                        {
                            DataRow r = items.Rows[row++];
                            string name = Convert.ToString(Find(r, "namabarang", "nama", "produk"));
                            decimal qty = ToDecimal(Find(r, "jumlah", "qty", "jumlahbeli"));
                            decimal price = ToDecimal(Find(r, "hargasatuan", "harga", "harga_jual"));
                            decimal lineTotal = ToDecimal(Find(r, "total", "subtotal"));
                            e.Graphics.DrawString(name, normal, Brushes.Black, x, y, new StringFormat()); y += 13;
                            e.Graphics.DrawString(qty.ToString("0.###") + " x " + price.ToString("N0"), normal, Brushes.Black, x, y);
                            e.Graphics.DrawString(lineTotal.ToString("N0"), normal, Brushes.Black, new RectangleF(x, y, width, 13), RightFormat()); y += 15;
                        }
                    }
                    e.Graphics.DrawString(new string('-', 32), normal, Brushes.Black, x, y); y += 14;
                    DrawMoney(e.Graphics, "Subtotal", subtotal, normal, x, ref y, width);
                    DrawMoney(e.Graphics, "Diskon", diskon, normal, x, ref y, width);
                    DrawMoney(e.Graphics, "TOTAL", total, bold, x, ref y, width);
                    DrawMoney(e.Graphics, "Bayar", dibayar, normal, x, ref y, width);
                    DrawMoney(e.Graphics, "Kembali", kembalian, normal, x, ref y, width);
                    y += 8; DrawCenter(e.Graphics, footer ?? "Terima kasih", normal, Brushes.Black, x, y, width);
                    e.HasMorePages = items != null && row < items.Rows.Count;
                }
            };
            using (var preview = new PrintPreviewDialog { Document = doc, Width = 460, Height = 700 }) preview.ShowDialog(owner);
        }

        private static object Find(DataRow row, params string[] names)
        {
            foreach (string name in names) if (row.Table.Columns.Contains(name)) return row[name];
            return null;
        }
        private static decimal ToDecimal(object value) { decimal result; return value != null && value != DBNull.Value && Decimal.TryParse(Convert.ToString(value), out result) ? result : 0m; }
        private static void DrawMoney(Graphics g, string label, decimal value, Font font, float x, ref float y, float width)
        { g.DrawString(label, font, Brushes.Black, x, y); g.DrawString(value.ToString("N0"), font, Brushes.Black, new RectangleF(x, y, width, 13), RightFormat()); y += 14; }
        private static void DrawCenter(Graphics g, string value, Font font, Brush brush, float x, float y, float width)
        { g.DrawString(value ?? "", font, brush, new RectangleF(x, y, width, font.GetHeight(g) + 3), CenterFormat()); }
        private static StringFormat CenterFormat() { return new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisCharacter }; }
        private static StringFormat RightFormat() { return new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near }; }
    }
}
