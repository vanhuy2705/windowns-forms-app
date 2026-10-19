using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Controls
{
    public class SFChart : Control
    {
        public string Title { get; set; } = "Doanh thu theo khung giờ";
        public Color AccentColor { get; set; } = Colors.Emerald;
        public string[] XLabels { get; set; } = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        public double[] Values { get; set; } = new double[] { 8.5, 12.2, 9.8, 14.5, 13.8, 19.4, 22.1 };
        public double MaxValue { get; set; } = 25.0; // in millions

        private int hoveredIndex = -1;
        private Point mousePos = Point.Empty;

        public SFChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Colors.Surface;
            Font = Typography.Secondary;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mousePos = e.Location;

            int paddingLeft = 52;
            int paddingRight = 24;
            int graphW = Width - paddingLeft - paddingRight;

            if (graphW > 0 && Values != null && Values.Length > 1)
            {
                float step = (float)graphW / (Values.Length - 1);
                int idx = (int)Math.Round((e.X - paddingLeft) / step);
                if (idx >= 0 && idx < Values.Length)
                {
                    if (hoveredIndex != idx)
                    {
                        hoveredIndex = idx;
                        Invalidate();
                    }
                    return;
                }
            }

            if (hoveredIndex != -1)
            {
                hoveredIndex = -1;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveredIndex = -1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int paddingLeft = 52;
            int paddingRight = 24;
            int paddingTop = 28;
            int paddingBottom = 38;

            int graphW = Math.Max(10, Width - paddingLeft - paddingRight);
            int graphH = Math.Max(10, Height - paddingTop - paddingBottom);

            // Draw Y Grid lines and Y labels
            int gridLines = 4;
            for (int i = 0; i <= gridLines; i++)
            {
                int y = paddingTop + (int)((float)i / gridLines * graphH);
                using (Pen gridPen = new Pen(ColorTranslator.FromHtml("#F1F5F9"), 1f))
                {
                    g.DrawLine(gridPen, paddingLeft, y, Width - paddingRight, y);
                }

                double val = MaxValue * (1 - (float)i / gridLines);
                string yLabel;
                if (val <= 0.001)
                {
                    yLabel = "0";
                }
                else if (MaxValue <= 5.0 || Math.Abs(val - Math.Round(val)) > 0.05)
                {
                    yLabel = $"{val:0.0}tr";
                }
                else
                {
                    yLabel = $"{val:0}tr";
                }
                Rectangle labelRect = new Rectangle(0, y - 8, paddingLeft - 8, 16);
                TextRenderer.DrawText(g, yLabel, Typography.Micro, labelRect, Colors.TextMuted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }

            if (Values == null || Values.Length < 2) return;

            PointF[] pts = new PointF[Values.Length];
            float step = (float)graphW / (Values.Length - 1);

            for (int i = 0; i < Values.Length; i++)
            {
                float ratio = (float)(Values[i] / MaxValue);
                float x = paddingLeft + i * step;
                float y = paddingTop + graphH - (ratio * graphH);
                pts[i] = new PointF(x, y);

                // Draw X Label
                if (XLabels != null && i < XLabels.Length)
                {
                    Rectangle xRect = new Rectangle((int)x - 20, Height - paddingBottom + 8, 40, 20);
                    Color xColor = (i == hoveredIndex) ? Colors.TextPrimary : Colors.TextMuted;
                    Font xFont = (i == hoveredIndex) ? Typography.SecondaryBold : Typography.Secondary;
                    TextRenderer.DrawText(g, XLabels[i], xFont, xRect, xColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                }
            }

            // Draw Area Gradient
            using (GraphicsPath areaPath = new GraphicsPath())
            {
                areaPath.AddLines(pts);
                areaPath.AddLine(pts[pts.Length - 1].X, paddingTop + graphH, pts[0].X, paddingTop + graphH);
                areaPath.CloseFigure();

                using (LinearGradientBrush fill = new LinearGradientBrush(
                    new Rectangle(paddingLeft, paddingTop, graphW, graphH),
                    Color.FromArgb(40, AccentColor),
                    Color.FromArgb(2, AccentColor),
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(fill, areaPath);
                }
            }

            // Draw Line
            using (Pen linePen = new Pen(AccentColor, 2.5f))
            {
                linePen.StartCap = LineCap.Round;
                linePen.EndCap = LineCap.Round;
                linePen.LineJoin = LineJoin.Round;
                g.DrawLines(linePen, pts);
            }

            // Draw Data Points
            for (int i = 0; i < pts.Length; i++)
            {
                PointF pt = pts[i];
                using (SolidBrush dotFill = new SolidBrush(Colors.Surface))
                using (Pen dotStroke = new Pen(AccentColor, 2f))
                {
                    g.FillEllipse(dotFill, pt.X - 4, pt.Y - 4, 8, 8);
                    g.DrawEllipse(dotStroke, pt.X - 4, pt.Y - 4, 8, 8);
                }
            }

            // Draw Hover Guide & Tooltip
            if (hoveredIndex >= 0 && hoveredIndex < pts.Length)
            {
                PointF hit = pts[hoveredIndex];

                // Vertical guideline
                using (Pen guide = new Pen(ColorTranslator.FromHtml("#CBD5E1"), 1.2f))
                {
                    guide.DashStyle = DashStyle.Dash;
                    g.DrawLine(guide, hit.X, paddingTop, hit.X, paddingTop + graphH);
                }

                // Highlighted point
                using (SolidBrush hiFill = new SolidBrush(AccentColor))
                using (Pen hiRing = new Pen(Color.White, 3f))
                {
                    g.FillEllipse(hiFill, hit.X - 6, hit.Y - 6, 12, 12);
                    g.DrawEllipse(hiRing, hit.X - 6, hit.Y - 6, 12, 12);
                }

                // Tooltip Box
                string dayName = XLabels != null && hoveredIndex < XLabels.Length ? XLabels[hoveredIndex] : "";
                string valStr = $"{Values[hoveredIndex]:0.0} triệu đ";
                string ordersStr = $"18 lượt đặt";

                int tipW = 125;
                int tipH = 58;
                int tipX = (int)hit.X - tipW / 2;
                int tipY = (int)hit.Y - tipH - 12;

                if (tipX < paddingLeft) tipX = paddingLeft;
                if (tipX + tipW > Width - paddingRight) tipX = Width - paddingRight - tipW;
                if (tipY < 4) tipY = (int)hit.Y + 12;

                Rectangle tipRect = new Rectangle(tipX, tipY, tipW, tipH);
                GraphicsUtility.DrawShadow(g, tipRect, 8, 1);

                using (GraphicsPath tipPath = GraphicsUtility.CreateRoundedRectangle(tipRect, 8))
                using (SolidBrush tipBg = new SolidBrush(ColorTranslator.FromHtml("#07111F")))
                {
                    g.FillPath(tipBg, tipPath);
                }

                Rectangle textR1 = new Rectangle(tipX + 10, tipY + 6, tipW - 20, 15);
                Rectangle textR2 = new Rectangle(tipX + 10, tipY + 22, tipW - 20, 18);
                Rectangle textR3 = new Rectangle(tipX + 10, tipY + 40, tipW - 20, 14);

                TextRenderer.DrawText(g, "Thứ " + dayName, Typography.Micro, textR1, Colors.SidebarText, TextFormatFlags.Left);
                TextRenderer.DrawText(g, valStr, Typography.SecondaryBold, textR2, Color.White, TextFormatFlags.Left);
                TextRenderer.DrawText(g, ordersStr, Typography.Micro, textR3, Colors.Mint, TextFormatFlags.Left);
            }
        }
    }

    public class SFDonut : Control
    {
        public string CenterMetric { get; set; } = "72%";
        public string CenterLabel { get; set; } = "Công suất";

        public string[] Categories { get; set; } = new[] { "Bóng đá", "Cầu lông", "Bóng chuyền", "Tennis" };
        public float[] Percentages { get; set; } = new[] { 45f, 25f, 18f, 12f };
        public Color[] SliceColors { get; set; } = new[] { Colors.Emerald, Colors.Cyan, Colors.Orange, Colors.Purple };

        public SFDonut()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Colors.Surface;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int pad = 16;
            int donutSize = Math.Min(Width * 42 / 100, Height - pad * 2);
            if (donutSize < 40) return;

            int donutX = pad + 4;
            int donutY = (Height - donutSize) / 2;
            Rectangle donutRect = new Rectangle(donutX, donutY, donutSize, donutSize);

            // Draw donut slices
            float currentAngle = -90f;
            float strokeW = Math.Max(12f, donutSize * 0.22f);
            donutRect.Inflate((int)-strokeW / 2, (int)-strokeW / 2);

            for (int i = 0; i < Percentages.Length; i++)
            {
                float sweep = (Percentages[i] / 100f) * 360f;
                Color col = i < SliceColors.Length ? SliceColors[i] : Colors.Emerald;

                using (Pen p = new Pen(col, strokeW))
                {
                    p.StartCap = LineCap.Flat;
                    p.EndCap = LineCap.Flat;
                    g.DrawArc(p, donutRect, currentAngle, sweep - 2f);
                }
                currentAngle += sweep;
            }

            // Draw center text
            Rectangle centerRect = new Rectangle(donutRect.X + 4, donutRect.Y + (donutRect.Height / 2) - 22, donutRect.Width - 8, 44);
            TextRenderer.DrawText(g, CenterMetric, Typography.SubTitle, new Rectangle(centerRect.X, centerRect.Y, centerRect.Width, 24),
                Colors.TextPrimary, TextFormatFlags.HorizontalCenter);
            TextRenderer.DrawText(g, CenterLabel, Typography.Micro, new Rectangle(centerRect.X, centerRect.Y + 22, centerRect.Width, 16),
                Colors.TextSecondary, TextFormatFlags.HorizontalCenter);

            // Draw vertical legend on right side
            int legendX = donutX + donutSize + 20;
            int legendY = Math.Max(pad, (Height - Categories.Length * 30) / 2);
            int legendW = Math.Max(110, Width - legendX - pad);

            for (int i = 0; i < Categories.Length; i++)
            {
                Color col = i < SliceColors.Length ? SliceColors[i] : Colors.Emerald;
                int itemY = legendY + i * 30;

                // Color dot
                using (SolidBrush b = new SolidBrush(col))
                {
                    g.FillEllipse(b, legendX, itemY + 4, 10, 10);
                }

                // Category name
                Rectangle catRect = new Rectangle(legendX + 16, itemY, legendW - 50, 20);
                TextRenderer.DrawText(g, Categories[i], Typography.SecondaryBold, catRect, Colors.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                // Percentage
                Rectangle pctRect = new Rectangle(legendX + legendW - 40, itemY, 38, 20);
                TextRenderer.DrawText(g, $"{Percentages[i]:0}%", Typography.Secondary, pctRect, Colors.TextSecondary,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
