using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.Json;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.Services;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy)]
public class SanPhamController : BaseController
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private static readonly string[] TrangThaiOptions = { "Đang bán", "Tạm ngưng", "Ngừng bán" };
    private const string DonGiaError = "Đơn giá phải lớn hơn 0 và không vượt quá 10.000.000đ.";

    private static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
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

    public SanPhamController(AppDbContext c, IAuditLogService auditLogService)
    {
        _context = c;
        _auditLogService = auditLogService;
    }

    private void SetUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[]
        {
            "MaSP",
            "TenSP",
            "DonGia",
            "MaLoaiSP",
            "TenLoaiSP",
            "TrangThai",
        };
        ViewData["Fields"] = new[] { "MaSP", "TenSP", "DonGia", "MaLoaiSP", "TrangThai" };
        ViewData["KeyNames"] = new[] { "MaSP" };
        ViewData["Labels"] = Labels;
    }

    private void SetDetailUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[]
        {
            "MaSP",
            "TenSP",
            "DonGia",
            "MaLoaiSP",
            "TenLoaiSP",
            "TrangThai",
        };
        ViewData["Fields"] = new[] { "MaSP", "TenSP", "DonGia", "MaLoaiSP" };
        ViewData["KeyNames"] = new[] { "MaSP" };
        ViewData["Labels"] = Labels;
    }

    private async Task LoadDropdownsAsync(
        string? selectedMaLoaiSP = null,
        string? selectedTrangThai = null
    )
    {
        var data = await _context.LoaiSPs.FromSqlInterpolated($"EXEC LoaiSP_DS").ToListAsync();
        ViewData["Dropdowns"] = new Dictionary<string, SelectList>
        {
            ["MaLoaiSP"] = new SelectList(data, "MaLoaiSP", "TenLoaiSP", selectedMaLoaiSP),
            ["TrangThai"] = new SelectList(TrangThaiOptions, selectedTrangThai),
        };
    }

    public async Task<IActionResult> Index(string? tuKhoa, string? maLoaiSP, string? trangThai)
    {
        SetUi("Sản phẩm");
        try
        {
            var products = await _context
                .SanPhamResults.FromSqlInterpolated($"EXEC SanPham_DS")
                .AsNoTracking()
                .ToListAsync();
            var categories = await _context
                .LoaiSPs.FromSqlInterpolated($"EXEC LoaiSP_DS")
                .AsNoTracking()
                .ToListAsync();
            var filtered = ApplyProductFilters(products, tuKhoa, maLoaiSP, trangThai).ToList();

            ViewBag.CurrentListUrl = Url.Action(
                nameof(Index),
                new { tuKhoa, maLoaiSP, trangThai }
            );

            return View(
                new SanPhamIndexViewModel
                {
                    TuKhoa = tuKhoa,
                    MaLoaiSP = maLoaiSP,
                    TrangThai = trangThai,
                    Items = filtered,
                    LoaiSanPhams = categories,
                    TrangThaiOptions = TrangThaiOptions.ToList(),
                    TongSo = products.Count,
                }
            );
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải danh sách: " + FriendlyControllerError(ex);
            return View(
                new SanPhamIndexViewModel
                {
                    TuKhoa = tuKhoa,
                    MaLoaiSP = maLoaiSP,
                    TrangThai = trangThai,
                    TrangThaiOptions = TrangThaiOptions.ToList(),
                }
            );
        }
    }

    public async Task<IActionResult> Details(string? id, string? returnUrl = null)
    {
        if (id == null)
            return NotFound();
        SetDetailUi("Chi tiết Sản phẩm");
        ViewBag.ReturnUrl = LocalReturnUrl(returnUrl);
        try
        {
            var item = (
                await _context
                    .SanPhamResults.FromSqlInterpolated($"EXEC SanPham_CT {id}")
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

    public async Task<IActionResult> Create()
    {
        SetUi("Thêm Sản phẩm");
        var model = new SanPham { TrangThai = "Tạm ngưng" };
        await LoadDropdownsAsync(model.MaLoaiSP, model.TrangThai);
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.SanPhams.Select(x => x.MaSP),
            "SP",
            2
        );
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SanPham model)
    {
        SetUi("Thêm Sản phẩm");
        await LoadDropdownsAsync(model.MaLoaiSP, model.TrangThai);
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.SanPhams.Select(x => x.MaSP),
            "SP",
            2
        );
        ModelState.Remove("MaSP");
        ValidateDonGia(model);
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC SanPham_Them {(string?)null}, {model.TenSP}, {model.DonGia}, {model.MaLoaiSP}, {model.TrangThai}"
            );
            TempData["Success"] = "Thêm dữ liệu thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể thêm dữ liệu: " + FriendlyError(ex);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(string? id, string? returnUrl = null)
    {
        if (id == null)
            return NotFound();
        SetUi("Sửa Sản phẩm");
        ViewBag.ReturnUrl = LocalReturnUrl(returnUrl);
        try
        {
            var item = (
                await _context
                    .SanPhamResults.FromSqlInterpolated($"EXEC SanPham_CT {id}")
                    .ToListAsync()
            ).FirstOrDefault();
            if (item == null)
                return NotFound();
            var model = MapToModel(item);
            await LoadDropdownsAsync(model.MaLoaiSP, model.TrangThai);
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải dữ liệu sửa: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string? id, SanPham model, string? returnUrl = null)
    {
        SetUi("Sửa Sản phẩm");
        await LoadDropdownsAsync(model.MaLoaiSP, model.TrangThai);
        ViewBag.ReturnUrl = LocalReturnUrl(returnUrl);
        if (id == null || Convert.ToString(id) != Convert.ToString(model.MaSP))
            return NotFound();
        ValidateDonGia(model);
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            var before = await _context.SanPhams.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaSP == model.MaSP);
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC SanPham_Sua {model.MaSP}, {model.TenSP}, {model.DonGia}, {model.MaLoaiSP}, {model.TrangThai}"
            );
            if (before != null && before.DonGia != model.DonGia)
            {
                await _auditLogService.LogAsync(
                    "Sản phẩm",
                    "Sửa giá sản phẩm",
                    "SanPham",
                    model.MaSP,
                    $"Sửa giá sản phẩm {model.MaSP} từ {before.DonGia:N0}đ thành {model.DonGia:N0}đ.",
                    duLieuTruoc: SerializeProductPrice(before),
                    duLieuSau: SerializeProductPrice(model)
                );
            }
            TempData["Success"] = "Cập nhật dữ liệu thành công.";
            return RedirectToLocalList(returnUrl);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể cập nhật dữ liệu: " + FriendlyError(ex);
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(string? id, string? returnUrl = null)
    {
        if (id == null)
            return NotFound();
        SetDetailUi("Xóa Sản phẩm");
        ViewBag.ReturnUrl = LocalReturnUrl(returnUrl);
        try
        {
            var item = (
                await _context
                    .SanPhamResults.FromSqlInterpolated($"EXEC SanPham_CT {id}")
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
    public async Task<IActionResult> DeleteConfirmed(string? id, string? returnUrl = null)
    {
        if (id == null)
            return NotFound();
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC SanPham_Xoa {id}");
            TempData["Success"] = "Xóa dữ liệu thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa dữ liệu: " + FriendlyControllerError(ex);
        }
        return RedirectToLocalList(returnUrl);
    }

    private static IEnumerable<SanPhamViewModel> ApplyProductFilters(
        IEnumerable<SanPhamViewModel> products,
        string? tuKhoa,
        string? maLoaiSP,
        string? trangThai
    )
    {
        var query = products;
        var normalizedKeyword = NormalizeSearch(tuKhoa);
        if (!string.IsNullOrWhiteSpace(normalizedKeyword))
        {
            query = query.Where(x =>
                NormalizeSearch(x.MaSP).Contains(normalizedKeyword)
                || NormalizeSearch(x.TenSP).Contains(normalizedKeyword)
            );
        }

        if (!string.IsNullOrWhiteSpace(maLoaiSP))
        {
            query = query.Where(x =>
                string.Equals(x.MaLoaiSP, maLoaiSP.Trim(), StringComparison.OrdinalIgnoreCase)
            );
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            query = query.Where(x =>
                string.Equals(x.TrangThai, trangThai.Trim(), StringComparison.OrdinalIgnoreCase)
            );
        }

        return query.OrderBy(x => x.MaSP);
    }

    private static string NormalizeSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                builder.Append(ch);
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd')
            .Replace('Đ', 'D')
            .ToUpperInvariant();
    }

    private string? LocalReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : null;
    }

    private IActionResult RedirectToLocalList(string? returnUrl)
    {
        var localUrl = LocalReturnUrl(returnUrl);
        return !string.IsNullOrWhiteSpace(localUrl)
            ? LocalRedirect(localUrl)
            : RedirectToAction(nameof(Index));
    }

    private static SanPham MapToModel(object item)
    {
        if (item is SanPham e)
            return e;
        var model = new SanPham();
        foreach (var p in typeof(SanPham).GetProperties())
        {
            var s = item.GetType().GetProperty(p.Name);
            if (s != null && p.CanWrite)
                p.SetValue(model, s.GetValue(item));
        }
        return model;
    }

    private void ValidateDonGia(SanPham model)
    {
        if (model.DonGia <= 0 || model.DonGia > 10000000)
            ModelState.AddModelError(nameof(model.DonGia), DonGiaError);
    }

    private static string FriendlyError(Exception ex)
    {
        return ex.Message.Contains("out of range", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Arithmetic overflow", StringComparison.OrdinalIgnoreCase)
            ? DonGiaError
            : FriendlyControllerError(ex);
    }

    private static string SerializeProductPrice(SanPham model)
    {
        return JsonSerializer.Serialize(
            new
            {
                model.MaSP,
                model.TenSP,
                model.DonGia,
                model.MaLoaiSP,
                model.TrangThai,
            }
        );
    }
}
