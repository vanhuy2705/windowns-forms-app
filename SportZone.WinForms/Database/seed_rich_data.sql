USE QuanLySanTheThaoDB;
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- 3. Insert Bookings (DonDatSan + ChiTietDatSan + HoaDon)
DECLARE @cId INT = (SELECT TOP 1 KhachHangID FROM dbo.KhachHang WHERE SoDienThoai='0988123456');
IF @cId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.DonDatSan WHERE MaDon = 'BK-2026-1094')
BEGIN
    DECLARE @today DATE = CAST(GETDATE() AS DATE);
    DECLARE @start1 DATETIME2(0) = DATEADD(HOUR, 20, CAST(@today AS DATETIME2(0)));
    DECLARE @end1 DATETIME2(0) = DATEADD(MINUTE, 90, @start1);

    INSERT INTO dbo.DonDatSan (MaDon, KhachHangID, NguonDon, TrangThaiDon, TrangThaiThanhToan, TienSan, TienDichVu, TienKhuyenMai, TienVoucher, TongTien, DaThanhToan, GhiChu)
    VALUES ('BK-2026-1094', @cId, 'QUAY', 'DANG_SU_DUNG', 'THANH_TOAN_MOT_PHAN', 600000, 250000, 0, 0, 850000, 500000, N'Khách VIP Gold, cọc qua VietQR');
    DECLARE @don1 BIGINT = SCOPE_IDENTITY();

    INSERT INTO dbo.ChiTietDatSan (DonDatSanID, SanID, BatDau, KetThuc, SoPhut, DonGiaSnapshot, TienSan, TenSanSnapshot, TenLoaiSanSnapshot)
    VALUES (@don1, 1, @start1, @end1, 90, 400000, 600000, N'Sân bóng đá A1 (5 người)', N'Bóng đá');

    INSERT INTO dbo.HoaDon (MaHoaDon, DonDatSanID, KhachHangID, TienSan, TienDichVu, TienKhuyenMai, TienVoucher, TongTien, DaThanhToan, TrangThaiThanhToan, NgayLap, GhiChu)
    VALUES ('HD-241024-001', @don1, @cId, 600000, 250000, 0, 0, 850000, 500000, 'THANH_TOAN_MOT_PHAN', SYSDATETIME(), N'Cọc trước 500k');
END

DECLARE @cId2 INT = (SELECT TOP 1 KhachHangID FROM dbo.KhachHang WHERE SoDienThoai='0904999112');
IF @cId2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.DonDatSan WHERE MaDon = 'BK-2026-1095')
BEGIN
    DECLARE @today2 DATE = CAST(GETDATE() AS DATE);
    DECLARE @start2 DATETIME2(0) = DATEADD(HOUR, 19, CAST(@today2 AS DATETIME2(0)));
    DECLARE @end2 DATETIME2(0) = DATEADD(HOUR, 2, @start2);

    INSERT INTO dbo.DonDatSan (MaDon, KhachHangID, NguonDon, TrangThaiDon, TrangThaiThanhToan, TienSan, TienDichVu, TienKhuyenMai, TienVoucher, TongTien, DaThanhToan, GhiChu)
    VALUES ('BK-2026-1095', @cId2, 'ONLINE', 'DA_XAC_NHAN', 'CHO_XAC_NHAN', 1200000, 0, 0, 0, 1200000, 600000, N'FC Phoenix cọc giữ sân');
    DECLARE @don2 BIGINT = SCOPE_IDENTITY();

    INSERT INTO dbo.ChiTietDatSan (DonDatSanID, SanID, BatDau, KetThuc, SoPhut, DonGiaSnapshot, TienSan, TenSanSnapshot, TenLoaiSanSnapshot)
    VALUES (@don2, 3, @start2, @end2, 120, 600000, 1200000, N'Sân bóng đá A3 (7 người)', N'Bóng đá');

    INSERT INTO dbo.HoaDon (MaHoaDon, DonDatSanID, KhachHangID, TienSan, TienDichVu, TienKhuyenMai, TienVoucher, TongTien, DaThanhToan, TrangThaiThanhToan, NgayLap, GhiChu)
    VALUES ('HD-241024-002', @don2, @cId2, 1200000, 0, 0, 0, 1200000, 600000, 'CHO_XAC_NHAN', SYSDATETIME(), N'Chờ đối soát chuyển khoản');
END

DECLARE @cId3 INT = (SELECT TOP 1 KhachHangID FROM dbo.KhachHang WHERE SoDienThoai='0912555689');
IF @cId3 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.DonDatSan WHERE MaDon = 'BK-2026-1096')
BEGIN
    DECLARE @today3 DATE = CAST(GETDATE() AS DATE);
    DECLARE @start3 DATETIME2(0) = DATEADD(MINUTE, 1230, CAST(@today3 AS DATETIME2(0)));
    DECLARE @end3 DATETIME2(0) = DATEADD(MINUTE, 90, @start3);

    INSERT INTO dbo.DonDatSan (MaDon, KhachHangID, NguonDon, TrangThaiDon, TrangThaiThanhToan, TienSan, TienDichVu, TienKhuyenMai, TienVoucher, TongTien, DaThanhToan, GhiChu)
    VALUES ('BK-2026-1096', @cId3, 'QUAY', 'HOAN_THANH', 'DA_THANH_TOAN', 240000, 0, 0, 0, 240000, 240000, N'Cầu lông C1 trả sân đúng giờ');
    DECLARE @don3 BIGINT = SCOPE_IDENTITY();

    INSERT INTO dbo.ChiTietDatSan (DonDatSanID, SanID, BatDau, KetThuc, SoPhut, DonGiaSnapshot, TienSan, TenSanSnapshot, TenLoaiSanSnapshot)
    VALUES (@don3, 4, @start3, @end3, 90, 120000, 240000, N'Sân cầu lông B1', N'Cầu lông');

    INSERT INTO dbo.HoaDon (MaHoaDon, DonDatSanID, KhachHangID, TienSan, TienDichVu, TienKhuyenMai, TienVoucher, TongTien, DaThanhToan, TrangThaiThanhToan, NgayLap, GhiChu)
    VALUES ('HD-241024-003', @don3, @cId3, 240000, 0, 0, 0, 240000, 240000, 'DA_THANH_TOAN', SYSDATETIME(), N'Thanh toán toàn bộ');
END
GO
