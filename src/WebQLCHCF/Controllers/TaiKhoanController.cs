using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.Services;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy)]
public class TaiKhoanController : BaseController
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IPasswordHasherService _passwordHasherService;
    private static readonly Dictionary<string, string> Labels = new()
    {
        ["MaBan"] = "Mã bàn",
        ["TenBan"] = "Tên bàn",
        ["KhuVuc"] = "Khu vực",
        ["TrangThai"] = "Trạng thái",
        ["MaKH"] = "Mã KH",
        ["TenKH"] = "Tên KH",
        ["SDT"] = "SĐT",
        ["DiemTichLuy"] = "Điểm tích lũy",
        ["MaKM"] = "Mã KM",
        ["TenKM"] = "Tên KM",
        ["LoaiKM"] = "Loại KM",
        ["GiaTri"] = "Giá trị",
        ["NgayBatDau"] = "Ngày bắt đầu",
        ["NgayKetThuc"] = "Ngày kết thúc",
        ["DieuKienApDung"] = "Điều kiện áp dụng",
        ["MaLoaiSP"] = "Mã loại",
        ["TenLoaiSP"] = "Tên loại",
        ["MaLuong"] = "Mã lương",
        ["Thang"] = "Tháng",
        ["Nam"] = "Năm",
        ["LuongCoBan"] = "Lương cơ bản",
        ["PhuCap"] = "Phụ cấp",
        ["Thuong"] = "Thưởng",
        ["Phat"] = "Phạt",
        ["TongLuong"] = "Tổng lương",
        ["MaNL"] = "Mã NL",
        ["TenNL"] = "Tên NL",
        ["DonViTinh"] = "ĐVT",
        ["SoLuongTon"] = "Số lượng tồn",
        ["GiaNhap"] = "Giá nhập",
        ["NgayCapNhat"] = "Ngày cập nhật",
        ["MucTonToiThieu"] = "Mức tồn tối thiểu",
        ["MaNV"] = "Mã NV",
        ["TenNV"] = "Tên NV",
        ["GioiTinh"] = "Giới tính",
        ["NgaySinh"] = "Ngày sinh",
        ["ChucVu"] = "Chức vụ",
        ["MaSP"] = "Mã SP",
        ["TenSP"] = "Tên SP",
        ["DonGia"] = "Đơn giá",
        ["MaTK"] = "Mã TK",
        ["TenDangNhap"] = "Tên đăng nhập",
        ["MatKhau"] = "Mật khẩu",
        ["VaiTro"] = "Vai trò",
        ["MaHD"] = "Mã HĐ",
        ["NgayLap"] = "Ngày lập",
        ["TongTien"] = "Tổng tiền",
        ["GhiChu"] = "Ghi chú",
        ["MaPN"] = "Mã PN",
        ["NgayNhap"] = "Ngày nhập",
        ["SoLuong"] = "Số lượng",
        ["ThanhTien"] = "Thành tiền",
        ["DinhLuong"] = "Định lượng",
        ["TenBan"] = "Tên bàn",
        ["TenLoaiSP"] = "Tên loại",
        ["TongSoLuong"] = "Tổng số lượng",
        ["TongTienNhap"] = "Tổng tiền nhập",
        ["DoanhThu"] = "Doanh thu",
    };

    public TaiKhoanController(
        AppDbContext c,
        IAuditLogService auditLogService,
        IPasswordHasherService? passwordHasherService = null
    )
    {
        _context = c;
        _auditLogService = auditLogService;
        _passwordHasherService = passwordHasherService ?? new PasswordHasherService();
    }

    private void SetUi(string title)
    {
        ViewData["Title"] = title;
        ViewData["Columns"] = new[] { "MaTK", "TenDangNhap", "MaNV", "TenNV", "VaiTro" };
        ViewData["Fields"] = new[] { "MaTK", "TenDangNhap", "MatKhau", "MaNV", "VaiTro" };
        ViewData["KeyNames"] = new[] { "MaTK" };
        ViewData["Labels"] = Labels;
    }

    private void SetDetailUi(string title)
    {
        ViewData["Title"] = title;
        ViewData["Columns"] = new[] { "MaTK", "TenDangNhap", "MaNV", "TenNV", "VaiTro" };
        ViewData["Fields"] = new[] { "MaTK", "TenDangNhap", "MatKhau", "MaNV", "VaiTro" };
        ViewData["KeyNames"] = new[] { "MaTK" };
        ViewData["Labels"] = Labels;
    }

    private async Task LoadDropdownsAsync()
    {
        var data = await _context
            .NhanVienResults.FromSqlInterpolated($"EXEC NhanVien_DS")
            .ToListAsync();
        ViewData["Dropdowns"] = new Dictionary<string, SelectList>
        {
            ["MaNV"] = new SelectList(data, "MaNV", "TenNV"),
            ["VaiTro"] = new SelectList(AppRoles.All),
        };
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        SetUi("Tài khoản");
        try
        {
            return View(
                (
                    await _context
                        .TaiKhoanListResults.FromSqlInterpolated($"EXEC TaiKhoan_DS")
                        .ToListAsync()
                ).Cast<object>()
            );
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải danh sách: " + FriendlyControllerError(ex);
            return View(Enumerable.Empty<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        SetDetailUi("Chi tiết Tài khoản");
        try
        {
            var item = (
                await _context
                    .TaiKhoanDetailResults.FromSqlInterpolated($"EXEC TaiKhoan_CT {id}")
                    .ToListAsync()
            ).FirstOrDefault();
            return item == null ? NotFound() : View((object)item);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xem chi tiết: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetUi("Thêm Tài khoản");
        await LoadDropdownsAsync();
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.TaiKhoans.Select(x => x.MaTK),
            "TK",
            2
        );
        return View(new TaiKhoan());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaiKhoan model)
    {
        SetUi("Thêm Tài khoản");
        await LoadDropdownsAsync();
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.TaiKhoans.Select(x => x.MaTK),
            "TK",
            2
        );
        ModelState.Remove("MaTK");
        if (!PasswordPolicyValidator.ValidatePassword(model.MatKhau, model.MatKhau, model.TenDangNhap, out var errors))
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(nameof(model.MatKhau), error);
            }
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            var passwordToSave = _passwordHasherService.HashPassword(model, model.MatKhau);
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC TaiKhoan_Them {(string?)null}, {model.TenDangNhap}, {passwordToSave}, {model.MaNV}, {model.VaiTro}"
            );
            var created = await _context.TaiKhoans.AsNoTracking()
                .Where(x => x.TenDangNhap == model.TenDangNhap && x.MaNV == model.MaNV)
                .OrderByDescending(x => x.MaTK)
                .FirstOrDefaultAsync();
            await _auditLogService.LogAsync(
                "Tài khoản",
                "Tạo tài khoản",
                "TaiKhoan",
                created?.MaTK,
                $"Tạo tài khoản {model.TenDangNhap} cho nhân viên {model.MaNV}, vai trò {model.VaiTro}."
            );
            TempData["Success"] = "Thêm dữ liệu thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể thêm dữ liệu: " + FriendlyControllerError(ex);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        SetUi("Sửa Tài khoản");
        await LoadDropdownsAsync();
        try
        {
            var item = (
                await _context
                    .TaiKhoanDetailResults.FromSqlInterpolated($"EXEC TaiKhoan_CT {id}")
                    .ToListAsync()
            ).FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }
            var model = MapToModel(item);
            model.MatKhau = string.Empty;
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải dữ liệu sửa: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string? id, TaiKhoan model)
    {
        SetUi("Sửa Tài khoản");
        await LoadDropdownsAsync();
        if (id == null || Convert.ToString(id) != Convert.ToString(model.MaTK))
        {
            return NotFound();
        }
        var before = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(x => x.MaTK == model.MaTK);
        if (before == null)
        {
            return NotFound();
        }

        bool isChangingPassword = !string.IsNullOrWhiteSpace(model.MatKhau);
        if (isChangingPassword)
        {
            if (!PasswordPolicyValidator.ValidatePassword(model.MatKhau, model.MatKhau, model.TenDangNhap, out var errors))
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(nameof(model.MatKhau), error);
                }
            }
        }
        else
        {
            ModelState.Remove(nameof(model.MatKhau));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            var passwordToSave = isChangingPassword
                ? _passwordHasherService.HashPassword(model, model.MatKhau)
                : before.MatKhau;

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC TaiKhoan_Sua {model.MaTK}, {model.TenDangNhap}, {passwordToSave}, {model.MaNV}, {model.VaiTro}"
            );
            await _auditLogService.LogAsync(
                "Tài khoản",
                "Sửa tài khoản",
                "TaiKhoan",
                model.MaTK,
                $"Sửa tài khoản {model.TenDangNhap}: vai trò {before.VaiTro} -> {model.VaiTro}, nhân viên {before.MaNV} -> {model.MaNV}{(isChangingPassword ? ", đổi mật khẩu" : "")}."
            );
            TempData["Success"] = "Cập nhật dữ liệu thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể cập nhật dữ liệu: " + FriendlyControllerError(ex);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        SetDetailUi("Xóa Tài khoản");
        try
        {
            var item = (
                await _context
                    .TaiKhoanDetailResults.FromSqlInterpolated($"EXEC TaiKhoan_CT {id}")
                    .ToListAsync()
            ).FirstOrDefault();
            return item == null ? NotFound() : View((object)item);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải dữ liệu xóa: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        try
        {
            var before = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(x => x.MaTK == id);
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC TaiKhoan_Xoa {id}");
            await _auditLogService.LogAsync(
                "Tài khoản",
                "Xóa tài khoản",
                "TaiKhoan",
                id,
                $"Xóa tài khoản {before?.TenDangNhap ?? id}, vai trò {before?.VaiTro ?? "không rõ"}."
            );
            TempData["Success"] = "Xóa dữ liệu thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa dữ liệu: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    private static TaiKhoan MapToModel(object item)
    {
        if (item is TaiKhoan e)
        {
            return e;
        }
        var model = new TaiKhoan();
        foreach (var p in typeof(TaiKhoan).GetProperties())
        {
            var s = item.GetType().GetProperty(p.Name);
            if (s != null && p.CanWrite)
            {
                p.SetValue(model, s.GetValue(item));
            }
        }
        return model;
    }
}
