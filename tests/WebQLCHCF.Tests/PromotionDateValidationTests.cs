using WebQLCHCF.Controllers;
using WebQLCHCF.Models;
using Xunit;

namespace WebQLCHCF.Tests;

public class PromotionDateValidationTests
{
    private static KhuyenMaiController CreateController() => new(null!, null!);

    [Theory]
    [InlineData("2026-06-01", "2026-06-30")] // Start strictly before end
    [InlineData("2026-06-15", "2026-06-15")] // Single-day campaign (start == end)
    [InlineData("2026-02-28", "2026-03-01")] // Month-boundary transition
    [InlineData("2026-12-31", "2027-01-01")] // Year-boundary transition
    public void ValidateDateRange_ValidDateRanges_PassesValidation(string startDateStr, string endDateStr)
    {
        var controller = CreateController();
        var model = new KhuyenMai
        {
            TenKM = "Khuyến mãi mùa hè",
            LoaiKM = "Phần trăm",
            GiaTri = 10,
            NgayBatDau = DateTime.Parse(startDateStr),
            NgayKetThuc = DateTime.Parse(endDateStr),
            DieuKienApDung = "Tất cả khách hàng",
            TrangThai = "Đang áp dụng",
        };

        controller.ValidateGiaTri(model);

        Assert.True(controller.ModelState.IsValid, "Expected valid model state for correct date ranges.");
        Assert.False(controller.ModelState.ContainsKey(nameof(KhuyenMai.NgayKetThuc)));
    }

    [Theory]
    [InlineData("2026-06-30", "2026-06-01")] // Start 29 days after end
    [InlineData("2026-06-16", "2026-06-15")] // Start 1 day after end
    [InlineData("2027-01-01", "2026-12-31")] // Year boundary inversion
    public void ValidateDateRange_InvalidDateRanges_RejectsWithFriendlyError(string startDateStr, string endDateStr)
    {
        var controller = CreateController();
        var model = new KhuyenMai
        {
            TenKM = "Khuyến mãi sai ngày",
            LoaiKM = "Phần trăm",
            GiaTri = 15,
            NgayBatDau = DateTime.Parse(startDateStr),
            NgayKetThuc = DateTime.Parse(endDateStr),
            DieuKienApDung = "Mọi đơn",
            TrangThai = "Đang áp dụng",
        };

        controller.ValidateGiaTri(model);

        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(KhuyenMai.NgayKetThuc)));

        var error = controller.ModelState[nameof(KhuyenMai.NgayKetThuc)]?.Errors.FirstOrDefault();
        Assert.NotNull(error);
        Assert.Equal("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.", error.ErrorMessage);
    }

    [Theory]
    [InlineData("Phần trăm", 20, "2026-07-01", "2026-06-01", false)]
    [InlineData("Tiền mặt", 15000, "2026-07-01", "2026-06-01", false)]
    [InlineData("Phần trăm", 20, "2026-06-01", "2026-07-01", true)]
    [InlineData("Tiền mặt", 15000, "2026-06-01", "2026-07-01", true)]
    public void ValidateDateRange_WorksConsistentlyAcrossBothPromotionTypes(
        string loaiKM,
        decimal giaTri,
        string startDateStr,
        string endDateStr,
        bool expectValid)
    {
        var controller = CreateController();
        var model = new KhuyenMai
        {
            TenKM = "Khuyến mãi test loại",
            LoaiKM = loaiKM,
            GiaTri = giaTri,
            NgayBatDau = DateTime.Parse(startDateStr),
            NgayKetThuc = DateTime.Parse(endDateStr),
            DieuKienApDung = "Đơn từ 50.000đ",
            TrangThai = "Đang áp dụng",
        };

        controller.ValidateGiaTri(model);

        if (expectValid)
        {
            Assert.True(controller.ModelState.IsValid);
        }
        else
        {
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(nameof(KhuyenMai.NgayKetThuc)));
            Assert.Equal(
                "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
                controller.ModelState[nameof(KhuyenMai.NgayKetThuc)]?.Errors[0].ErrorMessage
            );
        }
    }

    [Fact]
    public void ValidateDateRange_MultipleValidationErrors_CapturedSimultaneously()
    {
        var controller = CreateController();
        var model = new KhuyenMai
        {
            TenKM = "Khuyến mãi nhiều lỗi",
            LoaiKM = "Phần trăm",
            GiaTri = -10, // Negative value error
            NgayBatDau = new DateTime(2026, 8, 10),
            NgayKetThuc = new DateTime(2026, 8, 5), // Invalid date range error
        };

        controller.ValidateGiaTri(model);

        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(KhuyenMai.GiaTri)));
        Assert.True(controller.ModelState.ContainsKey(nameof(KhuyenMai.NgayKetThuc)));
        Assert.Equal("Giá trị khuyến mãi không được âm.", controller.ModelState[nameof(KhuyenMai.GiaTri)]?.Errors[0].ErrorMessage);
        Assert.Equal("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.", controller.ModelState[nameof(KhuyenMai.NgayKetThuc)]?.Errors[0].ErrorMessage);
    }
}
