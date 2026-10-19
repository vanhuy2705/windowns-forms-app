using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Dialogs;

namespace SportZone.WinForms
{
    public class LoginForm : Form
    {
        // Public properties expected by Program.cs
        public string SelectedRole { get; private set; }
        public string DisplayName { get; private set; }

        // Window dragging native interop
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        // Interactive controls
        private Panel cardPanel;
        private TextBox innerTxtUser;
        private TextBox innerTxtPassword;
        private bool isPasswordHidden = true;
        private bool isRememberChecked = true;
        private int activeTab = 0; // 0 = Đăng nhập, 1 = Đăng ký

        // Visual constants matching the mockup
        private readonly Color ColBgDark = Color.FromArgb(7, 24, 38);
        private readonly Color ColEmerald = Color.FromArgb(16, 185, 129);
        private readonly Color ColEmeraldDark = Color.FromArgb(5, 150, 105);
        private readonly Color ColTextDark = Color.FromArgb(15, 23, 42);
        private readonly Color ColTextMuted = Color.FromArgb(100, 116, 139);
        private readonly Color ColBorder = Color.FromArgb(226, 232, 240);
        private readonly Color ColBorderFocus = Color.FromArgb(16, 185, 129);

        public LoginForm()
        {
            DpiHelper.ApplyDpiMode(this);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Text = "SPORTFIELD — Hệ thống quản lý đặt sân thể thao";
            try { this.Icon = Helpers.SportFieldIconProvider.GetWindowIcon(Helpers.SportFieldIcon.App); } catch { }
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1024, 576);
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

            // Deselect any highlighted text on load
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
            if (cardPanel != null)
            {
                RepositionCard();
            }
            Invalidate();
        }

