using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SportZone.WinForms.DesignTokens
{
    public static class GraphicsUtility
    {
        public static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            int d = Math.Max(2, radius * 2);
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int x = bounds.X;
            int y = bounds.Y;
            int w = Math.Max(1, bounds.Width);
            int h = Math.Max(1, bounds.Height);
            int r = Math.Min(radius, Math.Min(w / 2, h / 2));
            d = r * 2;

            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void DrawShadow(Graphics g, Rectangle bounds, int radius, int depth = 1)
        {
            if (depth <= 0) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Multi-pass subtle ambient shadow
            int passes = depth == 1 ? 3 : 5;
            for (int i = passes; i >= 1; i--)
            {
                int alpha = (int)(depth == 1 ? (14 - i * 3) : (22 - i * 3));
                if (alpha <= 0) continue;
                Rectangle shadowRect = new Rectangle(
                    bounds.X - i,
                    bounds.Y + (i / 2),
                    bounds.Width + (i * 2),
                    bounds.Height + i + 1);

                using (GraphicsPath path = CreateRoundedRectangle(shadowRect, radius + i))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 11, 18, 32)))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        public static void DrawCenterCrop(Graphics g, Image img, Rectangle destRect) => DrawImageCenterCrop(g, img, destRect);

        public static void DrawImageCenterCrop(Graphics g, Image img, Rectangle destRect)
        {
            if (img == null || destRect.Width <= 0 || destRect.Height <= 0) return;

            float imgAspect = (float)img.Width / img.Height;
            float destAspect = (float)destRect.Width / destRect.Height;

            Rectangle srcRect;
            if (destAspect > imgAspect)
            {
                // Destination is wider than image: crop top and bottom
                int srcHeight = (int)(img.Width / destAspect);
                int srcY = (img.Height - srcHeight) / 2;
                srcRect = new Rectangle(0, srcY, img.Width, srcHeight);
            }
            else
            {
                // Destination is taller than image: crop left and right
                int srcWidth = (int)(img.Height * destAspect);
                int srcX = (img.Width - srcWidth) / 2;
                srcRect = new Rectangle(srcX, 0, srcWidth, img.Height);
            }

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);
        }

        public static Color GetEffectiveBackColor(Control control, Color fallback)
        {
            if (control == null) return fallback;
            Control cur = control.Parent;
            while (cur != null)
            {
                if (cur.BackColor != Color.Transparent && cur.BackColor.A > 0)
                {
                    return cur.BackColor;
                }
                cur = cur.Parent;
            }
            return fallback;
        }
    }
}
