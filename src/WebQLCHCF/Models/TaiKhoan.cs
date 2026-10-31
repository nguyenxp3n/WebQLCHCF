using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class TaiKhoan
{
    [Key, StringLength(10)]
    public string MaTK { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required, StringLength(50), DataType(DataType.Password)]
    public string MatKhau { get; set; } = string.Empty;

    [Required, StringLength(5)]
    public string MaNV { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string VaiTro { get; set; } = string.Empty;
}
