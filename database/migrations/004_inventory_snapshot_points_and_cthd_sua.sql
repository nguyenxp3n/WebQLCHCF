-- ==============================================================================
-- Migration: 004_inventory_snapshot_points_and_cthd_sua.sql
-- Purpose: 
-- 1. Add DaCongDiem column to HoaDon for loyalty points idempotency tracking.
-- 2. Create LichSuTruKho table for immutable inventory deduction snapshots.
-- 3. Harden CTHD_Sua stored procedure with UPDLOCK, HOLDLOCK and state guard.
-- 4. Enhance ThanhToan_HuyOrder with @ChoPhepKhiDaThanhToan flag for Manager void/refund.
-- ==============================================================================

USE QLCHCF;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Add DaCongDiem column to dbo.HoaDon
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.HoaDon') 
      AND name = 'DaCongDiem'
)
BEGIN
    PRINT '==> [Migration 004] Adding column HoaDon.DaCongDiem BIT NOT NULL DEFAULT 0...';
    ALTER TABLE dbo.HoaDon ADD DaCongDiem BIT NOT NULL CONSTRAINT DF_HoaDon_DaCongDiem DEFAULT 0;
    PRINT '==> [Migration 004] Column HoaDon.DaCongDiem added successfully.';
END
ELSE
BEGIN
    PRINT '==> [Migration 004] Column HoaDon.DaCongDiem already exists. Skipped.';
END
GO

-- 2. Create LichSuTruKho table for stock deduction snapshot
IF NOT EXISTS (
    SELECT 1 
    FROM sys.tables 
    WHERE name = 'LichSuTruKho' 
      AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    PRINT '==> [Migration 004] Creating table dbo.LichSuTruKho...';
    CREATE TABLE dbo.LichSuTruKho (
        MaLichSu INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaHD VARCHAR(9) NOT NULL,
        MaNL VARCHAR(5) NOT NULL,
        SoLuongTruThucTe INT NOT NULL,
        ThoiGianTru DATETIME2 NOT NULL CONSTRAINT DF_LichSuTruKho_ThoiGianTru DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_LichSuTruKho_HoaDon FOREIGN KEY (MaHD) REFERENCES dbo.HoaDon(MaHD) ON DELETE CASCADE,
        CONSTRAINT FK_LichSuTruKho_NguyenLieu FOREIGN KEY (MaNL) REFERENCES dbo.NguyenLieu(MaNL)
    );
    CREATE NONCLUSTERED INDEX IX_LichSuTruKho_MaHD ON dbo.LichSuTruKho(MaHD);
    PRINT '==> [Migration 004] Table dbo.LichSuTruKho created successfully.';
END
ELSE
BEGIN
    PRINT '==> [Migration 004] Table dbo.LichSuTruKho already exists. Skipped.';
END
GO

-- 3. Update CTHD_Sua with concurrency lock and immutability guard
CREATE OR ALTER PROC [dbo].[CTHD_Sua]
    @MaHD varchar(9),
    @MaSP varchar(5),
    @SoLuong int,
    @DonGia money,
    @MaKM varchar(5) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;

        DECLARE @TrangThaiOrder nvarchar(30), @TrangThaiThanhToan nvarchar(30);
        SELECT @TrangThaiOrder = TrangThaiOrder, @TrangThaiThanhToan = TrangThaiThanhToan 
        FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
        WHERE MaHD = @MaHD;

        IF @TrangThaiOrder IS NULL
        BEGIN
            THROW 50001, N'Hóa đơn không tồn tại.', 1;
        END

        IF @TrangThaiOrder = N'DaHuy' OR @TrangThaiThanhToan = N'DaThanhToan' OR @TrangThaiThanhToan = N'ChoThanhToanQR'
        BEGIN
            THROW 50006, N'Hóa đơn đã thanh toán hoặc đã hủy, không thể chỉnh sửa chi tiết món.', 1;
        END

        UPDATE CTHD 
        SET SoLuong = @SoLuong, DonGia = @DonGia, MaKM = @MaKM 
        WHERE MaHD = @MaHD AND MaSP = @MaSP;

        EXEC HoaDon_CapNhatTongTien @MaHD;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END;
GO

-- 4. Update ThanhToan_HuyOrder supporting manager refund/cancellation
CREATE OR ALTER PROC [dbo].[ThanhToan_HuyOrder]
    @MaHD varchar(9),
    @LyDo nvarchar(255) = NULL,
    @ChoPhepKhiDaThanhToan bit = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE
        @MaBan varchar(5),
        @TrangThaiThanhToan nvarchar(30);

    SELECT
        @MaBan = MaBan,
        @TrangThaiThanhToan = TrangThaiThanhToan
    FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
    WHERE MaHD = @MaHD;

    IF @MaBan IS NULL
    BEGIN
        RAISERROR(N'Hóa đơn không tồn tại.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TrangThaiThanhToan = N'DaThanhToan' AND @ChoPhepKhiDaThanhToan = 0
    BEGIN
        RAISERROR(N'Hóa đơn đã thanh toán, không được hủy.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TrangThaiThanhToan = N'DaThanhToan' AND @ChoPhepKhiDaThanhToan = 1
    BEGIN
        UPDATE dbo.ThanhToan
        SET TrangThai = N'DaHuy',
            GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | Quản lý hoàn đơn/hủy thanh toán. ', ISNULL(@LyDo, N'')), 255)
        WHERE MaHD = @MaHD AND TrangThai = N'DaThanhToan';

        UPDATE dbo.HoaDon
        SET TrangThaiOrder = N'DaHuy',
            TrangThaiThanhToan = N'DaHuy',
            GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | Quản lý hoàn đơn: ', ISNULL(@LyDo, N'Không có')), 100)
        WHERE MaHD = @MaHD;
    END
    ELSE
    BEGIN
        UPDATE dbo.ThanhToan
        SET TrangThai = N'DaHuy',
            GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | Hủy order. ', ISNULL(@LyDo, N'')), 255)
        WHERE MaHD = @MaHD
          AND TrangThai IN (N'ChuaThanhToan', N'ChoThanhToanQR', N'HetHan', N'ChoKiemTra');

        UPDATE dbo.HoaDon
        SET TrangThaiOrder = N'DaHuy',
            TrangThaiThanhToan = N'DaHuy',
            GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | Lý do hủy: ', ISNULL(@LyDo, N'Không có')), 100)
        WHERE MaHD = @MaHD;
    END;

    IF @MaBan IS NOT NULL AND @MaBan <> 'MVE'
    BEGIN
        IF NOT EXISTS (
            SELECT 1 
            FROM dbo.HoaDon 
            WHERE MaBan = @MaBan 
              AND MaHD <> @MaHD 
              AND TrangThaiOrder <> N'DaHuy' 
              AND TrangThaiThanhToan <> N'DaThanhToan'
        )
        BEGIN
            UPDATE dbo.Ban
            SET TrangThai = N'Trống'
            WHERE MaBan = @MaBan;
        END;
    END;

    COMMIT TRANSACTION;

    SELECT 
        1 AS Success, 
        CASE 
            WHEN @TrangThaiThanhToan = N'DaThanhToan' THEN N'Hoàn đơn và hủy thanh toán thành công.'
            ELSE N'Hủy order thành công.'
        END AS Message;
END;
GO

PRINT '==> [Migration 004] Completed successfully.';
