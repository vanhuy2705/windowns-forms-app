using System;
using System.Collections.Generic;
using System.Data;
using SportZone.WinForms.Helpers;
using SportZone.WinForms.Models;
using SportZone.WinForms.Repositories;
using SportZone.WinForms.Security;
using SportZone.WinForms.Session;

namespace SportZone.WinForms.Services
{
    public class PasswordRecoveryService
    {
        private static readonly Lazy<PasswordRecoveryService> _instance = new Lazy<PasswordRecoveryService>(() => new PasswordRecoveryService());
        public static PasswordRecoveryService Instance => _instance.Value;

        private readonly PasswordRecoveryRepository _repo = new PasswordRecoveryRepository();

        public const int MaxRequestsPer15Min = 5;
        public const int ResendCooldownSeconds = 60;
        public const int OtpExpiryMinutes = 5;
        public const int MaxFailedAttempts = 5;

        /// <summary>
        /// Tìm kiếm tài khoản với cơ chế chống Account Enumeration
        /// </summary>
        public bool FindAccount(string identifier, out RecoveryAccountInfo account, out string message)
        {
            account = null;
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(identifier))
            {
                message = "Vui lòng nhập tên đăng nhập, email hoặc số điện thoại.";
                return false;
            }

            try
            {
                var found = _repo.FindRecoveryAccount(identifier.Trim());
                if (found == null)
                {
                    // Thông báo trung tính, không tiết lộ tài khoản có tồn tại hay không
                    message = "Nếu thông tin bạn nhập khớp với một tài khoản đang hoạt động, hệ thống sẽ chuyển đến bước xác minh.";
                    return false;
                }

                if (!found.DangHoatDong)
                {
                    message = "Tài khoản hiện đang bị khóa hoặc ngừng hoạt động. Vui lòng liên hệ quản trị viên.";
                    return false;
                }

                account = found;
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PasswordRecoveryService.FindAccount");
                message = "Không thể kết nối máy chủ dữ liệu. Vui lòng thử lại sau.";
                return false;
            }
        }

