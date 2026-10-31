# UI/UX REDESIGN CONTRACT ALL-IN-ONE - LUẬT BẮT BUỘC KHI REDESIGN WEBQLCHCF

> File này là luật ưu tiên cao nhất cho toàn bộ workflow Gemini/Copilot/Cursor khi redesign UI/UX dự án **WebQLCHCF**.
>
> Nếu bất kỳ skill, prompt, tool, đề xuất refactor hoặc gợi ý code nào mâu thuẫn với file này, AI phải ưu tiên file này.

---

## 0. Mục tiêu tuyệt đối

AI chỉ được thực hiện nhiệm vụ:

```text
Cải thiện UI/UX.
Không thay đổi nghiệp vụ.
Không thay đổi SQL.
Không thay đổi database.
Không thay đổi stored procedure.
Không thay đổi kết quả hệ thống.
```

Project cố định:

```text
WebQLCHCF
ASP.NET Core MVC
Razor Views
Bootstrap 5
Bootstrap Icons
SQL Server
Entity Framework Core
Stored Procedure
Session
```

---

## 1. Phạm vi nhiệm vụ

AI chỉ được redesign frontend UI/UX cho hệ thống quản lý quán cà phê WebQLCHCF.

AI được phép cải thiện:

- Giao diện đẹp hơn.
- Bố cục rõ ràng hơn.
- Dashboard trực quan hơn.
- Sidebar/topbar chuyên nghiệp hơn.
- Form dễ nhập hơn.
- Table dễ đọc hơn.
- Order tại quầy thao tác nhanh hơn.
- Receipt/in bill sạch hơn.
- Badge trạng thái rõ hơn.
- Alert/thông báo thân thiện hơn.
- Empty state dễ hiểu hơn.
- Responsive tốt hơn trên desktop, tablet, mobile.
- Hover/focus/transition nhẹ và mượt hơn.
- Trải nghiệm thao tác người dùng tốt hơn.
- Cách người dùng thao tác trong UI tốt hơn, miễn là dữ liệu gửi về server và kết quả nghiệp vụ không đổi.

AI không được thay đổi bản chất hệ thống, logic nghiệp vụ, SQL, database hoặc kết quả cuối cùng của bất kỳ chức năng nào.

---

## 2. Nguyên tắc cốt lõi

Mọi thay đổi phải tuân theo nguyên tắc:

```text
Cách hiển thị có thể thay đổi.
Cách người dùng thao tác có thể tốt hơn.
Cách UI phản hồi có thể mượt hơn.
Nhưng dữ liệu gửi đi, dữ liệu nhận về, nghiệp vụ xử lý và kết quả cuối cùng phải giữ nguyên.
```

Được phép:

```text
Form cũ khó nhìn.
Form mới chia card, group field, label rõ hơn.
Đích đến vẫn là action cũ, field cũ, model binding cũ.
```

Được phép:

```text
Order cũ chọn món khó thao tác.
Order mới dùng card sản phẩm, nút cộng/trừ rõ hơn, preview tổng tiền tốt hơn.
Đích đến vẫn POST về action cũ, server vẫn lấy giá từ SQL.
```

Không được phép:

```text
UI mới tự tính tổng tiền bằng JavaScript rồi gửi lên server.
```

Không được phép:

```text
UI mới bỏ kiểm tra logout bằng mật khẩu.
```

Không được phép:

```text
UI mới đổi route/action/controller để thuận tiện hơn.
```

---

## 3. Vùng được phép sửa

AI được phép sửa các file thuộc tầng giao diện:

```text
Views/**/*.cshtml
Views/Shared/**/*.cshtml
wwwroot/css/**/*.css
wwwroot/js/**/*.js
wwwroot/images hoặc static assets giao diện nếu cần
```

AI được phép sửa trong Razor Views:

- HTML markup.
- Bootstrap 5 classes.
- Bootstrap Icons.
- Card layout.
- Table layout.
- Form layout.
- Modal layout.
- Sidebar.
- Topbar/navbar.
- Page header.
- Button group.
- Badge trạng thái.
- Alert.
- Toast nếu đã dùng Bootstrap.
- Empty state.
- Responsive wrapper.
- Receipt print layout.
- UX order tại quầy.

AI được phép sửa trong CSS:

