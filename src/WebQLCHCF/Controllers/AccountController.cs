using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Services;
using WebQLCHCF.Security;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly IChamCongService _chamCongService;
    private readonly IAuditLogService _auditLogService;

    public AccountController(
        AppDbContext context,
        IChamCongService chamCongService,
        IAuditLogService auditLogService
    )
    {
        _context = context;
        _chamCongService = chamCongService;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("MaTK")))
            return RedirectAfterLogin();

        TempData.Remove("Error");
        return View(new LoginViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("MaTK")))
        {
            TempData["Error"] =
                "Bạn đang đăng nhập. Vui lòng check-out và đăng xuất trước khi đăng nhập tài khoản khác.";
            return RedirectToAction(nameof(Logout));
        }

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = (
                await _context
                    .LoginResults.FromSqlInterpolated(
                        $"EXEC TaiKhoan_DangNhap {model.TenDangNhap}, {model.MatKhau}"
                    )
                    .AsNoTracking()
                    .ToListAsync()
            ).FirstOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            HttpContext.Session.SetString("MaTK", user.MaTK);
            HttpContext.Session.SetString("TenDangNhap", user.TenDangNhap);
            HttpContext.Session.SetString("MaNV", user.MaNV);
            HttpContext.Session.SetString("TenNV", user.TenNV);
            HttpContext.Session.SetString("VaiTro", user.VaiTro);

            await _chamCongService.AutoCheckInOnLoginAsync(user.MaNV);

            TempData.Remove("Error");
            TempData["Success"] = "Đăng nhập thành công.";
            return RedirectAfterLogin(user.VaiTro);
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Không thể đăng nhập. Vui lòng thử lại.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        var maNV = HttpContext.Session.GetString("MaNV");
        if (string.IsNullOrWhiteSpace(maNV))
            return RedirectToAction(nameof(Login));

        var dangLam = await _chamCongService.GetDangLamByNhanVienAsync(maNV);
        return View(
            "LogoutConfirm",
            new LogoutConfirmViewModel
            {
                MaNV = maNV,
                TenNV = HttpContext.Session.GetString("TenNV") ?? dangLam?.TenNV ?? string.Empty,
                ThoiGianCheckIn = dangLam?.ThoiGianCheckIn,
                ThoiGianDaLamText = dangLam?.ThoiGianDaLamText,
            }
        );
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(LogoutConfirmViewModel model)
    {
        var maNV = HttpContext.Session.GetString("MaNV");
        if (string.IsNullOrWhiteSpace(maNV))
            return RedirectToAction(nameof(Login));

        model.MaNV = maNV;
        model.TenNV = HttpContext.Session.GetString("TenNV") ?? string.Empty;

        if (!ModelState.IsValid)
        {
            await FillLogoutConfirmModelAsync(model, maNV);
            return View("LogoutConfirm", model);
        }

        try
        {
            var result = await _chamCongService.CheckOutAndGetResultAsync(maNV, model.MatKhau);
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Không lấy được kết quả check-out.");
                await FillLogoutConfirmModelAsync(model, maNV);
                return View("LogoutConfirm", model);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                "Không thể check-out: " + FriendlyAccountError(ex)
            );
            await FillLogoutConfirmModelAsync(model, maNV);
            return View("LogoutConfirm", model);
        }

        HttpContext.Session.Clear();
        TempData.Remove("Error");
        TempData["Success"] =
            "Check-out và đăng xuất thành công. Nhân viên khác có thể đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult CheckoutResult()
    {
        if (TempData["CheckoutTenNV"] == null)
            return RedirectToAction(nameof(Login));

        var model = new ChamCongCheckoutResultViewModel
        {
            TenNV = TempData["CheckoutTenNV"]?.ToString() ?? string.Empty,
            ThoiGianCheckIn = DateTime.TryParse(
                TempData["CheckoutCheckIn"]?.ToString(),
                out var checkIn
            )
                ? checkIn
                : DateTime.MinValue,
            ThoiGianCheckOut = DateTime.TryParse(
                TempData["CheckoutCheckOut"]?.ToString(),
                out var checkOut
            )
                ? checkOut
                : null,
            TongGio = decimal.TryParse(TempData["CheckoutTongGio"]?.ToString(), out var tongGio)
                ? tongGio
                : null,
            TienCong = decimal.TryParse(TempData["CheckoutTienCong"]?.ToString(), out var tienCong)
                ? tienCong
                : null,
            TongPhut = null,
        };

        ViewBag.ThoiGianDaLamText = TempData["CheckoutThoiGian"]?.ToString();
        return View(model);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("MaTK")))
            return RedirectToAction(nameof(Login));

        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    [HttpGet]
    [RoleAuthorize]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var maTK = HttpContext.Session.GetString("MaTK");
        var tenDangNhap = HttpContext.Session.GetString("TenDangNhap");
        if (string.IsNullOrWhiteSpace(maTK) || string.IsNullOrWhiteSpace(tenDangNhap))
            return RedirectToAction(nameof(Login));

        ValidateNewPassword(model, tenDangNhap);
        if (!ModelState.IsValid)
            return View(model);

        var account = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.MaTK == maTK);
        if (account == null)
        {
            ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản đang đăng nhập.");
            return View(model);
        }

        if (!string.Equals(account.MatKhau, model.MatKhauHienTai, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(model.MatKhauHienTai), "Mật khẩu hiện tại không đúng.");
            return View(model);
        }

        account.MatKhau = model.MatKhauMoi;
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            "Tài khoản",
            "Đổi mật khẩu",
            "TaiKhoan",
            maTK,
            $"Tài khoản {tenDangNhap} đổi mật khẩu thành công."
        );

        TempData["Success"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(ChangePassword));
    }

    private async Task FillLogoutConfirmModelAsync(LogoutConfirmViewModel model, string maNV)
    {
        var dangLam = await _chamCongService.GetDangLamByNhanVienAsync(maNV);
        model.TenNV = HttpContext.Session.GetString("TenNV") ?? dangLam?.TenNV ?? string.Empty;
        model.ThoiGianCheckIn = dangLam?.ThoiGianCheckIn;
        model.ThoiGianDaLamText = dangLam?.ThoiGianDaLamText;
    }

    private IActionResult RedirectAfterLogin(string? vaiTro = null)
    {
        vaiTro ??= HttpContext.Session.GetString("VaiTro");
        return string.Equals(vaiTro, AppRoles.QuanLy, StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Index", "Home")
            : RedirectToAction("Create", "Order");
    }

    private void ValidateNewPassword(ChangePasswordViewModel model, string tenDangNhap)
    {
        if (!PasswordPolicyValidator.ValidatePassword(model.MatKhauMoi, model.XacNhanMatKhauMoi, tenDangNhap, out var errors))
        {
            foreach (var error in errors)
            {
                if (error.Contains("khớp"))
                    ModelState.AddModelError(nameof(model.XacNhanMatKhauMoi), error);
                else
                    ModelState.AddModelError(nameof(model.MatKhauMoi), error);
            }
        }
    }

    private static string FriendlyAccountError(Exception ex)
    {
        if (ex.Message.Contains("Mật khẩu không đúng", StringComparison.OrdinalIgnoreCase))
            return "Mật khẩu không đúng.";

        if (ex.Message.Contains("chưa có ca đang làm", StringComparison.OrdinalIgnoreCase))
            return "Bạn chưa có ca đang làm để check-out.";

        return "Không thể thực hiện thao tác. Vui lòng thử lại.";
    }
}
