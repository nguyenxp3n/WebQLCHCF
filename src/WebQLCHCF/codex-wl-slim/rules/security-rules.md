# Security Rules

## SQL Injection

- Không nối chuỗi SQL từ input.
- Dùng `FromSqlInterpolated` hoặc parameterized call.
- Dùng `ExecuteSqlInterpolated` cho stored procedure thêm/sửa/xóa.

## Login

- Login dùng bảng `TaiKhoan` và stored procedure `TaiKhoan_DangNhap`.
- Lưu session: MaTK, TenDangNhap, MaNV, TenNV, VaiTro.
- Nếu chưa login, chuyển về `Account/Login`.

## Mật khẩu

- Database hiện có `TaiKhoan.MatKhau` theo SQL.
- Không tự ý đổi schema để hash mật khẩu nếu yêu cầu là bám sát SQL.
- Có thể ghi chú trong README rằng đây là hạn chế bảo mật của bản bài tập.