        /// <summary>
        /// Gửi OTP 6 số qua kênh đã chọn (SMS, Email, Zalo) và áp dụng Rate Limit
        /// </summary>
        public bool SendOtp(RecoveryAccountInfo account, string channel, out long requestId, out string errorMessage, out int cooldownSeconds, out string debugOtp)
        {
            requestId = 0;
            errorMessage = string.Empty;
            cooldownSeconds = 0;
            debugOtp = string.Empty;

            if (account == null)
            {
                errorMessage = "Thông tin tài khoản không hợp lệ.";
                return false;
            }

            try
            {
                // 1. Kiểm tra giới hạn số lần yêu cầu (Max 5 lần / 15 phút)
                int recentCount = _repo.GetRecentRequestCount(account.TaiKhoanID, 15);
                if (recentCount >= MaxRequestsPer15Min)
                {
                    errorMessage = "Tạm thời không thể gửi thêm mã do vượt quá số lần cho phép (tối đa 5 lần / 15 phút). Vui lòng thử lại sau.";
                    return false;
                }

                // 2. Kiểm tra Cooldown gửi lại (60 giây)
                var latest = _repo.GetLatestRequest(account.TaiKhoanID);
                if (latest != null)
                {
                    double elapsedSec = (DateTime.Now - latest.NgayTao).TotalSeconds;
                    if (elapsedSec < ResendCooldownSeconds)
                    {
                        cooldownSeconds = ResendCooldownSeconds - (int)elapsedSec;
                        errorMessage = $"Vui lòng đợi {cooldownSeconds} giây trước khi yêu cầu gửi lại mã.";
                        return false;
                    }
                }

                // 3. Sinh OTP 6 chữ số bằng CSP RNG
                string otp = PasswordSecurity.GenerateSecureOtp(6);

                // 4. Băm OTP bằng SHA256 để lưu CSDL (TUYỆT ĐỐI KHÔNG lưu plaintext)
                string otpHash = PasswordSecurity.HashOtp(otp);

                // 5. Lưu yêu cầu vào CSDL (kèm EmailNhan)
                requestId = _repo.CreateRecoveryRequest(account.TaiKhoanID, otpHash, channel, OtpExpiryMinutes, account.Email);

                // 6. Gửi OTP qua Gmail SMTP nếu là kênh EMAIL
                if (string.Equals(channel, "EMAIL", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(account.Email))
                    {
                        _repo.InvalidateRequest(requestId);
                        requestId = 0;
                        errorMessage = "Tài khoản của bạn chưa được liên kết địa chỉ Email nhận mã xác minh.";
                        return false;
                    }

                    // Gửi email thật qua MailKit
                    bool sent = EmailService.Instance.SendOtpEmail(account.Email, account.HoTen, otp, out string emailErr);
                    if (!sent)
                    {
                        // Nếu SMTP fail: hủy request trong DB ngay lập tức, không cho chuyển sang OTP panel
                        _repo.InvalidateRequest(requestId);
                        requestId = 0;
                        errorMessage = !string.IsNullOrWhiteSpace(emailErr)
                            ? emailErr
                            : "Không thể gửi mã xác minh lúc này. Vui lòng thử lại.";
                        return false;
                    }

                    // Lưu lịch sử gửi tin vào hàng đợi hệ thống (KHÔNG ghi OTP plaintext)
                    _repo.EnqueueOtpMessage(account.TaiKhoanID, account.KhachHangID, "EMAIL", account.Email, "******");
                }
                else
                {
                    // Kênh SMS / Zalo
                    string destination = account.SoDienThoai;
                    _repo.EnqueueOtpMessage(account.TaiKhoanID, account.KhachHangID, channel, destination, otp);
                }

                // 7. Ghi audit log (KHÔNG ghi OTP plaintext)
                AppLogger.LogInfo($"[OTP_SENT] TaiKhoanID={account.TaiKhoanID}, RequestID={requestId}, Channel={channel}");

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PasswordRecoveryService.SendOtp", $"TaiKhoanID={account.TaiKhoanID}");
                errorMessage = "Không thể gửi mã xác minh lúc này. Vui lòng thử lại.";
                return false;
            }
        }

        /// <summary>
        /// Xác minh OTP 6 số với giới hạn 5 lần nhập sai
        /// </summary>
        public bool VerifyOtp(long requestId, string inputOtp, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(inputOtp) || inputOtp.Trim().Length != 6)
            {
                errorMessage = "Mã xác minh phải gồm đúng 6 chữ số.";
                return false;
            }

