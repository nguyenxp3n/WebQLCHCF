# Test Checklist sau redesign

## Build

```powershell
dotnet restore
dotnet build
dotnet run
```

Yêu cầu:
```text
0 error
0 warning nếu trước đó đang 0 warning
```

## Test nghiệp vụ

- Login `qly_tram / 12345678`.
- Dashboard hiển thị dữ liệu động.
- Order ngồi tại quán.
- Order mang về `MVE`.
- Receipt hiển thị đúng tổng tiền.
- In bill không in sidebar/topbar.
- Trả bàn đưa bàn về `Trống`.
- CRUD sản phẩm không hiện `ThanhTien` computed.
- Phiếu nhập cập nhật tồn kho.
- Logout sai mật khẩu không xóa session.
- Logout đúng mật khẩu checkout.

## Test responsive

- 1440px.
- 1366px.
- 768px.
- 375px.

## Test accessibility

- Input có label.
- Button icon-only có `aria-label` hoặc text.
- Focus state rõ.
- Contrast đủ đọc.
- Không chỉ dùng màu để truyền trạng thái.
