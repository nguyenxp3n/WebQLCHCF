Dựa trên `QLCHCF_Full.sql`, hãy tạo:

1. Models cho đúng 14 bảng.
2. ViewModels cần thiết cho stored procedure join/thống kê.
3. `Data/AppDbContext.cs`.

Yêu cầu:

- Bám sát 100% tên bảng/cột SQL.
- Cấu hình khóa chính đơn và khóa chính kép.
- Cấu hình computed column.
- Cấu hình HasNoKey cho ViewModels.
- Cấu hình money/decimal đúng.
- Không tạo property ngoài SQL trong entity model, trừ navigation property nếu cần.
