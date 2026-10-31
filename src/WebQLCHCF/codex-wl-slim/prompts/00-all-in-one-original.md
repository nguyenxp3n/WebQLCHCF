# ALL-IN-ONE PROMPT - QLCHCF ASP.NET CORE MVC

> Dùng prompt này cho AI Agent/Cursor/Copilot/Codex để tự động hoàn thành project **QLCHCF - Quản lý quán cà phê** theo database mới `QLCHCF_Full.sql`.

---

```text
Bạn là AI Agent lập trình C# ASP.NET Core MVC chuyên nghiệp.

Nhiệm vụ của bạn là hoàn thành toàn bộ project quản lý quán cà phê QLCHCF dựa trên file database mới: QLCHCF_Full.sql.

====================================================================
MỤC TIÊU CUỐI CÙNG
====================================================================

Tạo một project ASP.NET Core MVC hoàn chỉnh, chạy được trong Visual Studio, có đầy đủ:

1. Models
2. ViewModels
3. Controllers
4. Views
5. appsettings.json
6. Program.cs
7. AppDbContext dùng Entity Framework Core
8. Login / Logout
9. Session
10. Phân quyền theo vai trò
11. CRUD bằng stored procedure thông qua EF Core
12. Xử lý hóa đơn và chi tiết hóa đơn
13. Xử lý phiếu nhập và chi tiết phiếu nhập
14. Xử lý công thức khóa chính kép
15. Thống kê
16. Layout Bootstrap 5
17. README.md
18. Kiểm tra build và sửa lỗi cuối cùng

Project phải chạy được thật, không viết pseudo-code, không để TODO, không tạo file rỗng.

====================================================================
THÔNG TIN DATABASE
====================================================================

Database:
- Tên database: QLCHCF
- File SQL: QLCHCF_Full.sql
- SQL Server
- Không tự ý đổi tên database, bảng, cột, khóa chính, khóa ngoại hoặc stored procedure.
- Ưu tiên dùng stored procedure có sẵn trong database.
- Không dùng raw SQL SELECT trực tiếp nếu đã có stored procedure tương ứng.

Database có 14 bảng:

1. Ban
2. KhachHang
3. KhuyenMai
4. LoaiSP
5. Luong
6. NguyenLieu
7. NhanVien
8. SanPham
9. HoaDon
10. PhieuNhap
11. TaiKhoan
12. CTHD
13. CTPN
14. CongThuc

====================================================================
YÊU CẦU CÔNG NGHỆ
====================================================================

Sử dụng:

- ASP.NET Core MVC
- C#
- SQL Server
- Razor View
- Bootstrap 5
- Entity Framework Core
- Session
- Stored Procedure

Các package cần có:

- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools

Không dùng ASP.NET Identity nếu quá phức tạp. Login dùng bảng TaiKhoan và stored procedure TaiKhoan_DangNhap.

====================================================================
CẤU TRÚC PROJECT CẦN TẠO
====================================================================

QLCHCF/
│
├── Controllers/
│   ├── AccountController.cs
│   ├── HomeController.cs
│   ├── BanController.cs
│   ├── KhachHangController.cs
│   ├── KhuyenMaiController.cs
│   ├── LoaiSPController.cs
│   ├── LuongController.cs
│   ├── NguyenLieuController.cs
│   ├── NhanVienController.cs
│   ├── SanPhamController.cs
│   ├── HoaDonController.cs
│   ├── PhieuNhapController.cs
│   ├── CongThucController.cs
│   ├── TaiKhoanController.cs
│   └── ThongKeController.cs
│
├── Models/
│   ├── Ban.cs
│   ├── KhachHang.cs
│   ├── KhuyenMai.cs
│   ├── LoaiSP.cs
│   ├── Luong.cs
│   ├── NguyenLieu.cs
│   ├── NhanVien.cs
│   ├── SanPham.cs
│   ├── HoaDon.cs
│   ├── CTHD.cs
│   ├── PhieuNhap.cs
│   ├── CTPN.cs
│   ├── CongThuc.cs
│   └── TaiKhoan.cs
│
├── ViewModels/
│   ├── LoginViewModel.cs
│   ├── SanPhamViewModel.cs
│   ├── NhanVienViewModel.cs
│   ├── HoaDonViewModel.cs
│   ├── HoaDonDetailViewModel.cs
│   ├── CTHDViewModel.cs
│   ├── PhieuNhapViewModel.cs
│   ├── PhieuNhapDetailViewModel.cs
│   ├── CTPNViewModel.cs
│   ├── CongThucViewModel.cs
│   ├── DoanhThuTheoNgayViewModel.cs
│   ├── SanPhamBanChayViewModel.cs
│   └── NhapHangTheoThangViewModel.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── Filters/
│   └── AuthFilter.cs
│
├── Views/
│   ├── Account/
│   ├── Home/
│   ├── Ban/
│   ├── KhachHang/
│   ├── KhuyenMai/
│   ├── LoaiSP/
│   ├── Luong/
│   ├── NguyenLieu/
│   ├── NhanVien/
│   ├── SanPham/
│   ├── HoaDon/
│   ├── PhieuNhap/
│   ├── CongThuc/
│   ├── TaiKhoan/
│   ├── ThongKe/
│   └── Shared/
│       ├── _Layout.cshtml
│       ├── _ValidationScriptsPartial.cshtml
│       └── Error.cshtml
│
├── wwwroot/
├── appsettings.json
├── Program.cs
└── README.md

====================================================================
APPSETTINGS.JSON
====================================================================

Tạo appsettings.json như sau:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QLCHCF;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

Lưu ý quan trọng:
- Tên project trên Visual Studio là WebQLCHCF.
- Tên database trong SQL Server là QLCHCF.
- Tên connection string trong appsettings.json là DefaultConnection.
- Vì vậy trong Program.cs bắt buộc phải gọi đúng tên DefaultConnection.
- Không được gọi tên connection string cũ như:
  + DbQLBH_65132292Connection
  + QLCHCFConnection
  + WebQLCHCFConnection

Trong Program.cs phải dùng đúng:

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

Tên project WebQLCHCF không bắt buộc phải giống tên connection string.
Quan trọng là connection string phải trỏ đúng database:

Database=QLCHCF

====================================================================
PROGRAM.CS
====================================================================

Program.cs phải có:

- builder.Services.AddControllersWithViews()
- builder.Services.AddSession()
- builder.Services.AddHttpContextAccessor()
- builder.Services.AddDbContext<AppDbContext>(options =>
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")))
- app.UseStaticFiles()
- app.UseRouting()
- app.UseSession()
- app.UseAuthorization()
- route mặc định về Account/Login

Route mặc định:

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

Program.cs cần using:

using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;

Lưu ý:
- Nếu namespace project trong Visual Studio là WebQLCHCF thì phải dùng WebQLCHCF.Data.
- Nếu namespace thực tế khác, hãy đổi using theo đúng namespace project.

====================================================================
APPDBCONTEXT
====================================================================

Tạo Data/AppDbContext.cs.

Yêu cầu:

- Dùng Entity Framework Core.
- Kế thừa DbContext.
- Constructor nhận DbContextOptions<AppDbContext>.
- Khai báo DbSet cho đủ 14 bảng:
  + Ban
  + KhachHang
  + KhuyenMai
  + LoaiSP
  + Luong
  + NguyenLieu
  + NhanVien
  + SanPham
  + HoaDon
  + PhieuNhap
  + TaiKhoan
  + CTHD
  + CTPN
  + CongThuc

Trong AppDbContext phải có:

- DbSet cho từng model.
- DbSet cho các ViewModel dùng nhận kết quả từ stored procedure.
- OnModelCreating để cấu hình bảng.
- Cấu hình khóa chính đơn.
- Cấu hình khóa chính kép.
- Cấu hình khóa ngoại.
- Cấu hình decimal precision cho money/decimal.
- Cấu hình computed column:
  + Luong.TongLuong
  + CTHD.ThanhTien
  + CTPN.ThanhTien

Khóa chính kép:

- CongThuc: MaSP + MaNL
- CTHD: MaHD + MaSP
- CTPN: MaPN + MaNL

Computed column phải đúng theo SQL:

- Luong.TongLuong = LuongCoBan + ISNULL(PhuCap, 0) + ISNULL(Thuong, 0) - ISNULL(Phat, 0)
- CTHD.ThanhTien = SoLuong * DonGia
- CTPN.ThanhTien = SoLuong * DonGia

Ví dụ cấu hình TongLuong:

entity.Property(e => e.TongLuong)
    .HasComputedColumnSql("([LuongCoBan]+isnull([PhuCap],(0))+isnull([Thuong],(0))-isnull([Phat],(0)))", stored: true);

Không cho nhập trực tiếp các cột computed:

- TongLuong
- ThanhTien

Các cột tổng tiền không nhập trực tiếp:

- HoaDon.TongTien
- PhieuNhap.TongTien

Nếu gọi stored procedure bằng EF Core:

- Dùng FromSqlRaw hoặc FromSqlInterpolated cho truy vấn danh sách/chi tiết.
- Dùng Database.ExecuteSqlRaw hoặc Database.ExecuteSqlInterpolated cho thêm/sửa/xóa.
- Không dùng raw SQL SELECT trực tiếp nếu đã có stored procedure.
- Truyền parameter an toàn, không nối chuỗi trực tiếp từ input người dùng.

Ví dụ định hướng:

- _context.Bans.FromSqlInterpolated($"EXEC Ban_DS").ToListAsync()
- _context.Bans.FromSqlInterpolated($"EXEC Ban_CT {id}").FirstOrDefaultAsync()
- _context.Database.ExecuteSqlInterpolated($"EXEC Ban_Them {model.MaBan}, {model.TenBan}, {model.KhuVuc}, {model.TrangThai}")

====================================================================
CẤU HÌNH KEYLESS VIEWMODEL
====================================================================

Các ViewModel dùng để nhận kết quả từ stored procedure có JOIN hoặc trả thêm cột phải cấu hình HasNoKey() trong AppDbContext.

Bắt buộc cấu hình keyless cho:

- SanPhamViewModel
- NhanVienViewModel
- HoaDonViewModel
- HoaDonDetailViewModel
- CTHDViewModel
- PhieuNhapViewModel
- PhieuNhapDetailViewModel
- CTPNViewModel
- CongThucViewModel
- DoanhThuTheoNgayViewModel
- SanPhamBanChayViewModel
- NhapHangTheoThangViewModel

Ví dụ trong OnModelCreating:

modelBuilder.Entity<SanPhamViewModel>().HasNoKey();
modelBuilder.Entity<NhanVienViewModel>().HasNoKey();
modelBuilder.Entity<HoaDonViewModel>().HasNoKey();
modelBuilder.Entity<HoaDonDetailViewModel>().HasNoKey();
modelBuilder.Entity<CTHDViewModel>().HasNoKey();
modelBuilder.Entity<PhieuNhapViewModel>().HasNoKey();
modelBuilder.Entity<PhieuNhapDetailViewModel>().HasNoKey();
modelBuilder.Entity<CTPNViewModel>().HasNoKey();
modelBuilder.Entity<CongThucViewModel>().HasNoKey();
modelBuilder.Entity<DoanhThuTheoNgayViewModel>().HasNoKey();
modelBuilder.Entity<SanPhamBanChayViewModel>().HasNoKey();
modelBuilder.Entity<NhapHangTheoThangViewModel>().HasNoKey();

Lý do:
- SanPham_DS có thể trả TenLoaiSP.
- NhanVien_DS có thể trả thông tin lương.
- HoaDon_DS/HoaDon_CT có thể trả TenNV, TenKH, TenBan.
- PhieuNhap_DS/PhieuNhap_CT có thể trả TenNV.
- CTHD_DSTheoMaHD có thể trả TenSP, TenKM.
- CTPN_DSTheoMaPN có thể trả TenNL, DonViTinh.
- CongThuc_DS có thể trả TenSP, TenNL.
- Stored procedure thống kê không map trực tiếp vào bảng thật.

====================================================================
QUY TẮC KIỂU DỮ LIỆU MODEL
====================================================================

Quy đổi kiểu dữ liệu SQL sang C#:

- varchar / nvarchar -> string
- int -> int
- int NULL -> int?
- money -> decimal
- money NULL -> decimal?
- decimal -> decimal
- date -> DateTime
- date NULL -> DateTime?

Mỗi model cần:

- Đúng tên class.
- Đúng tên property theo cột SQL.
- Có DataAnnotations phù hợp.
- Có DisplayName tiếng Việt.
- Có Required nếu SQL là NOT NULL.
- Có StringLength đúng với varchar/nvarchar.
- Không tự ý đổi tên property khác tên cột SQL.
- Với decimal/money, dùng [Column(TypeName = "money")] hoặc cấu hình HasColumnType("money") / HasPrecision.

====================================================================
FORM CREATE / EDIT
====================================================================

Form Create/Edit tuyệt đối không được có input cho:

- Luong.TongLuong
- CTHD.ThanhTien
- CTPN.ThanhTien
- HoaDon.TongTien
- PhieuNhap.TongTien

Các trường này chỉ hiển thị ở Index/Details.

Các field nullable cần xử lý đúng:
- HoaDon.MaKH có thể NULL.
- CTHD.MaKM có thể NULL.
- NhanVien.NgaySinh có thể NULL.
- KhachHang.DiemTichLuy có thể NULL/default 0.
- NguyenLieu.SoLuongTon có thể NULL/default 0.

====================================================================
DANH SÁCH BẢNG, CỘT, LƯU Ý
====================================================================

1. Ban

Cột:
- MaBan varchar(5) PK
- TenBan nvarchar(10)
- KhuVuc nvarchar(20)
- TrangThai nvarchar(30)

Stored procedure:
- Ban_DS
- Ban_CT
- Ban_Them
- Ban_Sua
- Ban_Xoa

2. KhachHang

Cột:
- MaKH varchar(5) PK
- TenKH nvarchar(50)
- SDT varchar(10)
- DiemTichLuy int NULL DEFAULT 0

Stored procedure:
- KhachHang_DS
- KhachHang_CT
- KhachHang_Them
- KhachHang_Sua
- KhachHang_Xoa

3. KhuyenMai

Cột:
- MaKM varchar(5) PK
- TenKM nvarchar(255)
- LoaiKM nvarchar(50)
- GiaTri decimal(18,2)
- NgayBatDau date
- NgayKetThuc date
- DieuKienApDung nvarchar(500)
- TrangThai nvarchar(50)

Lưu ý:
- NgayKetThuc >= NgayBatDau
- GiaTri >= 0

Stored procedure:
- KhuyenMai_DS
- KhuyenMai_CT
- KhuyenMai_Them
- KhuyenMai_Sua
- KhuyenMai_Xoa
- KhuyenMai_DangApDung

4. LoaiSP

Cột:
- MaLoaiSP varchar(5) PK
- TenLoaiSP nvarchar(30)

Stored procedure:
- LoaiSP_DS
- LoaiSP_CT
- LoaiSP_Them
- LoaiSP_Sua
- LoaiSP_Xoa

5. Luong

Cột:
- MaLuong int PK
- Thang int
- Nam int
- LuongCoBan money
- PhuCap money NULL
- Thuong money NULL
- Phat money NULL
- TongLuong computed

Lưu ý:
- TongLuong là computed column.
- TongLuong = LuongCoBan + ISNULL(PhuCap, 0) + ISNULL(Thuong, 0) - ISNULL(Phat, 0)
- Không cho nhập TongLuong ở Create/Edit.
- Chỉ hiển thị TongLuong ở Index/Details.

Stored procedure:
- Luong_DS
- Luong_CT
- Luong_Them
- Luong_Sua
- Luong_Xoa
- Luong_TheoThangNam

6. NguyenLieu

Cột:
- MaNL varchar(5) PK
- TenNL nvarchar(50)
- DonViTinh nvarchar(20)
- SoLuongTon int NULL DEFAULT 0
- GiaNhap money
- NgayCapNhat date
- MucTonToiThieu int

Stored procedure:
- NguyenLieu_DS
- NguyenLieu_CT
- NguyenLieu_Them
- NguyenLieu_Sua
- NguyenLieu_Xoa
- NguyenLieu_SapHet

Tạo thêm action:
- SapHet

Action này gọi stored procedure NguyenLieu_SapHet.

7. NhanVien

Cột:
- MaNV varchar(5) PK
- TenNV nvarchar(50)
- GioiTinh nvarchar(5)
- NgaySinh date NULL
- SDT varchar(10)
- ChucVu nvarchar(20)
- MaLuong int FK -> Luong

Stored procedure:
- NhanVien_DS
- NhanVien_CT
- NhanVien_Them
- NhanVien_Sua
- NhanVien_Xoa

Lưu ý:
- NhanVien_DS và NhanVien_CT có thể trả thêm thông tin lương:
  + Thang
  + Nam
  + LuongCoBan
  + PhuCap
  + Thuong
  + Phat
  + TongLuong

Nên tạo NhanVienViewModel.

8. SanPham

Cột:
- MaSP varchar(5) PK
- TenSP nvarchar(50)
- DonGia money
- MaLoaiSP varchar(5) FK -> LoaiSP

Stored procedure:
- SanPham_DS
- SanPham_CT
- SanPham_Them
- SanPham_Sua
- SanPham_Xoa

Lưu ý:
- SanPham_DS có thể trả thêm TenLoaiSP.
- Nên tạo SanPhamViewModel hoặc property TenLoaiSP.

9. HoaDon

Cột:
- MaHD varchar(9) PK
- NgayLap date
- MaNV varchar(5) FK -> NhanVien
- MaKH varchar(5) NULL FK -> KhachHang
- TongTien money DEFAULT 0
- GhiChu nvarchar(100) NULL
- MaBan varchar(5) FK -> Ban

Stored procedure:
- HoaDon_DS
- HoaDon_CT
- HoaDon_Them
- HoaDon_Sua
- HoaDon_Xoa
- HoaDon_CapNhatTongTien

Lưu ý:
- Không cho nhập TongTien.
- TongTien được cập nhật qua HoaDon_CapNhatTongTien.
- HoaDon_DS và HoaDon_CT có thể trả thêm TenNV, TenKH, TenBan.
- Nên tạo HoaDonViewModel và HoaDonDetailViewModel.

10. CTHD

Cột:
- MaHD varchar(9) PK, FK -> HoaDon
- MaSP varchar(5) PK, FK -> SanPham
- SoLuong int
- DonGia money
- ThanhTien computed
- MaKM varchar(5) NULL FK -> KhuyenMai

Khóa chính kép:
- MaHD + MaSP

Stored procedure:
- CTHD_DSTheoMaHD
- CTHD_Them
- CTHD_Sua
- CTHD_Xoa

Lưu ý:
- Không cho nhập ThanhTien.
- Khi thêm/sửa/xóa CTHD, DB tự cập nhật TongTien hóa đơn.
- MaKM có thể NULL.

11. PhieuNhap

Cột:
- MaPN varchar(9) PK
- NgayNhap date
- MaNV varchar(5) FK -> NhanVien
- TongTien money DEFAULT 0

Stored procedure:
- PhieuNhap_DS
- PhieuNhap_CT
- PhieuNhap_Them
- PhieuNhap_Sua
- PhieuNhap_Xoa
- PhieuNhap_CapNhatTongTien

Lưu ý:
- Không cho nhập TongTien.
- TongTien được cập nhật qua PhieuNhap_CapNhatTongTien.

12. CTPN

Cột:
- MaPN varchar(9) PK, FK -> PhieuNhap
- MaNL varchar(5) PK, FK -> NguyenLieu
- SoLuong int
- DonGia money
- ThanhTien computed

Khóa chính kép:
- MaPN + MaNL

Stored procedure:
- CTPN_DSTheoMaPN
- CTPN_Them
- CTPN_Sua
- CTPN_Xoa

Lưu ý:
- Không cho nhập ThanhTien.
- CTPN_Them tự tăng tồn kho nguyên liệu.
- CTPN_Sua tự cập nhật tồn kho theo số lượng cũ/mới.
- CTPN_Xoa tự trừ tồn kho nhưng không để âm.

13. CongThuc

Cột:
- MaSP varchar(5) PK, FK -> SanPham
- MaNL varchar(5) PK, FK -> NguyenLieu
- DinhLuong decimal(18,3)

Khóa chính kép:
- MaSP + MaNL

Stored procedure:
- CongThuc_DS
- CongThuc_CT
- CongThuc_TheoSanPham
- CongThuc_Them
- CongThuc_Sua
- CongThuc_Xoa

Lưu ý:
- Details/Edit/Delete phải truyền cả MaSP và MaNL.
- Có dropdown chọn SanPham và NguyenLieu.
- Không xử lý như khóa chính đơn.

14. TaiKhoan

Cột:
- MaTK varchar(10) PK
- TenDangNhap varchar(50) UNIQUE
- MatKhau varchar(50)
- MaNV varchar(5) FK -> NhanVien
- VaiTro nvarchar(50)

Stored procedure:
- TaiKhoan_DS
- TaiKhoan_CT
- TaiKhoan_DangNhap
- TaiKhoan_Them
- TaiKhoan_Sua
- TaiKhoan_Xoa

Lưu ý:
- Login phải dùng TaiKhoan_DangNhap.

====================================================================
STORED PROCEDURE THỐNG KÊ
====================================================================

Tạo ThongKeController với các action:

1. DoanhThuTheoNgay
2. SanPhamBanChay
3. NhapHangTheoThang

Stored procedure:

- ThongKe_DoanhThuTheoNgay
- ThongKe_SanPhamBanChay
- ThongKe_NhapHangTheoThang

Views cần tạo:

- Views/ThongKe/DoanhThuTheoNgay.cshtml
- Views/ThongKe/SanPhamBanChay.cshtml
- Views/ThongKe/NhapHangTheoThang.cshtml

Yêu cầu:
- Có form nhập TuNgay/DenNgay cho doanh thu theo ngày.
- Có form nhập Thang/Nam cho nhập hàng theo tháng.
- Hiển thị kết quả dạng bảng Bootstrap.
- Có TempData thông báo nếu không có dữ liệu.

====================================================================
LOGIN, LOGOUT, SESSION
====================================================================

Tạo AccountController gồm:

1. Login GET
2. Login POST
3. Logout

Login dùng stored procedure:

- TaiKhoan_DangNhap

Khi login thành công, lưu Session:

- MaTK
- TenDangNhap
- MaNV
- TenNV
- VaiTro

Tài khoản mẫu:

- qly_tram / 12345678 / Quản Lý
- pc_mai / 12345678 / Pha Chế
- pc_bao / 12345678 / Pha Chế
- pv_tu / 12345678 / Phục Vụ
- pv_vy / 12345678 / Phục Vụ

Nếu login sai:
- Hiển thị thông báo lỗi.

Nếu chưa đăng nhập:
- Chuyển về Account/Login.

Sau khi login thành công:
- Redirect về Home/Index hoặc Dashboard.

====================================================================
PHÂN QUYỀN
====================================================================

Tạo AuthFilter hoặc BaseController để kiểm tra đăng nhập và phân quyền.

Vai trò:

1. Quản Lý

Được dùng tất cả chức năng:

- Ban
- KhachHang
- KhuyenMai
- LoaiSP
- Luong
- NguyenLieu
- NhanVien
- SanPham
- HoaDon
- PhieuNhap
- CongThuc
- TaiKhoan
- ThongKe

2. Pha Chế

Được phép:

- Xem SanPham
- Xem NguyenLieu
- Xem CongThuc
- Xem HoaDon
- Xem thống kê sản phẩm bán chạy nếu cần

Không được phép:

- Quản lý TaiKhoan
- Quản lý Luong
- Quản lý NhanVien
- Xóa dữ liệu quan trọng

3. Phục Vụ

Được phép:

- Xem Ban
- Xem SanPham
- Tạo HoaDon
- Xem HoaDon
- Thêm KhachHang nếu cần

Không được phép:

- TaiKhoan
- Luong
- NhanVien
- PhieuNhap
- CongThuc
- Thống kê quản trị

Nếu không đủ quyền:
- Hiển thị trang lỗi hoặc TempData["Error"].
- Không cho thực hiện action.

====================================================================
CONTROLLERS CRUD CƠ BẢN
====================================================================

Tạo CRUD controller cho các bảng:

- BanController
- KhachHangController
- KhuyenMaiController
- LoaiSPController
- LuongController
- NguyenLieuController
- NhanVienController
- SanPhamController
- TaiKhoanController

Mỗi controller cần có:

- Index
- Details
- Create GET
- Create POST
- Edit GET
- Edit POST
- Delete GET
- DeleteConfirmed POST hoặc Delete POST

Yêu cầu:

- Validate ModelState.
- Dùng try/catch.
- Gọi stored procedure thông qua AppDbContext.
- Dùng FromSqlInterpolated/FromSqlRaw cho query.
- Dùng ExecuteSqlInterpolated/ExecuteSqlRaw cho thêm/sửa/xóa.
- Có TempData["Success"].
- Có TempData["Error"].
- Khi thành công RedirectToAction("Index").
- Khi lỗi trả lại View(model).
- Không crash trắng trang.
- Có dropdown cho khóa ngoại:
  + NhanVien cần dropdown Luong.
  + SanPham cần dropdown LoaiSP.
  + TaiKhoan cần dropdown NhanVien.

====================================================================
HOADONCONTROLLER
====================================================================

Tạo HoaDonController xử lý HoaDon + CTHD.

Actions cần có:

- Index
- Details(string id)
- Create GET
- Create POST
- Edit GET
- Edit POST
- Delete GET
- DeleteConfirmed POST
- AddDetail GET
- AddDetail POST
- EditDetail GET
- EditDetail POST
- DeleteDetail GET
- DeleteDetail POST

Stored procedure cần gọi:

- HoaDon_DS
- HoaDon_CT
- HoaDon_Them
- HoaDon_Sua
- HoaDon_Xoa
- CTHD_DSTheoMaHD
- CTHD_Them
- CTHD_Sua
- CTHD_Xoa

Logic:

- Index gọi HoaDon_DS.
- Details gọi HoaDon_CT và CTHD_DSTheoMaHD.
- Create gọi HoaDon_Them.
- Edit gọi HoaDon_Sua.
- Delete gọi HoaDon_Xoa.
- AddDetail gọi CTHD_Them.
- EditDetail gọi CTHD_Sua.
- DeleteDetail gọi CTHD_Xoa.

Dropdown cần có:

- NhanVien
- KhachHang nullable
- Ban
- SanPham
- KhuyenMai nullable

Lưu ý:

- MaKH có thể NULL.
- MaKM có thể NULL.
- Không nhập TongTien.
- Không nhập ThanhTien.
- Sau khi thêm/sửa/xóa chi tiết, quay về Details của hóa đơn.
- Hóa đơn Details phải hiển thị danh sách CTHD.

Views cần tạo:

- Views/HoaDon/Index.cshtml
- Views/HoaDon/Details.cshtml
- Views/HoaDon/Create.cshtml
- Views/HoaDon/Edit.cshtml
- Views/HoaDon/Delete.cshtml
- Views/HoaDon/AddDetail.cshtml
- Views/HoaDon/EditDetail.cshtml
- Views/HoaDon/DeleteDetail.cshtml

====================================================================
PHIEUNHAPCONTROLLER
====================================================================

Tạo PhieuNhapController xử lý PhieuNhap + CTPN.

Actions cần có:

- Index
- Details(string id)
- Create GET
- Create POST
- Edit GET
- Edit POST
- Delete GET
- DeleteConfirmed POST
- AddDetail GET
- AddDetail POST
- EditDetail GET
- EditDetail POST
- DeleteDetail GET
- DeleteDetail POST

Stored procedure cần gọi:

- PhieuNhap_DS
- PhieuNhap_CT
- PhieuNhap_Them
- PhieuNhap_Sua
- PhieuNhap_Xoa
- CTPN_DSTheoMaPN
- CTPN_Them
- CTPN_Sua
- CTPN_Xoa

Logic:

- Index gọi PhieuNhap_DS.
- Details gọi PhieuNhap_CT và CTPN_DSTheoMaPN.
- Create gọi PhieuNhap_Them.
- Edit gọi PhieuNhap_Sua.
- Delete gọi PhieuNhap_Xoa.
- AddDetail gọi CTPN_Them.
- EditDetail gọi CTPN_Sua.
- DeleteDetail gọi CTPN_Xoa.

Dropdown cần có:

- NhanVien
- NguyenLieu

Lưu ý:

- Không nhập TongTien.
- Không nhập ThanhTien.
- Sau khi thêm/sửa/xóa chi tiết, quay về Details của phiếu nhập.
- PhieuNhap Details phải hiển thị danh sách CTPN.
- CTPN_Them/Sua/Xoa tự cập nhật tồn kho nguyên liệu.

Views cần tạo:

- Views/PhieuNhap/Index.cshtml
- Views/PhieuNhap/Details.cshtml
- Views/PhieuNhap/Create.cshtml
- Views/PhieuNhap/Edit.cshtml
- Views/PhieuNhap/Delete.cshtml
- Views/PhieuNhap/AddDetail.cshtml
- Views/PhieuNhap/EditDetail.cshtml
- Views/PhieuNhap/DeleteDetail.cshtml

====================================================================
CONGTHUCCONTROLLER
====================================================================

Tạo CongThucController xử lý khóa chính kép MaSP + MaNL.

Actions cần có:

- Index
- Details(string maSP, string maNL)
- Create GET
- Create POST
- Edit GET
- Edit POST
- Delete GET
- Delete POST
- TheoSanPham(string maSP)

Stored procedure cần gọi:

- CongThuc_DS
- CongThuc_CT
- CongThuc_TheoSanPham
- CongThuc_Them
- CongThuc_Sua
- CongThuc_Xoa

Lưu ý:

- Details/Edit/Delete phải nhận cả maSP và maNL.
- Create có dropdown SanPham và NguyenLieu.
- Edit không cho sửa khóa chính nếu dễ lỗi, chỉ sửa DinhLuong.
- Không xử lý CongThuc như khóa chính đơn.

Views cần tạo:

- Views/CongThuc/Index.cshtml
- Views/CongThuc/Details.cshtml
- Views/CongThuc/Create.cshtml
- Views/CongThuc/Edit.cshtml
- Views/CongThuc/Delete.cshtml
- Views/CongThuc/TheoSanPham.cshtml

====================================================================
THONGKECONTROLLER
====================================================================

Tạo ThongKeController.

Actions:

1. DoanhThuTheoNgay(DateTime? tuNgay, DateTime? denNgay)
2. SanPhamBanChay()
3. NhapHangTheoThang(int? thang, int? nam)

Stored procedure:

- ThongKe_DoanhThuTheoNgay
- ThongKe_SanPhamBanChay
- ThongKe_NhapHangTheoThang

Yêu cầu:

- Dùng ViewModel keyless để nhận dữ liệu.
- Có form filter ngày/tháng.
- Có validate input.
- Nếu chưa nhập filter thì hiển thị form trống hoặc default hợp lý.
- Hiển thị dữ liệu bằng bảng Bootstrap.

====================================================================
VIEWS
====================================================================

Mỗi module CRUD cơ bản tạo đủ:

- Index.cshtml
- Details.cshtml
- Create.cshtml
- Edit.cshtml
- Delete.cshtml

Áp dụng cho:

- Ban
- KhachHang
- KhuyenMai
- LoaiSP
- Luong
- NguyenLieu
- NhanVien
- SanPham
- TaiKhoan

Riêng HoaDon, PhieuNhap, CongThuc, ThongKe tạo views theo yêu cầu riêng ở trên.

Yêu cầu giao diện:

- Dùng Bootstrap 5.
- Có bảng dữ liệu đẹp, dễ đọc.
- Có nút Thêm mới.
- Có nút Chi tiết.
- Có nút Sửa.
- Có nút Xóa.
- Có confirm khi xóa.
- Có form nhập liệu rõ ràng.
- Có validation message.
- Có TempData alert thành công/thất bại.
- Có dropdown cho khóa ngoại.
- Có format tiền tệ.
- Có format ngày tháng.
- Không hiển thị input cho các trường computed/tổng tiền ở form Create/Edit.

====================================================================
LAYOUT
====================================================================

Tạo Views/Shared/_Layout.cshtml.

Yêu cầu:

- Dùng Bootstrap 5.
- Có navbar hoặc sidebar.
- Có menu đến các module.
- Hiển thị TenNV nếu đã đăng nhập.
- Hiển thị VaiTro nếu đã đăng nhập.
- Có nút Logout.
- Có TempData Success/Error alert.
- Ẩn/hiện menu theo vai trò nếu có thể.

Menu đề xuất:

- Dashboard
- Bàn
- Sản phẩm
- Loại sản phẩm
- Khách hàng
- Hóa đơn
- Phiếu nhập
- Nguyên liệu
- Công thức
- Khuyến mãi
- Nhân viên
- Lương
- Tài khoản
- Thống kê

====================================================================
README.MD
====================================================================

Tạo README.md gồm:

1. Tên project: QLCHCF - Quản lý quán cà phê.
2. Công nghệ sử dụng.
3. Cách chạy file QLCHCF_Full.sql.
4. Cách chỉnh connection string.
5. Cách chạy project trong Visual Studio.
6. Tài khoản đăng nhập mẫu:
   - qly_tram / 12345678
   - pc_mai / 12345678
   - pc_bao / 12345678
   - pv_tu / 12345678
   - pv_vy / 12345678
7. Danh sách chức năng đã hoàn thành.
8. Ghi chú computed column:
   - Luong.TongLuong
   - CTHD.ThanhTien
   - CTPN.ThanhTien
9. Ghi chú không nhập trực tiếp:
   - HoaDon.TongTien
   - PhieuNhap.TongTien
10. Ghi chú khóa chính kép:
   - CongThuc: MaSP + MaNL
   - CTHD: MaHD + MaSP
   - CTPN: MaPN + MaNL
11. Ghi chú ViewModels keyless HasNoKey().
12. Ghi chú stored procedure.
13. Cách xử lý lỗi thường gặp.

====================================================================
THỨ TỰ THỰC HIỆN BẮT BUỘC
====================================================================

Thực hiện theo đúng thứ tự:

Bước 1: Đọc file QLCHCF_Full.sql.
Bước 2: Phân tích bảng, cột, khóa chính, khóa ngoại, stored procedure.
Bước 3: Tạo hoặc kiểm tra project ASP.NET Core MVC.
Bước 4: Cài package Entity Framework Core cần thiết.
Bước 5: Tạo appsettings.json.
Bước 6: Tạo AppDbContext dùng Entity Framework Core.
Bước 7: Tạo Models cho 14 bảng.
Bước 8: Tạo ViewModels.
Bước 9: Tạo Program.cs.
Bước 10: Tạo AccountController Login/Logout.
Bước 11: Tạo AuthFilter hoặc BaseController kiểm tra Session/phân quyền.
Bước 12: Tạo CRUD cho bảng đơn.
Bước 13: Tạo HoaDonController xử lý HoaDon + CTHD.
Bước 14: Tạo PhieuNhapController xử lý PhieuNhap + CTPN.
Bước 15: Tạo CongThucController xử lý khóa chính kép.
Bước 16: Tạo ThongKeController.
Bước 17: Tạo toàn bộ Views.
Bước 18: Tạo _Layout.cshtml.
Bước 19: Tạo README.md.
Bước 20: Build project.
Bước 21: Sửa toàn bộ lỗi build.
Bước 22: Kiểm tra chạy login và CRUD.
Bước 23: Liệt kê kết quả cuối cùng.

====================================================================
CHECKLIST CUỐI CÙNG
====================================================================

Trước khi kết thúc, kiểm tra:

1. Namespace thống nhất.
2. AppDbContext đăng ký đúng trong Program.cs.
3. AppDbContext có đủ DbSet entity.
4. AppDbContext có đủ DbSet ViewModel cần gọi stored procedure.
5. AppDbContext cấu hình đúng khóa chính đơn.
6. AppDbContext cấu hình đúng khóa chính kép.
7. AppDbContext cấu hình đúng computed column.
8. AppDbContext cấu hình HasNoKey() cho ViewModels.
9. Models đúng kiểu dữ liệu.
10. ViewModels đúng dữ liệu trả về từ stored procedure.
11. Không nhập computed column.
12. Không nhập TongTien hóa đơn.
13. Không nhập TongTien phiếu nhập.
14. Controllers không thiếu using.
15. Views đúng model/viewmodel.
16. Stored procedure gọi đúng tên.
17. Parameter truyền đúng tên.
18. Parameter truyền đúng kiểu.
19. Session hoạt động.
20. Login hoạt động.
21. Logout hoạt động.
22. Phân quyền hoạt động.
23. HoaDon Details hiển thị CTHD.
24. PhieuNhap Details hiển thị CTPN.
25. CongThuc xử lý đúng khóa kép.
26. CTHD xử lý đúng khóa kép.
27. CTPN xử lý đúng khóa kép.
28. Dropdown khóa ngoại hoạt động.
29. TempData hiển thị thông báo.
30. Delete có confirm.
31. Bootstrap load được.
32. Không còn TODO.
33. Không còn pseudo-code.
34. Không còn file rỗng.
35. Không còn lỗi build.
36. README đầy đủ.
37. Project chạy được trong Visual Studio.

====================================================================
YÊU CẦU CHẤT LƯỢNG CODE
====================================================================

Bắt buộc:

- Code phải chạy được thật.
- Không viết pseudo-code.
- Không ghi TODO.
- Không tạo file rỗng.
- Không giải thích dài thay cho code.
- Không bỏ qua controller/view nào.
- Không tự ý đổi tên bảng/cột/stored procedure.
- Không hard-code connection string trong controller.
- Không để lỗi null reference phổ biến.
- Không để crash trắng trang.
- Có try/catch khi thao tác database.
- Có thông báo lỗi thân thiện.
- Có giao diện Bootstrap tương đối hoàn chỉnh.
- Có README.
- Có kiểm tra build.

====================================================================
KẾT QUẢ CUỐI CÙNG PHẢI TRẢ VỀ
====================================================================

Khi hoàn thành, hãy trả về:

1. Danh sách file đã tạo.
2. Danh sách file đã chỉnh sửa.
3. Các chức năng đã hoàn thành.
4. Các stored procedure đã dùng.
5. Cách chạy project.
6. Tài khoản đăng nhập mẫu.
7. Các lỗi đã phát hiện và đã sửa.
8. Các ghi chú còn lại nếu có.

Không được kết thúc khi project còn lỗi build.
Không được kết thúc bằng câu “bạn hãy tự thêm phần còn lại”.
Không được bỏ dở giữa chừng.
```
