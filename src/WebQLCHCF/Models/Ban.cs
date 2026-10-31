using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLCHCF.Models;

public class Ban
{
    [Key, StringLength(5)]
    public string MaBan { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string TenBan { get; set; } = string.Empty;

    public int? SoBan { get; set; }

    [Required, StringLength(20)]
    public string KhuVuc { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string TrangThai { get; set; } = string.Empty;
}
