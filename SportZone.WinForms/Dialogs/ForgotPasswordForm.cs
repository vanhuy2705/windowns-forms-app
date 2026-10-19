using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Models;
using SportZone.WinForms.Security;
using SportZone.WinForms.Services;

namespace SportZone.WinForms.Dialogs
{
    /// <summary>
    /// Hộp thoại Khôi phục Mật khẩu SPORTFIELD
    /// Thiết kế UI/UX thương mại cao cấp, đồng bộ chuẩn mực với LoginForm
    /// Hỗ trợ High-DPI (100%, 125%, 150%) không vỡ layout, không thanh cuộn
    /// Tích hợp đầy đủ quy trình xác thực OTP Email thực tế qua Gmail SMTP + MailKit
    /// </summary>
    public class ForgotPasswordForm : Form
    {
        // ==========================================
        // DESIGN TOKENS & COLOR PALETTE
        // ==========================================
        private static readonly Color ColBg = Color.FromArgb(244, 247, 251);          // #F4F7FB
        private static readonly Color ColCard = Color.White;                           // #FFFFFF
        private static readonly Color ColPrimary = Color.FromArgb(15, 118, 110);       // #0F766E (Teal-700)
        private static readonly Color ColPrimaryHover = Color.FromArgb(17, 94, 89);   // #115E59 (Teal-800)
        private static readonly Color ColPrimaryActive = Color.FromArgb(13, 75, 71);
        private static readonly Color ColTealLight = Color.FromArgb(240, 253, 250);   // #F0FDFA
        private static readonly Color ColTealBorder = Color.FromArgb(153, 246, 228);  // #99F6E4
        private static readonly Color ColTextPrimary = Color.FromArgb(20, 35, 46);     // #14232E
        private static readonly Color ColTextSecondary = Color.FromArgb(100, 116, 139); // #64748B
        private static readonly Color ColTextMuted = Color.FromArgb(148, 163, 184);    // #94A3B8
        private static readonly Color ColBorder = Color.FromArgb(226, 232, 240);       // #E2E8F0
        private static readonly Color ColBorderInput = Color.FromArgb(203, 213, 225);  // #CBD5E1
        private static readonly Color ColSuccess = Color.FromArgb(21, 128, 61);        // #15803D (Emerald-700)
        private static readonly Color ColSuccessBg = Color.FromArgb(220, 252, 231);    // #DCFCE7
        private static readonly Color ColDanger = Color.FromArgb(185, 28, 28);         // #B91C1C (Red-700)
        private static readonly Color ColAmber = Color.FromArgb(217, 119, 6);          // #D97706 (Amber-600)

