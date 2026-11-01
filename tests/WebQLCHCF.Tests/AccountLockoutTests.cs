using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Controllers;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Services;
using WebQLCHCF.Tests.TestHarness;
using WebQLCHCF.ViewModels;
using Xunit;

namespace WebQLCHCF.Tests;

public class AccountLockoutTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);

        context.TaiKhoans.Add(new TaiKhoan
        {
            MaTK = "TK_TEST",
            TenDangNhap = "test_lockout_user",
            MatKhau = "PlainPassword123@",
            VaiTro = "QuanLy",
            MaNV = "NV_TEST"
        });
        context.SaveChanges();
        return context;
    }

    [Fact]
    public void AccountController_LockoutLogic_AfterFiveFailedAttempts_BecomesLockedOut()
    {
        var testUsername = "lockout_threshold_" + Guid.NewGuid().ToString("N")[..8];
        AccountController.ResetLoginAttempts(testUsername);

        try
        {
            Assert.False(AccountController.IsLockedOut(testUsername, out _));

            for (int i = 1; i <= 4; i++)
            {
                AccountController.RecordFailedLogin(testUsername);
                Assert.False(AccountController.IsLockedOut(testUsername, out _));
            }

            // 5th attempt triggers lockout
            AccountController.RecordFailedLogin(testUsername);
            Assert.True(AccountController.IsLockedOut(testUsername, out var remaining));
            Assert.True(remaining.TotalMinutes > 0);
            Assert.True(remaining.TotalMinutes <= 5);
        }
        finally
        {
            AccountController.ResetLoginAttempts(testUsername);
        }
    }

    [Fact]
    public void AccountController_ResetLoginAttempts_ClearsLockoutImmediately()
    {
        var testUsername = "lockout_reset_" + Guid.NewGuid().ToString("N")[..8];
        AccountController.ResetLoginAttempts(testUsername);

        try
        {
            for (int i = 0; i < 5; i++)
            {
                AccountController.RecordFailedLogin(testUsername);
            }
            Assert.True(AccountController.IsLockedOut(testUsername, out _));

            AccountController.ResetLoginAttempts(testUsername);
            Assert.False(AccountController.IsLockedOut(testUsername, out _));
        }
        finally
        {
            AccountController.ResetLoginAttempts(testUsername);
        }
    }

    [Fact]
    public async Task AccountController_Login_WhenLockedOut_RejectsLoginWithoutHittingDb()
    {
        var testUsername = "locked_login_" + Guid.NewGuid().ToString("N")[..8];
        AccountController.ResetLoginAttempts(testUsername);

        try
        {
            for (int i = 0; i < 5; i++)
            {
                AccountController.RecordFailedLogin(testUsername);
            }

            using var context = CreateInMemoryDbContext();
            var controller = new AccountController(context, new TestChamCongServiceDouble(), new TestAuditLogServiceDouble());
            TestControllerContextFactory.SetupController(controller);

            var result = await controller.Login(new LoginViewModel
            {
                TenDangNhap = testUsername,
                MatKhau = "AnyPassword"
            });

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains("khóa do nhập sai mật khẩu", controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
        }
        finally
        {
            AccountController.ResetLoginAttempts(testUsername);
        }
    }
}
