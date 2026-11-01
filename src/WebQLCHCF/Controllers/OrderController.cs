using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebQLCHCF.Services;
using WebQLCHCF.ViewModels;
using WebQLCHCF.Security;

namespace WebQLCHCF.Controllers;

[RoleAuthorize(AppRoles.QuanLy, AppRoles.PhucVu)]
public class OrderController : BaseController
{
    private readonly IOrderService _orderService;
    private readonly IAuditLogService _auditLogService;

    public OrderController(IOrderService orderService, IAuditLogService auditLogService)
    {
        _orderService = orderService;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Create));
    }

    [HttpGet]
    public async Task<IActionResult> Create(string? editMaHD)
    {
        var maNV = HttpContext.Session.GetString("MaNV");
        if (string.IsNullOrWhiteSpace(maNV))
            return RedirectToAction("Login", "Account");

        try
        {
            ViewBag.CanOrder = await _orderService.IsEmployeeCheckedInAsync(maNV);
            if (ViewBag.CanOrder == false)
                TempData["Error"] = "Bạn cần check-in trước khi order.";

            var model = string.IsNullOrWhiteSpace(editMaHD)
                ? await _orderService.GetCreateModelAsync()
                : await _orderService.GetCreateModelForEditAsync(editMaHD);

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải màn hình order: " + FriendlyControllerError(ex);
            return View(new OrderCreateViewModel());
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCreateViewModel model)
    {
        var maNV = HttpContext.Session.GetString("MaNV");
        if (string.IsNullOrWhiteSpace(maNV))
            return RedirectToAction("Login", "Account");

        try
        {
            var isEditing = !string.IsNullOrWhiteSpace(model.EditMaHD);
            var maHD = await _orderService.CreateOrderAsync(model, maNV);
            var receipt = await _orderService.GetReceiptAsync(maHD);
            var payload = JsonSerializer.Serialize(
                new
                {
                    model.HinhThuc,
                    model.MaKH,
                    model.GhiChu,
                    Items = model.Items
                        .Where(x => x.SoLuong > 0)
                        .Select(x => new { x.MaSP, x.SoLuong, x.MaKM }),
                }
            );
            await _auditLogService.LogAsync(
                "Order",
                isEditing ? "Sửa order" : "Tạo order",
                "HoaDon",
                maHD,
                isEditing
                    ? $"Sửa hóa đơn {maHD}, hủy phiên QR đang chờ nếu có, cập nhật danh sách món."
                    : $"Tạo hóa đơn {maHD} cho bàn {receipt?.MaBan ?? "không xác định"}, tổng tiền {FormatMoney(receipt?.TongTien ?? 0)}.",
                duLieuSau: payload
            );
            TempData["Success"] = string.IsNullOrWhiteSpace(model.EditMaHD)
                ? "Tạo hóa đơn thành công."
                : "Cập nhật order thành công. Bạn có thể chọn lại tiền mặt hoặc QR để thanh toán.";
            return RedirectToAction(nameof(Receipt), new { id = maHD });
        }
        catch (Exception ex)
        {
            TempData["Error"] = FriendlyControllerError(ex);
            ViewBag.CanOrder = await _orderService.IsEmployeeCheckedInAsync(maNV);
            return View(await _orderService.GetCreateModelAsync(model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Receipt(string id)
    {
        try
        {
            var model = await _orderService.GetReceiptAsync(id);
            if (model == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction(nameof(Create));
            }

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tải hóa đơn: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Create));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteTable(string id)
    {
        try
        {
            var receipt = await _orderService.GetReceiptAsync(id);
            await _orderService.CompleteTableAsync(id);
            if (!string.IsNullOrWhiteSpace(receipt?.MaBan))
            {
                await _auditLogService.LogAsync(
                    "Bàn",
                    "Trả bàn",
                    "Ban",
                    receipt.MaBan,
                    $"Bàn {receipt.MaBan} được trả về trạng thái Trống."
                );
            }
            TempData["Success"] = "Đã chuyển bàn về Trống.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể trả bàn: " + FriendlyControllerError(ex);
        }

        return RedirectToAction(nameof(Receipt), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PayCash(string id, string? ghiChuThanhToan)
    {
        try
        {
            var result = await _orderService.PayCashAsync(id, ghiChuThanhToan);
            if (result.Success)
            {
                var receipt = await _orderService.GetReceiptAsync(id);
                await _auditLogService.LogAsync(
                    "Thanh toán",
                    "Thanh toán tiền mặt",
                    "HoaDon",
                    id,
                    $"Hóa đơn {id} đã thanh toán tiền mặt, số tiền {FormatMoney(result.SoTien ?? receipt?.TongTien ?? 0)}."
                );
            }
            TempData[result.Success ? "Success" : "Error"] = result.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể thanh toán tiền mặt: " + FriendlyControllerError(ex);
        }

        return RedirectToAction(nameof(Receipt), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQrPayment(string id)
    {
        try
        {
            var result = await _orderService.CreateQrPaymentAsync(id);
            if (result.Success)
            {
                await _auditLogService.LogAsync(
                    "Thanh toán QR",
                    "Tạo QR",
                    "HoaDon",
                    id,
                    $"Tạo QR cho hóa đơn {id}, mã phiên {result.MaPhienThanhToan ?? "không xác định"}, số tiền {FormatMoney(result.SoTien ?? 0)}, hạn 15 phút."
                );
            }
            TempData[result.Success ? "Success" : "Error"] = result.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể tạo QR thanh toán: " + FriendlyControllerError(ex);
        }

        return RedirectToAction(nameof(Receipt), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmQrDemo(string id, string maPhienThanhToan)
    {
        try
        {
            var result = await _orderService.ConfirmQrDemoAsync(id, maPhienThanhToan);
            if (result.Success)
            {
                await _auditLogService.LogAsync(
                    "Thanh toán QR",
                    "Xác nhận QR",
                    "HoaDon",
                    id,
                    $"Xác nhận QR thành công cho hóa đơn {id}, mã phiên {maPhienThanhToan}."
                );
            }
            TempData[result.Success ? "Success" : "Error"] = result.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xác nhận QR demo: " + FriendlyControllerError(ex);
        }

        return RedirectToAction(nameof(Receipt), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ExpireQr(string id)
    {
        try
        {
            var result = await _orderService.ExpireQrAsync(id);
            if (result.Success)
            {
                await _auditLogService.LogAsync(
                    "Thanh toán QR",
                    "QR hết hạn",
                    "HoaDon",
                    id,
                    $"QR của hóa đơn {id} đã hết hạn, mã phiên {result.MaPhienThanhToan ?? "không xác định"}."
                );
            }
            TempData[result.Success ? "Success" : "Error"] = result.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể xử lý QR hết hạn: " + FriendlyControllerError(ex);
        }

        return RedirectToAction(nameof(Receipt), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(string id, string lyDo)
    {
        try
        {
            var result = await _orderService.CancelOrderAsync(id, lyDo);
            if (result.Success)
            {
                await _auditLogService.LogAsync(
                    "Order",
                    "Hủy order",
                    "HoaDon",
                    id,
                    $"Hủy hóa đơn {id}. Lý do: {lyDo.Trim()}."
                );
            }
            TempData[result.Success ? "Success" : "Error"] = result.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể hủy order: " + FriendlyControllerError(ex);
        }

        return RedirectToAction(nameof(Receipt), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RoleAuthorize(AppRoles.QuanLy)]
    public async Task<IActionResult> RefundOrder(string id, string lyDo)
    {
        try
        {
            var result = await _orderService.RefundOrderAsync(id, lyDo);
            if (result.Success)
            {
                await _auditLogService.LogAsync(
                    "Order",
                    "Hoàn đơn / Hủy thanh toán",
                    "HoaDon",
                    id,
                    $"Quản lý hoàn hóa đơn {id}. Lý do: {lyDo.Trim()}."
                );
            }
            TempData[result.Success ? "Success" : "Error"] = result.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể hoàn đơn: " + FriendlyControllerError(ex);
        }

        return RedirectToAction(nameof(Receipt), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOrder(string id)
    {
        try
        {
            var result = await _orderService.EditOrderAsync(id);
            if (result.Success)
            {
                await _auditLogService.LogAsync(
                    "Order",
                    "Sửa order",
                    "HoaDon",
                    id,
                    $"Sửa hóa đơn {id}, hủy phiên QR đang chờ nếu có, đưa order về màn chỉnh món."
                );
            }
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return result.Success
                ? RedirectToAction(nameof(Create), new { editMaHD = id })
                : RedirectToAction(nameof(Receipt), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Không thể chuyển order về trạng thái sửa: " + FriendlyControllerError(ex);
            return RedirectToAction(nameof(Receipt), new { id });
        }
    }

    [HttpGet]
    public async Task<IActionResult> SearchCustomers(string keyword)
    {
        try
        {
            return Json(await _orderService.SearchCustomersAsync(keyword ?? string.Empty));
        }
        catch
        {
            return Json(Array.Empty<OrderCustomerViewModel>());
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickCreateCustomer(OrderQuickCustomerCreateViewModel model)
    {
        try
        {
            var customer = await _orderService.CreateQuickCustomerAsync(model);
            return Json(new { success = true, customer });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = FriendlyOrderApiError(ex) });
        }
    }

    private static string FriendlyOrderApiError(Exception ex)
    {
        var message = ex.Message;
        if (message.Contains("Vui lòng nhập tên khách hàng", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Số điện thoại không hợp lệ", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Số điện thoại đã tồn tại", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Không thể tạo khách hàng nhanh", StringComparison.OrdinalIgnoreCase))
        {
            return message;
        }

        return "Không thể thực hiện thao tác. Vui lòng kiểm tra dữ liệu liên quan.";
    }

    private static string FormatMoney(decimal value) => $"{value:N0}đ";
}
