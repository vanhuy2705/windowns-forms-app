using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Services;
using SportZone.WinForms.Session;

namespace SportZone.WinForms.Dialogs
{
    public class AdminRecoveryDialog : BaseDialog
    {
        private readonly long _recoveryRequestId;
        private DataRow _requestRow;
        private Panel _bodyPanel;
        private Panel _resultPanel;
        private TextBox _txtTempPassword;
        private SFButton _btnGenerate;
        private SFButton _btnCopy;

        public AdminRecoveryDialog(long recoveryRequestId = 0) 
            : base("Xử Lý Yêu Cầu Khôi Phục Mật Khẩu", new Size(580, 520))
        {
            _recoveryRequestId = recoveryRequestId;
            RootLayout.RowStyles[2].Height = 0;
            Footer.Visible = false;

            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            _bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Canvas,
                Padding = new Padding(24, 16, 24, 20)
            };
            Body.Controls.Add(_bodyPanel);
        }

        private void LoadData()
        {
            _bodyPanel.Controls.Clear();

            DataTable dt = PasswordRecoveryService.Instance.GetAdminPendingRequests();
            if (dt == null || dt.Rows.Count == 0)
            {
                ShowEmpty("Không có yêu cầu khôi phục mật khẩu nào đang chờ xử lý.");
                return;
            }

            DataRow targetRow = null;
            if (_recoveryRequestId > 0)
            {
                foreach (DataRow r in dt.Rows)
                {
                    if (Convert.ToInt64(r["YeuCauDatLaiID"]) == _recoveryRequestId)
                    {
                        targetRow = r;
                        break;
                    }
                }
            }

            if (targetRow == null)
            {
                targetRow = dt.Rows[0]; // Lấy yêu cầu đầu tiên nếu không chỉ định ID cụ thể
            }

            _requestRow = targetRow;
            RenderRequestDetails(targetRow);
        }

