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
    public class AdminInvoicesView : UserControl
    {
        private SFSearchBox searchBox;
        private SFDataGridView grid;
        private SFMetricCard cardTotalToday;
        private SFMetricCard cardRemaining;
        private SFMetricCard cardPendingTransfer;
        private SFMetricCard cardSettledRate;
        private DataTable cachedData;

        public AdminInvoicesView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadDataFromDatabase();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Hóa Đơn & Thanh Toán", "Theo dõi công nợ, tiền cọc, thanh toán tiền mặt và giao dịch VietQR Napas 247."));

            FlowLayoutPanel tb = Ui.Toolbar();

            SFButton btnPay = new SFButton
            {
                Text = "Thanh Toán POS",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Dollar,
                Width = 160,
                Height = 38
            };
            btnPay.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    DataRow row = grid.SelectedRows[0].Tag as DataRow;
                    if (row != null)
                    {
                        long hoaDonId = Convert.ToInt64(row["HoaDonID"]);
                        string maDon = row["MaDon"]?.ToString() ?? "";
                        string kh = row["HoTen"]?.ToString() ?? "";
                        decimal tongTien = row["TongTien"] != DBNull.Value ? Convert.ToDecimal(row["TongTien"]) : 0;
                        decimal daTra = row["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(row["DaThanhToan"]) : 0;
                        decimal rem = row["ConLai"] != DBNull.Value ? Convert.ToDecimal(row["ConLai"]) : (tongTien - daTra);
                        if (rem <= 0)
                        {
                            SFToast.Show(this, "Hóa đơn này đã hoàn tất thanh toán (còn lại 0đ).", "THÔNG BÁO", SFIcon.Check, Colors.Emerald);
                            return;
                        }
                        var dlg = new QrPaymentForm(tongTien, daTra, rem, maDon, kh, hoaDonId);
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            LoadDataFromDatabase();
                        }
                        return;
                    }
                }
                SFToast.Show(this, "Vui lòng chọn một hóa đơn cần thanh toán trong danh sách.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Amber);
            };
            tb.Controls.Add(btnPay);

            SFButton btnConfirm = new SFButton
            {
                Text = "Xác Nhận Chuyển Khoản",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Check,
                Width = 195,
                Height = 38
            };
            btnConfirm.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    DataRow row = grid.SelectedRows[0].Tag as DataRow;
                    if (row != null)
                    {
                        long hoaDonId = Convert.ToInt64(row["HoaDonID"]);
                        string maHd = row["MaHoaDon"]?.ToString() ?? "";
                        try
                        {
                            DataTable gdDt = DatabaseManager.ExecuteQuery(
                                "SELECT TOP 1 GiaoDichID FROM dbo.GiaoDichThanhToan WHERE HoaDonID = @HD AND TrangThai = 'CHO_XAC_NHAN' AND PhuongThuc IN ('VIETQR','CHUYEN_KHOAN')",
                                new SqlParameter("@HD", hoaDonId));
                            if (gdDt != null && gdDt.Rows.Count > 0)
                            {
                                long gdId = Convert.ToInt64(gdDt.Rows[0]["GiaoDichID"]);
                                int userId = SessionGuard.RequireAccountId();
                                var payService = new PaymentService();
                                payService.ConfirmBankTransfer(gdId, userId, "XAC_NHAN_CHUYEN_KHOAN");
                                SFToast.Show(this, "Đã xác nhận giao dịch chuyển khoản cho hóa đơn " + maHd + " thành công!", "XÁC NHẬN CK", SFIcon.Check, Colors.Emerald);
                                LoadDataFromDatabase();
                            }
                            else
                            {
                                SFToast.Show(this, "Hóa đơn này không có giao dịch chuyển khoản nào đang chờ xác nhận.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Amber);
                            }
                        }
                        catch (Exception ex)
                        {
                            AppLogger.LogError(ex, "INVOICES", "btnConfirm");
                            SFToast.Show(this, "Lỗi xác nhận: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
                        }
                        return;
                    }
                }
                SFToast.Show(this, "Vui lòng chọn hóa đơn cần xác nhận chuyển khoản.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Amber);
            };
            tb.Controls.Add(btnConfirm);

            SFButton btnRefresh = new SFButton
            {
                Text = "Làm Mới",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Refresh,
                Width = 120,
                Height = 38
            };
            btnRefresh.Click += (s, e) => LoadDataFromDatabase();
            tb.Controls.Add(btnRefresh);

            searchBox = new SFSearchBox
            {
                PlaceholderText = "Mã HĐ / mã đơn / khách...",
                Width = 240,
                Height = 38
            };

            Timer debounceTimer = new Timer { Interval = 300 };
            debounceTimer.Tick += (s, e) =>
            {
                debounceTimer.Stop();
                if (!IsDisposed) FilterData();
            };
            searchBox.TextChanged += (s, e) =>
            {
                debounceTimer.Stop();
                debounceTimer.Start();
            };
            tb.Controls.Add(searchBox);

            Controls.Add(tb);
            tb.BringToFront();

            // KPI Cards
            TableLayoutPanel kpi = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                ColumnCount = 4,
                Padding = new Padding(0, 0, 0, 14)
            };
            for (int i = 0; i < 4; i++) kpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            cardTotalToday = new SFMetricCard("TỔNG THU HÔM NAY", "0 đ", "Hóa đơn hoàn tất", SFIcon.Dollar, Colors.Emerald);
            cardRemaining = new SFMetricCard("CÒN PHẢI THU", "0 đ", "Đơn đang còn dư nợ", SFIcon.Clock, Colors.Amber, "Thu khi trả sân");
            cardPendingTransfer = new SFMetricCard("CHỜ XÁC NHẬN CK", "0 đ", "Giao dịch VietQR", SFIcon.Bell, Colors.ElectricBlue);
            cardSettledRate = new SFMetricCard("TỶ LỆ QUYẾT TOÁN", "—", "Đơn thanh toán đủ", SFIcon.Check, Colors.Purple);

            kpi.Controls.Add(cardTotalToday, 0, 0);
            kpi.Controls.Add(cardRemaining, 1, 0);
            kpi.Controls.Add(cardPendingTransfer, 2, 0);
            kpi.Controls.Add(cardSettledRate, 3, 0);

            Controls.Add(kpi);
            kpi.BringToFront();

            // Main Data Grid
            SFCard card = new SFCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };

            Panel gridHeader = new Panel { Dock = DockStyle.Top, Height = 42 };
            Label lblGridTitle = new Label
            {
                Text = "Danh Sách Hóa Đơn & Giao Dịch Trong Ngày (Nhấp đúp dòng để mở mã QR thanh toán)",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Left,
                AutoSize = true,
                UseMnemonic = false
            };
            gridHeader.Controls.Add(lblGridTitle);
            card.Controls.Add(gridHeader);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("InvoiceId", "Mã HĐ");
            grid.Columns.Add("BookingId", "Mã đơn");
            grid.Columns.Add("Customer", "Khách hàng");
            grid.Columns.Add("Field", "Sân thi đấu");
            grid.Columns.Add("Total", "Tổng tiền");
            grid.Columns.Add("Paid", "Đã trả");
            grid.Columns.Add("Remaining", "Còn lại");
            grid.Columns.Add("Status", "Trạng thái");
            grid.Columns.Add("Date", "Thời gian");

            grid.Columns["InvoiceId"].Width = 120;
            grid.Columns["BookingId"].Width = 110;
            grid.Columns["Customer"].Width = 180;
            grid.Columns["Field"].Width = 140;
            grid.Columns["Total"].Width = 120;
            grid.Columns["Paid"].Width = 120;
            grid.Columns["Remaining"].Width = 110;
            grid.Columns["Status"].Width = 130;
            grid.Columns["Date"].Width = 130;

            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < grid.Rows.Count)
                {
                    DataRow row = grid.Rows[e.RowIndex].Tag as DataRow;
                    if (row != null)
                    {
                        long hoaDonId = Convert.ToInt64(row["HoaDonID"]);
                        string maDon = row["MaDon"]?.ToString() ?? "";
                        string kh = row["HoTen"]?.ToString() ?? "";
                        decimal tongTien = row["TongTien"] != DBNull.Value ? Convert.ToDecimal(row["TongTien"]) : 0;
                        decimal daTra = row["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(row["DaThanhToan"]) : 0;
                        decimal rem = row["ConLai"] != DBNull.Value ? Convert.ToDecimal(row["ConLai"]) : (tongTien - daTra);
                        if (rem > 0)
                        {
                            var dlg = new QrPaymentForm(tongTien, daTra, rem, maDon, kh, hoaDonId);
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                            {
                                LoadDataFromDatabase();
                            }
                        }
                        else
                        {
                            SFToast.Show(this, "Hóa đơn này đã hoàn tất thanh toán (dư nợ = 0đ).", "THÔNG BÁO", SFIcon.Check, Colors.Emerald);
                        }
                    }
                }
            };

            ContextMenuStrip cms = new ContextMenuStrip();
            var mnuPay = cms.Items.Add("Thanh Toán POS / VietQR");
            mnuPay.Click += (s, e) => btnPay.PerformClick();

            var mnuConfirm = cms.Items.Add("Xác Nhận Chuyển Khoản Ngân Hàng");
            mnuConfirm.Click += (s, e) => btnConfirm.PerformClick();

            var mnuExcel = cms.Items.Add("Xuất Danh Sách Hóa Đơn Ra Excel");
            mnuExcel.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Danh_Sach_Hoa_Don", this);

            grid.ContextMenuStrip = cms;

            card.Controls.Add(grid);
            grid.BringToFront();

            Controls.Add(card);
            card.BringToFront();
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachHoaDon");
                cachedData = dt;
                grid.Rows.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    RenderRows(dt);

                    // Compute KPI metrics
                    decimal totalPaid = 0;
                    decimal totalRemaining = 0;
                    decimal totalPendingTransfer = 0;
                    int settledCount = 0;
                    int totalInvoices = dt.Rows.Count;

                    foreach (DataRow row in dt.Rows)
                    {
                        decimal paid = row["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(row["DaThanhToan"]) : 0;
                        decimal rem = row["ConLai"] != DBNull.Value ? Convert.ToDecimal(row["ConLai"]) : 0;
                        string st = row["TrangThaiThanhToan"] != DBNull.Value ? row["TrangThaiThanhToan"].ToString() : "";

                        totalPaid += paid;
                        totalRemaining += rem;

                        if (st == "CHO_XAC_NHAN")
                        {
                            totalPendingTransfer += rem > 0 ? rem : paid;
                        }
                        if (st == "DA_THANH_TOAN" || rem == 0)
                        {
                            settledCount++;
                        }
                    }

                    cardTotalToday.Value = string.Format("{0:N0} đ", totalPaid);
                    cardTotalToday.SubValue = string.Format("{0} hóa đơn phát sinh", totalInvoices);

                    cardRemaining.Value = string.Format("{0:N0} đ", totalRemaining);
                    cardRemaining.SubValue = string.Format("{0} đơn còn dư nợ", totalInvoices - settledCount);

                    cardPendingTransfer.Value = string.Format("{0:N0} đ", totalPendingTransfer);
                    cardPendingTransfer.SubValue = "Giao dịch VietQR chờ duyệt";

                    double settledRate = totalInvoices > 0 ? ((double)settledCount / totalInvoices * 100.0) : 100.0;
                    cardSettledRate.Value = string.Format("{0:0.#}%", settledRate);
                    cardSettledRate.SubValue = string.Format("{0}/{1} đơn thanh toán đủ", settledCount, totalInvoices);
                }
                else
                {
                    cardTotalToday.Value = "0 đ";
                    cardRemaining.Value = "0 đ";
                    cardPendingTransfer.Value = "0 đ";
                    cardSettledRate.Value = "100%";
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "INVOICES", "LoadDataFromDatabase");
                cardTotalToday.Value = "0 đ";
                cardRemaining.Value = "0 đ";
                cardPendingTransfer.Value = "0 đ";
                cardSettledRate.Value = "100%";
                SFToast.Show(this, "Lỗi tải hóa đơn: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private void FilterData()
        {
            if (cachedData == null) return;
            string keyword = searchBox.Text != null ? searchBox.Text.Trim().ToLower() : "";
            if (string.IsNullOrEmpty(keyword))
            {
                RenderRows(cachedData);
                return;
            }

            grid.Rows.Clear();
            foreach (DataRow row in cachedData.Rows)
            {
                string id = row["MaHoaDon"]?.ToString() ?? "";
                string maDon = row["MaDon"]?.ToString() ?? "";
                string ten = row["HoTen"]?.ToString() ?? "";
                string phone = row.Table.Columns.Contains("SoDienThoai") ? (row["SoDienThoai"]?.ToString() ?? "") : "";
                if (id.ToLower().Contains(keyword) || maDon.ToLower().Contains(keyword) || ten.ToLower().Contains(keyword) || phone.ToLower().Contains(keyword))
                {
                    AddGridRow(row);
                }
            }
        }

        private void RenderRows(DataTable dt)
        {
            grid.Rows.Clear();
            foreach (DataRow row in dt.Rows)
            {
                AddGridRow(row);
            }
        }

        private void AddGridRow(DataRow row)
        {
            string maHd = row["MaHoaDon"] != DBNull.Value ? row["MaHoaDon"].ToString() : "";
            string maDon = row["MaDon"] != DBNull.Value ? row["MaDon"].ToString() : "";
            string hoTen = row["HoTen"] != DBNull.Value ? row["HoTen"].ToString() : "";
            string ghiChu = row["GhiChu"] != DBNull.Value ? row["GhiChu"].ToString() : "Sân thể thao";
            decimal tong = row["TongTien"] != DBNull.Value ? Convert.ToDecimal(row["TongTien"]) : 0;
            decimal daTra = row["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(row["DaThanhToan"]) : 0;
            decimal conLai = row["ConLai"] != DBNull.Value ? Convert.ToDecimal(row["ConLai"]) : 0;
            string stCode = row["TrangThaiThanhToan"] != DBNull.Value ? row["TrangThaiThanhToan"].ToString() : "";
            DateTime ngay = row["NgayLap"] != DBNull.Value ? Convert.ToDateTime(row["NgayLap"]) : DateTime.Now;

            string stDisplay = "Chưa thanh toán";
            if (stCode == "DA_THANH_TOAN") stDisplay = "Đã thanh toán";
            else if (stCode == "THANH_TOAN_MOT_PHAN") stDisplay = "Một phần";
            else if (stCode == "CHO_XAC_NHAN") stDisplay = "Chờ xác nhận CK";

            int rowIndex = grid.Rows.Add(
                maHd,
                maDon,
                hoTen,
                ghiChu,
                string.Format("{0:N0}đ", tong),
                string.Format("{0:N0}đ", daTra),
                string.Format("{0:N0}đ", conLai),
                stDisplay,
                ngay.ToString("dd/MM HH:mm")
            );
            grid.Rows[rowIndex].Tag = row;
        }
    }

    public class PromotionsView : UserControl
    {
        private SFDataGridView grid;
        private SFTextBox txtCode;
        private SFTextBox txtName;
        private SFTextBox txtDiscount;
        private SFTextBox txtStart;
        private SFTextBox txtEnd;
        private SFTextBox txtNotes;
        private CheckBox chkActive;
        private SFSearchBox search;
        private DataTable cachedData;
        private int? _selectedKhuyenMaiId = null;

        public PromotionsView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadDataFromDatabase();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Chương Trình Khuyến Mãi", "Quản lý chương trình giảm giá tự động theo khung giờ, đối tượng và điều kiện đặt sân."));

            FlowLayoutPanel tb = Ui.Toolbar();
            SFButton btnAdd = new SFButton { Text = "Tạo Khuyến Mãi", Variant = SFButtonVariant.Primary, IconType = SFIcon.Plus, Width = 170, Height = 38 };
            btnAdd.Click += (s, e) => ResetEditor();
            tb.Controls.Add(btnAdd);

            SFButton btnRefresh = new SFButton { Text = "Làm Mới", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Refresh, Width = 120, Height = 38 };
            btnRefresh.Click += (s, e) => LoadDataFromDatabase();
            tb.Controls.Add(btnRefresh);

            search = new SFSearchBox { PlaceholderText = "Mã / tên chương trình...", Width = 230, Height = 38 };
            Timer debounceTimer = new Timer { Interval = 300 };
            debounceTimer.Tick += (s, e) =>
            {
                debounceTimer.Stop();
                if (!IsDisposed) FilterData();
            };
            search.TextChanged += (s, e) =>
            {
                debounceTimer.Stop();
                debounceTimer.Start();
            };
            tb.Controls.Add(search);
            Controls.Add(tb);
            tb.BringToFront();

            TableLayoutPanel split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));

            // Left: List
            SFCard listCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(0, 0, 10, 0) };
            Label lblTitle = new Label { Text = "Danh Sách Chương Trình Đang Chạy", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            listCard.Controls.Add(lblTitle);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("Code", "Mã");
            grid.Columns.Add("Name", "Tên chương trình");
            grid.Columns.Add("Discount", "Giảm");
            grid.Columns.Add("Start", "Bắt đầu");
            grid.Columns.Add("End", "Kết thúc");
            grid.Columns.Add("Status", "Trạng thái");

            grid.Columns["Code"].Width = 110;
            grid.Columns["Name"].Width = 190;
            grid.Columns["Discount"].Width = 90;
            grid.Columns["Start"].Width = 95;
            grid.Columns["End"].Width = 95;
            grid.Columns["Status"].Width = 100;

            grid.SelectionChanged += Grid_SelectionChanged;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemEdit = cms.Items.Add("Chỉnh Sửa Khuyến Mãi");
            itemEdit.Click += (s, e) => Grid_SelectionChanged(s, e);

            var itemToggle = cms.Items.Add("Tạm Dừng / Kích Hoạt");
            itemToggle.Click += (s, e) =>
            {
                if (_selectedKhuyenMaiId.HasValue && _selectedKhuyenMaiId.Value > 0)
                {
                    try
                    {
                        DatabaseManager.ExecuteNonQuery(
                            "UPDATE dbo.KhuyenMai SET DangHoatDong = CASE WHEN DangHoatDong = 1 THEN 0 ELSE 1 END WHERE KhuyenMaiID = @ID",
                            new SqlParameter("@ID", _selectedKhuyenMaiId.Value));
                        SFToast.Show(this, "Đã chuyển đổi trạng thái khuyến mãi thành công.", "KHUYẾN MÃI", SFIcon.Check, Colors.Emerald);
                        LoadDataFromDatabase();
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogError(ex, "PROMOTIONS", "ToggleActive");
                        SFToast.Show(this, "Lỗi cập nhật: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
                    }
                }
            };

            var itemExport = cms.Items.Add("Xuất Danh Sách Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Danh_Sach_Khuyen_Mai", this);

            grid.ContextMenuStrip = cms;

            listCard.Controls.Add(grid);
            grid.BringToFront();
            split.Controls.Add(listCard, 0, 0);

            // Right: Detail Editor
            SFCard editCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(18), Margin = new Padding(10, 0, 0, 0) };
            Label lblEdit = new Label { Text = "Thông Tin & Thiết Lập Quy Tắc", UseMnemonic = false, Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            editCard.Controls.Add(lblEdit);

            Panel bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(4, 6, 4, 0), BackColor = Color.Transparent };
            SFButton btnSave = new SFButton { Text = "Lưu Khuyến Mãi", Variant = SFButtonVariant.Primary, IconType = SFIcon.Check, Width = 160, Height = 38, Dock = DockStyle.Left };
            btnSave.Click += (s, e) =>
            {
                btnSave.Enabled = false;
                try
                {
                    SavePromotion();
                }
                finally
                {
                    btnSave.Enabled = true;
                }
            };
            bottomBar.Controls.Add(btnSave);

            SFButton btnDel = new SFButton { Text = "Xóa", Variant = SFButtonVariant.Danger, IconType = SFIcon.Trash, Width = 80, Height = 38, Dock = DockStyle.Right };
            btnDel.Click += (s, e) =>
            {
                if (_selectedKhuyenMaiId.HasValue && _selectedKhuyenMaiId.Value > 0)
                {
                    if (MessageBox.Show("Bạn có muốn ngừng áp dụng chương trình khuyến mãi này?", "XÁC NHẬN", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            DatabaseManager.ExecuteNonQuery("UPDATE dbo.KhuyenMai SET DangHoatDong = 0 WHERE KhuyenMaiID = @ID", new SqlParameter("@ID", _selectedKhuyenMaiId.Value));
                            SFToast.Show(this, "Đã cập nhật trạng thái tạm dừng khuyến mãi.", "KHUYẾN MÃI", SFIcon.Check, Colors.Emerald);
                            LoadDataFromDatabase();
                        }
                        catch (Exception ex)
                        {
                            AppLogger.LogError(ex, "PROMOTIONS", "btnDel");
                            SFToast.Show(this, "Lỗi xóa: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
                        }
                    }
                }
                else
                {
                    SFToast.Show(this, "Vui lòng chọn khuyến mãi cần thao tác.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Amber);
                }
            };
            bottomBar.Controls.Add(btnDel);

            editCard.Controls.Add(bottomBar);

            Panel formBody = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            BuildEditorFields(formBody);
            editCard.Controls.Add(formBody);
            formBody.BringToFront();

            split.Controls.Add(editCard, 1, 0);
            Controls.Add(split);
            split.BringToFront();
        }

        private void BuildEditorFields(Control p)
        {
            int y = 8;
            Label l1 = new Label { Text = "Mã khuyến mãi *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l1);
            txtCode = new SFTextBox { Text = "KM-GIOVANG", Location = new Point(4, y + 20), Width = 310, Height = 42 };
            p.Controls.Add(txtCode);
            y += 62;

            Label l2 = new Label { Text = "Tên chương trình *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l2);
            txtName = new SFTextBox { Text = "Giờ vàng trưa các ngày T2-T6", Location = new Point(4, y + 20), Width = 310, Height = 42 };
            p.Controls.Add(txtName);
            y += 62;

            Label l3 = new Label { Text = "Tỷ lệ giảm (% hoặc VND) *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l3);
            txtDiscount = new SFTextBox { Text = "15", Location = new Point(4, y + 20), Width = 310, Height = 42 };
            p.Controls.Add(txtDiscount);
            y += 62;

            Label l4 = new Label { Text = "Ngày bắt đầu (yyyy-MM-dd)", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l4);
            txtStart = new SFTextBox { Text = DateTime.Today.ToString("yyyy-MM-dd"), Location = new Point(4, y + 20), Width = 310, Height = 42 };
            p.Controls.Add(txtStart);
            y += 62;

            Label l5 = new Label { Text = "Ngày kết thúc (yyyy-MM-dd)", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l5);
            txtEnd = new SFTextBox { Text = DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd"), Location = new Point(4, y + 20), Width = 310, Height = 42 };
            p.Controls.Add(txtEnd);
            y += 62;

            Label l6 = new Label { Text = "Ghi chú & Điều kiện áp dụng", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l6);
            txtNotes = new SFTextBox { Text = "Áp dụng tự động cho đơn bóng đá và cầu lông đặt trước 2 tiếng", Location = new Point(4, y + 20), Width = 310, Height = 42 };
            p.Controls.Add(txtNotes);
            y += 62;

            chkActive = new CheckBox { Text = "Đang áp dụng tự động", Location = new Point(4, y), AutoSize = true, Checked = true, Font = Typography.Body };
            p.Controls.Add(chkActive);
        }

        private void ResetEditor()
        {
            _selectedKhuyenMaiId = null;
            txtCode.Text = "KM-" + DateTime.Now.ToString("MMdd");
            txtName.Text = "Ưu Đãi Mùa Hè 2026";
            txtDiscount.Text = "10";
            txtStart.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtEnd.Text = DateTime.Today.AddMonths(2).ToString("yyyy-MM-dd");
            txtNotes.Text = "Áp dụng cho mọi khung giờ.";
            chkActive.Checked = true;
            txtCode.Focus();
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;
            DataRow row = grid.SelectedRows[0].Tag as DataRow;
            if (row != null)
            {
                _selectedKhuyenMaiId = row["KhuyenMaiID"] != DBNull.Value ? Convert.ToInt32(row["KhuyenMaiID"]) : (int?)null;
                txtCode.Text = row["MaKhuyenMai"]?.ToString() ?? "";
                txtName.Text = row["TenKhuyenMai"]?.ToString() ?? "";
                decimal val = row["GiaTriGiam"] != DBNull.Value ? Convert.ToDecimal(row["GiaTriGiam"]) : 10;
                txtDiscount.Text = val.ToString("0.#");
                txtStart.Text = row["NgayBatDau"] != DBNull.Value ? Convert.ToDateTime(row["NgayBatDau"]).ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd");
                txtEnd.Text = row["NgayKetThuc"] != DBNull.Value ? Convert.ToDateTime(row["NgayKetThuc"]).ToString("yyyy-MM-dd") : DateTime.Today.AddMonths(1).ToString("yyyy-MM-dd");
                txtNotes.Text = row["MoTa"]?.ToString() ?? "";
                chkActive.Checked = row["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(row["DangHoatDong"]);
            }
        }

        private void SavePromotion()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                SFToast.Show(this, "Vui lòng nhập đầy đủ mã và tên khuyến mãi.", "THIẾU THÔNG TIN", SFIcon.AlertCircle, Colors.Coral);
                return;
            }

            try
            {
                decimal disc = decimal.TryParse(txtDiscount.Text.Trim(), out decimal d) ? d : 10;
                if (disc <= 0)
                {
                    SFToast.Show(this, "Tỷ lệ giảm giá phải lớn hơn 0.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Coral);
                    return;
                }

                DateTime start = DateTime.TryParse(txtStart.Text.Trim(), out DateTime dtS) ? dtS : DateTime.Today;
                DateTime end = DateTime.TryParse(txtEnd.Text.Trim(), out DateTime dtE) ? dtE : DateTime.Today.AddMonths(1);
                if (start > end)
                {
                    SFToast.Show(this, "Ngày bắt đầu không được sau ngày kết thúc.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Coral);
                    return;
                }

                SqlParameter[] p = new SqlParameter[]
                {
                    new SqlParameter("@KhuyenMaiID", (object)_selectedKhuyenMaiId ?? DBNull.Value),
                    new SqlParameter("@MaKhuyenMai", txtCode.Text.Trim()),
                    new SqlParameter("@TenKhuyenMai", txtName.Text.Trim()),
                    new SqlParameter("@LoaiGiam", "PHAN_TRAM"),
                    new SqlParameter("@GiaTriGiam", disc),
                    new SqlParameter("@GiamToiDa", DBNull.Value),
                    new SqlParameter("@DonToiThieu", 0),
                    new SqlParameter("@NgayBatDau", start),
                    new SqlParameter("@NgayKetThuc", end),
                    new SqlParameter("@GioBatDau", DBNull.Value),
                    new SqlParameter("@GioKetThuc", DBNull.Value),
                    new SqlParameter("@MoTa", string.IsNullOrWhiteSpace(txtNotes.Text) ? (object)DBNull.Value : txtNotes.Text.Trim()),
                    new SqlParameter("@DangHoatDong", chkActive.Checked ? 1 : 0)
                };

                DatabaseManager.ExecuteNonQuery("EXEC dbo.sp_UI_LuuKhuyenMai @KhuyenMaiID, @MaKhuyenMai, @TenKhuyenMai, @LoaiGiam, @GiaTriGiam, @GiamToiDa, @DonToiThieu, @NgayBatDau, @NgayKetThuc, @GioBatDau, @GioKetThuc, @MoTa, @DangHoatDong", p);

                SFToast.Show(this, "Đã lưu chương trình khuyến mãi vào CSDL SQL Server thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                LoadDataFromDatabase();
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PROMOTIONS", "SavePromotion");
                SFToast.Show(this, "Lỗi lưu CSDL: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachKhuyenMai");
                cachedData = dt;
                grid.Rows.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    RenderRows(dt);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "PROMOTIONS", "LoadDataFromDatabase");
                SFToast.Show(this, "Lỗi tải khuyến mãi: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private void FilterData()
        {
            if (cachedData == null) return;
            string keyword = search.Text != null ? search.Text.Trim().ToLower() : "";
            if (string.IsNullOrEmpty(keyword))
            {
                RenderRows(cachedData);
                return;
            }

            grid.Rows.Clear();
            foreach (DataRow row in cachedData.Rows)
            {
                string code = row["MaKhuyenMai"]?.ToString() ?? "";
                string name = row["TenKhuyenMai"]?.ToString() ?? "";
                string loai = row["LoaiGiam"]?.ToString() ?? "";
                if (code.ToLower().Contains(keyword) || name.ToLower().Contains(keyword) || loai.ToLower().Contains(keyword))
                {
                    AddRow(row);
                }
            }
        }

        private void RenderRows(DataTable dt)
        {
            grid.Rows.Clear();
            foreach (DataRow row in dt.Rows)
            {
                AddRow(row);
            }
        }

        private void AddRow(DataRow row)
        {
            string code = row["MaKhuyenMai"]?.ToString() ?? "";
            string name = row["TenKhuyenMai"]?.ToString() ?? "";
            string loai = row["LoaiGiam"]?.ToString() ?? "";
            decimal val = row["GiaTriGiam"] != DBNull.Value ? Convert.ToDecimal(row["GiaTriGiam"]) : 0;
            string disc = loai == "PHAN_TRAM" ? string.Format("{0:0.#}%", val) : string.Format("{0:N0}đ", val);

            DateTime sDate = row["NgayBatDau"] != DBNull.Value ? Convert.ToDateTime(row["NgayBatDau"]) : DateTime.Today;
            DateTime eDate = row["NgayKetThuc"] != DBNull.Value ? Convert.ToDateTime(row["NgayKetThuc"]) : DateTime.Today;
            bool active = row["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(row["DangHoatDong"]);

            int rowIndex = grid.Rows.Add(code, name, disc, sDate.ToString("dd/MM/yyyy"), eDate.ToString("dd/MM/yyyy"), active ? "Đang chạy" : "Tạm dừng");
            grid.Rows[rowIndex].Tag = row;
        }
    }

    public class AccountsView : UserControl
    {
        private SFDataGridView grid;
        private SFSearchBox search;
        private DataTable cachedData;

        public AccountsView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadDataFromDatabase();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Tài Khoản & Phân Quyền", "Quản lý thông tin đăng nhập, vai trò truy cập, trạng thái khóa bảo mật và phiên làm việc."));

            FlowLayoutPanel tb = Ui.Toolbar();
            SFButton btnRefresh = new SFButton { Text = "Làm Mới", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Refresh, Width = 120, Height = 38 };
            btnRefresh.Click += (s, e) => LoadDataFromDatabase();
            tb.Controls.Add(btnRefresh);

            SFButton btnLock = new SFButton { Text = "Khóa / Mở Khóa", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Lock, Width = 150, Height = 38 };
            btnLock.Click += (s, e) => ToggleAccountLock();
            tb.Controls.Add(btnLock);

            SFButton btnReset = new SFButton { Text = "Đặt Lại Mật Khẩu", Variant = SFButtonVariant.Primary, IconType = SFIcon.Settings, Width = 175, Height = 38 };
            btnReset.Click += (s, e) => new ChangePasswordForm(true).ShowDialog(this);
            tb.Controls.Add(btnReset);

            search = new SFSearchBox { PlaceholderText = "Tài khoản / họ tên...", Width = 230, Height = 38 };
            search.TextChanged += (s, e) => FilterData();
            tb.Controls.Add(search);
            Controls.Add(tb);
            tb.BringToFront();

            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Label lblGridTitle = new Label { Text = "Danh Sách Tài Khoản Đăng Nhập Hệ Thống (3 Role Phân Quyền)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(lblGridTitle);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("User", "Tên đăng nhập");
            grid.Columns.Add("Role", "Vai trò");
            grid.Columns.Add("Fullname", "Họ và tên");
            grid.Columns.Add("Status", "Trạng thái");
            grid.Columns.Add("LastLogin", "Đăng nhập lần cuối");
            grid.Columns.Add("Source", "Nguồn khởi tạo");

            grid.Columns["User"].Width = 140;
            grid.Columns["Role"].Width = 120;
            grid.Columns["Fullname"].Width = 190;
            grid.Columns["Status"].Width = 110;
            grid.Columns["LastLogin"].Width = 160;
            grid.Columns["Source"].Width = 120;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemLock = cms.Items.Add("Khóa / Mở Khóa Tài Khoản");
            itemLock.Click += (s, e) => ToggleAccountLock();

            var itemReset = cms.Items.Add("Đặt Lại Mật Khẩu");
            itemReset.Click += (s, e) => new ChangePasswordForm(true).ShowDialog(this);

            var itemExport = cms.Items.Add("Xuất Danh Sách Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Danh_Sach_Tai_Khoan", this);

            grid.ContextMenuStrip = cms;

            card.Controls.Add(grid);
            grid.BringToFront();
            Controls.Add(card);
            card.BringToFront();
        }

        private void ToggleAccountLock()
        {
            if (grid.SelectedRows.Count == 0)
            {
                SFToast.Show(this, "Vui lòng chọn tài khoản cần thay đổi trạng thái.", "CHỌN TÀI KHOẢN", SFIcon.AlertCircle, Colors.Coral);
                return;
            }

            string user = grid.SelectedRows[0].Cells["User"].Value?.ToString();
            if (user == "admin")
            {
                SFToast.Show(this, "Không thể khóa tài khoản quản trị viên tối cao admin.", "BẢO VỆ ADMIN", SFIcon.AlertCircle, Colors.Coral);
                return;
            }

            try
            {
                DatabaseManager.ExecuteNonQuery(
                    "UPDATE dbo.TaiKhoan SET DangHoatDong = CASE WHEN DangHoatDong = 1 THEN 0 ELSE 1 END WHERE TenDangNhap = @User",
                    new SqlParameter("@User", user));
                SFToast.Show(this, string.Format("Đã chuyển đổi trạng thái khóa cho tài khoản: {0}", user), "KHÓA TÀI KHOẢN", SFIcon.Check, Colors.Emerald);
                LoadDataFromDatabase();
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "ACCOUNTS", "ToggleAccountLock");
                SFToast.Show(this, "Lỗi cập nhật CSDL: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachTaiKhoan");
                cachedData = dt;
                grid.Rows.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    RenderRows(dt);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "ACCOUNTS", "LoadDataFromDatabase");
                SFToast.Show(this, "Lỗi tải tài khoản: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private void FilterData()
        {
            if (cachedData == null) return;
            string keyword = search.Text != null ? search.Text.Trim().ToLower() : "";
            if (string.IsNullOrEmpty(keyword))
            {
                RenderRows(cachedData);
                return;
            }

            grid.Rows.Clear();
            foreach (DataRow row in cachedData.Rows)
            {
                string u = row["TenDangNhap"]?.ToString() ?? "";
                string f = row["HoTen"]?.ToString() ?? "";
                if (u.ToLower().Contains(keyword) || f.ToLower().Contains(keyword))
                {
                    AddRow(row);
                }
            }
        }

        private void RenderRows(DataTable dt)
        {
            grid.Rows.Clear();
            foreach (DataRow row in dt.Rows)
            {
                AddRow(row);
            }
        }

        private void AddRow(DataRow row)
        {
            string user = row["TenDangNhap"]?.ToString() ?? "";
            string role = row["TenVaiTro"]?.ToString() ?? (row["MaVaiTro"]?.ToString() ?? "Người dùng");
            string hoTen = row["HoTen"]?.ToString() ?? "";
            bool active = row["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(row["DangHoatDong"]);
            string lastLogin = row["LanDangNhapCuoi"] != DBNull.Value ? Convert.ToDateTime(row["LanDangNhapCuoi"]).ToString("dd/MM HH:mm") : "Chưa đăng nhập";
            string nguon = role.Contains("Khách") ? "Khách đăng ký" : (role.Contains("Admin") ? "Hệ thống" : "Nội bộ POS");

            int rowIndex = grid.Rows.Add(user, role, hoTen, active ? "Hoạt động" : "Tạm khóa", lastLogin, nguon);
            grid.Rows[rowIndex].Tag = row;
        }
    }

    public class AdminSettingsView : UserControl
    {
        private SFTextBox txtCenterName;
        private SFTextBox txtHotline;
        private SFTextBox txtOpenTime;
        private SFTextBox txtCloseTime;
        private SFTextBox txtPrefix;
        private SFTextBox txtBankName;
        private SFTextBox txtBin;
        private SFTextBox txtAccountNo;
        private SFTextBox txtAccountHolder;
        private SFTextBox txtServer;
        private SFTextBox txtDbName;

        public AdminSettingsView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadConfigFromDatabase();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Cấu Hình Hệ Thống", "Thông tin đơn vị, giờ vận hành, tài khoản thụ hưởng VietQR và chuỗi kết nối SQL Server."));

            TableLayoutPanel split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // Left
            SFCard left = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(18), Margin = new Padding(0, 0, 10, 0) };
            Label l1 = new Label { Text = "Thông Tin Cơ Sở & Giờ Hoạt Động", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32, UseMnemonic = false };
            left.Controls.Add(l1);

            Panel p1 = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            txtCenterName = AddField(p1, "Tên trung tâm thể thao", "SPORTFIELD - Smart Sports Management", 10);
            txtHotline = AddField(p1, "Hotline hỗ trợ", "1900 6868 - 0988 123 456", 74);
            txtOpenTime = AddField(p1, "Giờ mở cửa", "06:00", 138);
            txtCloseTime = AddField(p1, "Giờ đóng cửa", "23:30", 202);
            txtPrefix = AddField(p1, "Tiền tố nội dung thanh toán", "SPORTFIELD", 266);
            left.Controls.Add(p1);
            p1.BringToFront();
            split.Controls.Add(left, 0, 0);

            // Right
            SFCard right = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(18), Margin = new Padding(10, 0, 0, 0) };
            Label l2 = new Label { Text = "Ngân Hàng, VietQR Napas & Database", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32, UseMnemonic = false };
            right.Controls.Add(l2);

            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, WrapContents = false, Padding = new Padding(0, 6, 0, 0) };
            SFButton btnTest = new SFButton { Text = "Kiểm Tra Kết Nối SQL", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Check, Width = 180, Height = 38 };
            btnTest.Click += (s, e) =>
            {
                var res = DatabaseManager.TestConnection();
                if (res.Success)
                {
                    SFToast.Show(this, string.Format("Kết nối CSDL {0} thành công!\n• Máy chủ: {1}\n• Độ trễ: {2} ms ({3} bảng)\n• DATA: {4}", res.DatabaseName, res.ServerName, res.LatencyMs, res.TableCount, res.DataFilePath), "SQL SERVER TEST", SFIcon.Check, Colors.Emerald);
                }
                else
                {
                    SFToast.Show(this, "Không thể kết nối SQL Server:\n" + res.ErrorMessage, "SQL SERVER LỖI", SFIcon.AlertCircle, Colors.Coral);
                }
            };
            actions.Controls.Add(btnTest);

            SFButton btnSave = new SFButton { Text = "Lưu Cấu Hình", Variant = SFButtonVariant.Primary, IconType = SFIcon.Check, Width = 140, Height = 38 };
            btnSave.Click += (s, e) => SaveConfigToDatabase();
            actions.Controls.Add(btnSave);

            Panel p2 = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            txtBankName = AddField(p2, "Ngân hàng thụ hưởng", "MB Bank (Ngân hàng Quân Đội)", 10);
            txtBin = AddField(p2, "Mã BIN VietQR", "970422", 74);
            txtAccountNo = AddField(p2, "Số tài khoản nhận tiền", "0987654321098", 138);
            txtAccountHolder = AddField(p2, "Chủ tài khoản", "CONG TY TNHH THE THAO SPORTFIELD", 202);
            txtServer = AddField(p2, "Máy chủ SQL Server", @"DESKTOP-B9A50IM\SQLEXPRESS", 266);
            txtDbName = AddField(p2, "Tên cơ sở dữ liệu", "QuanLySanTheThaoDB", 330);

            right.Controls.Add(p2);
            right.Controls.Add(actions);
            actions.BringToFront();
            p2.BringToFront();
            split.Controls.Add(right, 1, 0);

            Controls.Add(split);
            split.BringToFront();
        }

        private void LoadConfigFromDatabase()
        {
            try
            {
                DataTable dtSys = DatabaseManager.ExecuteQuery("SELECT Khoa, GiaTri FROM dbo.CauHinhHeThong");
                if (dtSys != null && dtSys.Rows.Count > 0)
                {
                    foreach (DataRow r in dtSys.Rows)
                    {
                        string k = r["Khoa"]?.ToString() ?? "";
                        string v = r["GiaTri"]?.ToString() ?? "";
                        if (k == "TEN_HE_THONG" || k == "TEN_THUONG_HIEU") txtCenterName.Text = v;
                        else if (k == "GIO_MO_CUA") txtOpenTime.Text = v;
                        else if (k == "GIO_DONG_CUA") txtCloseTime.Text = v;
                    }
                }

                DataTable dtBank = DatabaseManager.ExecuteQuery("SELECT TOP 1 * FROM dbo.TaiKhoanNganHang WHERE DangHoatDong = 1");
                if (dtBank != null && dtBank.Rows.Count > 0)
                {
                    DataRow r = dtBank.Rows[0];
                    if (r["TenNganHang"] != DBNull.Value) txtBankName.Text = r["TenNganHang"].ToString();
                    if (r["BankBIN"] != DBNull.Value) txtBin.Text = r["BankBIN"].ToString();
                    if (r["SoTaiKhoan"] != DBNull.Value) txtAccountNo.Text = r["SoTaiKhoan"].ToString();
                    if (r["ChuTaiKhoan"] != DBNull.Value) txtAccountHolder.Text = r["ChuTaiKhoan"].ToString();
                    if (r["PrefixNoiDung"] != DBNull.Value) txtPrefix.Text = r["PrefixNoiDung"].ToString();
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "SETTINGS", "LoadConfigFromDatabase");
            }
        }

        private void SaveConfigToDatabase()
        {
            try
            {
                DatabaseManager.ExecuteNonQuery(
                    "UPDATE dbo.CauHinhHeThong SET GiaTri = @Val, NgayCapNhat = SYSDATETIME() WHERE Khoa = @Key",
                    new SqlParameter("@Key", "TEN_HE_THONG"),
                    new SqlParameter("@Val", txtCenterName.Text.Trim()));

                DatabaseManager.ExecuteNonQuery(
                    "UPDATE dbo.CauHinhHeThong SET GiaTri = @Val, NgayCapNhat = SYSDATETIME() WHERE Khoa = @Key",
                    new SqlParameter("@Key", "GIO_MO_CUA"),
                    new SqlParameter("@Val", txtOpenTime.Text.Trim()));

                DatabaseManager.ExecuteNonQuery(
                    "UPDATE dbo.CauHinhHeThong SET GiaTri = @Val, NgayCapNhat = SYSDATETIME() WHERE Khoa = @Key",
                    new SqlParameter("@Key", "GIO_DONG_CUA"),
                    new SqlParameter("@Val", txtCloseTime.Text.Trim()));

                DataTable dtCheck = DatabaseManager.ExecuteQuery("SELECT COUNT(*) AS Cnt FROM dbo.TaiKhoanNganHang");
                int cnt = (dtCheck != null && dtCheck.Rows.Count > 0) ? Convert.ToInt32(dtCheck.Rows[0]["Cnt"]) : 0;
                if (cnt > 0)
                {
                    DatabaseManager.ExecuteNonQuery(
                        "UPDATE dbo.TaiKhoanNganHang SET TenNganHang=@Bank, BankBIN=@Bin, SoTaiKhoan=@Acc, ChuTaiKhoan=@Holder, PrefixNoiDung=@Prefix WHERE DangHoatDong=1",
                        new SqlParameter("@Bank", txtBankName.Text.Trim()),
                        new SqlParameter("@Bin", txtBin.Text.Trim()),
                        new SqlParameter("@Acc", txtAccountNo.Text.Trim()),
                        new SqlParameter("@Holder", txtAccountHolder.Text.Trim()),
                        new SqlParameter("@Prefix", txtPrefix.Text.Trim()));
                }
                else
                {
                    DatabaseManager.ExecuteNonQuery(
                        "INSERT INTO dbo.TaiKhoanNganHang (TenNganHang, BankBIN, SoTaiKhoan, ChuTaiKhoan, PrefixNoiDung, DangHoatDong) VALUES (@Bank, @Bin, @Acc, @Holder, @Prefix, 1)",
                        new SqlParameter("@Bank", txtBankName.Text.Trim()),
                        new SqlParameter("@Bin", txtBin.Text.Trim()),
                        new SqlParameter("@Acc", txtAccountNo.Text.Trim()),
                        new SqlParameter("@Holder", txtAccountHolder.Text.Trim()),
                        new SqlParameter("@Prefix", txtPrefix.Text.Trim()));
                }

                SFToast.Show(this, "Đã lưu tham số cấu hình hệ thống vào CSDL SQL Server thành công!", "CẤU HÌNH", SFIcon.Check, Colors.Emerald);
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "SETTINGS", "SaveConfigToDatabase");
                SFToast.Show(this, "Lỗi lưu cấu hình: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private SFTextBox AddField(Control p, string label, string value, int y)
        {
            Label l = new Label { Text = label, Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(4, y), AutoSize = true };
            p.Controls.Add(l);
            SFTextBox t = new SFTextBox { Text = value, Location = new Point(4, y + 20), Width = 380, Height = 42 };
            p.Controls.Add(t);
            return t;
        }
    }

    public class SystemLogsView : UserControl
    {
        private SFDataGridView grid;
        private SFSearchBox search;
        private DataTable cachedData;

        public SystemLogsView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadDataFromDatabase();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Nhật Ký Hệ Thống & Kiểm Toán", "Truy vết chi tiết mọi hoạt động đặt sân, thanh toán, thay đổi cấu hình và bảo mật."));

            FlowLayoutPanel tb = Ui.Toolbar();
            SFButton btnExport = new SFButton { Text = "Xuất Excel", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Printer, Width = 130, Height = 38 };
            btnExport.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Nhat_Ky_He_Thong", this);
            tb.Controls.Add(btnExport);

            SFButton btnRefresh = new SFButton { Text = "Làm Mới", Variant = SFButtonVariant.Primary, IconType = SFIcon.Refresh, Width = 120, Height = 38 };
            btnRefresh.Click += (s, e) => LoadDataFromDatabase();
            tb.Controls.Add(btnRefresh);

            search = new SFSearchBox { PlaceholderText = "Tìm sự kiện, tài khoản, IP...", Width = 260, Height = 38 };
            search.TextChanged += (s, e) => FilterData();
            tb.Controls.Add(search);
            Controls.Add(tb);
            tb.BringToFront();

            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Label lblGridTitle = new Label { Text = "Lịch Sử Truy Vết Hoạt Động (Audit Trail Realtime)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(lblGridTitle);

            grid = new SFDataGridView { Dock = DockStyle.Fill };
            grid.Columns.Add("Time", "Thời gian");
            grid.Columns.Add("User", "Tài khoản");
            grid.Columns.Add("Role", "Vai trò");
            grid.Columns.Add("Event", "Sự kiện");
            grid.Columns.Add("Details", "Chi tiết đối tượng");
            grid.Columns.Add("IP", "Địa chỉ IP");
            grid.Columns.Add("Status", "Trạng thái");

            grid.Columns["Time"].Width = 150;
            grid.Columns["User"].Width = 110;
            grid.Columns["Role"].Width = 110;
            grid.Columns["Event"].Width = 150;
            grid.Columns["Details"].Width = 260;
            grid.Columns["IP"].Width = 120;
            grid.Columns["Status"].Width = 110;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemRefresh = cms.Items.Add("Làm Mới Nhật Ký");
            itemRefresh.Click += (s, e) => LoadDataFromDatabase();

            var itemExp = cms.Items.Add("Xuất Nhật Ký Ra Excel");
            itemExp.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Nhat_Ky_He_Thong", this);

            grid.ContextMenuStrip = cms;

            card.Controls.Add(grid);
            grid.BringToFront();
            Controls.Add(card);
            card.BringToFront();
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachNhatKyHeThong");
                cachedData = dt;
                grid.Rows.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    RenderRows(dt);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "AUDIT_LOGS", "LoadDataFromDatabase");
                SFToast.Show(this, "Lỗi tải nhật ký hệ thống: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private void FilterData()
        {
            if (cachedData == null) return;
            string keyword = search.Text != null ? search.Text.Trim().ToLower() : "";
            if (string.IsNullOrEmpty(keyword))
            {
                RenderRows(cachedData);
                return;
            }

            grid.Rows.Clear();
            foreach (DataRow row in cachedData.Rows)
            {
                string u = row["TenDangNhap"]?.ToString() ?? "";
                string evt = row["HanhDong"]?.ToString() ?? "";
                string det = row["DuLieuMoi"]?.ToString() ?? "";
                if (u.ToLower().Contains(keyword) || evt.ToLower().Contains(keyword) || det.ToLower().Contains(keyword))
                {
                    AddRow(row);
                }
            }
        }

        private void RenderRows(DataTable dt)
        {
            grid.Rows.Clear();
            foreach (DataRow row in dt.Rows)
            {
                AddRow(row);
            }
        }

        private void AddRow(DataRow row)
        {
            DateTime time = row["ThoiGian"] != DBNull.Value ? Convert.ToDateTime(row["ThoiGian"]) : DateTime.Now;
            string u = row["TenDangNhap"]?.ToString() ?? "";
            string role = row["TenVaiTro"]?.ToString() ?? "";
            string evt = row["HanhDong"]?.ToString() ?? (row["Module"]?.ToString() ?? "");
            string details = row["DuLieuMoi"] != DBNull.Value && !string.IsNullOrEmpty(row["DuLieuMoi"].ToString()) 
                ? row["DuLieuMoi"].ToString() 
                : string.Format("Thực hiện trên {0} #{1}", row["BangDuLieu"], row["KhoaDuLieu"]);
            string ip = row["TenMay"]?.ToString() ?? "127.0.0.1";

            int rowIndex = grid.Rows.Add(time.ToString("dd/MM/yyyy HH:mm:ss"), u, role, evt, details, ip, "Thành công");
            grid.Rows[rowIndex].Tag = row;
        }
    }
}
