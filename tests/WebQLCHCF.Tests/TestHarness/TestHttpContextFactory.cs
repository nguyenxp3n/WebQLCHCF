using System.Text;
using Microsoft.AspNetCore.Http;
using WebQLCHCF.Security;

namespace WebQLCHCF.Tests.TestHarness;

public class FakeSession : ISession
{
    private readonly Dictionary<string, byte[]> _storage = new(StringComparer.OrdinalIgnoreCase);

    public bool IsAvailable => true;
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public IEnumerable<string> Keys => _storage.Keys;

    public void Clear() => _storage.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _storage.Remove(key);

    public void Set(string key, byte[] value) => _storage[key] = value;

    public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out byte[]? value) =>
        _storage.TryGetValue(key, out value);

    public void SetString(string key, string value) =>
        Set(key, Encoding.UTF8.GetBytes(value));

    public string? GetString(string key) =>
        TryGetValue(key, out var bytes) ? Encoding.UTF8.GetString(bytes) : null;

    public void SetInt32(string key, int value) =>
        Set(key, BitConverter.GetBytes(value));

    public int? GetInt32(string key) =>
        TryGetValue(key, out var bytes) && bytes.Length == 4 ? BitConverter.ToInt32(bytes, 0) : null;
}

public static class TestHttpContextFactory
{
    public static DefaultHttpContext Create(
        string? role = null,
        string? maTK = null,
        string? maNV = null,
        string? tenNV = null)
    {
        var context = new DefaultHttpContext();
        var session = new FakeSession();

        if (!string.IsNullOrEmpty(maTK))
            session.SetString("MaTK", maTK);

        if (!string.IsNullOrEmpty(maNV))
            session.SetString("MaNV", maNV);

        if (!string.IsNullOrEmpty(tenNV))
            session.SetString("TenNV", tenNV);

        if (!string.IsNullOrEmpty(role))
            session.SetString("VaiTro", role);

        context.Session = session;
        return context;
    }

    public static DefaultHttpContext CreateQuanLy(
        string maTK = "TK_QL01",
        string maNV = "NV01",
        string tenNV = "Quản Lý Nguyễn") =>
        Create(AppRoles.QuanLy, maTK, maNV, tenNV);

    public static DefaultHttpContext CreatePhucVu(
        string maTK = "TK_PV01",
        string maNV = "NV02",
        string tenNV = "Phục Vụ Trần") =>
        Create(AppRoles.PhucVu, maTK, maNV, tenNV);

    public static DefaultHttpContext CreatePhaChe(
        string maTK = "TK_PC01",
        string maNV = "NV03",
        string tenNV = "Pha Chế Lê") =>
        Create(AppRoles.PhaChe, maTK, maNV, tenNV);

    public static DefaultHttpContext CreateAnonymous() =>
        Create(null, null, null, null);
}
