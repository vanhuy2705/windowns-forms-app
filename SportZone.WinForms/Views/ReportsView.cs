using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;

namespace SportZone.WinForms.Views
{
    public class ReportsView : UserControl
    {
        private SFChart chartRevenue;
        private SFDonut donutSport;
        private SFDataGridView gridTopFields;
        private SFComboBox cboPeriod;
        private SFMetricCard cardRev;
        private SFMetricCard cardOrders;
        private SFMetricCard cardOccupancy;
        private SFMetricCard cardService;

        public ReportsView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadReportData();
        }

        private void BuildUI()
        {
            // 1. Header
            Controls.Add(Ui.Header("Báo Cáo Doanh Thu & Phân Tích Tổng Hợp", "Số liệu doanh thu, lượt đặt, công suất khai thác sân và cơ cấu thu chi theo thời gian thực."));

            // 2. Toolbar
            FlowLayoutPanel tb = Ui.Toolbar();

            SFButton btnPdf = new SFButton
            {
                Text = "Xuất Báo Cáo PDF",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Printer,
                Width = 165,
                Height = 38
            };
            btnPdf.Click += (s, e) => Helpers.ExportHelper.ExportToPdf("BÁO CÁO HIỆU SUẤT KHAI THÁC SÂN THỂ THAO", gridTopFields, "BaoCaoHieuSuatSan.html", this);
            tb.Controls.Add(btnPdf);

            SFButton btnExcel = new SFButton
            {
                Text = "Xuất Excel Kế Toán",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Dollar,
                Width = 170,
                Height = 38
            };
            btnExcel.Click += (s, e) => Helpers.ExportHelper.ExportToExcel(gridTopFields, "BaoCaoHieuSuatSan.csv", this);
            tb.Controls.Add(btnExcel);

            cboPeriod = new SFComboBox
            {
                Width = 180,
                Height = 38
            };
            cboPeriod.Items.AddRange(new object[] { "Hôm nay", "7 ngày qua", "Tháng này (10/2026)", "Năm 2026" });
            cboPeriod.SelectedIndex = 2; // Default "Tháng này"
            cboPeriod.SelectedIndexChanged += (s, e) => LoadReportData();
            tb.Controls.Add(cboPeriod);

            SFButton btnRefresh = new SFButton
            {
                Text = "Làm Mới",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Refresh,
                Width = 110,
                Height = 38
            };
            btnRefresh.Click += (s, e) => LoadReportData();
            tb.Controls.Add(btnRefresh);

            Controls.Add(tb);
            tb.BringToFront();

            // 3. Metric Bar (4 KPI cards)
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
            cardRev = new SFMetricCard("DOANH THU KỲ CHỌN", "—", "Từ CSDL theo kỳ lọc", SFIcon.Dollar, Colors.Emerald);
            cardOrders = new SFMetricCard("TỔNG LƯỢT ĐẶT", "—", "Đơn & hóa đơn kỳ chọn", SFIcon.Calendar, Colors.ElectricBlue);
            cardOccupancy = new SFMetricCard("TỶ TRỌNG TOP 1", "—", "Bộ môn dẫn đầu doanh thu", SFIcon.Chart, Colors.Purple);
            cardService = new SFMetricCard("DỊCH VỤ & F&B", "—", "Tổng doanh thu dịch vụ", SFIcon.Star, Colors.Amber);

            kpiPanel.Controls.Add(cardRev, 0, 0);
            kpiPanel.Controls.Add(cardOrders, 1, 0);
            kpiPanel.Controls.Add(cardOccupancy, 2, 0);
            kpiPanel.Controls.Add(cardService, 3, 0);

            Controls.Add(kpiPanel);
            kpiPanel.BringToFront();

            // 4. Split Layout: Top Row = Charts, Bottom Row = Top Courts Table
            TableLayoutPanel mainGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0)
            };
            mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64f));
            mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 58f));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 42f));

            // Chart Left: Area Spline Chart
            SFCard cardChart = new SFCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                Margin = new Padding(0, 0, 10, 10)
            };
            Label lblChartTitle = new Label
            {
                Text = "Xu Hướng Tăng Trưởng Doanh Thu (Dữ Liệu SQL Server)",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 32
            };
            cardChart.Controls.Add(lblChartTitle);

            chartRevenue = new SFChart
            {
                Dock = DockStyle.Fill,
                AccentColor = Colors.Emerald
            };
            cardChart.Controls.Add(chartRevenue);
            chartRevenue.BringToFront();
            mainGrid.Controls.Add(cardChart, 0, 0);

            // Chart Right: Donut Chart
            SFCard cardDonut = new SFCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                Margin = new Padding(10, 0, 0, 10)
            };
            Label lblDonutTitle = new Label
            {
                Text = "Cơ Cấu Doanh Thu Theo Bộ Môn",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 32
            };
            cardDonut.Controls.Add(lblDonutTitle);

            donutSport = new SFDonut
            {
                Dock = DockStyle.Fill,
                CenterMetric = "—",
                CenterLabel = "Doanh thu",
                SliceColors = new[] { Colors.Emerald, Colors.ElectricBlue, Colors.Amber, Colors.Purple }
            };
            cardDonut.Controls.Add(donutSport);
            donutSport.BringToFront();
            mainGrid.Controls.Add(cardDonut, 1, 0);

            // Bottom Row: Top Court Table (Spans 2 columns)
            SFCard cardTable = new SFCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                Margin = new Padding(0, 10, 0, 0)
            };
            Label lblTableTitle = new Label
            {
                Text = "Hiệu Suất Khai Thác Sân Thể Thao Theo Bộ Môn",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 32
            };
            cardTable.Controls.Add(lblTableTitle);

            gridTopFields = new SFDataGridView
            {
                Dock = DockStyle.Fill
            };
            gridTopFields.Columns.Add("Field", "Sân thể thao / Bộ môn");
            gridTopFields.Columns.Add("Sport", "Phân loại");
            gridTopFields.Columns.Add("Orders", "Lượt đặt");
            gridTopFields.Columns.Add("Hours", "Giờ thi đấu");
            gridTopFields.Columns.Add("Revenue", "Doanh thu thực");
            gridTopFields.Columns.Add("Share", "Tỷ trọng doanh thu");
            gridTopFields.Columns.Add("Rank", "Xếp hạng");

            gridTopFields.Columns["Field"].Width = 200;
            gridTopFields.Columns["Sport"].Width = 140;
            gridTopFields.Columns["Orders"].Width = 100;
            gridTopFields.Columns["Hours"].Width = 110;
            gridTopFields.Columns["Revenue"].Width = 150;
            gridTopFields.Columns["Share"].Width = 140;
            gridTopFields.Columns["Rank"].Width = 100;

            // V9: số lượng/công suất/tiền căn phải, biệt thị định dạng N0
            gridTopFields.Columns["Orders"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridTopFields.Columns["Hours"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridTopFields.Columns["Revenue"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridTopFields.Columns["Share"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            cardTable.Controls.Add(gridTopFields);
            gridTopFields.BringToFront();

            mainGrid.Controls.Add(cardTable, 0, 1);
            mainGrid.SetColumnSpan(cardTable, 2);

            Controls.Add(mainGrid);
            mainGrid.BringToFront();
        }

        private void LoadReportData()
        {
            DateTime tuNgay = DateTime.Today;
            DateTime denNgay = DateTime.Today;

            int sel = cboPeriod != null ? cboPeriod.SelectedIndex : 2;
            if (sel == 0) // Hôm nay
            {
                tuNgay = DateTime.Today;
                denNgay = DateTime.Today;
            }
            else if (sel == 1) // 7 ngày qua
            {
                tuNgay = DateTime.Today.AddDays(-7);
                denNgay = DateTime.Today;
            }
            else if (sel == 2) // Tháng này
            {
                tuNgay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                denNgay = tuNgay.AddMonths(1).AddDays(-1);
            }
            else if (sel == 3) // Năm nay
            {
                tuNgay = new DateTime(DateTime.Today.Year, 1, 1);
                denNgay = new DateTime(DateTime.Today.Year, 12, 31);
            }

            try
            {
                var pTu = new System.Data.SqlClient.SqlParameter("@TuNgay", SqlDbType.Date) { Value = tuNgay.Date };
                var pDen = new System.Data.SqlClient.SqlParameter("@DenNgay", SqlDbType.Date) { Value = denNgay.Date };
                DataSet ds = DatabaseManager.ExecuteDataSet("EXEC dbo.sp_UI_BaoCaoTongQuan @TuNgay = @TuNgay, @DenNgay = @DenNgay", pTu, pDen);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow r = ds.Tables[0].Rows[0];
                    decimal daThu = r["DoanhThuDaThu"] != DBNull.Value ? Convert.ToDecimal(r["DoanhThuDaThu"]) : 0;
                    decimal tongGiaTri = r["GiaTriDon"] != DBNull.Value ? Convert.ToDecimal(r["GiaTriDon"]) : 0;
                    int tongDon = r["TongDon"] != DBNull.Value ? Convert.ToInt32(r["TongDon"]) : 0;

                    cardRev.Value = string.Format("{0:N0} đ", daThu > 0 ? daThu : tongGiaTri);
                    cardOrders.Value = string.Format("{0} đơn", tongDon);
                    // Daily trend in table 1
                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        List<string> xLabs = new List<string>();
                        List<double> vals = new List<double>();
                        double maxV = 1.0;
                        foreach (DataRow row in ds.Tables[1].Rows)
                        {
                            DateTime dt = Convert.ToDateTime(row["Ngay"]);
                            double revM = Convert.ToDouble(row["DoanhThu"]) / 1000000.0;
                            xLabs.Add(dt.ToString("dd/MM"));
                            vals.Add(Math.Round(revM, 2));
                            if (revM > maxV) maxV = revM;
                        }

                        chartRevenue.XLabels = xLabs.ToArray();
                        chartRevenue.Values = vals.ToArray();
                        chartRevenue.MaxValue = maxV * 1.3;
                        chartRevenue.Invalidate();
                    }

                    // Types in table 2
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

                        gridTopFields.Rows.Clear();
                        int rank = 1;
                        foreach (DataRow row in dtTypes.Rows)
                        {
                            string tName = row["TenLoaiSan"]?.ToString() ?? "";
                            int luotDat = row["LuotDat"] != DBNull.Value ? Convert.ToInt32(row["LuotDat"]) : 0;
                            double gio = row["TongGio"] != DBNull.Value ? Convert.ToDouble(row["TongGio"]) : 0;
                            decimal rev = row["DoanhThuSan"] != DBNull.Value ? Convert.ToDecimal(row["DoanhThuSan"]) : 0;
                            float pct = totalSum > 0 ? (float)(rev / totalSum * 100m) : 0f;

                            cats.Add(string.Format("{0} ({1:0.#}%)", tName, pct));
                            pcts.Add(pct);

                            gridTopFields.Rows.Add(
                                "Cụm sân " + tName,
                                tName,
                                string.Format("{0} lượt", luotDat),
                                string.Format("{0:0.#} giờ", gio),
                                string.Format("{0:N0}đ", rev),
                                string.Format("{0:0.#}%", pct),
                                string.Format("TOP {0}", rank++)
                            );
                            if (rank == 2)
                            {
                                cardOccupancy.Value = string.Format("{0:0.#}%", pct);
                                cardOccupancy.SubValue = "Bộ môn dẫn đầu doanh thu kỳ chọn";
                            }
                        }

                        if (cats.Count > 0)
                        {
                            donutSport.Categories = cats.ToArray();
                            donutSport.Percentages = pcts.ToArray();
                            donutSport.Invalidate();
                        }
                    }

                    // Services in table 3
                    if (ds.Tables.Count > 3 && ds.Tables[3].Rows.Count > 0)
                    {
                        decimal dvuTotal = 0;
                        foreach (DataRow row in ds.Tables[3].Rows)
                        {
                            dvuTotal += row["DoanhThuDichVu"] != DBNull.Value ? Convert.ToDecimal(row["DoanhThuDichVu"]) : 0;
                        }
                        cardService.Value = string.Format("{0:N0} đ", dvuTotal);
                    }

                    return;
                }

                // Không có số liệu đứng cấp nào cho kỳ → trạng thái trống trung thực
                Helpers.AppLogger.LogWarning("REPORTS_EMPTY");
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_REPORTS");
                cardRev.Value = "—";
                cardOrders.Value = "—";
                cardOccupancy.Value = "—";
                cardService.Value = "—";
                chartRevenue.Values = new double[0];
                donutSport.Categories = new string[0];
                donutSport.Invalidate();
                SFToast.Show(this, "Không tải được báo cáo từ CSDL. Kiểm tra kết nối SQL Server rồi nhấn làm mới.", "LỖI DỮ LIỆU", SFIcon.AlertCircle, Colors.Danger);
            }

            gridTopFields.Rows.Clear();
        }
    }
}
