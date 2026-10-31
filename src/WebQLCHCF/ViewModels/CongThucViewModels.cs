using System.ComponentModel.DataAnnotations;

namespace WebQLCHCF.ViewModels;

public class CongThucEditorViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn sản phẩm.")]
    public string MaSP { get; set; } = string.Empty;

    public string? TenSP { get; set; }

    public List<CongThucNguyenLieuItemViewModel> Items { get; set; } = new();

    public List<CongThucNguyenLieuItemViewModel> NguyenLieus { get; set; } = new();
}

public class CongThucNguyenLieuItemViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn nguyên liệu.")]
    public string MaNL { get; set; } = string.Empty;

    public decimal DinhLuong { get; set; }
}

public class CongThucIndexItemViewModel
{
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public List<CongThucViewModel> NguyenLieus { get; set; } = new();
}
