using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using Xunit;

namespace WebQLCHCF.Tests.Integration;

public class SqlServerIntegrationTests
{
    private static string GetTargetConnectionString()
    {
        return Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION")
            ?? "Server=127.0.0.1,14334;Database=QLCHCF;User Id=sa;Password=Password@123;TrustServerCertificate=True;Encrypt=False;Connect Timeout=10;";
    }

    [Fact]
    public void EfCoreModel_MapsAllDomainEntitiesWithValidPrimaryKeys()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ModelIntegrityCheck_" + Guid.NewGuid().ToString("N"))
            .Options;

        using var context = new AppDbContext(options);
        var entityTypes = context.Model.GetEntityTypes().ToList();

        // 18 core entity tables in QLCHCF domain (including new LichSuTruKho)
        var expectedEntityNames = new HashSet<string>
        {
            "AuditLog", "Ban", "ChamCong", "CongThuc", "CTHD", "CTPN",
            "HoaDon", "KhachHang", "KhuyenMai", "LoaiSP", "Luong",
            "NguyenLieu", "NhanVien", "PhieuNhap", "SanPham", "TaiKhoan", "ThanhToan",
            "LichSuTruKho"
        };

        var mappedEntities = entityTypes
            .Where(e => e.FindPrimaryKey() != null)
            .Select(e => e.ClrType.Name)
            .ToHashSet();

