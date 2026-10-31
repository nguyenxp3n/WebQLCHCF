using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class CongThuc
{
    [StringLength(5)]
    public string MaSP { get; set; } = string.Empty;

    [StringLength(5)]
    public string MaNL { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,3)")]
    public decimal DinhLuong { get; set; }
}
