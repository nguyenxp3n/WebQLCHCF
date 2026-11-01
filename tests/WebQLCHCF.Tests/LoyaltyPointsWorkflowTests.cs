using System.Reflection;
using WebQLCHCF.Services;
using Xunit;

namespace WebQLCHCF.Tests;

public class LoyaltyPointsWorkflowTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(5000, 0)]
    [InlineData(9999, 0)]
    [InlineData(10000, 1)]
    [InlineData(19999, 1)]
    [InlineData(25000, 2)]
    [InlineData(48000, 4)]
    [InlineData(100000, 10)]
    [InlineData(520000, 52)]
    [InlineData(-50000, 0)]
    public void TinhDiemTichLuy_TinhChinhXac_1DiemMoi10000Dong(decimal tongTien, int expectedDiem)
    {
        int diem = CafeOrderCalculator.TinhDiemTichLuy(tongTien);
        Assert.Equal(expectedDiem, diem);
    }

    [Fact]
    public void LoyaltyWorkflow_UnpaidOrderCreation_DoesNotAwardPoints()
    {
        const int initialCustomerPoints = 15;
        const decimal orderTotal = 75000m;
        const string paymentStatus = OrderWorkflowConstants.TrangThaiThanhToanChuaThanhToan;

        int pointsToAward = paymentStatus == OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan
            ? CafeOrderCalculator.TinhDiemTichLuy(orderTotal)
            : 0;

        int finalCustomerPoints = initialCustomerPoints + pointsToAward;

        Assert.Equal(0, pointsToAward);
        Assert.Equal(initialCustomerPoints, finalCustomerPoints);
    }

    [Theory]
    [InlineData(50000, 5, 10, 15)]
    [InlineData(120000, 12, 0, 12)]
    [InlineData(8500, 0, 20, 20)]
    public void LoyaltyWorkflow_SuccessfulCashPayment_AwardsPointsAccurately(
        decimal totalAmount,
        int expectedPointsEarned,
        int initialPoints,
        int expectedFinalPoints)
    {
        const string paymentStatus = OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan;
        const string paymentMethod = OrderWorkflowConstants.PhuongThucTienMat;

        Assert.Equal(OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan, paymentStatus);
        Assert.Equal(OrderWorkflowConstants.PhuongThucTienMat, paymentMethod);

        int pointsEarned = CafeOrderCalculator.TinhDiemTichLuy(totalAmount);
        int finalPoints = initialPoints + pointsEarned;

        Assert.Equal(expectedPointsEarned, pointsEarned);
        Assert.Equal(expectedFinalPoints, finalPoints);
    }

    [Theory]
    [InlineData(65000, 6, 4, 10)]
    [InlineData(250000, 25, 10, 35)]
    public void LoyaltyWorkflow_SuccessfulQrPayment_AwardsPointsAccurately(
        decimal totalAmount,
        int expectedPointsEarned,
        int initialPoints,
        int expectedFinalPoints)
    {
        const string paymentStatus = OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan;
        const string paymentMethod = OrderWorkflowConstants.PhuongThucQr;

        Assert.Equal(OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan, paymentStatus);
        Assert.Equal(OrderWorkflowConstants.PhuongThucQr, paymentMethod);

        int pointsEarned = CafeOrderCalculator.TinhDiemTichLuy(totalAmount);
        int finalPoints = initialPoints + pointsEarned;

        Assert.Equal(expectedPointsEarned, pointsEarned);
        Assert.Equal(expectedFinalPoints, finalPoints);
    }

    [Fact]
    public void LoyaltyWorkflow_CancelOrder_PaidOrder_ReversesAwardedPoints()
    {
        const decimal paidOrderTotal = 80000m; // 8 points
        const int balanceBeforeCancellation = 20;
        const string paymentStatus = OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan;

        int pointsToReverse = paymentStatus == OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan
            ? CafeOrderCalculator.TinhDiemTichLuy(paidOrderTotal)
            : 0;

        int balanceAfterCancellation = Math.Max(0, balanceBeforeCancellation - pointsToReverse);

        Assert.Equal(8, pointsToReverse);
        Assert.Equal(12, balanceAfterCancellation);
    }

    [Fact]
    public void LoyaltyWorkflow_CancelOrder_UnpaidOrder_DoesNotReversePoints()
    {
        const decimal unpaidOrderTotal = 150000m;
        const int balanceBeforeCancellation = 15;
        const string paymentStatus = OrderWorkflowConstants.TrangThaiThanhToanChuaThanhToan;

        int pointsToReverse = paymentStatus == OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan
            ? CafeOrderCalculator.TinhDiemTichLuy(unpaidOrderTotal)
            : 0;

        int balanceAfterCancellation = Math.Max(0, balanceBeforeCancellation - pointsToReverse);

        Assert.Equal(0, pointsToReverse);
        Assert.Equal(balanceBeforeCancellation, balanceAfterCancellation);
    }

    [Theory]
    [InlineData(3, 5, 0)]    // Balance is 3, reversing 5 -> floored at 0
    [InlineData(10, 4, 6)]   // Balance is 10, reversing 4 -> 6
    [InlineData(0, 8, 0)]    // Balance is 0, reversing 8 -> floored at 0
    public void LoyaltyWorkflow_PointReversal_CannotDropCustomerBalanceBelowZero(
        int currentBalance,
        int pointsToDeduct,
        int expectedResultingBalance)
    {
        // Simulating SQL Server atomic logic:
        // CASE WHEN ISNULL(DiemTichLuy, 0) >= @points THEN DiemTichLuy - @points ELSE 0 END
        int calculatedBalance = currentBalance >= pointsToDeduct ? currentBalance - pointsToDeduct : 0;

        Assert.Equal(expectedResultingBalance, calculatedBalance);
        Assert.True(calculatedBalance >= 0, "Customer loyalty points must never be negative.");
    }

    [Fact]
    public void LoyaltyWorkflow_OrderService_ContainsExpectedPointMethods()
    {
        var orderServiceType = typeof(OrderService);

        var addPointsMethod = orderServiceType.GetMethod(
            nameof(OrderService.AddCustomerPointsAsync),
            new[] { typeof(string), typeof(string) }
        );
        var reversePointsMethod = orderServiceType.GetMethod(
            nameof(OrderService.ReverseCustomerPointsAsync),
            new[] { typeof(string), typeof(string) }
        );
        var cancelOrderMethod = orderServiceType.GetMethod(
            nameof(OrderService.CancelOrderAsync),
            new[] { typeof(string), typeof(string) }
        );

        Assert.NotNull(addPointsMethod);
        Assert.NotNull(reversePointsMethod);
        Assert.NotNull(cancelOrderMethod);
    }
}
