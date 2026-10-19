using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SportZone.WinForms.Database;
using SportZone.WinForms.Models;

namespace SportZone.WinForms.Repositories
{
    public class NotificationRepository
    {
        public List<NotificationItem> GetNotifications(int taiKhoanId, bool? onlyUnread = null, int limit = 20)
        {
            var list = new List<NotificationItem>();
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(
                    "EXEC dbo.sp_UI_LayThongBao @TaiKhoanID, @ChiChuaDoc",
                    new SqlParameter("@TaiKhoanID", taiKhoanId),
                    new SqlParameter("@ChiChuaDoc", onlyUnread == true ? 1 : 0));

                if (dt != null)
                {
                    int count = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        if (limit > 0 && count >= limit) break;

                        var item = new NotificationItem
                        {
                            ThongBaoID = Convert.ToInt64(r["ThongBaoID"]),
                            TaiKhoanID = r.Table.Columns.Contains("TaiKhoanID") && r["TaiKhoanID"] != DBNull.Value ? (int?)Convert.ToInt32(r["TaiKhoanID"]) : taiKhoanId,
                            TieuDe = r["TieuDe"]?.ToString() ?? "",
                            NoiDung = r["NoiDung"]?.ToString() ?? "",
                            LoaiThongBao = r["LoaiThongBao"]?.ToString() ?? "SYSTEM",
                            DaDoc = r["DaDoc"] != DBNull.Value && Convert.ToBoolean(r["DaDoc"]),
                            LienKetLoai = r["LienKetLoai"] != DBNull.Value ? r["LienKetLoai"].ToString() : null,
                            LienKetID = r["LienKetID"] != DBNull.Value ? (long?)Convert.ToInt64(r["LienKetID"]) : null,
                            NgayTao = r["NgayTao"] != DBNull.Value ? Convert.ToDateTime(r["NgayTao"]) : DateTime.Now
                        };
                        list.Add(item);
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "NotificationRepository.GetNotifications", $"TaiKhoanID={taiKhoanId}");
                throw;
            }
            return list;
        }

        public int GetUnreadCount(int taiKhoanId)
        {
            try
            {
                object result = DatabaseManager.ExecuteScalar(
                    "SELECT COUNT(*) FROM dbo.ThongBao WHERE TaiKhoanID = @TaiKhoanID AND DaDoc = 0",
                    new SqlParameter("@TaiKhoanID", taiKhoanId));

                return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "NotificationRepository.GetUnreadCount", $"TaiKhoanID={taiKhoanId}");
                throw;
            }
        }

        public bool MarkAsRead(long thongBaoId, int taiKhoanId)
        {
            try
            {
                DatabaseManager.ExecuteNonQuery(
                    "EXEC dbo.sp_UI_DanhDauThongBaoDaDoc @ThongBaoID, @TaiKhoanID",
                    new SqlParameter("@ThongBaoID", thongBaoId),
                    new SqlParameter("@TaiKhoanID", taiKhoanId));
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "NotificationRepository.MarkAsRead", $"ThongBaoID={thongBaoId}, TaiKhoanID={taiKhoanId}");
                return false;
            }
        }

        public bool MarkAllAsRead(int taiKhoanId)
        {
            try
            {
                DatabaseManager.ExecuteNonQuery(
                    "UPDATE dbo.ThongBao SET DaDoc = 1 WHERE TaiKhoanID = @TaiKhoanID AND DaDoc = 0",
                    new SqlParameter("@TaiKhoanID", taiKhoanId));
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "NotificationRepository.MarkAllAsRead", $"TaiKhoanID={taiKhoanId}");
                return false;
            }
        }

        public bool CreateNotification(int taiKhoanId, string title, string content, string type = "SYSTEM", string linkType = null, long? linkId = null)
        {
            try
            {
                DatabaseManager.ExecuteNonQuery(
                    @"INSERT INTO dbo.ThongBao (TaiKhoanID, TieuDe, NoiDung, LoaiThongBao, DaDoc, LienKetLoai, LienKetID, NgayTao)
                      VALUES (@TaiKhoanID, @TieuDe, @NoiDung, @LoaiThongBao, 0, @LienKetLoai, @LienKetID, SYSDATETIME())",
                    new SqlParameter("@TaiKhoanID", taiKhoanId),
                    new SqlParameter("@TieuDe", (object)title ?? ""),
                    new SqlParameter("@NoiDung", (object)content ?? ""),
                    new SqlParameter("@LoaiThongBao", (object)type ?? "SYSTEM"),
                    new SqlParameter("@LienKetLoai", (object)linkType ?? DBNull.Value),
                    new SqlParameter("@LienKetID", (object)linkId ?? DBNull.Value));
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "NotificationRepository.CreateNotification", $"TaiKhoanID={taiKhoanId}, Title={title}");
                return false;
            }
        }

        public List<int> GetAccountIdsByRole(string maVaiTro)
        {
            var list = new List<int>();
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(
                    @"SELECT tk.TaiKhoanID 
                      FROM dbo.TaiKhoan tk
                      JOIN dbo.VaiTro vt ON vt.VaiTroID = tk.VaiTroID
                      WHERE vt.MaVaiTro = @MaVaiTro AND tk.DangHoatDong = 1",
                    new SqlParameter("@MaVaiTro", maVaiTro));

                if (dt != null)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        list.Add(Convert.ToInt32(r["TaiKhoanID"]));
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "NotificationRepository.GetAccountIdsByRole", $"Role={maVaiTro}");
            }
            return list;
        }
    }
}
