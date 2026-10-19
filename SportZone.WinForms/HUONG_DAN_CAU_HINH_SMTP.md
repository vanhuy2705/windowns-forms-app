# Hướng dẫn cấu hình Gmail SMTP (SPORTFIELD)

`App.config` **không còn chứa mật khẩu thật**. Khóa `SmtpPassword` chỉ giữ placeholder:

```xml
<add key="SmtpPassword" value="YOUR_NEW_GMAIL_APP_PASSWORD" />
```

`EmailService.GetSmtpPassword()` nạp mật khẩu theo thứ tự ưu tiên sau.

## 1. Biến môi trường (khuyến nghị cho máy build/CI)

Tên biến: `SPORTFIELD_SMTP_PASSWORD`

```powershell
# Chỉ cho phiên hiện tại
$env:SPORTFIELD_SMTP_PASSWORD = "app-password-16-ky-tu"

# Lưu cho tài khoản Windows hiện tại (mở terminal mới để có hiệu lực)
[Environment]::SetEnvironmentVariable("SPORTFIELD_SMTP_PASSWORD", "app-password-16-ky-tu", "User")
```

Không đặt biến này trong file `.cmd`, `.ps1`, `.json` được commit.

## 2. Tệp DPAPI cục bộ (khuyến nghị cho máy trạm)

Đường dẫn: `%LocalAppData%\SportZone\smtp.dat`
Nội dung là mật khẩu được mã hóa bằng Windows DPAPI, scope `CurrentUser` (chỉ tài khoản Windows đã mã hóa mới giải mã được). Không copy tệp này sang máy khác.

Tạo tệp bằng PowerShell (nhập mật khẩu dạng ẩn, không lưu vào lịch sử lệnh):

```powershell
Add-Type -AssemblyName System.Security
$secure = Read-Host "Nhap Google App Password" -AsSecureString
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try {
    $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    $bytes = [Text.Encoding]::UTF8.GetBytes($plain)
    $enc = [Security.Cryptography.ProtectedData]::Protect($bytes, $null, 'CurrentUser')
    $dir = Join-Path $env:LOCALAPPDATA "SportZone"
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    [IO.File]::WriteAllBytes((Join-Path $dir "smtp.dat"), $enc)
} finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    Remove-Variable plain, $secure -ErrorAction SilentlyContinue
}
```

## 3. Fallback `App.config`

Chỉ dùng cho môi trường phát triển cục bộ, không commit giá trị thật. Khi giá trị vẫn là placeholder, `EmailService.IsConfigured` trả `false` và `SendEmail` báo lỗi cấu hình thay vì gửi thư.

## Lưu ý bảo mật

- Mật khẩu ứng dụng Gmail cũ đã từng nằm trong lịch sử Git của repository này. Cần **thu hồi và tạo App Password mới** trong Tài khoản Google (Bảo mật → Mật khẩu ứng dụng). Việc thu hồi do người dùng thực hiện; không tự đổi tài khoản.
- Không ghi mật khẩu vào log, báo cáo, ảnh chụp màn hình, commit hay tin nhắn.
- Không sửa lịch sử Git trong phạm vi job này.
