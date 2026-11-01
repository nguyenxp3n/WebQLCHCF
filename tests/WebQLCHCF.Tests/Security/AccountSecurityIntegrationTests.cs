using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebQLCHCF.Controllers;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.Tests.TestHarness;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Tests.Security;

public class AccountSecurityIntegrationTests
{
    private readonly PasswordHasherService _hasher = new();
    private readonly TestAuditLogServiceDouble _auditLog = new();
    private readonly TestChamCongServiceDouble _chamCong = new();

    [Fact]
    public async Task Login_Post_WithHashedPassword_LogsInSuccessfully()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var account = new TaiKhoan
        {
            MaTK = "TK01",
            TenDangNhap = "qly_tram",
            MatKhau = "", // will hash below
            MaNV = "NV01",
            VaiTro = AppRoles.QuanLy
        };
        account.MatKhau = _hasher.HashPassword(account, "Secure@1234");
        context.TaiKhoans.Add(account);
        context.NhanViens.Add(new NhanVien { MaNV = "NV01", TenNV = "Khánh Trâm", ChucVu = "Quản Lý", GioiTinh = "Nữ" });
        await context.SaveChangesAsync();

        var controller = new AccountController(context, _chamCong, _auditLog, _hasher);
        var httpContext = TestHttpContextFactory.CreateAnonymous();
        TestControllerContextFactory.SetupController(controller, httpContext);

        var result = await controller.Login(new LoginViewModel
        {
            TenDangNhap = "qly_tram",
            MatKhau = "Secure@1234"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
        Assert.Equal("TK01", httpContext.Session.GetString("MaTK"));
        Assert.Equal("qly_tram", httpContext.Session.GetString("TenDangNhap"));
        Assert.Equal(AppRoles.QuanLy, httpContext.Session.GetString("VaiTro"));
    }

    [Fact]
    public async Task Login_Post_WithLegacyPlaintextPassword_LogsInAndUpgradesHashInDatabase()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var legacyPassword = "Password1234@";
        var account = new TaiKhoan
        {
            MaTK = "TK02",
            TenDangNhap = "pv_tu",
            MatKhau = legacyPassword, // Legacy unhashed plaintext password
            MaNV = "NV04",
            VaiTro = AppRoles.PhucVu
        };
        context.TaiKhoans.Add(account);
        context.NhanViens.Add(new NhanVien { MaNV = "NV04", TenNV = "Thanh Tú", ChucVu = "Phục Vụ", GioiTinh = "Nam" });
        await context.SaveChangesAsync();

        var controller = new AccountController(context, _chamCong, _auditLog, _hasher);
        var httpContext = TestHttpContextFactory.CreateAnonymous();
        TestControllerContextFactory.SetupController(controller, httpContext);

        var result = await controller.Login(new LoginViewModel
        {
            TenDangNhap = "pv_tu",
            MatKhau = legacyPassword
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Create", redirect.ActionName);
        Assert.Equal("Order", redirect.ControllerName);

        // Verify password in database was automatically upgraded to PBKDF2 hash!
        var updatedAccount = context.TaiKhoans.First(x => x.MaTK == "TK02");
        Assert.NotEqual(legacyPassword, updatedAccount.MatKhau);

        // Verify upgraded hash validates correctly
        var (succeeded, needsRehash) = _hasher.VerifyHashedPassword(updatedAccount, updatedAccount.MatKhau, legacyPassword);
        Assert.True(succeeded);
        Assert.False(needsRehash);
    }

    [Fact]
    public async Task Login_Post_WrongPassword_ReturnsViewWithError()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var account = new TaiKhoan
        {
            MaTK = "TK03",
            TenDangNhap = "pc_mai",
            MatKhau = "PlainSecret1@",
            MaNV = "NV02",
            VaiTro = AppRoles.PhaChe
        };
        context.TaiKhoans.Add(account);
        await context.SaveChangesAsync();

        var controller = new AccountController(context, _chamCong, _auditLog, _hasher);
        var httpContext = TestHttpContextFactory.CreateAnonymous();
        TestControllerContextFactory.SetupController(controller, httpContext);

        var result = await controller.Login(new LoginViewModel
        {
            TenDangNhap = "pc_mai",
            MatKhau = "WrongPassword999@"
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Null(httpContext.Session.GetString("MaTK"));
    }

    [Fact]
    public async Task ChangePassword_Post_ValidCurrentPassword_UpdatesToHashedPassword()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var oldPassword = "CurrentPass123@";
        var newPassword = "BrandNewPass456@";

        var account = new TaiKhoan
        {
            MaTK = "TK05",
            TenDangNhap = "qly_tram",
            MatKhau = oldPassword,
            MaNV = "NV01",
            VaiTro = AppRoles.QuanLy
        };
        context.TaiKhoans.Add(account);
        await context.SaveChangesAsync();

        var controller = new AccountController(context, _chamCong, _auditLog, _hasher);
        var httpContext = TestHttpContextFactory.CreateQuanLy(maTK: "TK05");
        httpContext.Session.SetString("TenDangNhap", "qly_tram");
        TestControllerContextFactory.SetupController(controller, httpContext);

        var result = await controller.ChangePassword(new ChangePasswordViewModel
        {
            MatKhauHienTai = oldPassword,
            MatKhauMoi = newPassword,
            XacNhanMatKhauMoi = newPassword
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ChangePassword", redirect.ActionName);

        var updatedAccount = context.TaiKhoans.First(x => x.MaTK == "TK05");
        Assert.NotEqual(oldPassword, updatedAccount.MatKhau);
        Assert.NotEqual(newPassword, updatedAccount.MatKhau);

        // Verify the stored password is a valid hash of newPassword
        var (succeeded, _) = _hasher.VerifyHashedPassword(updatedAccount, updatedAccount.MatKhau, newPassword);
        Assert.True(succeeded);
    }

    [Fact]
    public async Task ChangePassword_Post_WrongCurrentPassword_FailsWithModelError()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var account = new TaiKhoan
        {
            MaTK = "TK06",
            TenDangNhap = "pv_vy",
            MatKhau = _hasher.HashPassword(new TaiKhoan(), "CorrectCurrent123@"),
            MaNV = "NV05",
            VaiTro = AppRoles.PhucVu
        };
        context.TaiKhoans.Add(account);
        await context.SaveChangesAsync();

        var controller = new AccountController(context, _chamCong, _auditLog, _hasher);
        var httpContext = TestHttpContextFactory.CreatePhucVu(maTK: "TK06");
        httpContext.Session.SetString("TenDangNhap", "pv_vy");
        TestControllerContextFactory.SetupController(controller, httpContext);

        var result = await controller.ChangePassword(new ChangePasswordViewModel
        {
            MatKhauHienTai = "WrongCurrentPass999@",
            MatKhauMoi = "NewValidPass456@",
            XacNhanMatKhauMoi = "NewValidPass456@"
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(ChangePasswordViewModel.MatKhauHienTai)));
    }
}
