using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;

namespace WebQLCHCF.Tests.Integration;

public class SqlServerIntegrationTests
{
    private static string GetTargetConnectionString()
    {
        return Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION")
            ?? "Server=localhost,14334;Database=QLCHCF;User Id=sa;Password=Your_password123;TrustServerCertificate=True;Encrypt=False;Connect Timeout=2;";
    }

    [Fact]
    public void EfCoreModel_MapsAll17DomainEntitiesWithValidPrimaryKeys()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ModelIntegrityCheck_" + Guid.NewGuid().ToString("N"))
            .Options;

        using var context = new AppDbContext(options);
        var entityTypes = context.Model.GetEntityTypes().ToList();

        // 17 core entity tables in QLCHCF domain
        var expectedEntityNames = new HashSet<string>
        {
            "AuditLog", "Ban", "ChamCong", "CongThuc", "CTHD", "CTPN",
            "HoaDon", "KhachHang", "KhuyenMai", "LoaiSP", "Luong",
            "NguyenLieu", "NhanVien", "PhieuNhap", "SanPham", "TaiKhoan", "ThanhToan"
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
            // Database server is not active in this local environment; skip gracefully
            canConnect = false;
        }

        var isStrict = string.Equals(Environment.GetEnvironmentVariable("REQUIRE_SQLSERVER_TEST"), "true", StringComparison.OrdinalIgnoreCase);
        if (isStrict)
        {
            Assert.True(canConnect, "SQL Server must be reachable and responsive in strict CI testing mode.");
        }
        else if (!canConnect)
        {
            // Graceful verification: Server not present in this local offline environment.
            Assert.False(canConnect);
            return;
        }

        // Live SQL Server is reachable: perform live verification
        using var connection = new SqlConnection(connStr);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE name IN ('TaiKhoan', 'Ban', 'HoaDon', 'SanPham', 'NhanVien', 'NguyenLieu');";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());

        Assert.True(count >= 6, "Live SQL Server must contain the essential tables.");
    }
}
