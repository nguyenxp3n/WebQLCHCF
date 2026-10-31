using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class KhuyenMai
{
    [Key, StringLength(5)]
    public string MaKM { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string TenKM { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LoaiKM { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(
        typeof(decimal),
        "0",
        "100000000",
        ErrorMessage = "Giá trị khuyến mãi không được âm và không vượt quá 100.000.000đ."
    )]
    public decimal GiaTri { get; set; }

    [DataType(DataType.Date)]
    public DateTime NgayBatDau { get; set; }

    [DataType(DataType.Date)]
    public DateTime NgayKetThuc { get; set; }

    [Required, StringLength(500)]
    public string DieuKienApDung { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string TrangThai { get; set; } = string.Empty;
}
