# Project: WebQLCHCF Quality Assurance, Security Hardening & Defect Remediation

## Architecture
- **Web App**: ASP.NET Core (.NET 10 preview), MVC pattern, Razor views.
- **Data Access**: Entity Framework Core with SQL Server Stored Procedures (`database/init.sql`).
- **Domain Services**: `OrderService`, `ChamCongService`, `DashboardService`, `AuditLogService`.
- **Calculation Engines**: `CafeOrderCalculator`, `PayrollCalculator`, `TableWorkflowManager`, `PasswordPolicyValidator`.
- **Authorization**: Custom session-based `RoleAuthorizeAttribute` (`QuanLy`, `PhaChe`, `PhucVu`).
- **Test Suite**: xUnit, Coverlet, .NET Test SDK (`tests/WebQLCHCF.Tests/`).

## Feature Inventory
| # | Feature | Description | Milestone | Source |
|---|---------|-------------|-----------|--------|
| 1 | Customer Loyalty Points Security | Award points only on successful checkout; deduct/rollback on order cancellation | M1 | Survey Domain |
| 2 | Financial Invoice Immutability | Prevent adding, modifying, or deleting line items from paid invoices (`DaThanhToan`) | M1 | Survey Domain |
| 3 | Table State Transition Guards | Prevent setting table to 'Trống' when unpaid orders exist; enforce lifecycle | M1 | Survey Domain |
| 4 | Payroll Precision Alignment | Align C# `PayrollCalculator` with SQL exact minute calculation to eliminate drift | M1 | Survey Domain |
| 5 | Promotion Date Range Validation | Validate `NgayBatDau <= NgayKetThuc` in `KhuyenMaiController` | M1 | Survey Domain |
| 6 | Role Authorization Guards | Apply `[RoleAuthorize]` to `HoaDonController`, `BanController`, `KhachHangController`, `OrderController`, `NguyenLieuController`, and `[AllowAnonymous]` to `HomeController.Error` | M2 | Survey Security |
| 7 | HTTP Verbs & Anti-CSRF | Explicitly annotate read action methods with `[HttpGet]` and ensure anti-forgery protection | M2 | Survey Security |
| 8 | XSS Defense & Sanitization | Remove unsafe `@Html.Raw` usage in `_Layout.cshtml` and guard against over-posting | M2 | Survey Security |
| 9 | Concurrency & Table Locking | Implement pessimistic locking `WITH (UPDLOCK, HOLDLOCK)` during table assignment and check-out | M2 | Survey Security |
| 10 | Password Policy Enforcement | Enforce `PasswordPolicyValidator` in `TaiKhoanController` creation and modification | M2 | Survey Security |
| 11 | Architectural Unit Test Coverage | Add reflection tests for controller authorization, HTTP verbs, anti-CSRF, and anti-XSS | M3 | Survey Tests |
| 12 | Domain Defect Regression Tests | Add xUnit unit tests verifying all bug fixes (points on payment, paid invoice guard, payroll drift, promo validation, boundary validations) | M3 | Survey Tests |
| 13 | Final Automated Verification | Run `dotnet build` (0 warnings, 0 errors) and `dotnet test` (100% pass) | M4 | Survey Tests |
| 14 | Forensic Integrity Audit | Independent audit to verify authentic code fixes and zero cheating | M4 | Orchestrator |
| 15 | Git Delivery | Clean git tree, commit all fixes and tests, push to origin/main on GitHub | M4 | User Request |

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | Domain Business Logic Remediation | OrderService, PayrollCalculator, KhuyenMaiController, HoaDonController, BanController | Survey | COMPLETED |
| M2 | Security, Auth & Concurrency Hardening | RoleAuthorize across controllers, HttpGet annotations, _Layout XSS fix, TaiKhoan password enforcement, concurrency locks | M1 | COMPLETED |
| M3 | Test Suite Expansion | tests/WebQLCHCF.Tests/ new test classes for security guards, domain fixes, error handlers | M1, M2 | COMPLETED |
| M4 | Final Verification, Audit & Git Delivery | dotnet build, dotnet test, forensic audit, git commit & push | M3 | COMPLETED |

## Interface Contracts
### OrderService ↔ Loyalty Points
- `AddCustomerPointsAsync(string maHD, string maKH)`: Invoked only upon successful payment confirmation (`PayCashAsync`, `ConfirmQrDemoAsync`).
- `CancelOrderAsync(string maHD, string lyDo)`: Deducts/reverses any points awarded if the order was paid before cancellation.

### HoaDonController ↔ Invoice State
- `Edit`, `AddDetail`, `EditDetail`, `DeleteDetailConfirmed`: If `invoice.TrangThaiThanhToan == "DaThanhToan"`, reject request with friendly error and redirect.

### PayrollCalculator ↔ Exact Wage
- `TinhTienCongTuPhut(int tongPhut, decimal donGiaGio)`: Exact minute calculation `Math.Round((decimal)tongPhut / 60.0m * donGiaGio, 0, MidpointRounding.AwayFromZero)`.

### Role Authorization Matrix
- `HoaDonController`: `[RoleAuthorize(AppRoles.QuanLy, AppRoles.PhucVu)]`; `Delete`/`DeleteConfirmed`/`Edit`/`AddDetail`/`EditDetail`/`DeleteDetailConfirmed`: `[RoleAuthorize(AppRoles.QuanLy)]`.
- `BanController`: `[RoleAuthorize(AppRoles.QuanLy, AppRoles.PhucVu)]`; `Create`/`Edit`/`DeleteConfirmed`: `[RoleAuthorize(AppRoles.QuanLy)]`.
- `KhachHangController`: `[RoleAuthorize(AppRoles.QuanLy, AppRoles.PhucVu)]`; `DeleteConfirmed`: `[RoleAuthorize(AppRoles.QuanLy)]`.
- `NguyenLieuController`: `Create`/`Edit`/`DeleteConfirmed`: `[RoleAuthorize(AppRoles.QuanLy)]`.
- `OrderController`: `[RoleAuthorize(AppRoles.QuanLy, AppRoles.PhucVu)]`.
- `HomeController.Error`: `[AllowAnonymous]`.

## Code Layout
- `src/WebQLCHCF/Controllers/`: MVC Controllers
- `src/WebQLCHCF/Services/`: Domain engines and services
- `src/WebQLCHCF/Views/Shared/`: Shared Razor views
- `database/`: Database scripts (`init.sql`)
- `tests/WebQLCHCF.Tests/`: xUnit test projects and test classes
