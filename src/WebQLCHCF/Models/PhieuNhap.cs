using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class PhieuNhap
{
    [Key, StringLength(9)]
    public string MaPN { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime NgayNhap { get; set; }

    [Required, StringLength(5)]
    public string MaNV { get; set; } = string.Empty;

    [Column(TypeName = "money")]
    public decimal TongTien { get; private set; }
}
