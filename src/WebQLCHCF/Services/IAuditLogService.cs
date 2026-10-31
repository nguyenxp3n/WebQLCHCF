namespace WebQLCHCF.Services;

public interface IAuditLogService
{
    Task LogAsync(
        string chucNang,
        string hanhDong,
        string? doiTuong,
        string? maDoiTuong,
        string noiDung,
        string? duLieuTruoc = null,
        string? duLieuSau = null
    );
}
