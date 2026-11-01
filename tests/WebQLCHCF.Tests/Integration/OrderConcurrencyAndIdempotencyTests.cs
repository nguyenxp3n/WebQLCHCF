using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Services;
using WebQLCHCF.ViewModels;
using Xunit;

namespace WebQLCHCF.Tests.Integration;

public class OrderConcurrencyAndIdempotencyTests
{
    private static string GetTargetConnectionString()
    {
        return Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION")
            ?? "Server=localhost,14334;Database=QLCHCF;User Id=sa;Password=Password@123;TrustServerCertificate=True;Encrypt=False;Connect Timeout=3;";
    }

    #region 1. ClientRequestId Validation & Normalization Tests
    [Fact]
    public void NormalizeClientRequestId_WhenNullOrWhitespace_GeneratesValidStandardUuid()
    {
        var id1 = OrderService.NormalizeClientRequestId(null);
        var id2 = OrderService.NormalizeClientRequestId("   ");

        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
        Assert.Equal(36, id1.Length);
        Assert.Equal(36, id2.Length);
        Assert.True(Guid.TryParse(id1, out _));
        Assert.True(Guid.TryParse(id2, out _));
    }

    [Theory]
    [InlineData("c7a10738-f9b7-4b71-b258-cf2f4dc945c5")]
    [InlineData("C7A10738-F9B7-4B71-B258-CF2F4DC945C5")]
    [InlineData("c7a10738f9b74b71b258cf2f4dc945c5")]
    [InlineData("{c7a10738-f9b7-4b71-b258-cf2f4dc945c5}")]
    public void NormalizeClientRequestId_WhenValidUuidFormat_ReturnsNormalizedLowercaseStandardUuid(string input)
    {
        var normalized = OrderService.NormalizeClientRequestId(input);

        Assert.Equal("c7a10738-f9b7-4b71-b258-cf2f4dc945c5", normalized);
        Assert.Equal(36, normalized.Length);
    }

