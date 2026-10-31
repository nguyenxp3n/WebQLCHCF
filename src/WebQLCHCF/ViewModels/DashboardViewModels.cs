namespace WebQLCHCF.ViewModels;

public class DashboardViewModel
{
    public int TongSanPham { get; set; }
    public int SanPhamCoTheBan { get; set; }
    public int SanPhamKhongTheBan { get; set; }
    public int TongBan { get; set; }
    public int BanDangHoatDong { get; set; }
    public int TongNhanVien { get; set; }
    public int NhanVienDangLam { get; set; }
    public decimal DoanhThuHomNay { get; set; }
    public decimal DoanhThuThangNay { get; set; }
    public List<DashboardSanPhamKhongTheBanItem> SanPhamKhongTheBanItems { get; set; } =
        new();
    public List<DashboardNguyenLieuSapHetItem> NguyenLieuSapHet { get; set; } = new();
    public List<DashboardSanPhamBanChayItem> TopSanPhamBanChay { get; set; } = new();
    public List<DashboardHoaDonGanDayItem> HoaDonGanDay { get; set; } = new();
    public List<DashboardPhieuNhapGanDayItem> PhieuNhapGanDay { get; set; } = new();
}

public class DashboardSanPhamKhongTheBanItem
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
}

public class DashboardNguyenLieuSapHetItem
{
    public string MaNL { get; set; } = string.Empty;
    public string TenNL { get; set; } = string.Empty;
    public int SoLuongTon { get; set; }
    public int MucTonToiThieu { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
}

public class DashboardSanPhamBanChayItem
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public int TongSoLuong { get; set; }
    public decimal TongTien { get; set; }
}

public class DashboardHoaDonGanDayItem
{
    public string MaHD { get; set; } = string.Empty;
    public DateTime NgayLap { get; set; }
    public string TenNV { get; set; } = string.Empty;
    public string? TenKH { get; set; }
    public string TenBan { get; set; } = string.Empty;
    public decimal TongTien { get; set; }
}

public class DashboardPhieuNhapGanDayItem
{
    public string MaPN { get; set; } = string.Empty;
    public DateTime NgayNhap { get; set; }
    public string TenNV { get; set; } = string.Empty;
    public decimal TongTien { get; set; }
}
