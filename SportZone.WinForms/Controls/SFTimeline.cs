using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Controls
{
    public class TimelineBookingItem
    {
        public string BookingId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string FieldName { get; set; }
        public int FieldIndex { get; set; }
        public float StartHour { get; set; } // e.g. 7.5 = 07:30
        public float DurationHours { get; set; } // e.g. 1.5 = 1h30
        public string Status { get; set; } // Đã cọc, Đang thi đấu, Hoàn tất, Chờ duyệt
        public string TotalAmount { get; set; }
        public string PaidAmount { get; set; }
    }

    public class SFTimeline : Control
    {
        public string[] Fields { get; set; } = new[] { "Sân bóng 5A", "Sân bóng 5B", "Cầu lông C1", "Cầu lông C2", "Bóng chuyền B1", "Sân bóng 7A" };
        public List<TimelineBookingItem> Items { get; set; } = new List<TimelineBookingItem>();

        public event Action<TimelineBookingItem> OnBookingClicked;

        private const int TimeRulerWidth = 75;
        private const int HeaderHeight = 46;
        private const int HourHeight = 64;
        private const int StartHour = 6;
        private const int EndHour = 23;

        private TimelineBookingItem hoveredItem = null;
        private Point mousePos = Point.Empty;

        public SFTimeline()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Colors.Surface;
            Font = Typography.Body;
            Cursor = Cursors.Default;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mousePos = e.Location;

            TimelineBookingItem hit = HitTest(e.Location);
            if (hit != hoveredItem)
            {
                hoveredItem = hit;
                Cursor = hit != null ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveredItem = null;
            Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            TimelineBookingItem hit = HitTest(e.Location);
            if (hit != null)
            {
                OnBookingClicked?.Invoke(hit);
            }
        }

        private Rectangle GetItemRect(TimelineBookingItem item)
        {
            int colWidth = Math.Max(100, (Width - TimeRulerWidth) / Math.Max(1, Fields.Length));
            int x = TimeRulerWidth + item.FieldIndex * colWidth + 4;
            int w = colWidth - 8;

            int y = HeaderHeight + (int)((item.StartHour - StartHour) * HourHeight) + 2;
            int h = (int)(item.DurationHours * HourHeight) - 4;

            return new Rectangle(x, y, w, h);
        }

        private TimelineBookingItem HitTest(Point pt)
        {
            foreach (var item in Items)
            {
                Rectangle r = GetItemRect(item);
                if (r.Contains(pt)) return item;
            }
            return null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int numHours = EndHour - StartHour;
            int totalHeight = HeaderHeight + numHours * HourHeight;
            int colWidth = Math.Max(100, (Width - TimeRulerWidth) / Math.Max(1, Fields.Length));

            // Background
            using (SolidBrush b = new SolidBrush(Colors.Surface)) g.FillRectangle(b, ClientRectangle);

            // Time ruler background
            Rectangle rulerRect = new Rectangle(0, 0, TimeRulerWidth, totalHeight);
            using (SolidBrush rb = new SolidBrush(ColorTranslator.FromHtml("#F8FAFC"))) g.FillRectangle(rb, rulerRect);

            // Header background
            Rectangle headerRect = new Rectangle(0, 0, Width, HeaderHeight);
            using (SolidBrush hb = new SolidBrush(ColorTranslator.FromHtml("#F8FAFC"))) g.FillRectangle(hb, headerRect);
            using (Pen sep = new Pen(Colors.Border, 1.2f)) g.DrawLine(sep, 0, HeaderHeight, Width, HeaderHeight);

            // Top-left label "GIỜ / SÂN"
            Rectangle corner = new Rectangle(0, 0, TimeRulerWidth, HeaderHeight);
            TextRenderer.DrawText(g, "GIỜ", Typography.SectionHeader, corner, Colors.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // Draw Field Columns Header
            for (int i = 0; i < Fields.Length; i++)
            {
                int colX = TimeRulerWidth + i * colWidth;
                Rectangle colH = new Rectangle(colX, 0, colWidth, HeaderHeight);
                TextRenderer.DrawText(g, Fields[i], Typography.SecondaryBold, colH, Colors.TextPrimary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                using (Pen vp = new Pen(ColorTranslator.FromHtml("#EEF2F6"), 1f))
                {
                    g.DrawLine(vp, colX, 0, colX, totalHeight);
                }
            }

            // Draw Hour Rows
            for (int h = 0; h < numHours; h++)
            {
                int y = HeaderHeight + h * HourHeight;
                string timeStr = $"{(StartHour + h):00}:00";

                // Time label in ruler
                Rectangle tRect = new Rectangle(0, y - 10, TimeRulerWidth - 10, 20);
                TextRenderer.DrawText(g, timeStr, Typography.MicroBold, tRect, Colors.TextMuted, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

                // Horizontal grid line
                using (Pen hp = new Pen(ColorTranslator.FromHtml("#EEF2F6"), 1f))
                {
                    g.DrawLine(hp, TimeRulerWidth, y, Width, y);
                }
            }

            // Draw Booking Blocks
            foreach (var item in Items)
            {
                Rectangle ir = GetItemRect(item);
                if (ir.Width <= 0 || ir.Height <= 0) continue;

                Color bg, fg, border;
                GetStatusColor(item.Status, out bg, out fg, out border);

                bool isHover = (hoveredItem == item);
                if (isHover)
                {
                    GraphicsUtility.DrawShadow(g, ir, 8, 2);
                }

                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(ir, 8))
                {
                    using (SolidBrush bb = new SolidBrush(bg)) g.FillPath(bb, path);
                    using (Pen bp = new Pen(isHover ? fg : border, 1.2f)) g.DrawPath(bp, path);
                }

                // Left color stripe
                Rectangle stripe = new Rectangle(ir.X + 2, ir.Y + 4, 3, ir.Height - 8);
                using (SolidBrush sb = new SolidBrush(fg)) g.FillRectangle(sb, stripe);

                // Text info
                Rectangle nameR = new Rectangle(ir.X + 10, ir.Y + 6, ir.Width - 14, 18);
                TextRenderer.DrawText(g, item.CustomerName, Typography.SecondaryBold, nameR, Colors.TextPrimary, TextFormatFlags.Left);

                Rectangle timeR = new Rectangle(ir.X + 10, ir.Y + 24, ir.Width - 14, 16);
                string timeLabel = $"{item.StartHour:00}:00 - {(item.StartHour + item.DurationHours):00}:00";
                TextRenderer.DrawText(g, timeLabel, Typography.Micro, timeR, Colors.TextSecondary, TextFormatFlags.Left);

                if (ir.Height > 54)
                {
                    Rectangle statusR = new Rectangle(ir.X + 10, ir.Y + 42, ir.Width - 14, 16);
                    TextRenderer.DrawText(g, $"{item.Status} • {item.TotalAmount}", Typography.MicroBold, statusR, fg, TextFormatFlags.Left);
                }
            }

            // Draw Hover Tooltip if hovering a booking item
            if (hoveredItem != null)
            {
                DrawTooltip(g, hoveredItem, mousePos);
            }
        }

        private void DrawTooltip(Graphics g, TimelineBookingItem item, Point pt)
        {
            int tipW = 200;
            int tipH = 92;
            int tipX = pt.X + 14;
            int tipY = pt.Y + 14;

            if (tipX + tipW > Width - 10) tipX = pt.X - tipW - 14;
            if (tipY + tipH > Height - 10) tipY = pt.Y - tipH - 14;

            Rectangle tipRect = new Rectangle(tipX, tipY, tipW, tipH);
            GraphicsUtility.DrawShadow(g, tipRect, 8, 2);

            using (GraphicsPath tipPath = GraphicsUtility.CreateRoundedRectangle(tipRect, 8))
            using (SolidBrush tipBg = new SolidBrush(ColorTranslator.FromHtml("#07111F")))
            {
                g.FillPath(tipBg, tipPath);
            }

            Rectangle r1 = new Rectangle(tipX + 12, tipY + 8, tipW - 24, 18);
            Rectangle r2 = new Rectangle(tipX + 12, tipY + 28, tipW - 24, 16);
            Rectangle r3 = new Rectangle(tipX + 12, tipY + 46, tipW - 24, 16);
            Rectangle r4 = new Rectangle(tipX + 12, tipY + 66, tipW - 24, 18);

            TextRenderer.DrawText(g, $"{item.BookingId} • {item.CustomerName}", Typography.SecondaryBold, r1, Color.White, TextFormatFlags.Left);
            TextRenderer.DrawText(g, $"SĐT: {item.Phone}", Typography.Micro, r2, Colors.SidebarText, TextFormatFlags.Left);
            TextRenderer.DrawText(g, $"Sân: {item.FieldName}", Typography.Micro, r3, Colors.SidebarText, TextFormatFlags.Left);
            TextRenderer.DrawText(g, $"Tổng: {item.TotalAmount} (Đã trả {item.PaidAmount})", Typography.MicroBold, r4, Colors.Mint, TextFormatFlags.Left);
        }

        private void GetStatusColor(string status, out Color bg, out Color fg, out Color border)
        {
            string s = status.ToLower();
            if (s.Contains("hoàn tất") || s.Contains("sẵn sàng"))
            {
                bg = Colors.SuccessBg;
                fg = Colors.DeepEmerald;
                border = ColorTranslator.FromHtml("#86EFAC");
            }
            else if (s.Contains("đang"))
            {
                bg = Colors.InfoBg;
                fg = Colors.ElectricBlue;
                border = ColorTranslator.FromHtml("#93C5FD");
            }
            else if (s.Contains("cọc"))
            {
                bg = Colors.WarningBg;
                fg = ColorTranslator.FromHtml("#B45309");
                border = ColorTranslator.FromHtml("#FDE68A");
            }
            else
            {
                bg = Colors.OrangeBg;
                fg = Colors.Orange;
                border = ColorTranslator.FromHtml("#FED7AA");
            }
        }
    }
}
