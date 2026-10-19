using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms
{
    public class CardPanel : SFCard
    {
        public int Radius
        {
            get => CornerRadius;
            set => CornerRadius = value;
        }

        public CardPanel()
        {
            CornerRadius = 12;
            BorderColor = Colors.Border;
            BackColor = Colors.Surface;
            Padding = new Padding(16);
        }
    }

    public class BadgeLabel : Label
    {
        public Color BadgeFore { get; set; } = Colors.DeepEmerald;
        public Color BadgeBack { get; set; } = Colors.SuccessBg;

        public BadgeLabel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);
            AutoSize = true;
            Font = Typography.MicroBold;
            Padding = new Padding(8, 4, 8, 4);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(r, Math.Max(2, r.Height / 2)))
            using (SolidBrush b = new SolidBrush(BadgeBack))
            {
                g.FillPath(b, path);
            }

            TextRenderer.DrawText(g, Text.Trim(), Font, r, BadgeFore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    public class AccentButton : SFButton
    {
        public Color NormalColor
        {
            get => BackColor;
            set => BackColor = value;
        }

        public Color HoverColor { get; set; }

        public AccentButton()
        {
            Variant = SFButtonVariant.Primary;
            Height = Metrics.ButtonHeight;
        }
    }

    public class HeroPanel : Panel
    {
        public Color Color1 { get; set; } = Colors.SidebarBackground;
        public Color Color2 { get; set; } = Colors.DeepEmerald;

        public HeroPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle r = ClientRectangle;
            if (r.Width <= 0 || r.Height <= 0) return;

            using (LinearGradientBrush b = new LinearGradientBrush(r, Color1, Color2, 22f))
            {
                e.Graphics.FillRectangle(b, r);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen p = new Pen(Color.FromArgb(20, Color.White), 1.2f))
            {
                for (int i = 0; i < 6; i++)
                {
                    e.Graphics.DrawEllipse(p, Width - 320 + i * 36, -80 + i * 20, 320, 320);
                }
            }
        }
    }

    public class FieldVisualPanel : Panel
    {
        public Color Accent { get; set; } = Colors.Emerald;

        public FieldVisualPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = ColorTranslator.FromHtml("#064E3B");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = ClientRectangle;

            using (LinearGradientBrush b = new LinearGradientBrush(r, ColorTranslator.FromHtml("#064E3B"), ColorTranslator.FromHtml("#059669"), 15f))
            {
                g.FillRectangle(b, r);
            }

            using (Pen p = new Pen(Color.FromArgb(210, Color.White), 1.8f))
            {
                int m = 14;
                int pw = Math.Max(20, Width - m * 2);
                int ph = Math.Max(20, Height - m * 2);

                g.DrawRectangle(p, m, m, pw, ph);
                g.DrawLine(p, Width / 2, m, Width / 2, Height - m);
                g.DrawEllipse(p, Width / 2 - 26, Height / 2 - 26, 52, 52);

                int goalH = Math.Min(60, ph / 2);
                g.DrawRectangle(p, m, (Height - goalH) / 2, 38, goalH);
                g.DrawRectangle(p, Width - m - 38, (Height - goalH) / 2, 38, goalH);
            }
        }
    }

    public class SimpleChart : SFChart
    {
        public string Caption
        {
            get => Title;
            set => Title = value;
        }

        public Color Accent
        {
            get => AccentColor;
            set => AccentColor = value;
        }

        public SimpleChart()
        {
            AccentColor = Colors.Emerald;
        }
    }

    public class DonutChart : SFDonut
    {
        public DonutChart()
        {
        }
    }
}
