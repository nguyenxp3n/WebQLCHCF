using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class CTPN
{
    [Required, StringLength(9)]
    public string MaPN { get; set; } = string.Empty;

    [Required, StringLength(5)]
    public string MaNL { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0.")]
    public int SoLuong { get; set; }

    [Column(TypeName = "money")]
    [Range(
        typeof(decimal),
        "0.0001",
        "100000000",
        ErrorMessage = "Đơn giá nhập phải lớn hơn 0 và không vượt quá 100.000.000đ."
    )]
    public decimal DonGia { get; set; }

    [Column(TypeName = "money"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal ThanhTien { get; private set; }
}
