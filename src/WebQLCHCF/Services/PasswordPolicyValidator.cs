namespace WebQLCHCF.Services;

/// <summary>
/// Domain Engine thẩm định tính an toàn và độ phức tạp của mật khẩu tài khoản hệ thống.
/// </summary>
public static class PasswordPolicyValidator
{
    public const int MinPasswordLength = 8;

    /// <summary>
    /// Thẩm định mật khẩu mới theo chuẩn an toàn (tối thiểu 8 ký tự, có số, có ký tự đặc biệt, không trùng username).
    /// </summary>
    public static bool ValidatePassword(
        string? matKhauMoi,
        string? xacNhanMatKhau,
        string? tenDangNhap,
        out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(matKhauMoi))
        {
            errors.Add("Mật khẩu mới không được để trống.");
            return false;
        }

        if (matKhauMoi != xacNhanMatKhau)
        {
            errors.Add("Xác nhận mật khẩu mới không khớp.");
        }

        if (!string.IsNullOrWhiteSpace(tenDangNhap) &&
            string.Equals(matKhauMoi, tenDangNhap, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Mật khẩu mới không được trùng tên đăng nhập.");
        }

        if (matKhauMoi.Length < MinPasswordLength)
        {
            errors.Add($"Mật khẩu mới phải có ít nhất {MinPasswordLength} ký tự.");
        }

        if (!matKhauMoi.Any(char.IsDigit))
        {
            errors.Add("Mật khẩu mới phải có ít nhất 1 chữ số.");
        }

        if (!matKhauMoi.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            errors.Add("Mật khẩu mới phải có ít nhất 1 ký tự đặc biệt.");
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Kiểm tra nhanh mật khẩu có đạt chuẩn hay không.
    /// </summary>
    public static bool IsValidPassword(string? matKhau, string? tenDangNhap = null)
    {
        return ValidatePassword(matKhau, matKhau, tenDangNhap, out _);
    }
}
