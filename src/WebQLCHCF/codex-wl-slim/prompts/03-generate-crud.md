Tạo controller và Razor Views CRUD cho các bảng đơn sau:

- Ban
- KhachHang
- KhuyenMai
- LoaiSP
- Luong
- NguyenLieu
- NhanVien
- SanPham
- TaiKhoan

Yêu cầu:

- Mỗi controller có Index, Details, Create, Edit, Delete.
- Gọi stored procedure `_DS`, `_CT`, `_Them`, `_Sua`, `_Xoa`.
- Dùng FromSqlInterpolated và ExecuteSqlInterpolated.
- Có try/catch.
- Có TempData Success/Error.
- Có dropdown cho khóa ngoại.
- Views dùng Bootstrap 5.
- Không nhập computed column hoặc tổng tiền.
