using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class CTHD
{
    [StringLength(9)]
    public string MaHD { get; set; } = string.Empty;

    [StringLength(5)]
    public string MaSP { get; set; } = string.Empty;
    public int SoLuong { get; set; }

    [Column(TypeName = "money")]
    public decimal DonGia { get; set; }

    [Column(TypeName = "money"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal ThanhTien { get; private set; }

    [StringLength(5)]
    public string? MaKM { get; set; }
}
