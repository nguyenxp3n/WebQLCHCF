using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Services;
using WebQLCHCF.ViewModels;
using WebQLCHCF.Security;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy, AppRoles.PhucVu)]
public class BanController : BaseController
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private const string MaBanMangVe = OrderWorkflowConstants.MaBanMangVe;
    private const string TrangThaiOffline = OrderWorkflowConstants.TrangThaiBanTrong;
    private const string TrangThaiOnline = OrderWorkflowConstants.TrangThaiBanDangPhucVu;
    private static readonly string[] TrangThaiHopLe = [TrangThaiOffline, TrangThaiOnline];
    private static readonly string[] KhuVucHopLe = ["Tầng trệt", "Tầng lửng"];
    private static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
    {
        ["MaBan"] = "Mã bàn",
        ["SoBan"] = "Số bàn",
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

    public BanController(AppDbContext c, IAuditLogService auditLogService)
    {
        _context = c;
        _auditLogService = auditLogService;
    }

    private void SetUi(string t, bool isCreate = false)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[] { "MaBan", "SoBan", "TenBan", "KhuVuc", "TrangThai" };
        ViewData["Fields"] = isCreate
            ? new[] { "KhuVuc" }
            : new[] { "MaBan", "SoBan", "TenBan", "KhuVuc", "TrangThai" };
        ViewData["KeyNames"] = new[] { "MaBan" };
        ViewData["Labels"] = Labels;
        ViewData["ReadonlyFields"] = new HashSet<string> { "MaBan", "SoBan", "TenBan" };
        ViewData["Dropdowns"] = new Dictionary<string, SelectList>
        {
            ["KhuVuc"] = new SelectList(KhuVucHopLe),
            ["TrangThai"] = new SelectList(TrangThaiHopLe),
        };
    }

    private void SetDetailUi(string t)
    {
        ViewData["Title"] = t;
        ViewData["Columns"] = new[] { "MaBan", "SoBan", "TenBan", "KhuVuc", "TrangThai" };
        ViewData["Fields"] = new[] { "MaBan", "SoBan", "TenBan", "KhuVuc", "TrangThai" };
        ViewData["KeyNames"] = new[] { "MaBan" };
        ViewData["Labels"] = Labels;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        SetUi("Bàn");
        try
        {
            return View(
                (
                    await _context.Bans.FromSqlInterpolated($"EXEC Ban_DS").ToListAsync()
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
        SetDetailUi("Chi tiết Bàn");
        try
        {
            var item = (
                await _context.Bans.FromSqlInterpolated($"EXEC Ban_CT {id}").ToListAsync()
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
    public IActionResult Create()
    {
        SetUi("Thêm Bàn", isCreate: true);
        return View(new Ban { KhuVuc = KhuVucHopLe[0], TrangThai = TrangThaiOffline });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> Create(Ban model)
    {
        SetUi("Thêm Bàn", isCreate: true);
        ModelState.Remove("MaBan");
        ModelState.Remove("TenBan");
        ModelState.Remove("TrangThai");
        if (!KhuVucHopLe.Contains(model.KhuVuc))
            ModelState.AddModelError(nameof(model.KhuVuc), "Khu vực chỉ được chọn Tầng trệt hoặc Tầng lửng.");
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC Ban_Them @KhuVuc={model.KhuVuc}"
            );
            TempData["Success"] = "Thêm bàn thành công. Mã bàn, số bàn và tên bàn đã được hệ thống tự sinh.";
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
        if (string.Equals(id, MaBanMangVe, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "MVE là bàn đặc biệt cho đơn mang về, không thể chỉnh sửa.";
            return RedirectToAction(nameof(Index));
        }
        SetUi("Sửa Bàn");
        try
        {
            var item = (
                await _context.Bans.FromSqlInterpolated($"EXEC Ban_CT {id}").ToListAsync()
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
    public async Task<IActionResult> Edit(string? id, Ban model)
    {
        SetUi("Sửa Bàn");
        if (id == null || Convert.ToString(id) != Convert.ToString(model.MaBan))
            return NotFound();
        if (string.Equals(id, MaBanMangVe, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "MVE là bàn đặc biệt cho đơn mang về, không thể chỉnh sửa.";
            return RedirectToAction(nameof(Index));
        }
        if (!KhuVucHopLe.Contains(model.KhuVuc))
            ModelState.AddModelError(nameof(model.KhuVuc), "Khu vực chỉ được chọn Tầng trệt hoặc Tầng lửng.");

        if (model.TrangThai == OrderWorkflowConstants.TrangThaiBanTrong)
        {
            var hasUnpaidOrder = await _context.HoaDons.AsNoTracking().AnyAsync(x =>
                x.MaBan == model.MaBan
                && x.TrangThaiOrder != OrderWorkflowConstants.TrangThaiOrderDaHuy
                && x.TrangThaiThanhToan != OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan);

            if (hasUnpaidOrder)
            {
                var errorMsg = "Bàn đang có hóa đơn chưa thanh toán, không thể chuyển về trạng thái Trống.";
                ModelState.AddModelError(nameof(model.TrangThai), errorMsg);
                TempData["Error"] = errorMsg;
            }
        }

        if (!ModelState.IsValid)
            return View(model);
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC Ban_Sua @MaBan={model.MaBan}, @KhuVuc={model.KhuVuc}, @TrangThai={model.TrangThai}"
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
        if (string.Equals(id, MaBanMangVe, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "MVE là bàn đặc biệt cho đơn mang về, không thể xóa.";
            return RedirectToAction(nameof(Index));
        }
        SetDetailUi("Xóa Bàn");
        try
        {
            var item = (
                await _context.Bans.FromSqlInterpolated($"EXEC Ban_CT {id}").ToListAsync()
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
        if (string.Equals(id, MaBanMangVe, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "MVE là bàn đặc biệt cho đơn mang về, không thể xóa.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC Ban_Xoa {id}");
            TempData["Success"] = "Xóa dữ liệu thành công.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xóa dữ liệu: " + FriendlyControllerError(ex);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(string maBan)
    {
        try
        {
            var table = await GetPhysicalTableForStatusChangeAsync(maBan);
            if (table.TrangThai == TrangThaiOffline)
            {
                TempData["Success"] = $"Bàn {table.TenBan} hiện đã ở trạng thái Trống.";
                return RedirectToAction(nameof(Index));
            }

            var unpaidOrders = await _context.HoaDons
                .Where(x => x.MaBan == table.MaBan
                    && x.TrangThaiOrder != OrderWorkflowConstants.TrangThaiOrderDaHuy
                    && x.TrangThaiThanhToan != OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan)
                .ToListAsync();

            foreach (var order in unpaidOrders)
            {
                order.TrangThaiOrder = OrderWorkflowConstants.TrangThaiOrderDaHuy;
                order.GhiChu = string.IsNullOrWhiteSpace(order.GhiChu)
                    ? "[Hủy do khôi phục bàn]"
                    : order.GhiChu + " [Hủy do khôi phục bàn]";
            }

            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC Ban_KhachDi {table.MaBan}");
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Bàn",
                "Khôi phục bàn",
                "Ban",
                table.MaBan,
                $"Bàn {table.MaBan} ({table.TenBan}) được khôi phục về trạng thái Trống."
            );
            TempData["Success"] = $"Khôi phục thành công! {table.TenBan} đã chuyển về trạng thái Trống.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = FriendlyTableStatusError(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CustomerLeaves(string maBan)
    {
        try
        {
            var table = await GetPhysicalTableForStatusChangeAsync(maBan);
            if (table.TrangThai == TrangThaiOffline)
            {
                TempData["Error"] = "Bàn đã trống.";
                return RedirectToAction(nameof(Index));
            }

            if (table.TrangThai != TrangThaiOnline)
            {
                TempData["Error"] = "Chỉ có thể chuyển bàn đang phục vụ về Trống.";
                return RedirectToAction(nameof(Index));
            }

            var hasUnpaidOrder = await _context.HoaDons.AsNoTracking()
                .AnyAsync(x =>
                    x.MaBan == table.MaBan
                    && x.TrangThaiOrder != OrderWorkflowConstants.TrangThaiOrderDaHuy
                    && x.TrangThaiThanhToan != OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan);

            if (hasUnpaidOrder)
            {
                TempData["Error"] = "Bàn còn order chưa thanh toán, không thể trả bàn.";
                return RedirectToAction(nameof(Index));
            }

            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC Ban_KhachDi {table.MaBan}");
            await _auditLogService.LogAsync(
                "Bàn",
                "Trả bàn",
                "Ban",
                table.MaBan,
                $"Bàn {table.MaBan} được trả về trạng thái Trống."
            );
            TempData["Success"] = $"Đã hoàn tất {table.TenBan}. Bàn hiện Trống.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = FriendlyTableStatusError(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Ban> GetPhysicalTableForStatusChangeAsync(string? maBan)
    {
        maBan = (maBan ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(maBan))
            throw new InvalidOperationException("Mã bàn không hợp lệ.");

        if (string.Equals(maBan, MaBanMangVe, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("MVE là đơn mang về, không áp dụng trạng thái bàn.");

        var table = await _context.Bans.FirstOrDefaultAsync(x => x.MaBan == maBan);
        return table ?? throw new InvalidOperationException("Không tìm thấy bàn.");
    }

    private static string FriendlyTableStatusError(Exception ex)
    {
        var message = ex.Message;
        if (message.Contains("Mã bàn không hợp lệ", StringComparison.OrdinalIgnoreCase)
            || message.Contains("MVE là đơn mang về", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Không tìm thấy bàn", StringComparison.OrdinalIgnoreCase))
        {
            return message;
        }

        return "Không thể cập nhật trạng thái bàn. Vui lòng kiểm tra dữ liệu liên quan.";
    }

    private static Ban MapToModel(object item)
    {
        if (item is Ban e)
            return e;
        var model = new Ban();
        foreach (var p in typeof(Ban).GetProperties())
        {
            var s = item.GetType().GetProperty(p.Name);
            if (s != null && p.CanWrite)
                p.SetValue(model, s.GetValue(item));
        }
        return model;
    }
}
