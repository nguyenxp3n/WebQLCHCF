# Prompt chính - Build ASP.NET Core MVC từ SQL QLCHCF

Bạn là AI Agent lập trình C# ASP.NET Core MVC.

Nhiệm vụ: build project `WebQLCHCF` dựa 100% trên file SQL `QLCHCF_Full.sql`.

## Luật bắt buộc

- Đọc toàn bộ `QLCHCF_Full.sql` trước khi sinh code.
- Không đổi tên database, bảng, cột, khóa chính, khóa ngoại, stored procedure.
- Không mở rộng ngoài SQL.
- Không dùng ASP.NET Identity.
- Không dùng Docker, CI/CD, CodeQL, Dependabot.
- Không dùng React, Blazor, Minimal API, Web API nâng cao.
- Dùng ASP.NET Core MVC + EF Core + SQL Server + Razor Views + Bootstrap 5.
- CRUD phải gọi stored procedure có sẵn.
- Không dùng raw SQL nối chuỗi input.
- Không tạo pseudo-code, TODO hoặc file rỗng.
- Build phải chạy được.

## Database

Tên database: `QLCHCF`.

File SQL: `QLCHCF_Full.sql`.

Bảng:

1. Ban
2. KhachHang
3. KhuyenMai
4. LoaiSP
5. Luong
6. NguyenLieu
7. NhanVien
8. SanPham
9. HoaDon
10. PhieuNhap
11. TaiKhoan
12. CTHD
13. CTPN
14. CongThuc

## Công việc cần làm

1. Phân tích SQL: bảng, cột, khóa chính, khóa ngoại, computed column, stored procedure.
2. Tạo Models cho 14 bảng.
3. Tạo ViewModels chỉ khi stored procedure trả dữ liệu join/thống kê.
4. Tạo `Data/AppDbContext.cs`.
5. Tạo `appsettings.json` với connection string `DefaultConnection`.
6. Tạo `Program.cs` có MVC, Session, DbContext.
7. Tạo AccountController login/logout bằng `TaiKhoan_DangNhap`.
8. Tạo AuthFilter hoặc BaseController kiểm tra session.
9. Tạo CRUD controller + view cho bảng đơn.
10. Tạo HoaDonController xử lý HoaDon + CTHD.
11. Tạo PhieuNhapController xử lý PhieuNhap + CTPN.
12. Tạo CongThucController xử lý khóa chính kép.
13. Tạo ThongKeController gọi stored procedure thống kê.
14. Tạo Layout Bootstrap 5.
15. Tạo README.md.
16. Chạy build và sửa lỗi.

## Connection string

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=QLCHCF;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

## Program.cs bắt buộc

```csharp
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Route mặc định:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
```

## Kết quả cuối cùng cần trả về

- Danh sách file đã tạo/sửa.
- Các controller đã hoàn thành.
- Các stored procedure đã gọi.
- Cách chạy project.
- Tài khoản đăng nhập mẫu.
- Kết quả build.
- Lỗi còn lại nếu có.