- Design tokens.
- Spacing.
- Typography.
- Color system.
- Border radius.
- Shadow.
- Hover state.
- Focus state.
- Transition nhẹ.
- Responsive media query.
- Print CSS cho receipt.

AI được phép sửa JavaScript giao diện nếu chỉ phục vụ UI/UX:

- Toggle sidebar mobile.
- Toggle class giao diện.
- Active menu UI.
- Cải thiện thao tác cộng/trừ ở giao diện nếu dữ liệu submit không đổi.
- UX feedback không ảnh hưởng nghiệp vụ.
- Animation nhỏ bằng CSS class.
- Không thay đổi dữ liệu nghiệp vụ gửi về server.

---

## 4. Vùng hạn chế: ViewModel và Controller

Trong ASP.NET Core MVC, ViewModel và Controller đôi khi có phần phục vụ UI/UX. Vì vậy, AI được phép đề xuất hoặc chỉnh sửa rất giới hạn ở ViewModel/Controller nếu thay đổi đó chỉ thuộc tầng presentation.

Thứ tự ưu tiên khi sửa:

```text
1. Razor Views
2. CSS
3. JavaScript giao diện
4. Partial Views
5. ViewModel presentation-only nếu thật sự cần
6. Controller presentation-only nếu thật sự cần
```

Controller và ViewModel là vùng hạn chế. Chỉ được đụng tới khi Razor/CSS/JS không đủ để cải thiện UI/UX.

---

## 5. Được phép trong ViewModel nếu chỉ phục vụ hiển thị

AI được phép thêm hoặc chỉnh property presentation-only trong ViewModel, ví dụ:

- PageTitle.
- Breadcrumb.
- ActiveMenu.
- DisplayName.
- DisplayText.
- StatusText.
- StatusBadgeClass.
- RowCssClass.
- IconName.
- EmptyStateMessage.
- FormMode.
- UiGroupName.
- UiSortOrder nếu chỉ sắp xếp hiển thị.
- FormattedMoney nếu chỉ format hiển thị.
- FormattedDate nếu chỉ format hiển thị.

Ví dụ được phép:

```csharp
public string TrangThaiBadgeClass => TrangThai switch
{
    "Đang bán" => "bg-success-subtle text-success",
    "Tạm ngưng" => "bg-warning-subtle text-warning",
    "Ngừng bán" => "bg-danger-subtle text-danger",
    _ => "bg-secondary-subtle text-secondary"
};
```

Ví dụ được phép:

```csharp
public string TongTienDisplay => TongTien.ToString("N0") + " đ";
```

Điều kiện bắt buộc:

- Không đổi giá trị gốc.
- Không đổi dữ liệu lưu database.
- Không đổi field bind form.
- Không đổi validation nghiệp vụ.
- Không đổi dữ liệu gửi xuống SQL.
- Không đổi dữ liệu gửi lên Controller.
- Không đổi kết quả order, hóa đơn, tồn kho, lương, chấm công.

---

## 6. Được phép trong Controller nếu chỉ phục vụ hiển thị

AI được phép chỉnh rất giới hạn trong Controller nếu chỉ để cải thiện hiển thị UI:

- Thêm `ViewBag.PageTitle`.
- Thêm `ViewBag.Breadcrumb`.
- Thêm `ViewBag.ActiveMenu`.
- Chuẩn hóa `TempData["SuccessMessage"]`.
- Chuẩn hóa `TempData["ErrorMessage"]`.
- Truyền thêm dữ liệu presentation-only sang View.
- Chọn lại message hiển thị thân thiện hơn.
- Chọn lại View hiển thị nếu vẫn cùng action, cùng nghiệp vụ, cùng dữ liệu.

Ví dụ được phép:

```csharp
ViewBag.PageTitle = "Quản lý sản phẩm";
ViewBag.ActiveMenu = "SanPham";
```

Ví dụ được phép:

```csharp
TempData["SuccessMessage"] = "Thao tác thành công.";
```

Điều kiện bắt buộc:

