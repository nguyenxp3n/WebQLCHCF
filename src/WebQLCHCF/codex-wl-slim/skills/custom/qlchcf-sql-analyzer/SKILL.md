---
name: qlchcf-sql-analyzer
description: Analyze QLCHCF_Full.sql and extract tables, columns, keys, computed columns, relationships, and stored procedures for the ASP.NET Core MVC project.
---

# QLCHCF SQL Analyzer

## Use when

Use this skill before generating any C# code.

## Tasks

1. Read `QLCHCF_Full.sql` completely.
2. Extract all 14 tables.
3. Extract columns and SQL types.
4. Extract primary keys.
5. Extract foreign keys.
6. Extract computed columns.
7. Extract stored procedures.
8. Identify procedures that return joined data and need keyless ViewModels.

## Output

Return:

- Table summary
- Model mapping plan
- DbContext configuration plan
- Controller generation plan
- Special cases: CTHD, CTPN, CongThuc, HoaDon, PhieuNhap

## Rules

- Do not invent schema.
- Do not rename SQL objects.
- Use SQL file as the single source of truth.
