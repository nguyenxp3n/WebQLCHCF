using Microsoft.AspNetCore.Mvc;
using WebQLCHCF.Controllers;
using WebQLCHCF.Services;
using WebQLCHCF.Tests.TestHarness;
using WebQLCHCF.ViewModels;
using Xunit;

namespace WebQLCHCF.Tests.Journeys;

public class WaiterUserJourneyTests
{
    [Fact]
    public async Task WaiterJourney_CompleteOrderToCashCheckout_Succeeds()
    {
        // === 1. Setup Waiter Context & Order Double ===
        var orderService = new InMemoryOrderServiceDouble();
        var auditLog = new TestAuditLogServiceDouble();

        var waiterContext = TestHttpContextFactory.CreatePhucVu(
            maTK: "TK_WAITER",
            maNV: "NV02",
            tenNV: "Phục Vụ Trần");

        var orderController = new OrderController(orderService, auditLog);
        TestControllerContextFactory.SetupController(orderController, waiterContext);

        // Step 1: Waiter check-in verification
        var isCheckedIn = await orderService.IsEmployeeCheckedInAsync("NV02");
        Assert.True(isCheckedIn);

        // Verify Order/Create (GET) renders table list and products
        var createGetResult = await orderController.Create((string?)null);
        var createGetView = Assert.IsType<ViewResult>(createGetResult);
        var createModel = Assert.IsType<OrderCreateViewModel>(createGetView.Model);
        Assert.NotEmpty(createModel.DanhSachSanPhamDangBan);

        // Step 2: Table initial state = "Trống"
        var tableB01 = orderService.Tables.First(t => t.MaBan == "B01");
        Assert.Equal("Trống", tableB01.TrangThai);

        // Step 3: Order Construction: 2 Cà phê đen (25K) + 1 Trà đào (35K) = 85K, Promotion KM01 (10%)
        var customerA = orderService.Customers.First(c => c.MaKH == "KH01");
        var initialCustomerPoints = customerA.DiemTichLuy ?? 0;

        var orderSubmission = new OrderCreateViewModel
        {
            KhuVuc = "Tầng trệt",
            MaKH = "KH01",
            GhiChu = "Ít đá ngọt vừa",
            Items = new List<OrderItemViewModel>
            {
                new() { MaSP = "SP01", SoLuong = 2, MaKM = "KM01" }, // 50K - 10% = 45K
                new() { MaSP = "SP03", SoLuong = 1 }                 // 35K
            }
        };

        var maHD = await orderService.CreateOrderAsync(orderSubmission, "NV02");
        Assert.StartsWith("HD", maHD);

        // Verify table state changed to "Đang phục vụ"
        Assert.Equal("Đang phục vụ", tableB01.TrangThai);

        // Verify Invoice total: 45,000 + 35,000 = 80,000đ
        var receipt = await orderService.GetReceiptAsync(maHD);
        Assert.NotNull(receipt);
        Assert.Equal(80000m, receipt.TongTien);
        Assert.Equal("ChoThanhToan", receipt.TrangThaiOrder);
        Assert.Equal("ChuaThanhToan", receipt.TrangThaiThanhToan);
        Assert.True(receipt.CanPay);

        // Step 4: Process Cash Payment
        var payResult = await orderController.PayCash(maHD, "Khách đưa tiền mặt");
        var payRedirect = Assert.IsType<RedirectToActionResult>(payResult);
        Assert.Equal("Receipt", payRedirect.ActionName);

        // Verify post-payment states
        var paidReceipt = await orderService.GetReceiptAsync(maHD);
        Assert.NotNull(paidReceipt);
        Assert.Equal("DaThanhToan", paidReceipt.TrangThaiThanhToan);
        Assert.Equal("HoanThanh", paidReceipt.TrangThaiOrder);
        Assert.Equal("TienMat", paidReceipt.PhuongThucThanhToan);

        // Verify customer loyalty points accrued (80,000 / 10,000 = 8 points)
        Assert.Equal(initialCustomerPoints + 8, customerA.DiemTichLuy);

        // Step 5: Table Completion (customer leaves)
        var completeResult = await orderController.CompleteTable(maHD);
        Assert.IsType<RedirectToActionResult>(completeResult);

        // Verify table B01 returns to "Trống"
        Assert.Equal("Trống", tableB01.TrangThai);
    }

    [Fact]
    public async Task WaiterJourney_VietQrPaymentAndCancellationRollback_Succeeds()
    {
        var orderService = new InMemoryOrderServiceDouble();
        var auditLog = new TestAuditLogServiceDouble();

        var waiterContext = TestHttpContextFactory.CreatePhucVu(
            maTK: "TK_WAITER",
            maNV: "NV02",
            tenNV: "Phục Vụ Trần");

        var orderController = new OrderController(orderService, auditLog);
        TestControllerContextFactory.SetupController(orderController, waiterContext);

        var tableB02 = orderService.Tables.First(t => t.MaBan == "B02");
        var customerB = orderService.Customers.First(c => c.MaKH == "KH02");
        var initialPoints = customerB.DiemTichLuy ?? 0;

        // Step 1: Create Order at Table B02
        var orderSubmission = new OrderCreateViewModel
        {
            KhuVuc = "Tầng trệt",
            MaKH = "KH02",
            Items = new List<OrderItemViewModel>
            {
                new() { MaSP = "SP02", SoLuong = 2 } // 2 * 30,000 = 60,000đ
            }
        };

        var maHD = await orderService.CreateOrderAsync(orderSubmission, "NV02");

        // Step 2: Create QR Payment Session
        var qrResult = await orderController.CreateQrPayment(maHD);
        Assert.IsType<RedirectToActionResult>(qrResult);

        var qrReceipt = await orderService.GetReceiptAsync(maHD);
        Assert.NotNull(qrReceipt);
        Assert.Equal("ChoThanhToanQR", qrReceipt.TrangThaiThanhToan);

        var payment = orderService.Payments.First(p => p.MaHD == maHD);
        Assert.NotNull(payment.MaPhienThanhToan);

        // Verify VietQR URL format
        var vietQrUrl = CafeOrderCalculator.SinhUrlVietQr(60000, $"DH {maHD}");
        Assert.Contains("img.vietqr.io", vietQrUrl);
        Assert.Contains("amount=60000", vietQrUrl);

        // Step 3: Confirm QR Payment
        var confirmResult = await orderController.ConfirmQrDemo(maHD, payment.MaPhienThanhToan);
        Assert.IsType<RedirectToActionResult>(confirmResult);

        var confirmedReceipt = await orderService.GetReceiptAsync(maHD);
        Assert.NotNull(confirmedReceipt);
        Assert.Equal("DaThanhToan", confirmedReceipt.TrangThaiThanhToan);
        Assert.Equal("QR", confirmedReceipt.PhuongThucThanhToan);

        // Verify points accrued (60,000 / 10,000 = 6 points)
        Assert.Equal(initialPoints + 6, customerB.DiemTichLuy);

        // Step 4: Order Cancellation & Points Reversal Rollback
        var cancelResult = await orderController.CancelOrder(maHD, "Khách có việc gấp phải về ngay");
        Assert.IsType<RedirectToActionResult>(cancelResult);

        var cancelledReceipt = await orderService.GetReceiptAsync(maHD);
        Assert.NotNull(cancelledReceipt);
        Assert.Equal("DaHuy", cancelledReceipt.TrangThaiOrder);

        // Verify loyalty points reversed back
        Assert.Equal(initialPoints, customerB.DiemTichLuy);

        // Verify table B02 restored to "Trống"
        Assert.Equal("Trống", tableB02.TrangThai);
    }
}