    [Theory]
    [InlineData("invalid-not-a-guid")]
    [InlineData("12345")]
    [InlineData("req_abcdef123456")]
    [InlineData("DROP TABLE HoaDon;--")]
    public void NormalizeClientRequestId_WhenInvalidUuidFormat_ThrowsInvalidOperationExceptionImmediately(string invalidId)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => OrderService.NormalizeClientRequestId(invalidId));
        Assert.Contains("không đúng định dạng UUID", ex.Message);
    }

    [Fact]
    public void NormalizeClientRequestId_WhenExceeds36Chars_ThrowsInvalidOperationExceptionImmediately()
    {
        var overlyLongString = new string('a', 37);
        var ex = Assert.Throws<InvalidOperationException>(() => OrderService.NormalizeClientRequestId(overlyLongString));
        Assert.Contains("vượt quá 36 ký tự", ex.Message);
    }
    #endregion

    #region 2. BuildGhiChuWithToken Boundary & Resilience Tests
    [Fact]
    public void BuildGhiChuWithToken_WhenRequestIdIsLong_DoesNotUnderflowAndTruncatesSafely()
    {
        var userNote = new string('X', 120);
        var longId = "c7a10738-f9b7-4b71-b258-cf2f4dc945c5-extra-characters";
        
        var result = OrderService.BuildGhiChuWithToken(userNote, longId);

        Assert.NotNull(result);
        Assert.True(result.Length <= 100);
        Assert.Contains("[req:", result);
    }

    [Fact]
    public void BuildGhiChuWithToken_WhenRequestIdIsEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => OrderService.BuildGhiChuWithToken("Note", ""));
    }

    [Fact]
    public void StripRequestToken_PreservesUserNoteCleanly()
    {
        var raw = "Khách thích uống ngọt [req:c7a10738-f9b7-4b71-b258-cf2f4dc945c5]";
        var stripped = OrderService.StripRequestToken(raw);

        Assert.Equal("Khách thích uống ngọt", stripped);
    }
    #endregion

    #region 3. EF Core Model Metadata Verification
    [Fact]
    public void EfCoreModel_HoaDon_ConfiguredWithClientRequestIdAndUniqueIndex()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ModelIndexCheck_" + Guid.NewGuid().ToString("N"))
            .Options;

        using var context = new AppDbContext(options);
        var hoaDonEntity = context.Model.FindEntityType(typeof(HoaDon));

        Assert.NotNull(hoaDonEntity);

        var property = hoaDonEntity.FindProperty(nameof(HoaDon.ClientRequestId));
        Assert.NotNull(property);
        Assert.Equal(36, property.GetMaxLength());
        Assert.False(property.IsUnicode());

        var index = hoaDonEntity.GetIndexes()
            .FirstOrDefault(idx => idx.Properties.Any(p => p.Name == nameof(HoaDon.ClientRequestId)));

        Assert.NotNull(index);
        Assert.True(index.IsUnique, "Index on ClientRequestId must be UNIQUE.");
    }
    #endregion

    #region 4. OrderService Idempotency, Retry & Concurrency Tests
    private static async Task<(AppDbContext Context, OrderService Service)> CreateTestHarnessAsync(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new AppDbContext(options);

        // Seed required master records
        var staff = new NhanVien
        {
            MaNV = "NV01",
            TenNV = "Nguyễn Văn Test",
            GioiTinh = "Nam",
            SDT = "0901234567",
            ChucVu = "Phục vụ",
            MaLuong = 1
        };
        context.NhanViens.Add(staff);

        // Checked-in shift (required for ordering)
        var shift = new ChamCong
        {
            MaCC = 1,
            MaNV = "NV01",
            ThoiGianCheckIn = DateTime.Now.AddHours(-2),
            ThoiGianCheckOut = null,
            TrangThai = "Đang làm"
        };
        context.ChamCongs.Add(shift);

        // Active table
        var table = new Ban
        {
            MaBan = "BAN01",
            TenBan = "Bàn 1",
            KhuVuc = "Tầng trệt",
            TrangThai = OrderWorkflowConstants.TrangThaiBanTrong
        };
        context.Bans.Add(table);

        // Active product
        var product = new SanPham
        {
            MaSP = "CF01",
            TenSP = "Cà phê đen đá",
            DonGia = 25000,
            TrangThai = "Đang bán",
            MaLoaiSP = "L01"
        };
        context.SanPhams.Add(product);

        await context.SaveChangesAsync();

        var service = new OrderService(context);
        return (context, service);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenCalledRepeatedlyWithSameClientRequestId_ReturnsSameInvoiceWithoutDuplication()
    {
        var dbName = "IdempotencyRetry_" + Guid.NewGuid().ToString("N");
        var (context, service) = await CreateTestHarnessAsync(dbName);

        var clientRequestId = Guid.NewGuid().ToString("D");
        var model = new OrderCreateViewModel
        {
            ClientRequestId = clientRequestId,
            HinhThuc = OrderWorkflowConstants.HinhThucTaiQuan,
            KhuVuc = "Tầng trệt",
            GhiChu = "Không đường nhiều đá",
            Items = new List<OrderItemViewModel>
            {
                new() { MaSP = "CF01", SoLuong = 2 }
            }
        };

        // First attempt (initial creation)
        var maHD1 = await service.CreateOrderAsync(model, "NV01");
        Assert.False(string.IsNullOrWhiteSpace(maHD1));

        // Second attempt (simulating network timeout / retry with identical ClientRequestId)
        var maHD2 = await service.CreateOrderAsync(model, "NV01");
        Assert.Equal(maHD1, maHD2);

        // Third attempt
        var maHD3 = await service.CreateOrderAsync(model, "NV01");
        Assert.Equal(maHD1, maHD3);

        // Assert strictly only 1 invoice exists in storage
        var totalInvoices = await context.HoaDons.CountAsync();
        Assert.Equal(1, totalInvoices);

        var savedInvoice = await context.HoaDons.FirstAsync();
        Assert.Equal(clientRequestId, savedInvoice.ClientRequestId);
        Assert.Equal("Không đường nhiều đá", savedInvoice.GhiChu); // Clean note without token pollution
    }

    [Fact]
    public async Task CreateOrderAsync_WhenCalledWithDifferentClientRequestIds_CreatesDistinctInvoices()
    {
        var dbName = "DistinctOrders_" + Guid.NewGuid().ToString("N");
        var (context, service) = await CreateTestHarnessAsync(dbName);

        // Add a second table so second order can be seated
        context.Bans.Add(new Ban
        {
            MaBan = "BAN02",
            TenBan = "Bàn 2",
            KhuVuc = "Tầng trệt",
            TrangThai = OrderWorkflowConstants.TrangThaiBanTrong
        });
        await context.SaveChangesAsync();

        var model1 = new OrderCreateViewModel
        {
            ClientRequestId = Guid.NewGuid().ToString("D"),
            HinhThuc = OrderWorkflowConstants.HinhThucTaiQuan,
            KhuVuc = "Tầng trệt",
            GhiChu = "Bàn 1 note",
            Items = new List<OrderItemViewModel> { new() { MaSP = "CF01", SoLuong = 1 } }
        };

        var model2 = new OrderCreateViewModel
        {
            ClientRequestId = Guid.NewGuid().ToString("D"),
            HinhThuc = OrderWorkflowConstants.HinhThucTaiQuan,
            KhuVuc = "Tầng trệt",
            GhiChu = "Bàn 2 note",
            Items = new List<OrderItemViewModel> { new() { MaSP = "CF01", SoLuong = 3 } }
        };

        var maHD1 = await service.CreateOrderAsync(model1, "NV01");
        var maHD2 = await service.CreateOrderAsync(model2, "NV01");

        Assert.NotEqual(maHD1, maHD2);
        Assert.Equal(2, await context.HoaDons.CountAsync());
    }

    [Fact]
    public async Task CreateOrderAsync_WhenTenConcurrentRequestsHaveSameClientRequestId_AllResolveToSingleInvoice()
    {
        var dbName = "ConcurrentSameToken_" + Guid.NewGuid().ToString("N");
        var (context, service) = await CreateTestHarnessAsync(dbName);

        var sharedRequestId = Guid.NewGuid().ToString("D");

        // Concurrent execution simulation: 10 parallel tasks submitting identical order token
        var tasks = Enumerable.Range(1, 10).Select(_ =>
        {
            var model = new OrderCreateViewModel
            {
                ClientRequestId = sharedRequestId,
                HinhThuc = OrderWorkflowConstants.HinhThucTaiQuan,
                KhuVuc = "Tầng trệt",
                GhiChu = "Bàn đông khách",
                Items = new List<OrderItemViewModel> { new() { MaSP = "CF01", SoLuong = 1 } }
            };
            return service.CreateOrderAsync(model, "NV01");
        }).ToList();

        var results = await Task.WhenAll(tasks);

        // All 10 requests must return the exact same invoice ID
        var firstId = results[0];
        Assert.All(results, id => Assert.Equal(firstId, id));

        // Exactly one invoice persisted
        Assert.Equal(1, await context.HoaDons.CountAsync());
    }
    #endregion

    #region 5. Live SQL Server Schema & Concurrency Guard Verification
    [Fact]
    public async Task LiveSqlServer_WhenReachable_VerifiesClientRequestIdColumnAndUniqueIndex()
    {
        var connStr = GetTargetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connStr)
            .Options;

        using var context = new AppDbContext(options);

        bool canConnect;
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
            Assert.True(canConnect, "Live SQL Server is mandatory in strict CI mode.");
        }
        else if (!canConnect)
        {
            // Gracefully pass in offline developer environments where local SQL Server is not up
            return;
        }

        // Live SQL Server is reachable: perform physical schema verification
        using var connection = new SqlConnection(connStr);
        await connection.OpenAsync();

        // 1. Verify ClientRequestId column exists in dbo.HoaDon
        using var colCmd = connection.CreateCommand();
        colCmd.CommandText = @"
            SELECT max_length, is_nullable 
            FROM sys.columns 
            WHERE object_id = OBJECT_ID('dbo.HoaDon') AND name = 'ClientRequestId';";

        using var reader = await colCmd.ExecuteReaderAsync();
        Assert.True(reader.Read(), "Column ClientRequestId must exist in dbo.HoaDon.");
        var length = reader.GetInt16(0);
        var isNullable = reader.GetBoolean(1);
        Assert.Equal(36, length);
        Assert.True(isNullable);
        await reader.CloseAsync();

        // 2. Verify UX_HoaDon_ClientRequestId UNIQUE index exists
        using var idxCmd = connection.CreateCommand();
        idxCmd.CommandText = @"
            SELECT is_unique, has_filter 
            FROM sys.indexes 
            WHERE object_id = OBJECT_ID('dbo.HoaDon') AND name = 'UX_HoaDon_ClientRequestId';";

        using var idxReader = await idxCmd.ExecuteReaderAsync();
        Assert.True(idxReader.Read(), "Index UX_HoaDon_ClientRequestId must exist.");
        Assert.True(idxReader.GetBoolean(0), "Index UX_HoaDon_ClientRequestId must be UNIQUE.");
        Assert.True(idxReader.GetBoolean(1), "Index UX_HoaDon_ClientRequestId must have a filter condition (WHERE ClientRequestId IS NOT NULL).");
        await idxReader.CloseAsync();

        // 3. Verify HoaDon_Them stored procedure has @ClientRequestId parameter
        using var spCmd = connection.CreateCommand();
        spCmd.CommandText = @"
            SELECT COUNT(*) 
            FROM sys.parameters 
            WHERE object_id = OBJECT_ID('dbo.HoaDon_Them') AND name = '@ClientRequestId';";

        var spParamCount = Convert.ToInt32(await spCmd.ExecuteScalarAsync());
        Assert.Equal(1, spParamCount);
    }

    [Fact]
    public async Task LiveSqlServer_TenConcurrentRequests_SameClientRequestId_AllResolveToSingleInvoice()
    {
        var connStr = GetTargetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connStr)
            .Options;

        using var testContext = new AppDbContext(options);
        if (!await testContext.Database.CanConnectAsync())
        {
            var isStrict = string.Equals(Environment.GetEnvironmentVariable("REQUIRE_SQLSERVER_TEST"), "true", StringComparison.OrdinalIgnoreCase);
            if (isStrict)
                Assert.Fail("Live SQL Server is unreachable in strict CI mode.");
            return;
        }

        // Ensure employee NV01 is checked in on SQL Server
        await testContext.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM dbo.ChamCong WHERE MaNV = 'NV01' AND TrangThai = N'Đang làm' AND ThoiGianCheckOut IS NULL)
            BEGIN
                INSERT INTO dbo.ChamCong (MaNV, ThoiGianCheckIn, TrangThai, DonGiaGio)
                VALUES ('NV01', SYSDATETIME(), N'Đang làm', 25000);
            END");

        var sharedToken = Guid.NewGuid().ToString("D");

        // Simulate 10 simultaneous HTTP requests hitting SQL Server through separate DbContext & connection instances
        var tasks = Enumerable.Range(1, 10).Select(async _ =>
        {
            await using var reqContext = new AppDbContext(options);
            var reqService = new OrderService(reqContext);

            var model = new OrderCreateViewModel
            {
                ClientRequestId = sharedToken,
                HinhThuc = OrderWorkflowConstants.HinhThucMangVe,
                GhiChu = "Cà phê mang đi gấp",
                Items = new List<OrderItemViewModel>
                {
                    new() { MaSP = "SP01", SoLuong = 1 }
                }
            };

            return await reqService.CreateOrderAsync(model, "NV01");
        }).ToList();

        var results = await Task.WhenAll(tasks);

        // 1. All 10 concurrent requests must complete successfully and receive the exact same invoice ID
        var winnerId = results[0];
        Assert.False(string.IsNullOrWhiteSpace(winnerId));
        Assert.All(results, id => Assert.Equal(winnerId, id));

        // 2. Direct physical verification against SQL Server database
        await using var verifyContext = new AppDbContext(options);
        var matchingInvoices = await verifyContext.HoaDons
            .AsNoTracking()
            .Where(x => x.ClientRequestId == sharedToken)
            .ToListAsync();

        Assert.Single(matchingInvoices);
        Assert.Equal(winnerId, matchingInvoices[0].MaHD);
        Assert.Equal("Cà phê mang đi gấp", matchingInvoices[0].GhiChu);
    }

    [Fact]
    public async Task LiveSqlServer_TenConcurrentRequests_DifferentClientRequestId_AllCreateDistinctInvoices()
    {
        var connStr = GetTargetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connStr)
            .Options;

        using var testContext = new AppDbContext(options);
        if (!await testContext.Database.CanConnectAsync())
        {
            var isStrict = string.Equals(Environment.GetEnvironmentVariable("REQUIRE_SQLSERVER_TEST"), "true", StringComparison.OrdinalIgnoreCase);
            if (isStrict)
                Assert.Fail("Live SQL Server is unreachable in strict CI mode.");
            return;
        }

        // Ensure employee NV01 is checked in
        await testContext.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM dbo.ChamCong WHERE MaNV = 'NV01' AND TrangThai = N'Đang làm' AND ThoiGianCheckOut IS NULL)
            BEGIN
                INSERT INTO dbo.ChamCong (MaNV, ThoiGianCheckIn, TrangThai, DonGiaGio)
                VALUES ('NV01', SYSDATETIME(), N'Đang làm', 25000);
            END");

        var tokenList = Enumerable.Range(1, 10).Select(_ => Guid.NewGuid().ToString("D")).ToList();

        // 10 concurrent requests each with distinct ClientRequestId
        var tasks = tokenList.Select(async token =>
        {
            await using var reqContext = new AppDbContext(options);
            var reqService = new OrderService(reqContext);

            var model = new OrderCreateViewModel
            {
                ClientRequestId = token,
                HinhThuc = OrderWorkflowConstants.HinhThucMangVe,
                GhiChu = $"Order distinct {token[..8]}",
                Items = new List<OrderItemViewModel>
                {
                    new() { MaSP = "SP01", SoLuong = 1 }
                }
            };

            return await reqService.CreateOrderAsync(model, "NV01");
        }).ToList();

        var results = await Task.WhenAll(tasks);

        // 1. All 10 requests must produce distinct non-colliding order numbers
        Assert.Equal(10, results.Distinct().Count());

        // 2. Direct physical verification against SQL Server database
        await using var verifyContext = new AppDbContext(options);
        var createdCount = await verifyContext.HoaDons
            .AsNoTracking()
            .CountAsync(x => tokenList.Contains(x.ClientRequestId!));

        Assert.Equal(10, createdCount);
    }
    #endregion

    #region 6. Tier 3 Unique Index Exception Parsing Tests
    [Fact]
    public void IsUniqueIndexViolation_WhenMessageContainsIndexName_ReturnsTrue()
    {
        var ex = new Exception("Violation of UNIQUE KEY constraint 'UX_HoaDon_ClientRequestId'. Cannot insert duplicate key in object 'dbo.HoaDon'.");
        Assert.True(OrderService.IsUniqueIndexViolation(ex));
    }

    [Fact]
    public void IsUniqueIndexViolation_WhenWrappedInDbUpdateException_ReturnsTrue()
    {
        var inner = new Exception("Cannot insert duplicate key row in object 'dbo.HoaDon' with unique index 'UX_HoaDon_ClientRequestId'.");
        var dbEx = new DbUpdateException("An error occurred while saving the entity changes.", inner);

        Assert.True(OrderService.IsUniqueIndexViolation(dbEx));
    }

    [Fact]
    public void IsUniqueIndexViolation_WhenUnrelatedException_ReturnsFalse()
    {
        var unrelated = new InvalidOperationException("Connection timeout expired.");
        Assert.False(OrderService.IsUniqueIndexViolation(unrelated));
    }
    #endregion
}
