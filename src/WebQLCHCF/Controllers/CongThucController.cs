using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Security;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy)]
public class CongThucController : BaseController
{
    private readonly AppDbContext _context;

    public CongThucController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Công thức pha chế";
        try
        {
            var rows = await _context.CongThucResults
                .FromSqlInterpolated($"EXEC CongThuc_DS")
                .AsNoTracking()
                .ToListAsync();

            return View(
                rows.GroupBy(x => new { x.MaSP, x.TenSP })
                    .Select(x => new CongThucIndexItemViewModel
                    {
                        MaSP = x.Key.MaSP,
                        TenSP = x.Key.TenSP,
                        NguyenLieus = x.OrderBy(item => item.MaNL).ToList(),
                    })
                    .OrderBy(x => x.MaSP)
                    .ToList()
            );
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải công thức: " + FriendlyControllerError(ex);
            return View(new List<CongThucIndexItemViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();
        return View(new CongThucEditorViewModel
        {
            Items = [new CongThucNguyenLieuItemViewModel()],
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CongThucEditorViewModel model)
    {
        await LoadDropdownsAsync(model.MaSP);
        await ValidateEditorAsync(model, requireNewRecipe: true);
        if (!ModelState.IsValid)
            return View(model);

        return await SaveAsync(model, "Thêm công thức thành công.", nameof(Create));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string? maSP)
    {
        if (string.IsNullOrWhiteSpace(maSP))
            return NotFound();

        var model = await GetEditorModelAsync(maSP);
        if (model == null)
            return NotFound();

        await LoadDropdownsAsync(maSP);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string? maSP, CongThucEditorViewModel model)
    {
        if (string.IsNullOrWhiteSpace(maSP) || !string.Equals(maSP, model.MaSP, StringComparison.OrdinalIgnoreCase))
            return NotFound();

        await LoadDropdownsAsync(model.MaSP);
        await ValidateEditorAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        return await SaveAsync(model, "Cập nhật công thức thành công.", nameof(Edit));
    }

    [HttpGet]
    public async Task<IActionResult> Details(string? maSP)
    {
        if (string.IsNullOrWhiteSpace(maSP))
            return NotFound();

        var model = await GetIndexItemAsync(maSP);
        return model == null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string? maSP)
    {
        if (string.IsNullOrWhiteSpace(maSP))
            return NotFound();

        var model = await GetIndexItemAsync(maSP);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost, ActionName(nameof(Delete)), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? maSP)
    {
        if (string.IsNullOrWhiteSpace(maSP))
            return NotFound();

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC CongThuc_XoaTheoSanPham {maSP}"
            );

            TempData["Success"] = "Xóa công thức thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa công thức: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<IActionResult> SaveAsync(
        CongThucEditorViewModel model,
        string successMessage,
        string viewName
    )
    {
        try
        {
            var json = JsonSerializer.Serialize(
                model.Items.Select(x => new { x.MaNL, x.DinhLuong })
            );
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC CongThuc_LuuNhieu {model.MaSP}, {json}"
            );
            TempData["Success"] = successMessage;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, FriendlyControllerError(ex));
            return View(viewName, model);
        }
    }

    private async Task ValidateEditorAsync(
        CongThucEditorViewModel model,
        bool requireNewRecipe = false
    )
    {
        model.MaSP = model.MaSP?.Trim().ToUpperInvariant() ?? string.Empty;
        model.Items ??= new List<CongThucNguyenLieuItemViewModel>();
        if (model.Items.Count == 0 && model.NguyenLieus?.Count > 0)
            model.Items = model.NguyenLieus;
        model.NguyenLieus = model.Items;

        if (string.IsNullOrWhiteSpace(model.MaSP))
            ModelState.AddModelError(nameof(model.MaSP), "Vui lòng chọn sản phẩm.");
        else
        {
            var product = await _context.SanPhams.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaSP == model.MaSP);
            if (product == null)
                ModelState.AddModelError(nameof(model.MaSP), "Sản phẩm không tồn tại.");
            else
            {
                model.TenSP = product.TenSP;
                if (
                    requireNewRecipe
                    && await _context.CongThucs.AsNoTracking().AnyAsync(x => x.MaSP == model.MaSP)
                )
                {
                    ModelState.AddModelError(
                        nameof(model.MaSP),
                        "Sản phẩm đã có công thức. Vui lòng vào chức năng Sửa để cập nhật."
                    );
                }
            }
        }