        private void ShowEmpty(string message)
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Label lbl = new Label
            {
                Text = "✓ " + message,
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lbl);
            _bodyPanel.Controls.Add(card);
        }

        private void RenderRequestDetails(DataRow r)
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Panel inner = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            string maNv = r["MaNhanVien"]?.ToString() ?? "N/A";
            string hoTen = r["HoTen"]?.ToString() ?? r["TenDangNhap"]?.ToString() ?? "Chưa rõ";
            string chucVu = r["ChucVu"]?.ToString() ?? r["TenVaiTro"]?.ToString() ?? "Nhân viên";
            string sdt = r["SoDienThoai"]?.ToString() ?? "Chưa cập nhật";
            string email = r["Email"]?.ToString() ?? "Chưa cập nhật";
            string ghiChu = r["GhiChu"]?.ToString() ?? "Yêu cầu cấp lại mật khẩu từ quản trị viên";
            DateTime ngayTao = Convert.ToDateTime(r["NgayTao"]);
            long reqId = Convert.ToInt64(r["YeuCauDatLaiID"]);

            Label lblTitle = new Label
            {
                Text = "THÔNG TIN YÊU CẦU CỦA NHÂN VIÊN",
                Font = Typography.PageTitle,
                ForeColor = Colors.TextPrimary,
                Location = new Point(0, 4),
                AutoSize = true
            };
            inner.Controls.Add(lblTitle);

            // Information Box
            Panel infoBox = new Panel
            {
                Location = new Point(0, 36),
                Size = new Size(480, 150),
                BackColor = Colors.SurfaceElevated,
                Padding = new Padding(16)
            };
            infoBox.Paint += (s, e) =>
            {
                using (var pen = new Pen(Colors.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, infoBox.Width - 1, infoBox.Height - 1);
                }
            };

            int iy = 10;
            void AddRow(string label, string val)
            {
                Label lblL = new Label { Text = label, Font = Typography.CaptionBold, ForeColor = Colors.TextMuted, Location = new Point(14, iy), AutoSize = true };
                Label lblV = new Label { Text = val, Font = Typography.BodyBold, ForeColor = Colors.TextPrimary, Location = new Point(160, iy), AutoSize = true };
                infoBox.Controls.Add(lblL);
                infoBox.Controls.Add(lblV);
                iy += 26;
            }

            AddRow("Nhân viên:", $"{maNv} — {hoTen} ({chucVu})");
            AddRow("Tài khoản:", r["TenDangNhap"]?.ToString() ?? "");
            AddRow("Số điện thoại / Email:", $"{sdt} | {email}");
            AddRow("Thời gian yêu cầu:", ngayTao.ToString("HH:mm:ss  dd/MM/yyyy"));
            AddRow("Lý do / Ghi chú:", ghiChu);

            inner.Controls.Add(infoBox);

            // Warning Box
            Panel warnBox = new Panel
            {
                Location = new Point(0, 196),
                Size = new Size(480, 56),
                BackColor = Colors.WarningBg,
                Padding = new Padding(12)
            };
            Label lblWarn = new Label
            {
                Text = "⚠️ Chú ý: Mật khẩu tạm thời sẽ kích hoạt cờ 'BẮT BUỘC ĐỔI MẬT KHẨU'. Nhân viên phải tự đặt lại mật khẩu mới ngay trong lần đăng nhập đầu tiên.",
                Font = Typography.MicroBold,
                ForeColor = Colors.Warning,
                Dock = DockStyle.Fill
            };
            warnBox.Controls.Add(lblWarn);
            inner.Controls.Add(warnBox);

            // Action Result Panel (Hidden by default, shown upon generate)
            _resultPanel = new Panel
            {
                Location = new Point(0, 260),
                Size = new Size(480, 110),
                BackColor = Colors.SuccessBg,
                Visible = false,
                Padding = new Padding(14)
            };
            _resultPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Colors.Emerald, 1.2f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, _resultPanel.Width - 1, _resultPanel.Height - 1);
                }
            };

            Label lblResHeader = new Label
            {
                Text = "MẬT KHẨU TẠM THỜI ĐÃ ĐƯỢC TẠO THÀNH CÔNG (CHỈ XEM 1 LẦN):",
                Font = Typography.MicroBold,
                ForeColor = Colors.DeepEmerald,
                Location = new Point(12, 10),
                AutoSize = true
            };
            _resultPanel.Controls.Add(lblResHeader);

            _txtTempPassword = new TextBox
            {
                Location = new Point(14, 34),
                Size = new Size(320, 36),
                Font = new Font("Consolas", 14f, FontStyle.Bold),
                ForeColor = Colors.DeepEmerald,
                ReadOnly = true,
                BackColor = Color.White
            };
            _resultPanel.Controls.Add(_txtTempPassword);

            _btnCopy = new SFButton
            {
                Text = "Sao Chép",
                Variant = SFButtonVariant.Primary,
                Location = new Point(345, 32),
                Size = new Size(115, 36)
            };
            _btnCopy.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(_txtTempPassword.Text))
                {
                    Clipboard.SetText(_txtTempPassword.Text);
                    SFToast.Show(this, "Đã sao chép mật khẩu tạm thời vào Clipboard!", "ĐÃ SAO CHÉP", SFIcon.Check, Colors.Emerald);
                }
            };
            _resultPanel.Controls.Add(_btnCopy);

            Label lblResFooter = new Label
            {
                Text = "Vui lòng cung cấp mật khẩu này cho nhân viên để họ đăng nhập và đổi mật khẩu.",
                Font = Typography.Micro,
                ForeColor = Colors.TextSecondary,
                Location = new Point(12, 78),
                AutoSize = true
            };
            _resultPanel.Controls.Add(lblResFooter);

            inner.Controls.Add(_resultPanel);

            // Action Buttons
            _btnGenerate = new SFButton
            {
                Text = "CẤP MẬT KHẨU TẠM THỜI CHO NHÂN VIÊN",
                Variant = SFButtonVariant.Primary,
                Location = new Point(0, 380),
                Size = new Size(480, 42)
            };
            _btnGenerate.Click += (s, e) =>
            {
                int adminId = AppSession.Current != null ? AppSession.Current.TaiKhoanID : 1;
                _btnGenerate.Enabled = false;
                try
                {
                    bool ok = PasswordRecoveryService.Instance.AdminGenerateTemporaryPassword(
                        reqId,
                        adminId,
                        out string tempPwd,
                        out string err);

                    if (!ok)
                    {
                        SFToast.Show(this, err, "LỖI TẠO MẬT KHẨU", SFIcon.AlertCircle, Colors.Danger);
                        _btnGenerate.Enabled = true;
                        return;
                    }

                    _txtTempPassword.Text = tempPwd;
                    _resultPanel.Visible = true;
                    _btnGenerate.Visible = false;

                    SFToast.Show(this, "Đã tạo mật khẩu tạm thời thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                }
                catch (Exception ex)
                {
                    SFToast.Show(this, ex.Message, "LỖI HỆ THỐNG", SFIcon.AlertCircle, Colors.Danger);
                    _btnGenerate.Enabled = true;
                }
            };
            inner.Controls.Add(_btnGenerate);

            card.Controls.Add(inner);
            _bodyPanel.Controls.Add(card);
        }
    }
}
