using WebQLCHCF.Services;
using WebQLCHCF.Tests.TestHarness;
using WebQLCHCF.ViewModels;
using Xunit;

namespace WebQLCHCF.Tests.FaultInjection;

public class FaultInjectionBoundaryTests
{
    [Fact]
    public async Task CustomerLoyaltyPoints_CancellationUnderflow_FloorsAtZero()
    {
        var orderService = new InMemoryOrderServiceDouble();

        // Customer has only 3 points initially
        var customer = orderService.Customers.First(c => c.MaKH == "KH02");
        customer.DiemTichLuy = 3;

        // Create order: 2 * 30,000đ = 60,000đ (which awards 6 points)
        var model = new OrderCreateViewModel
        {
            KhuVuc = "Tầng trệt",
            MaKH = "KH02",
            Items = new List<OrderItemViewModel> { new() { MaSP = "SP02", SoLuong = 2 } }
        };
        var maHD = await orderService.CreateOrderAsync(model, "NV02");

        // Pay order: points become 3 + 6 = 9
        await orderService.PayCashAsync(maHD, "Thanh toán");
        Assert.Equal(9, customer.DiemTichLuy);

        // Simulate external points consumption down to 2 points
        customer.DiemTichLuy = 2;

        // Cancel order: tries to reverse 6 points from 2 points balance -> floor at 0!
        var cancelResult = await orderService.CancelOrderAsync(maHD, "Khách hoàn tiền");
        Assert.True(cancelResult.Success);

        Assert.Equal(0, customer.DiemTichLuy); // Math.Max(0, 2 - 6) == 0
    }

    [Fact]
    public async Task CreateOrder_EmptyOrderItems_ThrowsInvalidOperationException()
    {
        var orderService = new InMemoryOrderServiceDouble();

        var emptyModel = new OrderCreateViewModel
        {
            KhuVuc = "Tầng trệt",
            Items = new List<OrderItemViewModel>() // Empty!
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.CreateOrderAsync(emptyModel, "NV02"));

        Assert.Equal("Vui lòng chọn ít nhất 1 món.", ex.Message);

        var zeroQtyModel = new OrderCreateViewModel
        {
            KhuVuc = "Tầng trệt",
            Items = new List<OrderItemViewModel>
            {
                new() { MaSP = "SP01", SoLuong = 0 },
                new() { MaSP = "SP02", SoLuong = -1 }
            }
        };

        var ex2 = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.CreateOrderAsync(zeroQtyModel, "NV02"));

        Assert.Equal("Vui lòng chọn ít nhất 1 món.", ex2.Message);
    }

    [Fact]
    public async Task CreateOrder_InactiveProduct_ThrowsInvalidOperationException()
    {
        var orderService = new InMemoryOrderServiceDouble();

        // SP04 has TrangThai = "Ngừng bán"
        var inactiveProductModel = new OrderCreateViewModel
        {
            KhuVuc = "Tầng trệt",
            Items = new List<OrderItemViewModel>
            {
                new() { MaSP = "SP04", SoLuong = 1 }
            }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.CreateOrderAsync(inactiveProductModel, "NV02"));

        Assert.Contains("hiện không ở trạng thái Đang bán", ex.Message);
    }

    [Fact]
    public async Task CreateOrder_UncheckedInWaiter_ThrowsInvalidOperationException()
    {
        var orderService = new InMemoryOrderServiceDouble();

        // Remove NV02 from checked-in staff
        orderService.CheckedInStaff.Remove("NV02");

        var model = new OrderCreateViewModel
        {
            KhuVuc = "Tầng trệt",
            Items = new List<OrderItemViewModel> { new() { MaSP = "SP01", SoLuong = 1 } }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.CreateOrderAsync(model, "NV02"));

        Assert.Equal("Bạn cần check-in trước khi order.", ex.Message);
    }

    [Fact]
    public async Task CreateOrder_NoAvailableTableInArea_ThrowsInvalidOperationException()
    {
        var orderService = new InMemoryOrderServiceDouble();

        // Mark all tables in "Tầng lửng" as occupied
        foreach (var table in orderService.Tables.Where(t => t.KhuVuc == "Tầng lửng"))
        {
            table.TrangThai = "Đang phục vụ";
        }

        var model = new OrderCreateViewModel
        {
            KhuVuc = "Tầng lửng",
            Items = new List<OrderItemViewModel> { new() { MaSP = "SP01", SoLuong = 1 } }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orderService.CreateOrderAsync(model, "NV02"));

        Assert.Equal("Hiện không còn bàn trống tại khu vực này.", ex.Message);
    }

    [Theory]
    [InlineData("Đang phục vụ", false)]
    [InlineData("Trống", true)]
    [InlineData("Bảo trì", false)]
    [InlineData(null, false)]
    public void TableWorkflowManager_IsTableAvailable_EvaluatesCorrectly(string? status, bool expected)
    {
        var isAvailable = TableWorkflowManager.IsTableAvailable(status);
        Assert.Equal(expected, isAvailable);
    }
}
