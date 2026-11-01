using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy)]
public class LuongController : BaseController
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

    public LuongController(AppDbContext c) => _context = c;

    private void SetUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[]
        {
            "MaLuong",
            "Thang",
            "Nam",
            "LuongCoBan",
            "PhuCap",
            "Thuong",
            "Phat",
            "TongLuong",
        };
        ViewData["Fields"] = new[]
        {
            "MaLuong",
            "Thang",
            "Nam",
            "LuongCoBan",
            "PhuCap",
            "Thuong",
            "Phat",
        };
        ViewData["KeyNames"] = new[] { "MaLuong" };
        ViewData["Labels"] = Labels;
    }

    private void SetDetailUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[]
        {
            "MaLuong",
            "Thang",
            "Nam",
            "LuongCoBan",
            "PhuCap",
            "Thuong",
            "Phat",
            "TongLuong",
        };
        ViewData["Fields"] = new[]
        {
            "MaLuong",
            "Thang",
            "Nam",
            "LuongCoBan",
            "PhuCap",
            "Thuong",
            "Phat",
        };
        ViewData["KeyNames"] = new[] { "MaLuong" };
        ViewData["Labels"] = Labels;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        SetUi("Lương");
        try
        {
            return View(
                (
                    await _context.Luongs.FromSqlInterpolated($"EXEC Luong_DS").ToListAsync()
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
    public async Task<IActionResult> Thang(int? thang)
    {
        var selectedMonth = thang ?? DateTime.Today.Month;
        if (selectedMonth < 1 || selectedMonth > 12)
        {
            TempData["Error"] = "Tháng không hợp lệ.";
            selectedMonth = DateTime.Today.Month;
        }

        try
        {
            var tongQuan = (
                await _context
                    .ChamCongTongLuongTatCaThangResults.FromSqlInterpolated(
                        $"EXEC ChamCong_TongLuongTatCaTheoThang {selectedMonth}"
                    )
                    .AsNoTracking()
                    .ToListAsync()
            ).FirstOrDefault();

            var luongNhanVien = await _context
                .ChamCongTongLuongNhanVienThangResults.FromSqlInterpolated(
                    $"EXEC ChamCong_TongLuongNhanVienTheoThang {selectedMonth}"
                )
                .AsNoTracking()
                .ToListAsync();

            var caLam = await _context
                .ChamCongCaLamThangResults.FromSqlInterpolated(
                    $"EXEC ChamCong_CaLamTheoThang {selectedMonth}"
                )
                .AsNoTracking()
                .ToListAsync();

            return View(
                new LuongThangViewModel
                {
                    Thang = selectedMonth,
                    TongQuan = tongQuan,
                    LuongNhanVien = luongNhanVien,
                    CaLam = caLam,
                }
            );
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải lương tháng: " + FriendlyControllerError(ex);
            return View(new LuongThangViewModel { Thang = selectedMonth });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();
        SetDetailUi("Chi tiết Lương");
        try
        {
            var item = (
                await _context.Luongs.FromSqlInterpolated($"EXEC Luong_CT {id}").ToListAsync()
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
        SetUi("Thêm Lương");
        ViewBag.NextCode = await NextCodeHelper.NextNumberAsync(
            _context.Luongs.Select(x => x.MaLuong)
        );
        return View(new Luong());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Luong model)
    {
        SetUi("Thêm Lương");
        ViewBag.NextCode = await NextCodeHelper.NextNumberAsync(
            _context.Luongs.Select(x => x.MaLuong)
        );
        ModelState.Remove("MaLuong");
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC Luong_Them {(int?)null}, {model.Thang}, {model.Nam}, {model.LuongCoBan}, {model.PhuCap}, {model.Thuong}, {model.Phat}"
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
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();
        SetUi("Sửa Lương");
        try
        {
            var item = (
                await _context.Luongs.FromSqlInterpolated($"EXEC Luong_CT {id}").ToListAsync()
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
    public async Task<IActionResult> Edit(int? id, Luong model)
    {
        SetUi("Sửa Lương");
        if (id == null || Convert.ToString(id) != Convert.ToString(model.MaLuong))
            return NotFound();
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC Luong_Sua {model.MaLuong}, {model.Thang}, {model.Nam}, {model.LuongCoBan}, {model.PhuCap}, {model.Thuong}, {model.Phat}"
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
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();
        SetDetailUi("Xóa Lương");
        try
        {
            var item = (
                await _context.Luongs.FromSqlInterpolated($"EXEC Luong_CT {id}").ToListAsync()
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
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
            return NotFound();
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC Luong_Xoa {id}");
            TempData["Success"] = "Xóa dữ liệu thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa dữ liệu: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    private static Luong MapToModel(object item)
    {
        if (item is Luong e)
            return e;
        var model = new Luong();
        foreach (var p in typeof(Luong).GetProperties())
        {
            var s = item.GetType().GetProperty(p.Name);
            if (s != null && p.CanWrite)
                p.SetValue(model, s.GetValue(item));
        }
        return model;
    }
}
