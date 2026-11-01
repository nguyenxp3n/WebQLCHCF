using WebQLCHCF.Security;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Services;

public class ChamCongService : IChamCongService
{
    private const string AutoCheckInNote = "Tự động check-in khi đăng nhập";
    private const string TrangThaiDangLam = "Đang làm";

    private readonly AppDbContext _context;
    private readonly IPasswordHasherService _passwordHasherService;

    public ChamCongService(AppDbContext context, IPasswordHasherService? passwordHasherService = null)
    {
        _context = context;
        _passwordHasherService = passwordHasherService ?? new PasswordHasherService();
    }

    public async Task AutoCheckInOnLoginAsync(string maNV)
    {
        if (string.IsNullOrWhiteSpace(maNV))
            return;

        try
        {
            if (await HasDangLamAsync(maNV))
                return;

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC ChamCong_CheckIn {maNV}, {AutoCheckInNote}"
            );
        }
        catch
        {
            if (!await HasDangLamAsync(maNV))
                throw;
        }
    }

    public async Task<ChamCongDangLamViewModel?> GetDangLamByNhanVienAsync(string maNV)
    {
        if (string.IsNullOrWhiteSpace(maNV))
            return null;

        var now = DateTime.Now;
        var item = await (
            from cc in _context.ChamCongs.AsNoTracking()
            join nv in _context.NhanViens.AsNoTracking() on cc.MaNV equals nv.MaNV
            where cc.MaNV == maNV
                && cc.TrangThai == TrangThaiDangLam
                && cc.ThoiGianCheckOut == null
            orderby cc.ThoiGianCheckIn descending, cc.MaCC descending
            select new
            {
                cc.MaCC,
                cc.MaNV,
                nv.TenNV,
                nv.ChucVu,
                cc.ThoiGianCheckIn,
                cc.ThoiGianCheckOut,
                cc.TrangThai,
                cc.GhiChu,
                cc.DonGiaGio,
            }
        ).FirstOrDefaultAsync();

        if (item == null)
            return null;

        var tongPhutTamTinh = Math.Max(0, (int)(now - item.ThoiGianCheckIn).TotalMinutes);
        var tongGioTamTinh = Math.Round(tongPhutTamTinh / 60m, 2);

        return new ChamCongDangLamViewModel
        {
            MaCC = item.MaCC,
            MaNV = item.MaNV,
            TenNV = item.TenNV,
            ChucVu = item.ChucVu,
            ThoiGianCheckIn = item.ThoiGianCheckIn,
            ThoiGianCheckOut = item.ThoiGianCheckOut,
            TrangThai = item.TrangThai,
            GhiChu = item.GhiChu,
            DonGiaGio = item.DonGiaGio,
            TongPhutTamTinh = tongPhutTamTinh,
            TongGioTamTinh = tongGioTamTinh,
            TienCongTamTinh = Math.Round(item.DonGiaGio * tongPhutTamTinh / 60m, 2),
        };
    }

    public async Task<ChamCongCheckoutResultViewModel?> CheckOutAndGetResultAsync(
        string maNV,
        string matKhau
    )
    {
        if (string.IsNullOrWhiteSpace(maNV))
            throw new InvalidOperationException("Phiên đăng nhập không hợp lệ.");

        if (string.IsNullOrWhiteSpace(matKhau))
            throw new InvalidOperationException("Vui lòng nhập mật khẩu.");

        var account = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.MaNV == maNV);
        if (account == null)
            throw new InvalidOperationException("Mật khẩu không đúng.");

        var (verified, needsRehash) = _passwordHasherService.VerifyHashedPassword(account, account.MatKhau, matKhau);
        if (!verified)
            throw new InvalidOperationException("Mật khẩu không đúng.");

        if (needsRehash)
        {
            account.MatKhau = _passwordHasherService.HashPassword(account, matKhau);
            await _context.SaveChangesAsync();
        }

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC ChamCong_CheckOut {maNV}, {account.MatKhau}"
        );

        var item = await (
            from cc in _context.ChamCongs.AsNoTracking()
            join nv in _context.NhanViens.AsNoTracking() on cc.MaNV equals nv.MaNV
            where cc.MaNV == maNV && cc.ThoiGianCheckOut != null
            orderby cc.ThoiGianCheckOut descending, cc.MaCC descending
            select new
            {
                cc.MaCC,
                cc.MaNV,
                nv.TenNV,
                nv.ChucVu,
                cc.ThoiGianCheckIn,
                cc.ThoiGianCheckOut,
                cc.TrangThai,
                cc.GhiChu,
                cc.DonGiaGio,
                cc.TongPhut,
                cc.TongGio,
                cc.TienCong,
            }
        ).FirstOrDefaultAsync();

        return item == null
            ? null
            : new ChamCongCheckoutResultViewModel
            {
                MaCC = item.MaCC,
                MaNV = item.MaNV,
                TenNV = item.TenNV,
                ChucVu = item.ChucVu,
                ThoiGianCheckIn = item.ThoiGianCheckIn,
                ThoiGianCheckOut = item.ThoiGianCheckOut,
                TrangThai = item.TrangThai,
                GhiChu = item.GhiChu,
                DonGiaGio = item.DonGiaGio,
                TongPhut = item.TongPhut,
                TongGio = item.TongGio,
                TienCong = item.TienCong,
            };
    }

    private Task<bool> HasDangLamAsync(string maNV)
    {
        return _context.ChamCongs.AsNoTracking().AnyAsync(x =>
            x.MaNV == maNV && x.TrangThai == TrangThaiDangLam && x.ThoiGianCheckOut == null
        );
    }
}
