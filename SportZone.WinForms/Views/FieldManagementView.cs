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
    public class FieldManagementView : UserControl
    {
        private FlowLayoutPanel cardContainer;
        private SFDataGridView tableGrid;
        private Panel contentHolder;
        private DataTable fieldsTable;
        private string statusFilter = "ALL";
        private string searchText = "";
        private SFFilterChip chipAll, chipActive, chipMaint, chipPause;

        public FieldManagementView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.AppBackground;
            BuildUI();
            LoadDataFromDatabase();
        }

        private void BuildUI()
        {
            // Header
            Controls.Add(Ui.Header("Quản lý danh sách & trạng thái sân", "Theo dõi tình trạng hoạt động, thông số kỹ thuật, giá giờ và tiện ích từng sân thể thao."));

            // Smart Toolbar
            Panel toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Colors.AppBackground,
                Padding = new Padding(0, 0, 0, 14)
            };

            // Left: Search
            SFSearchBox searchBox = new SFSearchBox
            {
                Placeholder = "Tìm mã hoặc tên sân...",
                Width = 220,
                MinimumSize = new Size(180, 40),
                Dock = DockStyle.Left
            };
            searchBox.TextChanged += (s, e) =>
            {
                searchText = searchBox.Text.Trim().ToLowerInvariant();
                ApplyFilter();
            };
            toolbarPanel.Controls.Add(searchBox);

            // Filter Chips
            FlowLayoutPanel chipsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.Transparent
            };
            chipAll = new SFFilterChip { Text = "Tất cả", Count = 10, Selected = true, Size = new Size(78, 40) };
            chipActive = new SFFilterChip { Text = "Đang chạy", Count = 9, Size = new Size(105, 40) };
            chipMaint = new SFFilterChip { Text = "Bảo trì", Count = 1, Size = new Size(78, 40) };
            chipPause = new SFFilterChip { Text = "Tạm ngừng", Count = 0, Size = new Size(95, 40) };

            chipAll.Click += (s, e) => SetFilter("ALL", chipAll);
            chipActive.Click += (s, e) => SetFilter("SAN_SANG", chipActive);
            chipMaint.Click += (s, e) => SetFilter("BAO_TRI", chipMaint);
            chipPause.Click += (s, e) => SetFilter("TAM_NGUNG", chipPause);

            chipsPanel.Controls.Add(chipAll);
            chipsPanel.Controls.Add(chipActive);
            chipsPanel.Controls.Add(chipMaint);
            chipsPanel.Controls.Add(chipPause);
            toolbarPanel.Controls.Add(chipsPanel);

            // Right CTA: Primary Button [+ Thêm sân mới]
            SFButton btnAdd = new SFButton
            {
                Text = "Thêm Sân (F2)",
                Variant = SFButtonVariant.Primary,
                Width = 145,
                Dock = DockStyle.Right,
                LeadingIcon = SFIcon.Plus
            };
            btnAdd.Click += (s, e) =>
            {
                if (new AddFieldForm().ShowDialog(this) == DialogResult.OK)
                {
                    LoadDataFromDatabase();
                }
            };
            toolbarPanel.Controls.Add(btnAdd);

            // View Mode Toggle [Thẻ ảnh] [Bảng]
            Panel toggleBox = new Panel { Dock = DockStyle.Right, Width = 148, Height = 40, Padding = new Padding(4, 0, 4, 0), BackColor = Color.Transparent };
            SFButton btnCardMode = new SFButton { Text = "Thẻ", Variant = SFButtonVariant.Primary, Width = 68, Height = 40, Dock = DockStyle.Left };
            SFButton btnTableMode = new SFButton { Text = "Bảng", Variant = SFButtonVariant.Secondary, Width = 68, Height = 40, Dock = DockStyle.Right };

            btnCardMode.Click += (s, e) =>
            {
                btnCardMode.Variant = SFButtonVariant.Primary;
                btnTableMode.Variant = SFButtonVariant.Secondary;
                cardContainer.Visible = true;
                tableGrid.Visible = false;
            };

            btnTableMode.Click += (s, e) =>
            {
                btnTableMode.Variant = SFButtonVariant.Primary;
                btnCardMode.Variant = SFButtonVariant.Secondary;
                cardContainer.Visible = false;
                tableGrid.Visible = true;
            };

            toggleBox.Controls.Add(btnTableMode);
            toggleBox.Controls.Add(btnCardMode);
            toolbarPanel.Controls.Add(toggleBox);

            Controls.Add(toolbarPanel);
            toolbarPanel.BringToFront();

            // Content Holder
            contentHolder = new Panel { Dock = DockStyle.Fill, BackColor = Colors.AppBackground };
            Controls.Add(contentHolder);
            contentHolder.BringToFront();

            // 1. Card Container
            cardContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(0, 4, 0, 0),
                BackColor = Colors.AppBackground
            };
            contentHolder.Controls.Add(cardContainer);

            // 2. Table Grid
            tableGrid = new SFDataGridView { Dock = DockStyle.Fill, Visible = false };
            tableGrid.Columns.Add("C0", "Mã sân");
            tableGrid.Columns.Add("C1", "Tên sân");
            tableGrid.Columns.Add("C2", "Bộ môn");
            tableGrid.Columns.Add("C3", "Khu vực / Cụm");
            tableGrid.Columns.Add("C4", "Mặt sân / Chi tiết");
            tableGrid.Columns.Add("C5", "Đơn giá");
            tableGrid.Columns.Add("C6", "Trạng thái");
            tableGrid.Columns.Add("C7", "Hôm nay");

            tableGrid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && tableGrid.Rows[e.RowIndex].Cells[1].Value != null)
                {
                    string fName = tableGrid.Rows[e.RowIndex].Cells[1].Value.ToString();
                    new FieldDetailForm(fName).ShowDialog(this);
                }
            };
            contentHolder.Controls.Add(tableGrid);
        }

        private void SetFilter(string status, SFFilterChip activeChip)
        {
            statusFilter = status;
            chipAll.Selected = (activeChip == chipAll);
            chipActive.Selected = (activeChip == chipActive);
            chipMaint.Selected = (activeChip == chipMaint);
            chipPause.Selected = (activeChip == chipPause);
            ApplyFilter();
        }

        public void LoadDataFromDatabase()
        {
            try
            {
                fieldsTable = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachSan");
                if (fieldsTable != null && fieldsTable.Rows.Count > 0)
                {
                    int total = fieldsTable.Rows.Count;
                    int active = 0, maint = 0, pause = 0;
                    foreach (DataRow r in fieldsTable.Rows)
                    {
                        string st = r["TrangThaiVanHanh"]?.ToString() ?? "";
                        if (st == "SAN_SANG") active++;
                        else if (st == "BAO_TRI") maint++;
                        else pause++;
                    }

                    chipAll.Count = total;
                    chipActive.Count = active;
                    chipMaint.Count = maint;
                    chipPause.Count = pause;

                    ApplyFilter();
                    return;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_FIELDS");
            }

            tableGrid.Rows.Clear();
            cardContainer.Controls.Clear();
        }

        private void ApplyFilter()
        {
            tableGrid.Rows.Clear();
            cardContainer.Controls.Clear();

            if (fieldsTable == null || fieldsTable.Rows.Count == 0)
            {
                return;
            }

            foreach (DataRow row in fieldsTable.Rows)
            {
                int sanId = row.Table.Columns.Contains("SanID") && row["SanID"] != DBNull.Value ? Convert.ToInt32(row["SanID"]) : 0;
                string code = row["MaSan"]?.ToString() ?? "";
                string name = row["TenSan"]?.ToString() ?? "";
                string sport = row["TenLoaiSan"]?.ToString() ?? "";
                string cum = row["TenCumSan"]?.ToString() ?? "";
                string viTri = row["ViTri"]?.ToString() ?? "";
                string area = string.IsNullOrEmpty(viTri) ? cum : cum + " • " + viTri;
                decimal price = row["GiaCoBan"] != DBNull.Value ? Convert.ToDecimal(row["GiaCoBan"]) : 0;
                string stCode = row["TrangThaiVanHanh"]?.ToString() ?? "SAN_SANG";
                int ordersToday = row.Table.Columns.Contains("LuotDatHomNay") && row["LuotDatHomNay"] != DBNull.Value ? Convert.ToInt32(row["LuotDatHomNay"]) : 0;
                string desc = row["MoTa"]?.ToString() ?? "";

                // Apply status filter
                if (statusFilter != "ALL" && !stCode.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Apply text filter
                if (!string.IsNullOrEmpty(searchText))
                {
                    bool match = code.ToLowerInvariant().Contains(searchText) ||
                                 name.ToLowerInvariant().Contains(searchText) ||
                                 sport.ToLowerInvariant().Contains(searchText);
                    if (!match) continue;
                }

                string statusText = stCode == "SAN_SANG" ? "Sẵn sàng" : (stCode == "BAO_TRI" ? "Bảo trì" : "Tạm dừng");
                string priceText = price.ToString("#,##0") + "đ/h";
                string todayText = ordersToday > 0 ? (ordersToday + " lượt đặt") : "Chưa có lượt";

                // Add to table
                int rIdx = tableGrid.Rows.Add(new[] { code, name, sport, area, string.IsNullOrEmpty(desc) ? "Tiêu chuẩn FIFA/VBA" : desc, priceText, statusText, todayText });
                tableGrid.Rows[rIdx].Tag = sanId;

                // Add card
                Image img = AssetManager.GetFootballPitch();
                string maLoai = row["MaLoaiSan"]?.ToString() ?? "";
                if (maLoai.Contains("CAU_LONG")) img = AssetManager.GetBadmintonCourt();
                else if (maLoai.Contains("TENNIS") || maLoai.Contains("PICKLE")) img = AssetManager.GetTennisCourt();

                SFFieldCard card = new SFFieldCard
                {
                    FieldName = name,
                    SportType = sport,
                    LocationArea = area,
                    PricePerHour = priceText,
                    Status = statusText,
                    FieldImage = img,
                    Amenities = new[] { "Đèn LED", "Phòng thay đồ", "Wifi tốc độ cao" }
                };

                string cardName = name;
                int currentSanId = sanId;
                card.OnDetailClicked += (s, e) => new FieldDetailForm(cardName).ShowDialog(this);
                card.OnBookClicked += (s, e) => (FindForm() as MainForm)?.ShowView("Đặt sân");

                cardContainer.Controls.Add(card);
            }
        }
    }
}
