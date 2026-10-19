using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Dialogs;
using SportZone.WinForms.Helpers;
using SportZone.WinForms.Session;
using SportZone.WinForms.Services;
using SportZone.WinForms.Security;

namespace SportZone.WinForms.Views
{
    public class StaffDashboardView : UserControl
    {
        private SFDataGridView gridToday;
        private SFChart chartHourly;
        private SFMetricCard cardBookings;
        private SFMetricCard cardRevenue;
        private SFMetricCard cardCustomers;
        private SFMetricCard cardAlerts;

        public StaffDashboardView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadDataFromDatabase();
        }

        private void BuildUI()
        {
            // 1. Shift Operational Header
            string staffName = AppSession.Current != null && !string.IsNullOrEmpty(AppSession.Current.HoTen) ? AppSession.Current.HoTen : "Trần Văn An";
            Controls.Add(Ui.Header("Bàn Làm Việc Ca Trực • Quầy POS #02", string.Format("Nhân viên: {0} • Ca trực hôm nay • Trạng thái: POS Online & Sẵn sàng.", staffName)));

            // 2. 4 Large Operational CTA Buttons (Fast Counter Operations)
            TableLayoutPanel actionHero = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Colors.Canvas,
                Padding = new Padding(0, 0, 0, 10),
                Margin = new Padding(0)
            };
            for (int i = 0; i < 4; i++) actionHero.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            SFButton btnBook = new SFButton
            {
                Text = "ĐẶT SÂN NHANH (F2)",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Plus,
                Dock = DockStyle.Fill,
                Height = 42,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnBook.Click += (s, e) => (FindForm() as MainForm)?.ShowView("Đặt sân tại quầy");
            actionHero.Controls.Add(btnBook, 0, 0);

            SFButton btnCheckin = new SFButton
            {
                Text = "CHECK-IN / BẮT ĐẦU (F3)",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Check,
                Dock = DockStyle.Fill,
                Height = 42,
                Margin = new Padding(4, 0, 4, 0)
            };
            btnCheckin.Click += (s, e) =>
            {
                if (gridToday.SelectedRows.Count > 0)
                {
                    DataRow row = gridToday.SelectedRows[0].Tag as DataRow;
                    if (row != null)
                    {
                        long donId = Convert.ToInt64(row["DonDatSanID"]);
                        DatabaseManager.ExecuteNonQuery("UPDATE dbo.DonDatSan SET TrangThaiDon = 'DANG_SU_DUNG' WHERE DonDatSanID = @ID", new SqlParameter("@ID", donId));
                        SFToast.Show(this, "Đã check-in và bắt đầu sử dụng sân cho đơn " + row["MaDon"], "CHECK-IN SÂN", SFIcon.Check, Colors.Emerald);
                        LoadDataFromDatabase();
                        return;
                    }
                }
                SFToast.Show(this, "Vui lòng chọn đơn thi đấu trong lịch hôm nay để check-in.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Amber);
            };
            actionHero.Controls.Add(btnCheckin, 1, 0);

            SFButton btnPayment = new SFButton
            {
                Text = "THU TIỀN POS (F4)",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Dollar,
                Dock = DockStyle.Fill,
                Height = 42,
                Margin = new Padding(4, 0, 4, 0)
            };
            btnPayment.Click += (s, e) =>
            {
                if (gridToday.SelectedRows.Count > 0)
                {
                    DataRow row = gridToday.SelectedRows[0].Tag as DataRow;
                    if (row != null)
                    {
                        decimal rem = row["ConLai"] != DBNull.Value ? Convert.ToDecimal(row["ConLai"]) : 0;
                        string code = row["MaDon"]?.ToString() ?? "";
                        string cust = row["HoTen"]?.ToString() ?? "";
                        if (rem <= 0)
                        {
                            SFToast.Show(this, "Đơn này đã hoàn tất thanh toán đủ tiền.", "THÔNG BÁO", SFIcon.Check, Colors.Emerald);
                            return;
                        }

                        long donId = row.Table.Columns.Contains("DonDatSanID") ? Convert.ToInt64(row["DonDatSanID"]) : 0;
                        long hoaDonId = 0;
                        if (donId > 0)
                        {
                            var objHd = DatabaseManager.ExecuteScalar("SELECT TOP 1 HoaDonID FROM dbo.HoaDon WHERE DonDatSanID = @ID", new System.Data.SqlClient.SqlParameter("@ID", donId));
                            if (objHd != null && objHd != DBNull.Value) hoaDonId = Convert.ToInt64(objHd);
                            else
                            {
                                try
                                {
                                    var inv = new Services.PaymentService().CreateInvoice(donId, Session.AppSession.Current != null ? Session.AppSession.Current.TaiKhoanID : (int?)null);
                                    hoaDonId = inv.HoaDonID;
                                }
                                catch { }
                            }
                        }

                        if (hoaDonId > 0)
                        {
                            var dlg = new QrPaymentForm(rem, code, cust, hoaDonId);
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                            {
                                LoadDataFromDatabase();
                            }
                        }
                        else
                        {
                            SFToast.Show(this, "Không thể xác định hoặc tạo hóa đơn cho đơn đặt sân này.", "LỖI HÓA ĐƠN", SFIcon.AlertCircle, Colors.Danger);
                        }
                        return;
                    }
                }
                SFToast.Show(this, "Vui lòng chọn đơn đặt sân từ danh sách hôm nay để thu tiền.", "CHƯA CHỌN ĐƠN", SFIcon.Info, Colors.Cyan);
            };
            actionHero.Controls.Add(btnPayment, 2, 0);

            SFButton btnHandover = new SFButton
            {
                Text = "BÀN GIAO CA (F8)",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Clock,
                Dock = DockStyle.Fill,
                Height = 42,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnHandover.Click += (s, e) => new StaffShiftDialog().ShowDialog(this);
            actionHero.Controls.Add(btnHandover, 3, 0);

            Controls.Add(actionHero);
            actionHero.BringToFront();

            // 3. Operational Metric Bar
            TableLayoutPanel kpiPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                ColumnCount = 4,
                Padding = new Padding(0, 0, 0, 14)
            };
            for (int i = 0; i < 4; i++) kpiPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            cardBookings = new SFMetricCard("ĐẶT SÂN HÔM NAY", "0 Đơn", "Đã xác nhận trong ngày", SFIcon.Calendar, Colors.ElectricBlue);
            cardRevenue = new SFMetricCard("DOANH THU CA TRỰC", "0đ", "TM 0k • CK 0k", SFIcon.Dollar, Colors.Emerald, "+14% so ca qua");
            cardCustomers = new SFMetricCard("KHÁCH PHÁT SINH", "0 Khách", "Khách hội viên", SFIcon.User, Colors.Purple);
            cardAlerts = new SFMetricCard("CẦN XỬ LÝ NGAY", "00 Đơn", "Sân đang thi đấu", SFIcon.AlertCircle, Colors.Coral, "Ưu tiên cao");

            kpiPanel.Controls.Add(cardBookings, 0, 0);
            kpiPanel.Controls.Add(cardRevenue, 1, 0);
            kpiPanel.Controls.Add(cardCustomers, 2, 0);
            kpiPanel.Controls.Add(cardAlerts, 3, 0);

            Controls.Add(kpiPanel);
            kpiPanel.BringToFront();

            // 4. Split Content: Left = Schedule Table, Right = Hourly Revenue & Alerts
            TableLayoutPanel mid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63f));
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37f));

            // Left: Schedule Table
            SFCard cardToday = new SFCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                Margin = new Padding(0, 0, 10, 0)
            };
            Label lblToday = new Label
            {
                Text = "Lịch Trình Thi Đấu Trong Ca Hiện Tại (Sắp xếp theo giờ bắt đầu)",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 32
            };
            cardToday.Controls.Add(lblToday);

            gridToday = new SFDataGridView { Dock = DockStyle.Fill };
            gridToday.Columns.Add("Slot", "Khung giờ");
            gridToday.Columns.Add("Code", "Mã đơn");
            gridToday.Columns.Add("Field", "Sân thi đấu");
            gridToday.Columns.Add("Customer", "Khách hàng");
            gridToday.Columns.Add("Remaining", "Còn thu");
            gridToday.Columns.Add("Status", "Trạng thái");

            gridToday.Columns["Slot"].Width = 110;
            gridToday.Columns["Code"].Width = 110;
            gridToday.Columns["Field"].Width = 140;
            gridToday.Columns["Customer"].Width = 160;
            gridToday.Columns["Remaining"].Width = 100;
            gridToday.Columns["Status"].Width = 115;

            gridToday.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < gridToday.Rows.Count)
                {
                    DataRow row = gridToday.Rows[e.RowIndex].Tag as DataRow;
                    if (row != null)
                    {
                        decimal rem = row["ConLai"] != DBNull.Value ? Convert.ToDecimal(row["ConLai"]) : 0;
                        string code = row["MaDon"]?.ToString() ?? "";
                        string cust = row["HoTen"]?.ToString() ?? "";
                        if (rem > 0)
                        {
                            var dlg = new QrPaymentForm(rem, code, cust, 0);
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                            {
                                LoadDataFromDatabase();
                            }
                        }
                        else
                        {
                            SFToast.Show(this, "Đơn này đã hoàn tất thanh toán (còn lại 0đ).", "THÔNG BÁO", SFIcon.Check, Colors.Emerald);
                        }
                    }
                }
            };

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemCheckin = cms.Items.Add("Check-in / Bắt Đầu Sân");
            itemCheckin.Click += (s, e) => btnCheckin.PerformClick();

            var itemPay = cms.Items.Add("Thu Tiền POS / VietQR");
            itemPay.Click += (s, e) => btnPayment.PerformClick();

            var itemExport = cms.Items.Add("Xuất Lịch Ca Trực Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(gridToday, "Lich_Ca_Truc", this);

            gridToday.ContextMenuStrip = cms;

            cardToday.Controls.Add(gridToday);
            gridToday.BringToFront();
            mid.Controls.Add(cardToday, 0, 0);

            // Right: Revenue Chart & Operational Alerts
            TableLayoutPanel rightCol = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Margin = new Padding(10, 0, 0, 0)
            };
            rightCol.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
            rightCol.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));

            // Chart
            SFCard cardChart = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(14), Margin = new Padding(0, 0, 0, 8) };
            Label lblChart = new Label { Text = "Dòng Tiền Thu Theo Giờ Trong Ca", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 28 };
            cardChart.Controls.Add(lblChart);

            chartHourly = new SFChart
            {
                Dock = DockStyle.Fill,
                AccentColor = Colors.ElectricBlue,
                XLabels = new[] { "06h", "08h", "10h", "12h", "14h" },
                Values = new double[] { 1.2, 2.8, 4.5, 6.2, 8.45 },
                MaxValue = 10.0
            };
            cardChart.Controls.Add(chartHourly);
            chartHourly.BringToFront();
            rightCol.Controls.Add(cardChart, 0, 0);

            // Alerts
            SFCard cardAlertsBox = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(14), Margin = new Padding(0, 8, 0, 0) };
            Label lblAlerts = new Label { Text = "Cảnh Báo Vận Hành Quầy & Sân", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 28 };
            cardAlertsBox.Controls.Add(lblAlerts);

            Label txtAlerts = new Label
            {
                Text = "• Hệ thống kết nối cơ sở dữ liệu SQL Server ổn định\n\n• Sẵn sàng thanh toán mã QR chuẩn Napas 247\n\n• Quầy POS: Thiết bị kết nối hoạt động bình thường\n\n• Bàn giao ca và đối soát doanh thu tự động theo ca trực",
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Fill,
                Padding = new Padding(4, 6, 4, 4)
            };
            cardAlertsBox.Controls.Add(txtAlerts);
            txtAlerts.BringToFront();
            rightCol.Controls.Add(cardAlertsBox, 0, 1);

            mid.Controls.Add(rightCol, 1, 0);

            Controls.Add(mid);
            mid.BringToFront();
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                // 1. Shift Schedule
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_StaffLichHomNay");
                gridToday.Rows.Clear();
                int dangDau = 0;

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        DateTime bd = Convert.ToDateTime(row["BatDau"]);
                        DateTime kt = Convert.ToDateTime(row["KetThuc"]);
                        string slot = string.Format("{0:HH:mm} - {1:HH:mm}", bd, kt);
                        string code = row["MaDon"]?.ToString() ?? "";
                        string field = row["TenSan"]?.ToString() ?? (row["MaSan"]?.ToString() ?? "");
                        string cust = row["HoTen"]?.ToString() ?? "";
                        decimal rem = row["ConLai"] != DBNull.Value ? Convert.ToDecimal(row["ConLai"]) : 0;
                        string st = row["TrangThaiDon"]?.ToString() ?? "";

                        string stDisp = "Đã xác nhận";
                        if (st == "DANG_SU_DUNG") { stDisp = "Đang thi đấu"; dangDau++; }
                        else if (st == "HOAN_THANH") stDisp = "Hoàn tất";
                        else if (st == "CHO_XAC_NHAN") stDisp = "Chờ duyệt";

                        int rowIndex = gridToday.Rows.Add(slot, code, field, cust, string.Format("{0:N0}đ", rem), stDisp);
                        gridToday.Rows[rowIndex].Tag = row;
                    }

                    cardBookings.Value = string.Format("{0} Đơn", dt.Rows.Count);
                    cardAlerts.Value = string.Format("{0:D2} Đơn", dangDau);
                }
                else
                {
                    cardBookings.Value = "0 Đơn";
                    cardAlerts.Value = "00 Đơn";
                }

                // 2. Shift Stats
                int nhanVienId = SessionGuard.RequireEmployeeId();
                DataSet dsStats = DatabaseManager.ExecuteDataSet("EXEC dbo.sp_UI_StaffThongKeCa @NhanVienID = @NV", new SqlParameter("@NV", nhanVienId));
                if (dsStats != null && dsStats.Tables.Count > 1 && dsStats.Tables[1].Rows.Count > 0)
                {
                    DataRow r = dsStats.Tables[1].Rows[0];
                    decimal tongThu = r["TongDaThu"] != DBNull.Value ? Convert.ToDecimal(r["TongDaThu"]) : 0;
                    decimal tienMat = r["TienMat"] != DBNull.Value ? Convert.ToDecimal(r["TienMat"]) : 0;
                    decimal chuyenKhoan = r["ChuyenKhoan"] != DBNull.Value ? Convert.ToDecimal(r["ChuyenKhoan"]) : 0;

                    cardRevenue.Value = string.Format("{0:N0}đ", tongThu);
                    cardRevenue.SubValue = string.Format("TM {0:N0}k • CK {1:N0}k", tienMat / 1000m, chuyenKhoan / 1000m);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "STAFF_DASHBOARD", "LoadDataFromDatabase");
                cardBookings.Value = "0 Đơn";
                cardRevenue.Value = "0đ";
                cardAlerts.Value = "00 Đơn";
            }
        }
    }

    public class StaffBookingView : UserControl
    {
        private SFTextBox txtPhone;
        private SFTextBox txtNameCust;
        private SFComboBox cboSport;
        private SFComboBox cboField;
        private DateTimePicker dp;
        private int selectedSanId = 1;
        private long lastCreatedHoaDonId = 0;
        private string lastCreatedMaDon = "";
        private decimal lastCreatedTotal = 0;
        private SFButton btnPayQr;
        private SFButton btnSubmit;

        public StaffBookingView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Đặt Sân Nhanh Tại Quầy (POS)", "Tìm khách hàng, kiểm tra sân trống, chọn khung giờ và xác nhận thanh toán trên cùng một màn hình."));

            FlowLayoutPanel keys = Ui.Toolbar();
            string[] fKeys = { "F2 • Tìm khách", "F3 • Kiểm tra sân", "F4 • Thu tiền VietQR", "F9 • Lưu & In", "Esc • Làm mới" };
            for (int i = 0; i < fKeys.Length; i++)
            {
                SFButton b = new SFButton
                {
                    Text = fKeys[i],
                    Variant = i == 3 ? SFButtonVariant.Primary : SFButtonVariant.Secondary,
                    IconType = i == 3 ? SFIcon.Check : SFIcon.None,
                    Width = 150,
                    Height = 36
                };
                if (i == 2) b.Click += (s, e) => { if (btnPayQr != null) btnPayQr.PerformClick(); };
                keys.Controls.Add(b);
            }
            Controls.Add(keys);
            keys.BringToFront();

            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));

            // Col 1: Customer & Field
            SFCard left = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(0, 0, 8, 0) };
            Label lTitle = new Label { Text = "1. Khách Hàng & Sân Chỉ Định", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            left.Controls.Add(lTitle);

            Panel p1 = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            txtPhone = AddFieldControl(p1, "Số điện thoại tra cứu *", "0987654321", 8, 280);
            txtNameCust = AddFieldControl(p1, "Tên khách hàng / Đội bóng *", "Nguyễn Văn Tuấn", 72, 280);

            Label vipBadge = new Label
            {
                Text = "★ THÀNH VIÊN HỆ THỐNG • SPORTZONE POS",
                Font = Typography.CaptionBold,
                ForeColor = Colors.Purple,
                BackColor = Colors.PurpleBg,
                Padding = new Padding(8, 5, 8, 5),
                Location = new Point(4, 142),
                AutoSize = true
            };
            p1.Controls.Add(vipBadge);

            cboSport = AddComboControl(p1, "Bộ môn / Loại sân", 182, 280, "Bóng đá mini 7 người", "Bóng đá 5 người", "Pickleball tiêu chuẩn", "Cầu lông PVC");
            AddComboControl(p1, "Cụm sân vận hành", 252, 280, "Khu A - Trung Tâm", "Khu B - Mái Che", "Khu C - VIP Khép Kín");
            cboField = AddComboControl(p1, "Sân cụ thể", 322, 280, "Sân A1 - Cỏ FIFA Pro", "Sân A2 - Có mái", "Sân A3 - Ngoài trời", "Sân B1 - Tiêu chuẩn");
            left.Controls.Add(p1);
            p1.BringToFront();
            root.Controls.Add(left, 0, 0);

            // Col 2: Date & Slots
            SFCard center = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(8, 0, 8, 0) };
            Label cTitle = new Label { Text = "2. Ngày & Khung Giờ Khả Dụng", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            center.Controls.Add(cTitle);

            Panel p2 = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Label lblDate = new Label { Text = "Ngày thi đấu *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, 8), AutoSize = true };
            p2.Controls.Add(lblDate);
            dp = new DateTimePicker { Location = new Point(4, 28), Width = 280, Format = DateTimePickerFormat.Short, Font = Typography.Body };
            p2.Controls.Add(dp);

            Label lblSlots = new Label { Text = "Chọn khung giờ trực quan (Xanh: Trống, Xám: Đã đặt) *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, 76), AutoSize = true };
            p2.Controls.Add(lblSlots);

            FlowLayoutPanel slotGrid = new FlowLayoutPanel { Location = new Point(4, 98), Size = new Size(340, 160), AutoScroll = true, WrapContents = true };
            string[] times = { "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00" };
            for (int i = 0; i < times.Length; i++)
            {
                SFButton b = new SFButton
                {
                    Text = times[i],
                    Variant = (i == 9 || i == 12) ? SFButtonVariant.Secondary : (i == 10 ? SFButtonVariant.Primary : SFButtonVariant.Outline),
                    Size = new Size(62, 34),
                    Enabled = !(i == 9 || i == 12)
                };
                slotGrid.Controls.Add(b);
            }
            p2.Controls.Add(slotGrid);

            AddComboControl(p2, "Dịch vụ đi kèm F&B", 270, 310, "Không chọn", "Nước suối 4 chai (40k)", "Bóng thi đấu số 5 (30k)", "Áo Bib 10 cái (50k)");
            AddFieldControl(p2, "Ghi chú quầy", "Khách yêu cầu bật đèn cao áp sớm 10 phút", 340, 310);
            center.Controls.Add(p2);
            p2.BringToFront();
            root.Controls.Add(center, 1, 0);

            // Col 3: Summary & Big Total
            SFCard right = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(8, 0, 0, 0) };
            Label rTitle = new Label { Text = "3. Tóm Tắt & Thanh Toán", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            right.Controls.Add(rTitle);

            Panel p3 = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Label summary = new Label
            {
                Text = "Khách: Nguyễn Văn Tuấn\nSân A1 • Bóng đá mini\nThời gian: 17:00 - 18:30\n\nTiền sân (1.5h):        450.000đ\nDịch vụ:                       0đ\nKhuyến mãi:                   0đ\n──────────────────────\nCẦN THANH TOÁN:\n450.000 đ",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Location = new Point(4, 8),
                Size = new Size(Math.Max(180, p3.ClientSize.Width - 12), 270),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = false
            };
            p3.Controls.Add(summary);

            btnPayQr = new SFButton
            {
                Text = "Thanh Toán VietQR Napas",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Dollar,
                Location = new Point(4, 290),
                Size = new Size(Math.Max(180, p3.ClientSize.Width - 12), 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnPayQr.Click += (s, e) =>
            {
                if (lastCreatedHoaDonId > 0)
                {
                    new QrPaymentForm(lastCreatedTotal, lastCreatedMaDon, txtNameCust?.Text, lastCreatedHoaDonId).ShowDialog(this);
                }
                else
                {
                    btnSubmit?.PerformClick();
                }
            };
            p3.Controls.Add(btnPayQr);

            btnSubmit = new SFButton
            {
                Text = "LƯU ĐƠN & IN PHIẾU (F9)",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Check,
                Location = new Point(4, 338),
                Size = new Size(Math.Max(180, p3.ClientSize.Width - 12), 44),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnSubmit.Click += (s, e) =>
            {
                try
                {
                    string phone = txtPhone.Text.Trim();
                    string name = txtNameCust.Text.Trim();
                    if (string.IsNullOrEmpty(phone))
                    {
                        SFToast.Show(this, "Vui lòng nhập số điện thoại khách hàng.", "THIẾU THÔNG TIN", SFIcon.AlertCircle, Colors.Coral);
                        return;
                    }

                    var custService = new CustomerService();
                    int custId = 0;
                    DataTable dtCust = custService.GetCustomers(phone);
                    if (dtCust != null && dtCust.Rows.Count > 0)
                    {
                        custId = Convert.ToInt32(dtCust.Rows[0]["KhachHangID"]);
                    }
                    else
                    {
                        custId = custService.CreateCustomer(name.Length > 0 ? name : "Khách vãng lai", phone, null, null, null);
                    }

                    DateTime slotStart = dp.Value.Date.AddHours(17);
                    DateTime slotEnd = slotStart.AddHours(1.5);

                    var bookingService = new BookingService();
                    var bRes = bookingService.CreateBooking(custId, selectedSanId, slotStart, slotEnd, "TAI_QUAY", AppSession.Current != null ? (int?)AppSession.Current.TaiKhoanID : null, null, "Đặt sân tại quầy POS");
                    long donId = bRes.DonDatSanID;
                    lastCreatedMaDon = bRes.MaDon;

                    var payService = new PaymentService();
                    var invRes = payService.CreateInvoice(donId, AppSession.Current != null ? (int?)AppSession.Current.TaiKhoanID : null);
                    long hdId = invRes.HoaDonID;
                    lastCreatedHoaDonId = hdId;

                    decimal tot = 0;
                    try
                    {
                        var objTot = DatabaseManager.ExecuteScalar("SELECT TongTien FROM dbo.DonDatSan WHERE DonDatSanID = @ID", new System.Data.SqlClient.SqlParameter("@ID", donId));
                        if (objTot != null && objTot != DBNull.Value) tot = Convert.ToDecimal(objTot);
                    }
                    catch { tot = 350000; }
                    lastCreatedTotal = tot;

                    SFToast.Show(this, string.Format("Đã tạo đơn đặt sân {0} và hóa đơn #{1} thành công!", bRes.MaDon, hdId), "ĐẶT SÂN THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                    new QrPaymentForm(lastCreatedTotal, lastCreatedMaDon, name, lastCreatedHoaDonId).ShowDialog(this);
                }
                catch (Exception ex)
                {
                    AppLogger.LogError(ex, "STAFF_BOOKING", "btnSubmit");
                    SFToast.Show(this, "Lỗi tạo đơn: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
                }
            };
            p3.Controls.Add(btnSubmit);

            right.Controls.Add(p3);
            p3.BringToFront();
            root.Controls.Add(right, 2, 0);

            Controls.Add(root);
            root.BringToFront();
        }

        private SFTextBox AddFieldControl(Control p, string label, string value, int y, int w)
        {
            Label l = new Label { Text = label, Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l);
            SFTextBox t = new SFTextBox { Text = value, Location = new Point(4, y + 20), Width = w, Height = 36 };
            p.Controls.Add(t);
            return t;
        }

        private SFComboBox AddComboControl(Control p, string label, int y, int w, params string[] items)
        {
            Label l = new Label { Text = label, Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l);
            SFComboBox c = new SFComboBox { Location = new Point(4, y + 20), Width = w, Height = 36 };
            c.Items.AddRange(items);
            if (c.Items.Count > 0) c.SelectedIndex = 0;
            p.Controls.Add(c);
            return c;
        }
    }

    public class StaffScheduleView : UserControl
    {
        private SFDataGridView grid;
        private DateTimePicker d;
        private SFComboBox type;
        private SFSearchBox searchBox;

        public StaffScheduleView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Lịch Đặt Sân & Vận Hành", "Theo dõi trạng thái lượt đặt, check-in, bắt đầu thi đấu, hoàn tất và hủy theo quyền trực quầy."));

            FlowLayoutPanel tb = Ui.Toolbar();
            d = new DateTimePicker { Width = 130, Format = DateTimePickerFormat.Short, Font = Typography.Body };
            d.ValueChanged += (s, e) => LoadData();
            tb.Controls.Add(d);

            type = new SFComboBox { Width = 160, Height = 36 };
            type.Items.AddRange(new object[] { "Tất cả môn", "Bóng đá", "Cầu lông", "Bóng rổ", "Pickleball" });
            type.SelectedIndex = 0;
            type.SelectedIndexChanged += (s, e) => LoadData();
            tb.Controls.Add(type);

            searchBox = new SFSearchBox { PlaceholderText = "Mã đơn / khách / sân...", Width = 230, Height = 36 };
            searchBox.TextChanged += (s, e) => LoadData();
            tb.Controls.Add(searchBox);

            SFButton btnRefresh = new SFButton { Text = "F5 • Làm mới", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Refresh, Width = 130, Height = 36 };
            btnRefresh.Click += (s, e) => LoadData();
            tb.Controls.Add(btnRefresh);

            SFButton btnAdd = new SFButton { Text = "F2 • Đặt sân", Variant = SFButtonVariant.Primary, IconType = SFIcon.Plus, Width = 130, Height = 36 };
            btnAdd.Click += (s, e) => (FindForm() as MainForm)?.ShowView("Đặt sân tại quầy");
            tb.Controls.Add(btnAdd);

            Controls.Add(tb);
            tb.BringToFront();

            // Split: Left Table, Right Fast Actions
            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 78f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22f));

            SFCard listCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(0, 0, 10, 0) };
            Label lTitle = new Label { Text = "Danh Sách Lượt Đặt Thi Đấu Theo Ngày (SQL Server)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            listCard.Controls.Add(lTitle);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("Code", "Mã đơn");
            grid.Columns.Add("Customer", "Khách hàng");
            grid.Columns.Add("Field", "Sân thi đấu");
            grid.Columns.Add("Start", "Bắt đầu");
            grid.Columns.Add("End", "Kết thúc");
            grid.Columns.Add("Total", "Tổng tiền");
            grid.Columns.Add("Paid", "Thanh toán");
            grid.Columns.Add("Status", "Trạng thái");

            grid.Columns["Code"].Width = 110;
            grid.Columns["Customer"].Width = 160;
            grid.Columns["Field"].Width = 140;
            grid.Columns["Start"].Width = 80;
            grid.Columns["End"].Width = 80;
            grid.Columns["Total"].Width = 110;
            grid.Columns["Paid"].Width = 110;
            grid.Columns["Status"].Width = 115;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemCheckin = cms.Items.Add("Check-in / Bắt Đầu Sân");
            itemCheckin.Click += (s, e) => PerformActionOnSelected("DANG_SU_DUNG", "Đã check-in bắt đầu sử dụng sân.");

            var itemFinish = cms.Items.Add("Hoàn Tất & Trả Sân");
            itemFinish.Click += (s, e) => PerformActionOnSelected("HOAN_THANH", "Đã hoàn tất đơn đặt và giải phóng sân.");

            var itemPay = cms.Items.Add("Thu Tiền POS / VietQR");
            itemPay.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    DataRow row = grid.SelectedRows[0].Tag as DataRow;
                    if (row != null)
                    {
                        decimal rem = row["ConLai"] != DBNull.Value ? Convert.ToDecimal(row["ConLai"]) : 0;
                        string code = row["MaDon"]?.ToString() ?? "";
                        string cust = row["HoTen"]?.ToString() ?? "";
                        long donId = row.Table.Columns.Contains("DonDatSanID") ? Convert.ToInt64(row["DonDatSanID"]) : 0;
                        long hoaDonId = 0;
                        if (donId > 0)
                        {
                            var objHd = DatabaseManager.ExecuteScalar("SELECT TOP 1 HoaDonID FROM dbo.HoaDon WHERE DonDatSanID = @ID", new System.Data.SqlClient.SqlParameter("@ID", donId));
                            if (objHd != null && objHd != DBNull.Value) hoaDonId = Convert.ToInt64(objHd);
                            else
                            {
                                try
                                {
                                    var inv = new Services.PaymentService().CreateInvoice(donId, Session.AppSession.Current != null ? Session.AppSession.Current.TaiKhoanID : (int?)null);
                                    hoaDonId = inv.HoaDonID;
                                }
                                catch { }
                            }
                        }

                        if (hoaDonId > 0)
                        {
                            new QrPaymentForm(rem > 0 ? rem : 100000, code, cust, hoaDonId).ShowDialog(this);
                            LoadData();
                        }
                        else
                        {
                            SFToast.Show(this, "Không thể xác định hoặc tạo hóa đơn cho đơn đặt sân này.", "LỖI HÓA ĐƠN", SFIcon.AlertCircle, Colors.Danger);
                        }
                        return;
                    }
                }
                SFToast.Show(this, "Vui lòng chọn đơn đặt sân từ danh sách để thanh toán.", "CHƯA CHỌN ĐƠN", SFIcon.Info, Colors.Cyan);
            };

            var itemDetail = cms.Items.Add("Xem Chi Tiết Đơn Đặt Sân");
            itemDetail.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    DataRow row = grid.SelectedRows[0].Tag as DataRow;
                    if (row != null && row["DonDatSanID"] != DBNull.Value)
                    {
                        long donId = Convert.ToInt64(row["DonDatSanID"]);
                        new BookingDetailForm(donId).ShowDialog(this);
                    }
                }
            };

            var itemCancel = cms.Items.Add("Hủy Đơn & Hoàn Tiền");
            itemCancel.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    DataRow row = grid.SelectedRows[0].Tag as DataRow;
                    if (row != null && row["DonDatSanID"] != DBNull.Value)
                    {
                        long donId = Convert.ToInt64(row["DonDatSanID"]);
                        new CancelRefundForm(donId).ShowDialog(this);
                        LoadData();
                        return;
                    }
                }
                new CancelRefundForm().ShowDialog(this);
            };

            var itemExport = cms.Items.Add("Xuất Lịch Thi Đấu Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Lich_Thi_Dau_Quay", this);

            grid.ContextMenuStrip = cms;

            listCard.Controls.Add(grid);
            grid.BringToFront();
            root.Controls.Add(listCard, 0, 0);

            // Fast Actions Card
            SFCard actionsCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(10, 0, 0, 0) };
            Label aTitle = new Label { Text = "Thao Tác Nhanh", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            actionsCard.Controls.Add(aTitle);

            string[] ac = { "Xác nhận đơn", "Bắt đầu / Check-in", "Hoàn tất & Trả sân", "Thu tiền POS", "Xem lịch sử", "Hủy đơn" };
            int y = 48;
            for (int i = 0; i < ac.Length; i++)
            {
                string actionName = ac[i];
                SFButton b = new SFButton
                {
                    Text = actionName,
                    Variant = i == 0 ? SFButtonVariant.Primary : (i == ac.Length - 1 ? SFButtonVariant.Danger : SFButtonVariant.Secondary),
                    Location = new Point(4, y),
                    Size = new Size(185, 38)
                };

                if (actionName == "Xác nhận đơn") b.Click += (s, e) => PerformActionOnSelected("DA_XAC_NHAN", "Đã xác nhận đơn thành công.");
                else if (actionName == "Bắt đầu / Check-in") b.Click += (s, e) => PerformActionOnSelected("DANG_SU_DUNG", "Đã check-in bắt đầu thi đấu.");
                else if (actionName == "Hoàn tất & Trả sân") b.Click += (s, e) => PerformActionOnSelected("HOAN_THANH", "Đã hoàn tất thi đấu và trả sân.");
                else if (actionName == "Thu tiền POS") b.Click += (s, e) => itemPay.PerformClick();
                else if (actionName == "Xem lịch sử") b.Click += (s, e) => itemDetail.PerformClick();
                else if (actionName == "Hủy đơn") b.Click += (s, e) => itemCancel.PerformClick();

                actionsCard.Controls.Add(b);
                y += 48;
            }
            root.Controls.Add(actionsCard, 1, 0);

            Controls.Add(root);
            root.BringToFront();
        }

        private void PerformActionOnSelected(string status, string message)
        {
            if (grid.SelectedRows.Count > 0)
            {
                DataRow row = grid.SelectedRows[0].Tag as DataRow;
                if (row != null && row["DonDatSanID"] != DBNull.Value)
                {
                    long donId = Convert.ToInt64(row["DonDatSanID"]);
                    try
                    {
                        DatabaseManager.ExecuteNonQuery("UPDATE dbo.DonDatSan SET TrangThaiDon = @ST WHERE DonDatSanID = @ID",
                            new SqlParameter("@ST", status),
                            new SqlParameter("@ID", donId));
                        SFToast.Show(this, message, "CẬP NHẬT TRẠNG THÁI", SFIcon.Check, Colors.Emerald);
                        LoadData();
                        return;
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogError(ex, "STAFF_SCHEDULE", "PerformActionOnSelected");
                        SFToast.Show(this, "Lỗi cập nhật: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
                        return;
                    }
                }
            }
            SFToast.Show(this, "Vui lòng chọn đơn đặt sân từ danh sách.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Amber);
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_LichSanTheoNgay @Ngay = @Ngay", new SqlParameter("@Ngay", d.Value.Date));
                grid.Rows.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    string filterText = searchBox.Text != null ? searchBox.Text.Trim().ToLower() : "";
                    string filterSport = type.SelectedItem?.ToString() ?? "Tất cả môn";

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["DonDatSanID"] == DBNull.Value) continue;

                        string code = row["MaDon"]?.ToString() ?? "";
                        string cust = row["HoTen"]?.ToString() ?? "";
                        string field = row["TenSan"]?.ToString() ?? (row["MaSan"]?.ToString() ?? "");
                        string sport = row["TenLoaiSan"]?.ToString() ?? "";

                        if (filterSport != "Tất cả môn" && !sport.Contains(filterSport)) continue;
                        if (!string.IsNullOrEmpty(filterText) && !code.ToLower().Contains(filterText) && !cust.ToLower().Contains(filterText) && !field.ToLower().Contains(filterText))
                            continue;

                        DateTime bd = Convert.ToDateTime(row["BatDau"]);
                        DateTime kt = Convert.ToDateTime(row["KetThuc"]);
                        decimal tong = row["TongTien"] != DBNull.Value ? Convert.ToDecimal(row["TongTien"]) : 0;
                        decimal daTra = row["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(row["DaThanhToan"]) : 0;
                        string st = row["TrangThaiDon"]?.ToString() ?? "";

                        string stDisp = "Đã xác nhận";
                        if (st == "DANG_SU_DUNG") stDisp = "Đang thi đấu";
                        else if (st == "HOAN_THANH") stDisp = "Hoàn tất";
                        else if (st == "CHO_XAC_NHAN") stDisp = "Chờ duyệt";

                        int rowIndex = grid.Rows.Add(code, cust, field, bd.ToString("HH:mm"), kt.ToString("HH:mm"), string.Format("{0:N0}đ", tong), string.Format("{0:N0}đ", daTra), stDisp);
                        grid.Rows[rowIndex].Tag = row;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "STAFF_SCHEDULE", "LoadData");
                SFToast.Show(this, "Lỗi tải lịch sân: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }
    }

    public class StaffInvoicesView : UserControl
    {
        private SFDataGridView grid;
        private SFSearchBox searchBox;
        private SFMetricCard metricTongThu;
        private SFMetricCard metricChuaThu;
        private SFMetricCard metricChoXacNhan;

        public StaffInvoicesView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Hóa Đơn & Thu Tiền Quầy", "Theo dõi phiếu thu trong ca trực, công nợ và các giao dịch VietQR Napas 247 đang chờ đối soát."));

            TableLayoutPanel k = new TableLayoutPanel { Dock = DockStyle.Top, Height = 100, ColumnCount = 3, Padding = new Padding(0, 0, 0, 14) };
            for (int i = 0; i < 3; i++) k.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            metricTongThu = new SFMetricCard("TỔNG THU CA TRỰC", "0đ", "0 hóa đơn hoàn tất", SFIcon.Dollar, Colors.Emerald, "Đúng sổ sách");
            metricChuaThu = new SFMetricCard("CHƯA THANH TOÁN / NỢ", "0đ", "0 đơn cần thu khi trả sân", SFIcon.Clock, Colors.Amber);
            metricChoXacNhan = new SFMetricCard("CHỜ XÁC NHẬN CK", "0 Giao dịch", "0đ • VietQR", SFIcon.Bell, Colors.ElectricBlue);
            k.Controls.Add(metricTongThu, 0, 0);
            k.Controls.Add(metricChuaThu, 1, 0);
            k.Controls.Add(metricChoXacNhan, 2, 0);
            Controls.Add(k);
            k.BringToFront();

            FlowLayoutPanel tb = Ui.Toolbar();
            SFButton pay = new SFButton { Text = "Thu Tiền POS", Variant = SFButtonVariant.Primary, IconType = SFIcon.Dollar, Width = 150, Height = 36 };
            pay.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0 && grid.SelectedRows[0].Tag is DataRow r)
                {
                    decimal tongTien = r["TongTien"] != DBNull.Value ? Convert.ToDecimal(r["TongTien"]) : 0;
                    decimal daTra = r["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(r["DaThanhToan"]) : 0;
                    decimal conLai = r["ConLai"] != DBNull.Value ? Convert.ToDecimal(r["ConLai"]) : (tongTien - daTra);
                    string maDon = r["MaDon"]?.ToString() ?? "";
                    string hoTen = r["HoTen"]?.ToString() ?? "";
                    long hdId = r.Table.Columns.Contains("HoaDonID") && r["HoaDonID"] != DBNull.Value ? Convert.ToInt64(r["HoaDonID"]) : 0;
                    if (conLai <= 0)
                    {
                        SFToast.Show(this, "Hóa đơn này đã hoàn tất thanh toán (còn lại 0đ).", "THÔNG BÁO", SFIcon.Check, Colors.Emerald);
                        return;
                    }
                    if (new QrPaymentForm(tongTien, daTra, conLai, maDon, hoTen, hdId).ShowDialog(this) == DialogResult.OK)
                    {
                        LoadData();
                    }
                }
                else
                {
                    SFToast.Show(this, "Vui lòng chọn một hóa đơn từ danh sách để thanh toán.", "CHỌN HÓA ĐƠN", SFIcon.AlertCircle, Colors.Amber);
                }
            };
            tb.Controls.Add(pay);

            SFButton confirm = new SFButton { Text = "Xác Nhận Chuyển Khoản", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Check, Width = 185, Height = 36 };
            confirm.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0 && grid.SelectedRows[0].Tag is DataRow r)
                {
                    long hdId = r.Table.Columns.Contains("HoaDonID") && r["HoaDonID"] != DBNull.Value ? Convert.ToInt64(r["HoaDonID"]) : 0;
                    string maHd = r["MaHoaDon"]?.ToString() ?? "";
                    if (hdId > 0)
                    {
                        try
                        {
                            DataTable gdDt = DatabaseManager.ExecuteQuery(
                                "SELECT TOP 1 GiaoDichID FROM dbo.GiaoDichThanhToan WHERE HoaDonID = @HD AND TrangThai = 'CHO_XAC_NHAN' AND PhuongThuc IN ('VIETQR','CHUYEN_KHOAN')",
                                new SqlParameter("@HD", hdId));
                            if (gdDt != null && gdDt.Rows.Count > 0)
                            {
                                long gdId = Convert.ToInt64(gdDt.Rows[0]["GiaoDichID"]);
                                int userId = SessionGuard.RequireAccountId();
                                new PaymentService().ConfirmBankTransfer(gdId, userId, "XAC_NHAN_CHUYEN_KHOAN");
                                SFToast.Show(this, "Đã xác nhận giao dịch chuyển khoản cho hóa đơn " + maHd + " thành công!", "XÁC NHẬN CHUYỂN KHOẢN", SFIcon.Check, Colors.Emerald);
                                LoadData();
                            }
                            else
                            {
                                SFToast.Show(this, "Hóa đơn này không có giao dịch chuyển khoản nào đang chờ xác nhận.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Amber);
                            }
                            return;
                        }
                        catch (Exception ex)
                        {
                            AppLogger.LogError(ex, "STAFF_INVOICES", "ConfirmTransfer");
                            SFToast.Show(this, "Lỗi xác nhận: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
                            return;
                        }
                    }
                }
                SFToast.Show(this, "Vui lòng chọn một hóa đơn từ danh sách để xác nhận.", "CHỌN HÓA ĐƠN", SFIcon.AlertCircle, Colors.Amber);
            };
            tb.Controls.Add(confirm);

            SFButton refresh = new SFButton { Text = "Làm Mới", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Refresh, Width = 110, Height = 36 };
            refresh.Click += (s, e) => LoadData();
            tb.Controls.Add(refresh);

            searchBox = new SFSearchBox { PlaceholderText = "Mã HĐ / mã đơn / khách...", Width = 230, Height = 36 };
            Timer debounceTimer = new Timer { Interval = 300 };
            debounceTimer.Tick += (s, e) =>
            {
                debounceTimer.Stop();
                if (!IsDisposed) LoadData();
            };
            searchBox.TextChanged += (s, e) =>
            {
                debounceTimer.Stop();
                debounceTimer.Start();
            };
            tb.Controls.Add(searchBox);
            Controls.Add(tb);
            tb.BringToFront();

            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Label lTitle = new Label { Text = "Tất Cả Hóa Đơn & Phiếu Thu Trong Ca (SQL Server)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(lTitle);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("InvoiceId", "Mã HĐ");
            grid.Columns.Add("BookingId", "Mã đơn");
            grid.Columns.Add("Customer", "Khách hàng");
            grid.Columns.Add("Total", "Tổng tiền");
            grid.Columns.Add("Paid", "Đã trả");
            grid.Columns.Add("Remaining", "Còn lại");
            grid.Columns.Add("Method", "Phương thức");
            grid.Columns.Add("Status", "Trạng thái");

            grid.Columns["InvoiceId"].Width = 120;
            grid.Columns["BookingId"].Width = 110;
            grid.Columns["Customer"].Width = 180;
            grid.Columns["Total"].Width = 110;
            grid.Columns["Paid"].Width = 110;
            grid.Columns["Remaining"].Width = 100;
            grid.Columns["Method"].Width = 120;
            grid.Columns["Status"].Width = 125;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemPay = cms.Items.Add("Thu Tiền POS / VietQR Napas");
            itemPay.Click += (s, e) => pay.PerformClick();
            var itemConfirm = cms.Items.Add("Xác Nhận Đã Chuyển Khoản");
            itemConfirm.Click += (s, e) => confirm.PerformClick();
            cms.Items.Add("-");
            var itemExport = cms.Items.Add("Xuất Danh Sách Hóa Đơn Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Hoa_Don_Quay_Staff", this);
            grid.ContextMenuStrip = cms;

            grid.CellDoubleClick += (sender, e) => pay.PerformClick();

            card.Controls.Add(grid);
            grid.BringToFront();
            Controls.Add(card);
            card.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachHoaDon");
                grid.Rows.Clear();
                decimal totalThu = 0;
                decimal totalNo = 0;
                int countChoXacNhan = 0;
                decimal tienChoXacNhan = 0;
                int countHoanTat = 0;
                int countNo = 0;

                if (dt != null && dt.Rows.Count > 0)
                {
                    string filter = searchBox.Text != null ? searchBox.Text.Trim().ToLower() : "";

                    foreach (DataRow row in dt.Rows)
                    {
                        string hd = row["MaHoaDon"]?.ToString() ?? "";
                        string maDon = row["MaDon"]?.ToString() ?? "";
                        string cust = row["HoTen"]?.ToString() ?? "";

                        decimal tong = row["TongTien"] != DBNull.Value ? Convert.ToDecimal(row["TongTien"]) : 0;
                        decimal daTra = row["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(row["DaThanhToan"]) : 0;
                        decimal conLai = row["ConLai"] != DBNull.Value ? Convert.ToDecimal(row["ConLai"]) : 0;
                        string st = row["TrangThaiThanhToan"]?.ToString() ?? "";

                        totalThu += daTra;
                        if (conLai > 0)
                        {
                            totalNo += conLai;
                            countNo++;
                        }
                        if (st == "CHO_XAC_NHAN")
                        {
                            countChoXacNhan++;
                            tienChoXacNhan += conLai > 0 ? conLai : tong;
                        }
                        else if (st == "DA_THANH_TOAN")
                        {
                            countHoanTat++;
                        }

                        string phone = row.Table.Columns.Contains("SoDienThoai") ? (row["SoDienThoai"]?.ToString() ?? "") : "";
                        if (!string.IsNullOrEmpty(filter) && !hd.ToLower().Contains(filter) && !maDon.ToLower().Contains(filter) && !cust.ToLower().Contains(filter) && !phone.ToLower().Contains(filter))
                            continue;

                        string stDisp = "Chưa thanh toán";
                        if (st == "DA_THANH_TOAN") stDisp = "Đã thanh toán";
                        else if (st == "THANH_TOAN_MOT_PHAN") stDisp = "Một phần";
                        else if (st == "CHO_XAC_NHAN") stDisp = "Chờ duyệt CK";

                        int rowIndex = grid.Rows.Add(hd, maDon, cust, string.Format("{0:N0}đ", tong), string.Format("{0:N0}đ", daTra), string.Format("{0:N0}đ", conLai), "VietQR / Tiền mặt", stDisp);
                        grid.Rows[rowIndex].Tag = row;
                    }
                }

                if (metricTongThu != null) metricTongThu.Value = string.Format("{0:N0}đ", totalThu);
                if (metricTongThu != null) metricTongThu.Subtitle = string.Format("{0} hóa đơn hoàn tất", countHoanTat);
                if (metricChuaThu != null) metricChuaThu.Value = string.Format("{0:N0}đ", totalNo);
                if (metricChuaThu != null) metricChuaThu.Subtitle = string.Format("{0} đơn cần thu khi trả sân", countNo);
                if (metricChoXacNhan != null) metricChoXacNhan.Value = string.Format("{0} Giao dịch", countChoXacNhan);
                if (metricChoXacNhan != null) metricChoXacNhan.Subtitle = string.Format("{0:N0}đ • VietQR", tienChoXacNhan);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "STAFF_INVOICES", "LoadData");
                SFToast.Show(this, "Lỗi tải hóa đơn: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }
    }

    public class StaffCustomersView : UserControl
    {
        private SFDataGridView grid;
        private SFSearchBox searchBox;
        private SFMetricCard metricTongKh;
        private SFMetricCard metricVip;
        private SFMetricCard metricKhMoi;
        private SFMetricCard metricDiem;

        public StaffCustomersView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Quản Lý Khách Hàng & Thẻ Thành Viên", "Tra cứu hồ sơ, lịch sử tích điểm và cập nhật thông tin khách đặt sân tại quầy."));

            TableLayoutPanel k = new TableLayoutPanel { Dock = DockStyle.Top, Height = 100, ColumnCount = 4, Padding = new Padding(0, 0, 0, 14) };
            for (int i = 0; i < 4; i++) k.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            metricTongKh = new SFMetricCard("TỔNG KHÁCH HÀNG", "0 Khách", "Đã định danh trên CSDL", SFIcon.User, Colors.ElectricBlue);
            metricVip = new SFMetricCard("VIP / THÂN THIẾT", "0 Khách", "Hội viên Gold & Silver", SFIcon.Star, Colors.Purple);
            metricKhMoi = new SFMetricCard("KHÁCH MỚI THÁNG", "0 Khách", "Tạo trực tiếp tại quầy", SFIcon.Plus, Colors.Emerald);
            metricDiem = new SFMetricCard("ĐIỂM TÍCH LŨY", "0 pts", "0đ quy đổi", SFIcon.Dollar, Colors.Amber);
            k.Controls.Add(metricTongKh, 0, 0);
            k.Controls.Add(metricVip, 1, 0);
            k.Controls.Add(metricKhMoi, 2, 0);
            k.Controls.Add(metricDiem, 3, 0);
            Controls.Add(k);
            k.BringToFront();

            FlowLayoutPanel tb = Ui.Toolbar();
            SFButton add = new SFButton { Text = "Thêm Khách", Variant = SFButtonVariant.Primary, IconType = SFIcon.Plus, Width = 150, Height = 36 };
            add.Click += (s, e) =>
            {
                if (new CustomerRegisterForm().ShowDialog(this) == DialogResult.OK)
                    LoadData();
            };
            tb.Controls.Add(add);

            SFButton account = new SFButton { Text = "Cấp Tài Khoản App", Variant = SFButtonVariant.Secondary, IconType = SFIcon.User, Width = 165, Height = 36 };
            account.Click += (s, e) =>
            {
                if (new CustomerRegisterForm().ShowDialog(this) == DialogResult.OK)
                    LoadData();
            };
            tb.Controls.Add(account);

            SFButton refresh = new SFButton { Text = "Làm Mới", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Refresh, Width = 110, Height = 36 };
            refresh.Click += (s, e) => LoadData();
            tb.Controls.Add(refresh);

            searchBox = new SFSearchBox { PlaceholderText = "F3 • Tên / SĐT / mã KH...", Width = 240, Height = 36 };
            searchBox.TextChanged += (s, e) => LoadData();
            tb.Controls.Add(searchBox);
            Controls.Add(tb);
            tb.BringToFront();

            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Label lTitle = new Label { Text = "Danh Sách Khách Hàng Hoạt Động (SQL Server)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(lTitle);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("Code", "Mã KH");
            grid.Columns.Add("Name", "Họ và tên");
            grid.Columns.Add("Phone", "Số điện thoại");
            grid.Columns.Add("Tier", "Phân hạng");
            grid.Columns.Add("Points", "Điểm tích lũy");
            grid.Columns.Add("Orders", "Lượt đặt");
            grid.Columns.Add("Total", "Số dư ví");
            grid.Columns.Add("Status", "Trạng thái");

            grid.Columns["Code"].Width = 100;
            grid.Columns["Name"].Width = 190;
            grid.Columns["Phone"].Width = 130;
            grid.Columns["Tier"].Width = 110;
            grid.Columns["Points"].Width = 110;
            grid.Columns["Orders"].Width = 90;
            grid.Columns["Total"].Width = 130;
            grid.Columns["Status"].Width = 110;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemDetail = cms.Items.Add("Xem Chi Tiết Khách Hàng");
            itemDetail.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0 && grid.SelectedRows[0].Tag is DataRow r)
                {
                    int khId = r.Table.Columns.Contains("KhachHangID") && r["KhachHangID"] != DBNull.Value ? Convert.ToInt32(r["KhachHangID"]) : 0;
                    new CustomerDetailForm(khId).ShowDialog(this);
                }
                else
                {
                    new CustomerDetailForm().ShowDialog(this);
                }
            };
            cms.Items.Add("-");
            var itemExport = cms.Items.Add("Xuất Danh Sách Khách Hàng Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Danh_Sach_Khach_Hang_Staff", this);
            grid.ContextMenuStrip = cms;

            grid.CellDoubleClick += (sender, e) => itemDetail.PerformClick();

            card.Controls.Add(grid);
            grid.BringToFront();
            Controls.Add(card);
            card.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachKhachHang");
                grid.Rows.Clear();
                int totalVip = 0;
                int totalPts = 0;

                if (dt != null && dt.Rows.Count > 0)
                {
                    string filter = searchBox.Text != null ? searchBox.Text.Trim().ToLower() : "";

                    foreach (DataRow row in dt.Rows)
                    {
                        string ma = row["MaKhachHang"]?.ToString() ?? "";
                        string name = row["HoTen"]?.ToString() ?? "";
                        string phone = row["SoDienThoai"]?.ToString() ?? "";
                        string tier = (row.Table.Columns.Contains("TenHang") && row["TenHang"] != DBNull.Value) ? row["TenHang"].ToString() :
                                      ((row.Table.Columns.Contains("HangThanhVien") && row["HangThanhVien"] != DBNull.Value) ? row["HangThanhVien"].ToString() : "THUONG");
                        int pts = row["DiemTichLuy"] != DBNull.Value ? Convert.ToInt32(row["DiemTichLuy"]) : 0;
                        decimal vi = (row.Table.Columns.Contains("SoDuViCoc") && row["SoDuViCoc"] != DBNull.Value) ? Convert.ToDecimal(row["SoDuViCoc"]) :
                                     ((row.Table.Columns.Contains("SoDuVi") && row["SoDuVi"] != DBNull.Value) ? Convert.ToDecimal(row["SoDuVi"]) : 0);
                        bool active = row["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(row["DangHoatDong"]);

                        totalPts += pts;
                        if (tier.IndexOf("VIP", StringComparison.OrdinalIgnoreCase) >= 0 || tier.IndexOf("Gold", StringComparison.OrdinalIgnoreCase) >= 0 || tier.IndexOf("Silver", StringComparison.OrdinalIgnoreCase) >= 0)
                            totalVip++;

                        if (!string.IsNullOrEmpty(filter) && !ma.ToLower().Contains(filter) && !name.ToLower().Contains(filter) && !phone.Contains(filter))
                            continue;

                        int rowIndex = grid.Rows.Add(ma, name, phone, tier, string.Format("{0} pts", pts), "—", string.Format("{0:N0}đ", vi), active ? "Hoạt động" : "Tạm khóa");
                        grid.Rows[rowIndex].Tag = row;
                    }

                    if (metricTongKh != null) metricTongKh.Value = string.Format("{0} Khách", dt.Rows.Count);
                    if (metricVip != null) metricVip.Value = string.Format("{0} Khách", totalVip);
                    if (metricKhMoi != null) metricKhMoi.Value = string.Format("{0} Khách", dt.Rows.Count);
                    if (metricDiem != null) metricDiem.Value = string.Format("{0:N0} pts", totalPts);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "STAFF_CUSTOMERS", "LoadData");
                SFToast.Show(this, "Lỗi tải khách hàng: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }
    }

    public class StaffFieldsReadOnlyView : UserControl
    {
        private SFDataGridView grid;
        private SFSearchBox s;

        public StaffFieldsReadOnlyView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Danh Mục Sân Thể Thao", "Tra cứu thông số kỹ thuật, giá thuê niêm yết và tình trạng khả dụng thời gian thực."));

            Panel notice = Ui.Notice("CHẾ ĐỘ CHỈ XEM (READ-ONLY)", "Nhân viên quầy không có quyền thêm, sửa, xóa hoặc thay đổi giá sân. Dữ liệu do Quản trị viên quản lý.", Colors.Amber, Colors.WarningBg);
            Controls.Add(notice);
            notice.BringToFront();

            FlowLayoutPanel tb = Ui.Toolbar();
            s = new SFSearchBox { PlaceholderText = "Tìm mã / tên sân...", Width = 230, Height = 36 };
            s.TextChanged += (sender, e) => LoadData();
            tb.Controls.Add(s);

            SFButton check = new SFButton { Text = "Kiểm Tra Lịch Trống", Variant = SFButtonVariant.Primary, IconType = SFIcon.Calendar, Width = 175, Height = 36 };
            check.Click += (sender, e) => LoadData();
            tb.Controls.Add(check);
            Controls.Add(tb);
            tb.BringToFront();

            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Label lTitle = new Label { Text = "Thông Tin Sân Vận Hành Thời Gian Thực (SQL Server)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(lTitle);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("Code", "Mã sân");
            grid.Columns.Add("Name", "Tên sân");
            grid.Columns.Add("Cluster", "Cụm sân");
            grid.Columns.Add("Sport", "Bộ môn");
            grid.Columns.Add("Surface", "Mặt sân");
            grid.Columns.Add("Price", "Giá cơ sở");
            grid.Columns.Add("Status", "Tình trạng");
            grid.Columns.Add("Next", "Lịch kế tiếp");

            grid.Columns["Code"].Width = 95;
            grid.Columns["Name"].Width = 180;
            grid.Columns["Cluster"].Width = 120;
            grid.Columns["Sport"].Width = 120;
            grid.Columns["Surface"].Width = 130;
            grid.Columns["Price"].Width = 110;
            grid.Columns["Status"].Width = 110;
            grid.Columns["Next"].Width = 100;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemExport = cms.Items.Add("Xuất Danh Mục Sân Ra Excel");
            itemExport.Click += (sender, e) => ExportHelper.ExportToExcel(grid, "Danh_Muc_San_Staff", this);
            grid.ContextMenuStrip = cms;

            card.Controls.Add(grid);
            grid.BringToFront();
            Controls.Add(card);
            card.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachSan");
                grid.Rows.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    string filter = s.Text != null ? s.Text.Trim().ToLower() : "";

                    foreach (DataRow row in dt.Rows)
                    {
                        string ma = row["MaSan"]?.ToString() ?? "";
                        string name = row["TenSan"]?.ToString() ?? "";
                        string cum = row["TenCumSan"]?.ToString() ?? "Cụm chính";
                        string sport = row["TenLoaiSan"]?.ToString() ?? "";
                        decimal gia = (row.Table.Columns.Contains("GiaCoBan") && row["GiaCoBan"] != DBNull.Value) ? Convert.ToDecimal(row["GiaCoBan"]) :
                                      ((row.Table.Columns.Contains("GiaMacDinh") && row["GiaMacDinh"] != DBNull.Value) ? Convert.ToDecimal(row["GiaMacDinh"]) : 300000);
                        string st = (row.Table.Columns.Contains("TrangThaiVanHanh") && row["TrangThaiVanHanh"] != DBNull.Value) ? row["TrangThaiVanHanh"].ToString() :
                                    ((row.Table.Columns.Contains("TrangThai") && row["TrangThai"] != DBNull.Value) ? row["TrangThai"].ToString() : "SAN_SANG");

                        if (!string.IsNullOrEmpty(filter) && !ma.ToLower().Contains(filter) && !name.ToLower().Contains(filter))
                            continue;

                        string stDisp = "Sẵn sàng";
                        if (st == "DANG_SU_DUNG") stDisp = "Đang sử dụng";
                        else if (st == "BAO_TRI") stDisp = "Bảo trì";

                        int rowIndex = grid.Rows.Add(ma, name, cum, sport, "Tiêu chuẩn Pro", string.Format("{0:N0}đ", gia), stDisp, "Trống");
                        grid.Rows[rowIndex].Tag = row;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "STAFF_FIELDS", "LoadData");
                SFToast.Show(this, "Lỗi tải danh mục sân: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }
    }

    public class StaffFieldTypesReadOnlyView : UserControl
    {
        private SFDataGridView grid;
        private Label textDetail;

        public StaffFieldTypesReadOnlyView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Loại Sân & Quy Chuẩn Kỹ Thuật", "Tra cứu danh mục bộ môn, thông số quy chuẩn mặt sân và chính sách đơn giá cơ sở."));

            Panel notice = Ui.Notice("DỮ LIỆU ĐANG KHÓA", "Toàn bộ danh mục loại sân và chính sách đơn giá do Quản trị viên quản lý. Nhân viên quầy chỉ có quyền tra cứu.", Colors.Amber, Colors.WarningBg);
            Controls.Add(notice);
            notice.BringToFront();

            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));

            SFCard listCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(0, 0, 10, 0) };
            Label lTitle = new Label { Text = "Danh Mục Loại Sân & Tiêu Chuẩn Cơ Sở (SQL Server)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            listCard.Controls.Add(lTitle);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("Code", "Mã");
            grid.Columns.Add("Name", "Tên loại môn");
            grid.Columns.Add("Count", "Quy mô");
            grid.Columns.Add("Size", "Kích thước");
            grid.Columns.Add("Surface", "Mặt sân");
            grid.Columns.Add("BasePrice", "Giá cơ sở");

            grid.Columns["Code"].Width = 80;
            grid.Columns["Name"].Width = 170;
            grid.Columns["Count"].Width = 85;
            grid.Columns["Size"].Width = 110;
            grid.Columns["Surface"].Width = 120;
            grid.Columns["BasePrice"].Width = 110;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemExport = cms.Items.Add("Xuất Bảng Quy Chuẩn Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Quy_Chuan_Loai_San_Staff", this);
            grid.ContextMenuStrip = cms;

            grid.SelectionChanged += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0 && grid.SelectedRows[0].Tag is DataRow r && textDetail != null)
                {
                    string nm = r["TenLoaiSan"]?.ToString() ?? "";
                    int cnt = (r.Table.Columns.Contains("SoLuongSan") && r["SoLuongSan"] != DBNull.Value) ? Convert.ToInt32(r["SoLuongSan"]) :
                              ((r.Table.Columns.Contains("SoSanHienCo") && r["SoSanHienCo"] != DBNull.Value) ? Convert.ToInt32(r["SoSanHienCo"]) : 0);
                    decimal minP = (r.Table.Columns.Contains("GiaThapNhat") && r["GiaThapNhat"] != DBNull.Value) ? Convert.ToDecimal(r["GiaThapNhat"]) : 0;
                    decimal maxP = (r.Table.Columns.Contains("GiaCaoNhat") && r["GiaCaoNhat"] != DBNull.Value) ? Convert.ToDecimal(r["GiaCaoNhat"]) : 0;
                    textDetail.Text = string.Format(
                        "{0}\n\nSố lượng sân: {1} sân\nMặt sân: Tiêu chuẩn thi đấu\nHệ thống đèn: Đạt chuẩn\nPhòng thay đồ & Tiện ích: Đầy đủ\n\nĐƠN GIÁ CƠ SỞ:\n{2:N0}đ - {3:N0}đ / giờ\n\nPhụ thu giờ vàng (17h-22h): Theo chính sách\nPhụ thu cuối tuần (T7, CN): Theo chính sách",
                        nm, cnt, minP, maxP);
                }
            };

            listCard.Controls.Add(grid);
            grid.BringToFront();
            root.Controls.Add(listCard, 0, 0);

            // Right Specification Details
            SFCard detail = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(10, 0, 0, 0) };
            Label dTitle = new Label { Text = "Quy Chuẩn Đang Chọn", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            detail.Controls.Add(dTitle);

            textDetail = new Label
            {
                Text = "Chọn một loại sân từ danh sách để xem quy chuẩn kỹ thuật và mức giá chi tiết.",
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Fill,
                Padding = new Padding(4, 8, 4, 4)
            };
            detail.Controls.Add(textDetail);
            textDetail.BringToFront();
            root.Controls.Add(detail, 1, 0);

            Controls.Add(root);
            root.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachLoaiSan");
                grid.Rows.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string ma = row["MaLoaiSan"]?.ToString() ?? "";
                        string name = row["TenLoaiSan"]?.ToString() ?? "";
                        int soSan = (row.Table.Columns.Contains("SoLuongSan") && row["SoLuongSan"] != DBNull.Value) ? Convert.ToInt32(row["SoLuongSan"]) :
                                    ((row.Table.Columns.Contains("SoSanHienCo") && row["SoSanHienCo"] != DBNull.Value) ? Convert.ToInt32(row["SoSanHienCo"]) : 0);
                        decimal giaMin = (row.Table.Columns.Contains("GiaThapNhat") && row["GiaThapNhat"] != DBNull.Value) ? Convert.ToDecimal(row["GiaThapNhat"]) : 0;
                        decimal giaMax = (row.Table.Columns.Contains("GiaCaoNhat") && row["GiaCaoNhat"] != DBNull.Value) ? Convert.ToDecimal(row["GiaCaoNhat"]) : 0;

                        string giaHienThi = giaMin > 0 && giaMax > 0 ? (giaMin == giaMax ? string.Format("{0:N0}đ/h", giaMin) : string.Format("{0:N0}đ - {1:N0}đ/h", giaMin, giaMax)) :
                                            ((row.Table.Columns.Contains("GiaThamKhao") && row["GiaThamKhao"] != DBNull.Value) ? string.Format("{0:N0}đ/h", Convert.ToDecimal(row["GiaThamKhao"])) : "300.000đ/h");

                        int rowIndex = grid.Rows.Add(ma, name, string.Format("{0} sân", soSan), "Tiêu chuẩn", "Pro Turf", giaHienThi);
                        grid.Rows[rowIndex].Tag = row;
                    }

                    if (grid.Rows.Count > 0)
                    {
                        grid.Rows[0].Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "STAFF_FIELD_TYPES", "LoadData");
                SFToast.Show(this, "Lỗi tải quy chuẩn loại sân: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }
    }

    public class StaffStatisticsView : UserControl
    {
        private SFMetricCard cardRev;
        private SFMetricCard cardOrders;
        private SFDataGridView g;

        public StaffStatisticsView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Thống Kê Ca Trực & Báo Cáo Nhanh", "Doanh thu, lượt đặt, phương thức thanh toán và đối soát theo ca nhân viên hiện tại."));

            FlowLayoutPanel tb = Ui.Toolbar();
            SFButton show = new SFButton { Text = "Xem Báo Cáo", Variant = SFButtonVariant.Primary, IconType = SFIcon.Chart, Width = 140, Height = 36 };
            show.Click += (s, e) => LoadData();
            tb.Controls.Add(show);

            SFButton handover = new SFButton { Text = "Bàn Giao / Đóng Ca (F8)", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Clock, Width = 190, Height = 36 };
            handover.Click += (s, e) => new StaffShiftDialog().ShowDialog(this);
            tb.Controls.Add(handover);
            Controls.Add(tb);
            tb.BringToFront();

            TableLayoutPanel k = new TableLayoutPanel { Dock = DockStyle.Top, Height = 100, ColumnCount = 4, Padding = new Padding(0, 0, 0, 14) };
            for (int i = 0; i < 4; i++) k.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            cardRev = new SFMetricCard("DOANH THU CA", "0đ", "Quyết toán ca trực", SFIcon.Dollar, Colors.Emerald);
            cardOrders = new SFMetricCard("LƯỢT ĐẶT SÂN", "0 Đơn", "0 đơn hoàn tất", SFIcon.Calendar, Colors.ElectricBlue);
            k.Controls.Add(cardRev, 0, 0);
            k.Controls.Add(cardOrders, 1, 0);
            k.Controls.Add(new SFMetricCard("ĐÃ THANH TOÁN", "100%", "Đạt 100% quyết toán", SFIcon.Check, Colors.Purple), 2, 0);
            k.Controls.Add(new SFMetricCard("TRẠNG THÁI CA", "Đang trực", "Phiên hoạt động", SFIcon.User, Colors.Amber), 3, 0);
            Controls.Add(k);
            k.BringToFront();

            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));

            // Chart
            SFCard cardChart = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(0, 0, 10, 0) };
            Label cTitle = new Label { Text = "Doanh Thu Theo Khung Giờ Trong Ca", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            cardChart.Controls.Add(cTitle);

            SFChart chart = new SFChart
            {
                Dock = DockStyle.Fill,
                AccentColor = Colors.ElectricBlue,
                XLabels = new[] { "06h", "08h", "10h", "12h", "14h" },
                Values = new double[] { 2.1, 4.3, 7.8, 11.2, 14.85 },
                MaxValue = 16.0
            };
            cardChart.Controls.Add(chart);
            chart.BringToFront();
            root.Controls.Add(cardChart, 0, 0);

            // Reconciliation Grid
            SFCard sumCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(10, 0, 0, 0) };
            Label sTitle = new Label { Text = "Đối Soát Phương Thức Thanh Toán (SQL Server)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            sumCard.Controls.Add(sTitle);

            g = new SFDataGridView { Dock = DockStyle.Fill };
            g.Columns.Add("Method", "Phương thức");
            g.Columns.Add("Count", "Số phiếu");
            g.Columns.Add("Amount", "Tổng tiền");

            g.Columns["Method"].Width = 140;
            g.Columns["Count"].Width = 90;
            g.Columns["Amount"].Width = 120;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemExport = cms.Items.Add("Xuất Đối Soát Ca Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(g, "Doi_Soat_Ca_Staff", this);
            g.ContextMenuStrip = cms;

            sumCard.Controls.Add(g);
            g.BringToFront();
            root.Controls.Add(sumCard, 1, 0);

            Controls.Add(root);
            root.BringToFront();
        }

        private void LoadData()
        {
            try
            {
                int? nvId = Session.AppSession.Current.NhanVienID > 0 ? (int?)Session.AppSession.Current.NhanVienID : null;
                DataSet ds = DatabaseManager.ExecuteDataSet("EXEC dbo.sp_UI_StaffThongKeCa @NhanVienID = @NVID",
                    new SqlParameter("@NVID", (object)nvId ?? DBNull.Value));

                g.Rows.Clear();
                if (ds != null && ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    DataRow r = ds.Tables[1].Rows[0];
                    int soGd = r["SoGiaoDich"] != DBNull.Value ? Convert.ToInt32(r["SoGiaoDich"]) : 0;
                    decimal tongThu = r["TongDaThu"] != DBNull.Value ? Convert.ToDecimal(r["TongDaThu"]) : 0;
                    decimal tm = r["TienMat"] != DBNull.Value ? Convert.ToDecimal(r["TienMat"]) : 0;
                    decimal ck = r["ChuyenKhoan"] != DBNull.Value ? Convert.ToDecimal(r["ChuyenKhoan"]) : 0;

                    cardRev.Value = string.Format("{0:N0}đ", tongThu);
                    cardOrders.Value = string.Format("{0} Đơn", soGd);

                    g.Rows.Add("Tiền mặt tại quầy", "Tiền mặt", string.Format("{0:N0}đ", tm));
                    g.Rows.Add("VietQR / Chuyển khoản", "Điện tử", string.Format("{0:N0}đ", ck));
                    g.Rows.Add("Tổng quyết toán ca", string.Format("{0} đơn", soGd), string.Format("{0:N0}đ", tongThu));
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "STAFF_STATS", "LoadData");
                SFToast.Show(this, "Lỗi tải thống kê ca: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }
    }

    public class StaffAccountView : UserControl
    {
        private SFTextBox txtHoTen;
        private SFTextBox txtSdt;
        private SFTextBox txtEmail;
        private Label lblName;
        private Label lblInfo;
        private int currentNvId;

        public StaffAccountView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadProfile();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Tài Khoản Cá Nhân & Ca Hiện Tại", "Cập nhật thông tin liên hệ, bảo mật tài khoản và theo dõi trạng thái ca trực."));

            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));

            // Left: Shift Profile Card
            SFCard profile = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(20), Margin = new Padding(0, 0, 10, 0) };
            lblName = new Label { Text = Session.AppSession.Current.HoTen ?? "NHÂN VIÊN QUẦY", Font = Typography.PageTitle, ForeColor = Colors.TextPrimary, Location = new Point(8, 20), AutoSize = true };
            profile.Controls.Add(lblName);

            Label badge = new Label
            {
                Text = "● THU NGÂN QUẦY • POS #" + (Session.AppSession.Current.NhanVienID > 0 ? Session.AppSession.Current.NhanVienID.ToString() : "01") + " (ONLINE)",
                Font = Typography.CaptionBold,
                ForeColor = Colors.Emerald,
                BackColor = Colors.SuccessBg,
                Padding = new Padding(8, 5, 8, 5),
                Location = new Point(8, 60),
                AutoSize = true
            };
            profile.Controls.Add(badge);

            lblInfo = new Label
            {
                Text = "Đang tải thông tin nhân viên từ cơ sở dữ liệu...",
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Location = new Point(8, 105),
                Size = new Size(290, 260),
                AutoSize = false
            };
            profile.Controls.Add(lblInfo);
            root.Controls.Add(profile, 0, 0);

            // Right: Information & Security
            SFCard form = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(20), Margin = new Padding(10, 0, 0, 0) };
            Label fTitle = new Label { Text = "Thông Tin Cá Nhân & Bảo Mật", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            form.Controls.Add(fTitle);

            Panel pForm = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            AddField(pForm, "Mã nhân viên", "NV-0001", 10, true, out _);
            AddField(pForm, "Họ và tên", Session.AppSession.Current.HoTen ?? "", 74, false, out txtHoTen);
            AddField(pForm, "Số điện thoại liên hệ", "", 138, false, out txtSdt);
            AddField(pForm, "Địa chỉ Email", "", 202, false, out txtEmail);
            AddField(pForm, "Chức vụ", "Nhân viên quầy", 266, true, out _);

            FlowLayoutPanel actions = new FlowLayoutPanel { Location = new Point(4, 340), Size = new Size(450, 48) };
            SFButton btnSave = new SFButton { Text = "Lưu Thay Đổi", Variant = SFButtonVariant.Primary, IconType = SFIcon.Check, Width = 140, Height = 38 };
            btnSave.Click += (s, e) => SaveProfile();
            actions.Controls.Add(btnSave);

            SFButton btnPass = new SFButton { Text = "Đổi Mật Khẩu", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Lock, Width = 140, Height = 38 };
            btnPass.Click += (s, e) => new ChangePasswordForm(false).ShowDialog(this);
            actions.Controls.Add(btnPass);

            SFButton btnShift = new SFButton { Text = "Bàn Giao Ca (F8)", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Clock, Width = 150, Height = 38 };
            btnShift.Click += (s, e) => new StaffShiftDialog().ShowDialog(this);
            actions.Controls.Add(btnShift);

            pForm.Controls.Add(actions);
            form.Controls.Add(pForm);
            pForm.BringToFront();
            root.Controls.Add(form, 1, 0);

            Controls.Add(root);
            root.BringToFront();
        }

        private void LoadProfile()
        {
            string maNv = "NV-0001";
            string hoTen = Session.AppSession.Current.HoTen ?? "Nhân viên";
            string sdt = "";
            string email = "";
            string chucVu = "Nhân viên vận hành";
            currentNvId = (Session.AppSession.Current != null && Session.AppSession.Current.NhanVienID.HasValue) ? Session.AppSession.Current.NhanVienID.Value : 0;

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(
                    "SELECT TOP 1 NhanVienID, MaNhanVien, HoTen, SoDienThoai, Email, ChucVu FROM dbo.NhanVien " +
                    "WHERE (@NVID IS NOT NULL AND NhanVienID = @NVID) OR (@TKID IS NOT NULL AND TaiKhoanID = @TKID) " +
                    "ORDER BY NhanVienID ASC",
                    new SqlParameter("@NVID", currentNvId > 0 ? (object)currentNvId : DBNull.Value),
                    new SqlParameter("@TKID", Session.AppSession.Current.TaiKhoanID > 0 ? (object)Session.AppSession.Current.TaiKhoanID : DBNull.Value));

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    currentNvId = Convert.ToInt32(r["NhanVienID"]);
                    maNv = r["MaNhanVien"]?.ToString() ?? maNv;
                    hoTen = r["HoTen"]?.ToString() ?? hoTen;
                    sdt = r["SoDienThoai"]?.ToString() ?? "";
                    email = r["Email"]?.ToString() ?? "";
                    chucVu = r["ChucVu"]?.ToString() ?? chucVu;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "STAFF_ACCOUNT", "LoadProfile");
            }

            lblName.Text = hoTen.ToUpper();
            if (txtHoTen != null) txtHoTen.Text = hoTen;
            if (txtSdt != null) txtSdt.Text = sdt;
            if (txtEmail != null) txtEmail.Text = email;

            string khungCa = "Chưa phân ca hôm nay";
            if (currentNvId > 0)
            {
                try
                {
                    DataTable dtCa = DatabaseManager.ExecuteQuery(
                        "EXEC dbo.sp_UI_LichPhanCa @TuNgay = @HomNay, @DenNgay = @HomNay, @NhanVienID = @ID",
                        new SqlParameter("@HomNay", DateTime.Today),
                        new SqlParameter("@ID", currentNvId));
                    if (dtCa != null && dtCa.Rows.Count > 0)
                    {
                        var rowCa = dtCa.Rows[0];
                        object bdObj = rowCa["GioBatDau"];
                        object ktObj = rowCa["GioKetThuc"];
                        string cumSan = rowCa["TenCumSan"]?.ToString() ?? "";
                        if (bdObj != DBNull.Value && ktObj != DBNull.Value)
                        {
                            TimeSpan bd = (TimeSpan)bdObj;
                            TimeSpan kt = (TimeSpan)ktObj;
                            khungCa = string.Format("{0:hh\\:mm} - {1:hh\\:mm}", bd, kt);
                            if (!string.IsNullOrWhiteSpace(cumSan))
                            {
                                khungCa += " (" + cumSan + ")";
                            }
                        }
                    }
                }
                catch (Exception exCa)
                {
                    AppLogger.LogWarning("Load real shift in StaffAccountView failed: " + exCa.Message);
                }
            }

            lblInfo.Text = string.Format(
                "Mã nhân viên: {0}\nChức vụ: {1}\nKhung ca: {2}\nThời gian đăng nhập: {3:HH:mm} Hôm nay\n\nTài khoản: {4}\nQuyền hạn: {5}\n\nTrạng thái phiên: Hoạt động bình thường",
                maNv, chucVu, khungCa, DateTime.Now, Session.AppSession.Current.TenDangNhap, Session.AppSession.Current.MaVaiTro);
        }

        private void SaveProfile()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtHoTen.Text))
                {
                    SFToast.Show(this, "Họ và tên không được để trống.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Amber);
                    return;
                }

                if (currentNvId > 0)
                {
                    DatabaseManager.ExecuteNonQuery(
                        "UPDATE dbo.NhanVien SET HoTen = @HoTen, SoDienThoai = @SDT, Email = @Email WHERE NhanVienID = @ID",
                        new SqlParameter("@HoTen", txtHoTen.Text.Trim()),
                        new SqlParameter("@SDT", txtSdt.Text.Trim()),
                        new SqlParameter("@Email", txtEmail.Text.Trim()),
                        new SqlParameter("@ID", currentNvId));

                    Session.AppSession.Current.HoTen = txtHoTen.Text.Trim();
                    lblName.Text = txtHoTen.Text.Trim().ToUpper();
                    SFToast.Show(this, "Đã cập nhật thông tin cá nhân thành công.", "HỒ SƠ", SFIcon.Check, Colors.Emerald);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "STAFF_ACCOUNT", "SaveProfile");
                SFToast.Show(this, "Lỗi cập nhật: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private void AddField(Control p, string label, string value, int y, bool ro, out SFTextBox box)
        {
            Label l = new Label { Text = label, Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l);
            box = new SFTextBox { Text = value, Location = new Point(4, y + 20), Width = 380, Height = 36, ReadOnly = ro };
            p.Controls.Add(box);
        }
    }
}
