using Microsoft.AspNetCore.Mvc;
using WebQLCHCF.Controllers;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.Tests.TestHarness;
using WebQLCHCF.ViewModels;
using Xunit;

namespace WebQLCHCF.Tests.Controllers;

public class ControllerIntegrationTests
{
    private class TestSampleBaseController : BaseController
    {
        public IActionResult TestAction() => Ok("Success");
    }

    #region 1. BaseController Tests
    [Fact]
    public void BaseController_AnonymousUser_IsInterceptedAndRedirectedToLogin()
    {
        var controller = new TestSampleBaseController();
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateAnonymous());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(TestSampleBaseController.TestAction));

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Fact]
    public void BaseController_AuthenticatedUser_PassesSecurityPipeline()
    {
        var controller = new TestSampleBaseController();
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(TestSampleBaseController.TestAction));
        Assert.Null(result); // Allowed to proceed
    }
    #endregion

    #region 2. AccountController Tests
    [Fact]
    public void AccountController_Login_Get_AnonymousReturnsViewResult()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var chamCong = new TestChamCongServiceDouble();
        var audit = new TestAuditLogServiceDouble();

        var controller = new AccountController(context, chamCong, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateAnonymous());

        var result = controller.Login();
        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<LoginViewModel>(view.Model);
    }

    [Fact]
    public void AccountController_Login_Get_AlreadyLoggedInQuanLyRedirectsToHome()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var chamCong = new TestChamCongServiceDouble();
        var audit = new TestAuditLogServiceDouble();

        var controller = new AccountController(context, chamCong, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = controller.Login();
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public void AccountController_AccessDenied_ReturnsViewWith403()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var chamCong = new TestChamCongServiceDouble();
        var audit = new TestAuditLogServiceDouble();

        var controller = new AccountController(context, chamCong, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = controller.AccessDenied();
        Assert.IsType<ViewResult>(result);
        Assert.Equal(403, controller.Response.StatusCode);
    }

    [Fact]
    public async Task AccountController_Login_Post_InvalidModelState_ReturnsView()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var chamCong = new TestChamCongServiceDouble();
        var audit = new TestAuditLogServiceDouble();

        var controller = new AccountController(context, chamCong, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateAnonymous());
        controller.ModelState.AddModelError("TenDangNhap", "Tên đăng nhập không được để trống");

        var model = new LoginViewModel();
        var result = await controller.Login(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, view.Model);
    }
    #endregion

    #region 3. AuditLogController Tests
    [Fact]
    public void AuditLogController_UnauthorizedRole_RedirectsToAccessDenied()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new AuditLogController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(AuditLogController.Index));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Fact]
    public void AuditLogController_QuanLy_PassesSecurityPipeline()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new AuditLogController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(AuditLogController.Index));
        Assert.Null(result);
    }
    #endregion

    #region 4. BanController Tests
    [Fact]
    public async Task BanController_Details_NullId_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var controller = new BanController(context, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void BanController_Create_PhucVuRole_RedirectsToAccessDenied()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var controller = new BanController(context, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(BanController.Create));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }

    [Fact]
    public async Task BanController_Create_Post_InvalidModel_ReturnsView()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var controller = new BanController(context, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());
        controller.ModelState.AddModelError("TenBan", "Tên bàn không được để trống");

        var model = new Ban();
        var result = await controller.Create(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, view.Model);
    }
    #endregion

    #region 5. CongThucController Tests
    [Fact]
    public void CongThucController_PhucVuRole_IsDeniedAccess()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new CongThucController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(CongThucController.Index));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }

    [Fact]
    public async Task CongThucController_Details_NullMaSP_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new CongThucController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }
    #endregion

    #region 6. HoaDonController Tests
    [Fact]
    public async Task HoaDonController_Details_NullId_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new HoaDonController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void HoaDonController_Edit_PhucVuRole_RedirectsToAccessDenied()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new HoaDonController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(HoaDonController.Edit), "HD00001");
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }
    #endregion

    #region 7. HomeController Tests
    [Fact]
    public async Task HomeController_Index_QuanLy_ReturnsDashboardView()
    {
        var dashboard = new TestDashboardServiceDouble();
        var controller = new HomeController(dashboard);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = await controller.Index();
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DashboardViewModel>(view.Model);
        Assert.Equal(3500000, model.DoanhThuHomNay);
    }

    [Fact]
    public void HomeController_Index_PhucVu_RedirectsToAccessDenied()
    {
        var dashboard = new TestDashboardServiceDouble();
        var controller = new HomeController(dashboard);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(HomeController.Index));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }

    [Fact]
    public void HomeController_Error_AllowAnonymous_ReturnsViewResult()
    {
        var dashboard = new TestDashboardServiceDouble();
        var controller = new HomeController(dashboard);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateAnonymous());

        var result = controller.Error();
        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<ErrorViewModel>(view.Model);
    }
    #endregion

    #region 8. KhachHangController Tests
    [Fact]
    public async Task KhachHangController_Details_NullId_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new KhachHangController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void KhachHangController_Delete_PhucVu_RedirectsToAccessDenied()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new KhachHangController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(KhachHangController.Delete), "KH01");
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }
    #endregion

    #region 9. KhuyenMaiController Tests
    [Fact]
    public async Task KhuyenMaiController_Details_NullId_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var controller = new KhuyenMaiController(context, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void KhuyenMaiController_PhucVu_RedirectsToAccessDenied()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var controller = new KhuyenMaiController(context, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(KhuyenMaiController.Index));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }
    #endregion

    #region 10. LoaiSPController Tests
    [Fact]
    public async Task LoaiSPController_Details_NullId_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new LoaiSPController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task LoaiSPController_Create_Post_InvalidModel_ReturnsView()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new LoaiSPController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());
        controller.ModelState.AddModelError("TenLoaiSP", "Tên loại không được để trống");

        var model = new LoaiSP();
        var result = await controller.Create(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, view.Model);
    }
    #endregion

    #region 11. LuongController Tests
    [Fact]
    public async Task LuongController_Details_NullId_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new LuongController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void LuongController_PhucVuRole_IsDeniedAccess()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new LuongController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(LuongController.Index));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }
    #endregion

    #region 12. NguyenLieuController Tests
    [Fact]
    public void NguyenLieuController_PhaChe_AllowedIndexAndDetails()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new NguyenLieuController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhaChe());

        var indexResult = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(NguyenLieuController.Index));
        Assert.Null(indexResult); // Allowed

        var detailsResult = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(NguyenLieuController.Details), "NL01");
        Assert.Null(detailsResult); // Allowed
    }

    [Fact]
    public void NguyenLieuController_Create_PhaChe_RedirectsToAccessDenied()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new NguyenLieuController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhaChe());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(NguyenLieuController.Create));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }
    #endregion

    #region 13. NhanVienController Tests
    [Fact]
    public async Task NhanVienController_Details_NullId_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new NhanVienController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void NhanVienController_PhucVuRole_RedirectsToAccessDenied()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new NhanVienController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(NhanVienController.Index));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }
    #endregion

    #region 14. OrderController Tests
    [Fact]
    public void OrderController_Index_RedirectsToCreate()
    {
        var orderService = new InMemoryOrderServiceDouble();
        var audit = new TestAuditLogServiceDouble();
        var controller = new OrderController(orderService, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = controller.Index();
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Create", redirect.ActionName);
    }

    [Fact]
    public async Task OrderController_Create_Get_WhenCheckedIn_ReturnsViewWithModel()
    {
        var orderService = new InMemoryOrderServiceDouble();
        var audit = new TestAuditLogServiceDouble();
        var controller = new OrderController(orderService, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = await controller.Create((string?)null);
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OrderCreateViewModel>(view.Model);
        Assert.True(controller.ViewBag.CanOrder);
        Assert.NotEmpty(model.DanhSachSanPhamDangBan);
    }

    [Fact]
    public async Task OrderController_PayCash_Post_CallsServiceAndRedirectsToReceipt()
    {
        var orderService = new InMemoryOrderServiceDouble();
        var audit = new TestAuditLogServiceDouble();
        var controller = new OrderController(orderService, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        // Create an order first
        var createModel = new OrderCreateViewModel
        {
            KhuVuc = "Tầng trệt",
            Items = new List<OrderItemViewModel>
            {
                new() { MaSP = "SP01", SoLuong = 2 }
            }
        };
        var maHD = await orderService.CreateOrderAsync(createModel, "NV02");

        var result = await controller.PayCash(maHD, "Khách trả tiền mặt");
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Receipt", redirect.ActionName);
        Assert.Equal(maHD, redirect.RouteValues?["id"]);
    }

    [Fact]
    public async Task OrderController_SearchCustomers_ReturnsJsonResults()
    {
        var orderService = new InMemoryOrderServiceDouble();
        var audit = new TestAuditLogServiceDouble();
        var controller = new OrderController(orderService, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = await controller.SearchCustomers("090");
        var jsonResult = Assert.IsType<JsonResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<OrderCustomerViewModel>>(jsonResult.Value);
        Assert.NotEmpty(list);
    }
    #endregion

    #region 15. PhieuNhapController Tests
    [Fact]
    public async Task PhieuNhapController_Details_NullId_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new PhieuNhapController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void PhieuNhapController_PhucVuRole_IsDeniedAccess()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new PhieuNhapController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(PhieuNhapController.Index));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }
    #endregion

    #region 16. SanPhamController Tests
    [Fact]
    public async Task SanPhamController_Details_NullId_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var controller = new SanPhamController(context, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void SanPhamController_PhucVuRole_RedirectsToAccessDenied()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var controller = new SanPhamController(context, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(SanPhamController.Index));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }
    #endregion

    #region 17. TaiKhoanController Tests
    [Fact]
    public async Task TaiKhoanController_Details_NullId_ReturnsNotFound()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var controller = new TaiKhoanController(context, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void TaiKhoanController_PhucVuRole_RedirectsToAccessDenied()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var audit = new TestAuditLogServiceDouble();
        var controller = new TaiKhoanController(context, audit);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(TaiKhoanController.Index));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }
    #endregion

    #region 18. ThongKeController Tests
    [Fact]
    public void ThongKeController_Index_RedirectsToDoanhThuTheoNgay()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new ThongKeController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreateQuanLy());

        var result = controller.Index();
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("DoanhThuTheoNgay", redirect.ActionName);
    }

    [Fact]
    public void ThongKeController_PhucVuRole_RedirectsToAccessDenied()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var controller = new ThongKeController(context);
        TestControllerContextFactory.SetupController(controller, TestHttpContextFactory.CreatePhucVu());

        var result = TestControllerContextFactory.ExecuteSecurityPipeline(controller, nameof(ThongKeController.Index));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }
    #endregion
}
