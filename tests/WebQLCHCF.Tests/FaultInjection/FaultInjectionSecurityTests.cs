using Microsoft.AspNetCore.Mvc;
using WebQLCHCF.Controllers;
using WebQLCHCF.Security;
using WebQLCHCF.Tests.TestHarness;
using Xunit;

namespace WebQLCHCF.Tests.FaultInjection;

public class FaultInjectionSecurityTests
{
    [Theory]
    [InlineData(typeof(LuongController))]
    [InlineData(typeof(ThongKeController))]
    [InlineData(typeof(TaiKhoanController))]
    [InlineData(typeof(AuditLogController))]
    [InlineData(typeof(CongThucController))]
    [InlineData(typeof(PhieuNhapController))]
    [InlineData(typeof(KhuyenMaiController))]
    [InlineData(typeof(SanPhamController))]
    [InlineData(typeof(NhanVienController))]
    [InlineData(typeof(HomeController))]
    public void PhucVuRole_AttemptingRestrictedControllerIndex_IsStrictlyRejected(Type controllerType)
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var dashboard = new TestDashboardServiceDouble();

        Controller controller = controllerType switch
        {
            _ when controllerType == typeof(LuongController) => new LuongController(context),
            _ when controllerType == typeof(ThongKeController) => new ThongKeController(context),
            _ when controllerType == typeof(TaiKhoanController) => new TaiKhoanController(context, audit),
            _ when controllerType == typeof(AuditLogController) => new AuditLogController(context),
            _ when controllerType == typeof(CongThucController) => new CongThucController(context),
            _ when controllerType == typeof(PhieuNhapController) => new PhieuNhapController(context),
            _ when controllerType == typeof(KhuyenMaiController) => new KhuyenMaiController(context, audit),
            _ when controllerType == typeof(SanPhamController) => new SanPhamController(context, audit),
            _ when controllerType == typeof(NhanVienController) => new NhanVienController(context),
            _ when controllerType == typeof(HomeController) => new HomeController(dashboard),
            _ => throw new ArgumentException("Unknown controller")
        };

        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, "Index");
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Theory]
    [InlineData(typeof(LuongController))]
    [InlineData(typeof(ThongKeController))]
    [InlineData(typeof(TaiKhoanController))]
    [InlineData(typeof(AuditLogController))]
    [InlineData(typeof(BanController))]
    [InlineData(typeof(OrderController))]
    [InlineData(typeof(HoaDonController))]
    public void PhaCheRole_AttemptingUnauthorizedController_IsStrictlyRejected(Type controllerType)
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var orderService = new InMemoryOrderServiceDouble();

        Controller controller = controllerType switch
        {
            _ when controllerType == typeof(LuongController) => new LuongController(context),
            _ when controllerType == typeof(ThongKeController) => new ThongKeController(context),
            _ when controllerType == typeof(TaiKhoanController) => new TaiKhoanController(context, audit),
            _ when controllerType == typeof(AuditLogController) => new AuditLogController(context),
            _ when controllerType == typeof(BanController) => new BanController(context, audit),
            _ when controllerType == typeof(OrderController) => new OrderController(orderService, audit),
            _ when controllerType == typeof(HoaDonController) => new HoaDonController(context),
            _ => throw new ArgumentException("Unknown controller")
        };

        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhaChe());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, "Index");
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Theory]
    [InlineData(typeof(BanController), "Index")]
    [InlineData(typeof(HoaDonController), "Index")]
    [InlineData(typeof(OrderController), "Index")]
    [InlineData(typeof(KhachHangController), "Index")]
    [InlineData(typeof(NguyenLieuController), "Index")]
    [InlineData(typeof(SanPhamController), "Index")]
    public void AnonymousUser_AttemptingProtectedController_IsRedirectedToLogin(Type controllerType, string actionName)
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var orderService = new InMemoryOrderServiceDouble();

        Controller controller = controllerType switch
        {
            _ when controllerType == typeof(BanController) => new BanController(context, audit),
            _ when controllerType == typeof(HoaDonController) => new HoaDonController(context),
            _ when controllerType == typeof(OrderController) => new OrderController(orderService, audit),
            _ when controllerType == typeof(KhachHangController) => new KhachHangController(context),
            _ when controllerType == typeof(NguyenLieuController) => new NguyenLieuController(context),
            _ when controllerType == typeof(SanPhamController) => new SanPhamController(context, audit),
            _ => throw new ArgumentException("Unknown controller")
        };

        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateAnonymous());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, actionName);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }
}
