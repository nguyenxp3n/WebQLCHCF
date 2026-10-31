# PROJECT_OVERVIEW - WebQLCHCF / QLCHCF

## 1. Tổng quan dự án

`WebQLCHCF` là bài nhóm môn Kiểm thử phần mềm. Dự án là website quản lý quán cà phê dùng nội bộ, phục vụ các công việc chính như đăng nhập nhân viên, order tại quầy, quản lý bàn, hóa đơn, thanh toán, khách hàng, sản phẩm, nguyên liệu, công thức pha chế, phiếu nhập, nhân viên, tài khoản, lương, thống kê và lịch sử thao tác.

Hệ thống được xây dựng theo mô hình ASP.NET Core MVC. Giao diện dùng Razor Views, Bootstrap và Bootstrap Icons. Dữ liệu lưu trong SQL Server, database chính là `QLCHCF`. File SQL chính là `QLCHCF_Full.sql`.

Kiến trúc đang dùng trong source là:

* ASP.NET Core MVC cho controller, view và routing.
* Razor Views cho giao diện web.
* Entity Framework Core SQL Server cho `AppDbContext`, model mapping và gọi stored procedure.
* SQL Server với bảng, khóa chính, khóa ngoại, check constraint, default constraint, index, function, view và stored procedure.
* Session để lưu phiên đăng nhập, mã nhân viên, tên nhân viên và vai trò.

Thanh toán QR trong code là luồng demo. `OrderService` tạo URL ảnh VietQR bằng `https://img.vietqr.io`, dùng tài khoản ngân hàng cấu hình cứng trong code và có nút xác nhận QR demo. Code không gọi cổng thanh toán thật.

## 2. Công nghệ và môi trường chạy

Project đang dùng `.NET 10.0` theo file `WebQLCHCF.csproj`:

```xml
<TargetFramework>net10.0</TargetFramework>
```

Các gói NuGet chính:

* `Microsoft.EntityFrameworkCore` version `10.0.8`.
* `Microsoft.EntityFrameworkCore.SqlServer` version `10.0.8`.
* `Microsoft.EntityFrameworkCore.Tools` version `10.0.8`.

Các thành phần chạy web:

* ASP.NET Core MVC.
* Razor Views.
* SQL Server.
* Entity Framework Core.
* Stored Procedure.
* Bootstrap từ `wwwroot/lib/bootstrap`.
* Bootstrap Icons CDN version `1.11.3`.
* jQuery và jQuery Validation từ `wwwroot/lib`.

Connection string nằm trong `appsettings.json`:

```json
"DefaultConnection": "Server=localhost;Database=QLCHCF;Trusted_Connection=True;TrustServerCertificate=True;"
```

`Program.cs` dùng đúng tên connection string `DefaultConnection`:

```csharp
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
```

Port chạy theo `Properties/launchSettings.json`:

* HTTP: `http://localhost:5132`
* HTTPS: `https://localhost:7094`

Route mặc định trong `Program.cs` đưa người dùng vào trang đăng nhập:

```csharp
{controller=Account}/{action=Login}/{id?}
```

Tài khoản mẫu có trong `QLCHCF_Full.sql`:

| Tài khoản | Mật khẩu | Nhân viên | Vai trò |
|---|---:|---|---|
| `qly_tram` | `12345678` | `NV01` | `Quản Lý` |
| `pc_mai` | `12345678` | `NV02` | `Pha Chế` |
| `pc_bao` | `12345678` | `NV03` | `Pha Chế` |
| `pv_tu` | `12345678` | `NV04` | `Phục Vụ` |
| `pv_vy` | `12345678` | `NV05` | `Phục Vụ` |

Lưu ý khi chuyển máy: connection string đang dùng `Server=localhost`, `Trusted_Connection=True` và `TrustServerCertificate=True`. Nếu máy khác dùng instance SQL Server khác như `.\SQLEXPRESS`, cần chỉnh `Server` trong `appsettings.json`.

## 3. Cách cài đặt và chạy project từ máy mới

1. Mở SQL Server Management Studio.
2. Mở file `QLCHCF_Full.sql`.
3. Chạy toàn bộ script SQL.
4. Lưu ý: script có đoạn `DROP DATABASE [QLCHCF]` rồi `CREATE DATABASE [QLCHCF]`. Nếu máy đã có database `QLCHCF`, dữ liệu cũ sẽ bị xóa khi chạy nguyên file.
5. Mở thư mục project `WebQLCHCF` bằng Visual Studio hoặc VS Code.
6. Kiểm tra `appsettings.json`, nhất là `Server=localhost` và database `QLCHCF`.
7. Kiểm tra `Program.cs`, tên connection string phải là `DefaultConnection`.
8. Chạy `dotnet restore`.
9. Chạy `dotnet build`.
10. Chạy `dotnet run`.
11. Mở `http://localhost:5132` hoặc `https://localhost:7094`.
12. Đăng nhập bằng một tài khoản mẫu trong bảng trên.

Sau khi đăng nhập, hệ thống tự check-in ca làm cho nhân viên nếu nhân viên chưa có ca `Đang làm`. Khi đăng xuất, người dùng phải nhập mật khẩu để check-out.

## 4. Cấu trúc thư mục project

```text
WebQLCHCF
├── Controllers
├── Data
├── Models
├── Security
├── Services
├── ViewModels
├── Views
│   └── Shared
├── wwwroot
│   ├── css
│   ├── js
│   └── lib
├── Program.cs
├── appsettings.json
├── WebQLCHCF.csproj
└── QLCHCF_Full.sql
```

`Controllers` chứa các controller MVC xử lý route, nhận form, gọi `AppDbContext`, gọi service và trả view. Các controller chính gồm `AccountController`, `OrderController`, `BanController`, `HoaDonController`, `SanPhamController`, `CongThucController`, `PhieuNhapController`, `ThongKeController`, `AuditLogController`.

`Data` có `AppDbContext.cs`. File này khai báo `DbSet` cho bảng thật và các viewmodel không khóa dùng để nhận kết quả stored procedure.

`Models` chứa model bám theo bảng SQL như `Ban`, `HoaDon`, `ThanhToan`, `SanPham`, `TaiKhoan`, `ChamCong`, `AuditLog`.

`Security` chứa `AppRoles` và `RoleAuthorizeAttribute`. Đây là lớp phân quyền theo session, không phải ASP.NET Identity.

`Services` chứa logic dùng chung: `OrderService`, `DashboardService`, `ChamCongService`, `AuditLogService` và các interface tương ứng.

`ViewModels` chứa dữ liệu trung gian cho login, dashboard, order, receipt, payment, chấm công, công thức, thống kê và audit log.

`Views` chứa Razor Views theo từng module. `Views/Shared/_Layout.cshtml` là layout chính có sidebar, topbar, menu theo vai trò và import CSS/JS.

`wwwroot/css/site.css` là CSS chính của giao diện. `wwwroot/js/site.js` xử lý sidebar, PJAX, submit form động, tooltip và countdown QR. `wwwroot/js/order-create.js` xử lý màn order tại quầy. `wwwroot/js/congthuc-editor.js` xử lý thêm, xóa và đánh lại index dòng công thức.

`QLCHCF_Full.sql` là script database chính. File này tạo database `QLCHCF`, tạo bảng, constraint, index, function, view, stored procedure và seed data mẫu.

## 5. Database QLCHCF

Database chính là `QLCHCF`. SQL có 17 bảng nghiệp vụ, 1 function, 1 view, nhiều index và stored procedure.

Function có trong SQL:

* `fn_NextCode`: tạo mã tiếp theo theo prefix, số lớn nhất hiện tại và số chữ số tối thiểu.

View có trong SQL:

* `vw_AuditLog_GanDay`: lấy tối đa 500 log gần nhất để xem nhanh lịch sử thao tác.

### 5.1 Danh sách bảng

### `AuditLog`

* Mục đích: lưu lịch sử thao tác quan trọng trong hệ thống.
* Khóa chính: `MaLog`.
* Khóa ngoại chính: không có FK trực tiếp trong SQL.
* Cột quan trọng: `MaLog`, `ThoiGian`, `MaNV`, `TenDangNhap`, `VaiTro`, `ChucNang`, `HanhDong`, `DoiTuong`, `MaDoiTuong`, `NoiDung`, `DuLieuTruoc`, `DuLieuSau`, `DiaChiIP`, `UserAgent`.
* Logic: `AuditLogService` ghi log qua stored procedure `AuditLog_Them`. Các thao tác tài khoản, thanh toán, hủy order, sửa order và thao tác quan trọng có thể ghi vào bảng này tùy controller hoặc service gọi.
* Default: `ThoiGian` mặc định `sysdatetime()`.
* Index: có index theo `ChucNang/HanhDong`, `DoiTuong/MaDoiTuong`, `MaNV/ThoiGian`, `ThoiGian`.

