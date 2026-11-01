using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Security;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy)]
public class ThongKeController : BaseController
{
    private readonly AppDbContext _context;

    public ThongKeController(AppDbContext context) => _context = context;

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(DoanhThuTheoNgay));

    [HttpGet]
    public async Task<IActionResult> DoanhThuTheoNgay(DateTime? tuNgay, DateTime? denNgay)
    {
        ViewData["Title"] = "Doanh thu theo ngày";
        var start = tuNgay ?? DateTime.Today.AddDays(-30);
        var end = denNgay ?? DateTime.Today;
        ViewData["TuNgay"] = start.ToString("yyyy-MM-dd");
        ViewData["DenNgay"] = end.ToString("yyyy-MM-dd");
        if (start.Date > end.Date)
        {
            ModelState.AddModelError(string.Empty, "Từ ngày không được lớn hơn đến ngày.");
            ViewBag.HasInvalidDateRange = true;
            return View(new List<DoanhThuTheoNgayViewModel>());
        }

        try
        {
            return View(
                await _context
                    .DoanhThuTheoNgayResults.FromSqlInterpolated(
                        $"EXEC ThongKe_DoanhThuTheoNgay {start}, {end}"
                    )
                    .ToListAsync()
            );
        }
        catch (Exception)
        {
            ViewBag.ErrorMessage = "Không thể tải dữ liệu thống kê.";
            return View(new List<DoanhThuTheoNgayViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> SanPhamBanChay()
    {
        ViewData["Title"] = "Sản phẩm bán chạy";
        try
        {
            return View(
                await _context
                    .SanPhamBanChayResults.FromSqlInterpolated($"EXEC ThongKe_SanPhamBanChay")
                    .ToListAsync()
            );
        }
        catch (Exception)
        {
            ViewBag.ErrorMessage = "Không thể tải dữ liệu thống kê.";
            return View(new List<SanPhamBanChayViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> NhapHangTheoThang(int? thang, int? nam)
    {
        ViewData["Title"] = "Nhập hàng theo tháng";
        var m = thang ?? DateTime.Today.Month;
        var y = nam ?? DateTime.Today.Year;
        ViewData["Thang"] = m;
        ViewData["Nam"] = y;
        try
        {
            return View(
                await _context
                    .NhapHangTheoThangResults.FromSqlInterpolated(
                        $"EXEC ThongKe_NhapHangTheoThang {m}, {y}"
                    )
                    .ToListAsync()
            );
        }
        catch (Exception)
        {
            ViewBag.ErrorMessage = "Không thể tải dữ liệu thống kê.";
            return View(new List<NhapHangTheoThangViewModel>());
        }
    }
}
