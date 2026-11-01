-- ==============================================================================
-- Migration: 002_add_client_request_id_to_hoadon.sql
-- Purpose: Add dedicated ClientRequestId column with UNIQUE INDEX to HoaDon
--          and update HoaDon_Them stored procedure for unbreakable idempotency.
-- ==============================================================================

USE QLCHCF;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Add ClientRequestId column to HoaDon if it does not exist
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.HoaDon') 
      AND name = 'ClientRequestId'
)
BEGIN
    PRINT '==> [Migration 002] Adding column HoaDon.ClientRequestId VARCHAR(36) NULL...';
    ALTER TABLE dbo.HoaDon ADD ClientRequestId VARCHAR(36) NULL;
    PRINT '==> [Migration 002] Column HoaDon.ClientRequestId added successfully.';
END
ELSE
BEGIN
    PRINT '==> [Migration 002] Column HoaDon.ClientRequestId already exists. Skipped.';
END
GO

-- 2. Create Unique Filtered Index on HoaDon.ClientRequestId
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE object_id = OBJECT_ID('dbo.HoaDon') 
      AND name = 'UX_HoaDon_ClientRequestId'
)
BEGIN
    PRINT '==> [Migration 002] Creating UNIQUE INDEX UX_HoaDon_ClientRequestId...';
    CREATE UNIQUE NONCLUSTERED INDEX UX_HoaDon_ClientRequestId 
    ON dbo.HoaDon(ClientRequestId) 
    WHERE ClientRequestId IS NOT NULL;
    PRINT '==> [Migration 002] Index UX_HoaDon_ClientRequestId created successfully.';
END
ELSE
BEGIN
    PRINT '==> [Migration 002] Index UX_HoaDon_ClientRequestId already exists. Skipped.';
END
GO

-- 3. Update HoaDon_Them stored procedure to support @ClientRequestId with internal idempotency guard
CREATE OR ALTER PROC [dbo].[HoaDon_Them]
    @MaHD varchar(9) = NULL,
    @NgayLap date,
    @MaNV varchar(5),
    @MaKH varchar(5) = NULL,
    @GhiChu nvarchar(100) = NULL,
    @MaBan varchar(5),
    @ClientRequestId varchar(36) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        
        -- Idempotency Guard inside transaction:
        IF @ClientRequestId IS NOT NULL AND LTRIM(RTRIM(@ClientRequestId)) <> ''
        BEGIN
            DECLARE @ExistingMaHD varchar(9);
            SELECT @ExistingMaHD = MaHD 
            FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK) 
            WHERE ClientRequestId = @ClientRequestId;

            IF @ExistingMaHD IS NOT NULL
            BEGIN
                COMMIT;
                RETURN;
            END
        END

        IF @MaHD IS NULL OR LTRIM(RTRIM(@MaHD)) = ''
        BEGIN
            DECLARE @Prefix varchar(6) = FORMAT(@NgayLap, 'yyMMdd');
            DECLARE @MaxNumber int;
            SELECT @MaxNumber = MAX(TRY_CONVERT(int, RIGHT(MaHD, 3))) 
            FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK) 
            WHERE MaHD LIKE @Prefix + '%';
            SET @MaHD = dbo.fn_NextCode(@Prefix, @MaxNumber, 3);
        END

        INSERT INTO dbo.HoaDon(MaHD, NgayLap, MaNV, MaKH, TongTien, GhiChu, MaBan, ClientRequestId)
        VALUES(@MaHD, @NgayLap, @MaNV, @MaKH, 0, @GhiChu, @MaBan, @ClientRequestId);

        COMMIT;
    END TRY 
    BEGIN CATCH 
        IF @@TRANCOUNT > 0 ROLLBACK; 
        THROW; 
    END CATCH
END;
GO

PRINT '==> [Migration 002] Completed successfully.';
GO