### `Ban`

* Mục đích: lưu danh sách bàn vật lý và dòng đặc biệt cho đơn mang về.
* Khóa chính: `MaBan`.
* Khóa ngoại chính: được `HoaDon.MaBan` tham chiếu.
* Cột quan trọng: `MaBan`, `TenBan`, `SoBan`, `KhuVuc`, `TrangThai`.
* Logic: `MVE` là đơn mang về, không phải bàn vật lý. Bàn vật lý chỉ thuộc `Tầng trệt` hoặc `Tầng lửng`. `MVE` có `KhuVuc = Mang về`, `SoBan = NULL`, `TrangThai = Trống`.
* Check constraint: `TrangThai` chỉ gồm `Trống` hoặc `Đang phục vụ`; `CK_Ban_KhuVuc_SoBan` ràng buộc riêng cho `MVE` và bàn vật lý.
* Stored procedure liên quan: `Ban_DS`, `Ban_CT`, `Ban_Them`, `Ban_Sua`, `Ban_Xoa`, `Ban_KhachDi`, `Ban_DongBoMaSoTen`.

### `ChamCong`

* Mục đích: lưu ca làm, thời gian check-in, check-out, số phút làm, số giờ làm và tiền công.
* Khóa chính: `MaCC`.
* Khóa ngoại chính: `MaNV` tham chiếu `NhanVien.MaNV`.
* Cột quan trọng: `MaCC`, `MaNV`, `ThoiGianCheckIn`, `ThoiGianCheckOut`, `TrangThai`, `GhiChu`, `DonGiaGio`, `TongPhut`, `TongGio`, `TienCong`.
* Logic: khi login thành công, `ChamCongService.AutoCheckInOnLoginAsync` gọi `ChamCong_CheckIn` nếu nhân viên chưa có ca `Đang làm`. Khi logout, `ChamCong_CheckOut` kiểm tra mật khẩu rồi tính công.
* Default: `ThoiGianCheckIn = sysdatetime()`, `TrangThai = Đang làm`.
* Check constraint: check-out không được nhỏ hơn check-in; đơn giá, tổng phút, tổng giờ và tiền công không âm; trạng thái gồm `Đang làm` hoặc `Đã checkout`.
* Index: `IX_ChamCong_MaNV_ThoiGianCheckIn`.

### `CongThuc`

* Mục đích: lưu định lượng nguyên liệu cho từng sản phẩm.
* Khóa chính: khóa ghép `MaSP`, `MaNL`.
* Khóa ngoại chính: `MaSP` tham chiếu `SanPham.MaSP`; `MaNL` tham chiếu `NguyenLieu.MaNL`.
* Cột quan trọng: `MaSP`, `MaNL`, `DinhLuong`.
* Logic: một sản phẩm có thể có nhiều nguyên liệu. Form công thức dùng `CongThuc_LuuNhieu` để lưu nhiều dòng bằng JSON, sau đó xóa hoặc thêm lại các dòng công thức.
* Check constraint: `DinhLuong > 0`.

### `CTHD`

* Mục đích: lưu chi tiết món trong hóa đơn.
* Khóa chính: khóa ghép `MaHD`, `MaSP`.
* Khóa ngoại chính: `MaHD` tham chiếu `HoaDon.MaHD`; `MaSP` tham chiếu `SanPham.MaSP`; `MaKM` tham chiếu `KhuyenMai.MaKM`.
* Cột quan trọng: `MaHD`, `MaSP`, `SoLuong`, `DonGia`, `ThanhTien`, `MaKM`.
* Logic: `ThanhTien` là cột computed `SoLuong * DonGia`. Khi thêm, sửa hoặc xóa chi tiết, stored procedure cập nhật lại tổng tiền hóa đơn.
* Check constraint: `SoLuong > 0`; `DonGia > 0` và không quá `10000000`.
* Index: `IX_CTHD_MaSP`.

### `CTPN`

* Mục đích: lưu chi tiết nguyên liệu trong phiếu nhập.
* Khóa chính: khóa ghép `MaPN`, `MaNL`.
* Khóa ngoại chính: `MaPN` tham chiếu `PhieuNhap.MaPN`; `MaNL` tham chiếu `NguyenLieu.MaNL`.
* Cột quan trọng: `MaPN`, `MaNL`, `SoLuong`, `DonGia`, `ThanhTien`.
* Logic: `ThanhTien` là cột computed `SoLuong * DonGia`. Khi thêm, sửa hoặc xóa chi tiết phiếu nhập, stored procedure cập nhật lại `PhieuNhap.TongTien`.
* Check constraint: `SoLuong > 0`; `DonGia >= 0` và không quá `100000000`.

### `HoaDon`

* Mục đích: lưu thông tin order và hóa đơn.
* Khóa chính: `MaHD`.
* Khóa ngoại chính: `MaNV` tham chiếu `NhanVien.MaNV`; `MaKH` tham chiếu `KhachHang.MaKH`; `MaBan` tham chiếu `Ban.MaBan`.
* Cột quan trọng: `MaHD`, `NgayLap`, `MaNV`, `MaKH`, `TongTien`, `GhiChu`, `MaBan`, `TrangThaiOrder`, `TrangThaiThanhToan`, `PhuongThucThanhToan`, `ThoiGianTao`, `ThoiGianThanhToan`.
* Logic: hóa đơn được tạo từ order tại quầy hoặc từ màn CRUD hóa đơn. Chi tiết nằm trong `CTHD`. Thanh toán nằm trong `ThanhToan`. Order mang về dùng `MaBan = MVE`.
* Default: `TongTien = 0`, `TrangThaiOrder = ChoThanhToan`, `TrangThaiThanhToan = ChuaThanhToan`, `ThoiGianTao = sysdatetime()`.
* Check constraint: tổng tiền không âm; trạng thái order gồm `DangOrder`, `ChoThanhToan`, `HoanThanh`, `DaHuy`; trạng thái thanh toán gồm `ChuaThanhToan`, `ChoThanhToanQR`, `DaThanhToan`, `HetHan`, `DaHuy`, `ChoKiemTra`; phương thức thanh toán là `TienMat` hoặc `QR`.
* Index: `IX_HoaDon_NgayLap`.

### `KhachHang`

* Mục đích: lưu khách hàng và điểm tích lũy.
* Khóa chính: `MaKH`.
* Khóa ngoại chính: được `HoaDon.MaKH` tham chiếu.
* Cột quan trọng: `MaKH`, `TenKH`, `SDT`, `DiemTichLuy`.
* Logic: order có thể không chọn khách hàng. Nếu chọn khách hàng và thanh toán thành công, code có hàm cộng điểm theo tổng tiền. Màn order có tìm khách theo số điện thoại và tạo nhanh khách hàng.
* Default: `DiemTichLuy = 0`.
* Check constraint: điểm tích lũy không âm.

### `KhuyenMai`

* Mục đích: lưu chương trình khuyến mãi.
* Khóa chính: `MaKM`.
* Khóa ngoại chính: được `CTHD.MaKM` tham chiếu.
* Cột quan trọng: `MaKM`, `TenKM`, `LoaiKM`, `GiaTri`, `NgayBatDau`, `NgayKetThuc`, `DieuKienApDung`, `TrangThai`.
* Logic: loại khuyến mãi gồm `Tiền mặt` và `Phần trăm`. `OrderService` chỉ chấp nhận khuyến mãi đang áp dụng, còn trong khoảng ngày, giá trị hợp lệ và phần trăm không quá 100.
* Check constraint: ngày kết thúc không nhỏ hơn ngày bắt đầu; giá trị không âm; loại khuyến mãi hợp lệ.

### `LoaiSP`

* Mục đích: lưu loại sản phẩm.
* Khóa chính: `MaLoaiSP`.
* Khóa ngoại chính: được `SanPham.MaLoaiSP` tham chiếu.
* Cột quan trọng: `MaLoaiSP`, `TenLoaiSP`.
* Logic: dùng để phân nhóm sản phẩm như cà phê, trà sữa, trà trái cây, matcha.

### `Luong`

