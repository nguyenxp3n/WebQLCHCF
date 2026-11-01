using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using WebQLCHCF.Controllers;
using WebQLCHCF.Security;
using WebQLCHCF.Services;
using Xunit;

namespace WebQLCHCF.Tests;

public class InvoiceImmutabilityTests
{
    [Theory]
    [InlineData("DaThanhToan", true)]
    [InlineData(" DaThanhToan ", true)]
    [InlineData("dathanhtoan", true)]
    [InlineData("ChuaThanhToan", false)]
    [InlineData("ChoThanhToanQR", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsPaid_XacDinhChinhXacTrangThaiDaThanhToan(string? trangThaiThanhToan, bool expected)
    {
        bool isPaid = TableWorkflowManager.IsPaid(trangThaiThanhToan);
        Assert.Equal(expected, isPaid);
    }

    [Fact]
    public void CanEditOrder_HoaDonDangChoThanhToanQR_TuChoiChinhSua()
    {
        bool canEdit = TableWorkflowManager.CanEditOrder(
            OrderWorkflowConstants.TrangThaiOrderDangOrder,
            OrderWorkflowConstants.TrangThaiThanhToanChoThanhToanQr,
            out string? reason
        );

        Assert.False(canEdit);
        Assert.Equal("Hóa đơn đang chờ thanh toán QR, không thể sửa.", reason);
    }

    [Fact]
    public void CanEditOrder_HoaDonDaThanhToan_TuChoiChinhSua()
    {
        bool canEdit = TableWorkflowManager.CanEditOrder(
            OrderWorkflowConstants.TrangThaiOrderChoThanhToan,
            OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan,
            out string? reason
        );

        Assert.False(canEdit);
        Assert.Equal("Hóa đơn đã thanh toán, không thể sửa.", reason);
    }

    [Theory]
    [InlineData("DaThanhToan", false, "Hóa đơn đã thanh toán, không thể thêm chi tiết.")]
    [InlineData("ChuaThanhToan", true, null)]
    [InlineData("ChoThanhToanQR", true, null)]
    public void CanAddLineItem_KiemTraTinhBatBienKhiThemChiTiet(
        string? trangThaiThanhToan,
        bool expectedAllowed,
        string? expectedReason)
    {
        bool allowed = TableWorkflowManager.CanAddLineItem(trangThaiThanhToan, out string? reason);

        Assert.Equal(expectedAllowed, allowed);
        Assert.Equal(expectedReason, reason);
    }

    [Theory]
    [InlineData("DaThanhToan", false, "Hóa đơn đã thanh toán, không thể chỉnh sửa chi tiết.")]
    [InlineData("ChuaThanhToan", true, null)]
    [InlineData("ChoThanhToanQR", true, null)]
    public void CanEditLineItem_KiemTraTinhBatBienKhiSuaChiTiet(
        string? trangThaiThanhToan,
        bool expectedAllowed,
        string? expectedReason)
    {
        bool allowed = TableWorkflowManager.CanEditLineItem(trangThaiThanhToan, out string? reason);

        Assert.Equal(expectedAllowed, allowed);
        Assert.Equal(expectedReason, reason);
    }

    [Theory]
    [InlineData("DaThanhToan", false, "Hóa đơn đã thanh toán, không thể xóa chi tiết.")]
    [InlineData("ChuaThanhToan", true, null)]
    [InlineData("ChoThanhToanQR", true, null)]
    public void CanDeleteLineItem_KiemTraTinhBatBienKhiXoaChiTiet(
        string? trangThaiThanhToan,
        bool expectedAllowed,
        string? expectedReason)
    {
        bool allowed = TableWorkflowManager.CanDeleteLineItem(trangThaiThanhToan, out string? reason);

        Assert.Equal(expectedAllowed, allowed);
        Assert.Equal(expectedReason, reason);
    }

    [Theory]
    [InlineData(OrderWorkflowConstants.TrangThaiOrderChoThanhToan, OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan)]
    [InlineData(OrderWorkflowConstants.TrangThaiOrderHoanThanh, OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan)]
    public void IsPayable_HoaDonDaThanhToan_KhongTheThanhToanLai(string orderStatus, string paymentStatus)
    {
        bool payable = TableWorkflowManager.IsPayable(orderStatus, paymentStatus);
        Assert.False(payable);
    }

    [Fact]
    public void HoaDonController_LineItemMutationActions_RequireQuanLyRole()
    {
        var mutationMethodNames = new[]
        {
            nameof(HoaDonController.Edit),
            nameof(HoaDonController.AddDetail),
            nameof(HoaDonController.EditDetail),
            nameof(HoaDonController.DeleteDetail),
        };

        var controllerType = typeof(HoaDonController);
        var methods = controllerType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => mutationMethodNames.Contains(m.Name) || m.Name == "DeleteDetailConfirmed")
            .ToList();

        Assert.True(methods.Count >= 8, $"Expected at least 8 mutation action overloads in HoaDonController, found {methods.Count}.");

        foreach (var method in methods)
        {
            var roleAttr = method.GetCustomAttribute<RoleAuthorizeAttribute>();
            Assert.NotNull(roleAttr);
        }
    }

    [Theory]
    [InlineData("DaHuy", "ChuaThanhToan")]
    [InlineData("DaHuy", "ChoThanhToanQR")]
    public void CanMutateLineItem_WhenOrderIsCancelled_AlwaysRejects(string orderStatus, string paymentStatus)
    {
        Assert.False(TableWorkflowManager.CanAddLineItem(orderStatus, paymentStatus, out var addReason));
        Assert.Contains("đã hủy", addReason);

        Assert.False(TableWorkflowManager.CanEditLineItem(orderStatus, paymentStatus, out var editReason));
        Assert.Contains("đã hủy", editReason);

        Assert.False(TableWorkflowManager.CanDeleteLineItem(orderStatus, paymentStatus, out var deleteReason));
        Assert.Contains("đã hủy", deleteReason);
    }

    [Fact]
    public void HoaDonController_CreateActions_RequireQuanLyRole()
    {
        var controllerType = typeof(HoaDonController);
        var createMethods = controllerType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => m.Name == nameof(HoaDonController.Create))
            .ToList();

        Assert.Equal(2, createMethods.Count);
        foreach (var method in createMethods)
        {
            var roleAttr = method.GetCustomAttribute<RoleAuthorizeAttribute>();
            Assert.NotNull(roleAttr);
            Assert.Contains(AppRoles.QuanLy, roleAttr.AllowedRoles);
        }
    }
}