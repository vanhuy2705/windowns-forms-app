using System;
using System.Collections.Generic;
using System.Data;
using SportZone.WinForms.Database;
using SportZone.WinForms.Helpers;
using SportZone.WinForms.Repositories;
using SportZone.WinForms.Security;
using SportZone.WinForms.Session;

namespace SportZone.WinForms.Services
{
    public class AuthService
    {
        private readonly AuthRepository _repo = new AuthRepository();

        public bool Login(string username, string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                errorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return false;
            }

            try
            {
                if (!DatabaseManager.TestConnection().Success)
                {
                    errorMessage = "Không thể kết nối máy chủ dữ liệu. Vui lòng kiểm tra SQL Server.";
                    return false;
                }

                DataTable dt = _repo.GetLoginInfo(username.Trim());
                if (dt == null || dt.Rows.Count == 0)
                {
                    errorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                    return false;
                }

                DataRow row = dt.Rows[0];
                int taiKhoanId = Convert.ToInt32(row["TaiKhoanID"]);
                bool dangHoatDong = row["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(row["DangHoatDong"]);
                if (!dangHoatDong)
                {
                    errorMessage = "Tài khoản đã bị tạm khóa. Vui lòng liên hệ quản trị viên.";
                    return false;
                }

                if (row["KhoaDen"] != DBNull.Value)
                {
                    DateTime khoaDen = Convert.ToDateTime(row["KhoaDen"]);
                    if (khoaDen > DateTime.Now)
                    {
                        errorMessage = $"Tài khoản đang bị tạm khóa đến {khoaDen:HH:mm dd/MM/yyyy} do nhập sai mật khẩu nhiều lần.";
                        return false;
                    }
                }

                string hash = row["MatKhauHash"]?.ToString() ?? string.Empty;
                byte[] salt = row["MatKhauSalt"] != DBNull.Value ? (byte[])row["MatKhauSalt"] : null;
                string thuatToan = row["ThuatToanMatKhau"]?.ToString() ?? "PBKDF2-SHA256";
                int soVongLap = row["SoVongLapMatKhau"] != DBNull.Value ? Convert.ToInt32(row["SoVongLapMatKhau"]) : 10000;
                int soLanSai = row["SoLanSaiMatKhau"] != DBNull.Value ? Convert.ToInt32(row["SoLanSaiMatKhau"]) : 0;

                bool isValid = PasswordSecurity.VerifyPassword(password, hash, salt, soVongLap, thuatToan);
                if (!isValid)
                {
                    _repo.IncrementFailedLogin(taiKhoanId, soLanSai);
                    errorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                    return false;
                }

                // Đăng nhập thành công -> Reset số lần sai và ghi log
                _repo.ResetFailedLogin(taiKhoanId);
                _repo.RecordLoginAudit(taiKhoanId);

                string maVaiTro = row["MaVaiTro"]?.ToString() ?? string.Empty;
                var validRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ADMIN", "NHAN_VIEN", "KHACH_HANG" };
                if (string.IsNullOrWhiteSpace(maVaiTro) || !validRoles.Contains(maVaiTro))
                {
                    errorMessage = $"Tài khoản chưa được gán vai trò hợp lệ trong hệ thống ({maVaiTro}). Vui lòng liên hệ quản trị viên.";
                    return false;
                }

                string tenVaiTro = row["TenVaiTro"]?.ToString() ?? (maVaiTro == "ADMIN" ? "Quản trị viên" : (maVaiTro == "NHAN_VIEN" ? "Nhân viên trực quầy" : "Khách hàng"));
                string hoTen = (row.Table.Columns.Contains("HoTen") && row["HoTen"] != DBNull.Value) ? row["HoTen"].ToString() :
                               ((row.Table.Columns.Contains("HoTenHienThi") && row["HoTenHienThi"] != DBNull.Value) ? row["HoTenHienThi"].ToString() : username);
                string anhDaiDien = (row.Table.Columns.Contains("AnhDaiDien") && row["AnhDaiDien"] != DBNull.Value) ? row["AnhDaiDien"].ToString() : null;
                int? nhanVienId = (row.Table.Columns.Contains("NhanVienID") && row["NhanVienID"] != DBNull.Value) ? (int?)Convert.ToInt32(row["NhanVienID"]) : null;
                int? khachHangId = (row.Table.Columns.Contains("KhachHangID") && row["KhachHangID"] != DBNull.Value) ? (int?)Convert.ToInt32(row["KhachHangID"]) : null;

                List<string> permissions = _repo.GetPermissions(maVaiTro);
                bool batBuocDoiMatKhau = (row.Table.Columns.Contains("BatBuocDoiMatKhau") && row["BatBuocDoiMatKhau"] != DBNull.Value && Convert.ToBoolean(row["BatBuocDoiMatKhau"]));

                // Khởi tạo phiên làm việc AppSession
                AppSession.Current.SetSession(
                    taiKhoanId: taiKhoanId,
                    nhanVienId: nhanVienId,
                    khachHangId: khachHangId,
                    tenDangNhap: username.Trim(),
                    maVaiTro: maVaiTro,
                    tenVaiTro: tenVaiTro,
                    hoTen: hoTen,
                    anhDaiDien: anhDaiDien,
                    permissions: permissions,
                    batBuocDoiMatKhau: batBuocDoiMatKhau
                );

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "AuthService.Login", $"User={username}");
                errorMessage = "Không thể kết nối máy chủ dữ liệu. Vui lòng kiểm tra SQL Server.";
                return false;
            }
        }

