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

public class RolePermissionTests
{
    [Fact]
    public void AppRoles_ChuaDu3VaiTroNghiepVuCoDinh()
    {
        Assert.Equal("Quản Lý", AppRoles.QuanLy);
        Assert.Equal("Pha Chế", AppRoles.PhaChe);
        Assert.Equal("Phục Vụ", AppRoles.PhucVu);
        Assert.Equal(3, AppRoles.All.Length);
    }

    [Theory]
    [InlineData("Quản Lý", true)]
    [InlineData("Pha Chế", true)]
    [InlineData("Phục Vụ", true)]
    [InlineData("Khách Hàng", false)]
    [InlineData("Admin", false)]
    [InlineData("", false)]
    public void AppRoles_KiemTraVaiTroThuocHeThong(string role, bool expected)
    {
        bool exists = AppRoles.All.Contains(role);
        Assert.Equal(expected, exists);
    }

    [Fact]
    public void RoleAuthorize_WithAllowAnonymous_BypassesAuthorization()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new FakeSession();
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = new List<object> { new AllowAnonymousAttribute() }
        };
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        var attribute = new RoleAuthorizeAttribute(AppRoles.QuanLy);
        attribute.OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void RoleAuthorize_WithoutSession_RedirectsToLogin()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new FakeSession();
        var actionDescriptor = new ActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        var attribute = new RoleAuthorizeAttribute(AppRoles.QuanLy);
        attribute.OnActionExecuting(context);

        var redirect = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("Login", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Fact]
    public void RoleAuthorize_WithUnauthorizedRole_RedirectsToAccessDenied()
    {
        var httpContext = new DefaultHttpContext();
        var session = new FakeSession();
        session.SetString("MaTK", "TK01");
        session.SetString("VaiTro", AppRoles.PhucVu);
        httpContext.Session = session;

        var actionDescriptor = new ActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        var attribute = new RoleAuthorizeAttribute(AppRoles.QuanLy);
        attribute.OnActionExecuting(context);

        var redirect = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("AccessDenied", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Fact]
    public void RoleAuthorize_WithAuthorizedRole_AllowsExecution()
    {
        var httpContext = new DefaultHttpContext();
        var session = new FakeSession();
        session.SetString("MaTK", "TK01");
        session.SetString("VaiTro", AppRoles.QuanLy);
        httpContext.Session = session;

        var actionDescriptor = new ActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );

        var attribute = new RoleAuthorizeAttribute(AppRoles.QuanLy);
        attribute.OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void ControllerGuards_HoaDonController_HasExpectedRoleAuthorize()
    {
        var classAttr = typeof(HoaDonController).GetCustomAttribute<RoleAuthorizeAttribute>();
        Assert.NotNull(classAttr);

        var editGet = typeof(HoaDonController).GetMethod(nameof(HoaDonController.Edit), new[] { typeof(string) });
        Assert.NotNull(editGet?.GetCustomAttribute<RoleAuthorizeAttribute>());

        var deleteConfirmed = typeof(HoaDonController).GetMethod(nameof(HoaDonController.DeleteConfirmed), new[] { typeof(string) });
        Assert.NotNull(deleteConfirmed?.GetCustomAttribute<RoleAuthorizeAttribute>());
    }

    [Fact]
    public void ControllerGuards_BanController_HasExpectedRoleAuthorize()
    {
        var classAttr = typeof(BanController).GetCustomAttribute<RoleAuthorizeAttribute>();
        Assert.NotNull(classAttr);

        var createGet = typeof(BanController).GetMethod(nameof(BanController.Create), Type.EmptyTypes);
        Assert.NotNull(createGet?.GetCustomAttribute<RoleAuthorizeAttribute>());

        var editGet = typeof(BanController).GetMethod(nameof(BanController.Edit), new[] { typeof(string) });
        Assert.NotNull(editGet?.GetCustomAttribute<RoleAuthorizeAttribute>());
    }

    [Fact]
    public void ControllerGuards_KhachHangController_HasExpectedRoleAuthorize()
    {
        var classAttr = typeof(KhachHangController).GetCustomAttribute<RoleAuthorizeAttribute>();
        Assert.NotNull(classAttr);

        var deleteConfirmed = typeof(KhachHangController).GetMethod(nameof(KhachHangController.DeleteConfirmed), new[] { typeof(string) });
        Assert.NotNull(deleteConfirmed?.GetCustomAttribute<RoleAuthorizeAttribute>());
    }

    [Fact]
    public void ControllerGuards_NguyenLieuController_HasExpectedRoleAuthorize()
    {
        var classAttr = typeof(NguyenLieuController).GetCustomAttribute<RoleAuthorizeAttribute>();
        Assert.NotNull(classAttr);

        var createGet = typeof(NguyenLieuController).GetMethod(nameof(NguyenLieuController.Create), Type.EmptyTypes);
        Assert.NotNull(createGet?.GetCustomAttribute<RoleAuthorizeAttribute>());
    }

    [Fact]
    public void ControllerGuards_OrderAndKhuyenMai_HaveExpectedRoleAuthorize()
    {
        var orderAttr = typeof(OrderController).GetCustomAttribute<RoleAuthorizeAttribute>();
        Assert.NotNull(orderAttr);

        var kmAttr = typeof(KhuyenMaiController).GetCustomAttribute<RoleAuthorizeAttribute>();
        Assert.NotNull(kmAttr);
    }

    [Fact]
    public void HomeController_Error_HasAllowAnonymousAndHttpGet()
    {
        var errorMethod = typeof(HomeController).GetMethod(nameof(HomeController.Error));
        Assert.NotNull(errorMethod);
        Assert.NotNull(errorMethod.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(errorMethod.GetCustomAttribute<HttpGetAttribute>());
    }

    private class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _storage = new();
        public bool IsAvailable => true;
        public string Id => "fake-session-id";
        public IEnumerable<string> Keys => _storage.Keys;
        public void Clear() => _storage.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _storage.Remove(key);
        public void Set(string key, byte[] value) => _storage[key] = value;
        public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out byte[]? value) => _storage.TryGetValue(key, out value);
    }
}
