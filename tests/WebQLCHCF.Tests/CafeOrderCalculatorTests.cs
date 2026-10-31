using WebQLCHCF.Services;
using Xunit;

namespace WebQLCHCF.Tests;

public class CafeOrderCalculatorTests
{
    [Theory]
    [InlineData(25000, 2, 50000)]
    [InlineData(45000, 1, 45000)]
    [InlineData(30000, 0, 0)]
    [InlineData(0, 5, 0)]
    public void TinhThanhTienMon_KhongKhuyenMai_TraVeTichSoLuongVaDonGia(decimal donGia, int soLuong, decimal expected)
    {
        // Act
        decimal result = CafeOrderCalculator.TinhThanhTienMon(donGia, soLuong);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(30000, 2, 10000, 50000)] // 60.000 - 10.000 = 50.000
    [InlineData(25000, 1, 30000, 0)]     // 25.000 - 30.000 < 0 -> Sàn 0đ
    [InlineData(50000, 2, 0, 100000)]    // Giảm 0đ
    public void TinhThanhTienMon_KhuyenMaiTienMat_GiamTruChinhXac(
        decimal donGia, int soLuong, decimal giaTriGiam, decimal expected)
    {
        // Act
        decimal result = CafeOrderCalculator.TinhThanhTienMon(
            donGia, soLuong, CafeOrderCalculator.LoaiKMTienMat, giaTriGiam, true);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(50000, 2, 10, 90000)]   // 100.000 - 10% = 90.000
    [InlineData(40000, 1, 50, 20000)]   // 40.000 - 50% = 20.000
    [InlineData(35000, 2, 100, 0)]      // 70.000 - 100% = 0đ
    [InlineData(30000, 1, 150, 0)]      // Giảm > 100% -> Sàn 0đ
    [InlineData(30000, 1, 0, 30000)]    // Giảm 0% -> Giữ nguyên
    public void TinhThanhTienMon_KhuyenMaiPhanTram_ChietKhauChinhXac(
        decimal donGia, int soLuong, decimal phanTram, decimal expected)
    {
        // Act
        decimal result = CafeOrderCalculator.TinhThanhTienMon(
            donGia, soLuong, CafeOrderCalculator.LoaiKMPhanTram, phanTram, true);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TinhThanhTienMon_KhuyenMaiHetHieuLuc_GiuNguyenGiaGoc()
    {
        // Act
        decimal result = CafeOrderCalculator.TinhThanhTienMon(
            40000, 2, CafeOrderCalculator.LoaiKMPhanTram, 50, kmDangHieuLuc: false);

        // Assert
        Assert.Equal(80000, result);
    }

    [Fact]
    public void TinhThanhTienMon_DonGiaAm_NemNgoaiLeArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CafeOrderCalculator.TinhThanhTienMon(-1000, 2));
    }

    [Fact]
    public void TinhThanhTienMon_SoLuongAm_NemNgoaiLeArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CafeOrderCalculator.TinhThanhTienMon(20000, -1));
    }

    [Fact]
    public void TinhTongTienDonHang_DanhSachRong_TraVeKhong()
    {
        decimal result = CafeOrderCalculator.TinhTongTienDonHang(new List<decimal>());
        Assert.Equal(0m, result);
    }

    [Fact]
    public void TinhTongTienDonHang_NhieuMon_TinhTongChinhXac()
    {
        var items = new List<decimal> { 45000, 35000, 20000 };
        decimal result = CafeOrderCalculator.TinhTongTienDonHang(items);
        Assert.Equal(100000m, result);
    }

    [Theory]
    [InlineData(99000, 9)]       // 99.000 / 10.000 = 9 điểm
    [InlineData(100000, 10)]     // 100.000 / 10.000 = 10 điểm
    [InlineData(9999, 0)]        // Dưới 10.000đ = 0 điểm
    [InlineData(0, 0)]           // 0đ = 0 điểm
    [InlineData(-5000, 0)]       // Âm = 0 điểm
    public void TinhDiemTichLuy_Moi10NghinDongDuoc1Diem_LamTronXuong(decimal tongTien, int expectedPoints)
    {
        int points = CafeOrderCalculator.TinhDiemTichLuy(tongTien);
        Assert.Equal(expectedPoints, points);
    }

    [Theory]
    [InlineData(CafeOrderCalculator.LoaiKMPhanTram, 50, true)]
    [InlineData(CafeOrderCalculator.LoaiKMPhanTram, 100, true)]
    [InlineData(CafeOrderCalculator.LoaiKMPhanTram, 101, false)] // > 100% không hợp lệ
    [InlineData(CafeOrderCalculator.LoaiKMPhanTram, -5, false)]  // Âm không hợp lệ
    [InlineData(CafeOrderCalculator.LoaiKMTienMat, 20000, true)]
    [InlineData(CafeOrderCalculator.LoaiKMTienMat, -1000, false)]
    public void KiemTraHopLeGiaTriKhuyenMai_ChanGiaTriKhongHopLe(string loaiKM, decimal giaTri, bool expected)
    {
        bool isValid = CafeOrderCalculator.KiemTraHopLeGiaTriKhuyenMai(loaiKM, giaTri);
        Assert.Equal(expected, isValid);
    }

    [Fact]
    public void SinhUrlVietQr_ChuaDayDuThongTinNganHangVaMaPhien()
    {
        decimal soTien = 75000;
        string noiDung = "HD260531001";
        string url = CafeOrderCalculator.SinhUrlVietQr(soTien, noiDung);

        Assert.Contains("img.vietqr.io", url);
        Assert.Contains("MB-0332135258", url);
        Assert.Contains("amount=75000", url);
        Assert.Contains("HD260531001", url);
        Assert.Contains("HOANG+CHIEM+NGUYEN", url);
    }
}
