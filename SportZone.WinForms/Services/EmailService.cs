using System;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SportZone.WinForms.Helpers;

namespace SportZone.WinForms.Services
{
    /// <summary>
    /// Dịch vụ gửi email chuẩn SMTP MailKit cho hệ thống SPORTFIELD.
    /// Tách biệt hoàn toàn khỏi business logic, bảo vệ an toàn App Password.
    /// </summary>
    public class EmailService
    {
        private static readonly Lazy<EmailService> _instance = new Lazy<EmailService>(() => new EmailService());
        public static EmailService Instance => _instance.Value;

        public const string DefaultSmtpHost = "smtp.gmail.com";
        public const int DefaultSmtpPort = 587;
        public const string DefaultFromEmail = "vanhuy27506@gmail.com";
        public const string DefaultFromName = "SPORTFIELD";

        public string SmtpHost => GetAppSetting("SmtpHost", DefaultSmtpHost);
        public int SmtpPort
        {
            get
            {
                string portStr = GetAppSetting("SmtpPort", DefaultSmtpPort.ToString());
                return int.TryParse(portStr, out int p) ? p : DefaultSmtpPort;
            }
        }
        public string SmtpUsername => GetAppSetting("SmtpUsername", DefaultFromEmail);
        public string SmtpFromEmail => GetAppSetting("SmtpFromEmail", DefaultFromEmail);
        public string SmtpFromName => GetAppSetting("SmtpFromName", DefaultFromName);

