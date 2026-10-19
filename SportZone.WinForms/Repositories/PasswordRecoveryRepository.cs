using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SportZone.WinForms.Database;
using SportZone.WinForms.Helpers;
using SportZone.WinForms.Models;

namespace SportZone.WinForms.Repositories
{
    public class PasswordRecoveryRepository
    {
        public RecoveryAccountInfo FindRecoveryAccount(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return null;
            string idTrim = identifier.Trim();

            const string sql = @"
                SELECT TOP 1 
                    tk.TaiKhoanID,
                    tk.TenDangNhap,
                    tk.DangHoatDong,
                    tk.SoLanSaiMatKhau,
                    tk.KhoaDen,
                    tk.NhanVienID,
                    tk.KhachHangID,
                    vt.MaVaiTro,
                    vt.TenVaiTro,
                    nv.HoTen AS HoTenNV,
                    nv.SoDienThoai AS SdtNV,
                    nv.Email AS EmailNV,
                    kh.HoTen AS HoTenKH,
                    kh.SoDienThoai AS SdtKH,
                    kh.Email AS EmailKH
                FROM dbo.TaiKhoan tk
                JOIN dbo.VaiTro vt ON tk.VaiTroID = vt.VaiTroID
                LEFT JOIN dbo.NhanVien nv ON tk.NhanVienID = nv.NhanVienID
                LEFT JOIN dbo.KhachHang kh ON tk.KhachHangID = kh.KhachHangID
                WHERE tk.TenDangNhap = @Identifier
                   OR (nv.Email IS NOT NULL AND nv.Email = @Identifier)
                   OR (nv.SoDienThoai IS NOT NULL AND nv.SoDienThoai = @Identifier)
                   OR (kh.Email IS NOT NULL AND kh.Email = @Identifier)
                   OR (kh.SoDienThoai IS NOT NULL AND kh.SoDienThoai = @Identifier)";

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@Identifier", idTrim));
                if (dt == null || dt.Rows.Count == 0) return null;

                return MapAccount(dt.Rows[0]);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PasswordRecoveryRepository.FindRecoveryAccount", $"Identifier={idTrim}");
                throw;
            }
        }