* Mục đích: lưu bảng lương theo tháng, năm và các khoản cộng trừ.
* Khóa chính: `MaLuong`.
* Khóa ngoại chính: được `NhanVien.MaLuong` tham chiếu.
* Cột quan trọng: `MaLuong`, `Thang`, `Nam`, `LuongCoBan`, `PhuCap`, `Thuong`, `Phat`, `TongLuong`.
* Logic: `TongLuong` là cột computed `LuongCoBan + PhuCap + Thuong - Phat`. Module lương còn có trang lương tháng lấy dữ liệu từ chấm công.
* Default: `PhuCap`, `Thuong`, `Phat` mặc định `0`.
* Check constraint: tháng từ 1 đến 12; năm từ 2000 trở lên; các khoản tiền không âm.

### `NguyenLieu`

* Mục đích: lưu nguyên liệu pha chế và tồn kho.
* Khóa chính: `MaNL`.
* Khóa ngoại chính: được `CongThuc.MaNL` và `CTPN.MaNL` tham chiếu.
* Cột quan trọng: `MaNL`, `TenNL`, `DonViTinh`, `SoLuongTon`, `GiaNhap`, `NgayCapNhat`, `MucTonToiThieu`.
* Logic: dashboard và stored procedure có phần nguyên liệu sắp hết khi `SoLuongTon <= MucTonToiThieu`.
* Default: `SoLuongTon = 0`.
* Check constraint: tồn kho, mức tồn tối thiểu và giá nhập không âm; giá nhập không quá `100000000`.

### `NhanVien`

* Mục đích: lưu thông tin nhân viên.
* Khóa chính: `MaNV`.
* Khóa ngoại chính: `MaLuong` tham chiếu `Luong.MaLuong`.
* Cột quan trọng: `MaNV`, `TenNV`, `GioiTinh`, `NgaySinh`, `SDT`, `ChucVu`, `MaLuong`, `LuongTheoGio`.
* Logic: nhân viên liên kết với tài khoản, hóa đơn, phiếu nhập và ca chấm công. `LuongTheoGio` được dùng khi check-in và tính công theo ca.
* Default: `LuongTheoGio = 20000`.
* Check constraint: lương theo giờ không âm và không quá `1000000`.

### `PhieuNhap`

* Mục đích: lưu phiếu nhập kho.
* Khóa chính: `MaPN`.
* Khóa ngoại chính: `MaNV` tham chiếu `NhanVien.MaNV`.
* Cột quan trọng: `MaPN`, `NgayNhap`, `MaNV`, `TongTien`.
* Logic: phiếu nhập có nhiều dòng chi tiết trong `CTPN`. Tổng tiền được cập nhật lại sau thao tác trên chi tiết.
* Default: `TongTien = 0`.
* Check constraint: tổng tiền không âm.

### `SanPham`

* Mục đích: lưu sản phẩm bán tại quán.
* Khóa chính: `MaSP`.
* Khóa ngoại chính: `MaLoaiSP` tham chiếu `LoaiSP.MaLoaiSP`.
* Cột quan trọng: `MaSP`, `TenSP`, `DonGia`, `MaLoaiSP`, `TrangThai`.
* Logic: order chỉ lấy sản phẩm `Đang bán`. Sản phẩm có thể liên kết với công thức pha chế và chi tiết hóa đơn.
* Default: `TrangThai = Tạm ngưng`.
* Check constraint: đơn giá lớn hơn 0 và không quá `10000000`; trạng thái gồm `Đang bán`, `Tạm ngưng`, `Ngừng bán`.
* Index: `IX_SanPham_TrangThai`.

### `TaiKhoan`

* Mục đích: lưu tài khoản đăng nhập.
* Khóa chính: `MaTK`.
* Khóa ngoại chính: `MaNV` tham chiếu `NhanVien.MaNV`.
* Cột quan trọng: `MaTK`, `TenDangNhap`, `MatKhau`, `MaNV`, `VaiTro`.
* Logic: đăng nhập gọi `TaiKhoan_DangNhap`, sau đó lưu `MaTK`, `TenDangNhap`, `MaNV`, `TenNV`, `VaiTro` vào session. Mật khẩu trong SQL là chuỗi thường, chưa thấy hash trong code.
* Unique constraint: `TenDangNhap` là duy nhất.
* Vai trò trong code: `Quản Lý`, `Pha Chế`, `Phục Vụ`.

### `ThanhToan`

* Mục đích: lưu phiên thanh toán tiền mặt hoặc QR.
* Khóa chính: `MaThanhToan`.
* Khóa ngoại chính: `MaHD` tham chiếu `HoaDon.MaHD`.
* Cột quan trọng: `MaThanhToan`, `MaHD`, `MaPhienThanhToan`, `PhuongThuc`, `SoTien`, `NoiDungChuyenKhoan`, `MaGiaoDich`, `TrangThai`, `ThoiGianTao`, `ThoiGianHetHan`, `ThoiGianThanhToan`, `GhiChu`.
* Logic: tiền mặt thanh toán trực tiếp. QR tạo phiên có thời hạn 15 phút theo dữ liệu và ghi chú trong SQL. Code có xác nhận QR demo theo mã phiên, xử lý QR hết hạn và không cho thanh toán lặp với hóa đơn đã thanh toán.
* Default: `ThoiGianTao = sysdatetime()`.
* Check constraint: phương thức gồm `TienMat` hoặc `QR`; số tiền không âm; trạng thái gồm `ChuaThanhToan`, `ChoThanhToanQR`, `DaThanhToan`, `HetHan`, `DaHuy`, `ChoKiemTra`.
* Index: `IX_ThanhToan_MaHD`, `IX_ThanhToan_QR_DangCho`, `IX_ThanhToan_QR_HetHan`.

### 5.2 Quan hệ dữ liệu chính

* `NhanVien` - `TaiKhoan`: một nhân viên có thể gắn tài khoản đăng nhập qua `TaiKhoan.MaNV`.
* `NhanVien` - `HoaDon`: nhân viên lập hóa đơn qua `HoaDon.MaNV`.
* `NhanVien` - `PhieuNhap`: nhân viên lập phiếu nhập qua `PhieuNhap.MaNV`.
* `NhanVien` - `ChamCong`: nhân viên có nhiều ca chấm công qua `ChamCong.MaNV`.
* `Luong` - `NhanVien`: bảng lương được nhân viên tham chiếu qua `NhanVien.MaLuong`.
* `Ban` - `HoaDon`: hóa đơn gắn với bàn qua `HoaDon.MaBan`; đơn mang về dùng `MVE`.
* `KhachHang` - `HoaDon`: hóa đơn có thể có hoặc không có khách hàng qua `HoaDon.MaKH`.
* `HoaDon` - `CTHD`: hóa đơn có nhiều dòng chi tiết qua `CTHD.MaHD`.
* `HoaDon` - `ThanhToan`: hóa đơn có thể có nhiều phiên thanh toán qua `ThanhToan.MaHD`.
* `SanPham` - `CTHD`: sản phẩm xuất hiện trong chi tiết hóa đơn qua `CTHD.MaSP`.
* `SanPham` - `CongThuc`: sản phẩm có nhiều nguyên liệu trong công thức qua `CongThuc.MaSP`.
* `LoaiSP` - `SanPham`: loại sản phẩm có nhiều sản phẩm qua `SanPham.MaLoaiSP`.
* `KhuyenMai` - `CTHD`: khuyến mãi có thể gắn vào chi tiết hóa đơn qua `CTHD.MaKM`.
* `NguyenLieu` - `CongThuc`: nguyên liệu được dùng trong công thức qua `CongThuc.MaNL`.
* `PhieuNhap` - `CTPN`: phiếu nhập có nhiều dòng chi tiết qua `CTPN.MaPN`.
* `NguyenLieu` - `CTPN`: nguyên liệu xuất hiện trong chi tiết phiếu nhập qua `CTPN.MaNL`.

### 5.3 Stored Procedure quan trọng

Nhóm audit log:

* `AuditLog_DS`
* `AuditLog_Them`
* `AuditLog_TheoDoiTuong`
* `AuditLog_TheoNhanVien`

Nhóm bàn:

* `Ban_DS`
* `Ban_CT`
* `Ban_Them`
* `Ban_Sua`
* `Ban_Xoa`
* `Ban_KhachDi`
* `Ban_DongBoMaSoTen`

Nhóm chấm công:

