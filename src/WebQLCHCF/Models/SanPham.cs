using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class SanPham
{
    [Key, StringLength(5)]
    public string MaSP { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string TenSP { get; set; } = string.Empty;

    [Column(TypeName = "money")]
    [Range(
        typeof(decimal),
        "1",
        "10000000",
        ErrorMessage = "Đơn giá phải lớn hơn 0 và không vượt quá 10.000.000đ."
    )]
    public decimal DonGia { get; set; }

    [Required, StringLength(5)]
    public string MaLoaiSP { get; set; } = string.Empty;

    [Required, StringLength(30), DisplayName("Trạng thái")]
    public string TrangThai { get; set; } = "Tạm ngưng";
}
