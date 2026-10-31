using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class KhachHang
{
    [Key, StringLength(5)]
    public string MaKH { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên khách hàng không được để trống.")]
    [StringLength(50, ErrorMessage = "Tên khách hàng tối đa 50 ký tự.")]
    public string TenKH { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại không được để trống.")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có đúng 10 chữ số.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại chỉ được nhập 10 chữ số.")]
    public string SDT { get; set; } = string.Empty;
    public int? DiemTichLuy { get; set; } = 0;
}
