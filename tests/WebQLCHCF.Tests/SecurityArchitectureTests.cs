using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using WebQLCHCF.Controllers;
using WebQLCHCF.Security;
using Xunit;

namespace WebQLCHCF.Tests;

public class SecurityArchitectureTests
{
    private static readonly Type[] All18ControllerTypes =
    {
        typeof(AccountController),
        typeof(AuditLogController),
        typeof(BanController),
        typeof(BaseController),
        typeof(CongThucController),
        typeof(HoaDonController),
        typeof(HomeController),
        typeof(KhachHangController),
        typeof(KhuyenMaiController),
        typeof(LoaiSPController),
        typeof(LuongController),
        typeof(NguyenLieuController),
        typeof(NhanVienController),
        typeof(OrderController),
        typeof(PhieuNhapController),
        typeof(SanPhamController),
        typeof(TaiKhoanController),
        typeof(ThongKeController),
    };

    [Fact]
    public void All18Controllers_ExistAndAreAccountedFor()
    {
        var discoveredControllers = typeof(BaseController).Assembly
            .GetTypes()
            .Where(t => typeof(Controller).IsAssignableFrom(t) && t.Name.EndsWith("Controller"))
            .ToList();

        Assert.Equal(18, discoveredControllers.Count);
        Assert.Equal(18, All18ControllerTypes.Length);

        foreach (var expected in All18ControllerTypes)
        {
            Assert.Contains(expected, discoveredControllers);
        }
    }

    [Fact]
    public void All18Controllers_HaveRoleAuthorizeOrAllowAnonymous_AtClassLevel()
    {
        foreach (var controllerType in All18ControllerTypes)
        {
            bool hasRoleAuthorize = controllerType.GetCustomAttribute<RoleAuthorizeAttribute>(inherit: false) != null;
            bool hasAllowAnonymous = controllerType.GetCustomAttribute<AllowAnonymousAttribute>(inherit: false) != null;

            Assert.True(
                hasRoleAuthorize || hasAllowAnonymous,
                $"Controller '{controllerType.Name}' must be decorated with either [RoleAuthorize] or [AllowAnonymous] at class level."
            );
        }
    }

    [Theory]
    [InlineData(typeof(AccountController), true, false)]
    [InlineData(typeof(BaseController), false, true)]
    [InlineData(typeof(AuditLogController), false, true)]
    [InlineData(typeof(BanController), false, true)]
    [InlineData(typeof(CongThucController), false, true)]
    [InlineData(typeof(HoaDonController), false, true)]
    [InlineData(typeof(HomeController), false, true)]
    [InlineData(typeof(KhachHangController), false, true)]
    [InlineData(typeof(KhuyenMaiController), false, true)]
    [InlineData(typeof(LoaiSPController), false, true)]
    [InlineData(typeof(LuongController), false, true)]
    [InlineData(typeof(NguyenLieuController), false, true)]
    [InlineData(typeof(NhanVienController), false, true)]
    [InlineData(typeof(OrderController), false, true)]
    [InlineData(typeof(PhieuNhapController), false, true)]
    [InlineData(typeof(SanPhamController), false, true)]
    [InlineData(typeof(TaiKhoanController), false, true)]
    [InlineData(typeof(ThongKeController), false, true)]
    public void IndividualController_HasExpectedSecurityAnnotation(
        Type controllerType,
        bool expectAllowAnonymous,
        bool expectRoleAuthorize)
    {
        bool hasAllowAnonymous = controllerType.GetCustomAttribute<AllowAnonymousAttribute>(inherit: false) != null;
        bool hasRoleAuthorize = controllerType.GetCustomAttribute<RoleAuthorizeAttribute>(inherit: false) != null;

        Assert.Equal(expectAllowAnonymous, hasAllowAnonymous);
        Assert.Equal(expectRoleAuthorize, hasRoleAuthorize);
    }

    [Fact]
    public void AllStateAlteringPostActions_HaveValidateAntiForgeryToken()
    {
        var postActions = All18ControllerTypes
            .SelectMany(c => c.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            .Where(m => m.GetCustomAttribute<HttpPostAttribute>() != null)
            .ToList();

        Assert.True(postActions.Count >= 20, $"Expected at least 20 POST actions, but found {postActions.Count}.");

        foreach (var method in postActions)
        {
            var antiForgery = method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>();
            var declaringController = method.DeclaringType?.Name ?? "Unknown";

            Assert.True(
                antiForgery != null,
                $"POST action '{declaringController}.{method.Name}' is missing [ValidateAntiForgeryToken]."
            );
        }
    }

    [Fact]
    public void AllReadActions_HaveExplicitHttpGet()
    {
        var readActions = All18ControllerTypes
            .Where(t => !t.IsAbstract)
            .SelectMany(c => c.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            .Where(m => m.ReturnType == typeof(IActionResult) || m.ReturnType == typeof(Task<IActionResult>))
            .Where(m => m.GetCustomAttribute<HttpPostAttribute>() == null && m.GetCustomAttribute<NonActionAttribute>() == null)
            .ToList();

        Assert.True(readActions.Count >= 25, $"Expected at least 25 read actions, but found {readActions.Count}.");

        foreach (var method in readActions)
        {
            var httpGet = method.GetCustomAttribute<HttpGetAttribute>();
            var declaringController = method.DeclaringType?.Name ?? "Unknown";

            Assert.True(
                httpGet != null,
                $"Read action '{declaringController}.{method.Name}' is missing explicit [HttpGet]."
            );
        }
    }
}
