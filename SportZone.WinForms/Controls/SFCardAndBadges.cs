using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Controls
{
    public class SFCard : Panel
    {
        private int cornerRadius = Radius.Card;
        private Color borderColor = Colors.Border;
        private bool hasShadow = true;

        public int CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public bool HasShadow
        {
            get => hasShadow;
            set { hasShadow = value; Invalidate(); }
        }

        public SFCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Colors.Surface;
            Padding = new Padding(Spacing.L);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 2 || rect.Height <= 2) return;

            // Pre-fill parent background
            Color effParent = GraphicsUtility.GetEffectiveBackColor(this, Colors.AppBackground);
            using (SolidBrush pb = new SolidBrush(effParent))
            {
                g.FillRectangle(pb, ClientRectangle);
            }

            if (hasShadow)
            {
                GraphicsUtility.DrawShadow(g, rect, cornerRadius, 1);
            }

            using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, cornerRadius))
            {
                using (SolidBrush bg = new SolidBrush(BackColor))
                {
                    g.FillPath(bg, path);
                }

                if (borderColor != Color.Transparent)
                {
                    using (Pen p = new Pen(borderColor, 1.1f))
                    {
                        g.DrawPath(p, path);
                    }
                }
            }
        }
    }

    public class SFMetricCard : Panel
    {
        public string Title { get; set; } = "METRIC";
        public string Value { get; set; } = "0";
        public string TrendText { get; set; } = "";
        public string SubValue
        {
            get { return TrendText; }
            set { TrendText = value; }
        }
        public string Subtitle
        {
            get { return TrendText; }
            set { TrendText = value; }
        }
        public bool IsPositiveTrend { get; set; } = true;
        public Color AccentColor { get; set; } = Colors.Primary;
        public SFIcon Icon { get; set; } = SFIcon.TrendingUp;
        public int[] SparklineData { get; set; } = null;

        public SFMetricCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Height = 118;
            BackColor = Colors.Surface;
            Padding = new Padding(16, 14, 16, 12);
        }

        public SFMetricCard(string title, string value, string trendText = "", SFIcon icon = SFIcon.TrendingUp, Color? accent = null, string extra = "")
            : this()
        {
            Title = title;
            Value = value;
            TrendText = trendText;
            Icon = icon;
            if (accent.HasValue) AccentColor = accent.Value;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 2 || rect.Height <= 2) return;

            // Pre-fill parent background
            Color effParent = GraphicsUtility.GetEffectiveBackColor(this, Colors.AppBackground);
            using (SolidBrush pb = new SolidBrush(effParent))
            {
                g.FillRectangle(pb, ClientRectangle);
            }

            // Card body
            using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, Radius.Card))
            {
                using (SolidBrush bg = new SolidBrush(Colors.Surface))
                {
                    g.FillPath(bg, path);
                }
                using (Pen borderPen = new Pen(Colors.Border, 1.1f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Top accent indicator line
            Rectangle topAccentRect = new Rectangle(14, 0, Width - 28, 3);
            using (GraphicsPath topPath = GraphicsUtility.CreateRoundedRectangle(topAccentRect, 2))
            using (SolidBrush topBrush = new SolidBrush(AccentColor))
            {
                g.FillPath(topBrush, topPath);
            }

            // Title & Icon
            int topY = 14;
            Rectangle iconRect = new Rectangle(16, topY + 1, 15, 15);
            VectorIcons.Draw(g, Icon, iconRect, AccentColor, 1.8f);

            Rectangle titleRect = new Rectangle(36, topY, Width - 50, 18);
            TextRenderer.DrawText(g, Title.ToUpper(), Typography.SectionHeader, titleRect, Colors.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            // Large Metric Value
            int valueY = topY + 22;
            Rectangle valueRect = new Rectangle(16, valueY, Width - 32, 34);
            TextRenderer.DrawText(g, Value, Typography.KpiValue, valueRect, Colors.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            // Trend badge
            int bottomY = topY + 62;
            Rectangle trendRect = Rectangle.Empty;
            if (!string.IsNullOrEmpty(TrendText))
            {
                Color trendBg = IsPositiveTrend ? Colors.SuccessBg : Colors.DangerBg;
                Color trendFg = IsPositiveTrend ? Colors.DeepEmerald : Colors.Danger;

                Size trendSize = TextRenderer.MeasureText(TrendText, Typography.MicroBold, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix);
                int trendW = Math.Min(trendSize.Width + 12, Width - 32);
                trendRect = new Rectangle(16, bottomY, trendW, 20);

                using (GraphicsPath trendPath = GraphicsUtility.CreateRoundedRectangle(trendRect, 10))
                using (SolidBrush trendBrush = new SolidBrush(trendBg))
                {
                    g.FillPath(trendBrush, trendPath);
                }
                TextRenderer.DrawText(g, TrendText, Typography.MicroBold, trendRect, trendFg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }

            // Mini sparkline graph: carefully positioned so it NEVER overlaps with trend badge or metric text
            if (SparklineData != null && SparklineData.Length > 1)
            {
                int sparkW = 0, sparkH = 22, sparkX = 0, sparkY = 0;
                int rightAvail = (Width - 16) - (trendRect.IsEmpty ? 16 : trendRect.Right + 12);

                int valueW = TextRenderer.MeasureText(Value, Typography.KpiValue).Width;
                if (rightAvail >= 55)
                {
                    // Fits cleanly on the bottom right
                    sparkW = Math.Min(80, rightAvail);
                    sparkH = 22;
                    sparkX = Width - sparkW - 16;
                    sparkY = bottomY - 1;
                }
                else if (16 + valueW + 20 < Width - 60)
                {
                    // Long text badge: relocate sparkline to the upper right beside the metric value
                    sparkW = Math.Min(70, Width - (16 + valueW + 20));
                    sparkH = 24;
                    sparkX = Width - sparkW - 16;
                    sparkY = valueY + 4;
                }
                else
                {
                    // Value is wide and badge is long: omit sparkline to keep typography 100% clean
                    sparkW = 0;
                }

                if (sparkW >= 40)
                {
                    PointF[] pts = new PointF[SparklineData.Length];
                    int maxVal = 100;
                    for (int i = 0; i < SparklineData.Length; i++)
                    {
                        if (SparklineData[i] > maxVal) maxVal = SparklineData[i];
                    }

                    float step = (float)sparkW / (SparklineData.Length - 1);
                    for (int i = 0; i < SparklineData.Length; i++)
                    {
                        float ratio = (float)SparklineData[i] / maxVal;
                        float py = sparkY + sparkH - (ratio * sparkH);
                        pts[i] = new PointF(sparkX + i * step, py);
                    }

                    // Fill area gradient
                    using (GraphicsPath sparkArea = new GraphicsPath())
                    {
                        sparkArea.AddLines(pts);
                        sparkArea.AddLine(pts[pts.Length - 1].X, sparkY + sparkH, pts[0].X, sparkY + sparkH);
                        sparkArea.CloseFigure();
                        using (LinearGradientBrush fill = new LinearGradientBrush(
                            new Rectangle(sparkX, sparkY, sparkW, sparkH),
                            Color.FromArgb(50, AccentColor),
                            Color.FromArgb(5, AccentColor),
                            LinearGradientMode.Vertical))
                        {
                            g.FillPath(fill, sparkArea);
                        }
                    }

                    // Line
                    using (Pen sparkPen = new Pen(AccentColor, 2f))
                    {
                        sparkPen.StartCap = LineCap.Round;
                        sparkPen.EndCap = LineCap.Round;
                        sparkPen.LineJoin = LineJoin.Round;
                        g.DrawLines(sparkPen, pts);
                    }

                    // End dot
                    PointF lastPt = pts[pts.Length - 1];
                    using (SolidBrush dotBrush = new SolidBrush(AccentColor))
                    {
                        g.FillEllipse(dotBrush, lastPt.X - 3, lastPt.Y - 3, 6, 6);
                    }
                }
            }
        }
    }

    public class SFBadge : Control
    {
        private Color foreColor = Colors.Success;
        private Color backColor = Colors.SuccessBg;
        private bool isPill = true;

        public Color BadgeForeColor
        {
            get => foreColor;
            set { foreColor = value; Invalidate(); }
        }

        public Color BadgeBackColor
        {
            get => backColor;
            set { backColor = value; Invalidate(); }
        }

        public bool IsPill
        {
            get => isPill;
            set { isPill = value; Invalidate(); }
        }

        public SFBadge()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            Font = Typography.MicroBold;
            Height = 22;
            Width = 90;
            BackColor = Color.Transparent;
        }

        public static SFBadge Create(string text, Color fore, Color back)
        {
            SFBadge b = new SFBadge { Text = text, BadgeForeColor = fore, BadgeBackColor = back };
            Size s = TextRenderer.MeasureText(text, Typography.MicroBold, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix);
            b.Width = s.Width + 18;
            return b;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int r = isPill ? rect.Height / 2 : 6;
            using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, r))
            using (SolidBrush b = new SolidBrush(backColor))
            {
                g.FillPath(b, path);
            }

            TextRenderer.DrawText(g, Text, Font, rect, foreColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }

    public class SFFilterChip : Button
    {
        private bool isSelected = false;
        private int count = -1;

        public bool Selected
        {
            get => isSelected;
            set { isSelected = value; Invalidate(); }
        }

        public bool IsActive
        {
            get => isSelected;
            set { isSelected = value; Invalidate(); }
        }

        public int Count
        {
            get => count;
            set { count = value; Invalidate(); }
        }

        public SFFilterChip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Height = 34;
            Font = Typography.SecondaryBold;
            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
        }

        protected override void OnClick(EventArgs e)
        {
            Selected = !Selected;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Pre-fill background with parent color to eliminate black rounded corner artifacts
            Color effParent = GraphicsUtility.GetEffectiveBackColor(this, Colors.AppBackground);
            using (SolidBrush pb = new SolidBrush(effParent))
            {
                g.FillRectangle(pb, ClientRectangle);
            }

            Color bg = isSelected ? Colors.SuccessBg : Colors.Surface;
            Color fg = isSelected ? Colors.DeepEmerald : Colors.TextSecondary;
            Color border = isSelected ? Colors.Emerald : Colors.Border;

            using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, Radius.Pill))
            {
                using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, path);
                using (Pen p = new Pen(border, 1.2f)) g.DrawPath(p, path);
            }

            string label = Text;
            if (count >= 0) label += " " + count;

            TextRenderer.DrawText(g, label, Font, rect, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }

    public class SFAvatar : Control
    {
        public Image AvatarImage { get; set; }
        public string Initials { get; set; } = "SF";
        public bool ShowOnlineDot { get; set; } = true;
        public Color RingColor { get; set; } = Colors.Emerald;

        public SFAvatar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            Size = new Size(42, 42);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color clearColor = BackColor != Color.Transparent ? BackColor : (Parent != null && Parent.BackColor != Color.Transparent ? Parent.BackColor : Color.Transparent);
            if (clearColor != Color.Transparent)
            {
                g.Clear(clearColor);
            }

            int size = Math.Min(Width, Height) - 4;
            Rectangle rect = new Rectangle(2, 2, size, size);

            using (GraphicsPath clipPath = new GraphicsPath())
            {
                clipPath.AddEllipse(rect);
                if (AvatarImage != null)
                {
                    GraphicsState state = g.Save();
                    g.SetClip(clipPath);
                    GraphicsUtility.DrawImageCenterCrop(g, AvatarImage, rect);
                    g.Restore(state);
                }
                else
                {
                    using (SolidBrush bg = new SolidBrush(ColorTranslator.FromHtml("#0C1A2B")))
                    {
                        g.FillPath(bg, clipPath);
                    }
                    using (Font f = new Font("Segoe UI", size * 0.38f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, Initials, f, rect, Color.White,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }

                // Border ring
                using (Pen ringPen = new Pen(RingColor, 1.8f))
                {
                    g.DrawEllipse(ringPen, rect);
                }
            }

            if (ShowOnlineDot)
            {
                int dotSize = 10;
                Rectangle dotRect = new Rectangle(Width - dotSize - 1, Height - dotSize - 1, dotSize, dotSize);
                using (SolidBrush dotBg = new SolidBrush(Colors.Success))
                using (Pen dotBorder = new Pen(Colors.Surface, 2f))
                {
                    g.FillEllipse(dotBg, dotRect);
                    g.DrawEllipse(dotBorder, dotRect);
                }
            }
        }
    }
}
