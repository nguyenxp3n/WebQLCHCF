using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy)]
public class NhanVienController : BaseController
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
        ["LuongTheoGio"] = "Lương theo giờ",
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

    public NhanVienController(AppDbContext c) => _context = c;

    private void SetUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[]
        {
            "MaNV",
            "TenNV",
            "GioiTinh",
            "NgaySinh",
            "SDT",
            "ChucVu",
            "MaLuong",
            "LuongTheoGio",
            "TongLuong",
        };
        ViewData["Fields"] = new[]
        {
            "MaNV",
            "TenNV",
            "GioiTinh",
            "NgaySinh",
            "SDT",
            "ChucVu",
            "MaLuong",
            "LuongTheoGio",
        };
        ViewData["KeyNames"] = new[] { "MaNV" };
        ViewData["Labels"] = Labels;
    }

    private void SetDetailUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[]
        {
            "MaNV",
            "TenNV",
            "GioiTinh",
            "NgaySinh",
            "SDT",
            "ChucVu",
            "MaLuong",
            "LuongTheoGio",
            "TongLuong",
        };
        ViewData["Fields"] = new[]
        {
            "MaNV",
            "TenNV",
            "GioiTinh",
            "NgaySinh",
            "SDT",
            "ChucVu",
            "MaLuong",
            "LuongTheoGio",
        };
        ViewData["KeyNames"] = new[] { "MaNV" };
        ViewData["Labels"] = Labels;
    }

    private async Task LoadDropdownsAsync(int? selectedMaLuong = null)
    {
        var data = await _context.Luongs.FromSqlInterpolated($"EXEC Luong_DS").ToListAsync();
        ViewData["Dropdowns"] = new Dictionary<string, SelectList>
        {
            ["MaLuong"] = new SelectList(data, "MaLuong", "MaLuong", selectedMaLuong),
        };
    }

    public async Task<IActionResult> Index()
    {
        SetUi("Nhân viên");
        try
        {
            return View(
                (
                    await _context
                        .NhanVienResults.FromSqlInterpolated($"EXEC NhanVien_DS")
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
        SetDetailUi("Chi tiết Nhân viên");
        try
        {
            var item = (
                await _context
                    .NhanVienResults.FromSqlInterpolated($"EXEC NhanVien_CT {id}")
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
        SetUi("Thêm Nhân viên");
        await LoadDropdownsAsync();
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.NhanViens.Select(x => x.MaNV),
            "NV",
            2
        );
        return View(new NhanVien());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NhanVien model)
    {
        SetUi("Thêm Nhân viên");
        await LoadDropdownsAsync();
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.NhanViens.Select(x => x.MaNV),
            "NV",
            2
        );
        ModelState.Remove("MaNV");
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC NhanVien_Them {(string?)null}, {model.TenNV}, {model.GioiTinh}, {model.NgaySinh}, {model.SDT}, {model.ChucVu}, {model.MaLuong}, {model.LuongTheoGio}"
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
        SetUi("Sửa Nhân viên");
        try
        {
            var item = (
                await _context
                    .NhanVienResults.FromSqlInterpolated($"EXEC NhanVien_CT {id}")
                    .ToListAsync()
            ).FirstOrDefault();
            if (item == null)
                return NotFound();
            var model = MapToModel(item);
            await LoadDropdownsAsync(model.MaLuong);
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải dữ liệu sửa: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string? id, NhanVien model)
    {
        SetUi("Sửa Nhân viên");
        await LoadDropdownsAsync(model.MaLuong);
        if (id == null || Convert.ToString(id) != Convert.ToString(model.MaNV))
            return NotFound();
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC NhanVien_Sua {model.MaNV}, {model.TenNV}, {model.GioiTinh}, {model.NgaySinh}, {model.SDT}, {model.ChucVu}, {model.MaLuong}, {model.LuongTheoGio}"
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
        SetDetailUi("Xóa Nhân viên");
        try
        {
            var item = (
                await _context
                    .NhanVienResults.FromSqlInterpolated($"EXEC NhanVien_CT {id}")
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
            return NotFound();
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC NhanVien_Xoa {id}");
            TempData["Success"] = "Xóa dữ liệu thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa dữ liệu: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    private static NhanVien MapToModel(object item)
    {
        if (item is NhanVien e)
            return e;
        var model = new NhanVien();
        foreach (var p in typeof(NhanVien).GetProperties())
        {
            var s = item.GetType().GetProperty(p.Name);
            if (s != null && p.CanWrite)
                p.SetValue(model, s.GetValue(item));
        }
        return model;
    }
}
