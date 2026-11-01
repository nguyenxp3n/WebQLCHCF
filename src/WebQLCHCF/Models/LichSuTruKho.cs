using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

[Table("LichSuTruKho")]
public class LichSuTruKho
{
    [Key]
    public int MaLichSu { get; set; }

    [Required]
    [StringLength(9)]
    public string MaHD { get; set; } = string.Empty;

    [Required]
    [StringLength(5)]
    public string MaNL { get; set; } = string.Empty;

    public int SoLuongTruThucTe { get; set; }

    public DateTime ThoiGianTru { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(MaHD))]
    public virtual HoaDon? HoaDon { get; set; }

    [ForeignKey(nameof(MaNL))]
    public virtual NguyenLieu? NguyenLieu { get; set; }
}
