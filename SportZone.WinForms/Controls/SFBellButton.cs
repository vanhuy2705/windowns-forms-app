using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Controls
{
    /// <summary>
    /// Nút Chuông Thông Báo với Badge số chưa đọc tự động căn chỉnh
    /// Hỗ trợ trạng thái Hover, Click, và hiển thị 99+
    /// </summary>
    public class SFBellButton : Control
    {
        private int _unreadCount = 0;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private readonly ToolTip _toolTip = new ToolTip();

        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                if (_unreadCount != value)
                {
                    _unreadCount = value;
                    Invalidate();
                }
            }
        }

        public SFBellButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            Size = new Size(38, 32);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            _toolTip.SetToolTip(this, "Thông báo");
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 1. Nền Hover
            if (_isHovered || _isPressed)
            {
                using (var bgBrush = new SolidBrush(_isPressed ? Colors.Border : Colors.SurfaceAlt))
                using (var path = GetRoundedRectPath(new Rectangle(2, 2, Width - 4, Height - 4), 6))
                {
                    g.FillPath(bgBrush, path);
                }
            }

            // 2. Vẽ Icon Bell
            Rectangle iconRect = new Rectangle((Width - 20) / 2, (Height - 20) / 2 + (_isPressed ? 1 : 0), 20, 20);
            Color bellColor = _isHovered ? Colors.Primary : (_unreadCount > 0 ? Colors.TextPrimary : Colors.TextSecondary);
            VectorIcons.Draw(g, SFIcon.Bell, iconRect, bellColor, 1.8f);

            // 3. Vẽ Badge chưa đọc (Chỉ khi unreadCount > 0)
            if (_unreadCount > 0)
            {
                string badgeText = _unreadCount > 99 ? "99+" : _unreadCount.ToString();
                using (var badgeFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(badgeText, badgeFont);
                    int badgeHeight = 16;
                    int badgeWidth = Math.Max(16, (int)Math.Ceiling(textSize.Width) + 6);
                    int badgeX = Width - badgeWidth - 2;
                    int badgeY = 1;

                    Rectangle badgeRect = new Rectangle(badgeX, badgeY, badgeWidth, badgeHeight);

                    // Nền đỏ nổi bật
                    using (var badgeBrush = new SolidBrush(Colors.Danger))
                    using (var badgePath = GetRoundedRectPath(badgeRect, badgeHeight / 2))
                    {
                        g.FillPath(badgeBrush, badgePath);

                        // Viền trắng bảo vệ tương phản
                        using (var borderPen = new Pen(Color.White, 1.2f))
                        {
                            g.DrawPath(borderPen, badgePath);
                        }
                    }

                    // Chữ số trắng ở giữa
                    using (var textBrush = new SolidBrush(Color.White))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        g.DrawString(badgeText, badgeFont, textBrush, new RectangleF(badgeRect.X, badgeRect.Y + 0.5f, badgeRect.Width, badgeRect.Height), sf);
                    }
                }
            }
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
