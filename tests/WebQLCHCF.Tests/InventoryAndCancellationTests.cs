using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Controllers;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Services;
using WebQLCHCF.Tests.TestHarness;
using WebQLCHCF.ViewModels;
using Xunit;

namespace WebQLCHCF.Tests;

public class InventoryAndCancellationTests
{
    private static AppDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);

        // Seed NguyenLieu
        context.NguyenLieus.AddRange(
            new NguyenLieu { MaNL = "NL01", TenNL = "Hạt cà phê Robusta", SoLuongTon = 1000, DonViTinh = "gram" },
            new NguyenLieu { MaNL = "NL02", TenNL = "Sữa đặc", SoLuongTon = 500, DonViTinh = "ml" }
        );

        // Seed SanPham
        context.SanPhams.AddRange(
            new SanPham { MaSP = "SP01", TenSP = "Cà phê sữa đá", DonGia = 25000, TrangThai = "Đang bán" }
        );

        // Seed CongThuc: SP01 cần 20g NL01 và 30ml NL02
        context.CongThucs.AddRange(
            new CongThuc { MaSP = "SP01", MaNL = "NL01", DinhLuong = 20 },
            new CongThuc { MaSP = "SP01", MaNL = "NL02", DinhLuong = 30 }
        );

        // Seed Ban
        context.Bans.AddRange(
            new Ban { MaBan = "B01", TenBan = "Bàn 01", TrangThai = "Trống" },
            new Ban { MaBan = "MVE", TenBan = "Mang về", TrangThai = "Trống" }
        );

        // Seed KhachHang
        context.KhachHangs.Add(new KhachHang
        {
            MaKH = "KH01",
            TenKH = "Nguyễn Văn A",
            SDT = "0901234567",
            DiemTichLuy = 50
        });

        // Seed NhanVien
        context.NhanViens.Add(new NhanVien { MaNV = "NV01", TenNV = "Nhân viên 01" });

        // Seed ChamCong
        context.ChamCongs.Add(new ChamCong { MaNV = "NV01", ThoiGianCheckIn = DateTime.Now.AddHours(-1), TrangThai = "Đang làm" });

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task Inventory_DeductAndRestoreLifecycle_AccurateAndIdempotent()
    {
        using var context = CreateTestDbContext();
        var orderService = new OrderService(context);

        // 1. Tạo hóa đơn 2 ly cà phê sữa đá (SP01 x 2)
        // Mỗi ly: 20g NL01, 30ml NL02 -> Tổng: 40g NL01, 60ml NL02
        var createModel = new OrderCreateViewModel
        {
            ClientRequestId = Guid.NewGuid().ToString("D"),
            HinhThuc = OrderWorkflowConstants.HinhThucMangVe,
            Items = new List<OrderItemViewModel>
            {
                new() { MaSP = "SP01", SoLuong = 2 }
            }
        };

        var maHD = await orderService.CreateOrderAsync(createModel, "NV01");
        Assert.NotNull(maHD);

        var invoice = await context.HoaDons.FirstOrDefaultAsync(x => x.MaHD == maHD);
        Assert.NotNull(invoice);
        Assert.False(invoice.DaTruKho);

        // Kiểm tra tồn ban đầu: NL01 = 1000, NL02 = 500
        var nl1 = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL01");
        var nl2 = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL02");
        Assert.Equal(1000, nl1.SoLuongTon);
        Assert.Equal(500, nl2.SoLuongTon);

        // Đánh dấu hóa đơn đã thanh toán để kiểm tra quy trình PostPayment và Restore
        invoice.TrangThaiThanhToan = OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan;
        await context.SaveChangesAsync();

        // 2. Chạy logic trừ kho (ProcessPostPaymentAsync) qua reflection
        var processPostPaymentMethod = typeof(OrderService).GetMethod("ProcessPostPaymentAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(processPostPaymentMethod);

        await (Task)processPostPaymentMethod.Invoke(orderService, new object[] { maHD })!;

        // Xác nhận tồn kho sau khi trừ: NL01 còn 960 (1000 - 40), NL02 còn 440 (500 - 60)
        var nl1After = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL01");
        var nl2After = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL02");
        var invoiceAfter = await context.HoaDons.FirstAsync(x => x.MaHD == maHD);

        Assert.True(invoiceAfter.DaTruKho);
        Assert.Equal(960, nl1After.SoLuongTon);
        Assert.Equal(440, nl2After.SoLuongTon);

        // Xác nhận snapshot trừ kho trong LichSuTruKho
        var snapshots = await context.LichSuTruKhos.Where(x => x.MaHD == maHD).ToListAsync();
        Assert.Equal(2, snapshots.Count);
        Assert.Equal(40, snapshots.First(x => x.MaNL == "NL01").SoLuongTruThucTe);
        Assert.Equal(60, snapshots.First(x => x.MaNL == "NL02").SoLuongTruThucTe);

        // 3. Idempotency check: Gọi lại lần 2 không được trừ thêm
        await (Task)processPostPaymentMethod.Invoke(orderService, new object[] { maHD })!;

        var nl1Retry = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL01");
        var nl2Retry = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL02");
        Assert.Equal(960, nl1Retry.SoLuongTon);
        Assert.Equal(440, nl2Retry.SoLuongTon);

        // 4. Hoàn kho dựa trên snapshot (RestoreInventoryForInvoiceAsync)
        var restoreInventoryMethod = typeof(OrderService).GetMethod("RestoreInventoryForInvoiceAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(restoreInventoryMethod);

        await (Task)restoreInventoryMethod.Invoke(orderService, new object[] { maHD })!;

        var nl1Restored = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL01");
        var nl2Restored = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL02");
        var invoiceRestored = await context.HoaDons.FirstAsync(x => x.MaHD == maHD);

        Assert.False(invoiceRestored.DaTruKho);
        Assert.Equal(1000, nl1Restored.SoLuongTon);
        Assert.Equal(500, nl2Restored.SoLuongTon);

        // Snapshot phải được dọn dẹp sau khi hoàn kho
        var snapshotsRestored = await context.LichSuTruKhos.Where(x => x.MaHD == maHD).ToListAsync();
        Assert.Empty(snapshotsRestored);
    }

    [Fact]
    public async Task Inventory_SnapshotPreventsExcessiveRestoration_WhenInitialStockWasInsufficient()
    {
        using var context = CreateTestDbContext();
        var orderService = new OrderService(context);

        // Thiết lập NL01 chỉ còn 15g tồn kho (trong khi 1 ly SP01 cần 20g)
        var nl1 = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL01");
        nl1.SoLuongTon = 15;
        await context.SaveChangesAsync();

        var createModel = new OrderCreateViewModel
        {
            ClientRequestId = Guid.NewGuid().ToString("D"),
            HinhThuc = OrderWorkflowConstants.HinhThucMangVe,
            Items = new List<OrderItemViewModel>
            {
                new() { MaSP = "SP01", SoLuong = 1 }
            }
        };

        var maHD = await orderService.CreateOrderAsync(createModel, "NV01");
        var invoice = await context.HoaDons.FirstAsync(x => x.MaHD == maHD);
        invoice.TongTien = 50000m;
        invoice.TrangThaiThanhToan = OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan;
        await context.SaveChangesAsync();

        var processPostPaymentMethod = typeof(OrderService).GetMethod("ProcessPostPaymentAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        await (Task)processPostPaymentMethod!.Invoke(orderService, new object[] { maHD })!;

        // Tồn kho NL01 bị trừ về 0 (thực trừ 15g)
        var nl1Deducted = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL01");
        Assert.Equal(0, nl1Deducted.SoLuongTon);

        // Snapshot phải lưu chính xác 15g thực trừ (chứ không phải 20g theo công thức)
        var snap = await context.LichSuTruKhos.FirstAsync(x => x.MaHD == maHD && x.MaNL == "NL01");
        Assert.Equal(15, snap.SoLuongTruThucTe);

        // Thực hiện hoàn kho
        var restoreInventoryMethod = typeof(OrderService).GetMethod("RestoreInventoryForInvoiceAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        await (Task)restoreInventoryMethod!.Invoke(orderService, new object[] { maHD })!;

        // Tồn kho chỉ được hoàn đúng 15g -> quay lại 15g, không bị sai lệch thành 20g!
        var nl1Restored = await context.NguyenLieus.FirstAsync(x => x.MaNL == "NL01");
        Assert.Equal(15, nl1Restored.SoLuongTon);
    }

    [Fact]
    public async Task CustomerPoints_AccrualIsIdempotent_WithDaCongDiemFlag()
    {
        using var context = CreateTestDbContext();
        var orderService = new OrderService(context);

        var createModel = new OrderCreateViewModel
        {
            ClientRequestId = Guid.NewGuid().ToString("D"),
            HinhThuc = OrderWorkflowConstants.HinhThucMangVe,
            MaKH = "KH01", // Khách ban đầu có 50 điểm
            Items = new List<OrderItemViewModel>
            {
                new() { MaSP = "SP01", SoLuong = 2 } // 25.000 x 2 = 50.000đ -> được 5 điểm
            }
        };

        var maHD = await orderService.CreateOrderAsync(createModel, "NV01");
        var invoice = await context.HoaDons.FirstAsync(x => x.MaHD == maHD);
        invoice.TongTien = 50000m;
        invoice.TrangThaiThanhToan = OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan;
        await context.SaveChangesAsync();

        var processPostPaymentMethod = typeof(OrderService).GetMethod("ProcessPostPaymentAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Gọi lần 1: Cộng điểm thành công
        await (Task)processPostPaymentMethod!.Invoke(orderService, new object[] { maHD })!;

        var invoiceAfter1 = await context.HoaDons.FirstAsync(x => x.MaHD == maHD);
        var customer1 = await context.KhachHangs.FirstAsync(x => x.MaKH == "KH01");
        Assert.True(invoiceAfter1.DaCongDiem);
        Assert.Equal(55, customer1.DiemTichLuy); // 50 + 5

        // Gọi lại lần 2 (Retry mạng/job trùng): Không được cộng thêm điểm
        await (Task)processPostPaymentMethod.Invoke(orderService, new object[] { maHD })!;

        var customer2 = await context.KhachHangs.FirstAsync(x => x.MaKH == "KH01");
        Assert.Equal(55, customer2.DiemTichLuy); // Vẫn là 55, không bị tăng thành 60
    }

    [Fact]
    public async Task CancelOrderAsync_OnPaidInvoice_ThrowsInvalidOperationException()
    {
        using var context = CreateTestDbContext();
        var orderService = new OrderService(context);

        var createModel = new OrderCreateViewModel
        {
            ClientRequestId = Guid.NewGuid().ToString("D"),
            HinhThuc = OrderWorkflowConstants.HinhThucMangVe,
            Items = new List<OrderItemViewModel> { new() { MaSP = "SP01", SoLuong = 1 } }
        };

        var maHD = await orderService.CreateOrderAsync(createModel, "NV01");
        var invoice = await context.HoaDons.FirstAsync(x => x.MaHD == maHD);
        invoice.TongTien = 50000m;
        invoice.TrangThaiThanhToan = OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan;
        await context.SaveChangesAsync();

        // Hủy order thông thường trên hóa đơn đã thanh toán phải fail-fast
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            orderService.CancelOrderAsync(maHD, "Khách đổi ý"));

        Assert.Contains("không được hủy bằng chức năng Hủy order thông thường", ex.Message);
    }

    [Fact]
    public async Task BanController_Restore_WhenUnpaidOrdersExist_CancelsOrdersAndClearsTable()
    {
        using var context = CreateTestDbContext();
        var table = await context.Bans.FirstAsync(x => x.MaBan == "B01");
        table.TrangThai = OrderWorkflowConstants.TrangThaiBanDangPhucVu;

        context.HoaDons.Add(new HoaDon
        {
            MaHD = "HD_UNPAID",
            MaBan = "B01",
            MaNV = "NV01",
            NgayLap = DateTime.Today,
            ThoiGianTao = DateTime.Now,
            TrangThaiOrder = OrderWorkflowConstants.TrangThaiOrderChoThanhToan,
            TrangThaiThanhToan = OrderWorkflowConstants.TrangThaiThanhToanChuaThanhToan
        });
        await context.SaveChangesAsync();

        var auditDouble = new TestAuditLogServiceDouble();
        var controller = new BanController(context, auditDouble);
        TestControllerContextFactory.SetupController(controller);

        var result = await controller.Restore("B01");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var updatedOrder = await context.HoaDons.FirstAsync(x => x.MaHD == "HD_UNPAID");
        Assert.Equal(OrderWorkflowConstants.TrangThaiOrderDaHuy, updatedOrder.TrangThaiOrder);
        Assert.Contains("[Hủy do khôi phục bàn]", updatedOrder.GhiChu);

        var updatedTable = await context.Bans.FirstAsync(x => x.MaBan == "B01");
        Assert.Equal(OrderWorkflowConstants.TrangThaiBanTrong, updatedTable.TrangThai);
    }
}
