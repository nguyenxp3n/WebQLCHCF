# WebQLCHCF — Coffee Shop POS & Attendance Management System

[![CI Build & Automated Tests](https://github.com/nguyenxp3n/WebQLCHCF/actions/workflows/ci.yml/badge.svg)](https://github.com/nguyenxp3n/WebQLCHCF/actions/workflows/ci.yml)
![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)
![Database](https://img.shields.io/badge/SQL_Server-2022-red)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?logo=docker)
![Architecture](https://img.shields.io/badge/Architecture-4--Tier%20MVC-success)
![Automated Tests](https://img.shields.io/badge/xUnit-181%20Passed-brightgreen)

> **Enterprise Coursework Project:** A production-style coffee shop point-of-sale (POS) and operations platform featuring table dispatching, inventory recipes, shift attendance with concurrency control, dynamic VietQR contactless payments, and a 181-case automated test suite.

---

## 1. System Architecture (4-Tier Architecture)

```mermaid
flowchart TD
    Client["Client Layer (Web Browser)\nRazor Views • Bootstrap 5 • Responsive UI • VietQR Modal"]
    
    subgraph Presentation ["Presentation & Routing Layer"]
        Ctrl["18 MVC Controllers & Routing Engine\nAccount, Order, HoaDon, Ban, SanPham, NguyenLieu, ChamCong, Luong..."]
        AuthFilter["Security Filters\nRoleAuthorizeAttribute • AntiForgeryToken • SessionGuard"]
    end
    
    subgraph Domain ["Domain & Business Engine Layer"]
        OrderCalc["CafeOrderCalculator\nItem totals, percentage/cash discount calculations, loyalty points, VietQR URLs"]
        PayrollCalc["PayrollCalculator\nShift minute calculation, decimal hour conversion, and wage summation"]
        TableWF["TableWorkflowManager\nTable state transitions, availability guards, and cancellation rules"]
        PassPolicy["PasswordPolicyValidator\nPassword complexity policy enforcement"]
        Services["Core Services\nOrderService • ChamCongService • DashboardService • AuditLogService"]
    end
    
    subgraph Persistence ["Data & Persistence Layer"]
        EF["Entity Framework Core 10\nAppDbContext • LINQ Queries • Connection Resiliency"]
        SQL["SQL Server 2022 Database (QLCHCF)\n17 Tables • 16 Foreign Keys • Pessimistic Lock UPDLOCK, HOLDLOCK"]
    end

    Client -->|HTTP / HTTPS Requests| AuthFilter
    AuthFilter --> Ctrl
    Ctrl --> Domain
    Domain --> EF
    EF --> SQL
```

---

## 2. Entity Relationship Diagram (Mermaid ERD - 17 Tables)

```mermaid
erDiagram
    NhanVien ||--o{ ChamCong : "FK_ChamCong_NhanVien (MaNV)"
    NguyenLieu ||--o{ CongThuc : "FK_CongThuc_NguyenLieu (MaNL)"
    SanPham ||--o{ CongThuc : "FK_CongThuc_SanPham (MaSP)"
    HoaDon ||--o{ CTHD : "FK_CTHD_HoaDon (MaHD)"
    KhuyenMai ||--o{ CTHD : "FK_CTHD_KhuyenMai (MaKM)"
    SanPham ||--o{ CTHD : "FK_CTHD_SanPham (MaSP)"
    NguyenLieu ||--o{ CTPN : "FK_CTPN_NguyenLieu (MaNL)"
    PhieuNhap ||--o{ CTPN : "FK_CTPN_PhieuNhap (MaPN)"
    Ban ||--o{ HoaDon : "FK_HoaDon_Ban (MaBan)"
    KhachHang ||--o{ HoaDon : "FK_HoaDon_KhachHang (MaKH)"
    NhanVien ||--o{ HoaDon : "FK_HoaDon_NhanVien (MaNV)"
    Luong ||--o{ NhanVien : "FK_NhanVien_Luong (MaLuong)"
    NhanVien ||--o{ PhieuNhap : "FK_PhieuNhap_NhanVien (MaNV)"
    LoaiSP ||--o{ SanPham : "FK_SanPham_LoaiSP (MaLoaiSP)"
    NhanVien ||--o{ TaiKhoan : "FK_TaiKhoan_NhanVien (MaNV)"
    HoaDon ||--o{ ThanhToan : "FK_ThanhToan_HoaDon (MaHD)"

    AuditLog {
        int MaLog PK
        string ChucNang
        string HanhDong
        string DoiTuong
        string MaDoiTuong
        string NoiDung
        string NguoiThaoTac
        datetime ThoiGian
    }
    Ban {
        string MaBan PK
        string TenBan
        string KhuVuc
        string TrangThai
    }
    ChamCong {
        int MaCC PK
        string MaNV FK
        datetime ThoiGianCheckIn
        datetime ThoiGianCheckOut
        decimal TongGio
        decimal DonGiaGio
        decimal TienCong
        string TrangThai
    }
    CongThuc {
        string MaSP PK, FK
        string MaNL PK, FK
        decimal DinhLuong
        string DonViTinh
    }
    CTHD {
        string MaHD PK, FK
        string MaSP PK, FK
        int SoLuong
        decimal DonGia
        string MaKM FK
    }
    CTPN {
        string MaPN PK, FK
        string MaNL PK, FK
        decimal SoLuong
        decimal DonGia
    }
    HoaDon {
        string MaHD PK
        datetime NgayLap
        string MaNV FK
        string MaKH FK
        string MaBan FK
        decimal TongTien
        string TrangThaiOrder
        string TrangThaiThanhToan
    }
    KhachHang {
        string MaKH PK
        string TenKH
        string SDT
        int DiemTichLuy
    }
    KhuyenMai {
        string MaKM PK
        string TenKM
        string LoaiKM
        decimal GiaTri
        datetime NgayBatDau
        datetime NgayKetThuc
        string TrangThai
    }
    LoaiSP {
        string MaLoaiSP PK
        string TenLoaiSP
    }
    Luong {
        string MaLuong PK
        string TenBacLuong
        decimal MucLuongGio
    }
    NguyenLieu {
        string MaNL PK
        string TenNL
        string DonViTinh
        decimal TonKho
    }
    NhanVien {
        string MaNV PK
        string TenNV
        string GioiTinh
        datetime NgaySinh
        string SDT
        string ChucVu
        string MaLuong FK
    }
    PhieuNhap {
        string MaPN PK
        datetime NgayNhap
        string MaNV FK
        decimal TongTien
    }
    SanPham {
        string MaSP PK
        string TenSP
        decimal DonGia
        string MaLoaiSP FK
        string TrangThai
    }
    TaiKhoan {
        string MaTK PK
        string TenDangNhap
        string MatKhau
        string VaiTro
        string MaNV FK
    }
    ThanhToan {
        int MaThanhToan PK
        string MaHD FK
        string MaPhienThanhToan
        string PhuongThuc
        decimal SoTien
        string TrangThai
        datetime ThoiGianTao
    }
```

---

## 3. Engineering Highlights

1. **Role-Based Authorization Matrix:**
   * Enforces 3 distinct user roles (`Quản Lý`, `Pha Chế`, `Phục Vụ`) using `RoleAuthorizeAttribute` across controllers and views.
2. **Pessimistic Locking & Concurrency Control:**
   * Uses `WITH (UPDLOCK, HOLDLOCK)` in `ChamCong_CheckOut` stored procedure to prevent race conditions during concurrent check-out transactions.
3. **Contactless VietQR Dynamic Payment Engine:**
   * Implements standardized VietQR URL generation with exact invoice identifiers, currency rounding, and bank accounts.
4. **Isolated Business Logic Engines:**
   * Decouples calculation rules into pure domain engines: `CafeOrderCalculator`, `PayrollCalculator`, `TableWorkflowManager`, and `PasswordPolicyValidator`.
5. **High-Coverage Automated Testing:**
   * 77 xUnit test cases validating calculation boundaries, discount caps, shift intervals, authorization, and password entropy.

---

## 4. Quick Start

### Docker Compose (Recommended)

#### Windows (PowerShell):
```powershell
Copy-Item .env.example .env
docker compose up --build -d
```

#### Linux / macOS:
```bash
cp .env.example .env
docker compose up --build -d
```

Open web app at: **`http://localhost:8081`**

### Local Run:
```bash
dotnet test WebQLCHCF.slnx
dotnet run --project src/WebQLCHCF/WebQLCHCF.csproj
```

---

## 5. Seed Accounts

| Account | Password | Role | Permissions |
| :--- | :--- | :--- | :--- |
| `qly_tram` | `12345678` | **Quản Lý (Manager)** | Full management: reports, staff, payroll, promotions, audit log |
| `pc_mai` | `12345678` | **Pha Chế (Barista)** | Recipe ingredients, inventory stock, order queue |
| `pv_tu` | `12345678` | **Phục Vụ (Waiter)** | POS counter orders, table management, cash & VietQR checkout |
