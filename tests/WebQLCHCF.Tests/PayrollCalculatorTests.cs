using WebQLCHCF.Services;
using Xunit;

namespace WebQLCHCF.Tests;

public class PayrollCalculatorTests
{
    [Fact]
    public void TinhTongPhut_CaChuan_TraVeSoPhutChinhXac()
    {
        var checkIn = new DateTime(2026, 6, 1, 8, 0, 0);
        var checkOut = new DateTime(2026, 6, 1, 12, 30, 0);

        int phut = PayrollCalculator.TinhTongPhut(checkIn, checkOut);

        Assert.Equal(270, phut); // 4h30p = 270 phút
    }

    [Fact]
    public void TinhTongPhut_CheckOutNhoHonCheckIn_TraVeKhong()
    {
        var checkIn = new DateTime(2026, 6, 1, 12, 0, 0);
        var checkOut = new DateTime(2026, 6, 1, 8, 0, 0);

        int phut = PayrollCalculator.TinhTongPhut(checkIn, checkOut);

        Assert.Equal(0, phut);
    }

    [Theory]
    [InlineData(60, 1.00)]
    [InlineData(120, 2.00)]
    [InlineData(90, 1.50)]
    [InlineData(45, 0.75)]
    [InlineData(10, 0.17)]
    [InlineData(0, 0.00)]
    public void TinhTongGio_QuyDoiTuPhut_LamTron2ChuSoThapPhan(int tongPhut, decimal expectedGio)
    {
        decimal gio = PayrollCalculator.TinhTongGio(tongPhut);
        Assert.Equal(expectedGio, gio);
    }

    [Theory]
    [InlineData(4.5, 25000, 112500)]
    [InlineData(8.0, 30000, 240000)]
    [InlineData(0.75, 20000, 15000)]
    [InlineData(0, 25000, 0)]
    [InlineData(5.0, 0, 0)]
    public void TinhTienCong_NhanTongGioVoiDonGia_LamTronDong(decimal tongGio, decimal donGiaGio, decimal expectedTienCong)
    {
        decimal tienCong = PayrollCalculator.TinhTienCong(tongGio, donGiaGio);
        Assert.Equal(expectedTienCong, tienCong);
    }

    [Fact]
    public void KiemTraGioLamHopLe_CheckOutTruocCheckIn_TraVeFalseKemThongBao()
    {
        var checkIn = new DateTime(2026, 6, 1, 10, 0, 0);
        var checkOut = new DateTime(2026, 6, 1, 9, 0, 0);

        bool isValid = PayrollCalculator.KiemTraGioLamHopLe(checkIn, checkOut, out string? error);

        Assert.False(isValid);
        Assert.NotNull(error);
        Assert.Contains("không thể nhỏ hơn", error);
    }

    [Fact]
    public void KiemTraGioLamHopLe_CaLamVuotQua24h_TraVeFalseKemThongBao()
    {
        var checkIn = new DateTime(2026, 6, 1, 8, 0, 0);
        var checkOut = new DateTime(2026, 6, 2, 9, 0, 0); // 25 giờ

        bool isValid = PayrollCalculator.KiemTraGioLamHopLe(checkIn, checkOut, out string? error);

        Assert.False(isValid);
        Assert.NotNull(error);
        Assert.Contains("vượt quá 24 giờ", error);
    }

    [Fact]
    public void KiemTraGioLamHopLe_CaLamHopLe_TraVeTrueVaNoError()
    {
        var checkIn = new DateTime(2026, 6, 1, 8, 0, 0);
        var checkOut = new DateTime(2026, 6, 1, 16, 0, 0);

        bool isValid = PayrollCalculator.KiemTraGioLamHopLe(checkIn, checkOut, out string? error);

        Assert.True(isValid);
        Assert.Null(error);
    }
}