* `ChamCong_CheckIn`
* `ChamCong_CheckOut`
* `ChamCong_DangLam`
* `ChamCong_DS`
* `ChamCong_CT`
* `ChamCong_KiemTraDangLam`
* `ChamCong_LichSuTheoNgay`
* `ChamCong_CaLamTheoThang`
* `ChamCong_TongLuongNhanVienTheoThang`
* `ChamCong_TongLuongTatCaTheoThang`

Nhóm công thức:

* `CongThuc_DS`
* `CongThuc_CT`
* `CongThuc_TheoSanPham`
* `CongThuc_Them`
* `CongThuc_Sua`
* `CongThuc_Xoa`
* `CongThuc_LuuNhieu`
* `CongThuc_XoaTheoSanPham`

Nhóm hóa đơn và chi tiết hóa đơn:

* `HoaDon_DS`
* `HoaDon_CT`
* `HoaDon_Them`
* `HoaDon_Sua`
* `HoaDon_Xoa`
* `HoaDon_CapNhatTongTien`
* `CTHD_DSTheoMaHD`
* `CTHD_Them`
* `CTHD_Sua`
* `CTHD_Xoa`

Nhóm khách hàng:

* `KhachHang_DS`
* `KhachHang_CT`
* `KhachHang_Them`
* `KhachHang_Sua`
* `KhachHang_Xoa`

Nhóm khuyến mãi:

* `KhuyenMai_DS`
* `KhuyenMai_CT`
* `KhuyenMai_DangApDung`
* `KhuyenMai_Them`
* `KhuyenMai_Sua`
* `KhuyenMai_Xoa`

Nhóm loại sản phẩm:

* `LoaiSP_DS`
* `LoaiSP_CT`
* `LoaiSP_Them`
* `LoaiSP_Sua`
* `LoaiSP_Xoa`

Nhóm lương:

* `Luong_DS`
* `Luong_CT`
* `Luong_Them`
* `Luong_Sua`
* `Luong_Xoa`
* `Luong_TheoThangNam`

Nhóm nguyên liệu:

* `NguyenLieu_DS`
* `NguyenLieu_CT`
* `NguyenLieu_SapHet`
* `NguyenLieu_Them`
* `NguyenLieu_Sua`
* `NguyenLieu_Xoa`

Nhóm nhân viên:

* `NhanVien_DS`
* `NhanVien_CT`
* `NhanVien_Them`
* `NhanVien_Sua`
* `NhanVien_Xoa`

Nhóm phiếu nhập và chi tiết phiếu nhập:

* `PhieuNhap_DS`
* `PhieuNhap_CT`
* `PhieuNhap_Them`
* `PhieuNhap_Sua`
* `PhieuNhap_Xoa`
* `PhieuNhap_CapNhatTongTien`
* `CTPN_DSTheoMaPN`
* `CTPN_Them`
* `CTPN_Sua`
* `CTPN_Xoa`

Nhóm sản phẩm:

* `SanPham_DS`
* `SanPham_CT`
* `SanPham_Them`
* `SanPham_Sua`
* `SanPham_Xoa`
* `SanPham_DoiTrangThai`

Nhóm tài khoản và đăng nhập:

* `TaiKhoan_DS`
* `TaiKhoan_CT`
* `TaiKhoan_DangNhap`
* `TaiKhoan_Them`
* `TaiKhoan_Sua`
* `TaiKhoan_Xoa`

Nhóm thanh toán:

* `ThanhToan_ChiTietTheoHoaDon`
* `ThanhToan_DanhSachChoKiemTra`
* `ThanhToan_TaoQR`
* `ThanhToan_QRHetHan`
* `ThanhToan_TienMat`
* `ThanhToan_XacNhanQRTheoPhien`
* `ThanhToan_XacNhanQRWebhook`
* `ThanhToan_XuLyTatCaQRHetHan`
* `ThanhToan_HuyOrder`
* `ThanhToan_SuaOrder`

Nhóm thống kê:

* `ThongKe_DoanhThuTheoNgay`
* `ThongKe_SanPhamBanChay`
* `ThongKe_NhapHangTheoThang`

## 6. Source ASP.NET Core MVC

### 6.1 Cấu hình trong `Program.cs`

`Program.cs` đăng ký MVC, session, `IHttpContextAccessor`, `AppDbContext` và các service:

* `IDashboardService` -> `DashboardService`
* `IOrderService` -> `OrderService`
* `IChamCongService` -> `ChamCongService`
* `IAuditLogService` -> `AuditLogService`

Ứng dụng có warm-up EF Core bằng `WarmUpEfCoreAsync`. Nếu không kết nối được database khi warm-up, app chỉ ghi warning, không dừng hẳn.

Middleware chính gồm HTTPS redirection, static files, routing, session và authorization.

### 6.2 `AppDbContext`

`AppDbContext` map 17 bảng chính bằng `DbSet`. Các bảng có khóa ghép được cấu hình trong `OnModelCreating`:

* `CTHD`: khóa ghép `MaHD`, `MaSP`.
* `CTPN`: khóa ghép `MaPN`, `MaNL`.
* `CongThuc`: khóa ghép `MaSP`, `MaNL`.

`AppDbContext` cũng khai báo nhiều `DbSet` không khóa để nhận kết quả stored procedure, ví dụ `LoginResultViewModel`, `SanPhamViewModel`, `HoaDonViewModel`, `CTHDViewModel`, `PaymentCommandResultViewModel`, `AuditLogViewModel`.

### 6.3 Phân quyền

File `Security/AppRoles.cs` định nghĩa 3 vai trò:

* `Quản Lý`
* `Pha Chế`
* `Phục Vụ`

`RoleAuthorizeAttribute` kiểm tra session:

* Nếu không có `MaTK`, chuyển về `Account/Login`.
* Nếu action hoặc controller yêu cầu role cụ thể mà session không khớp, chuyển tới `Account/AccessDenied`.
* Nếu attribute không truyền role, chỉ cần đã đăng nhập.

Đây là phân quyền tự viết bằng session. Source không dùng ASP.NET Identity.

## 7. Chức năng theo controller

### `AccountController`

Chức năng chính:

* Đăng nhập bằng `TaiKhoan_DangNhap`.
* Lưu session `MaTK`, `TenDangNhap`, `MaNV`, `TenNV`, `VaiTro`.
* Tự check-in khi đăng nhập qua `ChamCongService`.
* Đăng xuất bằng màn xác nhận mật khẩu, đồng thời check-out.
* Đổi mật khẩu với yêu cầu mật khẩu mới ít nhất 8 ký tự, có chữ số, có ký tự đặc biệt và không trùng tên đăng nhập.
* Trang `AccessDenied`.

Sau login, `Quản Lý` được chuyển tới `Home/Index`. Vai trò khác được chuyển tới `Order/Create`.

### `HomeController`

Chỉ cho `Quản Lý`. Trang dashboard lấy dữ liệu qua `DashboardService`, gồm tổng sản phẩm, sản phẩm bán được, bàn đang phục vụ, nhân viên đang làm, doanh thu hôm nay, doanh thu tháng này, nguyên liệu sắp hết, top sản phẩm bán chạy, hóa đơn gần đây và phiếu nhập gần đây.

### `OrderController`

Chức năng order tại quầy:

* `Create`: tạo order mới hoặc mở lại order để sửa bằng `editMaHD`.
* `Receipt`: xem hóa đơn sau khi tạo order.
* `CompleteTable`: trả bàn khi hóa đơn đã thanh toán và bàn vật lý còn trạng thái `Đang phục vụ`.
* `PayCash`: thanh toán tiền mặt.
* `CreateQrPayment`: tạo phiên QR demo.
* `ConfirmQrDemo`: xác nhận QR demo.
* `ExpireQr`: cho QR hết hạn thủ công.
* `CancelOrder`: hủy order với lý do.
* `EditOrder`: chuyển order về trạng thái có thể sửa.
* `SearchCustomers`: tìm khách theo số điện thoại.
* `QuickCreateCustomer`: tạo nhanh khách hàng trong màn order.

`OrderService` là nơi chứa phần lớn logic nghiệp vụ của order.

### `BanController`

Quản lý bàn. Có danh sách, chi tiết, thêm, sửa, xóa và thao tác khách đi khỏi bàn. Code chặn chỉnh sửa hoặc xóa `MVE` vì đây là dòng đặc biệt cho đơn mang về.

Khi thêm bàn, người dùng chỉ chọn khu vực. Mã bàn, số bàn và tên bàn được sinh theo logic trong stored procedure.

