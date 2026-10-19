using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Views;
using SportZone.WinForms.Services;
using SportZone.WinForms.Models;
using SportZone.WinForms.Dialogs;

namespace SportZone.WinForms
{
    public class MainForm : Form
    {
        private TableLayoutPanel rootLayout;
        private Panel titlebar;
        private TableLayoutPanel workspaceLayout;
        private Panel sidebarHost;
        private TableLayoutPanel sidebarLayout;
        private Panel brandPanel;
        private Panel logoMark;
        private Label brandName, brandSub;
        private DoubleBufferedNavPanel navScrollPanel;
        private Panel userFooter;
        private Label lblUserName, lblUserRole;
        private SFIconButton btnLogout;
        private Panel rightHost;
        private TableLayoutPanel rightLayout;
        private Panel topbar;
        private TableLayoutPanel topbarLayout;
        private Panel contentHost;
        private Label lblTitle, lblCrumb, lblClock, dbStatus;
        private SFBellButton btnBell;
        private NotificationPopup notificationPopup;
        private Timer notificationTimer;
        private SFSearchBox topSearch;
        private SFAvatar userAvatar;
        private ToolTip navToolTip;
        private readonly List<Label> navGroupLabels = new List<Label>();
        private readonly Dictionary<string, NavItemButton> navButtons = new Dictionary<string, NavItemButton>();
        private readonly string role;
        private readonly string user;

        private bool isSidebarCollapsed = false;
        private int targetSidebarWidth = Metrics.SidebarExpanded;
        private Timer sidebarTimer;
        private Timer clockTimer;

        // Win32 API for Borderless Resizing & Moving
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        public MainForm(string role = null, string user = null)
        {
            this.role = Session.AppSession.Current.TenVaiTro ?? SanitizeUtf8(role) ?? "Quản trị viên";
            this.user = Session.AppSession.Current.HoTen ?? SanitizeUtf8(user) ?? "Quản trị viên";

            DpiHelper.ApplyDpiMode(this);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Text = "SPORTFIELD — Hệ Thống Quản Lý Thể Thao Cao Cấp";
            try { this.Icon = Helpers.SportFieldIconProvider.GetWindowIcon(Helpers.SportFieldIcon.App); } catch { }
            ClientSize = LayoutMetrics.DefaultAppSize;
            MinimumSize = LayoutMetrics.MinimumAppSize;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Colors.AppBackground;
            Font = Typography.Body;
            FormBorderStyle = FormBorderStyle.None;

            navToolTip = new ToolTip { ShowAlways = true };

            BuildShell();
            ShowView(GetHomeKey());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DpiHelper.ClampToScreen(this, 16, 24);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            CheckDatabaseConnectionAsync();
            InitializeNotificationCenter();
        }

        private async void CheckDatabaseConnectionAsync()
        {
            try
            {
                var res = await SportZone.WinForms.Database.DatabaseManager.TestConnectionAsync();
                if (IsDisposed || Disposing) return;

                if (res.Success)
                {
                    dbStatus.Text = IsCustomer ? "● SQL ONLINE" : "● SQL SERVER KẾT NỐI";
                    dbStatus.ForeColor = Colors.DeepEmerald;
                    dbStatus.BackColor = Colors.SuccessBg;
                    navToolTip.SetToolTip(dbStatus, string.Format("● SQL Server Trực Tuyến\n• Máy chủ: {0}\n• CSDL: {1} ({2} bảng)\n• Phản hồi: {3} ms\n• Tệp dữ liệu: {4}\n(Nhấn để làm mới)", res.ServerName, res.DatabaseName, res.TableCount, res.LatencyMs, res.DataFilePath));
                }
                else
                {
                    dbStatus.Text = IsCustomer ? "○ SQL OFFLINE" : "○ SQL MẤT KẾT NỐI";
                    dbStatus.ForeColor = Colors.Danger;
                    dbStatus.BackColor = Colors.DangerBg;
                    navToolTip.SetToolTip(dbStatus, "Không thể kết nối SQL Server:\n" + res.ErrorMessage + "\n(Nhấn để thử lại)");
                }
            }
            catch
            {
                // Fallback
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                if (WindowState == FormWindowState.Normal)
                {
                    Point pt = PointToClient(new Point(m.LParam.ToInt32()));
                    int resizeGrip = 8;

                    if (pt.X <= resizeGrip && pt.Y <= resizeGrip) { m.Result = (IntPtr)HTTOPLEFT; return; }
                    if (pt.X >= ClientSize.Width - resizeGrip && pt.Y <= resizeGrip) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                    if (pt.X <= resizeGrip && pt.Y >= ClientSize.Height - resizeGrip) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                    if (pt.X >= ClientSize.Width - resizeGrip && pt.Y >= ClientSize.Height - resizeGrip) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                    if (pt.X <= resizeGrip) { m.Result = (IntPtr)HTLEFT; return; }
                    if (pt.X >= ClientSize.Width - resizeGrip) { m.Result = (IntPtr)HTRIGHT; return; }
                    if (pt.Y <= resizeGrip) { m.Result = (IntPtr)HTTOP; return; }
                    if (pt.Y >= ClientSize.Height - resizeGrip) { m.Result = (IntPtr)HTBOTTOM; return; }
                }
                return;
            }

            base.WndProc(ref m);
        }

        private bool IsAdmin => Session.AppSession.Current.IsAdmin;
        private bool IsStaff => Session.AppSession.Current.IsStaff;
        private bool IsCustomer => Session.AppSession.Current.IsCustomer;

        private string GetHomeKey()
        {
            if (IsStaff) return "Trang chủ";
            if (IsCustomer) return "Trang chủ";
            return "Tổng quan";
        }

        private void BuildShell()
        {
            // 0. Root TableLayoutPanel
            rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Colors.AppBackground,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));  // Titlebar
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // Workspace
            Controls.Add(rootLayout);

