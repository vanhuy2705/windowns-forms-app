using System;
using System.Collections.Generic;

namespace SportZone.WinForms.Session
{
    public sealed class AppSession
    {
        private static AppSession _current;
        private static readonly object _padlock = new object();

        public static AppSession Current
        {
            get
            {
                lock (_padlock)
                {
                    if (_current == null)
                    {
                        _current = new AppSession();
                    }
                    return _current;
                }
            }
        }

        public int TaiKhoanID { get; set; }
        public int? NhanVienID { get; set; }
        public int? KhachHangID { get; set; }
        public string TenDangNhap { get; set; }
        public string MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }
        public string HoTen { get; set; }
        public string AnhDaiDien { get; set; }
        public bool BatBuocDoiMatKhau { get; set; }
        public HashSet<string> Permissions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool IsLoggedIn => TaiKhoanID > 0;
        public bool IsAuthenticated => IsLoggedIn;
        public bool IsAdmin => string.Equals(MaVaiTro, "ADMIN", StringComparison.OrdinalIgnoreCase);
        public bool IsStaff => string.Equals(MaVaiTro, "NHAN_VIEN", StringComparison.OrdinalIgnoreCase);
        public bool IsCustomer => string.Equals(MaVaiTro, "KHACH_HANG", StringComparison.OrdinalIgnoreCase);

        public bool HasPermission(string permissionCode)
        {
            if (string.IsNullOrEmpty(permissionCode)) return true;
            if (IsAdmin) return true;
            return Permissions.Contains(permissionCode);
        }

        public void RefreshPermissions()
        {
            if (string.IsNullOrEmpty(MaVaiTro)) return;
            try
            {
                var repo = new Repositories.AuthRepository();
                var list = repo.GetPermissions(MaVaiTro);
                Permissions.Clear();
                if (list != null)
                {
                    foreach (var p in list)
                    {
                        if (!string.IsNullOrEmpty(p)) Permissions.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "AppSession.RefreshPermissions");
            }
        }

        public void SetSession(int taiKhoanId, int? nhanVienId, int? khachHangId, string tenDangNhap,
                               string maVaiTro, string tenVaiTro, string hoTen, string anhDaiDien,
                               IEnumerable<string> permissions = null, bool batBuocDoiMatKhau = false)
        {
            TaiKhoanID = taiKhoanId;
            NhanVienID = nhanVienId;
            KhachHangID = khachHangId;
            TenDangNhap = tenDangNhap;
            MaVaiTro = maVaiTro;
            TenVaiTro = tenVaiTro;
            HoTen = hoTen;
            AnhDaiDien = anhDaiDien;
            BatBuocDoiMatKhau = batBuocDoiMatKhau;
            Permissions.Clear();
            if (permissions != null)
            {
                foreach (var p in permissions)
                {
                    if (!string.IsNullOrEmpty(p)) Permissions.Add(p);
                }
            }
        }

        public void SetUser(int taiKhoanId, string tenDangNhap, string maVaiTro, string tenVaiTro,
                            int? nhanVienId, int? khachHangId, string hoTen, string anhDaiDien)
        {
            SetSession(taiKhoanId, nhanVienId, khachHangId, tenDangNhap, maVaiTro, tenVaiTro, hoTen, anhDaiDien, null);
        }

        public void Clear()
        {
            TaiKhoanID = 0;
            NhanVienID = null;
            KhachHangID = null;
            TenDangNhap = null;
            MaVaiTro = null;
            TenVaiTro = null;
            HoTen = null;
            AnhDaiDien = null;
            Permissions.Clear();
        }

        public static void Reset()
        {
            lock (_padlock)
            {
                if (_current != null)
                {
                    _current.Clear();
                }
                _current = null;
            }
        }
    }
}
