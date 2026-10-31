using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}
