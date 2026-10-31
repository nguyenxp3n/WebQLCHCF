# Cách dùng webapp-testing cho WebQLCHCF

Dùng cuối cùng sau khi redesign và review.

## Tài khoản demo

```text
qly_tram / 12345678
pc_mai / 12345678
pc_bao / 12345678
pv_tu / 12345678
pv_vy / 12345678
```

## Flow test bắt buộc

```text
1. Login qly_tram.
2. Kiểm tra tự check-in.
3. Mở Dashboard.
4. Vào Order.
5. Tạo đơn ngồi tại quán.
6. Kiểm tra bàn chuyển Đang phục vụ.
7. Xem Receipt.
8. In bill.
9. Trả bàn.
10. Tạo đơn mang về MVE.
11. Kiểm tra hóa đơn.
12. Kiểm tra phiếu nhập/kho.
13. Kiểm tra chấm công.
14. Logout sai mật khẩu: không logout.
15. Logout đúng mật khẩu: checkout và xóa session.
```

## Test UI

```text
- Không có lỗi console.
- Không có broken link.
- Không có table overflow.
- Không có form submit thiếu AntiForgeryToken.
- Validation hiển thị đúng.
- Dashboard không fake data.
- Thống kê gọi dữ liệu thật.
```
