# PROJECT_CONTEXT_WebQLCHCF

Dự án: WebQLCHCF - Hệ thống quản lý quán cà phê.

Stack bắt buộc:
- ASP.NET Core MVC.
- Razor Views.
- SQL Server.
- Entity Framework Core.
- Stored Procedure.
- Bootstrap 5.
- Bootstrap Icons.
- Session.

Không dùng:
- React.
- Tailwind.
- shadcn/ui.
- GSAP.
- Lenis.
- 3D.
- custom cursor.
- Blazor.
- Web API nếu project hiện tại không dùng.
- Docker hoặc Clean Architecture nếu chỉ redesign UI.

Mục tiêu của workflow:
Redesign UI/UX toàn bộ web quản lý quán cà phê bằng Bootstrap 5 + Razor Views, giữ nguyên backend, database, stored procedure, session và logic nghiệp vụ.

Các module chính:
- Account/Login/Logout/Session/Check-in.
- Dashboard.
- Order tại quầy.
- Receipt/In bill.
- Sản phẩm.
- Loại sản phẩm.
- Bàn.
- Khách hàng.
- Khuyến mãi.
- Nguyên liệu.
- Công thức.
- Hóa đơn.
- Chi tiết hóa đơn.
- Phiếu nhập.
- Chi tiết phiếu nhập.
- Chấm công.
- Lương tháng.
- Thống kê.

Logic nghiệp vụ cần bảo vệ:
- Login thành công tự check-in.
- Logout phải nhập mật khẩu để checkout.
- Sai mật khẩu không logout, không checkout, không xóa session.
- Order kiểm tra nhân viên đang có ca làm.
- Order chỉ lấy sản phẩm `Đang bán`.
- Giá sản phẩm lấy lại từ SQL khi POST.
- Tổng tiền hóa đơn do SQL tính.
- Khuyến mãi do SQL tính, C# không trừ lần hai.
- Bàn `MVE` dùng cho mang về và không tính vào bàn đang hoạt động.
- Bàn ngồi tại quán lấy bàn `Trống`.
- Trả bàn đưa bàn về `Trống`.
- Không nhập/sửa computed columns: `CTHD.ThanhTien`, `CTPN.ThanhTien`, `Luong.TongLuong`.
- CTPN_Them/Sua/Xoa cập nhật tồn kho theo stored procedure.
- Chấm công mỗi nhân viên chỉ có một ca `Đang làm`.
