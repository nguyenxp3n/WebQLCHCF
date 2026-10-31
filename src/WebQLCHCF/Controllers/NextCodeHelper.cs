using Microsoft.EntityFrameworkCore;

namespace WebQLCHCF.Controllers;

internal static class NextCodeHelper
{
    public static async Task<string> NextPrefixedCodeAsync(
        IQueryable<string> codes,
        string prefix,
        int width
    )
    {
        var values = await codes.Where(code => code.StartsWith(prefix)).ToListAsync();
        var maxNumber = values
            .Select(code =>
            {
                var suffix = code.Length > prefix.Length ? code[prefix.Length..] : "";
                return int.TryParse(suffix, out var number) ? number : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return prefix + (maxNumber + 1).ToString($"D{width}");
    }

    public static async Task<int> NextNumberAsync(IQueryable<int> numbers)
    {
        var values = await numbers.ToListAsync();
        return values.DefaultIfEmpty(0).Max() + 1;
    }

    public static Task<string> NextHoaDonAsync(IQueryable<string> codes, DateTime ngayLap)
    {
        return NextDateCodeAsync(codes, ngayLap.ToString("yyMMdd"), 3);
    }

    public static Task<string> NextPhieuNhapAsync(IQueryable<string> codes, DateTime ngayNhap)
    {
        return NextDateCodeAsync(codes, "PN" + ngayNhap.ToString("yyMM"), 3);
    }

    private static async Task<string> NextDateCodeAsync(
        IQueryable<string> codes,
        string prefix,
        int width
    )
    {
        var values = await codes.Where(code => code.StartsWith(prefix)).ToListAsync();
        var maxNumber = values
            .Select(code =>
            {
                var suffix = code.Length >= width ? code[^width..] : "";
                return int.TryParse(suffix, out var number) ? number : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return prefix + (maxNumber + 1).ToString($"D{width}");
    }
}
