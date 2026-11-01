using WebQLCHCF.Models;

namespace WebQLCHCF.Security;

/// <summary>
/// Giao diện dịch vụ mã hóa và thẩm định mật khẩu chuẩn ASP.NET Core Identity PBKDF2.
/// </summary>
public interface IPasswordHasherService
{
    /// <summary>
    /// Mã hóa mật khẩu người dùng với salt ngẫu nhiên và PBKDF2 hashing.
    /// </summary>
    string HashPassword(TaiKhoan account, string password);

    /// <summary>
    /// Thẩm định mật khẩu cung cấp với mật khẩu lưu trong CSDL.
    /// Hỗ trợ tương thích ngược với mật khẩu thô cũ và đánh dấu cần rehash tự động.
    /// </summary>
    (bool Succeeded, bool NeedsRehash) VerifyHashedPassword(TaiKhoan account, string hashedPassword, string providedPassword);
}
