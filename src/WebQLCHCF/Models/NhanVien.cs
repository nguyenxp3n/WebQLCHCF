using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class NhanVien
{
    [Key, StringLength(5)]
    public string MaNV { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string TenNV { get; set; } = string.Empty;

    [Required, StringLength(5)]
    public string GioiTinh { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? NgaySinh { get; set; }

    [Required, StringLength(10)]
    public string SDT { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string ChucVu { get; set; } = string.Empty;
    public int MaLuong { get; set; }

    [Column(TypeName = "money")]
    [Range(
        typeof(decimal),
        "0",
        "1000000",
        ErrorMessage = "Lương theo giờ không được âm và không vượt quá 1.000.000đ."
    )]
    public decimal LuongTheoGio { get; set; } = 20000;
}
