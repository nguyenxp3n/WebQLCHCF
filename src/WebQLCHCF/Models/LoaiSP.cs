using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class LoaiSP
{
    [Key, StringLength(5)]
    public string MaLoaiSP { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string TenLoaiSP { get; set; } = string.Empty;
}
