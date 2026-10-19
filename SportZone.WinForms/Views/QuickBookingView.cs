using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SportZone.WinForms.Controls;
using SportZone.WinForms.Database;
using SportZone.WinForms.DesignTokens;
using SportZone.WinForms.Dialogs;

namespace SportZone.WinForms.Views
{
    public class QuickBookingView : UserControl
    {
        private int currentStep = 1;
        private Panel stepHeaderPanel;
        private Panel stepContentHost;
        private SFCard summaryCard;

        // Selection State
        private int selectedFieldId = 0;
        private string selectedField = "Chưa chọn sân";
        private string selectedSport = "Bóng đá";
        private DateTime selectedDateValue = DateTime.Today;
        private string selectedDate = DateTime.Today.ToString("dd/MM/yyyy");
        private string selectedTime = "18:00 - 19:30";
        private int fieldPrice = 0;
        private int servicePrice = 0;
        private int voucherDiscount = 0;
        private long? selectedVoucherKhachHangId = null;
        private int selectedCustomerId = 0;
        private string selectedCustomerName = "";
        private string bookingNote = "";

        private readonly Dictionary<int, (string Name, decimal DonGia, decimal SoLuong)> selectedServices 
            = new Dictionary<int, (string, decimal, decimal)>();

        private Label lblSumField, lblSumTime, lblSumFieldCost, lblSumServiceCost, lblSumVoucher, lblSumTotal;

        public QuickBookingView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.AppBackground;

            // Tự động nhận diện khách hàng nếu phiên là khách hàng
            if (Session.AppSession.Current != null && Session.AppSession.Current.IsCustomer && Session.AppSession.Current.KhachHangID.HasValue)
            {
                selectedCustomerId = Session.AppSession.Current.KhachHangID.Value;
                selectedCustomerName = Session.AppSession.Current.HoTen;
            }

            BuildUI();
        }

