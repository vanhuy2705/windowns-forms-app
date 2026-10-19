using System;

namespace SportZone.WinForms.Models
{
    /// <summary>
    /// Model đại diện cho một bản ghi thông báo từ bảng dbo.ThongBao
    /// Khớp 100% với schema SQL Server
    /// </summary>
    public class NotificationItem
    {
        public long ThongBaoID { get; set; }
        public int? TaiKhoanID { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string LoaiThongBao { get; set; }
        public bool DaDoc { get; set; }
        public string LienKetLoai { get; set; }
        public long? LienKetID { get; set; }
        public DateTime NgayTao { get; set; }

        /// <summary>
        /// Định dạng thời gian tương đối thân thiện người dùng
        /// </summary>
        public string RelativeTime => FormatRelativeTime(NgayTao);

        public static string FormatRelativeTime(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalSeconds < 0) return "Vừa xong";
            if (span.TotalSeconds < 60) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
            if (span.TotalDays < 2) return "Hôm qua";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} ngày trước";
            return dt.ToString("dd/MM/yyyy HH:mm");
        }
    }
}
