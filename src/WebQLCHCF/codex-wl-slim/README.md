# codex-wl-slim - QLCHCF MVC Workflow

Bộ workflow tối giản để AI Agent/Codex/Cursor/Copilot build project C# ASP.NET Core MVC dựa 100% trên SQL Server database đã có: `QLCHCF_Full.sql`.

## Mục tiêu

- Bám sát 100% database `QLCHCF`.
- Không mở rộng ngoài yêu cầu SQL.
- Không đổi tên database, bảng, cột, khóa chính, khóa ngoại, stored procedure.
- Tạo project ASP.NET Core MVC chạy được với EF Core + SQL Server + Razor Views + Bootstrap 5.
- CRUD gọi stored procedure có sẵn.

## Cách dùng nhanh

1. Đặt folder này cạnh project `WebQLCHCF` và file `QLCHCF_Full.sql`.
2. Đọc `AGENTS.md` trước.
3. Dùng prompt chính: `prompts/00-build-mvc-from-sql.md`.
4. Khi cần chia nhỏ, dùng prompt trong `prompts/` theo thứ tự.

## Những gì KHÔNG có trong bản slim

- Không CI/CD.
- Không Docker.
- Không CodeQL.
- Không Dependabot.
- Không migration sang DB khác.
- Không React/Blazor/Minimal API/Web API nâng cao.
- Không mở rộng chức năng ngoài schema và stored procedure hiện có.
