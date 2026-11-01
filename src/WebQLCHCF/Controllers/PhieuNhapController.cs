using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Security;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy)]
public class PhieuNhapController : BaseController
{
    private readonly AppDbContext _context;
    private static readonly Dictionary<string, string> Labels = new()
    {
        ["MaPN"] = "Mã PN",
        ["NgayNhap"] = "Ngày nhập",
        ["MaNV"] = "Mã NV",
        ["TenNV"] = "Tên NV",
        ["TongTien"] = "Tổng tiền",
        ["MaNL"] = "Mã NL",
        ["TenNL"] = "Tên NL",
        ["DonViTinh"] = "ĐVT",
        ["SoLuong"] = "Số lượng",
        ["DonGia"] = "Đơn giá",
        ["ThanhTien"] = "Thành tiền",
    };

    public PhieuNhapController(AppDbContext context) => _context = context;

    private void SetUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[] { "MaPN", "NgayNhap", "TenNV", "TongTien" };
        ViewData["Fields"] = new[] { "MaPN", "NgayNhap", "MaNV" };
        ViewData["KeyNames"] = new[] { "MaPN" };
        ViewData["Labels"] = Labels;
    }

    private async Task LoadDropdownsAsync(bool includeNguyenLieu = false, string? selectedMaNL = null)
    {
        var nvs = await _context
            .NhanVienResults.FromSqlInterpolated($"EXEC NhanVien_DS")
            .ToListAsync();
        var dropdowns = new Dictionary<string, SelectList>
        {
            ["MaNV"] = new SelectList(nvs, "MaNV", "TenNV"),
        };
        if (includeNguyenLieu)
        {
            var nls = await _context
                .NguyenLieus.FromSqlInterpolated($"EXEC NguyenLieu_DS")
                .ToListAsync();
            dropdowns["MaNL"] = new SelectList(nls, "MaNL", "TenNL", selectedMaNL);
        }

        ViewData["Dropdowns"] = dropdowns;
    }

    private async Task LoadDetailDropdownsAsync(bool lockKeys = false)
    {
        var nls = await _context
            .NguyenLieus.FromSqlInterpolated($"EXEC NguyenLieu_DS")
            .ToListAsync();
        ViewData["Dropdowns"] = new Dictionary<string, SelectList>
        {
            ["MaNL"] = new SelectList(nls, "MaNL", "TenNL"),
        };
        ViewData["Title"] = lockKeys ? "Sửa chi tiết phiếu nhập" : "Thêm chi tiết phiếu nhập";
        ViewData["Fields"] = new[] { "MaPN", "MaNL", "SoLuong", "DonGia" };
        ViewData["ReadonlyFields"] = lockKeys
            ? new HashSet<string> { "MaPN", "MaNL" }
            : new HashSet<string> { "MaPN" };
        ViewData["Labels"] = Labels;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        SetUi("Phiếu nhập");
        try
        {
            return View(
                (
                    await _context
                        .PhieuNhapResults.FromSqlInterpolated($"EXEC PhieuNhap_DS")
                        .ToListAsync()
                ).Cast<object>()
            );
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải phiếu nhập: " + FriendlyControllerError(ex);
            return View(Enumerable.Empty<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
            return NotFound();
        var h = (
            await _context
                .PhieuNhapResults.FromSqlInterpolated($"EXEC PhieuNhap_CT {id}")
                .ToListAsync()
        ).FirstOrDefault();
        if (h == null)
            return NotFound();
        ViewData["Details"] = await _context
            .CTPNResults.FromSqlInterpolated($"EXEC CTPN_DSTheoMaPN {id}")
            .ToListAsync();
        return View(h);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetUi("Thêm phiếu nhập");
        await LoadDropdownsAsync(includeNguyenLieu: true);
        var ngayNhap = DateTime.Today;
        ViewBag.NextCode = await NextCodeHelper.NextPhieuNhapAsync(
            _context.PhieuNhaps.Select(x => x.MaPN),
            ngayNhap
        );
        return View(new PhieuNhap { NgayNhap = ngayNhap });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PhieuNhap model,
        string? maNL,
        int? soLuong,
        decimal? donGia
    )
    {
        SetUi("Thêm phiếu nhập");
        await LoadDropdownsAsync(includeNguyenLieu: true, selectedMaNL: maNL);
        var ngayNhap = model.NgayNhap == default ? DateTime.Today : model.NgayNhap;
        model.NgayNhap = ngayNhap;
        ViewBag.NextCode = await NextCodeHelper.NextPhieuNhapAsync(
            _context.PhieuNhaps.Select(x => x.MaPN),
            ngayNhap
        );
        ViewBag.InitialSoLuong = soLuong;
        ViewBag.InitialDonGia = donGia;
        ModelState.Remove("MaPN");
        if (string.IsNullOrWhiteSpace(maNL))
            ModelState.AddModelError(string.Empty, "Phiếu nhập phải có ít nhất một nguyên liệu.");
        if (!soLuong.HasValue || soLuong.Value <= 0)
            ModelState.AddModelError(string.Empty, "Số lượng nhập phải lớn hơn 0.");
        if (!donGia.HasValue || donGia.Value <= 0)
            ModelState.AddModelError(string.Empty, "Đơn giá nhập phải lớn hơn 0.");
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable
            );
            var maPN = await NextCodeHelper.NextPhieuNhapAsync(
                _context.PhieuNhaps.Select(x => x.MaPN),
                model.NgayNhap
            );
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC PhieuNhap_Them {maPN}, {model.NgayNhap}, {model.MaNV}"
            );
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC CTPN_Them {maPN}, {maNL}, {soLuong!.Value}, {donGia!.Value}"
            );
            await transaction.CommitAsync();
            TempData["Success"] = "Thêm phiếu nhập thành công.";
            return RedirectToAction(nameof(Details), new { id = maPN });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể thêm phiếu nhập: " + FriendlyControllerError(ex);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
            return NotFound();
        SetUi("Sửa phiếu nhập");
        await LoadDropdownsAsync();
        var x = (
            await _context
                .PhieuNhapResults.FromSqlInterpolated($"EXEC PhieuNhap_CT {id}")
                .ToListAsync()
        ).FirstOrDefault();
        return x == null
            ? NotFound()
            : View(
                new PhieuNhap
                {
                    MaPN = x.MaPN,
                    NgayNhap = x.NgayNhap,
                    MaNV = x.MaNV,
                }
            );
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string? id, PhieuNhap model)
    {
        SetUi("Sửa phiếu nhập");
        await LoadDropdownsAsync();
        if (id == null || id != model.MaPN)
            return NotFound();
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC PhieuNhap_Sua {model.MaPN}, {model.NgayNhap}, {model.MaNV}"
            );
            TempData["Success"] = "Cập nhật phiếu nhập thành công.";
            return RedirectToAction(nameof(Details), new { id = model.MaPN });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể cập nhật phiếu nhập: " + FriendlyControllerError(ex);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
            return NotFound();
        SetUi("Xóa phiếu nhập");
        var item = (
            await _context
                .PhieuNhapResults.FromSqlInterpolated($"EXEC PhieuNhap_CT {id}")
                .ToListAsync()
        ).FirstOrDefault();
        return item == null ? NotFound() : View((object)item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? id)
    {
        if (id == null)
            return NotFound();
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC PhieuNhap_Xoa {id}");
            TempData["Success"] = "Xóa phiếu nhập thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa phiếu nhập: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> AddDetail(string maPN)
    {
        await LoadDetailDropdownsAsync();
        return View(new CTPN { MaPN = maPN });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDetail(CTPN model)
    {
        await LoadDetailDropdownsAsync();
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC CTPN_Them {model.MaPN}, {model.MaNL}, {model.SoLuong}, {model.DonGia}"
            );
            TempData["Success"] = "Thêm chi tiết thành công.";
            return RedirectToAction(nameof(Details), new { id = model.MaPN });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể thêm chi tiết: " + FriendlyControllerError(ex);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditDetail(string maPN, string maNL)
    {
        await LoadDetailDropdownsAsync(true);
        var item = (
            await _context
                .CTPNResults.FromSqlInterpolated($"EXEC CTPN_DSTheoMaPN {maPN}")
                .ToListAsync()
        ).FirstOrDefault(x => x.MaNL == maNL);
        return item == null
            ? NotFound()
            : View(
                new CTPN
                {
                    MaPN = item.MaPN,
                    MaNL = item.MaNL,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia,
                }
            );
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDetail(CTPN model)
    {
        await LoadDetailDropdownsAsync(true);
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC CTPN_Sua {model.MaPN}, {model.MaNL}, {model.SoLuong}, {model.DonGia}"
            );
            TempData["Success"] = "Cập nhật chi tiết thành công.";
            return RedirectToAction(nameof(Details), new { id = model.MaPN });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể cập nhật chi tiết: " + FriendlyCtpnError(ex, isDelete: false);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> DeleteDetail(string maPN, string maNL)
    {
        ViewData["Title"] = "Xóa chi tiết phiếu nhập";
        ViewData["Columns"] = new[]
        {
            "MaPN",
            "MaNL",
            "TenNL",
            "DonViTinh",
            "SoLuong",
            "DonGia",
            "ThanhTien",
        };
        ViewData["Labels"] = Labels;
        var item = (
            await _context
                .CTPNResults.FromSqlInterpolated($"EXEC CTPN_DSTheoMaPN {maPN}")
                .ToListAsync()
        ).FirstOrDefault(x => x.MaNL == maNL);
        return item == null ? NotFound() : View((object)item);
    }

    [HttpPost, ActionName("DeleteDetail"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDetailConfirmed(string maPN, string maNL)
    {
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC CTPN_Xoa {maPN}, {maNL}");
            TempData["Success"] = "Xóa chi tiết thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa chi tiết: " + FriendlyCtpnError(ex, isDelete: true);
        }
        return RedirectToAction(nameof(Details), new { id = maPN });
    }

    private static string FriendlyCtpnError(Exception ex, bool isDelete)
    {
        if (ex.Message.Contains("kho không đủ", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("hoàn tác", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("nguyên liệu đã được sử dụng", StringComparison.OrdinalIgnoreCase))
        {
            return isDelete
                ? "Không thể xóa vì nguyên liệu đã được sử dụng, kho không đủ để hoàn tác."
                : "Không thể sửa giảm vì nguyên liệu đã được sử dụng, kho không đủ để hoàn tác.";
        }

        return FriendlyControllerError(ex);
    }
}
