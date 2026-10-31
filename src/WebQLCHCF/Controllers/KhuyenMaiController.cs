using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.Services;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

public class KhuyenMaiController : BaseController
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private static readonly string[] LoaiKhuyenMaiHopLe = ["Phần trăm", "Tiền mặt"];

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

    public KhuyenMaiController(AppDbContext c, IAuditLogService auditLogService)
    {
        _context = c;
        _auditLogService = auditLogService;
    }

    private void SetUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[]
        {
            "MaKM",
            "TenKM",
            "LoaiKM",
            "GiaTri",
            "NgayBatDau",
            "NgayKetThuc",
            "DieuKienApDung",
            "TrangThai",
        };
        ViewData["Fields"] = new[]
        {
            "MaKM",
            "TenKM",
            "LoaiKM",
            "GiaTri",
            "NgayBatDau",
            "NgayKetThuc",
            "DieuKienApDung",
            "TrangThai",
        };
        ViewData["KeyNames"] = new[] { "MaKM" };
        ViewData["Labels"] = Labels;
    }

    private void SetDetailUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[]
        {
            "MaKM",
            "TenKM",
            "LoaiKM",
            "GiaTri",
            "NgayBatDau",
            "NgayKetThuc",
            "DieuKienApDung",
            "TrangThai",
        };
        ViewData["Fields"] = new[]
        {
            "MaKM",
            "TenKM",
            "LoaiKM",
            "GiaTri",
            "NgayBatDau",
            "NgayKetThuc",
            "DieuKienApDung",
            "TrangThai",
        };
        ViewData["KeyNames"] = new[] { "MaKM" };
        ViewData["Labels"] = Labels;
    }

    public async Task<IActionResult> Index()
    {
        SetUi("Khuyến mãi");
        try
        {
            return View(
                (
                    await _context
                        .KhuyenMais.FromSqlInterpolated($"EXEC KhuyenMai_DS")
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

    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
            return NotFound();
        SetDetailUi("Chi tiết Khuyến mãi");
        try
        {
            var item = (
                await _context
                    .KhuyenMais.FromSqlInterpolated($"EXEC KhuyenMai_CT {id}")
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

    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Create()
    {
        SetUi("Thêm Khuyến mãi");
        LoadDropdowns("Phần trăm");
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.KhuyenMais.Select(x => x.MaKM),
            "KM",
            2
        );
        return View(new KhuyenMai { LoaiKM = "Phần trăm" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Create(KhuyenMai model)
    {
        SetUi("Thêm Khuyến mãi");
        LoadDropdowns(model.LoaiKM);
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.KhuyenMais.Select(x => x.MaKM),
            "KM",
            2
        );
        ModelState.Remove("MaKM");
        ValidateGiaTri(model);
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC KhuyenMai_Them {(string?)null}, {model.TenKM}, {model.LoaiKM}, {model.GiaTri}, {model.NgayBatDau}, {model.NgayKetThuc}, {model.DieuKienApDung}, {model.TrangThai}"
            );
            var created = await _context.KhuyenMais.AsNoTracking()
                .Where(x =>
                    x.TenKM == model.TenKM
                    && x.LoaiKM == model.LoaiKM
                    && x.GiaTri == model.GiaTri
                    && x.NgayBatDau == model.NgayBatDau
                    && x.NgayKetThuc == model.NgayKetThuc)
                .OrderByDescending(x => x.MaKM)
                .FirstOrDefaultAsync();
            await _auditLogService.LogAsync(
                "Khuyến mãi",
                "Tạo khuyến mãi",
                "KhuyenMai",
                created?.MaKM,
                $"Tạo khuyến mãi {created?.MaKM ?? "(mới)"}, loại {model.LoaiKM}, giá trị {FormatPromotionValue(model)}.",
                duLieuSau: SerializePromotion(created ?? model)
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

    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
            return NotFound();
        SetUi("Sửa Khuyến mãi");
        try
        {
            var item = (
                await _context
                    .KhuyenMais.FromSqlInterpolated($"EXEC KhuyenMai_CT {id}")
                    .ToListAsync()
            ).FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }

            var model = MapToModel(item);
            LoadDropdowns(model.LoaiKM);
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải dữ liệu sửa: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Edit(string? id, KhuyenMai model)
    {
        SetUi("Sửa Khuyến mãi");
        LoadDropdowns(model.LoaiKM);
        if (id == null || Convert.ToString(id) != Convert.ToString(model.MaKM))
            return NotFound();
        ValidateGiaTri(model);
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            var before = await _context.KhuyenMais.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaKM == model.MaKM);
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC KhuyenMai_Sua {model.MaKM}, {model.TenKM}, {model.LoaiKM}, {model.GiaTri}, {model.NgayBatDau}, {model.NgayKetThuc}, {model.DieuKienApDung}, {model.TrangThai}"
            );
            await _auditLogService.LogAsync(
                "Khuyến mãi",
                "Sửa khuyến mãi",
                "KhuyenMai",
                model.MaKM,
                $"Sửa khuyến mãi {model.MaKM}, loại {model.LoaiKM}, giá trị {FormatPromotionValue(model)}.",
                duLieuTruoc: SerializePromotion(before),
                duLieuSau: SerializePromotion(model)
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

    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
            return NotFound();
        SetDetailUi("Xóa Khuyến mãi");
        try
        {
            var item = (
                await _context
                    .KhuyenMais.FromSqlInterpolated($"EXEC KhuyenMai_CT {id}")
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
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> DeleteConfirmed(string? id)
    {
        if (id == null)
            return NotFound();
        try
        {
            var before = await _context.KhuyenMais.AsNoTracking().FirstOrDefaultAsync(x => x.MaKM == id);
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC KhuyenMai_Xoa {id}");
            await _auditLogService.LogAsync(
                "Khuyến mãi",
                "Xóa khuyến mãi",
                "KhuyenMai",
                id,
                $"Xóa khuyến mãi {id}.",
                duLieuTruoc: SerializePromotion(before)
            );
            TempData["Success"] = "Xóa dữ liệu thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa dữ liệu: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    private static KhuyenMai MapToModel(object item)
    {
        if (item is KhuyenMai e)
            return e;
        var model = new KhuyenMai();
        foreach (var p in typeof(KhuyenMai).GetProperties())
        {
            var s = item.GetType().GetProperty(p.Name);
            if (s != null && p.CanWrite)
                p.SetValue(model, s.GetValue(item));
        }
        return model;
    }

    private void ValidateGiaTri(KhuyenMai model)
    {
        if (string.IsNullOrWhiteSpace(model.LoaiKM) || !LoaiKhuyenMaiHopLe.Contains(model.LoaiKM))
        {
            ModelState.AddModelError(
                nameof(model.LoaiKM),
                "Loại khuyến mãi chỉ được chọn Phần trăm hoặc Tiền mặt."
            );
        }

        if (model.GiaTri < 0)
        {
            ModelState.AddModelError(nameof(model.GiaTri), "Giá trị khuyến mãi không được âm.");
        }

        if (model.LoaiKM == "Phần trăm" && model.GiaTri > 100)
        {
            ModelState.AddModelError(
                nameof(model.GiaTri),
                "Giá trị khuyến mãi phần trăm phải từ 0 đến 100."
            );
        }

        if (model.LoaiKM == "Tiền mặt" && model.GiaTri > 100000000)
        {
            ModelState.AddModelError(
                nameof(model.GiaTri),
                "Giá trị khuyến mãi tiền mặt không được vượt quá 100.000.000đ."
            );
        }
    }

    private void LoadDropdowns(string? selectedLoaiKM = null)
    {
        ViewData["Dropdowns"] = new Dictionary<string, SelectList>
        {
            ["LoaiKM"] = new SelectList(LoaiKhuyenMaiHopLe, selectedLoaiKM),
        };
    }

    private static string FormatPromotionValue(KhuyenMai model)
    {
        return model.LoaiKM == "Phần trăm" ? $"{model.GiaTri:0.##}%" : $"{model.GiaTri:N0}đ";
    }

    private static string? SerializePromotion(KhuyenMai? model)
    {
        if (model == null)
            return null;

        return JsonSerializer.Serialize(
            new
            {
                model.MaKM,
                model.TenKM,
                model.LoaiKM,
                model.GiaTri,
                model.NgayBatDau,
                model.NgayKetThuc,
                model.DieuKienApDung,
                model.TrangThai,
            }
        );
    }
}
