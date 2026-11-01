-- ==============================================================================
-- Migration: 001_expand_password_column.sql
-- Purpose: Safely expand TaiKhoan.MatKhau column to VARCHAR(255) for PBKDF2 hash
--          and update related stored procedures on existing databases.
-- ==============================================================================

USE QLCHCF;
GO

-- 1. Check and alter column TaiKhoan.MatKhau if length is less than 255
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.TaiKhoan') 
      AND name = 'MatKhau' 
      AND max_length < 255
)
BEGIN
    PRINT '==> [Migration 001] Altering column TaiKhoan.MatKhau to VARCHAR(255)...';
    ALTER TABLE dbo.TaiKhoan ALTER COLUMN MatKhau VARCHAR(255) NOT NULL;
    PRINT '==> [Migration 001] Column TaiKhoan.MatKhau successfully updated to VARCHAR(255).';
END
ELSE
BEGIN
    PRINT '==> [Migration 001] Column TaiKhoan.MatKhau is already VARCHAR(255) or greater. Skipped.';
END
GO

-- 2. Ensure TaiKhoan_Them accepts @MatKhau VARCHAR(255)
CREATE OR ALTER PROCEDURE dbo.TaiKhoan_Them
    @MaTK VARCHAR(10),
    @TenDangNhap VARCHAR(50),
    @MatKhau VARCHAR(255),
    @MaNV VARCHAR(5),
    @VaiTro NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TaiKhoan (MaTK, TenDangNhap, MatKhau, MaNV, VaiTro)
    VALUES (@MaTK, @TenDangNhap, @MatKhau, @MaNV, @VaiTro);
END;
GO

-- 3. Ensure TaiKhoan_Sua accepts @MatKhau VARCHAR(255)
CREATE OR ALTER PROCEDURE dbo.TaiKhoan_Sua
    @MaTK VARCHAR(10),
    @TenDangNhap VARCHAR(50),
    @MatKhau VARCHAR(255),
    @MaNV VARCHAR(5),
    @VaiTro NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.TaiKhoan
    SET TenDangNhap = @TenDangNhap,
        MatKhau = @MatKhau,
        MaNV = @MaNV,
        VaiTro = @VaiTro
    WHERE MaTK = @MaTK;
END;
GO

PRINT '==> [Migration 001] Completed successfully.';
GO
