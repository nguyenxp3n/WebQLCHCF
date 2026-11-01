using WebQLCHCF.Services;
using WebQLCHCF.Tests.TestHarness;
using WebQLCHCF.ViewModels;
using Xunit;

namespace WebQLCHCF.Tests.FaultInjection;

public class FaultInjectionInvoiceTests
{
    [Theory]
    [InlineData("DaThanhToan")]
    public void PaidInvoice_RejectsAddingLineItems(string trangThaiThanhToan)
    {
        var canAdd = TableWorkflowManager.CanAddLineItem(trangThaiThanhToan, out var reason);
        Assert.False(canAdd);
        Assert.Equal("Hóa đơn đã thanh toán, không thể thêm chi tiết.", reason);
    }

    [Theory]
    [InlineData("DaThanhToan")]
    public void PaidInvoice_RejectsEditingLineItems(string trangThaiThanhToan)
    {
        var canEdit = TableWorkflowManager.CanEditLineItem(trangThaiThanhToan, out var reason);
        Assert.False(canEdit);
        Assert.Equal("Hóa đơn đã thanh toán, không thể chỉnh sửa chi tiết.", reason);
    }

    [Theory]
    [InlineData("DaThanhToan")]
    public void PaidInvoice_RejectsDeletingLineItems(string trangThaiThanhToan)
    {
        var canDelete = TableWorkflowManager.CanDeleteLineItem(trangThaiThanhToan, out var reason);
        Assert.False(canDelete);
        Assert.Equal("Hóa đơn đã thanh toán, không thể xóa chi tiết.", reason);
    }

    [Theory]
    [InlineData("ChoThanhToan", "DaThanhToan")]
    [InlineData("HoanThanh", "DaThanhToan")]
    [InlineData("DaHuy", "ChuaThanhToan")]
    [InlineData("DaHuy", "DaThanhToan")]
    public void ImmutableOrderStates_RejectEditingOrder(string trangThaiOrder, string trangThaiThanhToan)
    {
        var canEdit = TableWorkflowManager.CanEditOrder(trangThaiOrder, trangThaiThanhToan, out var reason);
        Assert.False(canEdit);
        Assert.NotNull(reason);
        Assert.Contains("không thể sửa", reason);
    }

    [Theory]
    [InlineData("HoanThanh", "DaThanhToan")]
    [InlineData("DaHuy", "ChuaThanhToan")]
    [InlineData("DaHuy", "DaThanhToan")]
    public void ImmutableOrderStates_RejectFurtherPayments(string trangThaiOrder, string trangThaiThanhToan)
    {
        var isPayable = TableWorkflowManager.IsPayable(trangThaiOrder, trangThaiThanhToan);
        Assert.False(isPayable);
    }

    [Fact]
    public async Task OrderService_UpdateOrderOnPaidInvoice_ThrowsInvalidOperationException()
    {
        var orderService = new InMemoryOrderServiceDouble();

        // 1. Create order
        var model = new OrderCreateViewModel
        {
            KhuVuc = "Tầng trệt",
            Items = new List<OrderItemViewModel> { new() { MaSP = "SP01", SoLuong = 1 } }
        };
        var maHD = await orderService.CreateOrderAsync(model, "NV02");

        // 2. Pay order
        var payResult = await orderService.PayCashAsync(maHD, "Đã trả");
        Assert.True(payResult.Success);

        // 3. Attempt to update paid order -> Must throw!
        var updateModel = new OrderCreateViewModel
        {
            EditMaHD = maHD,
            Items = new List<OrderItemViewModel> { new() { MaSP = "SP01", SoLuong = 5 } }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.UpdateOrderAsync(updateModel, "NV02"));

        Assert.Contains("không thể sửa", ex.Message);
    }

    [Fact]
    public async Task OrderService_DoublePaymentAttempt_IsRejected()
    {
        var orderService = new InMemoryOrderServiceDouble();

        var model = new OrderCreateViewModel
        {
            KhuVuc = "Tầng trệt",
            Items = new List<OrderItemViewModel> { new() { MaSP = "SP01", SoLuong = 1 } }
        };
        var maHD = await orderService.CreateOrderAsync(model, "NV02");

        // First payment succeeds
        var firstPay = await orderService.PayCashAsync(maHD, "Lần 1");
        Assert.True(firstPay.Success);

        // Second payment attempt is rejected
        var secondPay = await orderService.PayCashAsync(maHD, "Lần 2");
        Assert.False(secondPay.Success);
        Assert.Contains("không ở trạng thái có thể thanh toán", secondPay.Message);
    }
}
