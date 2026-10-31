namespace WebQLCHCF.Security;

public static class AppRoles
{
    public const string QuanLy = "Quản Lý";
    public const string PhaChe = "Pha Chế";
    public const string PhucVu = "Phục Vụ";

    public static readonly string[] All = [QuanLy, PhaChe, PhucVu];
}
