using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebQLCHCF.Models;

namespace WebQLCHCF.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu"), DataType(DataType.Password)]
    public string MatKhau { get; set; } = string.Empty;
}

public class LoginResultViewModel
{
    public string MaTK { get; set; } = string.Empty;
    public string TenDangNhap { get; set; } = string.Empty;
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
}

public class SanPhamViewModel
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;

    [Column(TypeName = "money")]
    public decimal DonGia { get; set; }
    public string MaLoaiSP { get; set; } = string.Empty;
    public string TenLoaiSP { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
}

public class SanPhamIndexViewModel
{
    public string? TuKhoa { get; set; }
    public string? MaLoaiSP { get; set; }
    public string? TrangThai { get; set; }
    public List<SanPhamViewModel> Items { get; set; } = new();
    public List<LoaiSP> LoaiSanPhams { get; set; } = new();
    public List<string> TrangThaiOptions { get; set; } = new();
    public int TongSo { get; set; }
    public int SoKetQua => Items.Count;
    public bool HasFilter =>
        !string.IsNullOrWhiteSpace(TuKhoa)
        || !string.IsNullOrWhiteSpace(MaLoaiSP)
        || !string.IsNullOrWhiteSpace(TrangThai);
}

public class NhanVienViewModel
{
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string GioiTinh { get; set; } = string.Empty;
    public DateTime? NgaySinh { get; set; }
    public string SDT { get; set; } = string.Empty;
    public string ChucVu { get; set; } = string.Empty;
    public int MaLuong { get; set; }

    [Column(TypeName = "money")]
    public decimal LuongTheoGio { get; set; }

    public int Thang { get; set; }
    public int Nam { get; set; }

    [Column(TypeName = "money")]
    public decimal LuongCoBan { get; set; }

    [Column(TypeName = "money")]
    public decimal? PhuCap { get; set; }

    [Column(TypeName = "money")]
    public decimal? Thuong { get; set; }

    [Column(TypeName = "money")]
    public decimal? Phat { get; set; }

    [Column(TypeName = "money")]
    public decimal TongLuong { get; set; }
}

public class TaiKhoanListViewModel
{
    public string MaTK { get; set; } = string.Empty;
    public string TenDangNhap { get; set; } = string.Empty;
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
}

public class TaiKhoanDetailViewModel
{
    public string MaTK { get; set; } = string.Empty;
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string VaiTro { get; set; } = string.Empty;
}

public class HoaDonViewModel
{
    public string MaHD { get; set; } = string.Empty;
    public DateTime NgayLap { get; set; }
    [NotMapped]
    public DateTime ThoiGianTao { get; set; }
    [NotMapped]
    public DateTime? ThoiGianThanhToan { get; set; }
    [NotMapped]
    public string TrangThaiOrder { get; set; } = string.Empty;
    [NotMapped]
    public string TrangThaiThanhToan { get; set; } = string.Empty;
    [NotMapped]
    public string? PhuongThucThanhToan { get; set; }
    [NotMapped]
    public int? MaThanhToanGanNhat { get; set; }
    [NotMapped]
    public string? MaPhienThanhToan { get; set; }
    [NotMapped]
    public string? TrangThaiThanhToanGanNhat { get; set; }
    [NotMapped]
    public string? PhuongThucThanhToanGanNhat { get; set; }
    [NotMapped]
    public DateTime? ThoiGianHetHanQr { get; set; }
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string? MaKH { get; set; }
    public string? TenKH { get; set; }
    public string MaBan { get; set; } = string.Empty;
    public string TenBan { get; set; } = string.Empty;

    [Column(TypeName = "money")]
    public decimal TongTien { get; set; }
    public string? GhiChu { get; set; }
}

public class CTHDViewModel
{
    public string MaHD { get; set; } = string.Empty;
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public int SoLuong { get; set; }

    [Column(TypeName = "money")]
    public decimal DonGia { get; set; }

    [Column(TypeName = "money")]
    public decimal ThanhTien { get; set; }
    public string? MaKM { get; set; }
    public string? TenKM { get; set; }
}

public class PhieuNhapViewModel
{
    public string MaPN { get; set; } = string.Empty;
    public DateTime NgayNhap { get; set; }
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;

    [Column(TypeName = "money")]
    public decimal TongTien { get; set; }
}

public class CTPNViewModel
{
    public string MaPN { get; set; } = string.Empty;
    public string MaNL { get; set; } = string.Empty;
    public string TenNL { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public int SoLuong { get; set; }

    [Column(TypeName = "money")]
    public decimal DonGia { get; set; }

    [Column(TypeName = "money")]
    public decimal ThanhTien { get; set; }
}

public class CongThucViewModel
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string MaNL { get; set; } = string.Empty;
    public string TenNL { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,3)")]
    public decimal DinhLuong { get; set; }
}

public class DoanhThuTheoNgayViewModel
{
    public DateTime NgayLap { get; set; }

    [Column(TypeName = "money")]
    public decimal DoanhThu { get; set; }
}

public class SanPhamBanChayViewModel
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public int TongSoLuong { get; set; }

    [Column(TypeName = "money")]
    public decimal TongTien { get; set; }
}

public class NhapHangTheoThangViewModel
{
    public int Thang { get; set; }
    public int Nam { get; set; }

    [Column(TypeName = "money")]
    public decimal TongTienNhap { get; set; }
}

public class AuditLogViewModel
{
    public long MaLog { get; set; }
    public DateTime ThoiGian { get; set; }
    public string? MaNV { get; set; }
    public string? TenDangNhap { get; set; }
    public string? VaiTro { get; set; }
    public string ChucNang { get; set; } = string.Empty;
    public string HanhDong { get; set; } = string.Empty;
    public string? DoiTuong { get; set; }
    public string? MaDoiTuong { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public string? DuLieuTruoc { get; set; }
    public string? DuLieuSau { get; set; }
    public string? DiaChiIP { get; set; }
    public string? UserAgent { get; set; }
}

public class AuditLogFilterViewModel
{
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public string? MaNV { get; set; }
    public string? ChucNang { get; set; }
    public string? HanhDong { get; set; }
    public string? DoiTuong { get; set; }
    public string? MaDoiTuong { get; set; }
    public int Top { get; set; } = 200;
    public List<AuditLogViewModel> Logs { get; set; } = new();
}
