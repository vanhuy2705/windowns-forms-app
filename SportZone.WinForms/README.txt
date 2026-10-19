SPORTFIELD WINFORMS MERGED UI
============================
Target: .NET Framework 4.8 / C# 7.3 / Windows Forms.
Main viewport reference: 1440x900, minimum 1280x720.

Đã hợp nhất giao diện theo 3 phân quyền:
- Admin: 13 module.
- Nhân viên: 9 module, Thông tin sân và Loại sân ở chế độ read-only.
- Khách hàng: 6 module + đăng ký, đổi mật khẩu, VietQR, hủy sân.

App.config:
Data Source=.\SQLEXPRESS;Initial Catalog=QuanLySanTheThaoDB;Integrated Security=True;TrustServerCertificate=True

Lưu ý: dữ liệu trên grid hiện là dữ liệu demo dùng để trình diễn giao diện và luồng điều hướng. Khi nối CSDL thật, đặt ADO.NET Repository/Service phía dưới các View này.
