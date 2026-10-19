using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Dialogs;

namespace SportZone.WinForms.Views
{
    public class ScheduleView : UserControl
    {
        private SFTimeline timeline;
        private DateTime currentDate = DateTime.Today;
        private Label lblCurrentDate;
        private string currentSportFilter = "ALL";
        private Panel scrollContainer;

        public ScheduleView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.AppBackground;
            BuildUI();
            LoadScheduleData();
        }

        private void BuildUI()
        {
            // Header
            Controls.Add(Ui.Header("Lịch sân theo timeline khung giờ", "Theo dõi tình trạng đặt sân thời gian thực từ 06:00 đến 23:00; nhấp chuột vào lượt đặt để xem chi tiết."));

            // Smart Toolbar
            Panel toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Colors.AppBackground,
                Padding = new Padding(0, 0, 0, 14)
            };

            // Left: Date Navigation
            SFButton btnToday = new SFButton
            {
                Text = "Hôm nay",
                Variant = SFButtonVariant.Primary,
                Width = 92,
                Dock = DockStyle.Left
            };
            btnToday.Click += (s, e) => { currentDate = DateTime.Today; UpdateDateLabel(); LoadScheduleData(); };
            toolbarPanel.Controls.Add(btnToday);

            SFIconButton btnPrev = new SFIconButton
            {
                Icon = SFIcon.ChevronLeft,
                Size = new Size(36, 40),
                Dock = DockStyle.Left,
                HoverBg = Colors.Surface,
                Margin = new Padding(6, 0, 0, 0)
            };
            btnPrev.Click += (s, e) => { currentDate = currentDate.AddDays(-1); UpdateDateLabel(); LoadScheduleData(); };
            toolbarPanel.Controls.Add(btnPrev);

            lblCurrentDate = new Label
            {
                Text = currentDate.ToString("dd/MM/yyyy (dddd)"),
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Left,
                Width = 190,
                TextAlign = ContentAlignment.MiddleCenter
            };
            toolbarPanel.Controls.Add(lblCurrentDate);

            SFIconButton btnNext = new SFIconButton
            {
                Icon = SFIcon.ChevronRight,
                Size = new Size(36, 40),
                Dock = DockStyle.Left,
                HoverBg = Colors.Surface
            };
            btnNext.Click += (s, e) => { currentDate = currentDate.AddDays(1); UpdateDateLabel(); LoadScheduleData(); };
            toolbarPanel.Controls.Add(btnNext);

