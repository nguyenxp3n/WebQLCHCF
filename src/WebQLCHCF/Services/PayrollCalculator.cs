namespace WebQLCHCF.Services;

/// <summary>
/// Domain Engine xử lý tính toán số phút, số giờ và tiền công chấm công cho nhân viên.
/// Khớp chính xác với logic Stored Procedure ChamCong_CheckOut trong SQL Server.
/// </summary>
public static class PayrollCalculator
{
    /// <summary>
    /// Tính tổng số phút làm việc giữa thời điểm Check-in và Check-out.
    /// </summary>
    public static int TinhTongPhut(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut < checkIn)
            return 0;

        TimeSpan duration = checkOut - checkIn;
        return (int)Math.Floor(duration.TotalMinutes);
    }

    /// <summary>
    /// Quy đổi tổng số phút ra số giờ làm việc, làm tròn 2 chữ số thập phân.
    /// Tương ứng: ROUND(CAST(@TongPhut AS decimal(10,2)) / 60.0, 2)
    /// </summary>
    public static decimal TinhTongGio(int tongPhut)
    {
        if (tongPhut <= 0)
            return 0m;

        return Math.Round((decimal)tongPhut / 60.0m, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Tính tiền công = Tổng giờ * Đơn giá theo giờ (đồng), làm tròn số nguyên gần nhất.
    /// Tương ứng: ROUND(@TongGio * @DonGiaGio, 0)
    /// </summary>
    public static decimal TinhTienCong(decimal tongGio, decimal donGiaGio)
    {
        if (tongGio <= 0m || donGiaGio <= 0m)
            return 0m;

        decimal tienCong = tongGio * donGiaGio;
        return Math.Round(tienCong, 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Tính tiền công trực tiếp từ số phút làm việc để khớp chính xác với SQL Server:
    /// CAST(ROUND((CAST(@TongPhut AS decimal(18,4)) / 60.0) * CAST(@DonGiaGio AS decimal(18,4)), 0) AS money)
    /// Loại bỏ hoàn toàn độ lệch làm tròn 67đ giữa SQL và C#.
    /// </summary>
    public static decimal TinhTienCongTuPhut(int tongPhut, decimal donGiaGio)
    {
        if (tongPhut <= 0 || donGiaGio <= 0m)
            return 0m;

        return Math.Round((decimal)tongPhut / 60.0m * donGiaGio, 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của mốc thời gian chấm công.
    /// </summary>
    public static bool KiemTraGioLamHopLe(DateTime checkIn, DateTime checkOut, out string? errorMessage)
    {
        if (checkOut < checkIn)
        {
            errorMessage = "Thời gian check-out không thể nhỏ hơn thời gian check-in.";
            return false;
        }

        if ((checkOut - checkIn).TotalHours > 24)
        {
            errorMessage = "Ca làm việc không được vượt quá 24 giờ liên tục.";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
