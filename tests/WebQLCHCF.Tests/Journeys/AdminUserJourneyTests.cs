using Microsoft.AspNetCore.Mvc;
using WebQLCHCF.Controllers;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.Services;
using WebQLCHCF.Tests.TestHarness;
using WebQLCHCF.ViewModels;
using Xunit;

namespace WebQLCHCF.Tests.Journeys;

public class AdminUserJourneyTests
{
    [Fact]
    public async Task AdminJourney_CompleteWorkflow_ExecutesSuccessfully()
    {
        // === 1. Setup Admin Context & Services ===
        using var dbContext = TestControllerContextFactory.CreateInMemoryDbContext();
        var auditLogService = new TestAuditLogServiceDouble();
        var dashboardService = new TestDashboardServiceDouble();

        var adminContext = TestHttpContextFactory.CreateQuanLy(
            maTK: "TK_ADMIN",
            maNV: "NV01",
            tenNV: "Quản Lý Nguyễn");

        // === 2. Step 1: Dashboard Inspection ===
        var homeController = new HomeController(dashboardService);
        TestControllerContextFactory.SetupController(homeController, adminContext);

        var homeResult = await homeController.Index();
        var homeView = Assert.IsType<ViewResult>(homeResult);
        var dashboard = Assert.IsType<DashboardViewModel>(homeView.Model);

        Assert.True(dashboard.DoanhThuHomNay > 0);
        Assert.NotEmpty(dashboard.NguyenLieuSapHet);
        Assert.Contains(dashboard.NguyenLieuSapHet, nl => nl.SoLuongTon <= nl.MucTonToiThieu);

        // === 3. Step 2: Staff Creation with Password Policy Verification ===
        var staff = new NhanVien
        {
            MaNV = "NV10",
            TenNV = "Lê Thị Phục Vụ",
            GioiTinh = "Nữ",
            SDT = "0934567890",
            ChucVu = "Phục Vụ",
            MaLuong = 1,
            LuongTheoGio = 25000
        };
        dbContext.NhanViens.Add(staff);
        await dbContext.SaveChangesAsync();

        // Validate password using PasswordPolicyValidator
        const string weakPassword = "123";
        Assert.False(PasswordPolicyValidator.ValidatePassword(weakPassword, weakPassword, "phucvu10", out _));

        const string strongPassword = "Employee@2026";
        Assert.True(PasswordPolicyValidator.ValidatePassword(strongPassword, strongPassword, "phucvu10", out _));

        var account = new TaiKhoan
        {
            MaTK = "TK10",
            TenDangNhap = "phucvu10",
            MatKhau = strongPassword,
            MaNV = staff.MaNV,
            VaiTro = AppRoles.PhucVu
        };
        dbContext.TaiKhoans.Add(account);
        await dbContext.SaveChangesAsync();

        await auditLogService.LogAsync(
            chucNang: "Tài khoản",
            hanhDong: "Thêm mới",
            doiTuong: "TaiKhoan",
            maDoiTuong: account.MaTK,
            noiDung: $"Tạo tài khoản {account.TenDangNhap} cho nhân viên {staff.MaNV}");

        // === 4. Step 3: Monthly Payroll Aggregation with Minute-Level Precision ===
        // Simulate shift attendance: 8h30m = 510 minutes at 25,000 VND/hour
        var checkIn = new DateTime(2026, 10, 15, 8, 0, 0);
        var checkOut = new DateTime(2026, 10, 15, 16, 30, 0);
        var durationMinutes = (int)(checkOut - checkIn).TotalMinutes;
        Assert.Equal(510, durationMinutes);

        var wage = PayrollCalculator.TinhTienCongTuPhut(durationMinutes, staff.LuongTheoGio);
        // 510 * 25000 / 60 = 212500
        Assert.Equal(212500m, wage);

        var salaryGrade = new Luong
        {
            MaLuong = 2,
            Thang = 10,
            Nam = 2026,
            LuongCoBan = 5000000,
            PhuCap = 500000,
            Thuong = 200000,
            Phat = 0
        };
        dbContext.Luongs.Add(salaryGrade);
        await dbContext.SaveChangesAsync();

        // === 5. Step 4: Promotion Configuration & Creation ===
        var promotion = new KhuyenMai
        {
            MaKM = "KM88",
            TenKM = "Giảm giá Mùa Đông",
            LoaiKM = "Phần trăm",
            GiaTri = 20m,
            NgayBatDau = DateTime.Today,
            NgayKetThuc = DateTime.Today.AddDays(14),
            DieuKienApDung = "Áp dụng toàn menu",
            TrangThai = "Đang áp dụng"
        };

        // Assert promotion constraints: start <= end, percentage <= 100
        Assert.True(promotion.NgayKetThuc >= promotion.NgayBatDau);
        Assert.True(CafeOrderCalculator.KiemTraHopLeGiaTriKhuyenMai(promotion.LoaiKM, promotion.GiaTri));

        dbContext.KhuyenMais.Add(promotion);
        await dbContext.SaveChangesAsync();

        await auditLogService.LogAsync(
            chucNang: "Khuyến mãi",
            hanhDong: "Thêm",
            doiTuong: "KhuyenMai",
            maDoiTuong: promotion.MaKM,
            noiDung: $"Thêm mới khuyến mãi {promotion.TenKM} ({promotion.GiaTri}%)");

        // === 6. Step 5: Audit Log Querying & Verification ===
        Assert.Equal(2, auditLogService.Logs.Count);
        Assert.Contains(auditLogService.Logs, l => l.ChucNang == "Tài khoản" && l.MaDoiTuong == "TK10");
        Assert.Contains(auditLogService.Logs, l => l.ChucNang == "Khuyến mãi" && l.MaDoiTuong == "KM88");

        var auditController = new AuditLogController(dbContext);
        TestControllerContextFactory.SetupController(auditController, adminContext);

        // Security check: Quản Lý passes RoleAuthorize
        var securityResult = TestControllerContextFactory.ExecuteSecurityPipeline(auditController, nameof(AuditLogController.Index));
        Assert.Null(securityResult);
    }
}
