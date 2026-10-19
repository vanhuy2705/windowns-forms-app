using System;
using System.IO;
using System.Text;
using System.Data.SqlClient;

namespace SportZone.WinForms.Helpers
{
    public static class AppLogger
    {
        private static readonly object _lock = new object();
        private static readonly string _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        static AppLogger()
        {
            try
            {
                if (!Directory.Exists(_logDir))
                {
                    Directory.CreateDirectory(_logDir);
                }
            }
            catch { }
        }

        public static void LogError(Exception ex, string module = "SYSTEM", string context = null)
        {
            if (ex == null) return;
            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            string logPath = Path.Combine(_logDir, $"app-{dateStr}.log");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] [{module}] {(string.IsNullOrEmpty(context) ? "" : $"Context: {context}")}");
            sb.AppendLine($"Exception: {ex.GetType().FullName}: {ex.Message}");
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner: {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}");
            }
            sb.AppendLine($"StackTrace: {ex.StackTrace}");
            sb.AppendLine(new string('-', 80));

            WriteToFile(logPath, sb.ToString());
        }

        public static void LogInfo(string message, string module = "SYSTEM")
        {
            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            string logPath = Path.Combine(_logDir, $"app-{dateStr}.log");

            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [INFO] [{module}] {message}{Environment.NewLine}";
            WriteToFile(logPath, entry);
        }

        public static void LogWarning(string message, string module = "SYSTEM")
        {
            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            string logPath = Path.Combine(_logDir, $"app-{dateStr}.log");

            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [WARN] [{module}] {message}{Environment.NewLine}";
            WriteToFile(logPath, entry);
        }

        private static void WriteToFile(string path, string content)
        {
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(path, content, Encoding.UTF8);
                }
            }
            catch { }
        }

        public static string GetFriendlyErrorMessage(Exception ex)
        {
            if (ex == null) return "Đã xảy ra lỗi không xác định.";

            if (ex is SqlException sqlEx)
            {
                // SQL Server Error numbers mapping
                if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                {
                    return "Mã hoặc thông tin đã tồn tại trong hệ thống.";
                }
                if (sqlEx.Number == 547)
                {
                    return "Dữ liệu đang được sử dụng và không thể xóa.";
                }
                if (sqlEx.Number == -2 || sqlEx.Number == 1222)
                {
                    return "Máy chủ phản hồi quá lâu. Vui lòng thử lại.";
                }
                if (sqlEx.Number == 53 || sqlEx.Number == 4060 || sqlEx.Number == 18456)
                {
                    return "Không thể kết nối SQL Server.";
                }
                if (sqlEx.Number == 52017 || sqlEx.Message.IndexOf("ca trùng", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Nhân viên đã có ca làm việc trùng thời gian.";
                }

                string msg = sqlEx.Message;
                if (msg.IndexOf("trùng", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    msg.IndexOf("Khung giờ", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    msg.IndexOf("overlap", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Khung giờ này vừa được đặt hoặc đang có lịch hoạt động. Vui lòng chọn thời gian khác.";
                }
                if (msg.IndexOf("Tên đăng nhập đã tồn tại", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.";
                }
                if (msg.IndexOf("Số điện thoại đã tồn tại", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Số điện thoại đã được sử dụng.";
                }
                if (msg.IndexOf("Email đã tồn tại", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Email đã được sử dụng.";
                }
                if (msg.IndexOf("Voucher", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Voucher không hợp lệ hoặc đã hết hạn sử dụng.";
                }
                if (msg.IndexOf("tồn kho", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    msg.IndexOf("không đủ", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Số lượng tồn kho không đủ để thực hiện xuất kho.";
                }
                if (msg.IndexOf("vượt quá số tiền", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Số tiền thanh toán vượt số tiền còn lại của hóa đơn.";
                }
                if (msg.IndexOf("bảo trì", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Sân đang có lịch bảo trì trong khoảng thời gian đã chọn.";
                }
                return msg;
            }

            return ex.Message;
        }

        public static string GetUserFriendlySqlMessage(Exception ex)
        {
            return GetFriendlyErrorMessage(ex);
        }
    }
}
