using WebQLCHCF.Models;
using WebQLCHCF.Security;

namespace WebQLCHCF.Tests.Security;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _hasher = new();
    private readonly TaiKhoan _testAccount = new()
    {
        MaTK = "TK99",
        TenDangNhap = "test_user",
        MatKhau = "OldPassword123@",
        MaNV = "NV99",
        VaiTro = "Phục Vụ"
    };

    [Fact]
    public void HashPassword_ValidPassword_ReturnsHashedString()
    {
        var rawPassword = "SecurePassword123@";
        var hash = _hasher.HashPassword(_testAccount, rawPassword);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(rawPassword, hash);
    }

    [Fact]
    public void VerifyHashedPassword_MatchingHashedPassword_ReturnsSuccess()
    {
        var rawPassword = "SecurePassword123@";
        var hash = _hasher.HashPassword(_testAccount, rawPassword);

        var (succeeded, needsRehash) = _hasher.VerifyHashedPassword(_testAccount, hash, rawPassword);

        Assert.True(succeeded);
        Assert.False(needsRehash);
    }

    [Fact]
    public void VerifyHashedPassword_WrongPassword_ReturnsFailed()
    {
        var rawPassword = "SecurePassword123@";
        var hash = _hasher.HashPassword(_testAccount, rawPassword);

        var (succeeded, needsRehash) = _hasher.VerifyHashedPassword(_testAccount, hash, "WrongPassword456@");

        Assert.False(succeeded);
        Assert.False(needsRehash);
    }

    [Fact]
    public void VerifyHashedPassword_LegacyPlaintextPassword_ReturnsSuccessAndNeedsRehash()
    {
        var rawPlaintext = "12345678";

        // Account has plain text in DB from legacy database
        var (succeeded, needsRehash) = _hasher.VerifyHashedPassword(_testAccount, rawPlaintext, rawPlaintext);

        Assert.True(succeeded);
        Assert.True(needsRehash);
    }

    [Fact]
    public void VerifyHashedPassword_LegacyPlaintextWrongPassword_ReturnsFailed()
    {
        var rawPlaintext = "12345678";

        var (succeeded, _) = _hasher.VerifyHashedPassword(_testAccount, rawPlaintext, "incorrect_pass");

        Assert.False(succeeded);
    }

    [Theory]
    [InlineData(null, "pass")]
    [InlineData("hash", null)]
    [InlineData("", "pass")]
    [InlineData("hash", "")]
    public void VerifyHashedPassword_NullOrEmptyInputs_ReturnsFailed(string? hash, string? pass)
    {
        var (succeeded, _) = _hasher.VerifyHashedPassword(_testAccount, hash!, pass!);
        Assert.False(succeeded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_InvalidPassword_ThrowsArgumentException(string? invalidPass)
    {
        Assert.Throws<ArgumentException>(() => _hasher.HashPassword(_testAccount, invalidPass!));
    }
}
