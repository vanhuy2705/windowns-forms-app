using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Controls
{
    public class SFFieldCard : Panel
    {
        public string FieldName { get; set; } = "Sân bóng đá A1";
        public string SportType { get; set; } = "Bóng đá 7 người";
        public string LocationArea { get; set; } = "Khu A • Cỏ FIFA Pro";
        public string PricePerHour { get; set; } = "350.000đ";
        public string Status { get; set; } = "Sẵn sàng";
        public string[] Amenities { get; set; } = new[] { "Đèn LED", "Phòng thay đồ", "Wifi" };
        public Image FieldImage { get; set; }

        public event EventHandler OnBookClicked;
        public event EventHandler OnDetailClicked;

        private bool isHovered = false;

        public SFFieldCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Size = new Size(330, 275);
            Margin = new Padding(0, 0, 16, 16);
            BackColor = Colors.Surface;

            SFButton btnDetail = new SFButton
            {
                Text = "Chi tiết",
                Variant = SFButtonVariant.Secondary,
                Size = new Size(110, 36),
                Location = new Point(14, 226)
            };
            btnDetail.Click += (s, e) => OnDetailClicked?.Invoke(this, EventArgs.Empty);

            SFButton btnBook = new SFButton
            {
                Text = "ĐẶT SÂN",
                Variant = SFButtonVariant.Primary,
                Size = new Size(130, 36),
                Location = new Point(132, 226),
                LeadingIcon = SFIcon.Plus
            };
            btnBook.Click += (s, e) => OnBookClicked?.Invoke(this, EventArgs.Empty);

            Controls.Add(btnDetail);
            Controls.Add(btnBook);

            Resize += (s, e) =>
            {
                btnDetail.Location = new Point(14, Height - 46);
                btnBook.Location = new Point(Math.Max(130, Width - btnBook.Width - 14), Height - 46);
            };
        }

        public SFFieldCard(string name, string sport, string location, string price, string status, Image img, string[] amenities)
            : this()
        {
            FieldName = name;
            SportType = sport;
            LocationArea = location;
            PricePerHour = price;
            Status = status;
            FieldImage = img;
            if (amenities != null) Amenities = amenities;
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovered = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovered = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            GraphicsUtility.DrawShadow(g, rect, Radius.Card, isHovered ? 2 : 1);

            // Card background & border
            using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, Radius.Card))
            {
                using (SolidBrush b = new SolidBrush(Colors.Surface)) g.FillPath(b, path);
                using (Pen p = new Pen(isHovered ? Colors.Primary : Colors.Border, 1.2f)) g.DrawPath(p, path);
            }

            // Top Image 16:9 banner
            int imgH = 132;
            Rectangle imgRect = new Rectangle(1, 1, Width - 2, imgH);
            using (GraphicsPath topPath = new GraphicsPath())
            {
                int r = Radius.Card;
                topPath.AddArc(1, 1, r * 2, r * 2, 180, 90);
                topPath.AddArc(Width - 1 - r * 2, 1, r * 2, r * 2, 270, 90);
                topPath.AddLine(Width - 1, imgH, 1, imgH);
                topPath.CloseFigure();

                GraphicsState state = g.Save();
                g.SetClip(topPath);

                Image img = FieldImage ?? AssetManager.GetFieldImage(FieldName);
                if (img != null)
                {
                    GraphicsUtility.DrawImageCenterCrop(g, img, imgRect);
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(ColorTranslator.FromHtml("#0C1A2B"))) g.FillRectangle(b, imgRect);
                }

                // Dark vignette gradient overlay for contrast
                using (LinearGradientBrush vig = new LinearGradientBrush(imgRect, Color.Transparent, Color.FromArgb(120, 7, 17, 31), LinearGradientMode.Vertical))
                {
                    g.FillRectangle(vig, imgRect);
                }

                g.Restore(state);
            }

            // Status Badge Overlay on top of image (top-right)
            Color badgeBg = Status == "Sẵn sàng" ? Colors.SuccessBg : (Status == "Bảo trì" ? Colors.OrangeBg : Colors.InfoBg);
            Color badgeFg = Status == "Sẵn sàng" ? Colors.DeepEmerald : (Status == "Bảo trì" ? Colors.Orange : Colors.ElectricBlue);
            Size badgeSize = TextRenderer.MeasureText(Status, Typography.MicroBold);
            Rectangle badgeRect = new Rectangle(Width - badgeSize.Width - 28, 12, badgeSize.Width + 16, 22);

            using (GraphicsPath bp = GraphicsUtility.CreateRoundedRectangle(badgeRect, 11))
            using (SolidBrush bb = new SolidBrush(badgeBg))
            {
                g.FillPath(bb, bp);
            }
            TextRenderer.DrawText(g, Status, Typography.MicroBold, badgeRect, badgeFg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // Field Name
            Rectangle nameRect = new Rectangle(14, imgH + 10, Width - 28, 20);
            TextRenderer.DrawText(g, FieldName, Typography.SectionTitle, nameRect, Colors.TextPrimary, TextFormatFlags.Left);

            // Sub info
            Rectangle subRect = new Rectangle(14, imgH + 32, Width - 28, 16);
            TextRenderer.DrawText(g, $"{SportType}  •  {LocationArea}", Typography.Secondary, subRect, Colors.TextSecondary, TextFormatFlags.Left);

            // Amenities Chips
            int chipX = 14;
            int chipY = imgH + 52;
            foreach (string amen in Amenities)
            {
                Size amSize = TextRenderer.MeasureText(amen, Typography.Micro);
                Rectangle chipRect = new Rectangle(chipX, chipY, amSize.Width + 10, 18);
                if (chipX + chipRect.Width < Width - 14)
                {
                    using (GraphicsPath cp = GraphicsUtility.CreateRoundedRectangle(chipRect, 4))
                    using (SolidBrush cb = new SolidBrush(ColorTranslator.FromHtml("#F1F5F9")))
                    {
                        g.FillPath(cb, cp);
                    }
                    TextRenderer.DrawText(g, amen, Typography.Micro, chipRect, Colors.TextSecondary,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    chipX += chipRect.Width + 6;
                }
            }

            // Price per hour (Right aligned above buttons or left)
            Rectangle priceRect = new Rectangle(Width - 145, 228, 130, 32);
            // Notice: buttons are placed at Location(14, 226) and Location(132, 226), so price can be placed on the line above
            Rectangle priceTopRect = new Rectangle(Width - 140, imgH + 10, 126, 20);
            TextRenderer.DrawText(g, PricePerHour + "/h", Typography.BodyBold, priceTopRect, Colors.DeepEmerald, TextFormatFlags.Right);
        }
    }

    public class SFVoucherCard : Panel
    {
        public string Code { get; set; } = "SPORT20";
        public string DiscountBadge { get; set; } = "-20%";
        public string Title { get; set; } = "Giảm tối đa 100K cho đơn từ 300K";
        public string Expiry { get; set; } = "HSD: 31/10/2026";
        public Color AccentColor { get; set; } = Colors.Purple;

        public event EventHandler OnUseClicked;

        private bool isHovered = false;

        public SFVoucherCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Size = new Size(350, 128);
            Margin = new Padding(0, 0, 16, 16);
            BackColor = Colors.Surface;

            btnUse = new SFButton
            {
                Text = "Dùng ngay",
                Variant = SFButtonVariant.Primary,
                Size = new Size(92, 32),
                Location = new Point(244, 82)
            };
            btnUse.Click += (s, e) => OnUseClicked?.Invoke(this, EventArgs.Empty);
            Controls.Add(btnUse);

            SizeChanged += (s, e) =>
            {
                if (btnUse != null)
                {
                    btnUse.Location = new Point(Width - btnUse.Width - 14, Height - btnUse.Height - 14);
                }
            };
        }

        private readonly SFButton btnUse;

        public SFVoucherCard(string code, string badge, string title, string expiry, Color accent)
            : this()
        {
            Code = code;
            DiscountBadge = badge;
            Title = title;
            Expiry = expiry;
            AccentColor = accent;
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovered = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovered = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Pre-fill parent background
            Color effParent = GraphicsUtility.GetEffectiveBackColor(this, Colors.AppBackground);
            using (SolidBrush pb = new SolidBrush(effParent))
            {
                g.FillRectangle(pb, ClientRectangle);
            }

            GraphicsUtility.DrawShadow(g, rect, 10, isHovered ? 2 : 1);

            int leftColW = 92;

            // Card body path
            using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, 10))
            {
                using (SolidBrush b = new SolidBrush(Colors.Surface)) g.FillPath(b, path);
                using (Pen p = new Pen(isHovered ? AccentColor : Colors.Border, 1.2f)) g.DrawPath(p, path);
            }

            // Left colored ticket coupon area
            Rectangle leftRect = new Rectangle(1, 1, leftColW, Height - 2);
            using (GraphicsPath lp = new GraphicsPath())
            {
                int r = 10;
                lp.AddArc(1, 1, r * 2, r * 2, 180, 90);
                lp.AddLine(leftColW, 1, leftColW, Height - 1);
                lp.AddArc(1, Height - 1 - r * 2, r * 2, r * 2, 90, 90);
                lp.CloseFigure();

                using (SolidBrush lb = new SolidBrush(Color.FromArgb(24, AccentColor))) g.FillPath(lb, lp);
                using (Pen lpen = new Pen(Color.FromArgb(60, AccentColor), 1f)) g.DrawPath(lpen, lp);
            }

            // Draw Discount in left block
            Font badgeFont = (DiscountBadge != null && DiscountBadge.Length > 5) ? Typography.SectionTitle : Typography.HeroTitle;
            Rectangle badgeRect = new Rectangle(4, (Height - 48) / 2, leftColW - 8, 48);
            TextRenderer.DrawText(g, DiscountBadge, badgeFont, badgeRect, AccentColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);

            // Dotted perforated line
            using (Pen dotPen = new Pen(Colors.BorderDark, 1.5f))
            {
                dotPen.DashStyle = DashStyle.Dot;
                g.DrawLine(dotPen, leftColW, 12, leftColW, Height - 12);
            }

            // Ticket cutout circular notches on top and bottom of perforated line
            int notchR = 7;
            using (SolidBrush notchBrush = new SolidBrush(effParent))
            using (Pen notchBorder = new Pen(isHovered ? AccentColor : Colors.Border, 1.2f))
            {
                Rectangle topNotch = new Rectangle(leftColW - notchR, -notchR, notchR * 2, notchR * 2);
                Rectangle bottomNotch = new Rectangle(leftColW - notchR, Height - notchR, notchR * 2, notchR * 2);

                g.FillEllipse(notchBrush, topNotch);
                g.DrawEllipse(notchBorder, topNotch);

                g.FillEllipse(notchBrush, bottomNotch);
                g.DrawEllipse(notchBorder, bottomNotch);
            }

            // Right ticket content
            int rightX = leftColW + 16;
            Rectangle codeRect = new Rectangle(rightX, 14, 110, 22);
            using (GraphicsPath cbp = GraphicsUtility.CreateRoundedRectangle(codeRect, 4))
            using (SolidBrush cbb = new SolidBrush(ColorTranslator.FromHtml("#F1F5F9")))
            {
                g.FillPath(cbb, cbp);
            }
            TextRenderer.DrawText(g, Code, Typography.SecondaryBold, codeRect, Colors.TextPrimary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            Rectangle titleRect = new Rectangle(rightX, 42, Width - rightX - 16, 34);
            TextRenderer.DrawText(g, Title, Typography.Secondary, titleRect, Colors.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.WordBreak);

            Rectangle expRect = new Rectangle(rightX, 88, 130, 20);
            TextRenderer.DrawText(g, Expiry, Typography.Micro, expRect, Colors.TextMuted, TextFormatFlags.Left);
        }
    }
}