### `HoaDonController`

Quản lý hóa đơn và chi tiết hóa đơn theo kiểu CRUD. Controller gọi `HoaDon_DS`, `HoaDon_CT`, `HoaDon_Them`, `HoaDon_Sua`, `HoaDon_Xoa`, `CTHD_Them`, `CTHD_Sua`, `CTHD_Xoa`.

Màn này khác với màn order tại quầy. Order tại quầy nằm trong `OrderController`.

### `SanPhamController`

Quản lý sản phẩm. Có lọc theo từ khóa, loại sản phẩm và trạng thái. Code dùng tìm kiếm không dấu cho từ khóa. Sản phẩm có trạng thái `Đang bán`, `Tạm ngưng`, `Ngừng bán`.

### `LoaiSPController`

Quản lý loại sản phẩm. Các thao tác chính là danh sách, chi tiết, thêm, sửa và xóa.

### `KhachHangController`

Quản lý khách hàng. Có validation số điện thoại 10 chữ số trong model. Order cũng dùng bảng này để tìm khách và tích điểm.

### `KhuyenMaiController`

Quản lý khuyến mãi. Một số action thêm, sửa, xóa yêu cầu `Quản Lý`. Khuyến mãi có loại `Tiền mặt` hoặc `Phần trăm`, có ngày bắt đầu, ngày kết thúc và trạng thái.

### `NguyenLieuController`

Cho `Quản Lý` và `Pha Chế`. Quản lý nguyên liệu, tồn kho, giá nhập, ngày cập nhật và mức tồn tối thiểu.

### `CongThucController`

Chỉ cho `Quản Lý`. Quản lý công thức pha chế theo sản phẩm. Form tạo và sửa dùng `CongThucEditorViewModel`, danh sách nguyên liệu và mảng `Items`. Khi lưu, controller gọi `CongThuc_LuuNhieu`.

### `PhieuNhapController`

Chỉ cho `Quản Lý`. Quản lý phiếu nhập và chi tiết phiếu nhập. Khi thêm phiếu nhập có thể thêm luôn dòng nguyên liệu đầu tiên. Chi tiết phiếu nhập dùng `CTPN`.

### `NhanVienController`

Chỉ cho `Quản Lý`. Quản lý nhân viên, lương theo giờ và liên kết bảng lương.

### `TaiKhoanController`

Chỉ cho `Quản Lý`. Quản lý tài khoản đăng nhập, vai trò và nhân viên liên kết. Các thao tác tạo, sửa, xóa tài khoản có ghi audit log.

### `LuongController`

Quản lý bảng lương và trang lương tháng. Trang `Thang` dùng các stored procedure chấm công để tính tổng giờ, tổng phút, tổng lương theo nhân viên và theo tháng.

### `ThongKeController`

Chỉ cho `Quản Lý`. Có 3 trang:

* `DoanhThuTheoNgay`
* `SanPhamBanChay`
* `NhapHangTheoThang`

### `AuditLogController`

Chỉ cho `Quản Lý`. Trang danh sách có filter theo ngày, nhân viên, chức năng, hành động, đối tượng, mã đối tượng và giới hạn số dòng.

## 8. Giao diện và layout

Layout chính là `Views/Shared/_Layout.cshtml`.

Các thành phần chính:

* Sidebar bên trái.
* Topbar phía trên.
* Khu vực nội dung chính có `data-pjax-container`.
* Menu hiển thị theo vai trò.
* Badge trạng thái ca làm nếu đã đăng nhập.
* Dropdown thông tin nhân viên.
* Nút đăng xuất có ý nghĩa check-out.

Menu theo vai trò:

* `Quản Lý`: dashboard, order, hóa đơn, khách hàng, bàn, sản phẩm, loại sản phẩm, khuyến mãi, nguyên liệu, công thức, phiếu nhập, nhân viên, tài khoản, lương, thống kê, audit log, đổi mật khẩu.
* `Pha Chế`: order, hóa đơn, khách hàng, bàn, nguyên liệu, đổi mật khẩu.
* `Phục Vụ`: order, hóa đơn, khách hàng, bàn, đổi mật khẩu.

CSS chính trong `wwwroot/css/site.css` tạo giao diện màu cà phê, sidebar có chế độ thu gọn, layout responsive, card, bảng, form, trạng thái order và các phần riêng cho màn order.

## 9. JavaScript chính

### `wwwroot/js/site.js`

File này xử lý:

* Thu gọn và mở rộng sidebar.
* Lưu trạng thái sidebar vào `localStorage` bằng key `qlchcf.sidebar.collapsed`.
* Sidebar mobile.
* PJAX cho link và form.
* Bỏ qua PJAX cho một số route nhạy cảm như account, logout, checkout, complete table, delete.
* Parse lại jQuery validation sau khi PJAX đổi nội dung.
* Khởi tạo Bootstrap tooltip.
* Gọi `initOrderCreate` và `initRecipeEditor` sau khi load trang.
* Countdown QR dựa trên `data-expire-at`.

### `wwwroot/js/order-create.js`

File này xử lý màn `Order/Create`:

* Chọn món, tăng giảm số lượng, xóa món.
* Tính tạm tính, giảm giá và tổng tiền.
* Lọc sản phẩm theo từ khóa, loại sản phẩm và chỉ món đã chọn.
* Chọn hình thức `Tại quán` hoặc `Mang về`.
* Chọn khu vực `Tầng trệt` hoặc `Tầng lửng`.
* Hiển thị bàn trống dự kiến theo khu vực.
* Tìm khách hàng theo số điện thoại.
* Tạo nhanh khách hàng bằng AJAX.
* Chọn khuyến mãi cho từng dòng giỏ hàng.

### `wwwroot/js/congthuc-editor.js`

File này xử lý form công thức:

* Thêm dòng nguyên liệu.
* Xóa dòng nguyên liệu.
* Đánh lại index `Items[index].MaNL` và `Items[index].DinhLuong`.
* Dùng event delegation trên `document`, nên vẫn hoạt động sau khi PJAX thay nội dung.

## 10. Luồng nghiệp vụ quan trọng

### 10.1 Đăng nhập, check-in, đăng xuất, check-out

1. Người dùng vào `Account/Login`.
2. Nhập tài khoản và mật khẩu.
3. Controller gọi `TaiKhoan_DangNhap`.
4. Nếu đúng, session được set.
5. `ChamCongService` tự tạo ca `Đang làm` nếu nhân viên chưa có ca đang làm.
6. Khi đăng xuất, người dùng vào màn xác nhận.
7. Người dùng nhập mật khẩu.
8. `ChamCong_CheckOut` tính tổng phút, tổng giờ và tiền công.
9. Session bị xóa sau khi check-out thành công.

### 10.2 Tạo order tại quầy

1. Người dùng vào `Order/Create`.
2. Chọn hình thức `Tại quán` hoặc `Mang về`.
3. Nếu `Tại quán`, hệ thống chọn bàn trống đầu tiên theo khu vực.
4. Nếu `Mang về`, hệ thống dùng `MVE`.
5. Chọn sản phẩm và số lượng.
6. Có thể chọn khách hàng để tích điểm.
7. Có thể áp dụng khuyến mãi cho dòng chi tiết.
8. Khi submit, `OrderService` tạo `HoaDon` và các dòng `CTHD`.
9. Với bàn vật lý, trạng thái bàn được đổi sang `Đang phục vụ`.
10. Sau khi tạo xong, người dùng xem `Order/Receipt/{MaHD}`.

### 10.3 Thanh toán

Tiền mặt:

1. Người dùng bấm thanh toán tiền mặt ở receipt.
2. `OrderController.PayCash` gọi `ThanhToan_TienMat`.
3. SQL tạo bản ghi `ThanhToan`, cập nhật trạng thái hóa đơn và thời gian thanh toán.

QR demo:

1. Người dùng bấm tạo QR.
2. `OrderService.CreateQrPaymentAsync` gọi `ThanhToan_TaoQR`.
3. Code tạo URL ảnh VietQR bằng bank code `MB`, account `0332135258`, account name `HOANG CHIEM NGUYEN`.
4. Người dùng có thể bấm xác nhận QR demo.
5. `ThanhToan_XacNhanQRTheoPhien` cập nhật phiên QR và hóa đơn.

QR hết hạn:

* Có stored procedure `ThanhToan_QRHetHan` cho một hóa đơn.
* Có stored procedure `ThanhToan_XuLyTatCaQRHetHan` để xử lý nhiều QR hết hạn.
* `site.js` có countdown hiển thị thời gian còn lại nếu view có `data-expire-at`.