        // ==========================================
        // WIN32 DRAG IMPORT
        // ==========================================
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return cp;
            }
        }

        // ==========================================
        // PUBLIC OUTPUT PROPERTY
        // ==========================================
        public string RecoveredUsername { get; private set; }

        // ==========================================
        // STATE VARIABLES
        // ==========================================
        private RecoveryAccountInfo _currentAccount;
        private long _currentRequestId = 0;
        private string _selectedChannel = "EMAIL";
        private int _otpTimeRemaining = 300; // 5 phút
        private int _resendCooldown = 60;   // 60 giây
        private Timer _timerOtp;
        private Timer _timerResend;
        private int _currentStepIndex = 1; // 1: Tài khoản, 2: Xác minh, 3: Mật khẩu, 4: Thành công

        // ==========================================
        // ROOT UI CONTROLS
        // ==========================================
        private Panel pnlTopBar;
        private StepperControl stepper;
        private CardPanel cardMain;
        private Label lnkBottomBackToLogin;

        // Step Panels (hosted inside cardMain)
        private Panel pnlStep1;
        private Panel pnlStep2;
        private Panel pnlStep3;
        private Panel pnlSuccess;
        private Panel pnlAdminAssistance;

        // Step 1 Controls
        private ModernInputBox txtIdentifier;
        private Label lblIdentifyError;
        private ModernButton btnIdentifyNext;
        private Panel pnlHint1;

        // Step 2 Controls
        private EmailBadgePanel pnlEmailBadge;
        private OtpBoxControl otpBox;
        private Label lblOtpError;
        private Label lblOtpCountdown;
        private Label lblOtpResendLink;
        private ModernButton btnOtpVerify;
        private Label lnkChangeAccount;

        // Step 3 Controls
        private ModernInputBox txtNewPassword;
        private ModernInputBox txtConfirmPassword;
        private Panel pnlRequirements;
        private Label lblReqLength;
        private Label lblReqLetterDigit;
        private Label lblReqMatch;
        private Label lblResetError;
        private ModernButton btnResetSubmit;

        // Step 4 Success Controls
        private PictureBox pbSuccess;
        private Label lblSuccessTitle;
        private Label lblSuccessDesc;
        private ModernButton btnSuccessBackToLogin;

        // Step Admin Assistance Controls
        private Panel pnlNoteBox;
        private TextBox txtAdminNote;
        private ModernButton btnSubmitAdminReq;
        private Label lblAdminReqError;
        private Label lnkAdminBack;

        // ==========================================
        // CONSTRUCTOR
        // ==========================================
        public ForgotPasswordForm()
        {
            DpiHelper.ApplyDpiMode(this);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(760, 620);
            try { this.Icon = Helpers.SportFieldIconProvider.GetWindowIcon(Helpers.SportFieldIcon.Key); } catch { }
            BackColor = ColBg;
            Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            DoubleBuffered = true;
            KeyPreview = true;

            // Keyboard navigation: Escape to cancel, Enter to submit current step
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            };

            BuildUI();
            LayoutFormControls();
            ShowStep(1);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            using (var path = GraphicsUtility.CreateRoundedRectangle(ClientRectangle, 16))
            {
                Region = new Region(path);
            }
            LayoutFormControls();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Outer delicate window border
            using (var pen = new Pen(ColBorderInput, 1.2f))
            using (var path = GraphicsUtility.CreateRoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), 16))
            {
                g.DrawPath(pen, path);
            }
        }

        // ==========================================
        // FORM-LEVEL RESPONSIVE DYNAMIC LAYOUT
        // ==========================================
        private void LayoutFormControls()
        {
            if (pnlTopBar == null || cardMain == null || stepper == null) return;

            int formW = ClientSize.Width;
            int formH = ClientSize.Height;

            // 1. Top bar: spans full width
            pnlTopBar.SetBounds(0, 0, formW, 68);

            // 2. Card width: 640px or responsive with 32px margins
            int cardW = Math.Min(640, formW - 48);
            int cardX = (formW - cardW) / 2;

            // 3. Stepper
            stepper.SetBounds(cardX, 74, cardW, 50);

            // 4. Central Card Panel
            int cardH = Math.Min(420, formH - 134 - 52);
            cardMain.SetBounds(cardX, 132, cardW, cardH);

            // 5. Bottom Return Link
            int linkY = cardMain.Bottom + 10;
            lnkBottomBackToLogin.SetBounds(cardX, linkY, cardW, 28);
        }

        // ==========================================
        // UI BUILDER
        // ==========================================
        private void BuildUI()
        {
            // 1. Top Bar / Header
            BuildTopBar();

            // 2. Stepper Control
            stepper = new StepperControl
            {
                BackColor = ColBg
            };
            Controls.Add(stepper);

            // 3. Central Content Card
            cardMain = new CardPanel
            {
                BackColor = ColCard
            };
            Controls.Add(cardMain);

            // 4. Step Content Panels inside card
            BuildStep1Identify();
            BuildStep2Otp();
            BuildStep3ResetPassword();
            BuildStepSuccess();
            BuildStepAdminAssistance();

            // 5. Bottom Return to Login Link
            lnkBottomBackToLogin = new Label
            {
                Text = "← Quay lại màn hình đăng nhập",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColTextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            lnkBottomBackToLogin.MouseEnter += (s, e) => lnkBottomBackToLogin.ForeColor = ColPrimary;
            lnkBottomBackToLogin.MouseLeave += (s, e) => lnkBottomBackToLogin.ForeColor = ColTextSecondary;
            lnkBottomBackToLogin.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(lnkBottomBackToLogin);
        }

        // ==========================================
        // TOP BAR
        // ==========================================
        private void BuildTopBar()
        {
            pnlTopBar = new Panel
            {
                BackColor = ColBg
            };

            pnlTopBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

            // Custom Paint for TopBar: Shield/Lock Icon + Title + Subtitle
            pnlTopBar.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int startX = Math.Max(24, (pnlTopBar.Width - 640) / 2);

                // Rounded Teal Icon Badge
                Rectangle iconBox = new Rectangle(startX, 14, 40, 40);
                using (var path = GraphicsUtility.CreateRoundedRectangle(iconBox, 10))
                using (var brush = new SolidBrush(Color.FromArgb(204, 251, 241))) // #CCFBF1
                {
                    g.FillPath(brush, path);
                }
                VectorIcons.Draw(g, SFIcon.Lock, new Rectangle(startX + 8, 22, 24, 24), ColPrimary, 2.2f);

                // Title
                using (Font fTitle = new Font("Segoe UI", 15.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "KHÔI PHỤC MẬT KHẨU", fTitle, new Point(startX + 50, 13), ColTextPrimary);
                }

                // Subtitle
                using (Font fSub = new Font("Segoe UI", 9.5f, FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, "Xác minh email để thiết lập mật khẩu mới.", fSub, new Point(startX + 51, 40), ColTextSecondary);
                }
            };

            // Close button [ ✕ ]
            Panel btnClose = new Panel
            {
                Size = new Size(32, 32),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            bool isCloseHover = false;
            btnClose.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, btnClose.Width - 1, btnClose.Height - 1);
                using (var path = GraphicsUtility.CreateRoundedRectangle(r, 8))
                {
                    if (isCloseHover)
                    {
                        using (var b = new SolidBrush(Color.FromArgb(254, 226, 226))) // #FEE2E2
                            g.FillPath(b, path);
                    }
                    Color iconCol = isCloseHover ? ColDanger : ColTextSecondary;
                    using (Pen p = new Pen(iconCol, 1.8f))
                    {
                        p.StartCap = LineCap.Round;
                        p.EndCap = LineCap.Round;
                        int m = 9;
                        g.DrawLine(p, m, m, btnClose.Width - m, btnClose.Height - m);
                        g.DrawLine(p, btnClose.Width - m, m, m, btnClose.Height - m);
                    }
                }
            };
            btnClose.MouseEnter += (s, e) => { isCloseHover = true; btnClose.Invalidate(); };
            btnClose.MouseLeave += (s, e) => { isCloseHover = false; btnClose.Invalidate(); };
            btnClose.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            pnlTopBar.Resize += (s, e) =>
            {
                btnClose.Location = new Point(pnlTopBar.Width - 46, 14);
            };
            pnlTopBar.Controls.Add(btnClose);

            Controls.Add(pnlTopBar);
        }

        // ==========================================
        // STEP 1: XÁC ĐỊNH TÀI KHOẢN
        // ==========================================
        private void BuildStep1Identify()
        {
            pnlStep1 = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColCard,
                Visible = false
            };

            Label lblTitle = new Label
            {
                Text = "XÁC ĐỊNH TÀI KHOẢN",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                AutoSize = true
            };
            pnlStep1.Controls.Add(lblTitle);

            Label lblDesc = new Label
            {
                Text = "Nhập tên đăng nhập hoặc email đã đăng ký.",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = ColTextSecondary,
                AutoSize = true
            };
            pnlStep1.Controls.Add(lblDesc);

            Label lblInputHeader = new Label
            {
                Text = "Tên đăng nhập hoặc Email *",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                AutoSize = true
            };
            pnlStep1.Controls.Add(lblInputHeader);

            txtIdentifier = new ModernInputBox
            {
                Placeholder = "Nhập tên đăng nhập hoặc email",
                LeftIcon = SFIcon.User
            };
            txtIdentifier.EnterPressed += (s, e) => HandleIdentifyNext();
            pnlStep1.Controls.Add(txtIdentifier);

            lblIdentifyError = new Label
            {
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = ColDanger,
                Text = "",
                AutoSize = true
            };
            pnlStep1.Controls.Add(lblIdentifyError);

            // Information hint card
            pnlHint1 = new Panel
            {
                Height = 64,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlHint1.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, pnlHint1.Width - 1, pnlHint1.Height - 1);
                using (var path = GraphicsUtility.CreateRoundedRectangle(r, 8))
                using (var pen = new Pen(ColBorder, 1f))
                {
                    g.DrawPath(pen, path);
                }
                VectorIcons.Draw(g, SFIcon.Info, new Rectangle(14, 12, 20, 20), ColPrimary, 2f);
                using (var font = new Font("Segoe UI", 9f, FontStyle.Regular))
                {
                    Rectangle textRect = new Rectangle(42, 10, pnlHint1.Width - 54, pnlHint1.Height - 16);
                    TextRenderer.DrawText(g, "Mã xác minh OTP 6 số sẽ được gửi trực tiếp đến địa chỉ email bảo mật đã đăng ký qua hệ thống Gmail SMTP của SPORTFIELD.", font, textRect, ColTextSecondary, TextFormatFlags.WordBreak);
                }
            };
            pnlStep1.Controls.Add(pnlHint1);

            btnIdentifyNext = new ModernButton
            {
                Text = "TIẾP TỤC →",
                Size = new Size(190, 44)
            };
            btnIdentifyNext.Click += (s, e) => HandleIdentifyNext();
            pnlStep1.Controls.Add(btnIdentifyNext);

            // Dynamic layout for Step 1
            pnlStep1.Resize += (s, e) =>
            {
                int padX = 32;
                int contentW = Math.Max(260, pnlStep1.Width - padX * 2);

                lblTitle.Location = new Point(padX, 16);
                lblDesc.Location = new Point(padX, 44);
                lblInputHeader.Location = new Point(padX, 82);

                txtIdentifier.SetBounds(padX, 106, contentW, 46);
                lblIdentifyError.Location = new Point(padX, 156);

                pnlHint1.SetBounds(padX, 184, contentW, 64);
                pnlHint1.Invalidate();

                btnIdentifyNext.Location = new Point(padX + (contentW - btnIdentifyNext.Width) / 2, 276);
            };

            cardMain.Controls.Add(pnlStep1);
        }

        private void HandleIdentifyNext()
        {
            string idText = txtIdentifier.Text.Trim();
            lblIdentifyError.Text = "";
            txtIdentifier.HasError = false;

            if (string.IsNullOrEmpty(idText))
            {
                txtIdentifier.HasError = true;
                lblIdentifyError.Text = "Vui lòng nhập tên đăng nhập hoặc email.";
                txtIdentifier.Focus();
                return;
            }

            btnIdentifyNext.Enabled = false;
            btnIdentifyNext.Text = "ĐANG TÌM KIẾM...";
            try
            {
                bool found = PasswordRecoveryService.Instance.FindAccount(idText, out _currentAccount, out string message);
                if (!found)
                {
                    // Thông báo trung tính bảo mật
                    lblIdentifyError.Text = message;
                    txtIdentifier.HasError = true;
                    return;
                }

                // Nếu tài khoản có email hợp lệ -> Tự động kích hoạt gửi mã OTP qua Email
                if (!string.IsNullOrWhiteSpace(_currentAccount.Email))
                {
                    _selectedChannel = "EMAIL";
                    SendOtpAndProceed();
                }
                else if (_currentAccount.IsStaff)
                {
                    // Nhân viên không có email -> Cho phép gửi yêu cầu Quản trị viên
                    ShowPanel(pnlAdminAssistance);
                }
                else
                {
                    txtIdentifier.HasError = true;
                    lblIdentifyError.Text = "Tài khoản chưa cập nhật email nhận mã OTP. Vui lòng liên hệ quầy tiếp tân để được hỗ trợ.";
                }
            }
            finally
            {
                btnIdentifyNext.Enabled = true;
                btnIdentifyNext.Text = "TIẾP TỤC →";
            }
        }

        private void SendOtpAndProceed()
        {
            btnIdentifyNext.Enabled = false;
            btnIdentifyNext.Text = "ĐANG GỬI MÃ...";
            try
            {
                bool ok = PasswordRecoveryService.Instance.SendOtp(
                    _currentAccount,
                    _selectedChannel,
                    out _currentRequestId,
                    out string err,
                    out int cooldown,
                    out _);

                if (!ok)
                {
                    txtIdentifier.HasError = true;
                    lblIdentifyError.Text = err;
                    return;
                }

                // Cập nhật Masked Email Badge ở Step 2
                pnlEmailBadge.MaskedEmail = _currentAccount.MaskedEmail;
                otpBox.Clear();
                lblOtpError.Text = "";

                // Khởi động đồng hồ đếm ngược
                StartOtpTimers();

                // Chuyển sang Bước 2
                ShowStep(2);
                otpBox.Focus();

                SFToast.Show(this, "Mã xác minh 6 số đã được gửi đến email của bạn.", "ĐÃ GỬI MÃ", SFIcon.Check, ColSuccess);
            }
            finally
            {
                btnIdentifyNext.Enabled = true;
                btnIdentifyNext.Text = "TIẾP TỤC →";
            }
        }

        // ==========================================
        // STEP 2: XÁC MINH OTP
        // ==========================================
        private void BuildStep2Otp()
        {
            pnlStep2 = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColCard,
                Visible = false
            };

            Label lblTitle = new Label
            {
                Text = "XÁC MINH EMAIL",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                AutoSize = true
            };
            pnlStep2.Controls.Add(lblTitle);

            Label lblDesc = new Label
            {
                Text = "Mã xác minh gồm 6 chữ số đã được gửi đến địa chỉ email:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = ColTextSecondary,
                AutoSize = true
            };
            pnlStep2.Controls.Add(lblDesc);

            // Masked Email Badge Container (True RoundedPanel)
            pnlEmailBadge = new EmailBadgePanel
            {
                Size = new Size(290, 36)
            };
            pnlStep2.Controls.Add(pnlEmailBadge);

            // 6-digit Interactive Box Control
            otpBox = new OtpBoxControl
            {
                Size = new Size(338, 54)
            };
            otpBox.OtpCompleted += (s, e) => HandleOtpVerify();
            otpBox.EnterPressed += (s, e) => HandleOtpVerify();
            pnlStep2.Controls.Add(otpBox);

            lblOtpError = new Label
            {
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = ColDanger,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "",
                AutoSize = false
            };
            pnlStep2.Controls.Add(lblOtpError);

            // Countdown Timer Label
            lblOtpCountdown = new Label
            {
                Text = "Mã còn hiệu lực trong: 05:00",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };
            pnlStep2.Controls.Add(lblOtpCountdown);

            // Resend Code Link
            lblOtpResendLink = new Label
            {
                Text = "Không nhận được mã? Gửi lại sau (60s)",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = ColTextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Default,
                AutoSize = false
            };
            lblOtpResendLink.Click += (s, e) =>
            {
                if (_resendCooldown <= 0)
                {
                    HandleResendOtp();
                }
            };
            pnlStep2.Controls.Add(lblOtpResendLink);

            btnOtpVerify = new ModernButton
            {
                Text = "XÁC MINH",
                Size = new Size(190, 44)
            };
            btnOtpVerify.Click += (s, e) => HandleOtpVerify();
            pnlStep2.Controls.Add(btnOtpVerify);

            lnkChangeAccount = new Label
            {
                Text = "← Đổi tài khoản khác",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColTextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                AutoSize = false
            };
            lnkChangeAccount.MouseEnter += (s, e) => lnkChangeAccount.ForeColor = ColPrimary;
            lnkChangeAccount.MouseLeave += (s, e) => lnkChangeAccount.ForeColor = ColTextSecondary;
            lnkChangeAccount.Click += (s, e) =>
            {
                StopOtpTimers();
                ShowStep(1);
            };
            pnlStep2.Controls.Add(lnkChangeAccount);

            // Dynamic Layout for Step 2
            pnlStep2.Resize += (s, e) =>
            {
                int padX = 32;
                int contentW = Math.Max(260, pnlStep2.Width - padX * 2);

                lblTitle.Location = new Point(padX, 14);
                lblDesc.Location = new Point(padX, 42);

                pnlEmailBadge.Location = new Point(padX + (contentW - pnlEmailBadge.Width) / 2, 70);
                otpBox.Location = new Point(padX + (contentW - otpBox.Width) / 2, 116);

                lblOtpError.SetBounds(padX, 176, contentW, 20);
                lblOtpCountdown.SetBounds(padX, 198, contentW, 22);
                lblOtpResendLink.SetBounds(padX, 222, contentW, 22);

                btnOtpVerify.Location = new Point(padX + (contentW - btnOtpVerify.Width) / 2, 256);
                lnkChangeAccount.SetBounds(padX, 310, contentW, 22);
            };

            cardMain.Controls.Add(pnlStep2);
        }

        private void StartOtpTimers()
        {
            StopOtpTimers();

            _otpTimeRemaining = 300; // 5 phút
            _resendCooldown = 60;   // 60 giây

            _timerOtp = new Timer { Interval = 1000 };
            _timerOtp.Tick += (s, e) =>
            {
                _otpTimeRemaining--;
                if (_otpTimeRemaining <= 0)
                {
                    _timerOtp.Stop();
                    lblOtpCountdown.Text = "Mã xác minh đã hết hạn. Vui lòng gửi lại mã mới.";
                    lblOtpCountdown.ForeColor = ColDanger;
                }
                else
                {
                    int mm = _otpTimeRemaining / 60;
                    int ss = _otpTimeRemaining % 60;
                    lblOtpCountdown.Text = $"Mã còn hiệu lực trong: {mm:D2}:{ss:D2}";
                    lblOtpCountdown.ForeColor = _otpTimeRemaining < 60 ? ColAmber : ColPrimary;
                }
            };
            _timerOtp.Start();

            _timerResend = new Timer { Interval = 1000 };
            _timerResend.Tick += (s, e) =>
            {
                _resendCooldown--;
                if (_resendCooldown <= 0)
                {
                    _timerResend.Stop();
                    lblOtpResendLink.Text = "Gửi lại mã xác minh";
                    lblOtpResendLink.ForeColor = ColPrimary;
                    lblOtpResendLink.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                    lblOtpResendLink.Cursor = Cursors.Hand;
                }
                else
                {
                    lblOtpResendLink.Text = $"Không nhận được mã? Gửi lại sau ({_resendCooldown}s)";
                    lblOtpResendLink.ForeColor = ColTextSecondary;
                    lblOtpResendLink.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
                    lblOtpResendLink.Cursor = Cursors.Default;
                }
            };
            _timerResend.Start();
        }

        private void StopOtpTimers()
        {
            if (_timerOtp != null)
            {
                _timerOtp.Stop();
                _timerOtp.Dispose();
                _timerOtp = null;
            }
            if (_timerResend != null)
            {
                _timerResend.Stop();
                _timerResend.Dispose();
                _timerResend = null;
            }
        }

        private void HandleResendOtp()
        {
            if (_currentAccount == null) return;

            lblOtpResendLink.Text = "Đang gửi lại...";
            lblOtpResendLink.Cursor = Cursors.Default;

            bool ok = PasswordRecoveryService.Instance.SendOtp(
                _currentAccount,
                _selectedChannel,
                out _currentRequestId,
                out string err,
                out _,
                out _);

            if (!ok)
            {
                lblOtpError.Text = err;
                lblOtpResendLink.Text = "Gửi lại mã xác minh";
                lblOtpResendLink.Cursor = Cursors.Hand;
                return;
            }

            StartOtpTimers();
            otpBox.Clear();
            lblOtpError.Text = "";
            otpBox.Focus();
            SFToast.Show(this, "Đã gửi lại mã OTP mới đến hộp thư của bạn.", "GỬI LẠI MÃ", SFIcon.Check, ColSuccess);
        }

        private void HandleOtpVerify()
        {
            string otp = otpBox.OtpCode.Trim();
            lblOtpError.Text = "";

            if (string.IsNullOrEmpty(otp) || otp.Length != 6)
            {
                lblOtpError.Text = "Vui lòng nhập đầy đủ 6 chữ số mã xác minh.";
                otpBox.Focus();
                return;
            }

            btnOtpVerify.Enabled = false;
            btnOtpVerify.Text = "ĐANG XÁC MINH...";
            try
            {
                bool ok = PasswordRecoveryService.Instance.VerifyOtp(_currentRequestId, otp, out string err);
                if (!ok)
                {
                    lblOtpError.Text = err;
                    otpBox.Clear();
                    otpBox.Focus();
                    return;
                }

                // Dừng đồng hồ
                StopOtpTimers();

                // Chuyển sang Bước 3: Đặt mật khẩu mới
                ShowStep(3);
                txtNewPassword.Focus();
            }
            finally
            {
                btnOtpVerify.Enabled = true;
                btnOtpVerify.Text = "XÁC MINH";
            }
        }

        // ==========================================
        // STEP 3: TẠO MẬT KHẨU MỚI
        // ==========================================
        private void BuildStep3ResetPassword()
        {
            pnlStep3 = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColCard,
                Visible = false
            };

            Label lblTitle = new Label
            {
                Text = "TẠO MẬT KHẨU MỚI",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                AutoSize = true
            };
            pnlStep3.Controls.Add(lblTitle);

            Label lblDesc = new Label
            {
                Text = "Mật khẩu mới sẽ được dùng cho lần đăng nhập tiếp theo.",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = ColTextSecondary,
                AutoSize = true
            };
            pnlStep3.Controls.Add(lblDesc);

            Label lblNewPwd = new Label
            {
                Text = "Mật khẩu mới *",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                AutoSize = true
            };
            pnlStep3.Controls.Add(lblNewPwd);

            txtNewPassword = new ModernInputBox
            {
                Placeholder = "Nhập mật khẩu mới an toàn",
                LeftIcon = SFIcon.Lock,
                IsPassword = true,
                ShowEyeToggle = true
            };
            txtNewPassword.TextChangedEvent += (s, e) => ValidatePasswordRequirements();
            pnlStep3.Controls.Add(txtNewPassword);

            Label lblConfirmPwd = new Label
            {
                Text = "Xác nhận mật khẩu mới *",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                AutoSize = true
            };
            pnlStep3.Controls.Add(lblConfirmPwd);

            txtConfirmPassword = new ModernInputBox
            {
                Placeholder = "Nhập lại mật khẩu mới để xác nhận",
                LeftIcon = SFIcon.Lock,
                IsPassword = true,
                ShowEyeToggle = true
            };
            txtConfirmPassword.TextChangedEvent += (s, e) => ValidatePasswordRequirements();
            txtConfirmPassword.EnterPressed += (s, e) => HandleResetPasswordSubmit();
            pnlStep3.Controls.Add(txtConfirmPassword);

            // Requirements Checklist Container
            pnlRequirements = new Panel
            {
                Height = 36,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlRequirements.Paint += (s, e) =>
            {
                using (var pen = new Pen(ColBorder, 1f))
                using (var path = GraphicsUtility.CreateRoundedRectangle(new Rectangle(0, 0, pnlRequirements.Width - 1, pnlRequirements.Height - 1), 6))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
                }
            };

            lblReqLength = new Label
            {
                Text = "○ Ít nhất 8 ký tự",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ColTextMuted,
                AutoSize = true
            };
            pnlRequirements.Controls.Add(lblReqLength);

            lblReqLetterDigit = new Label
            {
                Text = "○ Bao gồm chữ & số",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ColTextMuted,
                AutoSize = true
            };
            pnlRequirements.Controls.Add(lblReqLetterDigit);

            lblReqMatch = new Label
            {
                Text = "○ Khớp mật khẩu xác nhận",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ColTextMuted,
                AutoSize = true
            };
            pnlRequirements.Controls.Add(lblReqMatch);

            pnlStep3.Controls.Add(pnlRequirements);

            lblResetError = new Label
            {
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = ColDanger,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "",
                AutoSize = false
            };
            pnlStep3.Controls.Add(lblResetError);

            btnResetSubmit = new ModernButton
            {
                Text = "ĐẶT LẠI MẬT KHẨU",
                Size = new Size(220, 44)
            };
            btnResetSubmit.Click += (s, e) => HandleResetPasswordSubmit();
            pnlStep3.Controls.Add(btnResetSubmit);

            // Dynamic Layout for Step 3
            pnlStep3.Resize += (s, e) =>
            {
                int padX = 32;
                int contentW = Math.Max(260, pnlStep3.Width - padX * 2);

                lblTitle.Location = new Point(padX, 10);
                lblDesc.Location = new Point(padX, 36);

                lblNewPwd.Location = new Point(padX, 64);
                txtNewPassword.SetBounds(padX, 86, contentW, 44);

                lblConfirmPwd.Location = new Point(padX, 136);
                txtConfirmPassword.SetBounds(padX, 158, contentW, 44);

                pnlRequirements.SetBounds(padX, 210, contentW, 36);

                // Evenly distribute 3 requirements
                int reqColW = contentW / 3;
                lblReqLength.Location = new Point(12, 9);
                lblReqLetterDigit.Location = new Point(reqColW + 10, 9);
                lblReqMatch.Location = new Point(reqColW * 2 + 8, 9);

                lblResetError.SetBounds(padX, 252, contentW, 20);
                btnResetSubmit.Location = new Point(padX + (contentW - btnResetSubmit.Width) / 2, 276);
            };

            cardMain.Controls.Add(pnlStep3);
        }

        private void ValidatePasswordRequirements()
        {
            string p1 = txtNewPassword.Text;
            string p2 = txtConfirmPassword.Text;

            // 1. Min length
            bool lenOk = p1.Length >= 8;
            lblReqLength.ForeColor = lenOk ? ColSuccess : ColTextMuted;
            lblReqLength.Text = lenOk ? "✓ Ít nhất 8 ký tự" : "○ Ít nhất 8 ký tự";

            // 2. Letters & digits
            bool letterDigitOk = p1.Any(char.IsLetter) && p1.Any(char.IsDigit);
            lblReqLetterDigit.ForeColor = letterDigitOk ? ColSuccess : ColTextMuted;
            lblReqLetterDigit.Text = letterDigitOk ? "✓ Bao gồm chữ & số" : "○ Bao gồm chữ & số";

            // 3. Match
            bool matchOk = !string.IsNullOrEmpty(p2) && p1 == p2;
            lblReqMatch.ForeColor = matchOk ? ColSuccess : ColTextMuted;
            lblReqMatch.Text = matchOk ? "✓ Khớp xác nhận" : "○ Khớp xác nhận";
        }

        private void HandleResetPasswordSubmit()
        {
            string newPwd = txtNewPassword.Text;
            string confirmPwd = txtConfirmPassword.Text;
            lblResetError.Text = "";
            txtNewPassword.HasError = false;
            txtConfirmPassword.HasError = false;

            if (string.IsNullOrEmpty(newPwd) || string.IsNullOrEmpty(confirmPwd))
            {
                lblResetError.Text = "Vui lòng nhập đầy đủ mật khẩu mới và xác nhận mật khẩu.";
                return;
            }

            if (newPwd != confirmPwd)
            {
                txtConfirmPassword.HasError = true;
                lblResetError.Text = "Mật khẩu xác nhận không khớp với mật khẩu mới.";
                txtConfirmPassword.Focus();
                return;
            }

            btnResetSubmit.Enabled = false;
            btnResetSubmit.Text = "ĐANG CẬP NHẬT...";
            try
            {
                bool ok = PasswordRecoveryService.Instance.ResetPassword(
                    _currentRequestId,
                    _currentAccount.TaiKhoanID,
                    newPwd,
                    confirmPwd,
                    out string err);

                if (!ok)
                {
                    lblResetError.Text = err;
                    return;
                }

                RecoveredUsername = _currentAccount.TenDangNhap;
                ShowStep(4);
            }
            finally
            {
                btnResetSubmit.Enabled = true;
                btnResetSubmit.Text = "ĐẶT LẠI MẬT KHẨU";
            }
        }

        // ==========================================
        // STEP 4: THÀNH CÔNG (SUCCESS)
        // ==========================================
        private void BuildStepSuccess()
        {
            pnlSuccess = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColCard,
                Visible = false
            };

            // Success Checkmark Badge (56x56)
            pbSuccess = new PictureBox
            {
                Size = new Size(56, 56),
                BackColor = Color.Transparent
            };
            pbSuccess.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(ColSuccessBg))
                {
                    g.FillEllipse(brush, 0, 0, 55, 55);
                }
                VectorIcons.Draw(g, SFIcon.Check, new Rectangle(14, 14, 28, 28), ColSuccess, 3.2f);
            };
            pnlSuccess.Controls.Add(pbSuccess);

            lblSuccessTitle = new Label
            {
                Text = "ĐẶT LẠI MẬT KHẨU THÀNH CÔNG",
                Font = new Font("Segoe UI", 15.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };
            pnlSuccess.Controls.Add(lblSuccessTitle);

            lblSuccessDesc = new Label
            {
                Text = "Mật khẩu của bạn đã được cập nhật thành công vào hệ thống.\nBạn có thể đăng nhập ngay bây giờ bằng mật khẩu mới vừa tạo.",
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ColTextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };
            pnlSuccess.Controls.Add(lblSuccessDesc);

            btnSuccessBackToLogin = new ModernButton
            {
                Text = "VỀ ĐĂNG NHẬP",
                Size = new Size(220, 44)
            };
            btnSuccessBackToLogin.Click += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            pnlSuccess.Controls.Add(btnSuccessBackToLogin);

            pnlSuccess.Resize += (s, e) =>
            {
                int padX = 32;
                int contentW = Math.Max(260, pnlSuccess.Width - padX * 2);

                pbSuccess.Location = new Point(padX + (contentW - pbSuccess.Width) / 2, 42);
                lblSuccessTitle.SetBounds(padX, 116, contentW, 32);
                lblSuccessDesc.SetBounds(padX, 156, contentW, 48);
                btnSuccessBackToLogin.Location = new Point(padX + (contentW - btnSuccessBackToLogin.Width) / 2, 236);
            };

            cardMain.Controls.Add(pnlSuccess);
        }

        // ==========================================
        // STAFF ADMIN ASSISTANCE PANEL
        // ==========================================
        private void BuildStepAdminAssistance()
        {
            pnlAdminAssistance = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColCard,
                Visible = false
            };

            Label lblTitle = new Label
            {
                Text = "YÊU CẦU QUẢN TRỊ VIÊN HỖ TRỢ",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                AutoSize = true
            };
            pnlAdminAssistance.Controls.Add(lblTitle);

            Label lblDesc = new Label
            {
                Text = "Dành riêng cho Nhân viên cơ sở không còn quyền truy cập email hoặc SĐT.",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = ColTextSecondary,
                AutoSize = true
            };
            pnlAdminAssistance.Controls.Add(lblDesc);

            Label lblNoteTitle = new Label
            {
                Text = "Lý do mất quyền truy cập phương thức xác thực *",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColTextPrimary,
                AutoSize = true
            };
            pnlAdminAssistance.Controls.Add(lblNoteTitle);

            pnlNoteBox = new Panel
            {
                Height = 96,
                BackColor = Color.White
            };
            pnlNoteBox.Paint += (s, e) =>
            {
                using (var pen = new Pen(ColBorderInput, 1.2f))
                using (var path = GraphicsUtility.CreateRoundedRectangle(new Rectangle(0, 0, pnlNoteBox.Width - 1, pnlNoteBox.Height - 1), 8))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
                }
            };

            txtAdminNote = new TextBox
            {
                Multiline = true,
                BorderStyle = BorderStyle.None,
                Location = new Point(10, 10),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = ColTextPrimary,
                Text = "Tôi bị mất điện thoại và không thể nhận mã OTP. Kính đề nghị quản trị viên hỗ trợ cấp mật khẩu tạm thời."
            };
            pnlNoteBox.Controls.Add(txtAdminNote);
            pnlAdminAssistance.Controls.Add(pnlNoteBox);

            lblAdminReqError = new Label
            {
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = ColDanger,
                Text = "",
                AutoSize = true
            };
            pnlAdminAssistance.Controls.Add(lblAdminReqError);

            btnSubmitAdminReq = new ModernButton
            {
                Text = "GỬI YÊU CẦU HỖ TRỢ",
                Size = new Size(220, 44)
            };
            btnSubmitAdminReq.Click += (s, e) => HandleAdminAssistanceSubmit();
            pnlAdminAssistance.Controls.Add(btnSubmitAdminReq);

            lnkAdminBack = new Label
            {
                Text = "← Quay lại lựa chọn",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ColTextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                AutoSize = false
            };
            lnkAdminBack.MouseEnter += (s, e) => lnkAdminBack.ForeColor = ColPrimary;
            lnkAdminBack.MouseLeave += (s, e) => lnkAdminBack.ForeColor = ColTextSecondary;
            lnkAdminBack.Click += (s, e) => ShowStep(1);
            pnlAdminAssistance.Controls.Add(lnkAdminBack);

            pnlAdminAssistance.Resize += (s, e) =>
            {
                int padX = 32;
                int contentW = Math.Max(260, pnlAdminAssistance.Width - padX * 2);

                lblTitle.Location = new Point(padX, 14);
                lblDesc.Location = new Point(padX, 42);
                lblNoteTitle.Location = new Point(padX, 76);

                pnlNoteBox.SetBounds(padX, 100, contentW, 96);
                txtAdminNote.SetBounds(10, 10, contentW - 20, 76);

                lblAdminReqError.Location = new Point(padX, 204);
                btnSubmitAdminReq.Location = new Point(padX + (contentW - btnSubmitAdminReq.Width) / 2, 234);
                lnkAdminBack.SetBounds(padX, 290, contentW, 22);
            };

            cardMain.Controls.Add(pnlAdminAssistance);
        }

        private void HandleAdminAssistanceSubmit()
        {
            lblAdminReqError.Text = "";
            btnSubmitAdminReq.Enabled = false;
            btnSubmitAdminReq.Text = "ĐANG GỬI...";
            try
            {
                bool ok = PasswordRecoveryService.Instance.RequestAdminAssistance(_currentAccount, txtAdminNote.Text.Trim(), out string err);
                if (!ok)
                {
                    lblAdminReqError.Text = err;
                    return;
                }

                SFToast.Show(this, "Yêu cầu khôi phục đã được gửi tới Trung tâm Thông báo của Quản trị viên.", "ĐÃ GỬI YÊU CẦU", SFIcon.Check, ColSuccess);
                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                btnSubmitAdminReq.Enabled = true;
                btnSubmitAdminReq.Text = "GỬI YÊU CẦU HỖ TRỢ";
            }
        }

        // ==========================================
        // STEP & PANEL TRANSITION
        // ==========================================
        private void ShowStep(int stepIndex)
        {
            _currentStepIndex = stepIndex;
            stepper.CurrentStep = Math.Min(stepIndex, 3);

            switch (stepIndex)
            {
                case 1:
                    ShowPanel(pnlStep1);
                    txtIdentifier.Focus();
                    break;
                case 2:
                    ShowPanel(pnlStep2);
                    otpBox.Focus();
                    break;
                case 3:
                    ShowPanel(pnlStep3);
                    txtNewPassword.Focus();
                    break;
                case 4:
                    ShowPanel(pnlSuccess);
                    btnSuccessBackToLogin.Focus();
                    break;
            }
        }

        private void ShowPanel(Panel active)
        {
            pnlStep1.Visible = (active == pnlStep1);
            pnlStep2.Visible = (active == pnlStep2);
            pnlStep3.Visible = (active == pnlStep3);
            pnlSuccess.Visible = (active == pnlSuccess);
            pnlAdminAssistance.Visible = (active == pnlAdminAssistance);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopOtpTimers();
            base.OnFormClosed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopOtpTimers();
            }
            base.Dispose(disposing);
        }

        // =========================================================================
        // NESTED HELPER CONTROLS (PIXEL-PERFECT, HIGH-DPI RESILIENT & ISOLATED)
        // =========================================================================

        #region Helper Controls

        /// <summary>
        /// White card panel with smooth 14px rounded rectangle and subtle border
        /// </summary>
        private class CardPanel : Panel
        {
            public CardPanel()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = GraphicsUtility.CreateRoundedRectangle(r, 14))
                {
                    using (var b = new SolidBrush(Color.White))
                    {
                        g.FillPath(b, path);
                    }
                    using (var p = new Pen(ColBorder, 1.2f))
                    {
                        g.DrawPath(p, path);
                    }
                }
            }
        }

        /// <summary>
        /// Responsive 3-step Stepper control calculating nodes dynamically from Width
        /// </summary>
        private class StepperControl : Control
        {
            private int _currentStep = 1;
            public int CurrentStep
            {
                get => _currentStep;
                set { _currentStep = value; Invalidate(); }
            }

            public StepperControl()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                DoubleBuffered = true;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                string[] titles = { "Tài khoản", "Xác minh", "Mật khẩu mới" };

                // Calculate centers dynamically based on container width
                int colW = Width / 3;
                int[] xCenters = { colW / 2, colW + colW / 2, colW * 2 + colW / 2 };
                int cy = Height / 2;
                int radius = 15;

                // 1. Draw connecting lines between circles
                for (int i = 0; i < 2; i++)
                {
                    int xStart = xCenters[i] + radius + 10;
                    int xEnd = xCenters[i + 1] - radius - 10;
                    if (xStart < xEnd)
                    {
                        bool isLineActive = _currentStep > (i + 1);
                        using (var p = new Pen(isLineActive ? ColPrimary : ColBorder, 2f))
                        {
                            g.DrawLine(p, xStart, cy, xEnd, cy);
                        }
                    }
                }

                // 2. Draw nodes and labels
                for (int i = 0; i < 3; i++)
                {
                    int stepNum = i + 1;
                    int cx = xCenters[i];
                    bool isDone = _currentStep > stepNum;
                    bool isActive = _currentStep == stepNum;

                    // Measuring text to center circle + text as a visual group
                    FontStyle fStyle = isActive ? FontStyle.Bold : FontStyle.Regular;
                    Color txtCol = isActive ? ColPrimary : (isDone ? ColSuccess : ColTextMuted);
                    using (var fText = new Font("Segoe UI", 9.5f, fStyle))
                    {
                        Size szText = TextRenderer.MeasureText(titles[i], fText);
                        int totalGroupW = radius * 2 + 8 + szText.Width;
                        int groupStartX = cx - totalGroupW / 2;

                        Rectangle nodeCircle = new Rectangle(groupStartX, cy - radius, radius * 2, radius * 2);

                        if (isDone)
                        {
                            // Completed: Green circle with crisp checkmark
                            using (var b = new SolidBrush(ColSuccess))
                                g.FillEllipse(b, nodeCircle);
                            VectorIcons.Draw(g, SFIcon.Check, new Rectangle(groupStartX + 7, cy - 8, 16, 16), Color.White, 2.4f);
                        }
                        else if (isActive)
                        {
                            // Active: Teal circle with bold white number
                            using (var b = new SolidBrush(ColPrimary))
                                g.FillEllipse(b, nodeCircle);
                            using (var fNum = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                            {
                                Size szNum = TextRenderer.MeasureText(stepNum.ToString(), fNum);
                                int nx = nodeCircle.X + (nodeCircle.Width - szNum.Width) / 2 + 1;
                                int ny = nodeCircle.Y + (nodeCircle.Height - szNum.Height) / 2;
                                TextRenderer.DrawText(g, stepNum.ToString(), fNum, new Point(nx, ny), Color.White);
                            }
                        }
                        else
                        {
                            // Future: Subtle border circle with muted number
                            using (var b = new SolidBrush(Color.White))
                                g.FillEllipse(b, nodeCircle);
                            using (var p = new Pen(ColBorder, 1.5f))
                                g.DrawEllipse(p, nodeCircle);
                            using (var fNum = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                            {
                                Size szNum = TextRenderer.MeasureText(stepNum.ToString(), fNum);
                                int nx = nodeCircle.X + (nodeCircle.Width - szNum.Width) / 2 + 1;
                                int ny = nodeCircle.Y + (nodeCircle.Height - szNum.Height) / 2;
                                TextRenderer.DrawText(g, stepNum.ToString(), fNum, new Point(nx, ny), ColTextMuted);
                            }
                        }

                        // Step label text
                        int tx = nodeCircle.Right + 8;
                        int ty = cy - szText.Height / 2;
                        TextRenderer.DrawText(g, titles[i], fText, new Point(tx, ty), txtCol);
                    }
                }
            }
        }

        /// <summary>
        /// True rounded panel pill container for masked email address
        /// </summary>
        private class EmailBadgePanel : Panel
        {
            private string _maskedEmail = "va********@gmail.com";

            public string MaskedEmail
            {
                get => _maskedEmail;
                set { _maskedEmail = value; Invalidate(); }
            }

            public EmailBadgePanel()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                DoubleBuffered = true;
                BackColor = Color.Transparent;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
                int radius = Height / 2;
                using (var path = GraphicsUtility.CreateRoundedRectangle(r, radius))
                {
                    // Soft light green/teal background
                    using (var b = new SolidBrush(Color.FromArgb(236, 253, 245))) // #ECFDF5
                    {
                        g.FillPath(b, path);
                    }
                    using (var pen = new Pen(Color.FromArgb(167, 243, 208), 1.2f)) // #A7F3D0
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // Mail icon + Masked Email Text
                using (var font = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                {
                    Size sz = TextRenderer.MeasureText(_maskedEmail, font);
                    int totalW = 18 + 8 + sz.Width;
                    int startX = (Width - totalW) / 2;

                    VectorIcons.Draw(g, SFIcon.Check, new Rectangle(startX, (Height - 16) / 2, 16, 16), ColPrimary, 2f);
                    TextRenderer.DrawText(g, _maskedEmail, font, new Point(startX + 24, (Height - sz.Height) / 2), ColPrimary);
                }
            }
        }

        /// <summary>
        /// Interactive 6-digit OTP Box Control with individual boxes, active highlight, and paste support
        /// </summary>
        private class OtpBoxControl : Panel
        {
            private TextBox _innerTxt;
            public event EventHandler OtpCompleted;
            public event EventHandler EnterPressed;

            public string OtpCode
            {
                get => _innerTxt.Text;
                set { _innerTxt.Text = value; Invalidate(); }
            }

            public void Clear()
            {
                _innerTxt.Text = "";
                Invalidate();
            }

            public OtpBoxControl()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                DoubleBuffered = true;
                BackColor = Color.White;
                Cursor = Cursors.IBeam;

                // Hidden inner textbox for native focus, keyboard, and paste
                _innerTxt = new TextBox
                {
                    Location = new Point(-200, -200),
                    Size = new Size(100, 20),
                    MaxLength = 6
                };
                _innerTxt.TextChanged += (s, e) =>
                {
                    // Filter digits only
                    string filtered = new string(_innerTxt.Text.Where(char.IsDigit).Take(6).ToArray());
                    if (filtered != _innerTxt.Text)
                    {
                        _innerTxt.Text = filtered;
                        _innerTxt.SelectionStart = filtered.Length;
                    }
                    Invalidate();
                    if (_innerTxt.Text.Length == 6)
                    {
                        OtpCompleted?.Invoke(this, EventArgs.Empty);
                    }
                };
                _innerTxt.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true;
                        EnterPressed?.Invoke(this, EventArgs.Empty);
                    }
                };
                _innerTxt.GotFocus += (s, e) => Invalidate();
                _innerTxt.LostFocus += (s, e) => Invalidate();
                Controls.Add(_innerTxt);

                Click += (s, e) => _innerTxt.Focus();
            }

            public new void Focus()
            {
                _innerTxt.Focus();
                _innerTxt.SelectAll();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int boxWidth = 48;
                int boxHeight = 52;
                int gap = 10;
                string text = _innerTxt.Text;
                bool isFocused = _innerTxt.Focused;

                for (int i = 0; i < 6; i++)
                {
                    int x = i * (boxWidth + gap);
                    Rectangle r = new Rectangle(x, 1, boxWidth, boxHeight);

                    bool isCurrentBox = isFocused && (i == text.Length || (text.Length == 6 && i == 5));
                    bool isFilled = i < text.Length;

                    using (var path = GraphicsUtility.CreateRoundedRectangle(r, 8))
                    {
                        // Background
                        using (var bgBrush = new SolidBrush(isCurrentBox ? ColTealLight : Color.White))
                        {
                            g.FillPath(bgBrush, path);
                        }

                        // Border
                        Color borderCol = isCurrentBox ? ColPrimary : (isFilled ? ColPrimary : ColBorderInput);
                        float stroke = isCurrentBox ? 2f : 1.2f;
                        using (var pen = new Pen(borderCol, stroke))
                        {
                            g.DrawPath(pen, path);
                        }
                    }

                    // Digit
                    if (isFilled)
                    {
                        using (var fDigit = new Font("Segoe UI", 21f, FontStyle.Bold))
                        {
                            string d = text[i].ToString();
                            Size sz = TextRenderer.MeasureText(d, fDigit);
                            int tx = x + (boxWidth - sz.Width) / 2 + 1;
                            int ty = (boxHeight - sz.Height) / 2 + 2;
                            TextRenderer.DrawText(g, d, fDigit, new Point(tx, ty), ColTextPrimary);
                        }
                    }
                    else if (isCurrentBox && text.Length < 6)
                    {
                        // Active cursor indicator line
                        using (var pCursor = new Pen(ColPrimary, 2.5f))
                        {
                            g.DrawLine(pCursor, x + 16, boxHeight - 12, x + boxWidth - 16, boxHeight - 12);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Modern flat input box with rounded border, vector icon, eye toggle, and Win32 cue banner
        /// </summary>
        private class ModernInputBox : Panel
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
            private const int EM_SETCUEBANNER = 0x1501;

            private TextBox _innerTxt;
            private bool _isFocused = false;
            private bool _hasError = false;
            private bool _isPassword = false;
            private bool _showEyeToggle = false;
            private SFIcon _leftIcon = SFIcon.None;
            private string _placeholder = "";
            private Panel _btnEye;

            public event EventHandler TextChangedEvent;
            public event EventHandler EnterPressed;

            public override string Text
            {
                get => _innerTxt.Text;
                set { _innerTxt.Text = value; Invalidate(); }
            }

            public bool HasError
            {
                get => _hasError;
                set { _hasError = value; Invalidate(); }
            }

            public string Placeholder
            {
                get => _placeholder;
                set
                {
                    _placeholder = value;
                    ApplyCueBanner();
                }
            }

            public SFIcon LeftIcon
            {
                get => _leftIcon;
                set { _leftIcon = value; Invalidate(); }
            }

            public bool IsPassword
            {
                get => _isPassword;
                set
                {
                    _isPassword = value;
                    _innerTxt.UseSystemPasswordChar = value;
                }
            }

            public bool ShowEyeToggle
            {
                get => _showEyeToggle;
                set
                {
                    _showEyeToggle = value;
                    if (_btnEye != null) _btnEye.Visible = value;
                    AdjustInnerBoxWidth();
                }
            }

            public ModernInputBox()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                DoubleBuffered = true;
                BackColor = Color.White;
                Height = 46;
                Cursor = Cursors.IBeam;

                _innerTxt = new TextBox
                {
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                    ForeColor = ColTextPrimary,
                    BackColor = Color.White,
                    Location = new Point(44, 13),
                    Width = 480
                };

                _innerTxt.HandleCreated += (s, e) => ApplyCueBanner();
                _innerTxt.GotFocus += (s, e) => { _isFocused = true; Invalidate(); };
                _innerTxt.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };
                _innerTxt.TextChanged += (s, e) =>
                {
                    Invalidate();
                    TextChangedEvent?.Invoke(this, EventArgs.Empty);
                };
                _innerTxt.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true;
                        EnterPressed?.Invoke(this, EventArgs.Empty);
                    }
                };
                Controls.Add(_innerTxt);

                // Eye Toggle Button
                _btnEye = new Panel
                {
                    Size = new Size(36, 36),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand,
                    Visible = false
                };
                _btnEye.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    Color iconCol = _innerTxt.UseSystemPasswordChar ? ColTextSecondary : ColPrimary;
                    VectorIcons.Draw(g, SFIcon.Eye, new Rectangle(6, 6, 24, 24), iconCol, 1.8f);
                    if (!_innerTxt.UseSystemPasswordChar)
                    {
                        using (var slash = new Pen(ColDanger, 2f))
                        {
                            g.DrawLine(slash, 8, 28, 28, 8);
                        }
                    }
                };
                _btnEye.Click += (s, e) =>
                {
                    _innerTxt.UseSystemPasswordChar = !_innerTxt.UseSystemPasswordChar;
                    _btnEye.Invalidate();
                };
                Controls.Add(_btnEye);

                Click += (s, e) => _innerTxt.Focus();
                Resize += (s, e) => AdjustInnerBoxWidth();
            }

            private void ApplyCueBanner()
            {
                if (_innerTxt != null && _innerTxt.IsHandleCreated && !string.IsNullOrEmpty(_placeholder))
                {
                    SendMessage(_innerTxt.Handle, EM_SETCUEBANNER, (IntPtr)1, _placeholder);
                }
            }

            private void AdjustInnerBoxWidth()
            {
                int left = _leftIcon != SFIcon.None ? 44 : 14;
                _innerTxt.Location = new Point(left, 13);
                int rightPad = _showEyeToggle ? 48 : 14;
                _innerTxt.Width = Math.Max(50, Width - left - rightPad);

                if (_btnEye != null)
                {
                    _btnEye.Location = new Point(Width - 42, (Height - 36) / 2);
                }
            }

            public new void Focus() => _innerTxt.Focus();

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = GraphicsUtility.CreateRoundedRectangle(r, 8))
                {
                    // Border color determination
                    Color bCol = _hasError ? ColDanger : (_isFocused ? ColPrimary : ColBorderInput);
                    float stroke = (_isFocused || _hasError) ? 1.8f : 1.2f;
                    using (var pen = new Pen(bCol, stroke))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // Left icon
                if (_leftIcon != SFIcon.None)
                {
                    Color iCol = _hasError ? ColDanger : (_isFocused ? ColPrimary : ColTextSecondary);
                    VectorIcons.Draw(g, _leftIcon, new Rectangle(14, (Height - 20) / 2, 20, 20), iCol, 1.8f);
                }
            }
        }

        /// <summary>
        /// Premium flat action button with rounded corners, hover states, and smooth typography
        /// </summary>
        private class ModernButton : Control
        {
            private bool _isHover = false;
            private bool _isPressed = false;

            public ModernButton()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
                DoubleBuffered = true;
                Cursor = Cursors.Hand;
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            }

            protected override void OnMouseEnter(EventArgs e) { _isHover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _isHover = false; _isPressed = false; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnMouseDown(MouseEventArgs e) { _isPressed = true; Invalidate(); base.OnMouseDown(e); }
            protected override void OnMouseUp(MouseEventArgs e) { _isPressed = false; Invalidate(); base.OnMouseUp(e); }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = GraphicsUtility.CreateRoundedRectangle(r, 8))
                {
                    Color bgCol = !Enabled ? Color.FromArgb(148, 163, 184) :
                                  (_isPressed ? ColPrimaryActive :
                                  (_isHover ? ColPrimaryHover : ColPrimary));

                    using (var brush = new SolidBrush(bgCol))
                    {
                        g.FillPath(brush, path);
                    }
                }

                // Centered text
                Size sz = TextRenderer.MeasureText(Text, Font);
                int tx = (Width - sz.Width) / 2;
                int ty = (Height - sz.Height) / 2;
                TextRenderer.DrawText(g, Text, Font, new Point(tx, ty), Color.White);
            }
        }

        #endregion
    }
}
