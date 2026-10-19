using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SportZone.WinForms.Database;

namespace SportZone.WinForms.Repositories
{
    public class AuthRepository
    {
        public DataTable GetLoginInfo(string username)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_ThongTinDangNhap @TenDangNhap",
                new SqlParameter("@TenDangNhap", SqlDbType.VarChar, 100) { Value = username });
        }

        public void RecordLoginAudit(int taiKhoanId)
        {
            try
            {
                DatabaseManager.ExecuteNonQuery(
                    @"INSERT INTO dbo.NhatKyHeThong (TaiKhoanID, Module, HanhDong, BangDuLieu, KhoaDuLieu, DuLieuMoi, TenMay, ThoiGian)
                      VALUES (@tk, 'AUTH', 'DANG_NHAP', 'TaiKhoan', @tkStr, N'Đăng nhập thành công', @host, SYSDATETIME())",
                    new SqlParameter("@tk", taiKhoanId),
                    new SqlParameter("@tkStr", taiKhoanId.ToString()),
                    new SqlParameter("@host", Environment.MachineName));
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning("RecordLoginAudit failed: " + ex.Message);
            }
        }

        public void ResetFailedLogin(int taiKhoanId)
        {
            try
            {
                DatabaseManager.ExecuteNonQuery(
                    @"UPDATE dbo.TaiKhoan 
                      SET SoLanSaiMatKhau = 0, KhoaDen = NULL, LanDangNhapCuoi = SYSDATETIME(), NgayCapNhat = SYSDATETIME()
                      WHERE TaiKhoanID = @id",
                    new SqlParameter("@id", taiKhoanId));
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning("ResetFailedLogin failed: " + ex.Message);
            }
        }

        public void IncrementFailedLogin(int taiKhoanId, int currentFails)
        {
            try
            {
                int newFails = currentFails + 1;
                object lockUntil = DBNull.Value;
                if (newFails >= 5)
                {
                    lockUntil = DateTime.Now.AddMinutes(15);
                }

                DatabaseManager.ExecuteNonQuery(
                    @"UPDATE dbo.TaiKhoan 
                      SET SoLanSaiMatKhau = @fails, KhoaDen = @lock, NgayCapNhat = SYSDATETIME()
                      WHERE TaiKhoanID = @id",
                    new SqlParameter("@fails", newFails),
                    new SqlParameter("@lock", lockUntil),
                    new SqlParameter("@id", taiKhoanId));
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning("IncrementFailedLogin failed: " + ex.Message);
            }
        }

        public DataTable GetPasswordInfo(int taiKhoanId)
        {
            return DatabaseManager.ExecuteQuery(
                @"SELECT MatKhauHash, MatKhauSalt, ThuatToanMatKhau, SoVongLapMatKhau, BatBuocDoiMatKhau 
                  FROM dbo.TaiKhoan 
                  WHERE TaiKhoanID = @id",
                new SqlParameter("@id", taiKhoanId));
        }

        public void UpdatePassword(int taiKhoanId, string hash, byte[] salt, string thuatToan = "PBKDF2-SHA256", int soVongLap = 10000, bool batBuocDoiMatKhau = false)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_UI_CapNhatMatKhau @TaiKhoanID, @MatKhauHash, @MatKhauSalt, @ThuatToan, @SoVongLap, @BatBuocDoiMatKhau",
                new SqlParameter("@TaiKhoanID", taiKhoanId),
                new SqlParameter("@MatKhauHash", hash),
                new SqlParameter("@MatKhauSalt", (object)salt ?? DBNull.Value),
                new SqlParameter("@ThuatToan", thuatToan),
                new SqlParameter("@SoVongLap", soVongLap),
                new SqlParameter("@BatBuocDoiMatKhau", batBuocDoiMatKhau));
        }

        public List<string> GetPermissions(string maVaiTro)
        {
            var list = new List<string>();
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(
                    @"SELECT q.MaQuyen 
                      FROM dbo.VaiTroQuyen vtq
                      JOIN dbo.VaiTro vt ON vt.VaiTroID = vtq.VaiTroID
                      JOIN dbo.Quyen q ON q.QuyenID = vtq.QuyenID
                      WHERE vt.MaVaiTro = @ma",
                    new SqlParameter("@ma", maVaiTro));

                foreach (DataRow r in dt.Rows)
                {
                    list.Add(r["MaQuyen"]?.ToString() ?? "");
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning($"GetPermissions({maVaiTro}) failed: " + ex.Message);
            }
            return list;
        }

        public List<string> GetPermissions(int vaiTroId)
        {
            var list = new List<string>();
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(
                    @"SELECT q.MaQuyen 
                      FROM dbo.VaiTroQuyen vtq
                      JOIN dbo.Quyen q ON q.QuyenID = vtq.QuyenID
                      WHERE vtq.VaiTroID = @vtId",
                    new SqlParameter("@vtId", vaiTroId));

                foreach (DataRow r in dt.Rows)
                {
                    list.Add(r["MaQuyen"]?.ToString() ?? "");
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning($"GetPermissions({vaiTroId}) failed: " + ex.Message);
            }
            return list;
        }
    }

    public class FieldRepository
    {
        public DataTable GetFields(string tuKhoa = null, int? loaiSanId = null, string trangThaiVanHanh = null, int? cumSanId = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachSan @TuKhoa, @LoaiSanID, @TrangThaiVanHanh, @CumSanID",
                new SqlParameter("@TuKhoa", string.IsNullOrEmpty(tuKhoa) ? (object)DBNull.Value : tuKhoa),
                new SqlParameter("@LoaiSanID", loaiSanId.HasValue ? (object)loaiSanId.Value : DBNull.Value),
                new SqlParameter("@TrangThaiVanHanh", string.IsNullOrEmpty(trangThaiVanHanh) ? (object)DBNull.Value : trangThaiVanHanh),
                new SqlParameter("@CumSanID", cumSanId.HasValue ? (object)cumSanId.Value : DBNull.Value));
        }

        public DataTable GetFieldDetail(int sanId)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_ChiTietSan @SanID",
                new SqlParameter("@SanID", sanId));
        }

        public int SaveField(int? sanId, string maSan, string tenSan, int loaiSanId, int cumSanId,
                             string viTri, decimal giaCoBan, string trangThaiVanHanh, string moTa, bool dangHoatDong)
        {
            object res = DatabaseManager.ExecuteScalar(
                "EXEC dbo.sp_UI_LuuSan @SanID, @MaSan, @TenSan, @LoaiSanID, @CumSanID, @ViTri, @GiaCoBan, @TrangThaiVanHanh, @MoTa, @DangHoatDong",
                new SqlParameter("@SanID", sanId.HasValue && sanId.Value > 0 ? (object)sanId.Value : DBNull.Value),
                new SqlParameter("@MaSan", maSan),
                new SqlParameter("@TenSan", tenSan),
                new SqlParameter("@LoaiSanID", loaiSanId),
                new SqlParameter("@CumSanID", cumSanId),
                new SqlParameter("@ViTri", (object)viTri ?? DBNull.Value),
                new SqlParameter("@GiaCoBan", giaCoBan),
                new SqlParameter("@TrangThaiVanHanh", trangThaiVanHanh ?? "SAN_SANG"),
                new SqlParameter("@MoTa", (object)moTa ?? DBNull.Value),
                new SqlParameter("@DangHoatDong", dangHoatDong));

            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }

        public DataTable GetFieldTypes(string tuKhoa = null, bool chiDangHoatDong = false)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachLoaiSan @TuKhoa, @ChiDangHoatDong",
                new SqlParameter("@TuKhoa", string.IsNullOrEmpty(tuKhoa) ? (object)DBNull.Value : tuKhoa),
                new SqlParameter("@ChiDangHoatDong", chiDangHoatDong));
        }

        public int SaveFieldType(int? loaiSanId, string maLoai, string tenLoai, string moTa,
                                 string kichThuoc, string matSan, int? soNguoi, string iconKey, bool dangHoatDong)
        {
            object res = DatabaseManager.ExecuteScalar(
                "EXEC dbo.sp_UI_LuuLoaiSan @LoaiSanID, @MaLoaiSan, @TenLoaiSan, @MoTa, @KichThuoc, @MatSan, @SoNguoi, @IconKey, @DangHoatDong",
                new SqlParameter("@LoaiSanID", loaiSanId.HasValue && loaiSanId.Value > 0 ? (object)loaiSanId.Value : DBNull.Value),
                new SqlParameter("@MaLoaiSan", maLoai),
                new SqlParameter("@TenLoaiSan", tenLoai),
                new SqlParameter("@MoTa", (object)moTa ?? DBNull.Value),
                new SqlParameter("@KichThuoc", (object)kichThuoc ?? DBNull.Value),
                new SqlParameter("@MatSan", (object)matSan ?? DBNull.Value),
                new SqlParameter("@SoNguoi", soNguoi.HasValue ? (object)soNguoi.Value : DBNull.Value),
                new SqlParameter("@IconKey", (object)iconKey ?? DBNull.Value),
                new SqlParameter("@DangHoatDong", dangHoatDong));

            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }

        public DataTable GetPricing(int? sanId = null, int? loaiSanId = null, DateTime? ngay = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_LayBangGia @SanID, @LoaiSanID, @Ngay",
                new SqlParameter("@SanID", sanId.HasValue ? (object)sanId.Value : DBNull.Value),
                new SqlParameter("@LoaiSanID", loaiSanId.HasValue ? (object)loaiSanId.Value : DBNull.Value),
                new SqlParameter("@Ngay", ngay.HasValue ? (object)ngay.Value.Date : DBNull.Value));
        }

        public DataTable GetMaintenance(int? sanId = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_LayLichBaoTriSan @SanID",
                new SqlParameter("@SanID", sanId.HasValue ? (object)sanId.Value : DBNull.Value));
        }

        public int SavePricing(int? bangGiaId, int? sanId, int? loaiSanId, string tenBangGia,
                               string loaiNgay, byte? thuTrongTuan, TimeSpan gioBatDau,
                               TimeSpan gioKetThuc, decimal donGia, DateTime? hieuLucTu,
                               DateTime? hieuLucDen, int thuTuUuTien, bool dangHoatDong)
        {
            object res = DatabaseManager.ExecuteScalar(
                "EXEC dbo.sp_UI_LuuBangGia @BangGiaID, @SanID, @LoaiSanID, @TenBangGia, @LoaiNgay, @ThuTrongTuan, @GioBatDau, @GioKetThuc, @DonGia, @HieuLucTu, @HieuLucDen, @ThuTuUuTien, @DangHoatDong",
                new SqlParameter("@BangGiaID", bangGiaId.HasValue && bangGiaId.Value > 0 ? (object)bangGiaId.Value : DBNull.Value),
                new SqlParameter("@SanID", sanId.HasValue && sanId.Value > 0 ? (object)sanId.Value : DBNull.Value),
                new SqlParameter("@LoaiSanID", loaiSanId.HasValue && loaiSanId.Value > 0 ? (object)loaiSanId.Value : DBNull.Value),
                new SqlParameter("@TenBangGia", tenBangGia),
                new SqlParameter("@LoaiNgay", loaiNgay ?? "TAT_CA"),
                new SqlParameter("@ThuTrongTuan", thuTrongTuan.HasValue ? (object)thuTrongTuan.Value : DBNull.Value),
                new SqlParameter("@GioBatDau", gioBatDau),
                new SqlParameter("@GioKetThuc", gioKetThuc),
                new SqlParameter("@DonGia", donGia),
                new SqlParameter("@HieuLucTu", hieuLucTu.HasValue ? (object)hieuLucTu.Value.Date : (object)DateTime.Today),
                new SqlParameter("@HieuLucDen", hieuLucDen.HasValue ? (object)hieuLucDen.Value.Date : DBNull.Value),
                new SqlParameter("@ThuTuUuTien", thuTuUuTien),
                new SqlParameter("@DangHoatDong", dangHoatDong));

            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }
    }

    public class CustomerRepository
    {
        public DataTable GetCustomers(string tuKhoa = null, string maHang = null, bool? dangHoatDong = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachKhachHang @TuKhoa, @MaHang, @DangHoatDong",
                new SqlParameter("@TuKhoa", string.IsNullOrEmpty(tuKhoa) ? (object)DBNull.Value : tuKhoa),
                new SqlParameter("@MaHang", string.IsNullOrEmpty(maHang) ? (object)DBNull.Value : maHang),
                new SqlParameter("@DangHoatDong", dangHoatDong.HasValue ? (object)dangHoatDong.Value : DBNull.Value));
        }

        public DataTable GetCustomerProfile(int khachHangId)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_CustomerProfile @KhachHangID",
                new SqlParameter("@KhachHangID", khachHangId));
        }

        public void UpdateCustomer(int khachHangId, string hoTen, string sdt, string email, string diaChi,
                                   DateTime? ngaySinh, string anhDaiDien, bool? daXacMinh, bool? dangHoatDong)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_UI_CapNhatKhachHang @KhachHangID, @HoTen, @SoDienThoai, @Email, @DiaChi, @NgaySinh, @AnhDaiDien, @DaXacMinh, @DangHoatDong",
                new SqlParameter("@KhachHangID", khachHangId),
                new SqlParameter("@HoTen", hoTen),
                new SqlParameter("@SoDienThoai", (object)sdt ?? DBNull.Value),
                new SqlParameter("@Email", (object)email ?? DBNull.Value),
                new SqlParameter("@DiaChi", (object)diaChi ?? DBNull.Value),
                new SqlParameter("@NgaySinh", ngaySinh.HasValue ? (object)ngaySinh.Value.Date : DBNull.Value),
                new SqlParameter("@AnhDaiDien", (object)anhDaiDien ?? DBNull.Value),
                new SqlParameter("@DaXacMinh", daXacMinh.HasValue ? (object)daXacMinh.Value : DBNull.Value),
                new SqlParameter("@DangHoatDong", dangHoatDong.HasValue ? (object)dangHoatDong.Value : DBNull.Value));
        }

        public int CreateCustomer(string hoTen, string sdt, string email, string diaChi, DateTime? ngaySinh)
        {
            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_TaoKhachHang", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@HoTen", hoTen);
                    cmd.Parameters.AddWithValue("@SoDienThoai", (object)sdt ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DiaChi", (object)diaChi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NgaySinh", ngaySinh.HasValue ? (object)ngaySinh.Value.Date : DBNull.Value);

                    var pOut = new SqlParameter("@KhachHangID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pOut);
                    cmd.ExecuteNonQuery();
                    return Convert.ToInt32(pOut.Value);
                }
            }
        }

        public (int KhachHangID, int TaiKhoanID) RegisterCustomer(string hoTen, string sdt, string email,
                                                                 string diaChi, DateTime? ngaySinh,
                                                                 string tenDangNhap, string matKhauHash, byte[] matKhauSalt = null)
        {
            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        int khId = 0;
                        int tkId = 0;
                        using (var cmd = new SqlCommand("dbo.sp_DangKyKhachHang", conn, tran))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@HoTen", hoTen);
                            cmd.Parameters.AddWithValue("@SoDienThoai", sdt);
                            cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@DiaChi", (object)diaChi ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@NgaySinh", ngaySinh.HasValue ? (object)ngaySinh.Value.Date : DBNull.Value);
                            cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                            cmd.Parameters.AddWithValue("@MatKhauHash", matKhauHash);

                            var pKh = new SqlParameter("@KhachHangID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            var pTk = new SqlParameter("@TaiKhoanID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            cmd.Parameters.Add(pKh);
                            cmd.Parameters.Add(pTk);

                            cmd.ExecuteNonQuery();
                            khId = Convert.ToInt32(pKh.Value);
                            tkId = Convert.ToInt32(pTk.Value);
                        }

                        if (matKhauSalt != null && matKhauSalt.Length > 0 && tkId > 0)
                        {
                            using (var cmdSalt = new SqlCommand("dbo.sp_UI_CapNhatMatKhau", conn, tran))
                            {
                                cmdSalt.CommandType = CommandType.StoredProcedure;
                                cmdSalt.Parameters.AddWithValue("@TaiKhoanID", tkId);
                                cmdSalt.Parameters.AddWithValue("@MatKhauHash", matKhauHash);
                                cmdSalt.Parameters.AddWithValue("@MatKhauSalt", matKhauSalt);
                                cmdSalt.Parameters.AddWithValue("@ThuatToan", "PBKDF2-SHA256");
                                cmdSalt.Parameters.AddWithValue("@SoVongLap", 10000);
                                cmdSalt.Parameters.AddWithValue("@BatBuocDoiMatKhau", false);
                                cmdSalt.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        return (khId, tkId);
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public DataSet GetCustomerHome(int khachHangId, DateTime? ngay = null)
        {
            return DatabaseManager.ExecuteDataSet(
                "EXEC dbo.sp_UI_CustomerHome @KhachHangID, @Ngay",
                new SqlParameter("@KhachHangID", khachHangId),
                new SqlParameter("@Ngay", ngay.HasValue ? (object)ngay.Value.Date : DBNull.Value));
        }

        public DataTable GetCustomerBookings(int khachHangId, string trangThai = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_CustomerLichSuDatSan @KhachHangID, @TrangThai",
                new SqlParameter("@KhachHangID", khachHangId),
                new SqlParameter("@TrangThai", string.IsNullOrEmpty(trangThai) ? (object)DBNull.Value : trangThai));
        }

        public DataTable GetCustomerInvoices(int khachHangId)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_CustomerHoaDon @KhachHangID",
                new SqlParameter("@KhachHangID", khachHangId));
        }

        public DataTable GetCustomerVouchers(int khachHangId)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_CustomerVoucher @KhachHangID",
                new SqlParameter("@KhachHangID", khachHangId));
        }

        public DataTable GetMembershipTiers()
        {
            return DatabaseManager.ExecuteQuery(
                "SELECT HangThanhVienID, MaHang, TenHang, ChiTieuToiThieu, TyLeTichDiem, UuDaiMacDinh, MoTa FROM dbo.HangThanhVien WHERE DangHoatDong = 1 ORDER BY ChiTieuToiThieu ASC");
        }

        public void AssignMembershipTier(int khachHangId, int hangThanhVienId, int? taiKhoanId = null)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_GanHangThanhVien @KhachHangID, @HangThanhVienID, @TaiKhoanID",
                new SqlParameter("@KhachHangID", khachHangId),
                new SqlParameter("@HangThanhVienID", hangThanhVienId),
                new SqlParameter("@TaiKhoanID", taiKhoanId.HasValue ? (object)taiKhoanId.Value : DBNull.Value));
        }
    }

    public class EmployeeRepository
    {
        public DataTable GetEmployees(string tuKhoa = null, bool? dangHoatDong = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachNhanVien @TuKhoa, @DangHoatDong",
                new SqlParameter("@TuKhoa", string.IsNullOrEmpty(tuKhoa) ? (object)DBNull.Value : tuKhoa),
                new SqlParameter("@DangHoatDong", dangHoatDong.HasValue ? (object)dangHoatDong.Value : DBNull.Value));
        }

        public void UpdateEmployee(int nhanVienId, string hoTen, string sdt, string email,
                                   string chucVu, DateTime? ngayVaoLam, string diaChi, string anhDaiDien, bool dangHoatDong)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_UI_CapNhatNhanVien @NhanVienID, @HoTen, @SoDienThoai, @Email, @ChucVu, @NgayVaoLam, @DiaChi, @AnhDaiDien, @DangHoatDong",
                new SqlParameter("@NhanVienID", nhanVienId),
                new SqlParameter("@HoTen", hoTen),
                new SqlParameter("@SoDienThoai", (object)sdt ?? DBNull.Value),
                new SqlParameter("@Email", (object)email ?? DBNull.Value),
                new SqlParameter("@ChucVu", (object)chucVu ?? DBNull.Value),
                new SqlParameter("@NgayVaoLam", ngayVaoLam.HasValue ? (object)ngayVaoLam.Value.Date : DBNull.Value),
                new SqlParameter("@DiaChi", (object)diaChi ?? DBNull.Value),
                new SqlParameter("@AnhDaiDien", (object)anhDaiDien ?? DBNull.Value),
                new SqlParameter("@DangHoatDong", dangHoatDong));
        }

        public (int NhanVienID, int TaiKhoanID) CreateEmployeeAndAccount(string hoTen, string sdt, string email,
                                                                        string chucVu, string tenDangNhap, string matKhauHash, byte[] matKhauSalt = null)
        {
            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        int nvId = 0;
                        int tkId = 0;
                        using (var cmd = new SqlCommand("dbo.sp_TaoNhanVienVaTaiKhoan", conn, tran))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@HoTen", hoTen);
                            cmd.Parameters.AddWithValue("@SoDienThoai", (object)sdt ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ChucVu", (object)chucVu ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                            cmd.Parameters.AddWithValue("@MatKhauHash", matKhauHash);

                            var pNv = new SqlParameter("@NhanVienID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            var pTk = new SqlParameter("@TaiKhoanID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            cmd.Parameters.Add(pNv);
                            cmd.Parameters.Add(pTk);

                            cmd.ExecuteNonQuery();
                            nvId = Convert.ToInt32(pNv.Value);
                            tkId = Convert.ToInt32(pTk.Value);
                        }

                        if (matKhauSalt != null && matKhauSalt.Length > 0 && tkId > 0)
                        {
                            using (var cmdSalt = new SqlCommand("dbo.sp_UI_CapNhatMatKhau", conn, tran))
                            {
                                cmdSalt.CommandType = CommandType.StoredProcedure;
                                cmdSalt.Parameters.AddWithValue("@TaiKhoanID", tkId);
                                cmdSalt.Parameters.AddWithValue("@MatKhauHash", matKhauHash);
                                cmdSalt.Parameters.AddWithValue("@MatKhauSalt", matKhauSalt);
                                cmdSalt.Parameters.AddWithValue("@ThuatToan", "PBKDF2-SHA256");
                                cmdSalt.Parameters.AddWithValue("@SoVongLap", 10000);
                                cmdSalt.Parameters.AddWithValue("@BatBuocDoiMatKhau", true);
                                cmdSalt.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        return (nvId, tkId);
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }

    public class BookingRepository
    {
        public DataTable FindAvailableFields(DateTime batDau, DateTime ketThuc, int? loaiSanId = null, int? cumSanId = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_TimSanTrong @BatDau, @KetThuc, @LoaiSanID, @CumSanID",
                new SqlParameter("@BatDau", batDau),
                new SqlParameter("@KetThuc", ketThuc),
                new SqlParameter("@LoaiSanID", loaiSanId.HasValue ? (object)loaiSanId.Value : DBNull.Value),
                new SqlParameter("@CumSanID", cumSanId.HasValue ? (object)cumSanId.Value : DBNull.Value));
        }

        public DataTable CalculatePrice(int sanId, DateTime batDau, DateTime ketThuc)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_TinhGiaDatSan @SanID, @BatDau, @KetThuc",
                new SqlParameter("@SanID", sanId),
                new SqlParameter("@BatDau", batDau),
                new SqlParameter("@KetThuc", ketThuc));
        }

        public (long DonDatSanID, string MaDon) CreateBooking(int khachHangId, int sanId, DateTime batDau,
                                                              DateTime ketThuc, string nguonDon, int? nguoiTaoTaiKhoanId,
                                                              long? voucherKhachHangId, string ghiChu)
        {
            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_TaoDonDatSan", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@KhachHangID", khachHangId);
                    cmd.Parameters.AddWithValue("@SanID", sanId);
                    cmd.Parameters.AddWithValue("@BatDau", batDau);
                    cmd.Parameters.AddWithValue("@KetThuc", ketThuc);
                    cmd.Parameters.AddWithValue("@NguonDon", nguonDon ?? "QUAY");
                    cmd.Parameters.AddWithValue("@NguoiTaoTaiKhoanID", nguoiTaoTaiKhoanId.HasValue ? (object)nguoiTaoTaiKhoanId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@VoucherKhachHangID", voucherKhachHangId.HasValue ? (object)voucherKhachHangId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@GhiChu", (object)ghiChu ?? DBNull.Value);

                    var pDon = new SqlParameter("@DonDatSanID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                    var pMa = new SqlParameter("@MaDon", SqlDbType.VarChar, 40) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pDon);
                    cmd.Parameters.Add(pMa);

                    cmd.ExecuteNonQuery();
                    return (Convert.ToInt64(pDon.Value), pMa.Value?.ToString() ?? "");
                }
            }
        }

        public void AddServiceToBooking(long donDatSanId, int sanPhamDichVuId, decimal soLuong, int? khoId = null, int? taiKhoanId = null)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_ThemDichVuVaoDon @DonDatSanID, @SanPhamDichVuID, @SoLuong, @KhoID, @TaiKhoanID",
                new SqlParameter("@DonDatSanID", donDatSanId),
                new SqlParameter("@SanPhamDichVuID", sanPhamDichVuId),
                new SqlParameter("@SoLuong", soLuong),
                new SqlParameter("@KhoID", khoId.HasValue ? (object)khoId.Value : DBNull.Value),
                new SqlParameter("@TaiKhoanID", taiKhoanId.HasValue ? (object)taiKhoanId.Value : DBNull.Value));
        }

        public DataTable GetBookings(DateTime? tuNgay = null, DateTime? denNgay = null,
                                     string trangThai = null, string tuKhoa = null, int? khachHangId = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachDonDatSan @TuNgay, @DenNgay, @TrangThai, @TuKhoa, @KhachHangID",
                new SqlParameter("@TuNgay", tuNgay.HasValue ? (object)tuNgay.Value.Date : DBNull.Value),
                new SqlParameter("@DenNgay", denNgay.HasValue ? (object)denNgay.Value.Date : DBNull.Value),
                new SqlParameter("@TrangThai", string.IsNullOrEmpty(trangThai) ? (object)DBNull.Value : trangThai),
                new SqlParameter("@TuKhoa", string.IsNullOrEmpty(tuKhoa) ? (object)DBNull.Value : tuKhoa),
                new SqlParameter("@KhachHangID", khachHangId.HasValue ? (object)khachHangId.Value : DBNull.Value));
        }

        public void ConfirmBooking(long donDatSanId, int taiKhoanId)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_XacNhanDon @DonDatSanID, @TaiKhoanID",
                new SqlParameter("@DonDatSanID", donDatSanId),
                new SqlParameter("@TaiKhoanID", taiKhoanId));
        }

        public void CheckIn(long donDatSanId, int taiKhoanId)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_CheckIn @DonDatSanID, @TaiKhoanID",
                new SqlParameter("@DonDatSanID", donDatSanId),
                new SqlParameter("@TaiKhoanID", taiKhoanId));
        }

        public void CompleteBooking(long donDatSanId, int taiKhoanId)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_HoanTatDon @DonDatSanID, @TaiKhoanID",
                new SqlParameter("@DonDatSanID", donDatSanId),
                new SqlParameter("@TaiKhoanID", taiKhoanId));
        }

        public void CancelBooking(long donDatSanId, int? taiKhoanId, string lyDo)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_HuyDon @DonDatSanID, @TaiKhoanID, @LyDo",
                new SqlParameter("@DonDatSanID", donDatSanId),
                new SqlParameter("@TaiKhoanID", taiKhoanId.HasValue ? (object)taiKhoanId.Value : DBNull.Value),
                new SqlParameter("@LyDo", lyDo ?? "Khách yêu cầu hủy đơn"));
        }

        public void RescheduleBooking(long donDatSanId, int sanMoiId, DateTime batDauMoi, DateTime ketThucMoi, int taiKhoanId)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_DoiLich @DonDatSanID, @SanMoiID, @BatDauMoi, @KetThucMoi, @TaiKhoanID",
                new SqlParameter("@DonDatSanID", donDatSanId),
                new SqlParameter("@SanMoiID", sanMoiId),
                new SqlParameter("@BatDauMoi", batDauMoi),
                new SqlParameter("@KetThucMoi", ketThucMoi),
                new SqlParameter("@TaiKhoanID", taiKhoanId));
        }

        public DataTable GetStatusHistory(long donDatSanId)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_LichSuTrangThaiDon @DonDatSanID",
                new SqlParameter("@DonDatSanID", donDatSanId));
        }

        public DataTable GetScheduleByDate(DateTime? ngay = null, int? cumSanId = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_LichSanTheoNgay @Ngay, @CumSanID",
                new SqlParameter("@Ngay", ngay.HasValue ? (object)ngay.Value.Date : DBNull.Value),
                new SqlParameter("@CumSanID", cumSanId.HasValue ? (object)cumSanId.Value : DBNull.Value));
        }
    }

    public class InvoiceRepository
    {
        public (long HoaDonID, string MaHoaDon) CreateInvoice(long donDatSanId, int? nguoiLapId)
        {
            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_TaoHoaDon", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DonDatSanID", donDatSanId);
                    cmd.Parameters.AddWithValue("@NguoiLapID", nguoiLapId.HasValue ? (object)nguoiLapId.Value : DBNull.Value);

                    var pHd = new SqlParameter("@HoaDonID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                    var pMa = new SqlParameter("@MaHoaDon", SqlDbType.VarChar, 40) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pHd);
                    cmd.Parameters.Add(pMa);

                    cmd.ExecuteNonQuery();
                    return (Convert.ToInt64(pHd.Value), pMa.Value?.ToString() ?? "");
                }
            }
        }

        public DataTable GetInvoices(DateTime? tuNgay = null, DateTime? denNgay = null,
                                    string trangThai = null, string tuKhoa = null, int? khachHangId = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachHoaDon @TuNgay, @DenNgay, @TrangThai, @TuKhoa, @KhachHangID",
                new SqlParameter("@TuNgay", tuNgay.HasValue ? (object)tuNgay.Value.Date : DBNull.Value),
                new SqlParameter("@DenNgay", denNgay.HasValue ? (object)denNgay.Value.Date : DBNull.Value),
                new SqlParameter("@TrangThai", string.IsNullOrEmpty(trangThai) ? (object)DBNull.Value : trangThai),
                new SqlParameter("@TuKhoa", string.IsNullOrEmpty(tuKhoa) ? (object)DBNull.Value : tuKhoa),
                new SqlParameter("@KhachHangID", khachHangId.HasValue ? (object)khachHangId.Value : DBNull.Value));
        }
    }

    public class PaymentRepository
    {
        public (long GiaoDichID, string MaGiaoDich) RecordPayment(long hoaDonId, decimal soTien, string phuongThuc,
                                                                 int? nguoiTaoId, string maThamChieu, string ghiChu)
        {
            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_GhiNhanThanhToan", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@HoaDonID", hoaDonId);
                    cmd.Parameters.AddWithValue("@SoTien", soTien);
                    cmd.Parameters.AddWithValue("@PhuongThuc", phuongThuc);
                    cmd.Parameters.AddWithValue("@NguoiTaoID", nguoiTaoId.HasValue ? (object)nguoiTaoId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@MaThamChieu", (object)maThamChieu ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GhiChu", (object)ghiChu ?? DBNull.Value);

                    var pGd = new SqlParameter("@GiaoDichID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                    var pMa = new SqlParameter("@MaGiaoDich", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pGd);
                    cmd.Parameters.Add(pMa);

                    cmd.ExecuteNonQuery();
                    return (Convert.ToInt64(pGd.Value), pMa.Value?.ToString() ?? "");
                }
            }
        }

        public DataTable CreateVietQrRequest(long giaoDichId, int? taiKhoanNganHangId = null, int soPhutHieuLuc = 15)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_TaoYeuCauVietQR @GiaoDichID, @TaiKhoanNganHangID, @SoPhutHieuLuc",
                new SqlParameter("@GiaoDichID", giaoDichId),
                new SqlParameter("@TaiKhoanNganHangID", taiKhoanNganHangId.HasValue ? (object)taiKhoanNganHangId.Value : DBNull.Value),
                new SqlParameter("@SoPhutHieuLuc", soPhutHieuLuc));
        }

        public void ConfirmBankTransfer(long giaoDichId, int taiKhoanXacNhanId, string maThamChieu = null)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_XacNhanChuyenKhoan @GiaoDichID, @TaiKhoanXacNhanID, @MaThamChieu",
                new SqlParameter("@GiaoDichID", giaoDichId),
                new SqlParameter("@TaiKhoanXacNhanID", taiKhoanXacNhanId),
                new SqlParameter("@MaThamChieu", (object)maThamChieu ?? DBNull.Value));
        }

        public void Refund(long giaoDichThanhToanId, decimal soTien, string lyDo, int nguoiDuyetId)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_HoanTien @GiaoDichThanhToanID, @SoTien, @LyDo, @NguoiDuyetID",
                new SqlParameter("@GiaoDichThanhToanID", giaoDichThanhToanId),
                new SqlParameter("@SoTien", soTien),
                new SqlParameter("@LyDo", lyDo ?? "Hoàn tiền cọc theo chính sách"),
                new SqlParameter("@NguoiDuyetID", nguoiDuyetId));
        }

        public DataTable GetPaymentHistory(long? hoaDonId = null, long? donDatSanId = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_LichSuThanhToan @HoaDonID, @DonDatSanID",
                new SqlParameter("@HoaDonID", hoaDonId.HasValue ? (object)hoaDonId.Value : DBNull.Value),
                new SqlParameter("@DonDatSanID", donDatSanId.HasValue ? (object)donDatSanId.Value : DBNull.Value));
        }

        public DataTable GetBankAccounts()
        {
            return DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_ThongTinNganHang");
        }
    }

    public class InventoryRepository
    {
        public DataTable GetServicesAndInventory(string tuKhoa = null, string loai = null, int? khoId = null, bool chiCanhBao = false)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachDichVuTonKho @TuKhoa, @Loai, @KhoID, @ChiCanhBao",
                new SqlParameter("@TuKhoa", string.IsNullOrEmpty(tuKhoa) ? (object)DBNull.Value : tuKhoa),
                new SqlParameter("@Loai", string.IsNullOrEmpty(loai) ? (object)DBNull.Value : loai),
                new SqlParameter("@KhoID", khoId.HasValue ? (object)khoId.Value : DBNull.Value),
                new SqlParameter("@ChiCanhBao", chiCanhBao));
        }

        public void ImportStock(int khoId, int sanPhamDichVuId, decimal soLuong, int? taiKhoanId, string ghiChu)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_NhapKho @KhoID, @SanPhamDichVuID, @SoLuong, @TaiKhoanID, @GhiChu",
                new SqlParameter("@KhoID", khoId),
                new SqlParameter("@SanPhamDichVuID", sanPhamDichVuId),
                new SqlParameter("@SoLuong", soLuong),
                new SqlParameter("@TaiKhoanID", taiKhoanId.HasValue ? (object)taiKhoanId.Value : DBNull.Value),
                new SqlParameter("@GhiChu", (object)ghiChu ?? DBNull.Value));
        }

        public void ExportStock(int khoId, int sanPhamDichVuId, decimal soLuong, string loaiGiaoDich, int? taiKhoanId, string ghiChu)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_XuatKho @KhoID, @SanPhamDichVuID, @SoLuong, @LoaiGiaoDich, @TaiKhoanID, @GhiChu",
                new SqlParameter("@KhoID", khoId),
                new SqlParameter("@SanPhamDichVuID", sanPhamDichVuId),
                new SqlParameter("@SoLuong", soLuong),
                new SqlParameter("@LoaiGiaoDich", loaiGiaoDich ?? "BAN"),
                new SqlParameter("@TaiKhoanID", taiKhoanId.HasValue ? (object)taiKhoanId.Value : DBNull.Value),
                new SqlParameter("@GhiChu", (object)ghiChu ?? DBNull.Value));
        }

        public DataTable GetInventoryHistory(int? sanPhamDichVuId = null, int? khoId = null, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_LichSuKho @SanPhamDichVuID, @KhoID, @TuNgay, @DenNgay",
                new SqlParameter("@SanPhamDichVuID", sanPhamDichVuId.HasValue ? (object)sanPhamDichVuId.Value : DBNull.Value),
                new SqlParameter("@KhoID", khoId.HasValue ? (object)khoId.Value : DBNull.Value),
                new SqlParameter("@TuNgay", tuNgay.HasValue ? (object)tuNgay.Value.Date : DBNull.Value),
                new SqlParameter("@DenNgay", denNgay.HasValue ? (object)denNgay.Value.Date : DBNull.Value));
        }
    }

    public class VoucherRepository
    {
        public DataTable GetVouchers(string tuKhoa = null, bool? dangHoatDong = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachVoucher @TuKhoa, @DangHoatDong",
                new SqlParameter("@TuKhoa", string.IsNullOrEmpty(tuKhoa) ? (object)DBNull.Value : tuKhoa),
                new SqlParameter("@DangHoatDong", dangHoatDong.HasValue ? (object)dangHoatDong.Value : DBNull.Value));
        }

        public int SaveVoucher(int? voucherId, string maVoucher, string tenVoucher, string loaiGiam,
                               decimal giaTriGiam, decimal? giamToiDa, decimal donToiThieu,
                               DateTime ngayBatDau, DateTime ngayKetThuc, int? soLuongPhatHanh,
                               int gioiHanMoiKhach, string moTa, bool dangHoatDong)
        {
            object res = DatabaseManager.ExecuteScalar(
                "EXEC dbo.sp_UI_LuuVoucher @VoucherID, @MaVoucher, @TenVoucher, @LoaiGiam, @GiaTriGiam, @GiamToiDa, @DonToiThieu, @NgayBatDau, @NgayKetThuc, @SoLuongPhatHanh, @GioiHanMoiKhach, @MoTa, @DangHoatDong",
                new SqlParameter("@VoucherID", voucherId.HasValue && voucherId.Value > 0 ? (object)voucherId.Value : DBNull.Value),
                new SqlParameter("@MaVoucher", maVoucher),
                new SqlParameter("@TenVoucher", tenVoucher),
                new SqlParameter("@LoaiGiam", loaiGiam),
                new SqlParameter("@GiaTriGiam", giaTriGiam),
                new SqlParameter("@GiamToiDa", giamToiDa.HasValue ? (object)giamToiDa.Value : DBNull.Value),
                new SqlParameter("@DonToiThieu", donToiThieu),
                new SqlParameter("@NgayBatDau", ngayBatDau),
                new SqlParameter("@NgayKetThuc", ngayKetThuc),
                new SqlParameter("@SoLuongPhatHanh", soLuongPhatHanh.HasValue ? (object)soLuongPhatHanh.Value : DBNull.Value),
                new SqlParameter("@GioiHanMoiKhach", gioiHanMoiKhach),
                new SqlParameter("@MoTa", (object)moTa ?? DBNull.Value),
                new SqlParameter("@DangHoatDong", dangHoatDong));

            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }

        public long AddVoucherToCustomerWallet(int khachHangId, string maVoucher)
        {
            object res = DatabaseManager.ExecuteScalar(
                "EXEC dbo.sp_UI_ThemVoucherVaoViKhach @KhachHangID, @MaVoucher",
                new SqlParameter("@KhachHangID", khachHangId),
                new SqlParameter("@MaVoucher", maVoucher));

            return res != null && res != DBNull.Value ? Convert.ToInt64(res) : 0;
        }

        public void IssueVoucherToCustomer(int voucherId, int khachHangId, int soLuong = 1)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_CapVoucherChoKhachHang @VoucherID, @KhachHangID, @SoLuong",
                new SqlParameter("@VoucherID", voucherId),
                new SqlParameter("@KhachHangID", khachHangId),
                new SqlParameter("@SoLuong", soLuong));
        }

        public (int SuccessCount, int SkippedCount) BatchIssueVouchers(int voucherId, List<int> khachHangIds, int soLuong = 1)
        {
            if (khachHangIds == null || khachHangIds.Count == 0 || soLuong <= 0) return (0, 0);

            int success = 0;
            int skipped = 0;

            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Get voucher quota and individual limits
                        int? soLuongPhatHanh = null;
                        int? gioiHanMoiKhach = null;
                        using (var cmdInfo = new SqlCommand("SELECT SoLuongPhatHanh, GioiHanMoiKhach FROM dbo.Voucher WHERE VoucherID = @VID AND DangHoatDong = 1", conn, tran))
                        {
                            cmdInfo.Parameters.AddWithValue("@VID", voucherId);
                            using (var reader = cmdInfo.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new InvalidOperationException("Voucher không tồn tại hoặc đã ngưng hoạt động.");
                                }
                                if (reader["SoLuongPhatHanh"] != DBNull.Value) soLuongPhatHanh = Convert.ToInt32(reader["SoLuongPhatHanh"]);
                                if (reader["GioiHanMoiKhach"] != DBNull.Value) gioiHanMoiKhach = Convert.ToInt32(reader["GioiHanMoiKhach"]);
                            }
                        }

                        // 2. Count existing total issued vouchers
                        int currentTotalIssued = 0;
                        using (var cmdCount = new SqlCommand("SELECT COUNT(*) FROM dbo.VoucherKhachHang WHERE VoucherID = @VID", conn, tran))
                        {
                            cmdCount.Parameters.AddWithValue("@VID", voucherId);
                            currentTotalIssued = Convert.ToInt32(cmdCount.ExecuteScalar());
                        }

                        // 3. Issue for each customer inside transaction
                        foreach (int khId in khachHangIds)
                        {
                            // Check total quota
                            if (soLuongPhatHanh.HasValue && (currentTotalIssued + soLuong) > soLuongPhatHanh.Value)
                            {
                                skipped++;
                                continue;
                            }

                            // Check customer limit
                            if (gioiHanMoiKhach.HasValue)
                            {
                                int custCount = 0;
                                using (var cmdCust = new SqlCommand("SELECT COUNT(*) FROM dbo.VoucherKhachHang WHERE VoucherID = @VID AND KhachHangID = @KID", conn, tran))
                                {
                                    cmdCust.Parameters.AddWithValue("@VID", voucherId);
                                    cmdCust.Parameters.AddWithValue("@KID", khId);
                                    custCount = Convert.ToInt32(cmdCust.ExecuteScalar());
                                }

                                if ((custCount + soLuong) > gioiHanMoiKhach.Value)
                                {
                                    skipped++;
                                    continue;
                                }
                            }

                            // Call sp_CapVoucherChoKhachHang
                            using (var cmdIssue = new SqlCommand("dbo.sp_CapVoucherChoKhachHang", conn, tran))
                            {
                                cmdIssue.CommandType = CommandType.StoredProcedure;
                                cmdIssue.Parameters.AddWithValue("@VoucherID", voucherId);
                                cmdIssue.Parameters.AddWithValue("@KhachHangID", khId);
                                cmdIssue.Parameters.AddWithValue("@SoLuong", soLuong);
                                cmdIssue.ExecuteNonQuery();
                            }

                            currentTotalIssued += soLuong;
                            success++;
                        }

                        tran.Commit();
                        return (success, skipped);
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public DataTable GetVoucherEffectivenessReport(DateTime tuNgay, DateTime denNgay)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_BaoCaoHieuQuaVoucher @TuNgay, @DenNgay",
                new SqlParameter("@TuNgay", tuNgay.Date),
                new SqlParameter("@DenNgay", denNgay.Date));
        }
    }

    public class PromotionRepository
    {
        public DataTable GetPromotions(string tuKhoa = null, bool? dangHoatDong = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachKhuyenMai @TuKhoa, @DangHoatDong",
                new SqlParameter("@TuKhoa", string.IsNullOrEmpty(tuKhoa) ? (object)DBNull.Value : tuKhoa),
                new SqlParameter("@DangHoatDong", dangHoatDong.HasValue ? (object)dangHoatDong.Value : DBNull.Value));
        }

        public int SavePromotion(int? khuyenMaiId, string maKhuyenMai, string tenKhuyenMai, string loaiGiam,
                                 decimal giaTriGiam, decimal? giamToiDa, decimal donToiThieu,
                                 DateTime ngayBatDau, DateTime ngayKetThuc, TimeSpan? gioBatDau,
                                 TimeSpan? gioKetThuc, string moTa, bool dangHoatDong)
        {
            object res = DatabaseManager.ExecuteScalar(
                "EXEC dbo.sp_UI_LuuKhuyenMai @KhuyenMaiID, @MaKhuyenMai, @TenKhuyenMai, @LoaiGiam, @GiaTriGiam, @GiamToiDa, @DonToiThieu, @NgayBatDau, @NgayKetThuc, @GioBatDau, @GioKetThuc, @MoTa, @DangHoatDong",
                new SqlParameter("@KhuyenMaiID", khuyenMaiId.HasValue && khuyenMaiId.Value > 0 ? (object)khuyenMaiId.Value : DBNull.Value),
                new SqlParameter("@MaKhuyenMai", maKhuyenMai),
                new SqlParameter("@TenKhuyenMai", tenKhuyenMai),
                new SqlParameter("@LoaiGiam", loaiGiam),
                new SqlParameter("@GiaTriGiam", giaTriGiam),
                new SqlParameter("@GiamToiDa", giamToiDa.HasValue ? (object)giamToiDa.Value : DBNull.Value),
                new SqlParameter("@DonToiThieu", donToiThieu),
                new SqlParameter("@NgayBatDau", ngayBatDau),
                new SqlParameter("@NgayKetThuc", ngayKetThuc),
                new SqlParameter("@GioBatDau", gioBatDau.HasValue ? (object)gioBatDau.Value : DBNull.Value),
                new SqlParameter("@GioKetThuc", gioKetThuc.HasValue ? (object)gioKetThuc.Value : DBNull.Value),
                new SqlParameter("@MoTa", (object)moTa ?? DBNull.Value),
                new SqlParameter("@DangHoatDong", dangHoatDong));

            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }
    }

    public class ShiftRepository
    {
        public long OpenShift(int nhanVienId, int? cumSanId, decimal tienDauCa)
        {
            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.sp_MoCa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NhanVienID", nhanVienId);
                    cmd.Parameters.AddWithValue("@CumSanID", cumSanId.HasValue ? (object)cumSanId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TienDauCa", tienDauCa);

                    var pCa = new SqlParameter("@CaLamViecID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pCa);

                    cmd.ExecuteNonQuery();
                    return Convert.ToInt64(pCa.Value);
                }
            }
        }

        public void CloseShift(long caLamViecId, decimal tienMatKiemDem, int? nguoiNhanId, string ghiChu)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_DongCa @CaLamViecID, @TienMatKiemDem, @NguoiNhanID, @GhiChu",
                new SqlParameter("@CaLamViecID", caLamViecId),
                new SqlParameter("@TienMatKiemDem", tienMatKiemDem),
                new SqlParameter("@NguoiNhanID", nguoiNhanId.HasValue ? (object)nguoiNhanId.Value : DBNull.Value),
                new SqlParameter("@GhiChu", (object)ghiChu ?? DBNull.Value));
        }

        public DataSet GetShiftStatistics(int nhanVienId, long? caLamViecId = null)
        {
            return DatabaseManager.ExecuteDataSet(
                "EXEC dbo.sp_UI_StaffThongKeCa @NhanVienID, @CaLamViecID",
                new SqlParameter("@NhanVienID", nhanVienId),
                new SqlParameter("@CaLamViecID", caLamViecId.HasValue ? (object)caLamViecId.Value : DBNull.Value));
        }

        public DataTable GetStaffTodaySchedule(int? nhanVienId = null, DateTime? ngay = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_StaffLichHomNay @NhanVienID, @Ngay",
                new SqlParameter("@NhanVienID", nhanVienId.HasValue ? (object)nhanVienId.Value : DBNull.Value),
                new SqlParameter("@Ngay", ngay.HasValue ? (object)ngay.Value.Date : DBNull.Value));
        }

        public DataTable GetShiftSchedule(DateTime tuNgay, DateTime denNgay, int? nhanVienId = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_LichPhanCa @TuNgay, @DenNgay, @NhanVienID",
                new SqlParameter("@TuNgay", tuNgay.Date),
                new SqlParameter("@DenNgay", denNgay.Date),
                new SqlParameter("@NhanVienID", nhanVienId.HasValue ? (object)nhanVienId.Value : DBNull.Value));
        }

        public long SaveShiftSchedule(long? lichPhanCaId, int nhanVienId, int? cumSanId, DateTime ngayLam,
                                      TimeSpan gioBatDau, TimeSpan gioKetThuc, string trangThai = "DA_XEP", string ghiChu = null)
        {
            object res = DatabaseManager.ExecuteScalar(
                "EXEC dbo.sp_UI_LuuPhanCa @LichPhanCaID, @NhanVienID, @CumSanID, @NgayLam, @GioBatDau, @GioKetThuc, @TrangThai, @GhiChu",
                new SqlParameter("@LichPhanCaID", lichPhanCaId.HasValue && lichPhanCaId.Value > 0 ? (object)lichPhanCaId.Value : DBNull.Value),
                new SqlParameter("@NhanVienID", nhanVienId),
                new SqlParameter("@CumSanID", cumSanId.HasValue && cumSanId.Value > 0 ? (object)cumSanId.Value : DBNull.Value),
                new SqlParameter("@NgayLam", ngayLam.Date),
                new SqlParameter("@GioBatDau", gioBatDau),
                new SqlParameter("@GioKetThuc", gioKetThuc),
                new SqlParameter("@TrangThai", trangThai ?? "DA_XEP"),
                new SqlParameter("@GhiChu", (object)ghiChu ?? DBNull.Value));

            return res != null && res != DBNull.Value ? Convert.ToInt64(res) : 0;
        }
    }

    public class SystemRepository
    {
        public DataTable GetConfigs(string nhom = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachCauHinh @Nhom",
                new SqlParameter("@Nhom", string.IsNullOrEmpty(nhom) ? (object)DBNull.Value : nhom));
        }

        public void SaveConfig(string khoa, string giaTri, string moTa, string nhom, int? taiKhoanId)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_UI_LuuCauHinh @Khoa, @GiaTri, @MoTa, @Nhom, @TaiKhoanID",
                new SqlParameter("@Khoa", khoa),
                new SqlParameter("@GiaTri", giaTri),
                new SqlParameter("@MoTa", (object)moTa ?? DBNull.Value),
                new SqlParameter("@Nhom", (object)nhom ?? DBNull.Value),
                new SqlParameter("@TaiKhoanID", taiKhoanId.HasValue ? (object)taiKhoanId.Value : DBNull.Value));
        }

        public DataTable GetBankAccounts()
        {
            return DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_ThongTinNganHang");
        }

        public int SaveBankAccount(int? id, string tenNganHang, string bin, string stk, string chuTk, string prefix, bool dangHoatDong)
        {
            object res = DatabaseManager.ExecuteScalar(
                "EXEC dbo.sp_UI_LuuTaiKhoanNganHang @TaiKhoanNganHangID, @TenNganHang, @BankBIN, @SoTaiKhoan, @ChuTaiKhoan, @PrefixNoiDung, @DangHoatDong",
                new SqlParameter("@TaiKhoanNganHangID", id.HasValue && id.Value > 0 ? (object)id.Value : DBNull.Value),
                new SqlParameter("@TenNganHang", tenNganHang),
                new SqlParameter("@BankBIN", bin),
                new SqlParameter("@SoTaiKhoan", stk),
                new SqlParameter("@ChuTaiKhoan", chuTk),
                new SqlParameter("@PrefixNoiDung", prefix ?? "SPORTFIELD"),
                new SqlParameter("@DangHoatDong", dangHoatDong));

            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }

        public DataTable GetAuditLogs(DateTime? tuNgay = null, DateTime? denNgay = null, string module = null, string tuKhoa = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_DanhSachNhatKyHeThong @TuNgay, @DenNgay, @Module, @TuKhoa",
                new SqlParameter("@TuNgay", tuNgay.HasValue ? (object)tuNgay.Value.Date : DBNull.Value),
                new SqlParameter("@DenNgay", denNgay.HasValue ? (object)denNgay.Value.Date : DBNull.Value),
                new SqlParameter("@Module", string.IsNullOrEmpty(module) ? (object)DBNull.Value : module),
                new SqlParameter("@TuKhoa", string.IsNullOrEmpty(tuKhoa) ? (object)DBNull.Value : tuKhoa));
        }

        public DataTable HealthCheck()
        {
            return DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_HealthCheck");
        }
    }

    public class ReportRepository
    {
        public DataSet GetDashboardAdmin(DateTime? ngay = null)
        {
            return DatabaseManager.ExecuteDataSet(
                "EXEC dbo.sp_DashboardAdmin @Ngay",
                new SqlParameter("@Ngay", ngay.HasValue ? (object)ngay.Value.Date : DBNull.Value));
        }

        public DataSet GetGeneralReport(DateTime tuNgay, DateTime denNgay)
        {
            return DatabaseManager.ExecuteDataSet(
                "EXEC dbo.sp_UI_BaoCaoTongQuan @TuNgay, @DenNgay",
                new SqlParameter("@TuNgay", tuNgay.Date),
                new SqlParameter("@DenNgay", denNgay.Date));
        }

        public DataSet GetVoucherReport(DateTime tuNgay, DateTime denNgay)
        {
            return DatabaseManager.ExecuteDataSet(
                "EXEC dbo.sp_UI_BaoCaoHieuQuaVoucher @TuNgay, @DenNgay",
                new SqlParameter("@TuNgay", tuNgay.Date),
                new SqlParameter("@DenNgay", denNgay.Date));
        }
    }

    public class MessagingRepository
    {
        public void EnqueueMessage(int? khachHangId, int? taiKhoanId, string kenh, string diaChiNhan, string tieuDe, string noiDung, DateTime? guiSauLuc = null)
        {
            DatabaseManager.ExecuteNonQuery(
                "EXEC dbo.sp_GuiTinNhanVaoHangDoi @KhachHangID, @TaiKhoanID, @Kenh, @DiaChiNhan, @TieuDe, @NoiDung, @GuiSauLuc",
                new SqlParameter("@KhachHangID", khachHangId.HasValue ? (object)khachHangId.Value : DBNull.Value),
                new SqlParameter("@TaiKhoanID", taiKhoanId.HasValue ? (object)taiKhoanId.Value : DBNull.Value),
                new SqlParameter("@Kenh", string.IsNullOrEmpty(kenh) ? "SMS" : kenh.ToUpperInvariant()),
                new SqlParameter("@DiaChiNhan", diaChiNhan ?? ""),
                new SqlParameter("@TieuDe", string.IsNullOrEmpty(tieuDe) ? (object)DBNull.Value : tieuDe),
                new SqlParameter("@NoiDung", noiDung ?? ""),
                new SqlParameter("@GuiSauLuc", guiSauLuc.HasValue ? (object)guiSauLuc.Value : DBNull.Value));
        }
    }

    public class PermissionRepository
    {
        public DataTable GetPermissionMatrix(int? vaiTroId = null)
        {
            return DatabaseManager.ExecuteQuery(
                "EXEC dbo.sp_UI_MaTranQuyen @VaiTroID",
                new SqlParameter("@VaiTroID", vaiTroId.HasValue ? (object)vaiTroId.Value : DBNull.Value));
        }

        public DataTable GetRoles()
        {
            return DatabaseManager.ExecuteQuery(
                "SELECT VaiTroID, MaVaiTro, TenVaiTro, MoTa, DangHoatDong FROM dbo.VaiTro WHERE DangHoatDong = 1 ORDER BY VaiTroID");
        }

        public void SaveRolePermissions(int vaiTroId, List<int> quyenIds)
        {
            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var delCmd = new SqlCommand("DELETE FROM dbo.VaiTroQuyen WHERE VaiTroID = @VT", conn, tran))
                        {
                            delCmd.Parameters.AddWithValue("@VT", vaiTroId);
                            delCmd.ExecuteNonQuery();
                        }

                        if (quyenIds != null && quyenIds.Count > 0)
                        {
                            foreach (int qId in quyenIds)
                            {
                                using (var insCmd = new SqlCommand("INSERT INTO dbo.VaiTroQuyen (VaiTroID, QuyenID) VALUES (@VT, @Q)", conn, tran))
                                {
                                    insCmd.Parameters.AddWithValue("@VT", vaiTroId);
                                    insCmd.Parameters.AddWithValue("@Q", qId);
                                    insCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
