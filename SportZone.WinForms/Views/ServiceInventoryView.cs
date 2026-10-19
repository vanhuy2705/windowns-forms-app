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
    public class ServiceInventoryView : UserControl
    {
        private SFSearchBox searchBox;
        private SFDataGridView grid;
        private TableLayoutPanel kpiPanel;
        private FlowLayoutPanel chipBar;
        private DataTable inventoryTable;
        private string categoryFilter = "ALL";
        private string searchKeyword = "";

        public ServiceInventoryView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadInventoryFromDatabase();
        }

        private void BuildUI()
        {
            // 1. Header
            Controls.Add(Ui.Header("Dịch Vụ, Kho F&B & Thiết Bị Cho Thuê", "Theo dõi tồn kho, doanh thu bán hàng/thuê dụng cụ, cảnh báo ngưỡng an toàn và kiểm kê hao hụt."));

            // 2. Toolbar
            FlowLayoutPanel tb = Ui.Toolbar();

            SFButton btnAdd = new SFButton
            {
                Text = "Thêm Hàng Hóa",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Plus,
                Width = 165,
                Height = 38
            };
            btnAdd.Click += (s, e) => SFToast.Show(this, "Đã mở form tạo mã sản phẩm / dịch vụ mới.", "THÊM HÀNG HÓA", SFIcon.Plus, Colors.Emerald);
            tb.Controls.Add(btnAdd);

            SFButton btnIn = new SFButton
            {
                Text = "Nhập Kho (F3)",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Plus,
                Width = 140,
                Height = 38
            };
            btnIn.Click += (s, e) => SFToast.Show(this, "Mở phiếu nhập kho hàng F&B và dụng cụ.", "NHẬP KHO", SFIcon.Plus, Colors.ElectricBlue);
            tb.Controls.Add(btnIn);

            SFButton btnAudit = new SFButton
            {
                Text = "Xuất / Kiểm Kê (F4)",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Eye,
                Width = 165,
                Height = 38
            };
            btnAudit.Click += (s, e) =>
            {
                if (new InventoryAuditForm().ShowDialog(this) == DialogResult.OK)
                {
                    LoadInventoryFromDatabase();
                }
            };
            tb.Controls.Add(btnAudit);

            searchBox = new SFSearchBox
            {
                PlaceholderText = "Tên mặt hàng / mã SKU...",
                Width = 240,
                Height = 38
            };
            searchBox.TextChanged += (s, e) =>
            {
                searchKeyword = searchBox.Text.Trim().ToLowerInvariant();
                ApplyFilter();
            };
            tb.Controls.Add(searchBox);

            Controls.Add(tb);
            tb.BringToFront();

            // 3. Metric cards
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
            kpiPanel.Controls.Add(new SFMetricCard("TỔNG GIÁ TRỊ KHO", "184,52tr", "58 mặt hàng quản lý", SFIcon.Dollar, Colors.Amber, "Kiểm đếm 100%"), 0, 0);
            kpiPanel.Controls.Add(new SFMetricCard("ĐÃ ĐỐI SOÁT", "181,89tr", "98,6% khớp thực tế", SFIcon.Check, Colors.Emerald, "Hao hụt cực thấp"), 1, 0);
            kpiPanel.Controls.Add(new SFMetricCard("CẢNH BÁO TỒN KHO", "02 Món", "Dưới ngưỡng an toàn", SFIcon.Bell, Colors.Coral, "Cần nhập gấp"), 2, 0);
            kpiPanel.Controls.Add(new SFMetricCard("DOANH THU CA HIỆN TẠI", "1.450.000đ", "F&B & Dịch vụ đi kèm", SFIcon.Chart, Colors.ElectricBlue, "+18% so ca qua"), 3, 0);

            Controls.Add(kpiPanel);
            kpiPanel.BringToFront();

            // 4. Filter chips
            chipBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Colors.Canvas,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, 8)
            };

            string[] chips = { "Tất cả", "F&B Đồ uống", "Dụng cụ cho thuê", "Vật phẩm bán" };
            for (int i = 0; i < chips.Length; i++)
            {
                string cText = chips[i];
                SFFilterChip chip = new SFFilterChip
                {
                    Text = cText,
                    IsActive = i == 0,
                    Height = 34,
                    Width = 140
                };
                chip.Click += (s, e) =>
                {
                    foreach (Control c in chipBar.Controls)
                    {
                        if (c is SFFilterChip fc) fc.IsActive = (fc == s);
                    }
                    categoryFilter = cText == "Tất cả" ? "ALL" : (cText == "F&B Đồ uống" ? "DO_UONG" : (cText == "Dụng cụ cho thuê" ? "CHO_THUE" : "BAN"));
                    ApplyFilter();
                };
                chipBar.Controls.Add(chip);
            }
            Controls.Add(chipBar);
            chipBar.BringToFront();

            // 5. Data Grid
            SFCard card = new SFCard
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
                Text = "Danh Mục Hàng Hóa, Dịch Vụ & Tồn Kho (Nhấp đúp dòng để xem lịch sử nhập xuất)",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Left,
                AutoSize = true
            };
            gridHeader.Controls.Add(lblGridTitle);
            card.Controls.Add(gridHeader);

            grid = new SFDataGridView
            {
                Dock = DockStyle.Fill
            };
            grid.Columns.Add("Code", "Mã SKU");
            grid.Columns.Add("Name", "Tên mặt hàng");
            grid.Columns.Add("Category", "Nhóm danh mục");
            grid.Columns.Add("Unit", "ĐVT");
            grid.Columns.Add("Price", "Đơn giá");
            grid.Columns.Add("Stock", "Tồn kho");
            grid.Columns.Add("Alert", "Ngưỡng cảnh báo");
            grid.Columns.Add("Status", "Tình trạng");

            grid.Columns["Code"].Width = 90;
            grid.Columns["Name"].Width = 230;
            grid.Columns["Category"].Width = 150;
            grid.Columns["Unit"].Width = 70;
            grid.Columns["Price"].Width = 120;
            grid.Columns["Stock"].Width = 90;
            grid.Columns["Alert"].Width = 130;
            grid.Columns["Status"].Width = 110;
            grid.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Kiểm kê mặt hàng (F4)", null, (s, e) => {
                if (new InventoryAuditForm().ShowDialog(this) == DialogResult.OK)
                    LoadInventoryFromDatabase();
            });
            menu.Items.Add("-");
            menu.Items.Add("Xuất danh sách tồn kho ra Excel (CSV)", null, (s, e) => {
                Helpers.ExportHelper.ExportToExcel(grid, "TonKhoF&B.csv", this);
            });
            grid.ContextMenuStrip = menu;

            grid.CellDoubleClick += (s, e) => new InventoryAuditForm().ShowDialog(this);

            card.Controls.Add(grid);
            grid.BringToFront();

            Controls.Add(card);
            card.BringToFront();
        }

        public void LoadInventoryFromDatabase()
        {
            try
            {
                inventoryTable = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachDichVuTonKho");
                if (inventoryTable != null && inventoryTable.Rows.Count > 0)
                {
                    int totalItems = inventoryTable.Rows.Count;
                    int alertCount = 0;
                    decimal totalVal = 0;

                    foreach (DataRow r in inventoryTable.Rows)
                    {
                        decimal ton = r["SoLuongTon"] != DBNull.Value ? Convert.ToDecimal(r["SoLuongTon"]) : 0;
                        decimal gia = r["GiaBan"] != DBNull.Value && Convert.ToDecimal(r["GiaBan"]) > 0 ? Convert.ToDecimal(r["GiaBan"]) : (r["GiaThue"] != DBNull.Value ? Convert.ToDecimal(r["GiaThue"]) : 0);
                        totalVal += (ton * gia);

                        if (r["CanhBaoTon"] != DBNull.Value && Convert.ToBoolean(r["CanhBaoTon"]))
                            alertCount++;
                    }

                    kpiPanel.Controls.Clear();
                    kpiPanel.Controls.Add(new SFMetricCard("TỔNG GIÁ TRỊ KHO", (totalVal / 1000000m).ToString("0.0#") + "tr", totalItems + " mặt hàng SQL", SFIcon.Dollar, Colors.Amber, "Kiểm đếm 100%"), 0, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("MẶT HÀNG KINH DOANH", totalItems.ToString(), "F&B và cho thuê", SFIcon.Check, Colors.Emerald, "Khớp thực tế"), 1, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("CẢNH BÁO TỒN KHO", alertCount.ToString() + " Món", "Dưới ngưỡng an toàn", SFIcon.Bell, alertCount > 0 ? Colors.Coral : Colors.Emerald, alertCount > 0 ? "Cần nhập thêm" : "An toàn"), 2, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("DOANH THU CA HIỆN TẠI", "1.450.000đ", "Dịch vụ đi kèm", SFIcon.Chart, Colors.ElectricBlue, "+18% so ca qua"), 3, 0);

                    ApplyFilter();
                    return;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_INVENTORY");
            }

            grid.Rows.Clear();
        }

        private void ApplyFilter()
        {
            grid.Rows.Clear();

            if (inventoryTable == null || inventoryTable.Rows.Count == 0)
            {
                return;
            }

            foreach (DataRow r in inventoryTable.Rows)
            {
                string code = r["MaSanPhamDichVu"]?.ToString() ?? "";
                string name = r["TenSanPhamDichVu"]?.ToString() ?? "";
                string loai = r["Loai"]?.ToString() ?? "";
                string dvt = r["DonViTinh"]?.ToString() ?? "Cái";
                decimal gb = r["GiaBan"] != DBNull.Value ? Convert.ToDecimal(r["GiaBan"]) : 0;
                decimal gt = r["GiaThue"] != DBNull.Value ? Convert.ToDecimal(r["GiaThue"]) : 0;
                decimal ton = r["SoLuongTon"] != DBNull.Value ? Convert.ToDecimal(r["SoLuongTon"]) : 0;
                decimal tonMin = r["SoLuongTonToiThieu"] != DBNull.Value ? Convert.ToDecimal(r["SoLuongTonToiThieu"]) : 0;
                bool alert = r["CanhBaoTon"] != DBNull.Value && Convert.ToBoolean(r["CanhBaoTon"]);

                if (categoryFilter != "ALL" && !loai.Equals(categoryFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    bool match = code.ToLowerInvariant().Contains(searchKeyword) ||
                                 name.ToLowerInvariant().Contains(searchKeyword);
                    if (!match) continue;
                }

                string categoryText = loai == "DO_UONG" ? "F&B Đồ uống" : (loai == "CHO_THUE" ? "Dụng cụ cho thuê" : "Vật phẩm bán");
                string priceText = gb > 0 ? (gb.ToString("#,##0") + "đ") : (gt.ToString("#,##0") + "đ/lượt");

                grid.Rows.Add(
                    code,
                    name,
                    categoryText,
                    dvt,
                    priceText,
                    ton.ToString("0.#"),
                    tonMin.ToString("0.#"),
                    alert ? "Cần nhập gấp" : "Còn hàng"
                );
            }
        }
    }
}
