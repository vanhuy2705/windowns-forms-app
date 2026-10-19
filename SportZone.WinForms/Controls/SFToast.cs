using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Controls
{
    public class SFToast : Form
    {
        private readonly Timer dismissTimer;
        private readonly Timer animTimer;
        private int lifeMs = 3500;
        private int elapsedMs = 0;
        private readonly string titleText;
        private readonly string messageText;
        private readonly SFIcon iconType;
        private readonly Color accentColor;
        private int targetY;
        private int currentY;

        public SFToast(string title, string message, SFIcon icon = SFIcon.Check, Color? accent = null)
        {
            titleText = title;
            messageText = message;
            iconType = icon;
            accentColor = accent ?? Colors.Emerald;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Width = 360;
            Height = 78;
            BackColor = Colors.Surface;
            DoubleBuffered = true;

            dismissTimer = new Timer { Interval = 50 };
            dismissTimer.Tick += (s, e) =>
            {
                elapsedMs += 50;
                Invalidate();
                if (elapsedMs >= lifeMs)
                {
                    dismissTimer.Stop();
                    Close();
                }
            };

            animTimer = new Timer { Interval = 15 };
            animTimer.Tick += (s, e) =>
            {
                if (currentY > targetY)
                {
                    currentY -= 6;
                    if (currentY <= targetY)
                    {
                        currentY = targetY;
                        animTimer.Stop();
                        dismissTimer.Start();
                    }
                    Location = new Point(Location.X, currentY);
                }
            };

            Click += delegate { Close(); };
        }

        public static void Show(Control owner, string message, string title = "SPORTFIELD", SFIcon icon = SFIcon.Check, Color? accent = null)
        {
            Show(owner as Form ?? owner?.FindForm(), message, title, icon, accent);
        }

        public static void Show(Form owner, string message, string title = "SPORTFIELD", SFIcon icon = SFIcon.Check, Color? accent = null)
        {
            try
            {
                SFToast toast = new SFToast(title, message, icon, accent);
                DpiHelper.ApplyDpiMode(toast);
                Screen screen = (owner != null && owner.Visible) ? Screen.FromControl(owner) : Screen.PrimaryScreen;
                Rectangle workArea = screen.WorkingArea;

                int x = (owner != null && owner.Visible) 
                    ? Math.Min(owner.Right - toast.Width - 24, workArea.Right - toast.Width - 24) 
                    : workArea.Right - toast.Width - 24;
                int y = (owner != null && owner.Visible) 
                    ? Math.Min(owner.Bottom - toast.Height - 24, workArea.Bottom - toast.Height - 24) 
                    : workArea.Bottom - toast.Height - 24;

                x = Math.Max(workArea.Left + 24, x);
                y = Math.Max(workArea.Top + 24, y);

                Point basePoint = new Point(x, y);
                toast.targetY = basePoint.Y;
                toast.currentY = Math.Min(basePoint.Y + 24, workArea.Bottom - toast.Height);
                toast.Location = new Point(basePoint.X, toast.currentY);
                toast.Show();
                toast.animTimer.Start();
            }
            catch
            {
                // Fallback
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            GraphicsUtility.DrawShadow(g, rect, 10, 2);

            using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, 10))
            {
                using (SolidBrush bg = new SolidBrush(Colors.Surface)) g.FillPath(bg, path);
                using (Pen p = new Pen(Colors.Border, 1.2f)) g.DrawPath(p, path);
            }

            // Left icon pill
            int iconBoxSize = 36;
            Rectangle iconBox = new Rectangle(14, (Height - iconBoxSize) / 2, iconBoxSize, iconBoxSize);
            using (GraphicsPath ip = GraphicsUtility.CreateRoundedRectangle(iconBox, 8))
            using (SolidBrush ib = new SolidBrush(Color.FromArgb(28, accentColor)))
            {
                g.FillPath(ib, ip);
            }
            VectorIcons.Draw(g, iconType, iconBox, accentColor, 2f);

            // Title & message
            Rectangle tr = new Rectangle(58, 14, Width - 90, 20);
            TextRenderer.DrawText(g, titleText, Typography.BodyBold, tr, Colors.TextPrimary, TextFormatFlags.Left);

            Rectangle mr = new Rectangle(58, 36, Width - 90, 22);
            TextRenderer.DrawText(g, messageText, Typography.Secondary, mr, Colors.TextSecondary, TextFormatFlags.Left);

            // Close button
            Rectangle cr = new Rectangle(Width - 28, 14, 16, 16);
            VectorIcons.Draw(g, SFIcon.Close, cr, Colors.TextMuted, 1.6f);

            // Progress bar at bottom
            float pct = (float)elapsedMs / lifeMs;
            int barW = (int)((Width - 20) * (1f - pct));
            if (barW > 0)
            {
                using (Pen bp = new Pen(accentColor, 2.5f))
                {
                    g.DrawLine(bp, 10, Height - 3, 10 + barW, Height - 3);
                }
            }
        }
    }
}