            try
            {
                var req = _repo.GetRecoveryRequest(requestId);
                if (req == null)
                {
                    errorMessage = "Yêu cầu khôi phục không tồn tại hoặc đã bị hủy.";
                    return false;
                }

                if (req.DaSuDung || req.TrangThai == "USED" || req.TrangThai == "INVALIDATED")
                {
                    errorMessage = "Yêu cầu khôi phục này không còn hiệu lực.";
                    return false;
                }

                if (req.IsExpired)
                {
                    errorMessage = "Mã xác minh đã hết hạn. Vui lòng gửi lại mã mới.";
                    return false;
                }

                if (req.IsLocked)
                {
                    errorMessage = "Bạn đã nhập sai mã xác minh quá 5 lần. Yêu cầu đã bị khóa. Vui lòng yêu cầu mã mới.";
                    return false;
                }

                // So sánh băm OTP an toàn
                bool isMatch = PasswordSecurity.VerifyOtp(inputOtp.Trim(), req.TokenHash);
                if (!isMatch)
                {
                    _repo.IncrementFailedOtpAttempt(requestId);
                    int remaining = Math.Max(0, MaxFailedAttempts - (req.SoLanThuSai + 1));
                    errorMessage = remaining > 0
                        ? $"Mã xác minh không chính xác. Bạn còn {remaining} lần thử."
                        : "Bạn đã nhập sai mã xác minh 5 lần. Yêu cầu này đã bị khóa.";
                    return false;
                }

                // Xác minh thành công -> cập nhật trạng thái VERIFIED
                _repo.MarkOtpVerified(requestId);
                AppLogger.LogInfo($"[OTP_VERIFIED] RequestID={requestId}, TaiKhoanID={req.TaiKhoanID}");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PasswordRecoveryService.VerifyOtp", $"RequestID={requestId}");
                errorMessage = "Không thể kiểm tra mã xác minh. Vui lòng thử lại.";
                return false;
            }
        }

        /// <summary>
        /// Đặt lại mật khẩu an toàn theo giao dịch nguyên tử (Atomic Transaction)
        /// </summary>
        public bool ResetPassword(long requestId, int taiKhoanId, string newPassword, string confirmPassword, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                errorMessage = "Vui lòng nhập đầy đủ mật khẩu mới và xác nhận mật khẩu.";
                return false;
            }

            if (newPassword != confirmPassword)
            {
                errorMessage = "Xác nhận mật khẩu không khớp.";
                return false;
            }

            if (!PasswordSecurity.ValidatePasswordPolicy(newPassword, out string policyError))
            {
                errorMessage = policyError;
                return false;
            }

            try
            {
                // 1. Tạo salt mới ngẫu nhiên 32 bytes
                byte[] salt = PasswordSecurity.GenerateSalt(32);

                // 2. Băm mật khẩu bằng PBKDF2-SHA256 (10,000 vòng lặp chuẩn hệ thống)
                string hash = PasswordSecurity.HashPassword(newPassword, salt, 10000);

                // 3. Thực thi transaction nguyên tử ở database layer
                bool success = _repo.ResetPasswordAtomic(requestId, taiKhoanId, hash, salt, "PBKDF2-SHA256", 10000);
                if (!success)
                {
                    errorMessage = "Yêu cầu khôi phục không hợp lệ, đã hết hạn hoặc đã được sử dụng.";
                    return false;
                }

                // 4. Kích hoạt sự kiện làm mới Notification Center
                NotificationService.TriggerNotificationChanged();

                // 5. Gửi thông báo bảo mật vào Notification Center (dbo.ThongBao)
                try
                {
                    NotificationService.Instance.CreateNotification(
                        taiKhoanId,
                        "Bảo mật: Đổi mật khẩu thành công",
                        $"Mật khẩu tài khoản của bạn đã được cập nhật lúc {DateTime.Now:HH:mm dd/MM/yyyy}. Nếu không phải bạn thực hiện, vui lòng liên hệ quản trị viên ngay lập tức.",
                        "SECURITY",
                        "ACCOUNT_SECURITY",
                        requestId);
                }
                catch (Exception exNotify)
                {
                    AppLogger.LogWarning($"[POST_RESET_NOTIFICATION_ERROR] {exNotify.Message}");
                }

                // 6. Gửi email cảnh báo bảo mật tới email đã đăng ký
                try
                {
                    var acc = _repo.GetAccountById(taiKhoanId);
                    if (acc != null && !string.IsNullOrWhiteSpace(acc.Email))
                    {
                        EmailService.Instance.SendPasswordChangedAlert(acc.Email, acc.HoTen, DateTime.Now, out _);
                    }
                }
                catch (Exception exEmail)
                {
                    AppLogger.LogWarning($"[POST_RESET_EMAIL_ALERT_ERROR] {exEmail.Message}");
                }

                AppLogger.LogInfo($"[PASSWORD_RESET_SUCCESS] TaiKhoanID={taiKhoanId}, RequestID={requestId}");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PasswordRecoveryService.ResetPassword", $"TaiKhoanID={taiKhoanId}");
                errorMessage = "Không thể cập nhật mật khẩu mới vào hệ thống. Vui lòng thử lại.";
                return false;
            }
        }

        /// <summary>
        /// Nhân viên không có phương thức liên lạc gửi yêu cầu hỗ trợ tới Quản trị viên
        /// </summary>
        public bool RequestAdminAssistance(RecoveryAccountInfo account, string reason, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (account == null)
            {
                errorMessage = "Thông tin tài khoản không hợp lệ.";
                return false;
            }

            if (!account.IsStaff)
            {
                errorMessage = "Chức năng yêu cầu hỗ trợ chỉ áp dụng cho tài khoản nhân viên.";
                return false;
            }

            try
            {
                long reqId = _repo.CreateAdminAssistanceRequest(account.TaiKhoanID, reason);

                string staffCaption = $"{account.TenDangNhap} • {account.HoTen}";
                NotificationService.Instance.NotifyRole("ADMIN",
                    "Yêu cầu khôi phục tài khoản",
                    $"{staffCaption} gửi yêu cầu hỗ trợ cấp lại mật khẩu.",
                    "SECURITY",
                    "ACCOUNT_RECOVERY",
                    reqId);

                AppLogger.LogInfo($"[ADMIN_ASSISTANCE_REQUESTED] TaiKhoanID={account.TaiKhoanID}, RequestID={reqId}");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PasswordRecoveryService.RequestAdminAssistance", $"TaiKhoanID={account.TaiKhoanID}");
                errorMessage = "Không thể gửi yêu cầu hỗ trợ lúc này. Vui lòng liên hệ trực tiếp quản trị viên.";
                return false;
            }
        }

        /// <summary>
        /// Quản trị viên tạo mật khẩu tạm thời cho nhân viên và buộc đổi mật khẩu khi đăng nhập
        /// </summary>
        public bool AdminGenerateTemporaryPassword(long requestId, int adminTaiKhoanId, out string plainTempPassword, out string errorMessage)
        {
            plainTempPassword = string.Empty;
            errorMessage = string.Empty;

            if (AppSession.Current == null || !AppSession.Current.IsAdmin)
            {
                errorMessage = "Bạn không có quyền quản trị để thực hiện chức năng này.";
                return false;
            }

            try
            {
                var req = _repo.GetRecoveryRequest(requestId);
                if (req == null || req.DaSuDung || req.TrangThai != "ADMIN_PENDING")
                {
                    errorMessage = "Yêu cầu khôi phục không tồn tại hoặc đã được xử lý trước đó.";
                    return false;
                }

                // Kiểm tra tài khoản đích: Nhân viên tuyệt đối không được reset Admin
                var targetAcc = _repo.GetAccountById(req.TaiKhoanID);
                if (targetAcc != null && targetAcc.IsAdmin)
                {
                    errorMessage = "Không được phép cấp mật khẩu tạm thời cho tài khoản Quản trị viên.";
                    return false;
                }

                // Sinh mật khẩu tạm thời ngẫu nhiên 14 ký tự (gồm hoa, thường, số, ký tự đặc biệt)
                plainTempPassword = PasswordSecurity.GenerateSecureTemporaryPassword(14);

                byte[] salt = PasswordSecurity.GenerateSalt(32);
                string hash = PasswordSecurity.HashPassword(plainTempPassword, salt, 10000);

                bool ok = _repo.AdminResetPasswordAtomic(requestId, req.TaiKhoanID, adminTaiKhoanId, hash, salt);
                if (!ok)
                {
                    errorMessage = "Lỗi khi lưu mật khẩu tạm thời vào cơ sở dữ liệu.";
                    plainTempPassword = string.Empty;
                    return false;
                }

                NotificationService.TriggerNotificationChanged();
                AppLogger.LogInfo($"[ADMIN_TEMP_PASSWORD_GENERATED] RequestID={requestId}, TargetID={req.TaiKhoanID}, AdminID={adminTaiKhoanId}");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PasswordRecoveryService.AdminGenerateTemporaryPassword", $"RequestID={requestId}");
                errorMessage = "Không thể cấp mật khẩu tạm thời lúc này. Vui lòng thử lại.";
                plainTempPassword = string.Empty;
                return false;
            }
        }

        public DataTable GetAdminPendingRequests()
        {
            try
            {
                return _repo.GetAdminPendingRequests();
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PasswordRecoveryService.GetAdminPendingRequests");
                return new DataTable();
            }
        }
    }
}
