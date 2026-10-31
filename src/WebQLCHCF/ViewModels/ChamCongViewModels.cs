using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.ViewModels;

public class ChamCongDangLamViewModel
{
    public int MaCC { get; set; }
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string ChucVu { get; set; } = string.Empty;
    public DateTime ThoiGianCheckIn { get; set; }
    public DateTime? ThoiGianCheckOut { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? GhiChu { get; set; }

    [Column(TypeName = "money")]
    public decimal DonGiaGio { get; set; }

    public int TongPhutTamTinh { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TongGioTamTinh { get; set; }

    [Column(TypeName = "money")]
    public decimal TienCongTamTinh { get; set; }

    public string ThoiGianDaLamText
    {
        get
        {
            var totalMinutes = TongPhutTamTinh;
            return $"{totalMinutes / 60} giờ {totalMinutes % 60} phút";
        }
    }
}

public class ChamCongCheckoutResultViewModel
{
    public int MaCC { get; set; }
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string ChucVu { get; set; } = string.Empty;
    public DateTime ThoiGianCheckIn { get; set; }
    public DateTime? ThoiGianCheckOut { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? GhiChu { get; set; }

    [Column(TypeName = "money")]
    public decimal DonGiaGio { get; set; }

    public int? TongPhut { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? TongGio { get; set; }

    [Column(TypeName = "money")]
    public decimal? TienCong { get; set; }

    public string ThoiGianDaLamText
    {
        get
        {
            var totalMinutes = TongPhut ?? 0;
            return $"{totalMinutes / 60} giờ {totalMinutes % 60} phút";
        }
    }
}

public class LogoutConfirmViewModel
{
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public DateTime? ThoiGianCheckIn { get; set; }
    public string? ThoiGianDaLamText { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string MatKhau { get; set; } = string.Empty;
}

public class LuongThangViewModel
{
    public int Thang { get; set; }
    public ChamCongTongLuongTatCaThangViewModel? TongQuan { get; set; }
    public List<ChamCongTongLuongNhanVienThangViewModel> LuongNhanVien { get; set; } = new();
    public List<ChamCongCaLamThangViewModel> CaLam { get; set; } = new();
}

public class ChamCongTongLuongTatCaThangViewModel
{
    public int Thang { get; set; }
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public int TongSoCaLam { get; set; }
    public int TongNhanVienCoLam { get; set; }
    public int TongPhut { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TongGio { get; set; }

    [Column(TypeName = "money")]
    public decimal TongLuongPhaiChuyen { get; set; }
}

public class ChamCongTongLuongNhanVienThangViewModel
{
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string ChucVu { get; set; } = string.Empty;

    [Column(TypeName = "money")]
    public decimal LuongTheoGio { get; set; }

    public int SoCaLam { get; set; }
    public int TongPhut { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TongGio { get; set; }

    [Column(TypeName = "money")]
    public decimal TongLuong { get; set; }

    [Column(TypeName = "money")]
    public decimal TongTienCongTheoCaThamKhao { get; set; }

    public int Thang { get; set; }
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
}

public class ChamCongCaLamThangViewModel
{
    public int MaCC { get; set; }
    public string MaNV { get; set; } = string.Empty;
    public string TenNV { get; set; } = string.Empty;
    public string ChucVu { get; set; } = string.Empty;
    public DateTime ThoiGianCheckIn { get; set; }
    public DateTime? ThoiGianCheckOut { get; set; }

    [Column(TypeName = "money")]
    public decimal DonGiaGio { get; set; }

    public int TongPhut { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TongGio { get; set; }

    [Column(TypeName = "money")]
    public decimal TienCongTheoCa { get; set; }

    public string TrangThai { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public int Thang { get; set; }
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
}
