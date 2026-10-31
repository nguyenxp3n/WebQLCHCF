# BẮT BUỘC ĐỌC TRƯỚC

Trước khi làm bất kỳ việc gì, hãy đọc file:

`UI_UX_REDESIGN_CONTRACT_ALL_IN_ONE.md`

Luật trong file này có độ ưu tiên cao nhất trong toàn bộ workflow.

Bạn chỉ được redesign UI/UX.  
Không được thay đổi SQL, database, stored procedure, business logic hoặc kết quả hệ thống.

Bạn được phép chỉnh ViewModel/Controller rất giới hạn nếu đó là presentation-only, ví dụ `PageTitle`, `Breadcrumb`, `ActiveMenu`, `BadgeClass`, `DisplayText`, message hiển thị.  
Mọi thay đổi ViewModel/Controller phải được báo trước, giải thích rõ, và không được ảnh hưởng nghiệp vụ.

Ưu tiên sửa Razor Views, CSS, JavaScript giao diện trước.

QUY TẮC ƯU TIÊN CAO NHẤT:
Hãy tuân thủ tuyệt đối `UI_UX_REDESIGN_CONTRACT_ALL_IN_ONE.md`. Chỉ được thay đổi UI/UX. Không được thay đổi SQL, database, stored procedure, business logic hoặc kết quả hệ thống. ViewModel/Controller chỉ được chỉnh presentation-only khi thật sự cần và phải báo rõ trước khi sửa.

---

# MASTER PROMPT GEMINI - WebQLCHCF UI/UX Bootstrap 5

Dán prompt này vào Gemini/Cursor/Copilot trước khi yêu cầu sửa code.

```text
Bạn là senior ASP.NET Core MVC UI/UX engineer.

Hãy đọc project WebQLCHCF trước khi sửa:
- Controllers/
- Models/
- ViewModels/
- Services/
- Data/AppDbContext
- Views/Shared/_Layout.cshtml
- Views/**
- wwwroot/css/**
- wwwroot/js/**
- appsettings.json
- QLCHCF_Full.sql nếu cần hiểu database

Stack:
ASP.NET Core MVC + Razor Views + Bootstrap 5 + Bootstrap Icons + SQL Server + EF Core + Stored Procedure + Session.

Luật cứng:
Không dùng React.
Không dùng Tailwind.
Không dùng shadcn/ui.
Không dùng GSAP.
Không dùng Lenis.
Không dùng 3D.
Không dùng custom cursor.
Không đổi sang Blazor.
Không đổi sang Web API.
Không đổi database/stored procedure nếu mục tiêu chỉ là UI.

Mục tiêu:
Redesign toàn bộ UI/UX theo kiểu admin dashboard hiện đại cho quán cà phê.

Dùng skill theo thứ tự:
1. penpot-uiux-design: lập design system, UX flow, layout, navigation.
2. premium-frontend-ui: polish Bootstrap 5 ở mức admin dashboard.
3. dotnet-best-practices: bảo vệ MVC, Razor, Session, Stored Procedure, validation.
4. refactor: tách partial/CSS, giảm lặp, không đổi hành vi.
5. web-design-reviewer: review responsive/accessibility/visual consistency.
6. webapp-testing: test flow thật.

Các nghiệp vụ không được phá:
- Login tự check-in.
- Logout yêu cầu mật khẩu để checkout.
- Sai mật khẩu không logout.
- Order kiểm tra nhân viên đang có ca làm.
- Order chỉ lấy sản phẩm Đang bán.
- Giá lấy lại từ SQL khi POST.
- Tổng tiền/khuyến mãi do SQL tính.
- Bàn MVE là mang về.
- Bàn ngồi tại quán lấy bàn Trống.
- Trả bàn đưa bàn về Trống.
- Không nhập computed columns.
- Dashboard/thống kê lấy dữ liệu động từ SQL.

Hãy sửa theo từng đợt nhỏ:
Đợt 1: _Layout.cshtml + site.css + sidebar/topbar.
Đợt 2: Dashboard.
Đợt 3: Order + Receipt.
Đợt 4: CRUD pages.
Đợt 5: Kho/Phiếu nhập/Chấm công/Lương/Thống kê.
Đợt 6: Review + test.

Sau mỗi đợt, báo:
- File đã sửa.
- Lý do sửa.
- Logic backend nào đã được giữ nguyên.
- Cách test nhanh.
```