        public void Logout()
        {
            AppSession.Current.Clear();
        }

        public bool ChangePassword(int taiKhoanId, string oldPassword, string newPassword, out string errorMessage, bool isAdminReset = false)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                errorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return false;
            }

            try
            {
                DataTable dt = _repo.GetPasswordInfo(taiKhoanId);
                if (dt == null || dt.Rows.Count == 0)
                {
                    errorMessage = "Không tìm thấy thông tin tài khoản.";
                    return false;
                }

                DataRow row = dt.Rows[0];
                string currentHash = row["MatKhauHash"]?.ToString() ?? string.Empty;
                byte[] currentSalt = row["MatKhauSalt"] != DBNull.Value ? (byte[])row["MatKhauSalt"] : null;
                string currentThuatToan = row["ThuatToanMatKhau"]?.ToString() ?? "PBKDF2-SHA256";
                int currentIters = row["SoVongLapMatKhau"] != DBNull.Value ? Convert.ToInt32(row["SoVongLapMatKhau"]) : 10000;

                if (!isAdminReset)
                {
                    if (string.IsNullOrEmpty(oldPassword))
                    {
                        errorMessage = "Vui lòng nhập mật khẩu hiện tại.";
                        return false;
                    }

                    bool isOldValid = PasswordSecurity.VerifyPassword(oldPassword, currentHash, currentSalt, currentIters, currentThuatToan);
                    if (!isOldValid)
                    {
                        errorMessage = "Mật khẩu hiện tại không chính xác.";
                        return false;
                    }
                }

                byte[] salt = PasswordSecurity.GenerateSalt();
                string hash = PasswordSecurity.HashPassword(newPassword, salt, 10000);
                _repo.UpdatePassword(taiKhoanId, hash, salt, "PBKDF2-SHA256", 10000, batBuocDoiMatKhau: false);

                if (AppSession.Current != null && AppSession.Current.TaiKhoanID == taiKhoanId)
                {
                    AppSession.Current.BatBuocDoiMatKhau = false;
                }

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "AuthService.ChangePassword", $"TaiKhoanID={taiKhoanId}");
                errorMessage = AppLogger.GetUserFriendlySqlMessage(ex);
                return false;
            }
        }
    }

    public class FieldService
    {
        private readonly FieldRepository _repo = new FieldRepository();

        public DataTable GetFields(string tuKhoa = null, int? loaiSanId = null, string trangThaiVanHanh = null, int? cumSanId = null)
        {
            return _repo.GetFields(tuKhoa, loaiSanId, trangThaiVanHanh, cumSanId);
        }

        public DataTable GetFieldDetail(int sanId)
        {
            return _repo.GetFieldDetail(sanId);
        }

        public int SaveField(int? sanId, string maSan, string tenSan, int loaiSanId, int cumSanId,
                             string viTri, decimal giaCoBan, string trangThaiVanHanh, string moTa, bool dangHoatDong)
        {
            SessionGuard.RequirePermission(sanId.HasValue && sanId.Value > 0 ? "FIELD_UPDATE" : "FIELD_CREATE");
            return _repo.SaveField(sanId, maSan, tenSan, loaiSanId, cumSanId, viTri, giaCoBan, trangThaiVanHanh, moTa, dangHoatDong);
        }

        public DataTable GetFieldTypes(string tuKhoa = null, bool chiDangHoatDong = false)
        {
            return _repo.GetFieldTypes(tuKhoa, chiDangHoatDong);
        }

        public int SaveFieldType(int? loaiSanId, string maLoai, string tenLoai, string moTa,
                                 string kichThuoc, string matSan, int? soNguoi, string iconKey, bool dangHoatDong)
        {
            SessionGuard.RequirePermission("FIELD_TYPE_MANAGE");
            return _repo.SaveFieldType(loaiSanId, maLoai, tenLoai, moTa, kichThuoc, matSan, soNguoi, iconKey, dangHoatDong);
        }

        public DataTable GetPricing(int? sanId = null, int? loaiSanId = null, DateTime? ngay = null)
        {
            return _repo.GetPricing(sanId, loaiSanId, ngay);
        }

        public DataTable GetMaintenance(int? sanId = null)
        {
            return _repo.GetMaintenance(sanId);
        }

        public int SavePricing(int? bangGiaId, int? sanId, int? loaiSanId, string tenBangGia,
                               string loaiNgay, byte? thuTrongTuan, TimeSpan gioBatDau,
                               TimeSpan gioKetThuc, decimal donGia, DateTime? hieuLucTu,
                               DateTime? hieuLucDen, int thuTuUuTien, bool dangHoatDong)
        {
            SessionGuard.RequirePermission("PRICE_MANAGE");
            return _repo.SavePricing(bangGiaId, sanId, loaiSanId, tenBangGia, loaiNgay, thuTrongTuan, gioBatDau, gioKetThuc, donGia, hieuLucTu, hieuLucDen, thuTuUuTien, dangHoatDong);
        }
    }

    public class CustomerService
    {
        private readonly CustomerRepository _repo = new CustomerRepository();

        public DataTable GetCustomers(string tuKhoa = null, string maHang = null, bool? dangHoatDong = null)
        {
            return _repo.GetCustomers(tuKhoa, maHang, dangHoatDong);
        }

        public DataTable GetCustomerProfile(int khachHangId)
        {
            return _repo.GetCustomerProfile(khachHangId);
        }

        public void UpdateCustomer(int khachHangId, string hoTen, string sdt, string email, string diaChi,
                                   DateTime? ngaySinh, string anhDaiDien, bool? daXacMinh, bool? dangHoatDong)
        {
            SessionGuard.RequirePermission("CUSTOMER_MANAGE");
            _repo.UpdateCustomer(khachHangId, hoTen, sdt, email, diaChi, ngaySinh, anhDaiDien, daXacMinh, dangHoatDong);
        }

        public int CreateCustomer(string hoTen, string sdt, string email, string diaChi, DateTime? ngaySinh)
        {
            SessionGuard.RequirePermission("CUSTOMER_MANAGE");
            return _repo.CreateCustomer(hoTen, sdt, email, diaChi, ngaySinh);
        }

        public (int KhachHangID, int TaiKhoanID) RegisterCustomer(string hoTen, string sdt, string email,
                                                                 string diaChi, DateTime? ngaySinh,
                                                                 string tenDangNhap, string plainPassword)
        {
            byte[] salt = PasswordSecurity.GenerateSalt();
            string hash = PasswordSecurity.HashPassword(plainPassword, salt, 10000);
            return _repo.RegisterCustomer(hoTen, sdt, email, diaChi, ngaySinh, tenDangNhap, hash, salt);
        }

        public DataSet GetCustomerHome(int khachHangId, DateTime? ngay = null)
        {
            return _repo.GetCustomerHome(khachHangId, ngay);
        }

        public DataTable GetCustomerBookings(int khachHangId, string trangThai = null)
        {
            return _repo.GetCustomerBookings(khachHangId, trangThai);
        }

        public DataTable GetCustomerInvoices(int khachHangId)
        {
            return _repo.GetCustomerInvoices(khachHangId);
        }

        public DataTable GetCustomerVouchers(int khachHangId)
        {
            return _repo.GetCustomerVouchers(khachHangId);
        }

        public DataTable GetMembershipTiers()
        {
            return _repo.GetMembershipTiers();
        }

        public void AssignMembershipTier(int khachHangId, int hangThanhVienId, int? taiKhoanId = null)
        {
            SessionGuard.RequirePermission("CUSTOMER_VIP_ASSIGN");
            _repo.AssignMembershipTier(khachHangId, hangThanhVienId, taiKhoanId);
        }
    }

    public class EmployeeService
    {
        private readonly EmployeeRepository _repo = new EmployeeRepository();

        public DataTable GetEmployees(string tuKhoa = null, bool? dangHoatDong = null)
        {
            return _repo.GetEmployees(tuKhoa, dangHoatDong);
        }

        public void UpdateEmployee(int nhanVienId, string hoTen, string sdt, string email,
                                   string chucVu, DateTime? ngayVaoLam, string diaChi, string anhDaiDien, bool dangHoatDong)
        {
            SessionGuard.RequirePermission("EMPLOYEE_MANAGE");
            _repo.UpdateEmployee(nhanVienId, hoTen, sdt, email, chucVu, ngayVaoLam, diaChi, anhDaiDien, dangHoatDong);
        }

        public (int NhanVienID, int TaiKhoanID) CreateEmployeeAndAccount(string hoTen, string sdt, string email,
                                                                        string chucVu, string tenDangNhap, string plainPassword)
        {
            SessionGuard.RequirePermission("EMPLOYEE_MANAGE");
            byte[] salt = PasswordSecurity.GenerateSalt();
            string hash = PasswordSecurity.HashPassword(plainPassword, salt, 10000);
            return _repo.CreateEmployeeAndAccount(hoTen, sdt, email, chucVu, tenDangNhap, hash, salt);
        }
    }

    public class BookingService
    {
        private readonly BookingRepository _repo = new BookingRepository();

        public DataTable FindAvailableFields(DateTime batDau, DateTime ketThuc, int? loaiSanId = null, int? cumSanId = null)
        {
            return _repo.FindAvailableFields(batDau, ketThuc, loaiSanId, cumSanId);
        }

        public DataTable CalculatePrice(int sanId, DateTime batDau, DateTime ketThuc)
        {
            return _repo.CalculatePrice(sanId, batDau, ketThuc);
        }

        public (long DonDatSanID, string MaDon) CreateBooking(int khachHangId, int sanId, DateTime batDau,
                                                              DateTime ketThuc, string nguonDon, int? nguoiTaoTaiKhoanId,
                                                              long? voucherKhachHangId, string ghiChu)
        {
            if (AppSession.Current != null && !AppSession.Current.IsCustomer)
            {
                SessionGuard.RequirePermission("BOOKING_CREATE");
            }
            return _repo.CreateBooking(khachHangId, sanId, batDau, ketThuc, nguonDon, nguoiTaoTaiKhoanId, voucherKhachHangId, ghiChu);
        }

        public void AddServiceToBooking(long donDatSanId, int sanPhamDichVuId, decimal soLuong, int? khoId = null, int? taiKhoanId = null)
        {
            _repo.AddServiceToBooking(donDatSanId, sanPhamDichVuId, soLuong, khoId, taiKhoanId);
        }

        public DataTable GetBookings(DateTime? tuNgay = null, DateTime? denNgay = null,
                                     string trangThai = null, string tuKhoa = null, int? khachHangId = null)
        {
            return _repo.GetBookings(tuNgay, denNgay, trangThai, tuKhoa, khachHangId);
        }

        public void ConfirmBooking(long donDatSanId, int taiKhoanId)
        {
            SessionGuard.RequirePermission("BOOKING_CONFIRM");
            _repo.ConfirmBooking(donDatSanId, taiKhoanId);
        }

        public void CheckIn(long donDatSanId, int taiKhoanId)
        {
            SessionGuard.RequirePermission("BOOKING_CHECKIN");
            _repo.CheckIn(donDatSanId, taiKhoanId);
        }

        public void CompleteBooking(long donDatSanId, int taiKhoanId)
        {
            SessionGuard.RequirePermission("BOOKING_COMPLETE");
            _repo.CompleteBooking(donDatSanId, taiKhoanId);
        }

        public void CancelBooking(long donDatSanId, int? taiKhoanId, string lyDo)
        {
            if (AppSession.Current == null || !AppSession.Current.IsLoggedIn)
            {
                throw new UnauthorizedAccessException("Yêu cầu đăng nhập trước khi thực hiện hủy đơn.");
            }

            if (AppSession.Current.IsCustomer)
            {
                int? chuDonKhachHangId = _repo.GetBookingCustomerId(donDatSanId);
                if (!chuDonKhachHangId.HasValue ||
                    !AppSession.Current.KhachHangID.HasValue ||
                    chuDonKhachHangId.Value != AppSession.Current.KhachHangID.Value)
                {
                    throw new UnauthorizedAccessException("Không có quyền hủy đơn đặt sân của khách hàng khác.");
                }
            }
            else
            {
                SessionGuard.RequirePermission("BOOKING_CANCEL");
            }

            _repo.CancelBooking(donDatSanId, taiKhoanId, lyDo);
        }

        public void RescheduleBooking(long donDatSanId, int sanMoiId, DateTime batDauMoi, DateTime ketThucMoi, int taiKhoanId)
        {
            SessionGuard.RequirePermission("BOOKING_RESCHEDULE");
            _repo.RescheduleBooking(donDatSanId, sanMoiId, batDauMoi, ketThucMoi, taiKhoanId);
        }

        public DataTable GetStatusHistory(long donDatSanId)
        {
            return _repo.GetStatusHistory(donDatSanId);
        }

        public DataTable GetScheduleByDate(DateTime? ngay = null, int? cumSanId = null)
        {
            return _repo.GetScheduleByDate(ngay, cumSanId);
        }
    }

    public class PaymentService
    {
        private readonly PaymentRepository _payRepo = new PaymentRepository();
        private readonly InvoiceRepository _invRepo = new InvoiceRepository();

        public (long HoaDonID, string MaHoaDon) CreateInvoice(long donDatSanId, int? nguoiLapId)
        {
            if (AppSession.Current != null && !AppSession.Current.IsCustomer)
            {
                SessionGuard.RequirePermission("PAYMENT_CREATE");
            }
            return _invRepo.CreateInvoice(donDatSanId, nguoiLapId);
        }

        public DataTable GetInvoices(DateTime? tuNgay = null, DateTime? denNgay = null,
                                     string trangThai = null, string tuKhoa = null, int? khachHangId = null)
        {
            return _invRepo.GetInvoices(tuNgay, denNgay, trangThai, tuKhoa, khachHangId);
        }

        public (long GiaoDichID, string MaGiaoDich) RecordPayment(long hoaDonId, decimal soTien, string phuongThuc,
                                                                 int? nguoiTaoId, string maThamChieu, string ghiChu)
        {
            if (AppSession.Current != null && !AppSession.Current.IsCustomer && phuongThuc != "VIETQR")
            {
                SessionGuard.RequirePermission("PAYMENT_CREATE");
            }
            return _payRepo.RecordPayment(hoaDonId, soTien, phuongThuc, nguoiTaoId, maThamChieu, ghiChu);
        }

        public DataTable CreateVietQrRequest(long giaoDichId, int? taiKhoanNganHangId = null, int soPhutHieuLuc = 15)
        {
            return _payRepo.CreateVietQrRequest(giaoDichId, taiKhoanNganHangId, soPhutHieuLuc);
        }

        public void ConfirmBankTransfer(long giaoDichId, int taiKhoanXacNhanId, string maThamChieu = null)
        {
            SessionGuard.RequirePermission("PAYMENT_CONFIRM_TRANSFER");
            _payRepo.ConfirmBankTransfer(giaoDichId, taiKhoanXacNhanId, maThamChieu);
        }

        public void Refund(long giaoDichThanhToanId, decimal soTien, string lyDo, int nguoiDuyetId)
        {
            SessionGuard.RequirePermission("REFUND_MANAGE");
            _payRepo.Refund(giaoDichThanhToanId, soTien, lyDo, nguoiDuyetId);
        }

        public DataTable GetPaymentHistory(long? hoaDonId = null, long? donDatSanId = null)
        {
            return _payRepo.GetPaymentHistory(hoaDonId, donDatSanId);
        }

        public DataTable GetBankAccounts()
        {
            return _payRepo.GetBankAccounts();
        }

        public DataTable GetInvoiceTotals(long hoaDonId)
        {
            return _invRepo.GetInvoiceTotals(hoaDonId);
        }

        public DataTable GetInvoiceTotalsByBooking(long donDatSanId)
        {
            return _invRepo.GetInvoiceTotalsByBooking(donDatSanId);
        }

        public decimal? GetInvoiceTotal(long hoaDonId)
        {
            DataTable dt = _invRepo.GetInvoiceTotals(hoaDonId);
            if (dt == null || dt.Rows.Count == 0) return null;
            object val = dt.Rows[0]["TongTien"];
            if (val == null || val == DBNull.Value) return null;
            return Convert.ToDecimal(val);
        }
    }

    public class InventoryService
    {
        private readonly InventoryRepository _repo = new InventoryRepository();

        public DataTable GetServicesAndInventory(string tuKhoa = null, string loai = null, int? khoId = null, bool chiCanhBao = false)
        {
            return _repo.GetServicesAndInventory(tuKhoa, loai, khoId, chiCanhBao);
        }

        public void ImportStock(int khoId, int sanPhamDichVuId, decimal soLuong, int? taiKhoanId, string ghiChu)
        {
            SessionGuard.RequirePermission("INVENTORY_IMPORT");
            _repo.ImportStock(khoId, sanPhamDichVuId, soLuong, taiKhoanId, ghiChu);
        }

        public void ExportStock(int khoId, int sanPhamDichVuId, decimal soLuong, string loaiGiaoDich, int? taiKhoanId, string ghiChu)
        {
            SessionGuard.RequirePermission("INVENTORY_EXPORT");
            _repo.ExportStock(khoId, sanPhamDichVuId, soLuong, loaiGiaoDich, taiKhoanId, ghiChu);
        }

        public DataTable GetInventoryHistory(int? sanPhamDichVuId = null, int? khoId = null, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            return _repo.GetInventoryHistory(sanPhamDichVuId, khoId, tuNgay, denNgay);
        }
    }

    public class ShiftService
    {
        private readonly ShiftRepository _repo = new ShiftRepository();

        public long OpenShift(int nhanVienId, int? cumSanId, decimal tienDauCa)
        {
            return _repo.OpenShift(nhanVienId, cumSanId, tienDauCa);
        }

        public void CloseShift(long caLamViecId, decimal tienMatKiemDem, int? nguoiNhanId, string ghiChu)
        {
            _repo.CloseShift(caLamViecId, tienMatKiemDem, nguoiNhanId, ghiChu);
        }

        public DataSet GetShiftStatistics(int nhanVienId, long? caLamViecId = null)
        {
            return _repo.GetShiftStatistics(nhanVienId, caLamViecId);
        }

        public DataTable GetStaffTodaySchedule(int? nhanVienId = null, DateTime? ngay = null)
        {
            return _repo.GetStaffTodaySchedule(nhanVienId, ngay);
        }

        public DataTable GetShiftSchedule(DateTime tuNgay, DateTime denNgay, int? nhanVienId = null)
        {
            return _repo.GetShiftSchedule(tuNgay, denNgay, nhanVienId);
        }

        public long SaveShiftSchedule(long? lichPhanCaId, int nhanVienId, int? cumSanId, DateTime ngayLam,
                                      TimeSpan gioBatDau, TimeSpan gioKetThuc, string trangThai = "DA_XEP", string ghiChu = null)
        {
            SessionGuard.RequirePermission("SHIFT_MANAGE_ALL");
            return _repo.SaveShiftSchedule(lichPhanCaId, nhanVienId, cumSanId, ngayLam, gioBatDau, gioKetThuc, trangThai, ghiChu);
        }
    }

    public class VoucherService
    {
        private readonly VoucherRepository _repo = new VoucherRepository();

        public DataTable GetVouchers(string tuKhoa = null, bool? dangHoatDong = null)
        {
            return _repo.GetVouchers(tuKhoa, dangHoatDong);
        }

        public int SaveVoucher(int? voucherId, string maVoucher, string tenVoucher, string loaiGiam,
                               decimal giaTriGiam, decimal? giamToiDa, decimal donToiThieu,
                               DateTime ngayBatDau, DateTime ngayKetThuc, int? soLuongPhatHanh,
                               int gioiHanMoiKhach, string moTa, bool dangHoatDong)
        {
            SessionGuard.RequirePermission("VOUCHER_MANAGE");
            return _repo.SaveVoucher(voucherId, maVoucher, tenVoucher, loaiGiam, giaTriGiam, giamToiDa,
                                     donToiThieu, ngayBatDau, ngayKetThuc, soLuongPhatHanh,
                                     gioiHanMoiKhach, moTa, dangHoatDong);
        }

        public long AddVoucherToCustomerWallet(int khachHangId, string maVoucher)
        {
            return _repo.AddVoucherToCustomerWallet(khachHangId, maVoucher);
        }

        public void IssueVoucherToCustomer(int voucherId, int khachHangId, int soLuong = 1)
        {
            SessionGuard.RequirePermission("VOUCHER_ISSUE");
            _repo.IssueVoucherToCustomer(voucherId, khachHangId, soLuong);
        }

        public (int SuccessCount, int SkippedCount) BatchIssueVouchers(int voucherId, List<int> khachHangIds, int soLuong = 1)
        {
            SessionGuard.RequirePermission("VOUCHER_ISSUE");
            return _repo.BatchIssueVouchers(voucherId, khachHangIds, soLuong);
        }

        public DataTable GetVoucherEffectivenessReport(DateTime tuNgay, DateTime denNgay)
        {
            return _repo.GetVoucherEffectivenessReport(tuNgay, denNgay);
        }
    }

    public class MessagingService
    {
        private readonly MessagingRepository _repo = new MessagingRepository();

        public void EnqueueMessage(int? khachHangId, int? taiKhoanId, string kenh, string diaChiNhan, string tieuDe, string noiDung, DateTime? guiSauLuc = null)
        {
            _repo.EnqueueMessage(khachHangId, taiKhoanId, kenh, diaChiNhan, tieuDe, noiDung, guiSauLuc);
        }
    }

    public class PermissionService
    {
        private readonly PermissionRepository _repo = new PermissionRepository();

        public DataTable GetPermissionMatrix(int? vaiTroId = null)
        {
            return _repo.GetPermissionMatrix(vaiTroId);
        }

        public DataTable GetRoles()
        {
            return _repo.GetRoles();
        }

        public void SaveRolePermissions(int vaiTroId, List<int> quyenIds)
        {
            SessionGuard.RequirePermission("ACCOUNT_MANAGE");
            _repo.SaveRolePermissions(vaiTroId, quyenIds);
            AppSession.Current.RefreshPermissions();
        }
    }

    public class PromotionService
    {
        private readonly PromotionRepository _repo = new PromotionRepository();

        public DataTable GetPromotions(string tuKhoa = null, bool? dangHoatDong = null)
        {
            return _repo.GetPromotions(tuKhoa, dangHoatDong);
        }

        public int SavePromotion(int? khuyenMaiId, string maKhuyenMai, string tenKhuyenMai, string loaiGiam,
                                 decimal giaTriGiam, decimal? giamToiDa, decimal donToiThieu,
                                 DateTime ngayBatDau, DateTime ngayKetThuc, TimeSpan? gioBatDau,
                                 TimeSpan? gioKetThuc, string moTa, bool dangHoatDong)
        {
            SessionGuard.RequirePermission("PROMOTION_MANAGE");
            return _repo.SavePromotion(khuyenMaiId, maKhuyenMai, tenKhuyenMai, loaiGiam, giaTriGiam,
                                       giamToiDa, donToiThieu, ngayBatDau, ngayKetThuc, gioBatDau,
                                       gioKetThuc, moTa, dangHoatDong);
        }
    }

    public class SystemService
    {
        private readonly SystemRepository _repo = new SystemRepository();

        public DataTable GetConfigs(string nhom = null)
        {
            return _repo.GetConfigs(nhom);
        }

        public void SaveConfig(string khoa, string giaTri, string moTa, string nhom, int? taiKhoanId)
        {
            SessionGuard.RequirePermission("SETTING_MANAGE");
            _repo.SaveConfig(khoa, giaTri, moTa, nhom, taiKhoanId);
        }

        public DataTable GetBankAccounts()
        {
            return _repo.GetBankAccounts();
        }

        public int SaveBankAccount(int? id, string tenNganHang, string bin, string stk, string chuTk, string prefix, bool dangHoatDong)
        {
            SessionGuard.RequirePermission("SETTING_MANAGE");
            return _repo.SaveBankAccount(id, tenNganHang, bin, stk, chuTk, prefix, dangHoatDong);
        }

        public DataTable GetAuditLogs(DateTime? tuNgay = null, DateTime? denNgay = null, string module = null, string tuKhoa = null)
        {
            return _repo.GetAuditLogs(tuNgay, denNgay, module, tuKhoa);
        }

        public DataTable HealthCheck()
        {
            return _repo.HealthCheck();
        }
    }

    public class ReportService
    {
        private readonly ReportRepository _repo = new ReportRepository();

        public DataSet GetDashboardAdmin(DateTime? ngay = null)
        {
            return _repo.GetDashboardAdmin(ngay);
        }

        public DataSet GetGeneralReport(DateTime tuNgay, DateTime denNgay)
        {
            return _repo.GetGeneralReport(tuNgay, denNgay);
        }

        public DataSet GetVoucherReport(DateTime tuNgay, DateTime denNgay)
        {
            return _repo.GetVoucherReport(tuNgay, denNgay);
        }
    }
}
