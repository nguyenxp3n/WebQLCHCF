# Database Rules

## Bám sát SQL

- Database tên `QLCHCF`.
- File nguồn: `QLCHCF_Full.sql`.
- Không đổi tên bảng/cột/stored procedure.
- Không xóa khóa chính hoặc khóa ngoại.
- Không thay đổi logic computed column.

## Bảng

Bắt buộc tạo model cho 14 bảng:

- Ban
- KhachHang
- KhuyenMai
- LoaiSP
- Luong
- NguyenLieu
- NhanVien
- SanPham
- HoaDon
- PhieuNhap
- TaiKhoan
- CTHD
- CTPN
- CongThuc

## Khóa chính kép

- CTHD: MaHD + MaSP
- CTPN: MaPN + MaNL
- CongThuc: MaSP + MaNL

## Computed column

Không nhập trực tiếp các cột:

- Luong.TongLuong
- CTHD.ThanhTien
- CTPN.ThanhTien

Không nhập trực tiếp tổng tiền:

- HoaDon.TongTien
- PhieuNhap.TongTien

## EF Core

- Query danh sách/chi tiết bằng `FromSqlInterpolated` nếu gọi stored procedure.
- Thêm/sửa/xóa bằng `Database.ExecuteSqlInterpolated`.
- Không nối chuỗi SQL từ input.
