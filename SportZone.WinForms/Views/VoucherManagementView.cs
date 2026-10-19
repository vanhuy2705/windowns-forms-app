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
    public class VoucherManagementView : UserControl
    {
        private SFSearchBox searchBox;
        private SFDataGridView grid;
        private TableLayoutPanel kpiPanel;
        private FlowLayoutPanel ticketRow;
        private DataTable voucherTable;
        private string searchKeyword = "";

        public VoucherManagementView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadVouchersFromDatabase();
        }

        private void BuildUI()
        {
            // 1. Header
            Controls.Add(Ui.Header("Voucher, Khuyến Mãi & Chiến Dịch Ưu Đãi", "Quản lý mã giảm giá, điều kiện kích hoạt, nhóm khách hàng áp dụng và hiệu quả thúc đẩy doanh thu."));

            // 2. Toolbar
            FlowLayoutPanel tb = Ui.Toolbar();

            SFButton btnAdd = new SFButton
            {
                Text = "Thêm Mã Mới (F2)",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Plus,
                Width = 180,
                Height = 38
            };
            btnAdd.Click += (s, e) => OpenAddVoucherDialog();
            tb.Controls.Add(btnAdd);

            SFButton btnBatch = new SFButton
            {
                Text = "Phát Hành Hàng Loạt",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Ticket,
                Width = 175,
                Height = 38
            };
            btnBatch.Click += (s, e) =>
            {
                using (var dlg = new BatchVoucherIssueForm())
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadVouchersFromDatabase();
                    }
                }
            };
            tb.Controls.Add(btnBatch);

            SFButton btnReport = new SFButton
            {
                Text = "Báo Cáo Hiệu Quả",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Chart,
                Width = 160,
                Height = 38
            };
            btnReport.Click += (s, e) =>
            {
                using (var dlg = new VoucherReportForm())
                {
                    dlg.ShowDialog(this);
                }
            };
            tb.Controls.Add(btnReport);

            searchBox = new SFSearchBox
            {
                PlaceholderText = "Mã voucher / tên chiến dịch...",
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

            // 3. Metric bar
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
            kpiPanel.Controls.Add(new SFMetricCard("ĐANG CHẠY", "—", "Chiến dịch kích hoạt", SFIcon.Ticket, Colors.Coral), 0, 0);
            kpiPanel.Controls.Add(new SFMetricCard("LƯỢT ÁP MÃ", "—", "Khách đã sử dụng", SFIcon.Check, Colors.Purple), 1, 0);
            kpiPanel.Controls.Add(new SFMetricCard("TỔNG CHIẾN DỊCH", "—", "Giá trị giảm trực tiếp", SFIcon.Dollar, Colors.Amber), 2, 0);
            kpiPanel.Controls.Add(new SFMetricCard("HIỆU SUẤT ROI", "—", "Doanh thu mang lại", SFIcon.Chart, Colors.Emerald), 3, 0);

            Controls.Add(kpiPanel);
            kpiPanel.BringToFront();

            // 4. Ticket Showcase Panel (3 voucher cards)
            ticketRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 148,
                BackColor = Colors.Canvas,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 0, 0, 12)
            };

            SFVoucherCard v1 = new SFVoucherCard("SPORT10", "-10%", "Giảm 10% tối đa 100K cho đơn từ 300K", "HSD: 31/10/2026", Colors.Emerald);
            v1.Width = 340;
            v1.Margin = new Padding(0, 0, 14, 0);
            ticketRow.Controls.Add(v1);

            SFVoucherCard v2 = new SFVoucherCard("WELCOME50", "-50K", "Giảm 50.000đ trực tiếp cho khách mới", "HSD: 31/12/2026", Colors.ElectricBlue);
            v2.Width = 340;
            v2.Margin = new Padding(0, 0, 14, 0);
            ticketRow.Controls.Add(v2);

            SFVoucherCard v3 = new SFVoucherCard("VIP20", "-20%", "Ưu đãi VIP Gold 20% toàn bộ dịch vụ", "HSD: 30/11/2026", Colors.Purple);
            v3.Width = 340;
            v3.Margin = new Padding(0, 0, 14, 0);
            ticketRow.Controls.Add(v3);

            Controls.Add(ticketRow);
            ticketRow.BringToFront();

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
                Text = "Danh Sách Chiến Dịch Mã Ưu Đãi (Nhấp đúp để chỉnh sửa điều kiện)",
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
            grid.Columns.Add("Code", "Mã code");
            grid.Columns.Add("Name", "Tên chiến dịch");
            grid.Columns.Add("Type", "Hình thức");
            grid.Columns.Add("Value", "Mức giảm");
            grid.Columns.Add("MinOrder", "Đơn tối thiểu");
            grid.Columns.Add("Validity", "Thời hạn áp dụng");
            grid.Columns.Add("Usage", "Đã dùng / Giới hạn");
            grid.Columns.Add("Status", "Trạng thái");

            grid.Columns["Code"].Width = 95;
            grid.Columns["Name"].Width = 200;
            grid.Columns["Type"].Width = 100;
            grid.Columns["Value"].Width = 125;
            grid.Columns["MinOrder"].Width = 110;
            grid.Columns["Validity"].Width = 135;
            grid.Columns["Usage"].Width = 135;
            grid.Columns["Status"].Width = 110;

            grid.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Chỉnh sửa Voucher", null, (s, e) => {
                if (grid.CurrentRow != null && grid.CurrentRow.Tag != null)
                {
                    int vId = Convert.ToInt32(grid.CurrentRow.Tag);
                    if (new VoucherEditForm(vId).ShowDialog(this) == DialogResult.OK)
                        LoadVouchersFromDatabase();
                }
            });
            menu.Items.Add("Thêm Voucher mới", null, (s, e) => {
                if (new VoucherEditForm().ShowDialog(this) == DialogResult.OK)
                    LoadVouchersFromDatabase();
            });
            menu.Items.Add("-");
            menu.Items.Add("Xuất danh sách Voucher ra Excel (CSV)", null, (s, e) => {
                Helpers.ExportHelper.ExportToExcel(grid, "DanhSachVoucher.csv", this);
            });
            grid.ContextMenuStrip = menu;

            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && grid.Rows[e.RowIndex].Tag != null)
                {
                    int vId = Convert.ToInt32(grid.Rows[e.RowIndex].Tag);
                    if (new VoucherEditForm(vId).ShowDialog(this) == DialogResult.OK)
                        LoadVouchersFromDatabase();
                }
            };

            card.Controls.Add(grid);
            grid.BringToFront();

            Controls.Add(card);
            card.BringToFront();
        }

        public void LoadVouchersFromDatabase()
        {
            try
            {
                voucherTable = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachVoucher");
                if (voucherTable != null && voucherTable.Rows.Count > 0)
                {
                    int total = voucherTable.Rows.Count;
                    int active = 0;
                    int totalUsed = 0;

                    foreach (DataRow r in voucherTable.Rows)
                    {
                        if (r["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(r["DangHoatDong"])) active++;
                        if (r["SoLuongDaDung"] != DBNull.Value) totalUsed += Convert.ToInt32(r["SoLuongDaDung"]);
                    }

                    string roiStr = "—";
                    try
                    {
                        DataTable dtRoi = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_BaoCaoHieuQuaVoucher @TuNgay = '2020-01-01', @DenNgay = '2030-12-31'");
                        if (dtRoi != null && dtRoi.Rows.Count > 0)
                        {
                            decimal sumGiam = 0, sumDoanhThu = 0;
                            foreach (DataRow drRoi in dtRoi.Rows)
                            {
                                if (drRoi.Table.Columns.Contains("TongTienGiam") && drRoi["TongTienGiam"] != DBNull.Value)
                                    sumGiam += Convert.ToDecimal(drRoi["TongTienGiam"]);
                                if (drRoi.Table.Columns.Contains("DoanhThuKemVoucher") && drRoi["DoanhThuKemVoucher"] != DBNull.Value)
                                    sumDoanhThu += Convert.ToDecimal(drRoi["DoanhThuKemVoucher"]);
                            }
                            if (sumGiam > 0)
                            {
                                decimal roi = sumDoanhThu / sumGiam;
                                roiStr = string.Format("x{0:0.#} lần", roi);
                            }
                        }
                    }
                    catch (Exception exRoi)
                    {
                        Helpers.AppLogger.LogWarning("sp_UI_BaoCaoHieuQuaVoucher failed: " + exRoi.Message);
                    }

                    kpiPanel.Controls.Clear();
                    kpiPanel.Controls.Add(new SFMetricCard("ĐANG CHẠY", active.ToString(), "Chiến dịch kích hoạt", SFIcon.Ticket, Colors.Coral, "CSDL SQL Live"), 0, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("LƯỢT ÁP MÃ", totalUsed.ToString(), "Lượt đã áp dụng", SFIcon.Check, Colors.Purple, "Khách hàng thực"), 1, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("TỔNG CHIẾN DỊCH", total.ToString(), "Mã lưu hành", SFIcon.Dollar, Colors.Amber, "Hệ thống SQL"), 2, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("HIỆU SUẤT ROI", roiStr, "Doanh thu mang lại", SFIcon.Chart, Colors.Emerald, "Thúc đẩy booking"), 3, 0);

                    // Rebuild showcase ticket cards from live data
                    ticketRow.Controls.Clear();
                    Color[] colors = new[] { Colors.Emerald, Colors.ElectricBlue, Colors.Purple, Colors.Coral };
                    int idx = 0;
                    foreach (DataRow r in voucherTable.Rows)
                    {
                        string c = r["MaVoucher"]?.ToString() ?? "";
                        string lg = r["LoaiGiam"]?.ToString() ?? "";
                        decimal v = r["GiaTriGiam"] != DBNull.Value ? Convert.ToDecimal(r["GiaTriGiam"]) : 0;
                        string vStr = lg.Contains("TRAM") ? ("-" + v.ToString("0.#") + "%") : ("-" + (v / 1000m).ToString("0.#") + "K");
                        string d = (r.Table.Columns.Contains("MoTa") && r["MoTa"] != DBNull.Value && !string.IsNullOrEmpty(r["MoTa"].ToString())) 
                            ? r["MoTa"].ToString() 
                            : (r["TenVoucher"]?.ToString() ?? "");
                        string hsd = r["NgayKetThuc"] != DBNull.Value ? ("HSD: " + Convert.ToDateTime(r["NgayKetThuc"]).ToString("dd/MM/yyyy")) : "Không thời hạn";

                        SFVoucherCard vc = new SFVoucherCard(c, vStr, d, hsd, colors[idx % colors.Length]);
                        vc.Width = 340;
                        vc.Margin = new Padding(0, 0, 14, 0);
                        ticketRow.Controls.Add(vc);
                        idx++;
                        if (idx >= 6) break;
                    }

                    ApplyFilter();
                    return;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_VOUCHERS");
            }

            grid.Rows.Clear();
            ticketRow.Controls.Clear();
        }

        private void ApplyFilter()
        {
            grid.Rows.Clear();

            if (voucherTable == null || voucherTable.Rows.Count == 0)
            {
                return;
            }

            foreach (DataRow r in voucherTable.Rows)
            {
                int vId = r.Table.Columns.Contains("VoucherID") && r["VoucherID"] != DBNull.Value ? Convert.ToInt32(r["VoucherID"]) : 0;
                string code = r["MaVoucher"]?.ToString() ?? "";
                string name = r["TenVoucher"]?.ToString() ?? "";
                string loai = r["LoaiGiam"]?.ToString() ?? "";
                decimal giaTri = r["GiaTriGiam"] != DBNull.Value ? Convert.ToDecimal(r["GiaTriGiam"]) : 0;
                decimal minOrder = r["DonToiThieu"] != DBNull.Value ? Convert.ToDecimal(r["DonToiThieu"]) : 0;
                string start = r["NgayBatDau"] != DBNull.Value ? Convert.ToDateTime(r["NgayBatDau"]).ToString("dd/MM") : "";
                string end = r["NgayKetThuc"] != DBNull.Value ? Convert.ToDateTime(r["NgayKetThuc"]).ToString("dd/MM") : "";
                int used = r["SoLuongDaDung"] != DBNull.Value ? Convert.ToInt32(r["SoLuongDaDung"]) : 0;
                int total = r["SoLuongPhatHanh"] != DBNull.Value ? Convert.ToInt32(r["SoLuongPhatHanh"]) : 0;
                bool active = r["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(r["DangHoatDong"]);

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    bool match = code.ToLowerInvariant().Contains(searchKeyword) ||
                                 name.ToLowerInvariant().Contains(searchKeyword);
                    if (!match) continue;
                }

                string typeText = loai.Contains("TRAM") ? "Phần trăm" : "Cố định";
                string valText = loai.Contains("TRAM") ? (giaTri.ToString("0.#") + "%") : (giaTri.ToString("#,##0") + "đ");
                string usageText = string.Format("{0} / {1}", used, total > 0 ? total.ToString() : "∞");

                int rIdx = grid.Rows.Add(
                    code,
                    name,
                    typeText,
                    valText,
                    minOrder > 0 ? (minOrder.ToString("#,##0") + "đ") : "0đ",
                    start + " - " + end,
                    usageText,
                    active ? "Đang chạy" : "Tạm dừng"
                );
                grid.Rows[rIdx].Tag = vId;
            }
        }

        private void OpenAddVoucherDialog()
        {
            if (new VoucherEditForm().ShowDialog(this) == DialogResult.OK)
            {
                LoadVouchersFromDatabase();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F2)
            {
                OpenAddVoucherDialog();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
