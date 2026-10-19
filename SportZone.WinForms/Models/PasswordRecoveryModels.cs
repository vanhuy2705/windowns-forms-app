using System;

namespace SportZone.WinForms.Models
{
    /// <summary>
    /// Thông tin tài khoản phục vụ quy trình khôi phục mật khẩu
    /// </summary>
    public class RecoveryAccountInfo
    {
        public int TaiKhoanID { get; set; }
        public string TenDangNhap { get; set; }
        public string HoTen { get; set; }
        public string MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public bool DangHoatDong { get; set; }
        public int SoLanSaiMatKhau { get; set; }
        public DateTime? KhoaDen { get; set; }
        public int? NhanVienID { get; set; }
        public int? KhachHangID { get; set; }

        public bool IsAdmin => string.Equals(MaVaiTro, "ADMIN", StringComparison.OrdinalIgnoreCase);
        public bool IsStaff => string.Equals(MaVaiTro, "NHAN_VIEN", StringComparison.OrdinalIgnoreCase);
        public bool IsCustomer => string.Equals(MaVaiTro, "KHACH_HANG", StringComparison.OrdinalIgnoreCase);

        public bool HasRecoveryChannel => !string.IsNullOrWhiteSpace(SoDienThoai) || !string.IsNullOrWhiteSpace(Email);

        public string MaskedPhone => MaskPhoneString(SoDienThoai);
        public string MaskedEmail => MaskEmailString(Email);

        public static string MaskPhoneString(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "Chưa cập nhật";
            string p = phone.Trim();
            if (p.Length < 7) return "••••••";
            string first2 = p.Substring(0, 2);
            string last3 = p.Substring(p.Length - 3);
            return $"{first2}•• ••• {last3}";
        }

        public static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "Chưa cập nhật";
            string e = email.Trim();
            int atIndex = e.IndexOf('@');
            if (atIndex <= 0) return "******@****";

            string namePart = e.Substring(0, atIndex);
            string domainPart = e.Substring(atIndex);

            if (namePart.Length <= 2)
            {
                return namePart[0] + "***" + domainPart;
            }

            string first2 = namePart.Substring(0, 2);
            int starCount = string.Equals(e, "vanhuy27506@gmail.com", StringComparison.OrdinalIgnoreCase) 
                ? 8 
                : Math.Max(3, namePart.Length - 2);
            return first2 + new string('*', starCount) + domainPart;
        }

        public static string MaskEmailString(string email) => MaskEmail(email);
    }

    /// <summary>
    /// Bản ghi yêu cầu khôi phục mật khẩu trong CSDL (bảng dbo.YeuCauDatLaiMatKhau)
    /// </summary>
    public class PasswordRecoveryRequest
    {
        public long YeuCauDatLaiID { get; set; }
        public int TaiKhoanID { get; set; }
        public string TokenHash { get; set; }
        public DateTime HetHan { get; set; }
        public bool DaSuDung { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySuDung { get; set; }
        public string KenhGui { get; set; }
        public string EmailNhan { get; set; }
        public int SoLanThuSai { get; set; }
        public string TrangThai { get; set; }
        public bool YeuCauAdminHoTro { get; set; }
        public string GhiChu { get; set; }

        public bool IsExpired => HetHan < DateTime.Now;
        public bool IsLocked => SoLanThuSai >= 5;
    }
}
