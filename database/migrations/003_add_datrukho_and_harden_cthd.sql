-- ==============================================================================
-- Migration: 003_add_datrukho_and_harden_cthd.sql
-- Purpose: Add DaTruKho column to HoaDon for idempotent inventory tracking
--          and enforce invoice immutability in CTHD stored procedures for DaHuy.
-- ==============================================================================

USE QLCHCF;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Add DaTruKho column to HoaDon if it does not exist
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.HoaDon') 
      AND name = 'DaTruKho'
)
BEGIN
    PRINT '==> [Migration 003] Adding column HoaDon.DaTruKho BIT NOT NULL DEFAULT 0...';
    ALTER TABLE dbo.HoaDon ADD DaTruKho BIT NOT NULL CONSTRAINT DF_HoaDon_DaTruKho DEFAULT 0;
    PRINT '==> [Migration 003] Column HoaDon.DaTruKho added successfully.';
END
ELSE
BEGIN
    PRINT '==> [Migration 003] Column HoaDon.DaTruKho already exists. Skipped.';
END
GO

-- 2. Update CTHD_Them stored procedure to reject additions on DaHuy or DaThanhToan invoices
CREATE OR ALTER PROC [dbo].[CTHD_Them]
    @MaHD varchar(9),
    @MaSP varchar(5),
    @SoLuong int,
    @DonGia money = NULL,
    @MaKM varchar(5) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;

        -- Validate invoice state
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
            THROW 50006, N'Hóa đơn đã thanh toán hoặc đã hủy, không thể thêm chi tiết món.', 1;
        END

        IF @DonGia IS NULL
            SELECT @DonGia = DonGia FROM SanPham WHERE MaSP = @MaSP;

        IF EXISTS (SELECT 1 FROM CTHD WITH (UPDLOCK, HOLDLOCK) WHERE MaHD = @MaHD AND MaSP = @MaSP)
        BEGIN
            UPDATE CTHD 
            SET SoLuong = SoLuong + @SoLuong, DonGia = @DonGia, MaKM = @MaKM 
            WHERE MaHD = @MaHD AND MaSP = @MaSP;
        END
        ELSE
        BEGIN
            INSERT INTO CTHD(MaHD, MaSP, SoLuong, DonGia, MaKM)
            VALUES(@MaHD, @MaSP, @SoLuong, @DonGia, @MaKM);
        END

        EXEC HoaDon_CapNhatTongTien @MaHD;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END;
GO

-- 3. Update CTHD_Xoa stored procedure to reject deletions on DaHuy or DaThanhToan invoices
CREATE OR ALTER PROC [dbo].[CTHD_Xoa]
    @MaHD varchar(9),
    @MaSP varchar(5)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
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
            THROW 50006, N'Hóa đơn đã thanh toán hoặc đã hủy, không thể xóa chi tiết món.', 1;
        END

        DELETE FROM CTHD WHERE MaHD = @MaHD AND MaSP = @MaSP;
        EXEC HoaDon_CapNhatTongTien @MaHD;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END;
GO

PRINT '==> [Migration 003] Completed successfully.';
