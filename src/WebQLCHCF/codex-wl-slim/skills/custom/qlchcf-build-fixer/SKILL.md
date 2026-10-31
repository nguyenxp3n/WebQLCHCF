---
name: qlchcf-build-fixer
description: Build the WebQLCHCF ASP.NET Core MVC project, inspect compile errors, and fix them without changing the SQL schema.
---

# QLCHCF Build Fixer

## Use when

Use after generating or modifying code.

## Steps

1. Run `dotnet restore`.
2. Run `dotnet build`.
3. Read all compile errors.
4. Fix namespace, missing using, model mismatch, ViewModel mismatch, route/action mismatch.
5. Build again.
6. Repeat until build succeeds or report exact blocker.

## Rules

- Do not change SQL schema to hide C# errors.
- Do not remove required features just to make build pass.
- Do not leave TODO/pseudo-code.
- Keep code simple and MVC-focused.
