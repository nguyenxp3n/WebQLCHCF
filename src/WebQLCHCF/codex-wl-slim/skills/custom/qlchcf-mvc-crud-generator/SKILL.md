---
name: qlchcf-mvc-crud-generator
description: Generate ASP.NET Core MVC controllers and Razor CRUD views for simple QLCHCF tables using stored procedures.
---

# QLCHCF MVC CRUD Generator

## Use when

Generate CRUD for simple tables:

- Ban
- KhachHang
- KhuyenMai
- LoaiSP
- Luong
- NguyenLieu
- NhanVien
- SanPham
- TaiKhoan

## Each controller must include

- Index
- Details
- Create GET/POST
- Edit GET/POST
- Delete GET/POST

## Stored procedure rules

- Index uses `_DS`
- Details uses `_CT`
- Create uses `_Them`
- Edit uses `_Sua`
- Delete uses `_Xoa`

## View rules

- Bootstrap 5 table/forms
- Validation messages
- TempData Success/Error
- Dropdown for foreign keys
- No input for computed columns or totals

## Do not

- Do not use direct SELECT if stored procedure exists.
- Do not omit try/catch around database operations.
- Do not leave empty views.
