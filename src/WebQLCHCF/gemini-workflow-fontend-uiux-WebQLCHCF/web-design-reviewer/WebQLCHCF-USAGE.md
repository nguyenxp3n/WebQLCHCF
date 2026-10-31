# Cách dùng web-design-reviewer cho WebQLCHCF

Dùng sau khi AI sửa UI.

## Checklist màn hình

Kiểm tra:

```text
Login
Dashboard
Order
Receipt/In bill
Sản phẩm
Bàn
Khách hàng
Khuyến mãi
Nguyên liệu
Công thức
Hóa đơn
Phiếu nhập
Chấm công
Lương
Thống kê
```

## Breakpoint cần test

```text
1440px desktop
1366px laptop
768px tablet
375px mobile
```

## Lỗi cần bắt

```text
Sidebar che content.
Offcanvas mobile không mở/đóng.
Table tràn màn hình.
Button thao tác bị vỡ dòng.
Modal quá rộng.
Form 2 cột vỡ trên mobile.
Badge trạng thái không thống nhất.
Contrast chữ quá nhạt.
Focus input không rõ.
Receipt in bill bị mất layout.
```

## Output yêu cầu AI

Sau review, AI phải trả:

```text
- Danh sách lỗi tìm thấy.
- File đã sửa.
- Lý do sửa.
- Cách kiểm tra lại.
```
