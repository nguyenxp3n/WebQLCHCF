---
name: qlchcf-invoice-receipt-generator
description: Generate HoaDon/CTHD and PhieuNhap/CTPN MVC controllers and views based on composite keys and stored procedures in QLCHCF.
---

# QLCHCF Invoice and Receipt Generator

## Use when

Generate:

- HoaDonController
- PhieuNhapController
- Views/HoaDon
- Views/PhieuNhap

## HoaDon actions

- Index
- Details(string id)
- Create GET/POST
- Edit GET/POST
- Delete GET/POST
- AddDetail GET/POST
- EditDetail GET/POST
- DeleteDetail GET/POST

## HoaDon procedures

- HoaDon_DS
- HoaDon_CT
- HoaDon_Them
- HoaDon_Sua
- HoaDon_Xoa
- CTHD_DSTheoMaHD
- CTHD_Them
- CTHD_Sua
- CTHD_Xoa

## PhieuNhap actions

- Index
- Details(string id)
- Create GET/POST
- Edit GET/POST
- Delete GET/POST
- AddDetail GET/POST
- EditDetail GET/POST
- DeleteDetail GET/POST

## PhieuNhap procedures

- PhieuNhap_DS
- PhieuNhap_CT
- PhieuNhap_Them
- PhieuNhap_Sua
- PhieuNhap_Xoa
- CTPN_DSTheoMaPN
- CTPN_Them
- CTPN_Sua
- CTPN_Xoa

## Rules

- CTHD key: MaHD + MaSP
- CTPN key: MaPN + MaNL
- Do not input ThanhTien.
- Do not input TongTien.
- After detail add/edit/delete, redirect to Details of parent.
