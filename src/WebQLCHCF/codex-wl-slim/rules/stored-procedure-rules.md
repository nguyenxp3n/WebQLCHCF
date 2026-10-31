# Stored Procedure Rules

## Nguyên tắc

- Nếu SQL đã có stored procedure thì controller phải ưu tiên gọi stored procedure đó.
- Không dùng raw SELECT trực tiếp nếu đã có procedure tương ứng.
- Không đổi tên procedure.

## Quy ước thường gặp

- `_DS`: danh sách
- `_CT`: chi tiết
- `_Them`: thêm
- `_Sua`: sửa
- `_Xoa`: xóa

## Ví dụ

```csharp
var data = await _context.Bans
    .FromSqlInterpolated($"EXEC Ban_DS")
    .ToListAsync();

await _context.Database.ExecuteSqlInterpolatedAsync(
    $"EXEC Ban_Them {model.MaBan}, {model.TenBan}, {model.KhuVuc}, {model.TrangThai}");
```
