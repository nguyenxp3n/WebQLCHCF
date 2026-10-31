# ASP.NET MVC Rules

## Cấu trúc bắt buộc

- Controllers
- Models
- ViewModels
- Data/AppDbContext.cs
- Filters hoặc BaseController nếu cần kiểm tra session
- Views
- Views/Shared/_Layout.cshtml
- appsettings.json
- Program.cs
- README.md

## Controller CRUD cơ bản

Các controller CRUD cơ bản có:

- Index
- Details
- Create GET/POST
- Edit GET/POST
- Delete GET/POST

Áp dụng cho:

- Ban
- KhachHang
- KhuyenMai
- LoaiSP
- Luong
- NguyenLieu
- NhanVien
- SanPham
- TaiKhoan

## Controller đặc biệt

- HoaDonController xử lý HoaDon + CTHD.
- PhieuNhapController xử lý PhieuNhap + CTPN.
- CongThucController xử lý khóa chính kép MaSP + MaNL.
- ThongKeController gọi stored procedure thống kê.

## View

- Dùng Razor Views.
- Dùng Bootstrap 5.
- Không dùng input cho computed column và tổng tiền.
- Dropdown cho khóa ngoại.
- Có TempData Success/Error.
