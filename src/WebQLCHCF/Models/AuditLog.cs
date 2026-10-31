using System.ComponentModel.DataAnnotations;

namespace WebQLCHCF.Models;

public class AuditLog
{
    [Key]
    public long MaLog { get; set; }

    public DateTime ThoiGian { get; set; }

    [StringLength(50)]
    public string? MaNV { get; set; }

    [StringLength(100)]
    public string? TenDangNhap { get; set; }

    [StringLength(100)]
    public string? VaiTro { get; set; }

    [StringLength(100)]
    public string ChucNang { get; set; } = string.Empty;

    [StringLength(100)]
    public string HanhDong { get; set; } = string.Empty;

    [StringLength(100)]
    public string? DoiTuong { get; set; }

    [StringLength(100)]
    public string? MaDoiTuong { get; set; }

    [StringLength(1000)]
    public string NoiDung { get; set; } = string.Empty;

    public string? DuLieuTruoc { get; set; }

    public string? DuLieuSau { get; set; }

    [StringLength(100)]
    public string? DiaChiIP { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }
}
