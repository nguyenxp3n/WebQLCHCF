# Cách dùng dotnet-best-practices cho WebQLCHCF

Skill này không dùng để redesign UI trực tiếp. Nó dùng để bảo vệ ASP.NET Core MVC khi sửa Razor/Bootstrap.

## Không được phá

```text
Controllers/
Models/
ViewModels/
Services/
Data/AppDbContext
QLCHCF_Full.sql
Stored procedure
Route
Action name
View name
Session
ModelState
Validation
AntiForgeryToken
```

## Session/logic cần giữ

Giữ mọi key session hiện có của project, đặc biệt thông tin đăng nhập, nhân viên hiện tại, vai trò, trạng thái ca làm.

## Computed columns không nhập từ form

Không thêm input cho:

```text
CTHD.ThanhTien
CTPN.ThanhTien
Luong.TongLuong
```

## Order

Không đổi logic:

```text
- Giá lấy lại từ SQL khi POST.
- Chỉ order sản phẩm Đang bán.
- SQL tính tổng tiền.
- SQL tính khuyến mãi.
- MVE là bàn mang về.
```

## Chấm công

Không đổi logic:

```text
- Login tự check-in.
- Logout yêu cầu mật khẩu.
- Sai mật khẩu không logout.
- Checkout đúng nhân viên hiện tại.
```

## Khi sửa form Razor

Luôn giữ:

```html
@Html.AntiForgeryToken()
<label asp-for="..." class="form-label"></label>
<input asp-for="..." class="form-control" />
<span asp-validation-for="..." class="text-danger small"></span>
```