            // 1. Custom Titlebar
            titlebar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.SidebarBackground,
                Margin = new Padding(0)
            };
            titlebar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && e.Clicks == 1)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
            titlebar.DoubleClick += (s, e) =>
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            };

            // Titlebar app logo & name
            Label appBadge = new Label
            {
                Text = "  SPORTFIELD AURORA  ",
                Font = Typography.MicroBold,
                ForeColor = Colors.Mint,
                BackColor = Color.FromArgb(32, Colors.Emerald),
                AutoSize = true,
                Location = new Point(12, 8)
            };
            titlebar.Controls.Add(appBadge);

            Label appTitle = new Label
            {
                Text = "Hệ thống quản lý & đặt sân thể thao chuyên nghiệp  •  .NET 4.8",
                Font = Typography.Micro,
                ForeColor = Colors.SidebarText,
                AutoSize = true,
                Location = new Point(appBadge.Right + 10, 10)
            };
            titlebar.Controls.Add(appTitle);

            // Titlebar window buttons: Min, Max, Close
            Button btnMin = CreateWindowButton("—", false, () => { WindowState = FormWindowState.Minimized; });
            Button btnMax = CreateWindowButton("□", false, () =>
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            });
            Button btnClose = CreateWindowButton("✕", false, () => Close());
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Colors.Danger;
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Colors.SidebarBackground;

            titlebar.Controls.Add(btnClose);
            titlebar.Controls.Add(btnMax);
            titlebar.Controls.Add(btnMin);

            rootLayout.Controls.Add(titlebar, 0, 0);

            // 2. Main Workspace Layout
            workspaceLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Colors.AppBackground,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            workspaceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Metrics.SidebarExpanded));
            workspaceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            workspaceLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            rootLayout.Controls.Add(workspaceLayout, 0, 1);

            // 3. Sidebar Host
            sidebarHost = new Panel
            {
                Name = "sidebar",
                Dock = DockStyle.Fill,
                BackColor = Colors.SidebarBackground,
                Margin = new Padding(0)
            };
            sidebarHost.Paint += (s, e) =>
            {
                using (LinearGradientBrush sb = new LinearGradientBrush(
                    sidebarHost.ClientRectangle,
                    Colors.SidebarBackground,
                    Colors.SidebarSecondary,
                    90f))
                {
                    e.Graphics.FillRectangle(sb, sidebarHost.ClientRectangle);
                }
            };
            workspaceLayout.Controls.Add(sidebarHost, 0, 0);

            // Inside Sidebar: 3-row TableLayoutPanel
            sidebarLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Colors.SidebarBackground,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            sidebarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84f));  // Brand
            sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // Navigation
            sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88f));  // User footer
            sidebarHost.Controls.Add(sidebarLayout);

            // Brand Header in Sidebar
            brandPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.SidebarBackground,
                Margin = new Padding(0)
            };
            brandPanel.Paint += (s, e) =>
            {
                e.Graphics.Clear(Colors.SidebarBackground);
            };
            sidebarLayout.Controls.Add(brandPanel, 0, 0);

            logoMark = new Panel
            {
                Location = new Point(14, 18),
                Size = new Size(42, 42),
                BackColor = Colors.Emerald
            };
            logoMark.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, logoMark.Width - 1, logoMark.Height - 1);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 10))
                {
                    using (SolidBrush b = new SolidBrush(Colors.Emerald)) e.Graphics.FillPath(b, p);
                }
                Rectangle iconR = new Rectangle(11, 11, 20, 20);
                VectorIcons.Draw(e.Graphics, SFIcon.Field, iconR, Color.White, 2.2f);
            };
            brandPanel.Controls.Add(logoMark);

            brandName = new Label
            {
                Text = "SPORTFIELD",
                Font = Typography.SubTitle,
                ForeColor = Color.White,
                BackColor = Colors.SidebarBackground,
                Location = new Point(66, 18),
                AutoSize = true
            };
            brandPanel.Controls.Add(brandName);

            brandSub = new Label
            {
                Text = "AURORA PERFORMANCE",
                Font = Typography.MicroBold,
                ForeColor = Colors.Mint,
                BackColor = Colors.SidebarBackground,
                Location = new Point(66, 42),
                AutoSize = true
            };
            brandPanel.Controls.Add(brandSub);

            // Scrollable Navigation Items Panel
            navScrollPanel = new DoubleBufferedNavPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 4, 8, 8),
                BackColor = Colors.SidebarBackground,
                Margin = new Padding(0)
            };
            sidebarLayout.Controls.Add(navScrollPanel, 0, 1);

            // User Info Footer in Sidebar
            userFooter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.SidebarBackground,
                Margin = new Padding(0)
            };
            userFooter.Paint += (s, e) =>
            {
                e.Graphics.Clear(Colors.SidebarBackground);
                using (Pen p = new Pen(Color.FromArgb(50, Colors.Border), 1f))
                {
                    e.Graphics.DrawLine(p, 0, 0, userFooter.Width, 0);
                }
            };
            sidebarLayout.Controls.Add(userFooter, 0, 2);

            userAvatar = new SFAvatar
            {
                Location = new Point(12, 18),
                Size = new Size(40, 40),
                Initials = GetInitials(user),
                RingColor = IsAdmin ? Colors.Cyan : (IsStaff ? Colors.ElectricBlue : Colors.Emerald),
                BackColor = Colors.SidebarBackground
            };
            if (IsCustomer) userAvatar.AvatarImage = AssetManager.GetCustomerAvatar();
            else if (IsStaff) userAvatar.AvatarImage = AssetManager.GetStaffAvatar();
            userFooter.Controls.Add(userAvatar);

            lblUserName = new Label
            {
                Text = user,
                Font = Typography.SecondaryBold,
                ForeColor = Color.White,
                BackColor = Colors.SidebarBackground,
                Location = new Point(58, 16),
                Size = new Size(126, 20),
                AutoEllipsis = true
            };
            navToolTip.SetToolTip(lblUserName, this.user);
            userFooter.Controls.Add(lblUserName);

            lblUserRole = new Label
            {
                Text = RoleCaption(),
                Font = Typography.Micro,
                ForeColor = Colors.SidebarText,
                BackColor = Colors.SidebarBackground,
                Location = new Point(58, 38),
                Size = new Size(126, 18),
                AutoEllipsis = true
            };
            userFooter.Controls.Add(lblUserRole);

            btnLogout = new SFIconButton
            {
                Icon = SFIcon.LogOut,
                Size = new Size(26, 26),
                Location = new Point(186, 23),
                NormalColor = Colors.SidebarText,
                HoverColor = Colors.Danger,
                HoverBg = Color.FromArgb(40, Colors.Danger),
                NormalBg = Colors.SidebarBackground,
                BackColor = Colors.SidebarBackground
            };
            btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show(this, "Bạn có chắc chắn muốn đăng xuất khỏi hệ thống SPORTFIELD?", "XÁC NHẬN ĐĂNG XUẤT", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Session.AppSession.Current.Clear();
                    Application.Restart();
                }
            };
            navToolTip.SetToolTip(btnLogout, "Đăng xuất tài khoản");
            userFooter.Controls.Add(btnLogout);

            PopulateNavigation();

            // 4. Right Host (Topbar + Content)
            rightHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.AppBackground,
                Margin = new Padding(0)
            };
            workspaceLayout.Controls.Add(rightHost, 1, 0);

            rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Colors.AppBackground,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, Metrics.TopbarHeight));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            rightHost.Controls.Add(rightLayout);

            // Topbar
            topbar = new Panel
            {
                Name = "topbar",
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Margin = new Padding(0),
                Padding = new Padding(16, 0, 16, 0)
            };
            topbar.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Colors.Border, 1.2f))
                {
                    e.Graphics.DrawLine(p, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1);
                }
            };
            rightLayout.Controls.Add(topbar, 0, 0);

            // Topbar TableLayoutPanel (Responsive 3 columns)
            topbarLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            topbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));             // Col 0: Left titles
            topbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));         // Col 1: Middle Search
            topbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));             // Col 2: Right Status & Clock
            topbarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            topbar.Controls.Add(topbarLayout);

            // Left Section (Toggle + Title panel)
            FlowLayoutPanel leftFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                BackColor = Colors.Surface,
                Margin = new Padding(0),
                Padding = new Padding(0, 14, 16, 0)
            };

            SFIconButton btnToggleSidebar = new SFIconButton
            {
                Name = "btnToggle",
                Icon = SFIcon.Menu,
                Size = new Size(36, 36),
                Margin = new Padding(0, 2, 12, 0),
                NormalBg = Colors.Surface,
                BackColor = Colors.Surface
            };
            btnToggleSidebar.Click += (s, e) => ToggleSidebar();
            leftFlow.Controls.Add(btnToggleSidebar);

            Panel titleBlock = new Panel
            {
                AutoSize = true,
                Margin = new Padding(0)
            };
            lblCrumb = new Label
            {
                Text = "SPORTFIELD  /  " + RoleCaption().ToUpper(),
                Font = Typography.MicroBold,
                ForeColor = Colors.TextMuted,
                Location = new Point(0, 0),
                AutoSize = true
            };
            titleBlock.Controls.Add(lblCrumb);

            lblTitle = new Label
            {
                Text = "Tổng quan vận hành",
                Font = Typography.PageTitle,
                ForeColor = Colors.TextPrimary,
                Location = new Point(0, 16),
                AutoSize = true
            };
            titleBlock.Controls.Add(lblTitle);
            leftFlow.Controls.Add(titleBlock);

            topbarLayout.Controls.Add(leftFlow, 0, 0);

            // Middle Section (Search box)
            Panel searchContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(12, 0, 12, 0)
            };
            topSearch = new SFSearchBox
            {
                Placeholder = IsCustomer ? "Tìm sân, voucher, hóa đơn..." : "Tìm nhanh khách, sân, mã đơn...",
                Width = IsCustomer ? 300 : 260,
                Height = 38,
                Anchor = AnchorStyles.Right
            };
            topSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && IsCustomer)
                {
                    e.SuppressKeyPress = true;
                    string q = (topSearch.Text ?? "").Trim().ToLowerInvariant();
                    if (q.Contains("voucher") || q.Contains("mã") || q.Contains("ưu đãi") || q.Contains("khuyến mãi"))
                        ShowView("Voucher của tôi");
                    else if (q.Contains("hóa đơn") || q.Contains("hoa don") || q.Contains("bill") || q.Contains("thanh toán"))
                        ShowView("Hóa đơn của tôi");
                    else if (q.Contains("lịch sử") || q.Contains("lich su") || q.Contains("đơn đặt"))
                        ShowView("Lịch sử đặt sân");
                    else if (q.Contains("hồ sơ") || q.Contains("cá nhân") || q.Contains("tài khoản"))
                        ShowView("Thông tin cá nhân");
                    else
                        ShowView("Đặt sân");
                }
            };
            topSearch.Location = new Point(Math.Max(0, searchContainer.Width - topSearch.Width), (Metrics.TopbarHeight - topSearch.Height) / 2);
            searchContainer.Resize += (s, e) =>
            {
                topSearch.Location = new Point(Math.Max(0, searchContainer.Width - topSearch.Width), (Metrics.TopbarHeight - topSearch.Height) / 2);
            };
            searchContainer.Controls.Add(topSearch);
            topbarLayout.Controls.Add(searchContainer, 1, 0);

            // Right Section (DB Status + Live Clock + Customer Profile)
            FlowLayoutPanel rightFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(0, 16, 0, 0)
            };

            dbStatus = new Label
            {
                Text = IsCustomer ? "● SQL ONLINE" : "● SQL SERVER KẾT NỐI",
                Font = Typography.MicroBold,
                ForeColor = Colors.DeepEmerald,
                BackColor = Colors.SuccessBg,
                AutoSize = false,
                Width = IsCustomer ? 106 : 150,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 2, 10, 0),
                Cursor = Cursors.Hand
            };
            dbStatus.Click += (s, e) => CheckDatabaseConnectionAsync();
            rightFlow.Controls.Add(dbStatus);

            lblClock = new Label
            {
                Text = DateTime.Now.ToString("dd/MM/yyyy • HH:mm"),
                Font = Typography.SecondaryBold,
                ForeColor = Colors.TextSecondary,
                AutoSize = false,
                Width = 136,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 2, 10, 0)
            };
            rightFlow.Controls.Add(lblClock);

            btnBell = new SFBellButton
            {
                Size = new Size(38, 30),
                Margin = new Padding(0, 2, IsCustomer ? 10 : 0, 0),
                Cursor = Cursors.Hand
            };
            btnBell.Click += (s, e) => ToggleNotificationPopup();
            rightFlow.Controls.Add(btnBell);

            if (IsCustomer)
            {
                Panel pnlTopUser = new Panel
                {
                    Size = new Size(168, 36),
                    Margin = new Padding(0, -1, 0, 0),
                    BackColor = Colors.Surface,
                    Cursor = Cursors.Hand
                };
                SFAvatar topAvatar = new SFAvatar
                {
                    Location = new Point(4, 2),
                    Size = new Size(32, 32),
                    Initials = GetInitials(this.user),
                    RingColor = Colors.Emerald,
                    BackColor = Colors.Surface,
                    AvatarImage = AssetManager.GetCustomerAvatar(),
                    Cursor = Cursors.Hand
                };
                Label lblTopName = new Label
                {
                    Text = this.user,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Colors.TextPrimary,
                    Location = new Point(42, 1),
                    Size = new Size(122, 17),
                    AutoEllipsis = true,
                    Cursor = Cursors.Hand
                };
                Label lblTopRole = new Label
                {
                    Text = RoleCaption(),
                    Font = new Font("Segoe UI", 7.75f, FontStyle.Regular),
                    ForeColor = Colors.TextSecondary,
                    Location = new Point(42, 18),
                    Size = new Size(122, 15),
                    AutoEllipsis = true,
                    Cursor = Cursors.Hand
                };
                EventHandler openProfile = (s, e) => ShowView("Thông tin cá nhân");
                pnlTopUser.Click += openProfile;
                topAvatar.Click += openProfile;
                lblTopName.Click += openProfile;
                lblTopRole.Click += openProfile;
                navToolTip.SetToolTip(pnlTopUser, this.user + " — Nhấn để mở Thông tin cá nhân");
                pnlTopUser.Controls.Add(topAvatar);
                pnlTopUser.Controls.Add(lblTopName);
                pnlTopUser.Controls.Add(lblTopRole);
                rightFlow.Controls.Add(pnlTopUser);
            }

            topbarLayout.Controls.Add(rightFlow, 2, 0);

            clockTimer = new Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy • HH:mm");
            clockTimer.Start();

            // Responsive Topbar resize handling
            topbar.Resize += (s, e) =>
            {
                bool isCompact = topbar.Width < 1040;
                dbStatus.Visible = !isCompact;
                topSearch.Width = isCompact ? 180 : (IsCustomer ? 290 : 260);
            };

            // 5. Content Host
            contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.AppBackground,
                Padding = new Padding(Metrics.ContentPadding),
                Margin = new Padding(0)
            };
            rightLayout.Controls.Add(contentHost, 0, 1);

            // Animation timer for sidebar
            sidebarTimer = new Timer { Interval = 15 };
            sidebarTimer.Tick += (s, e) =>
            {
                int step = 28;
                int currentW = (int)workspaceLayout.ColumnStyles[0].Width;
                if (currentW < targetSidebarWidth)
                {
                    currentW = Math.Min(currentW + step, targetSidebarWidth);
                }
                else if (currentW > targetSidebarWidth)
                {
                    currentW = Math.Max(currentW - step, targetSidebarWidth);
                }

                workspaceLayout.ColumnStyles[0].Width = currentW;
                sidebarHost.Width = currentW;

                if (currentW == targetSidebarWidth)
                {
                    sidebarTimer.Stop();
                    ApplyCollapsedState(isSidebarCollapsed);
                }
            };
        }

        private void ToggleSidebar()
        {
            if (sidebarTimer.Enabled) return;
            isSidebarCollapsed = !isSidebarCollapsed;
            targetSidebarWidth = isSidebarCollapsed ? Metrics.SidebarCollapsed : Metrics.SidebarExpanded;
            sidebarTimer.Start();
        }

        private void ApplyCollapsedState(bool collapsed)
        {
            // Update Brand
            if (collapsed)
            {
                logoMark.Location = new Point((Metrics.SidebarCollapsed - logoMark.Width) / 2, 18);
                brandName.Visible = false;
                brandSub.Visible = false;
            }
            else
            {
                logoMark.Location = new Point(14, 18);
                brandName.Visible = true;
                brandSub.Visible = true;
            }
            brandPanel.Invalidate();

            // Relayout nav items
            navScrollPanel.Relayout(collapsed);

            // Update User Footer
            if (collapsed)
            {
                userAvatar.Location = new Point((Metrics.SidebarCollapsed - userAvatar.Width) / 2, 10);
                lblUserName.Visible = false;
                lblUserRole.Visible = false;
                btnLogout.Location = new Point((Metrics.SidebarCollapsed - btnLogout.Width) / 2, 54);
            }
            else
            {
                userAvatar.Location = new Point(12, 18);
                lblUserName.Visible = true;
                lblUserRole.Visible = true;
                btnLogout.Location = new Point(186, 23);
            }
            userFooter.Invalidate();
            sidebarHost.Invalidate();
        }

        private static string SanitizeUtf8(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Contains("Ã") || input.Contains("á»") || input.Contains("áº") || input.Contains("Æ°") || input.Contains("Â") || input.Contains("â€"))
            {
                try
                {
                    byte[] bytes = System.Text.Encoding.GetEncoding(1252).GetBytes(input);
                    string decoded = System.Text.Encoding.UTF8.GetString(bytes);
                    if (!decoded.Contains("?")) return decoded;
                }
                catch (Exception ex)
                {
                    Helpers.AppLogger.LogWarning("SanitizeUtf8 failed: " + ex.Message);
                }
            }
            return input;
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "SF";
            string[] parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        private Button CreateWindowButton(string text, bool isClose, Action onClick)
        {
            Button b = new Button
            {
                Text = text,
                Dock = DockStyle.Right,
                Width = 46,
                FlatStyle = FlatStyle.Flat,
                BackColor = Colors.SidebarBackground,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (s, e) => onClick();
            return b;
        }

        private string RoleCaption()
        {
            if (!string.IsNullOrEmpty(Session.AppSession.Current.TenVaiTro))
                return Session.AppSession.Current.TenVaiTro;
            if (IsAdmin) return "Quản trị viên";
            if (IsStaff) return "Nhân viên trực quầy";
            return "Khách hàng";
        }

        private void PopulateNavigation()
        {
            navScrollPanel.Controls.Clear();
            navButtons.Clear();
            navGroupLabels.Clear();

            if (IsAdmin)
            {
                AddNavGroup("VẬN HÀNH", new[] {
                    ("Tổng quan", Helpers.SportFieldIcon.Dashboard),
                    ("Đặt sân", Helpers.SportFieldIcon.Booking),
                    ("Lịch sân", Helpers.SportFieldIcon.Schedule),
                    ("Quản lý đơn", Helpers.SportFieldIcon.BookingList)
                });

                AddNavGroup("QUẢN LÝ", new[] {
                    ("Sân", Helpers.SportFieldIcon.Field),
                    ("Loại sân", Helpers.SportFieldIcon.FieldType),
                    ("Khách hàng", Helpers.SportFieldIcon.Users),
                    ("Nhân viên", Helpers.SportFieldIcon.Employee),
                    ("Dịch vụ & Kho", Helpers.SportFieldIcon.Inventory)
                });

                AddNavGroup("KINH DOANH", new[] {
                    ("Hóa đơn", Helpers.SportFieldIcon.Invoice),
                    ("Voucher", Helpers.SportFieldIcon.Voucher),
                    ("Khuyến mãi", Helpers.SportFieldIcon.Campaign),
                    ("Đặt cọc online", Helpers.SportFieldIcon.Vietqr)
                });

                AddNavGroup("PHÂN TÍCH", new[] {
                    ("Thống kê", Helpers.SportFieldIcon.Report)
                });

                AddNavGroup("HỆ THỐNG", new[] {
                    ("Tài khoản", Helpers.SportFieldIcon.Lock),
                    ("Cấu hình", Helpers.SportFieldIcon.Settings),
                    ("Nhật ký", Helpers.SportFieldIcon.Info)
                });
            }
            else if (IsStaff)
            {
                AddNavGroup("CA TRỰC", new[] {
                    ("Trang chủ", Helpers.SportFieldIcon.Dashboard),
                    ("Thống kê ca trực", Helpers.SportFieldIcon.Shift)
                });

                AddNavGroup("QUẦY POS", new[] {
                    ("Đặt sân tại quầy", Helpers.SportFieldIcon.Booking),
                    ("Hóa đơn & Thu tiền", Helpers.SportFieldIcon.Payment),
                    ("Lịch đặt sân", Helpers.SportFieldIcon.Schedule)
                });

                AddNavGroup("TRA CỨU", new[] {
                    ("Khách hàng", Helpers.SportFieldIcon.Users),
                    ("Thông tin sân", Helpers.SportFieldIcon.Field),
                    ("Loại sân", Helpers.SportFieldIcon.FieldType)
                });

                AddNavGroup("HỆ THỐNG", new[] {
                    ("Tài khoản cá nhân", Helpers.SportFieldIcon.Profile)
                });
            }
            else
            {
                AddNavGroup("KHÁM PHÁ", new[] {
                    ("Trang chủ", Helpers.SportFieldIcon.Home)
                });

                AddNavGroup("ĐẶT SÂN", new[] {
                    ("Đặt sân", Helpers.SportFieldIcon.Booking),
                    ("Lịch sử đặt sân", Helpers.SportFieldIcon.BookingList)
                });

                AddNavGroup("TÀI KHOẢN", new[] {
                    ("Hóa đơn của tôi", Helpers.SportFieldIcon.Invoice),
                    ("Voucher của tôi", Helpers.SportFieldIcon.Voucher),
                    ("Thông tin cá nhân", Helpers.SportFieldIcon.Profile)
                });
            }

            navScrollPanel.Relayout(isSidebarCollapsed);
        }

        private void AddNavGroup(string groupName, (string name, Helpers.SportFieldIcon icon)[] items)
        {
            // Group section header
            Label lblGroup = new Label
            {
                Text = groupName,
                Font = Typography.SectionHeader,
                ForeColor = Colors.SidebarSectionTitle,
                BackColor = Colors.SidebarBackground,
                Height = 26,
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(12, 0, 0, 4)
            };
            navGroupLabels.Add(lblGroup);
            navScrollPanel.Controls.Add(lblGroup);

            foreach (var item in items)
            {
                NavItemButton btn = new NavItemButton(item.name, item.icon);
                btn.Click += (s, e) => ShowView(item.name);
                navToolTip.SetToolTip(btn, item.name);
                navScrollPanel.Controls.Add(btn);
                navButtons[item.name] = btn;
            }
        }

        private void AddNavGroup(string groupName, (string name, SFIcon icon)[] items)
        {
            // Group section header
            Label lblGroup = new Label
            {
                Text = groupName,
                Font = Typography.SectionHeader,
                ForeColor = Colors.SidebarSectionTitle,
                BackColor = Colors.SidebarBackground,
                Height = 26,
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(12, 0, 0, 4)
            };
            navGroupLabels.Add(lblGroup);
            navScrollPanel.Controls.Add(lblGroup);

            foreach (var item in items)
            {
                NavItemButton btn = new NavItemButton(item.name, item.icon);
                btn.Click += (s, e) => ShowView(item.name);
                navToolTip.SetToolTip(btn, item.name);
                navScrollPanel.Controls.Add(btn);
                navButtons[item.name] = btn;
            }
        }

        private void ActiveNav(string key)
        {
            foreach (var kv in navButtons)
            {
                kv.Value.IsActive = (kv.Key == key);
            }
        }

        private string ResolveKey(string rawKey)
        {
            if (string.IsNullOrEmpty(rawKey)) return "Tổng quan";
            string k = SanitizeUtf8(rawKey).Trim();

            // English & normalized aliases
            if (k.IndexOf("history", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Lịch sử", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Lich su", StringComparison.OrdinalIgnoreCase) >= 0) return "Lịch sử đặt sân";
            if (k.IndexOf("schedule", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("timeline", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Lịch", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Lich", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("ch s", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("L<ch", StringComparison.OrdinalIgnoreCase) >= 0) return IsStaff ? "Lịch đặt sân" : "Lịch sân";
            if (k.IndexOf("manage", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Quản lý", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Quan ly", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Qu?n ly", StringComparison.OrdinalIgnoreCase) >= 0) return "Quản lý đơn";
            if (k.IndexOf("fieldtype", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Loại", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Loai", StringComparison.OrdinalIgnoreCase) >= 0) return IsStaff ? "Loại sân & Đơn giá" : "Loại sân";
            if (k.IndexOf("quickbook", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Đặt", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Dat", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Ä\u0090", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Book", StringComparison.OrdinalIgnoreCase) >= 0) return IsStaff ? "Đặt sân tại quầy" : "Đặt sân";
            if (k.IndexOf("field", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Sân", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("San", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("SÃ¢n", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("SAn", StringComparison.OrdinalIgnoreCase) >= 0) return IsStaff ? "Thông tin sân" : "Sân";
            if (k.IndexOf("customer", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Khách", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Khach", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("KhAch", StringComparison.OrdinalIgnoreCase) >= 0) return "Khách hàng";
            if (k.IndexOf("employee", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("staff", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Nhân viên", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Nhan vien", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("NhAn viAan", StringComparison.OrdinalIgnoreCase) >= 0) return "Nhân viên";
            if (k.IndexOf("service", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("inventory", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Kho", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Dịch vụ", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Dich vu", StringComparison.OrdinalIgnoreCase) >= 0) return "Dịch vụ & Kho";
            if (k.IndexOf("invoice", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("bill", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Hóa đơn", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Hoa don", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("HA3a", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("HÃ³a", StringComparison.OrdinalIgnoreCase) >= 0) return IsCustomer ? "Hóa đơn của tôi" : (IsStaff ? "Hóa đơn & Thu tiền" : "Hóa đơn");
            if (k.IndexOf("voucher", StringComparison.OrdinalIgnoreCase) >= 0) return IsCustomer ? "Voucher của tôi" : "Voucher";
            if (k.IndexOf("promo", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("promotion", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Khuyến mãi", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Khuyen mai", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Khuyn", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("KhuyÃªn", StringComparison.OrdinalIgnoreCase) >= 0) return "Khuyến mãi";
            if (k.IndexOf("deposit", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("cọc", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("coc", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("c?c", StringComparison.OrdinalIgnoreCase) >= 0) return "Đặt cọc online";
            if (k.IndexOf("report", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("stat", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Thống kê", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Thong ke", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Th`ng", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("ThÃ´ng", StringComparison.OrdinalIgnoreCase) >= 0) return IsStaff ? "Thống kê ca trực" : "Thống kê";
            if (k.IndexOf("profile", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Thông tin cá nhân", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("ca nhan", StringComparison.OrdinalIgnoreCase) >= 0) return "Thông tin cá nhân";
            if (k.IndexOf("account", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Tài khoản", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Tai khoan", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("TAi", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("TÃ i", StringComparison.OrdinalIgnoreCase) >= 0) return IsStaff ? "Tài khoản cá nhân" : "Tài khoản";
            if (k.IndexOf("setting", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("config", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Cấu hình", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Cau hinh", StringComparison.OrdinalIgnoreCase) >= 0) return "Cấu hình";
            if (k.IndexOf("log", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("audit", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Nhật ký", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Nhat ky", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Nh-t", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("NhÃ¢t", StringComparison.OrdinalIgnoreCase) >= 0) return "Nhật ký";
            if (k.IndexOf("dashboard", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("overview", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Tổng", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Tong", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("T ng", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("TÃ´ng", StringComparison.OrdinalIgnoreCase) >= 0) return "Tổng quan";
            if (k.IndexOf("home", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("Trang", StringComparison.OrdinalIgnoreCase) >= 0) return "Trang chủ";
            return rawKey;
        }

        public void ShowView(string rawKey)
        {
            string key = ResolveKey(rawKey);
            ActiveNav(key);
            lblTitle.Text = key;
            lblCrumb.Text = "SPORTFIELD  /  " + RoleCaption().ToUpper() + "  /  " + key.ToUpper();

            Control view = null;
            if (IsStaff) view = StaffView(key);
            else if (IsCustomer) view = CustomerView(key);
            else view = AdminView(key);

            if (view == null) view = new DashboardView();

            // Smooth animated page slide transition
            contentHost.SuspendLayout();
            contentHost.Padding = (IsCustomer && key == "Trang chủ") ? new Padding(0) : new Padding(Metrics.ContentPadding);
            contentHost.Controls.Clear();
            view.Dock = DockStyle.Fill;
            contentHost.Controls.Add(view);
            contentHost.ResumeLayout(true);
        }

        private Control AdminView(string key)
        {
            switch (key)
            {
                case "Tổng quan": return new DashboardView();
                case "Đặt sân": return new QuickBookingView();
                case "Lịch sân": return new ScheduleView();
                case "Quản lý đơn": return new BookingManagementView();
                case "Sân": return new FieldManagementView();
                case "Loại sân": return new FieldTypesPriceView();
                case "Khách hàng": return new CustomerManagementView();
                case "Nhân viên": return new EmployeeManagementView();
                case "Dịch vụ & Kho": return new ServiceInventoryView();
                case "Hóa đơn": return new AdminInvoicesView();
                case "Voucher": return new VoucherManagementView();
                case "Khuyến mãi": return new PromotionsView();
                case "Đặt cọc online": return new OnlineDepositView();
                case "Thống kê": return new ReportsView();
                case "Tài khoản": return new AccountsView();
                case "Cấu hình": return new AdminSettingsView();
                case "Nhật ký": return new SystemLogsView();
                default: return new DashboardView();
            }
        }

        private Control StaffView(string key)
        {
            switch (key)
            {
                case "Trang chủ": return new StaffDashboardView();
                case "Thống kê ca trực": return new StaffStatisticsView();
                case "Đặt sân tại quầy": return new StaffBookingView();
                case "Hóa đơn & Thu tiền": return new StaffInvoicesView();
                case "Lịch đặt sân": return new StaffScheduleView();
                case "Khách hàng": return new StaffCustomersView();
                case "Thông tin sân": return new StaffFieldsReadOnlyView();
                case "Loại sân": return new StaffFieldTypesReadOnlyView();
                case "Tài khoản cá nhân": return new StaffAccountView();
                default: return new StaffDashboardView();
            }
        }

        private Control CustomerView(string key)
        {
            switch (key)
            {
                case "Trang chủ": return new CustomerHomeView();
                case "Đặt sân": return new CustomerBookingView();
                case "Lịch sử đặt sân": return new CustomerHistoryView();
                case "Hóa đơn của tôi": return new CustomerInvoicesView();
                case "Voucher của tôi": return new CustomerVouchersView();
                case "Thông tin cá nhân": return new CustomerProfileView();
                default: return new CustomerHomeView();
            }
        }

        #region Notification Center

        private void InitializeNotificationCenter()
        {
            try
            {
                notificationPopup = new NotificationPopup();
                notificationPopup.NotificationSelected += OnNotificationSelected;

                NotificationService.OnNotificationUpdated += () =>
                {
                    if (IsDisposed || Disposing) return;
                    if (InvokeRequired)
                    {
                        try { BeginInvoke(new Action(RefreshNotificationBadge)); } catch { }
                    }
                    else
                    {
                        RefreshNotificationBadge();
                    }
                };

                RefreshNotificationBadge();

                notificationTimer = new Timer { Interval = 45000 };
                notificationTimer.Tick += (s, e) => RefreshNotificationBadge();
                notificationTimer.Start();
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "InitializeNotificationCenter");
            }
        }

        private void RefreshNotificationBadge()
        {
            if (IsDisposed || Disposing || btnBell == null) return;
            try
            {
                int count = NotificationService.Instance.GetUnreadCount();
                btnBell.UnreadCount = count < 0 ? 0 : count;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning("RefreshNotificationBadge failed: " + ex.Message);
                btnBell.UnreadCount = 0;
            }
        }

        private void ToggleNotificationPopup()
        {
            if (notificationPopup == null || btnBell == null) return;
            if (notificationPopup.Visible)
            {
                notificationPopup.Close();
            }
            else
            {
                notificationPopup.ShowBelow(btnBell);
            }
        }

        private void OnNotificationSelected(NotificationItem item)
        {
            if (item == null) return;
            try
            {
                string lienKetLoai = (item.LienKetLoai ?? "").Trim().ToUpperInvariant();
                long? lienKetId = item.LienKetID;

                switch (lienKetLoai)
                {
                    case "BOOKING":
                    case "DATSAN":
                    case "DONDATSAN":
                        if (lienKetId.HasValue && lienKetId.Value > 0)
                        {
                            using (var dlg = new BookingDetailForm(lienKetId.Value))
                            {
                                dlg.ShowDialog(this);
                            }
                        }
                        else
                        {
                            if (IsCustomer) ShowView("Lịch sử đặt sân");
                            else if (IsStaff) ShowView("Lịch đặt sân");
                            else ShowView("Quản lý đơn");
                        }
                        break;

                    case "PAYMENT":
                    case "HOADON":
                    case "THANHTOAN":
                        if (IsCustomer) ShowView("Hóa đơn của tôi");
                        else if (IsStaff) ShowView("Hóa đơn & Thu tiền");
                        else ShowView("Hóa đơn");
                        break;

                    case "VOUCHER":
                        if (IsCustomer) ShowView("Voucher của tôi");
                        else ShowView("Voucher");
                        break;

                    case "PROMOTION":
                    case "KHUYENMAI":
                        if (IsCustomer) ShowView("Voucher của tôi");
                        else ShowView("Khuyến mãi");
                        break;

                    case "DEPOSIT":
                    case "DATCOC":
                        if (IsCustomer) ShowView("Lịch sử đặt sân");
                        else ShowView("Đặt cọc online");
                        break;

                    case "ACCOUNT_RECOVERY":
                    case "KHOIPHUC":
                        if (IsAdmin)
                        {
                            using (var dlg = new AdminRecoveryDialog(lienKetId.HasValue ? lienKetId.Value : 0))
                            {
                                dlg.ShowDialog(this);
                            }
                        }
                        break;

                    default:
                        ShowView(GetHomeKey());
                        break;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "OnNotificationSelected");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (notificationTimer != null)
                {
                    notificationTimer.Stop();
                    notificationTimer.Dispose();
                    notificationTimer = null;
                }
                if (clockTimer != null)
                {
                    clockTimer.Stop();
                    clockTimer.Dispose();
                    clockTimer = null;
                }
                if (sidebarTimer != null)
                {
                    sidebarTimer.Stop();
                    sidebarTimer.Dispose();
                    sidebarTimer = null;
                }
                if (notificationPopup != null)
                {
                    notificationPopup.Dispose();
                    notificationPopup = null;
                }
            }
            catch { }
            base.OnFormClosed(e);
        }

        #endregion
    }

    public class DoubleBufferedNavPanel : Panel
    {
        private const int SB_HORZ = 0;
        private const int WM_NCPAINT = 0x85;
        private const int WM_HSCROLL = 0x114;

        [DllImport("user32.dll")]
        private static extern int ShowScrollBar(IntPtr hWnd, int wBar, int bShow);

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= ~0x00100000; // Strip WS_HSCROLL
                return cp;
            }
        }

        public DoubleBufferedNavPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            AutoScroll = true;
            HorizontalScroll.Enabled = false;
            HorizontalScroll.Visible = false;
            BackColor = Colors.SidebarBackground;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCPAINT || m.Msg == WM_HSCROLL)
            {
                if (IsHandleCreated) ShowScrollBar(Handle, SB_HORZ, 0);
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate(true);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Invalidate(true);
        }

        public void Relayout(bool isCollapsed)
        {
            SuspendLayout();
            try
            {
                HorizontalScroll.Enabled = false;
                HorizontalScroll.Visible = false;
                HorizontalScroll.Maximum = 0;

                int scrollY = -AutoScrollPosition.Y;
                AutoScrollPosition = new Point(0, 0);

                int y = 6;
                int clientW = Math.Max(40, ClientSize.Width);
                int scrollReserve = SystemInformation.VerticalScrollBarWidth + 2;
                int targetW = isCollapsed ? 46 : Math.Max(140, clientW - 16 - scrollReserve);
                int targetX = isCollapsed ? Math.Max(2, (clientW - 46) / 2) : 8;

                foreach (Control c in Controls)
                {
                    if (c is Label lbl)
                    {
                        if (isCollapsed)
                        {
                            lbl.Visible = false;
                        }
                        else
                        {
                            lbl.Visible = true;
                            lbl.Location = new Point(8, y);
                            lbl.Size = new Size(targetW, 26);
                            y += lbl.Height + 4;
                        }
                    }
                    else if (c is NavItemButton btn)
                    {
                        btn.IsCollapsed = isCollapsed;
                        btn.Location = new Point(targetX, y);
                        btn.Size = new Size(targetW, isCollapsed ? 38 : 44);
                        y += btn.Height + (isCollapsed ? 3 : 4);
                    }
                }

                AutoScrollMinSize = new Size(0, y + 10);
                if (scrollY > 0)
                {
                    AutoScrollPosition = new Point(0, Math.Min(scrollY, y));
                }

                if (IsHandleCreated) ShowScrollBar(Handle, SB_HORZ, 0);
            }
            finally
            {
                ResumeLayout(true);
                Invalidate(true);
            }
        }
    }

    public class NavItemButton : Button
    {
        private bool isHovered = false;
        private bool isActive = false;
        private bool isCollapsed = false;
        private readonly string itemText;
        private readonly SFIcon icon = SFIcon.None;
        private readonly Helpers.SportFieldIcon? sfIcon = null;

        public bool IsActive
        {
            get => isActive;
            set { isActive = value; Invalidate(); }
        }

        public bool IsCollapsed
        {
            get => isCollapsed;
            set { isCollapsed = value; Invalidate(); }
        }

        public NavItemButton(string text, Helpers.SportFieldIcon sfIcon)
        {
            this.itemText = text;
            this.sfIcon = sfIcon;
            this.icon = SFIcon.None;
            InitializeButton();
        }

        public NavItemButton(string text, SFIcon icon)
        {
            this.itemText = text;
            this.icon = icon;
            this.sfIcon = null;
            InitializeButton();
        }

        private void InitializeButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Size = new Size(210, 44);
            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Colors.SidebarBackground;
            ForeColor = Colors.SidebarText;
            Margin = new Padding(0, 2, 0, 2);
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); isHovered = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); isHovered = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Clear background with solid sidebar color to prevent any ghost trails
            g.Clear(Colors.SidebarBackground);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            Color bg = isActive ? Colors.SidebarActive : (isHovered ? Colors.SidebarHover : Color.Transparent);
            Color fg = isActive ? Color.White : (isHovered ? Color.White : Colors.SidebarText);

            if (isActive && rect.Width > 4 && rect.Height > 4)
            {
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, 10))
                using (LinearGradientBrush grad = new LinearGradientBrush(rect, Color.FromArgb(15, 118, 110), Color.FromArgb(16, 185, 129), 0f))
                {
                    g.FillPath(grad, path);
                }
            }
            else if (bg != Color.Transparent)
            {
                using (GraphicsPath path = GraphicsUtility.CreateRoundedRectangle(rect, 10))
                using (SolidBrush b = new SolidBrush(bg))
                {
                    g.FillPath(b, path);
                }
            }

            // Draw Icon (24x24 standard sidebar size)
            int iconSize = 24;
            int iconX = isCollapsed ? (Width - iconSize) / 2 : 12;
            Rectangle iconRect = new Rectangle(iconX, (Height - iconSize) / 2, iconSize, iconSize);
            if (sfIcon.HasValue)
            {
                Helpers.SportFieldIconProvider.Draw(g, sfIcon.Value, iconRect, fg);
            }
            else
            {
                VectorIcons.Draw(g, icon, iconRect, fg, 1.8f);
            }

            // Draw Text if not collapsed
            if (!isCollapsed && Width > 50)
            {
                Rectangle textRect = new Rectangle(44, 0, Width - 50, Height);
                TextRenderer.DrawText(g, itemText, isActive ? Typography.Button : Typography.Body,
                    textRect, fg, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
