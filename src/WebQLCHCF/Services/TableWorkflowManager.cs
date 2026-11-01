namespace WebQLCHCF.Services;

/// <summary>
/// Domain Engine quản lý trạng thái chuyển đổi bàn và vòng đời đơn hàng theo quy trình quán cafe.
/// </summary>
public static class TableWorkflowManager
{
    public const string TrangThaiTrong = OrderWorkflowConstants.TrangThaiBanTrong;
    public const string TrangThaiDangPhucVu = OrderWorkflowConstants.TrangThaiBanDangPhucVu;
    public const string MaBanMangVe = OrderWorkflowConstants.MaBanMangVe;

    /// <summary>
    /// Quyết định trạng thái mới của bàn khi khởi tạo order.
    /// Nếu dùng tại quán -> Bàn chuyển sang 'Đang phục vụ'.
    /// Nếu mang về -> Giữ nguyên mã mang về 'MVE'.
    /// </summary>
    public static string ResolveTableStatusOnOrder(string hinhThuc, string maBan, string currentStatus)
    {
        if (string.Equals(hinhThuc, OrderWorkflowConstants.HinhThucMangVe, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(maBan, MaBanMangVe, StringComparison.OrdinalIgnoreCase))
        {
            return currentStatus;
        }

        return TrangThaiDangPhucVu;
    }

    /// <summary>
    /// Kiểm tra xem bàn có sẵn sàng nhận khách hay không.
    /// </summary>
    public static bool IsTableAvailable(string? status)
    {
        return string.Equals(status?.Trim(), TrangThaiTrong, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Kiểm tra trạng thái hóa đơn có được phép sửa đổi danh sách món hay không.
    /// </summary>
    public static bool CanEditOrder(string? trangThaiOrder, string? trangThaiThanhToan, out string? reason)
    {
        if (string.Equals(trangThaiOrder, OrderWorkflowConstants.TrangThaiOrderDaHuy, StringComparison.OrdinalIgnoreCase))
        {
            reason = "Hóa đơn đã hủy, không thể sửa.";
            return false;
        }

        if (string.Equals(trangThaiThanhToan, OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan, StringComparison.OrdinalIgnoreCase))
        {
            reason = "Hóa đơn đã thanh toán, không thể sửa.";
            return false;
        }

        if (string.Equals(trangThaiThanhToan, OrderWorkflowConstants.TrangThaiThanhToanChoThanhToanQr, StringComparison.OrdinalIgnoreCase))
        {
            reason = "Hóa đơn đang chờ thanh toán QR, không thể sửa.";
            return false;
        }

        reason = null;
        return true;
    }

    /// <summary>
    /// Kiểm tra xem hóa đơn có đủ điều kiện để thực hiện thanh toán hay không.
    /// </summary>
    public static bool IsPayable(string? trangThaiOrder, string? trangThaiThanhToan)
    {
        if (string.Equals(trangThaiOrder, OrderWorkflowConstants.TrangThaiOrderDaHuy, StringComparison.OrdinalIgnoreCase))
            return false;

        return string.Equals(trangThaiThanhToan, OrderWorkflowConstants.TrangThaiThanhToanChuaThanhToan, StringComparison.OrdinalIgnoreCase)
            || string.Equals(trangThaiThanhToan, OrderWorkflowConstants.TrangThaiThanhToanChoThanhToanQr, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Kiểm tra hóa đơn đã thanh toán hay chưa.
    /// </summary>
    public static bool IsPaid(string? trangThaiThanhToan)
    {
        return string.Equals(trangThaiThanhToan?.Trim(), OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Kiểm tra xem hóa đơn có cho phép thêm chi tiết món hay không.
    /// Hóa đơn đã thanh toán tuyệt đối bất biến, từ chối thêm chi tiết món mới.
    /// </summary>
    public static bool CanAddLineItem(string? trangThaiThanhToan, out string? reason)
    {
        if (IsPaid(trangThaiThanhToan))
        {
            reason = "Hóa đơn đã thanh toán, không thể thêm chi tiết.";
            return false;
        }

        reason = null;
        return true;
    }

    /// <summary>
    /// Kiểm tra xem hóa đơn có cho phép chỉnh sửa chi tiết món hay không.
    /// Hóa đơn đã thanh toán tuyệt đối bất biến, từ chối chỉnh sửa chi tiết món.
    /// </summary>
    public static bool CanEditLineItem(string? trangThaiThanhToan, out string? reason)
    {
        if (IsPaid(trangThaiThanhToan))
        {
            reason = "Hóa đơn đã thanh toán, không thể chỉnh sửa chi tiết.";
            return false;
        }

        reason = null;
        return true;
    }

    /// <summary>
    /// Kiểm tra xem hóa đơn có cho phép xóa chi tiết món hay không.
    /// Hóa đơn đã thanh toán tuyệt đối bất biến, từ chối xóa chi tiết món.
    /// </summary>
    public static bool CanDeleteLineItem(string? trangThaiThanhToan, out string? reason)
    {
        if (IsPaid(trangThaiThanhToan))
        {
            reason = "Hóa đơn đã thanh toán, không thể xóa chi tiết.";
            return false;
        }

        reason = null;
        return true;
    }
}
