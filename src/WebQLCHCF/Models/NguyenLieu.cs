using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class NguyenLieu
{
    [Key, StringLength(5)]
    public string MaNL { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string TenNL { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string DonViTinh { get; set; } = string.Empty;
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn không được âm.")]
    public int? SoLuongTon { get; set; } = 0;

    [Column(TypeName = "money")]
    [Range(
        typeof(decimal),
        "0",
        "100000000",
        ErrorMessage = "Giá nhập không được âm và không vượt quá 100.000.000đ."
    )]
    public decimal GiaNhap { get; set; }

    [DataType(DataType.Date)]
    public DateTime NgayCapNhat { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Mức tồn tối thiểu không được âm.")]
    public int MucTonToiThieu { get; set; }
}
