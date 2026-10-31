using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Security;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy)]
public class AuditLogController : BaseController
{
    private readonly AppDbContext _context;

    public AuditLogController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(AuditLogFilterViewModel filter)
    {
        ViewData["Title"] = "Lịch sử thao tác";
        filter.Top = filter.Top <= 0 ? 200 : Math.Min(filter.Top, 1000);

        try
        {
            filter.Logs = await _context.AuditLogResults
                .FromSqlInterpolated(
                    $"EXEC AuditLog_DS {filter.TuNgay}, {filter.DenNgay}, {TrimToNull(filter.MaNV)}, {TrimToNull(filter.ChucNang)}, {TrimToNull(filter.HanhDong)}, {TrimToNull(filter.DoiTuong)}, {TrimToNull(filter.MaDoiTuong)}, {filter.Top}"
                )
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải audit log: " + FriendlyControllerError(ex);
            filter.Logs = new List<AuditLogViewModel>();
        }

        return View(filter);
    }

    private static string? TrimToNull(string? value)
    {
        value = value?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