        /// <summary>
        /// Kiểm tra xem cấu hình SMTP đã được thiết lập đầy đủ hay chưa (không phải placeholder).
        /// </summary>
        public bool IsConfigured
        {
            get
            {
                string pwd = GetSmtpPassword();
                return !string.IsNullOrWhiteSpace(SmtpHost) &&
                       !string.IsNullOrWhiteSpace(SmtpUsername) &&
                       !string.IsNullOrWhiteSpace(pwd) &&
                       !string.Equals(pwd, "YOUR_NEW_GMAIL_APP_PASSWORD", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Đọc mật khẩu ứng dụng Gmail (Google App Password).
        /// Thứ tự ưu tiên:
        /// 1. Biến môi trường SPORTFIELD_SMTP_PASSWORD
        /// 2. Tệp DPAPI mã hóa nội bộ máy tính (%LocalAppData%\SportZone\smtp.dat)
        /// 3. Khóa SmtpPassword trong App.config
        /// </summary>
        public string GetSmtpPassword()
        {
            string raw = null;

            // 1. Kiểm tra Environment Variable
            string envPwd = Environment.GetEnvironmentVariable("SPORTFIELD_SMTP_PASSWORD");
            if (!string.IsNullOrWhiteSpace(envPwd))
            {
                raw = envPwd.Trim();
            }

            // 2. Kiểm tra tệp DPAPI được mã hóa an toàn bằng Windows DPAPI
            if (string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    string dpapiFile = GetDpapiFilePath();
                    if (File.Exists(dpapiFile))
                    {
                        byte[] encrypted = File.ReadAllBytes(dpapiFile);
                        byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                        string pass = Encoding.UTF8.GetString(decrypted);
                        if (!string.IsNullOrWhiteSpace(pass))
                        {
                            raw = pass.Trim();
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.LogError(ex, "EmailService.GetSmtpPassword_DPAPI");
                }
            }

            // 3. Fallback: Đọc từ App.config
            if (string.IsNullOrWhiteSpace(raw))
            {
                raw = ConfigurationManager.AppSettings["SmtpPassword"]?.Trim();
            }

            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            // Chuẩn hóa: loại bỏ khoảng trắng giữa các cụm ký tự (xxxx xxxx xxxx xxxx)
            string normalized = raw.Replace(" ", "").Replace("\t", "").Trim();
            if (normalized.Length == 16)
            {
                normalized = normalized.ToLowerInvariant();
            }
            return normalized;
        }

        /// <summary>
        /// Lưu Google App Password bằng Windows DPAPI mã hóa theo User cục bộ, không lưu plaintext.
        /// </summary>
        public bool SaveEncryptedPassword(string plainAppPassword)
        {
            if (string.IsNullOrWhiteSpace(plainAppPassword)) return false;
            try
            {
                string dpapiFile = GetDpapiFilePath();
                string dir = Path.GetDirectoryName(dpapiFile);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                byte[] plainBytes = Encoding.UTF8.GetBytes(plainAppPassword.Trim());
                byte[] encrypted = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(dpapiFile, encrypted);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "EmailService.SaveEncryptedPassword");
                return false;
            }
        }

        private string GetDpapiFilePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "SportZone", "smtp.dat");
        }

        private string GetAppSetting(string key, string fallback)
        {
            try
            {
                string val = ConfigurationManager.AppSettings[key];
                return !string.IsNullOrWhiteSpace(val) ? val.Trim() : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        /// <summary>
        /// Gửi email qua Gmail SMTP sử dụng thư viện MailKit.
        /// </summary>
        public bool SendEmail(string toEmail, string toName, string subject, string bodyHtml, string bodyText, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                errorMessage = "Địa chỉ email người nhận không được để trống.";
                return false;
            }

            string password = GetSmtpPassword();
            if (string.IsNullOrWhiteSpace(password) ||
                string.Equals(password, "YOUR_NEW_GMAIL_APP_PASSWORD", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Hệ thống chưa được cấu hình Google App Password cho máy chủ Gmail SMTP. Vui lòng thiết lập SmtpPassword trong App.config.";
                AppLogger.LogError(new InvalidOperationException("SMTP App Password is not configured or still placeholder."), "EmailService.SendEmail");
                return false;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(SmtpFromName, SmtpFromEmail));
                message.To.Add(new MailboxAddress(toName ?? toEmail, toEmail.Trim()));
                message.Subject = subject ?? "SPORTFIELD Thông Báo";

                var builder = new BodyBuilder
                {
                    HtmlBody = bodyHtml,
                    TextBody = bodyText ?? bodyHtml
                };
                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // Giới hạn thời gian chờ 25 giây
                    client.Timeout = 25000;

                    // Kết nối máy chủ SMTP (Port 465 dùng SslOnConnect, Port 587 dùng StartTls)
                    var sslOption = (SmtpPort == 465) ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                    client.Connect(SmtpHost, SmtpPort, sslOption);

                    // Xác thực bằng App Password
                    client.Authenticate(SmtpUsername, password);

                    // Gửi email
                    client.Send(message);

                    // Ngắt kết nối an toàn
                    client.Disconnect(true);
                }

                AppLogger.LogInfo($"[SMTP_SENT_SUCCESS] To={toEmail}, Subject={subject}");
                return true;
            }
            catch (AuthenticationException ex)
            {
                AppLogger.LogError(ex, "EmailService.SendEmail_AuthFailed", $"Host={SmtpHost}, User={SmtpUsername}");
                errorMessage = "Lỗi xác thực tài khoản Gmail SMTP (sai Google App Password hoặc tài khoản chưa bật Xác minh 2 bước).";
                return false;
            }
            catch (SocketException ex)
            {
                AppLogger.LogError(ex, "EmailService.SendEmail_SocketError", $"Host={SmtpHost}, Port={SmtpPort}");
                errorMessage = "Không thể kết nối đến máy chủ Gmail SMTP (vui lòng kiểm tra kết nối mạng Internet hoặc cấu hình DNS).";
                return false;
            }
            catch (SmtpCommandException ex)
            {
                AppLogger.LogError(ex, "EmailService.SendEmail_SmtpCommandError", $"StatusCode={ex.StatusCode}");
                errorMessage = $"Máy chủ Gmail SMTP từ chối lệnh gửi thư (Mã lỗi: {ex.StatusCode}).";
                return false;
            }
            catch (SmtpProtocolException ex)
            {
                AppLogger.LogError(ex, "EmailService.SendEmail_ProtocolError");
                errorMessage = "Lỗi giao thức kết nối SMTP.";
                return false;
            }
            catch (TimeoutException ex)
            {
                AppLogger.LogError(ex, "EmailService.SendEmail_Timeout");
                errorMessage = "Quá thời gian kết nối tới máy chủ Gmail SMTP (Timeout).";
                return false;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "EmailService.SendEmail_GeneralError", $"To={toEmail}");
                errorMessage = "Không thể gửi email lúc này. Vui lòng kiểm tra lại cấu hình.";
                return false;
            }
        }

