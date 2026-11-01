using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.Services;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy, AppRoles.PhucVu)]
public class HoaDonController : BaseController
{
    private const string TrangThaiDangBan = "Đang bán";

    private readonly AppDbContext _context;
    private static readonly Dictionary<string, string> Labels = new()
    {
        ["MaHD"] = "Mã HĐ",
        ["NgayLap"] = "Ngày lập",
        ["ThoiGianTao"] = "Ngày lập",
        ["ThoiGianThanhToan"] = "Thời gian thanh toán",
        ["MaNV"] = "Mã NV",
        ["TenNV"] = "Tên NV",
        ["MaKH"] = "Mã KH",
        ["TenKH"] = "Tên KH",
        ["MaBan"] = "Mã bàn",
        ["TenBan"] = "Tên bàn",
        ["TongTien"] = "Tổng tiền",
        ["TrangThaiThanhToanHienThi"] = "Trạng thái thanh toán",
        ["GhiChu"] = "Ghi chú",
        ["MaSP"] = "Mã SP",
        ["TenSP"] = "Tên SP",
        ["SoLuong"] = "Số lượng",
        ["DonGia"] = "Đơn giá",
        ["ThanhTien"] = "Thành tiền",
        ["MaKM"] = "Mã KM",
        ["TenKM"] = "Tên KM",
    };

    public HoaDonController(AppDbContext context) => _context = context;

    private void SetHoaDonUi(string title)
    {
        ViewData["Title"] = title;
        ViewData["Columns"] = new[]
        {
            "MaHD",
            "ThoiGianTao",
            "TenNV",
            "TenKH",
            "TenBan",
            "TongTien",
            "TrangThaiThanhToanHienThi",
            "GhiChu",
        };
        ViewData["Fields"] = new[] { "MaHD", "NgayLap", "MaNV", "MaKH", "GhiChu", "MaBan" };
        ViewData["KeyNames"] = new[] { "MaHD" };
        ViewData["Labels"] = Labels;
    }

    private async Task LoadHoaDonDropdownsAsync()
    {
        var nvs = await _context
            .NhanVienResults.FromSqlInterpolated($"EXEC NhanVien_DS")
            .ToListAsync();
        var khs = await _context.KhachHangs.FromSqlInterpolated($"EXEC KhachHang_DS").ToListAsync();
        var bans = await _context.Bans.FromSqlInterpolated($"EXEC Ban_DS").ToListAsync();
        ViewData["Dropdowns"] = new Dictionary<string, SelectList>
        {
            ["MaNV"] = new SelectList(nvs, "MaNV", "TenNV"),
            ["MaKH"] = new SelectList(
                khs.Prepend(new KhachHang { MaKH = "", TenKH = "Không chọn" }),
                "MaKH",
                "TenKH"
            ),
            ["MaBan"] = new SelectList(bans, "MaBan", "TenBan"),
        };
    }

    private async Task LoadDetailDropdownsAsync(bool lockKeys = false)
    {
        var sps = await _context
            .SanPhamResults.FromSqlInterpolated($"EXEC SanPham_DS")
            .ToListAsync();
        var kms = await _context.KhuyenMais.FromSqlInterpolated($"EXEC KhuyenMai_DS").ToListAsync();
        ViewData["Dropdowns"] = new Dictionary<string, SelectList>
        {
            ["MaSP"] = new SelectList(sps, "MaSP", "TenSP"),
            ["MaKM"] = new SelectList(
                kms.Prepend(new KhuyenMai { MaKM = "", TenKM = "Không chọn" }),
                "MaKM",
                "TenKM"
            ),
        };
        ViewData["Title"] = lockKeys ? "Sửa chi tiết hóa đơn" : "Thêm chi tiết hóa đơn";
        ViewData["Fields"] = new[] { "MaHD", "MaSP", "SoLuong", "DonGia", "MaKM" };
        ViewData["ReadonlyFields"] = lockKeys
            ? new HashSet<string> { "MaHD", "MaSP" }
            : new HashSet<string> { "MaHD" };
        ViewData["Labels"] = Labels;
    }

    private async Task<(bool IsValid, decimal DonGia)> TryGetDonGiaSanPhamAsync(CTHD model)
    {
        var product = await _context
            .SanPhams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaSP == model.MaSP);

        if (product == null)
        {
            ModelState.AddModelError(string.Empty, "Sản phẩm không tồn tại.");
            return (false, 0);
        }

