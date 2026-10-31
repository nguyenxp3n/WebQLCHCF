# Bản Hướng Dẫn Ghi CV & Kịch Bản Phỏng Vấn: Dự Án WebQLCHCF

Tài liệu hướng dẫn cách đưa dự án **WebQLCHCF (Hệ Thống Quản Lý Quán Cà Phê & Chấm Công POS)** vào CV tuyển dụng, hồ sơ năng lực (Portfolio), LinkedIn và kịch bản bảo vệ kỹ thuật trước nhà tuyển dụng.

---

## 1. Tóm Tắt Dự Án Chuẩn STAR Để Ghi Vào CV

### Mẫu 1: Dành cho vị trí Backend / .NET Software Engineer

> **Hệ Thống Quản Lý Quán Cà Phê & POS (WebQLCHCF) | .NET Fullstack Developer & Test Lead**  
> *Công nghệ: ASP.NET Core 10 (MVC), SQL Server 2022, EF Core, Docker, xUnit, GitHub Actions, Bootstrap 5, VietQR*  
> - Thiết kế và xây dựng hệ thống quản lý quán cà phê đa phân hệ gồm **18 Controller, 17 bảng cơ sở dữ liệu và 16 khóa ngoại**, phục vụ trọn vẹn quy trình bán hàng tại quầy (POS), điều phối bàn thời gian thực và quản lý nguyên liệu định lượng.  
> - Trích xuất các Domain Engine độc lập (`CafeOrderCalculator`, `PayrollCalculator`), xây dựng bộ kiểm thử tự động **77 test cases với xUnit** bao phủ toàn bộ các ca biên (giảm giá âm, chiết khấu > 100%, ca làm xuyên ngày, độ phức tạp mật khẩu) và tích hợp GitHub Actions CI.  
> - Kiểm soát toàn vẹn giao dịch và phòng ngừa race condition bằng khóa bi quan `WITH (UPDLOCK, HOLDLOCK)` trong Stored Procedure tính lương, tích hợp thanh toán mã động VietQR không tiếp xúc, và container hóa toàn diện với Docker Compose.

---

### Mẫu 2: Dành cho vị trí Software QA / Automation Test Engineer

> **Hệ Thống Quản Lý Bán Hàng & Chấm Công POS (WebQLCHCF) | QA Engineer & Core Developer**  
> *Kỹ năng: xUnit, Unit Testing, Boundary Value Analysis, Equivalence Partitioning, GitHub Actions CI, SQL Server, Docker*  
> - Phân tích tài liệu đặc tả yêu cầu phần mềm (SRS), thiết kế ma trận truy xuất nguồn gốc (RTM) và triển khai bộ 52 kịch bản kiểm thử chức năng/phi chức năng cho hệ thống POS cà phê.  
> - Phát triển bộ kiểm thử tự động 77 test cases trên nền xUnit và .NET 10, áp dụng kỹ thuật kiểm thử phân lớp tương đương và giá trị biên cho các bài toán tính chiết khấu khuyến mãi, tính giờ làm việc lũy tiến và phân quyền 3 cấp (`RoleAuthorize`).  
> - Thiết lập pipeline CI/CD trên GitHub Actions tự động build và chạy test trên môi trường Ubuntu Container, duy trì tỷ lệ vượt qua kiểm thử đạt 100%.

---

## 2. Kịch Bản Trả Lời Phỏng Vấn Kỹ Thuật (Defense Playbook)

### Câu hỏi 1: Tại sao bạn lại tách `CafeOrderCalculator` và `PayrollCalculator` ra khỏi Controller?
* **Trả lời:**  
  *"Trong các hệ thống quản lý giao dịch và nhân sự, việc nhồi nhét logic tính tiền hoặc quy đổi giờ làm trực tiếp trong Controller hoặc DbContext sẽ gây ra hiện tượng coupling chặt chẽ, rất khó mock dữ liệu và kiểm thử tự động.  
  Em trích xuất các quy tắc này thành các Pure Domain Engine (không phụ thuộc HttpContext hay Database). Nhờ đó, em có thể viết 77 bài kiểm thử xUnit tham số hóa chạy cực nhanh (dưới 500ms) để quét sạch mọi trường hợp biên như giảm giá vượt quá 100%, đơn hàng giá trị 0đ, hoặc ca làm check-in lộn xộn trước khi dữ liệu được ghi vào cơ sở dữ liệu."*

---

### Câu hỏi 2: Bạn giải quyết bài toán xung đột dữ liệu (Concurrency / Race Condition) khi nhân viên cùng thao tác như thế nào?
* **Trả lời:**  
  *"Ở module chấm công và tính lương ca làm (`ChamCong_CheckOut`), khi nhân viên check-out, hệ thống cần đọc bản ghi ca đang làm, tính tổng phút và khóa trạng thái ca. Nếu hai tiến trình can thiệp đồng thời, có thể dẫn đến việc tính lương lặp lại.  
  Em đã áp dụng cơ chế khóa bi quan (Pessimistic Locking) thông qua gợi ý truy vấn `WITH (UPDLOCK, HOLDLOCK)` ngay trong transaction của SQL Server. Khóa này bảo vệ hàng dữ liệu trong suốt vòng đời của transaction, ngăn chặn hoàn toàn race condition."*

---

### Câu hỏi 3: Cơ chế tích hợp thanh toán VietQR của hệ thống hoạt động ra sao?
* **Trả lời:**  
  *"Em triển khai cơ chế sinh mã VietQR động theo chuẩn NAPAS. Khi nhân viên chọn phương thức thanh toán chuyển khoản, hệ thống tạo ra một phiên thanh toán (`MaPhienThanhToan`) và gọi hàm `CafeOrderCalculator.SinhUrlVietQr` để sinh URL ảnh QR chứa chính xác số tiền cần thanh toán và mã hóa nội dung chuyển khoản bằng `WebUtility.UrlEncode`. Khách hàng quét mã là toàn bộ thông tin tài khoản và số tiền được điền tự động trên app ngân hàng, triệt tiêu sai sót do khách nhập tay."*

---

### Câu hỏi 4: Dự án này được đóng gói và kiểm thử liên tục như thế nào?
* **Trả lời:**  
  *"Dự án được container hóa hoàn chỉnh bằng Docker Compose với cụm dịch vụ gồm SQL Server 2022 và Web App ASP.NET Core 10. File `init.sql` được container `db-init` tự động nạp khi SQL Server sẵn sàng nhận kết nối.  
  Đồng thời, em xây dựng workflow GitHub Actions chạy trên mỗi lệnh push hoặc Pull Request. Pipeline sẽ restore, build và chạy toàn bộ 77 bài test tự động, đảm bảo mã nguồn luôn ở trạng thái pass 100% trước khi triển khai."*
