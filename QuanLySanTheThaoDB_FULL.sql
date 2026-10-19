/*
================================================================================
 SPORTFIELD - QuanLySanTheThaoDB - FULL ONE FILE V5 REBUILD SAFE
 SQL Server 2016+ / SQL Server Express
 C# Windows Forms .NET Framework 4.8 + ADO.NET

 CHẠY 1 LẦN DUY NHẤT TRONG SQL SERVER MANAGEMENT STUDIO:
 Ctrl + A -> Execute/F5.

 V5 KHÔNG PHỤ THUỘC VÀO DROP DATABASE.
 Thay vào đó:
 - nếu database chưa tồn tại: tạo mới;
 - nếu database đã tồn tại: xóa sạch toàn bộ object SPORTFIELD cũ theo dependency;
 - sau đó dựng lại schema từ đầu.

 CẢNH BÁO:
 - Toàn bộ dữ liệu cũ trong các bảng SPORTFIELD sẽ bị xóa.
 - Hãy backup trước nếu có dữ liệu cần giữ.
================================================================================
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

USE master;
GO

IF DB_ID(N'QuanLySanTheThaoDB') IS NULL
BEGIN
    CREATE DATABASE QuanLySanTheThaoDB;
END
GO

ALTER DATABASE QuanLySanTheThaoDB SET RECOVERY SIMPLE;
ALTER DATABASE QuanLySanTheThaoDB SET MULTI_USER;
GO

USE QuanLySanTheThaoDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

PRINT N'[00/10] Chuẩn bị làm sạch toàn bộ object SPORTFIELD cũ...';
IF OBJECT_ID(N'dbo.trg_ChiTietDatSan_KhongTrungLich',N'TR') IS NOT NULL DROP TRIGGER dbo.trg_ChiTietDatSan_KhongTrungLich;
IF OBJECT_ID(N'dbo.sp_KiemTraDayDuSportField',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_KiemTraDayDuSportField;
IF OBJECT_ID(N'dbo.sp_UI_HealthCheck',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_HealthCheck;
IF OBJECT_ID(N'dbo.sp_UI_BaoCaoHieuQuaVoucher',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_BaoCaoHieuQuaVoucher;
IF OBJECT_ID(N'dbo.sp_UI_BaoCaoTongQuan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_BaoCaoTongQuan;
IF OBJECT_ID(N'dbo.sp_UI_StaffThongKeCa',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_StaffThongKeCa;
IF OBJECT_ID(N'dbo.sp_UI_StaffLichHomNay',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_StaffLichHomNay;
IF OBJECT_ID(N'dbo.sp_UI_CustomerVoucher',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_CustomerVoucher;
IF OBJECT_ID(N'dbo.sp_UI_CustomerHoaDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_CustomerHoaDon;
IF OBJECT_ID(N'dbo.sp_UI_CustomerLichSuDatSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_CustomerLichSuDatSan;
IF OBJECT_ID(N'dbo.sp_UI_CustomerHome',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_CustomerHome;
IF OBJECT_ID(N'dbo.sp_UI_DanhDauThongBaoDaDoc',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhDauThongBaoDaDoc;
IF OBJECT_ID(N'dbo.sp_UI_LayThongBao',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LayThongBao;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachNhatKyHeThong',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachNhatKyHeThong;
IF OBJECT_ID(N'dbo.sp_UI_LuuCauHinh',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LuuCauHinh;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachCauHinh',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachCauHinh;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachDatCocOnline',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachDatCocOnline;
IF OBJECT_ID(N'dbo.sp_UI_LichSuKho',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LichSuKho;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachDichVuTonKho',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachDichVuTonKho;
IF OBJECT_ID(N'dbo.sp_UI_LuuKhuyenMai',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LuuKhuyenMai;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachKhuyenMai',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachKhuyenMai;
IF OBJECT_ID(N'dbo.sp_UI_ThemVoucherVaoViKhach',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_ThemVoucherVaoViKhach;
IF OBJECT_ID(N'dbo.sp_UI_LuuVoucher',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LuuVoucher;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachVoucher',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachVoucher;
IF OBJECT_ID(N'dbo.sp_UI_LuuTaiKhoanNganHang',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LuuTaiKhoanNganHang;
IF OBJECT_ID(N'dbo.sp_UI_ThongTinNganHang',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_ThongTinNganHang;
IF OBJECT_ID(N'dbo.sp_UI_LichSuThanhToan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LichSuThanhToan;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachHoaDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachHoaDon;
IF OBJECT_ID(N'dbo.sp_UI_MaTranQuyen',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_MaTranQuyen;
IF OBJECT_ID(N'dbo.sp_UI_LuuPhanCa',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LuuPhanCa;
IF OBJECT_ID(N'dbo.sp_UI_LichPhanCa',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LichPhanCa;
IF OBJECT_ID(N'dbo.sp_UI_CapNhatNhanVien',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_CapNhatNhanVien;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachNhanVien',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachNhanVien;
IF OBJECT_ID(N'dbo.sp_UI_CustomerProfile',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_CustomerProfile;
IF OBJECT_ID(N'dbo.sp_UI_CapNhatKhachHang',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_CapNhatKhachHang;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachKhachHang',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachKhachHang;
IF OBJECT_ID(N'dbo.sp_UI_LichSuTrangThaiDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LichSuTrangThaiDon;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachDonDatSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachDonDatSan;
IF OBJECT_ID(N'dbo.sp_UI_LichSanTheoNgay',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LichSanTheoNgay;
IF OBJECT_ID(N'dbo.sp_UI_CapNhatTienIchSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_CapNhatTienIchSan;
IF OBJECT_ID(N'dbo.sp_UI_KetThucBaoTriSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_KetThucBaoTriSan;
IF OBJECT_ID(N'dbo.sp_UI_TaoBaoTriSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_TaoBaoTriSan;
IF OBJECT_ID(N'dbo.sp_UI_LayLichBaoTriSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LayLichBaoTriSan;
IF OBJECT_ID(N'dbo.sp_UI_LuuBangGia',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LuuBangGia;
IF OBJECT_ID(N'dbo.sp_UI_LayBangGia',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LayBangGia;
IF OBJECT_ID(N'dbo.sp_UI_LuuSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LuuSan;
IF OBJECT_ID(N'dbo.sp_UI_ChiTietSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_ChiTietSan;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachSan;
IF OBJECT_ID(N'dbo.sp_UI_LuuLoaiSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_LuuLoaiSan;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachLoaiSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachLoaiSan;
IF OBJECT_ID(N'dbo.sp_UI_DanhSachTaiKhoan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_DanhSachTaiKhoan;
IF OBJECT_ID(N'dbo.sp_UI_CapNhatMatKhau',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_CapNhatMatKhau;
IF OBJECT_ID(N'dbo.sp_UI_ThongTinDangNhap',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_ThongTinDangNhap;
IF OBJECT_ID(N'dbo.sp_UI_TimKiemNhanh',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_UI_TimKiemNhanh;
IF OBJECT_ID(N'dbo.sp_DongBoChiTietHoaDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DongBoChiTietHoaDon;
IF OBJECT_ID(N'dbo.sp_GhiNhatKyCauHinh',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_GhiNhatKyCauHinh;
IF OBJECT_ID(N'dbo.sp_GhiNhatKyXuatBaoCao',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_GhiNhatKyXuatBaoCao;
IF OBJECT_ID(N'dbo.sp_GhiNhatKyIn',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_GhiNhatKyIn;
IF OBJECT_ID(N'dbo.sp_TaoLenhIoT',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TaoLenhIoT;
IF OBJECT_ID(N'dbo.sp_GuiTinNhanVaoHangDoi',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_GuiTinNhanVaoHangDoi;
IF OBJECT_ID(N'dbo.sp_KhopSaoKeVoiThanhToan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_KhopSaoKeVoiThanhToan;
IF OBJECT_ID(N'dbo.sp_ImportSaoKeNganHang',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_ImportSaoKeNganHang;
IF OBJECT_ID(N'dbo.sp_GhiNhanTraVatDung',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_GhiNhanTraVatDung;
IF OBJECT_ID(N'dbo.sp_ThemChiTietPhieuNhapKho',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_ThemChiTietPhieuNhapKho;
IF OBJECT_ID(N'dbo.sp_TaoPhieuNhapKho',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TaoPhieuNhapKho;
IF OBJECT_ID(N'dbo.sp_TimChinhSachHoanCoc',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TimChinhSachHoanCoc;
IF OBJECT_ID(N'dbo.sp_GhiLichSuDoiLich',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_GhiLichSuDoiLich;
IF OBJECT_ID(N'dbo.sp_TraSanVaHoanTat',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TraSanVaHoanTat;
IF OBJECT_ID(N'dbo.sp_CheckInDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_CheckInDon;
IF OBJECT_ID(N'dbo.sp_DoiTrangThaiSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DoiTrangThaiSan;
IF OBJECT_ID(N'dbo.sp_GanHangThanhVien',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_GanHangThanhVien;
IF OBJECT_ID(N'dbo.sp_CongTruDiemKhachHang',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_CongTruDiemKhachHang;
IF OBJECT_ID(N'dbo.sp_CapQuyenTaiKhoan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_CapQuyenTaiKhoan;
IF OBJECT_ID(N'dbo.sp_DongPhienDangNhap',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DongPhienDangNhap;
IF OBJECT_ID(N'dbo.sp_TaoPhienDangNhap',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TaoPhienDangNhap;
IF OBJECT_ID(N'dbo.sp_GhiNhanDangNhap',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_GhiNhanDangNhap;
IF OBJECT_ID(N'dbo.sp_LayQuyenTaiKhoan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_LayQuyenTaiKhoan;
IF OBJECT_ID(N'dbo.sp_CapVoucherChoKhachHang',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_CapVoucherChoKhachHang;
IF OBJECT_ID(N'dbo.sp_DoiMatKhauHash',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DoiMatKhauHash;
IF OBJECT_ID(N'dbo.sp_DatLaiMatKhau',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DatLaiMatKhau;
IF OBJECT_ID(N'dbo.sp_KhoaMoTaiKhoan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_KhoaMoTaiKhoan;
IF OBJECT_ID(N'dbo.sp_TaoNhanVienVaTaiKhoan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TaoNhanVienVaTaiKhoan;
IF OBJECT_ID(N'dbo.sp_DangKyKhachHang',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DangKyKhachHang;
IF OBJECT_ID(N'dbo.sp_DonDepGiuChoHetHan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DonDepGiuChoHetHan;
IF OBJECT_ID(N'dbo.sp_TaoGiuChoOnline',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TaoGiuChoOnline;
IF OBJECT_ID(N'dbo.sp_LayVoucherKhachHang',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_LayVoucherKhachHang;
IF OBJECT_ID(N'dbo.sp_ThongKeDoanhThu',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_ThongKeDoanhThu;
IF OBJECT_ID(N'dbo.sp_DashboardNhanVien',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DashboardNhanVien;
IF OBJECT_ID(N'dbo.sp_DashboardAdmin',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DashboardAdmin;
IF OBJECT_ID(N'dbo.sp_DongCa',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DongCa;
IF OBJECT_ID(N'dbo.sp_MoCa',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_MoCa;
IF OBJECT_ID(N'dbo.sp_HoanTatKiemKeKho',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_HoanTatKiemKeKho;
IF OBJECT_ID(N'dbo.sp_XuatKho',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_XuatKho;
IF OBJECT_ID(N'dbo.sp_NhapKho',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_NhapKho;
IF OBJECT_ID(N'dbo.sp_HoanTien',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_HoanTien;
IF OBJECT_ID(N'dbo.sp_TuChoiChuyenKhoan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TuChoiChuyenKhoan;
IF OBJECT_ID(N'dbo.sp_XacNhanChuyenKhoan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_XacNhanChuyenKhoan;
IF OBJECT_ID(N'dbo.sp_TaoYeuCauVietQR',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TaoYeuCauVietQR;
IF OBJECT_ID(N'dbo.sp_GhiNhanThanhToan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_GhiNhanThanhToan;
IF OBJECT_ID(N'dbo.sp_TaoHoaDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TaoHoaDon;
IF OBJECT_ID(N'dbo.sp_ApDungVoucher',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_ApDungVoucher;
IF OBJECT_ID(N'dbo.sp_ThemDichVuVaoDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_ThemDichVuVaoDon;
IF OBJECT_ID(N'dbo.sp_TinhLaiTongTienDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TinhLaiTongTienDon;
IF OBJECT_ID(N'dbo.sp_DoiLich',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_DoiLich;
IF OBJECT_ID(N'dbo.sp_HuyDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_HuyDon;
IF OBJECT_ID(N'dbo.sp_HoanTatDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_HoanTatDon;
IF OBJECT_ID(N'dbo.sp_CheckIn',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_CheckIn;
IF OBJECT_ID(N'dbo.sp_XacNhanDon',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_XacNhanDon;
IF OBJECT_ID(N'dbo.sp_TaoDonDatSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TaoDonDatSan;
IF OBJECT_ID(N'dbo.sp_TinhGiaDatSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TinhGiaDatSan;
IF OBJECT_ID(N'dbo.sp_TimSanTrong',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TimSanTrong;
IF OBJECT_ID(N'dbo.sp_KiemTraTrungLichSan',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_KiemTraTrungLichSan;
IF OBJECT_ID(N'dbo.sp_TaoKhachHang',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TaoKhachHang;
IF OBJECT_ID(N'dbo.sp_TaoTaiKhoanAdminBanDau',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_TaoTaiKhoanAdminBanDau;
IF OBJECT_ID(N'dbo.sp_LayTaiKhoanDangNhap',N'P') IS NOT NULL DROP PROCEDURE dbo.sp_LayTaiKhoanDangNhap;
IF OBJECT_ID(N'dbo.vw_LichSanHomNay',N'V') IS NOT NULL DROP VIEW dbo.vw_LichSanHomNay;
IF OBJECT_ID(N'dbo.vw_ChiTietHoaDonInAn',N'V') IS NOT NULL DROP VIEW dbo.vw_ChiTietHoaDonInAn;
IF OBJECT_ID(N'dbo.vw_DoiSoatChuyenKhoanChoXuLy',N'V') IS NOT NULL DROP VIEW dbo.vw_DoiSoatChuyenKhoanChoXuLy;
IF OBJECT_ID(N'dbo.vw_ThietBiIoTTrangThai',N'V') IS NOT NULL DROP VIEW dbo.vw_ThietBiIoTTrangThai;
IF OBJECT_ID(N'dbo.vw_CaDangMo',N'V') IS NOT NULL DROP VIEW dbo.vw_CaDangMo;
IF OBJECT_ID(N'dbo.vw_CanhBaoTonKho',N'V') IS NOT NULL DROP VIEW dbo.vw_CanhBaoTonKho;
IF OBJECT_ID(N'dbo.vw_DonOnlineChoXuLy',N'V') IS NOT NULL DROP VIEW dbo.vw_DonOnlineChoXuLy;
IF OBJECT_ID(N'dbo.vw_VoucherKhachHangKhaDung',N'V') IS NOT NULL DROP VIEW dbo.vw_VoucherKhachHangKhaDung;
IF OBJECT_ID(N'dbo.vw_TaiKhoanPhanQuyenHieuLuc',N'V') IS NOT NULL DROP VIEW dbo.vw_TaiKhoanPhanQuyenHieuLuc;
IF OBJECT_ID(N'dbo.vw_DoanhThuTheoLoaiSan',N'V') IS NOT NULL DROP VIEW dbo.vw_DoanhThuTheoLoaiSan;
IF OBJECT_ID(N'dbo.vw_DoanhThuTheoSan',N'V') IS NOT NULL DROP VIEW dbo.vw_DoanhThuTheoSan;
IF OBJECT_ID(N'dbo.vw_DoanhThuTheoNgay',N'V') IS NOT NULL DROP VIEW dbo.vw_DoanhThuTheoNgay;
IF OBJECT_ID(N'dbo.vw_KhachHangTongHop',N'V') IS NOT NULL DROP VIEW dbo.vw_KhachHangTongHop;
IF OBJECT_ID(N'dbo.vw_TonKhoHienTai',N'V') IS NOT NULL DROP VIEW dbo.vw_TonKhoHienTai;
IF OBJECT_ID(N'dbo.vw_GiaoDichChoXacNhan',N'V') IS NOT NULL DROP VIEW dbo.vw_GiaoDichChoXacNhan;
IF OBJECT_ID(N'dbo.vw_HoaDonConNo',N'V') IS NOT NULL DROP VIEW dbo.vw_HoaDonConNo;
IF OBJECT_ID(N'dbo.vw_TrangThaiSanHienTai',N'V') IS NOT NULL DROP VIEW dbo.vw_TrangThaiSanHienTai;
IF OBJECT_ID(N'dbo.vw_DonDatSanDayDu',N'V') IS NOT NULL DROP VIEW dbo.vw_DonDatSanDayDu;
IF OBJECT_ID(N'dbo.fn_TinhTienSan',N'FN') IS NOT NULL DROP FUNCTION dbo.fn_TinhTienSan;
IF OBJECT_ID(N'dbo.fn_DonGiaSanTaiThoiDiem',N'FN') IS NOT NULL DROP FUNCTION dbo.fn_DonGiaSanTaiThoiDiem;
IF OBJECT_ID(N'dbo.DonDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Don_DoiKhachHang',N'F') IS NOT NULL ALTER TABLE dbo.DonDatSan DROP CONSTRAINT FK_Don_DoiKhachHang;
IF OBJECT_ID(N'dbo.LichPhanCa',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LichPhanCa_MauCa',N'F') IS NOT NULL ALTER TABLE dbo.LichPhanCa DROP CONSTRAINT FK_LichPhanCa_MauCa;
IF OBJECT_ID(N'dbo.SanPhamDichVu',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_SPDV_Nhom',N'F') IS NOT NULL ALTER TABLE dbo.SanPhamDichVu DROP CONSTRAINT FK_SPDV_Nhom;
IF OBJECT_ID(N'dbo.VoucherKhachHang',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_VKH_Don',N'F') IS NOT NULL ALTER TABLE dbo.VoucherKhachHang DROP CONSTRAINT FK_VKH_Don;
IF OBJECT_ID(N'dbo.DatCocOnline',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DatCoc_QR',N'F') IS NOT NULL ALTER TABLE dbo.DatCocOnline DROP CONSTRAINT FK_DatCoc_QR;
IF OBJECT_ID(N'dbo.DatCocOnline',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DatCoc_GiuCho',N'F') IS NOT NULL ALTER TABLE dbo.DatCocOnline DROP CONSTRAINT FK_DatCoc_GiuCho;
IF OBJECT_ID(N'dbo.DoiSoatThanhToan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DoiSoat_TK',N'F') IS NOT NULL ALTER TABLE dbo.DoiSoatThanhToan DROP CONSTRAINT FK_DoiSoat_TK;
IF OBJECT_ID(N'dbo.DoiSoatThanhToan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DoiSoat_SaoKe',N'F') IS NOT NULL ALTER TABLE dbo.DoiSoatThanhToan DROP CONSTRAINT FK_DoiSoat_SaoKe;
IF OBJECT_ID(N'dbo.DoiSoatThanhToan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DoiSoat_GD',N'F') IS NOT NULL ALTER TABLE dbo.DoiSoatThanhToan DROP CONSTRAINT FK_DoiSoat_GD;
IF OBJECT_ID(N'dbo.HoanTien',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_HoanTien_TK',N'F') IS NOT NULL ALTER TABLE dbo.HoanTien DROP CONSTRAINT FK_HoanTien_TK;
IF OBJECT_ID(N'dbo.HoanTien',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_HoanTien_Don',N'F') IS NOT NULL ALTER TABLE dbo.HoanTien DROP CONSTRAINT FK_HoanTien_Don;
IF OBJECT_ID(N'dbo.HoanTien',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_HoanTien_GD',N'F') IS NOT NULL ALTER TABLE dbo.HoanTien DROP CONSTRAINT FK_HoanTien_GD;
IF OBJECT_ID(N'dbo.YeuCauVietQR',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_QR_Bank',N'F') IS NOT NULL ALTER TABLE dbo.YeuCauVietQR DROP CONSTRAINT FK_QR_Bank;
IF OBJECT_ID(N'dbo.YeuCauVietQR',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_QR_GDTT',N'F') IS NOT NULL ALTER TABLE dbo.YeuCauVietQR DROP CONSTRAINT FK_QR_GDTT;
IF OBJECT_ID(N'dbo.ChiTietHoaDon',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_CTHD_HD',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietHoaDon DROP CONSTRAINT FK_CTHD_HD;
IF OBJECT_ID(N'dbo.ChiTietThueVatDung',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Thue_Nhan',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietThueVatDung DROP CONSTRAINT FK_Thue_Nhan;
IF OBJECT_ID(N'dbo.ChiTietThueVatDung',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Thue_Giao',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietThueVatDung DROP CONSTRAINT FK_Thue_Giao;
IF OBJECT_ID(N'dbo.ChiTietThueVatDung',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Thue_CT',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietThueVatDung DROP CONSTRAINT FK_Thue_CT;
IF OBJECT_ID(N'dbo.SuKienSuDungSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_SuKienSan_TK',N'F') IS NOT NULL ALTER TABLE dbo.SuKienSuDungSan DROP CONSTRAINT FK_SuKienSan_TK;
IF OBJECT_ID(N'dbo.SuKienSuDungSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_SuKienSan_CT',N'F') IS NOT NULL ALTER TABLE dbo.SuKienSuDungSan DROP CONSTRAINT FK_SuKienSan_CT;
IF OBJECT_ID(N'dbo.SuKienSuDungSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_SuKienSan_Don',N'F') IS NOT NULL ALTER TABLE dbo.SuKienSuDungSan DROP CONSTRAINT FK_SuKienSan_Don;
IF OBJECT_ID(N'dbo.GiaoDichThanhToan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GDTT_XN',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichThanhToan DROP CONSTRAINT FK_GDTT_XN;
IF OBJECT_ID(N'dbo.GiaoDichThanhToan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GDTT_Tao',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichThanhToan DROP CONSTRAINT FK_GDTT_Tao;
IF OBJECT_ID(N'dbo.GiaoDichThanhToan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GDTT_Don',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichThanhToan DROP CONSTRAINT FK_GDTT_Don;
IF OBJECT_ID(N'dbo.GiaoDichThanhToan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GDTT_HD',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichThanhToan DROP CONSTRAINT FK_GDTT_HD;
IF OBJECT_ID(N'dbo.LichSuGuiTinNhan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LSGuiTN_HangDoi',N'F') IS NOT NULL ALTER TABLE dbo.LichSuGuiTinNhan DROP CONSTRAINT FK_LSGuiTN_HangDoi;
IF OBJECT_ID(N'dbo.ChiTietPhieuNhapKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_CTPNK_SP',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietPhieuNhapKho DROP CONSTRAINT FK_CTPNK_SP;
IF OBJECT_ID(N'dbo.ChiTietPhieuNhapKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_CTPNK_Phieu',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietPhieuNhapKho DROP CONSTRAINT FK_CTPNK_Phieu;
IF OBJECT_ID(N'dbo.TepDinhKemDon',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TepDon_TK',N'F') IS NOT NULL ALTER TABLE dbo.TepDinhKemDon DROP CONSTRAINT FK_TepDon_TK;
IF OBJECT_ID(N'dbo.TepDinhKemDon',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TepDon_Don',N'F') IS NOT NULL ALTER TABLE dbo.TepDinhKemDon DROP CONSTRAINT FK_TepDon_Don;
IF OBJECT_ID(N'dbo.LichSuDoiLich',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DoiLich_TK',N'F') IS NOT NULL ALTER TABLE dbo.LichSuDoiLich DROP CONSTRAINT FK_DoiLich_TK;
IF OBJECT_ID(N'dbo.LichSuDoiLich',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DoiLich_SanMoi',N'F') IS NOT NULL ALTER TABLE dbo.LichSuDoiLich DROP CONSTRAINT FK_DoiLich_SanMoi;
IF OBJECT_ID(N'dbo.LichSuDoiLich',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DoiLich_SanCu',N'F') IS NOT NULL ALTER TABLE dbo.LichSuDoiLich DROP CONSTRAINT FK_DoiLich_SanCu;
IF OBJECT_ID(N'dbo.LichSuDoiLich',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DoiLich_Don',N'F') IS NOT NULL ALTER TABLE dbo.LichSuDoiLich DROP CONSTRAINT FK_DoiLich_Don;
IF OBJECT_ID(N'dbo.LichSuVoucher',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LSVoucher_TK',N'F') IS NOT NULL ALTER TABLE dbo.LichSuVoucher DROP CONSTRAINT FK_LSVoucher_TK;
IF OBJECT_ID(N'dbo.LichSuVoucher',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LSVoucher_Don',N'F') IS NOT NULL ALTER TABLE dbo.LichSuVoucher DROP CONSTRAINT FK_LSVoucher_Don;
IF OBJECT_ID(N'dbo.LichSuVoucher',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LSVoucher_VKH',N'F') IS NOT NULL ALTER TABLE dbo.LichSuVoucher DROP CONSTRAINT FK_LSVoucher_VKH;
IF OBJECT_ID(N'dbo.GiaoDichDiemTichLuy',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Diem_TK',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichDiemTichLuy DROP CONSTRAINT FK_Diem_TK;
IF OBJECT_ID(N'dbo.GiaoDichDiemTichLuy',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Diem_Don',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichDiemTichLuy DROP CONSTRAINT FK_Diem_Don;
IF OBJECT_ID(N'dbo.GiaoDichDiemTichLuy',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Diem_KH',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichDiemTichLuy DROP CONSTRAINT FK_Diem_KH;
IF OBJECT_ID(N'dbo.GiaoDichVi',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GDVi_Don',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichVi DROP CONSTRAINT FK_GDVi_Don;
IF OBJECT_ID(N'dbo.GiaoDichVi',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GDVi_Vi',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichVi DROP CONSTRAINT FK_GDVi_Vi;
IF OBJECT_ID(N'dbo.HoaDon',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_HD_TK',N'F') IS NOT NULL ALTER TABLE dbo.HoaDon DROP CONSTRAINT FK_HD_TK;
IF OBJECT_ID(N'dbo.HoaDon',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_HD_KH',N'F') IS NOT NULL ALTER TABLE dbo.HoaDon DROP CONSTRAINT FK_HD_KH;
IF OBJECT_ID(N'dbo.HoaDon',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_HD_Don',N'F') IS NOT NULL ALTER TABLE dbo.HoaDon DROP CONSTRAINT FK_HD_Don;
IF OBJECT_ID(N'dbo.ChiTietKiemKeKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_CTKK_SPDV',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietKiemKeKho DROP CONSTRAINT FK_CTKK_SPDV;
IF OBJECT_ID(N'dbo.ChiTietKiemKeKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_CTKK_KK',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietKiemKeKho DROP CONSTRAINT FK_CTKK_KK;
IF OBJECT_ID(N'dbo.ChiTietDichVuDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_CTDV_SPDV',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietDichVuDatSan DROP CONSTRAINT FK_CTDV_SPDV;
IF OBJECT_ID(N'dbo.ChiTietDichVuDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_CTDV_Don',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietDichVuDatSan DROP CONSTRAINT FK_CTDV_Don;
IF OBJECT_ID(N'dbo.LichSuTrangThaiDon',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LS_Don_TK',N'F') IS NOT NULL ALTER TABLE dbo.LichSuTrangThaiDon DROP CONSTRAINT FK_LS_Don_TK;
IF OBJECT_ID(N'dbo.LichSuTrangThaiDon',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LS_Don_Don',N'F') IS NOT NULL ALTER TABLE dbo.LichSuTrangThaiDon DROP CONSTRAINT FK_LS_Don_Don;
IF OBJECT_ID(N'dbo.ChiTietDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_CTDatSan_San',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietDatSan DROP CONSTRAINT FK_CTDatSan_San;
IF OBJECT_ID(N'dbo.ChiTietDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_CTDatSan_Don',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietDatSan DROP CONSTRAINT FK_CTDatSan_Don;
IF OBJECT_ID(N'dbo.NhatKyCauHinh',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_ConfigLog_TK',N'F') IS NOT NULL ALTER TABLE dbo.NhatKyCauHinh DROP CONSTRAINT FK_ConfigLog_TK;
IF OBJECT_ID(N'dbo.LichSuSaoLuu',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Backup_TK',N'F') IS NOT NULL ALTER TABLE dbo.LichSuSaoLuu DROP CONSTRAINT FK_Backup_TK;
IF OBJECT_ID(N'dbo.NhatKyXuatBaoCao',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_XuatBC_TK',N'F') IS NOT NULL ALTER TABLE dbo.NhatKyXuatBaoCao DROP CONSTRAINT FK_XuatBC_TK;
IF OBJECT_ID(N'dbo.NhatKyInAn',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_InAn_TK',N'F') IS NOT NULL ALTER TABLE dbo.NhatKyInAn DROP CONSTRAINT FK_InAn_TK;
IF OBJECT_ID(N'dbo.HangDoiTinNhan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_HangDoiTN_TK',N'F') IS NOT NULL ALTER TABLE dbo.HangDoiTinNhan DROP CONSTRAINT FK_HangDoiTN_TK;
IF OBJECT_ID(N'dbo.HangDoiTinNhan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_HangDoiTN_KH',N'F') IS NOT NULL ALTER TABLE dbo.HangDoiTinNhan DROP CONSTRAINT FK_HangDoiTN_KH;
IF OBJECT_ID(N'dbo.LenhDieuKhienIoT',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LenhIoT_TK',N'F') IS NOT NULL ALTER TABLE dbo.LenhDieuKhienIoT DROP CONSTRAINT FK_LenhIoT_TK;
IF OBJECT_ID(N'dbo.LenhDieuKhienIoT',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LenhIoT_TB',N'F') IS NOT NULL ALTER TABLE dbo.LenhDieuKhienIoT DROP CONSTRAINT FK_LenhIoT_TB;
IF OBJECT_ID(N'dbo.PhieuNhapKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_PNK_TK',N'F') IS NOT NULL ALTER TABLE dbo.PhieuNhapKho DROP CONSTRAINT FK_PNK_TK;
IF OBJECT_ID(N'dbo.PhieuNhapKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_PNK_NCC',N'F') IS NOT NULL ALTER TABLE dbo.PhieuNhapKho DROP CONSTRAINT FK_PNK_NCC;
IF OBJECT_ID(N'dbo.PhieuNhapKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_PNK_Kho',N'F') IS NOT NULL ALTER TABLE dbo.PhieuNhapKho DROP CONSTRAINT FK_PNK_Kho;
IF OBJECT_ID(N'dbo.DotPhatHanhVoucher',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DotVoucher_TK',N'F') IS NOT NULL ALTER TABLE dbo.DotPhatHanhVoucher DROP CONSTRAINT FK_DotVoucher_TK;
IF OBJECT_ID(N'dbo.DotPhatHanhVoucher',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DotVoucher_Voucher',N'F') IS NOT NULL ALTER TABLE dbo.DotPhatHanhVoucher DROP CONSTRAINT FK_DotVoucher_Voucher;
IF OBJECT_ID(N'dbo.LichSuTrangThaiSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LSStatusSan_TK',N'F') IS NOT NULL ALTER TABLE dbo.LichSuTrangThaiSan DROP CONSTRAINT FK_LSStatusSan_TK;
IF OBJECT_ID(N'dbo.LichSuTrangThaiSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LSStatusSan_San',N'F') IS NOT NULL ALTER TABLE dbo.LichSuTrangThaiSan DROP CONSTRAINT FK_LSStatusSan_San;
IF OBJECT_ID(N'dbo.LichDongCuaCoSo',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DongCua_TK',N'F') IS NOT NULL ALTER TABLE dbo.LichDongCuaCoSo DROP CONSTRAINT FK_DongCua_TK;
IF OBJECT_ID(N'dbo.LichDongCuaCoSo',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DongCua_Cum',N'F') IS NOT NULL ALTER TABLE dbo.LichDongCuaCoSo DROP CONSTRAINT FK_DongCua_Cum;
IF OBJECT_ID(N'dbo.ThanhVienDoi',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TVDoi_KH',N'F') IS NOT NULL ALTER TABLE dbo.ThanhVienDoi DROP CONSTRAINT FK_TVDoi_KH;
IF OBJECT_ID(N'dbo.ThanhVienDoi',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TVDoi_Doi',N'F') IS NOT NULL ALTER TABLE dbo.ThanhVienDoi DROP CONSTRAINT FK_TVDoi_Doi;
IF OBJECT_ID(N'dbo.YeuCauDatLaiMatKhau',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Reset_TK',N'F') IS NOT NULL ALTER TABLE dbo.YeuCauDatLaiMatKhau DROP CONSTRAINT FK_Reset_TK;
IF OBJECT_ID(N'dbo.ThietBiDangNhap',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_ThietBiDN_TK',N'F') IS NOT NULL ALTER TABLE dbo.ThietBiDangNhap DROP CONSTRAINT FK_ThietBiDN_TK;
IF OBJECT_ID(N'dbo.PhienDangNhap',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Phien_TK',N'F') IS NOT NULL ALTER TABLE dbo.PhienDangNhap DROP CONSTRAINT FK_Phien_TK;
IF OBJECT_ID(N'dbo.TaiKhoanQuyen',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TKQ_NguoiCap',N'F') IS NOT NULL ALTER TABLE dbo.TaiKhoanQuyen DROP CONSTRAINT FK_TKQ_NguoiCap;
IF OBJECT_ID(N'dbo.TaiKhoanQuyen',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TKQ_Q',N'F') IS NOT NULL ALTER TABLE dbo.TaiKhoanQuyen DROP CONSTRAINT FK_TKQ_Q;
IF OBJECT_ID(N'dbo.TaiKhoanQuyen',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TKQ_TK',N'F') IS NOT NULL ALTER TABLE dbo.TaiKhoanQuyen DROP CONSTRAINT FK_TKQ_TK;
IF OBJECT_ID(N'dbo.NhatKyHeThong',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_NhatKy_TK',N'F') IS NOT NULL ALTER TABLE dbo.NhatKyHeThong DROP CONSTRAINT FK_NhatKy_TK;
IF OBJECT_ID(N'dbo.ThongBao',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_ThongBao_TK',N'F') IS NOT NULL ALTER TABLE dbo.ThongBao DROP CONSTRAINT FK_ThongBao_TK;
IF OBJECT_ID(N'dbo.NhatKyThietBi',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_NKTB_TK',N'F') IS NOT NULL ALTER TABLE dbo.NhatKyThietBi DROP CONSTRAINT FK_NKTB_TK;
IF OBJECT_ID(N'dbo.NhatKyThietBi',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_NKTB_IOT',N'F') IS NOT NULL ALTER TABLE dbo.NhatKyThietBi DROP CONSTRAINT FK_NKTB_IOT;
IF OBJECT_ID(N'dbo.KiemKeKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_KiemKe_TK',N'F') IS NOT NULL ALTER TABLE dbo.KiemKeKho DROP CONSTRAINT FK_KiemKe_TK;
IF OBJECT_ID(N'dbo.KiemKeKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_KiemKe_Kho',N'F') IS NOT NULL ALTER TABLE dbo.KiemKeKho DROP CONSTRAINT FK_KiemKe_Kho;
IF OBJECT_ID(N'dbo.GiaoDichKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GDKho_TK',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichKho DROP CONSTRAINT FK_GDKho_TK;
IF OBJECT_ID(N'dbo.GiaoDichKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GDKho_SPDV',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichKho DROP CONSTRAINT FK_GDKho_SPDV;
IF OBJECT_ID(N'dbo.GiaoDichKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GDKho_Kho',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichKho DROP CONSTRAINT FK_GDKho_Kho;
IF OBJECT_ID(N'dbo.DonDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Don_TaiKhoan',N'F') IS NOT NULL ALTER TABLE dbo.DonDatSan DROP CONSTRAINT FK_Don_TaiKhoan;
IF OBJECT_ID(N'dbo.DonDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Don_KhachHang',N'F') IS NOT NULL ALTER TABLE dbo.DonDatSan DROP CONSTRAINT FK_Don_KhachHang;
IF OBJECT_ID(N'dbo.LichBaoTriSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_BaoTri_TaiKhoan',N'F') IS NOT NULL ALTER TABLE dbo.LichBaoTriSan DROP CONSTRAINT FK_BaoTri_TaiKhoan;
IF OBJECT_ID(N'dbo.LichBaoTriSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_BaoTri_San',N'F') IS NOT NULL ALTER TABLE dbo.LichBaoTriSan DROP CONSTRAINT FK_BaoTri_San;
IF OBJECT_ID(N'dbo.LichSuDangNhap',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_LichSuDangNhap_TaiKhoan',N'F') IS NOT NULL ALTER TABLE dbo.LichSuDangNhap DROP CONSTRAINT FK_LichSuDangNhap_TaiKhoan;
IF OBJECT_ID(N'dbo.ThietBiPOS',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TBPOS_POS',N'F') IS NOT NULL ALTER TABLE dbo.ThietBiPOS DROP CONSTRAINT FK_TBPOS_POS;
IF OBJECT_ID(N'dbo.ChiTietDoiSoatCa',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_CTDSCa_Ca',N'F') IS NOT NULL ALTER TABLE dbo.ChiTietDoiSoatCa DROP CONSTRAINT FK_CTDSCa_Ca;
IF OBJECT_ID(N'dbo.VoucherSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_VS_San',N'F') IS NOT NULL ALTER TABLE dbo.VoucherSan DROP CONSTRAINT FK_VS_San;
IF OBJECT_ID(N'dbo.VoucherSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_VS_Voucher',N'F') IS NOT NULL ALTER TABLE dbo.VoucherSan DROP CONSTRAINT FK_VS_Voucher;
IF OBJECT_ID(N'dbo.PhuThuDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_PhuThu_LoaiSan',N'F') IS NOT NULL ALTER TABLE dbo.PhuThuDatSan DROP CONSTRAINT FK_PhuThu_LoaiSan;
IF OBJECT_ID(N'dbo.PhuThuDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_PhuThu_San',N'F') IS NOT NULL ALTER TABLE dbo.PhuThuDatSan DROP CONSTRAINT FK_PhuThu_San;
IF OBJECT_ID(N'dbo.DoiKhachHang',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_DoiKH_TruongDoi',N'F') IS NOT NULL ALTER TABLE dbo.DoiKhachHang DROP CONSTRAINT FK_DoiKH_TruongDoi;
IF OBJECT_ID(N'dbo.TheThanhVien',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TheTV_Hang',N'F') IS NOT NULL ALTER TABLE dbo.TheThanhVien DROP CONSTRAINT FK_TheTV_Hang;
IF OBJECT_ID(N'dbo.TheThanhVien',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TheTV_KH',N'F') IS NOT NULL ALTER TABLE dbo.TheThanhVien DROP CONSTRAINT FK_TheTV_KH;
IF OBJECT_ID(N'dbo.ThietBiIoT',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_IOT_San',N'F') IS NOT NULL ALTER TABLE dbo.ThietBiIoT DROP CONSTRAINT FK_IOT_San;
IF OBJECT_ID(N'dbo.GiuChoDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GiuCho_San',N'F') IS NOT NULL ALTER TABLE dbo.GiuChoDatSan DROP CONSTRAINT FK_GiuCho_San;
IF OBJECT_ID(N'dbo.GiuChoDatSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_GiuCho_KH',N'F') IS NOT NULL ALTER TABLE dbo.GiuChoDatSan DROP CONSTRAINT FK_GiuCho_KH;
IF OBJECT_ID(N'dbo.PhienPOS',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_PhienPOS_Ca',N'F') IS NOT NULL ALTER TABLE dbo.PhienPOS DROP CONSTRAINT FK_PhienPOS_Ca;
IF OBJECT_ID(N'dbo.PhienPOS',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_PhienPOS_NV',N'F') IS NOT NULL ALTER TABLE dbo.PhienPOS DROP CONSTRAINT FK_PhienPOS_NV;
IF OBJECT_ID(N'dbo.PhienPOS',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_PhienPOS_POS',N'F') IS NOT NULL ALTER TABLE dbo.PhienPOS DROP CONSTRAINT FK_PhienPOS_POS;
IF OBJECT_ID(N'dbo.BanGiaoCa',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_BanGiao_Nhan',N'F') IS NOT NULL ALTER TABLE dbo.BanGiaoCa DROP CONSTRAINT FK_BanGiao_Nhan;
IF OBJECT_ID(N'dbo.BanGiaoCa',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_BanGiao_Giao',N'F') IS NOT NULL ALTER TABLE dbo.BanGiaoCa DROP CONSTRAINT FK_BanGiao_Giao;
IF OBJECT_ID(N'dbo.BanGiaoCa',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_BanGiao_Ca',N'F') IS NOT NULL ALTER TABLE dbo.BanGiaoCa DROP CONSTRAINT FK_BanGiao_Ca;
IF OBJECT_ID(N'dbo.ViKhachHang',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Vi_KH',N'F') IS NOT NULL ALTER TABLE dbo.ViKhachHang DROP CONSTRAINT FK_Vi_KH;
IF OBJECT_ID(N'dbo.TonKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TonKho_SPDV',N'F') IS NOT NULL ALTER TABLE dbo.TonKho DROP CONSTRAINT FK_TonKho_SPDV;
IF OBJECT_ID(N'dbo.TonKho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TonKho_Kho',N'F') IS NOT NULL ALTER TABLE dbo.TonKho DROP CONSTRAINT FK_TonKho_Kho;
IF OBJECT_ID(N'dbo.VoucherKhachHang',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_VKH_KhachHang',N'F') IS NOT NULL ALTER TABLE dbo.VoucherKhachHang DROP CONSTRAINT FK_VKH_KhachHang;
IF OBJECT_ID(N'dbo.VoucherKhachHang',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_VKH_Voucher',N'F') IS NOT NULL ALTER TABLE dbo.VoucherKhachHang DROP CONSTRAINT FK_VKH_Voucher;
IF OBJECT_ID(N'dbo.KhuyenMaiSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_KMS_San',N'F') IS NOT NULL ALTER TABLE dbo.KhuyenMaiSan DROP CONSTRAINT FK_KMS_San;
IF OBJECT_ID(N'dbo.KhuyenMaiSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_KMS_KM',N'F') IS NOT NULL ALTER TABLE dbo.KhuyenMaiSan DROP CONSTRAINT FK_KMS_KM;
IF OBJECT_ID(N'dbo.BangGia',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_BangGia_LoaiSan',N'F') IS NOT NULL ALTER TABLE dbo.BangGia DROP CONSTRAINT FK_BangGia_LoaiSan;
IF OBJECT_ID(N'dbo.BangGia',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_BangGia_San',N'F') IS NOT NULL ALTER TABLE dbo.BangGia DROP CONSTRAINT FK_BangGia_San;
IF OBJECT_ID(N'dbo.SanTienIch',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_SanTienIch_TienIch',N'F') IS NOT NULL ALTER TABLE dbo.SanTienIch DROP CONSTRAINT FK_SanTienIch_TienIch;
IF OBJECT_ID(N'dbo.SanTienIch',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_SanTienIch_San',N'F') IS NOT NULL ALTER TABLE dbo.SanTienIch DROP CONSTRAINT FK_SanTienIch_San;
IF OBJECT_ID(N'dbo.HinhAnhSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_HinhAnhSan_San',N'F') IS NOT NULL ALTER TABLE dbo.HinhAnhSan DROP CONSTRAINT FK_HinhAnhSan_San;
IF OBJECT_ID(N'dbo.TaiKhoan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TaiKhoan_KhachHang',N'F') IS NOT NULL ALTER TABLE dbo.TaiKhoan DROP CONSTRAINT FK_TaiKhoan_KhachHang;
IF OBJECT_ID(N'dbo.TaiKhoan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TaiKhoan_NhanVien',N'F') IS NOT NULL ALTER TABLE dbo.TaiKhoan DROP CONSTRAINT FK_TaiKhoan_NhanVien;
IF OBJECT_ID(N'dbo.TaiKhoan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_TaiKhoan_VaiTro',N'F') IS NOT NULL ALTER TABLE dbo.TaiKhoan DROP CONSTRAINT FK_TaiKhoan_VaiTro;
IF OBJECT_ID(N'dbo.GiaoDichSaoKeNganHang',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_SaoKe_Bank',N'F') IS NOT NULL ALTER TABLE dbo.GiaoDichSaoKeNganHang DROP CONSTRAINT FK_SaoKe_Bank;
IF OBJECT_ID(N'dbo.VoucherLoaiSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_VLS_LoaiSan',N'F') IS NOT NULL ALTER TABLE dbo.VoucherLoaiSan DROP CONSTRAINT FK_VLS_LoaiSan;
IF OBJECT_ID(N'dbo.VoucherLoaiSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_VLS_Voucher',N'F') IS NOT NULL ALTER TABLE dbo.VoucherLoaiSan DROP CONSTRAINT FK_VLS_Voucher;
IF OBJECT_ID(N'dbo.KhuyenMaiHangThanhVien',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_KMHang_Hang',N'F') IS NOT NULL ALTER TABLE dbo.KhuyenMaiHangThanhVien DROP CONSTRAINT FK_KMHang_Hang;
IF OBJECT_ID(N'dbo.KhuyenMaiHangThanhVien',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_KMHang_KM',N'F') IS NOT NULL ALTER TABLE dbo.KhuyenMaiHangThanhVien DROP CONSTRAINT FK_KMHang_KM;
IF OBJECT_ID(N'dbo.KhuyenMaiKhungGio',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_KMKG_KM',N'F') IS NOT NULL ALTER TABLE dbo.KhuyenMaiKhungGio DROP CONSTRAINT FK_KMKG_KM;
IF OBJECT_ID(N'dbo.TramPOS',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_POS_CumSan',N'F') IS NOT NULL ALTER TABLE dbo.TramPOS DROP CONSTRAINT FK_POS_CumSan;
IF OBJECT_ID(N'dbo.LichPhanCa',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_PhanCa_CumSan',N'F') IS NOT NULL ALTER TABLE dbo.LichPhanCa DROP CONSTRAINT FK_PhanCa_CumSan;
IF OBJECT_ID(N'dbo.LichPhanCa',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_PhanCa_NV',N'F') IS NOT NULL ALTER TABLE dbo.LichPhanCa DROP CONSTRAINT FK_PhanCa_NV;
IF OBJECT_ID(N'dbo.CaLamViec',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Ca_CumSan',N'F') IS NOT NULL ALTER TABLE dbo.CaLamViec DROP CONSTRAINT FK_Ca_CumSan;
IF OBJECT_ID(N'dbo.CaLamViec',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Ca_NV',N'F') IS NOT NULL ALTER TABLE dbo.CaLamViec DROP CONSTRAINT FK_Ca_NV;
IF OBJECT_ID(N'dbo.Kho',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_Kho_CumSan',N'F') IS NOT NULL ALTER TABLE dbo.Kho DROP CONSTRAINT FK_Kho_CumSan;
IF OBJECT_ID(N'dbo.KhuyenMaiLoaiSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_KMLS_LS',N'F') IS NOT NULL ALTER TABLE dbo.KhuyenMaiLoaiSan DROP CONSTRAINT FK_KMLS_LS;
IF OBJECT_ID(N'dbo.KhuyenMaiLoaiSan',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_KMLS_KM',N'F') IS NOT NULL ALTER TABLE dbo.KhuyenMaiLoaiSan DROP CONSTRAINT FK_KMLS_KM;
IF OBJECT_ID(N'dbo.San',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_San_CumSan',N'F') IS NOT NULL ALTER TABLE dbo.San DROP CONSTRAINT FK_San_CumSan;
IF OBJECT_ID(N'dbo.San',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_San_LoaiSan',N'F') IS NOT NULL ALTER TABLE dbo.San DROP CONSTRAINT FK_San_LoaiSan;
IF OBJECT_ID(N'dbo.VaiTroQuyen',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_VaiTroQuyen_Quyen',N'F') IS NOT NULL ALTER TABLE dbo.VaiTroQuyen DROP CONSTRAINT FK_VaiTroQuyen_Quyen;
IF OBJECT_ID(N'dbo.VaiTroQuyen',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_VaiTroQuyen_VaiTro',N'F') IS NOT NULL ALTER TABLE dbo.VaiTroQuyen DROP CONSTRAINT FK_VaiTroQuyen_VaiTro;
IF OBJECT_ID(N'dbo.KhachHang',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.FK_KhachHang_HangThanhVien',N'F') IS NOT NULL ALTER TABLE dbo.KhachHang DROP CONSTRAINT FK_KhachHang_HangThanhVien;
IF OBJECT_ID(N'dbo.DatCocOnline',N'U') IS NOT NULL DROP TABLE dbo.DatCocOnline;
IF OBJECT_ID(N'dbo.DoiSoatThanhToan',N'U') IS NOT NULL DROP TABLE dbo.DoiSoatThanhToan;
IF OBJECT_ID(N'dbo.HoanTien',N'U') IS NOT NULL DROP TABLE dbo.HoanTien;
IF OBJECT_ID(N'dbo.YeuCauVietQR',N'U') IS NOT NULL DROP TABLE dbo.YeuCauVietQR;
IF OBJECT_ID(N'dbo.ChiTietHoaDon',N'U') IS NOT NULL DROP TABLE dbo.ChiTietHoaDon;
IF OBJECT_ID(N'dbo.ChiTietThueVatDung',N'U') IS NOT NULL DROP TABLE dbo.ChiTietThueVatDung;
IF OBJECT_ID(N'dbo.SuKienSuDungSan',N'U') IS NOT NULL DROP TABLE dbo.SuKienSuDungSan;
IF OBJECT_ID(N'dbo.GiaoDichThanhToan',N'U') IS NOT NULL DROP TABLE dbo.GiaoDichThanhToan;
IF OBJECT_ID(N'dbo.LichSuGuiTinNhan',N'U') IS NOT NULL DROP TABLE dbo.LichSuGuiTinNhan;
IF OBJECT_ID(N'dbo.ChiTietPhieuNhapKho',N'U') IS NOT NULL DROP TABLE dbo.ChiTietPhieuNhapKho;
IF OBJECT_ID(N'dbo.TepDinhKemDon',N'U') IS NOT NULL DROP TABLE dbo.TepDinhKemDon;
IF OBJECT_ID(N'dbo.LichSuDoiLich',N'U') IS NOT NULL DROP TABLE dbo.LichSuDoiLich;
IF OBJECT_ID(N'dbo.LichSuVoucher',N'U') IS NOT NULL DROP TABLE dbo.LichSuVoucher;
IF OBJECT_ID(N'dbo.GiaoDichDiemTichLuy',N'U') IS NOT NULL DROP TABLE dbo.GiaoDichDiemTichLuy;
IF OBJECT_ID(N'dbo.GiaoDichVi',N'U') IS NOT NULL DROP TABLE dbo.GiaoDichVi;
IF OBJECT_ID(N'dbo.HoaDon',N'U') IS NOT NULL DROP TABLE dbo.HoaDon;
IF OBJECT_ID(N'dbo.ChiTietKiemKeKho',N'U') IS NOT NULL DROP TABLE dbo.ChiTietKiemKeKho;
IF OBJECT_ID(N'dbo.ChiTietDichVuDatSan',N'U') IS NOT NULL DROP TABLE dbo.ChiTietDichVuDatSan;
IF OBJECT_ID(N'dbo.LichSuTrangThaiDon',N'U') IS NOT NULL DROP TABLE dbo.LichSuTrangThaiDon;
IF OBJECT_ID(N'dbo.ChiTietDatSan',N'U') IS NOT NULL DROP TABLE dbo.ChiTietDatSan;
IF OBJECT_ID(N'dbo.NhatKyCauHinh',N'U') IS NOT NULL DROP TABLE dbo.NhatKyCauHinh;
IF OBJECT_ID(N'dbo.LichSuSaoLuu',N'U') IS NOT NULL DROP TABLE dbo.LichSuSaoLuu;
IF OBJECT_ID(N'dbo.NhatKyXuatBaoCao',N'U') IS NOT NULL DROP TABLE dbo.NhatKyXuatBaoCao;
IF OBJECT_ID(N'dbo.NhatKyInAn',N'U') IS NOT NULL DROP TABLE dbo.NhatKyInAn;
IF OBJECT_ID(N'dbo.HangDoiTinNhan',N'U') IS NOT NULL DROP TABLE dbo.HangDoiTinNhan;
IF OBJECT_ID(N'dbo.LenhDieuKhienIoT',N'U') IS NOT NULL DROP TABLE dbo.LenhDieuKhienIoT;
IF OBJECT_ID(N'dbo.PhieuNhapKho',N'U') IS NOT NULL DROP TABLE dbo.PhieuNhapKho;
IF OBJECT_ID(N'dbo.DotPhatHanhVoucher',N'U') IS NOT NULL DROP TABLE dbo.DotPhatHanhVoucher;
IF OBJECT_ID(N'dbo.LichSuTrangThaiSan',N'U') IS NOT NULL DROP TABLE dbo.LichSuTrangThaiSan;
IF OBJECT_ID(N'dbo.LichDongCuaCoSo',N'U') IS NOT NULL DROP TABLE dbo.LichDongCuaCoSo;
IF OBJECT_ID(N'dbo.ThanhVienDoi',N'U') IS NOT NULL DROP TABLE dbo.ThanhVienDoi;
IF OBJECT_ID(N'dbo.YeuCauDatLaiMatKhau',N'U') IS NOT NULL DROP TABLE dbo.YeuCauDatLaiMatKhau;
IF OBJECT_ID(N'dbo.ThietBiDangNhap',N'U') IS NOT NULL DROP TABLE dbo.ThietBiDangNhap;
IF OBJECT_ID(N'dbo.PhienDangNhap',N'U') IS NOT NULL DROP TABLE dbo.PhienDangNhap;
IF OBJECT_ID(N'dbo.TaiKhoanQuyen',N'U') IS NOT NULL DROP TABLE dbo.TaiKhoanQuyen;
IF OBJECT_ID(N'dbo.NhatKyHeThong',N'U') IS NOT NULL DROP TABLE dbo.NhatKyHeThong;
IF OBJECT_ID(N'dbo.ThongBao',N'U') IS NOT NULL DROP TABLE dbo.ThongBao;
IF OBJECT_ID(N'dbo.NhatKyThietBi',N'U') IS NOT NULL DROP TABLE dbo.NhatKyThietBi;
IF OBJECT_ID(N'dbo.KiemKeKho',N'U') IS NOT NULL DROP TABLE dbo.KiemKeKho;
IF OBJECT_ID(N'dbo.GiaoDichKho',N'U') IS NOT NULL DROP TABLE dbo.GiaoDichKho;
IF OBJECT_ID(N'dbo.DonDatSan',N'U') IS NOT NULL DROP TABLE dbo.DonDatSan;
IF OBJECT_ID(N'dbo.LichBaoTriSan',N'U') IS NOT NULL DROP TABLE dbo.LichBaoTriSan;
IF OBJECT_ID(N'dbo.LichSuDangNhap',N'U') IS NOT NULL DROP TABLE dbo.LichSuDangNhap;
IF OBJECT_ID(N'dbo.ThietBiPOS',N'U') IS NOT NULL DROP TABLE dbo.ThietBiPOS;
IF OBJECT_ID(N'dbo.ChiTietDoiSoatCa',N'U') IS NOT NULL DROP TABLE dbo.ChiTietDoiSoatCa;
IF OBJECT_ID(N'dbo.VoucherSan',N'U') IS NOT NULL DROP TABLE dbo.VoucherSan;
IF OBJECT_ID(N'dbo.PhuThuDatSan',N'U') IS NOT NULL DROP TABLE dbo.PhuThuDatSan;
IF OBJECT_ID(N'dbo.DoiKhachHang',N'U') IS NOT NULL DROP TABLE dbo.DoiKhachHang;
IF OBJECT_ID(N'dbo.TheThanhVien',N'U') IS NOT NULL DROP TABLE dbo.TheThanhVien;
IF OBJECT_ID(N'dbo.ThietBiIoT',N'U') IS NOT NULL DROP TABLE dbo.ThietBiIoT;
IF OBJECT_ID(N'dbo.GiuChoDatSan',N'U') IS NOT NULL DROP TABLE dbo.GiuChoDatSan;
IF OBJECT_ID(N'dbo.PhienPOS',N'U') IS NOT NULL DROP TABLE dbo.PhienPOS;
IF OBJECT_ID(N'dbo.BanGiaoCa',N'U') IS NOT NULL DROP TABLE dbo.BanGiaoCa;
IF OBJECT_ID(N'dbo.ViKhachHang',N'U') IS NOT NULL DROP TABLE dbo.ViKhachHang;
IF OBJECT_ID(N'dbo.TonKho',N'U') IS NOT NULL DROP TABLE dbo.TonKho;
IF OBJECT_ID(N'dbo.VoucherKhachHang',N'U') IS NOT NULL DROP TABLE dbo.VoucherKhachHang;
IF OBJECT_ID(N'dbo.KhuyenMaiSan',N'U') IS NOT NULL DROP TABLE dbo.KhuyenMaiSan;
IF OBJECT_ID(N'dbo.BangGia',N'U') IS NOT NULL DROP TABLE dbo.BangGia;
IF OBJECT_ID(N'dbo.SanTienIch',N'U') IS NOT NULL DROP TABLE dbo.SanTienIch;
IF OBJECT_ID(N'dbo.HinhAnhSan',N'U') IS NOT NULL DROP TABLE dbo.HinhAnhSan;
IF OBJECT_ID(N'dbo.TaiKhoan',N'U') IS NOT NULL DROP TABLE dbo.TaiKhoan;
IF OBJECT_ID(N'dbo.GiaoDichSaoKeNganHang',N'U') IS NOT NULL DROP TABLE dbo.GiaoDichSaoKeNganHang;
IF OBJECT_ID(N'dbo.VoucherLoaiSan',N'U') IS NOT NULL DROP TABLE dbo.VoucherLoaiSan;
IF OBJECT_ID(N'dbo.KhuyenMaiHangThanhVien',N'U') IS NOT NULL DROP TABLE dbo.KhuyenMaiHangThanhVien;
IF OBJECT_ID(N'dbo.KhuyenMaiKhungGio',N'U') IS NOT NULL DROP TABLE dbo.KhuyenMaiKhungGio;
IF OBJECT_ID(N'dbo.TramPOS',N'U') IS NOT NULL DROP TABLE dbo.TramPOS;
IF OBJECT_ID(N'dbo.LichPhanCa',N'U') IS NOT NULL DROP TABLE dbo.LichPhanCa;
IF OBJECT_ID(N'dbo.CaLamViec',N'U') IS NOT NULL DROP TABLE dbo.CaLamViec;
IF OBJECT_ID(N'dbo.Kho',N'U') IS NOT NULL DROP TABLE dbo.Kho;
IF OBJECT_ID(N'dbo.KhuyenMaiLoaiSan',N'U') IS NOT NULL DROP TABLE dbo.KhuyenMaiLoaiSan;
IF OBJECT_ID(N'dbo.San',N'U') IS NOT NULL DROP TABLE dbo.San;
IF OBJECT_ID(N'dbo.VaiTroQuyen',N'U') IS NOT NULL DROP TABLE dbo.VaiTroQuyen;
IF OBJECT_ID(N'dbo.KhachHang',N'U') IS NOT NULL DROP TABLE dbo.KhachHang;
IF OBJECT_ID(N'dbo.MauThongBao',N'U') IS NOT NULL DROP TABLE dbo.MauThongBao;
IF OBJECT_ID(N'dbo.MauCaLam',N'U') IS NOT NULL DROP TABLE dbo.MauCaLam;
IF OBJECT_ID(N'dbo.NhaCungCap',N'U') IS NOT NULL DROP TABLE dbo.NhaCungCap;
IF OBJECT_ID(N'dbo.NhomSanPhamDichVu',N'U') IS NOT NULL DROP TABLE dbo.NhomSanPhamDichVu;
IF OBJECT_ID(N'dbo.ChinhSachHuyHoanTien',N'U') IS NOT NULL DROP TABLE dbo.ChinhSachHuyHoanTien;
IF OBJECT_ID(N'dbo.NgayNghiLe',N'U') IS NOT NULL DROP TABLE dbo.NgayNghiLe;
IF OBJECT_ID(N'dbo.PhienBanCSDL',N'U') IS NOT NULL DROP TABLE dbo.PhienBanCSDL;
IF OBJECT_ID(N'dbo.CauHinhHeThong',N'U') IS NOT NULL DROP TABLE dbo.CauHinhHeThong;
IF OBJECT_ID(N'dbo.TaiKhoanNganHang',N'U') IS NOT NULL DROP TABLE dbo.TaiKhoanNganHang;
IF OBJECT_ID(N'dbo.SanPhamDichVu',N'U') IS NOT NULL DROP TABLE dbo.SanPhamDichVu;
IF OBJECT_ID(N'dbo.Voucher',N'U') IS NOT NULL DROP TABLE dbo.Voucher;
IF OBJECT_ID(N'dbo.KhuyenMai',N'U') IS NOT NULL DROP TABLE dbo.KhuyenMai;
IF OBJECT_ID(N'dbo.TienIch',N'U') IS NOT NULL DROP TABLE dbo.TienIch;
IF OBJECT_ID(N'dbo.LoaiSan',N'U') IS NOT NULL DROP TABLE dbo.LoaiSan;
IF OBJECT_ID(N'dbo.CumSan',N'U') IS NOT NULL DROP TABLE dbo.CumSan;
IF OBJECT_ID(N'dbo.Quyen',N'U') IS NOT NULL DROP TABLE dbo.Quyen;
IF OBJECT_ID(N'dbo.VaiTro',N'U') IS NOT NULL DROP TABLE dbo.VaiTro;
IF OBJECT_ID(N'dbo.NhanVien',N'U') IS NOT NULL DROP TABLE dbo.NhanVien;
IF OBJECT_ID(N'dbo.HangThanhVien',N'U') IS NOT NULL DROP TABLE dbo.HangThanhVien;
PRINT N'[00/10] CLEANUP OK - các object SPORTFIELD cũ đã được xóa.';
GO

PRINT N'[01/10] Bắt đầu tạo 92 bảng theo đúng dependency.';
GO

/* A001. dbo.HangThanhVien */
/* ============================================================================
   01. MEMBERSHIP / STAFF / CUSTOMER MASTER DATA
============================================================================ */

CREATE TABLE dbo.HangThanhVien
(
    HangThanhVienID     INT IDENTITY(1,1) PRIMARY KEY,
    MaHang              VARCHAR(30) NOT NULL UNIQUE,
    TenHang             NVARCHAR(100) NOT NULL,
    ChiTieuToiThieu     DECIMAL(18,2) NOT NULL CONSTRAINT DF_HangThanhVien_ChiTieu DEFAULT (0),
    TyLeTichDiem        DECIMAL(9,4) NOT NULL CONSTRAINT DF_HangThanhVien_TichDiem DEFAULT (0),
    UuDaiMacDinh        DECIMAL(9,4) NOT NULL CONSTRAINT DF_HangThanhVien_UuDai DEFAULT (0),
    MoTa                NVARCHAR(500) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_HangThanhVien_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_HangThanhVien_NgayTao DEFAULT (SYSDATETIME())
);
GO


/* A002. dbo.NhanVien */
CREATE TABLE dbo.NhanVien
(
    NhanVienID          INT IDENTITY(1,1) PRIMARY KEY,
    MaNhanVien          VARCHAR(30) NOT NULL UNIQUE,
    HoTen               NVARCHAR(150) NOT NULL,
    SoDienThoai         VARCHAR(20) NULL,
    Email               VARCHAR(150) NULL,
    ChucVu              NVARCHAR(100) NULL,
    NgayVaoLam          DATE NULL,
    DiaChi              NVARCHAR(300) NULL,
    AnhDaiDien          NVARCHAR(500) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_NhanVien_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_NhanVien_NgayTao DEFAULT (SYSDATETIME()),
    NgayCapNhat         DATETIME2(0) NULL
);
GO


/* A003. dbo.VaiTro */
/* ============================================================================
   02. AUTHORIZATION / ACCOUNTS
============================================================================ */

CREATE TABLE dbo.VaiTro
(
    VaiTroID            INT IDENTITY(1,1) PRIMARY KEY,
    MaVaiTro            VARCHAR(30) NOT NULL UNIQUE,
    TenVaiTro           NVARCHAR(100) NOT NULL,
    MoTa                NVARCHAR(500) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_VaiTro_Active DEFAULT (1)
);
GO


/* A004. dbo.Quyen */
CREATE TABLE dbo.Quyen
(
    QuyenID             INT IDENTITY(1,1) PRIMARY KEY,
    MaQuyen             VARCHAR(80) NOT NULL UNIQUE,
    TenQuyen            NVARCHAR(200) NOT NULL,
    NhomQuyen           VARCHAR(50) NULL,
    MoTa                NVARCHAR(500) NULL
);
GO


/* A005. dbo.CumSan */
/* ============================================================================
   03. FIELD MASTER DATA
============================================================================ */

CREATE TABLE dbo.CumSan
(
    CumSanID            INT IDENTITY(1,1) PRIMARY KEY,
    MaCumSan            VARCHAR(30) NOT NULL UNIQUE,
    TenCumSan           NVARCHAR(150) NOT NULL,
    DiaChi              NVARCHAR(300) NOT NULL,
    SoDienThoai         VARCHAR(20) NULL,
    MoCuaLuc            TIME(0) NOT NULL CONSTRAINT DF_CumSan_Open DEFAULT ('06:00'),
    DongCuaLuc          TIME(0) NOT NULL CONSTRAINT DF_CumSan_Close DEFAULT ('23:00'),
    MoTa                NVARCHAR(500) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_CumSan_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_CumSan_NgayTao DEFAULT (SYSDATETIME())
);
GO


/* A006. dbo.LoaiSan */
CREATE TABLE dbo.LoaiSan
(
    LoaiSanID           INT IDENTITY(1,1) PRIMARY KEY,
    MaLoaiSan           VARCHAR(30) NOT NULL UNIQUE,
    TenLoaiSan          NVARCHAR(120) NOT NULL,
    MoTa                NVARCHAR(500) NULL,
    KichThuoc           NVARCHAR(100) NULL,
    MatSan              NVARCHAR(100) NULL,
    SoNguoi             INT NULL,
    IconKey             VARCHAR(100) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_LoaiSan_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_LoaiSan_NgayTao DEFAULT (SYSDATETIME())
);
GO


/* A007. dbo.TienIch */
CREATE TABLE dbo.TienIch
(
    TienIchID           INT IDENTITY(1,1) PRIMARY KEY,
    MaTienIch           VARCHAR(30) NOT NULL UNIQUE,
    TenTienIch          NVARCHAR(120) NOT NULL,
    IconKey             VARCHAR(100) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_TienIch_Active DEFAULT (1)
);
GO


/* A008. dbo.KhuyenMai */
CREATE TABLE dbo.KhuyenMai
(
    KhuyenMaiID         INT IDENTITY(1,1) PRIMARY KEY,
    MaKhuyenMai         VARCHAR(50) NOT NULL UNIQUE,
    TenKhuyenMai        NVARCHAR(200) NOT NULL,
    LoaiGiam            VARCHAR(20) NOT NULL,
    GiaTriGiam          DECIMAL(18,2) NOT NULL,
    GiamToiDa           DECIMAL(18,2) NULL,
    DonToiThieu         DECIMAL(18,2) NOT NULL CONSTRAINT DF_KM_Min DEFAULT (0),
    NgayBatDau          DATE NOT NULL,
    NgayKetThuc         DATE NOT NULL,
    GioBatDau           TIME(0) NULL,
    GioKetThuc          TIME(0) NULL,
    MoTa                NVARCHAR(1000) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_KM_Active DEFAULT (1),
    CONSTRAINT CK_KM_Loai CHECK (LoaiGiam IN ('PHAN_TRAM','SO_TIEN')),
    CONSTRAINT CK_KM_Date CHECK (NgayKetThuc >= NgayBatDau)
);
GO


/* A009. dbo.Voucher */
CREATE TABLE dbo.Voucher
(
    VoucherID           INT IDENTITY(1,1) PRIMARY KEY,
    MaVoucher           VARCHAR(50) NOT NULL UNIQUE,
    TenVoucher          NVARCHAR(200) NOT NULL,
    LoaiGiam            VARCHAR(20) NOT NULL,
    GiaTriGiam          DECIMAL(18,2) NOT NULL,
    GiamToiDa           DECIMAL(18,2) NULL,
    DonToiThieu         DECIMAL(18,2) NOT NULL CONSTRAINT DF_Voucher_Min DEFAULT (0),
    NgayBatDau          DATE NOT NULL,
    NgayKetThuc         DATE NOT NULL,
    SoLuongPhatHanh     INT NULL,
    SoLuongDaDung       INT NOT NULL CONSTRAINT DF_Voucher_Used DEFAULT (0),
    GioiHanMoiKhach     INT NULL,
    MoTa                NVARCHAR(1000) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_Voucher_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_Voucher_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT CK_Voucher_Loai CHECK (LoaiGiam IN ('PHAN_TRAM','SO_TIEN')),
    CONSTRAINT CK_Voucher_Date CHECK (NgayKetThuc >= NgayBatDau)
);
GO


/* A010. dbo.SanPhamDichVu */
/* ============================================================================
   06. SERVICES / INVENTORY
============================================================================ */

CREATE TABLE dbo.SanPhamDichVu
(
    SanPhamDichVuID     INT IDENTITY(1,1) PRIMARY KEY,
    MaSanPhamDichVu     VARCHAR(40) NOT NULL UNIQUE,
    TenSanPhamDichVu    NVARCHAR(200) NOT NULL,
    Loai                VARCHAR(30) NOT NULL,
    DonViTinh           NVARCHAR(50) NOT NULL,
    GiaBan              DECIMAL(18,2) NULL,
    GiaThue             DECIMAL(18,2) NULL,
    QuanLyTonKho        BIT NOT NULL CONSTRAINT DF_SPDV_Stock DEFAULT (0),
    SoLuongTonToiThieu  DECIMAL(18,3) NOT NULL CONSTRAINT DF_SPDV_MinStock DEFAULT (0),
    MoTa                NVARCHAR(500) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_SPDV_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_SPDV_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT CK_SPDV_Loai CHECK (Loai IN ('DICH_VU','DO_UONG','DO_AN','CHO_THUE','BAN_HANG'))
);
GO


/* A011. dbo.TaiKhoanNganHang */
CREATE TABLE dbo.TaiKhoanNganHang
(
    TaiKhoanNganHangID  INT IDENTITY(1,1) PRIMARY KEY,
    TenNganHang         NVARCHAR(150) NOT NULL,
    BankBIN             VARCHAR(20) NOT NULL,
    SoTaiKhoan          VARCHAR(50) NOT NULL,
    ChuTaiKhoan         NVARCHAR(200) NOT NULL,
    PrefixNoiDung       VARCHAR(30) NOT NULL CONSTRAINT DF_Bank_Prefix DEFAULT ('SPORTFIELD'),
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_Bank_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_Bank_NgayTao DEFAULT (SYSDATETIME())
);
GO


/* A012. dbo.CauHinhHeThong */
CREATE TABLE dbo.CauHinhHeThong
(
    CauHinhID           INT IDENTITY(1,1) PRIMARY KEY,
    Khoa                VARCHAR(100) NOT NULL UNIQUE,
    GiaTri              NVARCHAR(MAX) NULL,
    MoTa                NVARCHAR(500) NULL,
    Nhom                VARCHAR(50) NULL,
    NgayCapNhat         DATETIME2(0) NOT NULL CONSTRAINT DF_Config_Update DEFAULT (SYSDATETIME())
);
GO


/* A013. dbo.PhienBanCSDL */
/* ============================================================================
   24. ULTIMATE EXTENSION - COVER EVERY SPORTFIELD WINFORMS FUNCTION
   These objects extend the 50-table core database without replacing it.
============================================================================ */

CREATE TABLE dbo.PhienBanCSDL
(
    PhienBanCSDLID      INT IDENTITY(1,1) PRIMARY KEY,
    SoPhienBan          VARCHAR(30) NOT NULL UNIQUE,
    MoTa                NVARCHAR(1000) NULL,
    NgayApDung          DATETIME2(0) NOT NULL CONSTRAINT DF_PhienBanCSDL_Date DEFAULT (SYSDATETIME())
);
GO


/* A014. dbo.NgayNghiLe */
/* --- Facility closures, status history, surcharges --- */
CREATE TABLE dbo.NgayNghiLe
(
    NgayNghiLeID        INT IDENTITY(1,1) PRIMARY KEY,
    Ngay                DATE NOT NULL UNIQUE,
    TenNgay             NVARCHAR(150) NOT NULL,
    HeSoGia             DECIMAL(9,4) NOT NULL CONSTRAINT DF_Holiday_Factor DEFAULT (1),
    DongCua             BIT NOT NULL CONSTRAINT DF_Holiday_Closed DEFAULT (0),
    GhiChu              NVARCHAR(500) NULL
);
GO


/* A015. dbo.ChinhSachHuyHoanTien */
CREATE TABLE dbo.ChinhSachHuyHoanTien
(
    ChinhSachID         INT IDENTITY(1,1) PRIMARY KEY,
    TenChinhSach        NVARCHAR(200) NOT NULL,
    SoPhutTruocBatDauTu INT NOT NULL,
    SoPhutTruocBatDauDen INT NULL,
    TyLeHoanCoc         DECIMAL(9,4) NOT NULL,
    PhiHuy              DECIMAL(18,2) NOT NULL CONSTRAINT DF_PhiHuy DEFAULT (0),
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_ChinhSachHuy_Active DEFAULT (1),
    CONSTRAINT CK_ChinhSachHuy_TyLe CHECK (TyLeHoanCoc BETWEEN 0 AND 100)
);
GO


/* A016. dbo.NhomSanPhamDichVu */
/* --- Inventory categories, suppliers, receipts, rental-return tracking --- */
CREATE TABLE dbo.NhomSanPhamDichVu
(
    NhomSanPhamDichVuID INT IDENTITY(1,1) PRIMARY KEY,
    MaNhom              VARCHAR(40) NOT NULL UNIQUE,
    TenNhom             NVARCHAR(150) NOT NULL,
    MoTa                NVARCHAR(500) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_NhomSPDV_Active DEFAULT (1)
);
GO


/* A017. dbo.NhaCungCap */
CREATE TABLE dbo.NhaCungCap
(
    NhaCungCapID        INT IDENTITY(1,1) PRIMARY KEY,
    MaNhaCungCap        VARCHAR(40) NOT NULL UNIQUE,
    TenNhaCungCap       NVARCHAR(200) NOT NULL,
    SoDienThoai         VARCHAR(20) NULL,
    Email               VARCHAR(150) NULL,
    DiaChi              NVARCHAR(500) NULL,
    MaSoThue            VARCHAR(50) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_NCC_Active DEFAULT (1)
);
GO


/* A018. dbo.MauCaLam */
/* --- Shift templates, reconciliation details, POS devices --- */
CREATE TABLE dbo.MauCaLam
(
    MauCaLamID          INT IDENTITY(1,1) PRIMARY KEY,
    MaCa                VARCHAR(30) NOT NULL UNIQUE,
    TenCa               NVARCHAR(100) NOT NULL,
    GioBatDau           TIME(0) NOT NULL,
    GioKetThuc          TIME(0) NOT NULL,
    MauSac              VARCHAR(20) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_MauCa_Active DEFAULT (1),
    CONSTRAINT CK_MauCa_Time CHECK (GioKetThuc<>GioBatDau)
);
GO


/* A019. dbo.MauThongBao */
/* --- Notifications, Zalo/SMS queue, print/export, backup logs --- */
CREATE TABLE dbo.MauThongBao
(
    MauThongBaoID       INT IDENTITY(1,1) PRIMARY KEY,
    MaMau               VARCHAR(50) NOT NULL UNIQUE,
    Kenh                VARCHAR(20) NOT NULL,
    TieuDeMau           NVARCHAR(250) NULL,
    NoiDungMau          NVARCHAR(MAX) NOT NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_MauTB_Active DEFAULT (1),
    CONSTRAINT CK_MauTB_Kenh CHECK (Kenh IN ('APP','SMS','ZALO','EMAIL'))
);
GO


/* A020. dbo.KhachHang */
CREATE TABLE dbo.KhachHang
(
    KhachHangID         INT IDENTITY(1,1) PRIMARY KEY,
    MaKhachHang         VARCHAR(30) NOT NULL UNIQUE,
    HoTen               NVARCHAR(150) NOT NULL,
    SoDienThoai         VARCHAR(20) NULL,
    Email               VARCHAR(150) NULL,
    DiaChi              NVARCHAR(300) NULL,
    NgaySinh            DATE NULL,
    HangThanhVienID     INT NULL,
    DiemTichLuy         DECIMAL(18,2) NOT NULL CONSTRAINT DF_KhachHang_Diem DEFAULT (0),
    TongChiTieu         DECIMAL(18,2) NOT NULL CONSTRAINT DF_KhachHang_ChiTieu DEFAULT (0),
    AnhDaiDien          NVARCHAR(500) NULL,
    DaXacMinh           BIT NOT NULL CONSTRAINT DF_KhachHang_XacMinh DEFAULT (0),
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_KhachHang_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_KhachHang_NgayTao DEFAULT (SYSDATETIME()),
    NgayCapNhat         DATETIME2(0) NULL,
    CONSTRAINT FK_KhachHang_HangThanhVien FOREIGN KEY (HangThanhVienID) REFERENCES dbo.HangThanhVien(HangThanhVienID)
);
GO


/* A021. dbo.VaiTroQuyen */
CREATE TABLE dbo.VaiTroQuyen
(
    VaiTroID            INT NOT NULL,
    QuyenID             INT NOT NULL,
    PRIMARY KEY (VaiTroID, QuyenID),
    CONSTRAINT FK_VaiTroQuyen_VaiTro FOREIGN KEY (VaiTroID) REFERENCES dbo.VaiTro(VaiTroID),
    CONSTRAINT FK_VaiTroQuyen_Quyen FOREIGN KEY (QuyenID) REFERENCES dbo.Quyen(QuyenID)
);
GO


/* A022. dbo.San */
CREATE TABLE dbo.San
(
    SanID               INT IDENTITY(1,1) PRIMARY KEY,
    MaSan               VARCHAR(30) NOT NULL UNIQUE,
    TenSan              NVARCHAR(150) NOT NULL,
    LoaiSanID           INT NOT NULL,
    CumSanID            INT NOT NULL,
    ViTri               NVARCHAR(200) NULL,
    GiaCoBan            DECIMAL(18,2) NOT NULL,
    TrangThaiVanHanh    VARCHAR(30) NOT NULL CONSTRAINT DF_San_Status DEFAULT ('SAN_SANG'),
    MoTa                NVARCHAR(1000) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_San_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_San_NgayTao DEFAULT (SYSDATETIME()),
    NgayCapNhat         DATETIME2(0) NULL,
    CONSTRAINT FK_San_LoaiSan FOREIGN KEY (LoaiSanID) REFERENCES dbo.LoaiSan(LoaiSanID),
    CONSTRAINT FK_San_CumSan FOREIGN KEY (CumSanID) REFERENCES dbo.CumSan(CumSanID),
    CONSTRAINT CK_San_TrangThai CHECK (TrangThaiVanHanh IN ('SAN_SANG','BAO_TRI','NGUNG_HOAT_DONG'))
);
GO


/* A023. dbo.KhuyenMaiLoaiSan */
CREATE TABLE dbo.KhuyenMaiLoaiSan
(
    KhuyenMaiID         INT NOT NULL,
    LoaiSanID           INT NOT NULL,
    PRIMARY KEY (KhuyenMaiID, LoaiSanID),
    CONSTRAINT FK_KMLS_KM FOREIGN KEY (KhuyenMaiID) REFERENCES dbo.KhuyenMai(KhuyenMaiID),
    CONSTRAINT FK_KMLS_LS FOREIGN KEY (LoaiSanID) REFERENCES dbo.LoaiSan(LoaiSanID)
);
GO


/* A024. dbo.Kho */
CREATE TABLE dbo.Kho
(
    KhoID               INT IDENTITY(1,1) PRIMARY KEY,
    MaKho               VARCHAR(30) NOT NULL UNIQUE,
    TenKho              NVARCHAR(150) NOT NULL,
    CumSanID            INT NULL,
    MoTa                NVARCHAR(500) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_Kho_Active DEFAULT (1),
    CONSTRAINT FK_Kho_CumSan FOREIGN KEY (CumSanID) REFERENCES dbo.CumSan(CumSanID)
);
GO


/* A025. dbo.CaLamViec */
/* ============================================================================
   08. SHIFT / POS / STAFF OPERATIONS
============================================================================ */

CREATE TABLE dbo.CaLamViec
(
    CaLamViecID         BIGINT IDENTITY(1,1) PRIMARY KEY,
    NhanVienID          INT NOT NULL,
    CumSanID            INT NULL,
    ThoiGianMoCa        DATETIME2(0) NOT NULL,
    ThoiGianDongCa      DATETIME2(0) NULL,
    TienDauCa           DECIMAL(18,2) NOT NULL CONSTRAINT DF_Ca_TienDau DEFAULT (0),
    TongThu             DECIMAL(18,2) NOT NULL CONSTRAINT DF_Ca_Tong DEFAULT (0),
    TienMat             DECIMAL(18,2) NOT NULL CONSTRAINT DF_Ca_Cash DEFAULT (0),
    VietQR              DECIMAL(18,2) NOT NULL CONSTRAINT DF_Ca_QR DEFAULT (0),
    ChuyenKhoan         DECIMAL(18,2) NOT NULL CONSTRAINT DF_Ca_CK DEFAULT (0),
    TrangThai           VARCHAR(20) NOT NULL CONSTRAINT DF_Ca_Status DEFAULT ('DANG_MO'),
    GhiChu              NVARCHAR(1000) NULL,
    CONSTRAINT FK_Ca_NV FOREIGN KEY (NhanVienID) REFERENCES dbo.NhanVien(NhanVienID),
    CONSTRAINT FK_Ca_CumSan FOREIGN KEY (CumSanID) REFERENCES dbo.CumSan(CumSanID),
    CONSTRAINT CK_Ca_Status CHECK (TrangThai IN ('DANG_MO','DA_DONG'))
);
GO


/* A026. dbo.LichPhanCa */
CREATE TABLE dbo.LichPhanCa
(
    LichPhanCaID        BIGINT IDENTITY(1,1) PRIMARY KEY,
    NhanVienID          INT NOT NULL,
    CumSanID            INT NULL,
    NgayLam             DATE NOT NULL,
    GioBatDau           TIME(0) NOT NULL,
    GioKetThuc          TIME(0) NOT NULL,
    TrangThai           VARCHAR(20) NOT NULL CONSTRAINT DF_PhanCa_Status DEFAULT ('DA_XEP'),
    GhiChu              NVARCHAR(500) NULL,
    CONSTRAINT FK_PhanCa_NV FOREIGN KEY (NhanVienID) REFERENCES dbo.NhanVien(NhanVienID),
    CONSTRAINT FK_PhanCa_CumSan FOREIGN KEY (CumSanID) REFERENCES dbo.CumSan(CumSanID),
    CONSTRAINT CK_PhanCa_Time CHECK (GioKetThuc > GioBatDau),
    CONSTRAINT CK_PhanCa_Status CHECK (TrangThai IN ('DA_XEP','DA_LAM','VANG','HUY'))
);
GO


/* A027. dbo.TramPOS */
CREATE TABLE dbo.TramPOS
(
    TramPOSID           INT IDENTITY(1,1) PRIMARY KEY,
    MaTram              VARCHAR(30) NOT NULL UNIQUE,
    TenTram             NVARCHAR(150) NOT NULL,
    CumSanID            INT NULL,
    TenMay              NVARCHAR(150) NULL,
    DiaChiIP            VARCHAR(64) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_POS_Active DEFAULT (1),
    CONSTRAINT FK_POS_CumSan FOREIGN KEY (CumSanID) REFERENCES dbo.CumSan(CumSanID)
);
GO


/* A028. dbo.KhuyenMaiKhungGio */
/* --- Promotion/voucher detailed eligibility & issuance history --- */
CREATE TABLE dbo.KhuyenMaiKhungGio
(
    KhuyenMaiID         INT NOT NULL,
    ThuTrongTuan        TINYINT NULL,
    GioBatDau           TIME(0) NOT NULL,
    GioKetThuc          TIME(0) NOT NULL,
    PRIMARY KEY (KhuyenMaiID, GioBatDau, GioKetThuc),
    CONSTRAINT FK_KMKG_KM FOREIGN KEY (KhuyenMaiID) REFERENCES dbo.KhuyenMai(KhuyenMaiID),
    CONSTRAINT CK_KMKG_Thu CHECK (ThuTrongTuan IS NULL OR ThuTrongTuan BETWEEN 1 AND 7),
    CONSTRAINT CK_KMKG_Time CHECK (GioKetThuc>GioBatDau)
);
GO


/* A029. dbo.KhuyenMaiHangThanhVien */
CREATE TABLE dbo.KhuyenMaiHangThanhVien
(
    KhuyenMaiID         INT NOT NULL,
    HangThanhVienID     INT NOT NULL,
    PRIMARY KEY (KhuyenMaiID, HangThanhVienID),
    CONSTRAINT FK_KMHang_KM FOREIGN KEY (KhuyenMaiID) REFERENCES dbo.KhuyenMai(KhuyenMaiID),
    CONSTRAINT FK_KMHang_Hang FOREIGN KEY (HangThanhVienID) REFERENCES dbo.HangThanhVien(HangThanhVienID)
);
GO


/* A030. dbo.VoucherLoaiSan */
CREATE TABLE dbo.VoucherLoaiSan
(
    VoucherID           INT NOT NULL,
    LoaiSanID           INT NOT NULL,
    PRIMARY KEY (VoucherID, LoaiSanID),
    CONSTRAINT FK_VLS_Voucher FOREIGN KEY (VoucherID) REFERENCES dbo.Voucher(VoucherID),
    CONSTRAINT FK_VLS_LoaiSan FOREIGN KEY (LoaiSanID) REFERENCES dbo.LoaiSan(LoaiSanID)
);
GO


/* A031. dbo.GiaoDichSaoKeNganHang */
/* --- Bank statement matching and settlement --- */
CREATE TABLE dbo.GiaoDichSaoKeNganHang
(
    SaoKeID             BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanNganHangID  INT NOT NULL,
    MaGiaoDichNganHang  VARCHAR(150) NULL,
    ThoiGianGiaoDich    DATETIME2(0) NOT NULL,
    SoTien              DECIMAL(18,2) NOT NULL,
    NoiDung             NVARCHAR(500) NULL,
    TaiKhoanDoiUng      NVARCHAR(150) NULL,
    TenDoiUng           NVARCHAR(200) NULL,
    TrangThaiDoiSoat    VARCHAR(30) NOT NULL CONSTRAINT DF_SaoKe_Status DEFAULT ('CHUA_KHOP'),
    DuLieuGoc           NVARCHAR(MAX) NULL,
    NgayNhap            DATETIME2(0) NOT NULL CONSTRAINT DF_SaoKe_Import DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_SaoKe_Bank FOREIGN KEY (TaiKhoanNganHangID) REFERENCES dbo.TaiKhoanNganHang(TaiKhoanNganHangID),
    CONSTRAINT CK_SaoKe_Status CHECK (TrangThaiDoiSoat IN ('CHUA_KHOP','DA_KHOP','BO_QUA','NGHI_NGO'))
);
GO


/* A032. dbo.TaiKhoan */
CREATE TABLE dbo.TaiKhoan
(
    TaiKhoanID          INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap         VARCHAR(100) NOT NULL UNIQUE,
    MatKhauHash         NVARCHAR(500) NOT NULL,
    MatKhauSalt         VARBINARY(64) NULL,
    ThuatToanMatKhau    VARCHAR(40) NOT NULL CONSTRAINT DF_TaiKhoan_Algo DEFAULT ('PBKDF2-SHA256'),
    SoVongLapMatKhau    INT NULL,
    LanDoiMatKhauCuoi   DATETIME2(0) NULL,
    VaiTroID            INT NOT NULL,
    NhanVienID          INT NULL,
    KhachHangID         INT NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_TaiKhoan_Active DEFAULT (1),
    BatBuocDoiMatKhau   BIT NOT NULL CONSTRAINT DF_TaiKhoan_ForcePwd DEFAULT (0),
    LanDangNhapCuoi     DATETIME2(0) NULL,
    SoLanSaiMatKhau     INT NOT NULL CONSTRAINT DF_TaiKhoan_Failed DEFAULT (0),
    KhoaDen             DATETIME2(0) NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_TaiKhoan_NgayTao DEFAULT (SYSDATETIME()),
    NgayCapNhat         DATETIME2(0) NULL,
    CONSTRAINT FK_TaiKhoan_VaiTro FOREIGN KEY (VaiTroID) REFERENCES dbo.VaiTro(VaiTroID),
    CONSTRAINT FK_TaiKhoan_NhanVien FOREIGN KEY (NhanVienID) REFERENCES dbo.NhanVien(NhanVienID),
    CONSTRAINT FK_TaiKhoan_KhachHang FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT CK_TaiKhoan_Profile CHECK (NOT (NhanVienID IS NOT NULL AND KhachHangID IS NOT NULL))
);
GO


/* A033. dbo.HinhAnhSan */
CREATE TABLE dbo.HinhAnhSan
(
    HinhAnhSanID        INT IDENTITY(1,1) PRIMARY KEY,
    SanID               INT NOT NULL,
    DuongDanAnh         NVARCHAR(500) NOT NULL,
    LaAnhChinh          BIT NOT NULL CONSTRAINT DF_HinhAnhSan_Main DEFAULT (0),
    ThuTu               INT NOT NULL CONSTRAINT DF_HinhAnhSan_Order DEFAULT (0),
    CONSTRAINT FK_HinhAnhSan_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID)
);
GO


/* A034. dbo.SanTienIch */
CREATE TABLE dbo.SanTienIch
(
    SanID               INT NOT NULL,
    TienIchID           INT NOT NULL,
    PRIMARY KEY (SanID, TienIchID),
    CONSTRAINT FK_SanTienIch_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID),
    CONSTRAINT FK_SanTienIch_TienIch FOREIGN KEY (TienIchID) REFERENCES dbo.TienIch(TienIchID)
);
GO


/* A035. dbo.BangGia */
/* ============================================================================
   04. PRICING / PROMOTIONS / VOUCHERS
============================================================================ */

CREATE TABLE dbo.BangGia
(
    BangGiaID           INT IDENTITY(1,1) PRIMARY KEY,
    SanID               INT NULL,
    LoaiSanID           INT NULL,
    TenBangGia          NVARCHAR(150) NOT NULL,
    LoaiNgay            VARCHAR(20) NOT NULL CONSTRAINT DF_BangGia_LoaiNgay DEFAULT ('TAT_CA'),
    ThuTrongTuan        TINYINT NULL,
    GioBatDau           TIME(0) NOT NULL,
    GioKetThuc          TIME(0) NOT NULL,
    DonGia              DECIMAL(18,2) NOT NULL,
    HieuLucTu           DATE NOT NULL,
    HieuLucDen          DATE NULL,
    ThuTuUuTien         INT NOT NULL CONSTRAINT DF_BangGia_Priority DEFAULT (100),
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_BangGia_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_BangGia_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_BangGia_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID),
    CONSTRAINT FK_BangGia_LoaiSan FOREIGN KEY (LoaiSanID) REFERENCES dbo.LoaiSan(LoaiSanID),
    CONSTRAINT CK_BangGia_Scope CHECK (SanID IS NOT NULL OR LoaiSanID IS NOT NULL),
    CONSTRAINT CK_BangGia_LoaiNgay CHECK (LoaiNgay IN ('TAT_CA','NGAY_THUONG','CUOI_TUAN','THU_CU_THE')),
    CONSTRAINT CK_BangGia_Thu CHECK (ThuTrongTuan IS NULL OR ThuTrongTuan BETWEEN 1 AND 7),
    CONSTRAINT CK_BangGia_Time CHECK (GioKetThuc > GioBatDau),
    CONSTRAINT CK_BangGia_Date CHECK (HieuLucDen IS NULL OR HieuLucDen >= HieuLucTu)
);
GO


/* A036. dbo.KhuyenMaiSan */
CREATE TABLE dbo.KhuyenMaiSan
(
    KhuyenMaiID         INT NOT NULL,
    SanID               INT NOT NULL,
    PRIMARY KEY (KhuyenMaiID, SanID),
    CONSTRAINT FK_KMS_KM FOREIGN KEY (KhuyenMaiID) REFERENCES dbo.KhuyenMai(KhuyenMaiID),
    CONSTRAINT FK_KMS_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID)
);
GO


/* A037. dbo.VoucherKhachHang */
CREATE TABLE dbo.VoucherKhachHang
(
    VoucherKhachHangID  BIGINT IDENTITY(1,1) PRIMARY KEY,
    VoucherID           INT NOT NULL,
    KhachHangID         INT NOT NULL,
    NgayNhan            DATETIME2(0) NOT NULL CONSTRAINT DF_VKH_NgayNhan DEFAULT (SYSDATETIME()),
    DaSuDung            BIT NOT NULL CONSTRAINT DF_VKH_Used DEFAULT (0),
    NgaySuDung          DATETIME2(0) NULL,
    DonDatSanID         BIGINT NULL,
    CONSTRAINT FK_VKH_Voucher FOREIGN KEY (VoucherID) REFERENCES dbo.Voucher(VoucherID),
    CONSTRAINT FK_VKH_KhachHang FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang(KhachHangID)
);
GO


/* A038. dbo.TonKho */
CREATE TABLE dbo.TonKho
(
    KhoID               INT NOT NULL,
    SanPhamDichVuID     INT NOT NULL,
    SoLuongTon          DECIMAL(18,3) NOT NULL CONSTRAINT DF_TonKho_Qty DEFAULT (0),
    NgayCapNhat         DATETIME2(0) NOT NULL CONSTRAINT DF_TonKho_Update DEFAULT (SYSDATETIME()),
    PRIMARY KEY (KhoID, SanPhamDichVuID),
    CONSTRAINT FK_TonKho_Kho FOREIGN KEY (KhoID) REFERENCES dbo.Kho(KhoID),
    CONSTRAINT FK_TonKho_SPDV FOREIGN KEY (SanPhamDichVuID) REFERENCES dbo.SanPhamDichVu(SanPhamDichVuID),
    CONSTRAINT CK_TonKho_NonNegative CHECK (SoLuongTon >= 0)
);
GO


/* A039. dbo.ViKhachHang */
CREATE TABLE dbo.ViKhachHang
(
    ViID                BIGINT IDENTITY(1,1) PRIMARY KEY,
    KhachHangID         INT NOT NULL UNIQUE,
    SoDu                DECIMAL(18,2) NOT NULL CONSTRAINT DF_Vi_SoDu DEFAULT (0),
    NgayCapNhat         DATETIME2(0) NOT NULL CONSTRAINT DF_Vi_Update DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_Vi_KH FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT CK_Vi_SoDu CHECK (SoDu >= 0)
);
GO


/* A040. dbo.BanGiaoCa */
CREATE TABLE dbo.BanGiaoCa
(
    BanGiaoID           BIGINT IDENTITY(1,1) PRIMARY KEY,
    CaLamViecID         BIGINT NOT NULL UNIQUE,
    TienMatHeThong      DECIMAL(18,2) NOT NULL,
    TienMatKiemDem      DECIMAL(18,2) NOT NULL,
    VietQR              DECIMAL(18,2) NOT NULL,
    ChuyenKhoan         DECIMAL(18,2) NOT NULL,
    ChenhLech           AS (CONVERT(DECIMAL(18,2), TienMatKiemDem - TienMatHeThong)) PERSISTED,
    GhiChu              NVARCHAR(1000) NULL,
    NguoiBanGiaoID      INT NOT NULL,
    NguoiNhanID         INT NULL,
    NgayBanGiao         DATETIME2(0) NOT NULL CONSTRAINT DF_BanGiao_Ngay DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_BanGiao_Ca FOREIGN KEY (CaLamViecID) REFERENCES dbo.CaLamViec(CaLamViecID),
    CONSTRAINT FK_BanGiao_Giao FOREIGN KEY (NguoiBanGiaoID) REFERENCES dbo.NhanVien(NhanVienID),
    CONSTRAINT FK_BanGiao_Nhan FOREIGN KEY (NguoiNhanID) REFERENCES dbo.NhanVien(NhanVienID)
);
GO


/* A041. dbo.PhienPOS */
CREATE TABLE dbo.PhienPOS
(
    PhienPOSID          BIGINT IDENTITY(1,1) PRIMARY KEY,
    TramPOSID           INT NOT NULL,
    NhanVienID          INT NOT NULL,
    CaLamViecID         BIGINT NULL,
    BatDau              DATETIME2(0) NOT NULL CONSTRAINT DF_PhienPOS_Start DEFAULT (SYSDATETIME()),
    KetThuc             DATETIME2(0) NULL,
    CONSTRAINT FK_PhienPOS_POS FOREIGN KEY (TramPOSID) REFERENCES dbo.TramPOS(TramPOSID),
    CONSTRAINT FK_PhienPOS_NV FOREIGN KEY (NhanVienID) REFERENCES dbo.NhanVien(NhanVienID),
    CONSTRAINT FK_PhienPOS_Ca FOREIGN KEY (CaLamViecID) REFERENCES dbo.CaLamViec(CaLamViecID)
);
GO


/* A042. dbo.GiuChoDatSan */
/* ============================================================================
   09. ONLINE HOLD / DEPOSIT
============================================================================ */

CREATE TABLE dbo.GiuChoDatSan
(
    GiuChoID            BIGINT IDENTITY(1,1) PRIMARY KEY,
    KhachHangID         INT NOT NULL,
    SanID               INT NOT NULL,
    BatDau              DATETIME2(0) NOT NULL,
    KetThuc             DATETIME2(0) NOT NULL,
    TokenGiuCho         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_GiuCho_Token DEFAULT (NEWID()),
    ThoiGianTao         DATETIME2(0) NOT NULL CONSTRAINT DF_GiuCho_Create DEFAULT (SYSDATETIME()),
    HetHanLuc           DATETIME2(0) NOT NULL,
    TrangThai           VARCHAR(30) NOT NULL CONSTRAINT DF_GiuCho_Status DEFAULT ('DANG_GIU'),
    CONSTRAINT FK_GiuCho_KH FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT FK_GiuCho_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID),
    CONSTRAINT UQ_GiuCho_Token UNIQUE (TokenGiuCho),
    CONSTRAINT CK_GiuCho_Time CHECK (KetThuc > BatDau),
    CONSTRAINT CK_GiuCho_Status CHECK (TrangThai IN ('DANG_GIU','DA_CHUYEN_DON','HET_HAN','HUY'))
);
GO


/* A043. dbo.ThietBiIoT */
/* ============================================================================
   10. IOT / CONFIG / NOTIFICATION / AUDIT
============================================================================ */

CREATE TABLE dbo.ThietBiIoT
(
    ThietBiID           INT IDENTITY(1,1) PRIMARY KEY,
    MaThietBi           VARCHAR(50) NOT NULL UNIQUE,
    SanID               INT NULL,
    LoaiThietBi         VARCHAR(50) NOT NULL,
    DiaChiIP            VARCHAR(64) NULL,
    TrangThai           VARCHAR(30) NOT NULL CONSTRAINT DF_IOT_Status DEFAULT ('OFFLINE'),
    LanKetNoiCuoi       DATETIME2(0) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_IOT_Active DEFAULT (1),
    CONSTRAINT FK_IOT_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID),
    CONSTRAINT CK_IOT_Status CHECK (TrangThai IN ('ONLINE','OFFLINE','LOI','BAO_TRI'))
);
GO


/* A044. dbo.TheThanhVien */
/* --- Customer membership, VIP card, loyalty ledger, teams --- */
CREATE TABLE dbo.TheThanhVien
(
    TheThanhVienID      BIGINT IDENTITY(1,1) PRIMARY KEY,
    KhachHangID         INT NOT NULL,
    MaThe               VARCHAR(50) NOT NULL UNIQUE,
    HangThanhVienID     INT NOT NULL,
    NgayCap             DATE NOT NULL CONSTRAINT DF_TheTV_NgayCap DEFAULT (CONVERT(date,GETDATE())),
    NgayHetHan          DATE NULL,
    TrangThai           VARCHAR(20) NOT NULL CONSTRAINT DF_TheTV_Status DEFAULT ('HOAT_DONG'),
    GhiChu              NVARCHAR(500) NULL,
    CONSTRAINT FK_TheTV_KH FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT FK_TheTV_Hang FOREIGN KEY (HangThanhVienID) REFERENCES dbo.HangThanhVien(HangThanhVienID),
    CONSTRAINT CK_TheTV_Status CHECK (TrangThai IN ('HOAT_DONG','HET_HAN','KHOA'))
);
GO


/* A045. dbo.DoiKhachHang */
CREATE TABLE dbo.DoiKhachHang
(
    DoiKhachHangID      INT IDENTITY(1,1) PRIMARY KEY,
    MaDoi               VARCHAR(40) NOT NULL UNIQUE,
    TenDoi              NVARCHAR(150) NOT NULL,
    TruongDoiKhachHangID INT NULL,
    SoDienThoaiLienHe   VARCHAR(20) NULL,
    GhiChu              NVARCHAR(500) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_DoiKH_Active DEFAULT (1),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_DoiKH_Create DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_DoiKH_TruongDoi FOREIGN KEY (TruongDoiKhachHangID) REFERENCES dbo.KhachHang(KhachHangID)
);
GO


/* A046. dbo.PhuThuDatSan */
CREATE TABLE dbo.PhuThuDatSan
(
    PhuThuID            INT IDENTITY(1,1) PRIMARY KEY,
    MaPhuThu            VARCHAR(40) NOT NULL UNIQUE,
    TenPhuThu           NVARCHAR(150) NOT NULL,
    SanID               INT NULL,
    LoaiSanID           INT NULL,
    LoaiTinh            VARCHAR(20) NOT NULL,
    GiaTri              DECIMAL(18,2) NOT NULL,
    NgayBatDau          DATE NULL,
    NgayKetThuc         DATE NULL,
    GioBatDau           TIME(0) NULL,
    GioKetThuc          TIME(0) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_PhuThu_Active DEFAULT (1),
    CONSTRAINT FK_PhuThu_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID),
    CONSTRAINT FK_PhuThu_LoaiSan FOREIGN KEY (LoaiSanID) REFERENCES dbo.LoaiSan(LoaiSanID),
    CONSTRAINT CK_PhuThu_Loai CHECK (LoaiTinh IN ('PHAN_TRAM','SO_TIEN'))
);
GO


/* A047. dbo.VoucherSan */
CREATE TABLE dbo.VoucherSan
(
    VoucherID           INT NOT NULL,
    SanID               INT NOT NULL,
    PRIMARY KEY (VoucherID, SanID),
    CONSTRAINT FK_VS_Voucher FOREIGN KEY (VoucherID) REFERENCES dbo.Voucher(VoucherID),
    CONSTRAINT FK_VS_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID)
);
GO


/* A048. dbo.ChiTietDoiSoatCa */
CREATE TABLE dbo.ChiTietDoiSoatCa
(
    ChiTietDoiSoatCaID  BIGINT IDENTITY(1,1) PRIMARY KEY,
    CaLamViecID         BIGINT NOT NULL,
    PhuongThuc          VARCHAR(30) NOT NULL,
    SoTienHeThong       DECIMAL(18,2) NOT NULL,
    SoTienKiemDem       DECIMAL(18,2) NULL,
    ChenhLech           AS (CONVERT(DECIMAL(18,2), ISNULL(SoTienKiemDem,SoTienHeThong)-SoTienHeThong)) PERSISTED,
    GhiChu              NVARCHAR(500) NULL,
    CONSTRAINT FK_CTDSCa_Ca FOREIGN KEY (CaLamViecID) REFERENCES dbo.CaLamViec(CaLamViecID),
    CONSTRAINT CK_CTDSCa_Method CHECK (PhuongThuc IN ('TIEN_MAT','VIETQR','CHUYEN_KHOAN','VI_COC'))
);
GO


/* A049. dbo.ThietBiPOS */
CREATE TABLE dbo.ThietBiPOS
(
    ThietBiPOSID        INT IDENTITY(1,1) PRIMARY KEY,
    TramPOSID           INT NOT NULL,
    LoaiThietBi         VARCHAR(30) NOT NULL,
    TenThietBi          NVARCHAR(150) NOT NULL,
    CongKetNoi          NVARCHAR(100) NULL,
    TrangThai           VARCHAR(20) NOT NULL CONSTRAINT DF_TBPOS_Status DEFAULT ('OFFLINE'),
    LanKiemTraCuoi      DATETIME2(0) NULL,
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_TBPOS_Active DEFAULT (1),
    CONSTRAINT FK_TBPOS_POS FOREIGN KEY (TramPOSID) REFERENCES dbo.TramPOS(TramPOSID),
    CONSTRAINT CK_TBPOS_Loai CHECK (LoaiThietBi IN ('MAY_IN','MAY_QUET_QR','KET_TIEN','MAN_HINH_PHU','KHAC')),
    CONSTRAINT CK_TBPOS_Status CHECK (TrangThai IN ('ONLINE','OFFLINE','LOI'))
);
GO


/* A050. dbo.LichSuDangNhap */
CREATE TABLE dbo.LichSuDangNhap
(
    LichSuDangNhapID    BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID          INT NULL,
    TenDangNhap         VARCHAR(100) NULL,
    ThanhCong           BIT NOT NULL,
    DiaChiIP            VARCHAR(64) NULL,
    TenMay              NVARCHAR(150) NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_LichSuDangNhap_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_LichSuDangNhap_TaiKhoan FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A051. dbo.LichBaoTriSan */
CREATE TABLE dbo.LichBaoTriSan
(
    BaoTriID            BIGINT IDENTITY(1,1) PRIMARY KEY,
    SanID               INT NOT NULL,
    TuThoiGian          DATETIME2(0) NOT NULL,
    DenThoiGian         DATETIME2(0) NOT NULL,
    NoiDung             NVARCHAR(1000) NOT NULL,
    ChiPhi              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BaoTri_ChiPhi DEFAULT (0),
    TrangThai           VARCHAR(30) NOT NULL CONSTRAINT DF_BaoTri_Status DEFAULT ('KE_HOACH'),
    NguoiTaoID          INT NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_BaoTri_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_BaoTri_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID),
    CONSTRAINT FK_BaoTri_TaiKhoan FOREIGN KEY (NguoiTaoID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_BaoTri_Time CHECK (DenThoiGian > TuThoiGian),
    CONSTRAINT CK_BaoTri_Status CHECK (TrangThai IN ('KE_HOACH','DANG_BAO_TRI','HOAN_THANH','HUY'))
);
GO


/* A052. dbo.DonDatSan */
/* ============================================================================
   05. BOOKING CORE
============================================================================ */

CREATE TABLE dbo.DonDatSan
(
    DonDatSanID         BIGINT IDENTITY(1,1) PRIMARY KEY,
    MaDon               VARCHAR(40) NOT NULL UNIQUE,
    KhachHangID         INT NOT NULL,
    NguoiTaoTaiKhoanID  INT NULL,
    NguonDon            VARCHAR(30) NOT NULL CONSTRAINT DF_Don_Nguon DEFAULT ('QUAY'),
    TrangThaiDon        VARCHAR(30) NOT NULL CONSTRAINT DF_Don_Status DEFAULT ('CHO_XAC_NHAN'),
    TrangThaiThanhToan  VARCHAR(30) NOT NULL CONSTRAINT DF_Don_Payment DEFAULT ('CHUA_THANH_TOAN'),
    TienSan             DECIMAL(18,2) NOT NULL CONSTRAINT DF_Don_TienSan DEFAULT (0),
    TienDichVu          DECIMAL(18,2) NOT NULL CONSTRAINT DF_Don_TienDV DEFAULT (0),
    TienKhuyenMai       DECIMAL(18,2) NOT NULL CONSTRAINT DF_Don_TienKM DEFAULT (0),
    TienVoucher         DECIMAL(18,2) NOT NULL CONSTRAINT DF_Don_TienVoucher DEFAULT (0),
    TongTien            DECIMAL(18,2) NOT NULL CONSTRAINT DF_Don_Tong DEFAULT (0),
    DaThanhToan         DECIMAL(18,2) NOT NULL CONSTRAINT DF_Don_DaTT DEFAULT (0),
    ConLai              AS (CONVERT(DECIMAL(18,2), CASE WHEN TongTien - DaThanhToan < 0 THEN 0 ELSE TongTien - DaThanhToan END)) PERSISTED,
    GhiChu              NVARCHAR(1000) NULL,
    LyDoHuy             NVARCHAR(1000) NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_Don_NgayTao DEFAULT (SYSDATETIME()),
    NgayCapNhat         DATETIME2(0) NULL,
    CONSTRAINT FK_Don_KhachHang FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT FK_Don_TaiKhoan FOREIGN KEY (NguoiTaoTaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_Don_Nguon CHECK (NguonDon IN ('ADMIN','QUAY','KHACH_HANG','ONLINE')),
    CONSTRAINT CK_Don_Status CHECK (TrangThaiDon IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_SU_DUNG','HOAN_THANH','DA_HUY')),
    CONSTRAINT CK_Don_Payment CHECK (TrangThaiThanhToan IN ('CHUA_THANH_TOAN','THANH_TOAN_MOT_PHAN','DA_THANH_TOAN','CHO_XAC_NHAN'))
);
GO


/* A053. dbo.GiaoDichKho */
CREATE TABLE dbo.GiaoDichKho
(
    GiaoDichKhoID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    KhoID               INT NOT NULL,
    SanPhamDichVuID     INT NOT NULL,
    LoaiGiaoDich        VARCHAR(30) NOT NULL,
    SoLuong             DECIMAL(18,3) NOT NULL,
    SoLuongTruoc        DECIMAL(18,3) NOT NULL,
    SoLuongSau          DECIMAL(18,3) NOT NULL,
    ThamChieuLoai       VARCHAR(50) NULL,
    ThamChieuID         BIGINT NULL,
    GhiChu              NVARCHAR(500) NULL,
    TaiKhoanID          INT NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_GDKho_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_GDKho_Kho FOREIGN KEY (KhoID) REFERENCES dbo.Kho(KhoID),
    CONSTRAINT FK_GDKho_SPDV FOREIGN KEY (SanPhamDichVuID) REFERENCES dbo.SanPhamDichVu(SanPhamDichVuID),
    CONSTRAINT FK_GDKho_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_GDKho_Loai CHECK (LoaiGiaoDich IN ('NHAP','BAN','CHO_THUE','TRA_THUE','HU_HONG','DIEU_CHINH','KIEM_KE'))
);
GO


/* A054. dbo.KiemKeKho */
CREATE TABLE dbo.KiemKeKho
(
    KiemKeKhoID         BIGINT IDENTITY(1,1) PRIMARY KEY,
    KhoID               INT NOT NULL,
    TrangThai           VARCHAR(30) NOT NULL CONSTRAINT DF_KiemKe_Status DEFAULT ('DANG_KIEM'),
    NguoiTaoID          INT NULL,
    NgayBatDau          DATETIME2(0) NOT NULL CONSTRAINT DF_KiemKe_Start DEFAULT (SYSDATETIME()),
    NgayHoanTat         DATETIME2(0) NULL,
    GhiChu              NVARCHAR(1000) NULL,
    CONSTRAINT FK_KiemKe_Kho FOREIGN KEY (KhoID) REFERENCES dbo.Kho(KhoID),
    CONSTRAINT FK_KiemKe_TK FOREIGN KEY (NguoiTaoID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_KiemKe_Status CHECK (TrangThai IN ('DANG_KIEM','HOAN_THANH','HUY'))
);
GO


/* A055. dbo.NhatKyThietBi */
CREATE TABLE dbo.NhatKyThietBi
(
    NhatKyThietBiID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    ThietBiID           INT NOT NULL,
    HanhDong            NVARCHAR(200) NOT NULL,
    GiaTri              NVARCHAR(500) NULL,
    TaiKhoanID          INT NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_NKTB_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_NKTB_IOT FOREIGN KEY (ThietBiID) REFERENCES dbo.ThietBiIoT(ThietBiID),
    CONSTRAINT FK_NKTB_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A056. dbo.ThongBao */
CREATE TABLE dbo.ThongBao
(
    ThongBaoID          BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID          INT NULL,
    TieuDe              NVARCHAR(250) NOT NULL,
    NoiDung             NVARCHAR(2000) NOT NULL,
    LoaiThongBao        VARCHAR(50) NULL,
    DaDoc               BIT NOT NULL CONSTRAINT DF_ThongBao_Read DEFAULT (0),
    LienKetLoai         VARCHAR(50) NULL,
    LienKetID           BIGINT NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_ThongBao_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_ThongBao_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A057. dbo.NhatKyHeThong */
CREATE TABLE dbo.NhatKyHeThong
(
    NhatKyID            BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID          INT NULL,
    Module              VARCHAR(80) NULL,
    HanhDong            VARCHAR(80) NOT NULL,
    BangDuLieu          VARCHAR(128) NULL,
    KhoaDuLieu          NVARCHAR(200) NULL,
    DuLieuCu            NVARCHAR(MAX) NULL,
    DuLieuMoi           NVARCHAR(MAX) NULL,
    TenMay              NVARCHAR(150) NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_NhatKy_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_NhatKy_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A058. dbo.TaiKhoanQuyen */
/* --- Per-account permission override, login sessions, trusted devices, password reset --- */
CREATE TABLE dbo.TaiKhoanQuyen
(
    TaiKhoanID          INT NOT NULL,
    QuyenID             INT NOT NULL,
    ChoPhep             BIT NOT NULL,
    NguoiCapID          INT NULL,
    NgayCap             DATETIME2(0) NOT NULL CONSTRAINT DF_TKQ_NgayCap DEFAULT (SYSDATETIME()),
    GhiChu              NVARCHAR(500) NULL,
    PRIMARY KEY (TaiKhoanID, QuyenID),
    CONSTRAINT FK_TKQ_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT FK_TKQ_Q FOREIGN KEY (QuyenID) REFERENCES dbo.Quyen(QuyenID),
    CONSTRAINT FK_TKQ_NguoiCap FOREIGN KEY (NguoiCapID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A059. dbo.PhienDangNhap */
CREATE TABLE dbo.PhienDangNhap
(
    PhienDangNhapID     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Phien_Token DEFAULT (NEWID()) PRIMARY KEY,
    TaiKhoanID          INT NOT NULL,
    MaThietBi           VARCHAR(100) NULL,
    TenMay              NVARCHAR(150) NULL,
    DiaChiIP            VARCHAR(64) NULL,
    BatDau              DATETIME2(0) NOT NULL CONSTRAINT DF_Phien_Start DEFAULT (SYSDATETIME()),
    HetHan              DATETIME2(0) NULL,
    KetThuc             DATETIME2(0) NULL,
    ConHieuLuc          BIT NOT NULL CONSTRAINT DF_Phien_Active DEFAULT (1),
    CONSTRAINT FK_Phien_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A060. dbo.ThietBiDangNhap */
CREATE TABLE dbo.ThietBiDangNhap
(
    ThietBiDangNhapID   BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID          INT NOT NULL,
    MaThietBi           VARCHAR(100) NOT NULL,
    TenThietBi          NVARCHAR(150) NULL,
    TokenNhoDangNhapHash NVARCHAR(500) NULL,
    LanSuDungCuoi       DATETIME2(0) NULL,
    DangTinCay          BIT NOT NULL CONSTRAINT DF_ThietBiDN_Trust DEFAULT (0),
    DaThuHoi            BIT NOT NULL CONSTRAINT DF_ThietBiDN_Revoke DEFAULT (0),
    CONSTRAINT UQ_ThietBiDN UNIQUE (TaiKhoanID, MaThietBi),
    CONSTRAINT FK_ThietBiDN_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A061. dbo.YeuCauDatLaiMatKhau */
CREATE TABLE dbo.YeuCauDatLaiMatKhau
(
    YeuCauDatLaiID      BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID          INT NOT NULL,
    TokenHash           NVARCHAR(500) NOT NULL,
    HetHan              DATETIME2(0) NOT NULL,
    DaSuDung            BIT NOT NULL CONSTRAINT DF_Reset_Used DEFAULT (0),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_Reset_Create DEFAULT (SYSDATETIME()),
    NgaySuDung          DATETIME2(0) NULL,
    CONSTRAINT FK_Reset_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A062. dbo.ThanhVienDoi */
CREATE TABLE dbo.ThanhVienDoi
(
    DoiKhachHangID      INT NOT NULL,
    KhachHangID         INT NOT NULL,
    VaiTroTrongDoi      NVARCHAR(100) NULL,
    NgayThamGia         DATE NOT NULL CONSTRAINT DF_TVDoi_Join DEFAULT (CONVERT(date,GETDATE())),
    DangHoatDong        BIT NOT NULL CONSTRAINT DF_TVDoi_Active DEFAULT (1),
    PRIMARY KEY (DoiKhachHangID, KhachHangID),
    CONSTRAINT FK_TVDoi_Doi FOREIGN KEY (DoiKhachHangID) REFERENCES dbo.DoiKhachHang(DoiKhachHangID),
    CONSTRAINT FK_TVDoi_KH FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang(KhachHangID)
);
GO


/* A063. dbo.LichDongCuaCoSo */
CREATE TABLE dbo.LichDongCuaCoSo
(
    LichDongCuaID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    CumSanID            INT NOT NULL,
    TuThoiGian          DATETIME2(0) NOT NULL,
    DenThoiGian         DATETIME2(0) NOT NULL,
    LyDo                NVARCHAR(500) NOT NULL,
    NguoiTaoID          INT NULL,
    CONSTRAINT FK_DongCua_Cum FOREIGN KEY (CumSanID) REFERENCES dbo.CumSan(CumSanID),
    CONSTRAINT FK_DongCua_TK FOREIGN KEY (NguoiTaoID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_DongCua_Time CHECK (DenThoiGian>TuThoiGian)
);
GO


/* A064. dbo.LichSuTrangThaiSan */
CREATE TABLE dbo.LichSuTrangThaiSan
(
    LichSuTrangThaiSanID BIGINT IDENTITY(1,1) PRIMARY KEY,
    SanID               INT NOT NULL,
    TrangThaiCu         VARCHAR(30) NULL,
    TrangThaiMoi        VARCHAR(30) NOT NULL,
    LyDo                NVARCHAR(500) NULL,
    TaiKhoanID          INT NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_LSStatusSan_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_LSStatusSan_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID),
    CONSTRAINT FK_LSStatusSan_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A065. dbo.DotPhatHanhVoucher */
CREATE TABLE dbo.DotPhatHanhVoucher
(
    DotPhatHanhID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    VoucherID           INT NOT NULL,
    TenDot              NVARCHAR(200) NOT NULL,
    DoiTuong            VARCHAR(30) NOT NULL CONSTRAINT DF_DotVoucher_Target DEFAULT ('TU_CHON'),
    SoLuong             INT NULL,
    NguoiPhatHanhID     INT NULL,
    NgayPhatHanh        DATETIME2(0) NOT NULL CONSTRAINT DF_DotVoucher_Date DEFAULT (SYSDATETIME()),
    GhiChu              NVARCHAR(1000) NULL,
    CONSTRAINT FK_DotVoucher_Voucher FOREIGN KEY (VoucherID) REFERENCES dbo.Voucher(VoucherID),
    CONSTRAINT FK_DotVoucher_TK FOREIGN KEY (NguoiPhatHanhID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_DotVoucher_Target CHECK (DoiTuong IN ('TAT_CA','HANG_THANH_VIEN','TU_CHON','KHACH_MOI','VIP'))
);
GO


/* A066. dbo.PhieuNhapKho */
CREATE TABLE dbo.PhieuNhapKho
(
    PhieuNhapKhoID      BIGINT IDENTITY(1,1) PRIMARY KEY,
    MaPhieuNhap         VARCHAR(50) NOT NULL UNIQUE,
    KhoID               INT NOT NULL,
    NhaCungCapID        INT NULL,
    NgayNhap            DATETIME2(0) NOT NULL CONSTRAINT DF_PNK_Date DEFAULT (SYSDATETIME()),
    TongTien            DECIMAL(18,2) NOT NULL CONSTRAINT DF_PNK_Total DEFAULT (0),
    TrangThai           VARCHAR(20) NOT NULL CONSTRAINT DF_PNK_Status DEFAULT ('NHAP'),
    NguoiNhapID         INT NULL,
    GhiChu              NVARCHAR(1000) NULL,
    CONSTRAINT FK_PNK_Kho FOREIGN KEY (KhoID) REFERENCES dbo.Kho(KhoID),
    CONSTRAINT FK_PNK_NCC FOREIGN KEY (NhaCungCapID) REFERENCES dbo.NhaCungCap(NhaCungCapID),
    CONSTRAINT FK_PNK_TK FOREIGN KEY (NguoiNhapID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_PNK_Status CHECK (TrangThai IN ('NHAP','HOAN_THANH','HUY'))
);
GO


/* A067. dbo.LenhDieuKhienIoT */
/* --- IoT command queue --- */
CREATE TABLE dbo.LenhDieuKhienIoT
(
    LenhIoTID           BIGINT IDENTITY(1,1) PRIMARY KEY,
    ThietBiID           INT NOT NULL,
    Lenh                VARCHAR(50) NOT NULL,
    ThamSo              NVARCHAR(1000) NULL,
    TrangThai           VARCHAR(20) NOT NULL CONSTRAINT DF_LenhIoT_Status DEFAULT ('CHO_GUI'),
    TaiKhoanID          INT NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_LenhIoT_Create DEFAULT (SYSDATETIME()),
    NgayGui             DATETIME2(0) NULL,
    NgayPhanHoi         DATETIME2(0) NULL,
    KetQua              NVARCHAR(1000) NULL,
    CONSTRAINT FK_LenhIoT_TB FOREIGN KEY (ThietBiID) REFERENCES dbo.ThietBiIoT(ThietBiID),
    CONSTRAINT FK_LenhIoT_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_LenhIoT_Status CHECK (TrangThai IN ('CHO_GUI','DA_GUI','THANH_CONG','THAT_BAI','HUY'))
);
GO


/* A068. dbo.HangDoiTinNhan */
CREATE TABLE dbo.HangDoiTinNhan
(
    HangDoiTinNhanID    BIGINT IDENTITY(1,1) PRIMARY KEY,
    KhachHangID         INT NULL,
    TaiKhoanID          INT NULL,
    Kenh                VARCHAR(20) NOT NULL,
    DiaChiNhan          NVARCHAR(200) NOT NULL,
    TieuDe              NVARCHAR(250) NULL,
    NoiDung             NVARCHAR(MAX) NOT NULL,
    TrangThai           VARCHAR(20) NOT NULL CONSTRAINT DF_HangDoiTN_Status DEFAULT ('CHO_GUI'),
    SoLanThu            INT NOT NULL CONSTRAINT DF_HangDoiTN_Attempt DEFAULT (0),
    GuiSauLuc           DATETIME2(0) NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_HangDoiTN_Create DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_HangDoiTN_KH FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT FK_HangDoiTN_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_HangDoiTN_Kenh CHECK (Kenh IN ('APP','SMS','ZALO','EMAIL')),
    CONSTRAINT CK_HangDoiTN_Status CHECK (TrangThai IN ('CHO_GUI','DANG_GUI','DA_GUI','THAT_BAI','HUY'))
);
GO


/* A069. dbo.NhatKyInAn */
CREATE TABLE dbo.NhatKyInAn
(
    NhatKyInAnID        BIGINT IDENTITY(1,1) PRIMARY KEY,
    LoaiTaiLieu         VARCHAR(30) NOT NULL,
    KhoaDuLieu          NVARCHAR(100) NULL,
    TenMayIn            NVARCHAR(200) NULL,
    SoBan               INT NOT NULL CONSTRAINT DF_InAn_SoBan DEFAULT (1),
    ThanhCong           BIT NOT NULL,
    TaiKhoanID          INT NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_InAn_Time DEFAULT (SYSDATETIME()),
    Loi                 NVARCHAR(1000) NULL,
    CONSTRAINT FK_InAn_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A070. dbo.NhatKyXuatBaoCao */
CREATE TABLE dbo.NhatKyXuatBaoCao
(
    NhatKyXuatID        BIGINT IDENTITY(1,1) PRIMARY KEY,
    LoaiBaoCao          VARCHAR(50) NOT NULL,
    DinhDang            VARCHAR(20) NOT NULL,
    TuNgay              DATE NULL,
    DenNgay             DATE NULL,
    DuongDanTep         NVARCHAR(1000) NULL,
    TaiKhoanID          INT NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_XuatBC_Time DEFAULT (SYSDATETIME()),
    ThanhCong           BIT NOT NULL,
    Loi                 NVARCHAR(1000) NULL,
    CONSTRAINT FK_XuatBC_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_XuatBC_Format CHECK (DinhDang IN ('PDF','EXCEL','CSV','PRINT'))
);
GO


/* A071. dbo.LichSuSaoLuu */
CREATE TABLE dbo.LichSuSaoLuu
(
    LichSuSaoLuuID      BIGINT IDENTITY(1,1) PRIMARY KEY,
    Loai                VARCHAR(20) NOT NULL,
    DuongDan            NVARCHAR(1000) NULL,
    DungLuongBytes      BIGINT NULL,
    TrangThai           VARCHAR(20) NOT NULL,
    TaiKhoanID          INT NULL,
    BatDau              DATETIME2(0) NOT NULL CONSTRAINT DF_Backup_Start DEFAULT (SYSDATETIME()),
    KetThuc             DATETIME2(0) NULL,
    Loi                 NVARCHAR(1000) NULL,
    CONSTRAINT FK_Backup_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_Backup_Loai CHECK (Loai IN ('BACKUP','RESTORE','SYNC')),
    CONSTRAINT CK_Backup_Status CHECK (TrangThai IN ('DANG_CHAY','THANH_CONG','THAT_BAI'))
);
GO


/* A072. dbo.NhatKyCauHinh */
CREATE TABLE dbo.NhatKyCauHinh
(
    NhatKyCauHinhID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    Khoa                VARCHAR(100) NOT NULL,
    GiaTriCu            NVARCHAR(MAX) NULL,
    GiaTriMoi           NVARCHAR(MAX) NULL,
    TaiKhoanID          INT NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_ConfigLog_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_ConfigLog_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A073. dbo.ChiTietDatSan */
CREATE TABLE dbo.ChiTietDatSan
(
    ChiTietDatSanID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    DonDatSanID         BIGINT NOT NULL,
    SanID               INT NOT NULL,
    BatDau              DATETIME2(0) NOT NULL,
    KetThuc             DATETIME2(0) NOT NULL,
    SoPhut              INT NOT NULL,
    DonGiaSnapshot      DECIMAL(18,2) NOT NULL,
    TienSan             DECIMAL(18,2) NOT NULL,
    TenSanSnapshot      NVARCHAR(150) NOT NULL,
    TenLoaiSanSnapshot  NVARCHAR(120) NOT NULL,
    CONSTRAINT FK_CTDatSan_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_CTDatSan_San FOREIGN KEY (SanID) REFERENCES dbo.San(SanID),
    CONSTRAINT CK_CTDatSan_Time CHECK (KetThuc > BatDau),
    CONSTRAINT CK_CTDatSan_Phut CHECK (SoPhut > 0)
);
GO


/* A074. dbo.LichSuTrangThaiDon */
CREATE TABLE dbo.LichSuTrangThaiDon
(
    LichSuID            BIGINT IDENTITY(1,1) PRIMARY KEY,
    DonDatSanID         BIGINT NOT NULL,
    TrangThaiCu         VARCHAR(30) NULL,
    TrangThaiMoi        VARCHAR(30) NOT NULL,
    TrangThaiThanhToanCu VARCHAR(30) NULL,
    TrangThaiThanhToanMoi VARCHAR(30) NULL,
    TaiKhoanThucHienID  INT NULL,
    LyDo                NVARCHAR(1000) NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_LS_Don_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_LS_Don_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_LS_Don_TK FOREIGN KEY (TaiKhoanThucHienID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A075. dbo.ChiTietDichVuDatSan */
CREATE TABLE dbo.ChiTietDichVuDatSan
(
    ChiTietDichVuDatSanID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    DonDatSanID         BIGINT NOT NULL,
    SanPhamDichVuID     INT NOT NULL,
    SoLuong             DECIMAL(18,3) NOT NULL,
    DonGiaSnapshot      DECIMAL(18,2) NOT NULL,
    ThanhTien           DECIMAL(18,2) NOT NULL,
    DaGiao              BIT NOT NULL CONSTRAINT DF_CTDV_Giao DEFAULT (0),
    DaTra               BIT NOT NULL CONSTRAINT DF_CTDV_Tra DEFAULT (0),
    SoLuongHong         DECIMAL(18,3) NOT NULL CONSTRAINT DF_CTDV_Hong DEFAULT (0),
    GhiChu              NVARCHAR(500) NULL,
    CONSTRAINT FK_CTDV_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_CTDV_SPDV FOREIGN KEY (SanPhamDichVuID) REFERENCES dbo.SanPhamDichVu(SanPhamDichVuID),
    CONSTRAINT CK_CTDV_Qty CHECK (SoLuong > 0)
);
GO


/* A076. dbo.ChiTietKiemKeKho */
CREATE TABLE dbo.ChiTietKiemKeKho
(
    ChiTietKiemKeKhoID  BIGINT IDENTITY(1,1) PRIMARY KEY,
    KiemKeKhoID         BIGINT NOT NULL,
    SanPhamDichVuID     INT NOT NULL,
    SoLuongHeThong      DECIMAL(18,3) NOT NULL,
    SoLuongThucTe       DECIMAL(18,3) NOT NULL,
    ChenhLech           AS (CONVERT(DECIMAL(18,3), SoLuongThucTe - SoLuongHeThong)) PERSISTED,
    GhiChu              NVARCHAR(500) NULL,
    CONSTRAINT FK_CTKK_KK FOREIGN KEY (KiemKeKhoID) REFERENCES dbo.KiemKeKho(KiemKeKhoID),
    CONSTRAINT FK_CTKK_SPDV FOREIGN KEY (SanPhamDichVuID) REFERENCES dbo.SanPhamDichVu(SanPhamDichVuID)
);
GO


/* A077. dbo.HoaDon */
/* ============================================================================
   07. INVOICES / PAYMENTS / QR / WALLET / REFUNDS
============================================================================ */

CREATE TABLE dbo.HoaDon
(
    HoaDonID            BIGINT IDENTITY(1,1) PRIMARY KEY,
    MaHoaDon            VARCHAR(40) NOT NULL UNIQUE,
    DonDatSanID         BIGINT NOT NULL UNIQUE,
    KhachHangID         INT NOT NULL,
    TienSan             DECIMAL(18,2) NOT NULL,
    TienDichVu          DECIMAL(18,2) NOT NULL,
    TienKhuyenMai       DECIMAL(18,2) NOT NULL CONSTRAINT DF_HD_KM DEFAULT (0),
    TienVoucher         DECIMAL(18,2) NOT NULL CONSTRAINT DF_HD_Voucher DEFAULT (0),
    TongTien            DECIMAL(18,2) NOT NULL,
    DaThanhToan         DECIMAL(18,2) NOT NULL CONSTRAINT DF_HD_DaTT DEFAULT (0),
    ConLai              AS (CONVERT(DECIMAL(18,2), CASE WHEN TongTien - DaThanhToan < 0 THEN 0 ELSE TongTien - DaThanhToan END)) PERSISTED,
    TrangThaiThanhToan  VARCHAR(30) NOT NULL CONSTRAINT DF_HD_Status DEFAULT ('CHUA_THANH_TOAN'),
    NgayLap             DATETIME2(0) NOT NULL CONSTRAINT DF_HD_NgayLap DEFAULT (SYSDATETIME()),
    NguoiLapID          INT NULL,
    GhiChu              NVARCHAR(1000) NULL,
    CONSTRAINT FK_HD_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_HD_KH FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT FK_HD_TK FOREIGN KEY (NguoiLapID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_HD_Status CHECK (TrangThaiThanhToan IN ('CHUA_THANH_TOAN','THANH_TOAN_MOT_PHAN','DA_THANH_TOAN','CHO_XAC_NHAN'))
);
GO


/* A078. dbo.GiaoDichVi */
CREATE TABLE dbo.GiaoDichVi
(
    GiaoDichViID        BIGINT IDENTITY(1,1) PRIMARY KEY,
    ViID                BIGINT NOT NULL,
    Loai                VARCHAR(30) NOT NULL,
    SoTien              DECIMAL(18,2) NOT NULL,
    SoDuTruoc           DECIMAL(18,2) NOT NULL,
    SoDuSau             DECIMAL(18,2) NOT NULL,
    DonDatSanID         BIGINT NULL,
    GhiChu              NVARCHAR(500) NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_GDVi_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_GDVi_Vi FOREIGN KEY (ViID) REFERENCES dbo.ViKhachHang(ViID),
    CONSTRAINT FK_GDVi_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT CK_GDVi_Loai CHECK (Loai IN ('NAP','TRU','HOAN','DIEU_CHINH')),
    CONSTRAINT CK_GDVi_SoTien CHECK (SoTien > 0)
);
GO


/* A079. dbo.GiaoDichDiemTichLuy */
CREATE TABLE dbo.GiaoDichDiemTichLuy
(
    GiaoDichDiemID      BIGINT IDENTITY(1,1) PRIMARY KEY,
    KhachHangID         INT NOT NULL,
    DonDatSanID         BIGINT NULL,
    LoaiGiaoDich        VARCHAR(20) NOT NULL,
    SoDiem              DECIMAL(18,2) NOT NULL,
    DiemTruoc           DECIMAL(18,2) NOT NULL,
    DiemSau             DECIMAL(18,2) NOT NULL,
    MoTa                NVARCHAR(500) NULL,
    NguoiThucHienID     INT NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_Diem_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_Diem_KH FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT FK_Diem_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_Diem_TK FOREIGN KEY (NguoiThucHienID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_Diem_Loai CHECK (LoaiGiaoDich IN ('CONG','TRU','DIEU_CHINH','HET_HAN'))
);
GO


/* A080. dbo.LichSuVoucher */
CREATE TABLE dbo.LichSuVoucher
(
    LichSuVoucherID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    VoucherKhachHangID  BIGINT NOT NULL,
    HanhDong            VARCHAR(30) NOT NULL,
    DonDatSanID         BIGINT NULL,
    TaiKhoanID          INT NULL,
    GhiChu              NVARCHAR(500) NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_LSVoucher_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_LSVoucher_VKH FOREIGN KEY (VoucherKhachHangID) REFERENCES dbo.VoucherKhachHang(VoucherKhachHangID),
    CONSTRAINT FK_LSVoucher_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_LSVoucher_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_LSVoucher_Action CHECK (HanhDong IN ('CAP','SU_DUNG','HOAN_LAI','HET_HAN','KHOA'))
);
GO


/* A081. dbo.LichSuDoiLich */
/* --- Booking reschedule, check-in/out, attachments and cancellation/refund policy --- */
CREATE TABLE dbo.LichSuDoiLich
(
    LichSuDoiLichID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    DonDatSanID         BIGINT NOT NULL,
    SanCuID             INT NOT NULL,
    SanMoiID            INT NOT NULL,
    BatDauCu            DATETIME2(0) NOT NULL,
    KetThucCu           DATETIME2(0) NOT NULL,
    BatDauMoi           DATETIME2(0) NOT NULL,
    KetThucMoi          DATETIME2(0) NOT NULL,
    ChenhLechTien       DECIMAL(18,2) NOT NULL CONSTRAINT DF_DoiLich_Chenh DEFAULT (0),
    LyDo                NVARCHAR(1000) NULL,
    TaiKhoanThucHienID  INT NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_DoiLich_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_DoiLich_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_DoiLich_SanCu FOREIGN KEY (SanCuID) REFERENCES dbo.San(SanID),
    CONSTRAINT FK_DoiLich_SanMoi FOREIGN KEY (SanMoiID) REFERENCES dbo.San(SanID),
    CONSTRAINT FK_DoiLich_TK FOREIGN KEY (TaiKhoanThucHienID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_DoiLich_Time CHECK (KetThucMoi>BatDauMoi)
);
GO


/* A082. dbo.TepDinhKemDon */
CREATE TABLE dbo.TepDinhKemDon
(
    TepDinhKemID        BIGINT IDENTITY(1,1) PRIMARY KEY,
    DonDatSanID         BIGINT NOT NULL,
    TenTep              NVARCHAR(255) NOT NULL,
    DuongDan            NVARCHAR(1000) NOT NULL,
    LoaiTep             VARCHAR(50) NULL,
    MoTa                NVARCHAR(500) NULL,
    NguoiTaiLenID       INT NULL,
    NgayTaiLen          DATETIME2(0) NOT NULL CONSTRAINT DF_TepDon_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_TepDon_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_TepDon_TK FOREIGN KEY (NguoiTaiLenID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A083. dbo.ChiTietPhieuNhapKho */
CREATE TABLE dbo.ChiTietPhieuNhapKho
(
    ChiTietPhieuNhapKhoID BIGINT IDENTITY(1,1) PRIMARY KEY,
    PhieuNhapKhoID      BIGINT NOT NULL,
    SanPhamDichVuID     INT NOT NULL,
    SoLuong             DECIMAL(18,3) NOT NULL,
    DonGiaNhap          DECIMAL(18,2) NOT NULL,
    ThanhTien           AS (CONVERT(DECIMAL(18,2), SoLuong*DonGiaNhap)) PERSISTED,
    LoSanXuat           NVARCHAR(100) NULL,
    HanSuDung           DATE NULL,
    CONSTRAINT FK_CTPNK_Phieu FOREIGN KEY (PhieuNhapKhoID) REFERENCES dbo.PhieuNhapKho(PhieuNhapKhoID),
    CONSTRAINT FK_CTPNK_SP FOREIGN KEY (SanPhamDichVuID) REFERENCES dbo.SanPhamDichVu(SanPhamDichVuID),
    CONSTRAINT CK_CTPNK_Qty CHECK (SoLuong>0),
    CONSTRAINT CK_CTPNK_Price CHECK (DonGiaNhap>=0)
);
GO


/* A084. dbo.LichSuGuiTinNhan */
CREATE TABLE dbo.LichSuGuiTinNhan
(
    LichSuGuiTinNhanID  BIGINT IDENTITY(1,1) PRIMARY KEY,
    HangDoiTinNhanID    BIGINT NOT NULL,
    LanThu              INT NOT NULL,
    ThanhCong           BIT NOT NULL,
    MaPhanHoi           NVARCHAR(200) NULL,
    NoiDungPhanHoi      NVARCHAR(1000) NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_LSGuiTN_Time DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_LSGuiTN_HangDoi FOREIGN KEY (HangDoiTinNhanID) REFERENCES dbo.HangDoiTinNhan(HangDoiTinNhanID)
);
GO


/* A085. dbo.GiaoDichThanhToan */
CREATE TABLE dbo.GiaoDichThanhToan
(
    GiaoDichID          BIGINT IDENTITY(1,1) PRIMARY KEY,
    MaGiaoDich          VARCHAR(50) NOT NULL UNIQUE,
    HoaDonID            BIGINT NOT NULL,
    DonDatSanID         BIGINT NOT NULL,
    SoTien              DECIMAL(18,2) NOT NULL,
    PhuongThuc          VARCHAR(30) NOT NULL,
    TrangThai           VARCHAR(30) NOT NULL,
    MaThamChieu         NVARCHAR(150) NULL,
    NoiDungChuyenKhoan  NVARCHAR(250) NULL,
    NguoiTaoID          INT NULL,
    NguoiXacNhanID      INT NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_GDTT_NgayTao DEFAULT (SYSDATETIME()),
    NgayXacNhan         DATETIME2(0) NULL,
    GhiChu              NVARCHAR(1000) NULL,
    CONSTRAINT FK_GDTT_HD FOREIGN KEY (HoaDonID) REFERENCES dbo.HoaDon(HoaDonID),
    CONSTRAINT FK_GDTT_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_GDTT_Tao FOREIGN KEY (NguoiTaoID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT FK_GDTT_XN FOREIGN KEY (NguoiXacNhanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_GDTT_Method CHECK (PhuongThuc IN ('TIEN_MAT','VIETQR','CHUYEN_KHOAN','VI_COC')),
    CONSTRAINT CK_GDTT_Status CHECK (TrangThai IN ('CHO_XAC_NHAN','THANH_CONG','TU_CHOI','HUY')),
    CONSTRAINT CK_GDTT_Amount CHECK (SoTien > 0)
);
GO


/* A086. dbo.SuKienSuDungSan */
CREATE TABLE dbo.SuKienSuDungSan
(
    SuKienSuDungSanID   BIGINT IDENTITY(1,1) PRIMARY KEY,
    DonDatSanID         BIGINT NOT NULL,
    ChiTietDatSanID     BIGINT NULL,
    LoaiSuKien          VARCHAR(30) NOT NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_SuKienSan_Time DEFAULT (SYSDATETIME()),
    TaiKhoanID          INT NULL,
    GhiChu              NVARCHAR(500) NULL,
    CONSTRAINT FK_SuKienSan_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_SuKienSan_CT FOREIGN KEY (ChiTietDatSanID) REFERENCES dbo.ChiTietDatSan(ChiTietDatSanID),
    CONSTRAINT FK_SuKienSan_TK FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_SuKienSan_Loai CHECK (LoaiSuKien IN ('CHECK_IN','BAT_DAU','GIA_HAN','TRA_SAN','HOAN_TAT','NO_SHOW'))
);
GO


/* A087. dbo.ChiTietThueVatDung */
CREATE TABLE dbo.ChiTietThueVatDung
(
    ChiTietThueID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    ChiTietDichVuDatSanID BIGINT NOT NULL UNIQUE,
    SoLuongGiao         DECIMAL(18,3) NOT NULL,
    SoLuongTra          DECIMAL(18,3) NOT NULL CONSTRAINT DF_Thue_Tra DEFAULT (0),
    SoLuongHong         DECIMAL(18,3) NOT NULL CONSTRAINT DF_Thue_Hong DEFAULT (0),
    SoLuongMat          DECIMAL(18,3) NOT NULL CONSTRAINT DF_Thue_Mat DEFAULT (0),
    TienPhat            DECIMAL(18,2) NOT NULL CONSTRAINT DF_Thue_Phat DEFAULT (0),
    ThoiGianGiao        DATETIME2(0) NULL,
    ThoiGianTra         DATETIME2(0) NULL,
    NguoiGiaoID         INT NULL,
    NguoiNhanLaiID      INT NULL,
    GhiChu              NVARCHAR(500) NULL,
    CONSTRAINT FK_Thue_CT FOREIGN KEY (ChiTietDichVuDatSanID) REFERENCES dbo.ChiTietDichVuDatSan(ChiTietDichVuDatSanID),
    CONSTRAINT FK_Thue_Giao FOREIGN KEY (NguoiGiaoID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT FK_Thue_Nhan FOREIGN KEY (NguoiNhanLaiID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO


/* A088. dbo.ChiTietHoaDon */
CREATE TABLE dbo.ChiTietHoaDon
(
    ChiTietHoaDonID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    HoaDonID            BIGINT NOT NULL,
    LoaiDong            VARCHAR(30) NOT NULL,
    MaThamChieu         VARCHAR(50) NULL,
    MoTa                NVARCHAR(500) NOT NULL,
    SoLuong             DECIMAL(18,3) NOT NULL CONSTRAINT DF_CTHD_Qty DEFAULT (1),
    DonGia              DECIMAL(18,2) NOT NULL,
    ThanhTien           DECIMAL(18,2) NOT NULL,
    ThuTu               INT NOT NULL CONSTRAINT DF_CTHD_Order DEFAULT (0),
    CONSTRAINT FK_CTHD_HD FOREIGN KEY (HoaDonID) REFERENCES dbo.HoaDon(HoaDonID),
    CONSTRAINT CK_CTHD_Loai CHECK (LoaiDong IN ('TIEN_SAN','DICH_VU','VOUCHER','KHUYEN_MAI','PHU_THU','PHI_HUY','TIEN_PHAT','DIEU_CHINH'))
);
GO


/* A089. dbo.YeuCauVietQR */
CREATE TABLE dbo.YeuCauVietQR
(
    VietQRID            BIGINT IDENTITY(1,1) PRIMARY KEY,
    GiaoDichID          BIGINT NOT NULL UNIQUE,
    TaiKhoanNganHangID  INT NOT NULL,
    BankBIN             VARCHAR(20) NOT NULL,
    SoTaiKhoan          VARCHAR(50) NOT NULL,
    ChuTaiKhoan         NVARCHAR(200) NOT NULL,
    SoTien              DECIMAL(18,2) NOT NULL,
    NoiDung             NVARCHAR(250) NOT NULL,
    QRPayload           NVARCHAR(MAX) NULL,
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_QR_NgayTao DEFAULT (SYSDATETIME()),
    HetHanLuc           DATETIME2(0) NULL,
    CONSTRAINT FK_QR_GDTT FOREIGN KEY (GiaoDichID) REFERENCES dbo.GiaoDichThanhToan(GiaoDichID),
    CONSTRAINT FK_QR_Bank FOREIGN KEY (TaiKhoanNganHangID) REFERENCES dbo.TaiKhoanNganHang(TaiKhoanNganHangID)
);
GO


/* A090. dbo.HoanTien */
CREATE TABLE dbo.HoanTien
(
    HoanTienID          BIGINT IDENTITY(1,1) PRIMARY KEY,
    GiaoDichThanhToanID BIGINT NOT NULL,
    DonDatSanID         BIGINT NOT NULL,
    SoTien              DECIMAL(18,2) NOT NULL,
    LyDo                NVARCHAR(1000) NOT NULL,
    TrangThai           VARCHAR(30) NOT NULL CONSTRAINT DF_HoanTien_Status DEFAULT ('CHO_DUYET'),
    NguoiDuyetID        INT NULL,
    NgayYeuCau          DATETIME2(0) NOT NULL CONSTRAINT DF_HoanTien_YC DEFAULT (SYSDATETIME()),
    NgayHoanTat         DATETIME2(0) NULL,
    CONSTRAINT FK_HoanTien_GD FOREIGN KEY (GiaoDichThanhToanID) REFERENCES dbo.GiaoDichThanhToan(GiaoDichID),
    CONSTRAINT FK_HoanTien_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID),
    CONSTRAINT FK_HoanTien_TK FOREIGN KEY (NguoiDuyetID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_HoanTien_Status CHECK (TrangThai IN ('CHO_DUYET','DA_DUYET','TU_CHOI','HOAN_TAT'))
);
GO


/* A091. dbo.DoiSoatThanhToan */
CREATE TABLE dbo.DoiSoatThanhToan
(
    DoiSoatID           BIGINT IDENTITY(1,1) PRIMARY KEY,
    GiaoDichID          BIGINT NOT NULL,
    SaoKeID             BIGINT NULL,
    KieuDoiSoat         VARCHAR(20) NOT NULL,
    TrangThai           VARCHAR(20) NOT NULL,
    DoTinCay            DECIMAL(9,4) NULL,
    NguoiXacNhanID      INT NULL,
    ThoiGian            DATETIME2(0) NOT NULL CONSTRAINT DF_DoiSoat_Time DEFAULT (SYSDATETIME()),
    GhiChu              NVARCHAR(500) NULL,
    CONSTRAINT FK_DoiSoat_GD FOREIGN KEY (GiaoDichID) REFERENCES dbo.GiaoDichThanhToan(GiaoDichID),
    CONSTRAINT FK_DoiSoat_SaoKe FOREIGN KEY (SaoKeID) REFERENCES dbo.GiaoDichSaoKeNganHang(SaoKeID),
    CONSTRAINT FK_DoiSoat_TK FOREIGN KEY (NguoiXacNhanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_DoiSoat_Kieu CHECK (KieuDoiSoat IN ('TU_DONG','THU_CONG')),
    CONSTRAINT CK_DoiSoat_Status CHECK (TrangThai IN ('CHO_XAC_NHAN','DA_KHOP','TU_CHOI'))
);
GO


/* A092. dbo.DatCocOnline */
CREATE TABLE dbo.DatCocOnline
(
    DatCocID            BIGINT IDENTITY(1,1) PRIMARY KEY,
    GiuChoID            BIGINT NOT NULL UNIQUE,
    SoTienCoc           DECIMAL(18,2) NOT NULL,
    VietQRID            BIGINT NULL,
    TrangThai           VARCHAR(30) NOT NULL CONSTRAINT DF_DatCoc_Status DEFAULT ('CHO_THANH_TOAN'),
    NgayTao             DATETIME2(0) NOT NULL CONSTRAINT DF_DatCoc_NgayTao DEFAULT (SYSDATETIME()),
    NgayXacNhan         DATETIME2(0) NULL,
    CONSTRAINT FK_DatCoc_GiuCho FOREIGN KEY (GiuChoID) REFERENCES dbo.GiuChoDatSan(GiuChoID),
    CONSTRAINT FK_DatCoc_QR FOREIGN KEY (VietQRID) REFERENCES dbo.YeuCauVietQR(VietQRID),
    CONSTRAINT CK_DatCoc_Status CHECK (TrangThai IN ('CHO_THANH_TOAN','CHO_XAC_NHAN','DA_XAC_NHAN','HUY','HET_HAN'))
);
GO


/* ==================== B. RELATIONAL PATCHES / ALTER TABLE ==================== */

ALTER TABLE dbo.VoucherKhachHang
ADD CONSTRAINT FK_VKH_Don FOREIGN KEY (DonDatSanID) REFERENCES dbo.DonDatSan(DonDatSanID);
GO

/* ============================================================================
   29. FINAL RELATIONAL PATCHES FOR SNAPSHOTS / GROUPING / SHIFT TEMPLATES
============================================================================ */

ALTER TABLE dbo.SanPhamDichVu ADD NhomSanPhamDichVuID INT NULL;
ALTER TABLE dbo.SanPhamDichVu ADD CONSTRAINT FK_SPDV_Nhom FOREIGN KEY (NhomSanPhamDichVuID) REFERENCES dbo.NhomSanPhamDichVu(NhomSanPhamDichVuID);
GO

ALTER TABLE dbo.LichPhanCa ADD MauCaLamID INT NULL;
ALTER TABLE dbo.LichPhanCa ADD CONSTRAINT FK_LichPhanCa_MauCa FOREIGN KEY (MauCaLamID) REFERENCES dbo.MauCaLam(MauCaLamID);
GO

ALTER TABLE dbo.DonDatSan ADD DoiKhachHangID INT NULL;
ALTER TABLE dbo.DonDatSan ADD CONSTRAINT FK_Don_DoiKhachHang FOREIGN KEY (DoiKhachHangID) REFERENCES dbo.DoiKhachHang(DoiKhachHangID);
GO


/* ==================== C. INDEXES ==================== */

CREATE UNIQUE INDEX UX_NhanVien_SDT
ON dbo.NhanVien(SoDienThoai)
WHERE SoDienThoai IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_NhanVien_Email
ON dbo.NhanVien(Email)
WHERE Email IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_KhachHang_SDT
ON dbo.KhachHang(SoDienThoai)
WHERE SoDienThoai IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_KhachHang_Email
ON dbo.KhachHang(Email)
WHERE Email IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_TaiKhoan_NhanVien
ON dbo.TaiKhoan(NhanVienID)
WHERE NhanVienID IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_TaiKhoan_KhachHang
ON dbo.TaiKhoan(KhachHangID)
WHERE KhachHangID IS NOT NULL;
GO

CREATE INDEX IX_San_LoaiSan ON dbo.San(LoaiSanID);
CREATE INDEX IX_San_CumSan ON dbo.San(CumSanID);
CREATE INDEX IX_San_TrangThai ON dbo.San(TrangThaiVanHanh, DangHoatDong);
GO

CREATE INDEX IX_BaoTri_SanTime ON dbo.LichBaoTriSan(SanID, TuThoiGian, DenThoiGian);
GO

CREATE INDEX IX_BangGia_San ON dbo.BangGia(SanID, HieuLucTu, HieuLucDen, GioBatDau, GioKetThuc, DangHoatDong);
CREATE INDEX IX_BangGia_LoaiSan ON dbo.BangGia(LoaiSanID, HieuLucTu, HieuLucDen, GioBatDau, GioKetThuc, DangHoatDong);
GO

CREATE INDEX IX_VKH_KhachHang ON dbo.VoucherKhachHang(KhachHangID, DaSuDung);
GO

CREATE INDEX IX_Don_KhachHang ON dbo.DonDatSan(KhachHangID, NgayTao DESC);
CREATE INDEX IX_Don_TrangThai ON dbo.DonDatSan(TrangThaiDon, TrangThaiThanhToan);
CREATE INDEX IX_Don_NgayTao ON dbo.DonDatSan(NgayTao DESC);
GO

CREATE INDEX IX_CTDatSan_SanTime ON dbo.ChiTietDatSan(SanID, BatDau, KetThuc, DonDatSanID);
CREATE INDEX IX_CTDatSan_Don ON dbo.ChiTietDatSan(DonDatSanID);
GO

CREATE INDEX IX_LS_Don ON dbo.LichSuTrangThaiDon(DonDatSanID, ThoiGian DESC);
GO

CREATE INDEX IX_CTDV_Don ON dbo.ChiTietDichVuDatSan(DonDatSanID);
GO

CREATE INDEX IX_GDKho_Stock ON dbo.GiaoDichKho(KhoID, SanPhamDichVuID, NgayTao DESC);
GO

CREATE INDEX IX_HD_Status ON dbo.HoaDon(TrangThaiThanhToan, NgayLap DESC);
GO

CREATE INDEX IX_GDTT_Status ON dbo.GiaoDichThanhToan(TrangThai, PhuongThuc, NgayTao DESC);
CREATE INDEX IX_GDTT_HD ON dbo.GiaoDichThanhToan(HoaDonID, NgayTao DESC);
GO

CREATE INDEX IX_Ca_NV ON dbo.CaLamViec(NhanVienID, ThoiGianMoCa DESC);
GO

CREATE INDEX IX_GiuCho_SanTime ON dbo.GiuChoDatSan(SanID, BatDau, KetThuc, HetHanLuc, TrangThai);
GO

CREATE INDEX IX_ThongBao_TK ON dbo.ThongBao(TaiKhoanID, DaDoc, NgayTao DESC);
GO

CREATE INDEX IX_NhatKy_Time ON dbo.NhatKyHeThong(ThoiGian DESC);
CREATE INDEX IX_NhatKy_TK ON dbo.NhatKyHeThong(TaiKhoanID, ThoiGian DESC);
GO

/* ============================================================================
   21. EXTRA INDEXES
============================================================================ */

CREATE INDEX IX_TaiKhoan_VaiTro ON dbo.TaiKhoan(VaiTroID,DangHoatDong);
CREATE INDEX IX_Voucher_Validity ON dbo.Voucher(DangHoatDong,NgayBatDau,NgayKetThuc);
CREATE INDEX IX_KhuyenMai_Validity ON dbo.KhuyenMai(DangHoatDong,NgayBatDau,NgayKetThuc);
CREATE INDEX IX_HoaDon_KhachHang ON dbo.HoaDon(KhachHangID,NgayLap DESC);
CREATE INDEX IX_GDTT_Don ON dbo.GiaoDichThanhToan(DonDatSanID,NgayTao DESC);
CREATE INDEX IX_Ca_Status ON dbo.CaLamViec(TrangThai,ThoiGianMoCa DESC);
GO

CREATE INDEX IX_PhienDangNhap_TK ON dbo.PhienDangNhap(TaiKhoanID, ConHieuLuc, BatDau DESC);
GO

CREATE INDEX IX_Diem_KH ON dbo.GiaoDichDiemTichLuy(KhachHangID, NgayTao DESC);
GO

CREATE INDEX IX_SaoKe_Match ON dbo.GiaoDichSaoKeNganHang(TrangThaiDoiSoat, ThoiGianGiaoDich DESC, SoTien);
GO

CREATE INDEX IX_HangDoiTN_Status ON dbo.HangDoiTinNhan(TrangThai, GuiSauLuc, NgayTao);
GO

CREATE INDEX IX_CTHD_HD ON dbo.ChiTietHoaDon(HoaDonID, ThuTu);
GO


/* ==================== D. SYSTEM SEED DATA ==================== */

/* ============================================================================
   11. BASE SEED DATA - SYSTEM CATALOG ONLY, NO DEMO BOOKINGS
============================================================================ */

INSERT INTO dbo.HangThanhVien(MaHang, TenHang, ChiTieuToiThieu, TyLeTichDiem, UuDaiMacDinh, MoTa)
VALUES
('THUONG', N'Thành viên', 0, 1, 0, N'Hạng mặc định'),
('SILVER', N'Silver', 5000000, 1.2, 0, N'Khách hàng thân thiết'),
('GOLD', N'Gold', 15000000, 1.5, 0, N'Khách hàng ưu tiên'),
('VIP', N'VIP', 30000000, 2, 0, N'Khách hàng VIP');
GO

INSERT INTO dbo.VaiTro(MaVaiTro, TenVaiTro, MoTa)
VALUES
('ADMIN', N'Quản trị viên', N'Toàn quyền hệ thống'),
('NHAN_VIEN', N'Nhân viên', N'Vận hành tại quầy và thu tiền'),
('KHACH_HANG', N'Khách hàng', N'Đặt sân và quản lý dữ liệu cá nhân');
GO

INSERT INTO dbo.Quyen(MaQuyen, TenQuyen, NhomQuyen)
VALUES
('DASHBOARD_VIEW', N'Xem tổng quan', 'DASHBOARD'),
('FIELD_VIEW', N'Xem sân', 'FIELD'),
('FIELD_CREATE', N'Thêm sân', 'FIELD'),
('FIELD_UPDATE', N'Sửa sân', 'FIELD'),
('FIELD_DELETE', N'Ngừng/xóa sân', 'FIELD'),
('FIELD_TYPE_VIEW', N'Xem loại sân', 'FIELD_TYPE'),
('FIELD_TYPE_MANAGE', N'Quản lý loại sân', 'FIELD_TYPE'),
('PRICE_VIEW', N'Xem bảng giá', 'PRICE'),
('PRICE_MANAGE', N'Quản lý bảng giá', 'PRICE'),
('BOOKING_VIEW', N'Xem đơn đặt sân', 'BOOKING'),
('BOOKING_CREATE', N'Tạo đơn đặt sân', 'BOOKING'),
('BOOKING_CONFIRM', N'Xác nhận đơn', 'BOOKING'),
('BOOKING_CHECKIN', N'Check-in/Bắt đầu', 'BOOKING'),
('BOOKING_COMPLETE', N'Hoàn tất/Trả sân', 'BOOKING'),
('BOOKING_CANCEL', N'Hủy đơn', 'BOOKING'),
('CUSTOMER_VIEW', N'Xem khách hàng', 'CUSTOMER'),
('CUSTOMER_MANAGE', N'Quản lý khách hàng', 'CUSTOMER'),
('EMPLOYEE_VIEW', N'Xem nhân viên', 'EMPLOYEE'),
('EMPLOYEE_MANAGE', N'Quản lý nhân viên', 'EMPLOYEE'),
('ACCOUNT_MANAGE', N'Quản lý tài khoản', 'ACCOUNT'),
('INVOICE_VIEW', N'Xem hóa đơn', 'INVOICE'),
('PAYMENT_CREATE', N'Ghi nhận thanh toán', 'PAYMENT'),
('PAYMENT_CONFIRM_TRANSFER', N'Xác nhận chuyển khoản', 'PAYMENT'),
('VOUCHER_VIEW', N'Xem voucher', 'VOUCHER'),
('VOUCHER_MANAGE', N'Quản lý voucher', 'VOUCHER'),
('PROMOTION_MANAGE', N'Quản lý khuyến mãi', 'PROMOTION'),
('SERVICE_VIEW', N'Xem dịch vụ', 'SERVICE'),
('SERVICE_MANAGE', N'Quản lý dịch vụ', 'SERVICE'),
('INVENTORY_VIEW', N'Xem kho', 'INVENTORY'),
('INVENTORY_MANAGE', N'Quản lý kho', 'INVENTORY'),
('REPORT_VIEW', N'Xem thống kê', 'REPORT'),
('SHIFT_MANAGE_SELF', N'Mở/đóng ca cá nhân', 'SHIFT'),
('SHIFT_MANAGE_ALL', N'Quản lý ca nhân viên', 'SHIFT'),
('SETTING_MANAGE', N'Quản lý cấu hình', 'SETTING'),
('AUDIT_VIEW', N'Xem nhật ký', 'AUDIT'),
('IOT_MANAGE', N'Quản lý IoT', 'IOT'),
('POS_MANAGE', N'Quản lý POS', 'POS');
GO

-- ADMIN = all permissions
INSERT INTO dbo.VaiTroQuyen(VaiTroID, QuyenID)
SELECT vt.VaiTroID, q.QuyenID
FROM dbo.VaiTro vt CROSS JOIN dbo.Quyen q
WHERE vt.MaVaiTro = 'ADMIN';
GO

-- STAFF permissions
INSERT INTO dbo.VaiTroQuyen(VaiTroID, QuyenID)
SELECT vt.VaiTroID, q.QuyenID
FROM dbo.VaiTro vt
JOIN dbo.Quyen q ON q.MaQuyen IN
(
'DASHBOARD_VIEW','FIELD_VIEW','FIELD_TYPE_VIEW','PRICE_VIEW',
'BOOKING_VIEW','BOOKING_CREATE','BOOKING_CONFIRM','BOOKING_CHECKIN','BOOKING_COMPLETE','BOOKING_CANCEL',
'CUSTOMER_VIEW','CUSTOMER_MANAGE','INVOICE_VIEW','PAYMENT_CREATE','PAYMENT_CONFIRM_TRANSFER',
'VOUCHER_VIEW','SERVICE_VIEW','INVENTORY_VIEW','REPORT_VIEW','SHIFT_MANAGE_SELF'
)
WHERE vt.MaVaiTro = 'NHAN_VIEN';
GO

-- CUSTOMER permissions
INSERT INTO dbo.VaiTroQuyen(VaiTroID, QuyenID)
SELECT vt.VaiTroID, q.QuyenID
FROM dbo.VaiTro vt
JOIN dbo.Quyen q ON q.MaQuyen IN
('FIELD_VIEW','FIELD_TYPE_VIEW','PRICE_VIEW','BOOKING_VIEW','BOOKING_CREATE','INVOICE_VIEW','VOUCHER_VIEW')
WHERE vt.MaVaiTro = 'KHACH_HANG';
GO

INSERT INTO dbo.LoaiSan(MaLoaiSan, TenLoaiSan, MoTa, DangHoatDong)
VALUES
('BONG_DA', N'Bóng đá', N'Sân bóng đá', 1),
('CAU_LONG', N'Cầu lông', N'Sân cầu lông', 1),
('BONG_CHUYEN', N'Bóng chuyền', N'Sân bóng chuyền', 1);
GO

INSERT INTO dbo.TienIch(MaTienIch, TenTienIch)
VALUES
('DEN_LED', N'Đèn LED'),
('WIFI', N'Wifi'),
('PHONG_THAY_DO', N'Phòng thay đồ'),
('BAI_XE', N'Bãi để xe'),
('NUOC_UONG', N'Nước uống'),
('NHA_VE_SINH', N'Nhà vệ sinh');
GO

INSERT INTO dbo.CauHinhHeThong(Khoa, GiaTri, MoTa, Nhom)
VALUES
('TEN_HE_THONG', N'SPORTFIELD', N'Tên hệ thống', 'GENERAL'),
('GIO_MO_CUA', N'06:00', N'Giờ mở cửa mặc định', 'GENERAL'),
('GIO_DONG_CUA', N'23:00', N'Giờ đóng cửa mặc định', 'GENERAL'),
('THOI_GIAN_GIU_CHO_PHUT', N'10', N'Số phút giữ chỗ online', 'BOOKING'),
('PHUT_TOI_THIEU_DAT_SAN', N'30', N'Thời lượng đặt tối thiểu', 'BOOKING'),
('CHO_PHEP_HUY_TRUOC_PHUT', N'120', N'Khoảng thời gian tối thiểu được phép hủy', 'BOOKING');
GO

INSERT INTO dbo.PhienBanCSDL(SoPhienBan, MoTa)
VALUES ('ULTIMATE-2026.10', N'Bản CSDL đầy đủ cho toàn bộ SportField WinForms Admin/Nhân viên/Khách hàng.');
GO

/* ==========================================================================
   25. EXTENDED PERMISSIONS TO COVER ALL BUTTONS / SMALL FEATURES
============================================================================ */
INSERT INTO dbo.Quyen(MaQuyen, TenQuyen, NhomQuyen)
SELECT v.MaQuyen, v.TenQuyen, v.NhomQuyen
FROM (VALUES
('BOOKING_RESCHEDULE',N'Đổi lịch đặt sân','BOOKING'),
('BOOKING_REFUND',N'Xử lý hoàn cọc/hoàn tiền','BOOKING'),
('BOOKING_ONLINE_APPROVE',N'Duyệt đơn online','ONLINE'),
('BOOKING_ONLINE_RECONCILE',N'Khớp cọc VietQR đơn online','ONLINE'),
('CUSTOMER_VIP_ASSIGN',N'Gán hạng/thẻ VIP','CUSTOMER'),
('CUSTOMER_MESSAGE',N'Gửi Zalo/SMS/Email khách hàng','CUSTOMER'),
('CUSTOMER_ACCOUNT_CREATE',N'Cấp tài khoản App cho khách','CUSTOMER'),
('CUSTOMER_WALLET_MANAGE',N'Quản lý ví cọc khách hàng','CUSTOMER'),
('LOYALTY_MANAGE',N'Quản lý điểm tích lũy','CUSTOMER'),
('TEAM_MANAGE',N'Quản lý đội/nhóm khách hàng','CUSTOMER'),
('VOUCHER_ISSUE',N'Cấp/phát hành voucher','VOUCHER'),
('BANK_RECONCILE',N'Đối soát sao kê ngân hàng','PAYMENT'),
('REFUND_MANAGE',N'Duyệt hoàn tiền','PAYMENT'),
('INVENTORY_IMPORT',N'Nhập kho','INVENTORY'),
('INVENTORY_EXPORT',N'Xuất kho','INVENTORY'),
('INVENTORY_AUDIT',N'Kiểm kê kho','INVENTORY'),
('RENTAL_RETURN',N'Nhận trả vật dụng thuê','SERVICE'),
('SHIFT_HANDOVER',N'Bàn giao ca','SHIFT'),
('REPORT_EXPORT',N'Xuất PDF/Excel/CSV','REPORT'),
('PRINT_DOCUMENT',N'In hóa đơn/phiếu/bảng giá','PRINT'),
('MESSAGE_SEND',N'Gửi thông báo/SMS/Zalo/Email','MESSAGE'),
('BACKUP_MANAGE',N'Sao lưu/khôi phục dữ liệu','SETTING'),
('FIELD_MAINTENANCE',N'Quản lý bảo trì sân','FIELD'),
('HOLIDAY_MANAGE',N'Quản lý ngày nghỉ/đóng cửa','SETTING'),
('IOT_CONTROL',N'Điều khiển thiết bị IoT','IOT')
) v(MaQuyen,TenQuyen,NhomQuyen)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Quyen q WHERE q.MaQuyen=v.MaQuyen);
GO

-- Ensure ADMIN receives every newly inserted permission.
INSERT INTO dbo.VaiTroQuyen(VaiTroID, QuyenID)
SELECT vt.VaiTroID, q.QuyenID
FROM dbo.VaiTro vt CROSS JOIN dbo.Quyen q
WHERE vt.MaVaiTro='ADMIN'
  AND NOT EXISTS (SELECT 1 FROM dbo.VaiTroQuyen x WHERE x.VaiTroID=vt.VaiTroID AND x.QuyenID=q.QuyenID);
GO

-- Extra staff permissions matching Staff UI buttons.
INSERT INTO dbo.VaiTroQuyen(VaiTroID, QuyenID)
SELECT vt.VaiTroID, q.QuyenID
FROM dbo.VaiTro vt JOIN dbo.Quyen q ON q.MaQuyen IN
('BOOKING_RESCHEDULE','BOOKING_REFUND','CUSTOMER_ACCOUNT_CREATE','CUSTOMER_MESSAGE',
 'BANK_RECONCILE','INVENTORY_EXPORT','INVENTORY_AUDIT','RENTAL_RETURN','SHIFT_HANDOVER','PRINT_DOCUMENT')
WHERE vt.MaVaiTro='NHAN_VIEN'
  AND NOT EXISTS (SELECT 1 FROM dbo.VaiTroQuyen x WHERE x.VaiTroID=vt.VaiTroID AND x.QuyenID=q.QuyenID);
GO

/* ==========================================================================
   26. DEFAULT CATALOG DATA FOR SMALL UI FEATURES
============================================================================ */
INSERT INTO dbo.MauCaLam(MaCa,TenCa,GioBatDau,GioKetThuc,MauSac)
VALUES
('CA_SANG',N'Ca sáng','06:00','14:00','#0EA5E9'),
('CA_CHIEU',N'Ca chiều','14:00','22:00','#F59E0B'),
('CA_TOI',N'Ca tối','18:00','23:00','#7C3AED');
GO

INSERT INTO dbo.ChinhSachHuyHoanTien(TenChinhSach,SoPhutTruocBatDauTu,SoPhutTruocBatDauDen,TyLeHoanCoc,PhiHuy)
VALUES
(N'Hủy trước 24 giờ',1440,NULL,100,0),
(N'Hủy trước 6 đến dưới 24 giờ',360,1439,75,0),
(N'Hủy trước 2 đến dưới 6 giờ',120,359,50,0),
(N'Hủy dưới 2 giờ',0,119,0,0);
GO

INSERT INTO dbo.MauThongBao(MaMau,Kenh,TieuDeMau,NoiDungMau)
VALUES
('BOOKING_CONFIRMED','APP',N'Đặt sân đã xác nhận',N'Đơn {MaDon} đã được xác nhận.'),
('BOOKING_REMINDER','APP',N'Nhắc lịch sân',N'Bạn có lịch tại {TenSan} lúc {BatDau}.'),
('PAYMENT_PENDING','APP',N'Thanh toán đang chờ',N'Giao dịch {MaGiaoDich} đang chờ nhân viên xác nhận.'),
('VOUCHER_RECEIVED','APP',N'Bạn nhận được voucher',N'Voucher {MaVoucher} đã được thêm vào tài khoản.');
GO


/* ==================== E. PRICING FUNCTIONS ==================== */

/* Function: dbo.fn_DonGiaSanTaiThoiDiem */
/* ============================================================================
   14. PRICING FUNCTIONS
============================================================================ */

CREATE FUNCTION dbo.fn_DonGiaSanTaiThoiDiem
(
    @SanID INT,
    @ThoiDiem DATETIME2(0)
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @DonGia DECIMAL(18,2);
    DECLARE @LoaiSanID INT;
    DECLARE @GiaCoBan DECIMAL(18,2);
    DECLARE @Ngay DATE = CONVERT(DATE, @ThoiDiem);
    DECLARE @Gio TIME(0) = CONVERT(TIME(0), @ThoiDiem);
    DECLARE @ThuISO INT = (DATEDIFF(DAY, '19000101', CONVERT(DATE,@ThoiDiem)) % 7) + 1; -- Monday=1

    SELECT @LoaiSanID = LoaiSanID, @GiaCoBan = GiaCoBan
    FROM dbo.San
    WHERE SanID = @SanID;

    SELECT TOP (1) @DonGia = bg.DonGia
    FROM dbo.BangGia bg
    WHERE bg.DangHoatDong = 1
      AND (bg.SanID = @SanID OR (bg.SanID IS NULL AND bg.LoaiSanID = @LoaiSanID))
      AND @Ngay >= bg.HieuLucTu
      AND (bg.HieuLucDen IS NULL OR @Ngay <= bg.HieuLucDen)
      AND @Gio >= bg.GioBatDau
      AND @Gio < bg.GioKetThuc
      AND
      (
          bg.LoaiNgay = 'TAT_CA'
          OR (bg.LoaiNgay = 'NGAY_THUONG' AND @ThuISO BETWEEN 1 AND 5)
          OR (bg.LoaiNgay = 'CUOI_TUAN' AND @ThuISO IN (6,7))
          OR (bg.LoaiNgay = 'THU_CU_THE' AND bg.ThuTrongTuan = @ThuISO)
      )
    ORDER BY
      CASE WHEN bg.SanID = @SanID THEN 0 ELSE 1 END,
      bg.ThuTuUuTien ASC,
      bg.BangGiaID DESC;

    RETURN ISNULL(@DonGia, ISNULL(@GiaCoBan, 0));
END
GO

/* Function: dbo.fn_TinhTienSan */
CREATE FUNCTION dbo.fn_TinhTienSan
(
    @SanID INT,
    @BatDau DATETIME2(0),
    @KetThuc DATETIME2(0)
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    IF @KetThuc <= @BatDau RETURN 0;

    DECLARE @Tong DECIMAL(18,4) = 0;
    DECLARE @Cursor DATETIME2(0) = @BatDau;
    DECLARE @Next DATETIME2(0);
    DECLARE @Phut INT;
    DECLARE @DonGia DECIMAL(18,2);

    WHILE @Cursor < @KetThuc
    BEGIN
        SET @Next = DATEADD(MINUTE, 15, @Cursor);
        IF @Next > @KetThuc SET @Next = @KetThuc;

        SET @Phut = DATEDIFF(MINUTE, @Cursor, @Next);
        SET @DonGia = dbo.fn_DonGiaSanTaiThoiDiem(@SanID, @Cursor);
        SET @Tong = @Tong + (@DonGia * @Phut / 60.0);
        SET @Cursor = @Next;
    END

    RETURN CONVERT(DECIMAL(18,2), ROUND(@Tong, 0));
END
GO


/* ==================== F. VIEWS ==================== */

/* View: dbo.vw_DonDatSanDayDu */
/* ============================================================================
   12. VIEWS
============================================================================ */

CREATE VIEW dbo.vw_DonDatSanDayDu
AS
SELECT
    d.DonDatSanID,
    d.MaDon,
    d.KhachHangID,
    kh.MaKhachHang,
    kh.HoTen AS TenKhachHang,
    kh.SoDienThoai,
    ct.ChiTietDatSanID,
    ct.SanID,
    s.MaSan,
    s.TenSan,
    ls.TenLoaiSan,
    cs.TenCumSan,
    ct.BatDau,
    ct.KetThuc,
    d.TienSan,
    d.TienDichVu,
    d.TienKhuyenMai,
    d.TienVoucher,
    d.TongTien,
    d.DaThanhToan,
    d.ConLai,
    d.TrangThaiDon,
    d.TrangThaiThanhToan,
    d.NguonDon,
    d.NgayTao
FROM dbo.DonDatSan d
JOIN dbo.KhachHang kh ON kh.KhachHangID = d.KhachHangID
JOIN dbo.ChiTietDatSan ct ON ct.DonDatSanID = d.DonDatSanID
JOIN dbo.San s ON s.SanID = ct.SanID
JOIN dbo.LoaiSan ls ON ls.LoaiSanID = s.LoaiSanID
JOIN dbo.CumSan cs ON cs.CumSanID = s.CumSanID;
GO

/* View: dbo.vw_TrangThaiSanHienTai */
CREATE VIEW dbo.vw_TrangThaiSanHienTai
AS
SELECT
    s.SanID,
    s.MaSan,
    s.TenSan,
    s.LoaiSanID,
    ls.TenLoaiSan,
    s.CumSanID,
    cs.TenCumSan,
    s.GiaCoBan,
    CASE
        WHEN s.DangHoatDong = 0 THEN 'NGUNG_HOAT_DONG'
        WHEN s.TrangThaiVanHanh = 'BAO_TRI' THEN 'BAO_TRI'
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.ChiTietDatSan ct
            JOIN dbo.DonDatSan d ON d.DonDatSanID = ct.DonDatSanID
            WHERE ct.SanID = s.SanID
              AND SYSDATETIME() >= ct.BatDau
              AND SYSDATETIME() < ct.KetThuc
              AND d.TrangThaiDon IN ('DA_XAC_NHAN','DANG_SU_DUNG')
        ) THEN 'DANG_SU_DUNG'
        ELSE 'SAN_SANG'
    END AS TrangThaiHienTai
FROM dbo.San s
JOIN dbo.LoaiSan ls ON ls.LoaiSanID = s.LoaiSanID
JOIN dbo.CumSan cs ON cs.CumSanID = s.CumSanID;
GO

/* View: dbo.vw_HoaDonConNo */
CREATE VIEW dbo.vw_HoaDonConNo
AS
SELECT
    hd.HoaDonID,
    hd.MaHoaDon,
    hd.DonDatSanID,
    d.MaDon,
    hd.KhachHangID,
    kh.HoTen AS TenKhachHang,
    hd.TongTien,
    hd.DaThanhToan,
    hd.ConLai,
    hd.TrangThaiThanhToan,
    hd.NgayLap
FROM dbo.HoaDon hd
JOIN dbo.DonDatSan d ON d.DonDatSanID = hd.DonDatSanID
JOIN dbo.KhachHang kh ON kh.KhachHangID = hd.KhachHangID
WHERE hd.ConLai > 0;
GO

/* View: dbo.vw_GiaoDichChoXacNhan */
CREATE VIEW dbo.vw_GiaoDichChoXacNhan
AS
SELECT
    gd.GiaoDichID,
    gd.MaGiaoDich,
    gd.HoaDonID,
    hd.MaHoaDon,
    d.MaDon,
    gd.SoTien,
    gd.PhuongThuc,
    gd.TrangThai,
    gd.MaThamChieu,
    gd.NoiDungChuyenKhoan,
    gd.NgayTao
FROM dbo.GiaoDichThanhToan gd
JOIN dbo.HoaDon hd ON hd.HoaDonID = gd.HoaDonID
JOIN dbo.DonDatSan d ON d.DonDatSanID = gd.DonDatSanID
WHERE gd.TrangThai = 'CHO_XAC_NHAN';
GO

/* View: dbo.vw_TonKhoHienTai */
CREATE VIEW dbo.vw_TonKhoHienTai
AS
SELECT
    tk.KhoID,
    k.MaKho,
    k.TenKho,
    tk.SanPhamDichVuID,
    sp.MaSanPhamDichVu,
    sp.TenSanPhamDichVu,
    sp.DonViTinh,
    tk.SoLuongTon,
    sp.SoLuongTonToiThieu,
    CASE WHEN tk.SoLuongTon <= sp.SoLuongTonToiThieu THEN 1 ELSE 0 END AS CanhBaoTonThap,
    tk.NgayCapNhat
FROM dbo.TonKho tk
JOIN dbo.Kho k ON k.KhoID = tk.KhoID
JOIN dbo.SanPhamDichVu sp ON sp.SanPhamDichVuID = tk.SanPhamDichVuID;
GO

/* View: dbo.vw_KhachHangTongHop */
CREATE VIEW dbo.vw_KhachHangTongHop
AS
SELECT
    kh.KhachHangID,
    kh.MaKhachHang,
    kh.HoTen,
    kh.SoDienThoai,
    kh.Email,
    htv.TenHang,
    kh.DiemTichLuy,
    kh.TongChiTieu,
    COUNT(d.DonDatSanID) AS SoLuotDat,
    SUM(CASE WHEN d.TrangThaiDon = 'HOAN_THANH' THEN 1 ELSE 0 END) AS SoLuotHoanThanh,
    SUM(CASE WHEN d.TrangThaiDon = 'DA_HUY' THEN 1 ELSE 0 END) AS SoLuotHuy,
    kh.DangHoatDong
FROM dbo.KhachHang kh
LEFT JOIN dbo.HangThanhVien htv ON htv.HangThanhVienID = kh.HangThanhVienID
LEFT JOIN dbo.DonDatSan d ON d.KhachHangID = kh.KhachHangID
GROUP BY kh.KhachHangID, kh.MaKhachHang, kh.HoTen, kh.SoDienThoai, kh.Email, htv.TenHang,
         kh.DiemTichLuy, kh.TongChiTieu, kh.DangHoatDong;
GO

/* View: dbo.vw_DoanhThuTheoNgay */
CREATE VIEW dbo.vw_DoanhThuTheoNgay
AS
SELECT
    CONVERT(date, gd.NgayXacNhan) AS Ngay,
    SUM(gd.SoTien) AS DoanhThu
FROM dbo.GiaoDichThanhToan gd
WHERE gd.TrangThai = 'THANH_CONG'
GROUP BY CONVERT(date, gd.NgayXacNhan);
GO

/* View: dbo.vw_DoanhThuTheoSan */
CREATE VIEW dbo.vw_DoanhThuTheoSan
AS
SELECT
    s.SanID,
    s.MaSan,
    s.TenSan,
    SUM(CASE WHEN d.TrangThaiDon <> 'DA_HUY' THEN ct.TienSan ELSE 0 END) AS DoanhThuTienSan,
    COUNT(DISTINCT CASE WHEN d.TrangThaiDon <> 'DA_HUY' THEN d.DonDatSanID END) AS SoLuotDat
FROM dbo.San s
LEFT JOIN dbo.ChiTietDatSan ct ON ct.SanID = s.SanID
LEFT JOIN dbo.DonDatSan d ON d.DonDatSanID = ct.DonDatSanID
GROUP BY s.SanID, s.MaSan, s.TenSan;
GO

/* View: dbo.vw_DoanhThuTheoLoaiSan */
CREATE VIEW dbo.vw_DoanhThuTheoLoaiSan
AS
SELECT
    ls.LoaiSanID,
    ls.MaLoaiSan,
    ls.TenLoaiSan,
    SUM(CASE WHEN d.TrangThaiDon <> 'DA_HUY' THEN ct.TienSan ELSE 0 END) AS DoanhThuTienSan,
    COUNT(DISTINCT CASE WHEN d.TrangThaiDon <> 'DA_HUY' THEN d.DonDatSanID END) AS SoLuotDat
FROM dbo.LoaiSan ls
LEFT JOIN dbo.San s ON s.LoaiSanID = ls.LoaiSanID
LEFT JOIN dbo.ChiTietDatSan ct ON ct.SanID = s.SanID
LEFT JOIN dbo.DonDatSan d ON d.DonDatSanID = ct.DonDatSanID
GROUP BY ls.LoaiSanID, ls.MaLoaiSan, ls.TenLoaiSan;
GO

/* View: dbo.vw_TaiKhoanPhanQuyenHieuLuc */
/* ==========================================================================
   27. ULTIMATE VIEWS FOR UI / DASHBOARD / FILTERS
============================================================================ */
CREATE VIEW dbo.vw_TaiKhoanPhanQuyenHieuLuc
AS
SELECT tk.TaiKhoanID, tk.TenDangNhap, vt.MaVaiTro, q.QuyenID, q.MaQuyen,
       CAST(CASE WHEN tq.ChoPhep IS NOT NULL THEN tq.ChoPhep ELSE 1 END AS BIT) AS ChoPhep
FROM dbo.TaiKhoan tk
JOIN dbo.VaiTro vt ON vt.VaiTroID=tk.VaiTroID
JOIN dbo.VaiTroQuyen vtq ON vtq.VaiTroID=vt.VaiTroID
JOIN dbo.Quyen q ON q.QuyenID=vtq.QuyenID
LEFT JOIN dbo.TaiKhoanQuyen tq ON tq.TaiKhoanID=tk.TaiKhoanID AND tq.QuyenID=q.QuyenID
WHERE ISNULL(tq.ChoPhep,1)=1
UNION
SELECT tk.TaiKhoanID, tk.TenDangNhap, vt.MaVaiTro, q.QuyenID, q.MaQuyen, CAST(1 AS BIT)
FROM dbo.TaiKhoanQuyen tq
JOIN dbo.TaiKhoan tk ON tk.TaiKhoanID=tq.TaiKhoanID
JOIN dbo.VaiTro vt ON vt.VaiTroID=tk.VaiTroID
JOIN dbo.Quyen q ON q.QuyenID=tq.QuyenID
WHERE tq.ChoPhep=1
  AND NOT EXISTS (SELECT 1 FROM dbo.VaiTroQuyen x WHERE x.VaiTroID=tk.VaiTroID AND x.QuyenID=q.QuyenID);
GO

/* View: dbo.vw_VoucherKhachHangKhaDung */
CREATE VIEW dbo.vw_VoucherKhachHangKhaDung
AS
SELECT vkh.VoucherKhachHangID, vkh.KhachHangID, v.VoucherID, v.MaVoucher, v.TenVoucher,
       v.LoaiGiam, v.GiaTriGiam, v.GiamToiDa, v.DonToiThieu, v.NgayBatDau, v.NgayKetThuc
FROM dbo.VoucherKhachHang vkh
JOIN dbo.Voucher v ON v.VoucherID=vkh.VoucherID
WHERE vkh.DaSuDung=0 AND v.DangHoatDong=1
  AND CONVERT(date,GETDATE()) BETWEEN v.NgayBatDau AND v.NgayKetThuc;
GO

/* View: dbo.vw_DonOnlineChoXuLy */
CREATE VIEW dbo.vw_DonOnlineChoXuLy
AS
SELECT gc.GiuChoID, gc.KhachHangID, kh.HoTen, kh.SoDienThoai, gc.SanID, s.TenSan,
       gc.BatDau, gc.KetThuc, gc.HetHanLuc, gc.TrangThai,
       dc.DatCocID, dc.SoTienCoc, dc.TrangThai AS TrangThaiCoc, dc.VietQRID
FROM dbo.GiuChoDatSan gc
JOIN dbo.KhachHang kh ON kh.KhachHangID=gc.KhachHangID
JOIN dbo.San s ON s.SanID=gc.SanID
LEFT JOIN dbo.DatCocOnline dc ON dc.GiuChoID=gc.GiuChoID
WHERE gc.TrangThai IN ('DANG_GIU') OR (dc.TrangThai IN ('CHO_THANH_TOAN','CHO_XAC_NHAN'));
GO

/* View: dbo.vw_CanhBaoTonKho */
CREATE VIEW dbo.vw_CanhBaoTonKho
AS
SELECT tk.KhoID,k.TenKho,sp.SanPhamDichVuID,sp.MaSanPhamDichVu,sp.TenSanPhamDichVu,
       tk.SoLuongTon,sp.SoLuongTonToiThieu,
       CASE WHEN tk.SoLuongTon<=sp.SoLuongTonToiThieu THEN 1 ELSE 0 END AS CanNhapThem
FROM dbo.TonKho tk
JOIN dbo.Kho k ON k.KhoID=tk.KhoID
JOIN dbo.SanPhamDichVu sp ON sp.SanPhamDichVuID=tk.SanPhamDichVuID
WHERE sp.QuanLyTonKho=1;
GO

/* View: dbo.vw_CaDangMo */
CREATE VIEW dbo.vw_CaDangMo
AS
SELECT c.CaLamViecID,c.NhanVienID,nv.MaNhanVien,nv.HoTen,c.CumSanID,cs.TenCumSan,
       c.ThoiGianMoCa,c.TienDauCa,c.TongThu,c.TienMat,c.VietQR,c.ChuyenKhoan,c.TrangThai
FROM dbo.CaLamViec c
JOIN dbo.NhanVien nv ON nv.NhanVienID=c.NhanVienID
LEFT JOIN dbo.CumSan cs ON cs.CumSanID=c.CumSanID
WHERE c.TrangThai='DANG_MO';
GO

/* View: dbo.vw_ThietBiIoTTrangThai */
CREATE VIEW dbo.vw_ThietBiIoTTrangThai
AS
SELECT tb.ThietBiID,tb.MaThietBi,tb.LoaiThietBi,tb.SanID,s.TenSan,tb.DiaChiIP,tb.TrangThai,tb.LanKetNoiCuoi,tb.DangHoatDong
FROM dbo.ThietBiIoT tb LEFT JOIN dbo.San s ON s.SanID=tb.SanID;
GO

/* View: dbo.vw_DoiSoatChuyenKhoanChoXuLy */
CREATE VIEW dbo.vw_DoiSoatChuyenKhoanChoXuLy
AS
SELECT gd.GiaoDichID,gd.MaGiaoDich,gd.SoTien,gd.NoiDungChuyenKhoan,gd.NgayTao,
       hd.MaHoaDon,d.MaDon,kh.HoTen, sk.SaoKeID,sk.MaGiaoDichNganHang,sk.ThoiGianGiaoDich,sk.TrangThaiDoiSoat
FROM dbo.GiaoDichThanhToan gd
JOIN dbo.HoaDon hd ON hd.HoaDonID=gd.HoaDonID
JOIN dbo.DonDatSan d ON d.DonDatSanID=gd.DonDatSanID
JOIN dbo.KhachHang kh ON kh.KhachHangID=d.KhachHangID
LEFT JOIN dbo.DoiSoatThanhToan ds ON ds.GiaoDichID=gd.GiaoDichID AND ds.TrangThai='DA_KHOP'
LEFT JOIN dbo.GiaoDichSaoKeNganHang sk ON sk.SaoKeID=ds.SaoKeID
WHERE gd.PhuongThuc IN ('VIETQR','CHUYEN_KHOAN') AND gd.TrangThai='CHO_XAC_NHAN';
GO

/* View: dbo.vw_ChiTietHoaDonInAn */
CREATE VIEW dbo.vw_ChiTietHoaDonInAn
AS
SELECT hd.HoaDonID,hd.MaHoaDon,hd.DonDatSanID,hd.KhachHangID,kh.HoTen,kh.SoDienThoai,
       hd.TongTien,hd.DaThanhToan,hd.ConLai,hd.TrangThaiThanhToan,hd.NgayLap,
       ct.ChiTietHoaDonID,ct.LoaiDong,ct.MaThamChieu,ct.MoTa,ct.SoLuong,ct.DonGia,ct.ThanhTien,ct.ThuTu
FROM dbo.HoaDon hd
JOIN dbo.KhachHang kh ON kh.KhachHangID=hd.KhachHangID
LEFT JOIN dbo.ChiTietHoaDon ct ON ct.HoaDonID=hd.HoaDonID;
GO

/* View: dbo.vw_LichSanHomNay */
CREATE VIEW dbo.vw_LichSanHomNay
AS
SELECT *
FROM dbo.vw_DonDatSanDayDu
WHERE CONVERT(date, BatDau) = CONVERT(date, GETDATE())
  AND TrangThaiDon <> 'DA_HUY';
GO


/* ==================== G. STORED PROCEDURES ==================== */

/* Procedure: dbo.sp_LayTaiKhoanDangNhap */
/* ============================================================================
   13. STORED PROCEDURES - ACCOUNTS
============================================================================ */

CREATE PROCEDURE dbo.sp_LayTaiKhoanDangNhap
    @TenDangNhap VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        tk.TaiKhoanID,
        tk.TenDangNhap,
        tk.MatKhauHash,
        tk.DangHoatDong,
        tk.BatBuocDoiMatKhau,
        tk.KhoaDen,
        vt.MaVaiTro,
        vt.TenVaiTro,
        tk.NhanVienID,
        nv.HoTen AS TenNhanVien,
        tk.KhachHangID,
        kh.HoTen AS TenKhachHang
    FROM dbo.TaiKhoan tk
    JOIN dbo.VaiTro vt ON vt.VaiTroID = tk.VaiTroID
    LEFT JOIN dbo.NhanVien nv ON nv.NhanVienID = tk.NhanVienID
    LEFT JOIN dbo.KhachHang kh ON kh.KhachHangID = tk.KhachHangID
    WHERE tk.TenDangNhap = @TenDangNhap;
END
GO

/* Procedure: dbo.sp_TaoTaiKhoanAdminBanDau */
CREATE PROCEDURE dbo.sp_TaoTaiKhoanAdminBanDau
    @TenDangNhap VARCHAR(100),
    @MatKhauHash NVARCHAR(500),
    @HoTen NVARCHAR(150),
    @SoDienThoai VARCHAR(20) = NULL,
    @Email VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE TenDangNhap = @TenDangNhap)
    BEGIN
        RAISERROR(N'Tên đăng nhập đã tồn tại.',16,1);
        RETURN;
    END

    BEGIN TRAN;

    DECLARE @MaNV VARCHAR(30);
    SET @MaNV = 'AD' + RIGHT('00000000' + CONVERT(VARCHAR(20), ABS(CHECKSUM(NEWID()))), 8);

    WHILE EXISTS (SELECT 1 FROM dbo.NhanVien WHERE MaNhanVien = @MaNV)
        SET @MaNV = 'AD' + RIGHT('00000000' + CONVERT(VARCHAR(20), ABS(CHECKSUM(NEWID()))), 8);

    INSERT INTO dbo.NhanVien(MaNhanVien, HoTen, SoDienThoai, Email, ChucVu)
    VALUES (@MaNV, @HoTen, @SoDienThoai, @Email, N'Quản trị viên');

    DECLARE @NhanVienID INT = SCOPE_IDENTITY();
    DECLARE @VaiTroID INT = (SELECT VaiTroID FROM dbo.VaiTro WHERE MaVaiTro = 'ADMIN');

    INSERT INTO dbo.TaiKhoan(TenDangNhap, MatKhauHash, VaiTroID, NhanVienID)
    VALUES (@TenDangNhap, @MatKhauHash, @VaiTroID, @NhanVienID);

    COMMIT TRAN;
END
GO

/* Procedure: dbo.sp_TaoKhachHang */
/* ============================================================================
   15. CUSTOMER / BOOKING PROCEDURES
============================================================================ */

CREATE PROCEDURE dbo.sp_TaoKhachHang
    @HoTen NVARCHAR(150),
    @SoDienThoai VARCHAR(20) = NULL,
    @Email VARCHAR(150) = NULL,
    @DiaChi NVARCHAR(300) = NULL,
    @NgaySinh DATE = NULL,
    @KhachHangID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @SoDienThoai IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.KhachHang WHERE SoDienThoai = @SoDienThoai)
    BEGIN
        RAISERROR(N'Số điện thoại đã tồn tại.',16,1);
        RETURN;
    END

    IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.KhachHang WHERE Email = @Email)
    BEGIN
        RAISERROR(N'Email đã tồn tại.',16,1);
        RETURN;
    END

    DECLARE @Ma VARCHAR(30);
    SET @Ma = 'KH' + RIGHT('00000000' + CONVERT(VARCHAR(20), ABS(CHECKSUM(NEWID()))), 8);
    WHILE EXISTS (SELECT 1 FROM dbo.KhachHang WHERE MaKhachHang = @Ma)
        SET @Ma = 'KH' + RIGHT('00000000' + CONVERT(VARCHAR(20), ABS(CHECKSUM(NEWID()))), 8);

    DECLARE @HangID INT = (SELECT TOP 1 HangThanhVienID FROM dbo.HangThanhVien WHERE MaHang='THUONG');

    INSERT INTO dbo.KhachHang(MaKhachHang, HoTen, SoDienThoai, Email, DiaChi, NgaySinh, HangThanhVienID)
    VALUES (@Ma, @HoTen, @SoDienThoai, @Email, @DiaChi, @NgaySinh, @HangID);

    SET @KhachHangID = SCOPE_IDENTITY();

    INSERT INTO dbo.ViKhachHang(KhachHangID, SoDu) VALUES (@KhachHangID, 0);
END
GO

/* Procedure: dbo.sp_KiemTraTrungLichSan */
CREATE PROCEDURE dbo.sp_KiemTraTrungLichSan
    @SanID INT,
    @BatDau DATETIME2(0),
    @KetThuc DATETIME2(0),
    @BoQuaDonDatSanID BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BiTrung BIT = 0;
    DECLARE @LyDo NVARCHAR(300) = NULL;

    IF @KetThuc <= @BatDau
    BEGIN
        SELECT CAST(1 AS BIT) AS BiTrung, N'Thời gian kết thúc phải lớn hơn thời gian bắt đầu.' AS LyDo;
        RETURN;
    END

    IF EXISTS
    (
        SELECT 1
        FROM dbo.LichBaoTriSan bt
        WHERE bt.SanID = @SanID
          AND bt.TrangThai IN ('KE_HOACH','DANG_BAO_TRI')
          AND @BatDau < bt.DenThoiGian
          AND @KetThuc > bt.TuThoiGian
    )
    BEGIN
        SET @BiTrung = 1;
        SET @LyDo = N'Sân đang có lịch bảo trì trong khoảng thời gian này.';
    END

    IF @BiTrung = 0 AND EXISTS
    (
        SELECT 1
        FROM dbo.ChiTietDatSan ct
        JOIN dbo.DonDatSan d ON d.DonDatSanID = ct.DonDatSanID
        WHERE ct.SanID = @SanID
          AND (@BoQuaDonDatSanID IS NULL OR d.DonDatSanID <> @BoQuaDonDatSanID)
          AND d.TrangThaiDon IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_SU_DUNG')
          AND @BatDau < ct.KetThuc
          AND @KetThuc > ct.BatDau
    )
    BEGIN
        SET @BiTrung = 1;
        SET @LyDo = N'Sân đã có đơn đặt trùng khung giờ.';
    END

    IF @BiTrung = 0 AND EXISTS
    (
        SELECT 1
        FROM dbo.GiuChoDatSan gc
        WHERE gc.SanID = @SanID
          AND gc.TrangThai = 'DANG_GIU'
          AND gc.HetHanLuc > SYSDATETIME()
          AND @BatDau < gc.KetThuc
          AND @KetThuc > gc.BatDau
    )
    BEGIN
        SET @BiTrung = 1;
        SET @LyDo = N'Sân đang được giữ chỗ bởi một giao dịch online khác.';
    END

    SELECT @BiTrung AS BiTrung, @LyDo AS LyDo;
END
GO

/* Procedure: dbo.sp_TimSanTrong */
CREATE PROCEDURE dbo.sp_TimSanTrong
    @BatDau DATETIME2(0),
    @KetThuc DATETIME2(0),
    @LoaiSanID INT = NULL,
    @CumSanID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.SanID,
        s.MaSan,
        s.TenSan,
        s.LoaiSanID,
        ls.TenLoaiSan,
        s.CumSanID,
        cs.TenCumSan,
        s.ViTri,
        s.GiaCoBan,
        dbo.fn_TinhTienSan(s.SanID,@BatDau,@KetThuc) AS TienDuKien
    FROM dbo.San s
    JOIN dbo.LoaiSan ls ON ls.LoaiSanID = s.LoaiSanID
    JOIN dbo.CumSan cs ON cs.CumSanID = s.CumSanID
    WHERE s.DangHoatDong = 1
      AND s.TrangThaiVanHanh = 'SAN_SANG'
      AND (@LoaiSanID IS NULL OR s.LoaiSanID = @LoaiSanID)
      AND (@CumSanID IS NULL OR s.CumSanID = @CumSanID)
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.LichBaoTriSan bt
          WHERE bt.SanID = s.SanID
            AND bt.TrangThai IN ('KE_HOACH','DANG_BAO_TRI')
            AND @BatDau < bt.DenThoiGian
            AND @KetThuc > bt.TuThoiGian
      )
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.ChiTietDatSan ct
          JOIN dbo.DonDatSan d ON d.DonDatSanID = ct.DonDatSanID
          WHERE ct.SanID = s.SanID
            AND d.TrangThaiDon IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_SU_DUNG')
            AND @BatDau < ct.KetThuc
            AND @KetThuc > ct.BatDau
      )
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.GiuChoDatSan gc
          WHERE gc.SanID = s.SanID
            AND gc.TrangThai = 'DANG_GIU'
            AND gc.HetHanLuc > SYSDATETIME()
            AND @BatDau < gc.KetThuc
            AND @KetThuc > gc.BatDau
      )
    ORDER BY ls.TenLoaiSan, s.TenSan;
END
GO

/* Procedure: dbo.sp_TinhGiaDatSan */
CREATE PROCEDURE dbo.sp_TinhGiaDatSan
    @SanID INT,
    @BatDau DATETIME2(0),
    @KetThuc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;

    IF @KetThuc <= @BatDau
    BEGIN
        RAISERROR(N'Thời gian không hợp lệ.',16,1);
        RETURN;
    END

    SELECT
        @SanID AS SanID,
        DATEDIFF(MINUTE,@BatDau,@KetThuc) AS SoPhut,
        dbo.fn_DonGiaSanTaiThoiDiem(@SanID,@BatDau) AS DonGiaTaiLucBatDau,
        dbo.fn_TinhTienSan(@SanID,@BatDau,@KetThuc) AS TienSan;
END
GO

/* Procedure: dbo.sp_TaoDonDatSan */
CREATE PROCEDURE dbo.sp_TaoDonDatSan
    @KhachHangID INT,
    @SanID INT,
    @BatDau DATETIME2(0),
    @KetThuc DATETIME2(0),
    @NguonDon VARCHAR(30),
    @NguoiTaoTaiKhoanID INT = NULL,
    @VoucherKhachHangID BIGINT = NULL,
    @GhiChu NVARCHAR(1000) = NULL,
    @DonDatSanID BIGINT OUTPUT,
    @MaDon VARCHAR(40) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @KetThuc <= @BatDau
    BEGIN
        RAISERROR(N'Thời gian kết thúc phải lớn hơn thời gian bắt đầu.',16,1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE KhachHangID=@KhachHangID AND DangHoatDong=1)
    BEGIN
        RAISERROR(N'Khách hàng không tồn tại hoặc đã bị khóa.',16,1);
        RETURN;
    END

    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRY
        BEGIN TRAN;

        DECLARE @LoaiSanID INT, @TenSan NVARCHAR(150), @TenLoaiSan NVARCHAR(120), @TrangThai VARCHAR(30), @SanActive BIT;
        SELECT @LoaiSanID=s.LoaiSanID, @TenSan=s.TenSan, @TenLoaiSan=ls.TenLoaiSan,
               @TrangThai=s.TrangThaiVanHanh, @SanActive=s.DangHoatDong
        FROM dbo.San s WITH (UPDLOCK,HOLDLOCK)
        JOIN dbo.LoaiSan ls ON ls.LoaiSanID=s.LoaiSanID
        WHERE s.SanID=@SanID;

        IF @TenSan IS NULL OR @SanActive=0 OR @TrangThai <> 'SAN_SANG'
        BEGIN
            RAISERROR(N'Sân không khả dụng để đặt.',16,1);
        END

        IF EXISTS
        (
            SELECT 1 FROM dbo.LichBaoTriSan WITH (UPDLOCK,HOLDLOCK)
            WHERE SanID=@SanID AND TrangThai IN ('KE_HOACH','DANG_BAO_TRI')
              AND @BatDau < DenThoiGian AND @KetThuc > TuThoiGian
        )
            RAISERROR(N'Sân đang có lịch bảo trì trong thời gian đã chọn.',16,1);

        IF EXISTS
        (
            SELECT 1
            FROM dbo.ChiTietDatSan ct WITH (UPDLOCK,HOLDLOCK)
            JOIN dbo.DonDatSan d WITH (UPDLOCK,HOLDLOCK) ON d.DonDatSanID=ct.DonDatSanID
            WHERE ct.SanID=@SanID
              AND d.TrangThaiDon IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_SU_DUNG')
              AND @BatDau < ct.KetThuc AND @KetThuc > ct.BatDau
        )
            RAISERROR(N'Khung giờ vừa được người khác đặt. Vui lòng chọn thời gian khác.',16,1);

        IF EXISTS
        (
            SELECT 1 FROM dbo.GiuChoDatSan WITH (UPDLOCK,HOLDLOCK)
            WHERE SanID=@SanID AND TrangThai='DANG_GIU' AND HetHanLuc>SYSDATETIME()
              AND @BatDau < KetThuc AND @KetThuc > BatDau
        )
            RAISERROR(N'Khung giờ đang được giữ chỗ bởi giao dịch online khác.',16,1);

        DECLARE @TienSan DECIMAL(18,2) = dbo.fn_TinhTienSan(@SanID,@BatDau,@KetThuc);
        DECLARE @TienKM DECIMAL(18,2) = 0;
        DECLARE @TienVoucher DECIMAL(18,2) = 0;
        DECLARE @KhuyenMaiID INT = NULL;
        DECLARE @KmLoai VARCHAR(20), @KmGiaTri DECIMAL(18,2), @KmMax DECIMAL(18,2);
        DECLARE @Ngay DATE=CONVERT(DATE,@BatDau), @Gio TIME(0)=CONVERT(TIME(0),@BatDau);

        SELECT TOP 1
            @KhuyenMaiID=km.KhuyenMaiID,
            @KmLoai=km.LoaiGiam,
            @KmGiaTri=km.GiaTriGiam,
            @KmMax=km.GiamToiDa
        FROM dbo.KhuyenMai km
        WHERE km.DangHoatDong=1
          AND @Ngay BETWEEN km.NgayBatDau AND km.NgayKetThuc
          AND @TienSan >= km.DonToiThieu
          AND (km.GioBatDau IS NULL OR @Gio >= km.GioBatDau)
          AND (km.GioKetThuc IS NULL OR @Gio < km.GioKetThuc)
          AND
          (
              NOT EXISTS (SELECT 1 FROM dbo.KhuyenMaiSan x WHERE x.KhuyenMaiID=km.KhuyenMaiID)
              OR EXISTS (SELECT 1 FROM dbo.KhuyenMaiSan x WHERE x.KhuyenMaiID=km.KhuyenMaiID AND x.SanID=@SanID)
          )
          AND
          (
              NOT EXISTS (SELECT 1 FROM dbo.KhuyenMaiLoaiSan x WHERE x.KhuyenMaiID=km.KhuyenMaiID)
              OR EXISTS (SELECT 1 FROM dbo.KhuyenMaiLoaiSan x WHERE x.KhuyenMaiID=km.KhuyenMaiID AND x.LoaiSanID=@LoaiSanID)
          )
        ORDER BY
          CASE WHEN km.LoaiGiam='PHAN_TRAM' THEN
             CASE WHEN km.GiamToiDa IS NULL THEN @TienSan*km.GiaTriGiam/100.0
                  WHEN @TienSan*km.GiaTriGiam/100.0 > km.GiamToiDa THEN km.GiamToiDa
                  ELSE @TienSan*km.GiaTriGiam/100.0 END
          ELSE km.GiaTriGiam END DESC;

        IF @KhuyenMaiID IS NOT NULL
        BEGIN
            IF @KmLoai='PHAN_TRAM'
                SET @TienKM = @TienSan*@KmGiaTri/100.0;
            ELSE
                SET @TienKM = @KmGiaTri;

            IF @KmMax IS NOT NULL AND @TienKM>@KmMax SET @TienKM=@KmMax;
            IF @TienKM>@TienSan SET @TienKM=@TienSan;
        END

        IF @VoucherKhachHangID IS NOT NULL
        BEGIN
            DECLARE @VLoai VARCHAR(20), @VGiaTri DECIMAL(18,2), @VMax DECIMAL(18,2), @VMin DECIMAL(18,2);

            SELECT @VLoai=v.LoaiGiam, @VGiaTri=v.GiaTriGiam, @VMax=v.GiamToiDa, @VMin=v.DonToiThieu
            FROM dbo.VoucherKhachHang vkh WITH (UPDLOCK,HOLDLOCK)
            JOIN dbo.Voucher v ON v.VoucherID=vkh.VoucherID
            WHERE vkh.VoucherKhachHangID=@VoucherKhachHangID
              AND vkh.KhachHangID=@KhachHangID
              AND vkh.DaSuDung=0
              AND v.DangHoatDong=1
              AND @Ngay BETWEEN v.NgayBatDau AND v.NgayKetThuc;

            IF @VLoai IS NULL
                RAISERROR(N'Voucher không hợp lệ, đã sử dụng hoặc đã hết hạn.',16,1);

            IF @TienSan < ISNULL(@VMin,0)
                RAISERROR(N'Đơn chưa đạt giá trị tối thiểu để sử dụng voucher.',16,1);

            IF @VLoai='PHAN_TRAM' SET @TienVoucher=@TienSan*@VGiaTri/100.0;
            ELSE SET @TienVoucher=@VGiaTri;

            IF @VMax IS NOT NULL AND @TienVoucher>@VMax SET @TienVoucher=@VMax;
            IF @TienVoucher > (@TienSan-@TienKM) SET @TienVoucher=@TienSan-@TienKM;
        END

        DECLARE @TongTien DECIMAL(18,2)=@TienSan-@TienKM-@TienVoucher;
        IF @TongTien < 0 SET @TongTien=0;

        SET @MaDon = 'BK' + CONVERT(VARCHAR(8),GETDATE(),112)
                    + REPLACE(CONVERT(VARCHAR(8),GETDATE(),108),':','')
                    + RIGHT('0000'+CONVERT(VARCHAR(10),ABS(CHECKSUM(NEWID()))%10000),4);
        WHILE EXISTS (SELECT 1 FROM dbo.DonDatSan WHERE MaDon=@MaDon)
            SET @MaDon = 'BK' + CONVERT(VARCHAR(8),GETDATE(),112)
                        + REPLACE(CONVERT(VARCHAR(8),GETDATE(),108),':','')
                        + RIGHT('0000'+CONVERT(VARCHAR(10),ABS(CHECKSUM(NEWID()))%10000),4);

        INSERT INTO dbo.DonDatSan
        (MaDon,KhachHangID,NguoiTaoTaiKhoanID,NguonDon,TrangThaiDon,TrangThaiThanhToan,
         TienSan,TienDichVu,TienKhuyenMai,TienVoucher,TongTien,DaThanhToan,GhiChu)
        VALUES
        (@MaDon,@KhachHangID,@NguoiTaoTaiKhoanID,@NguonDon,'CHO_XAC_NHAN','CHUA_THANH_TOAN',
         @TienSan,0,@TienKM,@TienVoucher,@TongTien,0,@GhiChu);

        SET @DonDatSanID=SCOPE_IDENTITY();

        INSERT INTO dbo.ChiTietDatSan
        (DonDatSanID,SanID,BatDau,KetThuc,SoPhut,DonGiaSnapshot,TienSan,TenSanSnapshot,TenLoaiSanSnapshot)
        VALUES
        (@DonDatSanID,@SanID,@BatDau,@KetThuc,DATEDIFF(MINUTE,@BatDau,@KetThuc),
         dbo.fn_DonGiaSanTaiThoiDiem(@SanID,@BatDau),@TienSan,@TenSan,@TenLoaiSan);

        INSERT INTO dbo.LichSuTrangThaiDon(DonDatSanID,TrangThaiCu,TrangThaiMoi,TaiKhoanThucHienID,LyDo)
        VALUES (@DonDatSanID,NULL,'CHO_XAC_NHAN',@NguoiTaoTaiKhoanID,N'Tạo đơn đặt sân');

        IF @VoucherKhachHangID IS NOT NULL
        BEGIN
            UPDATE dbo.VoucherKhachHang
            SET DaSuDung=1, NgaySuDung=SYSDATETIME(), DonDatSanID=@DonDatSanID
            WHERE VoucherKhachHangID=@VoucherKhachHangID;

            UPDATE v
            SET SoLuongDaDung=SoLuongDaDung+1
            FROM dbo.Voucher v
            JOIN dbo.VoucherKhachHang vkh ON vkh.VoucherID=v.VoucherID
            WHERE vkh.VoucherKhachHangID=@VoucherKhachHangID;
        END

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE();
        RAISERROR(N'%s',16,1,@Err);
        RETURN;
    END CATCH
END
GO

/* Procedure: dbo.sp_XacNhanDon */
CREATE PROCEDURE dbo.sp_XacNhanDon
    @DonDatSanID BIGINT,
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;

    DECLARE @Old VARCHAR(30), @Pay VARCHAR(30);
    SELECT @Old=TrangThaiDon,@Pay=TrangThaiThanhToan FROM dbo.DonDatSan WITH (UPDLOCK) WHERE DonDatSanID=@DonDatSanID;

    IF @Old <> 'CHO_XAC_NHAN'
    BEGIN
        ROLLBACK TRAN;
        RAISERROR(N'Chỉ đơn chờ xác nhận mới được xác nhận.',16,1);
        RETURN;
    END

    UPDATE dbo.DonDatSan SET TrangThaiDon='DA_XAC_NHAN',NgayCapNhat=SYSDATETIME() WHERE DonDatSanID=@DonDatSanID;
    INSERT INTO dbo.LichSuTrangThaiDon(DonDatSanID,TrangThaiCu,TrangThaiMoi,TrangThaiThanhToanCu,TrangThaiThanhToanMoi,TaiKhoanThucHienID)
    VALUES(@DonDatSanID,@Old,'DA_XAC_NHAN',@Pay,@Pay,@TaiKhoanID);

    COMMIT TRAN;
END
GO

/* Procedure: dbo.sp_CheckIn */
CREATE PROCEDURE dbo.sp_CheckIn
    @DonDatSanID BIGINT,
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;
    DECLARE @Old VARCHAR(30), @Pay VARCHAR(30);
    SELECT @Old=TrangThaiDon,@Pay=TrangThaiThanhToan FROM dbo.DonDatSan WITH (UPDLOCK) WHERE DonDatSanID=@DonDatSanID;
    IF @Old <> 'DA_XAC_NHAN'
    BEGIN
        ROLLBACK TRAN; RAISERROR(N'Đơn phải ở trạng thái đã xác nhận trước khi check-in.',16,1); RETURN;
    END
    UPDATE dbo.DonDatSan SET TrangThaiDon='DANG_SU_DUNG',NgayCapNhat=SYSDATETIME() WHERE DonDatSanID=@DonDatSanID;
    INSERT INTO dbo.LichSuTrangThaiDon(DonDatSanID,TrangThaiCu,TrangThaiMoi,TrangThaiThanhToanCu,TrangThaiThanhToanMoi,TaiKhoanThucHienID)
    VALUES(@DonDatSanID,@Old,'DANG_SU_DUNG',@Pay,@Pay,@TaiKhoanID);
    COMMIT TRAN;
END
GO

/* Procedure: dbo.sp_HoanTatDon */
CREATE PROCEDURE dbo.sp_HoanTatDon
    @DonDatSanID BIGINT,
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;
    DECLARE @Old VARCHAR(30), @Pay VARCHAR(30);
    SELECT @Old=TrangThaiDon,@Pay=TrangThaiThanhToan FROM dbo.DonDatSan WITH (UPDLOCK) WHERE DonDatSanID=@DonDatSanID;
    IF @Old NOT IN ('DA_XAC_NHAN','DANG_SU_DUNG')
    BEGIN
        ROLLBACK TRAN; RAISERROR(N'Trạng thái đơn không cho phép hoàn tất.',16,1); RETURN;
    END
    UPDATE dbo.DonDatSan SET TrangThaiDon='HOAN_THANH',NgayCapNhat=SYSDATETIME() WHERE DonDatSanID=@DonDatSanID;
    INSERT INTO dbo.LichSuTrangThaiDon(DonDatSanID,TrangThaiCu,TrangThaiMoi,TrangThaiThanhToanCu,TrangThaiThanhToanMoi,TaiKhoanThucHienID)
    VALUES(@DonDatSanID,@Old,'HOAN_THANH',@Pay,@Pay,@TaiKhoanID);
    COMMIT TRAN;
END
GO

/* Procedure: dbo.sp_HuyDon */
CREATE PROCEDURE dbo.sp_HuyDon
    @DonDatSanID BIGINT,
    @TaiKhoanID INT = NULL,
    @LyDo NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;

    DECLARE @Old VARCHAR(30), @Pay VARCHAR(30), @KH INT;
    SELECT @Old=TrangThaiDon,@Pay=TrangThaiThanhToan,@KH=KhachHangID
    FROM dbo.DonDatSan WITH (UPDLOCK) WHERE DonDatSanID=@DonDatSanID;

    IF @Old IN ('HOAN_THANH','DA_HUY')
    BEGIN
        ROLLBACK TRAN; RAISERROR(N'Đơn đã hoàn thành hoặc đã hủy, không thể hủy tiếp.',16,1); RETURN;
    END

    UPDATE dbo.DonDatSan
    SET TrangThaiDon='DA_HUY',LyDoHuy=@LyDo,NgayCapNhat=SYSDATETIME()
    WHERE DonDatSanID=@DonDatSanID;

    -- Return only the voucher that was actually consumed by this order.
    DECLARE @VoucherIDDaDung INT;
    SELECT TOP 1 @VoucherIDDaDung=VoucherID FROM dbo.VoucherKhachHang WHERE DonDatSanID=@DonDatSanID;

    IF @VoucherIDDaDung IS NOT NULL
    BEGIN
        UPDATE dbo.Voucher
        SET SoLuongDaDung = CASE WHEN SoLuongDaDung>0 THEN SoLuongDaDung-1 ELSE 0 END
        WHERE VoucherID=@VoucherIDDaDung;

        UPDATE dbo.VoucherKhachHang
        SET DaSuDung=0,NgaySuDung=NULL,DonDatSanID=NULL
        WHERE DonDatSanID=@DonDatSanID;
    END

    INSERT INTO dbo.LichSuTrangThaiDon(DonDatSanID,TrangThaiCu,TrangThaiMoi,TrangThaiThanhToanCu,TrangThaiThanhToanMoi,TaiKhoanThucHienID,LyDo)
    VALUES(@DonDatSanID,@Old,'DA_HUY',@Pay,@Pay,@TaiKhoanID,@LyDo);

    COMMIT TRAN;
END
GO

/* Procedure: dbo.sp_DoiLich */
CREATE PROCEDURE dbo.sp_DoiLich
    @DonDatSanID BIGINT,
    @SanMoiID INT,
    @BatDauMoi DATETIME2(0),
    @KetThucMoi DATETIME2(0),
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @Status VARCHAR(30), @OldPay VARCHAR(30), @TenSan NVARCHAR(150), @TenLoai NVARCHAR(120), @Active BIT, @VanHanh VARCHAR(30);
        SELECT @Status=TrangThaiDon,@OldPay=TrangThaiThanhToan FROM dbo.DonDatSan WITH (UPDLOCK,HOLDLOCK) WHERE DonDatSanID=@DonDatSanID;
        IF @Status NOT IN ('CHO_XAC_NHAN','DA_XAC_NHAN') RAISERROR(N'Chỉ đơn chưa sử dụng mới được đổi lịch.',16,1);
        IF @KetThucMoi<=@BatDauMoi RAISERROR(N'Thời gian mới không hợp lệ.',16,1);

        SELECT @TenSan=s.TenSan,@TenLoai=ls.TenLoaiSan,@Active=s.DangHoatDong,@VanHanh=s.TrangThaiVanHanh
        FROM dbo.San s WITH (UPDLOCK,HOLDLOCK)
        JOIN dbo.LoaiSan ls ON ls.LoaiSanID=s.LoaiSanID
        WHERE s.SanID=@SanMoiID;

        IF @TenSan IS NULL OR @Active=0 OR @VanHanh<>'SAN_SANG' RAISERROR(N'Sân mới không khả dụng.',16,1);

        IF EXISTS
        (
            SELECT 1 FROM dbo.ChiTietDatSan ct WITH (UPDLOCK,HOLDLOCK)
            JOIN dbo.DonDatSan d WITH (UPDLOCK,HOLDLOCK) ON d.DonDatSanID=ct.DonDatSanID
            WHERE ct.SanID=@SanMoiID AND d.DonDatSanID<>@DonDatSanID
              AND d.TrangThaiDon IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_SU_DUNG')
              AND @BatDauMoi<ct.KetThuc AND @KetThucMoi>ct.BatDau
        ) RAISERROR(N'Khung giờ mới bị trùng.',16,1);

        IF EXISTS
        (
            SELECT 1 FROM dbo.LichBaoTriSan
            WHERE SanID=@SanMoiID AND TrangThai IN ('KE_HOACH','DANG_BAO_TRI')
              AND @BatDauMoi<DenThoiGian AND @KetThucMoi>TuThoiGian
        ) RAISERROR(N'Sân mới đang bảo trì trong khung giờ này.',16,1);

        DECLARE @TienMoi DECIMAL(18,2)=dbo.fn_TinhTienSan(@SanMoiID,@BatDauMoi,@KetThucMoi);

        UPDATE dbo.ChiTietDatSan
        SET SanID=@SanMoiID,BatDau=@BatDauMoi,KetThuc=@KetThucMoi,SoPhut=DATEDIFF(MINUTE,@BatDauMoi,@KetThucMoi),
            DonGiaSnapshot=dbo.fn_DonGiaSanTaiThoiDiem(@SanMoiID,@BatDauMoi),TienSan=@TienMoi,
            TenSanSnapshot=@TenSan,TenLoaiSanSnapshot=@TenLoai
        WHERE DonDatSanID=@DonDatSanID;

        UPDATE dbo.DonDatSan
        SET TienSan=@TienMoi,
            TongTien=CASE WHEN @TienMoi+TienDichVu-TienKhuyenMai-TienVoucher<0 THEN 0 ELSE @TienMoi+TienDichVu-TienKhuyenMai-TienVoucher END,
            NgayCapNhat=SYSDATETIME()
        WHERE DonDatSanID=@DonDatSanID;

        UPDATE hd
        SET TienSan=d.TienSan,TienDichVu=d.TienDichVu,TienKhuyenMai=d.TienKhuyenMai,TienVoucher=d.TienVoucher,TongTien=d.TongTien
        FROM dbo.HoaDon hd JOIN dbo.DonDatSan d ON d.DonDatSanID=hd.DonDatSanID
        WHERE hd.DonDatSanID=@DonDatSanID;

        INSERT INTO dbo.LichSuTrangThaiDon(DonDatSanID,TrangThaiCu,TrangThaiMoi,TrangThaiThanhToanCu,TrangThaiThanhToanMoi,TaiKhoanThucHienID,LyDo)
        VALUES(@DonDatSanID,@Status,@Status,@OldPay,@OldPay,@TaiKhoanID,N'Đổi lịch/sân đặt');

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE(); RAISERROR(N'%s',16,1,@Err);
    END CATCH
END
GO

/* Procedure: dbo.sp_TinhLaiTongTienDon */
CREATE PROCEDURE dbo.sp_TinhLaiTongTienDon
    @DonDatSanID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DV DECIMAL(18,2)=ISNULL((SELECT SUM(ThanhTien) FROM dbo.ChiTietDichVuDatSan WHERE DonDatSanID=@DonDatSanID),0);

    UPDATE dbo.DonDatSan
    SET TienDichVu=@DV,
        TongTien=CASE WHEN TienSan+@DV-TienKhuyenMai-TienVoucher<0 THEN 0 ELSE TienSan+@DV-TienKhuyenMai-TienVoucher END,
        NgayCapNhat=SYSDATETIME()
    WHERE DonDatSanID=@DonDatSanID;

    UPDATE hd
    SET TienSan=d.TienSan,TienDichVu=d.TienDichVu,TienKhuyenMai=d.TienKhuyenMai,TienVoucher=d.TienVoucher,TongTien=d.TongTien,
        TrangThaiThanhToan=CASE WHEN hd.DaThanhToan<=0 THEN 'CHUA_THANH_TOAN'
                                WHEN hd.DaThanhToan>=d.TongTien THEN 'DA_THANH_TOAN'
                                ELSE 'THANH_TOAN_MOT_PHAN' END
    FROM dbo.HoaDon hd
    JOIN dbo.DonDatSan d ON d.DonDatSanID=hd.DonDatSanID
    WHERE hd.DonDatSanID=@DonDatSanID;
END
GO

/* Procedure: dbo.sp_ThemDichVuVaoDon */
CREATE PROCEDURE dbo.sp_ThemDichVuVaoDon
    @DonDatSanID BIGINT,
    @SanPhamDichVuID INT,
    @SoLuong DECIMAL(18,3),
    @KhoID INT = NULL,
    @TaiKhoanID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @SoLuong<=0 BEGIN RAISERROR(N'Số lượng phải lớn hơn 0.',16,1); RETURN; END

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @Loai VARCHAR(30), @GiaBan DECIMAL(18,2), @GiaThue DECIMAL(18,2), @QuanLyTon BIT, @DonGia DECIMAL(18,2);
        SELECT @Loai=Loai,@GiaBan=GiaBan,@GiaThue=GiaThue,@QuanLyTon=QuanLyTonKho
        FROM dbo.SanPhamDichVu WHERE SanPhamDichVuID=@SanPhamDichVuID AND DangHoatDong=1;
        IF @Loai IS NULL RAISERROR(N'Dịch vụ/sản phẩm không tồn tại.',16,1);

        SET @DonGia=CASE WHEN @Loai='CHO_THUE' THEN ISNULL(@GiaThue,0) ELSE ISNULL(@GiaBan,0) END;

        IF @QuanLyTon=1
        BEGIN
            IF @KhoID IS NULL RAISERROR(N'Sản phẩm quản lý tồn kho, phải chọn kho.',16,1);
            DECLARE @Ton DECIMAL(18,3);
            SELECT @Ton=SoLuongTon FROM dbo.TonKho WITH (UPDLOCK) WHERE KhoID=@KhoID AND SanPhamDichVuID=@SanPhamDichVuID;
            IF @Ton IS NULL OR @Ton<@SoLuong RAISERROR(N'Tồn kho không đủ.',16,1);

            UPDATE dbo.TonKho SET SoLuongTon=SoLuongTon-@SoLuong,NgayCapNhat=SYSDATETIME()
            WHERE KhoID=@KhoID AND SanPhamDichVuID=@SanPhamDichVuID;

            INSERT INTO dbo.GiaoDichKho(KhoID,SanPhamDichVuID,LoaiGiaoDich,SoLuong,SoLuongTruoc,SoLuongSau,ThamChieuLoai,ThamChieuID,TaiKhoanID)
            VALUES(@KhoID,@SanPhamDichVuID,CASE WHEN @Loai='CHO_THUE' THEN 'CHO_THUE' ELSE 'BAN' END,@SoLuong,@Ton,@Ton-@SoLuong,'DON_DAT_SAN',@DonDatSanID,@TaiKhoanID);
        END

        INSERT INTO dbo.ChiTietDichVuDatSan(DonDatSanID,SanPhamDichVuID,SoLuong,DonGiaSnapshot,ThanhTien)
        VALUES(@DonDatSanID,@SanPhamDichVuID,@SoLuong,@DonGia,@DonGia*@SoLuong);

        DECLARE @DV DECIMAL(18,2)=ISNULL((SELECT SUM(ThanhTien) FROM dbo.ChiTietDichVuDatSan WHERE DonDatSanID=@DonDatSanID),0);
        UPDATE dbo.DonDatSan
        SET TienDichVu=@DV,TongTien=CASE WHEN TienSan+@DV-TienKhuyenMai-TienVoucher<0 THEN 0 ELSE TienSan+@DV-TienKhuyenMai-TienVoucher END,
            NgayCapNhat=SYSDATETIME()
        WHERE DonDatSanID=@DonDatSanID;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE(); RAISERROR(N'%s',16,1,@Err);
    END CATCH
END
GO

/* Procedure: dbo.sp_ApDungVoucher */
CREATE PROCEDURE dbo.sp_ApDungVoucher
    @DonDatSanID BIGINT,
    @VoucherKhachHangID BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;

        DECLARE @KH INT,@Base DECIMAL(18,2),@CurrentVoucher DECIMAL(18,2),@Loai VARCHAR(20),@GiaTri DECIMAL(18,2),@Max DECIMAL(18,2),@Min DECIMAL(18,2),@Ngay DATE=CONVERT(DATE,GETDATE());
        SELECT @KH=KhachHangID,@Base=TienSan+TienDichVu-TienKhuyenMai,@CurrentVoucher=TienVoucher FROM dbo.DonDatSan WITH (UPDLOCK) WHERE DonDatSanID=@DonDatSanID;
        IF @KH IS NULL RAISERROR(N'Không tìm thấy đơn.',16,1);
        IF @CurrentVoucher>0 RAISERROR(N'Đơn đã áp dụng voucher.',16,1);

        SELECT @Loai=v.LoaiGiam,@GiaTri=v.GiaTriGiam,@Max=v.GiamToiDa,@Min=v.DonToiThieu
        FROM dbo.VoucherKhachHang vkh WITH (UPDLOCK)
        JOIN dbo.Voucher v ON v.VoucherID=vkh.VoucherID
        WHERE vkh.VoucherKhachHangID=@VoucherKhachHangID AND vkh.KhachHangID=@KH AND vkh.DaSuDung=0
          AND v.DangHoatDong=1 AND @Ngay BETWEEN v.NgayBatDau AND v.NgayKetThuc;

        IF @Loai IS NULL RAISERROR(N'Voucher không hợp lệ.',16,1);
        IF @Base<@Min RAISERROR(N'Đơn chưa đạt giá trị tối thiểu.',16,1);

        DECLARE @Discount DECIMAL(18,2)=CASE WHEN @Loai='PHAN_TRAM' THEN @Base*@GiaTri/100.0 ELSE @GiaTri END;
        IF @Max IS NOT NULL AND @Discount>@Max SET @Discount=@Max;
        IF @Discount>@Base SET @Discount=@Base;

        UPDATE dbo.DonDatSan SET TienVoucher=@Discount,TongTien=@Base-@Discount,NgayCapNhat=SYSDATETIME() WHERE DonDatSanID=@DonDatSanID;
        UPDATE dbo.VoucherKhachHang SET DaSuDung=1,NgaySuDung=SYSDATETIME(),DonDatSanID=@DonDatSanID WHERE VoucherKhachHangID=@VoucherKhachHangID;
        UPDATE v SET SoLuongDaDung=SoLuongDaDung+1 FROM dbo.Voucher v JOIN dbo.VoucherKhachHang vkh ON vkh.VoucherID=v.VoucherID WHERE vkh.VoucherKhachHangID=@VoucherKhachHangID;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE(); RAISERROR(N'%s',16,1,@Err);
    END CATCH
END
GO

/* Procedure: dbo.sp_TaoHoaDon */
/* ============================================================================
   16. INVOICE / PAYMENT PROCEDURES
============================================================================ */

CREATE PROCEDURE dbo.sp_TaoHoaDon
    @DonDatSanID BIGINT,
    @NguoiLapID INT = NULL,
    @HoaDonID BIGINT OUTPUT,
    @MaHoaDon VARCHAR(40) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS(SELECT 1 FROM dbo.HoaDon WHERE DonDatSanID=@DonDatSanID)
    BEGIN
        SELECT @HoaDonID=HoaDonID,@MaHoaDon=MaHoaDon FROM dbo.HoaDon WHERE DonDatSanID=@DonDatSanID;
        RETURN;
    END

    DECLARE @KH INT,@San DECIMAL(18,2),@DV DECIMAL(18,2),@KM DECIMAL(18,2),@V DECIMAL(18,2),@Tong DECIMAL(18,2),@DaTT DECIMAL(18,2);
    SELECT @KH=KhachHangID,@San=TienSan,@DV=TienDichVu,@KM=TienKhuyenMai,@V=TienVoucher,@Tong=TongTien,@DaTT=DaThanhToan
    FROM dbo.DonDatSan WHERE DonDatSanID=@DonDatSanID;
    IF @KH IS NULL BEGIN RAISERROR(N'Không tìm thấy đơn đặt sân.',16,1); RETURN; END

    SET @MaHoaDon='HD'+CONVERT(VARCHAR(8),GETDATE(),112)+REPLACE(CONVERT(VARCHAR(8),GETDATE(),108),':','')+RIGHT('0000'+CONVERT(VARCHAR(10),ABS(CHECKSUM(NEWID()))%10000),4);
    WHILE EXISTS(SELECT 1 FROM dbo.HoaDon WHERE MaHoaDon=@MaHoaDon)
        SET @MaHoaDon='HD'+CONVERT(VARCHAR(8),GETDATE(),112)+REPLACE(CONVERT(VARCHAR(8),GETDATE(),108),':','')+RIGHT('0000'+CONVERT(VARCHAR(10),ABS(CHECKSUM(NEWID()))%10000),4);

    INSERT INTO dbo.HoaDon(MaHoaDon,DonDatSanID,KhachHangID,TienSan,TienDichVu,TienKhuyenMai,TienVoucher,TongTien,DaThanhToan,TrangThaiThanhToan,NguoiLapID)
    VALUES(@MaHoaDon,@DonDatSanID,@KH,@San,@DV,@KM,@V,@Tong,@DaTT,
           CASE WHEN @DaTT<=0 THEN 'CHUA_THANH_TOAN' WHEN @DaTT>=@Tong THEN 'DA_THANH_TOAN' ELSE 'THANH_TOAN_MOT_PHAN' END,@NguoiLapID);
    SET @HoaDonID=SCOPE_IDENTITY();
END
GO

/* Procedure: dbo.sp_GhiNhanThanhToan */
CREATE PROCEDURE dbo.sp_GhiNhanThanhToan
    @HoaDonID BIGINT,
    @SoTien DECIMAL(18,2),
    @PhuongThuc VARCHAR(30),
    @NguoiTaoID INT = NULL,
    @MaThamChieu NVARCHAR(150) = NULL,
    @GhiChu NVARCHAR(1000) = NULL,
    @GiaoDichID BIGINT OUTPUT,
    @MaGiaoDich VARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @SoTien<=0 BEGIN RAISERROR(N'Số tiền phải lớn hơn 0.',16,1); RETURN; END
    IF @PhuongThuc NOT IN ('TIEN_MAT','VIETQR','CHUYEN_KHOAN','VI_COC') BEGIN RAISERROR(N'Phương thức thanh toán không hợp lệ.',16,1); RETURN; END

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @Don BIGINT,@ConLai DECIMAL(18,2),@KH INT;
        SELECT @Don=DonDatSanID,@ConLai=ConLai,@KH=KhachHangID FROM dbo.HoaDon WITH (UPDLOCK) WHERE HoaDonID=@HoaDonID;
        IF @Don IS NULL RAISERROR(N'Hóa đơn không tồn tại.',16,1);
        IF @SoTien>@ConLai RAISERROR(N'Số tiền thanh toán vượt quá số tiền còn lại.',16,1);

        DECLARE @Status VARCHAR(30)=CASE WHEN @PhuongThuc IN ('VIETQR','CHUYEN_KHOAN') THEN 'CHO_XAC_NHAN' ELSE 'THANH_CONG' END;
        SET @MaGiaoDich='GD'+CONVERT(VARCHAR(8),GETDATE(),112)+REPLACE(CONVERT(VARCHAR(8),GETDATE(),108),':','')+RIGHT('00000'+CONVERT(VARCHAR(10),ABS(CHECKSUM(NEWID()))%100000),5);
        WHILE EXISTS(SELECT 1 FROM dbo.GiaoDichThanhToan WHERE MaGiaoDich=@MaGiaoDich)
            SET @MaGiaoDich='GD'+CONVERT(VARCHAR(8),GETDATE(),112)+REPLACE(CONVERT(VARCHAR(8),GETDATE(),108),':','')+RIGHT('00000'+CONVERT(VARCHAR(10),ABS(CHECKSUM(NEWID()))%100000),5);

        IF @PhuongThuc='VI_COC'
        BEGIN
            DECLARE @ViID BIGINT,@SoDu DECIMAL(18,2);
            SELECT @ViID=ViID,@SoDu=SoDu FROM dbo.ViKhachHang WITH (UPDLOCK) WHERE KhachHangID=@KH;
            IF @ViID IS NULL OR @SoDu<@SoTien RAISERROR(N'Số dư ví cọc không đủ.',16,1);
            UPDATE dbo.ViKhachHang SET SoDu=SoDu-@SoTien,NgayCapNhat=SYSDATETIME() WHERE ViID=@ViID;
            INSERT INTO dbo.GiaoDichVi(ViID,Loai,SoTien,SoDuTruoc,SoDuSau,DonDatSanID,GhiChu)
            VALUES(@ViID,'TRU',@SoTien,@SoDu,@SoDu-@SoTien,@Don,N'Thanh toán đơn đặt sân');
        END

        INSERT INTO dbo.GiaoDichThanhToan(MaGiaoDich,HoaDonID,DonDatSanID,SoTien,PhuongThuc,TrangThai,MaThamChieu,NguoiTaoID,NguoiXacNhanID,NgayXacNhan,GhiChu)
        VALUES(@MaGiaoDich,@HoaDonID,@Don,@SoTien,@PhuongThuc,@Status,@MaThamChieu,@NguoiTaoID,
               CASE WHEN @Status='THANH_CONG' THEN @NguoiTaoID ELSE NULL END,
               CASE WHEN @Status='THANH_CONG' THEN SYSDATETIME() ELSE NULL END,@GhiChu);
        SET @GiaoDichID=SCOPE_IDENTITY();

        IF @Status='THANH_CONG'
        BEGIN
            UPDATE dbo.HoaDon SET DaThanhToan=DaThanhToan+@SoTien WHERE HoaDonID=@HoaDonID;
            UPDATE dbo.DonDatSan SET DaThanhToan=DaThanhToan+@SoTien WHERE DonDatSanID=@Don;
            UPDATE dbo.KhachHang SET TongChiTieu=TongChiTieu+@SoTien WHERE KhachHangID=@KH;

            UPDATE dbo.HoaDon
            SET TrangThaiThanhToan=CASE WHEN DaThanhToan>=TongTien THEN 'DA_THANH_TOAN' ELSE 'THANH_TOAN_MOT_PHAN' END
            WHERE HoaDonID=@HoaDonID;
            UPDATE dbo.DonDatSan
            SET TrangThaiThanhToan=CASE WHEN DaThanhToan>=TongTien THEN 'DA_THANH_TOAN' ELSE 'THANH_TOAN_MOT_PHAN' END
            WHERE DonDatSanID=@Don;
        END
        ELSE
        BEGIN
            UPDATE dbo.HoaDon SET TrangThaiThanhToan='CHO_XAC_NHAN' WHERE HoaDonID=@HoaDonID AND DaThanhToan<TongTien;
            UPDATE dbo.DonDatSan SET TrangThaiThanhToan='CHO_XAC_NHAN' WHERE DonDatSanID=@Don AND DaThanhToan<TongTien;
        END

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE(); RAISERROR(N'%s',16,1,@Err);
    END CATCH
END
GO

/* Procedure: dbo.sp_TaoYeuCauVietQR */
CREATE PROCEDURE dbo.sp_TaoYeuCauVietQR
    @GiaoDichID BIGINT,
    @TaiKhoanNganHangID INT = NULL,
    @SoPhutHieuLuc INT = 15
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS(SELECT 1 FROM dbo.YeuCauVietQR WHERE GiaoDichID=@GiaoDichID)
    BEGIN
        SELECT * FROM dbo.YeuCauVietQR WHERE GiaoDichID=@GiaoDichID;
        RETURN;
    END

    DECLARE @SoTien DECIMAL(18,2),@MaDon VARCHAR(40),@BankID INT,@BankBIN VARCHAR(20),@STK VARCHAR(50),@ChuTK NVARCHAR(200),@Prefix VARCHAR(30);
    SELECT @SoTien=gd.SoTien,@MaDon=d.MaDon FROM dbo.GiaoDichThanhToan gd JOIN dbo.DonDatSan d ON d.DonDatSanID=gd.DonDatSanID WHERE gd.GiaoDichID=@GiaoDichID AND gd.PhuongThuc='VIETQR';
    IF @SoTien IS NULL BEGIN RAISERROR(N'Giao dịch VietQR không tồn tại.',16,1); RETURN; END

    IF @TaiKhoanNganHangID IS NULL
        SELECT TOP 1 @BankID=TaiKhoanNganHangID,@BankBIN=BankBIN,@STK=SoTaiKhoan,@ChuTK=ChuTaiKhoan,@Prefix=PrefixNoiDung
        FROM dbo.TaiKhoanNganHang WHERE DangHoatDong=1 ORDER BY TaiKhoanNganHangID;
    ELSE
        SELECT @BankID=TaiKhoanNganHangID,@BankBIN=BankBIN,@STK=SoTaiKhoan,@ChuTK=ChuTaiKhoan,@Prefix=PrefixNoiDung
        FROM dbo.TaiKhoanNganHang WHERE TaiKhoanNganHangID=@TaiKhoanNganHangID AND DangHoatDong=1;

    IF @BankID IS NULL BEGIN RAISERROR(N'Chưa cấu hình tài khoản ngân hàng nhận VietQR.',16,1); RETURN; END

    DECLARE @NoiDung NVARCHAR(250)=@Prefix + ' ' + @MaDon;
    INSERT INTO dbo.YeuCauVietQR(GiaoDichID,TaiKhoanNganHangID,BankBIN,SoTaiKhoan,ChuTaiKhoan,SoTien,NoiDung,HetHanLuc)
    VALUES(@GiaoDichID,@BankID,@BankBIN,@STK,@ChuTK,@SoTien,@NoiDung,DATEADD(MINUTE,@SoPhutHieuLuc,SYSDATETIME()));

    SELECT * FROM dbo.YeuCauVietQR WHERE GiaoDichID=@GiaoDichID;
END
GO

/* Procedure: dbo.sp_XacNhanChuyenKhoan */
CREATE PROCEDURE dbo.sp_XacNhanChuyenKhoan
    @GiaoDichID BIGINT,
    @TaiKhoanXacNhanID INT,
    @MaThamChieu NVARCHAR(150)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;

        DECLARE @HD BIGINT,@Don BIGINT,@SoTien DECIMAL(18,2),@Status VARCHAR(30),@KH INT;
        SELECT @HD=HoaDonID,@Don=DonDatSanID,@SoTien=SoTien,@Status=TrangThai
        FROM dbo.GiaoDichThanhToan WITH (UPDLOCK) WHERE GiaoDichID=@GiaoDichID AND PhuongThuc IN ('VIETQR','CHUYEN_KHOAN');
        IF @HD IS NULL RAISERROR(N'Không tìm thấy giao dịch chuyển khoản/VietQR.',16,1);
        IF @Status<>'CHO_XAC_NHAN' RAISERROR(N'Giao dịch không ở trạng thái chờ xác nhận.',16,1);
        SELECT @KH=KhachHangID FROM dbo.HoaDon WHERE HoaDonID=@HD;

        UPDATE dbo.GiaoDichThanhToan
        SET TrangThai='THANH_CONG',NguoiXacNhanID=@TaiKhoanXacNhanID,NgayXacNhan=SYSDATETIME(),MaThamChieu=COALESCE(@MaThamChieu,MaThamChieu)
        WHERE GiaoDichID=@GiaoDichID;

        UPDATE dbo.HoaDon SET DaThanhToan=DaThanhToan+@SoTien WHERE HoaDonID=@HD;
        UPDATE dbo.DonDatSan SET DaThanhToan=DaThanhToan+@SoTien WHERE DonDatSanID=@Don;
        UPDATE dbo.KhachHang SET TongChiTieu=TongChiTieu+@SoTien WHERE KhachHangID=@KH;

        UPDATE dbo.HoaDon
        SET TrangThaiThanhToan=CASE WHEN DaThanhToan>=TongTien THEN 'DA_THANH_TOAN' ELSE 'THANH_TOAN_MOT_PHAN' END
        WHERE HoaDonID=@HD;
        UPDATE dbo.DonDatSan
        SET TrangThaiThanhToan=CASE WHEN DaThanhToan>=TongTien THEN 'DA_THANH_TOAN' ELSE 'THANH_TOAN_MOT_PHAN' END
        WHERE DonDatSanID=@Don;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE(); RAISERROR(N'%s',16,1,@Err);
    END CATCH
END
GO

/* Procedure: dbo.sp_TuChoiChuyenKhoan */
CREATE PROCEDURE dbo.sp_TuChoiChuyenKhoan
    @GiaoDichID BIGINT,
    @TaiKhoanXacNhanID INT,
    @LyDo NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRAN;
    DECLARE @HD BIGINT,@Don BIGINT,@Status VARCHAR(30);
    SELECT @HD=HoaDonID,@Don=DonDatSanID,@Status=TrangThai FROM dbo.GiaoDichThanhToan WITH (UPDLOCK) WHERE GiaoDichID=@GiaoDichID;
    IF @Status<>'CHO_XAC_NHAN'
    BEGIN ROLLBACK TRAN; RAISERROR(N'Giao dịch không ở trạng thái chờ xác nhận.',16,1); RETURN; END
    UPDATE dbo.GiaoDichThanhToan SET TrangThai='TU_CHOI',NguoiXacNhanID=@TaiKhoanXacNhanID,NgayXacNhan=SYSDATETIME(),GhiChu=@LyDo WHERE GiaoDichID=@GiaoDichID;
    UPDATE dbo.HoaDon SET TrangThaiThanhToan=CASE WHEN DaThanhToan<=0 THEN 'CHUA_THANH_TOAN' WHEN DaThanhToan>=TongTien THEN 'DA_THANH_TOAN' ELSE 'THANH_TOAN_MOT_PHAN' END WHERE HoaDonID=@HD;
    UPDATE dbo.DonDatSan SET TrangThaiThanhToan=CASE WHEN DaThanhToan<=0 THEN 'CHUA_THANH_TOAN' WHEN DaThanhToan>=TongTien THEN 'DA_THANH_TOAN' ELSE 'THANH_TOAN_MOT_PHAN' END WHERE DonDatSanID=@Don;
    COMMIT TRAN;
END
GO

/* Procedure: dbo.sp_HoanTien */
CREATE PROCEDURE dbo.sp_HoanTien
    @GiaoDichThanhToanID BIGINT,
    @SoTien DECIMAL(18,2),
    @LyDo NVARCHAR(1000),
    @NguoiDuyetID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @SoTien<=0 BEGIN RAISERROR(N'Số tiền hoàn phải lớn hơn 0.',16,1); RETURN; END

    BEGIN TRY
        BEGIN TRAN;
        DECLARE @Don BIGINT,@HD BIGINT,@Paid DECIMAL(18,2),@KH INT;
        SELECT @Don=DonDatSanID,@HD=HoaDonID,@Paid=SoTien FROM dbo.GiaoDichThanhToan WITH (UPDLOCK)
        WHERE GiaoDichID=@GiaoDichThanhToanID AND TrangThai='THANH_CONG';
        IF @Don IS NULL RAISERROR(N'Giao dịch gốc không hợp lệ.',16,1);
        IF @SoTien>@Paid RAISERROR(N'Số tiền hoàn vượt quá giao dịch gốc.',16,1);
        IF ISNULL((SELECT SUM(SoTien) FROM dbo.HoanTien WHERE GiaoDichThanhToanID=@GiaoDichThanhToanID AND TrangThai='HOAN_TAT'),0)+@SoTien>@Paid
            RAISERROR(N'Tổng tiền hoàn vượt quá giao dịch gốc.',16,1);
        SELECT @KH=KhachHangID FROM dbo.HoaDon WHERE HoaDonID=@HD;

        INSERT INTO dbo.HoanTien(GiaoDichThanhToanID,DonDatSanID,SoTien,LyDo,TrangThai,NguoiDuyetID,NgayHoanTat)
        VALUES(@GiaoDichThanhToanID,@Don,@SoTien,@LyDo,'HOAN_TAT',@NguoiDuyetID,SYSDATETIME());

        UPDATE dbo.HoaDon SET DaThanhToan=CASE WHEN DaThanhToan-@SoTien<0 THEN 0 ELSE DaThanhToan-@SoTien END WHERE HoaDonID=@HD;
        UPDATE dbo.DonDatSan SET DaThanhToan=CASE WHEN DaThanhToan-@SoTien<0 THEN 0 ELSE DaThanhToan-@SoTien END WHERE DonDatSanID=@Don;
        UPDATE dbo.KhachHang SET TongChiTieu=CASE WHEN TongChiTieu-@SoTien<0 THEN 0 ELSE TongChiTieu-@SoTien END WHERE KhachHangID=@KH;

        UPDATE dbo.HoaDon SET TrangThaiThanhToan=CASE WHEN DaThanhToan<=0 THEN 'CHUA_THANH_TOAN' WHEN DaThanhToan>=TongTien THEN 'DA_THANH_TOAN' ELSE 'THANH_TOAN_MOT_PHAN' END WHERE HoaDonID=@HD;
        UPDATE dbo.DonDatSan SET TrangThaiThanhToan=CASE WHEN DaThanhToan<=0 THEN 'CHUA_THANH_TOAN' WHEN DaThanhToan>=TongTien THEN 'DA_THANH_TOAN' ELSE 'THANH_TOAN_MOT_PHAN' END WHERE DonDatSanID=@Don;
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE(); RAISERROR(N'%s',16,1,@Err);
    END CATCH
END
GO

/* Procedure: dbo.sp_NhapKho */
/* ============================================================================
   17. INVENTORY PROCEDURES
============================================================================ */

CREATE PROCEDURE dbo.sp_NhapKho
    @KhoID INT,
    @SanPhamDichVuID INT,
    @SoLuong DECIMAL(18,3),
    @TaiKhoanID INT = NULL,
    @GhiChu NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @SoLuong<=0 BEGIN RAISERROR(N'Số lượng nhập phải lớn hơn 0.',16,1); RETURN; END
    BEGIN TRAN;
    DECLARE @Truoc DECIMAL(18,3)=0;
    SELECT @Truoc=SoLuongTon FROM dbo.TonKho WITH (UPDLOCK) WHERE KhoID=@KhoID AND SanPhamDichVuID=@SanPhamDichVuID;
    IF NOT EXISTS(SELECT 1 FROM dbo.TonKho WHERE KhoID=@KhoID AND SanPhamDichVuID=@SanPhamDichVuID)
        INSERT INTO dbo.TonKho(KhoID,SanPhamDichVuID,SoLuongTon) VALUES(@KhoID,@SanPhamDichVuID,@SoLuong);
    ELSE
        UPDATE dbo.TonKho SET SoLuongTon=SoLuongTon+@SoLuong,NgayCapNhat=SYSDATETIME() WHERE KhoID=@KhoID AND SanPhamDichVuID=@SanPhamDichVuID;
    INSERT INTO dbo.GiaoDichKho(KhoID,SanPhamDichVuID,LoaiGiaoDich,SoLuong,SoLuongTruoc,SoLuongSau,GhiChu,TaiKhoanID)
    VALUES(@KhoID,@SanPhamDichVuID,'NHAP',@SoLuong,@Truoc,@Truoc+@SoLuong,@GhiChu,@TaiKhoanID);
    COMMIT TRAN;
END
GO

/* Procedure: dbo.sp_XuatKho */
CREATE PROCEDURE dbo.sp_XuatKho
    @KhoID INT,
    @SanPhamDichVuID INT,
    @SoLuong DECIMAL(18,3),
    @LoaiGiaoDich VARCHAR(30),
    @TaiKhoanID INT = NULL,
    @GhiChu NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @SoLuong<=0 BEGIN RAISERROR(N'Số lượng xuất phải lớn hơn 0.',16,1); RETURN; END
    IF @LoaiGiaoDich NOT IN ('BAN','CHO_THUE','HU_HONG','DIEU_CHINH') BEGIN RAISERROR(N'Loại xuất kho không hợp lệ.',16,1); RETURN; END
    BEGIN TRAN;
    DECLARE @Truoc DECIMAL(18,3);
    SELECT @Truoc=SoLuongTon FROM dbo.TonKho WITH (UPDLOCK) WHERE KhoID=@KhoID AND SanPhamDichVuID=@SanPhamDichVuID;
    IF @Truoc IS NULL OR @Truoc<@SoLuong BEGIN ROLLBACK TRAN; RAISERROR(N'Tồn kho không đủ.',16,1); RETURN; END
    UPDATE dbo.TonKho SET SoLuongTon=SoLuongTon-@SoLuong,NgayCapNhat=SYSDATETIME() WHERE KhoID=@KhoID AND SanPhamDichVuID=@SanPhamDichVuID;
    INSERT INTO dbo.GiaoDichKho(KhoID,SanPhamDichVuID,LoaiGiaoDich,SoLuong,SoLuongTruoc,SoLuongSau,GhiChu,TaiKhoanID)
    VALUES(@KhoID,@SanPhamDichVuID,@LoaiGiaoDich,@SoLuong,@Truoc,@Truoc-@SoLuong,@GhiChu,@TaiKhoanID);
    COMMIT TRAN;
END
GO

/* Procedure: dbo.sp_HoanTatKiemKeKho */
CREATE PROCEDURE dbo.sp_HoanTatKiemKeKho
    @KiemKeKhoID BIGINT,
    @TaiKhoanID INT = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        DECLARE @KhoID INT,@Status VARCHAR(30);
        SELECT @KhoID=KhoID,@Status=TrangThai FROM dbo.KiemKeKho WITH (UPDLOCK) WHERE KiemKeKhoID=@KiemKeKhoID;
        IF @Status<>'DANG_KIEM' RAISERROR(N'Phiếu kiểm kê không ở trạng thái đang kiểm.',16,1);

        DECLARE @SP INT,@HT DECIMAL(18,3),@TT DECIMAL(18,3);
        DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT SanPhamDichVuID,SoLuongHeThong,SoLuongThucTe FROM dbo.ChiTietKiemKeKho WHERE KiemKeKhoID=@KiemKeKhoID;
        OPEN c; FETCH NEXT FROM c INTO @SP,@HT,@TT;
        WHILE @@FETCH_STATUS=0
        BEGIN
            UPDATE dbo.TonKho SET SoLuongTon=@TT,NgayCapNhat=SYSDATETIME() WHERE KhoID=@KhoID AND SanPhamDichVuID=@SP;
            IF @@ROWCOUNT=0 INSERT INTO dbo.TonKho(KhoID,SanPhamDichVuID,SoLuongTon) VALUES(@KhoID,@SP,@TT);
            INSERT INTO dbo.GiaoDichKho(KhoID,SanPhamDichVuID,LoaiGiaoDich,SoLuong,SoLuongTruoc,SoLuongSau,ThamChieuLoai,ThamChieuID,TaiKhoanID,GhiChu)
            VALUES(@KhoID,@SP,'KIEM_KE',ABS(@TT-@HT),@HT,@TT,'KIEM_KE',@KiemKeKhoID,@TaiKhoanID,N'Điều chỉnh sau kiểm kê');
            FETCH NEXT FROM c INTO @SP,@HT,@TT;
        END
        CLOSE c; DEALLOCATE c;
        UPDATE dbo.KiemKeKho SET TrangThai='HOAN_THANH',NgayHoanTat=SYSDATETIME() WHERE KiemKeKhoID=@KiemKeKhoID;
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('local','c') > -1 CLOSE c;
        IF CURSOR_STATUS('local','c') >= -1 DEALLOCATE c;
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE(); RAISERROR(N'%s',16,1,@Err);
    END CATCH
END
GO

/* Procedure: dbo.sp_MoCa */
/* ============================================================================
   18. SHIFT PROCEDURES
============================================================================ */

CREATE PROCEDURE dbo.sp_MoCa
    @NhanVienID INT,
    @CumSanID INT = NULL,
    @TienDauCa DECIMAL(18,2) = 0,
    @CaLamViecID BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS(SELECT 1 FROM dbo.CaLamViec WHERE NhanVienID=@NhanVienID AND TrangThai='DANG_MO')
    BEGIN RAISERROR(N'Nhân viên đang có một ca chưa đóng.',16,1); RETURN; END
    INSERT INTO dbo.CaLamViec(NhanVienID,CumSanID,ThoiGianMoCa,TienDauCa)
    VALUES(@NhanVienID,@CumSanID,SYSDATETIME(),@TienDauCa);
    SET @CaLamViecID=SCOPE_IDENTITY();
END
GO

/* Procedure: dbo.sp_DongCa */
CREATE PROCEDURE dbo.sp_DongCa
    @CaLamViecID BIGINT,
    @TienMatKiemDem DECIMAL(18,2),
    @NguoiNhanID INT = NULL,
    @GhiChu NVARCHAR(1000)=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRAN;
    DECLARE @NV INT,@Start DATETIME2(0),@Status VARCHAR(20),@TaiKhoanID INT;
    SELECT @NV=NhanVienID,@Start=ThoiGianMoCa,@Status=TrangThai FROM dbo.CaLamViec WITH (UPDLOCK) WHERE CaLamViecID=@CaLamViecID;
    IF @Status<>'DANG_MO' BEGIN ROLLBACK TRAN; RAISERROR(N'Ca làm đã đóng hoặc không tồn tại.',16,1); RETURN; END
    SELECT @TaiKhoanID=TaiKhoanID FROM dbo.TaiKhoan WHERE NhanVienID=@NV;

    DECLARE @Cash DECIMAL(18,2)=0,@QR DECIMAL(18,2)=0,@CK DECIMAL(18,2)=0;
    SELECT
      @Cash=ISNULL(SUM(CASE WHEN PhuongThuc='TIEN_MAT' THEN SoTien ELSE 0 END),0),
      @QR=ISNULL(SUM(CASE WHEN PhuongThuc='VIETQR' THEN SoTien ELSE 0 END),0),
      @CK=ISNULL(SUM(CASE WHEN PhuongThuc='CHUYEN_KHOAN' THEN SoTien ELSE 0 END),0)
    FROM dbo.GiaoDichThanhToan
    WHERE TrangThai='THANH_CONG' AND NguoiTaoID=@TaiKhoanID AND NgayTao>=@Start AND NgayTao<=SYSDATETIME();

    UPDATE dbo.CaLamViec
    SET ThoiGianDongCa=SYSDATETIME(),TongThu=@Cash+@QR+@CK,TienMat=@Cash,VietQR=@QR,ChuyenKhoan=@CK,TrangThai='DA_DONG',GhiChu=@GhiChu
    WHERE CaLamViecID=@CaLamViecID;

    INSERT INTO dbo.BanGiaoCa(CaLamViecID,TienMatHeThong,TienMatKiemDem,VietQR,ChuyenKhoan,GhiChu,NguoiBanGiaoID,NguoiNhanID)
    VALUES(@CaLamViecID,@Cash,@TienMatKiemDem,@QR,@CK,@GhiChu,@NV,@NguoiNhanID);
    COMMIT TRAN;
END
GO

/* Procedure: dbo.sp_DashboardAdmin */
/* ============================================================================
   19. DASHBOARD / REPORT PROCEDURES
============================================================================ */

CREATE PROCEDURE dbo.sp_DashboardAdmin
    @Ngay DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Ngay IS NULL SET @Ngay=CONVERT(DATE,GETDATE());

    -- Result set 1: KPI
    SELECT
      ISNULL((SELECT SUM(SoTien) FROM dbo.GiaoDichThanhToan WHERE TrangThai='THANH_CONG' AND CONVERT(DATE,NgayXacNhan)=@Ngay),0) AS DoanhThuHomNay,
      (SELECT COUNT(*) FROM dbo.DonDatSan d JOIN dbo.ChiTietDatSan ct ON ct.DonDatSanID=d.DonDatSanID WHERE CONVERT(DATE,ct.BatDau)=@Ngay AND d.TrangThaiDon<>'DA_HUY') AS SoDonHomNay,
      (SELECT COUNT(*) FROM dbo.vw_TrangThaiSanHienTai WHERE TrangThaiHienTai='DANG_SU_DUNG') AS SanDangSuDung,
      (SELECT COUNT(*) FROM dbo.San WHERE DangHoatDong=1) AS TongSan,
      (SELECT COUNT(*) FROM dbo.HoaDon WHERE ConLai>0) AS HoaDonConNo,
      (SELECT COUNT(*) FROM dbo.GiaoDichThanhToan WHERE TrangThai='CHO_XAC_NHAN') AS GiaoDichChoXacNhan;

    -- Result set 2: today's schedule
    SELECT TOP 10 * FROM dbo.vw_LichSanHomNay ORDER BY BatDau;

    -- Result set 3: last 7 days revenue
    ;WITH D AS
    (
        SELECT DATEADD(DAY,-6,@Ngay) AS Ngay
        UNION ALL SELECT DATEADD(DAY,1,Ngay) FROM D WHERE Ngay<@Ngay
    )
    SELECT D.Ngay,ISNULL(v.DoanhThu,0) AS DoanhThu
    FROM D LEFT JOIN dbo.vw_DoanhThuTheoNgay v ON v.Ngay=D.Ngay
    ORDER BY D.Ngay
    OPTION (MAXRECURSION 10);

    -- Result set 4: field status
    SELECT TrangThaiHienTai,COUNT(*) AS SoLuong FROM dbo.vw_TrangThaiSanHienTai GROUP BY TrangThaiHienTai;
END
GO

/* Procedure: dbo.sp_DashboardNhanVien */
CREATE PROCEDURE dbo.sp_DashboardNhanVien
    @NhanVienID INT,
    @Ngay DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Ngay IS NULL SET @Ngay=CONVERT(DATE,GETDATE());
    DECLARE @TaiKhoanID INT=(SELECT TaiKhoanID FROM dbo.TaiKhoan WHERE NhanVienID=@NhanVienID);
    DECLARE @CaID BIGINT=(SELECT TOP 1 CaLamViecID FROM dbo.CaLamViec WHERE NhanVienID=@NhanVienID AND TrangThai='DANG_MO' ORDER BY ThoiGianMoCa DESC);
    DECLARE @Start DATETIME2(0)=(SELECT ThoiGianMoCa FROM dbo.CaLamViec WHERE CaLamViecID=@CaID);

    SELECT
      (SELECT COUNT(*) FROM dbo.DonDatSan d JOIN dbo.ChiTietDatSan ct ON ct.DonDatSanID=d.DonDatSanID WHERE CONVERT(DATE,ct.BatDau)=@Ngay AND d.TrangThaiDon<>'DA_HUY') AS DatSanHomNay,
      ISNULL((SELECT SUM(SoTien) FROM dbo.GiaoDichThanhToan WHERE TrangThai='THANH_CONG' AND NguoiTaoID=@TaiKhoanID AND (@Start IS NULL OR NgayTao>=@Start)),0) AS DoanhThuCa,
      (SELECT COUNT(*) FROM dbo.KhachHang WHERE CONVERT(DATE,NgayTao)=@Ngay) AS KhachMoiHomNay,
      (SELECT COUNT(*) FROM dbo.GiaoDichThanhToan WHERE TrangThai='CHO_XAC_NHAN') AS CanXuLy;

    SELECT TOP 10 * FROM dbo.vw_LichSanHomNay ORDER BY BatDau;
END
GO

/* Procedure: dbo.sp_ThongKeDoanhThu */
CREATE PROCEDURE dbo.sp_ThongKeDoanhThu
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SET NOCOUNT ON;
    IF @DenNgay<@TuNgay BEGIN RAISERROR(N'Khoảng ngày không hợp lệ.',16,1); RETURN; END

    SELECT
      ISNULL(SUM(gd.SoTien),0) AS TongDoanhThu,
      COUNT(DISTINCT d.DonDatSanID) AS SoDonPhatSinh,
      COUNT(DISTINCT gd.GiaoDichID) AS SoGiaoDichThanhCong,
      COUNT(DISTINCT d.KhachHangID) AS SoKhachHang
    FROM dbo.GiaoDichThanhToan gd
    JOIN dbo.DonDatSan d ON d.DonDatSanID=gd.DonDatSanID
    WHERE gd.TrangThai='THANH_CONG' AND CONVERT(DATE,gd.NgayXacNhan) BETWEEN @TuNgay AND @DenNgay;

    SELECT CONVERT(DATE,gd.NgayXacNhan) AS Ngay,SUM(gd.SoTien) AS DoanhThu
    FROM dbo.GiaoDichThanhToan gd
    WHERE gd.TrangThai='THANH_CONG' AND CONVERT(DATE,gd.NgayXacNhan) BETWEEN @TuNgay AND @DenNgay
    GROUP BY CONVERT(DATE,gd.NgayXacNhan)
    ORDER BY Ngay;

    SELECT ls.TenLoaiSan,COUNT(DISTINCT d.DonDatSanID) AS LuotDat,SUM(ct.TienSan) AS GiaTriDat
    FROM dbo.DonDatSan d
    JOIN dbo.ChiTietDatSan ct ON ct.DonDatSanID=d.DonDatSanID
    JOIN dbo.San s ON s.SanID=ct.SanID
    JOIN dbo.LoaiSan ls ON ls.LoaiSanID=s.LoaiSanID
    WHERE d.TrangThaiDon<>'DA_HUY' AND CONVERT(DATE,ct.BatDau) BETWEEN @TuNgay AND @DenNgay
    GROUP BY ls.TenLoaiSan
    ORDER BY GiaTriDat DESC;
END
GO

/* Procedure: dbo.sp_LayVoucherKhachHang */
CREATE PROCEDURE dbo.sp_LayVoucherKhachHang
    @KhachHangID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
      vkh.VoucherKhachHangID,
      v.MaVoucher,
      v.TenVoucher,
      v.LoaiGiam,
      v.GiaTriGiam,
      v.GiamToiDa,
      v.DonToiThieu,
      v.NgayBatDau,
      v.NgayKetThuc,
      vkh.DaSuDung,
      vkh.NgaySuDung,
      CASE
        WHEN vkh.DaSuDung=1 THEN 'DA_SU_DUNG'
        WHEN CONVERT(DATE,GETDATE())<v.NgayBatDau THEN 'CHUA_BAT_DAU'
        WHEN CONVERT(DATE,GETDATE())>v.NgayKetThuc THEN 'HET_HAN'
        WHEN v.DangHoatDong=0 THEN 'BI_KHOA'
        ELSE 'KHA_DUNG'
      END AS TrangThai
    FROM dbo.VoucherKhachHang vkh
    JOIN dbo.Voucher v ON v.VoucherID=vkh.VoucherID
    WHERE vkh.KhachHangID=@KhachHangID
    ORDER BY vkh.DaSuDung,v.NgayKetThuc;
END
GO

/* Procedure: dbo.sp_TaoGiuChoOnline */
/* ============================================================================
   20. HELPER PROCEDURES FOR ONLINE HOLD
============================================================================ */

CREATE PROCEDURE dbo.sp_TaoGiuChoOnline
    @KhachHangID INT,
    @SanID INT,
    @BatDau DATETIME2(0),
    @KetThuc DATETIME2(0),
    @SoPhutGiu INT = 10,
    @GiuChoID BIGINT OUTPUT,
    @Token UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRY
        BEGIN TRAN;
        IF EXISTS
        (
            SELECT 1 FROM dbo.ChiTietDatSan ct WITH (UPDLOCK,HOLDLOCK)
            JOIN dbo.DonDatSan d WITH (UPDLOCK,HOLDLOCK) ON d.DonDatSanID=ct.DonDatSanID
            WHERE ct.SanID=@SanID AND d.TrangThaiDon IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_SU_DUNG')
              AND @BatDau<ct.KetThuc AND @KetThuc>ct.BatDau
        ) RAISERROR(N'Sân đã có người đặt trong khung giờ này.',16,1);
        IF EXISTS
        (
            SELECT 1 FROM dbo.GiuChoDatSan WITH (UPDLOCK,HOLDLOCK)
            WHERE SanID=@SanID AND TrangThai='DANG_GIU' AND HetHanLuc>SYSDATETIME()
              AND @BatDau<KetThuc AND @KetThuc>BatDau
        ) RAISERROR(N'Sân đang được giữ chỗ.',16,1);
        SET @Token=NEWID();
        INSERT INTO dbo.GiuChoDatSan(KhachHangID,SanID,BatDau,KetThuc,TokenGiuCho,HetHanLuc)
        VALUES(@KhachHangID,@SanID,@BatDau,@KetThuc,@Token,DATEADD(MINUTE,@SoPhutGiu,SYSDATETIME()));
        SET @GiuChoID=SCOPE_IDENTITY();
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE(); RAISERROR(N'%s',16,1,@Err);
    END CATCH
END
GO

/* Procedure: dbo.sp_DonDepGiuChoHetHan */
CREATE PROCEDURE dbo.sp_DonDepGiuChoHetHan
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.GiuChoDatSan
    SET TrangThai='HET_HAN'
    WHERE TrangThai='DANG_GIU' AND HetHanLuc<=SYSDATETIME();
END
GO

/* Procedure: dbo.sp_DangKyKhachHang */
/* ============================================================================
   22. ACCOUNT / PROFILE / VOUCHER MANAGEMENT PROCEDURES
============================================================================ */

CREATE PROCEDURE dbo.sp_DangKyKhachHang
    @HoTen NVARCHAR(150),
    @SoDienThoai VARCHAR(20),
    @Email VARCHAR(150) = NULL,
    @DiaChi NVARCHAR(300) = NULL,
    @NgaySinh DATE = NULL,
    @TenDangNhap VARCHAR(100),
    @MatKhauHash NVARCHAR(500),
    @KhachHangID INT OUTPUT,
    @TaiKhoanID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS(SELECT 1 FROM dbo.TaiKhoan WHERE TenDangNhap=@TenDangNhap)
    BEGIN RAISERROR(N'Tên đăng nhập đã tồn tại.',16,1); RETURN; END
    IF EXISTS(SELECT 1 FROM dbo.KhachHang WHERE SoDienThoai=@SoDienThoai)
    BEGIN RAISERROR(N'Số điện thoại đã tồn tại.',16,1); RETURN; END
    IF @Email IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.KhachHang WHERE Email=@Email)
    BEGIN RAISERROR(N'Email đã tồn tại.',16,1); RETURN; END

    BEGIN TRY
        BEGIN TRAN;
        DECLARE @Ma VARCHAR(30)='KH'+RIGHT('00000000'+CONVERT(VARCHAR(20),ABS(CHECKSUM(NEWID()))),8);
        WHILE EXISTS(SELECT 1 FROM dbo.KhachHang WHERE MaKhachHang=@Ma)
            SET @Ma='KH'+RIGHT('00000000'+CONVERT(VARCHAR(20),ABS(CHECKSUM(NEWID()))),8);
        DECLARE @HangID INT=(SELECT TOP 1 HangThanhVienID FROM dbo.HangThanhVien WHERE MaHang='THUONG');
        INSERT INTO dbo.KhachHang(MaKhachHang,HoTen,SoDienThoai,Email,DiaChi,NgaySinh,HangThanhVienID)
        VALUES(@Ma,@HoTen,@SoDienThoai,@Email,@DiaChi,@NgaySinh,@HangID);
        SET @KhachHangID=SCOPE_IDENTITY();
        INSERT INTO dbo.ViKhachHang(KhachHangID,SoDu) VALUES(@KhachHangID,0);
        DECLARE @VaiTroID INT=(SELECT VaiTroID FROM dbo.VaiTro WHERE MaVaiTro='KHACH_HANG');
        INSERT INTO dbo.TaiKhoan(TenDangNhap,MatKhauHash,VaiTroID,KhachHangID)
        VALUES(@TenDangNhap,@MatKhauHash,@VaiTroID,@KhachHangID);
        SET @TaiKhoanID=SCOPE_IDENTITY();
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE(); RAISERROR(N'%s',16,1,@Err);
    END CATCH
END
GO

/* Procedure: dbo.sp_TaoNhanVienVaTaiKhoan */
CREATE PROCEDURE dbo.sp_TaoNhanVienVaTaiKhoan
    @HoTen NVARCHAR(150),
    @SoDienThoai VARCHAR(20)=NULL,
    @Email VARCHAR(150)=NULL,
    @ChucVu NVARCHAR(100)=NULL,
    @TenDangNhap VARCHAR(100),
    @MatKhauHash NVARCHAR(500),
    @NhanVienID INT OUTPUT,
    @TaiKhoanID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF EXISTS(SELECT 1 FROM dbo.TaiKhoan WHERE TenDangNhap=@TenDangNhap)
    BEGIN RAISERROR(N'Tên đăng nhập đã tồn tại.',16,1); RETURN; END
    BEGIN TRY
        BEGIN TRAN;
        DECLARE @Ma VARCHAR(30)='NV'+RIGHT('00000000'+CONVERT(VARCHAR(20),ABS(CHECKSUM(NEWID()))),8);
        WHILE EXISTS(SELECT 1 FROM dbo.NhanVien WHERE MaNhanVien=@Ma)
            SET @Ma='NV'+RIGHT('00000000'+CONVERT(VARCHAR(20),ABS(CHECKSUM(NEWID()))),8);
        INSERT INTO dbo.NhanVien(MaNhanVien,HoTen,SoDienThoai,Email,ChucVu,NgayVaoLam)
        VALUES(@Ma,@HoTen,@SoDienThoai,@Email,@ChucVu,CONVERT(DATE,GETDATE()));
        SET @NhanVienID=SCOPE_IDENTITY();
        DECLARE @VaiTroID INT=(SELECT VaiTroID FROM dbo.VaiTro WHERE MaVaiTro='NHAN_VIEN');
        INSERT INTO dbo.TaiKhoan(TenDangNhap,MatKhauHash,VaiTroID,NhanVienID)
        VALUES(@TenDangNhap,@MatKhauHash,@VaiTroID,@NhanVienID);
        SET @TaiKhoanID=SCOPE_IDENTITY();
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRAN;
        DECLARE @Err NVARCHAR(4000)=ERROR_MESSAGE(); RAISERROR(N'%s',16,1,@Err);
    END CATCH
END
GO

/* Procedure: dbo.sp_KhoaMoTaiKhoan */
CREATE PROCEDURE dbo.sp_KhoaMoTaiKhoan
    @TaiKhoanID INT,
    @DangHoatDong BIT,
    @NguoiThucHienID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.TaiKhoan SET DangHoatDong=@DangHoatDong,NgayCapNhat=SYSDATETIME() WHERE TaiKhoanID=@TaiKhoanID;
    INSERT INTO dbo.NhatKyHeThong(TaiKhoanID,Module,HanhDong,BangDuLieu,KhoaDuLieu,DuLieuMoi)
    VALUES(@NguoiThucHienID,'ACCOUNT',CASE WHEN @DangHoatDong=1 THEN 'UNLOCK' ELSE 'LOCK' END,'TaiKhoan',CONVERT(NVARCHAR(50),@TaiKhoanID),CONVERT(NVARCHAR(10),@DangHoatDong));
END
GO

/* Procedure: dbo.sp_DatLaiMatKhau */
CREATE PROCEDURE dbo.sp_DatLaiMatKhau
    @TaiKhoanID INT,
    @MatKhauHashMoi NVARCHAR(500),
    @BatBuocDoiMatKhau BIT=1,
    @NguoiThucHienID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.TaiKhoan
    SET MatKhauHash=@MatKhauHashMoi,BatBuocDoiMatKhau=@BatBuocDoiMatKhau,SoLanSaiMatKhau=0,KhoaDen=NULL,NgayCapNhat=SYSDATETIME()
    WHERE TaiKhoanID=@TaiKhoanID;
    INSERT INTO dbo.NhatKyHeThong(TaiKhoanID,Module,HanhDong,BangDuLieu,KhoaDuLieu)
    VALUES(@NguoiThucHienID,'ACCOUNT','RESET_PASSWORD','TaiKhoan',CONVERT(NVARCHAR(50),@TaiKhoanID));
END
GO

/* Procedure: dbo.sp_DoiMatKhauHash */
CREATE PROCEDURE dbo.sp_DoiMatKhauHash
    @TaiKhoanID INT,
    @MatKhauHashMoi NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.TaiKhoan
    SET MatKhauHash=@MatKhauHashMoi,BatBuocDoiMatKhau=0,SoLanSaiMatKhau=0,KhoaDen=NULL,NgayCapNhat=SYSDATETIME()
    WHERE TaiKhoanID=@TaiKhoanID AND DangHoatDong=1;
END
GO

/* Procedure: dbo.sp_CapVoucherChoKhachHang */
CREATE PROCEDURE dbo.sp_CapVoucherChoKhachHang
    @VoucherID INT,
    @KhachHangID INT,
    @SoLuong INT=1
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @SoLuong<=0 BEGIN RAISERROR(N'Số lượng phải lớn hơn 0.',16,1); RETURN; END
    DECLARE @PhatHanh INT,@DaDung INT,@Limit INT;
    SELECT @PhatHanh=SoLuongPhatHanh,@DaDung=SoLuongDaDung,@Limit=GioiHanMoiKhach FROM dbo.Voucher WHERE VoucherID=@VoucherID AND DangHoatDong=1;
    IF @DaDung IS NULL BEGIN RAISERROR(N'Voucher không tồn tại hoặc đã khóa.',16,1); RETURN; END
    IF @PhatHanh IS NOT NULL AND ((SELECT COUNT(*) FROM dbo.VoucherKhachHang WHERE VoucherID=@VoucherID)+@SoLuong)>@PhatHanh
    BEGIN RAISERROR(N'Vượt quá số lượng voucher phát hành.',16,1); RETURN; END
    IF @Limit IS NOT NULL AND ((SELECT COUNT(*) FROM dbo.VoucherKhachHang WHERE VoucherID=@VoucherID AND KhachHangID=@KhachHangID)+@SoLuong)>@Limit
    BEGIN RAISERROR(N'Vượt quá giới hạn voucher trên mỗi khách.',16,1); RETURN; END
    DECLARE @i INT=0;
    WHILE @i<@SoLuong
    BEGIN
        INSERT INTO dbo.VoucherKhachHang(VoucherID,KhachHangID) VALUES(@VoucherID,@KhachHangID);
        SET @i=@i+1;
    END
END
GO

/* Procedure: dbo.sp_LayQuyenTaiKhoan */
/* ==========================================================================
   28. ULTIMATE STORED PROCEDURES FOR SMALL/MISSING FUNCTIONS
============================================================================ */
CREATE PROCEDURE dbo.sp_LayQuyenTaiKhoan
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MaQuyen FROM dbo.vw_TaiKhoanPhanQuyenHieuLuc WHERE TaiKhoanID=@TaiKhoanID AND ChoPhep=1 ORDER BY MaQuyen;
END
GO

/* Procedure: dbo.sp_GhiNhanDangNhap */
CREATE PROCEDURE dbo.sp_GhiNhanDangNhap
    @TaiKhoanID INT,
    @TenDangNhap VARCHAR(100),
    @ThanhCong BIT,
    @DiaChiIP VARCHAR(64)=NULL,
    @TenMay NVARCHAR(150)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.LichSuDangNhap(TaiKhoanID,TenDangNhap,ThanhCong,DiaChiIP,TenMay)
    VALUES(@TaiKhoanID,@TenDangNhap,@ThanhCong,@DiaChiIP,@TenMay);
    IF @ThanhCong=1 UPDATE dbo.TaiKhoan SET LanDangNhapCuoi=SYSDATETIME(),SoLanSaiMatKhau=0,KhoaDen=NULL WHERE TaiKhoanID=@TaiKhoanID;
END
GO

/* Procedure: dbo.sp_TaoPhienDangNhap */
CREATE PROCEDURE dbo.sp_TaoPhienDangNhap
    @TaiKhoanID INT,
    @MaThietBi VARCHAR(100)=NULL,
    @TenMay NVARCHAR(150)=NULL,
    @DiaChiIP VARCHAR(64)=NULL,
    @SoPhut INT=480
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @id UNIQUEIDENTIFIER=NEWID();
    INSERT dbo.PhienDangNhap(PhienDangNhapID,TaiKhoanID,MaThietBi,TenMay,DiaChiIP,HetHan)
    VALUES(@id,@TaiKhoanID,@MaThietBi,@TenMay,@DiaChiIP,DATEADD(MINUTE,@SoPhut,SYSDATETIME()));
    SELECT @id AS PhienDangNhapID;
END
GO

/* Procedure: dbo.sp_DongPhienDangNhap */
CREATE PROCEDURE dbo.sp_DongPhienDangNhap
    @PhienDangNhapID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PhienDangNhap SET KetThuc=SYSDATETIME(),ConHieuLuc=0 WHERE PhienDangNhapID=@PhienDangNhapID;
END
GO

/* Procedure: dbo.sp_CapQuyenTaiKhoan */
CREATE PROCEDURE dbo.sp_CapQuyenTaiKhoan
    @TaiKhoanID INT,
    @MaQuyen VARCHAR(80),
    @ChoPhep BIT,
    @NguoiCapID INT=NULL,
    @GhiChu NVARCHAR(500)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @QuyenID INT=(SELECT QuyenID FROM dbo.Quyen WHERE MaQuyen=@MaQuyen);
    IF @QuyenID IS NULL BEGIN RAISERROR(N'Quyền không tồn tại.',16,1); RETURN; END;
    MERGE dbo.TaiKhoanQuyen AS t
    USING (SELECT @TaiKhoanID TaiKhoanID,@QuyenID QuyenID) s
    ON t.TaiKhoanID=s.TaiKhoanID AND t.QuyenID=s.QuyenID
    WHEN MATCHED THEN UPDATE SET ChoPhep=@ChoPhep,NguoiCapID=@NguoiCapID,NgayCap=SYSDATETIME(),GhiChu=@GhiChu
    WHEN NOT MATCHED THEN INSERT(TaiKhoanID,QuyenID,ChoPhep,NguoiCapID,GhiChu) VALUES(@TaiKhoanID,@QuyenID,@ChoPhep,@NguoiCapID,@GhiChu);
END
GO

/* Procedure: dbo.sp_CongTruDiemKhachHang */
CREATE PROCEDURE dbo.sp_CongTruDiemKhachHang
    @KhachHangID INT,
    @Loai VARCHAR(20),
    @SoDiem DECIMAL(18,2),
    @DonDatSanID BIGINT=NULL,
    @TaiKhoanID INT=NULL,
    @MoTa NVARCHAR(500)=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRAN;
    DECLARE @Truoc DECIMAL(18,2);
    SELECT @Truoc=DiemTichLuy FROM dbo.KhachHang WITH (UPDLOCK,HOLDLOCK) WHERE KhachHangID=@KhachHangID;
    IF @Truoc IS NULL BEGIN ROLLBACK; RAISERROR(N'Khách hàng không tồn tại.',16,1); RETURN; END;
    DECLARE @Sau DECIMAL(18,2)=CASE WHEN @Loai='CONG' THEN @Truoc+@SoDiem ELSE @Truoc-@SoDiem END;
    IF @Sau<0 BEGIN ROLLBACK; RAISERROR(N'Điểm tích lũy không đủ.',16,1); RETURN; END;
    UPDATE dbo.KhachHang SET DiemTichLuy=@Sau,NgayCapNhat=SYSDATETIME() WHERE KhachHangID=@KhachHangID;
    INSERT dbo.GiaoDichDiemTichLuy(KhachHangID,DonDatSanID,LoaiGiaoDich,SoDiem,DiemTruoc,DiemSau,MoTa,NguoiThucHienID)
    VALUES(@KhachHangID,@DonDatSanID,@Loai,@SoDiem,@Truoc,@Sau,@MoTa,@TaiKhoanID);
    COMMIT;
END
GO

/* Procedure: dbo.sp_GanHangThanhVien */
CREATE PROCEDURE dbo.sp_GanHangThanhVien
    @KhachHangID INT,
    @HangThanhVienID INT,
    @TaiKhoanID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.KhachHang SET HangThanhVienID=@HangThanhVienID,NgayCapNhat=SYSDATETIME() WHERE KhachHangID=@KhachHangID;
    IF EXISTS(SELECT 1 FROM dbo.TheThanhVien WHERE KhachHangID=@KhachHangID AND TrangThai='HOAT_DONG')
        UPDATE dbo.TheThanhVien SET HangThanhVienID=@HangThanhVienID WHERE KhachHangID=@KhachHangID AND TrangThai='HOAT_DONG';
    ELSE
        INSERT dbo.TheThanhVien(KhachHangID,MaThe,HangThanhVienID)
        VALUES(@KhachHangID,CONCAT('SF-',FORMAT(GETDATE(),'yyyyMMdd'),'-',@KhachHangID),@HangThanhVienID);
END
GO

/* Procedure: dbo.sp_DoiTrangThaiSan */
CREATE PROCEDURE dbo.sp_DoiTrangThaiSan
    @SanID INT,
    @TrangThaiMoi VARCHAR(30),
    @LyDo NVARCHAR(500)=NULL,
    @TaiKhoanID INT=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @TrangThaiMoi NOT IN ('SAN_SANG','BAO_TRI','NGUNG_HOAT_DONG') BEGIN RAISERROR(N'Trạng thái sân không hợp lệ.',16,1); RETURN; END;
    DECLARE @Cu VARCHAR(30)=(SELECT TrangThaiVanHanh FROM dbo.San WHERE SanID=@SanID);
    IF @Cu IS NULL BEGIN RAISERROR(N'Sân không tồn tại.',16,1); RETURN; END;
    UPDATE dbo.San SET TrangThaiVanHanh=@TrangThaiMoi,NgayCapNhat=SYSDATETIME() WHERE SanID=@SanID;
    INSERT dbo.LichSuTrangThaiSan(SanID,TrangThaiCu,TrangThaiMoi,LyDo,TaiKhoanID) VALUES(@SanID,@Cu,@TrangThaiMoi,@LyDo,@TaiKhoanID);
END
GO

/* Procedure: dbo.sp_CheckInDon */
CREATE PROCEDURE dbo.sp_CheckInDon
    @DonDatSanID BIGINT,
    @TaiKhoanID INT,
    @GhiChu NVARCHAR(500)=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    EXEC dbo.sp_CheckIn @DonDatSanID=@DonDatSanID,@TaiKhoanID=@TaiKhoanID;
    INSERT dbo.SuKienSuDungSan(DonDatSanID,LoaiSuKien,TaiKhoanID,GhiChu) VALUES(@DonDatSanID,'CHECK_IN',@TaiKhoanID,@GhiChu);
END
GO

/* Procedure: dbo.sp_TraSanVaHoanTat */
CREATE PROCEDURE dbo.sp_TraSanVaHoanTat
    @DonDatSanID BIGINT,
    @TaiKhoanID INT,
    @GhiChu NVARCHAR(500)=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    EXEC dbo.sp_HoanTatDon @DonDatSanID=@DonDatSanID,@TaiKhoanID=@TaiKhoanID;
    INSERT dbo.SuKienSuDungSan(DonDatSanID,LoaiSuKien,TaiKhoanID,GhiChu) VALUES(@DonDatSanID,'TRA_SAN',@TaiKhoanID,@GhiChu);
END
GO

/* Procedure: dbo.sp_GhiLichSuDoiLich */
CREATE PROCEDURE dbo.sp_GhiLichSuDoiLich
    @DonDatSanID BIGINT,@SanCuID INT,@SanMoiID INT,@BatDauCu DATETIME2(0),@KetThucCu DATETIME2(0),
    @BatDauMoi DATETIME2(0),@KetThucMoi DATETIME2(0),@ChenhLechTien DECIMAL(18,2)=0,@LyDo NVARCHAR(1000)=NULL,@TaiKhoanID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.LichSuDoiLich(DonDatSanID,SanCuID,SanMoiID,BatDauCu,KetThucCu,BatDauMoi,KetThucMoi,ChenhLechTien,LyDo,TaiKhoanThucHienID)
    VALUES(@DonDatSanID,@SanCuID,@SanMoiID,@BatDauCu,@KetThucCu,@BatDauMoi,@KetThucMoi,@ChenhLechTien,@LyDo,@TaiKhoanID);
END
GO

/* Procedure: dbo.sp_TimChinhSachHoanCoc */
CREATE PROCEDURE dbo.sp_TimChinhSachHoanCoc
    @BatDau DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Phut INT=DATEDIFF(MINUTE,SYSDATETIME(),@BatDau);
    SELECT TOP 1 * FROM dbo.ChinhSachHuyHoanTien
    WHERE DangHoatDong=1 AND @Phut>=SoPhutTruocBatDauTu AND (SoPhutTruocBatDauDen IS NULL OR @Phut<=SoPhutTruocBatDauDen)
    ORDER BY SoPhutTruocBatDauTu DESC;
END
GO

/* Procedure: dbo.sp_TaoPhieuNhapKho */
CREATE PROCEDURE dbo.sp_TaoPhieuNhapKho
    @KhoID INT,@NhaCungCapID INT=NULL,@NguoiNhapID INT=NULL,@GhiChu NVARCHAR(1000)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Ma VARCHAR(50)=CONCAT('PN-',FORMAT(GETDATE(),'yyyyMMddHHmmss'));
    INSERT dbo.PhieuNhapKho(MaPhieuNhap,KhoID,NhaCungCapID,NguoiNhapID,GhiChu) VALUES(@Ma,@KhoID,@NhaCungCapID,@NguoiNhapID,@GhiChu);
    SELECT SCOPE_IDENTITY() AS PhieuNhapKhoID,@Ma AS MaPhieuNhap;
END
GO

/* Procedure: dbo.sp_ThemChiTietPhieuNhapKho */
CREATE PROCEDURE dbo.sp_ThemChiTietPhieuNhapKho
    @PhieuNhapKhoID BIGINT,@SanPhamDichVuID INT,@SoLuong DECIMAL(18,3),@DonGiaNhap DECIMAL(18,2),@HanSuDung DATE=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @KhoID INT=(SELECT KhoID FROM dbo.PhieuNhapKho WHERE PhieuNhapKhoID=@PhieuNhapKhoID AND TrangThai='NHAP');
    IF @KhoID IS NULL BEGIN RAISERROR(N'Phiếu nhập không hợp lệ.',16,1); RETURN; END;
    BEGIN TRAN;
    INSERT dbo.ChiTietPhieuNhapKho(PhieuNhapKhoID,SanPhamDichVuID,SoLuong,DonGiaNhap,HanSuDung)
    VALUES(@PhieuNhapKhoID,@SanPhamDichVuID,@SoLuong,@DonGiaNhap,@HanSuDung);
    EXEC dbo.sp_NhapKho @KhoID=@KhoID,@SanPhamDichVuID=@SanPhamDichVuID,@SoLuong=@SoLuong,@TaiKhoanID=NULL,@GhiChu=N'Nhập theo phiếu nhập kho';
    UPDATE dbo.PhieuNhapKho SET TongTien=(SELECT ISNULL(SUM(ThanhTien),0) FROM dbo.ChiTietPhieuNhapKho WHERE PhieuNhapKhoID=@PhieuNhapKhoID) WHERE PhieuNhapKhoID=@PhieuNhapKhoID;
    COMMIT;
END
GO

/* Procedure: dbo.sp_GhiNhanTraVatDung */
CREATE PROCEDURE dbo.sp_GhiNhanTraVatDung
    @ChiTietDichVuDatSanID BIGINT,@SoLuongTra DECIMAL(18,3),@SoLuongHong DECIMAL(18,3)=0,@SoLuongMat DECIMAL(18,3)=0,
    @TienPhat DECIMAL(18,2)=0,@NguoiNhanID INT=NULL,@GhiChu NVARCHAR(500)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    MERGE dbo.ChiTietThueVatDung t
    USING (SELECT @ChiTietDichVuDatSanID id) s ON t.ChiTietDichVuDatSanID=s.id
    WHEN MATCHED THEN UPDATE SET SoLuongTra=@SoLuongTra,SoLuongHong=@SoLuongHong,SoLuongMat=@SoLuongMat,TienPhat=@TienPhat,ThoiGianTra=SYSDATETIME(),NguoiNhanLaiID=@NguoiNhanID,GhiChu=@GhiChu
    WHEN NOT MATCHED THEN INSERT(ChiTietDichVuDatSanID,SoLuongGiao,SoLuongTra,SoLuongHong,SoLuongMat,TienPhat,ThoiGianTra,NguoiNhanLaiID,GhiChu)
        VALUES(@ChiTietDichVuDatSanID,@SoLuongTra+@SoLuongHong+@SoLuongMat,@SoLuongTra,@SoLuongHong,@SoLuongMat,@TienPhat,SYSDATETIME(),@NguoiNhanID,@GhiChu);
END
GO

/* Procedure: dbo.sp_ImportSaoKeNganHang */
CREATE PROCEDURE dbo.sp_ImportSaoKeNganHang
    @TaiKhoanNganHangID INT,@MaGiaoDichNganHang VARCHAR(150)=NULL,@ThoiGianGiaoDich DATETIME2(0),@SoTien DECIMAL(18,2),
    @NoiDung NVARCHAR(500)=NULL,@TaiKhoanDoiUng NVARCHAR(150)=NULL,@TenDoiUng NVARCHAR(200)=NULL,@DuLieuGoc NVARCHAR(MAX)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @MaGiaoDichNganHang IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.GiaoDichSaoKeNganHang WHERE TaiKhoanNganHangID=@TaiKhoanNganHangID AND MaGiaoDichNganHang=@MaGiaoDichNganHang)
        RETURN;
    INSERT dbo.GiaoDichSaoKeNganHang(TaiKhoanNganHangID,MaGiaoDichNganHang,ThoiGianGiaoDich,SoTien,NoiDung,TaiKhoanDoiUng,TenDoiUng,DuLieuGoc)
    VALUES(@TaiKhoanNganHangID,@MaGiaoDichNganHang,@ThoiGianGiaoDich,@SoTien,@NoiDung,@TaiKhoanDoiUng,@TenDoiUng,@DuLieuGoc);
END
GO

/* Procedure: dbo.sp_KhopSaoKeVoiThanhToan */
CREATE PROCEDURE dbo.sp_KhopSaoKeVoiThanhToan
    @GiaoDichID BIGINT,@SaoKeID BIGINT,@TaiKhoanXacNhanID INT,@TuDong BIT=0
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @SoTienGD DECIMAL(18,2),@SoTienSK DECIMAL(18,2);
    SELECT @SoTienGD=SoTien FROM dbo.GiaoDichThanhToan WHERE GiaoDichID=@GiaoDichID AND TrangThai='CHO_XAC_NHAN';
    SELECT @SoTienSK=SoTien FROM dbo.GiaoDichSaoKeNganHang WHERE SaoKeID=@SaoKeID AND TrangThaiDoiSoat='CHUA_KHOP';
    IF @SoTienGD IS NULL OR @SoTienSK IS NULL BEGIN RAISERROR(N'Giao dịch hoặc sao kê không hợp lệ.',16,1); RETURN; END;
    IF @SoTienGD<>@SoTienSK BEGIN RAISERROR(N'Số tiền giao dịch và sao kê không khớp.',16,1); RETURN; END;
    BEGIN TRAN;
    INSERT dbo.DoiSoatThanhToan(GiaoDichID,SaoKeID,KieuDoiSoat,TrangThai,DoTinCay,NguoiXacNhanID)
    VALUES(@GiaoDichID,@SaoKeID,CASE WHEN @TuDong=1 THEN 'TU_DONG' ELSE 'THU_CONG' END,'DA_KHOP',100,@TaiKhoanXacNhanID);
    UPDATE dbo.GiaoDichSaoKeNganHang SET TrangThaiDoiSoat='DA_KHOP' WHERE SaoKeID=@SaoKeID;
    EXEC dbo.sp_XacNhanChuyenKhoan @GiaoDichID=@GiaoDichID,@TaiKhoanXacNhanID=@TaiKhoanXacNhanID;
    COMMIT;
END
GO

/* Procedure: dbo.sp_GuiTinNhanVaoHangDoi */
CREATE PROCEDURE dbo.sp_GuiTinNhanVaoHangDoi
    @KhachHangID INT=NULL,@TaiKhoanID INT=NULL,@Kenh VARCHAR(20),@DiaChiNhan NVARCHAR(200),@TieuDe NVARCHAR(250)=NULL,@NoiDung NVARCHAR(MAX),@GuiSauLuc DATETIME2(0)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.HangDoiTinNhan(KhachHangID,TaiKhoanID,Kenh,DiaChiNhan,TieuDe,NoiDung,GuiSauLuc)
    VALUES(@KhachHangID,@TaiKhoanID,@Kenh,@DiaChiNhan,@TieuDe,@NoiDung,@GuiSauLuc);
END
GO

/* Procedure: dbo.sp_TaoLenhIoT */
CREATE PROCEDURE dbo.sp_TaoLenhIoT
    @ThietBiID INT,@Lenh VARCHAR(50),@ThamSo NVARCHAR(1000)=NULL,@TaiKhoanID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.LenhDieuKhienIoT(ThietBiID,Lenh,ThamSo,TaiKhoanID) VALUES(@ThietBiID,@Lenh,@ThamSo,@TaiKhoanID);
    SELECT SCOPE_IDENTITY() AS LenhIoTID;
END
GO

/* Procedure: dbo.sp_GhiNhatKyIn */
CREATE PROCEDURE dbo.sp_GhiNhatKyIn
    @LoaiTaiLieu VARCHAR(30),@KhoaDuLieu NVARCHAR(100)=NULL,@TenMayIn NVARCHAR(200)=NULL,@SoBan INT=1,@ThanhCong BIT,@TaiKhoanID INT=NULL,@Loi NVARCHAR(1000)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.NhatKyInAn(LoaiTaiLieu,KhoaDuLieu,TenMayIn,SoBan,ThanhCong,TaiKhoanID,Loi)
    VALUES(@LoaiTaiLieu,@KhoaDuLieu,@TenMayIn,@SoBan,@ThanhCong,@TaiKhoanID,@Loi);
END
GO

/* Procedure: dbo.sp_GhiNhatKyXuatBaoCao */
CREATE PROCEDURE dbo.sp_GhiNhatKyXuatBaoCao
    @LoaiBaoCao VARCHAR(50),@DinhDang VARCHAR(20),@TuNgay DATE=NULL,@DenNgay DATE=NULL,@DuongDanTep NVARCHAR(1000)=NULL,@TaiKhoanID INT=NULL,@ThanhCong BIT=1,@Loi NVARCHAR(1000)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.NhatKyXuatBaoCao(LoaiBaoCao,DinhDang,TuNgay,DenNgay,DuongDanTep,TaiKhoanID,ThanhCong,Loi)
    VALUES(@LoaiBaoCao,@DinhDang,@TuNgay,@DenNgay,@DuongDanTep,@TaiKhoanID,@ThanhCong,@Loi);
END
GO

/* Procedure: dbo.sp_GhiNhatKyCauHinh */
CREATE PROCEDURE dbo.sp_GhiNhatKyCauHinh
    @Khoa VARCHAR(100),@GiaTriMoi NVARCHAR(MAX),@TaiKhoanID INT=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Cu NVARCHAR(MAX)=(SELECT GiaTri FROM dbo.CauHinhHeThong WHERE Khoa=@Khoa);
    IF EXISTS(SELECT 1 FROM dbo.CauHinhHeThong WHERE Khoa=@Khoa)
        UPDATE dbo.CauHinhHeThong SET GiaTri=@GiaTriMoi,NgayCapNhat=SYSDATETIME() WHERE Khoa=@Khoa;
    ELSE
        INSERT dbo.CauHinhHeThong(Khoa,GiaTri) VALUES(@Khoa,@GiaTriMoi);
    INSERT dbo.NhatKyCauHinh(Khoa,GiaTriCu,GiaTriMoi,TaiKhoanID) VALUES(@Khoa,@Cu,@GiaTriMoi,@TaiKhoanID);
END
GO

/* Procedure: dbo.sp_DongBoChiTietHoaDon */
CREATE PROCEDURE dbo.sp_DongBoChiTietHoaDon
    @HoaDonID BIGINT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @DonDatSanID BIGINT=(SELECT DonDatSanID FROM dbo.HoaDon WHERE HoaDonID=@HoaDonID);
    IF @DonDatSanID IS NULL BEGIN RAISERROR(N'Hóa đơn không tồn tại.',16,1); RETURN; END;
    BEGIN TRAN;
    DELETE dbo.ChiTietHoaDon WHERE HoaDonID=@HoaDonID;
    INSERT dbo.ChiTietHoaDon(HoaDonID,LoaiDong,MaThamChieu,MoTa,SoLuong,DonGia,ThanhTien,ThuTu)
    SELECT @HoaDonID,'TIEN_SAN',s.MaSan,CONCAT(N'Tiền sân - ',ct.TenSanSnapshot),CAST(ct.SoPhut AS DECIMAL(18,3))/60.0,
           ct.DonGiaSnapshot,ct.TienSan,ROW_NUMBER() OVER(ORDER BY ct.ChiTietDatSanID)
    FROM dbo.ChiTietDatSan ct JOIN dbo.San s ON s.SanID=ct.SanID WHERE ct.DonDatSanID=@DonDatSanID;

    INSERT dbo.ChiTietHoaDon(HoaDonID,LoaiDong,MaThamChieu,MoTa,SoLuong,DonGia,ThanhTien,ThuTu)
    SELECT @HoaDonID,'DICH_VU',sp.MaSanPhamDichVu,sp.TenSanPhamDichVu,dv.SoLuong,dv.DonGiaSnapshot,dv.ThanhTien,1000+ROW_NUMBER() OVER(ORDER BY dv.ChiTietDichVuDatSanID)
    FROM dbo.ChiTietDichVuDatSan dv JOIN dbo.SanPhamDichVu sp ON sp.SanPhamDichVuID=dv.SanPhamDichVuID
    WHERE dv.DonDatSanID=@DonDatSanID;

    DECLARE @KM DECIMAL(18,2),@VC DECIMAL(18,2);
    SELECT @KM=TienKhuyenMai,@VC=TienVoucher FROM dbo.HoaDon WHERE HoaDonID=@HoaDonID;
    IF ISNULL(@KM,0)>0 INSERT dbo.ChiTietHoaDon(HoaDonID,LoaiDong,MoTa,SoLuong,DonGia,ThanhTien,ThuTu) VALUES(@HoaDonID,'KHUYEN_MAI',N'Giảm khuyến mãi',1,-@KM,-@KM,9001);
    IF ISNULL(@VC,0)>0 INSERT dbo.ChiTietHoaDon(HoaDonID,LoaiDong,MoTa,SoLuong,DonGia,ThanhTien,ThuTu) VALUES(@HoaDonID,'VOUCHER',N'Giảm voucher',1,-@VC,-@VC,9002);
    COMMIT;
END
GO


/* ==================== H. TRIGGERS ==================== */

/* Trigger: dbo.trg_ChiTietDatSan_KhongTrungLich */
/* ============================================================================
   23. HARD SAFETY TRIGGER FOR BOOKING OVERLAP
============================================================================ */

CREATE TRIGGER dbo.trg_ChiTietDatSan_KhongTrungLich
ON dbo.ChiTietDatSan
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        JOIN dbo.DonDatSan di ON di.DonDatSanID=i.DonDatSanID
        JOIN dbo.ChiTietDatSan x ON x.SanID=i.SanID AND x.ChiTietDatSanID<>i.ChiTietDatSanID
        JOIN dbo.DonDatSan dx ON dx.DonDatSanID=x.DonDatSanID
        WHERE di.TrangThaiDon IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_SU_DUNG')
          AND dx.TrangThaiDon IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_SU_DUNG')
          AND i.BatDau<x.KetThuc AND i.KetThuc>x.BatDau
    )
    BEGIN
        RAISERROR(N'Không thể lưu vì lịch sân bị trùng với một đơn đang hoạt động.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        JOIN dbo.LichBaoTriSan bt ON bt.SanID=i.SanID
        WHERE bt.TrangThai IN ('KE_HOACH','DANG_BAO_TRI')
          AND i.BatDau<bt.DenThoiGian AND i.KetThuc>bt.TuThoiGian
    )
    BEGIN
        RAISERROR(N'Không thể lưu vì sân có lịch bảo trì trong khoảng thời gian này.',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

/* =============================================================================
   SPORTFIELD CURRENT WINFORMS - UI COVERAGE EXTENSION
   Bao phủ trực tiếp các màn hiện có:
   ADMIN:
   - Dashboard
   - Sân / Loại sân / Bảng giá / Lịch bảo trì / Ảnh / Tiện ích
   - Khách hàng / đội bóng / VIP / ví cọc / điểm tích lũy / gửi tin nhắn
   - Đặt sân nhanh / Lịch sân / Quản lý đơn / đổi lịch / hủy / hoàn tiền
   - Hóa đơn / tiền mặt / VietQR / chuyển khoản / đối soát
   - Voucher / Khuyến mãi / phát hành voucher / ROI
   - Tài khoản / Nhân viên / phân ca / ma trận quyền
   - Dịch vụ / Kho / nhập-xuất / kiểm kê / thuê-trả vật dụng
   - Đặt cọc online / giữ chỗ / khớp cọc
   - Báo cáo / PDF-Excel audit / Cấu hình / Nhật ký
   STAFF:
   - Dashboard ca / đặt sân tại quầy / check-in / trả sân / thu tiền
   - Lịch sân / khách hàng / sân / loại sân / thống kê ca / hồ sơ / bàn giao ca
   CUSTOMER:
   - Home / sân khả dụng / booking / lịch sử / hóa đơn / voucher / hồ sơ
   - VietQR / hủy đơn / đổi mật khẩu / điểm & hạng thành viên
============================================================================= */
GO

/* AUTH columns đã được tạo trực tiếp trong dbo.TaiKhoan từ V5. */
GO
/* ---------------------------------------------------------------------------
   UI SEARCH
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_TimKiemNhanh
    @TuKhoa NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET @TuKhoa = LTRIM(RTRIM(ISNULL(@TuKhoa,N'')));

    SELECT TOP (15)
        'SAN' AS LoaiKetQua,
        CONVERT(BIGINT,s.SanID) AS ID,
        s.MaSan AS Ma,
        s.TenSan AS TieuDe,
        CONCAT(ls.TenLoaiSan,N' • ',ISNULL(s.ViTri,N'')) AS MoTa
    FROM dbo.San s
    JOIN dbo.LoaiSan ls ON ls.LoaiSanID=s.LoaiSanID
    WHERE @TuKhoa=N''
       OR s.MaSan LIKE '%' + @TuKhoa + '%'
       OR s.TenSan LIKE N'%' + @TuKhoa + N'%'
       OR ls.TenLoaiSan LIKE N'%' + @TuKhoa + N'%'

    UNION ALL

    SELECT TOP (15)
        'KHACH_HANG',
        CONVERT(BIGINT,kh.KhachHangID),
        kh.MaKhachHang,
        kh.HoTen,
        CONCAT(ISNULL(kh.SoDienThoai,''),N' • ',ISNULL(kh.Email,''))
    FROM dbo.KhachHang kh
    WHERE @TuKhoa=N''
       OR kh.MaKhachHang LIKE '%' + @TuKhoa + '%'
       OR kh.HoTen LIKE N'%' + @TuKhoa + N'%'
       OR kh.SoDienThoai LIKE '%' + @TuKhoa + '%'
       OR kh.Email LIKE '%' + @TuKhoa + '%'

    UNION ALL

    SELECT TOP (15)
        'DON_DAT_SAN',
        d.DonDatSanID,
        d.MaDon,
        kh.HoTen,
        CONCAT(d.TrangThaiDon,N' • ',CONVERT(NVARCHAR(20),d.TongTien))
    FROM dbo.DonDatSan d
    JOIN dbo.KhachHang kh ON kh.KhachHangID=d.KhachHangID
    WHERE @TuKhoa=N''
       OR d.MaDon LIKE '%' + @TuKhoa + '%'
       OR kh.HoTen LIKE N'%' + @TuKhoa + N'%'

    UNION ALL

    SELECT TOP (15)
        'HOA_DON',
        h.HoaDonID,
        h.MaHoaDon,
        kh.HoTen,
        CONCAT(h.TrangThaiThanhToan,N' • ',CONVERT(NVARCHAR(20),h.ConLai))
    FROM dbo.HoaDon h
    JOIN dbo.KhachHang kh ON kh.KhachHangID=h.KhachHangID
    WHERE @TuKhoa=N''
       OR h.MaHoaDon LIKE '%' + @TuKhoa + '%'
       OR kh.HoTen LIKE N'%' + @TuKhoa + N'%';
END
GO

/* ---------------------------------------------------------------------------
   AUTH / ACCOUNT
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_ThongTinDangNhap
    @TenDangNhap VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        tk.TaiKhoanID,
        tk.TenDangNhap,
        tk.MatKhauHash,
        tk.MatKhauSalt,
        tk.ThuatToanMatKhau,
        tk.SoVongLapMatKhau,
        tk.DangHoatDong,
        tk.BatBuocDoiMatKhau,
        tk.SoLanSaiMatKhau,
        tk.KhoaDen,
        vt.MaVaiTro,
        vt.TenVaiTro,
        tk.NhanVienID,
        tk.KhachHangID,
        COALESCE(nv.HoTen,kh.HoTen,tk.TenDangNhap) AS HoTen,
        COALESCE(nv.AnhDaiDien,kh.AnhDaiDien) AS AnhDaiDien
    FROM dbo.TaiKhoan tk
    JOIN dbo.VaiTro vt ON vt.VaiTroID=tk.VaiTroID
    LEFT JOIN dbo.NhanVien nv ON nv.NhanVienID=tk.NhanVienID
    LEFT JOIN dbo.KhachHang kh ON kh.KhachHangID=tk.KhachHangID
    WHERE tk.TenDangNhap=@TenDangNhap;
END
GO

CREATE PROCEDURE dbo.sp_UI_CapNhatMatKhau
    @TaiKhoanID INT,
    @MatKhauHash NVARCHAR(500),
    @MatKhauSalt VARBINARY(64) = NULL,
    @ThuatToan VARCHAR(40) = 'PBKDF2-SHA256',
    @SoVongLap INT = NULL,
    @BatBuocDoiMatKhau BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.TaiKhoan
       SET MatKhauHash=@MatKhauHash,
           MatKhauSalt=@MatKhauSalt,
           ThuatToanMatKhau=@ThuatToan,
           SoVongLapMatKhau=@SoVongLap,
           BatBuocDoiMatKhau=@BatBuocDoiMatKhau,
           SoLanSaiMatKhau=0,
           KhoaDen=NULL,
           LanDoiMatKhauCuoi=SYSDATETIME(),
           NgayCapNhat=SYSDATETIME()
     WHERE TaiKhoanID=@TaiKhoanID;

    IF @@ROWCOUNT=0
        THROW 52001, N'Không tìm thấy tài khoản.', 1;
END
GO

CREATE PROCEDURE dbo.sp_UI_DanhSachTaiKhoan
    @TuKhoa NVARCHAR(150)=NULL,
    @MaVaiTro VARCHAR(30)=NULL,
    @DangHoatDong BIT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        tk.TaiKhoanID,tk.TenDangNhap,vt.MaVaiTro,vt.TenVaiTro,
        COALESCE(nv.HoTen,kh.HoTen,N'') AS HoTen,
        COALESCE(nv.SoDienThoai,kh.SoDienThoai,'') AS SoDienThoai,
        tk.DangHoatDong,tk.BatBuocDoiMatKhau,tk.LanDangNhapCuoi,tk.KhoaDen,tk.NgayTao
    FROM dbo.TaiKhoan tk
    JOIN dbo.VaiTro vt ON vt.VaiTroID=tk.VaiTroID
    LEFT JOIN dbo.NhanVien nv ON nv.NhanVienID=tk.NhanVienID
    LEFT JOIN dbo.KhachHang kh ON kh.KhachHangID=tk.KhachHangID
    WHERE (@TuKhoa IS NULL OR @TuKhoa=N''
        OR tk.TenDangNhap LIKE N'%' + @TuKhoa + N'%'
        OR nv.HoTen LIKE N'%' + @TuKhoa + N'%'
        OR kh.HoTen LIKE N'%' + @TuKhoa + N'%')
      AND (@MaVaiTro IS NULL OR vt.MaVaiTro=@MaVaiTro)
      AND (@DangHoatDong IS NULL OR tk.DangHoatDong=@DangHoatDong)
    ORDER BY tk.NgayTao DESC;
END
GO

/* ---------------------------------------------------------------------------
   FIELD TYPES / FIELDS / PRICING / MAINTENANCE / AMENITIES / IMAGES
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_DanhSachLoaiSan
    @TuKhoa NVARCHAR(150)=NULL,
    @ChiDangHoatDong BIT=0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ls.LoaiSanID,ls.MaLoaiSan,ls.TenLoaiSan,ls.MoTa,ls.KichThuoc,ls.MatSan,
        ls.SoNguoi,ls.IconKey,ls.DangHoatDong,
        COUNT(DISTINCT s.SanID) AS SoLuongSan,
        MIN(s.GiaCoBan) AS GiaThapNhat,
        MAX(s.GiaCoBan) AS GiaCaoNhat
    FROM dbo.LoaiSan ls
    LEFT JOIN dbo.San s ON s.LoaiSanID=ls.LoaiSanID AND s.DangHoatDong=1
    WHERE (@TuKhoa IS NULL OR @TuKhoa=N''
        OR ls.MaLoaiSan LIKE '%' + @TuKhoa + '%'
        OR ls.TenLoaiSan LIKE N'%' + @TuKhoa + N'%')
      AND (@ChiDangHoatDong=0 OR ls.DangHoatDong=1)
    GROUP BY ls.LoaiSanID,ls.MaLoaiSan,ls.TenLoaiSan,ls.MoTa,ls.KichThuoc,
             ls.MatSan,ls.SoNguoi,ls.IconKey,ls.DangHoatDong
    ORDER BY ls.TenLoaiSan;
END
GO

CREATE PROCEDURE dbo.sp_UI_LuuLoaiSan
    @LoaiSanID INT=NULL,
    @MaLoaiSan VARCHAR(30),
    @TenLoaiSan NVARCHAR(120),
    @MoTa NVARCHAR(500)=NULL,
    @KichThuoc NVARCHAR(100)=NULL,
    @MatSan NVARCHAR(100)=NULL,
    @SoNguoi INT=NULL,
    @IconKey VARCHAR(100)=NULL,
    @DangHoatDong BIT=1
AS
BEGIN
    SET NOCOUNT ON;

    IF @LoaiSanID IS NULL OR @LoaiSanID=0
    BEGIN
        IF EXISTS(SELECT 1 FROM dbo.LoaiSan WHERE MaLoaiSan=@MaLoaiSan)
            THROW 52002, N'Mã loại sân đã tồn tại.', 1;

        INSERT dbo.LoaiSan(MaLoaiSan,TenLoaiSan,MoTa,KichThuoc,MatSan,SoNguoi,IconKey,DangHoatDong)
        VALUES(@MaLoaiSan,@TenLoaiSan,@MoTa,@KichThuoc,@MatSan,@SoNguoi,@IconKey,@DangHoatDong);
        SELECT CONVERT(INT,SCOPE_IDENTITY()) AS LoaiSanID;
    END
    ELSE
    BEGIN
        UPDATE dbo.LoaiSan
           SET MaLoaiSan=@MaLoaiSan,TenLoaiSan=@TenLoaiSan,MoTa=@MoTa,KichThuoc=@KichThuoc,
               MatSan=@MatSan,SoNguoi=@SoNguoi,IconKey=@IconKey,DangHoatDong=@DangHoatDong
         WHERE LoaiSanID=@LoaiSanID;

        IF @@ROWCOUNT=0 THROW 52003,N'Không tìm thấy loại sân.',1;
        SELECT @LoaiSanID AS LoaiSanID;
    END
END
GO

CREATE PROCEDURE dbo.sp_UI_DanhSachSan
    @TuKhoa NVARCHAR(150)=NULL,
    @LoaiSanID INT=NULL,
    @TrangThaiVanHanh VARCHAR(30)=NULL,
    @CumSanID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.SanID,s.MaSan,s.TenSan,s.LoaiSanID,ls.MaLoaiSan,ls.TenLoaiSan,
        s.CumSanID,cs.TenCumSan,s.ViTri,s.GiaCoBan,s.TrangThaiVanHanh,
        s.MoTa,s.DangHoatDong,s.NgayTao,s.NgayCapNhat,
        (
            SELECT COUNT(*)
            FROM dbo.ChiTietDatSan ct
            JOIN dbo.DonDatSan d ON d.DonDatSanID=ct.DonDatSanID
            WHERE ct.SanID=s.SanID
              AND CAST(ct.BatDau AS DATE)=CAST(GETDATE() AS DATE)
              AND d.TrangThaiDon NOT IN ('DA_HUY')
        ) AS LuotDatHomNay,
        (
            SELECT TOP (1) ha.DuongDanAnh
            FROM dbo.HinhAnhSan ha
            WHERE ha.SanID=s.SanID
            ORDER BY ha.LaAnhChinh DESC,ha.ThuTu,ha.HinhAnhSanID
        ) AS AnhChinh
    FROM dbo.San s
    JOIN dbo.LoaiSan ls ON ls.LoaiSanID=s.LoaiSanID
    JOIN dbo.CumSan cs ON cs.CumSanID=s.CumSanID
    WHERE (@TuKhoa IS NULL OR @TuKhoa=N''
        OR s.MaSan LIKE '%' + @TuKhoa + '%'
        OR s.TenSan LIKE N'%' + @TuKhoa + N'%'
        OR s.ViTri LIKE N'%' + @TuKhoa + N'%')
      AND (@LoaiSanID IS NULL OR s.LoaiSanID=@LoaiSanID)
      AND (@TrangThaiVanHanh IS NULL OR s.TrangThaiVanHanh=@TrangThaiVanHanh)
      AND (@CumSanID IS NULL OR s.CumSanID=@CumSanID)
    ORDER BY s.MaSan;
END
GO

CREATE PROCEDURE dbo.sp_UI_ChiTietSan
    @SanID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.*,ls.MaLoaiSan,ls.TenLoaiSan,cs.MaCumSan,cs.TenCumSan,cs.DiaChi
    FROM dbo.San s
    JOIN dbo.LoaiSan ls ON ls.LoaiSanID=s.LoaiSanID
    JOIN dbo.CumSan cs ON cs.CumSanID=s.CumSanID
    WHERE s.SanID=@SanID;

    SELECT ti.TienIchID,ti.MaTienIch,ti.TenTienIch,ti.IconKey
    FROM dbo.SanTienIch sti
    JOIN dbo.TienIch ti ON ti.TienIchID=sti.TienIchID
    WHERE sti.SanID=@SanID
    ORDER BY ti.TenTienIch;

    SELECT HinhAnhSanID,DuongDanAnh,LaAnhChinh,ThuTu
    FROM dbo.HinhAnhSan
    WHERE SanID=@SanID
    ORDER BY LaAnhChinh DESC,ThuTu,HinhAnhSanID;

    SELECT TOP (50)
        ct.ChiTietDatSanID,d.MaDon,kh.HoTen,ct.BatDau,ct.KetThuc,
        d.TrangThaiDon,d.TrangThaiThanhToan,d.TongTien
    FROM dbo.ChiTietDatSan ct
    JOIN dbo.DonDatSan d ON d.DonDatSanID=ct.DonDatSanID
    JOIN dbo.KhachHang kh ON kh.KhachHangID=d.KhachHangID
    WHERE ct.SanID=@SanID
      AND ct.KetThuc>=DATEADD(DAY,-7,SYSDATETIME())
    ORDER BY ct.BatDau DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_LuuSan
    @SanID INT=NULL,
    @MaSan VARCHAR(30),
    @TenSan NVARCHAR(150),
    @LoaiSanID INT,
    @CumSanID INT,
    @ViTri NVARCHAR(200)=NULL,
    @GiaCoBan DECIMAL(18,2),
    @TrangThaiVanHanh VARCHAR(30)='SAN_SANG',
    @MoTa NVARCHAR(1000)=NULL,
    @DangHoatDong BIT=1
AS
BEGIN
    SET NOCOUNT ON;

    IF @GiaCoBan<0 THROW 52004,N'Giá cơ bản không hợp lệ.',1;

    IF @SanID IS NULL OR @SanID=0
    BEGIN
        IF EXISTS(SELECT 1 FROM dbo.San WHERE MaSan=@MaSan)
            THROW 52005,N'Mã sân đã tồn tại.',1;

        INSERT dbo.San(MaSan,TenSan,LoaiSanID,CumSanID,ViTri,GiaCoBan,TrangThaiVanHanh,MoTa,DangHoatDong)
        VALUES(@MaSan,@TenSan,@LoaiSanID,@CumSanID,@ViTri,@GiaCoBan,@TrangThaiVanHanh,@MoTa,@DangHoatDong);

        SELECT CONVERT(INT,SCOPE_IDENTITY()) AS SanID;
    END
    ELSE
    BEGIN
        UPDATE dbo.San
           SET MaSan=@MaSan,TenSan=@TenSan,LoaiSanID=@LoaiSanID,CumSanID=@CumSanID,
               ViTri=@ViTri,GiaCoBan=@GiaCoBan,TrangThaiVanHanh=@TrangThaiVanHanh,
               MoTa=@MoTa,DangHoatDong=@DangHoatDong,NgayCapNhat=SYSDATETIME()
         WHERE SanID=@SanID;

        IF @@ROWCOUNT=0 THROW 52006,N'Không tìm thấy sân.',1;
        SELECT @SanID AS SanID;
    END
END
GO

CREATE PROCEDURE dbo.sp_UI_LayBangGia
    @LoaiSanID INT=NULL,
    @SanID INT=NULL,
    @Ngay DATE=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        bg.BangGiaID,bg.SanID,s.MaSan,s.TenSan,bg.LoaiSanID,ls.TenLoaiSan,
        bg.TenBangGia,bg.LoaiNgay,bg.ThuTrongTuan,bg.GioBatDau,bg.GioKetThuc,
        bg.DonGia,bg.HieuLucTu,bg.HieuLucDen,bg.ThuTuUuTien,bg.DangHoatDong
    FROM dbo.BangGia bg
    LEFT JOIN dbo.San s ON s.SanID=bg.SanID
    LEFT JOIN dbo.LoaiSan ls ON ls.LoaiSanID=bg.LoaiSanID
    WHERE (@LoaiSanID IS NULL OR bg.LoaiSanID=@LoaiSanID)
      AND (@SanID IS NULL OR bg.SanID=@SanID)
      AND (@Ngay IS NULL
           OR (bg.HieuLucTu IS NULL OR bg.HieuLucTu<=@Ngay)
          AND (bg.HieuLucDen IS NULL OR bg.HieuLucDen>=@Ngay))
    ORDER BY bg.ThuTuUuTien DESC,bg.GioBatDau;
END
GO

CREATE PROCEDURE dbo.sp_UI_LuuBangGia
    @BangGiaID INT=NULL,
    @SanID INT=NULL,
    @LoaiSanID INT=NULL,
    @TenBangGia NVARCHAR(150),
    @LoaiNgay VARCHAR(20),
    @ThuTrongTuan TINYINT=NULL,
    @GioBatDau TIME(0),
    @GioKetThuc TIME(0),
    @DonGia DECIMAL(18,2),
    @HieuLucTu DATE=NULL,
    @HieuLucDen DATE=NULL,
    @ThuTuUuTien INT=0,
    @DangHoatDong BIT=1
AS
BEGIN
    SET NOCOUNT ON;

    IF @GioKetThuc<=@GioBatDau THROW 52007,N'Giờ kết thúc phải lớn hơn giờ bắt đầu.',1;
    IF @DonGia<0 THROW 52008,N'Đơn giá không hợp lệ.',1;
    IF @SanID IS NULL AND @LoaiSanID IS NULL
        THROW 52009,N'Phải chọn sân hoặc loại sân cho bảng giá.',1;

    IF @BangGiaID IS NULL OR @BangGiaID=0
    BEGIN
        INSERT dbo.BangGia
        (SanID,LoaiSanID,TenBangGia,LoaiNgay,ThuTrongTuan,GioBatDau,GioKetThuc,
         DonGia,HieuLucTu,HieuLucDen,ThuTuUuTien,DangHoatDong)
        VALUES
        (@SanID,@LoaiSanID,@TenBangGia,@LoaiNgay,@ThuTrongTuan,@GioBatDau,@GioKetThuc,
         @DonGia,@HieuLucTu,@HieuLucDen,@ThuTuUuTien,@DangHoatDong);
        SELECT CONVERT(INT,SCOPE_IDENTITY()) AS BangGiaID;
    END
    ELSE
    BEGIN
        UPDATE dbo.BangGia
           SET SanID=@SanID,LoaiSanID=@LoaiSanID,TenBangGia=@TenBangGia,LoaiNgay=@LoaiNgay,
               ThuTrongTuan=@ThuTrongTuan,GioBatDau=@GioBatDau,GioKetThuc=@GioKetThuc,
               DonGia=@DonGia,HieuLucTu=@HieuLucTu,HieuLucDen=@HieuLucDen,
               ThuTuUuTien=@ThuTuUuTien,DangHoatDong=@DangHoatDong
         WHERE BangGiaID=@BangGiaID;
        IF @@ROWCOUNT=0 THROW 52010,N'Không tìm thấy bảng giá.',1;
        SELECT @BangGiaID AS BangGiaID;
    END
END
GO

CREATE PROCEDURE dbo.sp_UI_LayLichBaoTriSan
    @SanID INT=NULL,
    @TuNgay DATETIME2(0)=NULL,
    @DenNgay DATETIME2(0)=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT bt.BaoTriID,bt.SanID,s.MaSan,s.TenSan,bt.TuThoiGian,bt.DenThoiGian,
           bt.NoiDung,bt.ChiPhi,bt.TrangThai,bt.NguoiTaoID,bt.NgayTao
    FROM dbo.LichBaoTriSan bt
    JOIN dbo.San s ON s.SanID=bt.SanID
    WHERE (@SanID IS NULL OR bt.SanID=@SanID)
      AND (@TuNgay IS NULL OR bt.DenThoiGian>=@TuNgay)
      AND (@DenNgay IS NULL OR bt.TuThoiGian<=@DenNgay)
    ORDER BY bt.TuThoiGian DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_TaoBaoTriSan
    @SanID INT,
    @TuThoiGian DATETIME2(0),
    @DenThoiGian DATETIME2(0),
    @NoiDung NVARCHAR(1000),
    @ChiPhi DECIMAL(18,2)=0,
    @NguoiTaoID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @DenThoiGian<=@TuThoiGian THROW 52011,N'Thời gian bảo trì không hợp lệ.',1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.ChiTietDatSan ct
        JOIN dbo.DonDatSan d ON d.DonDatSanID=ct.DonDatSanID
        WHERE ct.SanID=@SanID
          AND d.TrangThaiDon NOT IN ('DA_HUY','HOAN_THANH')
          AND @TuThoiGian<ct.KetThuc
          AND @DenThoiGian>ct.BatDau
    )
        THROW 52012,N'Khoảng bảo trì trùng lịch đặt sân chưa hoàn tất.',1;

    BEGIN TRAN;
    INSERT dbo.LichBaoTriSan(SanID,TuThoiGian,DenThoiGian,NoiDung,ChiPhi,TrangThai,NguoiTaoID)
    VALUES(@SanID,@TuThoiGian,@DenThoiGian,@NoiDung,@ChiPhi,'DANG_BAO_TRI',@NguoiTaoID);

    UPDATE dbo.San
       SET TrangThaiVanHanh='BAO_TRI',NgayCapNhat=SYSDATETIME()
     WHERE SanID=@SanID;
    COMMIT TRAN;

    SELECT CONVERT(BIGINT,SCOPE_IDENTITY()) AS BaoTriID;
END
GO

CREATE PROCEDURE dbo.sp_UI_KetThucBaoTriSan
    @BaoTriID BIGINT,
    @TrangThai VARCHAR(30)='HOAN_THANH'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @SanID INT=(SELECT SanID FROM dbo.LichBaoTriSan WHERE BaoTriID=@BaoTriID);
    IF @SanID IS NULL THROW 52013,N'Không tìm thấy lịch bảo trì.',1;

    BEGIN TRAN;
    UPDATE dbo.LichBaoTriSan SET TrangThai=@TrangThai WHERE BaoTriID=@BaoTriID;

    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.LichBaoTriSan
        WHERE SanID=@SanID AND TrangThai='DANG_BAO_TRI' AND BaoTriID<>@BaoTriID
          AND SYSDATETIME()<DenThoiGian
    )
        UPDATE dbo.San
           SET TrangThaiVanHanh='SAN_SANG',NgayCapNhat=SYSDATETIME()
         WHERE SanID=@SanID AND DangHoatDong=1;
    COMMIT TRAN;
END
GO

CREATE PROCEDURE dbo.sp_UI_CapNhatTienIchSan
    @SanID INT,
    @DanhSachTienIchID NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRAN;
    DELETE FROM dbo.SanTienIch WHERE SanID=@SanID;

    DECLARE @xml XML;
    SET @xml = CAST(N'<r><i>' + REPLACE(REPLACE(ISNULL(@DanhSachTienIchID,N''),'&','&amp;'),',',N'</i><i>') + N'</i></r>' AS XML);

    INSERT dbo.SanTienIch(SanID,TienIchID)
    SELECT @SanID,TRY_CONVERT(INT,T.N.value('.','nvarchar(30)'))
    FROM @xml.nodes('/r/i') T(N)
    WHERE TRY_CONVERT(INT,T.N.value('.','nvarchar(30)')) IS NOT NULL
      AND EXISTS(SELECT 1 FROM dbo.TienIch ti WHERE ti.TienIchID=TRY_CONVERT(INT,T.N.value('.','nvarchar(30)')));

    COMMIT TRAN;
END
GO

/* ---------------------------------------------------------------------------
   SCHEDULE / BOOKING LISTS
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_LichSanTheoNgay
    @Ngay DATE,
    @LoaiSanID INT=NULL,
    @CumSanID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.SanID,s.MaSan,s.TenSan,ls.LoaiSanID,ls.TenLoaiSan,cs.TenCumSan,
        ct.ChiTietDatSanID,d.DonDatSanID,d.MaDon,kh.KhachHangID,kh.HoTen,kh.SoDienThoai,
        ct.BatDau,ct.KetThuc,d.TrangThaiDon,d.TrangThaiThanhToan,d.TongTien,d.DaThanhToan,d.ConLai
    FROM dbo.San s
    JOIN dbo.LoaiSan ls ON ls.LoaiSanID=s.LoaiSanID
    JOIN dbo.CumSan cs ON cs.CumSanID=s.CumSanID
    LEFT JOIN dbo.ChiTietDatSan ct
      ON ct.SanID=s.SanID
     AND ct.BatDau<DATEADD(DAY,1,CAST(@Ngay AS DATETIME2))
     AND ct.KetThuc>CAST(@Ngay AS DATETIME2)
    LEFT JOIN dbo.DonDatSan d
      ON d.DonDatSanID=ct.DonDatSanID
     AND d.TrangThaiDon<>'DA_HUY'
    LEFT JOIN dbo.KhachHang kh ON kh.KhachHangID=d.KhachHangID
    WHERE s.DangHoatDong=1
      AND (@LoaiSanID IS NULL OR s.LoaiSanID=@LoaiSanID)
      AND (@CumSanID IS NULL OR s.CumSanID=@CumSanID)
    ORDER BY ls.TenLoaiSan,s.MaSan,ct.BatDau;
END
GO

CREATE PROCEDURE dbo.sp_UI_DanhSachDonDatSan
    @TuNgay DATE=NULL,
    @DenNgay DATE=NULL,
    @TrangThai VARCHAR(30)=NULL,
    @TuKhoa NVARCHAR(150)=NULL,
    @KhachHangID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.DonDatSanID,d.MaDon,d.KhachHangID,kh.MaKhachHang,kh.HoTen,kh.SoDienThoai,
        d.NguonDon,d.TrangThaiDon,d.TrangThaiThanhToan,d.TienSan,d.TienDichVu,
        d.TienKhuyenMai,d.TienVoucher,d.TongTien,d.DaThanhToan,d.ConLai,d.GhiChu,d.NgayTao,
        MIN(ct.BatDau) AS BatDau,MAX(ct.KetThuc) AS KetThuc,
        STUFF((
            SELECT N', ' + s2.TenSan
            FROM dbo.ChiTietDatSan ct2
            JOIN dbo.San s2 ON s2.SanID=ct2.SanID
            WHERE ct2.DonDatSanID=d.DonDatSanID
            ORDER BY ct2.BatDau
            FOR XML PATH(''),TYPE).value('.','nvarchar(max)'),1,2,N'') AS DanhSachSan
    FROM dbo.DonDatSan d
    JOIN dbo.KhachHang kh ON kh.KhachHangID=d.KhachHangID
    LEFT JOIN dbo.ChiTietDatSan ct ON ct.DonDatSanID=d.DonDatSanID
    WHERE (@TuNgay IS NULL OR CAST(d.NgayTao AS DATE)>=@TuNgay)
      AND (@DenNgay IS NULL OR CAST(d.NgayTao AS DATE)<=@DenNgay)
      AND (@TrangThai IS NULL OR d.TrangThaiDon=@TrangThai)
      AND (@KhachHangID IS NULL OR d.KhachHangID=@KhachHangID)
      AND (@TuKhoa IS NULL OR @TuKhoa=N''
        OR d.MaDon LIKE '%' + @TuKhoa + '%'
        OR kh.HoTen LIKE N'%' + @TuKhoa + N'%'
        OR kh.SoDienThoai LIKE '%' + @TuKhoa + '%')
    GROUP BY
        d.DonDatSanID,d.MaDon,d.KhachHangID,kh.MaKhachHang,kh.HoTen,kh.SoDienThoai,
        d.NguonDon,d.TrangThaiDon,d.TrangThaiThanhToan,d.TienSan,d.TienDichVu,
        d.TienKhuyenMai,d.TienVoucher,d.TongTien,d.DaThanhToan,d.ConLai,d.GhiChu,d.NgayTao
    ORDER BY d.NgayTao DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_LichSuTrangThaiDon
    @DonDatSanID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ls.LichSuID,ls.TrangThaiCu,ls.TrangThaiMoi,
        ls.TrangThaiThanhToanCu,ls.TrangThaiThanhToanMoi,
        ls.TaiKhoanThucHienID,tk.TenDangNhap,ls.LyDo,ls.ThoiGian
    FROM dbo.LichSuTrangThaiDon ls
    LEFT JOIN dbo.TaiKhoan tk ON tk.TaiKhoanID=ls.TaiKhoanThucHienID
    WHERE ls.DonDatSanID=@DonDatSanID
    ORDER BY ls.ThoiGian;
END
GO

/* ---------------------------------------------------------------------------
   CUSTOMERS / VIP / TEAM / WALLET / LOYALTY
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_DanhSachKhachHang
    @TuKhoa NVARCHAR(150)=NULL,
    @MaHang VARCHAR(30)=NULL,
    @DangHoatDong BIT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        kh.KhachHangID,kh.MaKhachHang,kh.HoTen,kh.SoDienThoai,kh.Email,kh.DiaChi,
        kh.NgaySinh,htv.MaHang,htv.TenHang,kh.DiemTichLuy,kh.TongChiTieu,
        ISNULL(v.SoDu,0) AS SoDuViCoc,kh.DaXacMinh,kh.DangHoatDong,kh.NgayTao,
        COUNT(DISTINCT d.DonDatSanID) AS TongLuotDat
    FROM dbo.KhachHang kh
    LEFT JOIN dbo.HangThanhVien htv ON htv.HangThanhVienID=kh.HangThanhVienID
    LEFT JOIN dbo.ViKhachHang v ON v.KhachHangID=kh.KhachHangID
    LEFT JOIN dbo.DonDatSan d ON d.KhachHangID=kh.KhachHangID AND d.TrangThaiDon<>'DA_HUY'
    WHERE (@TuKhoa IS NULL OR @TuKhoa=N''
        OR kh.MaKhachHang LIKE '%' + @TuKhoa + '%'
        OR kh.HoTen LIKE N'%' + @TuKhoa + N'%'
        OR kh.SoDienThoai LIKE '%' + @TuKhoa + '%'
        OR kh.Email LIKE '%' + @TuKhoa + '%')
      AND (@MaHang IS NULL OR htv.MaHang=@MaHang)
      AND (@DangHoatDong IS NULL OR kh.DangHoatDong=@DangHoatDong)
    GROUP BY
        kh.KhachHangID,kh.MaKhachHang,kh.HoTen,kh.SoDienThoai,kh.Email,kh.DiaChi,
        kh.NgaySinh,htv.MaHang,htv.TenHang,kh.DiemTichLuy,kh.TongChiTieu,
        v.SoDu,kh.DaXacMinh,kh.DangHoatDong,kh.NgayTao
    ORDER BY kh.NgayTao DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_CapNhatKhachHang
    @KhachHangID INT,
    @HoTen NVARCHAR(150),
    @SoDienThoai VARCHAR(20)=NULL,
    @Email VARCHAR(150)=NULL,
    @DiaChi NVARCHAR(300)=NULL,
    @NgaySinh DATE=NULL,
    @AnhDaiDien NVARCHAR(500)=NULL,
    @DaXacMinh BIT=NULL,
    @DangHoatDong BIT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.KhachHang
       SET HoTen=@HoTen,SoDienThoai=@SoDienThoai,Email=@Email,DiaChi=@DiaChi,
           NgaySinh=@NgaySinh,AnhDaiDien=@AnhDaiDien,
           DaXacMinh=COALESCE(@DaXacMinh,DaXacMinh),
           DangHoatDong=COALESCE(@DangHoatDong,DangHoatDong),
           NgayCapNhat=SYSDATETIME()
     WHERE KhachHangID=@KhachHangID;

    IF @@ROWCOUNT=0 THROW 52014,N'Không tìm thấy khách hàng.',1;
END
GO

CREATE PROCEDURE dbo.sp_UI_CustomerProfile
    @KhachHangID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        kh.*,htv.MaHang,htv.TenHang,htv.TyLeTichDiem,htv.UuDaiMacDinh,
        ISNULL(v.SoDu,0) AS SoDuViCoc,
        COUNT(DISTINCT d.DonDatSanID) AS TongLuotDat,
        ISNULL(SUM(CASE WHEN d.TrangThaiDon='HOAN_THANH' THEN 1 ELSE 0 END),0) AS LuotHoanThanh,
        ISNULL(SUM(CASE WHEN d.TrangThaiDon='DA_HUY' THEN 1 ELSE 0 END),0) AS LuotDaHuy,
        ISNULL(SUM(CAST(ct.SoPhut AS DECIMAL(18,2)))/60.0,0) AS TongGioSan
    FROM dbo.KhachHang kh
    LEFT JOIN dbo.HangThanhVien htv ON htv.HangThanhVienID=kh.HangThanhVienID
    LEFT JOIN dbo.ViKhachHang v ON v.KhachHangID=kh.KhachHangID
    LEFT JOIN dbo.DonDatSan d ON d.KhachHangID=kh.KhachHangID
    LEFT JOIN dbo.ChiTietDatSan ct ON ct.DonDatSanID=d.DonDatSanID
    WHERE kh.KhachHangID=@KhachHangID
    GROUP BY
        kh.KhachHangID,kh.MaKhachHang,kh.HoTen,kh.SoDienThoai,kh.Email,kh.DiaChi,
        kh.NgaySinh,kh.HangThanhVienID,kh.DiemTichLuy,kh.TongChiTieu,kh.AnhDaiDien,
        kh.DaXacMinh,kh.DangHoatDong,kh.NgayTao,kh.NgayCapNhat,
        htv.MaHang,htv.TenHang,htv.TyLeTichDiem,htv.UuDaiMacDinh,v.SoDu;
END
GO

/* ---------------------------------------------------------------------------
   EMPLOYEES / SHIFTS / PERMISSIONS
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_DanhSachNhanVien
    @TuKhoa NVARCHAR(150)=NULL,
    @DangHoatDong BIT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        nv.NhanVienID,nv.MaNhanVien,nv.HoTen,nv.SoDienThoai,nv.Email,nv.ChucVu,
        nv.NgayVaoLam,nv.DiaChi,nv.AnhDaiDien,nv.DangHoatDong,
        tk.TaiKhoanID,tk.TenDangNhap,vt.MaVaiTro,vt.TenVaiTro,tk.LanDangNhapCuoi,
        (
            SELECT TOP (1) lp.NgayLam
            FROM dbo.LichPhanCa lp
            WHERE lp.NhanVienID=nv.NhanVienID AND lp.NgayLam>=CAST(GETDATE() AS DATE)
            ORDER BY lp.NgayLam,lp.GioBatDau
        ) AS NgayCaTiepTheo
    FROM dbo.NhanVien nv
    LEFT JOIN dbo.TaiKhoan tk ON tk.NhanVienID=nv.NhanVienID
    LEFT JOIN dbo.VaiTro vt ON vt.VaiTroID=tk.VaiTroID
    WHERE (@TuKhoa IS NULL OR @TuKhoa=N''
        OR nv.MaNhanVien LIKE '%' + @TuKhoa + '%'
        OR nv.HoTen LIKE N'%' + @TuKhoa + N'%'
        OR nv.SoDienThoai LIKE '%' + @TuKhoa + '%'
        OR nv.Email LIKE '%' + @TuKhoa + '%'
        OR nv.ChucVu LIKE N'%' + @TuKhoa + N'%')
      AND (@DangHoatDong IS NULL OR nv.DangHoatDong=@DangHoatDong)
    ORDER BY nv.HoTen;
END
GO

CREATE PROCEDURE dbo.sp_UI_CapNhatNhanVien
    @NhanVienID INT,
    @HoTen NVARCHAR(150),
    @SoDienThoai VARCHAR(20)=NULL,
    @Email VARCHAR(150)=NULL,
    @ChucVu NVARCHAR(100)=NULL,
    @NgayVaoLam DATE=NULL,
    @DiaChi NVARCHAR(300)=NULL,
    @AnhDaiDien NVARCHAR(500)=NULL,
    @DangHoatDong BIT=1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.NhanVien
       SET HoTen=@HoTen,SoDienThoai=@SoDienThoai,Email=@Email,ChucVu=@ChucVu,
           NgayVaoLam=@NgayVaoLam,DiaChi=@DiaChi,AnhDaiDien=@AnhDaiDien,
           DangHoatDong=@DangHoatDong,NgayCapNhat=SYSDATETIME()
     WHERE NhanVienID=@NhanVienID;

    IF @@ROWCOUNT=0 THROW 52015,N'Không tìm thấy nhân viên.',1;
END
GO

CREATE PROCEDURE dbo.sp_UI_LichPhanCa
    @TuNgay DATE,
    @DenNgay DATE,
    @NhanVienID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT lp.LichPhanCaID,lp.NhanVienID,nv.MaNhanVien,nv.HoTen,
           lp.CumSanID,cs.TenCumSan,lp.NgayLam,lp.GioBatDau,lp.GioKetThuc,
           lp.TrangThai,lp.GhiChu
    FROM dbo.LichPhanCa lp
    JOIN dbo.NhanVien nv ON nv.NhanVienID=lp.NhanVienID
    LEFT JOIN dbo.CumSan cs ON cs.CumSanID=lp.CumSanID
    WHERE lp.NgayLam BETWEEN @TuNgay AND @DenNgay
      AND (@NhanVienID IS NULL OR lp.NhanVienID=@NhanVienID)
    ORDER BY lp.NgayLam,lp.GioBatDau,nv.HoTen;
END
GO

CREATE PROCEDURE dbo.sp_UI_LuuPhanCa
    @LichPhanCaID BIGINT=NULL,
    @NhanVienID INT,
    @CumSanID INT=NULL,
    @NgayLam DATE,
    @GioBatDau TIME(0),
    @GioKetThuc TIME(0),
    @TrangThai VARCHAR(20)='DA_XEP',
    @GhiChu NVARCHAR(500)=NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @GioKetThuc<=@GioBatDau THROW 52016,N'Giờ kết thúc ca không hợp lệ.',1;

    IF EXISTS
    (
        SELECT 1 FROM dbo.LichPhanCa
        WHERE NhanVienID=@NhanVienID AND NgayLam=@NgayLam
          AND (@LichPhanCaID IS NULL OR LichPhanCaID<>@LichPhanCaID)
          AND @GioBatDau<GioKetThuc
          AND @GioKetThuc>GioBatDau
          AND TrangThai<>'HUY'
    )
        THROW 52017,N'Nhân viên đã có ca trùng thời gian.',1;

    IF @LichPhanCaID IS NULL OR @LichPhanCaID=0
    BEGIN
        INSERT dbo.LichPhanCa(NhanVienID,CumSanID,NgayLam,GioBatDau,GioKetThuc,TrangThai,GhiChu)
        VALUES(@NhanVienID,@CumSanID,@NgayLam,@GioBatDau,@GioKetThuc,@TrangThai,@GhiChu);
        SELECT SCOPE_IDENTITY() AS LichPhanCaID;
    END
    ELSE
    BEGIN
        UPDATE dbo.LichPhanCa
           SET NhanVienID=@NhanVienID,CumSanID=@CumSanID,NgayLam=@NgayLam,
               GioBatDau=@GioBatDau,GioKetThuc=@GioKetThuc,TrangThai=@TrangThai,GhiChu=@GhiChu
         WHERE LichPhanCaID=@LichPhanCaID;
        IF @@ROWCOUNT=0 THROW 52018,N'Không tìm thấy lịch phân ca.',1;
        SELECT @LichPhanCaID AS LichPhanCaID;
    END
END
GO

CREATE PROCEDURE dbo.sp_UI_MaTranQuyen
    @VaiTroID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        vt.VaiTroID,vt.MaVaiTro,vt.TenVaiTro,
        q.QuyenID,q.MaQuyen,q.TenQuyen,q.NhomQuyen,
        CONVERT(BIT,CASE WHEN vq.QuyenID IS NULL THEN 0 ELSE 1 END) AS DuocPhep
    FROM dbo.VaiTro vt
    CROSS JOIN dbo.Quyen q
    LEFT JOIN dbo.VaiTroQuyen vq
      ON vq.VaiTroID=vt.VaiTroID AND vq.QuyenID=q.QuyenID
    WHERE (@VaiTroID IS NULL OR vt.VaiTroID=@VaiTroID)
    ORDER BY vt.VaiTroID,q.NhomQuyen,q.MaQuyen;
END
GO

/* ---------------------------------------------------------------------------
   INVOICES / PAYMENTS / VIETQR / BANK RECONCILIATION
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_DanhSachHoaDon
    @TuNgay DATE=NULL,
    @DenNgay DATE=NULL,
    @TrangThai VARCHAR(30)=NULL,
    @TuKhoa NVARCHAR(150)=NULL,
    @KhachHangID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        h.HoaDonID,h.MaHoaDon,h.DonDatSanID,d.MaDon,h.KhachHangID,kh.HoTen,kh.SoDienThoai,
        h.TienSan,h.TienDichVu,h.TienKhuyenMai,h.TienVoucher,h.TongTien,
        h.DaThanhToan,h.ConLai,h.TrangThaiThanhToan,h.NgayLap,h.GhiChu
    FROM dbo.HoaDon h
    JOIN dbo.DonDatSan d ON d.DonDatSanID=h.DonDatSanID
    JOIN dbo.KhachHang kh ON kh.KhachHangID=h.KhachHangID
    WHERE (@TuNgay IS NULL OR CAST(h.NgayLap AS DATE)>=@TuNgay)
      AND (@DenNgay IS NULL OR CAST(h.NgayLap AS DATE)<=@DenNgay)
      AND (@TrangThai IS NULL OR h.TrangThaiThanhToan=@TrangThai)
      AND (@KhachHangID IS NULL OR h.KhachHangID=@KhachHangID)
      AND (@TuKhoa IS NULL OR @TuKhoa=N''
        OR h.MaHoaDon LIKE '%' + @TuKhoa + '%'
        OR d.MaDon LIKE '%' + @TuKhoa + '%'
        OR kh.HoTen LIKE N'%' + @TuKhoa + N'%')
    ORDER BY h.NgayLap DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_LichSuThanhToan
    @HoaDonID BIGINT=NULL,
    @TuNgay DATE=NULL,
    @DenNgay DATE=NULL,
    @TrangThai VARCHAR(30)=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        gd.GiaoDichID,gd.MaGiaoDich,gd.HoaDonID,h.MaHoaDon,gd.DonDatSanID,d.MaDon,
        kh.HoTen,gd.SoTien,gd.PhuongThuc,gd.TrangThai,gd.MaThamChieu,
        gd.NoiDungChuyenKhoan,gd.NgayTao,gd.NgayXacNhan,gd.GhiChu
    FROM dbo.GiaoDichThanhToan gd
    JOIN dbo.HoaDon h ON h.HoaDonID=gd.HoaDonID
    JOIN dbo.DonDatSan d ON d.DonDatSanID=gd.DonDatSanID
    JOIN dbo.KhachHang kh ON kh.KhachHangID=h.KhachHangID
    WHERE (@HoaDonID IS NULL OR gd.HoaDonID=@HoaDonID)
      AND (@TuNgay IS NULL OR CAST(gd.NgayTao AS DATE)>=@TuNgay)
      AND (@DenNgay IS NULL OR CAST(gd.NgayTao AS DATE)<=@DenNgay)
      AND (@TrangThai IS NULL OR gd.TrangThai=@TrangThai)
    ORDER BY gd.NgayTao DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_ThongTinNganHang
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TaiKhoanNganHangID,TenNganHang,BankBIN,SoTaiKhoan,ChuTaiKhoan,
           PrefixNoiDung,DangHoatDong,NgayTao
    FROM dbo.TaiKhoanNganHang
    ORDER BY DangHoatDong DESC,TaiKhoanNganHangID;
END
GO

CREATE PROCEDURE dbo.sp_UI_LuuTaiKhoanNganHang
    @TaiKhoanNganHangID INT=NULL,
    @TenNganHang NVARCHAR(150),
    @BankBIN VARCHAR(20),
    @SoTaiKhoan VARCHAR(50),
    @ChuTaiKhoan NVARCHAR(200),
    @PrefixNoiDung VARCHAR(30)='SPORTFIELD',
    @DangHoatDong BIT=1
AS
BEGIN
    SET NOCOUNT ON;

    IF @TaiKhoanNganHangID IS NULL OR @TaiKhoanNganHangID=0
    BEGIN
        INSERT dbo.TaiKhoanNganHang(TenNganHang,BankBIN,SoTaiKhoan,ChuTaiKhoan,PrefixNoiDung,DangHoatDong)
        VALUES(@TenNganHang,@BankBIN,@SoTaiKhoan,@ChuTaiKhoan,@PrefixNoiDung,@DangHoatDong);
        SELECT CONVERT(INT,SCOPE_IDENTITY()) AS TaiKhoanNganHangID;
    END
    ELSE
    BEGIN
        UPDATE dbo.TaiKhoanNganHang
           SET TenNganHang=@TenNganHang,BankBIN=@BankBIN,SoTaiKhoan=@SoTaiKhoan,
               ChuTaiKhoan=@ChuTaiKhoan,PrefixNoiDung=@PrefixNoiDung,DangHoatDong=@DangHoatDong
         WHERE TaiKhoanNganHangID=@TaiKhoanNganHangID;
        IF @@ROWCOUNT=0 THROW 52019,N'Không tìm thấy tài khoản ngân hàng.',1;
        SELECT @TaiKhoanNganHangID AS TaiKhoanNganHangID;
    END
END
GO

/* ---------------------------------------------------------------------------
   VOUCHERS / PROMOTIONS
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_DanhSachVoucher
    @TuKhoa NVARCHAR(150)=NULL,
    @DangHoatDong BIT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.VoucherID,v.MaVoucher,v.TenVoucher,v.LoaiGiam,v.GiaTriGiam,v.GiamToiDa,
        v.DonToiThieu,v.NgayBatDau,v.NgayKetThuc,v.SoLuongPhatHanh,v.SoLuongDaDung,
        v.GioiHanMoiKhach,v.MoTa,v.DangHoatDong,v.NgayTao,
        ISNULL(x.SoNguoiNhan,0) AS SoNguoiNhan,
        ISNULL(x.SoDaSuDung,0) AS SoDaSuDung
    FROM dbo.Voucher v
    OUTER APPLY
    (
        SELECT COUNT(*) SoNguoiNhan,
               SUM(CASE WHEN vk.DaSuDung=1 THEN 1 ELSE 0 END) SoDaSuDung
        FROM dbo.VoucherKhachHang vk
        WHERE vk.VoucherID=v.VoucherID
    ) x
    WHERE (@TuKhoa IS NULL OR @TuKhoa=N''
        OR v.MaVoucher LIKE '%' + @TuKhoa + '%'
        OR v.TenVoucher LIKE N'%' + @TuKhoa + N'%')
      AND (@DangHoatDong IS NULL OR v.DangHoatDong=@DangHoatDong)
    ORDER BY v.NgayTao DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_LuuVoucher
    @VoucherID INT=NULL,
    @MaVoucher VARCHAR(50),
    @TenVoucher NVARCHAR(150),
    @LoaiGiam VARCHAR(20),
    @GiaTriGiam DECIMAL(18,2),
    @GiamToiDa DECIMAL(18,2)=NULL,
    @DonToiThieu DECIMAL(18,2)=0,
    @NgayBatDau DATETIME2(0),
    @NgayKetThuc DATETIME2(0),
    @SoLuongPhatHanh INT=NULL,
    @GioiHanMoiKhach INT=1,
    @MoTa NVARCHAR(500)=NULL,
    @DangHoatDong BIT=1
AS
BEGIN
    SET NOCOUNT ON;

    IF @NgayKetThuc<=@NgayBatDau THROW 52020,N'Thời gian voucher không hợp lệ.',1;

    IF @VoucherID IS NULL OR @VoucherID=0
    BEGIN
        IF EXISTS(SELECT 1 FROM dbo.Voucher WHERE MaVoucher=@MaVoucher)
            THROW 52021,N'Mã voucher đã tồn tại.',1;

        INSERT dbo.Voucher
        (MaVoucher,TenVoucher,LoaiGiam,GiaTriGiam,GiamToiDa,DonToiThieu,
         NgayBatDau,NgayKetThuc,SoLuongPhatHanh,GioiHanMoiKhach,MoTa,DangHoatDong)
        VALUES
        (@MaVoucher,@TenVoucher,@LoaiGiam,@GiaTriGiam,@GiamToiDa,@DonToiThieu,
         @NgayBatDau,@NgayKetThuc,@SoLuongPhatHanh,@GioiHanMoiKhach,@MoTa,@DangHoatDong);

        SELECT CONVERT(INT,SCOPE_IDENTITY()) AS VoucherID;
    END
    ELSE
    BEGIN
        UPDATE dbo.Voucher
           SET MaVoucher=@MaVoucher,TenVoucher=@TenVoucher,LoaiGiam=@LoaiGiam,
               GiaTriGiam=@GiaTriGiam,GiamToiDa=@GiamToiDa,DonToiThieu=@DonToiThieu,
               NgayBatDau=@NgayBatDau,NgayKetThuc=@NgayKetThuc,
               SoLuongPhatHanh=@SoLuongPhatHanh,GioiHanMoiKhach=@GioiHanMoiKhach,
               MoTa=@MoTa,DangHoatDong=@DangHoatDong
         WHERE VoucherID=@VoucherID;
        IF @@ROWCOUNT=0 THROW 52022,N'Không tìm thấy voucher.',1;
        SELECT @VoucherID AS VoucherID;
    END
END
GO

CREATE PROCEDURE dbo.sp_UI_ThemVoucherVaoViKhach
    @KhachHangID INT,
    @MaVoucher VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @VoucherID INT;
    SELECT @VoucherID=VoucherID
    FROM dbo.Voucher
    WHERE MaVoucher=@MaVoucher
      AND DangHoatDong=1
      AND SYSDATETIME() BETWEEN NgayBatDau AND NgayKetThuc;

    IF @VoucherID IS NULL THROW 52023,N'Voucher không hợp lệ hoặc đã hết hạn.',1;

    DECLARE @GioiHan INT=(SELECT GioiHanMoiKhach FROM dbo.Voucher WHERE VoucherID=@VoucherID);
    IF (SELECT COUNT(*) FROM dbo.VoucherKhachHang WHERE VoucherID=@VoucherID AND KhachHangID=@KhachHangID)>=ISNULL(@GioiHan,1)
        THROW 52024,N'Khách hàng đã đạt giới hạn nhận voucher này.',1;

    INSERT dbo.VoucherKhachHang(VoucherID,KhachHangID)
    VALUES(@VoucherID,@KhachHangID);

    SELECT CONVERT(BIGINT,SCOPE_IDENTITY()) AS VoucherKhachHangID;
END
GO

CREATE PROCEDURE dbo.sp_UI_DanhSachKhuyenMai
    @TuKhoa NVARCHAR(150)=NULL,
    @DangHoatDong BIT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        km.KhuyenMaiID,km.MaKhuyenMai,km.TenKhuyenMai,km.LoaiGiam,km.GiaTriGiam,
        km.GiamToiDa,km.DonToiThieu,km.NgayBatDau,km.NgayKetThuc,
        km.GioBatDau,km.GioKetThuc,km.MoTa,km.DangHoatDong
    FROM dbo.KhuyenMai km
    WHERE (@TuKhoa IS NULL OR @TuKhoa=N''
        OR km.MaKhuyenMai LIKE '%' + @TuKhoa + '%'
        OR km.TenKhuyenMai LIKE N'%' + @TuKhoa + N'%')
      AND (@DangHoatDong IS NULL OR km.DangHoatDong=@DangHoatDong)
    ORDER BY km.NgayBatDau DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_LuuKhuyenMai
    @KhuyenMaiID INT=NULL,
    @MaKhuyenMai VARCHAR(50),
    @TenKhuyenMai NVARCHAR(150),
    @LoaiGiam VARCHAR(20),
    @GiaTriGiam DECIMAL(18,2),
    @GiamToiDa DECIMAL(18,2)=NULL,
    @DonToiThieu DECIMAL(18,2)=0,
    @NgayBatDau DATETIME2(0),
    @NgayKetThuc DATETIME2(0),
    @GioBatDau TIME(0)=NULL,
    @GioKetThuc TIME(0)=NULL,
    @MoTa NVARCHAR(500)=NULL,
    @DangHoatDong BIT=1
AS
BEGIN
    SET NOCOUNT ON;

    IF @NgayKetThuc<=@NgayBatDau THROW 52025,N'Thời gian khuyến mãi không hợp lệ.',1;

    IF @KhuyenMaiID IS NULL OR @KhuyenMaiID=0
    BEGIN
        IF EXISTS(SELECT 1 FROM dbo.KhuyenMai WHERE MaKhuyenMai=@MaKhuyenMai)
            THROW 52026,N'Mã khuyến mãi đã tồn tại.',1;

        INSERT dbo.KhuyenMai
        (MaKhuyenMai,TenKhuyenMai,LoaiGiam,GiaTriGiam,GiamToiDa,DonToiThieu,
         NgayBatDau,NgayKetThuc,GioBatDau,GioKetThuc,MoTa,DangHoatDong)
        VALUES
        (@MaKhuyenMai,@TenKhuyenMai,@LoaiGiam,@GiaTriGiam,@GiamToiDa,@DonToiThieu,
         @NgayBatDau,@NgayKetThuc,@GioBatDau,@GioKetThuc,@MoTa,@DangHoatDong);

        SELECT CONVERT(INT,SCOPE_IDENTITY()) AS KhuyenMaiID;
    END
    ELSE
    BEGIN
        UPDATE dbo.KhuyenMai
           SET MaKhuyenMai=@MaKhuyenMai,TenKhuyenMai=@TenKhuyenMai,LoaiGiam=@LoaiGiam,
               GiaTriGiam=@GiaTriGiam,GiamToiDa=@GiamToiDa,DonToiThieu=@DonToiThieu,
               NgayBatDau=@NgayBatDau,NgayKetThuc=@NgayKetThuc,
               GioBatDau=@GioBatDau,GioKetThuc=@GioKetThuc,
               MoTa=@MoTa,DangHoatDong=@DangHoatDong
         WHERE KhuyenMaiID=@KhuyenMaiID;
        IF @@ROWCOUNT=0 THROW 52027,N'Không tìm thấy khuyến mãi.',1;
        SELECT @KhuyenMaiID AS KhuyenMaiID;
    END
END
GO

/* ---------------------------------------------------------------------------
   SERVICES / INVENTORY
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_DanhSachDichVuTonKho
    @TuKhoa NVARCHAR(150)=NULL,
    @Loai VARCHAR(30)=NULL,
    @KhoID INT=NULL,
    @ChiCanhBao BIT=0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sp.SanPhamDichVuID,sp.MaSanPhamDichVu,sp.TenSanPhamDichVu,sp.Loai,
        sp.DonViTinh,sp.GiaBan,sp.GiaThue,sp.QuanLyTonKho,sp.SoLuongTonToiThieu,
        sp.MoTa,sp.DangHoatDong,
        tk.KhoID,k.MaKho,k.TenKho,ISNULL(tk.SoLuongTon,0) AS SoLuongTon,tk.NgayCapNhat,
        CONVERT(BIT,CASE WHEN sp.QuanLyTonKho=1 AND ISNULL(tk.SoLuongTon,0)<=sp.SoLuongTonToiThieu THEN 1 ELSE 0 END) AS CanhBaoTon
    FROM dbo.SanPhamDichVu sp
    LEFT JOIN dbo.TonKho tk ON tk.SanPhamDichVuID=sp.SanPhamDichVuID
    LEFT JOIN dbo.Kho k ON k.KhoID=tk.KhoID
    WHERE (@TuKhoa IS NULL OR @TuKhoa=N''
        OR sp.MaSanPhamDichVu LIKE '%' + @TuKhoa + '%'
        OR sp.TenSanPhamDichVu LIKE N'%' + @TuKhoa + N'%')
      AND (@Loai IS NULL OR sp.Loai=@Loai)
      AND (@KhoID IS NULL OR tk.KhoID=@KhoID)
      AND (@ChiCanhBao=0 OR (sp.QuanLyTonKho=1 AND ISNULL(tk.SoLuongTon,0)<=sp.SoLuongTonToiThieu))
    ORDER BY sp.Loai,sp.TenSanPhamDichVu,k.TenKho;
END
GO

CREATE PROCEDURE dbo.sp_UI_LichSuKho
    @SanPhamDichVuID INT=NULL,
    @KhoID INT=NULL,
    @TuNgay DATE=NULL,
    @DenNgay DATE=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        gd.GiaoDichKhoID,gd.KhoID,k.MaKho,k.TenKho,gd.SanPhamDichVuID,
        sp.MaSanPhamDichVu,sp.TenSanPhamDichVu,gd.LoaiGiaoDich,gd.SoLuong,
        gd.SoLuongTruoc,gd.SoLuongSau,gd.ThamChieuLoai,gd.ThamChieuID,
        gd.GhiChu,gd.TaiKhoanID,tk.TenDangNhap,gd.NgayTao
    FROM dbo.GiaoDichKho gd
    JOIN dbo.Kho k ON k.KhoID=gd.KhoID
    JOIN dbo.SanPhamDichVu sp ON sp.SanPhamDichVuID=gd.SanPhamDichVuID
    LEFT JOIN dbo.TaiKhoan tk ON tk.TaiKhoanID=gd.TaiKhoanID
    WHERE (@SanPhamDichVuID IS NULL OR gd.SanPhamDichVuID=@SanPhamDichVuID)
      AND (@KhoID IS NULL OR gd.KhoID=@KhoID)
      AND (@TuNgay IS NULL OR CAST(gd.NgayTao AS DATE)>=@TuNgay)
      AND (@DenNgay IS NULL OR CAST(gd.NgayTao AS DATE)<=@DenNgay)
    ORDER BY gd.NgayTao DESC;
END
GO

/* ---------------------------------------------------------------------------
   ONLINE HOLD / DEPOSIT
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_DanhSachDatCocOnline
    @TrangThai VARCHAR(30)=NULL,
    @TuNgay DATE=NULL,
    @DenNgay DATE=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        dc.DatCocID,dc.GiuChoID,gc.KhachHangID,kh.HoTen,kh.SoDienThoai,
        gc.SanID,s.MaSan,s.TenSan,gc.BatDau,gc.KetThuc,gc.HetHanLuc,gc.TrangThai AS TrangThaiGiuCho,
        dc.SoTienCoc,dc.VietQRID,dc.TrangThai AS TrangThaiDatCoc,dc.NgayTao,dc.NgayXacNhan,
        gd.MaGiaoDich,gd.TrangThai AS TrangThaiGiaoDich,gd.MaThamChieu
    FROM dbo.DatCocOnline dc
    JOIN dbo.GiuChoDatSan gc ON gc.GiuChoID=dc.GiuChoID
    JOIN dbo.KhachHang kh ON kh.KhachHangID=gc.KhachHangID
    JOIN dbo.San s ON s.SanID=gc.SanID
    LEFT JOIN dbo.YeuCauVietQR qr ON qr.VietQRID=dc.VietQRID
    LEFT JOIN dbo.GiaoDichThanhToan gd ON gd.GiaoDichID=qr.GiaoDichID
    WHERE (@TrangThai IS NULL OR dc.TrangThai=@TrangThai)
      AND (@TuNgay IS NULL OR CAST(dc.NgayTao AS DATE)>=@TuNgay)
      AND (@DenNgay IS NULL OR CAST(dc.NgayTao AS DATE)<=@DenNgay)
    ORDER BY dc.NgayTao DESC;
END
GO

/* ---------------------------------------------------------------------------
   SETTINGS / LOGS / NOTIFICATIONS
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_DanhSachCauHinh
    @Nhom VARCHAR(50)=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CauHinhID,Khoa,GiaTri,MoTa,Nhom,NgayCapNhat
    FROM dbo.CauHinhHeThong
    WHERE @Nhom IS NULL OR Nhom=@Nhom
    ORDER BY Nhom,Khoa;
END
GO

CREATE PROCEDURE dbo.sp_UI_LuuCauHinh
    @Khoa VARCHAR(100),
    @GiaTri NVARCHAR(MAX),
    @MoTa NVARCHAR(500)=NULL,
    @Nhom VARCHAR(50)=NULL,
    @TaiKhoanID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Cu NVARCHAR(MAX)=(SELECT GiaTri FROM dbo.CauHinhHeThong WHERE Khoa=@Khoa);

    BEGIN TRAN;
    IF EXISTS(SELECT 1 FROM dbo.CauHinhHeThong WHERE Khoa=@Khoa)
        UPDATE dbo.CauHinhHeThong
           SET GiaTri=@GiaTri,MoTa=COALESCE(@MoTa,MoTa),Nhom=COALESCE(@Nhom,Nhom),NgayCapNhat=SYSDATETIME()
         WHERE Khoa=@Khoa;
    ELSE
        INSERT dbo.CauHinhHeThong(Khoa,GiaTri,MoTa,Nhom)
        VALUES(@Khoa,@GiaTri,@MoTa,@Nhom);

    INSERT dbo.NhatKyCauHinh(Khoa,GiaTriCu,GiaTriMoi,TaiKhoanID,ThoiGian)
    VALUES(@Khoa,@Cu,@GiaTri,@TaiKhoanID,SYSDATETIME());
    COMMIT TRAN;
END
GO

CREATE PROCEDURE dbo.sp_UI_DanhSachNhatKyHeThong
    @TuNgay DATE=NULL,
    @DenNgay DATE=NULL,
    @Module VARCHAR(80)=NULL,
    @TuKhoa NVARCHAR(150)=NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1000)
        nk.NhatKyID,nk.TaiKhoanID,tk.TenDangNhap,vt.TenVaiTro,
        nk.Module,nk.HanhDong,nk.BangDuLieu,nk.KhoaDuLieu,nk.DuLieuCu,nk.DuLieuMoi,
        nk.TenMay,nk.ThoiGian
    FROM dbo.NhatKyHeThong nk
    LEFT JOIN dbo.TaiKhoan tk ON tk.TaiKhoanID=nk.TaiKhoanID
    LEFT JOIN dbo.VaiTro vt ON vt.VaiTroID=tk.VaiTroID
    WHERE (@TuNgay IS NULL OR CAST(nk.ThoiGian AS DATE)>=@TuNgay)
      AND (@DenNgay IS NULL OR CAST(nk.ThoiGian AS DATE)<=@DenNgay)
      AND (@Module IS NULL OR nk.Module=@Module)
      AND (@TuKhoa IS NULL OR @TuKhoa=N''
        OR tk.TenDangNhap LIKE N'%' + @TuKhoa + N'%'
        OR nk.HanhDong LIKE N'%' + @TuKhoa + N'%'
        OR nk.KhoaDuLieu LIKE N'%' + @TuKhoa + N'%')
    ORDER BY nk.ThoiGian DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_LayThongBao
    @TaiKhoanID INT,
    @ChiChuaDoc BIT=0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (100)
        ThongBaoID,TieuDe,NoiDung,LoaiThongBao,DaDoc,LienKetLoai,LienKetID,NgayTao
    FROM dbo.ThongBao
    WHERE TaiKhoanID=@TaiKhoanID
      AND (@ChiChuaDoc=0 OR DaDoc=0)
    ORDER BY NgayTao DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_DanhDauThongBaoDaDoc
    @ThongBaoID BIGINT,
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.ThongBao
       SET DaDoc=1
     WHERE ThongBaoID=@ThongBaoID AND TaiKhoanID=@TaiKhoanID;
END
GO

/* ---------------------------------------------------------------------------
   CUSTOMER ROLE DATA
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_CustomerHome
    @KhachHangID INT,
    @Ngay DATE=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @Ngay=ISNULL(@Ngay,CAST(GETDATE() AS DATE));

    SELECT
        kh.KhachHangID,kh.HoTen,kh.DiemTichLuy,kh.TongChiTieu,
        htv.MaHang,htv.TenHang,ISNULL(v.SoDu,0) AS SoDuViCoc,
        (SELECT COUNT(*) FROM dbo.DonDatSan d WHERE d.KhachHangID=kh.KhachHangID AND d.TrangThaiDon<>'DA_HUY') AS TongLuotDat
    FROM dbo.KhachHang kh
    LEFT JOIN dbo.HangThanhVien htv ON htv.HangThanhVienID=kh.HangThanhVienID
    LEFT JOIN dbo.ViKhachHang v ON v.KhachHangID=kh.KhachHangID
    WHERE kh.KhachHangID=@KhachHangID;

    SELECT TOP (1)
        d.DonDatSanID,d.MaDon,d.TrangThaiDon,d.TrangThaiThanhToan,d.TongTien,d.ConLai,
        ct.SanID,s.TenSan,ls.TenLoaiSan,ct.BatDau,ct.KetThuc,s.ViTri
    FROM dbo.DonDatSan d
    JOIN dbo.ChiTietDatSan ct ON ct.DonDatSanID=d.DonDatSanID
    JOIN dbo.San s ON s.SanID=ct.SanID
    JOIN dbo.LoaiSan ls ON ls.LoaiSanID=s.LoaiSanID
    WHERE d.KhachHangID=@KhachHangID
      AND d.TrangThaiDon IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_SU_DUNG')
      AND ct.KetThuc>=SYSDATETIME()
    ORDER BY ct.BatDau;

    SELECT TOP (8)
        s.SanID,s.MaSan,s.TenSan,ls.TenLoaiSan,s.ViTri,s.GiaCoBan,s.TrangThaiVanHanh,
        (SELECT TOP (1) ha.DuongDanAnh FROM dbo.HinhAnhSan ha WHERE ha.SanID=s.SanID ORDER BY ha.LaAnhChinh DESC,ha.ThuTu) AS AnhChinh
    FROM dbo.San s
    JOIN dbo.LoaiSan ls ON ls.LoaiSanID=s.LoaiSanID
    WHERE s.DangHoatDong=1 AND s.TrangThaiVanHanh='SAN_SANG'
    ORDER BY s.SanID DESC;

    SELECT TOP (8)
        vk.VoucherKhachHangID,v.VoucherID,v.MaVoucher,v.TenVoucher,v.LoaiGiam,
        v.GiaTriGiam,v.GiamToiDa,v.DonToiThieu,v.NgayKetThuc
    FROM dbo.VoucherKhachHang vk
    JOIN dbo.Voucher v ON v.VoucherID=vk.VoucherID
    WHERE vk.KhachHangID=@KhachHangID
      AND vk.DaSuDung=0
      AND v.DangHoatDong=1
      AND SYSDATETIME() BETWEEN v.NgayBatDau AND v.NgayKetThuc
    ORDER BY v.NgayKetThuc;
END
GO

CREATE PROCEDURE dbo.sp_UI_CustomerLichSuDatSan
    @KhachHangID INT,
    @TrangThai VARCHAR(30)=NULL
AS
BEGIN
    SET NOCOUNT ON;

    EXEC dbo.sp_UI_DanhSachDonDatSan
        @TuNgay=NULL,@DenNgay=NULL,@TrangThai=@TrangThai,@TuKhoa=NULL,@KhachHangID=@KhachHangID;
END
GO

CREATE PROCEDURE dbo.sp_UI_CustomerHoaDon
    @KhachHangID INT
AS
BEGIN
    SET NOCOUNT ON;

    EXEC dbo.sp_UI_DanhSachHoaDon
        @TuNgay=NULL,@DenNgay=NULL,@TrangThai=NULL,@TuKhoa=NULL,@KhachHangID=@KhachHangID;
END
GO

CREATE PROCEDURE dbo.sp_UI_CustomerVoucher
    @KhachHangID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        vk.VoucherKhachHangID,v.VoucherID,v.MaVoucher,v.TenVoucher,v.LoaiGiam,
        v.GiaTriGiam,v.GiamToiDa,v.DonToiThieu,v.NgayBatDau,v.NgayKetThuc,
        vk.NgayNhan,vk.DaSuDung,vk.NgaySuDung,vk.DonDatSanID
    FROM dbo.VoucherKhachHang vk
    JOIN dbo.Voucher v ON v.VoucherID=vk.VoucherID
    WHERE vk.KhachHangID=@KhachHangID
    ORDER BY vk.DaSuDung,v.NgayKetThuc,v.MaVoucher;
END
GO

/* ---------------------------------------------------------------------------
   STAFF ROLE DATA
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_StaffLichHomNay
    @NhanVienID INT=NULL,
    @Ngay DATE=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @Ngay=ISNULL(@Ngay,CAST(GETDATE() AS DATE));

    SELECT
        d.DonDatSanID,d.MaDon,kh.HoTen,kh.SoDienThoai,s.MaSan,s.TenSan,
        ct.BatDau,ct.KetThuc,d.TrangThaiDon,d.TrangThaiThanhToan,
        d.TongTien,d.DaThanhToan,d.ConLai
    FROM dbo.DonDatSan d
    JOIN dbo.KhachHang kh ON kh.KhachHangID=d.KhachHangID
    JOIN dbo.ChiTietDatSan ct ON ct.DonDatSanID=d.DonDatSanID
    JOIN dbo.San s ON s.SanID=ct.SanID
    WHERE CAST(ct.BatDau AS DATE)=@Ngay
      AND d.TrangThaiDon<>'DA_HUY'
    ORDER BY ct.BatDau,s.MaSan;
END
GO

CREATE PROCEDURE dbo.sp_UI_StaffThongKeCa
    @NhanVienID INT,
    @CaLamViecID BIGINT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @CaLamViecID IS NULL
        SELECT TOP (1) @CaLamViecID=CaLamViecID
        FROM dbo.CaLamViec
        WHERE NhanVienID=@NhanVienID
        ORDER BY ThoiGianMoCa DESC;

    SELECT *
    FROM dbo.CaLamViec
    WHERE CaLamViecID=@CaLamViecID AND NhanVienID=@NhanVienID;

    SELECT
        COUNT(*) AS SoGiaoDich,
        ISNULL(SUM(CASE WHEN gd.TrangThai='THANH_CONG' THEN gd.SoTien ELSE 0 END),0) AS TongDaThu,
        ISNULL(SUM(CASE WHEN gd.TrangThai='THANH_CONG' AND gd.PhuongThuc='TIEN_MAT' THEN gd.SoTien ELSE 0 END),0) AS TienMat,
        ISNULL(SUM(CASE WHEN gd.TrangThai='THANH_CONG' AND gd.PhuongThuc='VIETQR' THEN gd.SoTien ELSE 0 END),0) AS VietQR,
        ISNULL(SUM(CASE WHEN gd.TrangThai='THANH_CONG' AND gd.PhuongThuc='CHUYEN_KHOAN' THEN gd.SoTien ELSE 0 END),0) AS ChuyenKhoan
    FROM dbo.GiaoDichThanhToan gd
    JOIN dbo.TaiKhoan tk ON tk.TaiKhoanID=gd.NguoiTaoID
    WHERE tk.NhanVienID=@NhanVienID
      AND gd.NgayTao >= ISNULL((SELECT ThoiGianMoCa FROM dbo.CaLamViec WHERE CaLamViecID=@CaLamViecID),'19000101')
      AND gd.NgayTao <= ISNULL((SELECT ThoiGianDongCa FROM dbo.CaLamViec WHERE CaLamViecID=@CaLamViecID),SYSDATETIME());
END
GO

/* ---------------------------------------------------------------------------
   REPORTS
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_BaoCaoTongQuan
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ISNULL(SUM(CASE WHEN gd.TrangThai='THANH_CONG' THEN gd.SoTien ELSE 0 END),0) AS DoanhThuDaThu,
        COUNT(DISTINCT d.DonDatSanID) AS TongDon,
        COUNT(DISTINCT d.KhachHangID) AS TongKhach,
        ISNULL(SUM(d.TongTien),0) AS GiaTriDon,
        ISNULL(SUM(d.ConLai),0) AS CongNo
    FROM dbo.DonDatSan d
    LEFT JOIN dbo.GiaoDichThanhToan gd ON gd.DonDatSanID=d.DonDatSanID
    WHERE CAST(d.NgayTao AS DATE) BETWEEN @TuNgay AND @DenNgay;

    SELECT
        CAST(gd.NgayXacNhan AS DATE) AS Ngay,
        SUM(gd.SoTien) AS DoanhThu
    FROM dbo.GiaoDichThanhToan gd
    WHERE gd.TrangThai='THANH_CONG'
      AND CAST(gd.NgayXacNhan AS DATE) BETWEEN @TuNgay AND @DenNgay
    GROUP BY CAST(gd.NgayXacNhan AS DATE)
    ORDER BY Ngay;

    SELECT
        ls.TenLoaiSan,
        COUNT(*) AS LuotDat,
        SUM(ct.TienSan) AS DoanhThuSan,
        SUM(ct.SoPhut)/60.0 AS TongGio
    FROM dbo.ChiTietDatSan ct
    JOIN dbo.DonDatSan d ON d.DonDatSanID=ct.DonDatSanID
    JOIN dbo.San s ON s.SanID=ct.SanID
    JOIN dbo.LoaiSan ls ON ls.LoaiSanID=s.LoaiSanID
    WHERE d.TrangThaiDon<>'DA_HUY'
      AND CAST(ct.BatDau AS DATE) BETWEEN @TuNgay AND @DenNgay
    GROUP BY ls.TenLoaiSan
    ORDER BY DoanhThuSan DESC;

    SELECT TOP (10)
        sp.TenSanPhamDichVu,
        SUM(ctdv.SoLuong) AS SoLuong,
        SUM(ctdv.ThanhTien) AS DoanhThuDichVu
    FROM dbo.ChiTietDichVuDatSan ctdv
    JOIN dbo.SanPhamDichVu sp ON sp.SanPhamDichVuID=ctdv.SanPhamDichVuID
    JOIN dbo.DonDatSan d ON d.DonDatSanID=ctdv.DonDatSanID
    WHERE CAST(d.NgayTao AS DATE) BETWEEN @TuNgay AND @DenNgay
    GROUP BY sp.TenSanPhamDichVu
    ORDER BY DoanhThuDichVu DESC;
END
GO

CREATE PROCEDURE dbo.sp_UI_BaoCaoHieuQuaVoucher
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.VoucherID,v.MaVoucher,v.TenVoucher,
        COUNT(vk.VoucherKhachHangID) AS SoLuotCap,
        SUM(CASE WHEN vk.DaSuDung=1 THEN 1 ELSE 0 END) AS SoLuotSuDung,
        ISNULL(SUM(CASE WHEN vk.DaSuDung=1 THEN d.TienVoucher ELSE 0 END),0) AS TongTienGiam,
        ISNULL(SUM(CASE WHEN vk.DaSuDung=1 THEN d.TongTien ELSE 0 END),0) AS DoanhThuKemVoucher
    FROM dbo.Voucher v
    LEFT JOIN dbo.VoucherKhachHang vk ON vk.VoucherID=v.VoucherID
    LEFT JOIN dbo.DonDatSan d ON d.DonDatSanID=vk.DonDatSanID
    WHERE (vk.NgayNhan IS NULL OR CAST(vk.NgayNhan AS DATE) BETWEEN @TuNgay AND @DenNgay)
    GROUP BY v.VoucherID,v.MaVoucher,v.TenVoucher
    ORDER BY DoanhThuKemVoucher DESC;
END
GO

/* ---------------------------------------------------------------------------
   DATABASE HEALTH FOR SETTINGS UI
--------------------------------------------------------------------------- */
CREATE PROCEDURE dbo.sp_UI_HealthCheck
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DB_NAME() AS DatabaseName,
        @@SERVERNAME AS ServerName,
        CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(100)) AS ProductVersion,
        CAST(SERVERPROPERTY('Edition') AS NVARCHAR(200)) AS Edition,
        SYSDATETIME() AS ServerTime,
        (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
          WHERE TABLE_SCHEMA='dbo' AND TABLE_TYPE='BASE TABLE') AS SoBang,
        (SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES
          WHERE ROUTINE_SCHEMA='dbo' AND ROUTINE_TYPE='PROCEDURE') AS SoStoredProcedure;
END
GO

/* ---------------------------------------------------------------------------
   DEFAULT EXTRA CONFIG FOR CURRENT UI - ONLY SYSTEM CATALOG, NO DEMO BOOKINGS
--------------------------------------------------------------------------- */
IF NOT EXISTS(SELECT 1 FROM dbo.CauHinhHeThong WHERE Khoa='TEN_THUONG_HIEU')
    INSERT dbo.CauHinhHeThong(Khoa,GiaTri,MoTa,Nhom)
    VALUES('TEN_THUONG_HIEU',N'SPORTFIELD',N'Thương hiệu hiển thị trên toàn bộ WinForms','GENERAL');
GO
IF NOT EXISTS(SELECT 1 FROM dbo.CauHinhHeThong WHERE Khoa='TY_LE_COC_MAC_DINH')
    INSERT dbo.CauHinhHeThong(Khoa,GiaTri,MoTa,Nhom)
    VALUES('TY_LE_COC_MAC_DINH',N'50',N'Phần trăm tiền cọc mặc định','BOOKING');
GO
IF NOT EXISTS(SELECT 1 FROM dbo.CauHinhHeThong WHERE Khoa='THOI_GIAN_GIU_CHO_PHUT')
    INSERT dbo.CauHinhHeThong(Khoa,GiaTri,MoTa,Nhom)
    VALUES('THOI_GIAN_GIU_CHO_PHUT',N'15',N'Thời gian giữ chỗ online','BOOKING');
GO

PRINT N'[09/10] Schema/nghiệp vụ đã tạo - bắt đầu kiểm tra cuối.';
GO

/* =============================================================================
   FINAL DATABASE CONTRACT CHECK
============================================================================= */
IF OBJECT_ID(N'dbo.sp_KiemTraDayDuSportField', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_KiemTraDayDuSportField;
GO

CREATE PROCEDURE dbo.sp_KiemTraDayDuSportField
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Loi TABLE
    (
        Nhom NVARCHAR(50),
        DoiTuong NVARCHAR(200),
        MoTa NVARCHAR(500)
    );

    DECLARE @BatBuoc TABLE(Loai CHAR(2),Ten SYSNAME);
    INSERT @BatBuoc(Loai,Ten) VALUES
    ('U','VaiTro'),('U','Quyen'),('U','TaiKhoan'),('U','NhanVien'),('U','KhachHang'),
    ('U','HangThanhVien'),('U','CumSan'),('U','LoaiSan'),('U','San'),('U','HinhAnhSan'),
    ('U','TienIch'),('U','SanTienIch'),('U','LichBaoTriSan'),('U','BangGia'),
    ('U','DonDatSan'),('U','ChiTietDatSan'),('U','LichSuTrangThaiDon'),('U','LichSuDoiLich'),
    ('U','SanPhamDichVu'),('U','Kho'),('U','TonKho'),('U','GiaoDichKho'),('U','KiemKeKho'),
    ('U','Voucher'),('U','VoucherKhachHang'),('U','KhuyenMai'),('U','HoaDon'),
    ('U','GiaoDichThanhToan'),('U','YeuCauVietQR'),('U','HoanTien'),('U','DoiSoatThanhToan'),
    ('U','CaLamViec'),('U','LichPhanCa'),('U','BanGiaoCa'),('U','GiuChoDatSan'),('U','DatCocOnline'),
    ('U','TaiKhoanNganHang'),('U','CauHinhHeThong'),('U','ThongBao'),('U','NhatKyHeThong'),
    ('U','PhienDangNhap'),('U','ThietBiDangNhap'),('U','NhatKyInAn'),('U','NhatKyXuatBaoCao'),
    ('FN','fn_DonGiaSanTaiThoiDiem'),('FN','fn_TinhTienSan'),
    ('P','sp_TaoDonDatSan'),('P','sp_TimSanTrong'),('P','sp_TinhGiaDatSan'),
    ('P','sp_XacNhanDon'),('P','sp_CheckIn'),('P','sp_HoanTatDon'),('P','sp_HuyDon'),('P','sp_DoiLich'),
    ('P','sp_TaoHoaDon'),('P','sp_GhiNhanThanhToan'),('P','sp_TaoYeuCauVietQR'),
    ('P','sp_XacNhanChuyenKhoan'),('P','sp_HoanTien'),('P','sp_NhapKho'),('P','sp_XuatKho'),
    ('P','sp_MoCa'),('P','sp_DongCa'),('P','sp_DashboardAdmin'),('P','sp_DashboardNhanVien'),
    ('P','sp_DangKyKhachHang'),('P','sp_TaoNhanVienVaTaiKhoan'),
    ('P','sp_UI_TimKiemNhanh'),('P','sp_UI_DanhSachSan'),('P','sp_UI_LichSanTheoNgay'),
    ('P','sp_UI_DanhSachDonDatSan'),('P','sp_UI_DanhSachKhachHang'),('P','sp_UI_DanhSachNhanVien'),
    ('P','sp_UI_DanhSachHoaDon'),('P','sp_UI_DanhSachVoucher'),('P','sp_UI_DanhSachKhuyenMai'),
    ('P','sp_UI_DanhSachDichVuTonKho'),('P','sp_UI_DanhSachDatCocOnline'),
    ('P','sp_UI_CustomerHome'),('P','sp_UI_StaffLichHomNay'),('P','sp_UI_BaoCaoTongQuan'),
    ('P','sp_UI_DanhSachCauHinh'),('P','sp_UI_LuuCauHinh'),('P','sp_UI_DanhSachNhatKyHeThong'),
    ('P','sp_UI_HealthCheck');

    INSERT @Loi(Nhom,DoiTuong,MoTa)
    SELECT N'OBJECT',b.Ten,N'Không tồn tại'
    FROM @BatBuoc b
    WHERE OBJECT_ID(N'dbo.'+b.Ten,b.Loai) IS NULL;

    IF COL_LENGTH('dbo.TaiKhoan','MatKhauSalt') IS NULL
        INSERT @Loi VALUES(N'COLUMN',N'TaiKhoan.MatKhauSalt',N'Thiếu salt PBKDF2');
    IF COL_LENGTH('dbo.San','TrangThaiVanHanh') IS NULL
        INSERT @Loi VALUES(N'COLUMN',N'San.TrangThaiVanHanh',N'Thiếu trạng thái vận hành sân');
    IF COL_LENGTH('dbo.ChiTietDatSan','BatDau') IS NULL OR COL_LENGTH('dbo.ChiTietDatSan','KetThuc') IS NULL
        INSERT @Loi VALUES(N'COLUMN',N'ChiTietDatSan.BatDau/KetThuc',N'Thiếu thời gian booking');
    IF COL_LENGTH('dbo.DonDatSan','ConLai') IS NULL
        INSERT @Loi VALUES(N'COLUMN',N'DonDatSan.ConLai',N'Thiếu công nợ đơn');
    IF COL_LENGTH('dbo.HoaDon','ConLai') IS NULL
        INSERT @Loi VALUES(N'COLUMN',N'HoaDon.ConLai',N'Thiếu công nợ hóa đơn');

    IF EXISTS(SELECT 1 FROM @Loi)
    BEGIN
        SELECT * FROM @Loi;
        THROW 52999,N'CSDL SPORTFIELD chưa đạt kiểm tra contract. Xem result set lỗi.',1;
    END

    DECLARE @SoBang INT=
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA='dbo' AND TABLE_TYPE='BASE TABLE'
    );
    DECLARE @SoView INT=
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS
        WHERE TABLE_SCHEMA='dbo'
    );
    DECLARE @SoStoredProcedure INT=
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES
        WHERE ROUTINE_SCHEMA='dbo' AND ROUTINE_TYPE='PROCEDURE'
    );
    DECLARE @SoFunction INT=
    (
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES
        WHERE ROUTINE_SCHEMA='dbo' AND ROUTINE_TYPE='FUNCTION'
    );
    DECLARE @SoTrigger INT=
        CASE WHEN OBJECT_ID(N'dbo.trg_ChiTietDatSan_KhongTrungLich',N'TR') IS NULL THEN 0 ELSE 1 END;

    IF @SoBang<>92
        THROW 52991,N'Sai số lượng bảng: SPORTFIELD phải có đúng 92 bảng.',1;
    IF @SoView<>18
        THROW 52992,N'Sai số lượng View: SPORTFIELD phải có đúng 18 View.',1;
    IF @SoStoredProcedure<>114
        THROW 52993,N'Sai số lượng Stored Procedure: phải có đúng 114. Có CREATE PROCEDURE đã thất bại.',1;
    IF @SoFunction<>2
        THROW 52994,N'Sai số lượng Function: SPORTFIELD phải có đúng 2 Function.',1;
    IF @SoTrigger<>1
        THROW 52995,N'Sai số lượng Trigger: SPORTFIELD phải có đúng 1 Trigger nghiệp vụ.',1;

    SELECT
        @SoBang AS SoBang,
        @SoView AS SoView,
        @SoStoredProcedure AS SoStoredProcedure,
        @SoFunction AS SoFunction,
        @SoTrigger AS SoTrigger,
        N'PASS' AS TrangThai;

    PRINT N'==============================================================';
    PRINT N'SPORTFIELD DATABASE - FULL ONE FILE V2 - HOÀN TẤT THỰC SỰ';
    PRINT N'92 bảng | 18 View | 114 Stored Procedure | 2 Function | 1 Trigger';
    PRINT N'FINAL CONTRACT CHECK: PASS';
    PRINT N'==============================================================';
END
GO

EXEC dbo.sp_KiemTraDayDuSportField;
GO
