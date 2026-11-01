using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.Services;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy)]
public class HomeController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public HomeController(IDashboardService dashboardService) =>
        _dashboardService = dashboardService;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var model = await _dashboardService.GetDashboardAsync();
            return View(model);
        }
        catch (Exception)
        {
            ViewBag.ErrorMessage =
                "Không thể tải dữ liệu dashboard. Vui lòng kiểm tra kết nối SQL hoặc stored procedure.";
            return View(new DashboardViewModel());
        }
    }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
}
