# Cách dùng penpot-uiux-design cho WebQLCHCF

Dùng đầu tiên, trước khi sửa code.

## Thiết kế lại menu

Menu nên nhóm theo nghiệp vụ:

```text
Tổng quan
Bán hàng
- Order tại quầy
- Hóa đơn
- Khách hàng

Quản lý quán
- Bàn
- Sản phẩm
- Loại sản phẩm
- Khuyến mãi

Kho & pha chế
- Nguyên liệu
- Công thức
- Phiếu nhập

Nhân sự
- Nhân viên
- Tài khoản
- Chấm công
- Lương tháng

Báo cáo
- Thống kê
```

## Design system cho Bootstrap 5

Tạo biến CSS trong `site.css`:

```css
:root {
  --app-bg: #f6f7fb;
  --app-surface: #ffffff;
  --app-border: #e5e7eb;
  --app-text: #111827;
  --app-muted: #6b7280;
  --app-radius: 1rem;
  --app-sidebar-width: 260px;
}
```

## UX quan trọng của quán cà phê

- Order phải nhanh, ít bước.
- Sản phẩm `Đang bán` nổi bật.
- Bàn `Trống`, `Đang phục vụ`, `Đã đặt` phải phân biệt rõ.
- `MVE` hiển thị là "Mang về", không xử lý như bàn thường.
- Receipt/In bill phải rõ: món, số lượng, đơn giá, tổng tiền.
- Dashboard phải dùng dữ liệu động, không fake.
