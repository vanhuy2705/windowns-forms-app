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

namespace SportZone.WinForms.Dialogs
{
    public class BaseDialog : Form
    {
        protected TableLayoutPanel RootLayout;
        protected Panel HeaderPanel;
        protected Label TitleLabel;
        protected Label SubtitleLabel;
        protected Panel Body;
        protected FlowLayoutPanel Footer;

        public BaseDialog(string title, Size size)
        {
            DpiHelper.ApplyDpiMode(this);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Text = title;
            try { this.Icon = Helpers.SportFieldIconProvider.GetWindowIcon(Helpers.SportFieldIcon.App); } catch { }
            ClientSize = size;
            MinimumSize = new Size(Math.Min(420, size.Width), Math.Min(320, size.Height));
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.Sizable;

            // Root 3-row TableLayoutPanel
            RootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Colors.Canvas,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            RootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            RootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74f)); // Header
            RootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // Body Fill
            RootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f)); // Footer
            Controls.Add(RootLayout);

            // 1. Header
            HeaderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 12, 24, 10),
                Margin = new Padding(0)
            };
            HeaderPanel.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Colors.Border, 1f))
                {
                    e.Graphics.DrawLine(p, 0, HeaderPanel.Height - 1, HeaderPanel.Width, HeaderPanel.Height - 1);
                }
            };

            TitleLabel = new Label
            {
                Text = title,
                Font = Typography.PageTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                AutoSize = true,
                UseMnemonic = false
            };
            HeaderPanel.Controls.Add(TitleLabel);

            SubtitleLabel = new Label
            {
                Text = "SPORTFIELD Aurora Performance Platform",
                Font = Typography.Secondary,
                ForeColor = Colors.TextSecondary,
                Dock = DockStyle.Bottom,
                AutoSize = true
            };
            HeaderPanel.Controls.Add(SubtitleLabel);
            RootLayout.Controls.Add(HeaderPanel, 0, 0);

            // 2. Body
            Body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Canvas,
                Padding = new Padding(24),
                Margin = new Padding(0),
                AutoScroll = true
            };
            RootLayout.Controls.Add(Body, 0, 1);

            // 3. Footer
            Footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Colors.Surface,
                Padding = new Padding(24, 12, 24, 12),
                Margin = new Padding(0),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            Footer.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Colors.Border, 1f))
                {
                    e.Graphics.DrawLine(p, 0, 0, Footer.Width, 0);
                }
            };
            RootLayout.Controls.Add(Footer, 0, 2);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DpiHelper.ClampToScreen(this, 32, 48);
        }

        protected TextBox AddField(Control parent, string label, string value, int y, int width)
        {
            Label l = new Label
            {
                Text = label,
                Font = Typography.CaptionBold,
                ForeColor = Colors.TextSecondary,
                Location = new Point(0, y),
                AutoSize = true
            };
            parent.Controls.Add(l);

            SFTextBox t = new SFTextBox
            {
                Text = value,
                Location = new Point(0, y + 22),
                Width = width,
                Height = 36
            };
            parent.Controls.Add(t);
            return t.InnerTextBox;
        }

        public Func<bool> OnSaveAction { get; set; }

        protected virtual bool OnSave()
        {
            if (OnSaveAction != null) return OnSaveAction();
            return true;
        }

        protected void FooterButtons(string primary)
        {
            Footer.Controls.Clear();

            int reqW = TextRenderer.MeasureText(primary, Typography.SecondaryBold, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix).Width + 48;
            SFButton save = new SFButton
            {
                Text = primary,
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Check,
                Width = Math.Max(160, reqW),
                Height = 40,
                Margin = new Padding(8, 0, 0, 0)
            };
            bool isProcessing = false;
            save.Click += (s, e) =>
            {
                if (isProcessing) return;
                try
                {
                    isProcessing = true;
                    save.Enabled = false;
                    Cursor = Cursors.WaitCursor;
                    if (OnSave())
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
                finally
                {
                    isProcessing = false;
                    if (!IsDisposed)
                    {
                        save.Enabled = true;
                        Cursor = Cursors.Default;
                    }
                }
            };
            Footer.Controls.Add(save);

            SFButton cancel = new SFButton
            {
                Text = "Hủy Bỏ (Esc)",
                Variant = SFButtonVariant.Secondary,
                Width = 120,
                Height = 40,
                Margin = new Padding(0, 0, 0, 0)
            };
            cancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Footer.Controls.Add(cancel);

            AcceptButton = save;
            CancelButton = cancel;
        }
    }

    public class AddFieldForm : BaseDialog
    {
        protected TextBox txtMaSan;
        protected TextBox txtTenSan;
        protected TextBox txtGia;
        protected SFComboBox stateCombo;
        protected int? fieldId = null;
        protected int loaiSanId = 1;
        protected int cumSanId = 1;
        private readonly Services.FieldService _fieldService = new Services.FieldService();

        public AddFieldForm() : base("Thêm Mới Sân Vào Hệ Thống", new Size(860, 700))
        {
            Build();
            FooterButtons("Lưu Dữ Liệu Sân");
        }

        public AddFieldForm(int sanId, string maSan, string tenSan, decimal gia, string trangThai) : base("Cập Nhật Thông Tin Sân", new Size(860, 700))
        {
            fieldId = sanId;
            Build();
            if (txtMaSan != null) txtMaSan.Text = maSan;
            if (txtTenSan != null) txtTenSan.Text = tenSan;
            if (txtGia != null) txtGia.Text = gia.ToString("0");
            FooterButtons("Cập Nhật Dữ Liệu Sân");
        }

        private void Build()
        {
            TableLayoutPanel split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46f));

            // Left: Info
            SFCard left = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(18), Margin = new Padding(0, 0, 10, 0) };
            Label lTitle = new Label { Text = "Thông Tin Kỹ Thuật Cơ Bản", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            left.Controls.Add(lTitle);

            Panel p1 = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            txtMaSan = AddField(p1, "Mã định danh sân *", "SAN-BD-07", 10, 340);
            txtTenSan = AddField(p1, "Tên sân hiển thị *", "Sân bóng đá A3 (Cỏ Pro)", 74, 340);
            AddField(p1, "Môn thể thao *", "Bóng đá mini 7 người", 138, 340);
            txtGia = AddField(p1, "Đơn giá cơ bản / giờ (VNĐ) *", "350000", 202, 340);

            Label lblState = new Label { Text = "Trạng thái vận hành", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 268), AutoSize = true };
            p1.Controls.Add(lblState);
            stateCombo = new SFComboBox { Location = new Point(0, 290), Width = 340, Height = 36 };
            stateCombo.Items.AddRange(new object[] { "Đang hoạt động (Sẵn sàng)", "Bảo trì nâng cấp", "Tạm ngừng nhận khách" });
            stateCombo.SelectedIndex = 0;
            p1.Controls.Add(stateCombo);

            left.Controls.Add(p1);
            p1.BringToFront();
            split.Controls.Add(left, 0, 0);

            // Right: Image & Amenities
            SFCard right = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(18), Margin = new Padding(10, 0, 0, 0) };
            Label rTitle = new Label { Text = "Hình Ảnh & Tiện Ích Kèm Theo", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32, UseMnemonic = false };
            right.Controls.Add(rTitle);

            Panel p2 = new Panel { Dock = DockStyle.Fill };
            Panel imgPreview = new Panel
            {
                Location = new Point(0, 10),
                Size = new Size(330, 160),
                BackColor = Colors.SurfaceAlt
            };
            imgPreview.Paint += (s, e) =>
            {
                Image img = AssetManager.GetFootballPitch();
                if (img != null) GraphicsUtility.DrawImageCenterCrop(e.Graphics, img, imgPreview.ClientRectangle);
            };
            p2.Controls.Add(imgPreview);

            Label lblAmenities = new Label { Text = "Danh mục tiện ích sẵn có:", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 185), AutoSize = true };
            p2.Controls.Add(lblAmenities);

            CheckedListBox cl = new CheckedListBox
            {
                Location = new Point(0, 210),
                Size = new Size(330, 160),
                CheckOnClick = true,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            cl.Items.AddRange(new object[] { "Hệ thống đèn LED 400 lux", "Wifi băng thông cao", "Phòng thay đồ & Tắm nóng lạnh", "Bãi đỗ xe ô tô an ninh", "Nước uống miễn phí", "Mái che di động" });
            cl.SetItemChecked(0, true);
            cl.SetItemChecked(1, true);
            cl.SetItemChecked(2, true);
            p2.Controls.Add(cl);

            right.Controls.Add(p2);
            p2.BringToFront();
            split.Controls.Add(right, 1, 0);

            Body.Controls.Add(split);
        }

        protected override bool OnSave()
        {
            string maSan = txtMaSan?.Text?.Trim() ?? "";
            string tenSan = txtTenSan?.Text?.Trim() ?? "";
            string giaStr = txtGia?.Text?.Trim() ?? "0";

            if (string.IsNullOrWhiteSpace(maSan))
            {
                SFToast.Show(this, "Vui lòng nhập mã định danh sân.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
            if (string.IsNullOrWhiteSpace(tenSan))
            {
                SFToast.Show(this, "Vui lòng nhập tên sân hiển thị.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
            if (!decimal.TryParse(giaStr, out decimal gia) || gia <= 0)
            {
                SFToast.Show(this, "Đơn giá cơ bản phải là số dương hợp lệ.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }

            string trangThai = "SAN_SANG";
            if (stateCombo != null)
            {
                if (stateCombo.SelectedIndex == 1) trangThai = "BAO_TRI";
                else if (stateCombo.SelectedIndex == 2) trangThai = "TAM_NGUNG";
            }

            try
            {
                _fieldService.SaveField(fieldId, maSan, tenSan, loaiSanId, cumSanId, "Khu A", gia, trangThai, "", true);
                SFToast.Show(this, "Đã lưu thông tin sân thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "SAVE_FIELD");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI LƯU SÂN", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class EditFieldForm : AddFieldForm
    {
        public EditFieldForm(string name) : base()
        {
            Text = "Cập Nhật Thông Tin Sân";
            TitleLabel.Text = "Cập Nhật Thông Tin: " + name;
            SubtitleLabel.Text = "Chỉnh sửa quy chuẩn kỹ thuật và đơn giá của sân thi đấu.";
        }

        public EditFieldForm(int sanId, string maSan, string tenSan, decimal gia, string trangThai)
            : base(sanId, maSan, tenSan, gia, trangThai)
        {
            Text = "Cập Nhật Thông Tin Sân";
            TitleLabel.Text = "Cập Nhật Thông Tin: " + tenSan;
            SubtitleLabel.Text = "Chỉnh sửa quy chuẩn kỹ thuật và đơn giá của sân thi đấu.";
        }
    }

    public class FieldDetailForm : BaseDialog
    {
        [Obsolete("Sử dụng constructor có tên sân cụ thể.", false)]
        public FieldDetailForm() : this("") { }

        public FieldDetailForm(string name) : base("Chi Tiết Sân Thể Thao: " + (string.IsNullOrEmpty(name) ? "Tất Cả" : name), new Size(980, 720))
        {
            Build(name);
            FooterButtons("Đóng");
        }

        private void Build(string name)
        {
            SFCard top = new SFCard { Dock = DockStyle.Top, Height = 140, Padding = new Padding(18) };
            Label n = new Label { Text = string.IsNullOrEmpty(name) ? "Sân Thể Thao" : name, Font = Typography.HeroTitle, ForeColor = Colors.TextPrimary, Location = new Point(16, 14), AutoSize = true };
            top.Controls.Add(n);

            string fieldDetails = "Đang tải thông tin sân...";
            try
            {
                DataTable dtField = DatabaseManager.ExecuteQuery(
                    "SELECT TOP 1 s.TenSan, c.TenCumSan, l.TenLoaiSan, s.GiaCoBan, s.TrangThaiVanHanh, s.MoTa " +
                    "FROM dbo.SanTheThao s " +
                    "LEFT JOIN dbo.CumSan c ON s.CumSanID = c.CumSanID " +
                    "LEFT JOIN dbo.LoaiSan l ON s.LoaiSanID = l.LoaiSanID " +
                    "WHERE s.TenSan = @TenSan",
                    new SqlParameter("@TenSan", name ?? ""));

                if (dtField != null && dtField.Rows.Count > 0)
                {
                    DataRow fr = dtField.Rows[0];
                    string cum = fr["TenCumSan"]?.ToString() ?? "Cụm Sân";
                    string loai = fr["TenLoaiSan"]?.ToString() ?? "Loại Sân";
                    decimal gia = fr["GiaCoBan"] != DBNull.Value ? Convert.ToDecimal(fr["GiaCoBan"]) : 0;
                    string tt = fr["TrangThaiVanHanh"]?.ToString() ?? "SAN_SANG";
                    string desc = fr["MoTa"]?.ToString() ?? "Đang hoạt động ổn định";
                    fieldDetails = $"{cum}  •  {loai}  •  {gia:N0} đ/giờ  •  Trạng thái: {tt}\nMô tả & Tiện ích: {desc}";
                }
                else
                {
                    fieldDetails = "Không tìm thấy thông tin chi tiết của sân trong hệ thống.";
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "FIELD_DETAIL_LOAD");
                fieldDetails = "Lỗi khi tải thông tin chi tiết sân thể thao.";
            }

            Label info = new Label
            {
                Text = fieldDetails,
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Location = new Point(18, 54),
                Size = new Size(750, 48),
                AutoSize = false
            };
            top.Controls.Add(info);
            Body.Controls.Add(top);

            Panel spacer = new Panel { Dock = DockStyle.Top, Height = 14 };
            Body.Controls.Add(spacer);

            SFCard tabsCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16) };
            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Font = Typography.SecondaryBold };
            tabs.TabPages.Add("Lịch thi đấu hôm nay");
            tabs.TabPages.Add("Hiệu suất & Doanh thu");
            tabs.TabPages.Add("Bảo trì định kỳ");

            SFDataGridView g = new SFDataGridView { Dock = DockStyle.Fill };
            g.Columns.Add("Slot", "Khung giờ");
            g.Columns.Add("Customer", "Khách hàng");
            g.Columns.Add("Code", "Mã đơn");
            g.Columns.Add("Status", "Trạng thái");

            g.Columns["Slot"].Width = 140;
            g.Columns["Customer"].Width = 200;
            g.Columns["Code"].Width = 130;
            g.Columns["Status"].Width = 120;

            try
            {
                DataTable dtBookings = DatabaseManager.ExecuteQuery(
                    "SELECT BatDau, KetThuc, TenKhachHang, MaDon, TrangThaiDon " +
                    "FROM dbo.vw_DonDatSanDayDu " +
                    "WHERE TenSan = @TenSan AND CAST(BatDau AS DATE) = CAST(GETDATE() AS DATE) " +
                    "ORDER BY BatDau",
                    new SqlParameter("@TenSan", name ?? ""));

                if (dtBookings != null)
                {
                    foreach (DataRow r in dtBookings.Rows)
                    {
                        DateTime bd = Convert.ToDateTime(r["BatDau"]);
                        DateTime kt = Convert.ToDateTime(r["KetThuc"]);
                        string slot = $"{bd:HH:mm} - {kt:HH:mm}";
                        string cust = r["TenKhachHang"]?.ToString() ?? "";
                        string code = r["MaDon"]?.ToString() ?? "";
                        string status = r["TrangThaiDon"]?.ToString() ?? "";
                        g.Rows.Add(slot, cust, code, status);
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "FIELD_DETAIL_SCHEDULE_LOAD");
            }

            tabs.TabPages[0].Controls.Add(g);
            tabsCard.Controls.Add(tabs);
            Body.Controls.Add(tabsCard);

            tabsCard.BringToFront();
            spacer.BringToFront();
            top.BringToFront();
        }
    }

    public class BookingDetailForm : BaseDialog
    {
        private long _donDatSanId = 0;
        private string _bookingCode = "BK-2026-1094";
        private decimal _totalAmount = 0;
        private decimal _paidAmount = 0;
        private decimal _remainingAmount = 0;
        private string _customerName = "";
        private readonly Services.BookingService _bookingService = new Services.BookingService();

        [Obsolete("Sử dụng constructor có mã đơn hoặc ID thực tế.", false)]
        public BookingDetailForm() : this("")
        {
        }

        public BookingDetailForm(long donDatSanId) : this(GetBookingCode(donDatSanId))
        {
        }

        private static string GetBookingCode(long id)
        {
            try
            {
                var obj = DatabaseManager.ExecuteScalar("SELECT MaDon FROM dbo.DonDatSan WHERE DonDatSanID = @ID", new System.Data.SqlClient.SqlParameter("@ID", id));
                if (obj != null) return obj.ToString();
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning("GetBookingCode failed: " + ex.Message);
            }
            return "BK-" + id;
        }

        public BookingDetailForm(string bookingCode) : base("Chi Tiết Đơn Đặt Sân: " + (bookingCode ?? "Chưa xác định"), new Size(960, 700))
        {
            _bookingCode = bookingCode ?? "";
            Build();
            FooterButtons("XÁC NHẬN THANH TOÁN & TRẢ SÂN");
        }

        private void Build()
        {
            string custInfo = "Không tìm thấy thông tin đơn đặt sân: " + _bookingCode;
            decimal tienSan = 0, tienDv = 0, giamGia = 0;

            try
            {
                var p = new System.Data.SqlClient.SqlParameter("@MaDon", SqlDbType.VarChar) { Value = _bookingCode };
                DataTable dt = DatabaseManager.ExecuteQuery("SELECT TOP 1 * FROM dbo.vw_DonDatSanDayDu WHERE MaDon = @MaDon", p);
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    _donDatSanId = Convert.ToInt64(r["DonDatSanID"]);
                    _customerName = r["TenKhachHang"]?.ToString() ?? "";
                    string phone = r["SoDienThoai"]?.ToString() ?? "";
                    string sanName = r["TenSan"]?.ToString() ?? "";
                    string cum = r["TenCumSan"]?.ToString() ?? "";
                    DateTime bd = Convert.ToDateTime(r["BatDau"]);
                    DateTime kt = Convert.ToDateTime(r["KetThuc"]);
                    _totalAmount = Convert.ToDecimal(r["TongTien"]);
                    _paidAmount = Convert.ToDecimal(r["DaThanhToan"]);
                    _remainingAmount = Convert.ToDecimal(r["ConLai"]);
                    tienSan = Convert.ToDecimal(r["TienSan"]);
                    tienDv = Convert.ToDecimal(r["TienDichVu"]);
                    giamGia = Convert.ToDecimal(r["TienVoucher"]) + Convert.ToDecimal(r["TienKhuyenMai"]);

                    string dvList = "";
                    try
                    {
                        DataTable dtServices = DatabaseManager.ExecuteQuery(
                            "SELECT sp.TenSanPhamDichVu, ct.SoLuong, ct.ThanhTien " +
                            "FROM dbo.ChiTietDichVuDatSan ct " +
                            "INNER JOIN dbo.SanPhamDichVu sp ON ct.SanPhamDichVuID = sp.SanPhamDichVuID " +
                            "WHERE ct.DonDatSanID = @DonID",
                            new System.Data.SqlClient.SqlParameter("@DonID", _donDatSanId));
                        if (dtServices != null && dtServices.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dtServices.Rows)
                            {
                                dvList += $"\n• {dr["TenSanPhamDichVu"]} x{Convert.ToDecimal(dr["SoLuong"]):0.##}: {Convert.ToDecimal(dr["ThanhTien"]):N0}đ";
                            }
                        }
                        else
                        {
                            dvList = "\n• (Không có dịch vụ đi kèm)";
                        }
                    }
                    catch { dvList = "\n• (Không tải được chi tiết dịch vụ)"; }

                    custInfo = string.Format(
                        "Mã đơn: {0}\nKhách hàng: {1}\nSố điện thoại: {2}\nSân thi đấu: {3} ({4})\nThời gian: {5:HH:mm} - {6:HH:mm} • Ngày {5:dd/MM/yyyy}\nTrạng thái đơn: {7}\nTrạng thái thanh toán: {8}\n\nCHI TIẾT DỊCH VỤ SỬ DỤNG:{9}",
                        _bookingCode, _customerName, phone, sanName, cum, bd, kt, r["TrangThaiDon"], r["TrangThaiThanhToan"], dvList);
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_BOOKING_DETAIL");
            }

            TableLayoutPanel sp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            sp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            sp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

            // Left
            SFCard left = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(18), Margin = new Padding(0, 0, 10, 0) };
            Label lTitle = new Label { Text = "Thông Tin Đơn Đặt & Dịch Vụ Đi Kèm", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32, UseMnemonic = false };
            left.Controls.Add(lTitle);

            Label txt = new Label
            {
                Text = custInfo,
                Font = Typography.Body,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Fill,
                Padding = new Padding(4, 8, 4, 4)
            };
            left.Controls.Add(txt);
            txt.BringToFront();
            sp.Controls.Add(left, 0, 0);

            // Right
            SFCard right = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(18), Margin = new Padding(10, 0, 0, 0) };
            Label rTitle = new Label { Text = "Quyết Toán Tài Chính", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            right.Controls.Add(rTitle);

            Panel rBody = new Panel { Dock = DockStyle.Fill };
            Label s = new Label
            {
                Text = string.Format(
                    "Tiền thuê sân:                 {0,12:N0}đ\nDịch vụ đi kèm:                {1,12:N0}đ\nGiảm giá / Voucher:           -{2,12:N0}đ\n──────────────────────────────\nTỔNG CỘNG:                     {3,12:N0}đ\nĐã thanh toán (Cọc):          -{4,12:N0}đ\n\nCÒN PHẢI THU:\n{5:N0} đ",
                    tienSan, tienDv, giamGia, _totalAmount, _paidAmount, _remainingAmount),
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Location = new Point(4, 8),
                Size = new Size(320, 240),
                AutoSize = false
            };
            rBody.Controls.Add(s);

            SFButton btnQr = new SFButton
            {
                Text = "Thanh Toán VietQR Napas",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.QrCode,
                Location = new Point(4, 260),
                Size = new Size(270, 42)
            };
            btnQr.Click += (sender, e) =>
            {
                long hoaDonId = 0;
                if (_donDatSanId > 0)
                {
                    var objHd = DatabaseManager.ExecuteScalar("SELECT TOP 1 HoaDonID FROM dbo.HoaDon WHERE DonDatSanID = @ID", new System.Data.SqlClient.SqlParameter("@ID", _donDatSanId));
                    if (objHd != null && objHd != DBNull.Value)
                    {
                        hoaDonId = Convert.ToInt64(objHd);
                    }
                    else
                    {
                        try
                        {
                            var inv = new Services.PaymentService().CreateInvoice(_donDatSanId, Session.AppSession.Current != null ? Session.AppSession.Current.TaiKhoanID : (int?)null);
                            hoaDonId = inv.HoaDonID;
                        }
                        catch (Exception invEx)
                        {
                            Helpers.AppLogger.LogWarning("CreateInvoice on BookingDetailForm failed: " + invEx.Message);
                        }
                    }
                }

                if (hoaDonId > 0)
                {
                    new QrPaymentForm(_remainingAmount > 0 ? _remainingAmount : _totalAmount, _bookingCode, _customerName, hoaDonId).ShowDialog(this);
                }
                else
                {
                    SFToast.Show(this, "Không thể xác định hoặc tạo hóa đơn cho đơn đặt sân này.", "LỖI HÓA ĐƠN", SFIcon.AlertCircle, Colors.Danger);
                }
            };
            rBody.Controls.Add(btnQr);

            right.Controls.Add(rBody);
            rBody.BringToFront();
            sp.Controls.Add(right, 1, 0);

            Body.Controls.Add(sp);
        }

        protected override bool OnSave()
        {
            if (_donDatSanId > 0)
            {
                try
                {
                    _bookingService.CompleteBooking(_donDatSanId, Session.AppSession.Current.TaiKhoanID);
                    SFToast.Show(this, "Đã hoàn thành và quyết toán đơn đặt sân!", "TRẢ SÂN THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                    return true;
                }
                catch (Exception ex)
                {
                    Helpers.AppLogger.LogError(ex, "COMPLETE_BOOKING");
                    SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI QUYẾT TOÁN", SFIcon.AlertCircle, Colors.Danger);
                    return false;
                }
            }
            return true;
        }
    }

    public class CustomerDetailForm : BaseDialog
    {
        private readonly int? _khachHangId;

        public CustomerDetailForm() : this(null) { }

        public CustomerDetailForm(int? khachHangId) : base("Hồ Sơ Khách Hàng & Đội Bóng", new Size(980, 720))
        {
            _khachHangId = khachHangId;
            Build();
            FooterButtons("Đóng");
        }

        private void Build()
        {
            TableLayoutPanel split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));

            string hoTen = "Khách Hàng";
            string maKh = "KH0001";
            string tenHang = "THÀNH VIÊN";
            string sdt = "—";
            string email = "—";
            int diem = 0;
            decimal viCoc = 0;
            int totalOrders = 0;
            decimal totalSpend = 0;
            int actualId = _khachHangId ?? 0;

            try
            {
                DataTable dtKh;
                if (actualId > 0)
                {
                    dtKh = DatabaseManager.ExecuteQuery(
                        "SELECT TOP 1 kh.KhachHangID, kh.MaKhachHang, kh.HoTen, kh.SoDienThoai, kh.Email, kh.DiemTichLuy, kh.SoDuViCoc, h.TenHang, kh.DangHoatDong " +
                        "FROM dbo.KhachHang kh LEFT JOIN dbo.HangThanhVien h ON kh.HangThanhVienID = h.HangThanhVienID WHERE kh.KhachHangID = @ID",
                        new System.Data.SqlClient.SqlParameter("@ID", actualId));
                }
                else
                {
                    dtKh = DatabaseManager.ExecuteQuery(
                        "SELECT TOP 1 kh.KhachHangID, kh.MaKhachHang, kh.HoTen, kh.SoDienThoai, kh.Email, kh.DiemTichLuy, kh.SoDuViCoc, h.TenHang, kh.DangHoatDong " +
                        "FROM dbo.KhachHang kh LEFT JOIN dbo.HangThanhVien h ON kh.HangThanhVienID = h.HangThanhVienID ORDER BY kh.KhachHangID ASC");
                }

                if (dtKh != null && dtKh.Rows.Count > 0)
                {
                    DataRow r = dtKh.Rows[0];
                    actualId = Convert.ToInt32(r["KhachHangID"]);
                    hoTen = r["HoTen"]?.ToString() ?? hoTen;
                    maKh = r["MaKhachHang"]?.ToString() ?? maKh;
                    tenHang = r["TenHang"]?.ToString() ?? tenHang;
                    sdt = r["SoDienThoai"]?.ToString() ?? sdt;
                    email = r["Email"]?.ToString() ?? email;
                    diem = r["DiemTichLuy"] != DBNull.Value ? Convert.ToInt32(r["DiemTichLuy"]) : 0;
                    viCoc = r["SoDuViCoc"] != DBNull.Value ? Convert.ToDecimal(r["SoDuViCoc"]) : 0;
                }

                if (actualId > 0)
                {
                    DataTable dtStats = DatabaseManager.ExecuteQuery(
                        "SELECT COUNT(DonDatSanID) AS TotalOrders, ISNULL(SUM(TongTien), 0) AS TotalSpend FROM dbo.DonDatSan WHERE KhachHangID = @ID",
                        new System.Data.SqlClient.SqlParameter("@ID", actualId));
                    if (dtStats != null && dtStats.Rows.Count > 0)
                    {
                        totalOrders = dtStats.Rows[0]["TotalOrders"] != DBNull.Value ? Convert.ToInt32(dtStats.Rows[0]["TotalOrders"]) : 0;
                        totalSpend = dtStats.Rows[0]["TotalSpend"] != DBNull.Value ? Convert.ToDecimal(dtStats.Rows[0]["TotalSpend"]) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "CUSTOMER_DETAIL", "LoadInfo");
            }

            // Left Pass
            SFCard left = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(18), Margin = new Padding(0, 0, 10, 0) };
            Label name = new Label { Text = hoTen, Font = Typography.PageTitle, ForeColor = Colors.TextPrimary, Location = new Point(8, 14), AutoSize = true };
            left.Controls.Add(name);

            Label badge = new Label
            {
                Text = "★ " + tenHang.ToUpper() + " • " + maKh,
                Font = Typography.CaptionBold,
                ForeColor = Colors.Purple,
                BackColor = Colors.PurpleBg,
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(8, 48),
                AutoSize = true
            };
            left.Controls.Add(badge);

            Label info = new Label
            {
                Text = string.Format(
                    "Số điện thoại: {0}\nEmail: {1}\n\nTổng lượt đặt:    {2} trận\nTổng chi tiêu:    {3:N0}đ\nSố dư ví cọc:     {4:N0}đ\nĐiểm tích lũy:    {5} pts\n\nTrạng thái: Đang hoạt động",
                    sdt, email, totalOrders, totalSpend, viCoc, diem),
                Font = Typography.Body,
                ForeColor = Colors.TextSecondary,
                Location = new Point(8, 90),
                Size = new Size(260, 260),
                AutoSize = false
            };
            left.Controls.Add(info);
            split.Controls.Add(left, 0, 0);

            // Right: History Grid
            SFCard right = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(10, 0, 0, 0) };
            Label rTitle = new Label { Text = "Lịch Sử Đặt Sân & Giao Dịch Gần Nhất (SQL Server)", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            right.Controls.Add(rTitle);

            SFDataGridView g = new SFDataGridView { Dock = DockStyle.Fill };
            g.Columns.Add("Code", "Mã đơn");
            g.Columns.Add("Field", "Sân thi đấu");
            g.Columns.Add("Date", "Ngày");
            g.Columns.Add("Total", "Tổng tiền");
            g.Columns.Add("Status", "Trạng thái");

            g.Columns["Code"].Width = 120;
            g.Columns["Field"].Width = 140;
            g.Columns["Date"].Width = 100;
            g.Columns["Total"].Width = 120;
            g.Columns["Status"].Width = 120;

            if (actualId > 0)
            {
                try
                {
                    DataTable dtHistory = DatabaseManager.ExecuteQuery(
                        "SELECT TOP 15 d.MaDon, s.TenSan, d.NgayGioBatDau, d.TongTien, d.TrangThaiDon " +
                        "FROM dbo.DonDatSan d JOIN dbo.San s ON d.SanID = s.SanID " +
                        "WHERE d.KhachHangID = @ID ORDER BY d.DonDatSanID DESC",
                        new System.Data.SqlClient.SqlParameter("@ID", actualId));
                    if (dtHistory != null && dtHistory.Rows.Count > 0)
                    {
                        foreach (DataRow r in dtHistory.Rows)
                        {
                            string code = r["MaDon"]?.ToString() ?? "";
                            string field = r["TenSan"]?.ToString() ?? "";
                            DateTime dtStart = r["NgayGioBatDau"] != DBNull.Value ? Convert.ToDateTime(r["NgayGioBatDau"]) : DateTime.Today;
                            decimal tt = r["TongTien"] != DBNull.Value ? Convert.ToDecimal(r["TongTien"]) : 0;
                            string st = r["TrangThaiDon"]?.ToString() ?? "";
                            string stDisp = "Đã xác nhận";
                            if (st == "DANG_SU_DUNG") stDisp = "Đang dùng";
                            else if (st == "HOAN_THANH") stDisp = "Hoàn thành";
                            else if (st == "DA_HUY") stDisp = "Đã hủy";

                            g.Rows.Add(code, field, dtStart.ToString("dd/MM/yyyy"), string.Format("{0:N0}đ", tt), stDisp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helpers.AppLogger.LogError(ex, "CUSTOMER_DETAIL", "LoadHistory");
                }
            }

            right.Controls.Add(g);
            g.BringToFront();
            split.Controls.Add(right, 1, 0);

            Body.Controls.Add(split);
        }
    }

    public class EmployeeEditForm : BaseDialog
    {
        private TextBox txtMaNv;
        private TextBox txtHoTen;
        private TextBox txtSdt;
        private TextBox txtTenDangNhap;
        private readonly int? _nhanVienId;
        private readonly Services.EmployeeService _empService = new Services.EmployeeService();

        public EmployeeEditForm() : this(null, "", "", "", "") { }

        public EmployeeEditForm(int? nhanVienId, string hoTen, string sdt, string username, string maNv)
            : base("Hồ Sơ Nhân Viên & Phân Quyền Quầy", new Size(880, 680))
        {
            _nhanVienId = nhanVienId;
            Build(hoTen, sdt, username, maNv);
            FooterButtons("Lưu & Đóng (F2)");
        }

        private void Build(string hoTen, string sdt, string username, string maNv)
        {
            TableLayoutPanel split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // Left
            SFCard left = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(18), Margin = new Padding(0, 0, 10, 0) };
            Label lTitle = new Label { Text = "Thông Tin Cá Nhân & Tài Khoản", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            left.Controls.Add(lTitle);

            Panel p1 = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            txtMaNv = AddField(p1, "Mã nhân viên *", maNv ?? "NV001", 10, 340);
            txtHoTen = AddField(p1, "Họ và tên nhân viên *", hoTen ?? "", 74, 340);
            txtSdt = AddField(p1, "Số điện thoại di động *", sdt ?? "", 138, 340);
            txtTenDangNhap = AddField(p1, "Tên đăng nhập hệ thống *", username ?? "", 202, 340);
            left.Controls.Add(p1);
            p1.BringToFront();
            split.Controls.Add(left, 0, 0);

            // Right
            SFCard right = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(18), Margin = new Padding(10, 0, 0, 0) };
            Label rTitle = new Label { Text = "Ma Trận Phân Quyền Vận Hành", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            right.Controls.Add(rTitle);

            CheckedListBox rights = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                Font = Typography.Body,
                BorderStyle = BorderStyle.FixedSingle
            };
            rights.Items.AddRange(new object[] {
                "Đặt sân & Điều chỉnh đơn đặt",
                "Check-in & Bàn giao sân cho khách",
                "Thanh toán POS & In hóa đơn",
                "Quản lý kho F&B và dịch vụ",
                "Điều khiển hệ thống đèn sân IoT",
                "Xem thống kê doanh thu ca",
                "Xuất tệp báo cáo Excel kế toán"
            });
            rights.SetItemChecked(0, true);
            rights.SetItemChecked(1, true);
            rights.SetItemChecked(2, true);

            right.Controls.Add(rights);
            rights.BringToFront();
            split.Controls.Add(right, 1, 0);

            Body.Controls.Add(split);
        }

        protected override bool OnSave()
        {
            string name = txtHoTen?.Text?.Trim() ?? "";
            string phone = txtSdt?.Text?.Trim() ?? "";
            string user = txtTenDangNhap?.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name))
            {
                SFToast.Show(this, "Vui lòng nhập họ và tên nhân viên.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }

            try
            {
                if (_nhanVienId.HasValue && _nhanVienId.Value > 0)
                {
                    _empService.UpdateEmployee(_nhanVienId.Value, name, phone, $"{user}@sportfield.vn", "Nhân viên trực quầy", DateTime.Today, "", null, true);
                }
                else
                {
                    string tempPassword = Security.PasswordSecurity.GenerateSecureTemporaryPassword(14);
                    _empService.CreateEmployeeAndAccount(name, phone, $"{user}@sportfield.vn", "Nhân viên trực quầy", user, tempPassword);
                    MessageBox.Show(
                        $"Tài khoản nhân viên [{user}] đã được tạo thành công.\n\nMật khẩu tạm thời (chỉ hiển thị 1 lần duy nhất):\n{tempPassword}\n\nNhân viên sẽ được yêu cầu đổi mật khẩu ở lần đăng nhập đầu tiên.",
                        "MẬT KHẨU TẠM THỜI CHO NHÂN VIÊN",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                SFToast.Show(this, "Đã lưu dữ liệu nhân viên thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "SAVE_EMPLOYEE");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI LƯU NHÂN VIÊN", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class VoucherEditForm : BaseDialog
    {
        private TextBox txtMa;
        private TextBox txtTen;
        private TextBox txtGiam;
        private TextBox txtGiamMax;
        private TextBox txtDonMin;
        private readonly int? _voucherId;
        private readonly Services.VoucherService _voucherService = new Services.VoucherService();

        public VoucherEditForm() : this(null) { }

        public VoucherEditForm(int? voucherId) : base("Thiết Lập Mã Voucher / Khuyến Mãi", new Size(840, 620))
        {
            _voucherId = voucherId;
            Build();
            FooterButtons("Lưu & Kích Hoạt");
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Label title = new Label { Text = "Thông Tin & Điều Kiện Áp Dụng", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(title);

            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            txtMa = AddField(p, "Mã Voucher (Code) *", "SPORT10", 10, 360);
            txtTen = AddField(p, "Tên chương trình khuyến mãi *", "Giảm 10% sân buổi tối", 74, 360);
            txtGiam = AddField(p, "Mức giảm (%) hoặc số tiền *", "10", 138, 180);
            txtGiamMax = AddField(p, "Mức giảm tối đa (VNĐ) *", "100000", 202, 180);
            txtDonMin = AddField(p, "Giá trị đơn tối thiểu (VNĐ) *", "300000", 266, 180);

            card.Controls.Add(p);
            p.BringToFront();
            Body.Controls.Add(card);

            if (_voucherId.HasValue && _voucherId.Value > 0)
            {
                try
                {
                    DataTable dt = DatabaseManager.ExecuteQuery("SELECT TOP 1 * FROM dbo.Voucher WHERE VoucherID = @ID", new System.Data.SqlClient.SqlParameter("@ID", _voucherId.Value));
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        var r = dt.Rows[0];
                        txtMa.Text = r["MaVoucher"]?.ToString() ?? "";
                        txtTen.Text = r["TenVoucher"]?.ToString() ?? "";
                        txtGiam.Text = r["GiaTriGiam"] != DBNull.Value ? Convert.ToDecimal(r["GiaTriGiam"]).ToString("0.#") : "10";
                        txtGiamMax.Text = r["GiamToiDa"] != DBNull.Value ? Convert.ToDecimal(r["GiamToiDa"]).ToString("0.#") : "100000";
                        txtDonMin.Text = r["DonToiThieu"] != DBNull.Value ? Convert.ToDecimal(r["DonToiThieu"]).ToString("0.#") : "0";
                    }
                }
                catch (Exception ex)
                {
                    Helpers.AppLogger.LogWarning("Load Voucher details failed: " + ex.Message);
                }
            }
        }

        protected override bool OnSave()
        {
            string code = txtMa?.Text?.Trim() ?? "";
            string name = txtTen?.Text?.Trim() ?? "";
            decimal.TryParse(txtGiam?.Text?.Trim() ?? "0", out decimal giam);
            decimal.TryParse(txtGiamMax?.Text?.Trim() ?? "0", out decimal giamMax);
            decimal.TryParse(txtDonMin?.Text?.Trim() ?? "0", out decimal donMin);

            if (string.IsNullOrWhiteSpace(code))
            {
                SFToast.Show(this, "Vui lòng nhập mã voucher.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                SFToast.Show(this, "Vui lòng nhập tên chiến dịch khuyến mãi.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }

            if (giam <= 0)
            {
                SFToast.Show(this, "Mức giảm giá phải lớn hơn 0.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }

            try
            {
                _voucherService.SaveVoucher(_voucherId, code, name, "PHAN_TRAM", giam, giamMax, donMin, DateTime.Today, DateTime.Today.AddMonths(3), 100, 1, name, true);
                SFToast.Show(this, "Đã lưu và kích hoạt voucher thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "SAVE_VOUCHER");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI LƯU VOUCHER", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class QrPaymentForm : BaseDialog
    {
        private readonly decimal _tongTien;
        private readonly decimal _daTra;
        private readonly decimal _conLai;
        private readonly string _bookingCode;
        private readonly string _customerName;
        private readonly long _hoaDonId;
        private decimal _payAmount;

        private string _bankName = "CHƯA CẤU HÌNH";
        private string _bankBin = "";
        private string _stk = "";
        private string _chuTk = "CHƯA CẤU HÌNH";
        private string _prefix = "SPORTFIELD";

        private Bitmap _qrBitmap;
        private Panel _boxQr;
        private Label _lblSummary;
        private TextBox _txtAmount;
        private SFButton _btnCash;
        private SFButton _btnTransfer;
        private SFButton _btnVietQr;

        [Obsolete("Sử dụng constructor có tham số đầy đủ để tránh dữ liệu demo.", false)]
        public QrPaymentForm() : this(0, "CHƯA_CÓ_MÃ", "Chưa xác định", 0) { }

        public QrPaymentForm(decimal amount, string bookingCode, string customerName, long hoaDonId = 0)
            : this(amount, 0, amount, bookingCode, customerName, hoaDonId) { }

        public QrPaymentForm(decimal tongTien, decimal daTra, decimal conLai, string bookingCode, string customerName, long hoaDonId = 0)
            : base("Thanh Toán POS / VietQR Napas 247", new Size(780, 660))
        {
            _tongTien = tongTien > 0 ? tongTien : 0;
            _daTra = daTra >= 0 ? daTra : 0;
            _conLai = conLai > 0 ? conLai : (_tongTien - _daTra);
            _payAmount = _conLai > 0 ? _conLai : _tongTien;
            _bookingCode = string.IsNullOrEmpty(bookingCode) ? "BK-CHUA-CO-MA" : bookingCode;
            _customerName = string.IsNullOrEmpty(customerName) ? "Khách hàng" : customerName;
            _hoaDonId = hoaDonId;

            Build();
            FooterButtons("Đóng");
        }

        private void Build()
        {
            try
            {
                var dt = new Services.PaymentService().GetBankAccounts();
                if (dt != null && dt.Rows.Count > 0)
                {
                    _bankName = dt.Rows[0]["TenNganHang"]?.ToString() ?? "";
                    _bankBin = dt.Rows[0]["BankBIN"]?.ToString() ?? "";
                    _stk = dt.Rows[0]["SoTaiKhoan"]?.ToString() ?? "";
                    _chuTk = dt.Rows[0]["ChuTaiKhoan"]?.ToString() ?? "";
                    _prefix = dt.Rows[0]["PrefixNoiDung"]?.ToString() ?? "SPORTFIELD";
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning("Load BankAccounts failed: " + ex.Message);
            }

            GenerateQrImage();

            TableLayoutPanel split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0) };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46f));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54f));

            // Left: QR Card
            SFCard qrCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(0, 0, 8, 0) };
            Label bankTitle = new Label { Text = $"VIETQR PRO • {_bankName.ToUpper()}", Font = Typography.SectionHeader, ForeColor = Colors.ElectricBlue, Dock = DockStyle.Top, Height = 26 };
            qrCard.Controls.Add(bankTitle);

            _boxQr = new Panel
            {
                Size = new Size(240, 240),
                Location = new Point(35, 45),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            _boxQr.Paint += (s, e) =>
            {
                if (_qrBitmap != null)
                {
                    e.Graphics.DrawImage(_qrBitmap, 0, 0, _boxQr.Width, _boxQr.Height);
                }
                else
                {
                    using (Font f = new Font("Segoe UI", 9f, FontStyle.Regular))
                    {
                        TextRenderer.DrawText(e.Graphics, "Chưa cấu hình tài khoản\nngân hàng thụ hưởng.", f, _boxQr.ClientRectangle, Colors.Danger, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
            };
            qrCard.Controls.Add(_boxQr);

            Label timerBadge = new Label
            {
                Text = "⏱ Napas 24/7 • Quét mã tự động",
                Font = Typography.CaptionBold,
                ForeColor = Colors.Emerald,
                BackColor = Color.FromArgb(20, 16, 185, 129),
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(50, 300),
                AutoSize = true
            };
            qrCard.Controls.Add(timerBadge);

            Label lblBankDetail = new Label
            {
                Text = $"STK: {_stk}\nChủ TK: {_chuTk}\nNội dung: {_prefix} {_bookingCode.Replace("-", "")}",
                Font = Typography.Secondary,
                ForeColor = Colors.TextSecondary,
                Location = new Point(15, 335),
                Size = new Size(280, 60)
            };
            qrCard.Controls.Add(lblBankDetail);

            split.Controls.Add(qrCard, 0, 0);

            // Right: Information & Payment actions
            SFCard infoCard = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16), Margin = new Padding(8, 0, 0, 0) };
            Label iTitle = new Label { Text = "Thông Tin Hóa Đơn & Thu Tiền", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 30 };
            infoCard.Controls.Add(iTitle);

            Panel rBody = new Panel { Dock = DockStyle.Fill };

            _lblSummary = new Label
            {
                Text = $"Mã đơn: {_bookingCode}\nKhách: {_customerName}\n" +
                       $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                       $"Tổng tiền đơn:   {_tongTien:N0} đ\n" +
                       $"Đã thanh toán:  {_daTra:N0} đ\n" +
                       $"Còn nợ lại:         {_conLai:N0} đ",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Location = new Point(4, 8),
                Size = new Size(330, 120),
                AutoSize = false
            };
            rBody.Controls.Add(_lblSummary);

            Label lblInputHeader = new Label
            {
                Text = "Số tiền thanh toán đợt này (VNĐ):",
                Font = Typography.CaptionBold,
                ForeColor = Colors.ElectricBlue,
                Location = new Point(4, 136),
                AutoSize = true
            };
            rBody.Controls.Add(lblInputHeader);

            _txtAmount = new TextBox
            {
                Text = string.Format("{0:N0}", (long)_payAmount),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Location = new Point(4, 160),
                Width = 320,
                Height = 32
            };
            _txtAmount.TextChanged += (s, e) =>
            {
                string raw = _txtAmount.Text.Replace(",", "").Replace(".", "").Trim();
                if (decimal.TryParse(raw, out decimal val))
                {
                    _payAmount = val;
                    GenerateQrImage();
                    if (_boxQr != null) _boxQr.Invalidate();
                }
            };
            rBody.Controls.Add(_txtAmount);

            _btnCash = new SFButton
            {
                Text = "Thu Tiền Mặt (F8)",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Dollar,
                Location = new Point(4, 210),
                Size = new Size(320, 38)
            };
            _btnCash.Click += (s, e) => ProcessPayment("TIEN_MAT", "Thu tiền mặt tại quầy");
            rBody.Controls.Add(_btnCash);

            _btnVietQr = new SFButton
            {
                Text = "Tạo GD Chuyển Khoản / VietQR",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.QrCode,
                Location = new Point(4, 256),
                Size = new Size(320, 38)
            };
            _btnVietQr.Click += (s, e) => ProcessPayment("VIETQR", "Quét mã VietQR Napas 247");
            rBody.Controls.Add(_btnVietQr);

            _btnTransfer = new SFButton
            {
                Text = "Chuyển Khoản Trực Tiếp",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Clock,
                Location = new Point(4, 302),
                Size = new Size(320, 38)
            };
            _btnTransfer.Click += (s, e) => ProcessPayment("CHUYEN_KHOAN", "Chuyển khoản trực tiếp");
            rBody.Controls.Add(_btnTransfer);

            infoCard.Controls.Add(rBody);
            rBody.BringToFront();
            split.Controls.Add(infoCard, 1, 0);

            Body.Controls.Add(split);
        }

        private void GenerateQrImage()
        {
            try
            {
                if (string.IsNullOrEmpty(_bankBin) || string.IsNullOrEmpty(_stk) || _payAmount <= 0)
                {
                    _qrBitmap = null;
                    return;
                }
                string transferContent = $"{_prefix} {_bookingCode.Replace("-", "")}";
                _qrBitmap = Helpers.VietQRHelper.GenerateQrBitmap(_bankBin, _stk, _payAmount, transferContent, 240, 240);
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "QR_PAYMENT_GEN", "GenerateQrImage");
                _qrBitmap = null;
            }
        }

        private void ProcessPayment(string phuongThuc, string ghiChu)
        {
            if (_hoaDonId <= 0)
            {
                SFToast.Show(this, "Không tìm thấy hóa đơn hợp lệ để ghi nhận thanh toán.", "LỖI HÓA ĐƠN", SFIcon.AlertCircle, Colors.Danger);
                return;
            }

            string raw = _txtAmount.Text.Replace(",", "").Replace(".", "").Trim();
            if (!decimal.TryParse(raw, out decimal soTien) || soTien <= 0)
            {
                SFToast.Show(this, "Số tiền thanh toán phải lớn hơn 0.", "LỖI NHẬP TIỀN", SFIcon.AlertCircle, Colors.Danger);
                _txtAmount.Focus();
                return;
            }

            if (_conLai > 0 && soTien > _conLai)
            {
                SFToast.Show(this, $"Số tiền thanh toán ({soTien:N0}đ) vượt quá số tiền còn lại ({_conLai:N0}đ).", "VƯỢT QUÁ CÒN LẠI", SFIcon.AlertCircle, Colors.Warning);
                _txtAmount.Focus();
                return;
            }

            try
            {
                int userId = Security.SessionGuard.RequireAccountId();
                var payService = new Services.PaymentService();
                var pRes = payService.RecordPayment(_hoaDonId, soTien, phuongThuc, userId, "", ghiChu);
                if (pRes.GiaoDichID <= 0)
                {
                    SFToast.Show(this, "Không thể ghi nhận giao dịch thanh toán vào hệ thống.", "LỖI THANH TOÁN", SFIcon.AlertCircle, Colors.Danger);
                    return;
                }

                if (phuongThuc == "VIETQR")
                {
                    payService.CreateVietQrRequest(pRes.GiaoDichID);
                }

                string ptDesc = phuongThuc == "TIEN_MAT" ? "tiền mặt" : (phuongThuc == "VIETQR" ? "VietQR" : "chuyển khoản");
                SFToast.Show(this, $"Đã ghi nhận thanh toán {ptDesc} {soTien:N0}đ thành công!", "THANH TOÁN THÀNH CÔNG", SFIcon.Check, Colors.Emerald);

                // Gửi thông báo tới nhân viên và quản trị viên
                try
                {
                    Services.NotificationService.Instance.NotifyRole("NHAN_VIEN",
                        $"Thanh toán thành công: {soTien:N0}đ",
                        $"Hóa đơn #{_hoaDonId} cho đơn {_bookingCode} đã được thanh toán bằng {ptDesc}.",
                        "PAYMENT", "PAYMENT", _hoaDonId);

                    Services.NotificationService.Instance.NotifyRole("ADMIN",
                        $"Thanh toán thành công: {soTien:N0}đ",
                        $"Hóa đơn #{_hoaDonId} cho đơn {_bookingCode} đã được thanh toán bằng {ptDesc}.",
                        "PAYMENT", "PAYMENT", _hoaDonId);
                }
                catch (Exception notifEx)
                {
                    Helpers.AppLogger.LogWarning("ProcessPayment notification emit failed: " + notifEx.Message);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI THANH TOÁN", SFIcon.AlertCircle, Colors.Danger);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F8 && _btnCash != null)
            {
                _btnCash.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override bool OnSave()
        {
            DialogResult = DialogResult.OK;
            Close();
            return true;
        }
    }

    public class SettingsForm : BaseDialog
    {
        public SettingsForm() : base("Cấu Hình Hệ Thống & Kết Nối SQL Server", new Size(920, 680))
        {
            Build();
            FooterButtons("Áp Dụng & Lưu Cấu Hình");
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Label title = new Label { Text = "Thông Số Kết Nối Cơ Sở Dữ Liệu SQL Server", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(title);

            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            AddField(p, "Máy chủ SQL Server *", @".\SQLEXPRESS", 10, 400);
            AddField(p, "Tên Database *", "QuanLySanTheThaoDB", 74, 400);
            AddField(p, "Chế độ xác thực *", "Windows Authentication", 138, 400);

            SFButton test = new SFButton
            {
                Text = "Kiểm Tra Kết Nối (F5)",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Check,
                Location = new Point(0, 220),
                Size = new Size(220, 40)
            };
            test.Click += (s, e) =>
            {
                var res = SportZone.WinForms.Database.DatabaseManager.TestConnection();
                if (res.Success)
                {
                    SFToast.Show(this, string.Format("Kết nối CSDL {0} thành công!\n• Máy chủ: {1}\n• Độ trễ: {2} ms ({3} bảng)\n• DATA: {4}", res.DatabaseName, res.ServerName, res.LatencyMs, res.TableCount, res.DataFilePath), "SQL SERVER", SFIcon.Check, Colors.Emerald);
                }
                else
                {
                    SFToast.Show(this, "Không thể kết nối SQL Server:\n" + res.ErrorMessage, "SQL SERVER LỖI", SFIcon.AlertCircle, Colors.Danger);
                }
            };
            p.Controls.Add(test);

            card.Controls.Add(p);
            p.BringToFront();
            Body.Controls.Add(card);
        }
    }

    public class InventoryAuditForm : BaseDialog
    {
        public InventoryAuditForm() : base("Báo Cáo Kiểm Kê & Cân Đối Hao Hụt Kho", new Size(880, 620))
        {
            Build();
            FooterButtons("Cân Đối Tồn Tự Động (F9)");
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(16) };
            Label title = new Label { Text = "Bảng So Sánh Sổ Sách vs Tồn Kho Thực Tế", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(title);

            SFDataGridView g = new SFDataGridView { Dock = DockStyle.Fill };
            g.Columns.Add("Code", "Mã SKU");
            g.Columns.Add("Name", "Mặt hàng");
            g.Columns.Add("Book", "Sổ sách");
            g.Columns.Add("Actual", "Thực tế");
            g.Columns.Add("Diff", "Chênh lệch");
            g.Columns.Add("Value", "Giá trị chênh");

            g.Columns["Code"].Width = 100;
            g.Columns["Name"].Width = 220;
            g.Columns["Book"].Width = 90;
            g.Columns["Actual"].Width = 90;
            g.Columns["Diff"].Width = 100;
            g.Columns["Value"].Width = 120;

            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery("SELECT * FROM dbo.vw_TonKhoHienTai");
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        string sku = r["MaSanPhamDichVu"]?.ToString() ?? "";
                        string name = r["TenSanPhamDichVu"]?.ToString() ?? "";
                        decimal ton = r["SoLuongTon"] != DBNull.Value ? Convert.ToDecimal(r["SoLuongTon"]) : 0;
                        decimal tonMin = r["SoLuongTonToiThieu"] != DBNull.Value ? Convert.ToDecimal(r["SoLuongTonToiThieu"]) : 0;
                        decimal chenh = ton - tonMin;
                        string diffStr = chenh >= 0 ? ("+" + chenh.ToString("0.#")) : chenh.ToString("0.#");
                        g.Rows.Add(sku, name, tonMin.ToString("0.#"), ton.ToString("0.#"), diffStr, chenh >= 0 ? "Đủ hàng" : "Thiếu hàng");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_INVENTORY_AUDIT");
            }

            card.Controls.Add(g);
            g.BringToFront();
            Body.Controls.Add(card);
        }
    }

    public class CancelRefundForm : BaseDialog
    {
        private readonly long _donDatSanId;
        private SFComboBox reasonCombo;
        private readonly Services.BookingService _bookingService = new Services.BookingService();

        public CancelRefundForm() : this(0, "BK-2026-1094", "Nguyễn Hữu Đạt", 500000) { }

        public CancelRefundForm(long donDatSanId) : this(donDatSanId, GetCancelInfo(donDatSanId).MaDon, GetCancelInfo(donDatSanId).CustomerName, GetCancelInfo(donDatSanId).TienCoc) { }

        private static (string MaDon, string CustomerName, decimal TienCoc) GetCancelInfo(long id)
        {
            try
            {
                DataTable dt = DatabaseManager.ExecuteQuery(
                    "SELECT TOP 1 d.MaDon, kh.HoTen, d.DaThanhToan FROM dbo.DonDatSan d JOIN dbo.KhachHang kh ON d.KhachHangID = kh.KhachHangID WHERE d.DonDatSanID = @ID",
                    new System.Data.SqlClient.SqlParameter("@ID", id));
                if (dt != null && dt.Rows.Count > 0)
                {
                    return (dt.Rows[0]["MaDon"]?.ToString() ?? ("BK-" + id),
                            dt.Rows[0]["HoTen"]?.ToString() ?? "Khách hàng",
                            dt.Rows[0]["DaThanhToan"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["DaThanhToan"]) : 0);
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogWarning("GetCancelInfo failed: " + ex.Message);
            }
            return ("BK-" + id, "Khách hàng", 0);
        }

        public CancelRefundForm(long donDatSanId, string maDon, string customerName, decimal tienCoc)
            : base("Hủy Đơn / Đổi Lịch / Xử Lý Hoàn Cọc", new Size(780, 520))
        {
            _donDatSanId = donDatSanId;
            Build(maDon, customerName, tienCoc);
            FooterButtons("XÁC NHẬN HỦY & HOÀN TIỀN");
        }

        private void Build(string maDon, string customerName, decimal tienCoc)
        {
            decimal phiHuy = tienCoc * 0.2m;
            decimal hoanTien = Math.Max(0, tienCoc - phiHuy);

            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(20) };
            Label title = new Label { Text = $"Chi Tiết Đơn Hủy: {maDon} ({customerName})", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(title);

            Label t = new Label
            {
                Text = $"Tiền đã đặt cọc:              {tienCoc:N0}đ\nPhí hủy theo quy định (20%):   {phiHuy:N0}đ\n──────────────────────\nSỐ TIỀN DỰ KIẾN HOÀN VÍ:      {hoanTien:N0}đ\n\nLý do hủy đơn *:",
                Font = Typography.BodyBold,
                ForeColor = Colors.TextPrimary,
                Location = new Point(8, 48),
                Size = new Size(500, 130),
                AutoSize = false
            };
            card.Controls.Add(t);

            reasonCombo = new SFComboBox { Location = new Point(8, 190), Width = 380, Height = 36 };
            reasonCombo.Items.AddRange(new object[] { "Khách đổi kế hoạch đột xuất", "Thời tiết mưa bão không thể đá", "Sân gặp sự cố kỹ thuật bảo trì", "Lý do khác" });
            reasonCombo.SelectedIndex = 0;
            card.Controls.Add(reasonCombo);

            Body.Controls.Add(card);
        }

        protected override bool OnSave()
        {
            try
            {
                string lyDo = reasonCombo?.SelectedItem?.ToString() ?? "Khách yêu cầu hủy đơn";
                if (_donDatSanId > 0)
                {
                    _bookingService.CancelBooking(_donDatSanId, Session.AppSession.Current.TaiKhoanID, lyDo);
                }
                SFToast.Show(this, "Đã hủy đơn đặt sân và ghi nhận hoàn tiền theo quy định.", "HỦY ĐƠN THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "CANCEL_BOOKING");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI HỦY ĐƠN", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class ComboItem
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public string Code { get; set; }
        public override string ToString() => Text ?? "";
    }

    public class AssignVipCardForm : BaseDialog
    {
        private readonly int _khachHangId;
        private readonly string _customerName;
        private readonly string _currentTier;
        private SFComboBox cboTiers;
        private TextBox txtNote;
        private readonly Services.CustomerService _customerService = new Services.CustomerService();

        public AssignVipCardForm(int khachHangId, string customerName, string currentTier)
            : base("Gán Thẻ VIP & Hạng Thành Viên", new Size(680, 480))
        {
            _khachHangId = khachHangId;
            _customerName = customerName;
            _currentTier = currentTier;
            Build();
            FooterButtons("Cập Nhật Hạng Thẻ");
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Label title = new Label
            {
                Text = $"Khách hàng: {_customerName} • Hạng hiện tại: {_currentTier}",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 32
            };
            card.Controls.Add(title);

            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            Label l1 = new Label
            {
                Text = "Chọn Hạng Thành Viên Mới *",
                Font = Typography.CaptionBold,
                ForeColor = Colors.TextSecondary,
                Location = new Point(0, 10),
                AutoSize = true
            };
            p.Controls.Add(l1);

            cboTiers = new SFComboBox { Location = new Point(0, 32), Width = 400, Height = 36 };
            try
            {
                DataTable dt = _customerService.GetMembershipTiers();
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        cboTiers.Items.Add(new ComboItem
                        {
                            Text = $"{r["TenHang"]} ({r["MaHang"]}) - Ưu đãi {Convert.ToDecimal(r["UuDaiMacDinh"]) * 100:0.#}%",
                            Value = Convert.ToInt32(r["HangThanhVienID"])
                        });
                    }
                    if (cboTiers.Items.Count > 0) cboTiers.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_MEMBERSHIP_TIERS");
            }
            p.Controls.Add(cboTiers);

            txtNote = AddField(p, "Lý do thay đổi / Ghi chú điều chỉnh", "Nâng hạng hội viên thân thiết theo chính sách CSKH", 80, 400);

            card.Controls.Add(p);
            p.BringToFront();
            Body.Controls.Add(card);
        }

        protected override bool OnSave()
        {
            if (cboTiers?.SelectedItem == null)
            {
                SFToast.Show(this, "Vui lòng chọn hạng thành viên cần gán.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            try
            {
                int tierId = ((ComboItem)cboTiers.SelectedItem).Value;
                _customerService.AssignMembershipTier(_khachHangId, tierId, Session.AppSession.Current.TaiKhoanID);
                SFToast.Show(this, "Đã cập nhật hạng thành viên thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "ASSIGN_VIP");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI CẬP NHẬT", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class SendMessagingForm : BaseDialog
    {
        private readonly int _khachHangId;
        private readonly string _customerName;
        private readonly string _phone;
        private SFComboBox cboChannel;
        private SFComboBox cboTemplate;
        private TextBox txtContent;
        private readonly Services.MessagingService _messagingService = new Services.MessagingService();

        public SendMessagingForm(int khachHangId, string customerName, string phone)
            : base("Gửi Tin Nhắn CSKH (Zalo ZNS / SMS)", new Size(720, 560))
        {
            _khachHangId = khachHangId;
            _customerName = customerName;
            _phone = phone;
            Build();
            FooterButtons("Thêm Vào Hàng Đợi Gửi");
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Label title = new Label
            {
                Text = $"Gửi tới: {_customerName} • SĐT: {_phone}",
                Font = Typography.SectionTitle,
                ForeColor = Colors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 32
            };
            card.Controls.Add(title);

            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            Label l1 = new Label { Text = "Kênh gửi tin nhắn *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 10), AutoSize = true };
            p.Controls.Add(l1);

            cboChannel = new SFComboBox { Location = new Point(0, 32), Width = 320, Height = 36 };
            cboChannel.Items.AddRange(new object[] { "ZALO - Zalo ZNS CSKH", "SMS - Tin Nhắn SMS Brandname", "APP - Thông Báo Ứng Dụng", "EMAIL - Thư Điện Tử" });
            cboChannel.SelectedIndex = 0;
            p.Controls.Add(cboChannel);

            Label l2 = new Label { Text = "Mẫu tin nhắn mẫu (Template)", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 78), AutoSize = true };
            p.Controls.Add(l2);

            cboTemplate = new SFComboBox { Location = new Point(0, 100), Width = 460, Height = 36 };
            cboTemplate.Items.AddRange(new object[] {
                "Nhắc lịch đặt sân sắp tới",
                "Tặng voucher ưu đãi hội viên thân thiết",
                "Xác nhận thanh toán hóa đơn thành công",
                "Thông báo bảo trì hệ thống sân",
                "Nội dung tùy chỉnh"
            });
            cboTemplate.SelectedIndex = 0;
            cboTemplate.SelectedIndexChanged += (s, e) =>
            {
                switch (cboTemplate.SelectedIndex)
                {
                    case 0:
                        txtContent.Text = $"Kính gửi quý khách {_customerName}, lịch đặt sân thể thao của bạn tại SPORTFIELD sắp diễn ra. Vui lòng có mặt trước 10 phút để nhận sân thuận lợi.";
                        break;
                    case 1:
                        txtContent.Text = $"SPORTFIELD kính tặng quý khách {_customerName} mã voucher giảm giá khi đặt sân trong tuần này. Trân trọng cảm ơn quý khách!";
                        break;
                    case 2:
                        txtContent.Text = $"SPORTFIELD đã ghi nhận giao dịch thanh toán thành công của quý khách {_customerName}. Chúc quý khách có trận đấu hào hứng!";
                        break;
                    case 3:
                        txtContent.Text = $"SPORTFIELD xin thông báo lịch bảo trì kỹ thuật để nâng cấp chất lượng mặt sân. Mọi chi tiết xin liên hệ hotline hỗ trợ.";
                        break;
                }
            };
            p.Controls.Add(cboTemplate);

            Label l3 = new Label { Text = "Nội dung tin nhắn *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 146), AutoSize = true };
            p.Controls.Add(l3);

            txtContent = new TextBox
            {
                Location = new Point(0, 168),
                Width = 460,
                Height = 110,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = Typography.Body,
                BackColor = Colors.Surface,
                ForeColor = Colors.TextPrimary,
                Text = $"Kính gửi quý khách {_customerName}, lịch đặt sân thể thao của bạn tại SPORTFIELD sắp diễn ra. Vui lòng có mặt trước 10 phút để nhận sân thuận lợi."
            };
            p.Controls.Add(txtContent);

            card.Controls.Add(p);
            p.BringToFront();
            Body.Controls.Add(card);
        }

        protected override bool OnSave()
        {
            if (string.IsNullOrWhiteSpace(txtContent?.Text))
            {
                SFToast.Show(this, "Vui lòng nhập nội dung tin nhắn cần gửi.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            try
            {
                string kenh = "ZALO";
                if (cboChannel.SelectedIndex == 1) kenh = "SMS";
                else if (cboChannel.SelectedIndex == 2) kenh = "APP";
                else if (cboChannel.SelectedIndex == 3) kenh = "EMAIL";

                _messagingService.EnqueueMessage(
                    _khachHangId,
                    Security.SessionGuard.RequireAccountId(),
                    kenh,
                    _phone,
                    "Thông báo SPORTFIELD",
                    txtContent.Text.Trim(),
                    null
                );

                SFToast.Show(this, "Tin nhắn đã được lưu vào hàng đợi gửi (Trạng thái: CHO_GUI trong HangDoiTinNhan). Sẽ được tiến trình nền xử lý phát đi.", "HÀNG ĐỢI TIN NHẮN", SFIcon.Check, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "ENQUEUE_MESSAGE");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI GỬI TIN", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class ShiftScheduleForm : BaseDialog
    {
        private SFComboBox cboEmployee;
        private SFComboBox cboComplex;
        private DateTimePicker dtpWorkDate;
        private SFComboBox cboStartTime;
        private SFComboBox cboEndTime;
        private TextBox txtNote;
        private readonly Services.ShiftService _shiftService = new Services.ShiftService();
        private readonly Services.EmployeeService _employeeService = new Services.EmployeeService();

        public ShiftScheduleForm() : base("Phân Ca Trực Nhân Viên", new Size(720, 560))
        {
            Build();
            FooterButtons("Lưu Phân Ca");
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Label title = new Label { Text = "Thiết Lập Ca Làm Việc & Phân Công Nhân Sự", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(title);

            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            Label l1 = new Label { Text = "Nhân viên *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 10), AutoSize = true };
            p.Controls.Add(l1);

            cboEmployee = new SFComboBox { Location = new Point(0, 32), Width = 380, Height = 36 };
            try
            {
                DataTable dtNv = _employeeService.GetEmployees(null, true);
                if (dtNv != null)
                {
                    foreach (DataRow r in dtNv.Rows)
                    {
                        cboEmployee.Items.Add(new ComboItem
                        {
                            Text = $"{r["HoTen"]} ({r["MaNhanVien"]}) - {r["ChucVu"]}",
                            Value = Convert.ToInt32(r["NhanVienID"])
                        });
                    }
                    if (cboEmployee.Items.Count > 0) cboEmployee.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { Helpers.AppLogger.LogError(ex, "LOAD_EMPLOYEES_SHIFT"); }
            p.Controls.Add(cboEmployee);

            Label l2 = new Label { Text = "Cụm sân trực *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 78), AutoSize = true };
            p.Controls.Add(l2);

            cboComplex = new SFComboBox { Location = new Point(0, 100), Width = 380, Height = 36 };
            try
            {
                DataTable dtCs = DatabaseManager.ExecuteQuery("SELECT CumSanID, TenCumSan FROM dbo.CumSan WHERE DangHoatDong = 1");
                if (dtCs != null)
                {
                    foreach (DataRow r in dtCs.Rows)
                    {
                        cboComplex.Items.Add(new ComboItem
                        {
                            Text = r["TenCumSan"]?.ToString(),
                            Value = Convert.ToInt32(r["CumSanID"])
                        });
                    }
                    if (cboComplex.Items.Count > 0) cboComplex.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { Helpers.AppLogger.LogError(ex, "LOAD_COMPLEX_SHIFT"); }
            p.Controls.Add(cboComplex);

            Label l3 = new Label { Text = "Ngày làm việc *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 146), AutoSize = true };
            p.Controls.Add(l3);

            dtpWorkDate = new DateTimePicker
            {
                Location = new Point(0, 168),
                Width = 200,
                Font = Typography.Body,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            p.Controls.Add(dtpWorkDate);

            Label l4 = new Label { Text = "Giờ bắt đầu *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 210), AutoSize = true };
            p.Controls.Add(l4);

            cboStartTime = new SFComboBox { Location = new Point(0, 232), Width = 180, Height = 36 };
            cboStartTime.Items.AddRange(new object[] { "06:00", "07:00", "08:00", "13:00", "14:00", "15:00", "17:00", "18:00" });
            cboStartTime.SelectedIndex = 0;
            p.Controls.Add(cboStartTime);

            Label l5 = new Label { Text = "Giờ kết thúc *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(200, 210), AutoSize = true };
            p.Controls.Add(l5);

            cboEndTime = new SFComboBox { Location = new Point(200, 232), Width = 180, Height = 36 };
            cboEndTime.Items.AddRange(new object[] { "12:00", "14:00", "15:00", "18:00", "22:00", "23:00", "23:30" });
            cboEndTime.SelectedIndex = 1;
            p.Controls.Add(cboEndTime);

            txtNote = AddField(p, "Ghi chú phân ca", "Ca vận hành sáng / quản lý sân", 278, 380);

            card.Controls.Add(p);
            p.BringToFront();
            Body.Controls.Add(card);
        }

        protected override bool OnSave()
        {
            if (cboEmployee?.SelectedItem == null)
            {
                SFToast.Show(this, "Vui lòng chọn nhân viên.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            TimeSpan.TryParse(cboStartTime?.SelectedItem?.ToString() ?? "06:00", out TimeSpan tStart);
            TimeSpan.TryParse(cboEndTime?.SelectedItem?.ToString() ?? "14:00", out TimeSpan tEnd);

            if (tEnd <= tStart)
            {
                SFToast.Show(this, "Giờ kết thúc ca phải lớn hơn giờ bắt đầu.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            try
            {
                int nvId = ((ComboItem)cboEmployee.SelectedItem).Value;
                int? csId = cboComplex?.SelectedItem != null ? ((ComboItem)cboComplex.SelectedItem).Value : (int?)null;
                DateTime workDate = dtpWorkDate.Value.Date;
                string note = txtNote?.Text?.Trim();

                _shiftService.SaveShiftSchedule(null, nvId, csId, workDate, tStart, tEnd, "DA_XEP", note);
                SFToast.Show(this, "Đã lưu phân ca trực thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "SAVE_SHIFT");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI PHÂN CA", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class PermissionMatrixForm : BaseDialog
    {
        private SFComboBox cboRole;
        private SFDataGridView gridMatrix;
        private readonly Services.PermissionService _permissionService = new Services.PermissionService();
        private DataTable matrixData;

        public PermissionMatrixForm() : base("Ma Trận Phân Quyền Vai Trò Hệ Thống", new Size(880, 640))
        {
            Build();
            FooterButtons("Lưu Phân Quyền");
            LoadRolesAndMatrix();
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.Transparent };
            Label l = new Label { Text = "Chọn Vai Trò Hệ Thống:", Font = Typography.BodyBold, ForeColor = Colors.TextPrimary, Location = new Point(0, 14), AutoSize = true };
            topBar.Controls.Add(l);

            cboRole = new SFComboBox { Location = new Point(160, 8), Width = 280, Height = 36 };
            cboRole.SelectedIndexChanged += (s, e) => LoadMatrixForSelectedRole();
            topBar.Controls.Add(cboRole);
            card.Controls.Add(topBar);

            gridMatrix = new SFDataGridView { Dock = DockStyle.Fill };
            gridMatrix.Columns.Add(new DataGridViewCheckBoxColumn { Name = "DuocPhep", HeaderText = "Được phép", Width = 90 });
            gridMatrix.Columns.Add("MaQuyen", "Mã Quyền");
            gridMatrix.Columns.Add("TenQuyen", "Tên Quyền & Thao Tác");
            gridMatrix.Columns.Add("NhomQuyen", "Nhóm Chức Năng");

            gridMatrix.Columns["MaQuyen"].Width = 180;
            gridMatrix.Columns["TenQuyen"].Width = 280;
            gridMatrix.Columns["NhomQuyen"].Width = 160;

            card.Controls.Add(gridMatrix);
            gridMatrix.BringToFront();
            Body.Controls.Add(card);
        }

        private void LoadRolesAndMatrix()
        {
            try
            {
                DataTable dtRoles = _permissionService.GetRoles();
                cboRole.Items.Clear();
                if (dtRoles != null)
                {
                    foreach (DataRow r in dtRoles.Rows)
                    {
                        cboRole.Items.Add(new ComboItem
                        {
                            Text = $"{r["TenVaiTro"]} ({r["MaVaiTro"]})",
                            Value = Convert.ToInt32(r["VaiTroID"]),
                            Code = r["MaVaiTro"]?.ToString()
                        });
                    }
                    if (cboRole.Items.Count > 0) cboRole.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_ROLES_MATRIX");
            }
        }

        private void LoadMatrixForSelectedRole()
        {
            if (cboRole.SelectedItem == null) return;
            int roleId = ((ComboItem)cboRole.SelectedItem).Value;

            try
            {
                matrixData = _permissionService.GetPermissionMatrix(roleId);
                gridMatrix.Rows.Clear();
                if (matrixData != null)
                {
                    foreach (DataRow r in matrixData.Rows)
                    {
                        bool duocPhep = r["DuocPhep"] != DBNull.Value && Convert.ToBoolean(r["DuocPhep"]);
                        int rowIndex = gridMatrix.Rows.Add(
                            duocPhep,
                            r["MaQuyen"]?.ToString(),
                            r["TenQuyen"]?.ToString(),
                            r["NhomQuyen"]?.ToString()
                        );
                        gridMatrix.Rows[rowIndex].Tag = r;
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_PERMISSION_MATRIX");
            }
        }

        protected override bool OnSave()
        {
            if (cboRole?.SelectedItem == null) return false;
            int roleId = ((ComboItem)cboRole.SelectedItem).Value;

            try
            {
                List<int> grantedQuyenIds = new List<int>();
                foreach (DataGridViewRow row in gridMatrix.Rows)
                {
                    if (row.IsNewRow) continue;
                    bool isChecked = row.Cells["DuocPhep"].Value != null && Convert.ToBoolean(row.Cells["DuocPhep"].Value);
                    if (isChecked && row.Tag is DataRow dr)
                    {
                        grantedQuyenIds.Add(Convert.ToInt32(dr["QuyenID"]));
                    }
                }

                _permissionService.SaveRolePermissions(roleId, grantedQuyenIds);
                SFToast.Show(this, "Đã cập nhật phân quyền vai trò vào CSDL SQL Server thành công!", "PHÂN QUYỀN", SFIcon.Check, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "SAVE_PERMISSIONS");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI LƯU QUYỀN", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class AddFieldTypeForm : BaseDialog
    {
        private TextBox txtMa;
        private TextBox txtTen;
        private TextBox txtSoNguoi;
        private TextBox txtKichThuoc;
        private TextBox txtMatSan;
        private TextBox txtMoTa;
        private CheckBox chkActive;
        private readonly Services.FieldService _fieldService = new Services.FieldService();

        public AddFieldTypeForm() : base("Thêm Loại Sân & Môn Thể Thao Mới", new Size(720, 580))
        {
            Build();
            FooterButtons("Lưu Loại Sân");
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Label title = new Label { Text = "Thông Số Loại Sân Mới", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(title);

            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            txtMa = AddField(p, "Mã Loại Sân (VD: BD5, BD7, CL, BR, PK) *", "BD7", 10, 360);
            txtTen = AddField(p, "Tên Loại Sân / Bộ Môn *", "Sân Bóng Đá 7 Người", 74, 360);
            txtSoNguoi = AddField(p, "Số người thi đấu chuẩn", "14", 138, 180);
            txtKichThuoc = AddField(p, "Kích thước sân (VD: 30m x 50m)", "30m x 50m", 202, 360);
            txtMatSan = AddField(p, "Chất liệu mặt sân (VD: Cỏ nhân tạo FIFA)", "Cỏ nhân tạo chuyên nghiệp", 266, 360);
            txtMoTa = AddField(p, "Mô tả & Tiện ích kèm theo", "Đèn chiếu sáng LED, lưới chắn bóng, khán đài mini", 330, 480);

            chkActive = new CheckBox
            {
                Text = "Kích hoạt đưa vào vận hành ngay",
                Location = new Point(0, 400),
                AutoSize = true,
                Checked = true,
                Font = Typography.Body
            };
            p.Controls.Add(chkActive);

            card.Controls.Add(p);
            p.BringToFront();
            Body.Controls.Add(card);
        }

        protected override bool OnSave()
        {
            string ma = txtMa?.Text?.Trim() ?? "";
            string ten = txtTen?.Text?.Trim() ?? "";
            int.TryParse(txtSoNguoi?.Text?.Trim() ?? "0", out int soNguoi);

            if (string.IsNullOrWhiteSpace(ma))
            {
                SFToast.Show(this, "Vui lòng nhập mã loại sân.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
            if (string.IsNullOrWhiteSpace(ten))
            {
                SFToast.Show(this, "Vui lòng nhập tên loại sân.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }

            try
            {
                _fieldService.SaveFieldType(
                    null,
                    ma,
                    ten,
                    txtMoTa?.Text?.Trim(),
                    txtKichThuoc?.Text?.Trim(),
                    txtMatSan?.Text?.Trim(),
                    soNguoi > 0 ? (int?)soNguoi : null,
                    "Field",
                    chkActive.Checked
                );

                SFToast.Show(this, "Đã lưu loại sân mới vào CSDL SQL Server thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "SAVE_FIELD_TYPE");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI LƯU LOẠI SÂN", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class PricingConfigForm : BaseDialog
    {
        private SFComboBox cboFieldType;
        private TextBox txtPriceName;
        private SFComboBox cboDayType;
        private SFComboBox cboStartTime;
        private SFComboBox cboEndTime;
        private TextBox txtPrice;
        private TextBox txtPriority;
        private readonly Services.FieldService _fieldService = new Services.FieldService();

        public PricingConfigForm() : base("Thiết Lập Khung Giờ & Chính Sách Giá Sân", new Size(740, 580))
        {
            Build();
            FooterButtons("Lưu Bảng Giá");
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Label title = new Label { Text = "Cấu Hình Bảng Giá Giờ Vàng & Giờ Thường", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(title);

            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            Label l1 = new Label { Text = "Loại sân áp dụng *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 10), AutoSize = true };
            p.Controls.Add(l1);

            cboFieldType = new SFComboBox { Location = new Point(0, 32), Width = 380, Height = 36 };
            try
            {
                DataTable dtFt = _fieldService.GetFieldTypes(null, true);
                if (dtFt != null)
                {
                    foreach (DataRow r in dtFt.Rows)
                    {
                        cboFieldType.Items.Add(new ComboItem
                        {
                            Text = $"{r["TenLoaiSan"]} ({r["MaLoaiSan"]})",
                            Value = Convert.ToInt32(r["LoaiSanID"])
                        });
                    }
                    if (cboFieldType.Items.Count > 0) cboFieldType.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { Helpers.AppLogger.LogError(ex, "LOAD_FIELD_TYPES_PRICING"); }
            p.Controls.Add(cboFieldType);

            txtPriceName = AddField(p, "Tên quy tắc bảng giá *", "Giờ Vàng Buổi Tối (17h - 22h)", 74, 380);

            Label l2 = new Label { Text = "Loại ngày áp dụng *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 138), AutoSize = true };
            p.Controls.Add(l2);

            cboDayType = new SFComboBox { Location = new Point(0, 160), Width = 380, Height = 36 };
            cboDayType.Items.AddRange(new object[] { "TAT_CA - Tất cả các ngày trong tuần", "NGAY_THUONG - Thứ 2 đến Thứ 6", "CUOI_TUAN - Thứ 7 và Chủ Nhật" });
            cboDayType.SelectedIndex = 0;
            p.Controls.Add(cboDayType);

            Label l3 = new Label { Text = "Giờ bắt đầu *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 206), AutoSize = true };
            p.Controls.Add(l3);

            cboStartTime = new SFComboBox { Location = new Point(0, 228), Width = 180, Height = 36 };
            cboStartTime.Items.AddRange(new object[] { "06:00", "08:00", "14:00", "16:30", "17:00", "18:00" });
            cboStartTime.SelectedIndex = 3;
            p.Controls.Add(cboStartTime);

            Label l4 = new Label { Text = "Giờ kết thúc *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(200, 206), AutoSize = true };
            p.Controls.Add(l4);

            cboEndTime = new SFComboBox { Location = new Point(200, 228), Width = 180, Height = 36 };
            cboEndTime.Items.AddRange(new object[] { "12:00", "17:00", "21:00", "22:00", "22:30", "23:00" });
            cboEndTime.SelectedIndex = 4;
            p.Controls.Add(cboEndTime);

            txtPrice = AddField(p, "Đơn giá thuê sân (VNĐ/giờ) *", "350000", 274, 180);
            txtPriority = AddField(p, "Thứ tự ưu tiên (Cao hơn sẽ ghi đè)", "100", 338, 180);

            card.Controls.Add(p);
            p.BringToFront();
            Body.Controls.Add(card);
        }

        protected override bool OnSave()
        {
            if (cboFieldType?.SelectedItem == null)
            {
                SFToast.Show(this, "Vui lòng chọn loại sân.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            string name = txtPriceName?.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                SFToast.Show(this, "Vui lòng nhập tên bảng giá.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            TimeSpan.TryParse(cboStartTime?.SelectedItem?.ToString() ?? "06:00", out TimeSpan tStart);
            TimeSpan.TryParse(cboEndTime?.SelectedItem?.ToString() ?? "22:00", out TimeSpan tEnd);

            if (tEnd <= tStart)
            {
                SFToast.Show(this, "Giờ kết thúc phải lớn hơn giờ bắt đầu.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            decimal.TryParse(txtPrice?.Text?.Trim() ?? "0", out decimal price);
            if (price < 0)
            {
                SFToast.Show(this, "Đơn giá không được âm.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            int.TryParse(txtPriority?.Text?.Trim() ?? "100", out int priority);

            try
            {
                int loaiSanId = ((ComboItem)cboFieldType.SelectedItem).Value;
                string dayType = "TAT_CA";
                if (cboDayType.SelectedIndex == 1) dayType = "NGAY_THUONG";
                else if (cboDayType.SelectedIndex == 2) dayType = "CUOI_TUAN";

                _fieldService.SavePricing(
                    null,
                    null,
                    loaiSanId,
                    name,
                    dayType,
                    null,
                    tStart,
                    tEnd,
                    price,
                    DateTime.Today,
                    null,
                    priority,
                    true
                );

                SFToast.Show(this, "Đã lưu quy tắc bảng giá & khung giờ vào CSDL thành công!", "THÀNH CÔNG", SFIcon.Check, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "SAVE_PRICING");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI LƯU BẢNG GIÁ", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class BatchVoucherIssueForm : BaseDialog
    {
        private SFComboBox cboVouchers;
        private SFComboBox cboTarget;
        private Label lblPreview;
        private TextBox txtQuantity;
        private readonly Services.VoucherService _voucherService = new Services.VoucherService();
        private readonly Services.CustomerService _customerService = new Services.CustomerService();

        public BatchVoucherIssueForm() : base("Phát Hành Voucher Hàng Loạt", new Size(720, 520))
        {
            Build();
            FooterButtons("Xác Nhận Phát Hành");
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(24) };
            Label title = new Label { Text = "Tặng Mã Ưu Đãi Tới Nhóm Khách Hàng", Font = Typography.SectionTitle, ForeColor = Colors.TextPrimary, Dock = DockStyle.Top, Height = 32 };
            card.Controls.Add(title);

            Panel p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            Label l1 = new Label { Text = "Chọn Voucher đang kích hoạt *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 10), AutoSize = true };
            p.Controls.Add(l1);

            cboVouchers = new SFComboBox { Location = new Point(0, 32), Width = 420, Height = 36 };
            try
            {
                DataTable dtV = _voucherService.GetVouchers(null, true);
                if (dtV != null)
                {
                    foreach (DataRow r in dtV.Rows)
                    {
                        cboVouchers.Items.Add(new ComboItem
                        {
                            Text = $"{r["TenVoucher"]} ({r["MaVoucher"]})",
                            Value = Convert.ToInt32(r["VoucherID"]),
                            Code = r["MaVoucher"]?.ToString()
                        });
                    }
                    if (cboVouchers.Items.Count > 0) cboVouchers.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { Helpers.AppLogger.LogError(ex, "LOAD_VOUCHERS_BATCH"); }
            p.Controls.Add(cboVouchers);

            Label l2 = new Label { Text = "Nhóm khách hàng mục tiêu *", Font = Typography.CaptionBold, ForeColor = Colors.TextSecondary, Location = new Point(0, 78), AutoSize = true };
            p.Controls.Add(l2);

            cboTarget = new SFComboBox { Location = new Point(0, 100), Width = 420, Height = 36 };
            cboTarget.Items.AddRange(new object[] {
                "Tất cả khách hàng (Toàn bộ hội viên)",
                "Hội viên Hạng Bạc (SILVER)",
                "Hội viên Hạng Vàng (GOLD)",
                "Hội viên Hạng Kim Cương / VIP"
            });
            cboTarget.SelectedIndex = 0;
            cboTarget.SelectedIndexChanged += (s, e) => UpdatePreviewCount();
            p.Controls.Add(cboTarget);

            lblPreview = new Label
            {
                Text = "Số khách dự kiến nhận: Đang tính...",
                Font = Typography.BodyBold,
                ForeColor = Colors.ElectricBlue,
                Location = new Point(0, 146),
                AutoSize = true
            };
            p.Controls.Add(lblPreview);

            txtQuantity = AddField(p, "Số lượng voucher phát cho mỗi khách *", "1", 175, 120);

            card.Controls.Add(p);
            p.BringToFront();
            Body.Controls.Add(card);

            UpdatePreviewCount();
        }

        private void UpdatePreviewCount()
        {
            try
            {
                string maHang = null;
                if (cboTarget.SelectedIndex == 1) maHang = "SILVER";
                else if (cboTarget.SelectedIndex == 2) maHang = "GOLD";
                else if (cboTarget.SelectedIndex == 3) maHang = "DIAMOND";

                DataTable dtKh = _customerService.GetCustomers(null, maHang, true);
                int count = dtKh != null ? dtKh.Rows.Count : 0;
                lblPreview.Text = $"Số khách dự kiến nhận: {count} khách hàng phù hợp";
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "BATCH_VOUCHER_PREVIEW");
                lblPreview.Text = "Số khách dự kiến nhận: 0 khách hàng";
            }
        }

        protected override bool OnSave()
        {
            if (cboVouchers?.SelectedItem == null)
            {
                SFToast.Show(this, "Vui lòng chọn voucher cần phát hành.", "CẢNH BÁO", SFIcon.AlertCircle, Colors.Warning);
                return false;
            }

            int.TryParse(txtQuantity?.Text?.Trim() ?? "1", out int qty);
            if (qty <= 0) qty = 1;

            try
            {
                int voucherId = ((ComboItem)cboVouchers.SelectedItem).Value;
                string maHang = null;
                if (cboTarget.SelectedIndex == 1) maHang = "SILVER";
                else if (cboTarget.SelectedIndex == 2) maHang = "GOLD";
                else if (cboTarget.SelectedIndex == 3) maHang = "DIAMOND";

                DataTable dtKh = _customerService.GetCustomers(null, maHang, true);
                List<int> customerIds = new List<int>();
                if (dtKh != null)
                {
                    foreach (DataRow r in dtKh.Rows)
                    {
                        customerIds.Add(Convert.ToInt32(r["KhachHangID"]));
                    }
                }

                if (customerIds.Count == 0)
                {
                    SFToast.Show(this, "Không có khách hàng nào thuộc nhóm được chọn.", "THÔNG BÁO", SFIcon.AlertCircle, Colors.Warning);
                    return false;
                }

                var res = _voucherService.BatchIssueVouchers(voucherId, customerIds, qty);
                SFToast.Show(this, $"Đã phát hành {res.SuccessCount} voucher. {res.SkippedCount} khách được bỏ qua vì đã đạt giới hạn.", "PHÁT HÀNH VOUCHER", SFIcon.Ticket, Colors.Emerald);
                return true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "BATCH_VOUCHER");
                SFToast.Show(this, Helpers.AppLogger.GetUserFriendlySqlMessage(ex), "LỖI PHÁT HÀNH", SFIcon.AlertCircle, Colors.Danger);
                return false;
            }
        }
    }

    public class VoucherReportForm : BaseDialog
    {
        private DateTimePicker dtpTuNgay;
        private DateTimePicker dtpDenNgay;
        private SFDataGridView gridReport;
        private readonly Services.VoucherService _voucherService = new Services.VoucherService();

        public VoucherReportForm() : base("Báo Cáo Hiệu Quả Chiến Dịch Voucher", new Size(960, 640))
        {
            Build();
            FooterButtons("Đóng Báo Cáo");
            LoadReportData();
        }

        private void Build()
        {
            SFCard card = new SFCard { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.Transparent };
            Label l1 = new Label { Text = "Từ ngày:", Font = Typography.BodyBold, ForeColor = Colors.TextPrimary, Location = new Point(0, 10), AutoSize = true };
            topBar.Controls.Add(l1);

            dtpTuNgay = new DateTimePicker { Location = new Point(70, 7), Width = 130, Font = Typography.Body, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-3) };
            topBar.Controls.Add(dtpTuNgay);

            Label l2 = new Label { Text = "Đến ngày:", Font = Typography.BodyBold, ForeColor = Colors.TextPrimary, Location = new Point(220, 10), AutoSize = true };
            topBar.Controls.Add(l2);

            dtpDenNgay = new DateTimePicker { Location = new Point(290, 7), Width = 130, Font = Typography.Body, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            topBar.Controls.Add(dtpDenNgay);

            SFButton btnFilter = new SFButton { Text = "Lọc Dữ Liệu", Variant = SFButtonVariant.Secondary, IconType = SFIcon.Filter, Width = 110, Height = 32, Location = new Point(440, 6) };
            btnFilter.Click += (s, e) => LoadReportData();
            topBar.Controls.Add(btnFilter);

            SFButton btnExcel = new SFButton { Text = "Xuất Excel", Variant = SFButtonVariant.Primary, IconType = SFIcon.Printer, Width = 120, Height = 32, Location = new Point(560, 6) };
            btnExcel.Click += (s, e) => Helpers.ExportHelper.ExportToExcel(gridReport, "BaoCaoHieuQuaVoucher.csv", this);
            topBar.Controls.Add(btnExcel);

            card.Controls.Add(topBar);

            gridReport = new SFDataGridView { Dock = DockStyle.Fill };
            gridReport.Columns.Add("MaVoucher", "Mã Voucher");
            gridReport.Columns.Add("TenVoucher", "Tên Chương Trình");
            gridReport.Columns.Add("SoLuotCap", "Lượt Cấp");
            gridReport.Columns.Add("SoLuotSuDung", "Đã Dùng");
            gridReport.Columns.Add("TyLe", "Tỷ Lệ Dùng");
            gridReport.Columns.Add("TongTienGiam", "Tổng Tiền Giảm");
            gridReport.Columns.Add("DoanhThu", "Doanh Thu Kèm");
            gridReport.Columns.Add("ROI", "Hiệu Quả / ROI");

            gridReport.Columns["MaVoucher"].Width = 110;
            gridReport.Columns["TenVoucher"].Width = 180;
            gridReport.Columns["SoLuotCap"].Width = 90;
            gridReport.Columns["SoLuotSuDung"].Width = 90;
            gridReport.Columns["TyLe"].Width = 95;
            gridReport.Columns["TongTienGiam"].Width = 130;
            gridReport.Columns["DoanhThu"].Width = 130;
            gridReport.Columns["ROI"].Width = 110;

            card.Controls.Add(gridReport);
            gridReport.BringToFront();
            Body.Controls.Add(card);
        }

        private void LoadReportData()
        {
            try
            {
                DataTable dt = _voucherService.GetVoucherEffectivenessReport(dtpTuNgay.Value.Date, dtpDenNgay.Value.Date);
                gridReport.Rows.Clear();
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int cap = r["SoLuotCap"] != DBNull.Value ? Convert.ToInt32(r["SoLuotCap"]) : 0;
                        int dung = r["SoLuotSuDung"] != DBNull.Value ? Convert.ToInt32(r["SoLuotSuDung"]) : 0;
                        decimal giam = r["TongTienGiam"] != DBNull.Value ? Convert.ToDecimal(r["TongTienGiam"]) : 0;
                        decimal dtKem = r["DoanhThuKemVoucher"] != DBNull.Value ? Convert.ToDecimal(r["DoanhThuKemVoucher"]) : 0;

                        double tyLe = cap > 0 ? ((double)dung / cap) * 100 : 0;
                        string roiText = "—";
                        if (giam > 0 && dtKem > 0)
                        {
                            decimal roi = (dtKem - giam) / giam;
                            roiText = $"x{roi:0.#}";
                        }

                        gridReport.Rows.Add(
                            r["MaVoucher"]?.ToString(),
                            r["TenVoucher"]?.ToString(),
                            cap.ToString("N0"),
                            dung.ToString("N0"),
                            $"{tyLe:0.#}%",
                            $"{giam:N0} đ",
                            $"{dtKem:N0} đ",
                            roiText
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_VOUCHER_REPORT");
            }
        }

        protected override bool OnSave()
        {
            return true;
        }
    }
}
