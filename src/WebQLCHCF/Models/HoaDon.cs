using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class HoaDon
{
    [Key, StringLength(9)]
    public string MaHD { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime NgayLap { get; set; }

    [Required, StringLength(5)]
    public string MaNV { get; set; } = string.Empty;

    [StringLength(5)]
    public string? MaKH { get; set; }

    [Column(TypeName = "money")]
    public decimal TongTien { get; set; }

    [StringLength(100)]
    public string? GhiChu { get; set; }

    [Required, StringLength(5)]
    public string MaBan { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string TrangThaiOrder { get; set; } = "ChoThanhToan";

    [Required, StringLength(30)]
    public string TrangThaiThanhToan { get; set; } = "ChuaThanhToan";

    [StringLength(30)]
    public string? PhuongThucThanhToan { get; set; }

    public DateTime ThoiGianTao { get; set; }

    public DateTime? ThoiGianThanhToan { get; set; }

    [StringLength(36)]
    public string? ClientRequestId { get; set; }

    public bool DaTruKho { get; set; } = false;

    public bool DaCongDiem { get; set; } = false;
}
