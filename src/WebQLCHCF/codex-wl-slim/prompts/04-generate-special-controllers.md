Tạo controller và views cho các module đặc biệt:

1. HoaDon + CTHD
2. PhieuNhap + CTPN
3. CongThuc khóa chính kép
4. ThongKe

Yêu cầu:

- Bám sát stored procedure trong `QLCHCF_Full.sql`.
- CTHD dùng khóa MaHD + MaSP.
- CTPN dùng khóa MaPN + MaNL.
- CongThuc dùng khóa MaSP + MaNL.
- Không nhập ThanhTien.
- Không nhập TongTien.
- Sau khi thêm/sửa/xóa chi tiết thì quay về Details của cha.