        /// <summary>
        /// Gửi email kiểm tra kết nối SMTP (Section 9)
        /// </summary>
        public bool SendTestEmail(string toEmail, out string errorMessage)
        {
            string subject = "SPORTFIELD - Kiểm tra Gmail SMTP";
            string bodyHtml = @"
<div style='font-family: Segoe UI, Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;'>
    <div style='background-color: #0d9488; color: #ffffff; padding: 20px; text-align: center;'>
        <h2 style='margin: 0; font-size: 24px; letter-spacing: 1px;'>SPORTFIELD</h2>
        <p style='margin: 4px 0 0 0; opacity: 0.9;'>HỆ THỐNG QUẢN LÝ SÂN THỂ THAO</p>
    </div>
    <div style='padding: 24px; background-color: #ffffff; color: #334155; line-height: 1.6;'>
        <h3 style='color: #0f172a; margin-top: 0;'>Kiểm tra kết nối Gmail SMTP thành công</h3>
        <p>Nếu bạn nhận được email này, tính năng gửi email qua Gmail SMTP của <strong>SPORTFIELD</strong> đã hoạt động hoàn toàn chính xác.</p>
        <div style='background-color: #f8fafc; border-left: 4px solid #0d9488; padding: 12px; margin: 20px 0;'>
            <p style='margin: 0; font-size: 14px;'><strong>Thời gian kiểm tra:</strong> " + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + @"</p>
            <p style='margin: 4px 0 0 0; font-size: 14px;'><strong>Thư viện:</strong> MailKit (MimeKit) .NET Framework 4.8</p>
        </div>
        <p style='font-size: 13px; color: #64748b;'>Đây là email thử nghiệm tự động, vui lòng không trả lời.</p>
    </div>
</div>";

            string bodyText = "SPORTFIELD - Kiểm tra Gmail SMTP\n\nNếu bạn nhận được email này, Gmail SMTP của SPORTFIELD đã hoạt động.\nThời gian: " + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            return SendEmail(toEmail, toEmail, subject, bodyHtml, bodyText, out errorMessage);
        }

        /// <summary>
        /// Gửi email chứa mã OTP 6 số xác minh khôi phục mật khẩu (Section 27)
        /// </summary>
        public bool SendOtpEmail(string toEmail, string toName, string otpCode, out string errorMessage)
        {
            string subject = "SPORTFIELD - Mã xác minh khôi phục mật khẩu";
            string displayName = !string.IsNullOrWhiteSpace(toName) ? toName : toEmail;

            string bodyHtml = @"
<div style='font-family: Segoe UI, Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;'>
    <div style='background-color: #0d9488; color: #ffffff; padding: 20px; text-align: center;'>
        <h2 style='margin: 0; font-size: 24px; letter-spacing: 1px;'>SPORTFIELD</h2>
        <p style='margin: 4px 0 0 0; opacity: 0.9;'>XÁC MINH KHÔI PHỤC MẬT KHẨU</p>
    </div>
    <div style='padding: 24px; background-color: #ffffff; color: #334155; line-height: 1.6;'>
        <p>Xin chào <strong>" + displayName + @"</strong>,</p>
        <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản tại hệ thống SPORTFIELD.</p>
        <p style='margin-bottom: 8px;'>Mã xác minh (OTP) của bạn là:</p>
        <div style='background-color: #f0fdfa; border: 2px dashed #0d9488; border-radius: 8px; padding: 16px; text-align: center; margin: 16px 0;'>
            <span style='font-size: 36px; font-weight: bold; letter-spacing: 10px; color: #0d9488; font-family: Consolas, monospace;'>" + otpCode + @"</span>
        </div>
        <p style='color: #dc2626; font-size: 14px;'><strong>Lưu ý quan trọng:</strong></p>
        <ul style='color: #64748b; font-size: 14px; padding-left: 20px; margin-top: 4px;'>
            <li>Mã xác minh này có hiệu lực trong vòng <strong>5 phút</strong>.</li>
            <li>Tuyệt đối <strong>không chia sẻ</strong> mã này cho bất kỳ ai để bảo vệ an toàn tài khoản.</li>
            <li>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email và đổi mật khẩu ngay nếu nghi ngờ bị lộ thông tin.</li>
        </ul>
        <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;' />
        <p style='font-size: 12px; color: #94a3b8; text-align: center; margin: 0;'>© " + DateTime.Now.Year + @" SPORTFIELD Management System. All rights reserved.</p>
    </div>
</div>";

            string bodyText = @"SPORTFIELD
XÁC MINH KHÔI PHỤC MẬT KHẨU

Xin chào " + displayName + @",

Bạn vừa yêu cầu đặt lại mật khẩu tài khoản SPORTFIELD.

Mã OTP: " + otpCode + @"

Mã có hiệu lực trong 5 phút.
Không chia sẻ mã này với bất kỳ ai.

Nếu bạn không thực hiện yêu cầu, hãy bỏ qua email.

SPORTFIELD";

            return SendEmail(toEmail, displayName, subject, bodyHtml, bodyText, out errorMessage);
        }

