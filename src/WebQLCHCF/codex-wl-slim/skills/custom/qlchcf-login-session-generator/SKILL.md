---
name: qlchcf-login-session-generator
description: Generate AccountController, session setup, role checking, and layout login/logout display for QLCHCF using TaiKhoan_DangNhap.
---

# QLCHCF Login Session Generator

## Use when

Generate login/logout and session-based access control.

## Required

- AccountController
- Login GET
- Login POST
- Logout
- Session setup in Program.cs
- AuthFilter or BaseController
- Layout showing TenNV and VaiTro

## Login procedure

Use:

- TaiKhoan_DangNhap

## Session keys

- MaTK
- TenDangNhap
- MaNV
- TenNV
- VaiTro

## Rules

- Do not use ASP.NET Identity.
- Do not hard-code controller authorization logic without session check.
- If login fails, show friendly error.
- If not logged in, redirect to Account/Login.
