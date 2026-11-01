using Microsoft.AspNetCore.Mvc;
using WebQLCHCF.Controllers;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.Tests.TestHarness;
using WebQLCHCF.ViewModels;
using Xunit;

namespace WebQLCHCF.Tests.Journeys;

public class BaristaUserJourneyTests
{
    [Fact]
    public async Task BaristaJourney_CompleteWorkflow_ExecutesSuccessfully()
    {
        // === 1. Setup Barista Context & InMemory DB ===
        using var dbContext = TestControllerContextFactory.CreateInMemoryDbContext();
        var baristaContext = TestHttpContextFactory.CreatePhaChe(
            maTK: "TK_BARISTA",
            maNV: "NV03",
            tenNV: "Pha Chế Lê");

        // Seed Ingredients and Products
        var nlCaPhe = new NguyenLieu
        {
            MaNL = "NL01",
            TenNL = "Hạt Robusta",
            DonViTinh = "g",
            GiaNhap = 200,
            SoLuongTon = 5000,
            MucTonToiThieu = 1000,
            NgayCapNhat = DateTime.Today
        };

        var nlSuaDac = new NguyenLieu
        {
            MaNL = "NL02",
            TenNL = "Sữa đặc Ngôi Sao",
            DonViTinh = "ml",
            GiaNhap = 50,
            SoLuongTon = 300, // Below threshold!
            MucTonToiThieu = 500,
            NgayCapNhat = DateTime.Today
        };

        var spCaPheSua = new SanPham
        {
            MaSP = "SP02",
            TenSP = "Cà phê sữa đá",
            DonGia = 30000,
            MaLoaiSP = "L01",
            TrangThai = "Đang bán"
        };

        dbContext.NguyenLieus.AddRange(nlCaPhe, nlSuaDac);
        dbContext.SanPhams.Add(spCaPheSua);
        await dbContext.SaveChangesAsync();

        // === 2. Step 1: Recipe Lookup for Product ===
        var ctCaPhe = new CongThuc { MaSP = "SP02", MaNL = "NL01", DinhLuong = 25m };
        var ctSuaDac = new CongThuc { MaSP = "SP02", MaNL = "NL02", DinhLuong = 30m };
        dbContext.CongThucs.AddRange(ctCaPhe, ctSuaDac);
        await dbContext.SaveChangesAsync();

        var recipeItems = dbContext.CongThucs.Where(c => c.MaSP == "SP02").ToList();
        Assert.Equal(2, recipeItems.Count);
        Assert.All(recipeItems, r => Assert.True(r.DinhLuong > 0));

        // === 3. Step 2: Inventory Inspection via NguyenLieuController ===
        var nlController = new NguyenLieuController(dbContext);
        TestControllerContextFactory.SetupController(nlController, baristaContext);

        // Security check: Pha Chế is AUTHORIZED for Index and Details
        var indexSecurityResult = TestControllerContextFactory.ExecuteSecurityPipeline(nlController, nameof(NguyenLieuController.Index));
        Assert.Null(indexSecurityResult);

        var detailsSecurityResult = TestControllerContextFactory.ExecuteSecurityPipeline(nlController, nameof(NguyenLieuController.Details), "NL01");
        Assert.Null(detailsSecurityResult);

        // === 4. Step 3: Low Ingredient Alert Detection ===
        var lowStockIngredients = dbContext.NguyenLieus
            .Where(nl => (nl.SoLuongTon ?? 0) <= nl.MucTonToiThieu)
            .ToList();

        Assert.Single(lowStockIngredients);
        Assert.Equal("NL02", lowStockIngredients[0].MaNL);
        Assert.Equal("Sữa đặc Ngôi Sao", lowStockIngredients[0].TenNL);

        // === 5. Step 4: Role Boundary Guard Enforcement ===
        // 4.1 Pha Chế attempts to Create new NguyenLieu -> Rejected!
        var createSecurityResult = TestControllerContextFactory.ExecuteSecurityPipeline(nlController, nameof(NguyenLieuController.Create));
        var createRedirect = Assert.IsType<RedirectToActionResult>(createSecurityResult);
        Assert.Equal("AccessDenied", createRedirect.ActionName);
        Assert.Equal("Account", createRedirect.ControllerName);

        // 4.2 Pha Chế attempts to access LuongController -> Rejected!
        var luongController = new LuongController(dbContext);
        TestControllerContextFactory.SetupController(luongController, baristaContext);

        var luongSecurityResult = TestControllerContextFactory.ExecuteSecurityPipeline(luongController, nameof(LuongController.Index));
        var luongRedirect = Assert.IsType<RedirectToActionResult>(luongSecurityResult);
        Assert.Equal("AccessDenied", luongRedirect.ActionName);

        // 4.3 Pha Chế attempts to access ThongKeController -> Rejected!
        var thongKeController = new ThongKeController(dbContext);
        TestControllerContextFactory.SetupController(thongKeController, baristaContext);

        var thongKeSecurityResult = TestControllerContextFactory.ExecuteSecurityPipeline(thongKeController, nameof(ThongKeController.Index));
        var thongKeRedirect = Assert.IsType<RedirectToActionResult>(thongKeSecurityResult);
        Assert.Equal("AccessDenied", thongKeRedirect.ActionName);
    }
}
