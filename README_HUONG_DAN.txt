SPORTFIELD - WINDOWS FORMS MERGED FINAL
========================================

Mục tiêu
- Hợp nhất 3 nguồn: WinForms nền + Stitch Nhân viên + Stitch Khách hàng.
- Giữ giao diện native Windows Forms, không nhúng HTML/WebView.
- Target: .NET Framework 4.8, Visual Studio, SQL Server / ADO.NET.
- Main workspace: 1440 x 900, Minimum 1280 x 720.
- Sidebar: 224 px; Topbar: 64 px; nội dung dài cuộn bên trong control, không kéo form như website.

Mở project
1. Giải nén toàn bộ ZIP.
2. Mở SportZone.WinForms.sln bằng Visual Studio 2022.
3. Đảm bảo máy có .NET Framework 4.8 Developer Pack.
4. Build > Rebuild Solution.
5. F5 để chạy.

Tài khoản demo giao diện
- Admin:       admin / 123456
- Nhân viên:   nhanvien / 123456
- Khách hàng:  khachhang / 123456
Chọn đúng vai trò trong ComboBox trên màn hình đăng nhập.

PHÂN QUYỀN ADMIN
1. Tổng quan
2. Loại sân
3. Quản lý sân
4. Khách hàng
5. Đặt sân
6. Lịch đặt sân
7. Hóa đơn
8. Voucher
9. Khuyến mãi
10. Tài khoản
11. Nhân viên
12. Thống kê
13. Cấu hình

PHÂN QUYỀN NHÂN VIÊN
1. Trang chủ
2. Đặt sân tại quầy
3. Lịch đặt sân
4. Hóa đơn & Thu tiền
5. Khách hàng
6. Thông tin sân - CHỈ XEM
7. Loại sân - CHỈ XEM
8. Thống kê ca trực
9. Tài khoản cá nhân

PHÂN QUYỀN KHÁCH HÀNG
1. Trang chủ
2. Đặt sân
3. Lịch sử đặt sân
4. Hóa đơn của tôi
5. Voucher của tôi
6. Thông tin cá nhân
Ngoài ra có Đăng ký khách hàng, Đổi mật khẩu, VietQR và Xác nhận hủy sân.

Kết nối SQL Server
- App.config đã có connection string mẫu:
  Data Source=.\SQLEXPRESS;Initial Catalog=QuanLySanTheThaoDB;Integrated Security=True;TrustServerCertificate=True
- Bản hợp nhất này tập trung vào UI, phân quyền và luồng thao tác giao diện.
- Dữ liệu trong DataGridView hiện là dữ liệu demo để thể hiện đúng design Stitch.
- Khi nối database thật, thay dữ liệu demo bằng Repository/ADO.NET parameterized queries.

Thư mục ReferenceDesigns
- Admin: ảnh/reference từ bộ WinForms/Stitch nền.
- Staff: ảnh Stitch Nhân viên gốc.
- Customer: ảnh Stitch Khách hàng gốc.
Dùng để so sánh khi tiếp tục pixel-tuning trong Visual Studio.

Lưu ý UI
- Không tăng chiều cao Main Form để chứa dữ liệu.
- DataGridView / FlowLayoutPanel dùng scroll nội bộ.
- Màu chung: Sidebar #0F172A, Primary #16A34A, Canvas #F8FAFC.
- Font runtime: Segoe UI để bảo đảm tương thích Windows Forms và tiếng Việt.
