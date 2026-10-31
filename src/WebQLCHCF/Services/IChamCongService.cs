using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Services;

public interface IChamCongService
{
    Task AutoCheckInOnLoginAsync(string maNV);

    Task<ChamCongDangLamViewModel?> GetDangLamByNhanVienAsync(string maNV);

    Task<ChamCongCheckoutResultViewModel?> CheckOutAndGetResultAsync(
        string maNV,
        string matKhau
    );
}