### 10.4 Trả bàn

Sau khi hóa đơn đã thanh toán, nếu là bàn vật lý, người dùng có thể trả bàn. Code kiểm tra bàn không còn order chưa thanh toán rồi gọi `Ban_KhachDi`.

`MVE` không cần trả bàn vì là đơn mang về.

### 10.5 Công thức pha chế

Mỗi sản phẩm có thể có nhiều nguyên liệu. Màn tạo hoặc sửa công thức dùng danh sách dòng nguyên liệu. JavaScript đánh lại index trước khi submit. Controller lưu bằng `CongThuc_LuuNhieu`, dữ liệu dòng được gửi dạng JSON.

### 10.6 Phiếu nhập kho

Phiếu nhập lưu thông tin chung trong `PhieuNhap` và dòng nguyên liệu trong `CTPN`. Logic tồn kho đã nằm trong stored procedure:

* `CTPN_Them`: thêm dòng chi tiết phiếu nhập, tăng `NguyenLieu.SoLuongTon`, cập nhật `GiaNhap`, `NgayCapNhat`, rồi gọi `PhieuNhap_CapNhatTongTien`.
* `CTPN_Sua`: lấy số lượng cũ, nếu sửa giảm thì kiểm tra kho có đủ để hoàn tác hay không, sau đó cộng hoặc trừ chênh lệch vào `NguyenLieu.SoLuongTon`, cập nhật giá nhập và ngày cập nhật.
* `CTPN_Xoa`: lấy số lượng cũ, kiểm tra kho có đủ để trừ lại hay không, sau đó trừ khỏi `NguyenLieu.SoLuongTon` và cập nhật tổng tiền phiếu nhập.

Vì vậy phiếu nhập không chỉ cập nhật tổng tiền. Nó còn cập nhật tồn kho nguyên liệu ở tầng SQL.

## 11. View và màn hình chính

Các thư mục view hiện có:

* `Views/Account`: login, logout confirm, checkout result, access denied, change password.
* `Views/Home`: dashboard và privacy.
* `Views/Order`: tạo order và receipt.
* `Views/Ban`: CRUD bàn.
* `Views/HoaDon`: CRUD hóa đơn và chi tiết hóa đơn.
* `Views/SanPham`: CRUD sản phẩm.
* `Views/LoaiSP`: CRUD loại sản phẩm.
* `Views/KhachHang`: CRUD khách hàng.
* `Views/KhuyenMai`: CRUD khuyến mãi.
* `Views/NguyenLieu`: CRUD nguyên liệu.
* `Views/CongThuc`: danh sách, tạo, sửa, chi tiết, xóa và partial editor form.
* `Views/PhieuNhap`: CRUD phiếu nhập và chi tiết phiếu nhập.
* `Views/NhanVien`: CRUD nhân viên.
* `Views/TaiKhoan`: CRUD tài khoản.
* `Views/Luong`: danh sách lương, lương tháng, CRUD lương.
* `Views/ThongKe`: doanh thu theo ngày, sản phẩm bán chạy, nhập hàng theo tháng.
* `Views/AuditLog`: danh sách lịch sử thao tác.
* `Views/Shared`: layout, error, validation scripts.

Không thấy `ThanhToanController` riêng trong source. Thanh toán nằm trong `OrderController`, `OrderService` và stored procedure nhóm `ThanhToan_*`.

## 12. Dữ liệu mẫu trong SQL

SQL có seed data cho các nhóm sau:

* Bàn: `BAN01` đến `BAN05` và `MVE`.
* Nhân viên: `NV01` đến `NV05`.
* Tài khoản: `TK01` đến `TK05`.
* Loại sản phẩm: `L01` đến `L04`.
* Sản phẩm: `SP01` đến `SP20`.
* Nguyên liệu: `NL01` đến `NL08`.
* Khách hàng: `KH01` đến `KH09`.
* Khuyến mãi: `KM01`, `KM02`, `KM03`.
* Lương tháng 5/2026.
* Một số hóa đơn, chi tiết hóa đơn, thanh toán, công thức, phiếu nhập, chi tiết phiếu nhập và chấm công mẫu.

Một điểm cần chú ý khi demo: dữ liệu mẫu có `ChamCong` mã `6` của `NV01` đang `Đang làm`. Nếu chạy lại SQL, trạng thái ca làm mẫu quay về đúng dữ liệu trong file.

## 13. Điểm cần kiểm tra khi test web

Các URL nên mở để kiểm tra nhanh:

* `http://localhost:5132/Account/Login`
* `http://localhost:5132/Home`
* `http://localhost:5132/Order/Create`
* `http://localhost:5132/HoaDon`
* `http://localhost:5132/Ban`
* `http://localhost:5132/SanPham`
* `http://localhost:5132/CongThuc`
* `http://localhost:5132/PhieuNhap`
* `http://localhost:5132/Luong/Thang`
* `http://localhost:5132/ThongKe/DoanhThuTheoNgay`
* `http://localhost:5132/AuditLog`

Các bước smoke test cơ bản:

1. Login bằng `qly_tram` và mật khẩu `12345678`.
2. Kiểm tra dashboard có mở được không.
3. Vào `Order/Create`, chọn món, chọn `Tại quán` hoặc `Mang về`, tạo order.
4. Vào receipt, thử thanh toán tiền mặt hoặc tạo QR demo.
5. Kiểm tra menu theo vai trò bằng tài khoản `pc_mai` hoặc `pv_tu`.
6. Kiểm tra logout yêu cầu nhập mật khẩu để check-out.

Các bước này dùng để kiểm tra nhanh app sau khi dựng máy mới. Nếu kiểm thử chính thức cho báo cáo, cần đối chiếu thêm database sau thao tác, nhất là các case liên quan thanh toán, tồn kho, điểm khách hàng, audit log và phân quyền.

## 14. Mapping chức năng với code

