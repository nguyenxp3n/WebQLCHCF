using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy)]
public class LoaiSPController : BaseController
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

    public LoaiSPController(AppDbContext c) => _context = c;

    private void SetUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[] { "MaLoaiSP", "TenLoaiSP" };
        ViewData["Fields"] = new[] { "MaLoaiSP", "TenLoaiSP" };
        ViewData["KeyNames"] = new[] { "MaLoaiSP" };
        ViewData["Labels"] = Labels;
    }

    private void SetDetailUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[] { "MaLoaiSP", "TenLoaiSP" };
        ViewData["Fields"] = new[] { "MaLoaiSP", "TenLoaiSP" };
        ViewData["KeyNames"] = new[] { "MaLoaiSP" };
        ViewData["Labels"] = Labels;
    }

    public async Task<IActionResult> Index()
    {
        SetUi("Loại sản phẩm");
        try
        {
            return View(
                (
                    await _context.LoaiSPs.FromSqlInterpolated($"EXEC LoaiSP_DS").ToListAsync()
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
        SetDetailUi("Chi tiết Loại sản phẩm");
        try
        {
            var item = (
                await _context.LoaiSPs.FromSqlInterpolated($"EXEC LoaiSP_CT {id}").ToListAsync()
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
        SetUi("Thêm Loại sản phẩm");
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.LoaiSPs.Select(x => x.MaLoaiSP),
            "L",
            2
        );
        return View(new LoaiSP());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LoaiSP model)
    {
        SetUi("Thêm Loại sản phẩm");
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.LoaiSPs.Select(x => x.MaLoaiSP),
            "L",
            2
        );
        ModelState.Remove("MaLoaiSP");
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC LoaiSP_Them {(string?)null}, {model.TenLoaiSP}"
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

    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
            return NotFound();
        SetUi("Sửa Loại sản phẩm");
        try
        {
            var item = (
                await _context.LoaiSPs.FromSqlInterpolated($"EXEC LoaiSP_CT {id}").ToListAsync()
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
    public async Task<IActionResult> Edit(string? id, LoaiSP model)
    {
        SetUi("Sửa Loại sản phẩm");
        if (id == null || Convert.ToString(id) != Convert.ToString(model.MaLoaiSP))
            return NotFound();
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC LoaiSP_Sua {model.MaLoaiSP}, {model.TenLoaiSP}"
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

    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
            return NotFound();
        SetDetailUi("Xóa Loại sản phẩm");
        try
        {
            var item = (
                await _context.LoaiSPs.FromSqlInterpolated($"EXEC LoaiSP_CT {id}").ToListAsync()
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
            return NotFound();
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC LoaiSP_Xoa {id}");
            TempData["Success"] = "Xóa dữ liệu thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa dữ liệu: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    private static LoaiSP MapToModel(object item)
    {
        if (item is LoaiSP e)
            return e;
        var model = new LoaiSP();
        foreach (var p in typeof(LoaiSP).GetProperties())
        {
            var s = item.GetType().GetProperty(p.Name);
            if (s != null && p.CanWrite)
                p.SetValue(model, s.GetValue(item));
        }
        return model;
    }
}
