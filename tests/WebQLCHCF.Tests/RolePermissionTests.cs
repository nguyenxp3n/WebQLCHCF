using WebQLCHCF.Security;
using Xunit;

namespace WebQLCHCF.Tests;

public class RolePermissionTests
{
    [Fact]
    public void AppRoles_ChuaDu3VaiTroNghiepVuCoDinh()
    {
        Assert.Equal("Quản Lý", AppRoles.QuanLy);
        Assert.Equal("Pha Chế", AppRoles.PhaChe);
        Assert.Equal("Phục Vụ", AppRoles.PhucVu);
        Assert.Equal(3, AppRoles.All.Length);
    }

    [Theory]
    [InlineData("Quản Lý", true)]
    [InlineData("Pha Chế", true)]
    [InlineData("Phục Vụ", true)]
    [InlineData("Khách Hàng", false)]
    [InlineData("Admin", false)]
    [InlineData("", false)]
    public void AppRoles_KiemTraVaiTroThuocHeThong(string role, bool expected)
    {
        bool exists = AppRoles.All.Contains(role);
        Assert.Equal(expected, exists);
    }
}