        if (product.TrangThai != TrangThaiDangBan)
        {
            ModelState.AddModelError(
                string.Empty,
                "Sản phẩm hiện không ở trạng thái Đang bán."
            );
            return (false, 0);
        }

        return (true, product.DonGia);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        SetHoaDonUi("Thêm hóa đơn");
        try
        {
            var data = await BuildHoaDonQuery()
                .OrderByDescending(x => x.ThoiGianTao)
                .ThenByDescending(x => x.MaHD)
                .ToListAsync();
            await AttachLatestPaymentsAsync(data);

            return View(data.Cast<object>());
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải hóa đơn: " + FriendlyControllerError(ex);
            return View(Enumerable.Empty<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
            return NotFound();
        try
        {
            var header = (
                await BuildHoaDonQuery()
                    .Where(x => x.MaHD == id)
                    .ToListAsync()
            ).FirstOrDefault();
            if (header == null)
                return NotFound();
            await AttachLatestPaymentsAsync(new List<HoaDonViewModel> { header });
            ViewData["Details"] = await _context
                .CTHDResults.FromSqlInterpolated($"EXEC CTHD_DSTheoMaHD {id}")
                .ToListAsync();
            return View(header);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xem hóa đơn: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Index));
        }
    }

    private IQueryable<HoaDonViewModel> BuildHoaDonQuery()
    {
        return from h in _context.HoaDons.AsNoTracking()
               join nv in _context.NhanViens.AsNoTracking() on h.MaNV equals nv.MaNV
               join b in _context.Bans.AsNoTracking() on h.MaBan equals b.MaBan
               join kh in _context.KhachHangs.AsNoTracking() on h.MaKH equals kh.MaKH into khJoin
               from kh in khJoin.DefaultIfEmpty()
               select new HoaDonViewModel
               {
                   MaHD = h.MaHD,
                   NgayLap = h.NgayLap,
                   ThoiGianTao = h.ThoiGianTao,
                   ThoiGianThanhToan = h.ThoiGianThanhToan,
                   TrangThaiOrder = h.TrangThaiOrder,
                   TrangThaiThanhToan = h.TrangThaiThanhToan,
                   PhuongThucThanhToan = h.PhuongThucThanhToan,
                   MaNV = h.MaNV,
                   TenNV = nv.TenNV,
                   MaKH = h.MaKH,
                   TenKH = kh == null ? null : kh.TenKH,
                   MaBan = h.MaBan,
                   TenBan = b.TenBan,
                   TongTien = h.TongTien,
                   GhiChu = h.GhiChu,
               };
    }

    private async Task AttachLatestPaymentsAsync(List<HoaDonViewModel> invoices)
    {
        if (invoices.Count == 0)
        {
            return;
        }

        var invoiceIds = invoices
            .Select(x => x.MaHD)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        var payments = await _context.ThanhToans
            .AsNoTracking()
            .Where(x => x.MaHD != null && invoiceIds.Contains(x.MaHD!))
            .OrderByDescending(x => x.ThoiGianTao)
            .ThenByDescending(x => x.MaThanhToan)
            .ToListAsync();
        var latestByInvoice = payments
            .GroupBy(x => x.MaHD!)
            .ToDictionary(x => x.Key, x => x.First());

        foreach (var invoice in invoices)
        {
            if (!latestByInvoice.TryGetValue(invoice.MaHD, out var payment))
            {
                continue;
            }

            invoice.MaThanhToanGanNhat = payment.MaThanhToan;
            invoice.MaPhienThanhToan = payment.MaPhienThanhToan;
            invoice.TrangThaiThanhToanGanNhat = payment.TrangThai;
            invoice.PhuongThucThanhToanGanNhat = payment.PhuongThuc;
            invoice.ThoiGianHetHanQr =
                payment.PhuongThuc == OrderWorkflowConstants.PhuongThucQr
                    ? payment.ThoiGianHetHan
                    : null;
        }
    }

    [HttpGet]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Create()
    {
        SetHoaDonUi("Thêm hóa đơn");
        await LoadHoaDonDropdownsAsync();
        var ngayLap = DateTime.Today;
        ViewBag.NextCode = await NextCodeHelper.NextHoaDonAsync(
            _context.HoaDons.Select(x => x.MaHD),
            ngayLap
        );
        return View(new HoaDon { NgayLap = ngayLap });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Create(HoaDon model)
    {
        SetHoaDonUi("Thêm hóa đơn");
        await LoadHoaDonDropdownsAsync();
        var ngayLap = model.NgayLap == default ? DateTime.Today : model.NgayLap;
        ViewBag.NextCode = await NextCodeHelper.NextHoaDonAsync(
            _context.HoaDons.Select(x => x.MaHD),
            ngayLap
        );
        ModelState.Remove("MaHD");
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC HoaDon_Them {(string?)null}, {model.NgayLap}, {model.MaNV}, {model.MaKH}, {model.GhiChu}, {model.MaBan}"
            );
            TempData["Success"] = "Thêm hóa đơn thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể thêm hóa đơn: " + FriendlyControllerError(ex);
            return View(model);
        }
    }

    private async Task<(bool CanModify, string Reason)> CanModifyHoaDonAsync(string? maHD)
    {
        if (string.IsNullOrWhiteSpace(maHD))
            return (false, "Mã hóa đơn không hợp lệ.");

        var hoaDon = await _context.HoaDons.AsNoTracking()
            .Where(x => x.MaHD == maHD)
            .Select(x => new { x.TrangThaiOrder, x.TrangThaiThanhToan })
            .FirstOrDefaultAsync();

        if (hoaDon == null)
            return (false, "Không tìm thấy hóa đơn.");

        if (hoaDon.TrangThaiOrder == OrderWorkflowConstants.TrangThaiOrderDaHuy)
            return (false, "Hóa đơn đã hủy, không thể chỉnh sửa hoặc thao tác chi tiết.");

        if (hoaDon.TrangThaiThanhToan == OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan)
            return (false, "Hóa đơn đã thanh toán, không thể chỉnh sửa hoặc thao tác chi tiết.");

        if (hoaDon.TrangThaiThanhToan == OrderWorkflowConstants.TrangThaiThanhToanChoThanhToanQr)
            return (false, "Hóa đơn đang chờ thanh toán QR, không thể chỉnh sửa hoặc thao tác chi tiết.");

        return (true, string.Empty);
    }

    [HttpGet]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
            return NotFound();

        var check = await CanModifyHoaDonAsync(id);
        if (!check.CanModify)
        {
            TempData["Error"] = check.Reason;
            return RedirectToAction(nameof(Details), new { id });
        }

        SetHoaDonUi("Thêm hóa đơn");
        await LoadHoaDonDropdownsAsync();
        var item = (
            await _context.HoaDonResults.FromSqlInterpolated($"EXEC HoaDon_CT {id}").ToListAsync()
        ).FirstOrDefault();
        return item == null
            ? NotFound()
            : View(
                new HoaDon
                {
                    MaHD = item.MaHD,
                    NgayLap = item.NgayLap,
                    MaNV = item.MaNV,
                    MaKH = item.MaKH,
                    GhiChu = item.GhiChu,
                    MaBan = item.MaBan,
                }
            );
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Edit(string? id, HoaDon model)
    {
        SetHoaDonUi("Thêm hóa đơn");
        await LoadHoaDonDropdownsAsync();
        if (id == null || id != model.MaHD)
            return NotFound();

        var check = await CanModifyHoaDonAsync(model.MaHD);
        if (!check.CanModify)
        {
            TempData["Error"] = check.Reason;
            return RedirectToAction(nameof(Details), new { id = model.MaHD });
        }

        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC HoaDon_Sua {model.MaHD}, {model.NgayLap}, {model.MaNV}, {model.MaKH}, {model.GhiChu}, {model.MaBan}"
            );
            TempData["Success"] = "Cập nhật hóa đơn thành công.";
            return RedirectToAction(nameof(Details), new { id = model.MaHD });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể cập nhật hóa đơn: " + FriendlyControllerError(ex);
            return View(model);
        }
    }

    [HttpGet]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
            return NotFound();
        SetHoaDonUi("Thêm hóa đơn");
        var item = (
            await _context.HoaDonResults.FromSqlInterpolated($"EXEC HoaDon_CT {id}").ToListAsync()
        ).FirstOrDefault();
        return item == null ? NotFound() : View((object)item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> DeleteConfirmed(string? id)
    {
        if (id == null)
            return NotFound();
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC HoaDon_Xoa {id}");
            TempData["Success"] = "Xóa hóa đơn thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa hóa đơn: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> AddDetail(string maHD)
    {
        var check = await CanModifyHoaDonAsync(maHD);
        if (!check.CanModify)
        {
            TempData["Error"] = check.Reason;
            return RedirectToAction(nameof(Details), new { id = maHD });
        }

        await LoadDetailDropdownsAsync();
        return View(new CTHD { MaHD = maHD });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> AddDetail(CTHD model)
    {
        var check = await CanModifyHoaDonAsync(model.MaHD);
        if (!check.CanModify)
        {
            TempData["Error"] = check.Reason;
            return RedirectToAction(nameof(Details), new { id = model.MaHD });
        }

        await LoadDetailDropdownsAsync();
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            var productPrice = await TryGetDonGiaSanPhamAsync(model);
            if (!productPrice.IsValid)
            {
                TempData["Error"] = "Không thể cập nhật chi tiết hóa đơn.";
                return View(model);
            }

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC CTHD_Them {model.MaHD}, {model.MaSP}, {model.SoLuong}, {productPrice.DonGia}, {model.MaKM}"
            );
            TempData["Success"] = "Thêm chi tiết thành công.";
            return RedirectToAction(nameof(Details), new { id = model.MaHD });
        }
        catch (Exception)
        {
            TempData["Error"] = "Không thể cập nhật chi tiết hóa đơn.";
            return View(model);
        }
    }

    [HttpGet]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> EditDetail(string maHD, string maSP)
    {
        var check = await CanModifyHoaDonAsync(maHD);
        if (!check.CanModify)
        {
            TempData["Error"] = check.Reason;
            return RedirectToAction(nameof(Details), new { id = maHD });
        }

        await LoadDetailDropdownsAsync(true);
        var item = (
            await _context
                .CTHDResults.FromSqlInterpolated($"EXEC CTHD_DSTheoMaHD {maHD}")
                .ToListAsync()
        ).FirstOrDefault(x => x.MaSP == maSP);
        return item == null
            ? NotFound()
            : View(
                new CTHD
                {
                    MaHD = item.MaHD,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia,
                    MaKM = item.MaKM,
                }
            );
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> EditDetail(CTHD model)
    {
        var check = await CanModifyHoaDonAsync(model.MaHD);
        if (!check.CanModify)
        {
            TempData["Error"] = check.Reason;
            return RedirectToAction(nameof(Details), new { id = model.MaHD });
        }

        await LoadDetailDropdownsAsync(true);
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            var productPrice = await TryGetDonGiaSanPhamAsync(model);
            if (!productPrice.IsValid)
            {
                TempData["Error"] = "Không thể cập nhật chi tiết hóa đơn.";
                return View(model);
            }

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC CTHD_Sua {model.MaHD}, {model.MaSP}, {model.SoLuong}, {productPrice.DonGia}, {model.MaKM}"
            );
            TempData["Success"] = "Cập nhật chi tiết thành công.";
            return RedirectToAction(nameof(Details), new { id = model.MaHD });
        }
        catch (Exception)
        {
            TempData["Error"] = "Không thể cập nhật chi tiết hóa đơn.";
            return View(model);
        }
    }

    [HttpGet]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> DeleteDetail(string maHD, string maSP)
    {
        var check = await CanModifyHoaDonAsync(maHD);
        if (!check.CanModify)
        {
            TempData["Error"] = check.Reason;
            return RedirectToAction(nameof(Details), new { id = maHD });
        }

        ViewData["Title"] = "Xóa chi tiết hóa đơn";
        ViewData["Columns"] = new[]
        {
            "MaHD",
            "MaSP",
            "TenSP",
            "SoLuong",
            "DonGia",
            "ThanhTien",
            "MaKM",
            "TenKM",
        };
        ViewData["Labels"] = Labels;
        var item = (
            await _context
                .CTHDResults.FromSqlInterpolated($"EXEC CTHD_DSTheoMaHD {maHD}")
                .ToListAsync()
        ).FirstOrDefault(x => x.MaSP == maSP);
        return item == null ? NotFound() : View((object)item);
    }

    [HttpPost, ActionName("DeleteDetail"), ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> DeleteDetailConfirmed(string maHD, string maSP)
    {
        var check = await CanModifyHoaDonAsync(maHD);
        if (!check.CanModify)
        {
            TempData["Error"] = check.Reason;
            return RedirectToAction(nameof(Details), new { id = maHD });
        }

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC CTHD_Xoa {maHD}, {maSP}");
            TempData["Success"] = "Xóa chi tiết thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa chi tiết: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Details), new { id = maHD });
    }
}