            // Sport Filter Chips
            FlowLayoutPanel sportChips = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(16, 0, 0, 0),
                BackColor = Color.Transparent
            };
            SFFilterChip chipAll = new SFFilterChip { Text = "Tất cả môn", Selected = true, Size = new Size(100, 40) };
            SFFilterChip chipSoccer = new SFFilterChip { Text = "Bóng đá", Size = new Size(85, 40) };
            SFFilterChip chipBadm = new SFFilterChip { Text = "Cầu lông", Size = new Size(85, 40) };
            SFFilterChip chipOther = new SFFilterChip { Text = "Bóng rổ & Pickleball", Size = new Size(160, 40) };

            chipAll.Click += (s, e) =>
            {
                chipAll.Selected = true; chipSoccer.Selected = false; chipBadm.Selected = false; chipOther.Selected = false;
                currentSportFilter = "ALL";
                LoadScheduleData();
            };
            chipSoccer.Click += (s, e) =>
            {
                chipAll.Selected = false; chipSoccer.Selected = true; chipBadm.Selected = false; chipOther.Selected = false;
                currentSportFilter = "Bóng đá";
                LoadScheduleData();
            };
            chipBadm.Click += (s, e) =>
            {
                chipAll.Selected = false; chipSoccer.Selected = false; chipBadm.Selected = true; chipOther.Selected = false;
                currentSportFilter = "Cầu lông";
                LoadScheduleData();
            };
            chipOther.Click += (s, e) =>
            {
                chipAll.Selected = false; chipSoccer.Selected = false; chipBadm.Selected = false; chipOther.Selected = true;
                currentSportFilter = "OTHER";
                LoadScheduleData();
            };

            sportChips.Controls.Add(chipAll);
            sportChips.Controls.Add(chipSoccer);
            sportChips.Controls.Add(chipBadm);
            sportChips.Controls.Add(chipOther);
            toolbarPanel.Controls.Add(sportChips);

            // Right CTA: + Đặt sân
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

            Controls.Add(toolbarPanel);
            toolbarPanel.BringToFront();

            // Status Legend Strip
            Panel legendPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Colors.Surface,
                Padding = new Padding(16, 6, 16, 6)
            };
            legendPanel.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Colors.Border, 1.1f)) e.Graphics.DrawLine(p, 0, legendPanel.Height - 1, legendPanel.Width, legendPanel.Height - 1);
            };

            FlowLayoutPanel legendFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            legendFlow.Controls.Add(SFBadge.Create("● Đã thanh toán / Hoàn tất", Colors.DeepEmerald, Colors.SuccessBg));
            legendFlow.Controls.Add(SFBadge.Create("● Đang thi đấu", Colors.ElectricBlue, Colors.InfoBg));
            legendFlow.Controls.Add(SFBadge.Create("● Đã xác nhận / Cọc", ColorTranslator.FromHtml("#B45309"), Colors.WarningBg));
            legendFlow.Controls.Add(SFBadge.Create("● Chờ duyệt", Colors.Orange, Colors.OrangeBg));
            legendPanel.Controls.Add(legendFlow);

            Controls.Add(legendPanel);
            legendPanel.BringToFront();

            // Timeline Scroll Container
            scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.Surface,
                Padding = new Padding(1)
            };
            Controls.Add(scrollContainer);
            scrollContainer.BringToFront();

            timeline = new SFTimeline
            {
                Location = new Point(0, 0),
                Size = new Size(Math.Max(1050, scrollContainer.ClientSize.Width), 1180)
            };
            timeline.OnBookingClicked += (item) =>
            {
                if (item != null && !string.IsNullOrEmpty(item.BookingId))
                {
                    new BookingDetailForm(item.BookingId).ShowDialog(this);
                }
            };

            scrollContainer.Controls.Add(timeline);

            // Responsive timeline width update
            scrollContainer.Resize += (s, e) =>
            {
                int minW = 75 + (timeline.Fields != null ? timeline.Fields.Length : 6) * 165;
                timeline.Width = Math.Max(minW, scrollContainer.ClientSize.Width);
            };
        }

        private void LoadScheduleData()
        {
            try
            {
                var pNgay = new System.Data.SqlClient.SqlParameter("@Ngay", SqlDbType.Date) { Value = currentDate.Date };
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_LichSanTheoNgay @Ngay = @Ngay", pNgay);

                if (dt != null && dt.Rows.Count > 0)
                {
                    // 1. Collect unique courts filtered by sport
                    Dictionary<string, int> fieldIndexMap = new Dictionary<string, int>();
                    List<string> fieldNames = new List<string>();

                    foreach (DataRow row in dt.Rows)
                    {
                        string sName = row["TenSan"]?.ToString() ?? (row["MaSan"]?.ToString() ?? "");
                        string sport = row["TenLoaiSan"]?.ToString() ?? "";

                        if (currentSportFilter == "ALL" ||
                            (currentSportFilter == "Bóng đá" && sport.Contains("Bóng đá")) ||
                            (currentSportFilter == "Cầu lông" && sport.Contains("Cầu lông")) ||
                            (currentSportFilter == "OTHER" && !sport.Contains("Bóng đá") && !sport.Contains("Cầu lông")))
                        {
                            if (!fieldIndexMap.ContainsKey(sName))
                            {
                                fieldIndexMap[sName] = fieldNames.Count;
                                fieldNames.Add(sName);
                            }
                        }
                    }

                    if (fieldNames.Count > 0)
                    {
                        timeline.Fields = fieldNames.ToArray();
                        timeline.Items = new List<TimelineBookingItem>();

                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["DonDatSanID"] == DBNull.Value || row["BatDau"] == DBNull.Value || row["KetThuc"] == DBNull.Value)
                                continue;

                            string sName = row["TenSan"]?.ToString() ?? (row["MaSan"]?.ToString() ?? "");
                            if (!fieldIndexMap.ContainsKey(sName)) continue;

                            DateTime bd = Convert.ToDateTime(row["BatDau"]);
                            DateTime kt = Convert.ToDateTime(row["KetThuc"]);
                            float startH = bd.Hour + (float)bd.Minute / 60.0f;
                            float durH = (float)(kt - bd).TotalMinutes / 60.0f;
                            if (durH <= 0) durH = 1.0f;

                            string st = row["TrangThaiDon"]?.ToString() ?? "";
                            string stFriendly = "Đã xác nhận";
                            if (st == "DANG_SU_DUNG") stFriendly = "Đang thi đấu";
                            else if (st == "HOAN_THANH") stFriendly = "Hoàn tất";
                            else if (st == "CHO_XAC_NHAN") stFriendly = "Chờ duyệt";

                            timeline.Items.Add(new TimelineBookingItem
                            {
                                BookingId = row["MaDon"]?.ToString() ?? "",
                                CustomerName = row["HoTen"]?.ToString() ?? "Khách đặt",
                                Phone = row["SoDienThoai"]?.ToString() ?? "",
                                FieldName = sName,
                                FieldIndex = fieldIndexMap[sName],
                                StartHour = startH,
                                DurationHours = durH,
                                Status = stFriendly,
                                TotalAmount = string.Format("{0:N0}đ", row["TongTien"] != DBNull.Value ? Convert.ToDecimal(row["TongTien"]) : 0),
                                PaidAmount = string.Format("{0:N0}đ", row["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(row["DaThanhToan"]) : 0)
                            });
                        }

                        int minW = 75 + timeline.Fields.Length * 165;
                        timeline.Width = Math.Max(minW, scrollContainer.ClientSize.Width);
                        timeline.Invalidate();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_SCHEDULE");
            }

            timeline.Items = new List<TimelineBookingItem>();
            timeline.Invalidate();
        }

        private void UpdateDateLabel()
        {
            lblCurrentDate.Text = currentDate.ToString("dd/MM/yyyy (dddd)");
            SFToast.Show(this, "Đã cập nhật lịch theo ngày: " + currentDate.ToString("dd/MM/yyyy"), "LỊCH THI ĐẤU", SFIcon.Calendar, Colors.Emerald);
        }
    }
}
