using WebQLCHCF.Controllers;
using WebQLCHCF.Models;
using WebQLCHCF.Services;
using WebQLCHCF.Tests.TestHarness;
using Xunit;

namespace WebQLCHCF.Tests.FaultInjection;

public class FaultInjectionPromotionTests
{
    private static KhuyenMaiController CreateController()
    {
        var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var controller = new KhuyenMaiController(context, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());
        return controller;
    }

    [Fact]
    public void Promotion_InvertedDates_FailsValidation()
    {
        var controller = CreateController();
        var model = new KhuyenMai
        {
            MaKM = "KM01",
            TenKM = "KM Test",
            LoaiKM = "Phần trăm",
            GiaTri = 15,
            NgayBatDau = DateTime.Today.AddDays(5),
            NgayKetThuc = DateTime.Today, // Inverted!
            DieuKienApDung = "Test",
            TrangThai = "Đang áp dụng"
        };

        controller.ValidateGiaTri(model);

        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(KhuyenMai.NgayKetThuc)));
        Assert.Equal("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
            controller.ModelState[nameof(KhuyenMai.NgayKetThuc)]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Promotion_NegativeValue_FailsValidation()
    {
        var controller = CreateController();
        var model = new KhuyenMai
        {
            MaKM = "KM01",
            TenKM = "KM Test",
            LoaiKM = "Tiền mặt",
            GiaTri = -50000, // Negative!
            NgayBatDau = DateTime.Today,
            NgayKetThuc = DateTime.Today.AddDays(7),
            DieuKienApDung = "Test",
            TrangThai = "Đang áp dụng"
        };

        controller.ValidateGiaTri(model);

        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(KhuyenMai.GiaTri)));
        Assert.Equal("Giá trị khuyến mãi không được âm.",
            controller.ModelState[nameof(KhuyenMai.GiaTri)]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Promotion_PercentageOver100_FailsValidation()
    {
        var controller = CreateController();
        var model = new KhuyenMai
        {
            MaKM = "KM01",
            TenKM = "KM Test",
            LoaiKM = "Phần trăm",
            GiaTri = 105, // Over 100%!
            NgayBatDau = DateTime.Today,
            NgayKetThuc = DateTime.Today.AddDays(7),
            DieuKienApDung = "Test",
            TrangThai = "Đang áp dụng"
        };

        controller.ValidateGiaTri(model);

        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(KhuyenMai.GiaTri)));
        Assert.Equal("Giá trị khuyến mãi phần trăm phải từ 0 đến 100.",
            controller.ModelState[nameof(KhuyenMai.GiaTri)]!.Errors[0].ErrorMessage);
    }

    [Fact]
    public void Promotion_CashOver100Million_FailsValidation()
    {
        var controller = CreateController();
        var model = new KhuyenMai
        {
            MaKM = "KM01",
            TenKM = "KM Test",
            LoaiKM = "Tiền mặt",
            GiaTri = 120_000_000m, // Over 100M!
            NgayBatDau = DateTime.Today,
            NgayKetThuc = DateTime.Today.AddDays(7),
            DieuKienApDung = "Test",
            TrangThai = "Đang áp dụng"
        };

        controller.ValidateGiaTri(model);

        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(KhuyenMai.GiaTri)));
        Assert.Equal("Giá trị khuyến mãi tiền mặt không được vượt quá 100.000.000đ.",
            controller.ModelState[nameof(KhuyenMai.GiaTri)]!.Errors[0].ErrorMessage);
    }

    [Theory]
    [InlineData("Voucher")]
    [InlineData("GiamGiaDacBiet")]
    [InlineData("")]
    public void Promotion_InvalidType_FailsValidation(string invalidType)
    {
        var controller = CreateController();
        var model = new KhuyenMai
        {
            MaKM = "KM01",
            TenKM = "KM Test",
            LoaiKM = invalidType,
            GiaTri = 50000,
            NgayBatDau = DateTime.Today,
            NgayKetThuc = DateTime.Today.AddDays(7),
            DieuKienApDung = "Test",
            TrangThai = "Đang áp dụng"
        };

        controller.ValidateGiaTri(model);

        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(KhuyenMai.LoaiKM)));
        Assert.Equal("Loại khuyến mãi chỉ được chọn Phần trăm hoặc Tiền mặt.",
            controller.ModelState[nameof(KhuyenMai.LoaiKM)]!.Errors[0].ErrorMessage);
    }

    [Theory]
    [InlineData("Phần trăm", 0)]
    [InlineData("Phần trăm", 100)]
    [InlineData("Tiền mặt", 0)]
    [InlineData("Tiền mặt", 100_000_000)]
    public void Promotion_ValidBoundaryValues_PassValidation(string loaiKM, decimal giaTri)
    {
        var controller = CreateController();
        var model = new KhuyenMai
        {
            MaKM = "KM01",
            TenKM = "KM Boundary",
            LoaiKM = loaiKM,
            GiaTri = giaTri,
            NgayBatDau = DateTime.Today,
            NgayKetThuc = DateTime.Today, // Same day is valid!
            DieuKienApDung = "Test",
            TrangThai = "Đang áp dụng"
        };

        controller.ValidateGiaTri(model);

        Assert.True(controller.ModelState.IsValid);
    }
}
