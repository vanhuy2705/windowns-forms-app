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
    public class FieldTypesPriceView : UserControl
    {
        private SFSearchBox searchBox;
        private SFDataGridView gridTypes;
        private SFDataGridView gridPrices;
        private TableLayoutPanel kpiPanel;
        private DataTable typesTable;
        private DataTable pricesTable;
        private string searchKeyword = "";

        public FieldTypesPriceView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadDataFromDatabase();
        }

        private void BuildUI()
        {
            // 1. Header
            Controls.Add(Ui.Header("Loại Sân & Bảng Giá Khung Giờ", "Thiết lập danh mục môn thể thao, thông số kỹ thuật, khung giờ cao điểm và chính sách đơn giá."));

            // 2. Toolbar
            FlowLayoutPanel tb = Ui.Toolbar();
            
            SFButton btnAddType = new SFButton
            {
                Text = "Thêm Loại Sân",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Plus,
                Width = 160,
                Height = 38
            };
            btnAddType.Click += (s, e) =>
            {
                using (var dlg = new AddFieldTypeForm())
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadDataFromDatabase();
                        SFToast.Show(this, "Thêm loại sân thành công!", "LOẠI SÂN", SFIcon.Check, Colors.Emerald);
                    }
                }
            };
            tb.Controls.Add(btnAddType);

            SFButton btnSetSlot = new SFButton
            {
                Text = "Thiết Lập Khung Giờ",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Clock,
                Width = 175,
                Height = 38
            };
            btnSetSlot.Click += (s, e) =>
            {
                using (var dlg = new PricingConfigForm())
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadDataFromDatabase();
                        SFToast.Show(this, "Cập nhật bảng giá thành công!", "BẢNG GIÁ", SFIcon.Check, Colors.Emerald);
                    }
                }
            };
            tb.Controls.Add(btnSetSlot);

            SFButton btnExport = new SFButton
            {
                Text = "Xuất Excel / PDF",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Printer,
                Width = 150,
                Height = 38
            };

            ContextMenuStrip exportMenu = new ContextMenuStrip();
            exportMenu.Items.Add("Xuất Bảng Giá Khung Giờ (Excel/CSV)", null, (ms, me) => Helpers.ExportHelper.ExportToExcel(gridPrices, "BangGiaVaKhungGio.csv", this));
            exportMenu.Items.Add("Xuất Bảng Giá Khung Giờ (PDF/HTML)", null, (ms, me) => Helpers.ExportHelper.ExportToPdf("BẢNG GIÁ KHUNG GIỜ", gridPrices, "BangGiaVaKhungGio.html", this));
            exportMenu.Items.Add("-");
            exportMenu.Items.Add("Xuất Danh Mục Loại Sân (Excel/CSV)", null, (ms, me) => Helpers.ExportHelper.ExportToExcel(gridTypes, "DanhMucLoaiSan.csv", this));
            exportMenu.Items.Add("Xuất Danh Mục Loại Sân (PDF/HTML)", null, (ms, me) => Helpers.ExportHelper.ExportToPdf("DANH MỤC LOẠI SÂN", gridTypes, "DanhMucLoaiSan.html", this));

            btnExport.Click += (s, e) => exportMenu.Show(btnExport, 0, btnExport.Height);
            tb.Controls.Add(btnExport);

            searchBox = new SFSearchBox
            {
                PlaceholderText = "Tìm loại sân / khung giờ...",
                Width = 240,
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

            // 3. Metric Bar (3 cards)
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
            kpiPanel.Controls.Add(new SFMetricCard("TỔNG LOẠI SÂN", "—", "Đang vận hành", SFIcon.Field, Colors.Emerald), 0, 0);
            kpiPanel.Controls.Add(new SFMetricCard("QUY MÔ SÂN", "—", "Khu trung tâm & phụ", SFIcon.Field, Colors.ElectricBlue), 1, 0);
            kpiPanel.Controls.Add(new SFMetricCard("KHUNG GIỜ VÀNG", "—", "Phụ thu cao điểm", SFIcon.Clock, Colors.Amber), 2, 0);
            kpiPanel.Controls.Add(new SFMetricCard("ĐƠN GIÁ BÌNH QUÂN", "—", "/ giờ thi đấu chuẩn", SFIcon.Dollar, Colors.Purple), 3, 0);
            
            Controls.Add(kpiPanel);
            kpiPanel.BringToFront();

            // 4. Split Body: Left (42%) = Danh mục Loại sân, Right (58%) = Bảng giá chi tiết
            TableLayoutPanel split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));

            // Left Card
            SFCard cardLeft = new SFCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                Margin = new Padding(0, 0, 10, 0)
            };
            Label lblLeftTitle = new Label
            {
                Text = "Danh Mục Loại Môn & Tiêu Chuẩn",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 32
            };
            cardLeft.Controls.Add(lblLeftTitle);

            gridTypes = new SFDataGridView
            {
                Dock = DockStyle.Fill
            };
            gridTypes.Columns.Add("Code", "Mã");
            gridTypes.Columns.Add("Name", "Tên loại môn");
            gridTypes.Columns.Add("Size", "Mô tả quy cách");
            gridTypes.Columns.Add("Count", "Số sân");
            gridTypes.Columns.Add("Status", "Trạng thái");

            gridTypes.Columns["Code"].Width = 65;
            gridTypes.Columns["Name"].Width = 140;
            gridTypes.Columns["Size"].Width = 120;
            gridTypes.Columns["Count"].Width = 70;
            gridTypes.Columns["Status"].Width = 105;
            gridTypes.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            cardLeft.Controls.Add(gridTypes);
            gridTypes.BringToFront();
            split.Controls.Add(cardLeft, 0, 0);

            // Right Card
            SFCard cardRight = new SFCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                Margin = new Padding(10, 0, 0, 0)
            };
            
            Panel rightHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36
            };
            Label lblRightTitle = new Label
            {
                Text = "Bảng Giá Theo Khung Giờ & Ngày",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Left,
                AutoSize = true
            };
            rightHeader.Controls.Add(lblRightTitle);
            cardRight.Controls.Add(rightHeader);

            gridPrices = new SFDataGridView
            {
                Dock = DockStyle.Fill
            };
            gridPrices.Columns.Add("Sport", "Loại sân");
            gridPrices.Columns.Add("Days", "Ngày áp dụng");
            gridPrices.Columns.Add("From", "Từ");
            gridPrices.Columns.Add("To", "Đến");
            gridPrices.Columns.Add("Price", "Giá niêm yết/h");
            gridPrices.Columns.Add("Peak", "Loại khung");
            gridPrices.Columns.Add("Status", "Hiệu lực");

            gridPrices.Columns["Sport"].Width = 130;
            gridPrices.Columns["Days"].Width = 90;
            gridPrices.Columns["From"].Width = 65;
            gridPrices.Columns["To"].Width = 65;
            gridPrices.Columns["Price"].Width = 110;
            gridPrices.Columns["Peak"].Width = 110;
            gridPrices.Columns["Status"].Width = 95;
            gridPrices.Columns["Sport"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            cardRight.Controls.Add(gridPrices);
            gridPrices.BringToFront();
            split.Controls.Add(cardRight, 1, 0);

            Controls.Add(split);
            split.BringToFront();
        }

        public void LoadDataFromDatabase()
        {
            try
            {
                typesTable = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachLoaiSan");
                pricesTable = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_LayBangGia");

                if (typesTable != null && typesTable.Rows.Count > 0)
                {
                    int totalTypes = typesTable.Rows.Count;
                    int totalCourts = 0;
                    foreach (DataRow r in typesTable.Rows)
                    {
                        if (r.Table.Columns.Contains("SoLuongSan") && r["SoLuongSan"] != DBNull.Value)
                            totalCourts += Convert.ToInt32(r["SoLuongSan"]);
                        else if (r.Table.Columns.Contains("SoSan") && r["SoSan"] != DBNull.Value)
                            totalCourts += Convert.ToInt32(r["SoSan"]);
                    }

                    decimal avgPrice = 0;
                    bool hasAvg = false;
                    if (pricesTable != null && pricesTable.Rows.Count > 0)
                    {
                        decimal sumPrice = 0;
                        int countPrice = 0;
                        foreach (DataRow pr in pricesTable.Rows)
                        {
                            if (pr.Table.Columns.Contains("DonGia") && pr["DonGia"] != DBNull.Value)
                            {
                                sumPrice += Convert.ToDecimal(pr["DonGia"]);
                                countPrice++;
                            }
                        }
                        if (countPrice > 0) { avgPrice = sumPrice / countPrice; hasAvg = true; }
                    }

                    kpiPanel.Controls.Clear();
                    kpiPanel.Controls.Add(new SFMetricCard("TỔNG LOẠI SÂN", totalTypes.ToString("00"), "Môn thể thao", SFIcon.Field, Colors.Emerald, "CSDL Live"), 0, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("QUY MÔ SÂN", totalCourts.ToString("00") + " Sân", "Khu trung tâm & phụ", SFIcon.Field, Colors.ElectricBlue), 1, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("KHUNG GIỜ VÀNG", "—", "Phụ thu cao điểm", SFIcon.Clock, Colors.Amber), 2, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("ĐƠN GIÁ BÌNH QUÂN", hasAvg ? string.Format("{0:N0}đ", (int)avgPrice) : "—", "/ giờ thi đấu chuẩn", SFIcon.Dollar, Colors.Purple), 3, 0);

                    ApplyFilter();
                    return;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_FIELD_TYPES_PRICES");
            }

            gridTypes.Rows.Clear();
            gridPrices.Rows.Clear();
        }

        private void ApplyFilter()
        {
            gridTypes.Rows.Clear();
            gridPrices.Rows.Clear();

            if (typesTable == null || typesTable.Rows.Count == 0)
            {
                return;
            }

            // Bind Types
            foreach (DataRow r in typesTable.Rows)
            {
                string code = r["MaLoaiSan"]?.ToString() ?? "";
                string name = r["TenLoaiSan"]?.ToString() ?? "";
                string moTa = r["MoTa"]?.ToString() ?? "Quy chuẩn thi đấu";
                
                int soSan = 0;
                if (r.Table.Columns.Contains("SoLuongSan") && r["SoLuongSan"] != DBNull.Value)
                    soSan = Convert.ToInt32(r["SoLuongSan"]);
                else if (r.Table.Columns.Contains("SoSan") && r["SoSan"] != DBNull.Value)
                    soSan = Convert.ToInt32(r["SoSan"]);

                if (r.Table.Columns.Contains("GiaThapNhat") && r.Table.Columns.Contains("GiaCaoNhat") && r["GiaThapNhat"] != DBNull.Value && r["GiaCaoNhat"] != DBNull.Value)
                {
                    decimal minP = Convert.ToDecimal(r["GiaThapNhat"]);
                    decimal maxP = Convert.ToDecimal(r["GiaCaoNhat"]);
                    if (minP > 0 || maxP > 0)
                    {
                        moTa = string.Format("{0} • {1:N0}đ - {2:N0}đ", moTa, minP, maxP);
                    }
                }

                bool active = r["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(r["DangHoatDong"]);

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    if (!code.ToLowerInvariant().Contains(searchKeyword) && !name.ToLowerInvariant().Contains(searchKeyword))
                        continue;
                }

                gridTypes.Rows.Add(
                    code,
                    name,
                    moTa,
                    soSan + " sân",
                    active ? "Hoạt động" : "Tạm dừng"
                );
            }

            // Bind Prices
            if (pricesTable != null && pricesTable.Rows.Count > 0)
            {
                foreach (DataRow r in pricesTable.Rows)
                {
                    string sport = r["TenLoaiSan"]?.ToString() ?? (r["TenSan"]?.ToString() ?? "Tất cả");
                    string loaiNgay = r["LoaiNgay"]?.ToString() == "CUOI_TUAN" ? "T7 - CN" : "T2 - T6";
                    string fromTime = r["GioBatDau"] != DBNull.Value ? r["GioBatDau"].ToString().Substring(0, 5) : "06:00";
                    string toTime = r["GioKetThuc"] != DBNull.Value ? r["GioKetThuc"].ToString().Substring(0, 5) : "23:00";
                    decimal price = r["DonGia"] != DBNull.Value ? Convert.ToDecimal(r["DonGia"]) : 0;
                    string nameBang = r["TenBangGia"]?.ToString() ?? "";
                    string peakText = nameBang.Contains("vàng") ? "+Giờ vàng" : (loaiNgay.Contains("CN") ? "Cuối tuần" : "Tiêu chuẩn");
                    bool active = r["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(r["DangHoatDong"]);

                    if (!string.IsNullOrEmpty(searchKeyword))
                    {
                        if (!sport.ToLowerInvariant().Contains(searchKeyword) && !nameBang.ToLowerInvariant().Contains(searchKeyword))
                            continue;
                    }

                    gridPrices.Rows.Add(
                        sport,
                        loaiNgay,
                        fromTime,
                        toTime,
                        price.ToString("#,##0") + "đ",
                        peakText,
                        active ? "Áp dụng" : "Hết hạn"
                    );
                }
            }
        }
    }
}
