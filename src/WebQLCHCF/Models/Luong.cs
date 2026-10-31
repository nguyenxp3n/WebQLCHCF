using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class Luong
{
    [Key]
    public int MaLuong { get; set; }

    [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12.")]
    public int Thang { get; set; }

    [Range(2000, 2100, ErrorMessage = "Năm không hợp lệ.")]
    public int Nam { get; set; }

    [Column(TypeName = "money")]
    [Range(
        typeof(decimal),
        "0",
        "1000000000",
        ErrorMessage = "Lương cơ bản không được âm và không vượt quá 1.000.000.000đ."
    )]
    public decimal LuongCoBan { get; set; }

    [Column(TypeName = "money")]
    [Range(
        typeof(decimal),
        "0",
        "1000000000",
        ErrorMessage = "Phụ cấp không được âm và không vượt quá 1.000.000.000đ."
    )]
    public decimal? PhuCap { get; set; } = 0;

    [Column(TypeName = "money")]
    [Range(
        typeof(decimal),
        "0",
        "1000000000",
        ErrorMessage = "Thưởng không được âm và không vượt quá 1.000.000.000đ."
    )]
    public decimal? Thuong { get; set; } = 0;

    [Column(TypeName = "money")]
    [Range(
        typeof(decimal),
        "0",
        "1000000000",
        ErrorMessage = "Phạt không được âm và không vượt quá 1.000.000.000đ."
    )]
    public decimal? Phat { get; set; } = 0;

    [Column(TypeName = "money"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal TongLuong { get; private set; }
}