        private void BuildUI()
        {
            Controls.Add(Ui.Header("Quy trình đặt sân trực tiếp (Wizard)", "Trình hướng dẫn 4 bước tối ưu cho nhân viên quầy và khách hàng đặt sân nhanh chóng."));

            // Main 2-column Layout: Left Wizard (70%), Right Live Summary (30%)
            TableLayoutPanel mainSplit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = Colors.AppBackground
            };
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
            mainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));

            // Left Wizard Panel
            SFCard wizardCard = new SFCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 16, 0),
                Padding = new Padding(24)
            };

            // Custom Progress Step Header (Rule 39)
            stepHeaderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.Transparent
            };
            stepHeaderPanel.Paint += DrawWizardProgress;
            wizardCard.Controls.Add(stepHeaderPanel);

            // Step Content Host
            stepContentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 16, 0, 16)
            };
            wizardCard.Controls.Add(stepContentHost);
            stepContentHost.BringToFront();

            // Bottom Navigation Buttons
            Panel bottomNav = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Colors.Surface,
                Padding = new Padding(0, 8, 0, 0)
            };
            SFButton btnPrev = new SFButton
            {
                Text = "← Quay lại",
                Variant = SFButtonVariant.Secondary,
                Width = 120,
                Dock = DockStyle.Left
            };
            btnPrev.Click += (s, e) =>
            {
                if (currentStep > 1)
                {
                    currentStep--;
                    LoadStepContent();
                    stepHeaderPanel.Invalidate();
                }
            };

            SFButton btnNext = new SFButton
            {
                Text = "Tiếp tục bước kế →",
                Variant = SFButtonVariant.Primary,
                Width = 160,
                Dock = DockStyle.Right
            };
            btnNext.Click += (s, e) =>
            {
                if (currentStep == 1)
                {
                    if (selectedFieldId <= 0)
                    {
                        SFToast.Show(this, "Vui lòng chọn sân thi đấu trước khi chuyển sang bước tiếp theo.", "CHƯA CHỌN SÂN", SFIcon.AlertCircle, Colors.Coral);
                        return;
                    }
                    currentStep++;
                    LoadStepContent();
                    stepHeaderPanel.Invalidate();
                }
                else if (currentStep == 2)
                {
                    if (string.IsNullOrEmpty(selectedTime))
                    {
                        SFToast.Show(this, "Vui lòng chọn khung giờ thi đấu.", "CHƯA CHỌN KHUNG GIỜ", SFIcon.AlertCircle, Colors.Coral);
                        return;
                    }
                    currentStep++;
                    LoadStepContent();
                    stepHeaderPanel.Invalidate();
                }
                else if (currentStep == 3)
                {
                    currentStep++;
                    LoadStepContent();
                    stepHeaderPanel.Invalidate();
                }
                else
                {
                    ExecuteBooking();
                }
            };

            bottomNav.Controls.Add(btnPrev);
            bottomNav.Controls.Add(btnNext);
            wizardCard.Controls.Add(bottomNav);

            mainSplit.Controls.Add(wizardCard, 0, 0);

            // Right Live Summary Card
            summaryCard = new SFCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            BuildSummaryCard();
            mainSplit.Controls.Add(summaryCard, 1, 0);

            Controls.Add(mainSplit);
            mainSplit.BringToFront();

            LoadStepContent();
        }

        private void ExecuteBooking()
        {
            try
            {
                int khId = selectedCustomerId;
                if (khId <= 0)
                {
                    if (Session.AppSession.Current != null && Session.AppSession.Current.KhachHangID.HasValue && Session.AppSession.Current.KhachHangID.Value > 0)
                    {
                        khId = Session.AppSession.Current.KhachHangID.Value;
                    }
                }

                if (khId <= 0)
                {
                    SFToast.Show(this, "Vui lòng chọn hoặc nhập thông tin khách hàng đặt sân.", "CHƯA CHỌN KHÁCH HÀNG", SFIcon.AlertCircle, Colors.Coral);
                    return;
                }

                if (selectedFieldId <= 0)
                {
                    SFToast.Show(this, "Vui lòng chọn sân thi đấu hợp lệ ở Bước 1 trước khi xác nhận đặt.", "CHƯA CHỌN SÂN", SFIcon.AlertCircle, Colors.Coral);
                    return;
                }

                string[] timeParts = selectedTime.Split('-');
                if (timeParts.Length != 2)
                {
                    SFToast.Show(this, "Khung giờ thi đấu không hợp lệ.", "LỖI KHUNG GIỜ", SFIcon.AlertCircle, Colors.Coral);
                    return;
                }

                TimeSpan tStart = TimeSpan.Parse(timeParts[0].Trim());
                TimeSpan tEnd = TimeSpan.Parse(timeParts[1].Trim());
                DateTime batDau = selectedDateValue.Date.Add(tStart);
                DateTime ketThuc = selectedDateValue.Date.Add(tEnd);

                if (batDau <= DateTime.Now && batDau.Date == DateTime.Today)
                {
                    SFToast.Show(this, "Khung giờ đặt sân phải diễn ra trong tương lai.", "LỖI THỜI GIAN", SFIcon.AlertCircle, Colors.Coral);
                    return;
                }

                string nguonDon = (Session.AppSession.Current != null && Session.AppSession.Current.IsCustomer) ? "TRUC_TUYEN" : "QUAY";
                int taiKhoanId = Security.SessionGuard.RequireAccountId();

                var bookingRes = new Services.BookingService().CreateBooking(
                    khachHangId: khId,
                    sanId: selectedFieldId,
                    batDau: batDau,
                    ketThuc: ketThuc,
                    nguonDon: nguonDon,
                    nguoiTaoTaiKhoanId: taiKhoanId,
                    voucherKhachHangId: selectedVoucherKhachHangId,
                    ghiChu: string.IsNullOrEmpty(bookingNote) ? "Đặt sân qua Quick Booking" : bookingNote
                );

                if (bookingRes.DonDatSanID <= 0)
                {
                    SFToast.Show(this, "Không thể tạo đơn đặt sân. Vui lòng kiểm tra lại trạng thái sân.", "LỖI TẠO ĐƠN", SFIcon.AlertCircle, Colors.Danger);
                    return;
                }

                // Thêm các dịch vụ được chọn vào đơn đặt sân
                var bookingService = new Services.BookingService();
                foreach (var kvp in selectedServices)
                {
                    try
                    {
                        bookingService.AddServiceToBooking(bookingRes.DonDatSanID, kvp.Key, kvp.Value.SoLuong, khoId: 1, taiKhoanId: taiKhoanId);
                    }
                    catch (Exception exSv)
                    {
                        Helpers.AppLogger.LogWarning("Thêm dịch vụ " + kvp.Key + " vào đơn thất bại: " + exSv.Message);
                    }
                }

                // Tạo hóa đơn
                var payService = new Services.PaymentService();
                var invRes = payService.CreateInvoice(bookingRes.DonDatSanID, taiKhoanId);
                long hoaDonId = invRes.HoaDonID;

                if (hoaDonId <= 0)
                {
                    SFToast.Show(this, $"Đã tạo đơn {bookingRes.MaDon} nhưng chưa thể phát hành hóa đơn.", "LỖI HÓA ĐƠN", SFIcon.AlertCircle, Colors.Warning);
                    return;
                }

                // Đọc tổng tiền thực tế sau khi đã áp dụng dịch vụ và voucher
                decimal finalTotal = 0;
                try
                {
                    var objTot = DatabaseManager.ExecuteScalar("SELECT TongTien FROM dbo.DonDatSan WHERE DonDatSanID = @ID", new SqlParameter("@ID", bookingRes.DonDatSanID));
                    if (objTot != null && objTot != DBNull.Value) finalTotal = Convert.ToDecimal(objTot);
                }
                catch { finalTotal = Math.Max(0, fieldPrice + servicePrice - voucherDiscount); }

                string displayCustomer = string.IsNullOrEmpty(selectedCustomerName) ? (Session.AppSession.Current?.HoTen ?? "Khách hàng") : selectedCustomerName;
                new QrPaymentForm(finalTotal, bookingRes.MaDon, displayCustomer, hoaDonId).ShowDialog(this);
                SFToast.Show(this, $"Đã tạo đơn {bookingRes.MaDon} thành công trên hệ thống!", "ĐẶT SÂN THÀNH CÔNG", SFIcon.Check, Colors.Emerald);

                // Bắn thông báo thời gian thực vào Notification Center
                try
                {
                    if (taiKhoanId > 0)
                    {
                        Services.NotificationService.Instance.CreateNotification(
                            taiKhoanId,
                            $"Đặt sân thành công - {bookingRes.MaDon}",
                            $"Đơn đặt sân {bookingRes.MaDon} cho sân {selectedField} đã được tạo thành công.",
                            "BOOKING",
                            "BOOKING",
                            bookingRes.DonDatSanID);
                    }
                    Services.NotificationService.Instance.NotifyRole(
                        "NHAN_VIEN",
                        $"Đơn đặt sân mới - {bookingRes.MaDon}",
                        $"Khách hàng {displayCustomer} vừa tạo đơn đặt sân {selectedField}.",
                        "BOOKING",
                        "BOOKING",
                        bookingRes.DonDatSanID);
                }
                catch (Exception notifEx)
                {
                    Helpers.AppLogger.LogWarning("QuickBooking notification emit failed: " + notifEx.Message);
                }

                // Reset trạng thái sau khi hoàn tất đặt sân
                currentStep = 1;
                selectedFieldId = 0;
                selectedField = "Chưa chọn sân";
                fieldPrice = 0;
                servicePrice = 0;
                voucherDiscount = 0;
                selectedVoucherKhachHangId = null;
                selectedServices.Clear();
                LoadStepContent();
                stepHeaderPanel.Invalidate();
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "QUICK_BOOKING_CREATE");
                SFToast.Show(this, "Lỗi tạo đơn đặt sân: " + Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI TẠO ĐƠN", SFIcon.AlertCircle, Colors.Coral);
            }
        }

        private void DrawWizardProgress(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            string[] stepTitles = { "1. Chọn sân", "2. Ngày & Giờ", "3. Dịch vụ", "4. Xác nhận" };
            int count = stepTitles.Length;
            int width = stepHeaderPanel.Width;
            int nodeY = 20;

            float stepDist = (float)(width - 120) / (count - 1);

            // Draw connecting bar
            for (int i = 0; i < count - 1; i++)
            {
                float x1 = 60 + i * stepDist;
                float x2 = 60 + (i + 1) * stepDist;
                bool isCompleted = (i + 1) < currentStep;

                using (Pen p = new Pen(isCompleted ? Colors.Emerald : Colors.Border, 3f))
                {
                    g.DrawLine(p, x1, nodeY, x2, nodeY);
                }
            }

            // Draw step nodes
            for (int i = 0; i < count; i++)
            {
                float x = 60 + i * stepDist;
                int stepNum = i + 1;
                bool isPassed = stepNum < currentStep;
                bool isCurrent = stepNum == currentStep;

                Color nodeBg = isPassed ? Colors.Emerald : (isCurrent ? Colors.Emerald : Colors.Surface);
                Color nodeBorder = (isPassed || isCurrent) ? Colors.Emerald : Colors.Border;
                Color nodeFg = (isPassed || isCurrent) ? Color.White : Colors.TextMuted;

                int radius = isCurrent ? 14 : 12;
                Rectangle nodeRect = new Rectangle((int)x - radius, nodeY - radius, radius * 2, radius * 2);

                using (SolidBrush b = new SolidBrush(nodeBg)) g.FillEllipse(b, nodeRect);
                using (Pen p = new Pen(nodeBorder, 2f)) g.DrawEllipse(p, nodeRect);

                string numStr = isPassed ? "✓" : stepNum.ToString();
                TextRenderer.DrawText(g, numStr, Typography.MicroBold, nodeRect, nodeFg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // Step label
                Rectangle labelRect = new Rectangle((int)x - 65, nodeY + 16, 130, 20);
                Font f = isCurrent ? Typography.SecondaryBold : Typography.Micro;
                Color textCol = isCurrent ? Colors.TextPrimary : Colors.TextMuted;
                TextRenderer.DrawText(g, stepTitles[i], f, labelRect, textCol,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
            }
        }

        private void LoadStepContent()
        {
            stepContentHost.Controls.Clear();

            switch (currentStep)
            {
                case 1:
                    LoadStep1_SelectField();
                    break;
                case 2:
                    LoadStep2_SelectTime();
                    break;
                case 3:
                    LoadStep3_Services();
                    break;
                case 4:
                    LoadStep4_Confirmation();
                    break;
            }

            UpdateSummaryDisplay();
        }

        private void LoadStep1_SelectField()
        {
            Label lblTitle = new Label { Text = "Bước 1: Chọn môn & Sân thi đấu (Dữ liệu SQL Server)", UseMnemonic = false, Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 28 };
            stepContentHost.Controls.Add(lblTitle);

            FlowLayoutPanel chips = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, WrapContents = false, BackColor = Colors.Surface };
            chips.Controls.Add(new SFFilterChip { Text = "Bóng đá", Selected = selectedSport == "Bóng đá", Size = new Size(100, 36) });
            chips.Controls.Add(new SFFilterChip { Text = "Cầu lông", Selected = selectedSport == "Cầu lông", Size = new Size(100, 36) });
            chips.Controls.Add(new SFFilterChip { Text = "Bóng rổ", Selected = selectedSport == "Bóng rổ", Size = new Size(95, 36) });
            chips.Controls.Add(new SFFilterChip { Text = "Pickleball", Selected = selectedSport == "Pickleball", Size = new Size(110, 36) });
            stepContentHost.Controls.Add(chips);

            FlowLayoutPanel fieldList = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Colors.Surface, Padding = new Padding(0, 10, 0, 0) };

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachSan");
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int sId = Convert.ToInt32(r["SanID"]);
                        string sName = r["TenSan"]?.ToString() ?? "";
                        string cum = r["TenCumSan"]?.ToString() ?? "Cụm trung tâm";
                        decimal price = 300000;
                        if (dt.Columns.Contains("GiaCoBan") && r["GiaCoBan"] != DBNull.Value)
                            price = Convert.ToDecimal(r["GiaCoBan"]);
                        else if (dt.Columns.Contains("GiaMacDinh") && r["GiaMacDinh"] != DBNull.Value)
                            price = Convert.ToDecimal(r["GiaMacDinh"]);
                        int pInt = (int)price;

                        Panel card = new Panel { Size = new Size(240, 100), Margin = new Padding(0, 0, 12, 12), BackColor = Colors.SurfaceElevated, Cursor = Cursors.Hand };
                        bool isSel = (sId == selectedFieldId);

                        card.Paint += (s, e) =>
                        {
                            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            Rectangle rec = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                            using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(rec, 8))
                            {
                                using (SolidBrush b = new SolidBrush(isSel ? Colors.SuccessBg : Colors.SurfaceElevated)) e.Graphics.FillPath(b, p);
                                using (Pen pen = new Pen(isSel ? Colors.Emerald : Colors.Border, isSel ? 2f : 1.1f)) e.Graphics.DrawPath(pen, p);
                            }
                        };

                        Label cn = new Label { Text = sName, Font = Typography.BodyBold, ForeColor = Colors.TextPrimary, Location = new Point(12, 12), AutoSize = true };
                        Label cs = new Label { Text = cum, Font = Typography.Micro, ForeColor = Colors.TextSecondary, Location = new Point(12, 34), AutoSize = true };
                        Label cp = new Label { Text = string.Format("{0:N0}đ/h", pInt), Font = Typography.SecondaryBold, ForeColor = Colors.DeepEmerald, Location = new Point(12, 64), AutoSize = true };

                        card.Controls.Add(cn);
                        card.Controls.Add(cs);
                        card.Controls.Add(cp);

                        card.Click += (s, e) =>
                        {
                            selectedField = sName;
                            selectedFieldId = sId;
                            fieldPrice = pInt;
                            LoadStep1_SelectField();
                            UpdateSummaryDisplay();
                        };

                        fieldList.Controls.Add(card);
                    }

                    stepContentHost.Controls.Add(fieldList);
                    fieldList.BringToFront();
                    return;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "QUICK_BOOKING_LOAD_FIELDS");
            }

            stepContentHost.Controls.Add(fieldList);
            fieldList.BringToFront();
        }

        private void LoadStep2_SelectTime()
        {
            Label lblTitle = new Label { Text = "Bước 2: Chọn Ngày thi đấu & Khung giờ trống", UseMnemonic = false, Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 28 };
            stepContentHost.Controls.Add(lblTitle);

            Panel dateRow = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.Transparent };
            Label dl = new Label { Text = "Ngày thi đấu:", Font = Typography.SecondaryBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 12), AutoSize = true };
            DateTimePicker dtPicker = new DateTimePicker { Location = new Point(100, 8), Width = 160, Format = DateTimePickerFormat.Short, Font = Typography.Body, MinDate = DateTime.Today, Value = selectedDateValue };
            dtPicker.ValueChanged += (s, e) => 
            { 
                selectedDateValue = dtPicker.Value;
                selectedDate = dtPicker.Value.ToString("dd/MM/yyyy"); 
                UpdateSummaryDisplay(); 
            };
            dateRow.Controls.Add(dl);
            dateRow.Controls.Add(dtPicker);
            stepContentHost.Controls.Add(dateRow);

            Label slotHead = new Label { Text = "Khung giờ sẵn sàng hôm nay:", Font = Typography.SecondaryBold, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 30 };
            stepContentHost.Controls.Add(slotHead);

            FlowLayoutPanel slotsGrid = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
            string[] slots = { "06:00 - 07:30", "07:30 - 09:00", "09:00 - 10:30", "15:00 - 16:30", "16:30 - 18:00", "18:00 - 19:30", "19:30 - 21:00", "21:00 - 22:30" };

            foreach (var slot in slots)
            {
                SFButton btnSlot = new SFButton
                {
                    Text = slot,
                    Variant = (slot == selectedTime) ? SFButtonVariant.Primary : SFButtonVariant.Secondary,
                    Size = new Size(130, 42),
                    Margin = new Padding(0, 0, 10, 10)
                };
                btnSlot.Click += (s, e) =>
                {
                    selectedTime = slot;
                    LoadStep2_SelectTime();
                    UpdateSummaryDisplay();
                };
                slotsGrid.Controls.Add(btnSlot);
            }

            stepContentHost.Controls.Add(slotsGrid);
            slotsGrid.BringToFront();
        }

        private void LoadStep3_Services()
        {
            Label lblTitle = new Label { Text = "Bước 3: Dịch vụ bổ sung & Tiện ích (Kho hàng thực tế)", UseMnemonic = false, Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 28 };
            stepContentHost.Controls.Add(lblTitle);

            Panel list = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("SELECT SanPhamDichVuID, TenSanPhamDichVu, DonGia, DonViTinh FROM dbo.SanPhamDichVu WHERE DangHoatDong = 1 ORDER BY TenSanPhamDichVu");
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int spId = Convert.ToInt32(r["SanPhamDichVuID"]);
                        string spName = r["TenSanPhamDichVu"]?.ToString() ?? "";
                        decimal gia = Convert.ToDecimal(r["DonGia"]);
                        string dvt = r["DonViTinh"]?.ToString() ?? "món";
                        string priceStr = $"{gia:N0}đ / {dvt}";

                        bool isChecked = selectedServices.ContainsKey(spId);
                        AddServiceRow(list, spId, spName, priceStr, isChecked, (int)gia);
                    }
                }
                else
                {
                    Label emptyLbl = new Label { Text = "Chưa có dịch vụ nào đang kinh doanh trong hệ thống.", Font = Typography.Body, ForeColor = Colors.TextMuted, Dock = DockStyle.Top, Height = 36 };
                    list.Controls.Add(emptyLbl);
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "QUICK_BOOKING_LOAD_SERVICES");
            }

            stepContentHost.Controls.Add(list);
            list.BringToFront();
        }

        private void AddServiceRow(Panel parent, int spId, string name, string priceStr, bool defChecked, int cost)
        {
            Panel row = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Colors.SurfaceElevated, Margin = new Padding(0, 0, 0, 8), Padding = new Padding(12, 10, 12, 10) };
            row.Paint += (s, e) =>
            {
                Rectangle r = new Rectangle(0, 0, row.Width - 1, row.Height - 1);
                using (GraphicsPath p = GraphicsUtility.CreateRoundedRectangle(r, 6))
                {
                    using (Pen pen = new Pen(Colors.Border, 1.1f)) e.Graphics.DrawPath(pen, p);
                }
            };

            CheckBox chk = new CheckBox { Text = name, Font = Typography.Body, ForeColor = Colors.TextPrimary, Dock = DockStyle.Left, AutoSize = true, Checked = defChecked };
            Label pr = new Label { Text = priceStr, Font = Typography.BodyBold, ForeColor = Colors.DeepEmerald, Dock = DockStyle.Right, AutoSize = true, TextAlign = ContentAlignment.MiddleRight };

            chk.CheckedChanged += (s, e) =>
            {
                if (chk.Checked)
                {
                    selectedServices[spId] = (name, cost, 1);
                }
                else
                {
                    selectedServices.Remove(spId);
                }

                int sum = 0;
                foreach (var item in selectedServices.Values) sum += (int)(item.DonGia * item.SoLuong);
                servicePrice = sum;
                UpdateSummaryDisplay();
            };

            row.Controls.Add(pr);
            row.Controls.Add(chk);
            parent.Controls.Add(row);
            row.BringToFront();
        }

        private void LoadStep4_Confirmation()
        {
            Label lblTitle = new Label { Text = "Bước 4: Xác nhận & Đặt cọc giữ chỗ", UseMnemonic = false, Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 28 };
            stepContentHost.Controls.Add(lblTitle);

            Panel formBox = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent, Padding = new Padding(0, 8, 0, 0) };

            // Hiển thị lựa chọn khách hàng
            bool isCustomerRole = Session.AppSession.Current != null && Session.AppSession.Current.IsCustomer;
            if (isCustomerRole)
            {
                selectedCustomerId = Session.AppSession.Current.KhachHangID ?? 0;
                selectedCustomerName = Session.AppSession.Current.HoTen;
                Panel infoBox = Ui.Notice("THÔNG TIN KHÁCH HÀNG", $"Khách hàng: {selectedCustomerName} • Đặt trực tuyến qua tài khoản hội viên.\nĐơn đặt sân sẽ được giữ chỗ trong 15 phút để hoàn tất tiền cọc.", Colors.Emerald, Colors.SuccessBg);
                infoBox.Dock = DockStyle.Top;
                formBox.Controls.Add(infoBox);
            }
            else
            {
                Panel custSelectPanel = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.Transparent };
                Label lblCust = new Label { Text = "Khách hàng đặt sân *:", Font = Typography.SecondaryBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 6), AutoSize = true };
                ComboBox cboCust = new ComboBox { Location = new Point(160, 2), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList, Font = Typography.Body };

                try
                {
                    DataTable dtCust = DatabaseManager.ExecuteQuery("SELECT KhachHangID, HoTen, SoDienThoai FROM dbo.KhachHang WHERE DangHoatDong = 1 ORDER BY HoTen");
                    if (dtCust != null && dtCust.Rows.Count > 0)
                    {
                        dtCust.Columns.Add("DisplayText", typeof(string), "HoTen + ' (' + SoDienThoai + ')'");
                        cboCust.DataSource = dtCust;
                        cboCust.DisplayMember = "DisplayText";
                        cboCust.ValueMember = "KhachHangID";

                        if (selectedCustomerId > 0)
                        {
                            cboCust.SelectedValue = selectedCustomerId;
                        }
                        else
                        {
                            selectedCustomerId = Convert.ToInt32(dtCust.Rows[0]["KhachHangID"]);
                            selectedCustomerName = dtCust.Rows[0]["HoTen"]?.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helpers.AppLogger.LogError(ex, "QUICK_BOOKING_LOAD_CUSTOMERS");
                }

                cboCust.SelectedIndexChanged += (s, e) =>
                {
                    if (cboCust.SelectedValue != null && int.TryParse(cboCust.SelectedValue.ToString(), out int cId))
                    {
                        selectedCustomerId = cId;
                        if (cboCust.SelectedItem is DataRowView drv)
                        {
                            selectedCustomerName = drv["HoTen"]?.ToString();
                        }
                        LoadVoucherChoices(formBox);
                    }
                };

                custSelectPanel.Controls.Add(lblCust);
                custSelectPanel.Controls.Add(cboCust);
                formBox.Controls.Add(custSelectPanel);
            }

            // Voucher ComboBox Panel
            Panel voucherPanel = new Panel { Name = "pnlVoucher", Dock = DockStyle.Top, Height = 56, BackColor = Color.Transparent };
            Label lblVoucher = new Label { Text = "Mã giảm giá (Voucher):", Font = Typography.SecondaryBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 6), AutoSize = true };
            ComboBox cboVoucher = new ComboBox { Name = "cboVoucher", Location = new Point(160, 2), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList, Font = Typography.Body };
            voucherPanel.Controls.Add(lblVoucher);
            voucherPanel.Controls.Add(cboVoucher);
            formBox.Controls.Add(voucherPanel);

            // Ghi chú
            Panel notePanel = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.Transparent };
            Label l1 = new Label { Text = "Ghi chú đặt sân của khách:", Font = Typography.SecondaryBold, ForeColor = Colors.TextSecondary, Dock = DockStyle.Top, Height = 22 };
            TextBox txtNote = new TextBox { Text = bookingNote, Font = Typography.Body, Multiline = true, Height = 60, Dock = DockStyle.Top };
            txtNote.TextChanged += (s, e) => bookingNote = txtNote.Text;
            notePanel.Controls.Add(txtNote);
            notePanel.Controls.Add(l1);
            formBox.Controls.Add(notePanel);

            stepContentHost.Controls.Add(formBox);
            formBox.BringToFront();

            LoadVoucherChoices(formBox);
        }

        private void LoadVoucherChoices(Control host)
        {
            ComboBox cboVoucher = host.Controls.Find("cboVoucher", true).Length > 0 
                ? host.Controls.Find("cboVoucher", true)[0] as ComboBox 
                : null;
            if (cboVoucher == null) return;

            cboVoucher.Items.Clear();
            cboVoucher.Items.Add("(Không áp dụng voucher)");
            cboVoucher.SelectedIndex = 0;
            voucherDiscount = 0;
            selectedVoucherKhachHangId = null;

            if (selectedCustomerId <= 0)
            {
                UpdateSummaryDisplay();
                return;
            }

            try
            {
                string sql = "SELECT vk.VoucherKhachHangID, v.TenVoucher, v.MaVoucher, v.LoaiGiam, v.GiaTriGiam, v.GiamToiDa " +
                             "FROM dbo.VoucherKhachHang vk " +
                             "INNER JOIN dbo.Voucher v ON vk.VoucherID = v.VoucherID " +
                             "WHERE vk.KhachHangID = @KH AND vk.DaSuDung = 0 " +
                             "AND CAST(GETDATE() AS DATE) BETWEEN CAST(v.NgayBatDau AS DATE) AND CAST(v.NgayKetThuc AS DATE)";
                DataTable dtVoucher = DatabaseManager.ExecuteQuery(sql, new SqlParameter("@KH", selectedCustomerId));

                if (dtVoucher != null && dtVoucher.Rows.Count > 0)
                {
                    foreach (DataRow r in dtVoucher.Rows)
                    {
                        long vkhId = Convert.ToInt64(r["VoucherKhachHangID"]);
                        string code = r["MaVoucher"]?.ToString();
                        string name = r["TenVoucher"]?.ToString();
                        decimal val = Convert.ToDecimal(r["GiaTriGiam"]);
                        string loai = r["LoaiGiam"]?.ToString();
                        decimal? maxGiam = r["GiamToiDa"] != DBNull.Value ? (decimal?)Convert.ToDecimal(r["GiamToiDa"]) : null;

                        string itemText = $"{code} - {name} (Giảm {(loai == "PHAN_TRAM" ? val + "%" : val.ToString("N0") + "đ")})";
                        cboVoucher.Items.Add(new VoucherComboItem(vkhId, itemText, loai, val, maxGiam));
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_VOUCHER_CHOICES");
            }

            cboVoucher.SelectedIndexChanged += (s, e) =>
            {
                if (cboVoucher.SelectedItem is VoucherComboItem vItem)
                {
                    selectedVoucherKhachHangId = vItem.VoucherKhachHangID;
                    if (vItem.LoaiGiam == "PHAN_TRAM")
                    {
                        decimal d = fieldPrice * vItem.GiaTriGiam / 100m;
                        if (vItem.GiamToiDa.HasValue && d > vItem.GiamToiDa.Value)
                            d = vItem.GiamToiDa.Value;
                        voucherDiscount = (int)d;
                    }
                    else
                    {
                        voucherDiscount = (int)vItem.GiaTriGiam;
                    }
                }
                else
                {
                    selectedVoucherKhachHangId = null;
                    voucherDiscount = 0;
                }
                UpdateSummaryDisplay();
            };

            UpdateSummaryDisplay();
        }

        private class VoucherComboItem
        {
            public long VoucherKhachHangID { get; }
            public string DisplayText { get; }
            public string LoaiGiam { get; }
            public decimal GiaTriGiam { get; }
            public decimal? GiamToiDa { get; }

            public VoucherComboItem(long id, string text, string loai, decimal val, decimal? max)
            {
                VoucherKhachHangID = id;
                DisplayText = text;
                LoaiGiam = loai;
                GiaTriGiam = val;
                GiamToiDa = max;
            }

            public override string ToString() => DisplayText;
        }

        private void BuildSummaryCard()
        {
            Label head = new Label { Text = "Tóm tắt đơn đặt", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            summaryCard.Controls.Add(head);

            Panel details = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            lblSumField = new Label { Text = selectedField, Font = Typography.BodyBold, ForeColor = Colors.TextPrimary, Location = new Point(0, 10), AutoSize = true };
            lblSumTime = new Label { Text = $"{selectedSport} • {selectedDate} • {selectedTime}", Font = Typography.Secondary, ForeColor = Colors.TextSecondary, Location = new Point(0, 32), AutoSize = true };
            details.Controls.Add(lblSumField);
            details.Controls.Add(lblSumTime);

            Panel sep1 = new Panel { Location = new Point(0, 64), Size = new Size(240, 1), BackColor = Colors.Border };
            details.Controls.Add(sep1);

            int y = 78;
            AddCostLine(details, "Tiền sân", ref lblSumFieldCost, y); y += 28;
            AddCostLine(details, "Dịch vụ", ref lblSumServiceCost, y); y += 28;
            AddCostLine(details, "Voucher giảm", ref lblSumVoucher, y, true); y += 38;

            Panel sep2 = new Panel { Location = new Point(0, y), Size = new Size(240, 1), BackColor = Colors.Border };
            details.Controls.Add(sep2); y += 14;

            Label lblTotalCaption = new Label { Text = "TỔNG THANH TOÁN", Font = Typography.SectionHeader, ForeColor = Colors.TextSecondary, Location = new Point(0, y), AutoSize = true };
            y += 20;
            lblSumTotal = new Label { Text = "0 đ", Font = Typography.KpiValue, ForeColor = Colors.DeepEmerald, Location = new Point(0, y), AutoSize = true };
            details.Controls.Add(lblTotalCaption);
            details.Controls.Add(lblSumTotal);

            summaryCard.Controls.Add(details);
            details.BringToFront();
        }

        private void AddCostLine(Panel p, string title, ref Label valLabel, int y, bool isNegative = false)
        {
            Label t = new Label { Text = title, Font = Typography.Secondary, ForeColor = Colors.TextSecondary, Location = new Point(0, y), AutoSize = true };
            valLabel = new Label { Text = "0 đ", Font = Typography.SecondaryBold, ForeColor = isNegative ? Colors.Danger : Colors.TextPrimary, Location = new Point(140, y), AutoSize = true, TextAlign = ContentAlignment.TopRight };
            p.Controls.Add(t);
            p.Controls.Add(valLabel);
        }

        private void UpdateSummaryDisplay()
        {
            if (lblSumField != null) lblSumField.Text = selectedField;
            if (lblSumTime != null) lblSumTime.Text = $"{selectedDate} • {selectedTime}";
            if (lblSumFieldCost != null) lblSumFieldCost.Text = $"{fieldPrice:N0} đ";
            if (lblSumServiceCost != null) lblSumServiceCost.Text = $"{servicePrice:N0} đ";
            if (lblSumVoucher != null) lblSumVoucher.Text = $"-{voucherDiscount:N0} đ";

            int total = Math.Max(0, fieldPrice + servicePrice - voucherDiscount);
            if (lblSumTotal != null) lblSumTotal.Text = $"{total:N0} đ";
        }
    }
}
