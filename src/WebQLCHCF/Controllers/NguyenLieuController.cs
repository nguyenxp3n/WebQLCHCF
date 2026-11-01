using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy, AppRoles.PhaChe)]
public class NguyenLieuController : BaseController
{
    private readonly AppDbContext _context;
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

    public NguyenLieuController(AppDbContext c) => _context = c;

    private void SetUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[]
        {
            "MaNL",
            "TenNL",
            "DonViTinh",
            "SoLuongTon",
            "GiaNhap",
            "NgayCapNhat",
            "MucTonToiThieu",
        };
        ViewData["Fields"] = new[]
        {
            "MaNL",
            "TenNL",
            "DonViTinh",
            "SoLuongTon",
            "GiaNhap",
            "NgayCapNhat",
            "MucTonToiThieu",
        };
        ViewData["KeyNames"] = new[] { "MaNL" };
        ViewData["Labels"] = Labels;
    }

    private void SetDetailUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[]
        {
            "MaNL",
            "TenNL",
            "DonViTinh",
            "SoLuongTon",
            "GiaNhap",
            "NgayCapNhat",
            "MucTonToiThieu",
        };
        ViewData["Fields"] = new[]
        {
            "MaNL",
            "TenNL",
            "DonViTinh",
            "SoLuongTon",
            "GiaNhap",
            "NgayCapNhat",
            "MucTonToiThieu",
        };
        ViewData["KeyNames"] = new[] { "MaNL" };
        ViewData["Labels"] = Labels;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        SetUi("Nguyên liệu");
        try
        {
            return View(
                (
                    await _context
                        .NguyenLieus.FromSqlInterpolated($"EXEC NguyenLieu_DS")
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
            return NotFound();
        SetDetailUi("Chi tiết Nguyên liệu");
        try
        {
            var item = (
                await _context
                    .NguyenLieus.FromSqlInterpolated($"EXEC NguyenLieu_CT {id}")
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
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Create()
    {
        SetUi("Thêm Nguyên liệu");
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.NguyenLieus.Select(x => x.MaNL),
            "NL",
            2
        );
        return View(new NguyenLieu());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Create(NguyenLieu model)
    {
        SetUi("Thêm Nguyên liệu");
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.NguyenLieus.Select(x => x.MaNL),
            "NL",
            2
        );
        ModelState.Remove("MaNL");
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC NguyenLieu_Them {(string?)null}, {model.TenNL}, {model.DonViTinh}, {model.SoLuongTon}, {model.GiaNhap}, {model.NgayCapNhat}, {model.MucTonToiThieu}"
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
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
            return NotFound();
        SetUi("Sửa Nguyên liệu");
        try
        {
            var item = (
                await _context
                    .NguyenLieus.FromSqlInterpolated($"EXEC NguyenLieu_CT {id}")
                    .ToListAsync()
            ).FirstOrDefault();
            return item == null ? NotFound() : View(MapToModel(item));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải dữ liệu sửa: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Edit(string? id, NguyenLieu model)
    {
        SetUi("Sửa Nguyên liệu");
        if (id == null || Convert.ToString(id) != Convert.ToString(model.MaNL))
            return NotFound();
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC NguyenLieu_Sua {model.MaNL}, {model.TenNL}, {model.DonViTinh}, {model.SoLuongTon}, {model.GiaNhap}, {model.NgayCapNhat}, {model.MucTonToiThieu}"
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
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
            return NotFound();
        SetDetailUi("Xóa Nguyên liệu");
        try
        {
            var item = (
                await _context
                    .NguyenLieus.FromSqlInterpolated($"EXEC NguyenLieu_CT {id}")
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
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC NguyenLieu_Xoa {id}");
            TempData["Success"] = "Xóa dữ liệu thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa dữ liệu: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    private static NguyenLieu MapToModel(object item)
    {
        if (item is NguyenLieu e)
            return e;
        var model = new NguyenLieu();
        foreach (var p in typeof(NguyenLieu).GetProperties())
        {
            var s = item.GetType().GetProperty(p.Name);
            if (s != null && p.CanWrite)
                p.SetValue(model, s.GetValue(item));
        }
        return model;
    }
}