        public RecoveryAccountInfo GetAccountById(int taiKhoanId)
        {
            if (taiKhoanId <= 0) return null;

            const string sql = @"
                SELECT TOP 1 
                    tk.TaiKhoanID,
                    tk.TenDangNhap,
                    tk.DangHoatDong,
                    tk.SoLanSaiMatKhau,
                    tk.KhoaDen,
                    tk.NhanVienID,
                    tk.KhachHangID,
                    vt.MaVaiTro,
                    vt.TenVaiTro,
                    nv.HoTen AS HoTenNV,
                    nv.SoDienThoai AS SdtNV,
                    nv.Email AS EmailNV,
                    kh.HoTen AS HoTenKH,
                    kh.SoDienThoai AS SdtKH,
                    kh.Email AS EmailKH
                FROM dbo.TaiKhoan tk
                JOIN dbo.VaiTro vt ON tk.VaiTroID = vt.VaiTroID
                LEFT JOIN dbo.NhanVien nv ON tk.NhanVienID = nv.NhanVienID
                LEFT JOIN dbo.KhachHang kh ON tk.KhachHangID = kh.KhachHangID
                WHERE tk.TaiKhoanID = @TaiKhoanID";

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@TaiKhoanID", taiKhoanId));
                if (dt == null || dt.Rows.Count == 0) return null;

                return MapAccount(dt.Rows[0]);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PasswordRecoveryRepository.GetAccountById", $"TaiKhoanID={taiKhoanId}");
                throw;
            }
        }

        private RecoveryAccountInfo MapAccount(DataRow r)
        {
            var acc = new RecoveryAccountInfo
            {
                TaiKhoanID = Convert.ToInt32(r["TaiKhoanID"]),
                TenDangNhap = r["TenDangNhap"]?.ToString() ?? "",
                DangHoatDong = r["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(r["DangHoatDong"]),
                SoLanSaiMatKhau = r["SoLanSaiMatKhau"] != DBNull.Value ? Convert.ToInt32(r["SoLanSaiMatKhau"]) : 0,
                KhoaDen = r["KhoaDen"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["KhoaDen"]) : null,
                MaVaiTro = r["MaVaiTro"]?.ToString() ?? "",
                TenVaiTro = r["TenVaiTro"]?.ToString() ?? "",
                NhanVienID = r["NhanVienID"] != DBNull.Value ? (int?)Convert.ToInt32(r["NhanVienID"]) : null,
                KhachHangID = r["KhachHangID"] != DBNull.Value ? (int?)Convert.ToInt32(r["KhachHangID"]) : null
            };

            if (acc.NhanVienID.HasValue)
            {
                acc.HoTen = r["HoTenNV"]?.ToString();
                acc.SoDienThoai = r["SdtNV"]?.ToString();
                acc.Email = r["EmailNV"]?.ToString();
            }
            else if (acc.KhachHangID.HasValue)
            {
                acc.HoTen = r["HoTenKH"]?.ToString();
                acc.SoDienThoai = r["SdtKH"]?.ToString();
                acc.Email = r["EmailKH"]?.ToString();
            }

            if (string.IsNullOrWhiteSpace(acc.HoTen))
            {
                acc.HoTen = acc.TenDangNhap;
            }

            return acc;
        }

        public int GetRecentRequestCount(int taiKhoanId, int minutes = 15)
        {
            const string sql = @"
                SELECT COUNT(*) 
                FROM dbo.YeuCauDatLaiMatKhau 
                WHERE TaiKhoanID = @TaiKhoanID 
                  AND NgayTao >= DATEADD(minute, -@Minutes, SYSDATETIME())";

            object obj = DatabaseManager.ExecuteScalar(sql,
                new SqlParameter("@TaiKhoanID", taiKhoanId),
                new SqlParameter("@Minutes", minutes));

            return (obj != null && obj != DBNull.Value) ? Convert.ToInt32(obj) : 0;
        }

        public PasswordRecoveryRequest GetLatestRequest(int taiKhoanId)
        {
            const string sql = @"
                SELECT TOP 1 * 
                FROM dbo.YeuCauDatLaiMatKhau 
                WHERE TaiKhoanID = @TaiKhoanID 
                ORDER BY YeuCauDatLaiID DESC";

            DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@TaiKhoanID", taiKhoanId));
            if (dt == null || dt.Rows.Count == 0) return null;
            return MapRequest(dt.Rows[0]);
        }

        public long CreateRecoveryRequest(int taiKhoanId, string otpHash, string kenhGui, int expiryMinutes = 5, string emailNhan = null)
        {
            const string sql = @"
                INSERT INTO dbo.YeuCauDatLaiMatKhau (
                    TaiKhoanID, TokenHash, HetHan, DaSuDung, NgayTao, KenhGui, EmailNhan, SoLanThuSai, TrangThai, YeuCauAdminHoTro
                )
                OUTPUT INSERTED.YeuCauDatLaiID
                VALUES (
                    @TaiKhoanID, @TokenHash, DATEADD(minute, @Expiry, SYSDATETIME()), 0, SYSDATETIME(), @KenhGui, @EmailNhan, 0, 'PENDING', 0
                )";

            object obj = DatabaseManager.ExecuteScalar(sql,
                new SqlParameter("@TaiKhoanID", taiKhoanId),
                new SqlParameter("@TokenHash", otpHash ?? ""),
                new SqlParameter("@Expiry", expiryMinutes),
                new SqlParameter("@KenhGui", (object)kenhGui ?? "EMAIL"),
                new SqlParameter("@EmailNhan", (object)emailNhan ?? DBNull.Value));

            return Convert.ToInt64(obj);
        }

        public void InvalidateRequest(long yeuCauDatLaiId)
        {
            const string sql = "UPDATE dbo.YeuCauDatLaiMatKhau SET TrangThai = 'INVALIDATED', DaSuDung = 1 WHERE YeuCauDatLaiID = @ID";
            DatabaseManager.ExecuteNonQuery(sql, new SqlParameter("@ID", yeuCauDatLaiId));
        }

        public void EnqueueOtpMessage(int taiKhoanId, int? khachHangId, string kenh, string destination, string otp)
        {
            const string sql = @"
                INSERT INTO dbo.HangDoiTinNhan (
                    KhachHangID, TaiKhoanID, Kenh, DiaChiNhan, TieuDe, NoiDung, TrangThai, SoLanThu, NgayTao
                )
                VALUES (
                    @KhachHangID, @TaiKhoanID, @Kenh, @DiaChiNhan, @TieuDe, @NoiDung, 'CHO_GUI', 0, SYSDATETIME()
                )";

            string content = $"Mã xác minh SPORTFIELD của bạn là: {otp}. Mã có hiệu lực trong 5 phút. Tuyệt đối không chia sẻ mã này cho bất kỳ ai.";

            DatabaseManager.ExecuteNonQuery(sql,
                new SqlParameter("@KhachHangID", (object)khachHangId ?? DBNull.Value),
                new SqlParameter("@TaiKhoanID", taiKhoanId),
                new SqlParameter("@Kenh", (object)kenh ?? "SMS"),
                new SqlParameter("@DiaChiNhan", (object)destination ?? ""),
                new SqlParameter("@TieuDe", "Mã xác minh khôi phục mật khẩu"),
                new SqlParameter("@NoiDung", content));
        }

        public PasswordRecoveryRequest GetRecoveryRequest(long yeuCauDatLaiId)
        {
            const string sql = "SELECT TOP 1 * FROM dbo.YeuCauDatLaiMatKhau WHERE YeuCauDatLaiID = @ID";
            DataTable dt = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@ID", yeuCauDatLaiId));
            if (dt == null || dt.Rows.Count == 0) return null;
            return MapRequest(dt.Rows[0]);
        }

        public void IncrementFailedOtpAttempt(long yeuCauDatLaiId)
        {
            const string sql = @"
                UPDATE dbo.YeuCauDatLaiMatKhau 
                SET SoLanThuSai = SoLanThuSai + 1,
                    TrangThai = CASE WHEN SoLanThuSai + 1 >= 5 THEN 'LOCKED' ELSE TrangThai END
                WHERE YeuCauDatLaiID = @ID";

            DatabaseManager.ExecuteNonQuery(sql, new SqlParameter("@ID", yeuCauDatLaiId));
        }

        public void MarkOtpVerified(long yeuCauDatLaiId)
        {
            const string sql = "UPDATE dbo.YeuCauDatLaiMatKhau SET TrangThai = 'VERIFIED' WHERE YeuCauDatLaiID = @ID";
            DatabaseManager.ExecuteNonQuery(sql, new SqlParameter("@ID", yeuCauDatLaiId));
        }

        public bool ResetPasswordAtomic(long yeuCauDatLaiId, int taiKhoanId, string newPasswordHash, byte[] salt, string thuatToan = "PBKDF2-SHA256", int soVongLap = 10000)
        {
            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // 1. Kiểm tra tính hợp lệ của request
                        using (var checkCmd = conn.CreateCommand())
                        {
                            checkCmd.Transaction = trans;
                            checkCmd.CommandText = @"
                                SELECT TrangThai, DaSuDung, HetHan 
                                FROM dbo.YeuCauDatLaiMatKhau 
                                WHERE YeuCauDatLaiID = @ID AND TaiKhoanID = @TaiKhoanID";
                            checkCmd.Parameters.AddWithValue("@ID", yeuCauDatLaiId);
                            checkCmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);

                            using (var rdr = checkCmd.ExecuteReader())
                            {
                                if (!rdr.Read())
                                {
                                    trans.Rollback();
                                    return false;
                                }
                                string status = rdr["TrangThai"]?.ToString() ?? "";
                                bool used = Convert.ToBoolean(rdr["DaSuDung"]);
                                DateTime hetHan = Convert.ToDateTime(rdr["HetHan"]);

                                if (status != "VERIFIED" || used || hetHan < DateTime.Now)
                                {
                                    rdr.Close();
                                    trans.Rollback();
                                    return false;
                                }
                            }
                        }

                        // 2. Cập nhật mật khẩu qua stored procedure chuẩn
                        using (var updatePwdCmd = conn.CreateCommand())
                        {
                            updatePwdCmd.Transaction = trans;
                            updatePwdCmd.CommandType = CommandType.StoredProcedure;
                            updatePwdCmd.CommandText = "dbo.sp_UI_CapNhatMatKhau";
                            updatePwdCmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            updatePwdCmd.Parameters.AddWithValue("@MatKhauHash", newPasswordHash);
                            updatePwdCmd.Parameters.AddWithValue("@MatKhauSalt", (object)salt ?? DBNull.Value);
                            updatePwdCmd.Parameters.AddWithValue("@ThuatToan", thuatToan);
                            updatePwdCmd.Parameters.AddWithValue("@SoVongLap", soVongLap);
                            updatePwdCmd.Parameters.AddWithValue("@BatBuocDoiMatKhau", false);
                            updatePwdCmd.ExecuteNonQuery();
                        }

                        // 3. Đánh dấu request này đã hoàn tất
                        using (var markUsedCmd = conn.CreateCommand())
                        {
                            markUsedCmd.Transaction = trans;
                            markUsedCmd.CommandText = @"
                                UPDATE dbo.YeuCauDatLaiMatKhau 
                                SET DaSuDung = 1, NgaySuDung = SYSDATETIME(), TrangThai = 'USED' 
                                WHERE YeuCauDatLaiID = @ID";
                            markUsedCmd.Parameters.AddWithValue("@ID", yeuCauDatLaiId);
                            markUsedCmd.ExecuteNonQuery();
                        }

                        // 4. Vô hiệu hóa mọi request khôi phục khác của tài khoản này
                        using (var invalCmd = conn.CreateCommand())
                        {
                            invalCmd.Transaction = trans;
                            invalCmd.CommandText = @"
                                UPDATE dbo.YeuCauDatLaiMatKhau 
                                SET DaSuDung = 1, TrangThai = 'INVALIDATED' 
                                WHERE TaiKhoanID = @TaiKhoanID AND YeuCauDatLaiID != @ID AND DaSuDung = 0";
                            invalCmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            invalCmd.Parameters.AddWithValue("@ID", yeuCauDatLaiId);
                            invalCmd.ExecuteNonQuery();
                        }

                        // 5. Invalidate tất cả phiên làm việc cũ
                        using (var sessCmd = conn.CreateCommand())
                        {
                            sessCmd.Transaction = trans;
                            sessCmd.CommandText = @"
                                UPDATE dbo.PhienDangNhap 
                                SET ConHieuLuc = 0, KetThuc = SYSDATETIME() 
                                WHERE TaiKhoanID = @TaiKhoanID AND ConHieuLuc = 1";
                            sessCmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            sessCmd.ExecuteNonQuery();
                        }

                        // 6. Thu hồi token ghi nhớ thiết bị
                        using (var devCmd = conn.CreateCommand())
                        {
                            devCmd.Transaction = trans;
                            devCmd.CommandText = @"
                                UPDATE dbo.ThietBiDangNhap 
                                SET DaThuHoi = 1 
                                WHERE TaiKhoanID = @TaiKhoanID AND DaThuHoi = 0";
                            devCmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            devCmd.ExecuteNonQuery();
                        }

                        // 7. Reset bộ đếm số lần sai mật khẩu & mở khóa tạm thời
                        using (var unlockCmd = conn.CreateCommand())
                        {
                            unlockCmd.Transaction = trans;
                            unlockCmd.CommandText = @"
                                UPDATE dbo.TaiKhoan 
                                SET SoLanSaiMatKhau = 0, KhoaDen = NULL 
                                WHERE TaiKhoanID = @TaiKhoanID";
                            unlockCmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            unlockCmd.ExecuteNonQuery();
                        }

                        // 8. Ghi nhật ký bảo mật hệ thống
                        using (var auditCmd = conn.CreateCommand())
                        {
                            auditCmd.Transaction = trans;
                            auditCmd.CommandText = @"
                                INSERT INTO dbo.NhatKyHeThong (TaiKhoanID, Module, HanhDong, BangDuLieu, KhoaDuLieu, ThoiGian)
                                VALUES (@TaiKhoanID, 'SECURITY', 'PASSWORD_RESET', 'TaiKhoan', CONVERT(NVARCHAR(50), @TaiKhoanID), SYSDATETIME())";
                            auditCmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            auditCmd.ExecuteNonQuery();
                        }

                        // 9. Tạo thông báo bảo mật trong Notification Center
                        using (var notifCmd = conn.CreateCommand())
                        {
                            notifCmd.Transaction = trans;
                            notifCmd.CommandText = @"
                                INSERT INTO dbo.ThongBao (TaiKhoanID, TieuDe, NoiDung, LoaiThongBao, DaDoc, NgayTao)
                                VALUES (@TaiKhoanID, N'Mật khẩu đã được thay đổi', N'Mật khẩu tài khoản của bạn vừa được đặt lại lúc ' + FORMAT(SYSDATETIME(), 'HH:mm dd/MM/yyyy') + N'.', 'SECURITY', 0, SYSDATETIME())";
                            notifCmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            notifCmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        AppLogger.LogError(ex, "PasswordRecoveryRepository.ResetPasswordAtomic", $"TaiKhoanID={taiKhoanId}");
                        return false;
                    }
                }
            }
        }

        public long CreateAdminAssistanceRequest(int taiKhoanId, string note)
        {
            const string sql = @"
                INSERT INTO dbo.YeuCauDatLaiMatKhau (
                    TaiKhoanID, TokenHash, HetHan, DaSuDung, NgayTao, KenhGui, SoLanThuSai, TrangThai, YeuCauAdminHoTro, GhiChu
                )
                OUTPUT INSERTED.YeuCauDatLaiID
                VALUES (
                    @TaiKhoanID, 'ADMIN_REQUEST', DATEADD(hour, 24, SYSDATETIME()), 0, SYSDATETIME(), 'ADMIN_REQUEST', 0, 'ADMIN_PENDING', 1, @GhiChu
                )";

            object obj = DatabaseManager.ExecuteScalar(sql,
                new SqlParameter("@TaiKhoanID", taiKhoanId),
                new SqlParameter("@GhiChu", (object)note ?? "Yêu cầu hỗ trợ cấp mật khẩu tạm thời từ quản trị viên"));

            return Convert.ToInt64(obj);
        }

        public DataTable GetAdminPendingRequests()
        {
            const string sql = @"
                SELECT 
                    yc.YeuCauDatLaiID,
                    yc.TaiKhoanID,
                    yc.NgayTao,
                    yc.HetHan,
                    yc.TrangThai,
                    yc.GhiChu,
                    tk.TenDangNhap,
                    vt.TenVaiTro,
                    nv.MaNhanVien,
                    nv.HoTen,
                    nv.ChucVu,
                    nv.SoDienThoai,
                    nv.Email
                FROM dbo.YeuCauDatLaiMatKhau yc
                JOIN dbo.TaiKhoan tk ON yc.TaiKhoanID = tk.TaiKhoanID
                JOIN dbo.VaiTro vt ON tk.VaiTroID = vt.VaiTroID
                LEFT JOIN dbo.NhanVien nv ON tk.NhanVienID = nv.NhanVienID
                WHERE yc.YeuCauAdminHoTro = 1 
                  AND yc.DaSuDung = 0
                  AND yc.TrangThai = 'ADMIN_PENDING'
                ORDER BY yc.NgayTao DESC";

            return DatabaseManager.ExecuteQuery(sql);
        }

        public bool AdminResetPasswordAtomic(long yeuCauDatLaiId, int taiKhoanId, int adminId, string tempPasswordHash, byte[] salt)
        {
            using (var conn = DatabaseManager.CreateConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // 1. Cập nhật mật khẩu tạm thời, BẮT BUỘC ĐỔI MẬT KHẨU = 1
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "dbo.sp_UI_CapNhatMatKhau";
                            cmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            cmd.Parameters.AddWithValue("@MatKhauHash", tempPasswordHash);
                            cmd.Parameters.AddWithValue("@MatKhauSalt", (object)salt ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ThuatToan", "PBKDF2-SHA256");
                            cmd.Parameters.AddWithValue("@SoVongLap", 10000);
                            cmd.Parameters.AddWithValue("@BatBuocDoiMatKhau", true);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Cập nhật trạng thái request
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            cmd.CommandText = @"
                                UPDATE dbo.YeuCauDatLaiMatKhau 
                                SET DaSuDung = 1, NgaySuDung = SYSDATETIME(), TrangThai = 'ADMIN_COMPLETED' 
                                WHERE YeuCauDatLaiID = @ID";
                            cmd.Parameters.AddWithValue("@ID", yeuCauDatLaiId);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Vô hiệu các request khác
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            cmd.CommandText = @"
                                UPDATE dbo.YeuCauDatLaiMatKhau 
                                SET DaSuDung = 1, TrangThai = 'INVALIDATED' 
                                WHERE TaiKhoanID = @TaiKhoanID AND YeuCauDatLaiID != @ID AND DaSuDung = 0";
                            cmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            cmd.Parameters.AddWithValue("@ID", yeuCauDatLaiId);
                            cmd.ExecuteNonQuery();
                        }

                        // 4. Invalidate sessions
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            cmd.CommandText = @"
                                UPDATE dbo.PhienDangNhap 
                                SET ConHieuLuc = 0, KetThuc = SYSDATETIME() 
                                WHERE TaiKhoanID = @TaiKhoanID AND ConHieuLuc = 1";
                            cmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            cmd.ExecuteNonQuery();
                        }

                        // 5. Reset failed login counter
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            cmd.CommandText = @"
                                UPDATE dbo.TaiKhoan 
                                SET SoLanSaiMatKhau = 0, KhoaDen = NULL 
                                WHERE TaiKhoanID = @TaiKhoanID";
                            cmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            cmd.ExecuteNonQuery();
                        }

                        // 6. Ghi audit log
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            cmd.CommandText = @"
                                INSERT INTO dbo.NhatKyHeThong (TaiKhoanID, Module, HanhDong, BangDuLieu, KhoaDuLieu, ThoiGian)
                                VALUES (@AdminID, 'SECURITY', 'ADMIN_TEMP_PASSWORD_RESET', 'TaiKhoan', CONVERT(NVARCHAR(50), @TaiKhoanID), SYSDATETIME())";
                            cmd.Parameters.AddWithValue("@AdminID", adminId);
                            cmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            cmd.ExecuteNonQuery();
                        }

                        // 7. Tạo thông báo cho tài khoản nhân viên
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            cmd.CommandText = @"
                                INSERT INTO dbo.ThongBao (TaiKhoanID, TieuDe, NoiDung, LoaiThongBao, DaDoc, NgayTao)
                                VALUES (@TaiKhoanID, N'Quản trị viên đã cấp mật khẩu tạm thời', N'Quản trị viên đã phê duyệt yêu cầu đặt lại mật khẩu. Bạn bắt buộc phải đổi mật khẩu khi đăng nhập.', 'SECURITY', 0, SYSDATETIME())";
                            cmd.Parameters.AddWithValue("@TaiKhoanID", taiKhoanId);
                            cmd.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        AppLogger.LogError(ex, "PasswordRecoveryRepository.AdminResetPasswordAtomic", $"TaiKhoanID={taiKhoanId}");
                        return false;
                    }
                }
            }
        }

        private static PasswordRecoveryRequest MapRequest(DataRow r)
        {
            return new PasswordRecoveryRequest
            {
                YeuCauDatLaiID = Convert.ToInt64(r["YeuCauDatLaiID"]),
                TaiKhoanID = Convert.ToInt32(r["TaiKhoanID"]),
                TokenHash = r["TokenHash"]?.ToString(),
                HetHan = Convert.ToDateTime(r["HetHan"]),
                DaSuDung = r["DaSuDung"] != DBNull.Value && Convert.ToBoolean(r["DaSuDung"]),
                NgayTao = Convert.ToDateTime(r["NgayTao"]),
                NgaySuDung = r["NgaySuDung"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(r["NgaySuDung"]) : null,
                KenhGui = r.Table.Columns.Contains("KenhGui") && r["KenhGui"] != DBNull.Value ? r["KenhGui"].ToString() : "EMAIL",
                EmailNhan = r.Table.Columns.Contains("EmailNhan") && r["EmailNhan"] != DBNull.Value ? r["EmailNhan"].ToString() : null,
                SoLanThuSai = r.Table.Columns.Contains("SoLanThuSai") && r["SoLanThuSai"] != DBNull.Value ? Convert.ToInt32(r["SoLanThuSai"]) : 0,
                TrangThai = r.Table.Columns.Contains("TrangThai") && r["TrangThai"] != DBNull.Value ? r["TrangThai"].ToString() : "PENDING",
                YeuCauAdminHoTro = r.Table.Columns.Contains("YeuCauAdminHoTro") && r["YeuCauAdminHoTro"] != DBNull.Value && Convert.ToBoolean(r["YeuCauAdminHoTro"]),
                GhiChu = r.Table.Columns.Contains("GhiChu") && r["GhiChu"] != DBNull.Value ? r["GhiChu"].ToString() : null
            };
        }
    }
}