        private void RepositionCard()
        {
            int cardW = 412;
            int cardH = Math.Min(438, ClientSize.Height - 24);
            int cardX = ClientSize.Width - cardW - 24;
            int cardY = (ClientSize.Height - cardH) / 2;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // 1. Draw Clean Background (Hero on left, clean dark gradient + aurora glow on right)
            Image bg = AssetManager.GetLoginCleanBackground();
            if (bg != null)
            {
                g.DrawImage(bg, ClientRectangle);
            }
            else
            {
                // Fallback rich gradient
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    ClientRectangle,
                    Color.FromArgb(12, 35, 54),
                    Color.FromArgb(4, 78, 62),
                    35f))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
            }
        }

        private void BuildUI()
        {
            // Floating White Card Panel
            cardPanel = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(22, 10, 22, 14)
            };
            cardPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Card border radius
                Rectangle r = cardPanel.ClientRectangle;
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(new Rectangle(0, 0, r.Width - 1, r.Height - 1), 22))
                {
                    cardPanel.Region = new Region(p);

                    // Soft subtle border
                    using (Pen pen = new Pen(Color.FromArgb(235, 238, 242), 1.2f))
                    {
                        g.DrawPath(pen, p);
                    }
                }
            };
            Controls.Add(cardPanel);
            RepositionCard();

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

            // ==========================================
            // ROW 0: WINDOW CONTROL BUTTONS (Inside Card Header)
            // ==========================================
            Panel topBar = CreateCardTopBar();
            topBar.Margin = new Padding(0, 0, 0, 4);
            stack.Controls.Add(topBar, 0, 0);

            // ==========================================
            // ROW 1: TAB SWITCHER (Segmented Control)
            // ==========================================
            Panel tabContainer = CreateTabSwitcher();
            tabContainer.Margin = new Padding(0, 0, 0, 8);
            stack.Controls.Add(tabContainer, 0, 1);

            // ==========================================
            // ROW 2: HEADER (Chào mừng trở lại 👋)
            // ==========================================
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = Color.White
            };
            headerPanel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Title
                using (Font fTitle = new Font("Segoe UI", 15.5f, FontStyle.Bold))
                {
                    string title = "Chào mừng trở lại";
                    Size sz = TextRenderer.MeasureText(title, fTitle);
                    TextRenderer.DrawText(g, title, fTitle, new Point(0, 0), ColTextDark);

                    // Waving hand drawing
                    int wx = sz.Width + 4;
                    int wy = 3;
                    using (SolidBrush bGold = new SolidBrush(Color.FromArgb(245, 158, 11)))
                    {
                        // Palm & fingers
                        g.FillEllipse(bGold, wx, wy + 2, 13, 14);
                        g.FillEllipse(bGold, wx - 2, wy + 5, 5, 6); // Thumb
                        g.FillEllipse(bGold, wx + 3, wy - 3, 5, 8); // Finger
                        g.FillEllipse(bGold, wx + 7, wy - 4, 5, 9); // Middle
                        g.FillEllipse(bGold, wx + 11, wy - 2, 5, 8); // Ring
                    }
                    // Motion waves
                    using (Pen pWave = new Pen(Color.FromArgb(217, 119, 6), 1.2f))
                    {
                        g.DrawArc(pWave, wx + 15, wy - 3, 6, 8, 280, 100);
                        g.DrawArc(pWave, wx + 18, wy - 5, 8, 11, 280, 100);
                    }
                }

                // Subtitle
                using (Font fSub = new Font("Segoe UI", 9f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, "Đăng nhập để tiếp tục trải nghiệm SportField", fSub, new Point(0, 26), ColTextMuted);
                }
            };
            stack.Controls.Add(headerPanel, 0, 2);

            // ==========================================
            // ROW 3: USERNAME INPUT
            // ==========================================
            Panel userInput = CreateInputField(
                label: "Tên đăng nhập",
                placeholder: "Nhập tên đăng nhập",
                defaultVal: "",
                isPassword: false,
                outTextBox: out innerTxtUser
            );
            userInput.Margin = new Padding(0, 0, 0, 8);
            stack.Controls.Add(userInput, 0, 3);

            // ==========================================
            // ROW 4: PASSWORD INPUT
            // ==========================================
            Panel pwdInput = CreateInputField(
                label: "Mật khẩu",
                placeholder: "Nhập mật khẩu",
                defaultVal: "",
                isPassword: true,
                outTextBox: out innerTxtPassword
            );
            pwdInput.Margin = new Padding(0, 0, 0, 8);
            stack.Controls.Add(pwdInput, 0, 4);

            // ==========================================
            // ROW 5: REMEMBER & FORGOT PASSWORD ROW
            // ==========================================
            Panel optionsRow = CreateOptionsRow();
            optionsRow.Margin = new Padding(0, 0, 0, 10);
            stack.Controls.Add(optionsRow, 0, 5);

            // ==========================================
            // ROW 6: PRIMARY BUTTON (➔ ĐĂNG NHẬP)
            // ==========================================
            Panel btnLogin = CreateLoginButton();
            btnLogin.Margin = new Padding(0, 0, 0, 14);
            stack.Controls.Add(btnLogin, 0, 6);

            // ==========================================
            // ROW 7: REGISTER PROMPT (Chưa có tài khoản? Đăng ký ngay)
            // ==========================================
            Panel regPrompt = CreateRegisterPrompt();
            regPrompt.Margin = new Padding(0, 0, 0, 14);
            stack.Controls.Add(regPrompt, 0, 7);

            // ==========================================
            // ROW 8: SECURITY BANNER (Đăng nhập an toàn)
            // ==========================================
            Panel secBanner = CreateSecurityBanner();
            secBanner.Margin = new Padding(0);
            stack.Controls.Add(secBanner, 0, 8);
        }

        // =========================================================
        // HELPER 0: CARD TOP BAR (Window Controls: Minimize, Maximize, Close)
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
            int rightEdge = bar.Width;

            // Close button (✕)
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

            // Minimize button (—)
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

            // Layout buttons on resize
            bar.Resize += (s, e) =>
            {
                btnClose.Location = new Point(bar.Width - btnSize, 1);
                btnMin.Location = new Point(btnClose.Left - btnSize - 4, 1);
            };

            bar.Controls.Add(btnClose);
            bar.Controls.Add(btnMin);

            // Allow dragging window from card topbar
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
        // HELPER 1: TAB SWITCHER (Segmented Control)
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
                    // Tab 1 Active Pill
                    Rectangle pill = new Rectangle(3, 3, halfW, tabBox.Height - 6);
                    using (GraphicsPath pp = GraphicsUtility.CreateRoundedRectangle(pill, 10))
                    {
                        using (SolidBrush b = new SolidBrush(Color.White))
                            g.FillPath(b, pp);
                        using (Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1f))
                            g.DrawPath(pen, pp);
                    }

                    // Green Active Accent Bar at bottom
                    using (SolidBrush b = new SolidBrush(ColEmerald))
                    {
                        Rectangle acc = new Rectangle(pill.Left + 22, pill.Bottom - 3, pill.Width - 44, 3);
                        using (GraphicsPath ap = GraphicsUtility.CreateRoundedRectangle(acc, 2))
                            g.FillPath(b, ap);
                    }
                }
                else
                {
                    // Tab 2 Active Pill
                    Rectangle pill = new Rectangle(3 + halfW, 3, halfW, tabBox.Height - 6);
                    using (GraphicsPath pp = GraphicsUtility.CreateRoundedRectangle(pill, 10))
                    {
                        using (SolidBrush b = new SolidBrush(Color.White))
                            g.FillPath(b, pp);
                        using (Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1f))
                            g.DrawPath(pen, pp);
                    }
                    using (SolidBrush b = new SolidBrush(ColEmerald))
                    {
                        Rectangle acc = new Rectangle(pill.Left + 22, pill.Bottom - 3, pill.Width - 44, 3);
                        using (GraphicsPath ap = GraphicsUtility.CreateRoundedRectangle(acc, 2))
                            g.FillPath(b, ap);
                    }
                }

                // Draw Tab 1 Text & Icon
                int t1CenterX = 3 + halfW / 2;
                int iconY = (tabBox.Height - 16) / 2;
                Color t1Color = activeTab == 0 ? ColTextDark : ColTextMuted;
                Font t1Font = new Font("Segoe UI", 9.5f, activeTab == 0 ? FontStyle.Bold : FontStyle.Regular);

                // User icon
                using (SolidBrush b = new SolidBrush(activeTab == 0 ? ColEmerald : ColTextMuted))
                {
                    g.FillEllipse(b, t1CenterX - 44, iconY + 1, 8, 8);
                    g.FillPie(b, t1CenterX - 47, iconY + 8, 14, 12, 180, 180);
                }
                TextRenderer.DrawText(g, "Đăng nhập", t1Font, new Point(t1CenterX - 24, iconY - 1), t1Color);

                // Draw Tab 2 Text & Icon
                int t2CenterX = 3 + halfW + halfW / 2;
                Color t2Color = activeTab == 1 ? ColTextDark : ColTextMuted;
                Font t2Font = new Font("Segoe UI", 9.5f, activeTab == 1 ? FontStyle.Bold : FontStyle.Regular);

                using (SolidBrush b = new SolidBrush(activeTab == 1 ? ColEmerald : ColTextMuted))
                {
                    g.FillEllipse(b, t2CenterX - 36, iconY + 1, 7, 7);
                    g.FillPie(b, t2CenterX - 39, iconY + 7, 13, 11, 180, 180);
                    // plus sign
                    using (Pen p = new Pen(b.Color, 1.5f))
                    {
                        g.DrawLine(p, t2CenterX - 24, iconY + 5, t2CenterX - 20, iconY + 5);
                        g.DrawLine(p, t2CenterX - 22, iconY + 3, t2CenterX - 22, iconY + 7);
                    }
                }
                TextRenderer.DrawText(g, "Đăng ký", t2Font, new Point(t2CenterX - 14, iconY - 1), t2Color);
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
        // HELPER 2: CUSTOM OUTLINED INPUT FIELD
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

                // Left Icon (User or Lock)
                int iconX = 14;
                int iconY = (box.Height - 18) / 2;
                Color iconColor = isFocused ? ColEmerald : Color.FromArgb(148, 163, 184);

                if (!isPassword)
                {
                    // User icon
                    using (Pen p = new Pen(iconColor, 1.5f))
                    {
                        g.DrawEllipse(p, iconX + 3, iconY + 1, 9, 9);
                        g.DrawArc(p, iconX, iconY + 9, 15, 12, 190, 160);
                    }
                }
                else
                {
                    // Lock icon
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

                // Top label text
                using (Font lblFont = new Font("Segoe UI", 7.5f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, label, lblFont, new Point(44, 4), ColTextMuted);
                }

                // Right action button: Clear (✕) or Eye (👁)
                int rightBtnX = box.Width - 24;
                int cy = box.Height / 2;

                if (!isPassword)
                {
                    // Clear circle ✕
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
                    // Eye icon
                    Color eyeCol = isPasswordHidden ? Color.FromArgb(148, 163, 184) : ColEmerald;
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
        // HELPER 3: OPTIONS ROW (Remember Me & Forgot Password)
        // =========================================================
        private Panel CreateOptionsRow()
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

                // Checkbox Square (Green checked)
                Rectangle chkRect = new Rectangle(2, 3, 16, 16);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(chkRect, 4))
                {
                    if (isRememberChecked)
                    {
                        using (SolidBrush b = new SolidBrush(ColEmerald))
                            g.FillPath(b, p);
                        using (Pen pen = new Pen(Color.White, 1.8f))
                        {
                            pen.StartCap = LineCap.Round;
                            pen.EndCap = LineCap.Round;
                            g.DrawLine(pen, 5, 11, 8, 14);
                            g.DrawLine(pen, 8, 14, 14, 7);
                        }
                    }
                    else
                    {
                        using (Pen pen = new Pen(ColBorder, 1.5f))
                            g.DrawPath(pen, p);
                    }
                }

                // Checkbox Label
                using (Font font = new Font("Segoe UI", 9f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, "Ghi nhớ đăng nhập", font, new Point(24, 2), Color.FromArgb(51, 65, 85));
                }

                // Forgot Password Link
                using (Font linkFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                {
                    Size sz = TextRenderer.MeasureText("Quên mật khẩu?", linkFont);
                    int linkX = row.Width - sz.Width - 2;
                    TextRenderer.DrawText(g, "Quên mật khẩu?", linkFont, new Point(linkX, 2), ColEmerald);
                }
            };

            row.MouseClick += (s, e) =>
            {
                if (e.X < 160)
                {
                    isRememberChecked = !isRememberChecked;
                    row.Invalidate();
                }
                else if (e.X > row.Width - 120)
                {
                    using (var forgotForm = new Dialogs.ForgotPasswordForm())
                    {
                        var res = forgotForm.ShowDialog(this);
                        if (res == DialogResult.OK && !string.IsNullOrEmpty(forgotForm.RecoveredUsername))
                        {
                            if (innerTxtUser != null)
                            {
                                innerTxtUser.Text = forgotForm.RecoveredUsername;
                                innerTxtUser.SelectionStart = innerTxtUser.Text.Length;
                            }
                            if (innerTxtPassword != null)
                            {
                                innerTxtPassword.Text = "";
                                innerTxtPassword.Focus();
                            }
                        }
                    }
                }
            };
            row.Cursor = Cursors.Hand;
            return row;
        }

        // =========================================================
        // HELPER 4: PRIMARY BUTTON (➔ ĐĂNG NHẬP)
        // =========================================================
        private Panel CreateLoginButton()
        {
            Panel btn = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = ColEmerald,
                Cursor = Cursors.Hand
            };

            bool isHovered = false;

            btn.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 12))
                {
                    btn.Region = new Region(p);

                    // Gradient Fill
                    Color c1 = isHovered ? Color.FromArgb(5, 150, 105) : ColEmerald;
                    Color c2 = isHovered ? Color.FromArgb(16, 185, 129) : Color.FromArgb(13, 148, 100);
                    using (LinearGradientBrush b = new LinearGradientBrush(r, c1, c2, LinearGradientMode.Horizontal))
                    {
                        g.FillPath(b, p);
                    }
                }

                // Text: ➔  ĐĂNG NHẬP
                using (Font font = new Font("Segoe UI", 10.25f, FontStyle.Bold))
                {
                    string text = "→  ĐĂNG NHẬP";
                    Size sz = TextRenderer.MeasureText(text, font);
                    int tx = (btn.Width - sz.Width) / 2;
                    int ty = (btn.Height - sz.Height) / 2;
                    TextRenderer.DrawText(g, text, font, new Point(tx, ty), Color.White);
                }
            };

            btn.MouseEnter += (s, e) => { isHovered = true; btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { isHovered = false; btn.Invalidate(); };
            btn.Click += DoLogin;

            return btn;
        }

        // =========================================================
        // HELPER 5: REGISTER PROMPT (Chưa có tài khoản? Đăng ký ngay)
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
                    TextRenderer.DrawText(g, part2, f2, new Point(startX + sz1.Width, startY), ColEmerald);
                }
            };

            row.MouseClick += (s, e) => new CustomerRegisterForm().ShowDialog(this);
            row.Cursor = Cursors.Hand;
            return row;
        }

        // =========================================================
        // HELPER 6: SECURITY BANNER (Đăng nhập an toàn)
        // =========================================================
        private Panel CreateSecurityBanner()
        {
            Panel banner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.FromArgb(236, 253, 245)
            };

            banner.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = new Rectangle(0, 0, banner.Width - 1, banner.Height - 1);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 12))
                {
                    banner.Region = new Region(p);

                    using (Pen pen = new Pen(Color.FromArgb(167, 243, 208), 1.2f))
                        g.DrawPath(pen, p);
                }

                // Shield badge circle
                int circleSize = 28;
                int circleX = 12;
                int circleY = (banner.Height - circleSize) / 2;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(52, 211, 153)))
                    g.FillEllipse(b, circleX, circleY, circleSize, circleSize);

                // White shield & checkmark
                using (Pen p = new Pen(Color.White, 1.6f))
                {
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;
                    int sx = circleX + 7;
                    int sy = circleY + 7;
                    // Shield
                    g.DrawLine(p, sx, sy, sx + 14, sy);
                    g.DrawLine(p, sx + 14, sy, sx + 14, sy + 6);
                    g.DrawLine(p, sx + 14, sy + 6, sx + 7, sy + 13);
                    g.DrawLine(p, sx + 7, sy + 13, sx, sy + 6);
                    g.DrawLine(p, sx + 6, sy + 6, sx, sy);
                    // Check
                    g.DrawLine(p, sx + 4, sy + 6, sx + 6, sy + 9);
                    g.DrawLine(p, sx + 6, sy + 9, sx + 10, sy + 4);
                }

                // Title
                using (Font titleFont = new Font("Segoe UI", 8.75f, FontStyle.Bold))
                    TextRenderer.DrawText(g, "Đăng nhập an toàn", titleFont, new Point(48, 5), Color.FromArgb(6, 95, 70));

                // Description
                using (Font subFont = new Font("Segoe UI", 7.25f, FontStyle.Regular))
                {
                    Rectangle descRect = new Rectangle(48, 20, banner.Width - 54, 24);
                    TextRenderer.DrawText(g, "Tài khoản được bảo vệ bằng mã hóa, đảm bảo an toàn thông tin cá nhân và dữ liệu đặt sân.", subFont, descRect, Color.FromArgb(4, 120, 87), TextFormatFlags.WordBreak);
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

                // Đồng bộ thuộc tính công khai cho Program.cs và MainForm
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
