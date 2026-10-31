# BẮT BUỘC ĐỌC TRƯỚC

Trước khi dùng bất kỳ skill hoặc prompt nào trong workflow này, AI phải đọc file:

`UI_UX_REDESIGN_CONTRACT_ALL_IN_ONE.md`

File này có độ ưu tiên cao nhất trong toàn bộ workflow.

Tóm tắt bắt buộc:

- Chỉ được redesign UI/UX.
- Không được thay đổi SQL, database, stored procedure, business logic hoặc kết quả hệ thống.
- Ưu tiên sửa Razor Views, CSS, JavaScript giao diện trước.
- ViewModel/Controller chỉ được chỉnh rất giới hạn nếu đó là presentation-only, ví dụ `PageTitle`, `Breadcrumb`, `ActiveMenu`, `BadgeClass`, `DisplayText`, message hiển thị.
- Mọi thay đổi ViewModel/Controller phải được báo trước, giải thích rõ, và không được ảnh hưởng nghiệp vụ.
- Nếu phát hiện cần sửa backend/SQL/logic, AI phải dừng lại và hỏi người dùng trước.

---

# Gemini Workflow Frontend UI/UX cho WebQLCHCF

Bộ workflow này được thiết kế riêng cho dự án:

**WebQLCHCF - Hệ thống quản lý quán cà phê**

Stack cố định:

```text
ASP.NET Core MVC
Razor Views
SQL Server
Entity Framework Core
Stored Procedure
Bootstrap 5
Bootstrap Icons
Session
```

## Luật cứng

```text
Không dùng React.
Không dùng Tailwind.
Không dùng shadcn/ui.
Không dùng GSAP.
Không dùng Lenis.
Không dùng 3D.
Không dùng custom cursor.
Không đổi sang Blazor.
Không đổi sang Web API.
Không đổi database.
Không đổi stored procedure nếu mục tiêu chỉ là redesign UI.
```

## Thứ tự dùng skill tốt nhất

### Bước 1: `penpot-uiux-design`

Dùng để hiểu nghiệp vụ và dựng lại UX tổng thể trước khi code.

Áp dụng cho:

```text
Dashboard
Order tại quầy
Receipt/In bill
CRUD sản phẩm
CRUD bàn
CRUD khách hàng
CRUD khuyến mãi
Nguyên liệu/Công thức
Phiếu nhập/Kho
Chấm công
Lương
Thống kê
```

Kết quả cần có:

```text
Design system Bootstrap 5
Layout admin dashboard
Sidebar/topbar
Page header
Card statistics
Search/filter area
Table pattern
Form pattern
Modal pattern
Empty/error/loading states
Responsive rules
```

### Bước 2: `premium-frontend-ui`

Dùng để làm giao diện đẹp hơn, nhưng chỉ ở mức **premium admin dashboard**.

Chỉ áp dụng:

```text
Card đẹp hơn
Table đẹp hơn
Form đẹp hơn
Sidebar/topbar đẹp hơn
Spacing/typography tốt hơn
Hover/focus nhẹ
Badge trạng thái rõ
Nút thao tác đẹp và dễ hiểu
```

Không áp dụng:

```text
Landing page style
Animation nặng
Scroll storytelling
Custom cursor
GSAP
Lenis
3D
```

### Bước 3: `dotnet-best-practices`

Dùng để bảo vệ backend ASP.NET Core MVC khi sửa UI.

Không được phá:

```text
Controller
Model
ViewModel
Service
AppDbContext
Route
View name
asp-controller
asp-action
asp-for
ModelState
Validation
AntiForgeryToken
Session
Stored Procedure call
Computed columns
Logic order/chấm công/lương/kho
```

### Bước 4: `refactor`

Dùng để dọn code Razor/CSS sau khi đã có UI mới.

Có thể tách:

```text
Views/Shared/Partials/_Sidebar.cshtml
Views/Shared/Partials/_Topbar.cshtml
Views/Shared/Partials/_UserMenu.cshtml
Views/Shared/Partials/_PageHeader.cshtml
Views/Shared/Partials/_ToastMessages.cshtml
Views/Shared/Partials/_ValidationSummary.cshtml
```

Có thể tổ chức CSS:

```text
wwwroot/css/site.css
wwwroot/css/layout.css
wwwroot/css/components.css
wwwroot/css/pages-dashboard.css
wwwroot/css/pages-order.css
```

Không refactor architecture lớn khi chỉ redesign UI.

### Bước 5: `web-design-reviewer`

Dùng để kiểm tra giao diện sau khi code.

Checklist:

```text
Responsive desktop/tablet/mobile
Sidebar không che content
Offcanvas mobile hoạt động
Table không tràn màn hình
Form validation dễ nhìn
Modal không vỡ trên mobile
Contrast đủ đọc
Focus state rõ
Button đủ lớn
Badge trạng thái nhất quán
Dashboard không dùng fake data
```

### Bước 6: `webapp-testing`

Dùng để test flow thật.

Flow phải test:

```text
Login qly_tram/12345678
Dashboard
Order ngồi tại quán
Order mang về MVE
Receipt/In bill
Trả bàn
CRUD sản phẩm
CRUD bàn
CRUD khách hàng
Phiếu nhập/kho
Chấm công logout có mật khẩu
Lương tháng
Thống kê
```

## Prompt tổng hợp dùng cho Gemini/Copilot/Cursor

```text
Tôi dùng ASP.NET Core MVC, Razor Views, Bootstrap 5, Bootstrap Icons, SQL Server, EF Core và Stored Procedure.

Dự án là WebQLCHCF - hệ thống quản lý quán cà phê.

Hãy redesign UI/UX toàn bộ web bằng Bootstrap 5, không dùng React, không dùng Tailwind, không dùng shadcn/ui, không dùng GSAP, không dùng Lenis, không dùng 3D, không dùng custom cursor.

Áp dụng theo thứ tự:
1. penpot-uiux-design để thiết kế design system, UX flow, layout, navigation, dashboard, form, table.
2. premium-frontend-ui ở mức admin dashboard để làm card/table/form/sidebar/topbar đẹp hơn, hover/focus nhẹ.
3. dotnet-best-practices để bảo vệ ASP.NET Core MVC, Razor Views, Session, Controller, Model, ViewModel, Service, Stored Procedure.
4. refactor để tách partial views, tổ chức CSS, giảm lặp code Razor nhưng không đổi hành vi.
5. web-design-reviewer để review responsive, accessibility, visual consistency.
6. webapp-testing để test các flow chính.

Giữ nguyên:
- Database QLCHCF.
- QLCHCF_Full.sql.
- Stored procedure hiện có.
- Controller/action hiện có nếu không cần đổi.
- Model/ViewModel hiện có.
- Logic Login tự check-in.
- Logic Logout nhập mật khẩu để checkout.
- Logic Order lấy giá và trạng thái sản phẩm từ SQL.
- Logic SQL tính tổng tiền, khuyến mãi, tồn kho, lương, chấm công.
- Các computed column không nhập từ form.

Tập trung sửa:
- Views/Shared/_Layout.cshtml.
- site.css.
- Các Razor Views trong Views/.
- Partial views nếu cần.
- Bootstrap 5 classes.
- Bootstrap Icons.
```
