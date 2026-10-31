using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;

namespace WebQLCHCF.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger
    )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
        string chucNang,
        string hanhDong,
        string? doiTuong,
        string? maDoiTuong,
        string noiDung,
        string? duLieuTruoc = null,
        string? duLieuSau = null
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(chucNang)
                || string.IsNullOrWhiteSpace(hanhDong)
                || string.IsNullOrWhiteSpace(noiDung))
            {
                _logger.LogWarning(
                    "Bỏ qua audit log do thiếu dữ liệu bắt buộc. ChucNang={ChucNang}, HanhDong={HanhDong}",
                    chucNang,
                    hanhDong
                );
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            var session = httpContext?.Session;
            var maNV = TrimToNull(session?.GetString("MaNV"));
            var tenDangNhap = TrimToNull(session?.GetString("TenDangNhap"));
            var vaiTro = TrimToNull(session?.GetString("VaiTro"));
            var diaChiIp = TrimToLength(httpContext?.Connection.RemoteIpAddress?.ToString(), 100);
            var userAgent = TrimToLength(httpContext?.Request.Headers.UserAgent.ToString(), 500);

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC AuditLog_Them {maNV}, {tenDangNhap}, {vaiTro}, {TrimToLength(chucNang, 100)}, {TrimToLength(hanhDong, 100)}, {TrimToLength(doiTuong, 100)}, {TrimToLength(maDoiTuong, 100)}, {TrimToLength(noiDung, 1000)}, {duLieuTruoc}, {duLieuSau}, {diaChiIp}, {userAgent}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Không thể ghi audit log. ChucNang={ChucNang}, HanhDong={HanhDong}, DoiTuong={DoiTuong}, MaDoiTuong={MaDoiTuong}",
                chucNang,
                hanhDong,
                doiTuong,
                maDoiTuong
            );
        }
    }

    private static string? TrimToNull(string? value)
    {
        value = value?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? TrimToLength(string? value, int maxLength)
    {
        value = TrimToNull(value);
        return value != null && value.Length > maxLength ? value[..maxLength] : value;
    }
}
