using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Dialogs;

namespace SportZone.WinForms
{
    /// <summary>
    /// V9 Premium Redesigned Login Form with genuine 2-column layout:
    /// Left Column: Sports Hero & Branding showcase with feature badges.
    /// Right Column: Elevated Login Card with modern input controls, password eye-toggle, and V9 tokens.
    /// </summary>
    public class LoginForm : Form
    {
        // Public properties expected by Program.cs & MainForm
        public string SelectedRole { get; private set; }
        public string DisplayName { get; private set; }

        // Native Win32 window dragging
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        // Interactive controls
        private TableLayoutPanel rootSplit;
        private Panel heroPanel;
        private Panel cardWrapper;
        private Panel cardPanel;
        private TextBox innerTxtUser;
        private TextBox innerTxtPassword;
        private bool isPasswordHidden = true;
        private bool isRememberChecked = true;
        private int activeTab = 0; // 0 = Đăng nhập, 1 = Đăng ký

        // Design Tokens V9 (#0F766E Teal, #14B8A6 Accent, #F5F7FA Background)
        private static readonly Color ColBgDark = Color.FromArgb(8, 28, 36);
        private static readonly Color ColTeal = Color.FromArgb(15, 118, 110);         // #0F766E
        private static readonly Color ColTealHover = Color.FromArgb(17, 94, 89);      // #115E59
        private static readonly Color ColTealPressed = Color.FromArgb(19, 78, 74);    // #134E4A
        private static readonly Color ColAccent = Color.FromArgb(20, 184, 166);       // #14B8A6
        private static readonly Color ColAccentLight = Color.FromArgb(204, 251, 241); // #CCFBF1
        private static readonly Color ColTextDark = Color.FromArgb(15, 23, 42);       // #0F172A
        private static readonly Color ColTextMuted = Color.FromArgb(100, 116, 139);   // #64748B
        private static readonly Color ColBorder = Color.FromArgb(226, 232, 240);      // #E2E8F0
        private static readonly Color ColBorderFocus = Color.FromArgb(15, 118, 110);  // #0F766E

        public LoginForm()
        {
            DpiHelper.ApplyDpiMode(this);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Text = "SPORTZONE - Hệ thống quản lý đặt sân thể thao";
            try { this.Icon = Helpers.SportFieldIconProvider.GetWindowIcon(Helpers.SportFieldIcon.App); } catch { }
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1040, 600);
            MinimumSize = new Size(960, 540);
            FormBorderStyle = FormBorderStyle.None;
            BackColor = ColBgDark;
            DoubleBuffered = true;

            BuildUI();
            UpdateRegion();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\SportField\Login"))
                {
                    if (key != null)
                    {
                        string savedUser = key.GetValue("Username") as string;
                        object remVal = key.GetValue("Remember");
                        bool isRem = remVal == null || Convert.ToInt32(remVal) == 1;
                        if (!string.IsNullOrEmpty(savedUser) && isRem)
                        {
                            isRememberChecked = true;
                            if (innerTxtUser != null) innerTxtUser.Text = savedUser;
                        }
                    }
                }
            }
            catch { }

