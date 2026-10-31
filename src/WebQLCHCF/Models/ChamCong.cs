using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class ChamCong
{
    [Key]
    public int MaCC { get; set; }

    [Required, StringLength(5)]
    public string MaNV { get; set; } = string.Empty;

    public DateTime ThoiGianCheckIn { get; set; }

    public DateTime? ThoiGianCheckOut { get; set; }

    [Required, StringLength(30)]
    public string TrangThai { get; set; } = string.Empty;

    [StringLength(200)]
    public string? GhiChu { get; set; }

    [Column(TypeName = "money")]
    public decimal DonGiaGio { get; set; }

    public int? TongPhut { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? TongGio { get; set; }

    [Column(TypeName = "money")]
    public decimal? TienCong { get; set; }
}
