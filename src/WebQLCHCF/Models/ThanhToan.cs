using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class ThanhToan
{
    [Key]
    public int MaThanhToan { get; set; }

    [StringLength(9)]
    public string? MaHD { get; set; }

    [StringLength(50)]
    public string? MaPhienThanhToan { get; set; }

    [Required, StringLength(30)]
    public string PhuongThuc { get; set; } = string.Empty;

    [Column(TypeName = "money")]
    public decimal SoTien { get; set; }

    [StringLength(255)]
    public string? NoiDungChuyenKhoan { get; set; }

    [StringLength(100)]
    public string? MaGiaoDich { get; set; }

    [Required, StringLength(30)]
    public string TrangThai { get; set; } = string.Empty;

    public DateTime ThoiGianTao { get; set; }

    public DateTime? ThoiGianHetHan { get; set; }

    public DateTime? ThoiGianThanhToan { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }
}
