using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CommonClass;

namespace Toko_Root_ID.UI
{
    /// <summary>Kartu angka dashboard yang digambar sendiri agar tampil modern.</summary>
    internal sealed class StatCard : Control
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Subtitle { get; set; }
        public string Glyph { get; set; }
        public Color Accent { get; set; }

        public StatCard()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Accent = AppTheme.Accent;
            Title = Value = Subtitle = Glyph = string.Empty;
            Height = 106;
            Margin = new Padding(6);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle body = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Round(body, 12))
            using (var fill = new SolidBrush(AppTheme.Surface))
            using (var pen = new Pen(AppTheme.Border))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
                using (GraphicsPath top = TopRound(body, 12))
                using (var accent = new SolidBrush(Accent))
                    g.FillPath(accent, top);
            }

            Rectangle icon = new Rectangle(16, 22, 42, 42);
            using (GraphicsPath path = Round(icon, 10))
            using (var fill = new SolidBrush(AppTheme.Tint(Accent, 0.84)))
                g.FillPath(fill, path);
            using (var glyphBrush = new SolidBrush(Accent))
            using (var glyphFont = new Font("Segoe UI", 15F, FontStyle.Bold))
            using (var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(Glyph ?? "", glyphFont, glyphBrush, icon, center);

            using (var titleFont = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var valueFont = new Font("Segoe UI", 16F, FontStyle.Bold))
            using (var subFont = new Font("Segoe UI", 8F))
            using (var titleBrush = new SolidBrush(AppTheme.Muted))
            using (var valueBrush = new SolidBrush(AppTheme.Text))
            using (var subBrush = new SolidBrush(AppTheme.Muted))
            {
                g.DrawString((Title ?? "").ToUpperInvariant(), titleFont, titleBrush, 70, 22);
                g.DrawString(Value ?? "-", valueFont, valueBrush, 68, 40);
                g.DrawString(Subtitle ?? "", subFont, subBrush, 16, 78);
            }
        }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            var p = new GraphicsPath();
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private static GraphicsPath TopRound(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddLine(r.Right, r.Y + 5, r.X, r.Y + 5);
            p.CloseFigure();
            return p;
        }
    }

    /// <summary>Grafik batang vertikal untuk omzet harian/bulanan.</summary>
    internal sealed class BarChartControl : Control
    {
        private List<ClassChartPoint> _points = new List<ClassChartPoint>();
        public string ValuePrefix { get; set; }
        public bool Currency { get; set; }
        public Color Accent { get; set; }

        public BarChartControl()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Accent = AppTheme.Accent;
            ValuePrefix = "Rp";
            Currency = true;
            MinimumSize = new Size(200, 120);
        }

        public void SetData(List<ClassChartPoint> points)
        {
            _points = points ?? new List<ClassChartPoint>();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (_points.Count == 0)
            {
                TextRenderer.DrawText(g, "Belum ada data penjualan", Font, ClientRectangle,
                    AppTheme.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            int left = 10, right = 10, top = 14, bottom = 30;
            int width = Math.Max(10, Width - left - right);
            int height = Math.Max(10, Height - top - bottom);
            decimal max = 0m;
            foreach (ClassChartPoint p in _points) if (p.value > max) max = p.value;
            if (max <= 0) max = 1m;

            using (var gridPen = new Pen(AppTheme.Border) { DashStyle = DashStyle.Dot })
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = top + (height * i / 4);
                    g.DrawLine(gridPen, left, y, left + width, y);
                }
            }

            int count = _points.Count;
            int slot = width / Math.Max(1, count);
            int barWidth = Math.Max(10, Math.Min(46, (int)(slot * 0.55)));
            using (var barBrush = new SolidBrush(AppTheme.Tint(Accent, 0.35)))
            using (var lastBrush = new SolidBrush(Accent))
            using (var labelBrush = new SolidBrush(AppTheme.Muted))
            using (var valueBrush = new SolidBrush(AppTheme.Text))
            using (var labelFont = new Font("Segoe UI", 8F))
            using (var valueFont = new Font("Segoe UI", 7F, FontStyle.Bold))
            {
                for (int i = 0; i < count; i++)
                {
                    ClassChartPoint p = _points[i];
                    int barHeight = (int)(height * (double)(p.value / max));
                    if (barHeight < 2 && p.value > 0) barHeight = 2;
                    int x = left + (i * slot) + ((slot - barWidth) / 2);
                    int y = top + height - barHeight;
                    g.FillRectangle(i == count - 1 ? lastBrush : barBrush, x, y, barWidth, barHeight);
                    string label = p.label ?? "";
                    SizeF ls = g.MeasureString(label, labelFont);
                    g.DrawString(label, labelFont, labelBrush, x + (barWidth - ls.Width) / 2, top + height + 7);
                    if (p.value > 0)
                    {
                        string val = Compact(p.value);
                        SizeF vs = g.MeasureString(val, valueFont);
                        g.DrawString(val, valueFont, valueBrush, x + (barWidth - vs.Width) / 2, Math.Max(0, y - 13));
                    }
                }
            }
        }

        private string Compact(decimal value)
        {
            decimal abs = Math.Abs(value);
            if (abs >= 1000000000m) return (value / 1000000000m).ToString("0.#") + " M";
            if (abs >= 1000000m) return (value / 1000000m).ToString("0.#") + " jt";
            if (abs >= 1000m) return (value / 1000m).ToString("0.#") + " rb";
            return value.ToString("0");
        }
    }

    /// <summary>Grafik batang horizontal untuk produk terlaris / stok kritis.</summary>
    internal sealed class HBarChartControl : Control
    {
        private List<ClassChartPoint> _points = new List<ClassChartPoint>();
        public Color Accent { get; set; }
        public string ValueSuffix { get; set; }

        public HBarChartControl()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Accent = AppTheme.Accent;
            ValueSuffix = "";
        }

        public void SetData(List<ClassChartPoint> points)
        {
            _points = points ?? new List<ClassChartPoint>();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (_points.Count == 0)
            {
                TextRenderer.DrawText(g, "Belum ada data", Font, ClientRectangle,
                    AppTheme.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            decimal max = 1m;
            foreach (ClassChartPoint p in _points) if (p.value > max) max = p.value;

            int rowHeight = Math.Max(28, Height / _points.Count);
            int labelWidth = Math.Min(150, Width / 3);
            int barLeft = labelWidth + 10;
            int barMax = Math.Max(30, Width - barLeft - 70);
            using (var labelFont = new Font("Segoe UI", 8.5F))
            using (var valueFont = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var labelBrush = new SolidBrush(AppTheme.Text))
            using (var valueBrush = new SolidBrush(AppTheme.Muted))
            {
                for (int i = 0; i < _points.Count; i++)
                {
                    ClassChartPoint p = _points[i];
                    int y = i * rowHeight;
                    string label = p.label ?? "";
                    if (label.Length > 20) label = label.Substring(0, 19) + "…";
                    g.DrawString(label, labelFont, labelBrush,
                        new RectangleF(0, y + 6, labelWidth, 20));
                    int barWidth = (int)(barMax * (double)(p.value / max));
                    if (barWidth < 3 && p.value > 0) barWidth = 3;
                    var barRect = new Rectangle(barLeft, y + 7, barWidth, Math.Max(10, rowHeight - 18));
                    using (var fill = new SolidBrush(AppTheme.Tint(Accent, i == 0 ? 0.0 : 0.35)))
                        g.FillRectangle(fill, barRect);
                    g.DrawString(p.value.ToString("N0") + ValueSuffix, valueFont, valueBrush,
                        barLeft + barWidth + 8, y + 6);
                }
            }
        }
    }

    /// <summary>Mini donut untuk komposisi stok aman/menipis/habis.</summary>
    internal sealed class DonutChartControl : Control
    {
        public int Aman { get; set; }
        public int Menipis { get; set; }
        public int Habis { get; set; }
        public string Caption { get; set; }

        public DonutChartControl()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Caption = "Status stok";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int total = Aman + Menipis + Habis;
            int d = Math.Min(Width, Height) - 30;
            Rectangle rect = new Rectangle((Width - d) / 2, 15, d, d);
            using (var baseBrush = new SolidBrush(AppTheme.Border))
                g.FillEllipse(baseBrush, rect);
            if (total > 0)
            {
                float start = -90;
                DrawSlice(g, rect, start, Aman, total, AppTheme.Success); start += Aman * 360f / total;
                DrawSlice(g, rect, start, Menipis, total, AppTheme.Warning); start += Menipis * 360f / total;
                DrawSlice(g, rect, start, Habis, total, AppTheme.Danger);
            }
            int inner = (int)(d * 0.62);
            var hole = new Rectangle(rect.X + (d - inner) / 2, rect.Y + (d - inner) / 2, inner, inner);
            using (var holeBrush = new SolidBrush(AppTheme.Surface))
                g.FillEllipse(holeBrush, hole);
            using (var valueFont = new Font("Segoe UI", 14F, FontStyle.Bold))
            using (var captionFont = new Font("Segoe UI", 7.5F))
            using (var valueBrush = new SolidBrush(AppTheme.Text))
            using (var captionBrush = new SolidBrush(AppTheme.Muted))
            using (var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(total.ToString("N0"), valueFont, valueBrush, hole, center);
                g.DrawString(Caption, captionFont, captionBrush,
                    new RectangleF(hole.X, hole.Bottom - 8, hole.Width, 18), center);
            }
        }

        private static void DrawSlice(Graphics g, Rectangle rect, float start, int value, int total, Color color)
        {
            if (value <= 0) return;
            float sweep = value * 360f / total;
            using (var brush = new SolidBrush(color))
                g.FillPie(brush, rect, start, sweep);
        }
    }
}