- Không đổi action name.
- Không đổi route.
- Không đổi HTTP method GET/POST.
- Không đổi stored procedure được gọi.
- Không đổi query nghiệp vụ.
- Không đổi điều kiện kiểm tra quyền.
- Không đổi login/logout/check-in/check-out.
- Không đổi order/tính tiền/khuyến mãi/tồn kho/lương.
- Không đổi model binding.
- Không đổi redirect nghiệp vụ.
- Không đổi kết quả cuối cùng.

---

## 7. Luật xin phép trước khi sửa ViewModel/Controller

Nếu AI muốn sửa ViewModel hoặc Controller, AI phải dừng lại và báo trước:

```text
Tôi phát hiện cần chỉnh một phần presentation trong ViewModel/Controller để cải thiện UI/UX.

Thay đổi này chỉ phục vụ hiển thị.
Thay đổi này không sửa SQL.
Thay đổi này không sửa nghiệp vụ.
Thay đổi này không đổi kết quả hệ thống.

File dự kiến sửa:
- ...

Lý do UI/UX:
- ...

Phần logic được cam kết giữ nguyên:
- ...
```

AI chỉ được sửa nếu thay đổi nằm trong nhóm presentation-only.

Nếu có nghi ngờ thay đổi ảnh hưởng nghiệp vụ, AI không được sửa.

---

## 8. Luật cấm tuyệt đối

AI không được tự ý sửa, đổi, xóa, thêm hoặc refactor các phần sau nếu không có lệnh riêng rõ ràng từ người dùng:

- Database.
- File `QLCHCF_Full.sql`.
- Stored procedure.
- Function SQL.
- Trigger.
- Constraint.
- Computed column.
- Index.
- Entity Framework mapping.
- AppDbContext.
- Program.cs.
- appsettings.json.
- Connection string.
- Business service logic.
- Controller business logic.
- ViewModel business property.
- Model entity property nghiệp vụ.
- Route.
- Action name.
- Controller name.
- SQL query.
- Stored procedure name.
- Stored procedure parameter.
- Session key.
- Validation backend.
- Authentication/session logic.
- Logic phân quyền.
- Logic đăng nhập.
- Logic đăng xuất.
- Logic check-in/check-out.
- Logic order.
- Logic tính tiền.
- Logic khuyến mãi.
- Logic bàn.
- Logic phiếu nhập.
- Logic tồn kho.
- Logic lương.
- Logic thống kê.
- Logic dashboard lấy dữ liệu.

Bất kỳ thay đổi nào làm khác kết quả nghiệp vụ trước và sau redesign đều bị xem là sai nhiệm vụ.

---

## 9. Luật bảo vệ Razor binding

Khi sửa `.cshtml`, AI phải giữ nguyên các thành phần MVC/Razor binding:

- Giữ nguyên `asp-controller`.
- Giữ nguyên `asp-action`.
- Giữ nguyên `asp-route-*`.
- Giữ nguyên `asp-for`.
- Giữ nguyên `asp-items`.
- Giữ nguyên `name` của input nếu đang dùng để bind dữ liệu.
- Giữ nguyên `id` quan trọng nếu JavaScript hoặc CSS hiện tại đang dùng.
- Giữ nguyên hidden input nghiệp vụ.
- Giữ nguyên form `method`.
- Giữ nguyên URL action.
- Giữ nguyên validation tag helper.

Được phép:

```html
<input asp-for="TenSP" class="form-control form-control-lg" />
```

Không được phép:

```html
<input name="TenSanPhamMoi" class="form-control" />
```

Nếu đổi `asp-for`, `name`, `action`, `controller`, `route`, AI có thể phá binding MVC, nên bị cấm.

---

## 10. Luật bảo vệ form POST

Trong mọi form POST, AI phải giữ nguyên:

```cshtml
@Html.AntiForgeryToken()
```

nếu form đang có.

AI phải giữ nguyên validation nếu view đang có:

```cshtml
<div asp-validation-summary="ModelOnly"></div>
<span asp-validation-for="..."></span>
```

AI chỉ được bọc form bằng Bootstrap 5 card/grid để đẹp hơn, không được đổi dữ liệu gửi lên server.

Được phép:

```html
<div class="card border-0 shadow-sm rounded-4">
    <div class="card-body p-4">
        <!-- giữ nguyên input Razor bên trong -->
    </div>
</div>
```

Không được phép:

