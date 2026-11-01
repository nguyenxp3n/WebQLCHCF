using WebQLCHCF.Models;
using WebQLCHCF.Services;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Tests.TestHarness;

public class TestAuditLogServiceDouble : IAuditLogService
{
    public List<AuditLog> Logs { get; } = new();

    public Task LogAsync(
        string chucNang,
        string hanhDong,
        string? doiTuong,
        string? maDoiTuong,
        string noiDung,
        string? duLieuTruoc = null,
        string? duLieuSau = null)
    {
        Logs.Add(new AuditLog
        {
            MaLog = Logs.Count + 1,
            ThoiGian = DateTime.Now,
            ChucNang = chucNang,
            HanhDong = hanhDong,
            DoiTuong = doiTuong,
            MaDoiTuong = maDoiTuong,
            NoiDung = noiDung,
            DuLieuTruoc = duLieuTruoc,
            DuLieuSau = duLieuSau
        });

        return Task.CompletedTask;
    }
}

public class TestChamCongServiceDouble : IChamCongService
{
    public HashSet<string> ActiveCheckIns { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, DateTime> CheckInTimes { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Task AutoCheckInOnLoginAsync(string maNV)
    {
        ActiveCheckIns.Add(maNV);
        if (!CheckInTimes.ContainsKey(maNV))
            CheckInTimes[maNV] = DateTime.Now.AddHours(-4);
        return Task.CompletedTask;
    }

    public Task<ChamCongDangLamViewModel?> GetDangLamByNhanVienAsync(string maNV)
    {
        if (!ActiveCheckIns.Contains(maNV))
            return Task.FromResult<ChamCongDangLamViewModel?>(null);

        var checkIn = CheckInTimes.TryGetValue(maNV, out var time) ? time : DateTime.Now.AddHours(-4);
        var result = new ChamCongDangLamViewModel
        {
            MaCC = 101,
            MaNV = maNV,
            TenNV = "Nhân viên " + maNV,
            ChucVu = "Phục vụ",
            ThoiGianCheckIn = checkIn,
            DonGiaGio = 25000,
            TongPhutTamTinh = 240,
            TongGioTamTinh = 4.00m,
            TienCongTamTinh = 100000
        };

        return Task.FromResult<ChamCongDangLamViewModel?>(result);
    }

    public Task<ChamCongCheckoutResultViewModel?> CheckOutAndGetResultAsync(string maNV, string matKhau)
    {
        if (matKhau == "wrongpassword")
            return Task.FromResult<ChamCongCheckoutResultViewModel?>(null);

        ActiveCheckIns.Remove(maNV);
        CheckInTimes.Remove(maNV);

        var result = new ChamCongCheckoutResultViewModel
        {
            MaCC = 101,
            MaNV = maNV,
            TenNV = "Nhân viên " + maNV,
            ThoiGianCheckIn = DateTime.Now.AddHours(-4),
            ThoiGianCheckOut = DateTime.Now,
            DonGiaGio = 25000,
            TongPhut = 240,
            TongGio = 4.00m,
            TienCong = 100000
        };

        return Task.FromResult<ChamCongCheckoutResultViewModel?>(result);
    }
}

public class TestDashboardServiceDouble : IDashboardService
{
    public DashboardViewModel Dashboard { get; set; } = new()
    {
        TongSanPham = 25,
        SanPhamCoTheBan = 20,
        SanPhamKhongTheBan = 5,
        TongBan = 10,
        BanDangHoatDong = 3,
        TongNhanVien = 8,
        NhanVienDangLam = 5,
        DoanhThuHomNay = 3500000,
        DoanhThuThangNay = 75000000,
        NguyenLieuSapHet = new List<DashboardNguyenLieuSapHetItem>
        {
            new() { MaNL = "NL01", TenNL = "Sữa đặc", DonViTinh = "Hộp", SoLuongTon = 2, MucTonToiThieu = 5 },
            new() { MaNL = "NL02", TenNL = "Hạt cà phê Robusta", DonViTinh = "Kg", SoLuongTon = 3, MucTonToiThieu = 10 }
        },
        TopSanPhamBanChay = new List<DashboardSanPhamBanChayItem>
        {
            new() { MaSP = "SP01", TenSP = "Cà phê sữa", TongSoLuong = 85, TongTien = 2550000 },
            new() { MaSP = "SP02", TenSP = "Trà đào cam sả", TongSoLuong = 60, TongTien = 2100000 }
        }
    };

    public Task<DashboardViewModel> GetDashboardAsync() => Task.FromResult(Dashboard);
}
