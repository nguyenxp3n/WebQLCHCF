using System.ComponentModel.DataAnnotations;

namespace WebQLCHCF.ViewModels;

public class OrderCreateViewModel
{
    public string? EditMaHD { get; set; }
    public bool IsEditing => !string.IsNullOrWhiteSpace(EditMaHD);
    [StringLength(36)]
    public string? ClientRequestId { get; set; }
    public string HinhThuc { get; set; } = "TaiQuan";
    public string KhuVuc { get; set; } = "Tầng trệt";
    public string? MaKH { get; set; }
    public string? TenKH { get; set; }
    public string? SDTKhachHang { get; set; }
    public string? GhiChu { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();
    public List<OrderCustomerViewModel> DanhSachKhachHang { get; set; } = new();
    public List<OrderAvailableProductViewModel> DanhSachSanPhamDangBan { get; set; } = new();
    public List<OrderPromotionViewModel> DanhSachKhuyenMai { get; set; } = new();
    public List<OrderTableViewModel> BanTrongDauTienTheoKhuVuc { get; set; } = new();
    public OrderTableViewModel? BanTrongTiepTheo { get; set; }
    public string? ThongBaoBan { get; set; }
}

public class OrderItemViewModel
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public decimal DonGia { get; set; }
    public int SoLuong { get; set; }
    public string? MaKM { get; set; }
    public string? TenKM { get; set; }
    public decimal ThanhTien { get; set; }
}

public class OrderReceiptViewModel
{
    public string MaHD { get; set; } = string.Empty;
    public DateTime NgayLap { get; set; }
    public DateTime GioIn { get; set; } = DateTime.Now;
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string? MaKH { get; set; }
    public string? TenKH { get; set; }
    public int? DiemTichLuy { get; set; }
    public string MaBan { get; set; } = string.Empty;
    public string TenBan { get; set; } = string.Empty;
    public string HinhThuc { get; set; } = string.Empty;
    public decimal TongTien { get; set; }
    public string? GhiChu { get; set; }
    public int? DiemDaCong { get; set; }
    public string TrangThaiOrder { get; set; } = string.Empty;
    public string TrangThaiThanhToan { get; set; } = string.Empty;
    public string? PhuongThucThanhToan { get; set; }
    public DateTime ThoiGianTao { get; set; }
    public DateTime? ThoiGianThanhToan { get; set; }
    public string? TrangThaiBan { get; set; }
    public bool CanPay { get; set; }
    public bool CanCreateQr { get; set; }
    public bool CanCompleteTable { get; set; }
    public bool CanEditOrder { get; set; }
    public bool CanCancelOrder { get; set; }
    public PaymentInfoViewModel? PendingQr { get; set; }
    public List<PaymentInfoViewModel> Payments { get; set; } = new();
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class PaymentInfoViewModel
{
    public int MaThanhToan { get; set; }
    public string? MaHD { get; set; }
    public string? MaPhienThanhToan { get; set; }
    public string PhuongThuc { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string? NoiDungChuyenKhoan { get; set; }
    public string? MaGiaoDich { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime ThoiGianTao { get; set; }
    public DateTime? ThoiGianHetHan { get; set; }
    public DateTime? ThoiGianThanhToan { get; set; }
    public string? GhiChu { get; set; }
    public string? VietQrUrl { get; set; }
}

public class PaymentCommandResultViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MaHD { get; set; }
    public string? MaBan { get; set; }
    public string? MaPhienThanhToan { get; set; }
    public decimal? SoTien { get; set; }
    public string? NoiDungChuyenKhoan { get; set; }
    public DateTime? ThoiGianHetHan { get; set; }
    public string? VietQrUrl { get; set; }
    public int? SoHoaDonXuLy { get; set; }
}

public class OrderAvailableProductViewModel
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public decimal DonGia { get; set; }
    public string MaLoaiSP { get; set; } = string.Empty;
    public string TenLoaiSP { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
}

public class OrderCustomerViewModel
{
    public string MaKH { get; set; } = string.Empty;
    public string TenKH { get; set; } = string.Empty;
    public string SDT { get; set; } = string.Empty;
    public int? DiemTichLuy { get; set; }
    public string? GiamGiaCoTheApDung { get; set; }
}

public class OrderPromotionViewModel
{
    public string MaKM { get; set; } = string.Empty;
    public string TenKM { get; set; } = string.Empty;
    public string LoaiKM { get; set; } = string.Empty;
    public decimal GiaTri { get; set; }
    public string TrangThai { get; set; } = string.Empty;
}

public class OrderChamCongStatusViewModel
{
    public int MaCC { get; set; }
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string ChucVu { get; set; } = string.Empty;
    public DateTime ThoiGianCheckIn { get; set; }
    public DateTime? ThoiGianCheckOut { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public decimal DonGiaGio { get; set; }
    public int TongPhutTamTinh { get; set; }
    public decimal TongGioTamTinh { get; set; }
    public decimal TienCongTamTinh { get; set; }
}

public class OrderQuickCustomerCreateViewModel
{
    public string TenKH { get; set; } = string.Empty;
    public string SDT { get; set; } = string.Empty;
}

public class OrderTableViewModel
{
    public string MaBan { get; set; } = string.Empty;
    public string TenBan { get; set; } = string.Empty;
    public string KhuVuc { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
}
