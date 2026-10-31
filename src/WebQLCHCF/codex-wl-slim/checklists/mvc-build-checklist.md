# MVC Build Checklist

## SQL

- [ ] Đã chạy được `QLCHCF_Full.sql`.
- [x] Database tên `QLCHCF`.
- [x] Có đủ 14 bảng.
- [x] Có stored procedure.

## Project

- [x] Project tên `WebQLCHCF`.
- [x] Có package EF Core SQL Server.
- [x] Có `appsettings.json`.
- [x] Connection string tên `DefaultConnection`.
- [x] `Program.cs` đăng ký MVC, Session, DbContext.

## Models/DbContext

- [x] Có đủ 14 model.
- [x] Có AppDbContext.
- [x] Có DbSet đủ 14 bảng.
- [x] Có composite key cho CTHD, CTPN, CongThuc.
- [x] Có computed column cho TongLuong/ThanhTien.
- [x] Có HasNoKey cho ViewModels.

## Controllers/Views

- [x] Có AccountController.
- [x] Có CRUD cho bảng đơn.
- [x] Có HoaDonController.
- [x] Có PhieuNhapController.
- [x] Có CongThucController.
- [x] Có ThongKeController.
- [x] Có Views tương ứng.
- [x] Có Layout Bootstrap 5.

## Build

- [x] `dotnet restore` OK.
- [x] `dotnet build` OK.
- [ ] Login chạy.
- [ ] CRUD cơ bản chạy.
