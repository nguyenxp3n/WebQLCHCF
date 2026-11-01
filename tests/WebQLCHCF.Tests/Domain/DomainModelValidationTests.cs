using System.ComponentModel.DataAnnotations;
using WebQLCHCF.Models;
using Xunit;

namespace WebQLCHCF.Tests.Domain;

public class DomainModelValidationTests
{
    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }

    private static bool IsValid(object model) => ValidateModel(model).Count == 0;

    #region 1. AuditLog Tests
    [Fact]
    public void AuditLog_ValidInstance_PassesValidation()
    {
        var log = new AuditLog
        {
            MaLog = 1,
            ThoiGian = DateTime.Now,
            MaNV = "NV01",
            TenDangNhap = "admin",
            VaiTro = "Quản Lý",
            ChucNang = "Hệ thống",
            HanhDong = "Đăng nhập",
            DoiTuong = "TaiKhoan",
            MaDoiTuong = "TK01",
            NoiDung = "Đăng nhập thành công",
            DiaChiIP = "127.0.0.1",
            UserAgent = "Mozilla/5.0"
        };

        Assert.True(IsValid(log));
    }

    [Fact]
    public void AuditLog_ExceedingStringLengths_FailsValidation()
    {
        var log = new AuditLog
        {
            MaNV = new string('A', 51), // Max 50
            TenDangNhap = new string('B', 101), // Max 100
            NoiDung = new string('C', 1001), // Max 1000
            DiaChiIP = new string('D', 101), // Max 100
            UserAgent = new string('E', 501) // Max 500
        };

        var errors = ValidateModel(log);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AuditLog.MaNV)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AuditLog.TenDangNhap)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AuditLog.NoiDung)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AuditLog.DiaChiIP)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(AuditLog.UserAgent)));
    }
    #endregion

    #region 2. Ban Tests
    [Fact]
    public void Ban_ValidInstance_PassesValidation()
    {
        var ban = new Ban
        {
            MaBan = "B01",
            TenBan = "Bàn số 1",
            SoBan = 1,
            KhuVuc = "Tầng trệt",
            TrangThai = "Trống"
        };

        Assert.True(IsValid(ban));
    }

    [Fact]
    public void Ban_MissingRequiredFields_FailsValidation()
    {
        var ban = new Ban
        {
            MaBan = "B01",
            TenBan = "", // Required
            KhuVuc = "", // Required
            TrangThai = "" // Required
        };

        var errors = ValidateModel(ban);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Ban.TenBan)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Ban.KhuVuc)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Ban.TrangThai)));
    }

    [Fact]
    public void Ban_ExceedingLengths_FailsValidation()
    {
        var ban = new Ban
        {
            MaBan = "TOOLONG123", // Max 5
            TenBan = new string('B', 25), // Max 20
            KhuVuc = new string('K', 25), // Max 20
            TrangThai = new string('T', 35) // Max 30
        };

        var errors = ValidateModel(ban);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Ban.MaBan)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Ban.TenBan)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Ban.KhuVuc)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Ban.TrangThai)));
    }
    #endregion

    #region 3. CTHD Tests
    [Fact]
    public void CTHD_ValidInstance_PassesValidation()
    {
        var cthd = new CTHD
        {
            MaHD = "HD00001",
            MaSP = "SP01",
            SoLuong = 2,
            DonGia = 25000,
            MaKM = "KM01"
        };

        Assert.True(IsValid(cthd));
    }

    [Fact]
    public void CTHD_ExceedingLengths_FailsValidation()
    {
        var cthd = new CTHD
        {
            MaHD = "HD0000000000001", // Max 9
            MaSP = "SP00001", // Max 5
            MaKM = "KM00001" // Max 5
        };

        var errors = ValidateModel(cthd);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CTHD.MaHD)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CTHD.MaSP)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CTHD.MaKM)));
    }
    #endregion

    #region 4. CTPN Tests
    [Fact]
    public void CTPN_ValidInstance_PassesValidation()
    {
        var ctpn = new CTPN
        {
            MaPN = "PN00001",
            MaNL = "NL01",
            SoLuong = 10,
            DonGia = 150000
        };

        Assert.True(IsValid(ctpn));
    }

    [Fact]
    public void CTPN_InvalidQuantityAndPriceRange_FailsValidation()
    {
        var ctpn = new CTPN
        {
            MaPN = "PN00001",
            MaNL = "NL01",
            SoLuong = 0, // Must be >= 1
            DonGia = -1000 // Must be > 0 and <= 100,000,000
        };

        var errors = ValidateModel(ctpn);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CTPN.SoLuong)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CTPN.DonGia)));
    }
    #endregion

    #region 5. ChamCong Tests
    [Fact]
    public void ChamCong_ValidInstance_PassesValidation()
    {
        var cc = new ChamCong
        {
            MaCC = 1,
            MaNV = "NV01",
            ThoiGianCheckIn = DateTime.Now.AddHours(-4),
            ThoiGianCheckOut = DateTime.Now,
            TrangThai = "Đã checkout",
            DonGiaGio = 25000,
            TongPhut = 240,
            TongGio = 4.00m,
            TienCong = 100000,
            GhiChu = "Đúng giờ"
        };

        Assert.True(IsValid(cc));
    }

    [Fact]
    public void ChamCong_MissingRequiredFields_FailsValidation()
    {
        var cc = new ChamCong
        {
            MaNV = "", // Required
            TrangThai = "" // Required
        };

        var errors = ValidateModel(cc);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ChamCong.MaNV)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ChamCong.TrangThai)));
    }
    #endregion

    #region 6. CongThuc Tests
    [Fact]
    public void CongThuc_ValidInstance_PassesValidation()
    {
        var ct = new CongThuc
        {
            MaSP = "SP01",
            MaNL = "NL01",
            DinhLuong = 25.500m
        };

        Assert.True(IsValid(ct));
    }

    [Fact]
    public void CongThuc_ExceedingLength_FailsValidation()
    {
        var ct = new CongThuc
        {
            MaSP = "SP00001", // Max 5
            MaNL = "NL00001", // Max 5
            DinhLuong = 10
        };

        var errors = ValidateModel(ct);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CongThuc.MaSP)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CongThuc.MaNL)));
    }
    #endregion

    #region 7. HoaDon Tests
    [Fact]
    public void HoaDon_DefaultValuesAndValidInstance_PassesValidation()
    {
        var hd = new HoaDon
        {
            MaHD = "HD00001",
            NgayLap = DateTime.Today,
            MaNV = "NV01",
            MaBan = "B01",
            ThoiGianTao = DateTime.Now
        };

        // Assert Defaults
        Assert.Equal("ChoThanhToan", hd.TrangThaiOrder);
        Assert.Equal("ChuaThanhToan", hd.TrangThaiThanhToan);
        Assert.True(IsValid(hd));
    }

    [Fact]
    public void HoaDon_MissingRequiredFields_FailsValidation()
    {
        var hd = new HoaDon
        {
            MaHD = "HD00001",
            MaNV = "", // Required
            MaBan = "", // Required
            TrangThaiOrder = "", // Required
            TrangThaiThanhToan = "" // Required
        };

        var errors = ValidateModel(hd);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(HoaDon.MaNV)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(HoaDon.MaBan)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(HoaDon.TrangThaiOrder)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(HoaDon.TrangThaiThanhToan)));
    }
    #endregion

    #region 8. KhachHang Tests
    [Fact]
    public void KhachHang_DefaultValuesAndValidInstance_PassesValidation()
    {
        var kh = new KhachHang
        {
            MaKH = "KH01",
            TenKH = "Nguyễn Văn A",
            SDT = "0901234567"
        };

        Assert.Equal(0, kh.DiemTichLuy);
        Assert.True(IsValid(kh));
    }

    [Theory]
    [InlineData("")] // Empty
    [InlineData("12345")] // Too short
    [InlineData("09012345678")] // 11 digits
    [InlineData("abcdefghij")] // Non-digits
    [InlineData("090123456a")] // Mixed
    public void KhachHang_InvalidPhone_FailsValidation(string invalidPhone)
    {
        var kh = new KhachHang
        {
            MaKH = "KH01",
            TenKH = "Nguyễn Văn A",
            SDT = invalidPhone
        };

        var errors = ValidateModel(kh);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(KhachHang.SDT)));
    }

    [Fact]
    public void KhachHang_MissingTenKH_FailsValidation()
    {
        var kh = new KhachHang
        {
            MaKH = "KH01",
            TenKH = "",
            SDT = "0901234567"
        };

        var errors = ValidateModel(kh);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(KhachHang.TenKH)));
    }
    #endregion

    #region 9. KhuyenMai Tests
    [Fact]
    public void KhuyenMai_ValidInstance_PassesValidation()
    {
        var km = new KhuyenMai
        {
            MaKM = "KM01",
            TenKM = "Khuyến Mãi Khai Trương",
            LoaiKM = "Phần trăm",
            GiaTri = 15.00m,
            NgayBatDau = DateTime.Today,
            NgayKetThuc = DateTime.Today.AddDays(7),
            DieuKienApDung = "Mọi hóa đơn",
            TrangThai = "Đang áp dụng"
        };

        Assert.True(IsValid(km));
    }

    [Fact]
    public void KhuyenMai_NegativeGiaTri_FailsValidation()
    {
        var km = new KhuyenMai
        {
            MaKM = "KM01",
            TenKM = "KM Lỗi",
            LoaiKM = "Tiền mặt",
            GiaTri = -50000m, // Negative
            NgayBatDau = DateTime.Today,
            NgayKetThuc = DateTime.Today.AddDays(7),
            DieuKienApDung = "Hóa đơn",
            TrangThai = "Đang áp dụng"
        };

        var errors = ValidateModel(km);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(KhuyenMai.GiaTri)));
    }
    #endregion

    #region 10. LoaiSP Tests
    [Fact]
    public void LoaiSP_ValidInstance_PassesValidation()
    {
        var loai = new LoaiSP
        {
            MaLoaiSP = "L01",
            TenLoaiSP = "Cà phê truyền thống"
        };

        Assert.True(IsValid(loai));
    }

    [Fact]
    public void LoaiSP_MissingTenLoaiSP_FailsValidation()
    {
        var loai = new LoaiSP
        {
            MaLoaiSP = "L01",
            TenLoaiSP = "" // Required
        };

        var errors = ValidateModel(loai);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(LoaiSP.TenLoaiSP)));
    }
    #endregion

    #region 11. Luong Tests
    [Fact]
    public void Luong_DefaultValuesAndValidInstance_PassesValidation()
    {
        var luong = new Luong
        {
            MaLuong = 1,
            Thang = 10,
            Nam = 2026,
            LuongCoBan = 15000000
        };

        // Assert Defaults
        Assert.Equal(0, luong.PhuCap);
        Assert.Equal(0, luong.Thuong);
        Assert.Equal(0, luong.Phat);
        Assert.True(IsValid(luong));
    }

    [Theory]
    [InlineData(0, 2026)] // Month < 1
    [InlineData(13, 2026)] // Month > 12
    [InlineData(5, 1999)] // Year < 2000
    [InlineData(5, 2101)] // Year > 2100
    public void Luong_InvalidMonthOrYear_FailsValidation(int month, int year)
    {
        var luong = new Luong
        {
            MaLuong = 1,
            Thang = month,
            Nam = year,
            LuongCoBan = 10000000
        };

        var errors = ValidateModel(luong);
        if (month < 1 || month > 12)
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Luong.Thang)));
        if (year < 2000 || year > 2100)
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Luong.Nam)));
    }

    [Fact]
    public void Luong_NegativeValues_FailsValidation()
    {
        var luong = new Luong
        {
            MaLuong = 1,
            Thang = 10,
            Nam = 2026,
            LuongCoBan = -1000,
            PhuCap = -500,
            Thuong = -200,
            Phat = -100
        };

        var errors = ValidateModel(luong);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Luong.LuongCoBan)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Luong.PhuCap)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Luong.Thuong)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Luong.Phat)));
    }
    #endregion

    #region 12. NguyenLieu Tests
    [Fact]
    public void NguyenLieu_DefaultValuesAndValidInstance_PassesValidation()
    {
        var nl = new NguyenLieu
        {
            MaNL = "NL01",
            TenNL = "Hạt Arabica Cầu Đất",
            DonViTinh = "Kg",
            GiaNhap = 250000,
            NgayCapNhat = DateTime.Today,
            MucTonToiThieu = 5
        };

        Assert.Equal(0, nl.SoLuongTon);
        Assert.True(IsValid(nl));
    }

    [Fact]
    public void NguyenLieu_NegativeValues_FailsValidation()
    {
        var nl = new NguyenLieu
        {
            MaNL = "NL01",
            TenNL = "Hạt Cà phê",
            DonViTinh = "Kg",
            SoLuongTon = -10, // Must be >= 0
            GiaNhap = -1000, // Must be >= 0
            NgayCapNhat = DateTime.Today,
            MucTonToiThieu = -1 // Must be >= 0
        };

        var errors = ValidateModel(nl);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(NguyenLieu.SoLuongTon)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(NguyenLieu.GiaNhap)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(NguyenLieu.MucTonToiThieu)));
    }
    #endregion

    #region 13. NhanVien Tests
    [Fact]
    public void NhanVien_DefaultValuesAndValidInstance_PassesValidation()
    {
        var nv = new NhanVien
        {
            MaNV = "NV01",
            TenNV = "Nguyễn Văn Trưởng",
            GioiTinh = "Nam",
            NgaySinh = new DateTime(1995, 5, 12),
            SDT = "0987654321",
            ChucVu = "Quản Lý",
            MaLuong = 1
        };

        Assert.Equal(20000, nv.LuongTheoGio);
        Assert.True(IsValid(nv));
    }

    [Fact]
    public void NhanVien_MissingRequiredFields_FailsValidation()
    {
        var nv = new NhanVien
        {
            MaNV = "NV01",
            TenNV = "", // Required
            GioiTinh = "", // Required
            SDT = "", // Required
            ChucVu = "" // Required
        };

        var errors = ValidateModel(nv);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(NhanVien.TenNV)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(NhanVien.GioiTinh)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(NhanVien.SDT)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(NhanVien.ChucVu)));
    }

    [Fact]
    public void NhanVien_NegativeLuongTheoGio_FailsValidation()
    {
        var nv = new NhanVien
        {
            MaNV = "NV01",
            TenNV = "Nguyễn Văn Trưởng",
            GioiTinh = "Nam",
            SDT = "0987654321",
            ChucVu = "Quản Lý",
            MaLuong = 1,
            LuongTheoGio = -5000 // Negative
        };

        var errors = ValidateModel(nv);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(NhanVien.LuongTheoGio)));
    }
    #endregion

    #region 14. PhieuNhap Tests
    [Fact]
    public void PhieuNhap_ValidInstance_PassesValidation()
    {
        var pn = new PhieuNhap
        {
            MaPN = "PN00001",
            NgayNhap = DateTime.Today,
            MaNV = "NV01"
        };

        Assert.True(IsValid(pn));
    }

    [Fact]
    public void PhieuNhap_MissingMaNV_FailsValidation()
    {
        var pn = new PhieuNhap
        {
            MaPN = "PN00001",
            NgayNhap = DateTime.Today,
            MaNV = "" // Required
        };

        var errors = ValidateModel(pn);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(PhieuNhap.MaNV)));
    }
    #endregion

    #region 15. SanPham Tests
    [Fact]
    public void SanPham_DefaultValuesAndValidInstance_PassesValidation()
    {
        var sp = new SanPham
        {
            MaSP = "SP01",
            TenSP = "Cà phê sữa đá",
            DonGia = 28000,
            MaLoaiSP = "L01"
        };

        Assert.Equal("Tạm ngưng", sp.TrangThai);
        Assert.True(IsValid(sp));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5000)]
    [InlineData(10000001)] // Above 10M limit
    public void SanPham_InvalidPrice_FailsValidation(decimal invalidPrice)
    {
        var sp = new SanPham
        {
            MaSP = "SP01",
            TenSP = "Cà phê sữa đá",
            DonGia = invalidPrice,
            MaLoaiSP = "L01"
        };

        var errors = ValidateModel(sp);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SanPham.DonGia)));
    }

    [Fact]
    public void SanPham_MissingRequiredFields_FailsValidation()
    {
        var sp = new SanPham
        {
            MaSP = "SP01",
            TenSP = "", // Required
            DonGia = 25000,
            MaLoaiSP = "" // Required
        };

        var errors = ValidateModel(sp);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SanPham.TenSP)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SanPham.MaLoaiSP)));
    }
    #endregion

    #region 16. TaiKhoan Tests
    [Fact]
    public void TaiKhoan_ValidInstance_PassesValidation()
    {
        var tk = new TaiKhoan
        {
            MaTK = "TK01",
            TenDangNhap = "quanly01",
            MatKhau = "Password@123",
            MaNV = "NV01",
            VaiTro = "Quản Lý"
        };

        Assert.True(IsValid(tk));
    }

    [Fact]
    public void TaiKhoan_MissingRequiredFields_FailsValidation()
    {
        var tk = new TaiKhoan
        {
            MaTK = "TK01",
            TenDangNhap = "", // Required
            MatKhau = "", // Required
            MaNV = "", // Required
            VaiTro = "" // Required
        };

        var errors = ValidateModel(tk);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(TaiKhoan.TenDangNhap)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(TaiKhoan.MatKhau)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(TaiKhoan.MaNV)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(TaiKhoan.VaiTro)));
    }
    #endregion

    #region 17. ThanhToan Tests
    [Fact]
    public void ThanhToan_ValidInstance_PassesValidation()
    {
        var tt = new ThanhToan
        {
            MaThanhToan = 1,
            MaHD = "HD00001",
            PhuongThuc = "TienMat",
            SoTien = 85000,
            TrangThai = "DaThanhToan",
            ThoiGianTao = DateTime.Now
        };

        Assert.True(IsValid(tt));
    }

    [Fact]
    public void ThanhToan_MissingRequiredFields_FailsValidation()
    {
        var tt = new ThanhToan
        {
            MaThanhToan = 1,
            PhuongThuc = "", // Required
            TrangThai = "" // Required
        };

        var errors = ValidateModel(tt);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ThanhToan.PhuongThuc)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ThanhToan.TrangThai)));
    }
    #endregion
}
