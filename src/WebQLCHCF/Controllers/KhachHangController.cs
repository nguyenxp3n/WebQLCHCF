using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

public class KhachHangController : BaseController
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

    public KhachHangController(AppDbContext c) => _context = c;

    private void SetUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[] { "MaKH", "TenKH", "SDT", "DiemTichLuy" };
        ViewData["Fields"] = new[] { "MaKH", "TenKH", "SDT", "DiemTichLuy" };
        ViewData["KeyNames"] = new[] { "MaKH" };
        ViewData["Labels"] = Labels;
    }

    private void SetDetailUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[] { "MaKH", "TenKH", "SDT", "DiemTichLuy" };
        ViewData["Fields"] = new[] { "MaKH", "TenKH", "SDT", "DiemTichLuy" };
        ViewData["KeyNames"] = new[] { "MaKH" };
        ViewData["Labels"] = Labels;
    }

    public async Task<IActionResult> Index()
    {
        SetUi("Khách hàng");
        try
        {
            return View(
                (
                    await _context
                        .KhachHangs.FromSqlInterpolated($"EXEC KhachHang_DS")
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
        SetDetailUi("Chi tiết Khách hàng");
        try
        {
            var item = (
                await _context
                    .KhachHangs.FromSqlInterpolated($"EXEC KhachHang_CT {id}")
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
        SetUi("Thêm Khách hàng");
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.KhachHangs.Select(x => x.MaKH),
            "KH",
            2
        );
        return View(new KhachHang());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KhachHang model)
    {
        SetUi("Thêm Khách hàng");
        ViewBag.NextCode = await NextCodeHelper.NextPrefixedCodeAsync(
            _context.KhachHangs.Select(x => x.MaKH),
            "KH",
            2
        );
        NormalizeInput(model);
        ModelState.Remove("MaKH");
        AddInputValidation(model);
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC KhachHang_Them {(string?)null}, {model.TenKH}, {model.SDT}, {model.DiemTichLuy}"
            );
            TempData["Success"] = "Thêm dữ liệu thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, FriendlyKhachHangError(ex));
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
            return NotFound();
        SetUi("Sửa Khách hàng");
        try
        {
            var item = (
                await _context
                    .KhachHangs.FromSqlInterpolated($"EXEC KhachHang_CT {id}")
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
    public async Task<IActionResult> Edit(string? id, KhachHang model)
    {
        SetUi("Sửa Khách hàng");
        if (id == null || Convert.ToString(id) != Convert.ToString(model.MaKH))
            return NotFound();
        NormalizeInput(model);
        AddInputValidation(model);
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC KhachHang_Sua {model.MaKH}, {model.TenKH}, {model.SDT}, {model.DiemTichLuy}"
            );
            TempData["Success"] = "Cập nhật dữ liệu thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, FriendlyKhachHangError(ex));
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
            return NotFound();
        SetDetailUi("Xóa Khách hàng");
        try
        {
            var item = (
                await _context
                    .KhachHangs.FromSqlInterpolated($"EXEC KhachHang_CT {id}")
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
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC KhachHang_Xoa {id}");
            TempData["Success"] = "Xóa dữ liệu thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa dữ liệu: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    private static KhachHang MapToModel(object item)
    {
        if (item is KhachHang e)
            return e;
        var model = new KhachHang();
        foreach (var p in typeof(KhachHang).GetProperties())
        {
            var s = item.GetType().GetProperty(p.Name);
            if (s != null && p.CanWrite)
                p.SetValue(model, s.GetValue(item));
        }
        return model;
    }

    private static void NormalizeInput(KhachHang model)
    {
        model.TenKH = model.TenKH?.Trim() ?? string.Empty;
        model.SDT = model.SDT?.Trim() ?? string.Empty;
    }

    private void AddInputValidation(KhachHang model)
    {
        if (string.IsNullOrWhiteSpace(model.TenKH))
            ModelState.AddModelError(nameof(model.TenKH), "Tên khách hàng không được để trống.");

        if (string.IsNullOrWhiteSpace(model.SDT))
            ModelState.AddModelError(nameof(model.SDT), "Số điện thoại không được để trống.");
        else if (!System.Text.RegularExpressions.Regex.IsMatch(model.SDT, @"^\d{10}$"))
            ModelState.AddModelError(nameof(model.SDT), "Số điện thoại chỉ được nhập 10 chữ số.");
    }

    private static string FriendlyKhachHangError(Exception ex)
    {
        var message = ex.GetBaseException().Message;

        if (message.Contains("Tên khách hàng đã tồn tại.", StringComparison.OrdinalIgnoreCase))
            return "Tên khách hàng đã tồn tại.";

        if (message.Contains("Số điện thoại khách hàng đã tồn tại.", StringComparison.OrdinalIgnoreCase))
            return "Số điện thoại khách hàng đã tồn tại.";

        if (message.Contains("Tên khách hàng không được để trống.", StringComparison.OrdinalIgnoreCase))
            return "Tên khách hàng không được để trống.";

        if (message.Contains("Số điện thoại không được để trống.", StringComparison.OrdinalIgnoreCase))
            return "Số điện thoại không được để trống.";

        return FriendlyControllerError(ex);
    }
}
