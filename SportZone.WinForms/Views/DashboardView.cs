using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Dialogs;

namespace SportZone.WinForms.Views
{
    public class DashboardView : UserControl
    {
        private Label sub;
        private SFBadge activeBadge;
        private SFMetricCard kpi1;
        private SFMetricCard kpi2;
        private SFMetricCard kpi3;
        private SFMetricCard kpi4;
        private SFChart revChart;
        private SFDonut donutChart;
        private SFDataGridView gridToday;
        private SFEmptyState schedEmpty;

        public DashboardView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.AppBackground;
            AutoScroll = true;
            BuildUI();
            LoadDataFromDatabase(DateTime.Today, DateTime.Today);
        }

        private void BuildUI()
        {
            Panel root = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Colors.AppBackground,
                Padding = new Padding(0, 0, 0, 24)
            };
            Controls.Add(root);

            // ROW 1: Header / Context & Controls
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = Colors.AppBackground,
                Padding = new Padding(0, 0, 0, 14)
            };

            Label title = new Label
            {
                Text = "Tổng quan vận hành",
                Font = Typography.PageTitle,
                ForeColor = Colors.TextPrimary,
                Location = new Point(0, 2),
                AutoSize = true
            };
            header.Controls.Add(title);

            sub = new Label
            {
                Text = "Đang tải dữ liệu vận hành từ CSDL...",
                Font = Typography.Secondary,
                ForeColor = Colors.TextSecondary,
                Location = new Point(1, 32),
                Size = new Size(400, 22),
                AutoEllipsis = true
            };
            header.Controls.Add(sub);

            // Right header controls: Live system pill + Date Chips + Refresh
            FlowLayoutPanel rightControls = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 560,
                Height = 44,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Colors.AppBackground
            };
            header.Controls.Add(rightControls);

            header.Resize += (s, e) =>
            {
                sub.Width = Math.Max(150, header.Width - rightControls.Width - 16);
            };

            SFIconButton btnRefresh = new SFIconButton
            {
                Icon = SFIcon.Refresh,
                Size = new Size(36, 36),
                HoverBg = Colors.Surface,
                NormalColor = Colors.TextSecondary,
                Margin = new Padding(4, 2, 0, 2)
            };
            btnRefresh.Click += (s, e) =>
            {
                LoadDataFromDatabase(DateTime.Today, DateTime.Today);
                SFToast.Show(this, "Dữ liệu vận hành CSDL đã được đồng bộ mới nhất.", "ĐỒNG BỘ", SFIcon.Check, Colors.Emerald);
            };
            rightControls.Controls.Add(btnRefresh);

            SFFilterChip chipMonth = new SFFilterChip { Text = "Tháng này", Size = new Size(88, 34), Margin = new Padding(4, 2, 4, 2) };
            SFFilterChip chipWeek = new SFFilterChip { Text = "7 ngày qua", Size = new Size(92, 34), Margin = new Padding(4, 2, 4, 2) };
            SFFilterChip chipToday = new SFFilterChip { Text = "Hôm nay", Selected = true, Size = new Size(84, 34), Margin = new Padding(4, 2, 4, 2) };

            chipToday.Click += (s, e) =>
            {
                chipToday.Selected = true; chipWeek.Selected = false; chipMonth.Selected = false;
                LoadDataFromDatabase(DateTime.Today, DateTime.Today);
            };
            chipWeek.Click += (s, e) =>
            {
                chipToday.Selected = false; chipWeek.Selected = true; chipMonth.Selected = false;
                LoadDataFromDatabase(DateTime.Today.AddDays(-7), DateTime.Today);
            };
            chipMonth.Click += (s, e) =>
            {
                chipToday.Selected = false; chipWeek.Selected = false; chipMonth.Selected = true;
                DateTime firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                LoadDataFromDatabase(firstDayOfMonth, DateTime.Today);
            };

            rightControls.Controls.Add(chipMonth);
            rightControls.Controls.Add(chipWeek);
            rightControls.Controls.Add(chipToday);

            activeBadge = SFBadge.Create("● --/-- SÂN SẴN SÀNG", Colors.Primary, Colors.SelectedBg);
            activeBadge.Margin = new Padding(8, 6, 8, 2);
            rightControls.Controls.Add(activeBadge);

            root.Controls.Add(header);

            // ROW 2: 4 Metric Cards
            TableLayoutPanel kpiRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 120,
                ColumnCount = 4,
                Padding = new Padding(0, 0, 0, 16),
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 4; i++) kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            // Giá trị hiển thị ban đầu là placeholder trung tính; toàn bộ số liệu
            // được nạp thật từ CSDL trong LoadDataFromDatabase (không bịa số liệu).
            kpi1 = new SFMetricCard
            {
                Title = "DOANH THU KỲ CHỌN",
                Value = "—",
                TrendText = "",
                IsPositiveTrend = true,
                AccentColor = Colors.Primary,
                Icon = SFIcon.Dollar
            };
            kpiRow.Controls.Add(kpi1, 0, 0);

            kpi2 = new SFMetricCard
            {
                Title = "LƯỢT ĐẶT SÂN",
                Value = "—",
                TrendText = "",
                IsPositiveTrend = true,
                AccentColor = Colors.Accent,
                Icon = SFIcon.Ticket
            };
            kpiRow.Controls.Add(kpi2, 1, 0);

            kpi3 = new SFMetricCard
            {
                Title = "SÂN ĐANG HOẠT ĐỘNG",
                Value = "—",
                TrendText = "",
                IsPositiveTrend = true,
                AccentColor = Colors.ElectricBlue,
                Icon = SFIcon.Field
            };
            kpiRow.Controls.Add(kpi3, 2, 0);

            kpi4 = new SFMetricCard
            {
                Title = "TỔNG GIÁ TRỊ ĐƠN",
                Value = "—",
                TrendText = "",
                IsPositiveTrend = true,
                AccentColor = Colors.Warning,
                Icon = SFIcon.TrendingUp
            };
            kpiRow.Controls.Add(kpi4, 3, 0);

            root.Controls.Add(kpiRow);
            kpiRow.BringToFront();

            // ROW 3: Revenue Chart 65% + Occupancy Donut 35%
            TableLayoutPanel chartRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 330,
                ColumnCount = 2,
                Padding = new Padding(0, 0, 0, 16),
                BackColor = Color.Transparent
            };
            chartRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
            chartRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));

            // Revenue Chart Card
            SFCard revCard = new SFCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(18) };
            Panel revHead = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.Transparent };
            Label revTitle = new Label { Text = "Doanh thu theo khung thời gian vận hành", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Left, AutoSize = true };
            Label revBadge = new Label { Text = "● Dữ liệu tự động cập nhật", Font = Typography.MicroBold, ForeColor = Colors.DeepEmerald, BackColor = Colors.SuccessBg, Dock = DockStyle.Right, AutoSize = true, Padding = new Padding(6, 4, 6, 4) };
            revHead.Controls.Add(revBadge);
            revHead.Controls.Add(revTitle);
            revCard.Controls.Add(revHead);

            revChart = new SFChart
            {
                Dock = DockStyle.Fill,
                AccentColor = Colors.Primary,
                XLabels = new string[0],
                Values = new double[0],
                MaxValue = 1.0
            };
            revCard.Controls.Add(revChart);
            revChart.BringToFront();
            chartRow.Controls.Add(revCard, 0, 0);

            // Donut Chart Card
            SFCard donutCard = new SFCard { Dock = DockStyle.Fill, Margin = new Padding(4, 0, 0, 0), Padding = new Padding(18) };
            Panel donutHead = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.Transparent };
            Label donutTitle = new Label { Text = "Tỷ trọng doanh thu theo bộ môn", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Location = new Point(0, 2), AutoSize = true };
            donutHead.Controls.Add(donutTitle);
            donutCard.Controls.Add(donutHead);

            donutChart = new SFDonut
            {
                Dock = DockStyle.Fill,
                CenterMetric = "—",
                CenterLabel = "Doanh thu",
                Categories = new string[0],
                Percentages = new float[0],
                SliceColors = new[] { Colors.Primary, Colors.Accent, Colors.Warning, Colors.Purple }
            };
            donutCard.Controls.Add(donutChart);
            donutChart.BringToFront();
            chartRow.Controls.Add(donutCard, 1, 0);

            root.Controls.Add(chartRow);
            chartRow.BringToFront();

            // ROW 4: Today Schedule (60%) + Action Center (40%)
            TableLayoutPanel bottomRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 310,
                ColumnCount = 2,
                BackColor = Color.Transparent
            };
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

            // Today Schedule Card
            SFCard schedCard = new SFCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(18) };
            Panel schedHead = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.Transparent };
            Label schedTitle = new Label { Text = "Lịch thi đấu hôm nay sắp diễn ra", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Location = new Point(0, 2), AutoSize = true };
            SFButton btnAllSched = new SFButton { Text = "Xem toàn bộ lịch", Variant = SFButtonVariant.Ghost, Size = new Size(130, 28), Dock = DockStyle.Right };
            btnAllSched.Click += (s, e) => (FindForm() as MainForm)?.ShowView("Lịch sân");
            schedHead.Controls.Add(schedTitle);
            schedHead.Controls.Add(btnAllSched);
            schedCard.Controls.Add(schedHead);

            gridToday = new SFDataGridView();
            gridToday.Columns.Add("C0", "Khung giờ");
            gridToday.Columns.Add("C1", "Sân đấu");
            gridToday.Columns.Add("C2", "Khách hàng");
            gridToday.Columns.Add("C3", "Số tiền");
            gridToday.Columns.Add("C4", "Trạng thái");

            gridToday.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < gridToday.Rows.Count)
                {
                    string maDon = gridToday.Rows[e.RowIndex].Tag as string;
                    if (!string.IsNullOrEmpty(maDon))
                    {
                        new BookingDetailForm(maDon).ShowDialog(this);
                    }
                }
            };

            // V9: cột tiền căn phải
            gridToday.Columns["C3"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridToday.Columns["C3"].DefaultCellStyle.Format = "N0";

            // Trạng thái trống: hiển thị khi CSDL không trả về lịch hôm nay
            schedEmpty = new SFEmptyState
            {
                Icon = SFIcon.Clock,
                Title = "Chưa có lịch thi đấu hôm nay",
                Description = "Các đơn đặt sân hôm nay sẽ hiển thị tại đây.",
                ActionText = "",
                BackColor = Colors.Surface
            };
            schedEmpty.Visible = false;
            schedCard.Controls.Add(schedEmpty);
            schedEmpty.BringToFront();
            schedCard.Controls.Add(gridToday);
            gridToday.BringToFront();
            bottomRow.Controls.Add(schedCard, 0, 0);

            // Action Center Card (Rule 18)
            SFCard actionCard = new SFCard { Dock = DockStyle.Fill, Margin = new Padding(4, 0, 0, 0), Padding = new Padding(18) };
            Panel actionHead = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.Transparent };
            Label actionTitle = new Label { Text = "Action Center • Cần xử lý ngay", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Location = new Point(0, 2), AutoSize = true };
            actionHead.Controls.Add(actionTitle);
            actionCard.Controls.Add(actionHead);

            Panel actionList = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
            AddActionItem(actionList, SFIcon.Ticket, "Đơn chờ duyệt cọc online", "Đơn đặt trực tuyến chờ kiểm tra đối soát", "!", Colors.Warning, () => (FindForm() as MainForm)?.ShowView("Đặt cọc online"));
            AddActionItem(actionList, SFIcon.Dollar, "Giao dịch quầy & POS", "Theo dõi giao dịch thu ngân và quyết toán", "POS", Colors.Accent, () => (FindForm() as MainForm)?.ShowView("Hóa đơn"));
            AddActionItem(actionList, SFIcon.Clock, "Lịch thi đấu sân trực tiếp", "Kiểm tra danh sách đơn đặt sân hôm nay", "LIVE", Colors.ElectricBlue, () => (FindForm() as MainForm)?.ShowView("Quản lý đơn"));
            AddActionItem(actionList, SFIcon.AlertCircle, "Cảnh báo tồn kho & Dịch vụ", "Kiểm tra mức tồn kho sản phẩm và nước giải khát", "KHO", Colors.Danger, () => (FindForm() as MainForm)?.ShowView("Dịch vụ & Kho"));

            actionCard.Controls.Add(actionList);
            actionList.BringToFront();
            bottomRow.Controls.Add(actionCard, 1, 0);

            root.Controls.Add(bottomRow);
            bottomRow.BringToFront();
        }

        private void LoadDataFromDatabase(DateTime tuNgay, DateTime denNgay)
        {
            try
            {
                // Reset trung thực: mỗi lần load đều bắt đầu từ trạng thái trống/—
                kpi1.Value = "—"; kpi2.Value = "—"; kpi3.Value = "—"; kpi4.Value = "—";
                kpi1.SparklineData = null;
                revChart.XLabels = new string[0];
                revChart.Values = new double[0];
                donutChart.Categories = new string[0];
                donutChart.Percentages = new float[0];
                revChart.Invalidate();
                donutChart.Invalidate();

                // 1. Overview report using ReportRepository / ReportService
                var reportService = new Services.ReportService();
                DataSet ds = reportService.GetGeneralReport(tuNgay, denNgay);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow r = ds.Tables[0].Rows[0];
                    decimal daThu = r.Table.Columns.Contains("DoanhThuDaThu") && r["DoanhThuDaThu"] != DBNull.Value ? Convert.ToDecimal(r["DoanhThuDaThu"]) : 0;
                    int tongDon = r.Table.Columns.Contains("TongDon") && r["TongDon"] != DBNull.Value ? Convert.ToInt32(r["TongDon"]) : 0;
                    decimal giaTriDon = r.Table.Columns.Contains("GiaTriDon") && r["GiaTriDon"] != DBNull.Value ? Convert.ToDecimal(r["GiaTriDon"]) : 0;

                    kpi1.Value = string.Format("{0:N0} đ", daThu);
                    kpi2.Value = string.Format("{0} đơn", tongDon);
                    kpi4.Value = string.Format("{0:N0} đ", giaTriDon);
                    sub.Text = string.Format("Dữ liệu tổng hợp CSDL từ {0:dd/MM/yyyy} đến {1:dd/MM/yyyy}.", tuNgay, denNgay);

                    // Daily trend in table 1 if present — thực sự từ CSDL
                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        DataTable dtDaily = ds.Tables[1];
                        List<string> lbls = new List<string>();
                        List<double> vals = new List<double>();
                        List<double> spark = new List<double>();
                        double maxV = 1.0;
                        double sparkMax = 1.0;
                        foreach (DataRow row in dtDaily.Rows)
                        {
                            DateTime d = Convert.ToDateTime(row["Ngay"]);
                            double revM = Convert.ToDouble(row["DoanhThu"]) / 1000000.0;
                            double sparkVal = row.Table.Columns.Contains("DoanhThu") && row["DoanhThu"] != DBNull.Value
                                ? Convert.ToDouble(row["DoanhThu"])
                                : 0;
                            lbls.Add(d.ToString("dd/MM"));
                            vals.Add(Math.Round(revM, 1));
                            spark.Add(sparkVal);
                            if (revM > maxV) maxV = revM;
                            if (sparkVal > sparkMax) sparkMax = sparkVal;
                        }
                        revChart.XLabels = lbls.ToArray();
                        revChart.Values = vals.ToArray();
                        revChart.MaxValue = maxV * 1.25;
                        revChart.Invalidate();
                        // Sparkline KPI doanh thu: chỉ gán khi có ≥2 điểm dữ liệu thật
                        if (spark.Count > 1)
                        {
                            kpi1.SparklineData = spark.ToArray();
                            kpi1.Invalidate();
                        }
                    }

                    // Court categories in table 2 if present
                    if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                    {
                        DataTable dtTypes = ds.Tables[2];
                        List<string> cats = new List<string>();
                        List<float> pcts = new List<float>();
                        decimal totalSum = 0;
                        foreach (DataRow row in dtTypes.Rows)
                        {
                            totalSum += row["DoanhThuSan"] != DBNull.Value ? Convert.ToDecimal(row["DoanhThuSan"]) : 0;
                        }

                        foreach (DataRow row in dtTypes.Rows)
                        {
                            string tName = row["TenLoaiSan"]?.ToString() ?? "";
                            decimal rev = row["DoanhThuSan"] != DBNull.Value ? Convert.ToDecimal(row["DoanhThuSan"]) : 0;
                            float pct = totalSum > 0 ? (float)(rev / totalSum * 100m) : 0f;
                            cats.Add(tName);
                            pcts.Add(pct);
                        }

                        if (cats.Count > 0)
                        {
                            donutChart.Categories = cats.ToArray();
                            donutChart.Percentages = pcts.ToArray();
                            donutChart.Invalidate();
                        }
                    }
                }

                // 2. Load Court Status (TrangThaiVanHanh contract)
                DataTable dtSan = new Services.FieldService().GetFields();
                if (dtSan != null && dtSan.Rows.Count > 0)
                {
                    int totalSan = dtSan.Rows.Count;
                    int activeSan = 0;
                    foreach (DataRow s in dtSan.Rows)
                    {
                        string st = s.Table.Columns.Contains("TrangThaiVanHanh") 
                            ? s["TrangThaiVanHanh"]?.ToString() 
                            : (s.Table.Columns.Contains("TrangThai") ? s["TrangThai"]?.ToString() : "");
                        if (st == "SAN_SANG" || st == "DANG_SU_DUNG") activeSan++;
                    }

                    kpi3.Value = string.Format("{0} / {1}", activeSan, totalSan);
                    activeBadge.Text = string.Format("● {0}/{1} SÂN HOẠT ĐỘNG", activeSan, totalSan);
                }

                // 3. Load Bookings for Today
                DataTable dtDon = new Services.ShiftService().GetStaffTodaySchedule();
                gridToday.Rows.Clear();
                bool hasSchedule = dtDon != null && dtDon.Rows.Count > 0;
                schedEmpty.Visible = !hasSchedule;
                gridToday.Visible = hasSchedule;
                if (hasSchedule)
                {
                    foreach (DataRow row in dtDon.Rows)
                    {
                        DateTime bd = Convert.ToDateTime(row["BatDau"]);
                        DateTime kt = Convert.ToDateTime(row["KetThuc"]);
                        string slot = string.Format("{0:HH:mm} - {1:HH:mm}", bd, kt);
                        string san = row["TenSan"]?.ToString() ?? (row["MaSan"]?.ToString() ?? "");
                        string kh = row["HoTen"]?.ToString() ?? "";
                        decimal tong = row["TongTien"] != DBNull.Value ? Convert.ToDecimal(row["TongTien"]) : 0;
                        string st = row["TrangThaiDon"]?.ToString() ?? "";

                        string stDisp = "Đã xác nhận";
                        if (st == "DANG_SU_DUNG") stDisp = "Đang thi đấu";
                        else if (st == "HOAN_THANH") stDisp = "Hoàn tất";
                        else if (st == "CHO_XAC_NHAN") stDisp = "Chờ cọc";

                        int rIdx = gridToday.Rows.Add(slot, san, kh, string.Format("{0:N0}đ", tong), stDisp);
                        gridToday.Rows[rIdx].Tag = row["MaDon"]?.ToString();
                    }
                    gridToday.ApplySmartColumnSizing();
                }
            }
            catch (Exception ex)
            {
                // Trạng thái lỗi: thông báo trung thực, không hiển thị số liệu ảo
                sub.Text = "Không tải được dữ liệu vận hành từ CSDL. Nhấn làm mới để thử lại.";
                kpi1.Value = "—"; kpi2.Value = "—"; kpi3.Value = "—"; kpi4.Value = "—";
                activeBadge.Text = "● CSDL LỖI";
                schedEmpty.Title = "Lỗi tải dữ liệu";
                schedEmpty.Description = "Kiểm tra kết nối SQL Server rồi nhấn làm mới.";
                schedEmpty.Visible = true;
                gridToday.Visible = false;
                Helpers.AppLogger.LogError(ex, "DASHBOARD_LOAD");
            }
        }

        private void AddActionItem(Panel parent, SFIcon icon, string title, string desc, string badgeCount, Color accent, Action onClick)
        {
            Panel item = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Colors.SurfaceElevated,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(12, 10, 12, 10),
                Cursor = Cursors.Hand
            };
            item.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, item.Width - 1, item.Height - 1);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 8))
                {
                    using (SolidBrush b = new SolidBrush(Colors.SurfaceElevated)) e.Graphics.FillPath(b, p);
                    using (Pen pen = new Pen(Colors.Border, 1.1f)) e.Graphics.DrawPath(pen, p);
                }
            };

            Panel iconBox = new Panel { Location = new Point(12, 14), Size = new Size(32, 32), BackColor = Color.FromArgb(24, accent) };
            iconBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, 31, 31);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 6))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(30, accent))) e.Graphics.FillPath(b, p);
                Rectangle ir = new Rectangle(7, 7, 18, 18);
                VectorIcons.Draw(e.Graphics, icon, ir, accent, 1.8f);
            };
            item.Controls.Add(iconBox);

            Label lblTitle = new Label
            {
                Text = title,
                Font = Typography.SecondaryBold,
                ForeColor = Colors.TextPrimary,
                Location = new Point(54, 10),
                AutoSize = true
            };
            item.Controls.Add(lblTitle);

            Label lblDesc = new Label
            {
                Text = desc,
                Font = Typography.Micro,
                ForeColor = Colors.TextSecondary,
                Location = new Point(54, 30),
                AutoSize = true
            };
            item.Controls.Add(lblDesc);

            SFBadge countBadge = SFBadge.Create(badgeCount, Color.White, accent);
            countBadge.Location = new Point(item.Width - 52, 18);
            countBadge.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            item.Controls.Add(countBadge);

            item.Click += (s, e) => onClick();
            lblTitle.Click += (s, e) => onClick();
            lblDesc.Click += (s, e) => onClick();
            iconBox.Click += (s, e) => onClick();

            parent.Controls.Add(item);
            item.BringToFront();
        }
    }
}