        foreach (var expected in expectedEntityNames)
        {
            Assert.Contains(expected, mappedEntities);
            var entity = entityTypes.First(e => e.ClrType.Name == expected);
            Assert.NotNull(entity.FindPrimaryKey());
        }
    }

    [Fact]
    public async Task LiveSqlServer_WhenReachable_VerifiesConnectionAndSchema()
    {
        var connStr = GetTargetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connStr)
            .Options;

        using var context = new AppDbContext(options);

        bool canConnect = false;
        try
        {
            canConnect = await context.Database.CanConnectAsync();
        }
        catch
        {
            canConnect = false;
        }

        var isStrict = string.Equals(Environment.GetEnvironmentVariable("REQUIRE_SQLSERVER_TEST"), "true", StringComparison.OrdinalIgnoreCase);
        if (isStrict)
        {
            Assert.True(canConnect, "SQL Server must be reachable and responsive in strict CI testing mode.");
        }
        else if (!canConnect)
        {
            Assert.False(canConnect);
            return;
        }

        using var connection = new SqlConnection(connStr);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE name IN ('TaiKhoan', 'Ban', 'HoaDon', 'SanPham', 'NhanVien', 'NguyenLieu', 'LichSuTruKho');";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());

        Assert.True(count >= 6, "Live SQL Server must contain the essential tables.");
    }

    [Fact]
    public async Task LiveSqlServer_CTHD_Sua_RejectsEditOnPaidOrCancelledInvoice_WithErrorCode50006()
    {
        var connStr = GetTargetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connStr)
            .Options;

        using var context = new AppDbContext(options);

        bool canConnect = false;
        try
        {
            canConnect = await context.Database.CanConnectAsync();
        }
        catch
        {
            canConnect = false;
        }

        var isStrict = string.Equals(Environment.GetEnvironmentVariable("REQUIRE_SQLSERVER_TEST"), "true", StringComparison.OrdinalIgnoreCase);
        if (!canConnect)
        {
            if (isStrict)
                Assert.Fail("SQL Server must be reachable in strict CI mode.");
            return;
        }

        using var connection = new SqlConnection(connStr);
        await connection.OpenAsync();

        // 1. Tạo một hóa đơn mẫu đã hủy để thử nghiệm
        var testMaHD = "HDTEST99";
        using (var setupCmd = connection.CreateCommand())
        {
            setupCmd.CommandText = @"
                IF EXISTS (SELECT 1 FROM dbo.CTHD WHERE MaHD = @MaHD) DELETE FROM dbo.CTHD WHERE MaHD = @MaHD;
                IF EXISTS (SELECT 1 FROM dbo.HoaDon WHERE MaHD = @MaHD) DELETE FROM dbo.HoaDon WHERE MaHD = @MaHD;
                
                INSERT INTO dbo.HoaDon (MaHD, NgayLap, MaNV, TongTien, MaBan, TrangThaiOrder, TrangThaiThanhToan, ThoiGianTao)
                VALUES (@MaHD, CAST(GETDATE() AS DATE), 'NV01', 50000, 'BAN01', N'DaHuy', N'DaHuy', SYSUTCDATETIME());

                INSERT INTO dbo.CTHD (MaHD, MaSP, SoLuong, DonGia)
                VALUES (@MaHD, 'SP01', 2, 25000);
            ";
            setupCmd.Parameters.AddWithValue("@MaHD", testMaHD);
            await setupCmd.ExecuteNonQueryAsync();
        }

        // 2. Gọi CTHD_Sua trên hóa đơn đã hủy -> Phải ném ngoại lệ 50006
        using (var editCmd = connection.CreateCommand())
        {
            editCmd.CommandText = "EXEC dbo.CTHD_Sua @MaHD, @MaSP, @SoLuong, @DonGia";
            editCmd.Parameters.AddWithValue("@MaHD", testMaHD);
            editCmd.Parameters.AddWithValue("@MaSP", "SP01");
            editCmd.Parameters.AddWithValue("@SoLuong", 3);
            editCmd.Parameters.AddWithValue("@DonGia", 25000);

            var ex = await Assert.ThrowsAsync<SqlException>(() => editCmd.ExecuteNonQueryAsync());
            Assert.Equal(50006, ex.Number);
            Assert.Contains("không thể chỉnh sửa chi tiết món", ex.Message);
        }

        // Cleanup
        using (var cleanCmd = connection.CreateCommand())
        {
            cleanCmd.CommandText = @"
                DELETE FROM dbo.CTHD WHERE MaHD = @MaHD;
                DELETE FROM dbo.HoaDon WHERE MaHD = @MaHD;
            ";
            cleanCmd.Parameters.AddWithValue("@MaHD", testMaHD);
            await cleanCmd.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task LiveSqlServer_ThanhToan_HuyOrder_EnforcesPermissionFlagForPaidInvoices()
    {
        var connStr = GetTargetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connStr)
            .Options;

        using var context = new AppDbContext(options);

        bool canConnect = false;
        try
        {
            canConnect = await context.Database.CanConnectAsync();
        }
        catch
        {
            canConnect = false;
        }

        var isStrict = string.Equals(Environment.GetEnvironmentVariable("REQUIRE_SQLSERVER_TEST"), "true", StringComparison.OrdinalIgnoreCase);
        if (!canConnect)
        {
            if (isStrict)
                Assert.Fail("SQL Server must be reachable in strict CI mode.");
            return;
        }

        using var connection = new SqlConnection(connStr);
        await connection.OpenAsync();

        // 1. Tạo hóa đơn đã thanh toán
        var testMaHD = "HDTEST98";
        using (var setupCmd = connection.CreateCommand())
        {
            setupCmd.CommandText = @"
                IF EXISTS (SELECT 1 FROM dbo.ThanhToan WHERE MaHD = @MaHD) DELETE FROM dbo.ThanhToan WHERE MaHD = @MaHD;
                IF EXISTS (SELECT 1 FROM dbo.CTHD WHERE MaHD = @MaHD) DELETE FROM dbo.CTHD WHERE MaHD = @MaHD;
                IF EXISTS (SELECT 1 FROM dbo.HoaDon WHERE MaHD = @MaHD) DELETE FROM dbo.HoaDon WHERE MaHD = @MaHD;

                INSERT INTO dbo.HoaDon (MaHD, NgayLap, MaNV, TongTien, MaBan, TrangThaiOrder, TrangThaiThanhToan, ThoiGianTao, DaTruKho, DaCongDiem)
                VALUES (@MaHD, CAST(GETDATE() AS DATE), 'NV01', 50000, 'BAN01', N'HoanThanh', N'DaThanhToan', SYSUTCDATETIME(), 1, 1);

                INSERT INTO dbo.ThanhToan (MaHD, PhuongThuc, SoTien, TrangThai, ThoiGianThanhToan)
                VALUES (@MaHD, N'TienMat', 50000, N'DaThanhToan', SYSUTCDATETIME());
            ";
            setupCmd.Parameters.AddWithValue("@MaHD", testMaHD);
            await setupCmd.ExecuteNonQueryAsync();
        }

        // 2. Gọi ThanhToan_HuyOrder với @ChoPhepKhiDaThanhToan = 0 -> Ném lỗi không được hủy
        using (var rejectCmd = connection.CreateCommand())
        {
            rejectCmd.CommandText = "EXEC dbo.ThanhToan_HuyOrder @MaHD = @MaHD, @LyDo = N'Thử hủy', @ChoPhepKhiDaThanhToan = 0";
            rejectCmd.Parameters.AddWithValue("@MaHD", testMaHD);

            var ex = await Assert.ThrowsAsync<SqlException>(() => rejectCmd.ExecuteNonQueryAsync());
            Assert.Contains("không được hủy", ex.Message);
        }

        // 3. Gọi ThanhToan_HuyOrder với @ChoPhepKhiDaThanhToan = 1 -> Thành công
        using (var allowCmd = connection.CreateCommand())
        {
            allowCmd.CommandText = "EXEC dbo.ThanhToan_HuyOrder @MaHD = @MaHD, @LyDo = N'Khách trả món', @ChoPhepKhiDaThanhToan = 1";
            allowCmd.Parameters.AddWithValue("@MaHD", testMaHD);

            using var reader = await allowCmd.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(1, reader.GetInt32(0)); // Success = 1
        }

        // Cleanup
        using (var cleanCmd = connection.CreateCommand())
        {
            cleanCmd.CommandText = @"
                DELETE FROM dbo.ThanhToan WHERE MaHD = @MaHD;
                DELETE FROM dbo.CTHD WHERE MaHD = @MaHD;
                DELETE FROM dbo.HoaDon WHERE MaHD = @MaHD;
            ";
            cleanCmd.Parameters.AddWithValue("@MaHD", testMaHD);
            await cleanCmd.ExecuteNonQueryAsync();
        }
    }
}
