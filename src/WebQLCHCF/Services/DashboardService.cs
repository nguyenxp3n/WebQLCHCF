using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Services;

public class DashboardService : IDashboardService
{
    private const string TrangThaiDangBan = "Đang bán";
    private const string MaBanMangVe = "MVE";
    private const string TrangThaiBanOnline = "Đang phục vụ";
    private const string TrangThaiThanhToanDaThanhToan = "DaThanhToan";

    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context) => _context = context;

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

        var productStats = await _context
            .SanPhams.AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TongSanPham = g.Count(),
                SanPhamCoTheBan = g.Sum(x => x.TrangThai == TrangThaiDangBan ? 1 : 0),
                SanPhamKhongTheBan = g.Sum(x => x.TrangThai != TrangThaiDangBan ? 1 : 0),
            })
            .SingleOrDefaultAsync();

        var banStats = await _context
            .Bans.AsNoTracking()
            .Where(x => x.MaBan != MaBanMangVe)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TongBan = g.Count(),
                BanDangHoatDong = g.Sum(x => x.TrangThai == TrangThaiBanOnline ? 1 : 0),
            })
            .SingleOrDefaultAsync();

        var nhanVienDangLam = await _context
            .ChamCongs.AsNoTracking()
            .Where(x => x.TrangThai == "Đang làm" && x.ThoiGianCheckOut == null)
            .Select(x => x.MaNV)
            .Distinct()
            .CountAsync();

        return new DashboardViewModel
        {
            TongSanPham = productStats?.TongSanPham ?? 0,
            SanPhamCoTheBan = productStats?.SanPhamCoTheBan ?? 0,
            SanPhamKhongTheBan = productStats?.SanPhamKhongTheBan ?? 0,
            SanPhamKhongTheBanItems = await GetSanPhamKhongTheBanAsync(),
            TongBan = banStats?.TongBan ?? 0,
            BanDangHoatDong = banStats?.BanDangHoatDong ?? 0,
            TongNhanVien = await _context.NhanViens.AsNoTracking().CountAsync(),
            NhanVienDangLam = nhanVienDangLam,
            DoanhThuHomNay =
                await _context
                    .HoaDons.AsNoTracking()
                    .Where(x =>
                        x.NgayLap >= today
                        && x.NgayLap < tomorrow
                        && x.TrangThaiThanhToan == TrangThaiThanhToanDaThanhToan
                    )
                    .Select(x => (decimal?)x.TongTien)
                    .SumAsync() ?? 0,
            DoanhThuThangNay =
                await _context
                    .HoaDons.AsNoTracking()
                    .Where(x =>
                        x.NgayLap >= firstDayOfMonth
                        && x.NgayLap < firstDayOfNextMonth
                        && x.TrangThaiThanhToan == TrangThaiThanhToanDaThanhToan
                    )
                    .Select(x => (decimal?)x.TongTien)
                    .SumAsync() ?? 0,
            NguyenLieuSapHet = await GetNguyenLieuSapHetAsync(),
            TopSanPhamBanChay = await GetTopSanPhamBanChayAsync(),
            HoaDonGanDay = await GetHoaDonGanDayAsync(),
            PhieuNhapGanDay = await GetPhieuNhapGanDayAsync(),
        };
    }

    private async Task<List<DashboardSanPhamKhongTheBanItem>> GetSanPhamKhongTheBanAsync()
    {
        return await _context
            .SanPhams.AsNoTracking()
            .Where(x => x.TrangThai != TrangThaiDangBan)
            .OrderBy(x => x.MaSP)
            .Select(x => new DashboardSanPhamKhongTheBanItem
            {
                MaSP = x.MaSP,
                TenSP = x.TenSP,
                TrangThai = x.TrangThai,
            })
            .ToListAsync();
    }

    private async Task<List<DashboardNguyenLieuSapHetItem>> GetNguyenLieuSapHetAsync()
    {
        return await _context
            .NguyenLieus.AsNoTracking()
            .Where(x => (x.SoLuongTon ?? 0) <= x.MucTonToiThieu)
            .OrderBy(x => x.TenNL)
            .Select(x => new DashboardNguyenLieuSapHetItem
            {
                MaNL = x.MaNL,
                TenNL = x.TenNL,
                SoLuongTon = x.SoLuongTon ?? 0,
                MucTonToiThieu = x.MucTonToiThieu,
                DonViTinh = x.DonViTinh,
            })
            .ToListAsync();
    }

    private async Task<List<DashboardSanPhamBanChayItem>> GetTopSanPhamBanChayAsync()
    {
        try
        {
            return await (
                from ct in _context.CTHDs.AsNoTracking()
                join hd in _context.HoaDons.AsNoTracking() on ct.MaHD equals hd.MaHD
                join sp in _context.SanPhams.AsNoTracking() on ct.MaSP equals sp.MaSP
                where hd.TrangThaiThanhToan == TrangThaiThanhToanDaThanhToan
                group new { ct, sp } by new { sp.MaSP, sp.TenSP } into g
                orderby g.Sum(x => x.ct.SoLuong) descending
                select new DashboardSanPhamBanChayItem
                {
                    MaSP = g.Key.MaSP,
                    TenSP = g.Key.TenSP,
                    TongSoLuong = g.Sum(x => x.ct.SoLuong),
                    TongTien = g.Sum(x => x.ct.ThanhTien),
                }
            )
                .Take(5)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Không thể tải top sản phẩm bán chạy từ SQL.", ex);
        }
    }

    private async Task<List<DashboardHoaDonGanDayItem>> GetHoaDonGanDayAsync()
    {
        try
        {
            return await (
                from hd in _context.HoaDons.AsNoTracking()
                join nv in _context.NhanViens.AsNoTracking() on hd.MaNV equals nv.MaNV
                join ban in _context.Bans.AsNoTracking() on hd.MaBan equals ban.MaBan
                join kh in _context.KhachHangs.AsNoTracking() on hd.MaKH equals kh.MaKH into khGroup
                from kh in khGroup.DefaultIfEmpty()
                orderby hd.NgayLap descending, hd.MaHD descending
                select new DashboardHoaDonGanDayItem
                {
                    MaHD = hd.MaHD,
                    NgayLap = hd.NgayLap,
                    TenNV = nv.TenNV,
                    TenKH = kh == null ? null : kh.TenKH,
                    TenBan = ban.TenBan,
                    TongTien = hd.TongTien,
                }
            )
                .Take(5)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Không thể tải hóa đơn gần đây từ SQL.", ex);
        }
    }

    private async Task<List<DashboardPhieuNhapGanDayItem>> GetPhieuNhapGanDayAsync()
    {
        try
        {
            return await (
                from pn in _context.PhieuNhaps.AsNoTracking()
                join nv in _context.NhanViens.AsNoTracking() on pn.MaNV equals nv.MaNV
                orderby pn.NgayNhap descending, pn.MaPN descending
                select new DashboardPhieuNhapGanDayItem
                {
                    MaPN = pn.MaPN,
                    NgayNhap = pn.NgayNhap,
                    TenNV = nv.TenNV,
                    TongTien = pn.TongTien,
                }
            )
                .Take(5)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Không thể tải phiếu nhập gần đây từ SQL.", ex);
        }
    }
}
