using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Controls
{
    public class SFEmptyState : Panel
    {
        public SFIcon Icon { get; set; } = SFIcon.Field;
        public string Title { get; set; } = "Chưa có dữ liệu";
        public string Description { get; set; } = "Lịch đặt sân hoặc bản ghi mới sẽ xuất hiện tại đây.";
        public string ActionText { get; set; } = "+ Thêm mới";

        public event EventHandler OnActionClicked;

        private readonly SFButton btnAction;

        public SFEmptyState()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Colors.Surface;
            Dock = DockStyle.Fill;

            btnAction = new SFButton
            {
                Text = ActionText,
                Variant = SFButtonVariant.Primary,
                Size = new Size(160, 40),
                LeadingIcon = SFIcon.Plus
            };
            btnAction.Click += (s, e) => OnActionClicked?.Invoke(this, EventArgs.Empty);
            Controls.Add(btnAction);

            Resize += (s, e) =>
            {
                btnAction.Location = new Point((Width - btnAction.Width) / 2, Height / 2 + 50);
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int cy = Height / 2 - 40;

            // Big soft circular icon background
            int circleSize = 68;
            Rectangle circleRect = new Rectangle((Width - circleSize) / 2, cy - 50, circleSize, circleSize);
            using (SolidBrush cb = new SolidBrush(ColorTranslator.FromHtml("#F1F5F9")))
            {
                g.FillEllipse(cb, circleRect);
            }

            Rectangle iconRect = new Rectangle(circleRect.X + 18, circleRect.Y + 18, 32, 32);
            VectorIcons.Draw(g, Icon, iconRect, Colors.Emerald, 2.2f);

            // Title
            Rectangle tr = new Rectangle(20, cy + 24, Width - 40, 24);
            TextRenderer.DrawText(g, Title, Typography.SectionTitle, tr, Colors.TextPrimary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // Description
            Rectangle dr = new Rectangle(40, cy + 50, Width - 80, 20);
            TextRenderer.DrawText(g, Description, Typography.Secondary, dr, Colors.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    public class SFLoadingOverlay : Control
    {
        private readonly Timer timer;
        private float angle = 0;

        public SFLoadingOverlay()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(160, 255, 255, 255);
            Visible = false;

            timer = new Timer { Interval = 20 };
            timer.Tick += (s, e) =>
            {
                angle = (angle + 12) % 360;
                Invalidate();
            };
        }

        public void Start() { Visible = true; BringToFront(); timer.Start(); }
        public void Stop() { timer.Stop(); Visible = false; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cx = Width / 2;
            int cy = Height / 2;
            int radius = 24;
            Rectangle rect = new Rectangle(cx - radius, cy - radius, radius * 2, radius * 2);

            using (Pen track = new Pen(ColorTranslator.FromHtml("#E2E8F0"), 4f))
            {
                g.DrawArc(track, rect, 0, 360);
            }

            using (Pen spinner = new Pen(Colors.Emerald, 4f))
            {
                spinner.StartCap = LineCap.Round;
                spinner.EndCap = LineCap.Round;
                g.DrawArc(spinner, rect, angle, 90);
            }
        }
    }
}
