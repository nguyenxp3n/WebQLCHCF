---
name: qlchcf-model-dbcontext-generator
description: Generate C# Models, ViewModels, and AppDbContext for QLCHCF based strictly on QLCHCF_Full.sql.
---

# QLCHCF Model and DbContext Generator

## Use when

Use after SQL analysis.

## Generate

- 14 entity Models
- ViewModels only for stored procedures with JOIN/statistics results
- Data/AppDbContext.cs

## Type mapping

- varchar/nvarchar -> string
- int -> int
- int NULL -> int?
- money -> decimal
- money NULL -> decimal?
- decimal -> decimal
- date/datetime -> DateTime
- nullable date/datetime -> DateTime?

## Required DbContext configuration

- DbSet for 14 tables
- DbSet for keyless ViewModels
- Table names
- Primary keys
- Composite keys
- Foreign keys
- Computed columns
- Decimal/money precision

## Composite keys

- CTHD: MaHD + MaSP
- CTPN: MaPN + MaNL
- CongThuc: MaSP + MaNL

## Do not

- Do not create properties not present in SQL entity models.
- Do not remove nullable behavior from SQL.
- Do not allow form input for computed columns.
