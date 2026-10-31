# UI Component Guide Bootstrap 5 cho WebQLCHCF

## Layout

Desktop:
```text
Sidebar 260px + Topbar 64px + Content
```

Mobile:
```text
Topbar + Offcanvas sidebar + Content full width
```

## Badge trạng thái

```text
Sản phẩm:
- Đang bán: success
- Tạm ngưng: warning
- Ngừng bán: secondary/danger

Bàn:
- Trống: success
- Đang phục vụ: primary
- Đã đặt: warning
- MVE: info

Chấm công:
- Đang làm: success
- Đã checkout: secondary
```

## Button chuẩn

```text
Thêm mới: btn-primary + bi-plus-lg
Lưu: btn-primary + bi-save
Hủy: btn-outline-secondary
Sửa: btn-outline-primary + bi-pencil
Xóa: btn-outline-danger + bi-trash
Chi tiết: btn-outline-secondary + bi-eye
In bill: btn-outline-dark + bi-printer
Trả bàn: btn-warning + bi-check2-circle
```

## Table chuẩn

- Bọc bằng `.table-responsive`.
- Dùng `.table.table-hover.align-middle`.
- Cột thao tác `.text-end`.
- Không để quá nhiều nút lớn; dùng `btn-sm`.

## Form chuẩn

- Card wrapper.
- Label rõ.
- Validation dưới input.
- `row g-3`.
- Desktop 2 cột, mobile 1 cột.
- Giữ `asp-for`, `asp-action`, `asp-controller`.

## Print receipt

- CSS `@media print`.
- Ẩn sidebar/topbar/button khi in.
- Chỉ in nội dung bill.
