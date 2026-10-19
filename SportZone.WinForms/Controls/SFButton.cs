using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Controls
{
    public enum SFButtonVariant
    {
        Primary,
        Secondary,
        Outline,
        Danger,
        Ghost,
        Accent,
        Dark
    }

    public class SFButton : Button
    {
        private bool isHovered = false;
        private bool isPressed = false;
        private SFButtonVariant variant = SFButtonVariant.Primary;
        private SFIcon leadingIcon = SFIcon.None;
        private SFIcon trailingIcon = SFIcon.None;
        private int cornerRadius = Radius.Button;

        public SFButtonVariant Variant
        {
            get => variant;
            set { variant = value; Invalidate(); }
        }

        public SFIcon LeadingIcon
        {
            get => leadingIcon;
            set { leadingIcon = value; Invalidate(); }
        }

        public SFIcon IconType
        {
            get => leadingIcon;
            set { leadingIcon = value; Invalidate(); }
        }

        public SFIcon TrailingIcon
        {
            get => trailingIcon;
            set { trailingIcon = value; Invalidate(); }
        }

        public int CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = value; Invalidate(); }
        }

        public SFButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            Height = Metrics.ButtonHeight;
            Cursor = Cursors.Hand;
            Font = Typography.Button;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (mevent.Button == MouseButtons.Left)
            {
                isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = ClientRectangle;
            if (rect.Width <= 1 || rect.Height <= 1) return;

            // Inset slightly when pressed for tactical click feedback
            if (isPressed)
            {
                rect.Inflate(-1, -1);
            }

            Color bg, fg, border;
            GetColors(out bg, out fg, out border);

            // Pre-fill background with parent color to eliminate black rounded corner artifacts
            Color effParent = GraphicsUtility.GetEffectiveBackColor(this, Colors.AppBackground);
            using (SolidBrush pb = new SolidBrush(effParent))
            {
                g.FillRectangle(pb, ClientRectangle);
            }

            // Draw background
            using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, cornerRadius))
            {
                if (bg != Color.Transparent)
                {
                    using (SolidBrush b = new SolidBrush(bg))
                    {
                        g.FillPath(b, path);
                    }
                }

                if (border != Color.Transparent)
                {
                    using (Pen p = new Pen(border, 1.2f))
                    {
                        g.DrawPath(p, path);
                    }
                }
            }

            // Draw Leading Icon, Text, Trailing Icon
            string displayText = Text ?? "";
            if (leadingIcon == SFIcon.Plus && (displayText.StartsWith("+ ") || displayText.StartsWith("+")))
            {
                displayText = displayText.TrimStart('+', ' ');
            }

            int iconSize = 16;
            int textWidth = TextRenderer.MeasureText(displayText, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix).Width;
            int totalContentWidth = textWidth;
            if (leadingIcon != SFIcon.None) totalContentWidth += iconSize + 8;
            if (trailingIcon != SFIcon.None) totalContentWidth += iconSize + 8;

            int startX = (Width - totalContentWidth) / 2;
            if (TextAlign == ContentAlignment.MiddleLeft) startX = 14;

            if (leadingIcon != SFIcon.None)
            {
                Rectangle iconRect = new Rectangle(startX, (Height - iconSize) / 2, iconSize, iconSize);
                VectorIcons.Draw(g, leadingIcon, iconRect, fg, 1.8f);
                startX += iconSize + 8;
            }

            if (!string.IsNullOrEmpty(displayText))
            {
                Rectangle textRect = new Rectangle(startX, 0, textWidth + 4, Height);
                TextRenderer.DrawText(g, displayText, Font, textRect, fg,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                startX += textWidth + 8;
            }

            if (trailingIcon != SFIcon.None)
            {
                Rectangle iconRect = new Rectangle(startX, (Height - iconSize) / 2, iconSize, iconSize);
                VectorIcons.Draw(g, trailingIcon, iconRect, fg, 1.8f);
            }
        }

        private void GetColors(out Color bg, out Color fg, out Color border)
        {
            if (!Enabled)
            {
                bg = ColorTranslator.FromHtml("#E2E8F0");
                fg = ColorTranslator.FromHtml("#94A3B8");
                border = Color.Transparent;
                return;
            }

            switch (variant)
            {
                case SFButtonVariant.Primary:
                    bg = isPressed ? Colors.PrimaryPressed : (isHovered ? Colors.PrimaryHover : Colors.Primary);
                    fg = Color.White;
                    border = Color.Transparent;
                    break;

                case SFButtonVariant.Secondary:
                    bg = isPressed ? ColorTranslator.FromHtml("#E2E8F0") : (isHovered ? ColorTranslator.FromHtml("#F8FAFC") : Colors.Surface);
                    fg = Colors.TextPrimary;
                    border = isHovered ? Colors.BorderDark : Colors.Border;
                    break;

                case SFButtonVariant.Outline:
                    bg = isPressed ? Color.FromArgb(30, Colors.Primary) : (isHovered ? Color.FromArgb(15, Colors.Primary) : Color.Transparent);
                    fg = Colors.Primary;
                    border = Colors.Primary;
                    break;

                case SFButtonVariant.Danger:
                    bg = isPressed ? ColorTranslator.FromHtml("#BE123C") : (isHovered ? ColorTranslator.FromHtml("#E11D48") : Colors.Danger);
                    fg = Color.White;
                    border = Color.Transparent;
                    break;

                case SFButtonVariant.Ghost:
                    bg = isPressed ? ColorTranslator.FromHtml("#E2E8F0") : (isHovered ? ColorTranslator.FromHtml("#F1F5F9") : Color.Transparent);
                    fg = isHovered ? Colors.TextPrimary : Colors.TextSecondary;
                    border = Color.Transparent;
                    break;

                case SFButtonVariant.Accent:
                    bg = isPressed ? ColorTranslator.FromHtml("#1D4ED8") : (isHovered ? ColorTranslator.FromHtml("#2563EB") : Colors.ElectricBlue);
                    fg = Color.White;
                    border = Color.Transparent;
                    break;

                case SFButtonVariant.Dark:
                    bg = isPressed ? ColorTranslator.FromHtml("#07111F") : (isHovered ? ColorTranslator.FromHtml("#12243A") : ColorTranslator.FromHtml("#0C1A2B"));
                    fg = Color.White;
                    border = Color.Transparent;
                    break;

                default:
                    bg = Colors.Primary;
                    fg = Color.White;
                    border = Color.Transparent;
                    break;
            }
        }
    }

    public class SFIconButton : Button
    {
        private bool isHovered = false;
        private bool isPressed = false;
        private SFIcon icon = SFIcon.None;
        private Color normalColor = Colors.TextSecondary;
        private Color hoverColor = Colors.TextPrimary;
        private Color normalBg = Color.Transparent;
        private Color hoverBg = ColorTranslator.FromHtml("#F1F5F9");
        private int cornerRadius = 6;

        public SFIcon Icon
        {
            get => icon;
            set { icon = value; Invalidate(); }
        }

        public Color NormalColor
        {
            get => normalColor;
            set { normalColor = value; Invalidate(); }
        }

        public Color HoverColor
        {
            get => hoverColor;
            set { hoverColor = value; Invalidate(); }
        }

        public Color NormalBg
        {
            get => normalBg;
            set { normalBg = value; Invalidate(); }
        }

        public Color HoverBg
        {
            get => hoverBg;
            set { hoverBg = value; Invalidate(); }
        }

        public SFIconButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            Size = new Size(34, 34);
            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovered = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovered = false; isPressed = false; Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs mevent) { base.OnMouseDown(mevent); isPressed = true; Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs mevent) { base.OnMouseUp(mevent); isPressed = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            if (rect.Width <= 1 || rect.Height <= 1) return;

            Color clearColor = normalBg != Color.Transparent ? normalBg : (BackColor != Color.Transparent ? BackColor : GraphicsUtility.GetEffectiveBackColor(this, Colors.AppBackground));
            g.Clear(clearColor);

            Color currentBg = isHovered ? hoverBg : normalBg;
            Color currentFg = isHovered ? hoverColor : normalColor;

            if (currentBg != Color.Transparent)
            {
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, cornerRadius))
                using (SolidBrush b = new SolidBrush(currentBg))
                {
                    g.FillPath(b, path);
                }
            }

            Rectangle iconRect = new Rectangle((Width - 18) / 2, (Height - 18) / 2, 18, 18);
            if (isPressed) iconRect.Offset(0, 1);
            VectorIcons.Draw(g, icon, iconRect, currentFg, 1.8f);
        }
    }
}
