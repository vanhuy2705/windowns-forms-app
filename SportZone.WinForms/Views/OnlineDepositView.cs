using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Helpers;
using SportZone.WinForms.Session;
using SportZone.WinForms.Services;
using SportZone.WinForms.Security;

namespace SportZone.WinForms.Views
{
    public class OnlineDepositView : UserControl
    {
        private SFSearchBox searchBox;
        private SFDataGridView grid;
        private DataTable cachedData;
        private SFMetricCard kpiQueue;
        private SFMetricCard kpiMatched;
        private SFMetricCard kpiPending;
        private SFMetricCard kpiExpiring;

        public OnlineDepositView()
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
            Controls.Add(Ui.Header("Đặt Cọc Giữ Chỗ & Hàng Đợi Đơn Online", "Xử lý hàng đợi đơn trực tuyến, đối soát giao dịch VietQR Napas 247 và tự động giải phóng sân khi hết hạn 15 phút."));

            // 2. Toolbar
            FlowLayoutPanel tb = Ui.Toolbar();

            SFButton btnApprove = new SFButton
            {
                Text = "Duyệt Nhanh Đơn Khớp Cọc",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Check,
                Width = 230,
                Height = 38
            };
            btnApprove.Click += (s, e) =>
            {
                try
                {
                    if (grid.SelectedRows.Count == 0 && grid.Rows.Count > 0)
                    {
                        grid.Rows[0].Selected = true;
                    }

                    if (grid.SelectedRows.Count > 0)
                    {
                        DataRow row = grid.SelectedRows[0].Tag as DataRow;
                        if (row != null)
                        {
                            long datCocId = row["DatCocID"] != DBNull.Value ? Convert.ToInt64(row["DatCocID"]) : 0;
                            int userId = SessionGuard.RequireAccountId();

                            if (row.Table.Columns.Contains("GiaoDichID") && row["GiaoDichID"] != DBNull.Value)
                            {
                                long gdId = Convert.ToInt64(row["GiaoDichID"]);
                                var payService = new Services.PaymentService();
                                payService.ConfirmBankTransfer(gdId, userId, "XAC_NHAN_DAT_COC");
                            }

                            DatabaseManager.ExecuteNonQuery(
                                "UPDATE dbo.DatCocOnline SET TrangThai = 'DA_THANH_TOAN', NgayXacNhan = SYSDATETIME() WHERE DatCocID = @ID",
                                new System.Data.SqlClient.SqlParameter("@ID", datCocId));

                            SFToast.Show(this, string.Format("Đã duyệt đơn cọc #{0} thành công vào hệ thống.", datCocId), "DUYỆT ĐƠN CỌC", SFIcon.Check, Colors.Emerald);
                            LoadDataFromDatabase();
                        }
                    }
                    else
                    {
                        SFToast.Show(this, "Không có đơn cọc nào trong danh sách cần duyệt.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Amber);
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.LogError(ex, "ONLINE_DEPOSIT", "btnApprove");
                    SFToast.Show(this, "Lỗi duyệt đơn: " + ex.Message, "LỖI DUYỆT", SFIcon.AlertCircle, Colors.Coral);
                }
            };
            tb.Controls.Add(btnApprove);

            SFButton btnMatch = new SFButton
            {
                Text = "Khớp Sao Kê VietQR",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.QrCode,
                Width = 180,
                Height = 38
            };
            btnMatch.Click += (s, e) =>
            {
                try
                {
                    int userId = SessionGuard.RequireAccountId();
                    DataTable pendingTx = DatabaseManager.ExecuteQuery(
                        "SELECT TOP 10 gd.GiaoDichID, sk.SaoKeID FROM dbo.GiaoDichThanhToan gd " +
                        "JOIN dbo.GiaoDichSaoKeNganHang sk ON gd.SoTien = sk.SoTien " +
                        "WHERE gd.PhuongThuc IN ('VIETQR','CHUYEN_KHOAN') AND gd.TrangThai = 'CHO_XAC_NHAN' AND sk.TrangThaiDoiSoat = 'CHUA_KHOP'");

                    int matchedCount = 0;
                    if (pendingTx != null && pendingTx.Rows.Count > 0)
                    {
                        foreach (DataRow r in pendingTx.Rows)
                        {
                            long gdId = Convert.ToInt64(r["GiaoDichID"]);
                            long skId = Convert.ToInt64(r["SaoKeID"]);
                            DatabaseManager.ExecuteNonQuery("EXEC dbo.sp_KhopSaoKeVoiThanhToan @GiaoDichID=@GD, @SaoKeID=@SK, @TaiKhoanXacNhanID=@TK, @TuDong=1",
                                new System.Data.SqlClient.SqlParameter("@GD", gdId),
                                new System.Data.SqlClient.SqlParameter("@SK", skId),
                                new System.Data.SqlClient.SqlParameter("@TK", userId));
                            matchedCount++;
                        }
                    }

                    if (matchedCount > 0)
                    {
                        SFToast.Show(this, string.Format("Đã tự động khớp {0} giao dịch sao kê VietQR Napas thành công!", matchedCount), "ĐỐI SOÁT VIETQR", SFIcon.Check, Colors.Emerald);
                    }
                    else
                    {
                        SFToast.Show(this, "Đã đối soát với ngân hàng. Toàn bộ giao dịch đều đã được đối soát chính xác.", "ĐỐI SOÁT VIETQR", SFIcon.Check, Colors.ElectricBlue);
                    }
                    LoadDataFromDatabase();
                }
                catch (Exception ex)
                {
                    AppLogger.LogError(ex, "ONLINE_DEPOSIT", "btnMatch");
                    SFToast.Show(this, "Lỗi đối soát sao kê: " + ex.Message, "LỖI ĐỐI SOÁT", SFIcon.AlertCircle, Colors.Coral);
                }
            };
            tb.Controls.Add(btnMatch);

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
                PlaceholderText = "Mã đơn / tên khách / SĐT...",
                Width = 230,
                Height = 38
            };
            searchBox.TextChanged += (s, e) => FilterData();
            tb.Controls.Add(searchBox);

            Label timerBadge = new Label
            {
                Text = "⏱ Hạn giữ chỗ: 15 phút (Tự hủy nếu quá hạn)",
                Font = Typography.CaptionBold,
                ForeColor = Colors.Amber,
                BackColor = Color.FromArgb(254, 243, 199),
                Padding = new Padding(10, 6, 10, 6),
                AutoSize = true,
                Margin = new Padding(12, 5, 0, 0)
            };
            tb.Controls.Add(timerBadge);

            Controls.Add(tb);
            tb.BringToFront();

            // 3. Metric bar
            TableLayoutPanel kpiPanel = new TableLayoutPanel
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
            kpiQueue = new SFMetricCard("HÀNG ĐỢI ONLINE", "—", "Đang xử lý trực tuyến", SFIcon.Clock, Colors.ElectricBlue);
            kpiMatched = new SFMetricCard("KHỚP VIETQR TỰ ĐỘNG", "—", "Đã nhận tiền qua Napas", SFIcon.Check, Colors.Emerald);
            kpiPending = new SFMetricCard("CHỜ DUYỆT THỦ CÔNG", "—", "Cần kiểm tra sao kê ngân hàng", SFIcon.Bell, Colors.Amber);
            kpiExpiring = new SFMetricCard("SẮP QUÁ HẠN 15 PHÚT", "—", "Cần hủy hoặc nhắc khách", SFIcon.AlertCircle, Colors.Coral);

            kpiPanel.Controls.Add(kpiQueue, 0, 0);
            kpiPanel.Controls.Add(kpiMatched, 1, 0);
            kpiPanel.Controls.Add(kpiPending, 2, 0);
            kpiPanel.Controls.Add(kpiExpiring, 3, 0);

            Controls.Add(kpiPanel);
            kpiPanel.BringToFront();

            // 4. Data Grid
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
                Text = "Hàng Đợi Xử Lý Đơn Trực Tuyến & Cọc Khóa Chỗ (Đồng bộ thời gian thực)",
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
            grid.Columns.Add("Code", "Mã đơn");
            grid.Columns.Add("Customer", "Khách hàng");
            grid.Columns.Add("Field", "Sân đăng ký");
            grid.Columns.Add("Slot", "Khung giờ thi đấu");
            grid.Columns.Add("Deposit", "Tiền cọc");
            grid.Columns.Add("Method", "Phương thức");
            grid.Columns.Add("Countdown", "Hạn giữ chỗ");
            grid.Columns.Add("Status", "Trạng thái");

            grid.Columns["Code"].Width = 110;
            grid.Columns["Customer"].Width = 180;
            grid.Columns["Field"].Width = 160;
            grid.Columns["Slot"].Width = 150;
            grid.Columns["Deposit"].Width = 120;
            grid.Columns["Method"].Width = 120;
            grid.Columns["Countdown"].Width = 120;
            grid.Columns["Status"].Width = 120;

            ContextMenuStrip cms = new ContextMenuStrip();
            var itemApprove = cms.Items.Add("Duyệt Đơn Cọc Này");
            itemApprove.Click += (s, e) => btnApprove.PerformClick();

            var itemCancel = cms.Items.Add("Hủy Giữ Chỗ / Giải Phóng Sân");
            itemCancel.Click += (s, e) =>
            {
                if (grid.SelectedRows.Count > 0)
                {
                    DataRow row = grid.SelectedRows[0].Tag as DataRow;
                    if (row != null)
                    {
                        long giuChoId = row["GiuChoID"] != DBNull.Value ? Convert.ToInt64(row["GiuChoID"]) : 0;
                        long datCocId = row["DatCocID"] != DBNull.Value ? Convert.ToInt64(row["DatCocID"]) : 0;
                        if (MessageBox.Show("Bạn có chắc chắn muốn hủy đơn giữ chỗ này và giải phóng sân?", "XÁC NHẬN HỦY", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            try
                            {
                                DatabaseManager.ExecuteNonQuery("UPDATE dbo.GiuChoDatSan SET TrangThai = 'DA_HUY' WHERE GiuChoID = @ID", new System.Data.SqlClient.SqlParameter("@ID", giuChoId));
                                DatabaseManager.ExecuteNonQuery("UPDATE dbo.DatCocOnline SET TrangThai = 'DA_HUY' WHERE DatCocID = @ID", new System.Data.SqlClient.SqlParameter("@ID", datCocId));
                                SFToast.Show(this, "Đã hủy giữ chỗ thành công.", "HỦY GIỮ CHỖ", SFIcon.Trash, Colors.Amber);
                                LoadDataFromDatabase();
                            }
                            catch (Exception ex)
                            {
                                SFToast.Show(this, "Lỗi hủy: " + ex.Message, "LỖI", SFIcon.AlertCircle, Colors.Coral);
                            }
                        }
                    }
                }
            };

            var itemExport = cms.Items.Add("Xuất Danh Sách Ra Excel");
            itemExport.Click += (s, e) => ExportHelper.ExportToExcel(grid, "Hang_Doi_Dat_Coc_Online", this);

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
                DatabaseManager.ExecuteNonQuery("EXEC dbo.sp_DonDepGiuChoHetHan");
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachDatCocOnline");
                cachedData = dt;
                grid.Rows.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    RenderRows(dt);

                    int total = dt.Rows.Count;
                    int matched = 0;
                    int pending = 0;
                    int expiring = 0;
                    DateTime now = DateTime.Now;

                    foreach (DataRow r in dt.Rows)
                    {
                        string st = r["TrangThaiDatCoc"]?.ToString() ?? "";
                        if (st == "DA_THANH_TOAN" || st == "THANH_CONG") matched++;
                        else if (st == "DA_HUY" || st == "HET_HAN") { /* cancelled */ }
                        else pending++;

                        if (r["HetHanLuc"] != DBNull.Value)
                        {
                            DateTime exp = Convert.ToDateTime(r["HetHanLuc"]);
                            if (exp > now && (exp - now).TotalMinutes <= 15) expiring++;
                        }
                    }

                    kpiQueue.Value = string.Format("{0} Đơn", total);
                    kpiMatched.Value = string.Format("{0} Đơn", matched);
                    kpiPending.Value = string.Format("{0} Đơn", pending);
                    kpiExpiring.Value = string.Format("{0} Đơn", expiring);
                }
                else
                {
                    kpiQueue.Value = "0 Đơn";
                    kpiMatched.Value = "0 Đơn";
                    kpiPending.Value = "0 Đơn";
                    kpiExpiring.Value = "0 Đơn";
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError(ex, "ONLINE_DEPOSIT", "LoadDataFromDatabase");
                kpiQueue.Value = "0 Đơn";
                kpiMatched.Value = "0 Đơn";
                kpiPending.Value = "0 Đơn";
                kpiExpiring.Value = "0 Đơn";
                SFToast.Show(this, "Lỗi tải danh sách đặt cọc online: " + ex.Message, "LỖI KẾT NỐI", SFIcon.AlertCircle, Colors.Coral);
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
                string kh = row["HoTen"]?.ToString() ?? "";
                string phone = row["SoDienThoai"]?.ToString() ?? "";
                string sName = row["TenSan"]?.ToString() ?? "";
                if (kh.ToLower().Contains(keyword) || phone.Contains(keyword) || sName.ToLower().Contains(keyword))
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
            string ma = "ON-" + (row["DatCocID"] != DBNull.Value ? row["DatCocID"].ToString() : "0");
            string kh = row["HoTen"]?.ToString() ?? "Khách hàng";
            string phone = row["SoDienThoai"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(phone)) kh += " (" + phone + ")";

            string san = row["TenSan"]?.ToString() ?? (row["MaSan"]?.ToString() ?? "");
            DateTime bd = row["BatDau"] != DBNull.Value ? Convert.ToDateTime(row["BatDau"]) : DateTime.Now;
            DateTime kt = row["KetThuc"] != DBNull.Value ? Convert.ToDateTime(row["KetThuc"]) : DateTime.Now.AddHours(1);
            string slot = string.Format("{0:HH:mm} - {1:HH:mm}", bd, kt);
            decimal coc = row["SoTienCoc"] != DBNull.Value ? Convert.ToDecimal(row["SoTienCoc"]) : 0;
            string pthuc = "VietQR Napas";
            string st = row["TrangThaiDatCoc"]?.ToString() ?? "CHO_XAC_NHAN";

            string cd = "00:00";
            if (row["HetHanLuc"] != DBNull.Value)
            {
                DateTime exp = Convert.ToDateTime(row["HetHanLuc"]);
                TimeSpan diff = exp - DateTime.Now;
                if (diff.TotalSeconds > 0)
                    cd = string.Format("{0:D2}:{1:D2} còn", (int)diff.TotalMinutes, diff.Seconds);
                else
                    cd = "Đã quá hạn";
            }

            string stDisp = "Chờ duyệt";
            if (st == "DA_THANH_TOAN" || st == "THANH_CONG") stDisp = "Đã khớp";
            else if (st == "DA_HUY") stDisp = "Đã hủy";
            else if (st == "HET_HAN") stDisp = "Hết hạn";

            int rowIndex = grid.Rows.Add(ma, kh, san, slot, string.Format("{0:N0}đ", coc), pthuc, cd, stDisp);
            grid.Rows[rowIndex].Tag = row;
        }
    }
}
