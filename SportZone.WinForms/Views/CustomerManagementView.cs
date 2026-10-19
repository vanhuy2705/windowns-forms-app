using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Dialogs;

namespace SportZone.WinForms.Views
{
    public class CustomerManagementView : UserControl
    {
        private SFSearchBox searchBox;
        private SFDataGridView grid;
        private TableLayoutPanel kpiPanel;
        private FlowLayoutPanel chipBar;
        private DataTable customersTable;
        private string tierFilter = "ALL";
        private string searchKeyword = "";

        public CustomerManagementView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadCustomersFromDatabase();
        }

        private void BuildUI()
        {
            // 1. Header
            Controls.Add(Ui.Header("Quản Lý Khách Hàng & Đội Bóng", "Hồ sơ hội viên, phân hạng VIP, lịch sử đặt sân, số dư ví cọc và hạn mức ưu đãi."));

            // 2. Toolbar
            FlowLayoutPanel tb = Ui.Toolbar();

            SFButton btnAdd = new SFButton
            {
                Text = "Thêm Khách Hàng",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Plus,
                Width = 175,
                Height = 38
            };
            btnAdd.Click += (s, e) =>
            {
                if (new CustomerRegisterForm().ShowDialog(this) == DialogResult.OK)
                {
                    LoadCustomersFromDatabase();
                }
            };
            tb.Controls.Add(btnAdd);

            SFButton btnVip = new SFButton
            {
                Text = "Gán Thẻ VIP",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Star,
                Width = 130,
                Height = 38
            };
            btnVip.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0)
                {
                    SFToast.Show(this, "Vui lòng chọn khách hàng cần thay đổi hạng thành viên.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Amber);
                    return;
                }
                DataRow row = grid.SelectedRows[0].Tag as DataRow;
                if (row != null)
                {
                    int khId = Convert.ToInt32(row["KhachHangID"]);
                    string name = row["HoTen"]?.ToString() ?? "";
                    string tier = row.Table.Columns.Contains("TenHang") && row["TenHang"] != DBNull.Value ? row["TenHang"].ToString() : "Member";
                    var dlg = new AssignVipCardForm(khId, name, tier);
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadCustomersFromDatabase();
                    }
                }
            };
            tb.Controls.Add(btnVip);

            SFButton btnSms = new SFButton
            {
                Text = "Gửi Zalo / SMS",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Bell,
                Width = 145,
                Height = 38
            };
            btnSms.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count == 0)
                {
                    SFToast.Show(this, "Vui lòng chọn khách hàng cần gửi tin nhắn.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Amber);
                    return;
                }
                DataRow row = grid.SelectedRows[0].Tag as DataRow;
                if (row != null)
                {
                    int khId = Convert.ToInt32(row["KhachHangID"]);
                    string name = row["HoTen"]?.ToString() ?? "";
                    string phone = row["SoDienThoai"]?.ToString() ?? "";
                    var dlg = new SendMessagingForm(khId, name, phone);
                    dlg.ShowDialog(this);
                }
            };
            tb.Controls.Add(btnSms);

            searchBox = new SFSearchBox
            {
                PlaceholderText = "Tên / SĐT / mã KH / email...",
                Width = 260,
                Height = 38
            };

            Timer debounceTimer = new Timer { Interval = 300 };
            debounceTimer.Tick += (s, e) =>
            {
                debounceTimer.Stop();
                if (!IsDisposed)
                {
                    searchKeyword = searchBox.Text.Trim().ToLowerInvariant();
                    ApplyFilter();
                }
            };

            searchBox.TextChanged += (s, e) =>
            {
                debounceTimer.Stop();
                debounceTimer.Start();
            };
            tb.Controls.Add(searchBox);

            Controls.Add(tb);
            tb.BringToFront();

            // 3. KPI Metric Cards
            kpiPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                ColumnCount = 4,
                Padding = new Padding(0, 0, 0, 14)
            };
            for (int i = 0; i < 4; i++)
            {
                kpiPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            }
            kpiPanel.Controls.Add(new SFMetricCard("TỔNG KHÁCH HÀNG", "7", "Đã định danh", SFIcon.User, Colors.ElectricBlue, "+6 tài khoản mới"), 0, 0);
            kpiPanel.Controls.Add(new SFMetricCard("THÀNH VIÊN VIP", "4", "Gold & Silver", SFIcon.Star, Colors.Purple, "Chiếm 57%"), 1, 0);
            kpiPanel.Controls.Add(new SFMetricCard("SỐ DƯ VÍ CỌC", "6,65tr", "Ký quỹ giữ chỗ", SFIcon.Dollar, Colors.Emerald, "An toàn 100%"), 2, 0);
            kpiPanel.Controls.Add(new SFMetricCard("CHI TIÊU BÌNH QUÂN", "13,6tr", "/ khách hoạt động", SFIcon.Chart, Colors.Amber, "+14% YoY"), 3, 0);

            Controls.Add(kpiPanel);
            kpiPanel.BringToFront();

            // 4. Filter Chips Bar
            chipBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Colors.Canvas,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, 8)
            };

            string[] chips = { "Tất cả", "VIP Gold", "VIP Silver", "Thành viên" };
            for (int i = 0; i < chips.Length; i++)
            {
                string t = chips[i];
                SFFilterChip chip = new SFFilterChip
                {
                    Text = t,
                    IsActive = i == 0,
                    Height = 34,
                    Width = 120
                };
                chip.Click += (s, e) =>
                {
                    foreach (Control c in chipBar.Controls)
                    {
                        if (c is SFFilterChip fc) fc.IsActive = (fc == s);
                    }
                    tierFilter = t == "Tất cả" ? "ALL" : (t == "VIP Gold" ? "GOLD" : (t == "VIP Silver" ? "SILVER" : "THUONG"));
                    ApplyFilter();
                };
                chipBar.Controls.Add(chip);
            }
            Controls.Add(chipBar);
            chipBar.BringToFront();

            // 5. Main Card Data Grid
            SFCard cardGrid = new SFCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };

            Panel gridHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36
            };
            Label lblGridTitle = new Label
            {
                Text = "Danh Sách Khách Hàng & Đội Bóng Hoạt Động (Nhấp đúp để xem chi tiết)",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Left,
                AutoSize = true
            };
            gridHeader.Controls.Add(lblGridTitle);
            cardGrid.Controls.Add(gridHeader);

            grid = new SFDataGridView
            {
                Dock = DockStyle.Fill
            };
            grid.Columns.Add("Code", "Mã KH");
            grid.Columns.Add("Name", "Họ tên & Đội bóng");
            grid.Columns.Add("Phone", "Số điện thoại");
            grid.Columns.Add("Tier", "Phân hạng");
            grid.Columns.Add("Orders", "Lượt đặt");
            grid.Columns.Add("Total", "Tổng chi tiêu");
            grid.Columns.Add("Deposit", "Số dư cọc");
            grid.Columns.Add("Status", "Trạng thái");

            grid.Columns["Code"].Width = 95;
            grid.Columns["Name"].Width = 220;
            grid.Columns["Phone"].Width = 130;
            grid.Columns["Tier"].Width = 110;
            grid.Columns["Orders"].Width = 85;
            grid.Columns["Total"].Width = 135;
            grid.Columns["Deposit"].Width = 125;
            grid.Columns["Status"].Width = 105;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Xem chi tiết khách hàng", null, (s, e) => new CustomerDetailForm().ShowDialog(this));
            menu.Items.Add("Đăng ký khách hàng mới", null, (s, e) => {
                if (new CustomerRegisterForm().ShowDialog(this) == DialogResult.OK)
                    LoadCustomersFromDatabase();
            });
            menu.Items.Add("-");
            menu.Items.Add("Xuất danh sách khách hàng ra Excel (CSV)", null, (s, e) => {
                Helpers.ExportHelper.ExportToExcel(grid, "DanhSachKhachHang.csv", this);
            });
            grid.ContextMenuStrip = menu;

            grid.CellDoubleClick += (s, e) => new CustomerDetailForm().ShowDialog(this);

            cardGrid.Controls.Add(grid);
            grid.BringToFront();

            Controls.Add(cardGrid);
            cardGrid.BringToFront();
        }

        public void LoadCustomersFromDatabase()
        {
            try
            {
                customersTable = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachKhachHang");
                if (customersTable != null && customersTable.Rows.Count > 0)
                {
                    // Update KPI cards
                    int count = customersTable.Rows.Count;
                    int vipCount = 0;
                    decimal totalDeposit = 0;
                    decimal totalSpend = 0;

                    foreach (DataRow r in customersTable.Rows)
                    {
                        string hang = r["MaHang"]?.ToString() ?? "";
                        if (hang == "GOLD" || hang == "VIP" || hang == "SILVER") vipCount++;
                        if (r["SoDuViCoc"] != DBNull.Value) totalDeposit += Convert.ToDecimal(r["SoDuViCoc"]);
                        if (r["TongChiTieu"] != DBNull.Value) totalSpend += Convert.ToDecimal(r["TongChiTieu"]);
                    }

                    decimal avgSpend = count > 0 ? (totalSpend / count) : 0;

                    kpiPanel.Controls.Clear();
                    kpiPanel.Controls.Add(new SFMetricCard("TỔNG KHÁCH HÀNG", count.ToString(), "Đã định danh", SFIcon.User, Colors.ElectricBlue, "CSDL SQL Live"), 0, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("THÀNH VIÊN VIP", vipCount.ToString(), "Gold & Silver", SFIcon.Star, Colors.Purple, string.Format("{0:0}% tổng khách", (double)vipCount / count * 100)), 1, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("SỐ DƯ VÍ CỌC", (totalDeposit / 1000000m).ToString("0.0#") + "tr", "Ký quỹ giữ chỗ", SFIcon.Dollar, Colors.Emerald, "Ví bảo chứng"), 2, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("CHI TIÊU BÌNH QUÂN", (avgSpend / 1000000m).ToString("0.0#") + "tr", "/ khách", SFIcon.Chart, Colors.Amber, "Doanh thu chuẩn"), 3, 0);

                    ApplyFilter();
                    return;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_CUSTOMERS");
            }

            grid.Rows.Clear();
        }

        private void ApplyFilter()
        {
            grid.Rows.Clear();

            if (customersTable == null || customersTable.Rows.Count == 0)
            {
                return;
            }

            foreach (DataRow r in customersTable.Rows)
            {
                string code = r["MaKhachHang"]?.ToString() ?? "";
                string name = r["HoTen"]?.ToString() ?? "";
                string phone = r["SoDienThoai"]?.ToString() ?? "";
                string tier = r["TenHang"]?.ToString() ?? "Thành viên";
                string maHang = r["MaHang"]?.ToString() ?? "THUONG";
                int orders = r["TongLuotDat"] != DBNull.Value ? Convert.ToInt32(r["TongLuotDat"]) : 0;
                decimal total = r["TongChiTieu"] != DBNull.Value ? Convert.ToDecimal(r["TongChiTieu"]) : 0;
                decimal deposit = r["SoDuViCoc"] != DBNull.Value ? Convert.ToDecimal(r["SoDuViCoc"]) : 0;
                bool active = r["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(r["DangHoatDong"]);

                // Filter tier
                if (tierFilter != "ALL" && !maHang.Equals(tierFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Filter search
                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    string email = r.Table.Columns.Contains("Email") && r["Email"] != DBNull.Value ? r["Email"].ToString().ToLowerInvariant() : "";
                    bool match = code.ToLowerInvariant().Contains(searchKeyword) ||
                                 name.ToLowerInvariant().Contains(searchKeyword) ||
                                 phone.ToLowerInvariant().Contains(searchKeyword) ||
                                 email.Contains(searchKeyword);
                    if (!match) continue;
                }

                int rowIndex = grid.Rows.Add(
                    code,
                    name,
                    phone,
                    tier,
                    orders.ToString(),
                    total.ToString("#,##0") + "đ",
                    deposit.ToString("#,##0") + "đ",
                    active ? "Hoạt động" : "Tạm khóa"
                );
                grid.Rows[rowIndex].Tag = r;
            }
        }
    }
}
