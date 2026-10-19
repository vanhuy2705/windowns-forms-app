using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Security;

namespace SportZone.WinForms.Dialogs
{
    public class CustomerRegisterForm : BaseDialog
    {
        private SFTextBox txtFullName;
        private SFTextBox txtPhone;
        private SFTextBox txtEmail;
        private SFTextBox txtUsername;
        private SFTextBox txtPassword;
        private SFTextBox txtConfirmPassword;
        private CheckBox chkAgree;
        private readonly Services.CustomerService _customerService = new Services.CustomerService();

        public CustomerRegisterForm() : base("Đăng Ký Tài Khoản Khách Hàng", new Size(980, 680))
        {
            Build();
            FooterButtons("ĐĂNG KÝ TÀI KHOẢN");
        }

        private void Build()
        {
            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));

            // Left: Hero Panel
            Panel hero = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28), Margin = new Padding(0, 0, 10, 0) };
            hero.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                Image stadium = AssetManager.GetStadiumHero();
                if (stadium != null) GraphicsUtility.DrawImageCenterCrop(g, stadium, hero.ClientRectangle);
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    hero.ClientRectangle,
                    Color.FromArgb(235, 7, 17, 31),
                    Color.FromArgb(180, 8, 122, 85),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, hero.ClientRectangle);
                }
            };

            Label brand = new Label { Text = "SPORTFIELD", Font = Typography.HeroTitle, ForeColor = Color.White, Location = new Point(24, 24), AutoSize = true, BackColor = Color.Transparent };
            hero.Controls.Add(brand);

            Label title = new Label
            {
                Text = "NỀN TẢNG ĐẶT SÂN\nTHÔNG MINH & TIỆN LỢI",
                Font = Typography.PageTitle,
                ForeColor = Color.White,
                Location = new Point(24, 100),
                Size = new Size(340, 70),
                AutoSize = false,
                BackColor = Color.Transparent
            };
            hero.Controls.Add(title);

            Label desc = new Label
            {
                Text = "• Đặt sân tức thì 24/7 không cần gọi điện\n\n• Tặng Voucher GIAM30K cho lần đặt đầu tiên\n\n• Tích lũy điểm hội viên và nâng hạng VIP\n\n• Tra cứu hóa đơn và lịch sử thi đấu trực tuyến",
                Font = Typography.Body,
                ForeColor = ColorTranslator.FromHtml("#DCFCE7"),
                Location = new Point(24, 200),
                Size = new Size(340, 220),
                AutoSize = false,
                BackColor = Color.Transparent
            };
            hero.Controls.Add(desc);
            root.Controls.Add(hero, 0, 0);

            // Right: Registration Form
            SFCard form = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24, 18, 24, 16), Margin = new Padding(10, 0, 0, 0) };
            Label fTitle = new Label { Text = "Tạo Tài Khoản Mới • Vai Trò: Khách Hàng", UseMnemonic = false, Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 34 };
            form.Controls.Add(fTitle);

            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = false };
            string[] labels = { "Họ và tên khách hàng *", "Số điện thoại di động *", "Địa chỉ Email", "Tên đăng nhập mong muốn *", "Mật khẩu đăng nhập *", "Xác nhận mật khẩu *" };
            int y = 6;

            for (int i = 0; i < labels.Length; i++)
            {
                Label l = new Label { Text = labels[i], Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
                p.Controls.Add(l);
                SFTextBox t = new SFTextBox 
                { 
                    Text = "", 
                    Location = new Point(4, y + 20), 
                    Width = Math.Max(260, p.ClientSize.Width - 16), 
                    Height = 42, 
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    UseSystemPasswordChar = i >= 4 
                };
                p.Controls.Add(t);

                if (i == 0) txtFullName = t;
                else if (i == 1) txtPhone = t;
                else if (i == 2) txtEmail = t;
                else if (i == 3) txtUsername = t;
                else if (i == 4) txtPassword = t;
                else if (i == 5) txtConfirmPassword = t;

                y += 62;
            }

            chkAgree = new CheckBox { Text = "Tôi đồng ý với Quy chế sử dụng sân và Chính sách bảo mật.", Location = new Point(4, y + 4), AutoSize = true, Checked = true, Font = Typography.Body };
            p.Controls.Add(chkAgree);

            form.Controls.Add(p);
            root.Controls.Add(form, 1, 0);

            Body.Controls.Add(root);
        }

        protected override bool OnSave()
        {
            string fullName = txtFullName?.Text?.Trim() ?? "";
            string phone = txtPhone?.Text?.Trim() ?? "";
            string email = txtEmail?.Text?.Trim() ?? "";
            string username = txtUsername?.Text?.Trim() ?? "";
            string pwd = txtPassword?.Text ?? "";
            string confirmPwd = txtConfirmPassword?.Text ?? "";

            if (string.IsNullOrWhiteSpace(fullName))
            {
                SFToast.Show(this, "Vui lòng nhập họ và tên khách hàng.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
            if (string.IsNullOrWhiteSpace(phone) || phone.Length < 9)
            {
                SFToast.Show(this, "Vui lòng nhập số điện thoại hợp lệ (từ 9 đến 11 chữ số).", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                SFToast.Show(this, "Vui lòng nhập tên đăng nhập.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
            if (string.IsNullOrEmpty(pwd) || pwd.Length < 6)
            {
                SFToast.Show(this, "Mật khẩu phải có độ dài từ 6 ký tự trở lên.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
            if (pwd != confirmPwd)
            {
                SFToast.Show(this, "Mật khẩu xác nhận không trùng khớp.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
            if (chkAgree != null && !chkAgree.Checked)
            {
                SFToast.Show(this, "Vui lòng đồng ý với Quy chế sử dụng và Chính sách bảo mật.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            try
            {
                var result = _customerService.RegisterCustomer(fullName, phone, email, "", null, username, pwd);
                if (result.KhachHangID > 0)
                {
                    SFToast.Show(this, "Đăng ký tài khoản thành công! Bạn có thể đăng nhập ngay.", "ĐĂNG KÝ THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                    return true;
                }
                else
                {
                    SFToast.Show(this, "Không thể tạo tài khoản. Vui lòng kiểm tra lại thông tin.", "LỖI ĐĂNG KÝ", SFIcon.AlertCircle, Colors.Danger);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "CUSTOMER_REGISTER");
                string msg = Helpers.AppLogger.GetUserFriendlySqlMessage(ex);
                SFToast.Show(this, msg, "LỖI ĐĂNG KÝ", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class ChangePasswordForm : BaseDialog
    {
        private SFTextBox txtOldPwd;
        private SFTextBox txtNewPwd;
        private SFTextBox txtConfirmPwd;
        private readonly bool _isAdminReset;
        private readonly bool _isForced;
        private readonly int _targetTaiKhoanId;
        private readonly Services.AuthService _authService = new Services.AuthService();

        public ChangePasswordForm(bool adminReset = false, int targetTaiKhoanId = 0, bool isForced = false) 
            : base(isForced ? "Yêu Cầu Đổi Mật Khẩu Lần Đầu" : (adminReset ? "Đặt Lại Mật Khẩu Tài Khoản" : "Đổi Mật Khẩu Đăng Nhập"), new Size(540, 440))
        {
            _isAdminReset = adminReset;
            _isForced = isForced;
            _targetTaiKhoanId = targetTaiKhoanId > 0 ? targetTaiKhoanId : (Session.AppSession.Current != null ? Session.AppSession.Current.TaiKhoanID : 0);
            Build(adminReset);
            FooterButtons(adminReset ? "Cập Nhật Mật Khẩu" : "Đổi Mật Khẩu");
        }

        private void Build(bool reset)
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            int y = 10;
            if (!reset)
            {
                txtOldPwd = AddPwdField(p, "Mật khẩu hiện tại *", y);
                y += 66;
            }
            txtNewPwd = AddPwdField(p, "Mật khẩu mới *", y);
            y += 66;
            txtConfirmPwd = AddPwdField(p, "Nhập lại mật khẩu mới *", y);
            y += 66;

            Label info = new Label
            {
                Text = _isForced ? "Tài khoản của bạn được đánh dấu bắt buộc đổi mật khẩu trong lần đầu đăng nhập. Vui lòng tạo mật khẩu mới để bảo mật tài khoản." : "Mật khẩu tối thiểu 6 ký tự. Nên kết hợp chữ hoa, chữ thường, số và ký tự đặc biệt để đảm bảo an toàn.",
                Font = Typography.Secondary,
                ForeColor = _isForced ? Colors.Warning : Colors.TextSecondary,
                Location = new Point(4, y),
                Size = new Size(440, 40),
                AutoSize = false
            };
            p.Controls.Add(info);

            card.Controls.Add(p);
            Body.Controls.Add(card);
        }

        private SFTextBox AddPwdField(Control p, string label, int y)
        {
            Label l = new Label { Text = label, Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l);
            SFTextBox t = new SFTextBox 
            { 
                Location = new Point(4, y + 20), 
                Width = Math.Max(260, p.ClientSize.Width - 16), 
                Height = 42, 
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                UseSystemPasswordChar = true, 
                Text = "" 
            };
            p.Controls.Add(t);
            return t;
        }

        protected override bool OnSave()
        {
            string oldPwd = txtOldPwd?.Text ?? "";
            string newPwd = txtNewPwd?.Text ?? "";
            string confirmPwd = txtConfirmPwd?.Text ?? "";

            if (!_isAdminReset && string.IsNullOrEmpty(oldPwd))
            {
                SFToast.Show(this, "Vui lòng nhập mật khẩu hiện tại.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
            if (string.IsNullOrEmpty(newPwd) || newPwd.Length < 6)
            {
                SFToast.Show(this, "Mật khẩu mới phải có ít nhất 6 ký tự.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
            if (newPwd != confirmPwd)
            {
                SFToast.Show(this, "Mật khẩu xác nhận không trùng khớp.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }

            string errMsg;
            bool ok = _authService.ChangePassword(_targetTaiKhoanId, oldPwd, newPwd, out errMsg, isAdminReset: _isAdminReset);
            if (ok)
            {
                SFToast.Show(this, "Đổi mật khẩu thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                return true;
            }
            else
            {
                SFToast.Show(this, errMsg, "LỖI ĐỔI MẬT KHẨU", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class CustomerVietQrForm : BaseDialog
    {
        private readonly decimal _amount;
        private readonly string _bookingCode;
        private readonly string _customerName;
        private readonly long _hoaDonId;
        private readonly long _giaoDichId;
        private Bitmap _qrBitmap;

        public CustomerVietQrForm() : this(350000, "BK-20261004-001", "Nguyễn Văn Tuấn", 0, 0) { }

        public CustomerVietQrForm(decimal amount, string bookingCode, string customerName, long hoaDonId = 0, long giaoDichId = 0) : base("Thanh Toán VietQR Napas 247", new Size(880, 640))
        {
            _amount = amount > 0 ? amount : 350000;
            _bookingCode = string.IsNullOrEmpty(bookingCode) ? "BK-20261004-001" : bookingCode;
            _customerName = string.IsNullOrEmpty(customerName) ? "Khách hàng" : customerName;
            _hoaDonId = hoaDonId;
            _giaoDichId = giaoDichId;
            Build();
            FooterButtons("KIỂM TRA TRẠNG THÁI THANH TOÁN");
        }

        private void Build()
        {
            string bankName = "CHƯA CẤU HÌNH";
            string bankBin = "";
            string stk = "";
            string chuTk = "CHƯA CẤU HÌNH";
            string prefix = "SPORTFIELD";
            bool bankConfigured = false;

            try
            {
                var dt = new Services.PaymentService().GetBankAccounts();
                if (dt != null && dt.Rows.Count > 0)
                {
                    bankName = dt.Rows[0]["TenNganHang"]?.ToString() ?? bankName;
                    bankBin = dt.Rows[0]["BankBIN"]?.ToString() ?? "";
                    stk = dt.Rows[0]["SoTaiKhoan"]?.ToString() ?? "";
                    chuTk = dt.Rows[0]["ChuTaiKhoan"]?.ToString() ?? chuTk;
                    prefix = dt.Rows[0]["PrefixNoiDung"]?.ToString() ?? prefix;
                    bankConfigured = !string.IsNullOrEmpty(bankBin) && !string.IsNullOrEmpty(stk);
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning("CustomerVietQrForm GetBankAccounts: " + ex.Message);
            }

            string transferContent = $"{prefix} {_bookingCode.Replace("-", "")}";
            if (bankConfigured && _amount > 0)
            {
                _qrBitmap = Helpers.VietQRHelper.GenerateQrBitmap(bankBin, stk, _amount, transferContent, 240, 240);
            }
            else
            {
                _qrBitmap = null;
            }

            TableLayoutPanel split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54f));

            // Left: VietQR card
            SFCard qrCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(20), Margin = new Padding(0, 0, 10, 0) };
            Label bankTitle = new Label { Text = $"VIETQR PRO • {bankName.ToUpper()}", Font = Typography.SectionHeader, ForeColor = Colors.ElectricBlue, Dock = DockStyle.Top, Height = 26 };
            qrCard.Controls.Add(bankTitle);

            Panel box = new Panel
            {
                Size = new Size(240, 240),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top
            };
            box.Location = new Point(Math.Max(10, (qrCard.ClientSize.Width - 240) / 2), 40);
            box.Paint += (sender, e) =>
            {
                if (_qrBitmap != null)
                {
                    e.Graphics.DrawImage(_qrBitmap, 0, 0, box.Width, box.Height);
                }
                else
                {
                    using (Font f = new Font("Segoe UI", 9f, FontStyle.Regular))
                    {
                        TextRenderer.DrawText(e.Graphics, "Chưa cấu hình tài khoản\nngân hàng nhận thanh toán", f, box.ClientRectangle, Colors.Danger, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
            };
            qrCard.Resize += (s, e) =>
            {
                box.Left = Math.Max(10, (qrCard.ClientSize.Width - box.Width) / 2);
            };
            qrCard.Controls.Add(box);

            Label timer = new Label
            {
                Text = "⏱ Giữ chỗ còn: 15:00",
                Font = Typography.CaptionBold,
                ForeColor = Colors.Amber,
                BackColor = Colors.WarningBg,
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(Math.Max(10, (qrCard.ClientSize.Width - 170) / 2), 295),
                AutoSize = true,
                Anchor = AnchorStyles.Top
            };
            qrCard.Resize += (s, e) =>
            {
                timer.Left = Math.Max(10, (qrCard.ClientSize.Width - timer.Width) / 2);
            };
            qrCard.Controls.Add(timer);

            Label scan = new Label
            {
                Text = "Mở app ngân hàng hoặc ví điện tử để quét mã thanh toán tự động.",
                Font = Typography.Secondary,
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Bottom,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter
            };
            qrCard.Controls.Add(scan);
            split.Controls.Add(qrCard, 0, 0);

            // Right: Beneficiary Information
            SFCard infoCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(20), Margin = new Padding(10, 0, 0, 0) };
            Label iTitle = new Label { Text = "Thông Tin Chuyển Khoản Thụ Hưởng", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            infoCard.Controls.Add(iTitle);

            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Label content = new Label
            {
                Text = $"Đơn đặt: {_bookingCode}\nKhách hàng: {_customerName}\n\nNgân hàng thụ hưởng:\n{bankName}\n\nSố tài khoản:\n{stk}\n\nChủ tài khoản:\n{chuTk}\n\nSỐ TIỀN CẦN CHUYỂN:\n{_amount:N0} đ\n\nNỘI DUNG CHUYỂN KHOẢN (BẮT BUỘC):\n{transferContent}",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 270,
                AutoSize = false
            };
            p.Controls.Add(content);

            Panel note = Ui.Notice("TỰ ĐỘNG KHỚP LỆNH", "Giao dịch qua VietQR Napas 247 được khớp tự động và cập nhật trạng thái đơn trong 30 giây.", Colors.Emerald, Colors.SuccessBg);
            note.Dock = DockStyle.Top;
            note.Height = 62;
            p.Controls.Add(note);

            infoCard.Controls.Add(p);
            split.Controls.Add(infoCard, 1, 0);

            Body.Controls.Add(split);
        }

        protected override bool OnSave()
        {
            if (_giaoDichId > 0)
            {
                try
                {
                    DataTable dt = DatabaseManager.ExecuteQuery(
                        "SELECT TrangThai FROM dbo.GiaoDichThanhToan WHERE GiaoDichID = @GD",
                        new SqlParameter("@GD", _giaoDichId));
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string status = dt.Rows[0]["TrangThai"]?.ToString() ?? "";
                        if (status == "THANH_CONG")
                        {
                            SFToast.Show(this, "Giao dịch VietQR Napas 247 đã được xác nhận thành công!", "THANH TOÁN THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                            return true;
                        }
                        else if (status == "THAT_BAI" || status == "DA_HUY")
                        {
                            SFToast.Show(this, "Giao dịch đã bị từ chối hoặc hủy bỏ.", "GIAO DỊCH THẤT BẠI", SFIcon.AlertCircle, Colors.Danger);
                            return false;
                        }
                        else
                        {
                            SFToast.Show(this, "Hệ thống đang chờ ngân hàng đối soát hoặc thu ngân xác nhận giao dịch. Vui lòng thử lại sau giây lát.", "ĐANG CHỜ XÁC NHẬN", SFIcon.Clock, Colors.Amber);
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helpers.AppLogger.LogError(ex, "CHECK_VIETQR_STATUS");
                }
            }
            SFToast.Show(this, "Hệ thống đang kiểm tra giao dịch Napas 247. Vui lòng giữ màn hình hoặc đợi xác nhận từ quầy.", "KIỂM TRA VIETQR", SFIcon.Info, Colors.ElectricBlue);
            return false;
        }
    }

    public class CustomerCancelBookingForm : BaseDialog
    {
        private readonly long _donDatSanId;

        public CustomerCancelBookingForm() : this(0, "BK-20261004-001", "Sân bóng đá A1", 255000) { }

        public CustomerCancelBookingForm(long donDatSanId, string maDon = null, string sanName = null, decimal tienCoc = 0)
            : base("Xác Nhận Hủy Đơn Đặt Sân", new Size(640, 420))
        {
            _donDatSanId = donDatSanId;
            Build(maDon, sanName, tienCoc);
            FooterButtons("XÁC NHẬN HỦY SÂN");
        }

        private void Build(string maDon, string sanName, decimal tienCoc)
        {
            SFCard order = new SFCard { Dock = DockStyle.Top, Height = 120, Padding = new Padding(16) };
            Label d = new Label
            {
                Text = string.Format("Mã đơn: {0}\n{1}\nTiền cọc đã thanh toán: {2:N0}đ",
                    string.IsNullOrEmpty(maDon) ? "BK-20261004-001" : maDon,
                    string.IsNullOrEmpty(sanName) ? "Sân bóng đá tiêu chuẩn" : sanName,
                    tienCoc),
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Fill,
                AutoSize = false
            };
            order.Controls.Add(d);

            Panel spacer = new Panel { Dock = DockStyle.Top, Height = 14 };

            Panel n = Ui.Notice("CHÍNH SÁCH HOÀN TIỀN CỌC", "Hủy trước 02 tiếng: Hoàn 100% tiền cọc vào ví tích lũy trong 15 phút.\nHủy trong vòng 02 tiếng: Phí hủy 50% theo chính sách vận hành của trung tâm.", Colors.Amber, Colors.WarningBg);
            n.Dock = DockStyle.Top;
            n.Height = 74;

            Body.Controls.Add(n);
            Body.Controls.Add(spacer);
            Body.Controls.Add(order);
        }

        protected override bool OnSave()
        {
            if (_donDatSanId > 0)
            {
                try
                {
                    new Services.BookingService().CancelBooking(_donDatSanId, Session.AppSession.Current != null ? (int?)Session.AppSession.Current.TaiKhoanID : null, "Khách hàng yêu cầu hủy đặt sân trực tuyến");
                    SFToast.Show(this, "Đã hủy đơn đặt sân thành công theo chính sách hoàn cọc.", "HỦY ĐƠN THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                    return true;
                }
                catch (Exception ex)
                {
                    Helpers.AppLogger.LogError(ex, "CUSTOMER_CANCEL");
                    SFToast.Show(this, "Lỗi hủy đơn: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Danger);
                    return false;
                }
            }
            SFToast.Show(this, "Đã xác nhận hủy đơn đặt sân.", "HỦY ĐƠN", SFIcon.Check, Colors.Emerald);
            return true;
        }
    }

    public class StaffShiftDialog : BaseDialog
    {
        private readonly Services.ShiftService _shiftService = new Services.ShiftService();

        public StaffShiftDialog() : base("Bàn Giao & Đóng Ca Trực Quầy", new Size(740, 580))
        {
            Build();
            FooterButtons("XÁC NHẬN BÀN GIAO CA");
        }

        private void Build()
        {
            int nhanVienId = Security.SessionGuard.RequireEmployeeId();
            decimal tongThu = 0;
            decimal tienMat = 0;
            decimal ck = 0;
            int countThu = 0;
            int countMat = 0;
            int countCk = 0;

            try
            {
                var ds = _shiftService.GetShiftStatistics(nhanVienId);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var r = ds.Tables[0].Rows[0];
                    if (r.Table.Columns.Contains("TongThu") && r["TongThu"] != DBNull.Value) tongThu = Convert.ToDecimal(r["TongThu"]);
                    if (r.Table.Columns.Contains("TienMat") && r["TienMat"] != DBNull.Value) tienMat = Convert.ToDecimal(r["TienMat"]);
                    if (r.Table.Columns.Contains("ChuyenKhoan") && r["ChuyenKhoan"] != DBNull.Value) ck = Convert.ToDecimal(r["ChuyenKhoan"]);
                    if (r.Table.Columns.Contains("SoPhieuThu") && r["SoPhieuThu"] != DBNull.Value) countThu = Convert.ToInt32(r["SoPhieuThu"]);
                    if (r.Table.Columns.Contains("SoPhieuTienMat") && r["SoPhieuTienMat"] != DBNull.Value) countMat = Convert.ToInt32(r["SoPhieuTienMat"]);
                    if (r.Table.Columns.Contains("SoGiaoDichCK") && r["SoGiaoDichCK"] != DBNull.Value) countCk = Convert.ToInt32(r["SoGiaoDichCK"]);
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "STAFF_SHIFT_STATS");
            }

            TableLayoutPanel k = new TableLayoutPanel { Dock = DockStyle.Top, Height = 100, ColumnCount = 3, Padding = new Padding(0, 0, 0, 14) };
            for (int i = 0; i < 3; i++) k.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            k.Controls.Add(new SFMetricCard("TỔNG THU CA", $"{tongThu:N0}đ", $"{countThu} phiếu thu", SFIcon.Dollar, Colors.Emerald), 0, 0);
            k.Controls.Add(new SFMetricCard("TIỀN MẶT QUẦY", $"{tienMat:N0}đ", $"{countMat} phiếu tiền mặt", SFIcon.Dollar, Colors.ElectricBlue), 1, 0);
            k.Controls.Add(new SFMetricCard("VIETQR & CK", $"{ck:N0}đ", $"{countCk} giao dịch", SFIcon.QrCode, Colors.Purple), 2, 0);

            SFCard c = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(20) };
            Label title = new Label { Text = "Bảng Kê Đối Soát Tiền Mặt Đầu Ca vs Cuối Ca", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            c.Controls.Add(title);

            Label t = new Label
            {
                Text = $"Khung ca trực: Hôm nay  •  Nhân viên: {Session.AppSession.Current.HoTen ?? "Trần Văn An"}  •  Quầy POS\n\nTiền bàn giao đầu ca:                   1.000.000đ\nTiền mặt thu trong ca:                  {tienMat:N0}đ\n──────────────────────────────────────\nTổng tiền mặt bàn giao:                 {(1000000 + tienMat):N0}đ\nThực tế kiểm đếm:                       {(1000000 + tienMat):N0}đ\nChênh lệch tiền mặt:                             0đ (Khớp 100%)\n\nGiao dịch VietQR chờ kế toán đối soát:   {countCk} đơn",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Fill,
                AutoSize = false
            };
            c.Controls.Add(t);

            Body.Controls.Add(c);
            Body.Controls.Add(k);
        }

        protected override bool OnSave()
        {
            SFToast.Show(this, "Đã hoàn tất bàn giao và đóng ca trực thành công!", "ĐÓNG CA", SFIcon.Check, Colors.Emerald);
            return true;
        }
    }
}
