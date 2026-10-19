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
    public class EmployeeManagementView : UserControl
    {
        private SFSearchBox searchBox;
        private SFDataGridView grid;
        private TableLayoutPanel kpiPanel;
        private DataTable employeeTable;
        private string searchKeyword = "";

        public EmployeeManagementView()
        {
            Dock = DockStyle.Fill;
            BackColor = Colors.Canvas;
            Font = Typography.Body;
            BuildUI();
            LoadEmployeesFromDatabase();
        }

        private void BuildUI()
        {
            // 1. Header
            Controls.Add(Ui.Header("Quản Lý Nhân Viên & Phân Quyền Vận Hành", "Danh mục nhân sự, ca trực, ma trận phân quyền quầy POS và trạng thái hoạt động."));

            // 2. Toolbar
            FlowLayoutPanel tb = Ui.Toolbar();

            SFButton btnAdd = new SFButton
            {
                Text = "Thêm Nhân Viên (F2)",
                Variant = SFButtonVariant.Primary,
                IconType = SFIcon.Plus,
                Width = 190,
                Height = 38
            };
            btnAdd.Click += (s, e) => OpenAddEmployeeDialog();
            tb.Controls.Add(btnAdd);

            SFButton btnShift = new SFButton
            {
                Text = "Phân Ca Trực (F3)",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Clock,
                Width = 160,
                Height = 38
            };
            btnShift.Click += (s, e) => OpenShiftScheduleDialog();
            tb.Controls.Add(btnShift);

            SFButton btnPerm = new SFButton
            {
                Text = "Ma Trận Quyền (F4)",
                Variant = SFButtonVariant.Secondary,
                IconType = SFIcon.Settings,
                Width = 170,
                Height = 38
            };
            btnPerm.Click += (s, e) => OpenPermissionMatrixDialog();
            tb.Controls.Add(btnPerm);

            searchBox = new SFSearchBox
            {
                PlaceholderText = "Tên / mã NV / chức vụ / email...",
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
            kpiPanel.Controls.Add(new SFMetricCard("TỔNG NHÂN SỰ", "4", "Hội đồng & vận hành", SFIcon.User, Colors.ElectricBlue), 0, 0);
            kpiPanel.Controls.Add(new SFMetricCard("ĐANG TRONG CA", "03", "Ca hôm nay", SFIcon.Clock, Colors.Emerald, "100% đúng giờ"), 1, 0);
            kpiPanel.Controls.Add(new SFMetricCard("LƯỢT POS HÔM NAY", "148", "Giao dịch xử lý", SFIcon.Chart, Colors.Purple, "+12% tốc độ"), 2, 0);
            kpiPanel.Controls.Add(new SFMetricCard("CẢNH BÁO BẢO MẬT", "00", "Không có truy cập lạ", SFIcon.Check, Colors.Emerald, "An toàn tuyệt đối"), 3, 0);

            Controls.Add(kpiPanel);
            kpiPanel.BringToFront();

            // 4. Main Grid
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
                Text = "Danh Sách Nhân Viên & Vai Trò Hệ Thống (Nhấp đúp dòng để sửa/phân quyền)",
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
            grid.Columns.Add("Code", "Mã NV");
            grid.Columns.Add("Name", "Họ và tên");
            grid.Columns.Add("Role", "Chức vụ");
            grid.Columns.Add("Shift", "Ca làm việc");
            grid.Columns.Add("Perms", "Nhóm quyền");
            grid.Columns.Add("Phone", "Số điện thoại");
            grid.Columns.Add("LastLogin", "Đăng nhập cuối");
            grid.Columns.Add("Status", "Trạng thái");

            grid.Columns["Code"].Width = 85;
            grid.Columns["Name"].Width = 180;
            grid.Columns["Role"].Width = 130;
            grid.Columns["Shift"].Width = 150;
            grid.Columns["Perms"].Width = 160;
            grid.Columns["Phone"].Width = 120;
            grid.Columns["LastLogin"].Width = 140;
            grid.Columns["Status"].Width = 110;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Chỉnh sửa thông tin & Phân quyền", null, (s, e) => {
                if (grid.CurrentRow != null && grid.CurrentRow.Tag != null)
                {
                    int id = Convert.ToInt32(grid.CurrentRow.Tag);
                    string name = grid.CurrentRow.Cells[1].Value?.ToString() ?? "";
                    string phone = grid.CurrentRow.Cells[5].Value?.ToString() ?? "";
                    string maNv = grid.CurrentRow.Cells[0].Value?.ToString() ?? "";
                    if (new EmployeeEditForm(id, name, phone, "", maNv).ShowDialog(this) == DialogResult.OK)
                        LoadEmployeesFromDatabase();
                }
            });
            menu.Items.Add("Thêm nhân viên mới", null, (s, e) => {
                if (new EmployeeEditForm().ShowDialog(this) == DialogResult.OK)
                    LoadEmployeesFromDatabase();
            });
            menu.Items.Add("-");
            menu.Items.Add("Xuất danh sách nhân viên ra Excel (CSV)", null, (s, e) => {
                Helpers.ExportHelper.ExportToExcel(grid, "DanhSachNhanVien.csv", this);
            });
            grid.ContextMenuStrip = menu;

            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && grid.Rows[e.RowIndex].Tag != null)
                {
                    int id = Convert.ToInt32(grid.Rows[e.RowIndex].Tag);
                    string name = grid.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "";
                    string phone = grid.Rows[e.RowIndex].Cells[5].Value?.ToString() ?? "";
                    string maNv = grid.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? "";
                    if (new EmployeeEditForm(id, name, phone, "", maNv).ShowDialog(this) == DialogResult.OK)
                    {
                        LoadEmployeesFromDatabase();
                    }
                }
            };

            card.Controls.Add(grid);
            grid.BringToFront();

            Controls.Add(card);
            card.BringToFront();
        }

        public void LoadEmployeesFromDatabase()
        {
            try
            {
                employeeTable = DatabaseManager.ExecuteQuery("EXEC dbo.sp_UI_DanhSachNhanVien");
                if (employeeTable != null && employeeTable.Rows.Count > 0)
                {
                    int total = employeeTable.Rows.Count;
                    int active = 0;
                    foreach (DataRow r in employeeTable.Rows)
                    {
                        if (r["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(r["DangHoatDong"])) active++;
                    }

                    int posCount = 0;
                    try
                    {
                        var objPos = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM dbo.GiaoDichThanhToan WHERE CAST(ThoiGianTao AS DATE) = CAST(GETDATE() AS DATE)");
                        if (objPos != null && objPos != DBNull.Value) posCount = Convert.ToInt32(objPos);
                    }
                    catch { }

                    int alertCount = 0;
                    try
                    {
                        var objAlert = DatabaseManager.ExecuteScalar("SELECT COUNT(*) FROM dbo.NhatKyHeThong WHERE CAST(ThoiGian AS DATE) = CAST(GETDATE() AS DATE) AND CapDo IN ('CANH_BAO', 'NGHIEM_TRONG')");
                        if (objAlert != null && objAlert != DBNull.Value) alertCount = Convert.ToInt32(objAlert);
                    }
                    catch { }

                    kpiPanel.Controls.Clear();
                    kpiPanel.Controls.Add(new SFMetricCard("TỔNG NHÂN SỰ", total.ToString(), "Nhân sự hệ thống", SFIcon.User, Colors.ElectricBlue, "CSDL SQL Live"), 0, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("ĐANG HOẠT ĐỘNG", active.ToString(), "Tài khoản active", SFIcon.Clock, Colors.Emerald, "100% sẵn sàng"), 1, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("LƯỢT POS HÔM NAY", posCount.ToString(), "Giao dịch xử lý", SFIcon.Chart, Colors.Purple, "Giao dịch quầy"), 2, 0);
                    kpiPanel.Controls.Add(new SFMetricCard("CẢNH BÁO BẢO MẬT", alertCount.ToString("D2"), "Cảnh báo hệ thống", SFIcon.Check, Colors.Emerald, "Nhật ký bảo mật"), 3, 0);

                    ApplyFilter();
                    return;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogError(ex, "LOAD_EMPLOYEES");
            }

            grid.Rows.Clear();
        }

        private void ApplyFilter()
        {
            grid.Rows.Clear();

            if (employeeTable == null || employeeTable.Rows.Count == 0)
            {
                return;
            }

            Dictionary<int, string> shiftMap = new Dictionary<int, string>();
            try
            {
                DataTable dtShifts = DatabaseManager.ExecuteQuery(
                    "SELECT NhanVienID, GioBatDau, GioKetThuc FROM dbo.LichPhanCa WHERE CAST(NgayLam AS DATE) = CAST(GETDATE() AS DATE)");
                if (dtShifts != null)
                {
                    foreach (DataRow sr in dtShifts.Rows)
                    {
                        int nid = Convert.ToInt32(sr["NhanVienID"]);
                        if (sr["GioBatDau"] != DBNull.Value && sr["GioKetThuc"] != DBNull.Value)
                        {
                            TimeSpan bd = (TimeSpan)sr["GioBatDau"];
                            TimeSpan kt = (TimeSpan)sr["GioKetThuc"];
                            shiftMap[nid] = string.Format("{0:hh\\:mm} - {1:hh\\:mm}", bd, kt);
                        }
                    }
                }
            }
            catch { }

            foreach (DataRow r in employeeTable.Rows)
            {
                int empId = r.Table.Columns.Contains("NhanVienID") && r["NhanVienID"] != DBNull.Value ? Convert.ToInt32(r["NhanVienID"]) : 0;
                string code = r["MaNhanVien"]?.ToString() ?? "";
                string name = r["HoTen"]?.ToString() ?? "";
                string chucVu = r["ChucVu"]?.ToString() ?? "Nhân viên vận hành";
                string role = r["TenVaiTro"]?.ToString() ?? "Nhân viên";
                string phone = r["SoDienThoai"]?.ToString() ?? "";
                string lastLogin = r["LanDangNhapCuoi"] != DBNull.Value ? Convert.ToDateTime(r["LanDangNhapCuoi"]).ToString("dd/MM HH:mm") : "Chưa đăng nhập";
                bool active = r["DangHoatDong"] != DBNull.Value && Convert.ToBoolean(r["DangHoatDong"]);
                string email = r.Table.Columns.Contains("Email") ? (r["Email"]?.ToString() ?? "") : "";

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    bool match = code.ToLowerInvariant().Contains(searchKeyword) ||
                                 name.ToLowerInvariant().Contains(searchKeyword) ||
                                 chucVu.ToLowerInvariant().Contains(searchKeyword) ||
                                 phone.ToLowerInvariant().Contains(searchKeyword) ||
                                 email.ToLowerInvariant().Contains(searchKeyword);
                    if (!match) continue;
                }

                string shiftText = shiftMap.ContainsKey(empId) ? shiftMap[empId] : "— (Chưa xếp ca)";

                int rIdx = grid.Rows.Add(
                    code,
                    name,
                    chucVu,
                    shiftText,
                    role + " • POS",
                    phone,
                    lastLogin,
                    active ? "Đang làm" : "Nghỉ việc"
                );
                grid.Rows[rIdx].Tag = empId;
            }
        }

        private void OpenAddEmployeeDialog()
        {
            if (new EmployeeEditForm().ShowDialog(this) == DialogResult.OK)
            {
                LoadEmployeesFromDatabase();
            }
        }

        private void OpenShiftScheduleDialog()
        {
            new ShiftScheduleForm().ShowDialog(this);
        }

        private void OpenPermissionMatrixDialog()
        {
            new PermissionMatrixForm().ShowDialog(this);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F2)
            {
                OpenAddEmployeeDialog();
                return true;
            }
            if (keyData == Keys.F3)
            {
                OpenShiftScheduleDialog();
                return true;
            }
            if (keyData == Keys.F4)
            {
                OpenPermissionMatrixDialog();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