- Đổi tên field.
- Bỏ hidden input nghiệp vụ.
- Bỏ anti-forgery token.
- Bỏ validation.
- Đổi `method="post"` sang GET.
- Đổi action submit.
- Tự thêm field nghiệp vụ mới.
- Tự xóa field nghiệp vụ cũ.

---

## 11. Luật bảo vệ nghiệp vụ WebQLCHCF

AI bắt buộc phải giữ nguyên toàn bộ nghiệp vụ sau.

### Login / Logout / Chấm công

- Login thành công tự check-in.
- Logout phải nhập mật khẩu để checkout.
- Sai mật khẩu không logout.
- Sai mật khẩu không checkout.
- Sai mật khẩu không xóa session.
- Mỗi nhân viên chỉ có một ca đang làm.
- Navbar chỉ hiển thị trạng thái ca làm của nhân viên hiện tại.

### Order tại quầy

- Nhân viên phải đăng nhập.
- Nhân viên phải có ca làm.
- Order chỉ lấy sản phẩm `Đang bán`.
- Khi POST order, giá sản phẩm phải lấy lại từ SQL.
- Không tin giá từ client.
- Tạo hóa đơn bằng logic hiện có.
- Thêm món bằng logic hiện có.
- Tổng tiền do SQL tính.
- C# không tự tính lại tổng tiền thay SQL.
- JavaScript không được thay thế logic tính tiền server.

### Bàn

- Bàn `MVE` là bàn đặc biệt cho đơn mang về.
- `MVE` không tính vào bàn đang hoạt động.
- Đơn tại quán lấy bàn `Trống`.
- Tạo đơn tại quán chuyển bàn sang `Đang phục vụ`.
- Trả bàn đưa bàn về `Trống`.

### Khuyến mãi

- SQL tính khuyến mãi.
- `HoaDon.TongTien` là tổng cuối sau khuyến mãi.
- C# không trừ khuyến mãi lần hai.
- UI chỉ hiển thị kết quả, không đổi logic tính.

### Phiếu nhập / Tồn kho

- `CTPN_Them` tăng tồn kho.
- `CTPN_Sua` cập nhật chênh lệch tồn kho.
- `CTPN_Xoa` kiểm tra kho trước khi hoàn tác.
- Không ép tồn kho âm về 0.
- Không tự tính tồn kho bằng JavaScript.

### Computed columns

AI không được nhập, sửa, bind hoặc update các cột:

- `CTHD.ThanhTien`
- `CTPN.ThanhTien`
- `Luong.TongLuong`

Các cột này do SQL tính.

---

## 12. Luật về animation và chuyển động

AI được phép thêm chuyển động UI nhẹ để web mượt hơn, nhưng chỉ ở tầng giao diện.

Được phép:

- CSS transition nhẹ cho button, card, nav-link.
- Hover nâng card nhẹ.
- Focus input rõ hơn.
- Sidebar mobile trượt bằng Bootstrap offcanvas hoặc CSS class.
- Dropdown mượt nhẹ.
- Modal Bootstrap 5 mặc định.
- Loading state UI nếu không ảnh hưởng nghiệp vụ.
- Toast/alert UI nếu dùng Bootstrap 5.
- Animation nhỏ dùng CSS class.

Không được phép:

- Không dùng GSAP.
- Không dùng Lenis.
- Không dùng Three.js.
- Không dùng 3D.
- Không dùng custom cursor.
- Không dùng scroll hijacking.
- Không dùng animation nặng.
- Không dùng hiệu ứng landing page.
- Không làm chậm thao tác order.
- Không làm animation cản trở nhập liệu.

Animation phải ưu tiên:

```css
transform
opacity
box-shadow nhẹ
transition 0.15s - 0.25s
```

Không animation các thuộc tính dễ gây lag như:

```css
width
height
top
left
margin
padding lớn
```

trừ khi thật sự cần cho responsive/layout.

---

## 13. Luật cấm đổi stack

AI tuyệt đối không được thêm hoặc chuyển sang:

- React.
- Vue.
- Angular.
- Blazor.
- Tailwind CSS.
- shadcn/ui.
- Next.js.
- Web API.
- Docker.
- Clean Architecture.
- Repository Pattern mới.
- CQRS.
- MediatR.
- Semantic Kernel.
- GSAP.
- Lenis.
- Three.js.