| Chức năng | Controller | View chính | Service/ViewModel | Stored Procedure/DB |
|---|---|---|---|---|
| Đăng nhập | `AccountController` | `Views/Account/Login.cshtml` | `LoginViewModel`, `LoginResultViewModel`, `ChamCongService` | `TaiKhoan_DangNhap`, `ChamCong_CheckIn`, bảng `TaiKhoan`, `NhanVien`, `ChamCong` |
| Đăng xuất, check-out | `AccountController` | `Views/Account/LogoutConfirm.cshtml`, `CheckoutResult.cshtml` | `LogoutConfirmViewModel`, `ChamCongCheckoutResultViewModel`, `ChamCongService` | `ChamCong_CheckOut`, bảng `ChamCong` |
| Đổi mật khẩu | `AccountController` | `Views/Account/ChangePassword.cshtml` | `ChangePasswordViewModel`, `AuditLogService` | Bảng `TaiKhoan`, `AuditLog_Them` |
| Dashboard | `HomeController` | `Views/Home/Index.cshtml` | `DashboardService`, `DashboardViewModel` | Bảng `SanPham`, `Ban`, `NhanVien`, `ChamCong`, `HoaDon`, `NguyenLieu`, `CTHD`, `PhieuNhap` |
| Order tại quầy | `OrderController` | `Views/Order/Create.cshtml` | `OrderService`, `OrderCreateViewModel`, `OrderItemViewModel` | `HoaDon_Them`, `HoaDon_Sua`, `CTHD_Them`, `CTHD_Xoa`, `SanPham_DS`, `KhuyenMai_DangApDung`, `ChamCong_KiemTraDangLam` |
| Receipt hóa đơn | `OrderController` | `Views/Order/Receipt.cshtml` | `OrderReceiptViewModel`, `PaymentInfoViewModel` | `HoaDon_CT`, `CTHD_DSTheoMaHD`, bảng `ThanhToan`, `Ban`, `KhachHang` |
| Thanh toán tiền mặt | `OrderController` | `Views/Order/Receipt.cshtml` | `OrderService`, `PaymentCommandResultViewModel`, `AuditLogService` | `ThanhToan_TienMat`, `AuditLog_Them`, bảng `HoaDon`, `ThanhToan` |
| Thanh toán QR demo | `OrderController` | `Views/Order/Receipt.cshtml` | `OrderService`, `PaymentCommandResultViewModel` | `ThanhToan_TaoQR`, `ThanhToan_XacNhanQRTheoPhien`, `ThanhToan_QRHetHan`, `ThanhToan_XuLyTatCaQRHetHan` |
| Hủy hoặc sửa order | `OrderController` | `Views/Order/Receipt.cshtml`, `Views/Order/Create.cshtml` | `OrderService`, `AuditLogService` | `ThanhToan_HuyOrder`, `ThanhToan_SuaOrder`, `HoaDon_Sua`, `CTHD_Xoa`, `CTHD_Them` |
| Quản lý bàn | `BanController` | `Views/Ban/*.cshtml` | `Ban`, `OrderWorkflowConstants`, `AuditLogService` | `Ban_DS`, `Ban_CT`, `Ban_Them`, `Ban_Sua`, `Ban_Xoa`, `Ban_KhachDi`, `Ban_DongBoMaSoTen` |
| Quản lý hóa đơn CRUD | `HoaDonController` | `Views/HoaDon/*.cshtml` | `HoaDon`, `CTHD`, `HoaDonViewModel`, `CTHDViewModel` | `HoaDon_DS`, `HoaDon_CT`, `HoaDon_Them`, `HoaDon_Sua`, `HoaDon_Xoa`, `CTHD_*` |
| Khách hàng | `KhachHangController` | `Views/KhachHang/*.cshtml` | `KhachHang` | `KhachHang_DS`, `KhachHang_CT`, `KhachHang_Them`, `KhachHang_Sua`, `KhachHang_Xoa`, bảng `KhachHang` |
| Khuyến mãi | `KhuyenMaiController` | `Views/KhuyenMai/*.cshtml` | `KhuyenMai` | `KhuyenMai_DS`, `KhuyenMai_CT`, `KhuyenMai_DangApDung`, `KhuyenMai_Them`, `KhuyenMai_Sua`, `KhuyenMai_Xoa` |
| Sản phẩm | `SanPhamController` | `Views/SanPham/*.cshtml` | `SanPham`, `SanPhamViewModel`, `SanPhamIndexViewModel` | `SanPham_DS`, `SanPham_CT`, `SanPham_Them`, `SanPham_Sua`, `SanPham_Xoa`, `SanPham_DoiTrangThai` |
| Loại sản phẩm | `LoaiSPController` | `Views/LoaiSP/*.cshtml` | `LoaiSP` | `LoaiSP_DS`, `LoaiSP_CT`, `LoaiSP_Them`, `LoaiSP_Sua`, `LoaiSP_Xoa` |
| Nguyên liệu | `NguyenLieuController` | `Views/NguyenLieu/*.cshtml` | `NguyenLieu` | `NguyenLieu_DS`, `NguyenLieu_CT`, `NguyenLieu_SapHet`, `NguyenLieu_Them`, `NguyenLieu_Sua`, `NguyenLieu_Xoa` |
| Công thức pha chế | `CongThucController` | `Views/CongThuc/*.cshtml`, `_EditorForm.cshtml` | `CongThucEditorViewModel`, `CongThucViewModel`, `CongThucIndexItemViewModel` | `CongThuc_DS`, `CongThuc_TheoSanPham`, `CongThuc_LuuNhieu`, `CongThuc_XoaTheoSanPham` |
| Phiếu nhập kho | `PhieuNhapController` | `Views/PhieuNhap/*.cshtml` | `PhieuNhap`, `CTPN`, `PhieuNhapViewModel`, `CTPNViewModel` | `PhieuNhap_DS`, `PhieuNhap_CT`, `PhieuNhap_Them`, `PhieuNhap_Sua`, `PhieuNhap_Xoa`, `CTPN_Them`, `CTPN_Sua`, `CTPN_Xoa` |
| Nhân viên | `NhanVienController` | `Views/NhanVien/*.cshtml` | `NhanVien`, `NhanVienViewModel` | `NhanVien_DS`, `NhanVien_CT`, `NhanVien_Them`, `NhanVien_Sua`, `NhanVien_Xoa`, bảng `Luong` |
| Tài khoản | `TaiKhoanController` | `Views/TaiKhoan/*.cshtml` | `TaiKhoan`, `TaiKhoanListViewModel`, `TaiKhoanDetailViewModel`, `AuditLogService` | `TaiKhoan_DS`, `TaiKhoan_CT`, `TaiKhoan_Them`, `TaiKhoan_Sua`, `TaiKhoan_Xoa`, `AuditLog_Them` |
| Lương | `LuongController` | `Views/Luong/*.cshtml` | `Luong`, `LuongThangViewModel`, các viewmodel chấm công tháng | `Luong_DS`, `Luong_CT`, `Luong_Them`, `Luong_Sua`, `Luong_Xoa`, `ChamCong_TongLuong*`, `ChamCong_CaLamTheoThang` |
| Thống kê | `ThongKeController` | `Views/ThongKe/*.cshtml` | `DoanhThuTheoNgayViewModel`, `SanPhamBanChayViewModel`, `NhapHangTheoThangViewModel` | `ThongKe_DoanhThuTheoNgay`, `ThongKe_SanPhamBanChay`, `ThongKe_NhapHangTheoThang` |
| Audit log | `AuditLogController` | `Views/AuditLog/Index.cshtml` | `AuditLogFilterViewModel`, `AuditLogViewModel` | `AuditLog_DS`, `AuditLog_Them`, `vw_AuditLog_GanDay`, bảng `AuditLog` |
| Layout, menu, PJAX | Không có controller riêng | `Views/Shared/_Layout.cshtml` | Session, `AppRoles`, `IChamCongService` | Bảng `ChamCong`, session `MaTK`, `VaiTro`, `MaNV` |
| JS order | Không có controller riêng | `Views/Order/Create.cshtml` | `window.orderCreateConfig` | API `Order/SearchCustomers`, `Order/QuickCreateCustomer` |
| JS công thức | Không có controller riêng | `Views/CongThuc/_EditorForm.cshtml` | `CongThucEditorViewModel.Items` | `CongThuc_LuuNhieu` |

## 15. Validation quan trọng

### Account

* Login gọi `TaiKhoan_DangNhap`. Nếu sai tài khoản hoặc mật khẩu, form báo lỗi.
* Nếu đã có session `MaTK`, không cho đăng nhập tài khoản khác ngay. Người dùng phải đăng xuất và check-out trước.
* Login thành công sẽ tự check-in nếu nhân viên chưa có ca `Đang làm`.
* Logout bắt buộc nhập mật khẩu để gọi `ChamCong_CheckOut`.
* Đổi mật khẩu yêu cầu mật khẩu mới dài ít nhất 8 ký tự, có chữ số, có ký tự đặc biệt, xác nhận khớp và không trùng tên đăng nhập.

### Bàn

* `MVE` là bàn đặc biệt cho mang về, không được sửa, không được xóa, không được xử lý như bàn vật lý.
* Bàn vật lý chỉ thuộc `Tầng trệt` hoặc `Tầng lửng`.
* Trạng thái bàn chỉ gồm `Trống` và `Đang phục vụ`.
* Khi thêm bàn, người dùng chỉ chọn khu vực. SQL tự sinh `MaBan`, `SoBan`, `TenBan`.
* Khi trả bàn, code kiểm tra không còn order chưa thanh toán trước khi gọi `Ban_KhachDi`.

### Order

* Nhân viên phải đăng nhập và đang check-in mới được tạo hoặc sửa order.
* Order phải có ít nhất 1 món có số lượng lớn hơn 0.
* Order chỉ lấy sản phẩm `Đang bán`.
* Order tại quán tự chọn bàn trống đầu tiên theo khu vực. Nếu khu vực chọn hết bàn và tầng trệt còn bàn, code fallback về tầng trệt.
* Order mang về dùng `MVE`, không đổi trạng thái bàn vật lý.
* Hóa đơn đã hủy hoặc đã thanh toán không được sửa.
* Nếu có chọn khuyến mãi, code kiểm tra khuyến mãi còn hiệu lực, đang áp dụng, đúng loại và giá trị hợp lệ.

### Thanh toán

* Hóa đơn đã thanh toán không được thanh toán lặp.
* Tiền mặt gọi `ThanhToan_TienMat`, tạo dòng `ThanhToan` và cập nhật hóa đơn.
* Tạo QR gọi `ThanhToan_TaoQR`, có mã phiên và thời gian hết hạn.
* QR demo xác nhận bằng `ThanhToan_XacNhanQRTheoPhien`.
* QR hết hạn dùng `ThanhToan_QRHetHan` hoặc `ThanhToan_XuLyTatCaQRHetHan`.
* Khi sửa order đang có QR chờ, logic thanh toán phải hủy hoặc chuyển trạng thái QR cũ để tránh thanh toán nhầm.
* Audit log nên ghi các thao tác tạo QR, xác nhận QR, QR hết hạn, thanh toán tiền mặt, hủy order, sửa order.

