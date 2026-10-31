using WebQLCHCF.Services;
using Xunit;

namespace WebQLCHCF.Tests;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Abc@1234", "user01", true)]
    [InlineData("Nguyen@2026", "qly_tram", true)]
    [InlineData("Pass#9999", "admin", true)]
    public void ValidatePassword_MatKhauDatChuan_TraVeTrueVaKhongCoLoi(
        string password, string username, bool expected)
    {
        bool result = PasswordPolicyValidator.ValidatePassword(
            password, password, username, out var errors);

        Assert.Equal(expected, result);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("Short1!", "user01", "ít nhất 8 ký tự")]
    [InlineData("NoDigitAtAll!", "user01", "ít nhất 1 chữ số")]
    [InlineData("NoSpecial123", "user01", "ít nhất 1 ký tự đặc biệt")]
    [InlineData("user01@123", "user01@123", "không được trùng tên đăng nhập")]
    public void ValidatePassword_MatKhauViPhamChinhSach_TraVeLoiTuongUng(
        string password, string username, string expectedErrorSubstring)
    {
        bool result = PasswordPolicyValidator.ValidatePassword(
            password, password, username, out var errors);

        Assert.False(result);
        Assert.Contains(errors, e => e.Contains(expectedErrorSubstring));
    }

    [Fact]
    public void ValidatePassword_XacNhanMatKhauKhongKhop_TraVeLoiXacNhan()
    {
        bool result = PasswordPolicyValidator.ValidatePassword(
            "ValidPass@123", "Mismatch@456", "user01", out var errors);

        Assert.False(result);
        Assert.Contains(errors, e => e.Contains("không khớp"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidatePassword_MatKhauRong_TraVeFalseVaLoiDeTrong(string? emptyPassword)
    {
        bool result = PasswordPolicyValidator.ValidatePassword(
            emptyPassword, emptyPassword, "user01", out var errors);

        Assert.False(result);
        Assert.Contains(errors, e => e.Contains("không được để trống"));
    }
}