Project phải giữ nguyên stack:

```text
ASP.NET Core MVC
Razor Views
Bootstrap 5
Bootstrap Icons
SQL Server
Entity Framework Core
Stored Procedure
Session
```

---

## 14. Luật khi phát hiện cần sửa backend/SQL/logic

Nếu AI phát hiện cần sửa C#, SQL hoặc logic để cải thiện UX, AI không được tự sửa.

AI phải dừng lại và báo:

```text
Tôi phát hiện vấn đề cần thay đổi backend/SQL/logic.
Theo UI/UX REDESIGN CONTRACT, tôi không được tự sửa phần này.
Tôi chỉ đề xuất thay đổi và chờ người dùng cho phép.
```

Sau đó AI chỉ được mô tả đề xuất, không được sửa file.

---

## 15. Quy trình bắt buộc trước khi sửa

Trước khi sửa code, AI phải liệt kê:

```text
1. File sẽ đọc.
2. File dự kiến sửa.
3. Loại thay đổi UI/UX sẽ thực hiện.
4. Có sửa ViewModel/Controller không.
5. Nếu có, phần sửa có phải presentation-only không.
6. Xác nhận không sửa SQL/database/business logic.
7. Rủi ro nếu có.
```

AI chỉ được bắt đầu sửa khi kế hoạch nằm trong phạm vi UI/UX.

---

## 16. Checklist sau khi sửa

Sau khi sửa, AI phải tự kiểm tra:

- Có sửa SQL không?
- Có sửa database không?
- Có sửa stored procedure không?
- Có sửa function SQL không?
- Có sửa Controller business logic không?
- Có sửa Service business logic không?
- Có sửa Model/ViewModel nghiệp vụ không?
- Có sửa route/action/controller name không?
- Có đổi `asp-for` không?
- Có đổi `asp-action` không?
- Có đổi `asp-controller` không?
- Có bỏ AntiForgeryToken không?
- Có bỏ validation không?
- Có đổi session key không?
- Có đổi nghiệp vụ login/logout/chấm công không?
- Có đổi nghiệp vụ order không?
- Có đổi nghiệp vụ tính tiền không?
- Có đổi nghiệp vụ khuyến mãi không?
- Có đổi nghiệp vụ tồn kho không?
- Có đổi nghiệp vụ lương/thống kê không?
- Có thêm thư viện ngoài stack không?
- Có dùng React/Tailwind/shadcn/GSAP/Lenis/3D/custom cursor không?

Nếu bất kỳ câu trả lời nào là “Có” và không thuộc presentation-only được phép, AI phải hoàn tác thay đổi đó.

---

## 17. Định nghĩa thay đổi hợp lệ

Một thay đổi được xem là hợp lệ khi thỏa tất cả điều kiện:

- Chỉ cải thiện UI/UX.
- Không đổi dữ liệu gốc.
- Không đổi dữ liệu lưu database.
- Không đổi SQL.
- Không đổi stored procedure.
- Không đổi luồng nghiệp vụ.
- Không đổi kết quả cuối cùng.
- Không phá model binding.
- Không phá validation.
- Không phá session.
- Không phá flow demo.
- Không phá build.
- Có thể rollback dễ dàng.

---

## 18. Định nghĩa hoàn thành đúng

Redesign UI/UX được xem là đúng khi:

- Giao diện đẹp hơn.
- Trải nghiệm thao tác tốt hơn.
- Responsive tốt hơn.
- Dashboard rõ hơn.
- Order nhanh hơn.
- Form dễ nhập hơn.
- Table dễ đọc hơn.
- Receipt dễ in hơn.
- Sidebar/topbar chuyên nghiệp hơn.
- Animation nhẹ và mượt hơn.
- Không thay đổi kết quả nghiệp vụ.
- Không thay đổi database.
- Không thay đổi SQL.
- Không phá ASP.NET Core MVC.
- Không phá Razor binding.
- Không phá Bootstrap 5.
- Không phá flow demo.

Mục tiêu cuối cùng:

```text
UI/UX tốt hơn.
Nghiệp vụ giữ nguyên 100%.
SQL giữ nguyên 100%.
Database giữ nguyên 100%.
Kết quả hệ thống giữ nguyên 100%.
```
