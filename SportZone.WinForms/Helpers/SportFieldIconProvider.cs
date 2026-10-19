using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace SportZone.WinForms.Helpers
{
    public enum SportFieldIcon
    {
        App,
        Home,
        Dashboard,
        Settings,
        Logout,
        Bell,
        Search,
        Refresh,
        Calendar,
        Clock,
        Info,
        Warning,
        Error,
        Success,
        User,
        Users,
        UserAdd,
        Profile,
        Lock,
        Key,
        Shield,
        Eye,
        EyeOff,
        Mail,
        Phone,
        Address,
        Vip,
        Admin,
        Field,
        FieldAdd,
        FieldEdit,
        FieldDetail,
        FieldType,
        Pricing,
        Time,
        Schedule,
        Lighting,
        Facility,
        Service,
        Location,
        Map,
        Sport,
        Booking,
        BookingList,
        BookingEdit,
        BookingCancel,
        BookingConfirm,
        Player,
        Group,
        Waiting,
        Finished,
        Payment,
        Qr,
        Wallet,
        Refund,
        Invoice,
        Vietqr,
        Voucher,
        VoucherAdd,
        VoucherEdit,
        VoucherUsed,
        Discount,
        Gift,
        Campaign,
        Tag,
        Star,
        Employee,
        Shift,
        Permission,
        Role,
        Chart,
        Report,
        Team,
        Inventory,
        Stock,
        ServiceRental,
        ServiceFood,
        ServiceOther,
        Cart,
        Stats,
        Revenue,
        ChartLine,
        ChartPie,
        Export,
        Print,
        Add,
        Edit,
        Delete,
        Save,
        Close,
        Back,
        Next
    }

    /// <summary>
    /// Nạp và quản lý bộ icon chuẩn SPORTFIELD từ Resources\Icons (16x16, 24x24, 32x32, 48x48, 64x64, ICO).
    /// Hỗ trợ nạp an toàn, cache bộ nhớ chống lock file, fallback tự động và vẽ tương thích High-DPI.
    /// </summary>
    public static class SportFieldIconProvider
    {
        private static readonly Dictionary<string, Image> Cache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Icon> WindowIconCache = new Dictionary<string, Icon>(StringComparer.OrdinalIgnoreCase);
        private static string _resourceIconsRoot;

        private static string GetIconsRoot()
        {
            if (_resourceIconsRoot != null && Directory.Exists(_resourceIconsRoot))
                return _resourceIconsRoot;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string p1 = Path.Combine(baseDir, "Resources", "Icons");
            if (Directory.Exists(p1)) return _resourceIconsRoot = p1;

            string p2 = Path.Combine(baseDir, "..", "..", "Resources", "Icons");
            if (Directory.Exists(p2)) return _resourceIconsRoot = Path.GetFullPath(p2);

            string p3 = Path.Combine(Application.StartupPath, "Resources", "Icons");
            if (Directory.Exists(p3)) return _resourceIconsRoot = p3;

            return _resourceIconsRoot = p1;
        }

        public static Image Get(SportFieldIcon icon, int size = 24)
        {
            if (size != 16 && size != 24 && size != 32 && size != 48 && size != 64)
            {
                if (size <= 18) size = 16;
                else if (size <= 26) size = 24;
                else if (size <= 36) size = 32;
                else if (size <= 52) size = 48;
                else size = 64;
            }

            string fileName = GetFileName(icon);
            string key = size + ":" + fileName;

            lock (Cache)
            {
                if (Cache.TryGetValue(key, out Image cached))
                    return cached;

                string root = GetIconsRoot();
                string path = Path.Combine(root, size + "x" + size, fileName + ".png");

                if (!File.Exists(path))
                {
                    AppLogger.LogWarning("SportFieldIconProvider: Khong tim thay icon " + path + ". Dang dung fallback.");
                    string fallbackPath = Path.Combine(root, size + "x" + size, "ico_app.png");
                    if (!File.Exists(fallbackPath))
                        fallbackPath = Path.Combine(root, size + "x" + size, "ico_info.png");

                    if (File.Exists(fallbackPath))
                        path = fallbackPath;
                    else
                    {
                        Bitmap placeholder = new Bitmap(size, size);
                        Cache[key] = placeholder;
                        return placeholder;
                    }
                }

                try
                {
                    using (var temp = Image.FromFile(path))
                    {
                        cached = new Bitmap(temp);
                    }
                    Cache[key] = cached;
                    return cached;
                }
                catch (Exception ex)
                {
                    AppLogger.LogWarning("SportFieldIconProvider nap icon that bai: " + ex.Message);
                    Bitmap placeholder = new Bitmap(size, size);
                    Cache[key] = placeholder;
                    return placeholder;
                }
            }
        }

        public static void Draw(Graphics g, SportFieldIcon icon, Rectangle rect, Color? tintColor = null)
        {
            if (g == null || rect.Width <= 2 || rect.Height <= 2) return;

            int bestSize;
            int maxDim = Math.Max(rect.Width, rect.Height);
            if (maxDim <= 18) bestSize = 16;
            else if (maxDim <= 28) bestSize = 24;
            else if (maxDim <= 38) bestSize = 32;
            else if (maxDim <= 54) bestSize = 48;
            else bestSize = 64;

            Image img = Get(icon, bestSize);
            if (img == null) return;

            int drawW = Math.Min(rect.Width, img.Width);
            int drawH = Math.Min(rect.Height, img.Height);
            int drawX = rect.X + (rect.Width - drawW) / 2;
            int drawY = rect.Y + (rect.Height - drawH) / 2;
            Rectangle destRect = new Rectangle(drawX, drawY, drawW, drawH);

            InterpolationMode oldInterp = g.InterpolationMode;
            PixelOffsetMode oldOffset = g.PixelOffsetMode;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            try
            {
                if (tintColor.HasValue && (tintColor.Value == Color.White || (tintColor.Value.R > 250 && tintColor.Value.G > 250 && tintColor.Value.B > 250)))
                {
                    using (var ia = new ImageAttributes())
                    {
                        ColorMatrix matrix = new ColorMatrix(new float[][]
                        {
                            new float[] { 0, 0, 0, 0, 0 },
                            new float[] { 0, 0, 0, 0, 0 },
                            new float[] { 0, 0, 0, 0, 0 },
                            new float[] { 0, 0, 0, 1, 0 },
                            new float[] { 1f, 1f, 1f, 0, 1f }
                        });
                        ia.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        g.DrawImage(img, destRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                    }
                }
                else
                {
                    g.DrawImage(img, destRect);
                }
            }
            finally
            {
                g.InterpolationMode = oldInterp;
                g.PixelOffsetMode = oldOffset;
            }
        }

        public static void Apply(Button button, SportFieldIcon icon, int size = 24)
        {
            if (button == null) return;
            button.Image = Get(icon, size);
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        public static void Apply(PictureBox pictureBox, SportFieldIcon icon, int size = 32)
        {
            if (pictureBox == null) return;
            pictureBox.Image = Get(icon, size);
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        public static Icon GetWindowIcon(SportFieldIcon icon)
        {
            string fileName = GetFileName(icon);
            lock (WindowIconCache)
            {
                if (WindowIconCache.TryGetValue(fileName, out Icon cached))
                    return cached;

                string root = GetIconsRoot();
                string path = Path.Combine(root, "ICO", fileName + ".ico");

                if (!File.Exists(path))
                {
                    AppLogger.LogWarning("SportFieldIconProvider: Khong tim thay ICO " + path + ". Dang dung ico_app.ico.");
                    path = Path.Combine(root, "ICO", "ico_app.ico");
                }

                if (File.Exists(path))
                {
                    try
                    {
                        cached = new Icon(path);
                        WindowIconCache[fileName] = cached;
                        return cached;
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogWarning("SportFieldIconProvider nap ICO that bai: " + ex.Message);
                    }
                }

                return null;
            }
        }

        public static string GetFileName(SportFieldIcon icon)
        {
            switch (icon)
            {
                case SportFieldIcon.App: return "ico_app";
                case SportFieldIcon.Home: return "ico_home";
                case SportFieldIcon.Dashboard: return "ico_dashboard";
                case SportFieldIcon.Settings: return "ico_settings";
                case SportFieldIcon.Logout: return "ico_logout";
                case SportFieldIcon.Bell: return "ico_bell";
                case SportFieldIcon.Search: return "ico_search";
                case SportFieldIcon.Refresh: return "ico_refresh";
                case SportFieldIcon.Calendar: return "ico_calendar";
                case SportFieldIcon.Clock: return "ico_clock";
                case SportFieldIcon.Info: return "ico_info";
                case SportFieldIcon.Warning: return "ico_warning";
                case SportFieldIcon.Error: return "ico_error";
                case SportFieldIcon.Success: return "ico_success";
                case SportFieldIcon.User: return "ico_user";
                case SportFieldIcon.Users: return "ico_users";
                case SportFieldIcon.UserAdd: return "ico_user_add";
                case SportFieldIcon.Profile: return "ico_profile";
                case SportFieldIcon.Lock: return "ico_lock";
                case SportFieldIcon.Key: return "ico_key";
                case SportFieldIcon.Shield: return "ico_shield";
                case SportFieldIcon.Eye: return "ico_eye";
                case SportFieldIcon.EyeOff: return "ico_eye_off";
                case SportFieldIcon.Mail: return "ico_mail";
                case SportFieldIcon.Phone: return "ico_phone";
                case SportFieldIcon.Address: return "ico_address";
                case SportFieldIcon.Vip: return "ico_vip";
                case SportFieldIcon.Admin: return "ico_admin";
                case SportFieldIcon.Field: return "ico_field";
                case SportFieldIcon.FieldAdd: return "ico_field_add";
                case SportFieldIcon.FieldEdit: return "ico_field_edit";
                case SportFieldIcon.FieldDetail: return "ico_field_detail";
                case SportFieldIcon.FieldType: return "ico_field_type";
                case SportFieldIcon.Pricing: return "ico_pricing";
                case SportFieldIcon.Time: return "ico_time";
                case SportFieldIcon.Schedule: return "ico_schedule";
                case SportFieldIcon.Lighting: return "ico_lighting";
                case SportFieldIcon.Facility: return "ico_facility";
                case SportFieldIcon.Service: return "ico_service";
                case SportFieldIcon.Location: return "ico_location";
                case SportFieldIcon.Map: return "ico_map";
                case SportFieldIcon.Sport: return "ico_sport";
                case SportFieldIcon.Booking: return "ico_booking";
                case SportFieldIcon.BookingList: return "ico_booking_list";
                case SportFieldIcon.BookingEdit: return "ico_booking_edit";
                case SportFieldIcon.BookingCancel: return "ico_booking_cancel";
                case SportFieldIcon.BookingConfirm: return "ico_booking_confirm";
                case SportFieldIcon.Player: return "ico_player";
                case SportFieldIcon.Group: return "ico_group";
                case SportFieldIcon.Waiting: return "ico_waiting";
                case SportFieldIcon.Finished: return "ico_finished";
                case SportFieldIcon.Payment: return "ico_payment";
                case SportFieldIcon.Qr: return "ico_qr";
                case SportFieldIcon.Wallet: return "ico_wallet";
                case SportFieldIcon.Refund: return "ico_refund";
                case SportFieldIcon.Invoice: return "ico_invoice";
                case SportFieldIcon.Vietqr: return "ico_vietqr";
                case SportFieldIcon.Voucher: return "ico_voucher";
                case SportFieldIcon.VoucherAdd: return "ico_voucher_add";
                case SportFieldIcon.VoucherEdit: return "ico_voucher_edit";
                case SportFieldIcon.VoucherUsed: return "ico_voucher_used";
                case SportFieldIcon.Discount: return "ico_discount";
                case SportFieldIcon.Gift: return "ico_gift";
                case SportFieldIcon.Campaign: return "ico_campaign";
                case SportFieldIcon.Tag: return "ico_tag";
                case SportFieldIcon.Star: return "ico_star";
                case SportFieldIcon.Employee: return "ico_employee";
                case SportFieldIcon.Shift: return "ico_shift";
                case SportFieldIcon.Permission: return "ico_permission";
                case SportFieldIcon.Role: return "ico_role";
                case SportFieldIcon.Chart: return "ico_chart";
                case SportFieldIcon.Report: return "ico_report";
                case SportFieldIcon.Team: return "ico_team";
                case SportFieldIcon.Inventory: return "ico_inventory";
                case SportFieldIcon.Stock: return "ico_stock";
                case SportFieldIcon.ServiceRental: return "ico_service_rental";
                case SportFieldIcon.ServiceFood: return "ico_service_food";
                case SportFieldIcon.ServiceOther: return "ico_service_other";
                case SportFieldIcon.Cart: return "ico_cart";
                case SportFieldIcon.Stats: return "ico_stats";
                case SportFieldIcon.Revenue: return "ico_revenue";
                case SportFieldIcon.ChartLine: return "ico_chart_line";
                case SportFieldIcon.ChartPie: return "ico_chart_pie";
                case SportFieldIcon.Export: return "ico_export";
                case SportFieldIcon.Print: return "ico_print";
                case SportFieldIcon.Add: return "ico_add";
                case SportFieldIcon.Edit: return "ico_edit";
                case SportFieldIcon.Delete: return "ico_delete";
                case SportFieldIcon.Save: return "ico_save";
                case SportFieldIcon.Close: return "ico_close";
                case SportFieldIcon.Back: return "ico_back";
                case SportFieldIcon.Next: return "ico_next";
                default: return "ico_app";
            }
        }

        public static void ClearCache()
        {
            lock (Cache)
            {
                foreach (var image in Cache.Values)
                    image.Dispose();
                Cache.Clear();
            }
            lock (WindowIconCache)
            {
                foreach (var icon in WindowIconCache.Values)
                    icon.Dispose();
                WindowIconCache.Clear();
            }
        }
    }
}
