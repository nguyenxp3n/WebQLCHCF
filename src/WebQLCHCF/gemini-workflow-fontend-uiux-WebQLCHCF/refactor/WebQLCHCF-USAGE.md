# Cách dùng refactor cho WebQLCHCF

Dùng sau khi giao diện đã được redesign cơ bản.

## Có thể tách partial

```text
Views/Shared/Partials/_Sidebar.cshtml
Views/Shared/Partials/_Topbar.cshtml
Views/Shared/Partials/_UserMenu.cshtml
Views/Shared/Partials/_PageHeader.cshtml
Views/Shared/Partials/_ToastMessages.cshtml
Views/Shared/Partials/_TableActions.cshtml
```

## Có thể tách CSS

```text
wwwroot/css/site.css
wwwroot/css/layout.css
wwwroot/css/components.css
wwwroot/css/pages-dashboard.css
wwwroot/css/pages-order.css
wwwroot/css/pages-print.css
```

## Không được đổi

```text
- Controller/action.
- asp-action.
- asp-controller.
- asp-route-*.
- asp-for.
- name attribute quan trọng trong form.
- hidden input dùng cho POST.
- session key.
- stored procedure call.
```

## Refactor tốt cho WebQLCHCF

- Chuẩn hóa page header.
- Chuẩn hóa card.
- Chuẩn hóa table.
- Chuẩn hóa form.
- Chuẩn hóa badge trạng thái.
- Chuẩn hóa nút xem/sửa/xóa/in bill/trả bàn.
