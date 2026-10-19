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
    public class BookingManagementView : UserControl
    {
        private SFDataGridView grid;
        private Label countLabel;
        private DataTable bookingTable;
        private string statusFilter = "ALL";
        private string searchKeyword = "";
        private SFFilterChip chipAll, chipUsing, chipDeposit, chipWait, chipDone, chipCancel;

        public BookingManagementView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.AppBackground;
            BuildUI();
            LoadBookingsFromDatabase();
        }

        private void BuildUI()
        {
            // Header
            Controls.Add(Ui.Header("Quản lý đơn đặt sân", "Tra cứu toàn diện, xác nhận, check-in vào sân, phụ thu dịch vụ và quyết toán hóa đơn."));

            // Smart Toolbar
            Panel toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Colors.AppBackground,
                Padding = new Padding(0, 0, 0, 14)
            };

            SFSearchBox search = new SFSearchBox
            {
                Placeholder = "Tìm mã đơn, tên khách, số điện thoại...",
                Width = 280,
                Dock = DockStyle.Left
            };
            search.TextChanged += (s, e) =>
            {
                searchKeyword = search.Text.Trim().ToLowerInvariant();
                ApplyFilter();
            };
            toolbarPanel.Controls.Add(search);

            FlowLayoutPanel chips = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.Transparent
            };
            chipAll = new SFFilterChip { Text = "Tất cả", Count = 3, Selected = true, Size = new Size(95, 40) };
            chipUsing = new SFFilterChip { Text = "Đang sử dụng", Count = 1, Size = new Size(125, 40) };
            chipDeposit = new SFFilterChip { Text = "Đã cọc", Count = 1, Size = new Size(90, 40) };
            chipWait = new SFFilterChip { Text = "Chờ duyệt", Count = 0, Size = new Size(100, 40) };
            chipDone = new SFFilterChip { Text = "Hoàn thành", Count = 1, Size = new Size(110, 40) };
            chipCancel = new SFFilterChip { Text = "Đã hủy", Count = 0, Size = new Size(88, 40) };

            chipAll.Click += (s, e) => SetFilter("ALL", chipAll);
            chipUsing.Click += (s, e) => SetFilter("DANG_SU_DUNG", chipUsing);
            chipDeposit.Click += (s, e) => SetFilter("DA_XAC_NHAN", chipDeposit);
            chipWait.Click += (s, e) => SetFilter("CHO_XAC_NHAN", chipWait);
            chipDone.Click += (s, e) => SetFilter("HOAN_THANH", chipDone);
            chipCancel.Click += (s, e) => SetFilter("DA_HUY", chipCancel);

            chips.Controls.Add(chipAll);
            chips.Controls.Add(chipUsing);
            chips.Controls.Add(chipDeposit);
            chips.Controls.Add(chipWait);
            chips.Controls.Add(chipDone);
            chips.Controls.Add(chipCancel);
            toolbarPanel.Controls.Add(chips);

            SFButton btnAdd = new SFButton
            {
                Text = "Đặt Sân Mới (F2)",
                Variant = SFButtonVariant.Primary,
                Width = 160,
                Dock = DockStyle.Right,
                LeadingIcon = SFIcon.Plus
            };
            btnAdd.Click += (s, e) => (FindForm() as MainForm)?.ShowView("Đặt sân");
            toolbarPanel.Controls.Add(btnAdd);

            SFButton btnPay = new SFButton
            {
                Text = "Thu Cọc / QR (F8)",
                Variant = SFButtonVariant.Secondary,
                Width = 150,
                Dock = DockStyle.Right,
                LeadingIcon = SFIcon.QrCode,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnPay.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0 && grid.SelectedRows[0].Cells[0].Value != null)
                {
                    string code = grid.SelectedRows[0].Cells[0].Value.ToString();
                    long id = grid.SelectedRows[0].Tag is long donId ? donId : 0;
                    string kh = grid.SelectedRows[0].Cells[1].Value?.ToString() ?? "";
                    
                    try
                    {
                        var dtDon = DatabaseManager.ExecuteQuery("SELECT TOP 1 * FROM dbo.vw_DonDatSanDayDu WHERE DonDatSanID = @ID OR MaDon = @Ma", 
                            new System.Data.SqlClient.SqlParameter("@ID", id),
                            new System.Data.SqlClient.SqlParameter("@Ma", code));

                        if (dtDon != null && dtDon.Rows.Count > 0)
                        {
                            DataRow dr = dtDon.Rows[0];
                            long realId = Convert.ToInt64(dr["DonDatSanID"]);
                            decimal tong = Convert.ToDecimal(dr["TongTien"]);
                            decimal daTra = Convert.ToDecimal(dr["DaThanhToan"]);
                            decimal conLai = Convert.ToDecimal(dr["ConLai"]);

                            long hoaDonId = 0;
                            var objHd = DatabaseManager.ExecuteScalar("SELECT TOP 1 HoaDonID FROM dbo.HoaDon WHERE DonDatSanID = @ID", new System.Data.SqlClient.SqlParameter("@ID", realId));
                            if (objHd != null && objHd != DBNull.Value)
                            {
                                hoaDonId = Convert.ToInt64(objHd);
                            }
                            else
                            {
                                var inv = new Services.PaymentService().CreateInvoice(realId, Session.AppSession.Current != null ? Session.AppSession.Current.TaiKhoanID : (int?)null);
                                hoaDonId = inv.HoaDonID;
                            }

                            if (hoaDonId > 0)
                            {
                                new QrPaymentForm(tong, daTra, conLai, code, kh, hoaDonId).ShowDialog(this);
                                LoadBookingsFromDatabase();
                            }
                            else
                            {
                                SFToast.Show(this, "Không thể xác định hoặc tạo hóa đơn cho đơn đặt sân này.", "LỖI HÓA ĐƠN", SFIcon.AlertCircle, Colors.Danger);
                            }
                            return;
                        }
                    }
                    catch (Exception exPay)
                    {
                        Helpers.AppLogger.LogError(exPay, "BOOKING_PAY");
                    }
                }
                SFToast.Show(this, "Vui lòng chọn một đơn đặt sân từ danh sách để thực hiện thanh toán.", "CHƯA CHỌN ĐƠN", SFIcon.Info, Colors.Cyan);
            };
            toolbarPanel.Controls.Add(btnPay);

            Controls.Add(toolbarPanel);
            toolbarPanel.BringToFront();

            // Table Card Container
            SFCard tableCard = new SFCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(1)
            };

            grid = new SFDataGridView();
            grid.Columns.Add("C0", "Mã đơn");
            grid.Columns.Add("C1", "Khách hàng");
            grid.Columns.Add("C2", "Số điện thoại");
            grid.Columns.Add("C3", "Sân đấu");
            grid.Columns.Add("C4", "Khung giờ thi đấu");
            grid.Columns.Add("C5", "Tổng tiền");
            grid.Columns.Add("C6", "Đã trả");
            grid.Columns.Add("C7", "Còn lại");
            grid.Columns.Add("C8", "Trạng thái");

            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && grid.Rows[e.RowIndex].Cells[0].Value != null)
                {
                    string code = grid.Rows[e.RowIndex].Cells[0].Value.ToString();
                    new BookingDetailForm(code).ShowDialog(this);
                }
            };

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Xem chi tiết đơn", null, (s, e) => {
                if (grid.CurrentRow != null && grid.CurrentRow.Cells[0].Value != null)
                    new BookingDetailForm(grid.CurrentRow.Cells[0].Value.ToString()).ShowDialog(this);
            });
            menu.Items.Add("Nhận sân (Check-in)", null, (s, e) => {
                if (grid.CurrentRow != null && grid.CurrentRow.Tag != null)
                {
                    long id = Convert.ToInt64(grid.CurrentRow.Tag);
                    try {
                        new Services.BookingService().CheckIn(id, Session.AppSession.Current.TaiKhoanID);
                        SFToast.Show(this, "Check-in thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                        LoadBookingsFromDatabase();
                    } catch (Exception ex) {
                        SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI CHECK-IN", SFIcon.AlertCircle, Colors.Danger);
                    }
                }
            });
            menu.Items.Add("Hoàn tất trả sân & Quyết toán", null, (s, e) => {
                if (grid.CurrentRow != null && grid.CurrentRow.Tag != null)
                {
                    long id = Convert.ToInt64(grid.CurrentRow.Tag);
                    try {
                        new Services.BookingService().CompleteBooking(id, Session.AppSession.Current.TaiKhoanID);
                        SFToast.Show(this, "Đã hoàn tất trả sân!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                        LoadBookingsFromDatabase();
                    } catch (Exception ex) {
                        SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI QUYẾT TOÁN", SFIcon.AlertCircle, Colors.Danger);
                    }
                }
            });
            menu.Items.Add("Hủy đơn & Hoàn tiền", null, (s, e) => {
                if (grid.CurrentRow != null && grid.CurrentRow.Tag != null)
                {
                    long id = Convert.ToInt64(grid.CurrentRow.Tag);
                    string code = grid.CurrentRow.Cells[0].Value?.ToString() ?? "";
                    string cust = grid.CurrentRow.Cells[1].Value?.ToString() ?? "";
                    string paidStr = grid.CurrentRow.Cells[6].Value?.ToString() ?? "0";
                    paidStr = paidStr.Replace("đ", "").Replace(".", "").Replace(",", "").Trim();
                    decimal.TryParse(paidStr, out decimal paid);
                    if (new CancelRefundForm(id, code, cust, paid).ShowDialog(this) == DialogResult.OK)
                        LoadBookingsFromDatabase();
                }
            });
            menu.Items.Add("Thanh toán VietQR Napas", null, (s, e) => {
                if (grid.CurrentRow != null && grid.CurrentRow.Cells[0].Value != null)
                {
                    string code = grid.CurrentRow.Cells[0].Value.ToString();
                    string cust = grid.CurrentRow.Cells[1].Value?.ToString() ?? "";
                    string remStr = grid.CurrentRow.Cells[7].Value?.ToString() ?? "0";
                    remStr = remStr.Replace("đ", "").Replace(".", "").Replace(",", "").Trim();
                    decimal.TryParse(remStr, out decimal rem);
                    new QrPaymentForm(rem > 0 ? rem : 100000, code, cust, 0).ShowDialog(this);
                }
            });
            menu.Items.Add("-");
            menu.Items.Add("Xuất danh sách ra Excel (CSV)", null, (s, e) => {
                Helpers.ExportHelper.ExportToExcel(grid, "DanhSachDonDatSan.csv", this);
            });
            grid.ContextMenuStrip = menu;

            tableCard.Controls.Add(grid);
            grid.BringToFront();

            // Pagination footer
            Panel pager = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Colors.SurfaceElevated,
                Padding = new Padding(16, 8, 16, 8)
            };
            pager.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Colors.Border, 1.1f)) e.Graphics.DrawLine(p, 0, 0, pager.Width, 0);
            };

            countLabel = new Label
            {
                Text = "Hiển thị 3 đơn đặt sân từ CSDL SQL Server",
                Font = Typography.Secondary,
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pager.Controls.Add(countLabel);

            FlowLayoutPanel pageButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            pageButtons.Controls.Add(new SFButton { Text = "<", Variant = SFButtonVariant.Secondary, Size = new Size(34, 30) });
            pageButtons.Controls.Add(new SFButton { Text = "1", Variant = SFButtonVariant.Primary, Size = new Size(34, 30) });
            pageButtons.Controls.Add(new SFButton { Text = "2", Variant = SFButtonVariant.Secondary, Size = new Size(34, 30) });
            pageButtons.Controls.Add(new SFButton { Text = "3", Variant = SFButtonVariant.Secondary, Size = new Size(34, 30) });
            pageButtons.Controls.Add(new SFButton { Text = ">", Variant = SFButtonVariant.Secondary, Size = new Size(34, 30) });
            pager.Controls.Add(pageButtons);

            tableCard.Controls.Add(pager);
            Controls.Add(tableCard);
            tableCard.BringToFront();
        }

        private void SetFilter(string status, SFFilterChip active)
        {
            statusFilter = status;
            chipAll.Selected = (active == chipAll);
            chipUsing.Selected = (active == chipUsing);
            chipDeposit.Selected = (active == chipDeposit);
            chipWait.Selected = (active == chipWait);
            chipDone.Selected = (active == chipDone);
            chipCancel.Selected = (active == chipCancel);
            ApplyFilter();
        }

        public void LoadBookingsFromDatabase()
        {
            try
            {
                bookingTable = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachDonDatSan");
                if (bookingTable != null && bookingTable.Rows.Count > 0)
                {
                    int total = bookingTable.Rows.Count;
                    int u = 0, dep = 0, w = 0, done = 0, c = 0;

                    foreach (DataRow r in bookingTable.Rows)
                    {
                        string st = r["TrangThaiDon"]?.ToString() ?? "";
                        if (st == "DANG_SU_DUNG") u++;
                        else if (st == "DA_XAC_NHAN") dep++;
                        else if (st == "CHO_XAC_NHAN") w++;
                        else if (st == "HOAN_THANH") done++;
                        else if (st == "DA_HUY") c++;
                    }

                    chipAll.Count = total;
                    chipUsing.Count = u;
                    chipDeposit.Count = dep;
                    chipWait.Count = w;
                    chipDone.Count = done;
                    chipCancel.Count = c;

                    ApplyFilter();
                    return;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_BOOKINGS");
            }

            grid.Rows.Clear();
            countLabel.Text = "Không có dữ liệu đơn đặt sân.";
        }

        private void ApplyFilter()
        {
            grid.Rows.Clear();

            if (bookingTable == null || bookingTable.Rows.Count == 0)
            {
                countLabel.Text = "Không có dữ liệu đơn đặt sân.";
                return;
            }

            int displayed = 0;

            foreach (DataRow r in bookingTable.Rows)
            {
                long id = r.Table.Columns.Contains("DonDatSanID") && r["DonDatSanID"] != DBNull.Value ? Convert.ToInt64(r["DonDatSanID"]) : 0;
                string code = r["MaDon"]?.ToString() ?? "";
                string customer = r["HoTen"]?.ToString() ?? "";
                string phone = r["SoDienThoai"]?.ToString() ?? "";
                string court = r["DanhSachSan"]?.ToString() ?? "Sân thi đấu";
                string stCode = r["TrangThaiDon"]?.ToString() ?? "CHO_XAC_NHAN";

                string timeText = "20:00 - 21:30";
                if (r["BatDau"] != DBNull.Value && r["KetThuc"] != DBNull.Value)
                {
                    DateTime b = Convert.ToDateTime(r["BatDau"]);
                    DateTime k = Convert.ToDateTime(r["KetThuc"]);
                    timeText = string.Format("{0:HH:mm} - {1:HH:mm}", b, k);
                }

                decimal total = r["TongTien"] != DBNull.Value ? Convert.ToDecimal(r["TongTien"]) : 0;
                decimal paid = r["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(r["DaThanhToan"]) : 0;
                decimal rem = r["ConLai"] != DBNull.Value ? Convert.ToDecimal(r["ConLai"]) : (total - paid);

                if (statusFilter != "ALL" && !stCode.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    bool match = code.ToLowerInvariant().Contains(searchKeyword) ||
                                 customer.ToLowerInvariant().Contains(searchKeyword) ||
                                 phone.ToLowerInvariant().Contains(searchKeyword) ||
                                 court.ToLowerInvariant().Contains(searchKeyword);
                    if (!match) continue;
                }

                string stText = stCode == "DANG_SU_DUNG" ? "Đang sử dụng" :
                                (stCode == "DA_XAC_NHAN" ? "Đã cọc" :
                                (stCode == "CHO_XAC_NHAN" ? "Chờ duyệt" :
                                (stCode == "HOAN_THANH" ? "Hoàn thành" : "Đã hủy")));

                int rIdx = grid.Rows.Add(new[] {
                    code,
                    customer,
                    phone,
                    court,
                    timeText,
                    total.ToString("#,##0") + "đ",
                    paid.ToString("#,##0") + "đ",
                    rem.ToString("#,##0") + "đ",
                    stText
                });
                grid.Rows[rIdx].Tag = id;
                displayed++;
            }

            countLabel.Text = string.Format("Hiển thị {0} trong tổng số {1} đơn đặt sân (SQL Server Live)", displayed, bookingTable.Rows.Count);
        }
    }
}
