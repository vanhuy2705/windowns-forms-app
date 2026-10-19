using System;
using System.Collections.Generic;
using SportZone.WinForms.Helpers;
using SportZone.WinForms.Models;
using SportZone.WinForms.Repositories;
using SportZone.WinForms.Session;

namespace SportZone.WinForms.Services
{
    public class NotificationService
    {
        private static readonly Lazy<NotificationService> _instance = new Lazy<NotificationService>(() => new NotificationService());
        public static NotificationService Instance => _instance.Value;

        private readonly NotificationRepository _repo = new NotificationRepository();

        /// <summary>
        /// Sự kiện kích hoạt khi có thay đổi trạng thái thông báo để Topbar cập nhật tức thì
        /// </summary>
        public static event Action OnNotificationUpdated;

        public static void TriggerNotificationChanged()
        {
            try
            {
                OnNotificationUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning("TriggerNotificationChanged error: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy số thông báo chưa đọc của tài khoản hiện tại trong phiên
        /// Trả về -1 nếu có lỗi kết nối database (Fail-closed)
        /// </summary>
        public int GetUnreadCount()
        {
            int taiKhoanId = AppSession.Current.TaiKhoanID;
            if (taiKhoanId <= 0) return 0;

            try
            {
                return _repo.GetUnreadCount(taiKhoanId);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "NotificationService.GetUnreadCount", $"TaiKhoanID={taiKhoanId}");
                return -1; // -1 biểu thị lỗi kết nối
            }
        }

        /// <summary>
        /// Lấy danh sách thông báo của tài khoản hiện tại
        /// </summary>
        public List<NotificationItem> GetNotifications(bool? onlyUnread = null, int limit = 20)
        {
            int taiKhoanId = AppSession.Current.TaiKhoanID;
            if (taiKhoanId <= 0) return new List<NotificationItem>();

            try
            {
                return _repo.GetNotifications(taiKhoanId, onlyUnread, limit);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "NotificationService.GetNotifications", $"TaiKhoanID={taiKhoanId}");
                throw;
            }
        }

        /// <summary>
        /// Đánh dấu một thông báo đã đọc với ràng buộc bảo mật TaiKhoanID
        /// </summary>
        public bool MarkAsRead(long thongBaoId)
        {
            int taiKhoanId = AppSession.Current.TaiKhoanID;
            if (taiKhoanId <= 0 || thongBaoId <= 0) return false;

            try
            {
                bool ok = _repo.MarkAsRead(thongBaoId, taiKhoanId);
                if (ok)
                {
                    TriggerNotificationChanged();
                }
                return ok;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "NotificationService.MarkAsRead", $"ThongBaoID={thongBaoId}");
                return false;
            }
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo của tài khoản hiện tại là đã đọc
        /// </summary>
        public bool MarkAllAsRead()
        {
            int taiKhoanId = AppSession.Current.TaiKhoanID;
            if (taiKhoanId <= 0) return false;

            try
            {
                bool ok = _repo.MarkAllAsRead(taiKhoanId);
                if (ok)
                {
                    TriggerNotificationChanged();
                }
                return ok;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "NotificationService.MarkAllAsRead", $"TaiKhoanID={taiKhoanId}");
                return false;
            }
        }

        /// <summary>
        /// Tạo thông báo gửi đến đích danh một tài khoản
        /// </summary>
        public bool CreateNotification(int taiKhoanId, string title, string content, string type = "SYSTEM", string linkType = null, long? linkId = null)
        {
            if (taiKhoanId <= 0 || string.IsNullOrWhiteSpace(title)) return false;

            try
            {
                bool ok = _repo.CreateNotification(taiKhoanId, title, content, type, linkType, linkId);
                if (ok)
                {
                    TriggerNotificationChanged();
                }
                return ok;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "NotificationService.CreateNotification", $"TargetID={taiKhoanId}");
                return false;
            }
        }

        /// <summary>
        /// Phát thông báo tới tất cả các tài khoản thuộc một vai trò nhất định (ADMIN, NHAN_VIEN, KHACH_HANG)
        /// </summary>
        public bool NotifyRole(string maVaiTro, string title, string content, string type = "SYSTEM", string linkType = null, long? linkId = null)
        {
            if (string.IsNullOrWhiteSpace(maVaiTro)) return false;

            try
            {
                var accountIds = _repo.GetAccountIdsByRole(maVaiTro);
                bool anySuccess = false;
                foreach (int tkId in accountIds)
                {
                    if (_repo.CreateNotification(tkId, title, content, type, linkType, linkId))
                    {
                        anySuccess = true;
                    }
                }
                if (anySuccess)
                {
                    TriggerNotificationChanged();
                }
                return anySuccess;
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "NotificationService.NotifyRole", $"Role={maVaiTro}");
                return false;
            }
        }
    }
}
