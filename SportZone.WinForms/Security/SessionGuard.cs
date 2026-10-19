using System;
using SportZone.WinForms.Session;

namespace SportZone.WinForms.Security
{
    public static class SessionGuard
    {
        public static int RequireAccountId()
        {
            if (AppSession.Current == null || AppSession.Current.TaiKhoanID <= 0)
            {
                throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại.");
            }
            return AppSession.Current.TaiKhoanID;
        }

        public static int RequireCustomerId()
        {
            RequireAccountId();
            if (!AppSession.Current.KhachHangID.HasValue || AppSession.Current.KhachHangID.Value <= 0)
            {
                throw new UnauthorizedAccessException("Tài khoản hiện tại không gắn với hồ sơ khách hàng hợp lệ.");
            }
            return AppSession.Current.KhachHangID.Value;
        }

        public static int RequireEmployeeId()
        {
            RequireAccountId();
            if (!AppSession.Current.NhanVienID.HasValue || AppSession.Current.NhanVienID.Value <= 0)
            {
                throw new UnauthorizedAccessException("Tài khoản hiện tại không gắn với hồ sơ nhân viên hợp lệ.");
            }
            return AppSession.Current.NhanVienID.Value;
        }

        public static bool HasPermission(string permissionCode)
        {
            if (AppSession.Current == null || AppSession.Current.TaiKhoanID <= 0) return false;
            return AppSession.Current.HasPermission(permissionCode);
        }

        public static void RequirePermission(string permissionCode)
        {
            RequireAccountId();
            if (!HasPermission(permissionCode))
            {
                throw new UnauthorizedAccessException($"Bạn không có quyền thực hiện chức năng này ({permissionCode}).");
            }
        }
    }
}