        if (model.Items.Count == 0)
        {
            ModelState.AddModelError(nameof(model.Items), "Công thức phải có ít nhất một nguyên liệu.");
            ModelState.AddModelError(string.Empty, "Công thức phải có ít nhất một nguyên liệu.");
            return;
        }

        for (var index = 0; index < model.Items.Count; index++)
        {
            var item = model.Items[index];
            item.MaNL = item.MaNL?.Trim().ToUpperInvariant() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(item.MaNL))
                ModelState.AddModelError($"Items[{index}].MaNL", "Vui lòng chọn nguyên liệu.");
            else if (!await _context.NguyenLieus.AsNoTracking().AnyAsync(x => x.MaNL == item.MaNL))
                ModelState.AddModelError($"Items[{index}].MaNL", "Nguyên liệu không tồn tại.");

            if (item.DinhLuong <= 0)
            {
                ModelState.AddModelError($"Items[{index}].DinhLuong", "Định lượng phải lớn hơn 0.");
                ModelState.AddModelError(string.Empty, "Định lượng phải lớn hơn 0.");
            }
        }

        var duplicateIngredients = model.Items
            .Where(x => !string.IsNullOrWhiteSpace(x.MaNL))
            .GroupBy(x => x.MaNL, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < model.Items.Count; index++)
        {
            if (duplicateIngredients.Contains(model.Items[index].MaNL))
            {
                ModelState.AddModelError(
                    $"Items[{index}].MaNL",
                    "Không được chọn trùng nguyên liệu trong cùng một công thức."
                );
            }
        }
        if (duplicateIngredients.Count > 0)
            ModelState.AddModelError(string.Empty, "Không được chọn trùng nguyên liệu trong cùng một công thức.");
    }

    private async Task LoadDropdownsAsync(string? selectedMaSP = null)
    {
        var products = await _context.SanPhams.AsNoTracking().OrderBy(x => x.MaSP).ToListAsync();
        var ingredients = await _context.NguyenLieus.AsNoTracking().OrderBy(x => x.MaNL).ToListAsync();

        ViewBag.SanPhams = new SelectList(products, "MaSP", "TenSP", selectedMaSP);
        ViewBag.NguyenLieus = ingredients;
    }

    private async Task<CongThucEditorViewModel?> GetEditorModelAsync(string maSP)
    {
        var rows = await _context.CongThucResults
            .FromSqlInterpolated($"EXEC CongThuc_TheoSanPham {maSP}")
            .AsNoTracking()
            .ToListAsync();

        if (rows.Count == 0)
            return null;

        return new CongThucEditorViewModel
        {
            MaSP = rows[0].MaSP,
            TenSP = rows[0].TenSP,
            Items = rows.Select(x => new CongThucNguyenLieuItemViewModel
            {
                MaNL = x.MaNL,
                DinhLuong = x.DinhLuong,
            }).ToList(),
        };
    }

    private async Task<CongThucIndexItemViewModel?> GetIndexItemAsync(string maSP)
    {
        var rows = await _context.CongThucResults
            .FromSqlInterpolated($"EXEC CongThuc_TheoSanPham {maSP}")
            .AsNoTracking()
            .ToListAsync();

        if (rows.Count == 0)
            return null;

        return new CongThucIndexItemViewModel
        {
            MaSP = rows[0].MaSP,
            TenSP = rows[0].TenSP,
            NguyenLieus = rows,
        };
    }
}
