using System.Net;

namespace WebQLCHCF.Services;

/// <summary>
/// Domain Engine xử lý tính toán giá tiền, chiết khấu khuyến mãi, điểm tích lũy và VietQR cho đơn hàng cafe.
/// </summary>
public static class CafeOrderCalculator
{
    public const string LoaiKMTienMat = "Tiền mặt";
    public const string LoaiKMPhanTram = "Phần trăm";
    public const string TrangThaiKMDangApDung = "Đang áp dụng";
    public const decimal DiemTichLuyHeSo = 10000m; // 10.000 VNĐ = 1 điểm tích lũy

    /// <summary>
    /// Tính thành tiền cho 1 dòng món trong hóa đơn (CTHD) dựa trên khuyến mãi.
    /// Khớp chính xác với logic Stored Procedure HoaDon_CapNhatTongTien trong SQL Server.
    /// </summary>
    public static decimal TinhThanhTienMon(
        decimal donGia,
        int soLuong,
        string? loaiKM = null,
        decimal? giaTriKM = null,
        bool kmDangHieuLuc = true)
    {
        if (donGia < 0)
            throw new ArgumentOutOfRangeException(nameof(donGia), "Đơn giá không được âm.");
        if (soLuong < 0)
            throw new ArgumentOutOfRangeException(nameof(soLuong), "Số lượng không được âm.");

        decimal tongTienGoc = donGia * soLuong;
        if (tongTienGoc == 0)
            return 0m;

        if (string.IsNullOrWhiteSpace(loaiKM) || !giaTriKM.HasValue || !kmDangHieuLuc)
            return tongTienGoc;

        if (string.Equals(loaiKM.Trim(), LoaiKMTienMat, StringComparison.OrdinalIgnoreCase))
        {
            decimal giamGia = Math.Max(0m, giaTriKM.Value);
            return Math.Max(0m, tongTienGoc - giamGia);
        }

        if (string.Equals(loaiKM.Trim(), LoaiKMPhanTram, StringComparison.OrdinalIgnoreCase))
        {
            decimal phanTram = giaTriKM.Value;
            if (phanTram <= 0m)
                return tongTienGoc;
            if (phanTram >= 100m)
                return 0m;

            decimal thanhTien = tongTienGoc * (1m - phanTram / 100m);
            return Math.Round(thanhTien, 0, MidpointRounding.AwayFromZero);
        }

        return tongTienGoc;
    }

    /// <summary>
    /// Tính tổng tiền đơn hàng từ danh sách thành tiền từng món.
    /// </summary>
    public static decimal TinhTongTienDonHang(IEnumerable<decimal>? thanhTienCacMon)
    {
        if (thanhTienCacMon == null)
            return 0m;

        return thanhTienCacMon.Sum();
    }

    /// <summary>
    /// Tính điểm tích lũy cộng thêm dựa trên tổng tiền hóa đơn (10.000đ = 1 điểm).
    /// </summary>
    public static int TinhDiemTichLuy(decimal tongTien)
    {
        if (tongTien <= 0m)
            return 0;

        return (int)Math.Floor(tongTien / DiemTichLuyHeSo);
    }

    /// <summary>
    /// Kiểm tra giá trị đơn giá hợp lệ theo ràng buộc nghiệp vụ (1đ đến 10.000.000đ).
    /// </summary>
    public static bool KiemTraHopLeDonGia(decimal donGia)
    {
        return donGia >= 1m && donGia <= 10_000_000m;
    }

    /// <summary>
    /// Kiểm tra số lượng món hợp lệ (ít nhất 1 món, không vượt quá 9.999).
    /// </summary>
    public static bool KiemTraHopLeSoLuong(int soLuong)
    {
        return soLuong >= 1 && soLuong <= 9999;
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của giá trị khuyến mãi theo loại.
    /// </summary>
    public static bool KiemTraHopLeGiaTriKhuyenMai(string? loaiKM, decimal? giaTriKM)
    {
        if (string.IsNullOrWhiteSpace(loaiKM) || !giaTriKM.HasValue)
            return false;

        if (giaTriKM.Value < 0)
            return false;

        if (string.Equals(loaiKM.Trim(), LoaiKMPhanTram, StringComparison.OrdinalIgnoreCase))
            return giaTriKM.Value <= 100m;

        return true;
    }

    /// <summary>
    /// Sinh URL tạo mã QR chuẩn VietQR phục vụ thanh toán chuyển khoản không tiếp xúc.
    /// </summary>
    public static string SinhUrlVietQr(
        decimal soTien,
        string noiDungChuyenKhoan,
        string bankCode = "MB",
        string accountNo = "0332135258",
        string accountName = "HOANG CHIEM NGUYEN")
    {
        string encodedContent = WebUtility.UrlEncode(noiDungChuyenKhoan ?? string.Empty);
        string encodedAccount = WebUtility.UrlEncode(accountName ?? string.Empty);
        long soTienNguyen = (long)Math.Max(0m, Math.Round(soTien, 0));

        return $"https://img.vietqr.io/image/{bankCode}-{accountNo}-compact2.png?amount={soTienNguyen}&addInfo={encodedContent}&accountName={encodedAccount}";
    }
}