        /// <summary>
        /// Gửi email thông báo bảo mật sau khi mật khẩu đã được đổi thành công (Section 39)
        /// Tuyệt đối KHÔNG chứa mật khẩu mới.
        /// </summary>
        public bool SendPasswordChangedAlert(string toEmail, string toName, DateTime changedTime, out string errorMessage)
        {
            string subject = "SPORTFIELD - Mật khẩu đã được thay đổi";
            string displayName = !string.IsNullOrWhiteSpace(toName) ? toName : toEmail;
            string timeStr = changedTime.ToString("HH:mm dd/MM/yyyy");

            string bodyHtml = @"
<div style='font-family: Segoe UI, Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;'>
    <div style='background-color: #1e293b; color: #ffffff; padding: 20px; text-align: center;'>
        <h2 style='margin: 0; font-size: 24px; letter-spacing: 1px;'>SPORTFIELD</h2>
        <p style='margin: 4px 0 0 0; opacity: 0.9;'>CẢNH BÁO BẢO MẬT TÀI KHOẢN</p>
    </div>
    <div style='padding: 24px; background-color: #ffffff; color: #334155; line-height: 1.6;'>
        <p>Xin chào <strong>" + displayName + @"</strong>,</p>
        <p>Mật khẩu tài khoản SPORTFIELD của bạn vừa được thay đổi thành công vào lúc:</p>
        <div style='background-color: #f8fafc; border-left: 4px solid #10b981; padding: 12px; margin: 16px 0;'>
            <strong style='font-size: 16px; color: #0f172a;'>" + timeStr + @"</strong>
        </div>
        <p style='color: #dc2626; font-size: 14px;'><strong>Nếu bạn KHÔNG thực hiện thay đổi này:</strong></p>
        <p style='font-size: 14px; color: #64748b;'>Vui lòng liên hệ ngay với Quản trị viên hệ thống để tạm khóa và bảo vệ tài khoản của bạn.</p>
        <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;' />
        <p style='font-size: 12px; color: #94a3b8; text-align: center; margin: 0;'>© " + DateTime.Now.Year + @" SPORTFIELD Security Center</p>
    </div>
</div>";

            string bodyText = @"Xin chào " + displayName + @",

Mật khẩu tài khoản SPORTFIELD của bạn vừa được thay đổi lúc:
" + timeStr + @"

Nếu không phải bạn thực hiện, hãy liên hệ quản trị viên ngay.

SPORTFIELD";

            return SendEmail(toEmail, displayName, subject, bodyHtml, bodyText, out errorMessage);
        }
    }
}
