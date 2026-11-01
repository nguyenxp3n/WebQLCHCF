using Microsoft.AspNetCore.Identity;
using WebQLCHCF.Models;

namespace WebQLCHCF.Security;

/// <summary>
/// Dịch vụ mã hóa và thẩm định mật khẩu chuẩn ASP.NET Core Identity PBKDF2 (HMAC-SHA512/256 + 100,000 iterations).
/// Cung cấp cơ chế tự động nâng cấp (rehash) cho mật khẩu thô kế thừa từ hệ thống cũ.
/// </summary>
public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<TaiKhoan> _hasher = new();

    public string HashPassword(TaiKhoan account, string password)
    {
        ArgumentNullException.ThrowIfNull(account);
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Mật khẩu không được để trống.", nameof(password));

        return _hasher.HashPassword(account, password);
    }

    public (bool Succeeded, bool NeedsRehash) VerifyHashedPassword(TaiKhoan account, string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
            return (false, false);

        try
        {
            var result = _hasher.VerifyHashedPassword(account, hashedPassword, providedPassword);
            if (result == PasswordVerificationResult.Success)
                return (true, false);

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
                return (true, true);
        }
        catch (FormatException)
        {
            // Bắt lỗi format khi hashedPassword là chuỗi thô chưa qua mã hóa Base64
        }
        catch (Exception)
        {
            // Bắt các ngoại lệ biên giải mã payload không hợp lệ
        }

        // Tương thích ngược: kiểm tra so khớp chuỗi thô cũ
        if (string.Equals(hashedPassword, providedPassword, StringComparison.Ordinal))
        {
            return (true, true);
        }

        return (false, false);
    }
}
