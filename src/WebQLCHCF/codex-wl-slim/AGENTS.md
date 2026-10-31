# AGENTS.md - QLCHCF ASP.NET Core MVC

## Vai trò của AI Agent

Bạn là AI Agent lập trình C# ASP.NET Core MVC. Nhiệm vụ là build project quản lý quán cà phê `WebQLCHCF` dựa trên SQL Server database `QLCHCF` từ file `QLCHCF_Full.sql`.

## Nguyên tắc tuyệt đối

- Bám sát 100% file SQL.
- Không tự ý đổi tên database, bảng, cột, khóa chính, khóa ngoại, stored procedure.
- Không tự tạo chức năng ngoài SQL nếu chưa được yêu cầu.
- Không dùng ASP.NET Identity nếu bảng `TaiKhoan` và stored procedure `TaiKhoan_DangNhap` đã đủ cho bài.
- Không dùng raw SQL nối chuỗi từ input người dùng.
- Ưu tiên gọi stored procedure có sẵn bằng EF Core.
- Không tạo pseudo-code, TODO hoặc file rỗng.
- Build project phải chạy được trong Visual Studio.

## Tech stack bắt buộc

- ASP.NET Core MVC
- C#
- SQL Server
- Entity Framework Core
- Razor Views
- Bootstrap 5
- Session
- Stored Procedure

## Database

Database: `QLCHCF`

Bảng bắt buộc:

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

## Thứ tự làm việc

1. Đọc `QLCHCF_Full.sql`.
2. Lập danh sách bảng, cột, khóa chính, khóa ngoại, stored procedure.
3. Tạo Models đúng schema.
4. Tạo ViewModels chỉ khi stored procedure trả dữ liệu join/thống kê.
5. Tạo `AppDbContext`.
6. Tạo `Program.cs`, `appsettings.json`.
7. Tạo Login/Logout bằng `TaiKhoan_DangNhap`.
8. Tạo CRUD theo stored procedure.
9. Tạo xử lý khóa chính kép cho `CTHD`, `CTPN`, `CongThuc`.
10. Tạo Views Bootstrap 5.
11. Build và sửa lỗi.

## QLCHCF Scope Lock

This skill must follow the existing QLCHCF_Full.sql database 100%.
Do not rename tables, columns, primary keys, foreign keys, or stored procedures.
Do not introduce Clean Architecture, Repository Pattern, Docker, CI/CD, Web API, Blazor, React, or ASP.NET Identity unless explicitly requested.
Use this skill only to support ASP.NET Core MVC + EF Core + SQL Server stored procedure implementation.