            if (innerTxtUser != null)
            {
                innerTxtUser.SelectionLength = 0;
                innerTxtUser.SelectionStart = innerTxtUser.Text.Length;
            }
        }

        private void UpdateRegion()
        {
            try
            {
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(new Rectangle(0, 0, Width, Height), 18))
                {
                    Region = new Region(path);
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning("UpdateRegion failed: " + ex.Message);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
            if (cardPanel != null && cardWrapper != null)
            {
                CenterCardInWrapper();
            }
            Invalidate();
        }

        private void CenterCardInWrapper()
        {
            if (cardPanel == null || cardWrapper == null) return;
            int cardW = Math.Min(430, cardWrapper.ClientSize.Width - 32);
            int cardH = Math.Min(540, cardWrapper.ClientSize.Height - 32);
            int cardX = (cardWrapper.ClientSize.Width - cardW) / 2;
            int cardY = (cardWrapper.ClientSize.Height - cardH) / 2;
            cardPanel.SetBounds(cardX, cardY, cardW, cardH);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void BuildUI()
        {
            // 2-Column Split: Column 0 = Left Hero (54%), Column 1 = Right Card (46%)
            rootSplit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ColBgDark,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            rootSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            rootSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            rootSplit.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(rootSplit);

            // ==========================================
            // COLUMN 0: HERO BRANDING PANEL
            // ==========================================
            heroPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBgDark,
                Margin = new Padding(0),
                Padding = new Padding(36, 28, 24, 28)
            };
            heroPanel.Paint += DrawHeroPanel;
            heroPanel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };
            rootSplit.Controls.Add(heroPanel, 0, 0);

            // ==========================================
            // COLUMN 1: RIGHT CARD WRAPPER & ELEVATED CARD
            // ==========================================
            cardWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBgDark,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            cardWrapper.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };
            cardWrapper.Resize += (s, e) => CenterCardInWrapper();

            // Floating Elevated Card Panel
            cardPanel = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(22, 12, 22, 14)
            };
            cardPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = cardPanel.ClientRectangle;
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(new Rectangle(0, 0, r.Width - 1, r.Height - 1), 18))
                {
                    cardPanel.Region = new Region(p);

                    using (Pen pen = new Pen(Color.FromArgb(230, 235, 242), 1.2f))
                    {
                        g.DrawPath(pen, p);
                    }
                }
            };
            cardWrapper.Controls.Add(cardPanel);
            rootSplit.Controls.Add(cardWrapper, 1, 0);
            CenterCardInWrapper();

            // Card Inner Stack
            TableLayoutPanel stack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 9,
                BackColor = Color.White,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < 9; i++) stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            cardPanel.Controls.Add(stack);

            // ROW 0: WINDOW CONTROLS
            Panel topBar = CreateCardTopBar();
            topBar.Margin = new Padding(0, 0, 0, 4);
            stack.Controls.Add(topBar, 0, 0);

            // ROW 1: TAB SWITCHER
            Panel tabContainer = CreateTabSwitcher();
            tabContainer.Margin = new Padding(0, 0, 0, 8);
            stack.Controls.Add(tabContainer, 0, 1);

            // ROW 2: GREETING HEADER
            Panel headerPanel = CreateHeaderPanel();
            headerPanel.Margin = new Padding(0, 0, 0, 8);
            stack.Controls.Add(headerPanel, 0, 2);

            // ROW 3: USERNAME INPUT
            Panel userInput = CreateInputField(
                label: "Tên đăng nhập",
                placeholder: "Nhập tên đăng nhập",
                defaultVal: "",
                isPassword: false,
                outTextBox: out innerTxtUser
            );
            userInput.Margin = new Padding(0, 0, 0, 8);
            stack.Controls.Add(userInput, 0, 3);

            // ROW 4: PASSWORD INPUT WITH EYE TOGGLE
            Panel pwdInput = CreateInputField(
                label: "Mật khẩu",
                placeholder: "Nhập mật khẩu",
                defaultVal: "",
                isPassword: true,
                outTextBox: out innerTxtPassword
            );
            pwdInput.Margin = new Padding(0, 0, 0, 8);
            stack.Controls.Add(pwdInput, 0, 4);

            // ROW 5: OPTIONS ROW (Remember Me & Forgot Password)
            Panel optionsRow = CreateOptionsRow();
            optionsRow.Margin = new Padding(0, 0, 0, 10);
            stack.Controls.Add(optionsRow, 0, 5);

            // ROW 6: LOGIN BUTTON
            Panel btnLogin = CreateLoginButton();
            btnLogin.Margin = new Padding(0, 0, 0, 12);
            stack.Controls.Add(btnLogin, 0, 6);

            // ROW 7: REGISTER PROMPT
            Panel regPrompt = CreateRegisterPrompt();
            regPrompt.Margin = new Padding(0, 0, 0, 10);
            stack.Controls.Add(regPrompt, 0, 7);

            // ROW 8: SECURITY BANNER
            Panel secBanner = CreateSecurityBanner();
            secBanner.Margin = new Padding(0);
            stack.Controls.Add(secBanner, 0, 8);
        }

        // =========================================================
        // LEFT HERO PANEL DRAWING (Sports theme, badges, value props)
        // =========================================================
        private void DrawHeroPanel(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = heroPanel.ClientRectangle;

            // 1. Rich dark teal gradient background
            using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                rect,
                Color.FromArgb(6, 22, 30),
                Color.FromArgb(12, 54, 52),
                45f))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // Subtle field markings (pitch lines in soft teal)
            using (Pen pitchPen = new Pen(Color.FromArgb(18, 255, 255, 255), 1.5f))
            {
                g.DrawEllipse(pitchPen, -40, rect.Height / 2 - 100, 200, 200);
                g.DrawLine(pitchPen, 60, 0, 60, rect.Height);
            }

            int leftPad = 36;
            int curY = 32;

            // 2. Brand Mark & Badge
            Rectangle logoBox = new Rectangle(leftPad, curY, 40, 40);
            using (GraphicsPath lp = GraphicsUtility.CreateRoundedRectangle(logoBox, 10))
            using (SolidBrush lb = new SolidBrush(ColTeal))
            {
                g.FillPath(lb, lp);
            }
            VectorIcons.Draw(g, SFIcon.Field, new Rectangle(logoBox.X + 8, logoBox.Y + 8, 24, 24), Color.White, 2.2f);

            // Brand Text
            using (Font fBrand = new Font("Segoe UI", 16f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, "SPORTZONE", fBrand, new Point(leftPad + 48, curY + 6), Color.White);
            }

            // Pill Badge "V9 PREMIUM"
            using (Font fBadge = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            {
                Rectangle pillRect = new Rectangle(leftPad + 195, curY + 11, 88, 22);
                using (GraphicsPath pp = GraphicsUtility.CreateRoundedRectangle(pillRect, 11))
                using (SolidBrush pb = new SolidBrush(Color.FromArgb(30, ColAccent)))
                {
                    g.FillPath(pb, pp);
                    using (Pen ppen = new Pen(ColAccent, 1.2f))
                        g.DrawPath(ppen, pp);
                }
                TextRenderer.DrawText(g, "V9 PREMIUM", fBadge, pillRect, ColAccentLight, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            curY += 68;

            // 3. Hero Headline
            using (Font fTitle = new Font("Segoe UI", 20f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, "HỆ THỐNG QUẢN LÝ", fTitle, new Point(leftPad, curY), Color.White);
                curY += 34;
                TextRenderer.DrawText(g, "SÂN THỂ THAO ĐA NĂNG", fTitle, new Point(leftPad, curY), ColAccentLight);
                curY += 40;
            }

            // Subtitle
            using (Font fSub = new Font("Segoe UI", 9.5f, FontStyle.Regular))
            {
                Rectangle subRect = new Rectangle(leftPad, curY, rect.Width - leftPad - 30, 40);
                TextRenderer.DrawText(g, "Nền tảng vận hành sân thông minh, kết nối khách hàng và ban quản lý với trải nghiệm mượt mà, tức thời.", fSub, subRect, Color.FromArgb(203, 213, 225), TextFormatFlags.WordBreak);
                curY += 48;
            }

            // 4. Feature Highlights (4 feature items)
            var features = new[]
            {
                ("Đặt sân trực tuyến & giữ chỗ", "Lịch trống thời gian thực, chống trùng lịch tuyệt đối"),
                ("Thanh toán tự động VietQR", "Sinh mã QR động, đối soát tự động nhanh chóng"),
                ("Phân quyền đa cấp an toàn", "Dành riêng cho Quản trị viên, Nhân viên và Khách hàng"),
                ("Báo cáo & thống kê tức thì", "Theo dõi doanh thu, công suất sân và lịch đặt liên tục")
            };

            using (Font fFeatTitle = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (Font fFeatDesc = new Font("Segoe UI", 8.25f, FontStyle.Regular))
            {
                foreach (var feat in features)
                {
                    int circleSize = 22;
                    Rectangle circleRect = new Rectangle(leftPad, curY + 2, circleSize, circleSize);
                    using (SolidBrush cb = new SolidBrush(Color.FromArgb(35, ColAccent)))
                    {
                        g.FillEllipse(cb, circleRect);
                    }
                    using (Pen cp = new Pen(ColAccent, 1.8f))
                    {
                        cp.StartCap = LineCap.Round;
                        cp.EndCap = LineCap.Round;
                        g.DrawLine(cp, leftPad + 6, curY + 13, leftPad + 9, curY + 16);
                        g.DrawLine(cp, leftPad + 9, curY + 16, leftPad + 16, curY + 9);
                    }

                    TextRenderer.DrawText(g, feat.Item1, fFeatTitle, new Point(leftPad + 30, curY), Color.White);
                    TextRenderer.DrawText(g, feat.Item2, fFeatDesc, new Point(leftPad + 30, curY + 18), Color.FromArgb(148, 163, 184));

                    curY += 46;
                }
            }

            // 5. Bottom Security Badge
            int footY = rect.Height - 44;
            Rectangle footBox = new Rectangle(leftPad, footY, rect.Width - leftPad - 30, 28);
            using (GraphicsPath fp = GraphicsUtility.CreateRoundedRectangle(footBox, 8))
            using (SolidBrush fb = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
            {
                g.FillPath(fb, fp);
            }
            using (Font fSec = new Font("Segoe UI", 8f, FontStyle.Regular))
            {
                TextRenderer.DrawText(g, "🔒 Chuẩn bảo mật PBKDF2-HMAC-SHA256 • Mã hóa đa lớp an toàn", fSec, footBox, Color.FromArgb(167, 243, 208), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        // =========================================================
        // HELPER: CARD TOP BAR (Window Controls: Minimize, Close)
        // =========================================================
        private Panel CreateCardTopBar()
        {
            Panel bar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 24,
                BackColor = Color.White
            };

            int btnSize = 22;

            Panel btnClose = new Panel
            {
                Size = new Size(btnSize, btnSize),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            bool closeHover = false;
            btnClose.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, btnClose.Width - 1, btnClose.Height - 1);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 5))
                {
                    if (closeHover)
                    {
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(239, 68, 68)))
                            g.FillPath(b, p);
                    }
                    using (Pen pen = new Pen(closeHover ? Color.White : Color.FromArgb(100, 116, 139), 1.5f))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        int m = 6;
                        g.DrawLine(pen, m, m, btnClose.Width - m, btnClose.Height - m);
                        g.DrawLine(pen, btnClose.Width - m, m, m, btnClose.Height - m);
                    }
                }
            };
            btnClose.MouseEnter += (s, e) => { closeHover = true; btnClose.Invalidate(); };
            btnClose.MouseLeave += (s, e) => { closeHover = false; btnClose.Invalidate(); };
            btnClose.Click += (s, e) => Application.Exit();

            Panel btnMin = new Panel
            {
                Size = new Size(btnSize, btnSize),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            bool minHover = false;
            btnMin.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, btnMin.Width - 1, btnMin.Height - 1);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 5))
                {
                    if (minHover)
                    {
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(241, 245, 249)))
                            g.FillPath(b, p);
                    }
                    using (Pen pen = new Pen(Color.FromArgb(100, 116, 139), 1.5f))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        int m = 6;
                        g.DrawLine(pen, m, btnMin.Height / 2 + 3, btnMin.Width - m, btnMin.Height / 2 + 3);
                    }
                }
            };
            btnMin.MouseEnter += (s, e) => { minHover = true; btnMin.Invalidate(); };
            btnMin.MouseLeave += (s, e) => { minHover = false; btnMin.Invalidate(); };
            btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;

            bar.Resize += (s, e) =>
            {
                btnClose.Location = new Point(bar.Width - btnSize, 1);
                btnMin.Location = new Point(btnClose.Left - btnSize - 4, 1);
            };

            bar.Controls.Add(btnClose);
            bar.Controls.Add(btnMin);

            bar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };

            return bar;
        }

        // =========================================================
        // HELPER: TAB SWITCHER (Segmented Control: Đăng nhập / Đăng ký)
        // =========================================================
        private Panel CreateTabSwitcher()
        {
            Panel tabBox = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(243, 244, 246)
            };

            tabBox.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = tabBox.ClientRectangle;
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(new Rectangle(0, 0, r.Width - 1, r.Height - 1), 12))
                {
                    tabBox.Region = new Region(p);
                }

                int halfW = (tabBox.Width - 6) / 2;
                if (activeTab == 0)
                {
                    Rectangle pill = new Rectangle(3, 3, halfW, tabBox.Height - 6);
                    using (GraphicsPath pp = GraphicsUtility.CreateRoundedRectangle(pill, 10))
                    {
                        using (SolidBrush b = new SolidBrush(Color.White))
                            g.FillPath(b, pp);
                        using (Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1f))
                            g.DrawPath(pen, pp);
                    }

                    using (SolidBrush b = new SolidBrush(ColTeal))
                    {
                        Rectangle acc = new Rectangle(pill.Left + 22, pill.Bottom - 3, pill.Width - 44, 3);
                        using (GraphicsPath ap = GraphicsUtility.CreateRoundedRectangle(acc, 2))
                            g.FillPath(b, ap);
                    }
                }
                else
                {
                    Rectangle pill = new Rectangle(3 + halfW, 3, halfW, tabBox.Height - 6);
                    using (GraphicsPath pp = GraphicsUtility.CreateRoundedRectangle(pill, 10))
                    {
                        using (SolidBrush b = new SolidBrush(Color.White))
                            g.FillPath(b, pp);
                        using (Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1f))
                            g.DrawPath(pen, pp);
                    }
                    using (SolidBrush b = new SolidBrush(ColTeal))
                    {
                        Rectangle acc = new Rectangle(pill.Left + 22, pill.Bottom - 3, pill.Width - 44, 3);
                        using (GraphicsPath ap = GraphicsUtility.CreateRoundedRectangle(acc, 2))
                            g.FillPath(b, ap);
                    }
                }

                int t1CenterX = 3 + halfW / 2;
                using (Font t1Font = new Font("Segoe UI", 9.5f, activeTab == 0 ? FontStyle.Bold : FontStyle.Regular))
                {
                    Color t1Color = activeTab == 0 ? ColTeal : ColTextMuted;
                    Size sz = TextRenderer.MeasureText("Đăng nhập", t1Font);
                    TextRenderer.DrawText(g, "Đăng nhập", t1Font, new Point(t1CenterX - sz.Width / 2, (tabBox.Height - sz.Height) / 2), t1Color);
                }

                int t2CenterX = 3 + halfW + halfW / 2;
                using (Font t2Font = new Font("Segoe UI", 9.5f, activeTab == 1 ? FontStyle.Bold : FontStyle.Regular))
                {
                    Color t2Color = activeTab == 1 ? ColTeal : ColTextMuted;
                    Size sz = TextRenderer.MeasureText("Đăng ký", t2Font);
                    TextRenderer.DrawText(g, "Đăng ký", t2Font, new Point(t2CenterX - sz.Width / 2, (tabBox.Height - sz.Height) / 2), t2Color);
                }
            };

            tabBox.MouseClick += (s, e) =>
            {
                int halfW = tabBox.Width / 2;
                if (e.X < halfW)
                {
                    activeTab = 0;
                    tabBox.Invalidate();
                }
                else
                {
                    new CustomerRegisterForm().ShowDialog(this);
                    activeTab = 0;
                    tabBox.Invalidate();
                }
            };
            tabBox.Cursor = Cursors.Hand;
            return tabBox;
        }

        // =========================================================
        // HELPER: GREETING HEADER PANEL
        // =========================================================
        private Panel CreateHeaderPanel()
        {
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.White
            };
            headerPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (Font fTitle = new Font("Segoe UI", 15.5f, FontStyle.Bold))
                {
                    string title = "Chào mừng trở lại";
                    Size sz = TextRenderer.MeasureText(title, fTitle);
                    TextRenderer.DrawText(g, title, fTitle, new Point(0, 0), ColTextDark);

                    int wx = sz.Width + 4;
                    int wy = 3;
                    using (SolidBrush bGold = new SolidBrush(Color.FromArgb(245, 158, 11)))
                    {
                        g.FillEllipse(bGold, wx, wy + 2, 13, 14);
                        g.FillEllipse(bGold, wx - 2, wy + 5, 5, 6);
                        g.FillEllipse(bGold, wx + 3, wy - 3, 5, 8);
                        g.FillEllipse(bGold, wx + 7, wy - 4, 5, 9);
                        g.FillEllipse(bGold, wx + 11, wy - 2, 5, 8);
                    }
                    using (Pen pWave = new Pen(Color.FromArgb(217, 119, 6), 1.2f))
                    {
                        g.DrawArc(pWave, wx + 15, wy - 3, 6, 8, 280, 100);
                        g.DrawArc(pWave, wx + 18, wy - 5, 8, 11, 280, 100);
                    }
                }

                using (Font fSub = new Font("Segoe UI", 9f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, "Đăng nhập để tiếp tục trải nghiệm SportZone", fSub, new Point(0, 26), ColTextMuted);
                }
            };
            return headerPanel;
        }

        // =========================================================
        // HELPER: CUSTOM INPUT FIELD WITH USER/LOCK & EYE TOGGLE
        // =========================================================
        private Panel CreateInputField(string label, string placeholder, string defaultVal, bool isPassword, out TextBox outTextBox)
        {
            Panel box = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.White
            };

            bool isFocused = false;

            TextBox txt = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ColTextDark,
                BackColor = Color.White,
                Text = defaultVal,
                UseSystemPasswordChar = isPassword,
                Location = new Point(44, 20),
                Width = 268
            };
            outTextBox = txt;
            box.Resize += (s, e) => { txt.Width = Math.Max(120, box.Width - 44 - 44); };

            txt.Enter += (s, e) => { isFocused = true; box.Invalidate(); };
            txt.Leave += (s, e) => { isFocused = false; box.Invalidate(); };
            txt.TextChanged += (s, e) => { box.Invalidate(); };
            txt.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    DoLogin(s, EventArgs.Empty);
                }
            };

            box.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = new Rectangle(0, 0, box.Width - 1, box.Height - 1);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 12))
                {
                    using (Pen pen = new Pen(isFocused ? ColBorderFocus : ColBorder, isFocused ? 1.6f : 1.1f))
                    {
                        g.DrawPath(pen, p);
                    }
                }

                int iconX = 14;
                int iconY = (box.Height - 18) / 2;
                Color iconColor = isFocused ? ColTeal : Color.FromArgb(148, 163, 184);

                if (!isPassword)
                {
                    using (Pen p = new Pen(iconColor, 1.5f))
                    {
                        g.DrawEllipse(p, iconX + 3, iconY + 1, 9, 9);
                        g.DrawArc(p, iconX, iconY + 9, 15, 12, 190, 160);
                    }
                }
                else
                {
                    using (Pen p = new Pen(iconColor, 1.5f))
                    {
                        g.DrawArc(p, iconX + 3, iconY + 1, 9, 10, 180, 180);
                        using (GraphicsPath pLock = GraphicsUtility.CreateRoundedRectangle(new Rectangle(iconX + 1, iconY + 6, 13, 11), 3))
                        {
                            g.DrawPath(p, pLock);
                        }
                        g.DrawLine(p, iconX + 7, iconY + 10, iconX + 7, iconY + 13);
                    }
                }

                using (Font lblFont = new Font("Segoe UI", 7.5f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, label, lblFont, new Point(44, 4), ColTextMuted);
                }

                int rightBtnX = box.Width - 24;
                int cy = box.Height / 2;

                if (!isPassword)
                {
                    if (!string.IsNullOrEmpty(txt.Text))
                    {
                        int cr = 8;
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(203, 213, 225)))
                            g.FillEllipse(b, rightBtnX - cr, cy - cr, cr * 2, cr * 2);
                        using (Pen p = new Pen(Color.White, 1.6f))
                        {
                            p.StartCap = LineCap.Round;
                            p.EndCap = LineCap.Round;
                            g.DrawLine(p, rightBtnX - 3, cy - 3, rightBtnX + 3, cy + 3);
                            g.DrawLine(p, rightBtnX + 3, cy - 3, rightBtnX - 3, cy + 3);
                        }
                    }
                }
                else
                {
                    Color eyeCol = isPasswordHidden ? Color.FromArgb(148, 163, 184) : ColTeal;
                    using (Pen p = new Pen(eyeCol, 1.5f))
                    {
                        p.StartCap = LineCap.Round;
                        p.EndCap = LineCap.Round;
                        g.DrawArc(p, rightBtnX - 10, cy - 6, 20, 12, 20, 140);
                        g.DrawArc(p, rightBtnX - 10, cy - 6, 20, 12, 200, 140);
                        using (SolidBrush pb = new SolidBrush(eyeCol))
                            g.FillEllipse(pb, rightBtnX - 3, cy - 3, 6, 6);
                        if (isPasswordHidden)
                        {
                            g.DrawLine(p, rightBtnX - 8, cy + 6, rightBtnX + 8, cy - 6);
                        }
                    }
                }
            };

            box.MouseClick += (s, e) =>
            {
                int rightBtnX = box.Width - 24;
                Rectangle btnRect = new Rectangle(rightBtnX - 12, 8, 24, 30);
                if (btnRect.Contains(e.Location))
                {
                    if (!isPassword)
                    {
                        txt.Text = "";
                        txt.Focus();
                    }
                    else
                    {
                        isPasswordHidden = !isPasswordHidden;
                        txt.UseSystemPasswordChar = isPasswordHidden;
                        box.Invalidate();
                    }
                }
                else
                {
                    txt.Focus();
                }
            };

            box.Controls.Add(txt);
            return box;
        }

        // =========================================================
        // HELPER: REMEMBER ME & FORGOT PASSWORD ROW
        // =========================================================
        private Panel CreateOptionsRow()
        {
            Panel row = new Panel
            {
                Dock = DockStyle.Top,
                Height = 24,
                BackColor = Color.White
            };

            row.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int boxSize = 16;
                int boxY = (row.Height - boxSize) / 2;
                Rectangle chkRect = new Rectangle(0, boxY, boxSize, boxSize);

                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(chkRect, 4))
                {
                    if (isRememberChecked)
                    {
                        using (SolidBrush b = new SolidBrush(ColTeal))
                            g.FillPath(b, p);
                        using (Pen pen = new Pen(Color.White, 1.8f))
                        {
                            pen.StartCap = LineCap.Round;
                            pen.EndCap = LineCap.Round;
                            g.DrawLine(pen, 3, boxY + 8, 6, boxY + 11);
                            g.DrawLine(pen, 6, boxY + 11, 12, boxY + 4);
                        }
                    }
                    else
                    {
                        using (SolidBrush b = new SolidBrush(Color.White))
                            g.FillPath(b, p);
                        using (Pen pen = new Pen(ColBorder, 1.2f))
                            g.DrawPath(pen, p);
                    }
                }

                using (Font f = new Font("Segoe UI", 9f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, "Ghi nhớ đăng nhập", f, new Point(22, 2), ColTextDark);

                    string forgotText = "Quên mật khẩu?";
                    Size sz = TextRenderer.MeasureText(forgotText, f);
                    TextRenderer.DrawText(g, forgotText, f, new Point(row.Width - sz.Width, 2), ColTeal);
                }
            };

            row.MouseClick += (s, e) =>
            {
                if (e.X < 140)
                {
                    isRememberChecked = !isRememberChecked;
                    row.Invalidate();
                }
                else if (e.X > row.Width - 110)
                {
                    new ForgotPasswordForm().ShowDialog(this);
                }
            };
            row.Cursor = Cursors.Hand;
            return row;
        }

        // =========================================================
        // HELPER: LOGIN BUTTON
        // =========================================================
        private Panel CreateLoginButton()
        {
            Panel btn = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.White
            };

            bool isHovered = false;
            bool isPressed = false;

            btn.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 12))
                {
                    Color bgCol = isPressed ? ColTealPressed : (isHovered ? ColTealHover : ColTeal);
                    using (SolidBrush b = new SolidBrush(bgCol))
                    {
                        g.FillPath(b, p);
                    }
                }

                using (Font f = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                {
                    string text = "ĐĂNG NHẬP";
                    Size sz = TextRenderer.MeasureText(text, f);
                    int tx = (btn.Width - sz.Width) / 2;
                    int ty = (btn.Height - sz.Height) / 2;
                    TextRenderer.DrawText(g, text, f, new Point(tx, ty), Color.White);
                }
            };

            btn.MouseEnter += (s, e) => { isHovered = true; btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { isHovered = false; isPressed = false; btn.Invalidate(); };
            btn.MouseDown += (s, e) => { isPressed = true; btn.Invalidate(); };
            btn.MouseUp += (s, e) => { isPressed = false; btn.Invalidate(); };
            btn.Click += DoLogin;
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        // =========================================================
        // HELPER: REGISTER PROMPT
        // =========================================================
        private Panel CreateRegisterPrompt()
        {
            Panel row = new Panel
            {
                Dock = DockStyle.Top,
                Height = 22,
                BackColor = Color.White
            };

            row.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                string part1 = "Chưa có tài khoản? ";
                string part2 = "Đăng ký ngay";

                using (Font f1 = new Font("Segoe UI", 9f, FontStyle.Regular))
                using (Font f2 = new Font("Segoe UI", 9f, FontStyle.Bold))
                {
                    Size sz1 = TextRenderer.MeasureText(part1, f1);
                    Size sz2 = TextRenderer.MeasureText(part2, f2);
                    int totalW = sz1.Width + sz2.Width;
                    int startX = (row.Width - totalW) / 2;
                    int startY = (row.Height - sz1.Height) / 2;

                    TextRenderer.DrawText(g, part1, f1, new Point(startX, startY), ColTextMuted);
                    TextRenderer.DrawText(g, part2, f2, new Point(startX + sz1.Width, startY), ColTeal);
                }
            };

            row.MouseClick += (s, e) => new CustomerRegisterForm().ShowDialog(this);
            row.Cursor = Cursors.Hand;
            return row;
        }

        // =========================================================
        // HELPER: SECURITY BANNER (Đăng nhập an toàn)
        // =========================================================
        private Panel CreateSecurityBanner()
        {
            Panel banner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.FromArgb(240, 253, 250) // #F0FDFA
            };

            banner.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = new Rectangle(0, 0, banner.Width - 1, banner.Height - 1);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 12))
                {
                    banner.Region = new Region(p);

                    using (Pen pen = new Pen(Color.FromArgb(153, 246, 228), 1.2f))
                        g.DrawPath(pen, p);
                }

                int circleSize = 28;
                int circleX = 12;
                int circleY = (banner.Height - circleSize) / 2;
                using (SolidBrush b = new SolidBrush(ColTeal))
                    g.FillEllipse(b, circleX, circleY, circleSize, circleSize);

                using (Pen p = new Pen(Color.White, 1.6f))
                {
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;
                    int sx = circleX + 7;
                    int sy = circleY + 7;
                    g.DrawLine(p, sx, sy, sx + 14, sy);
                    g.DrawLine(p, sx + 14, sy, sx + 14, sy + 6);
                    g.DrawLine(p, sx + 14, sy + 6, sx + 7, sy + 13);
                    g.DrawLine(p, sx + 7, sy + 13, sx, sy + 6);
                    g.DrawLine(p, sx + 6, sy + 6, sx, sy);
                    g.DrawLine(p, sx + 4, sy + 6, sx + 6, sy + 9);
                    g.DrawLine(p, sx + 6, sy + 9, sx + 10, sy + 4);
                }

                using (Font titleFont = new Font("Segoe UI", 8.75f, FontStyle.Bold))
                    TextRenderer.DrawText(g, "Đăng nhập an toàn", titleFont, new Point(48, 5), Color.FromArgb(15, 118, 110));

                using (Font subFont = new Font("Segoe UI", 7.25f, FontStyle.Regular))
                {
                    Rectangle descRect = new Rectangle(48, 20, banner.Width - 54, 24);
                    TextRenderer.DrawText(g, "Tài khoản được bảo vệ bằng mã hóa, đảm bảo an toàn thông tin cá nhân và dữ liệu đặt sân.", subFont, descRect, Color.FromArgb(17, 94, 89), TextFormatFlags.WordBreak);
                }
            };

            return banner;
        }

        // =========================================================
        // AUTHENTICATION LOGIC (PBKDF2-HMAC-SHA256, ZERO FALLBACK)
        // =========================================================
        private readonly Services.AuthService _authService = new Services.AuthService();
        private bool _isLoggingIn = false;

        private void DoLogin(object sender, EventArgs e)
        {
            if (_isLoggingIn) return;
            _isLoggingIn = true;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                string username = innerTxtUser?.Text?.Trim() ?? "";
                string password = innerTxtPassword?.Text ?? "";

                if (string.IsNullOrWhiteSpace(username))
                {
                    SFToast.Show(this, "Vui lòng nhập tên đăng nhập.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    SFToast.Show(this, "Vui lòng nhập mật khẩu.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                    return;
                }

                string errorMsg;
                bool success = _authService.Login(username, password, out errorMsg);

                if (!success)
                {
                    SFToast.Show(this, errorMsg, "ĐĂNG NHẬP THẤT BẠI", SFIcon.AlertCircle, Colors.Danger);
                    return;
                }

                if (Session.AppSession.Current.BatBuocDoiMatKhau)
                {
                    using (var pwdForm = new Dialogs.ChangePasswordForm(adminReset: false, targetTaiKhoanId: Session.AppSession.Current.TaiKhoanID, isForced: true))
                    {
                        var pwdRes = pwdForm.ShowDialog(this);
                        if (pwdRes != DialogResult.OK || Session.AppSession.Current.BatBuocDoiMatKhau)
                        {
                            _authService.Logout();
                            SFToast.Show(this, "Bạn bắt buộc phải đổi mật khẩu trong lần đăng nhập đầu tiên để tiếp tục sử dụng hệ thống.", "BẮT BUỘC ĐỔI MẬT KHẨU", SFIcon.AlertCircle, Colors.Danger);
                            return;
                        }
                    }
                }

                try
                {
                    using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\SportField\Login"))
                    {
                        if (key != null)
                        {
                            if (isRememberChecked)
                            {
                                key.SetValue("Username", username);
                                key.SetValue("Remember", 1);
                            }
                            else
                            {
                                key.DeleteValue("Username", false);
                                key.SetValue("Remember", 0);
                            }
                        }
                    }
                }
                catch { }

                SelectedRole = Session.AppSession.Current.TenVaiTro;
                DisplayName = Session.AppSession.Current.HoTen;

                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                _isLoggingIn = false;
                Cursor.Current = Cursors.Default;
            }
        }
    }
}
