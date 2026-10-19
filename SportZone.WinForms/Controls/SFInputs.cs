using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Controls
{
    public class SFTextBox : Panel
    {
        protected TextBox innerBox;
        protected bool isFocused = false;
        protected string placeholder = "";
        protected Color borderColor = Colors.Border;
        protected Color focusColor = Colors.Primary;

        public string Placeholder
        {
            get => placeholder;
            set { placeholder = value; Invalidate(); }
        }

        public string PlaceholderText
        {
            get => placeholder;
            set { placeholder = value; Invalidate(); }
        }

        public override string Text
        {
            get => innerBox != null ? innerBox.Text : base.Text;
            set
            {
                base.Text = value;
                if (innerBox != null) innerBox.Text = value;
                Invalidate();
            }
        }

        public bool UseSystemPasswordChar
        {
            get => innerBox != null && innerBox.UseSystemPasswordChar;
            set { if (innerBox != null) innerBox.UseSystemPasswordChar = value; }
        }

        public bool ReadOnly
        {
            get => innerBox != null && innerBox.ReadOnly;
            set
            {
                if (innerBox != null)
                {
                    innerBox.ReadOnly = value;
                    innerBox.BackColor = value ? Colors.SurfaceElevated : Colors.Surface;
                    BackColor = innerBox.BackColor;
                }
            }
        }

        public TextBox InnerTextBox => innerBox;

        public event EventHandler TextChangedEvent;

        public SFTextBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Height = Metrics.InputHeight;
            BackColor = Colors.Surface;
            Padding = new Padding(12, 10, 12, 10);
            Font = Typography.Body;

            innerBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = Typography.Body,
                ForeColor = Colors.TextPrimary,
                BackColor = Colors.Surface,
                Dock = DockStyle.Fill
            };

            innerBox.GotFocus += delegate { isFocused = true; Invalidate(); };
            innerBox.LostFocus += delegate { isFocused = false; Invalidate(); };
            innerBox.TextChanged += delegate
            {
                base.Text = innerBox.Text;
                TextChangedEvent?.Invoke(this, EventArgs.Empty);
                Invalidate();
            };

            Controls.Add(innerBox);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, Radius.Input))
            {
                using (SolidBrush bg = new SolidBrush(innerBox.ReadOnly ? Colors.SurfaceElevated : Colors.Surface))
                {
                    g.FillPath(bg, path);
                }

                Color border = isFocused ? focusColor : (innerBox.ReadOnly ? Colors.Border : borderColor);
                float width = isFocused ? 1.6f : 1.2f;
                using (Pen p = new Pen(border, width))
                {
                    g.DrawPath(p, path);
                }
            }

            // Draw placeholder if empty and not focused
            if (string.IsNullOrEmpty(innerBox.Text) && !string.IsNullOrEmpty(placeholder) && !isFocused)
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                Rectangle phRect = new Rectangle(innerBox.Left + 2, (Height - Font.Height) / 2, Width - innerBox.Left - 10, Font.Height);
                TextRenderer.DrawText(g, placeholder, Font, phRect, Colors.TextMuted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
        }
    }

    public class SFSearchBox : SFTextBox
    {
        private readonly SFIconButton btnClear;

        public SFSearchBox()
        {
            Padding = new Padding(36, 10, 32, 10);
            placeholder = "Tìm kiếm...";

            btnClear = new SFIconButton
            {
                Icon = SFIcon.Close,
                Size = new Size(24, 24),
                Cursor = Cursors.Hand,
                Visible = false,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            btnClear.Click += delegate { Text = ""; };
            Controls.Add(btnClear);
            btnClear.Location = new Point(Width - 28, (Height - 24) / 2);

            innerBox.TextChanged += delegate
            {
                btnClear.Visible = !string.IsNullOrEmpty(innerBox.Text);
            };

            Resize += delegate
            {
                btnClear.Location = new Point(Width - 28, (Height - 24) / 2);
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            // Draw search icon on left
            Rectangle iconRect = new Rectangle(12, (Height - 16) / 2, 16, 16);
            VectorIcons.Draw(g, SFIcon.Search, iconRect, isFocused ? Colors.Primary : Colors.TextMuted, 1.8f);
        }
    }

    public class SFComboBox : ComboBox
    {
        private bool isHovered = false;

        public SFComboBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            Font = Typography.Body;
            Height = Metrics.InputHeight;
            BackColor = Colors.Surface;
            ForeColor = Colors.TextPrimary;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovered = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovered = false; Invalidate(); }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // WM_PAINT = 0xF
            if (m.Msg == 0xF)
            {
                using (Graphics g = Graphics.FromHwnd(Handle))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

                    // Paint custom border
                    using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, Radius.Input))
                    using (Pen p = new Pen(isHovered ? Colors.Primary : Colors.Border, 1.2f))
                    {
                        g.DrawPath(p, path);
                    }

                    // Paint clean arrow button
                    Rectangle arrowRect = new Rectangle(Width - 26, (Height - 14) / 2, 14, 14);
                    VectorIcons.Draw(g, SFIcon.ChevronDown, arrowRect, isHovered ? Colors.Primary : Colors.TextSecondary, 1.8f);
                }
            }
        }
    }
}