### Sản phẩm

* `DonGia` phải lớn hơn 0 và không vượt quá `10000000`.
* Trạng thái sản phẩm chỉ gồm `Đang bán`, `Tạm ngưng`, `Ngừng bán`.
* Order không được bán sản phẩm không ở trạng thái `Đang bán`.
* Màn sản phẩm có lọc theo từ khóa không dấu, loại sản phẩm và trạng thái.
* Lỗi nhập số quá lớn cần được bắt và chuyển thành thông báo dễ hiểu, không để lộ lỗi decimal hoặc overflow của SQL.

### Khách hàng

* Tên khách hàng không được để trống.
* Số điện thoại phải đúng 10 chữ số.
* Controller có normalize bằng `Trim`.
* SQL/stored procedure hiện có thông báo lỗi cho tên khách hàng trùng và số điện thoại trùng. Đây là logic quan trọng vì trước đây test duplicate phone từng là điểm lỗi thật.
* Tạo nhanh khách hàng ở order cũng kiểm tra trùng số điện thoại trước khi gọi `KhachHang_Them`.

### Công thức

* Sản phẩm phải tồn tại.
* Khi tạo mới, sản phẩm chưa được có công thức.
* Công thức phải có ít nhất 1 nguyên liệu.
* Nguyên liệu phải tồn tại.
* `DinhLuong` phải lớn hơn 0.
* Không được chọn trùng nguyên liệu trong cùng một công thức.
* JS phải đánh lại index `Items[index].MaNL` và `Items[index].DinhLuong` trước khi submit.
* Nút thêm, xóa dòng công thức phải dùng event delegation để sống được sau PJAX.

### Phiếu nhập

* Phiếu nhập bắt buộc có nhân viên và ngày nhập.
* Chi tiết phiếu nhập bắt buộc có nguyên liệu, số lượng lớn hơn 0 và đơn giá hợp lệ.
* `CTPN_Them` tăng tồn kho nguyên liệu.
* `CTPN_Sua` điều chỉnh tồn kho theo chênh lệch số lượng cũ và mới.
* `CTPN_Xoa` trừ lại tồn kho nếu kho còn đủ để hoàn tác.
* Nếu kho không đủ để hoàn tác khi sửa giảm hoặc xóa chi tiết, controller trả thông báo thân thiện qua `FriendlyCtpnError`.
* Sau thêm, sửa, xóa chi tiết, SQL gọi `PhieuNhap_CapNhatTongTien`.

### Báo cáo, thống kê, lương

* `ThongKe/DoanhThuTheoNgay` không cho `tuNgay` lớn hơn `denNgay`.
* `ThongKe/NhapHangTheoThang` mặc định lấy tháng, năm hiện tại nếu không truyền tham số.
* `Luong/Thang` chỉ nhận tháng từ 1 đến 12. Nếu sai, controller đưa về tháng hiện tại và báo lỗi.
* Các trang báo cáo phải được kiểm tra bằng cả UI và dữ liệu SQL khi làm báo cáo kiểm thử, nhất là doanh thu, nhập hàng và lương tháng.

## 16. Những điểm cần chú ý khi AI khác tiếp tục làm

* Luôn đọc `QLCHCF_Full.sql` trước khi sửa nghiệp vụ. Logic quan trọng nằm nhiều trong stored procedure, không chỉ trong C#.
* Không dùng nhầm SQL, README hoặc logic của bài cá nhân khác. Project này là `WebQLCHCF`, database là `QLCHCF`.
* Không đổi connection string, schema, tên database, khóa chính, khóa ngoại hoặc chữ ký stored procedure nếu chưa có yêu cầu rõ.
* Không sửa authentication/authorization theo hướng ASP.NET Identity nếu chưa được giao. Project hiện dùng session và `RoleAuthorizeAttribute`.
* Không xem `MVE` là bàn vật lý. Đây là dòng đặc biệt cho đơn mang về.
* Không bỏ `app.initPage(scope)` trong `site.js`. PJAX phụ thuộc vào hook này để khởi tạo lại validation, tooltip, order và công thức.
* Không chuyển nút thêm/xóa công thức về binding chỉ trong `DOMContentLoaded`. Cần giữ event delegation trong `congthuc-editor.js`.
* Không sửa Order chọn khu vực nếu task không nhắc tới. Logic chọn bàn theo `Tầng trệt`, `Tầng lửng`, fallback và `MVE` đang gắn với nhiều test case.
* Không báo pass cho validation nếu chỉ nhìn UI. Với khách hàng trùng SĐT, thanh toán, tồn kho, audit log và báo cáo, cần kiểm tra thêm DB.
* Khi sửa các form có tiền hoặc số lượng, phải kiểm tra cả C# validation và SQL constraint để tránh lỗi decimal/money.
* Khi sửa thanh toán QR, phải giữ các trạng thái `ChuaThanhToan`, `ChoThanhToanQR`, `DaThanhToan`, `HetHan`, `DaHuy`, `ChoKiemTra`.
* Khi sửa phiếu nhập, phải kiểm tra `NguyenLieu.SoLuongTon`, `PhieuNhap.TongTien` và các dòng `CTPN`.
* Sau build/test, tắt server web cục bộ nếu không còn cần kiểm tra live.

## 17. Điểm còn hạn chế hoặc cần kiểm tra thêm

Các điểm dưới đây không phải kết luận sai chức năng. Đây là phần nên kiểm tra thêm khi bàn giao hoặc khi người sau tiếp tục làm:

* QR là demo, chưa tích hợp webhook ngân hàng thật. `ThanhToan_XacNhanQRWebhook` có trong SQL nhưng luồng web chính đang dùng xác nhận demo theo mã phiên.
* Mật khẩu tài khoản đang lưu dạng text thường trong SQL và model. Nếu muốn bảo mật thật, cần thiết kế lại phần hash mật khẩu, nhưng không nên tự sửa khi chưa được giao vì sẽ ảnh hưởng login và dữ liệu mẫu.
* `Program.cs` có `app.UseAuthorization()` nhưng không thấy dùng ASP.NET Core authentication chuẩn. Phân quyền thực tế nằm ở session và `RoleAuthorizeAttribute`.
* Một số controller CRUD dùng stored procedure xóa trực tiếp. Khi dữ liệu đã có ràng buộc FK, thao tác xóa có thể thất bại đúng theo SQL. Khi test cần ghi rõ dữ liệu đang bị tham chiếu bởi bảng nào.
* Giao diện responsive nên được kiểm tra lại trên màn hình nhỏ bằng web view nếu dùng để demo chính thức.
* Báo cáo thống kê cần kiểm tra DB khi số liệu demo thay đổi, vì dữ liệu seed có nhiều hóa đơn ngày `2026-05-30` và một hóa đơn đang `DangOrder`.
* Dữ liệu mẫu có một ca chấm công `NV01` đang `Đang làm`. Sau khi chạy lại SQL, trạng thái này quay về như seed data.

## 18. Kết luận bàn giao

`WebQLCHCF` là website quản lý quán cà phê nội bộ cho bài nhóm Kiểm thử phần mềm. Project dùng ASP.NET Core MVC, Razor Views, EF Core SQL Server, session-based role authorization và database `QLCHCF`.

Source hiện có đủ các module chính: đăng nhập, chấm công, order tại quầy, hóa đơn, thanh toán tiền mặt, QR demo, bàn, khách hàng, sản phẩm, loại sản phẩm, khuyến mãi, nguyên liệu, công thức, phiếu nhập, nhân viên, tài khoản, lương, thống kê và audit log.

Database `QLCHCF` được dựng từ `QLCHCF_Full.sql`. File SQL này không chỉ tạo bảng mà còn chứa constraint, index, function, view, stored procedure và dữ liệu mẫu. Khi sửa lỗi hoặc viết test, phải xem SQL là nguồn nghiệp vụ chính ngang với code C#.

Tài liệu này chỉ dựa trên project `WebQLCHCF`, database `QLCHCF` và file `QLCHCF_Full.sql`. Những điểm cần kiểm tra thêm đã được tách riêng ở mục 18 để người tiếp tục làm không nhầm giữa logic đã xác nhận và phần cần kiểm thử thêm.
