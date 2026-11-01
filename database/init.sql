USE [master]
GO

/* =========================================================
   SCRIPT CÀI ĐẶT LẠI DATABASE QLCHCF
   - Nếu đã có database [QLCHCF] thì tự ngắt kết nối, xóa DB cũ
   - Tạo database mới không gắn đường dẫn MDF/LDF cố định
   - Sau đó chạy toàn bộ schema + dữ liệu + view/function/procedure
   ========================================================= */

IF DB_ID(N'QLCHCF') IS NULL
BEGIN
    CREATE DATABASE [QLCHCF];
END
GO

ALTER DATABASE [QLCHCF] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [QLCHCF].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [QLCHCF] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [QLCHCF] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [QLCHCF] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [QLCHCF] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [QLCHCF] SET ARITHABORT OFF 
GO
ALTER DATABASE [QLCHCF] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [QLCHCF] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [QLCHCF] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [QLCHCF] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [QLCHCF] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [QLCHCF] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [QLCHCF] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [QLCHCF] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [QLCHCF] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [QLCHCF] SET  ENABLE_BROKER 
GO
ALTER DATABASE [QLCHCF] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [QLCHCF] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [QLCHCF] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [QLCHCF] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [QLCHCF] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [QLCHCF] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [QLCHCF] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [QLCHCF] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [QLCHCF] SET  MULTI_USER 
GO
ALTER DATABASE [QLCHCF] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [QLCHCF] SET DB_CHAINING OFF 
GO
ALTER DATABASE [QLCHCF] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [QLCHCF] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [QLCHCF] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [QLCHCF] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [QLCHCF] SET QUERY_STORE = ON
GO
ALTER DATABASE [QLCHCF] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [QLCHCF]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_NextCode]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   3. HÀM PHỤ TRỢ
   ========================================================= */

CREATE   FUNCTION [dbo].[fn_NextCode]
(
    @Prefix varchar(10),
    @MaxNumber int,
    @MinDigits int
)
RETURNS varchar(20)
AS
BEGIN
    DECLARE @NextNumber int = ISNULL(@MaxNumber, 0) + 1;
    DECLARE @NextText varchar(20) = CAST(@NextNumber AS varchar(20));
    DECLARE @PadLength int = CASE WHEN LEN(@NextText) > @MinDigits THEN LEN(@NextText) ELSE @MinDigits END;
    RETURN @Prefix + RIGHT(REPLICATE('0', @PadLength) + @NextText, @PadLength);
END;



GO
/****** Object:  Table [dbo].[AuditLog]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditLog](
	[MaLog] [bigint] IDENTITY(1,1) NOT NULL,
	[ThoiGian] [datetime2](0) NOT NULL,
	[MaNV] [nvarchar](50) NULL,
	[TenDangNhap] [nvarchar](100) NULL,
	[VaiTro] [nvarchar](100) NULL,
	[ChucNang] [nvarchar](100) NOT NULL,
	[HanhDong] [nvarchar](100) NOT NULL,
	[DoiTuong] [nvarchar](100) NULL,
	[MaDoiTuong] [nvarchar](100) NULL,
	[NoiDung] [nvarchar](1000) NOT NULL,
	[DuLieuTruoc] [nvarchar](max) NULL,
	[DuLieuSau] [nvarchar](max) NULL,
	[DiaChiIP] [nvarchar](100) NULL,
	[UserAgent] [nvarchar](500) NULL,
 CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED 
(
	[MaLog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_AuditLog_GanDay]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   9. View xem log nhanh
   ========================================================= */

CREATE   VIEW [dbo].[vw_AuditLog_GanDay]
AS
SELECT TOP 500
    MaLog,
    ThoiGian,
    MaNV,
    TenDangNhap,
    VaiTro,
    ChucNang,
    HanhDong,
    DoiTuong,
    MaDoiTuong,
    NoiDung,
    DiaChiIP
FROM dbo.AuditLog
ORDER BY ThoiGian DESC, MaLog DESC;
GO
/****** Object:  Table [dbo].[Ban]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ban](
	[MaBan] [varchar](5) NOT NULL,
	[TenBan] [nvarchar](20) NOT NULL,
	[SoBan] [int] NULL,
	[KhuVuc] [nvarchar](20) NOT NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_Ban] PRIMARY KEY CLUSTERED 
(
	[MaBan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChamCong]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChamCong](
	[MaCC] [int] IDENTITY(1,1) NOT NULL,
	[MaNV] [varchar](5) NOT NULL,
	[ThoiGianCheckIn] [datetime2](0) NOT NULL,
	[ThoiGianCheckOut] [datetime2](0) NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
	[GhiChu] [nvarchar](200) NULL,
	[DonGiaGio] [money] NOT NULL,
	[TongPhut] [int] NULL,
	[TongGio] [decimal](10, 2) NULL,
	[TienCong] [money] NULL,
 CONSTRAINT [PK_ChamCong] PRIMARY KEY CLUSTERED 
(
	[MaCC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CongThuc]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CongThuc](
	[MaSP] [varchar](5) NOT NULL,
	[MaNL] [varchar](5) NOT NULL,
	[DinhLuong] [decimal](18, 3) NOT NULL,
 CONSTRAINT [PK_CongThuc] PRIMARY KEY CLUSTERED 
(
	[MaSP] ASC,
	[MaNL] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CTHD]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CTHD](
	[MaHD] [varchar](9) NOT NULL,
	[MaSP] [varchar](5) NOT NULL,
	[SoLuong] [int] NOT NULL,
	[DonGia] [money] NOT NULL,
	[ThanhTien]  AS ([SoLuong]*[DonGia]) PERSISTED,
	[MaKM] [varchar](5) NULL,
 CONSTRAINT [PK_CTHD] PRIMARY KEY CLUSTERED 
(
	[MaHD] ASC,
	[MaSP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CTPN]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CTPN](
	[MaPN] [varchar](9) NOT NULL,
	[MaNL] [varchar](5) NOT NULL,
	[SoLuong] [int] NOT NULL,
	[DonGia] [money] NOT NULL,
	[ThanhTien]  AS ([SoLuong]*[DonGia]) PERSISTED,
 CONSTRAINT [PK_CTPN] PRIMARY KEY CLUSTERED 
(
	[MaPN] ASC,
	[MaNL] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HoaDon]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HoaDon](
	[MaHD] [varchar](9) NOT NULL,
	[NgayLap] [date] NOT NULL,
	[MaNV] [varchar](5) NOT NULL,
	[MaKH] [varchar](5) NULL,
	[TongTien] [money] NOT NULL,
	[GhiChu] [nvarchar](100) NULL,
	[MaBan] [varchar](5) NOT NULL,
	[TrangThaiOrder] [nvarchar](30) NOT NULL,
	[TrangThaiThanhToan] [nvarchar](30) NOT NULL,
	[PhuongThucThanhToan] [nvarchar](30) NULL,
	[ThoiGianTao] [datetime2](0) NOT NULL,
	[ThoiGianThanhToan] [datetime2](0) NULL,
	[ClientRequestId] [varchar](36) NULL,
	[DaTruKho] [bit] NOT NULL CONSTRAINT [DF_HoaDon_DaTruKho] DEFAULT ((0)),
	[DaCongDiem] [bit] NOT NULL CONSTRAINT [DF_HoaDon_DaCongDiem] DEFAULT ((0)),
 CONSTRAINT [PK_HoaDon] PRIMARY KEY CLUSTERED 
(
	[MaHD] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_HoaDon_ClientRequestId] ON [dbo].[HoaDon]
(
	[ClientRequestId] ASC
)
WHERE ([ClientRequestId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KhachHang]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KhachHang](
	[MaKH] [varchar](5) NOT NULL,
	[TenKH] [nvarchar](50) NOT NULL,
	[SDT] [varchar](10) NOT NULL,
	[DiemTichLuy] [int] NULL,
 CONSTRAINT [PK_KhachHang] PRIMARY KEY CLUSTERED 
(
	[MaKH] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KhuyenMai]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KhuyenMai](
	[MaKM] [varchar](5) NOT NULL,
	[TenKM] [nvarchar](255) NOT NULL,
	[LoaiKM] [nvarchar](50) NOT NULL,
	[GiaTri] [decimal](18, 2) NOT NULL,
	[NgayBatDau] [date] NOT NULL,
	[NgayKetThuc] [date] NOT NULL,
	[DieuKienApDung] [nvarchar](500) NOT NULL,
	[TrangThai] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_KhuyenMai] PRIMARY KEY CLUSTERED 
(
	[MaKM] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LoaiSP]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoaiSP](
	[MaLoaiSP] [varchar](5) NOT NULL,
	[TenLoaiSP] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_LoaiSP] PRIMARY KEY CLUSTERED 
(
	[MaLoaiSP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Luong]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Luong](
	[MaLuong] [int] NOT NULL,
	[Thang] [int] NOT NULL,
	[Nam] [int] NOT NULL,
	[LuongCoBan] [money] NOT NULL,
	[PhuCap] [money] NULL,
	[Thuong] [money] NULL,
	[Phat] [money] NULL,
	[TongLuong]  AS ((([LuongCoBan]+isnull([PhuCap],(0)))+isnull([Thuong],(0)))-isnull([Phat],(0))) PERSISTED,
 CONSTRAINT [PK_Luong] PRIMARY KEY CLUSTERED 
(
	[MaLuong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NguyenLieu]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NguyenLieu](
	[MaNL] [varchar](5) NOT NULL,
	[TenNL] [nvarchar](50) NOT NULL,
	[DonViTinh] [nvarchar](20) NOT NULL,
	[SoLuongTon] [int] NULL,
	[GiaNhap] [money] NOT NULL,
	[NgayCapNhat] [date] NOT NULL,
	[MucTonToiThieu] [int] NOT NULL,
 CONSTRAINT [PK_NguyenLieu] PRIMARY KEY CLUSTERED 
(
	[MaNL] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NhanVien]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NhanVien](
	[MaNV] [varchar](5) NOT NULL,
	[TenNV] [nvarchar](50) NOT NULL,
	[GioiTinh] [nvarchar](5) NOT NULL,
	[NgaySinh] [date] NULL,
	[SDT] [varchar](10) NOT NULL,
	[ChucVu] [nvarchar](20) NOT NULL,
	[MaLuong] [int] NOT NULL,
	[LuongTheoGio] [money] NOT NULL,
 CONSTRAINT [PK_NhanVien] PRIMARY KEY CLUSTERED 
(
	[MaNV] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PhieuNhap]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhieuNhap](
	[MaPN] [varchar](9) NOT NULL,
	[NgayNhap] [date] NOT NULL,
	[MaNV] [varchar](5) NOT NULL,
	[TongTien] [money] NOT NULL,
 CONSTRAINT [PK_PhieuNhap] PRIMARY KEY CLUSTERED 
(
	[MaPN] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SanPham]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SanPham](
	[MaSP] [varchar](5) NOT NULL,
	[TenSP] [nvarchar](50) NOT NULL,
	[DonGia] [money] NOT NULL,
	[MaLoaiSP] [varchar](5) NOT NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_SanPham] PRIMARY KEY CLUSTERED 
(
	[MaSP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TaiKhoan]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaiKhoan](
	[MaTK] [varchar](10) NOT NULL,
	[TenDangNhap] [varchar](50) NOT NULL,
	[MatKhau] [varchar](255) NOT NULL,
	[MaNV] [varchar](5) NOT NULL,
	[VaiTro] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_TaiKhoan] PRIMARY KEY CLUSTERED 
(
	[MaTK] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ThanhToan]    Script Date: 05/30/2026 11:12:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ThanhToan](
	[MaThanhToan] [int] IDENTITY(1,1) NOT NULL,
	[MaHD] [varchar](9) NULL,
	[MaPhienThanhToan] [nvarchar](50) NULL,
	[PhuongThuc] [nvarchar](30) NOT NULL,
	[SoTien] [money] NOT NULL,
	[NoiDungChuyenKhoan] [nvarchar](255) NULL,
	[MaGiaoDich] [nvarchar](100) NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
	[ThoiGianTao] [datetime2](0) NOT NULL,
	[ThoiGianHetHan] [datetime2](0) NULL,
	[ThoiGianThanhToan] [datetime2](0) NULL,
	[GhiChu] [nvarchar](255) NULL,
 CONSTRAINT [PK_ThanhToan] PRIMARY KEY CLUSTERED 
(
	[MaThanhToan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Ban] ([MaBan], [TenBan], [SoBan], [KhuVuc], [TrangThai]) VALUES (N'BAN01', N'Bàn 01', 1, N'Tầng trệt', N'Trống')
INSERT [dbo].[Ban] ([MaBan], [TenBan], [SoBan], [KhuVuc], [TrangThai]) VALUES (N'BAN02', N'Bàn 02', 2, N'Tầng trệt', N'Trống')
INSERT [dbo].[Ban] ([MaBan], [TenBan], [SoBan], [KhuVuc], [TrangThai]) VALUES (N'BAN03', N'Bàn 03', 3, N'Tầng trệt', N'Trống')
INSERT [dbo].[Ban] ([MaBan], [TenBan], [SoBan], [KhuVuc], [TrangThai]) VALUES (N'BAN04', N'Bàn 04', 4, N'Tầng lửng', N'Trống')
INSERT [dbo].[Ban] ([MaBan], [TenBan], [SoBan], [KhuVuc], [TrangThai]) VALUES (N'BAN05', N'Bàn 05', 5, N'Tầng lửng', N'Trống')
INSERT [dbo].[Ban] ([MaBan], [TenBan], [SoBan], [KhuVuc], [TrangThai]) VALUES (N'MVE', N'Mang về', NULL, N'Mang về', N'Trống')
GO
SET IDENTITY_INSERT [dbo].[ChamCong] ON 

INSERT [dbo].[ChamCong] ([MaCC], [MaNV], [ThoiGianCheckIn], [ThoiGianCheckOut], [TrangThai], [GhiChu], [DonGiaGio], [TongPhut], [TongGio], [TienCong]) VALUES (1, N'NV01', CAST(N'2026-05-28T21:27:59.0000000' AS DateTime2), CAST(N'2026-05-28T21:28:02.0000000' AS DateTime2), N'Đã checkout', N'Tự động check-in khi đăng nhập', 30000.0000, 1, CAST(0.02 AS Decimal(10, 2)), 500.0000)
INSERT [dbo].[ChamCong] ([MaCC], [MaNV], [ThoiGianCheckIn], [ThoiGianCheckOut], [TrangThai], [GhiChu], [DonGiaGio], [TongPhut], [TongGio], [TienCong]) VALUES (2, N'NV01', CAST(N'2026-05-28T21:32:23.0000000' AS DateTime2), CAST(N'2026-05-28T21:45:24.0000000' AS DateTime2), N'Đã checkout', N'Tự động check-in khi đăng nhập', 30000.0000, 13, CAST(0.22 AS Decimal(10, 2)), 6500.0000)
INSERT [dbo].[ChamCong] ([MaCC], [MaNV], [ThoiGianCheckIn], [ThoiGianCheckOut], [TrangThai], [GhiChu], [DonGiaGio], [TongPhut], [TongGio], [TienCong]) VALUES (3, N'NV01', CAST(N'2026-05-28T21:45:44.0000000' AS DateTime2), CAST(N'2026-05-28T21:47:14.0000000' AS DateTime2), N'Đã checkout', N'Tự động check-in khi đăng nhập', 30000.0000, 2, CAST(0.03 AS Decimal(10, 2)), 1000.0000)
INSERT [dbo].[ChamCong] ([MaCC], [MaNV], [ThoiGianCheckIn], [ThoiGianCheckOut], [TrangThai], [GhiChu], [DonGiaGio], [TongPhut], [TongGio], [TienCong]) VALUES (4, N'NV01', CAST(N'2026-05-28T21:50:02.0000000' AS DateTime2), CAST(N'2026-05-28T21:50:04.0000000' AS DateTime2), N'Đã checkout', N'Tự động check-in khi đăng nhập', 30000.0000, 0, CAST(0.00 AS Decimal(10, 2)), 0.0000)
INSERT [dbo].[ChamCong] ([MaCC], [MaNV], [ThoiGianCheckIn], [ThoiGianCheckOut], [TrangThai], [GhiChu], [DonGiaGio], [TongPhut], [TongGio], [TienCong]) VALUES (5, N'NV01', CAST(N'2026-05-28T21:51:26.0000000' AS DateTime2), CAST(N'2026-05-29T23:31:23.0000000' AS DateTime2), N'Đã checkout', N'Tự động check-in khi đăng nhập', 30000.0000, 1540, CAST(25.67 AS Decimal(10, 2)), 770000.0000)
INSERT [dbo].[ChamCong] ([MaCC], [MaNV], [ThoiGianCheckIn], [ThoiGianCheckOut], [TrangThai], [GhiChu], [DonGiaGio], [TongPhut], [TongGio], [TienCong]) VALUES (6, N'NV01', CAST(N'2026-05-29T23:31:38.0000000' AS DateTime2), CAST(N'2026-05-30T03:31:38.0000000' AS DateTime2), N'Đã checkout', N'Tự động check-in khi đăng nhập', 30000.0000, 240, CAST(4.00 AS Decimal(10, 2)), 120000.0000)
SET IDENTITY_INSERT [dbo].[ChamCong] OFF
GO
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP01', N'NL01', CAST(9.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP01', N'NL08', CAST(5.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP02', N'NL01', CAST(20.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP02', N'NL02', CAST(30.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP03', N'NL01', CAST(15.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP03', N'NL02', CAST(25.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP03', N'NL03', CAST(40.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP06', N'NL03', CAST(80.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP06', N'NL04', CAST(10.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP06', N'NL05', CAST(30.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP06', N'NL08', CAST(10.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP11', N'NL04', CAST(10.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP11', N'NL06', CAST(30.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP12', N'NL04', CAST(10.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP12', N'NL08', CAST(15.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP16', N'NL03', CAST(100.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP16', N'NL07', CAST(15.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP17', N'NL03', CAST(80.000 AS Decimal(18, 3)))
INSERT [dbo].[CongThuc] ([MaSP], [MaNL], [DinhLuong]) VALUES (N'SP17', N'NL07', CAST(20.000 AS Decimal(18, 3)))
GO
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260529002', N'SP03', 1, 28000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530001', N'SP03', 1, 28000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530002', N'SP03', 1, 28000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530003', N'SP03', 1, 28000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530004', N'SP08', 1, 40000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530004', N'SP11', 1, 37000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530005', N'SP07', 1, 38000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530006', N'SP05', 1, 32000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530007', N'SP02', 1, 30000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530007', N'SP04', 1, 35000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530008', N'SP04', 1, 35000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530009', N'SP03', 1, 28000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530010', N'SP03', 1, 28000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530011', N'SP03', 1, 28000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530012', N'SP03', 1, 28000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530013', N'SP03', 10, 28000.0000, NULL)
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530014', N'SP01', 2, 25000.0000, N'KM03')
INSERT [dbo].[CTHD] ([MaHD], [MaSP], [SoLuong], [DonGia], [MaKM]) VALUES (N'260530014', N'SP20', 3, 45000.0000, N'KM01')
GO
INSERT [dbo].[CTPN] ([MaPN], [MaNL], [SoLuong], [DonGia]) VALUES (N'PN2605001', N'NL01', 10, 100000.0000)
GO
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260529002', CAST(N'2026-05-29' AS Date), N'NV01', NULL, 28000.0000, NULL, N'BAN02', N'HoanThanh', N'DaThanhToan', N'QR', CAST(N'2026-05-29T23:32:28.0000000' AS DateTime2), CAST(N'2026-05-29T23:33:29.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530001', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 28000.0000, NULL, N'BAN01', N'HoanThanh', N'DaThanhToan', N'TienMat', CAST(N'2026-05-30T08:49:19.0000000' AS DateTime2), CAST(N'2026-05-30T08:49:28.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530002', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 28000.0000, NULL, N'BAN01', N'HoanThanh', N'DaThanhToan', N'QR', CAST(N'2026-05-30T08:50:32.0000000' AS DateTime2), CAST(N'2026-05-30T08:50:46.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530003', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 28000.0000, NULL, N'MVE', N'HoanThanh', N'DaThanhToan', N'TienMat', CAST(N'2026-05-30T08:51:11.0000000' AS DateTime2), CAST(N'2026-05-30T08:51:14.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530004', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 77000.0000, NULL, N'BAN01', N'HoanThanh', N'DaThanhToan', N'TienMat', CAST(N'2026-05-30T09:15:43.0000000' AS DateTime2), CAST(N'2026-05-30T09:16:10.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530005', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 38000.0000, NULL, N'BAN02', N'HoanThanh', N'DaThanhToan', N'TienMat', CAST(N'2026-05-30T09:16:41.0000000' AS DateTime2), CAST(N'2026-05-30T09:16:44.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530006', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 32000.0000, NULL, N'BAN01', N'HoanThanh', N'DaThanhToan', N'TienMat', CAST(N'2026-05-30T09:16:55.0000000' AS DateTime2), CAST(N'2026-05-30T09:17:04.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530007', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 65000.0000, NULL, N'BAN01', N'HoanThanh', N'DaThanhToan', N'QR', CAST(N'2026-05-30T09:26:11.0000000' AS DateTime2), CAST(N'2026-05-30T09:27:33.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530008', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 35000.0000, NULL, N'BAN01', N'HoanThanh', N'DaThanhToan', N'TienMat', CAST(N'2026-05-30T09:29:11.0000000' AS DateTime2), CAST(N'2026-05-30T09:29:13.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530009', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 28000.0000, NULL, N'BAN05', N'HoanThanh', N'DaThanhToan', N'QR', CAST(N'2026-05-30T09:38:33.0000000' AS DateTime2), CAST(N'2026-05-30T09:39:28.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530010', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 28000.0000, NULL, N'BAN01', N'HoanThanh', N'DaThanhToan', N'QR', CAST(N'2026-05-30T09:41:08.0000000' AS DateTime2), CAST(N'2026-05-30T09:41:21.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530011', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 28000.0000, NULL, N'BAN01', N'HoanThanh', N'DaThanhToan', N'TienMat', CAST(N'2026-05-30T09:54:06.0000000' AS DateTime2), CAST(N'2026-05-30T09:54:09.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530012', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 28000.0000, NULL, N'BAN02', N'HoanThanh', N'DaThanhToan', N'QR', CAST(N'2026-05-30T09:55:10.0000000' AS DateTime2), CAST(N'2026-05-30T09:55:18.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530013', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 280000.0000, NULL, N'BAN03', N'HoanThanh', N'DaThanhToan', N'TienMat', CAST(N'2026-05-30T11:11:04.0000000' AS DateTime2), CAST(N'2026-05-30T11:11:14.0000000' AS DateTime2))
INSERT [dbo].[HoaDon] ([MaHD], [NgayLap], [MaNV], [MaKH], [TongTien], [GhiChu], [MaBan], [TrangThaiOrder], [TrangThaiThanhToan], [PhuongThucThanhToan], [ThoiGianTao], [ThoiGianThanhToan]) VALUES (N'260530014', CAST(N'2026-05-30' AS Date), N'NV01', NULL, 60000.0000, NULL, N'BAN03', N'HoanThanh', N'DaThanhToan', N'TienMat', CAST(N'2026-05-30T11:11:45.0000000' AS DateTime2), CAST(N'2026-05-30T11:12:00.0000000' AS DateTime2))
GO
INSERT [dbo].[KhachHang] ([MaKH], [TenKH], [SDT], [DiemTichLuy]) VALUES (N'KH01', N'Lê Thị Hồng', N'0901111001', 5)
INSERT [dbo].[KhachHang] ([MaKH], [TenKH], [SDT], [DiemTichLuy]) VALUES (N'KH02', N'Nguyễn Văn Dũng', N'0901111002', 0)
INSERT [dbo].[KhachHang] ([MaKH], [TenKH], [SDT], [DiemTichLuy]) VALUES (N'KH03', N'Trần Thị Mỹ', N'0901111003', 10)
INSERT [dbo].[KhachHang] ([MaKH], [TenKH], [SDT], [DiemTichLuy]) VALUES (N'KH04', N'Võ Thành Tâm', N'0901111004', 0)
INSERT [dbo].[KhachHang] ([MaKH], [TenKH], [SDT], [DiemTichLuy]) VALUES (N'KH05', N'Phan Hữu Phước', N'0901111005', 3)
INSERT [dbo].[KhachHang] ([MaKH], [TenKH], [SDT], [DiemTichLuy]) VALUES (N'KH06', N'Lý Hoàng Anh', N'0901111006', 0)
INSERT [dbo].[KhachHang] ([MaKH], [TenKH], [SDT], [DiemTichLuy]) VALUES (N'KH07', N'Đỗ Bích Ngọc', N'0901111007', 8)
INSERT [dbo].[KhachHang] ([MaKH], [TenKH], [SDT], [DiemTichLuy]) VALUES (N'KH08', N'Lâm Quốc Trí', N'0901111008', 0)
INSERT [dbo].[KhachHang] ([MaKH], [TenKH], [SDT], [DiemTichLuy]) VALUES (N'KH09', N'Trương Mỹ Linh', N'0901111009', 4)
GO
INSERT [dbo].[KhuyenMai] ([MaKM], [TenKM], [LoaiKM], [GiaTri], [NgayBatDau], [NgayKetThuc], [DieuKienApDung], [TrangThai]) VALUES (N'KM01', N'Giảm 100', N'Tiền mặt', CAST(100000.00 AS Decimal(18, 2)), CAST(N'2025-11-01' AS Date), CAST(N'2026-12-31' AS Date), N'Áp dụng cho hóa đơn từ 50000', N'Đang áp dụng')
INSERT [dbo].[KhuyenMai] ([MaKM], [TenKM], [LoaiKM], [GiaTri], [NgayBatDau], [NgayKetThuc], [DieuKienApDung], [TrangThai]) VALUES (N'KM02', N'Giảm 50000', N'Tiền mặt', CAST(50000.00 AS Decimal(18, 2)), CAST(N'2025-11-01' AS Date), CAST(N'2026-12-31' AS Date), N'Áp dụng cho trà sữa', N'Đang áp dụng')
INSERT [dbo].[KhuyenMai] ([MaKM], [TenKM], [LoaiKM], [GiaTri], [NgayBatDau], [NgayKetThuc], [DieuKienApDung], [TrangThai]) VALUES (N'KM03', N'%', N'Phần trăm', CAST(50.00 AS Decimal(18, 2)), CAST(N'2025-01-01' AS Date), CAST(N'2026-12-31' AS Date), N'Khách hàng có điểm tích lũy', N'Đang áp dụng')
GO
INSERT [dbo].[LoaiSP] ([MaLoaiSP], [TenLoaiSP]) VALUES (N'L01', N'Cà Phê')
INSERT [dbo].[LoaiSP] ([MaLoaiSP], [TenLoaiSP]) VALUES (N'L02', N'Trà Sữa')
INSERT [dbo].[LoaiSP] ([MaLoaiSP], [TenLoaiSP]) VALUES (N'L03', N'Trà Trái Cây')
INSERT [dbo].[LoaiSP] ([MaLoaiSP], [TenLoaiSP]) VALUES (N'L04', N'Matcha')
GO
INSERT [dbo].[Luong] ([MaLuong], [Thang], [Nam], [LuongCoBan], [PhuCap], [Thuong], [Phat]) VALUES (1, 5, 2026, 7000000.0000, 500000.0000, 500000.0000, 0.0000)
INSERT [dbo].[Luong] ([MaLuong], [Thang], [Nam], [LuongCoBan], [PhuCap], [Thuong], [Phat]) VALUES (2, 5, 2026, 5000000.0000, 500000.0000, 300000.0000, 0.0000)
INSERT [dbo].[Luong] ([MaLuong], [Thang], [Nam], [LuongCoBan], [PhuCap], [Thuong], [Phat]) VALUES (3, 5, 2026, 5000000.0000, 500000.0000, 0.0000, 0.0000)
INSERT [dbo].[Luong] ([MaLuong], [Thang], [Nam], [LuongCoBan], [PhuCap], [Thuong], [Phat]) VALUES (4, 5, 2026, 4000000.0000, 300000.0000, 0.0000, 0.0000)
INSERT [dbo].[Luong] ([MaLuong], [Thang], [Nam], [LuongCoBan], [PhuCap], [Thuong], [Phat]) VALUES (5, 5, 2026, 4000000.0000, 300000.0000, 0.0000, 0.0000)
GO
INSERT [dbo].[NguyenLieu] ([MaNL], [TenNL], [DonViTinh], [SoLuongTon], [GiaNhap], [NgayCapNhat], [MucTonToiThieu]) VALUES (N'NL01', N'Cà phê bột', N'gram', 5010, 100000.0000, CAST(N'2026-05-30' AS Date), 500)
INSERT [dbo].[NguyenLieu] ([MaNL], [TenNL], [DonViTinh], [SoLuongTon], [GiaNhap], [NgayCapNhat], [MucTonToiThieu]) VALUES (N'NL02', N'Sữa đặc', N'ml', 3000, 80.0000, CAST(N'2026-05-28' AS Date), 300)
INSERT [dbo].[NguyenLieu] ([MaNL], [TenNL], [DonViTinh], [SoLuongTon], [GiaNhap], [NgayCapNhat], [MucTonToiThieu]) VALUES (N'NL03', N'Sữa tươi', N'ml', 5000, 60.0000, CAST(N'2026-05-28' AS Date), 500)
INSERT [dbo].[NguyenLieu] ([MaNL], [TenNL], [DonViTinh], [SoLuongTon], [GiaNhap], [NgayCapNhat], [MucTonToiThieu]) VALUES (N'NL04', N'Trà đen', N'gram', 2500, 0.0000, CAST(N'2026-05-30' AS Date), 300)
INSERT [dbo].[NguyenLieu] ([MaNL], [TenNL], [DonViTinh], [SoLuongTon], [GiaNhap], [NgayCapNhat], [MucTonToiThieu]) VALUES (N'NL05', N'Trân châu', N'gram', 4000, 90.0000, CAST(N'2026-05-28' AS Date), 500)
INSERT [dbo].[NguyenLieu] ([MaNL], [TenNL], [DonViTinh], [SoLuongTon], [GiaNhap], [NgayCapNhat], [MucTonToiThieu]) VALUES (N'NL06', N'Siro đào', N'ml', 2000, 150.0000, CAST(N'2026-05-28' AS Date), 200)
INSERT [dbo].[NguyenLieu] ([MaNL], [TenNL], [DonViTinh], [SoLuongTon], [GiaNhap], [NgayCapNhat], [MucTonToiThieu]) VALUES (N'NL07', N'Matcha', N'gram', 1500, 250.0000, CAST(N'2026-05-28' AS Date), 200)
INSERT [dbo].[NguyenLieu] ([MaNL], [TenNL], [DonViTinh], [SoLuongTon], [GiaNhap], [NgayCapNhat], [MucTonToiThieu]) VALUES (N'NL08', N'Đường', N'gram', 5000, 30.0000, CAST(N'2026-05-28' AS Date), 500)
GO
INSERT [dbo].[NhanVien] ([MaNV], [TenNV], [GioiTinh], [NgaySinh], [SDT], [ChucVu], [MaLuong], [LuongTheoGio]) VALUES (N'NV01', N'Nguyễn Khánh Trâm', N'Nữ', CAST(N'2000-04-10' AS Date), N'0901112233', N'Quản Lý', 1, 30000.0000)
INSERT [dbo].[NhanVien] ([MaNV], [TenNV], [GioiTinh], [NgaySinh], [SDT], [ChucVu], [MaLuong], [LuongTheoGio]) VALUES (N'NV02', N'Lê Tuyết Mai', N'Nữ', CAST(N'1999-07-15' AS Date), N'0902223344', N'Pha chế', 2, 22000.0000)
INSERT [dbo].[NhanVien] ([MaNV], [TenNV], [GioiTinh], [NgaySinh], [SDT], [ChucVu], [MaLuong], [LuongTheoGio]) VALUES (N'NV03', N'Trần Quốc Bảo', N'Nam', CAST(N'2003-11-22' AS Date), N'0903334455', N'Pha chế', 3, 22000.0000)
INSERT [dbo].[NhanVien] ([MaNV], [TenNV], [GioiTinh], [NgaySinh], [SDT], [ChucVu], [MaLuong], [LuongTheoGio]) VALUES (N'NV04', N'Phạm Minh Tú', N'Nam', CAST(N'2000-02-28' AS Date), N'0904445566', N'Phục vụ', 4, 20000.0000)
INSERT [dbo].[NhanVien] ([MaNV], [TenNV], [GioiTinh], [NgaySinh], [SDT], [ChucVu], [MaLuong], [LuongTheoGio]) VALUES (N'NV05', N'Đỗ Thanh Vy', N'Nữ', CAST(N'2001-08-03' AS Date), N'0905556677', N'Phục vụ', 5, 20000.0000)
GO
INSERT [dbo].[PhieuNhap] ([MaPN], [NgayNhap], [MaNV], [TongTien]) VALUES (N'PN2605001', CAST(N'2026-05-30' AS Date), N'NV01', 1000000.0000)
INSERT [dbo].[PhieuNhap] ([MaPN], [NgayNhap], [MaNV], [TongTien]) VALUES (N'PN2605002', CAST(N'2026-05-30' AS Date), N'NV01', 0.0000)
INSERT [dbo].[PhieuNhap] ([MaPN], [NgayNhap], [MaNV], [TongTien]) VALUES (N'PN2605003', CAST(N'2026-05-30' AS Date), N'NV01', 0.0000)
GO
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP01', N'Cà phê đen', 25000.0000, N'L01', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP02', N'Cà phê sữa', 30000.0000, N'L01', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP03', N'Bạc xỉu', 28000.0000, N'L01', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP04', N'Cappuccino', 35000.0000, N'L01', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP05', N'Espresso', 32000.0000, N'L01', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP06', N'Trà sữa trân châu', 35000.0000, N'L02', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP07', N'Trà sữa socola', 38000.0000, N'L02', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP08', N'Trà sữa matcha', 40000.0000, N'L02', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP09', N'Trà sữa caramel', 38000.0000, N'L02', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP10', N'Trà sữa thái xanh', 36000.0000, N'L02', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP11', N'Trà đào cam sả', 37000.0000, N'L03', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP12', N'Trà chanh mật ong', 30000.0000, N'L03', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP13', N'Trà vải', 32000.0000, N'L03', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP14', N'Trà dâu tằm', 33000.0000, N'L03', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP15', N'Trà ổi hồng', 34000.0000, N'L03', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP16', N'Matcha latte', 40000.0000, N'L04', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP17', N'Matcha đá xay', 45000.0000, N'L04', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP18', N'Matcha sữa hạt', 42000.0000, N'L04', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP19', N'Matcha kem cheese', 48000.0000, N'L04', N'Đang bán')
INSERT [dbo].[SanPham] ([MaSP], [TenSP], [DonGia], [MaLoaiSP], [TrangThai]) VALUES (N'SP20', N'Matcha caramel', 45000.0000, N'L04', N'Đang bán')
GO
INSERT [dbo].[TaiKhoan] ([MaTK], [TenDangNhap], [MatKhau], [MaNV], [VaiTro]) VALUES (N'TK01', N'qly_tram', N'12345678', N'NV01', N'Quản Lý')
INSERT [dbo].[TaiKhoan] ([MaTK], [TenDangNhap], [MatKhau], [MaNV], [VaiTro]) VALUES (N'TK02', N'pc_mai', N'12345678', N'NV02', N'Pha Chế')
INSERT [dbo].[TaiKhoan] ([MaTK], [TenDangNhap], [MatKhau], [MaNV], [VaiTro]) VALUES (N'TK03', N'pc_bao', N'12345678', N'NV03', N'Pha Chế')
INSERT [dbo].[TaiKhoan] ([MaTK], [TenDangNhap], [MatKhau], [MaNV], [VaiTro]) VALUES (N'TK04', N'pv_tu', N'12345678', N'NV04', N'Phục Vụ')
INSERT [dbo].[TaiKhoan] ([MaTK], [TenDangNhap], [MatKhau], [MaNV], [VaiTro]) VALUES (N'TK05', N'pv_vy', N'12345678', N'NV05', N'Phục Vụ')
GO
SET IDENTITY_INSERT [dbo].[ThanhToan] ON 

INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (12, N'260529002', N'260529002-L1', N'QR', 28000.0000, N'260529002-L1', NULL, N'DaHuy', CAST(N'2026-05-29T23:32:38.0000000' AS DateTime2), CAST(N'2026-05-29T23:47:38.0000000' AS DateTime2), NULL, N'Phiên QR có hiệu lực 15 phút. | Hủy phiên QR cũ do tạo QR mới.')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (13, N'260529002', N'260529002-L2', N'QR', 28000.0000, N'260529002-L2', N'DEMO-20260529233328671-13', N'DaThanhToan', CAST(N'2026-05-29T23:33:20.0000000' AS DateTime2), CAST(N'2026-05-29T23:48:20.0000000' AS DateTime2), CAST(N'2026-05-29T23:33:29.0000000' AS DateTime2), N'Phiên QR có hiệu lực 15 phút. | QR được xác nhận tự động theo mã phiên chính xác.')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (14, N'260530001', NULL, N'TienMat', 28000.0000, NULL, NULL, N'DaThanhToan', CAST(N'2026-05-30T08:49:28.0000000' AS DateTime2), NULL, CAST(N'2026-05-30T08:49:28.0000000' AS DateTime2), N'Thanh toán tiền mặt tại quầy')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (15, N'260530002', N'260530002-L1', N'QR', 28000.0000, N'260530002-L1', N'DEMO-20260530085045719-15', N'DaThanhToan', CAST(N'2026-05-30T08:50:34.0000000' AS DateTime2), CAST(N'2026-05-30T09:05:34.0000000' AS DateTime2), CAST(N'2026-05-30T08:50:46.0000000' AS DateTime2), N'Phiên QR có hiệu lực 15 phút. | QR được xác nhận tự động theo mã phiên chính xác.')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (16, N'260530003', NULL, N'TienMat', 28000.0000, NULL, NULL, N'DaThanhToan', CAST(N'2026-05-30T08:51:14.0000000' AS DateTime2), NULL, CAST(N'2026-05-30T08:51:14.0000000' AS DateTime2), N'Thanh toán tiền mặt tại quầy')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (19, N'260530004', N'260530004-L1', N'QR', 37000.0000, N'260530004-L1', NULL, N'DaHuy', CAST(N'2026-05-30T09:15:48.0000000' AS DateTime2), CAST(N'2026-05-30T09:30:48.0000000' AS DateTime2), NULL, N'Phiên QR có hiệu lực 15 phút. | Hủy phiên thanh toán do nhân viên bấm Sửa.')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (20, N'260530004', NULL, N'TienMat', 77000.0000, NULL, NULL, N'DaThanhToan', CAST(N'2026-05-30T09:16:10.0000000' AS DateTime2), NULL, CAST(N'2026-05-30T09:16:10.0000000' AS DateTime2), N'Thanh toán tiền mặt tại quầy')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (21, N'260530005', NULL, N'TienMat', 38000.0000, NULL, NULL, N'DaThanhToan', CAST(N'2026-05-30T09:16:44.0000000' AS DateTime2), NULL, CAST(N'2026-05-30T09:16:44.0000000' AS DateTime2), N'Thanh toán tiền mặt tại quầy')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (22, N'260530006', NULL, N'TienMat', 32000.0000, NULL, NULL, N'DaThanhToan', CAST(N'2026-05-30T09:17:04.0000000' AS DateTime2), NULL, CAST(N'2026-05-30T09:17:04.0000000' AS DateTime2), N'Thanh toán tiền mặt tại quầy')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (23, N'260530007', N'260530007-L1', N'QR', 35000.0000, N'260530007-L1', NULL, N'DaHuy', CAST(N'2026-05-30T09:26:14.0000000' AS DateTime2), CAST(N'2026-05-30T09:41:14.0000000' AS DateTime2), NULL, N'Phiên QR có hiệu lực 15 phút. | Hủy phiên thanh toán do nhân viên bấm Sửa.')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (24, N'260530007', N'260530007-L2', N'QR', 65000.0000, N'260530007-L2', N'DEMO-20260530092732968-24', N'DaThanhToan', CAST(N'2026-05-30T09:26:24.0000000' AS DateTime2), CAST(N'2026-05-30T09:41:24.0000000' AS DateTime2), CAST(N'2026-05-30T09:27:33.0000000' AS DateTime2), N'Phiên QR có hiệu lực 15 phút. | QR được xác nhận tự động theo mã phiên chính xác.')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (25, N'260530008', NULL, N'TienMat', 35000.0000, NULL, NULL, N'DaThanhToan', CAST(N'2026-05-30T09:29:13.0000000' AS DateTime2), NULL, CAST(N'2026-05-30T09:29:13.0000000' AS DateTime2), N'Thanh toán tiền mặt tại quầy')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (31, N'260530009', N'260530009-L1', N'QR', 28000.0000, N'260530009-L1', N'DEMO-20260530093927556-31', N'DaThanhToan', CAST(N'2026-05-30T09:38:52.0000000' AS DateTime2), CAST(N'2026-05-30T09:53:52.0000000' AS DateTime2), CAST(N'2026-05-30T09:39:28.0000000' AS DateTime2), N'Phiên QR có hiệu lực 15 phút. | QR được xác nhận tự động theo mã phiên chính xác.')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (32, N'260530010', N'260530010-L1', N'QR', 28000.0000, N'260530010-L1', N'DEMO-20260530094121235-32', N'DaThanhToan', CAST(N'2026-05-30T09:41:11.0000000' AS DateTime2), CAST(N'2026-05-30T09:56:11.0000000' AS DateTime2), CAST(N'2026-05-30T09:41:21.0000000' AS DateTime2), N'Phiên QR có hiệu lực 15 phút. | QR được xác nhận tự động theo mã phiên chính xác.')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (36, N'260530011', NULL, N'TienMat', 28000.0000, NULL, NULL, N'DaThanhToan', CAST(N'2026-05-30T09:54:09.0000000' AS DateTime2), NULL, CAST(N'2026-05-30T09:54:09.0000000' AS DateTime2), N'Thanh toán tiền mặt tại quầy')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (37, N'260530012', N'260530012-L1', N'QR', 28000.0000, N'260530012-L1', N'DEMO-20260530095517578-37', N'DaThanhToan', CAST(N'2026-05-30T09:55:12.0000000' AS DateTime2), CAST(N'2026-05-30T10:10:12.0000000' AS DateTime2), CAST(N'2026-05-30T09:55:18.0000000' AS DateTime2), N'Phiên QR có hiệu lực 15 phút. | QR được xác nhận tự động theo mã phiên chính xác.')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (40, N'260530013', NULL, N'TienMat', 280000.0000, NULL, NULL, N'DaThanhToan', CAST(N'2026-05-30T11:11:14.0000000' AS DateTime2), NULL, CAST(N'2026-05-30T11:11:14.0000000' AS DateTime2), N'Thanh toán tiền mặt tại quầy')
INSERT [dbo].[ThanhToan] ([MaThanhToan], [MaHD], [MaPhienThanhToan], [PhuongThuc], [SoTien], [NoiDungChuyenKhoan], [MaGiaoDich], [TrangThai], [ThoiGianTao], [ThoiGianHetHan], [ThoiGianThanhToan], [GhiChu]) VALUES (41, N'260530014', N'260530014-L1', N'QR', 60000.0000, N'260530014-L1', NULL, N'DaHuy', CAST(N'2026-05-30T11:11:53.0000000' AS DateTime2), CAST(N'2026-05-30T11:26:53.0000000' AS DateTime2), NULL, N'Phiên QR có hiệu lực 15 phút. | Hủy phiên thanh toán do nhân viên bấm Sửa.')
SET IDENTITY_INSERT [dbo].[ThanhToan] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AuditLog_ChucNang_HanhDong]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_AuditLog_ChucNang_HanhDong] ON [dbo].[AuditLog]
(
	[ChucNang] ASC,
	[HanhDong] ASC,
	[ThoiGian] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AuditLog_DoiTuong_MaDoiTuong]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_AuditLog_DoiTuong_MaDoiTuong] ON [dbo].[AuditLog]
(
	[DoiTuong] ASC,
	[MaDoiTuong] ASC,
	[ThoiGian] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AuditLog_MaNV_ThoiGian]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_AuditLog_MaNV_ThoiGian] ON [dbo].[AuditLog]
(
	[MaNV] ASC,
	[ThoiGian] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AuditLog_ThoiGian]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_AuditLog_ThoiGian] ON [dbo].[AuditLog]
(
	[ThoiGian] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ChamCong_MaNV_ThoiGianCheckIn]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_ChamCong_MaNV_ThoiGianCheckIn] ON [dbo].[ChamCong]
(
	[MaNV] ASC,
	[ThoiGianCheckIn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_ChamCong_MaNV_DangLam]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ChamCong_MaNV_DangLam] ON [dbo].[ChamCong]
(
	[MaNV] ASC
)
WHERE ([TrangThai]=N'Đang làm' AND [ThoiGianCheckOut] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CTHD_MaSP]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_CTHD_MaSP] ON [dbo].[CTHD]
(
	[MaSP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_HoaDon_NgayLap]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_HoaDon_NgayLap] ON [dbo].[HoaDon]
(
	[NgayLap] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SanPham_TrangThai]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_SanPham_TrangThai] ON [dbo].[SanPham]
(
	[TrangThai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_TaiKhoan_TenDangNhap]    Script Date: 05/30/2026 11:12:09 AM ******/
ALTER TABLE [dbo].[TaiKhoan] ADD  CONSTRAINT [UQ_TaiKhoan_TenDangNhap] UNIQUE NONCLUSTERED 
(
	[TenDangNhap] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ThanhToan_MaHD]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_ThanhToan_MaHD] ON [dbo].[ThanhToan]
(
	[MaHD] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ThanhToan_QR_DangCho]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_ThanhToan_QR_DangCho] ON [dbo].[ThanhToan]
(
	[PhuongThuc] ASC,
	[TrangThai] ASC,
	[MaPhienThanhToan] ASC,
	[ThoiGianHetHan] ASC
)
INCLUDE([MaHD],[SoTien]) 
WHERE ([PhuongThuc]=N'QR' AND [TrangThai]=N'ChoThanhToanQR' AND [MaPhienThanhToan] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ThanhToan_QR_HetHan]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE NONCLUSTERED INDEX [IX_ThanhToan_QR_HetHan] ON [dbo].[ThanhToan]
(
	[TrangThai] ASC,
	[ThoiGianHetHan] ASC
)
INCLUDE([MaHD],[PhuongThuc],[MaPhienThanhToan]) 
WHERE ([PhuongThuc]=N'QR' AND [TrangThai]=N'ChoThanhToanQR' AND [ThoiGianHetHan] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_ThanhToan_MaGiaoDich]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ThanhToan_MaGiaoDich] ON [dbo].[ThanhToan]
(
	[MaGiaoDich] ASC
)
WHERE ([MaGiaoDich] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_ThanhToan_MaPhienThanhToan]    Script Date: 05/30/2026 11:12:09 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ThanhToan_MaPhienThanhToan] ON [dbo].[ThanhToan]
(
	[MaPhienThanhToan] ASC
)
WHERE ([MaPhienThanhToan] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AuditLog] ADD  CONSTRAINT [DF_AuditLog_ThoiGian]  DEFAULT (sysdatetime()) FOR [ThoiGian]
GO
ALTER TABLE [dbo].[ChamCong] ADD  CONSTRAINT [DF_ChamCong_CheckIn]  DEFAULT (sysdatetime()) FOR [ThoiGianCheckIn]
GO
ALTER TABLE [dbo].[ChamCong] ADD  CONSTRAINT [DF_ChamCong_TrangThai]  DEFAULT (N'Đang làm') FOR [TrangThai]
GO
ALTER TABLE [dbo].[HoaDon] ADD  CONSTRAINT [DF_HoaDon_TongTien]  DEFAULT ((0)) FOR [TongTien]
GO
ALTER TABLE [dbo].[HoaDon] ADD  CONSTRAINT [DF_HoaDon_TrangThaiOrder]  DEFAULT (N'ChoThanhToan') FOR [TrangThaiOrder]
GO
ALTER TABLE [dbo].[HoaDon] ADD  CONSTRAINT [DF_HoaDon_TrangThaiThanhToan]  DEFAULT (N'ChuaThanhToan') FOR [TrangThaiThanhToan]
GO
ALTER TABLE [dbo].[HoaDon] ADD  CONSTRAINT [DF_HoaDon_ThoiGianTao]  DEFAULT (sysdatetime()) FOR [ThoiGianTao]
GO
ALTER TABLE [dbo].[KhachHang] ADD  CONSTRAINT [DF_KhachHang_Diem]  DEFAULT ((0)) FOR [DiemTichLuy]
GO
ALTER TABLE [dbo].[Luong] ADD  CONSTRAINT [DF_Luong_PhuCap]  DEFAULT ((0)) FOR [PhuCap]
GO
ALTER TABLE [dbo].[Luong] ADD  CONSTRAINT [DF_Luong_Thuong]  DEFAULT ((0)) FOR [Thuong]
GO
ALTER TABLE [dbo].[Luong] ADD  CONSTRAINT [DF_Luong_Phat]  DEFAULT ((0)) FOR [Phat]
GO
ALTER TABLE [dbo].[NguyenLieu] ADD  CONSTRAINT [DF_NguyenLieu_SoLuong]  DEFAULT ((0)) FOR [SoLuongTon]
GO
ALTER TABLE [dbo].[NhanVien] ADD  CONSTRAINT [DF_NhanVien_LuongTheoGio]  DEFAULT ((20000)) FOR [LuongTheoGio]
GO
ALTER TABLE [dbo].[PhieuNhap] ADD  CONSTRAINT [DF_PhieuNhap_TongTien]  DEFAULT ((0)) FOR [TongTien]
GO
ALTER TABLE [dbo].[SanPham] ADD  CONSTRAINT [DF_SanPham_TrangThai]  DEFAULT (N'Tạm ngưng') FOR [TrangThai]
GO
ALTER TABLE [dbo].[ThanhToan] ADD  CONSTRAINT [DF_ThanhToan_ThoiGianTao]  DEFAULT (sysdatetime()) FOR [ThoiGianTao]
GO
/****** Object:  Table [dbo].[LichSuTruKho] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LichSuTruKho](
	[MaLichSu] [int] IDENTITY(1,1) NOT NULL,
	[MaHD] [varchar](9) NOT NULL,
	[MaNL] [varchar](5) NOT NULL,
	[SoLuongTruThucTe] [int] NOT NULL,
	[ThoiGianTru] [datetime2](7) NOT NULL CONSTRAINT [DF_LichSuTruKho_ThoiGianTru] DEFAULT (sysutcdatetime()),
 CONSTRAINT [PK_LichSuTruKho] PRIMARY KEY CLUSTERED 
(
	[MaLichSu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_LichSuTruKho_MaHD] ON [dbo].[LichSuTruKho]
(
	[MaHD] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ChamCong]  WITH CHECK ADD  CONSTRAINT [FK_ChamCong_NhanVien] FOREIGN KEY([MaNV])
REFERENCES [dbo].[NhanVien] ([MaNV])
GO
ALTER TABLE [dbo].[ChamCong] CHECK CONSTRAINT [FK_ChamCong_NhanVien]
GO
ALTER TABLE [dbo].[CongThuc]  WITH CHECK ADD  CONSTRAINT [FK_CongThuc_NguyenLieu] FOREIGN KEY([MaNL])
REFERENCES [dbo].[NguyenLieu] ([MaNL])
GO
ALTER TABLE [dbo].[CongThuc] CHECK CONSTRAINT [FK_CongThuc_NguyenLieu]
GO
ALTER TABLE [dbo].[CongThuc]  WITH CHECK ADD  CONSTRAINT [FK_CongThuc_SanPham] FOREIGN KEY([MaSP])
REFERENCES [dbo].[SanPham] ([MaSP])
GO
ALTER TABLE [dbo].[CongThuc] CHECK CONSTRAINT [FK_CongThuc_SanPham]
GO
ALTER TABLE [dbo].[CTHD]  WITH CHECK ADD  CONSTRAINT [FK_CTHD_HoaDon] FOREIGN KEY([MaHD])
REFERENCES [dbo].[HoaDon] ([MaHD])
GO
ALTER TABLE [dbo].[CTHD] CHECK CONSTRAINT [FK_CTHD_HoaDon]
GO
ALTER TABLE [dbo].[CTHD]  WITH CHECK ADD  CONSTRAINT [FK_CTHD_KhuyenMai] FOREIGN KEY([MaKM])
REFERENCES [dbo].[KhuyenMai] ([MaKM])
GO
ALTER TABLE [dbo].[CTHD] CHECK CONSTRAINT [FK_CTHD_KhuyenMai]
GO
ALTER TABLE [dbo].[CTHD]  WITH CHECK ADD  CONSTRAINT [FK_CTHD_SanPham] FOREIGN KEY([MaSP])
REFERENCES [dbo].[SanPham] ([MaSP])
GO
ALTER TABLE [dbo].[CTHD] CHECK CONSTRAINT [FK_CTHD_SanPham]
GO
ALTER TABLE [dbo].[CTPN]  WITH CHECK ADD  CONSTRAINT [FK_CTPN_NguyenLieu] FOREIGN KEY([MaNL])
REFERENCES [dbo].[NguyenLieu] ([MaNL])
GO
ALTER TABLE [dbo].[CTPN] CHECK CONSTRAINT [FK_CTPN_NguyenLieu]
GO
ALTER TABLE [dbo].[CTPN]  WITH CHECK ADD  CONSTRAINT [FK_CTPN_PhieuNhap] FOREIGN KEY([MaPN])
REFERENCES [dbo].[PhieuNhap] ([MaPN])
GO
ALTER TABLE [dbo].[CTPN] CHECK CONSTRAINT [FK_CTPN_PhieuNhap]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT [FK_HoaDon_Ban] FOREIGN KEY([MaBan])
REFERENCES [dbo].[Ban] ([MaBan])
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [FK_HoaDon_Ban]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT [FK_HoaDon_KhachHang] FOREIGN KEY([MaKH])
REFERENCES [dbo].[KhachHang] ([MaKH])
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [FK_HoaDon_KhachHang]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT [FK_HoaDon_NhanVien] FOREIGN KEY([MaNV])
REFERENCES [dbo].[NhanVien] ([MaNV])
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [FK_HoaDon_NhanVien]
GO
ALTER TABLE [dbo].[NhanVien]  WITH CHECK ADD  CONSTRAINT [FK_NhanVien_Luong] FOREIGN KEY([MaLuong])
REFERENCES [dbo].[Luong] ([MaLuong])
GO
ALTER TABLE [dbo].[NhanVien] CHECK CONSTRAINT [FK_NhanVien_Luong]
GO
ALTER TABLE [dbo].[PhieuNhap]  WITH CHECK ADD  CONSTRAINT [FK_PhieuNhap_NhanVien] FOREIGN KEY([MaNV])
REFERENCES [dbo].[NhanVien] ([MaNV])
GO
ALTER TABLE [dbo].[PhieuNhap] CHECK CONSTRAINT [FK_PhieuNhap_NhanVien]
GO
ALTER TABLE [dbo].[SanPham]  WITH CHECK ADD  CONSTRAINT [FK_SanPham_LoaiSP] FOREIGN KEY([MaLoaiSP])
REFERENCES [dbo].[LoaiSP] ([MaLoaiSP])
GO
ALTER TABLE [dbo].[SanPham] CHECK CONSTRAINT [FK_SanPham_LoaiSP]
GO
ALTER TABLE [dbo].[TaiKhoan]  WITH CHECK ADD  CONSTRAINT [FK_TaiKhoan_NhanVien] FOREIGN KEY([MaNV])
REFERENCES [dbo].[NhanVien] ([MaNV])
GO
ALTER TABLE [dbo].[TaiKhoan] CHECK CONSTRAINT [FK_TaiKhoan_NhanVien]
GO
ALTER TABLE [dbo].[ThanhToan]  WITH CHECK ADD  CONSTRAINT [FK_ThanhToan_HoaDon] FOREIGN KEY([MaHD])
REFERENCES [dbo].[HoaDon] ([MaHD])
GO
ALTER TABLE [dbo].[ThanhToan] CHECK CONSTRAINT [FK_ThanhToan_HoaDon]
GO
ALTER TABLE [dbo].[LichSuTruKho]  WITH CHECK ADD  CONSTRAINT [FK_LichSuTruKho_HoaDon] FOREIGN KEY([MaHD])
REFERENCES [dbo].[HoaDon] ([MaHD])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LichSuTruKho] CHECK CONSTRAINT [FK_LichSuTruKho_HoaDon]
GO
ALTER TABLE [dbo].[LichSuTruKho]  WITH CHECK ADD  CONSTRAINT [FK_LichSuTruKho_NguyenLieu] FOREIGN KEY([MaNL])
REFERENCES [dbo].[NguyenLieu] ([MaNL])
GO
ALTER TABLE [dbo].[LichSuTruKho] CHECK CONSTRAINT [FK_LichSuTruKho_NguyenLieu]
GO
ALTER TABLE [dbo].[Ban]  WITH CHECK ADD  CONSTRAINT [CK_Ban_TrangThai] CHECK  (([TrangThai]=N'Đang phục vụ' OR [TrangThai]=N'Trống'))
GO
ALTER TABLE [dbo].[Ban] CHECK CONSTRAINT [CK_Ban_TrangThai]
GO
ALTER TABLE [dbo].[Ban]  WITH CHECK ADD CONSTRAINT [CK_Ban_KhuVuc_SoBan] CHECK (
    ([MaBan] = 'MVE' AND [KhuVuc] = N'Mang về' AND [SoBan] IS NULL)
    OR
    ([MaBan] <> 'MVE' AND [KhuVuc] IN (N'Tầng trệt', N'Tầng lửng') AND [SoBan] IS NOT NULL AND [SoBan] > 0)
)
GO
ALTER TABLE [dbo].[Ban] CHECK CONSTRAINT [CK_Ban_KhuVuc_SoBan]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [UX_Ban_SoBan] ON [dbo].[Ban] ([SoBan])
WHERE [SoBan] IS NOT NULL
GO
ALTER TABLE [dbo].[ChamCong]  WITH CHECK ADD  CONSTRAINT [CK_ChamCong_CheckOut] CHECK  (([ThoiGianCheckOut] IS NULL OR [ThoiGianCheckOut]>=[ThoiGianCheckIn]))
GO
ALTER TABLE [dbo].[ChamCong] CHECK CONSTRAINT [CK_ChamCong_CheckOut]
GO
ALTER TABLE [dbo].[ChamCong]  WITH CHECK ADD  CONSTRAINT [CK_ChamCong_DonGiaGio] CHECK  (([DonGiaGio]>=(0)))
GO
ALTER TABLE [dbo].[ChamCong] CHECK CONSTRAINT [CK_ChamCong_DonGiaGio]
GO
ALTER TABLE [dbo].[ChamCong]  WITH CHECK ADD  CONSTRAINT [CK_ChamCong_TienCong] CHECK  (([TienCong] IS NULL OR [TienCong]>=(0)))
GO
ALTER TABLE [dbo].[ChamCong] CHECK CONSTRAINT [CK_ChamCong_TienCong]
GO
ALTER TABLE [dbo].[ChamCong]  WITH CHECK ADD  CONSTRAINT [CK_ChamCong_TongGio] CHECK  (([TongGio] IS NULL OR [TongGio]>=(0)))
GO
ALTER TABLE [dbo].[ChamCong] CHECK CONSTRAINT [CK_ChamCong_TongGio]
GO
ALTER TABLE [dbo].[ChamCong]  WITH CHECK ADD  CONSTRAINT [CK_ChamCong_TongPhut] CHECK  (([TongPhut] IS NULL OR [TongPhut]>=(0)))
GO
ALTER TABLE [dbo].[ChamCong] CHECK CONSTRAINT [CK_ChamCong_TongPhut]
GO
ALTER TABLE [dbo].[ChamCong]  WITH CHECK ADD  CONSTRAINT [CK_ChamCong_TrangThai] CHECK  (([TrangThai]=N'Đã checkout' OR [TrangThai]=N'Đang làm'))
GO
ALTER TABLE [dbo].[ChamCong] CHECK CONSTRAINT [CK_ChamCong_TrangThai]
GO
ALTER TABLE [dbo].[CongThuc]  WITH CHECK ADD  CONSTRAINT [CK_CongThuc_DinhLuong] CHECK  (([DinhLuong]>(0)))
GO
ALTER TABLE [dbo].[CongThuc] CHECK CONSTRAINT [CK_CongThuc_DinhLuong]
GO
ALTER TABLE [dbo].[CTHD]  WITH CHECK ADD  CONSTRAINT [CK_CTHD_DonGia] CHECK  (([DonGia]>(0) AND [DonGia]<=(10000000)))
GO
ALTER TABLE [dbo].[CTHD] CHECK CONSTRAINT [CK_CTHD_DonGia]
GO
ALTER TABLE [dbo].[CTHD]  WITH CHECK ADD  CONSTRAINT [CK_CTHD_SoLuong] CHECK  (([SoLuong]>(0)))
GO
ALTER TABLE [dbo].[CTHD] CHECK CONSTRAINT [CK_CTHD_SoLuong]
GO
ALTER TABLE [dbo].[CTPN]  WITH CHECK ADD  CONSTRAINT [CK_CTPN_DonGia] CHECK  (([DonGia]>=(0) AND [DonGia]<=(100000000)))
GO
ALTER TABLE [dbo].[CTPN] CHECK CONSTRAINT [CK_CTPN_DonGia]
GO
ALTER TABLE [dbo].[CTPN]  WITH CHECK ADD  CONSTRAINT [CK_CTPN_SoLuong] CHECK  (([SoLuong]>(0)))
GO
ALTER TABLE [dbo].[CTPN] CHECK CONSTRAINT [CK_CTPN_SoLuong]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT [CK_HoaDon_PhuongThucThanhToan] CHECK  (([PhuongThucThanhToan] IS NULL OR ([PhuongThucThanhToan]=N'QR' OR [PhuongThucThanhToan]=N'TienMat')))
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [CK_HoaDon_PhuongThucThanhToan]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT [CK_HoaDon_TongTien] CHECK  (([TongTien]>=(0)))
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [CK_HoaDon_TongTien]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT [CK_HoaDon_TrangThaiOrder] CHECK  (([TrangThaiOrder]=N'DaHuy' OR [TrangThaiOrder]=N'HoanThanh' OR [TrangThaiOrder]=N'ChoThanhToan' OR [TrangThaiOrder]=N'DangOrder'))
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [CK_HoaDon_TrangThaiOrder]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT [CK_HoaDon_TrangThaiThanhToan] CHECK  (([TrangThaiThanhToan]=N'ChoKiemTra' OR [TrangThaiThanhToan]=N'DaHuy' OR [TrangThaiThanhToan]=N'HetHan' OR [TrangThaiThanhToan]=N'DaThanhToan' OR [TrangThaiThanhToan]=N'ChoThanhToanQR' OR [TrangThaiThanhToan]=N'ChuaThanhToan'))
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [CK_HoaDon_TrangThaiThanhToan]
GO
ALTER TABLE [dbo].[KhachHang]  WITH CHECK ADD  CONSTRAINT [CK_KhachHang_Diem] CHECK  ((isnull([DiemTichLuy],(0))>=(0)))
GO
ALTER TABLE [dbo].[KhachHang] CHECK CONSTRAINT [CK_KhachHang_Diem]
GO
ALTER TABLE [dbo].[KhuyenMai]  WITH CHECK ADD  CONSTRAINT [CK_KhuyenMai_GiaTri] CHECK  (([GiaTri]>=(0) AND ([LoaiKM]=N'Tiền mặt' OR [LoaiKM]=N'Phần trăm' AND [GiaTri]<=(100))))
GO
ALTER TABLE [dbo].[KhuyenMai] CHECK CONSTRAINT [CK_KhuyenMai_GiaTri]
GO
ALTER TABLE [dbo].[KhuyenMai]  WITH CHECK ADD  CONSTRAINT [CK_KhuyenMai_Loai] CHECK  (([LoaiKM]=N'Tiền mặt' OR [LoaiKM]=N'Phần trăm'))
GO
ALTER TABLE [dbo].[KhuyenMai] CHECK CONSTRAINT [CK_KhuyenMai_Loai]
GO
ALTER TABLE [dbo].[KhuyenMai]  WITH CHECK ADD  CONSTRAINT [CK_KhuyenMai_Ngay] CHECK  (([NgayKetThuc]>=[NgayBatDau]))
GO
ALTER TABLE [dbo].[KhuyenMai] CHECK CONSTRAINT [CK_KhuyenMai_Ngay]
GO
ALTER TABLE [dbo].[Luong]  WITH CHECK ADD  CONSTRAINT [CK_Luong_Nam] CHECK  (([Nam]>=(2000)))
GO
ALTER TABLE [dbo].[Luong] CHECK CONSTRAINT [CK_Luong_Nam]
GO
ALTER TABLE [dbo].[Luong]  WITH CHECK ADD  CONSTRAINT [CK_Luong_Thang] CHECK  (([Thang]>=(1) AND [Thang]<=(12)))
GO
ALTER TABLE [dbo].[Luong] CHECK CONSTRAINT [CK_Luong_Thang]
GO
ALTER TABLE [dbo].[Luong]  WITH CHECK ADD  CONSTRAINT [CK_Luong_Tien] CHECK  (([LuongCoBan]>=(0) AND isnull([PhuCap],(0))>=(0) AND isnull([Thuong],(0))>=(0) AND isnull([Phat],(0))>=(0)))
GO
ALTER TABLE [dbo].[Luong] CHECK CONSTRAINT [CK_Luong_Tien]
GO
ALTER TABLE [dbo].[NguyenLieu]  WITH CHECK ADD  CONSTRAINT [CK_NguyenLieu_GiaNhap] CHECK  (([GiaNhap]>=(0) AND [GiaNhap]<=(100000000)))
GO
ALTER TABLE [dbo].[NguyenLieu] CHECK CONSTRAINT [CK_NguyenLieu_GiaNhap]
GO
ALTER TABLE [dbo].[NguyenLieu]  WITH CHECK ADD  CONSTRAINT [CK_NguyenLieu_MucTon] CHECK  (([MucTonToiThieu]>=(0)))
GO
ALTER TABLE [dbo].[NguyenLieu] CHECK CONSTRAINT [CK_NguyenLieu_MucTon]
GO
ALTER TABLE [dbo].[NguyenLieu]  WITH CHECK ADD  CONSTRAINT [CK_NguyenLieu_SoLuong] CHECK  ((isnull([SoLuongTon],(0))>=(0)))
GO
ALTER TABLE [dbo].[NguyenLieu] CHECK CONSTRAINT [CK_NguyenLieu_SoLuong]
GO
ALTER TABLE [dbo].[NhanVien]  WITH CHECK ADD  CONSTRAINT [CK_NhanVien_LuongTheoGio] CHECK  (([LuongTheoGio]>=(0) AND [LuongTheoGio]<=(1000000)))
GO
ALTER TABLE [dbo].[NhanVien] CHECK CONSTRAINT [CK_NhanVien_LuongTheoGio]
GO
ALTER TABLE [dbo].[PhieuNhap]  WITH CHECK ADD  CONSTRAINT [CK_PhieuNhap_TongTien] CHECK  (([TongTien]>=(0)))
GO
ALTER TABLE [dbo].[PhieuNhap] CHECK CONSTRAINT [CK_PhieuNhap_TongTien]
GO
ALTER TABLE [dbo].[SanPham]  WITH CHECK ADD  CONSTRAINT [CK_SanPham_DonGia] CHECK  (([DonGia]>(0) AND [DonGia]<=(10000000)))
GO
ALTER TABLE [dbo].[SanPham] CHECK CONSTRAINT [CK_SanPham_DonGia]
GO
ALTER TABLE [dbo].[SanPham]  WITH CHECK ADD  CONSTRAINT [CK_SanPham_TrangThai] CHECK  (([TrangThai]=N'Ngừng bán' OR [TrangThai]=N'Tạm ngưng' OR [TrangThai]=N'Đang bán'))
GO
ALTER TABLE [dbo].[SanPham] CHECK CONSTRAINT [CK_SanPham_TrangThai]
GO
ALTER TABLE [dbo].[ThanhToan]  WITH CHECK ADD  CONSTRAINT [CK_ThanhToan_PhuongThuc] CHECK  (([PhuongThuc]=N'QR' OR [PhuongThuc]=N'TienMat'))
GO
ALTER TABLE [dbo].[ThanhToan] CHECK CONSTRAINT [CK_ThanhToan_PhuongThuc]
GO
ALTER TABLE [dbo].[ThanhToan]  WITH CHECK ADD  CONSTRAINT [CK_ThanhToan_SoTien] CHECK  (([SoTien]>=(0)))
GO
ALTER TABLE [dbo].[ThanhToan] CHECK CONSTRAINT [CK_ThanhToan_SoTien]
GO
ALTER TABLE [dbo].[ThanhToan]  WITH CHECK ADD  CONSTRAINT [CK_ThanhToan_TrangThai] CHECK  (([TrangThai]=N'ChoKiemTra' OR [TrangThai]=N'DaHuy' OR [TrangThai]=N'HetHan' OR [TrangThai]=N'DaThanhToan' OR [TrangThai]=N'ChoThanhToanQR' OR [TrangThai]=N'ChuaThanhToan'))
GO
ALTER TABLE [dbo].[ThanhToan] CHECK CONSTRAINT [CK_ThanhToan_TrangThai]
GO
/****** Object:  StoredProcedure [dbo].[AuditLog_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   6. Proc xem danh sách log
   ========================================================= */

CREATE   PROCEDURE [dbo].[AuditLog_DS]
    @TuNgay DATE = NULL,
    @DenNgay DATE = NULL,
    @MaNV NVARCHAR(50) = NULL,
    @ChucNang NVARCHAR(100) = NULL,
    @HanhDong NVARCHAR(100) = NULL,
    @DoiTuong NVARCHAR(100) = NULL,
    @MaDoiTuong NVARCHAR(100) = NULL,
    @Top INT = 200
AS
BEGIN
    SET NOCOUNT ON;

    IF @Top IS NULL OR @Top <= 0 SET @Top = 200;
    IF @Top > 1000 SET @Top = 1000;

    SELECT TOP (@Top)
        MaLog,
        ThoiGian,
        MaNV,
        TenDangNhap,
        VaiTro,
        ChucNang,
        HanhDong,
        DoiTuong,
        MaDoiTuong,
        NoiDung,
        DuLieuTruoc,
        DuLieuSau,
        DiaChiIP,
        UserAgent
    FROM dbo.AuditLog
    WHERE
        (@TuNgay IS NULL OR ThoiGian >= CAST(@TuNgay AS DATETIME2(0)))
        AND (
            @DenNgay IS NULL
            OR ThoiGian < DATEADD(DAY, 1, CAST(@DenNgay AS DATETIME2(0)))
        )
        AND (@MaNV IS NULL OR MaNV = @MaNV)
        AND (@ChucNang IS NULL OR ChucNang = @ChucNang)
        AND (@HanhDong IS NULL OR HanhDong = @HanhDong)
        AND (@DoiTuong IS NULL OR DoiTuong = @DoiTuong)
        AND (@MaDoiTuong IS NULL OR MaDoiTuong = @MaDoiTuong)
    ORDER BY ThoiGian DESC, MaLog DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[AuditLog_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   5. Proc thêm audit log
   - C# gọi proc này sau khi thao tác nghiệp vụ thành công.
   ========================================================= */

CREATE   PROCEDURE [dbo].[AuditLog_Them]
    @MaNV NVARCHAR(50) = NULL,
    @TenDangNhap NVARCHAR(100) = NULL,
    @VaiTro NVARCHAR(100) = NULL,

    @ChucNang NVARCHAR(100),
    @HanhDong NVARCHAR(100),

    @DoiTuong NVARCHAR(100) = NULL,
    @MaDoiTuong NVARCHAR(100) = NULL,

    @NoiDung NVARCHAR(1000),

    @DuLieuTruoc NVARCHAR(MAX) = NULL,
    @DuLieuSau NVARCHAR(MAX) = NULL,

    @DiaChiIP NVARCHAR(100) = NULL,
    @UserAgent NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@ChucNang)), N'') IS NULL
    BEGIN
        RAISERROR(N'Chức năng ghi log không được để trống.', 16, 1);
        RETURN;
    END

    IF NULLIF(LTRIM(RTRIM(@HanhDong)), N'') IS NULL
    BEGIN
        RAISERROR(N'Hành động ghi log không được để trống.', 16, 1);
        RETURN;
    END

    IF NULLIF(LTRIM(RTRIM(@NoiDung)), N'') IS NULL
    BEGIN
        RAISERROR(N'Nội dung ghi log không được để trống.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.AuditLog
    (
        MaNV,
        TenDangNhap,
        VaiTro,
        ChucNang,
        HanhDong,
        DoiTuong,
        MaDoiTuong,
        NoiDung,
        DuLieuTruoc,
        DuLieuSau,
        DiaChiIP,
        UserAgent
    )
    VALUES
    (
        NULLIF(LTRIM(RTRIM(@MaNV)), N''),
        NULLIF(LTRIM(RTRIM(@TenDangNhap)), N''),
        NULLIF(LTRIM(RTRIM(@VaiTro)), N''),
        LTRIM(RTRIM(@ChucNang)),
        LTRIM(RTRIM(@HanhDong)),
        NULLIF(LTRIM(RTRIM(@DoiTuong)), N''),
        NULLIF(LTRIM(RTRIM(@MaDoiTuong)), N''),
        LTRIM(RTRIM(@NoiDung)),
        @DuLieuTruoc,
        @DuLieuSau,
        NULLIF(LTRIM(RTRIM(@DiaChiIP)), N''),
        NULLIF(LTRIM(RTRIM(@UserAgent)), N'')
    );

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS MaLog;
END
GO
/****** Object:  StoredProcedure [dbo].[AuditLog_TheoDoiTuong]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   7. Proc xem log theo 1 đối tượng cụ thể
   Ví dụ: xem lịch sử của hóa đơn HD001 hoặc bàn BAN01
   ========================================================= */

CREATE   PROCEDURE [dbo].[AuditLog_TheoDoiTuong]
    @DoiTuong NVARCHAR(100),
    @MaDoiTuong NVARCHAR(100),
    @Top INT = 100
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@DoiTuong)), N'') IS NULL
    BEGIN
        RAISERROR(N'Đối tượng không được để trống.', 16, 1);
        RETURN;
    END

    IF NULLIF(LTRIM(RTRIM(@MaDoiTuong)), N'') IS NULL
    BEGIN
        RAISERROR(N'Mã đối tượng không được để trống.', 16, 1);
        RETURN;
    END

    IF @Top IS NULL OR @Top <= 0 SET @Top = 100;
    IF @Top > 1000 SET @Top = 1000;

    SELECT TOP (@Top)
        MaLog,
        ThoiGian,
        MaNV,
        TenDangNhap,
        VaiTro,
        ChucNang,
        HanhDong,
        DoiTuong,
        MaDoiTuong,
        NoiDung,
        DuLieuTruoc,
        DuLieuSau,
        DiaChiIP,
        UserAgent
    FROM dbo.AuditLog
    WHERE DoiTuong = @DoiTuong
      AND MaDoiTuong = @MaDoiTuong
    ORDER BY ThoiGian DESC, MaLog DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[AuditLog_TheoNhanVien]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   8. Proc xem log theo nhân viên
   ========================================================= */

CREATE   PROCEDURE [dbo].[AuditLog_TheoNhanVien]
    @MaNV NVARCHAR(50),
    @TuNgay DATE = NULL,
    @DenNgay DATE = NULL,
    @Top INT = 200
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@MaNV)), N'') IS NULL
    BEGIN
        RAISERROR(N'Mã nhân viên không được để trống.', 16, 1);
        RETURN;
    END

    IF @Top IS NULL OR @Top <= 0 SET @Top = 200;
    IF @Top > 1000 SET @Top = 1000;

    SELECT TOP (@Top)
        MaLog,
        ThoiGian,
        MaNV,
        TenDangNhap,
        VaiTro,
        ChucNang,
        HanhDong,
        DoiTuong,
        MaDoiTuong,
        NoiDung,
        DuLieuTruoc,
        DuLieuSau,
        DiaChiIP,
        UserAgent
    FROM dbo.AuditLog
    WHERE MaNV = @MaNV
      AND (@TuNgay IS NULL OR ThoiGian >= CAST(@TuNgay AS DATETIME2(0)))
      AND (
            @DenNgay IS NULL
            OR ThoiGian < DATEADD(DAY, 1, CAST(@DenNgay AS DATETIME2(0)))
      )
    ORDER BY ThoiGian DESC, MaLog DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[Ban_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[Ban_CT] @MaBan varchar(5) AS BEGIN SET NOCOUNT ON; SELECT * FROM Ban WHERE MaBan=@MaBan; END;



GO
/****** Object:  StoredProcedure [dbo].[Ban_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   4. STORED PROCEDURE CRUD
   ========================================================= */

/* ---------- Bàn ---------- */
CREATE   PROC [dbo].[Ban_DS]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM dbo.Ban
    ORDER BY
        CASE WHEN MaBan = 'MVE' THEN 1 ELSE 0 END,
        CASE KhuVuc WHEN N'Tầng trệt' THEN 0 WHEN N'Tầng lửng' THEN 1 ELSE 2 END,
        SoBan,
        MaBan;
END;



GO
/****** Object:  StoredProcedure [dbo].[Ban_DongBoMaSoTen] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[Ban_DongBoMaSoTen]
    @MaBanCu varchar(5) = NULL,
    @MaBanMoi varchar(5) = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET ANSI_WARNINGS ON;
    SET ANSI_PADDING ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;

    BEGIN TRY
        BEGIN TRAN;

        IF OBJECT_ID('tempdb..#BanMap') IS NOT NULL
            DROP TABLE #BanMap;

        SELECT
            b.MaBan AS MaBanCu,
            CAST('T' + RIGHT('0000' + CONVERT(varchar(4), ROW_NUMBER() OVER (
                ORDER BY
                    CASE b.KhuVuc WHEN N'Tầng trệt' THEN 0 WHEN N'Tầng lửng' THEN 1 ELSE 2 END,
                    b.SoBan,
                    b.MaBan
            )), 4) AS varchar(5)) AS MaBanTam,
            CAST('BAN' + RIGHT('00' + CONVERT(varchar(2), ROW_NUMBER() OVER (
                ORDER BY
                    CASE b.KhuVuc WHEN N'Tầng trệt' THEN 0 WHEN N'Tầng lửng' THEN 1 ELSE 2 END,
                    b.SoBan,
                    b.MaBan
            )), 2) AS varchar(5)) AS MaBanMoi,
            CONVERT(int, ROW_NUMBER() OVER (
                ORDER BY
                    CASE b.KhuVuc WHEN N'Tầng trệt' THEN 0 WHEN N'Tầng lửng' THEN 1 ELSE 2 END,
                    b.SoBan,
                    b.MaBan
            )) AS SoBanMoi,
            b.KhuVuc,
            b.TrangThai
        INTO #BanMap
        FROM dbo.Ban b WITH (UPDLOCK, HOLDLOCK)
        WHERE b.MaBan <> 'MVE';

        IF EXISTS (SELECT 1 FROM #BanMap WHERE SoBanMoi > 99)
        BEGIN
            THROW 50001, N'Hệ thống chỉ hỗ trợ tối đa 99 bàn vật lý.', 1;
        END;

        IF EXISTS (
            SELECT 1
            FROM #BanMap m
            INNER JOIN dbo.Ban b ON b.MaBan = m.MaBanTam
        )
        BEGIN
            THROW 50002, N'Không thể tạo mã bàn tạm để đồng bộ dữ liệu.', 1;
        END;

        INSERT INTO dbo.Ban(MaBan, TenBan, SoBan, KhuVuc, TrangThai)
        SELECT
            MaBanTam,
            N'Tạm ' + CONVERT(nvarchar(4), SoBanMoi),
            10000 + SoBanMoi,
            KhuVuc,
            TrangThai
        FROM #BanMap;

        UPDATE hd
        SET MaBan = m.MaBanTam
        FROM dbo.HoaDon hd
        INNER JOIN #BanMap m ON m.MaBanCu = hd.MaBan;

        DELETE b
        FROM dbo.Ban b
        INNER JOIN #BanMap m ON m.MaBanCu = b.MaBan;

        INSERT INTO dbo.Ban(MaBan, TenBan, SoBan, KhuVuc, TrangThai)
        SELECT
            MaBanMoi,
            N'Bàn ' + RIGHT(N'00' + CONVERT(nvarchar(2), SoBanMoi), 2),
            SoBanMoi,
            KhuVuc,
            TrangThai
        FROM #BanMap;

        UPDATE hd
        SET MaBan = m.MaBanMoi
        FROM dbo.HoaDon hd
        INNER JOIN #BanMap m ON m.MaBanTam = hd.MaBan;

        DELETE b
        FROM dbo.Ban b
        INNER JOIN #BanMap m ON m.MaBanTam = b.MaBan;

        SELECT @MaBanMoi = MaBanMoi
        FROM #BanMap
        WHERE MaBanCu = @MaBanCu;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[Ban_KhachDi]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   8. Khách đi / trả bàn
   - Đây là proc chính để trả bàn về Trống sau khi khách rời quán
   - Không phụ thuộc hóa đơn đã thanh toán hay chưa
   - Chặn MVE
   ========================================================= */
CREATE   PROC [dbo].[Ban_KhachDi]
    @MaBan varchar(5)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @MaBan IS NULL OR LTRIM(RTRIM(@MaBan)) = ''
    BEGIN
        RAISERROR(N'Mã bàn không được để trống.', 16, 1);
        RETURN;
    END;

    IF @MaBan = 'MVE'
    BEGIN
        RAISERROR(N'MVE là đơn mang về, không phải bàn vật lý.', 16, 1);
        RETURN;
    END;

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @CurrentStatus nvarchar(30);

        SELECT @CurrentStatus = TrangThai
        FROM dbo.Ban WITH (UPDLOCK, HOLDLOCK)
        WHERE MaBan = @MaBan;

        IF @CurrentStatus IS NULL
        BEGIN
            RAISERROR(N'Bàn không tồn tại.', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END;

        IF @CurrentStatus = N'Trống'
        BEGIN
            COMMIT TRAN;
            SELECT
                CAST(0 AS bit) AS Success,
                N'Bàn đã Trống, không cần trả bàn.' AS Message,
                @MaBan AS MaBan;
            RETURN;
        END;

        IF EXISTS (
            SELECT 1
            FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
            WHERE MaBan = @MaBan
              AND TrangThaiOrder <> N'DaHuy'
              AND TrangThaiThanhToan <> N'DaThanhToan'
        )
        BEGIN
            RAISERROR(N'Bàn còn order chưa thanh toán, không thể trả bàn.', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END;

        UPDATE dbo.Ban
        SET TrangThai = N'Trống'
        WHERE MaBan = @MaBan;

        COMMIT TRAN;

        SELECT
            CAST(1 AS bit) AS Success,
            N'Khách đi, bàn đã được chuyển về Trống.' AS Message,
            @MaBan AS MaBan;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH;
END

GO
/****** Object:  StoredProcedure [dbo].[Ban_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[Ban_Sua]
    @MaBan varchar(5),
    @TenBan nvarchar(20) = NULL,
    @KhuVuc nvarchar(20),
    @TrangThai nvarchar(30) = N'Trống'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET ANSI_WARNINGS ON;
    SET ANSI_PADDING ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;

    SET @MaBan = UPPER(LTRIM(RTRIM(ISNULL(@MaBan, ''))));
    SET @KhuVuc = NULLIF(LTRIM(RTRIM(ISNULL(@KhuVuc, N''))), N'');
    SET @TrangThai = LTRIM(RTRIM(ISNULL(@TrangThai, N'Trống')));

    IF @MaBan = ''
    BEGIN
        RAISERROR(N'Mã bàn không được để trống.', 16, 1);
        RETURN;
    END;

    IF @MaBan = 'MVE'
    BEGIN
        RAISERROR(N'MVE là bàn đặc biệt cho đơn mang về, không thể chỉnh sửa.', 16, 1);
        RETURN;
    END;

    IF @KhuVuc = N'Tầng trệt'
        SET @KhuVuc = N'Tầng trệt';
    ELSE IF @KhuVuc = N'Tầng lửng'
        SET @KhuVuc = N'Tầng lửng';
    ELSE
    BEGIN
        RAISERROR(N'Khu vực chỉ được chọn Tầng trệt hoặc Tầng lửng.', 16, 1);
        RETURN;
    END;

    IF @TrangThai NOT IN (N'Trống', N'Đang phục vụ')
    BEGIN
        RAISERROR(N'Trạng thái bàn không hợp lệ. Chỉ được dùng Trống hoặc Đang phục vụ.', 16, 1);
        RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Ban WHERE MaBan = @MaBan)
    BEGIN
        RAISERROR(N'Bàn không tồn tại.', 16, 1);
        RETURN;
    END;

    BEGIN TRY
        BEGIN TRAN;
        DECLARE @KhuVucCu nvarchar(20);
        DECLARE @SoBanTam int;
        DECLARE @MaBanMoi varchar(5);

        SELECT @KhuVucCu = KhuVuc
        FROM dbo.Ban WITH (UPDLOCK, HOLDLOCK)
        WHERE MaBan = @MaBan;

        IF @KhuVucCu <> @KhuVuc
        BEGIN
            SELECT @SoBanTam = ISNULL(MAX(SoBan), 0) + 1
            FROM dbo.Ban WITH (UPDLOCK, HOLDLOCK)
            WHERE MaBan <> 'MVE';
        END;

        IF @TrangThai = N'Trống' AND EXISTS (
            SELECT 1
            FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
            WHERE MaBan = @MaBan
              AND TrangThaiOrder <> N'DaHuy'
              AND TrangThaiThanhToan <> N'DaThanhToan'
        )
        BEGIN
            RAISERROR(N'Bàn đang có hóa đơn chưa thanh toán, không thể chuyển về trạng thái Trống.', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END;

        UPDATE dbo.Ban
        SET KhuVuc = @KhuVuc,
            SoBan = CASE WHEN @KhuVucCu <> @KhuVuc THEN @SoBanTam ELSE SoBan END,
            TrangThai = @TrangThai
        WHERE MaBan = @MaBan;

        EXEC dbo.Ban_DongBoMaSoTen
            @MaBanCu = @MaBan,
            @MaBanMoi = @MaBanMoi OUTPUT;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;
        THROW;
    END CATCH;

    SELECT
        CAST(1 AS bit) AS Success,
        N'Cập nhật bàn thành công.' AS Message,
        @MaBanMoi AS MaBan,
        @TrangThai AS TrangThai;
END;


GO
/****** Object:  StoredProcedure [dbo].[Ban_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[Ban_Them]
    @MaBan varchar(5) = NULL,
    @TenBan nvarchar(20) = NULL,
    @KhuVuc nvarchar(20),
    @TrangThai nvarchar(30) = N'Trống'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET ANSI_WARNINGS ON;
    SET ANSI_PADDING ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;

    SET @MaBan = NULLIF(UPPER(LTRIM(RTRIM(ISNULL(@MaBan, '')))), '');
    SET @KhuVuc = NULLIF(LTRIM(RTRIM(ISNULL(@KhuVuc, N''))), N'');

    IF @MaBan IS NOT NULL
    BEGIN
        RAISERROR(N'Mã bàn do hệ thống tự sinh, không được nhập thủ công.', 16, 1);
        RETURN;
    END;

    IF @KhuVuc = N'Tầng trệt'
        SET @KhuVuc = N'Tầng trệt';
    ELSE IF @KhuVuc = N'Tầng lửng'
        SET @KhuVuc = N'Tầng lửng';
    ELSE
    BEGIN
        RAISERROR(N'Khu vực chỉ được chọn Tầng trệt hoặc Tầng lửng.', 16, 1);
        RETURN;
    END;

    SET @TrangThai = N'Trống';

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @SoBanTam int;
        DECLARE @ThuTuTam int = 1;
        DECLARE @MaBanTam varchar(5);

        SELECT @SoBanTam = ISNULL(MAX(SoBan), 0) + 1
        FROM dbo.Ban WITH (UPDLOCK, HOLDLOCK)
        WHERE MaBan <> 'MVE';

        WHILE @ThuTuTam <= 9999
        BEGIN
            SET @MaBanTam = 'N' + RIGHT('0000' + CONVERT(varchar(4), @ThuTuTam), 4);
            IF NOT EXISTS (SELECT 1 FROM dbo.Ban WHERE MaBan = @MaBanTam)
                BREAK;
            SET @ThuTuTam += 1;
        END;

        IF @ThuTuTam > 9999
            THROW 50003, N'Không thể tạo mã bàn tạm để thêm bàn.', 1;

        INSERT INTO dbo.Ban(MaBan, TenBan, SoBan, KhuVuc, TrangThai)
        VALUES(@MaBanTam, N'Tạm mới', @SoBanTam, @KhuVuc, @TrangThai);

        EXEC dbo.Ban_DongBoMaSoTen
            @MaBanCu = @MaBanTam,
            @MaBanMoi = @MaBan OUTPUT;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;
        THROW;
    END CATCH;

    SELECT
        CAST(1 AS bit) AS Success,
        N'Thêm bàn thành công.' AS Message,
        @MaBan AS MaBan,
        (SELECT SoBan FROM dbo.Ban WHERE MaBan = @MaBan) AS SoBan,
        @TrangThai AS TrangThai;
END;


GO
/****** Object:  StoredProcedure [dbo].[Ban_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[Ban_Xoa]
    @MaBan varchar(5)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET ANSI_WARNINGS ON;
    SET ANSI_PADDING ON;
    SET ARITHABORT ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET NUMERIC_ROUNDABORT OFF;

    SET @MaBan = UPPER(LTRIM(RTRIM(ISNULL(@MaBan, ''))));

    IF @MaBan = ''
    BEGIN
        RAISERROR(N'Mã bàn không được để trống.', 16, 1);
        RETURN;
    END;

    IF @MaBan = 'MVE'
    BEGIN
        RAISERROR(N'MVE là bàn đặc biệt cho đơn mang về, không thể xóa.', 16, 1);
        RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Ban WHERE MaBan = @MaBan)
    BEGIN
        RAISERROR(N'Bàn không tồn tại.', 16, 1);
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM dbo.HoaDon WHERE MaBan = @MaBan)
    BEGIN
        RAISERROR(N'Bàn đã phát sinh dữ liệu, không thể xóa để bảo toàn lịch sử.', 16, 1);
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM dbo.Ban WHERE MaBan = @MaBan AND TrangThai <> N'Trống')
    BEGIN
        RAISERROR(N'Chỉ có thể xóa bàn đang Trống.', 16, 1);
        RETURN;
    END;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM dbo.Ban
        WHERE MaBan = @MaBan;

        EXEC dbo.Ban_DongBoMaSoTen;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;
        THROW;
    END CATCH;

    SELECT
        CAST(1 AS bit) AS Success,
        N'Xóa bàn thành công.' AS Message,
        @MaBan AS MaBan;
END;

GO
/****** Object:  StoredProcedure [dbo].[ChamCong_CaLamTheoThang]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   2. ChamCong_CaLamTheoThang: thêm @Nam tùy chọn
   ========================================================= */
CREATE   PROC [dbo].[ChamCong_CaLamTheoThang]
    @Thang int,
    @Nam int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Thang NOT BETWEEN 1 AND 12
    BEGIN
        RAISERROR(N'Tháng không hợp lệ.', 16, 1);
        RETURN;
    END;

    SET @Nam = ISNULL(@Nam, YEAR(SYSDATETIME()));

    IF @Nam < 2000 OR @Nam > 2100
    BEGIN
        RAISERROR(N'Năm không hợp lệ.', 16, 1);
        RETURN;
    END;

    DECLARE @TuNgay date = DATEFROMPARTS(@Nam, @Thang, 1);
    DECLARE @DenNgay date = EOMONTH(@TuNgay);

    SELECT
        cc.MaCC,
        cc.MaNV,
        nv.TenNV,
        nv.ChucVu,
        cc.ThoiGianCheckIn,
        cc.ThoiGianCheckOut,
        cc.DonGiaGio,
        cc.TongPhut,
        cc.TongGio,
        cc.TienCong AS TienCongTheoCa,
        cc.TrangThai,
        cc.GhiChu,
        @Thang AS Thang,
        @Nam AS Nam,
        @TuNgay AS TuNgay,
        @DenNgay AS DenNgay
    FROM dbo.ChamCong cc
    INNER JOIN dbo.NhanVien nv ON cc.MaNV = nv.MaNV
    WHERE cc.TrangThai = N'Đã checkout'
      AND cc.ThoiGianCheckOut IS NOT NULL
      AND CAST(cc.ThoiGianCheckIn AS date) BETWEEN @TuNgay AND @DenNgay
    ORDER BY nv.MaNV, cc.ThoiGianCheckIn;
END;

GO
/****** Object:  StoredProcedure [dbo].[ChamCong_CheckIn]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   5. CHẤM CÔNG + LƯƠNG THEO GIỜ
   ========================================================= */

CREATE   PROC [dbo].[ChamCong_CheckIn] @MaNV varchar(5), @GhiChu nvarchar(200)=NULL AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV=@MaNV) BEGIN RAISERROR(N'Nhân viên không tồn tại.',16,1); ROLLBACK; RETURN; END
        IF EXISTS (SELECT 1 FROM ChamCong WITH(UPDLOCK,HOLDLOCK) WHERE MaNV=@MaNV AND TrangThai=N'Đang làm' AND ThoiGianCheckOut IS NULL)
        BEGIN RAISERROR(N'Nhân viên đã check-in và chưa check-out.',16,1); ROLLBACK; RETURN; END
        DECLARE @DonGiaGio money;
        SELECT @DonGiaGio=LuongTheoGio FROM NhanVien WHERE MaNV=@MaNV;
        INSERT INTO ChamCong(MaNV,ThoiGianCheckIn,ThoiGianCheckOut,TrangThai,GhiChu,DonGiaGio,TongPhut,TongGio,TienCong)
        VALUES(@MaNV,SYSDATETIME(),NULL,N'Đang làm',@GhiChu,@DonGiaGio,NULL,NULL,NULL);
        COMMIT;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;



GO
/****** Object:  StoredProcedure [dbo].[ChamCong_CheckOut]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   FINAL PRODUCTION-LOGIC PATCH - 2026-05-29
   Mục tiêu:
   1. Chấm công: ép kiểu rõ, làm tròn tiền công VNĐ.
   2. Lương/chấm công theo tháng: hỗ trợ @Nam, vẫn tương thích khi C# chỉ truyền @Thang.
   3. Khuyến mãi trong hóa đơn: đồng bộ SYSDATETIME().
   4. Xóa hóa đơn: không crash FK ThanhToan, không xóa hóa đơn đã thanh toán.
   5. QR webhook đến muộn sau khi hết hạn: đưa vào Chờ kiểm tra, không tự hủy cứng khi tiền đã về.
   ========================================================= */

/* =========================================================
   1. ChamCong_CheckOut: giữ đúng chữ ký hiện tại @MaNV, @MatKhau
   ========================================================= */
CREATE   PROC [dbo].[ChamCong_CheckOut]
    @MaNV varchar(5),
    @MatKhau varchar(255)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.TaiKhoan
            WHERE MaNV = @MaNV
              AND MatKhau = @MatKhau
        )
        BEGIN
            RAISERROR(N'Mật khẩu không đúng.', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END;

        DECLARE @MaCC int;

        SELECT TOP 1
            @MaCC = MaCC
        FROM dbo.ChamCong WITH (UPDLOCK, HOLDLOCK)
        WHERE MaNV = @MaNV
          AND TrangThai = N'Đang làm'
          AND ThoiGianCheckOut IS NULL
        ORDER BY ThoiGianCheckIn DESC;

        IF @MaCC IS NULL
        BEGIN
            RAISERROR(N'Nhân viên chưa check-in hoặc đã check-out.', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END;

        DECLARE
            @CheckIn datetime2(0),
            @CheckOut datetime2(0) = SYSDATETIME(),
            @DonGiaGio money,
            @TongPhut int,
            @TongGio decimal(10,2),
            @TienCong money;

        SELECT
            @CheckIn = ThoiGianCheckIn,
            @DonGiaGio = DonGiaGio
        FROM dbo.ChamCong
        WHERE MaCC = @MaCC;

        SET @TongPhut = DATEDIFF(MINUTE, @CheckIn, @CheckOut);

        IF @TongPhut < 0
        BEGIN
            SET @TongPhut = 0;
        END;

        SET @TongGio = ROUND(CAST(@TongPhut AS decimal(10,2)) / 60.0, 2);
        SET @TienCong = CAST(ROUND((CAST(@TongPhut AS decimal(18,4)) / 60.0) * CAST(@DonGiaGio AS decimal(18,4)), 0) AS money);

        UPDATE dbo.ChamCong
        SET ThoiGianCheckOut = @CheckOut,
            TrangThai = N'Đã checkout',
            TongPhut = @TongPhut,
            TongGio = @TongGio,
            TienCong = @TienCong
        WHERE MaCC = @MaCC;

        COMMIT TRAN;

        SELECT
            CAST(1 AS bit) AS Success,
            N'Check-out thành công.' AS Message,
            @MaCC AS MaCC,
            @MaNV AS MaNV,
            @TongPhut AS TongPhut,
            @TongGio AS TongGio,
            @TienCong AS TienCong;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END;

GO
/****** Object:  StoredProcedure [dbo].[ChamCong_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[ChamCong_CT] @MaCC int AS
BEGIN
    SET NOCOUNT ON;
    SELECT cc.MaCC,cc.MaNV,nv.TenNV,nv.ChucVu,cc.ThoiGianCheckIn,cc.ThoiGianCheckOut,cc.TrangThai,cc.GhiChu,cc.DonGiaGio,cc.TongPhut,cc.TongGio,cc.TienCong
    FROM ChamCong cc INNER JOIN NhanVien nv ON cc.MaNV=nv.MaNV WHERE cc.MaCC=@MaCC;
END;



GO
/****** Object:  StoredProcedure [dbo].[ChamCong_DangLam]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[ChamCong_DangLam] AS
BEGIN
    SET NOCOUNT ON;
    SELECT cc.MaCC,cc.MaNV,nv.TenNV,nv.ChucVu,cc.ThoiGianCheckIn,cc.ThoiGianCheckOut,cc.TrangThai,cc.GhiChu,cc.DonGiaGio,
           DATEDIFF(MINUTE,cc.ThoiGianCheckIn,SYSDATETIME()) AS TongPhutTamTinh,
           CAST(DATEDIFF(MINUTE,cc.ThoiGianCheckIn,SYSDATETIME())/60.0 AS decimal(10,2)) AS TongGioTamTinh,
           CONCAT(DATEDIFF(MINUTE,cc.ThoiGianCheckIn,SYSDATETIME())/60,N' giờ ',DATEDIFF(MINUTE,cc.ThoiGianCheckIn,SYSDATETIME())%60,N' phút') AS ThoiGianDaLam,
           CAST(CAST(DATEDIFF(MINUTE,cc.ThoiGianCheckIn,SYSDATETIME())/60.0 AS decimal(10,2))*cc.DonGiaGio AS money) AS TienCongTamTinh
    FROM ChamCong cc INNER JOIN NhanVien nv ON cc.MaNV=nv.MaNV
    WHERE cc.TrangThai=N'Đang làm' AND cc.ThoiGianCheckOut IS NULL
    ORDER BY cc.ThoiGianCheckIn;
END;



GO
/****** Object:  StoredProcedure [dbo].[ChamCong_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROC [dbo].[ChamCong_DS] AS
BEGIN
    SET NOCOUNT ON;
    SELECT cc.MaCC,cc.MaNV,nv.TenNV,nv.ChucVu,cc.ThoiGianCheckIn,cc.ThoiGianCheckOut,cc.TrangThai,cc.GhiChu,cc.DonGiaGio,cc.TongPhut,cc.TongGio,cc.TienCong
    FROM ChamCong cc INNER JOIN NhanVien nv ON cc.MaNV=nv.MaNV
    ORDER BY cc.ThoiGianCheckIn DESC;
END;



GO
/****** Object:  StoredProcedure [dbo].[ChamCong_KiemTraDangLam]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[ChamCong_KiemTraDangLam] @MaNV varchar(5) AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 cc.MaCC,cc.MaNV,nv.TenNV,nv.ChucVu,cc.ThoiGianCheckIn,cc.ThoiGianCheckOut,cc.TrangThai,cc.GhiChu,cc.DonGiaGio,
           DATEDIFF(MINUTE,cc.ThoiGianCheckIn,SYSDATETIME()) AS TongPhutTamTinh,
           CAST(DATEDIFF(MINUTE,cc.ThoiGianCheckIn,SYSDATETIME())/60.0 AS decimal(10,2)) AS TongGioTamTinh,
           CONCAT(DATEDIFF(MINUTE,cc.ThoiGianCheckIn,SYSDATETIME())/60,N' giờ ',DATEDIFF(MINUTE,cc.ThoiGianCheckIn,SYSDATETIME())%60,N' phút') AS ThoiGianDaLam,
           CAST(CAST(DATEDIFF(MINUTE,cc.ThoiGianCheckIn,SYSDATETIME())/60.0 AS decimal(10,2))*cc.DonGiaGio AS money) AS TienCongTamTinh
    FROM ChamCong cc INNER JOIN NhanVien nv ON cc.MaNV=nv.MaNV
    WHERE cc.MaNV=@MaNV AND cc.TrangThai=N'Đang làm' AND cc.ThoiGianCheckOut IS NULL
    ORDER BY cc.ThoiGianCheckIn DESC;
END;



GO
/****** Object:  StoredProcedure [dbo].[ChamCong_LichSuTheoNgay]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[ChamCong_LichSuTheoNgay] @Ngay date AS
BEGIN
    SET NOCOUNT ON;
    SELECT cc.MaCC,cc.MaNV,nv.TenNV,nv.ChucVu,cc.ThoiGianCheckIn,cc.ThoiGianCheckOut,cc.TrangThai,cc.GhiChu,cc.DonGiaGio,cc.TongPhut,cc.TongGio,cc.TienCong
    FROM ChamCong cc INNER JOIN NhanVien nv ON cc.MaNV=nv.MaNV
    WHERE CAST(cc.ThoiGianCheckIn AS date)=@Ngay
    ORDER BY cc.ThoiGianCheckIn DESC;
END;



GO
/****** Object:  StoredProcedure [dbo].[ChamCong_TongLuongNhanVienTheoThang]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   3. ChamCong_TongLuongNhanVienTheoThang: thêm @Nam tùy chọn
   ========================================================= */
CREATE   PROC [dbo].[ChamCong_TongLuongNhanVienTheoThang]
    @Thang int,
    @Nam int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Thang NOT BETWEEN 1 AND 12
    BEGIN
        RAISERROR(N'Tháng không hợp lệ.', 16, 1);
        RETURN;
    END;

    SET @Nam = ISNULL(@Nam, YEAR(SYSDATETIME()));

    IF @Nam < 2000 OR @Nam > 2100
    BEGIN
        RAISERROR(N'Năm không hợp lệ.', 16, 1);
        RETURN;
    END;

    DECLARE @TuNgay date = DATEFROMPARTS(@Nam, @Thang, 1);
    DECLARE @DenNgay date = EOMONTH(@TuNgay);

    SELECT
        nv.MaNV,
        nv.TenNV,
        nv.ChucVu,
        nv.LuongTheoGio,
        COUNT(cc.MaCC) AS SoCaLam,
        SUM(ISNULL(cc.TongPhut, 0)) AS TongPhut,
        CAST(CAST(SUM(ISNULL(cc.TongPhut, 0)) AS decimal(18,4)) / 60.0 AS decimal(10,2)) AS TongGio,
        CAST(ROUND((CAST(SUM(ISNULL(cc.TongPhut, 0)) AS decimal(18,4)) / 60.0) * CAST(nv.LuongTheoGio AS decimal(18,4)), 0) AS money) AS TongLuong,
        SUM(ISNULL(cc.TienCong, 0)) AS TongTienCongTheoCaThamKhao,
        @Thang AS Thang,
        @Nam AS Nam,
        @TuNgay AS TuNgay,
        @DenNgay AS DenNgay
    FROM dbo.NhanVien nv
    LEFT JOIN dbo.ChamCong cc
        ON nv.MaNV = cc.MaNV
       AND cc.TrangThai = N'Đã checkout'
       AND cc.ThoiGianCheckOut IS NOT NULL
       AND CAST(cc.ThoiGianCheckIn AS date) BETWEEN @TuNgay AND @DenNgay
    GROUP BY
        nv.MaNV,
        nv.TenNV,
        nv.ChucVu,
        nv.LuongTheoGio
    ORDER BY nv.MaNV;
END;

GO
/****** Object:  StoredProcedure [dbo].[ChamCong_TongLuongTatCaTheoThang]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   4. ChamCong_TongLuongTatCaTheoThang: thêm @Nam tùy chọn
   ========================================================= */
CREATE   PROC [dbo].[ChamCong_TongLuongTatCaTheoThang]
    @Thang int,
    @Nam int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Thang NOT BETWEEN 1 AND 12
    BEGIN
        RAISERROR(N'Tháng không hợp lệ.', 16, 1);
        RETURN;
    END;

    SET @Nam = ISNULL(@Nam, YEAR(SYSDATETIME()));

    IF @Nam < 2000 OR @Nam > 2100
    BEGIN
        RAISERROR(N'Năm không hợp lệ.', 16, 1);
        RETURN;
    END;

    DECLARE @TuNgay date = DATEFROMPARTS(@Nam, @Thang, 1);
    DECLARE @DenNgay date = EOMONTH(@TuNgay);

    ;WITH TongTheoNhanVien AS
    (
        SELECT
            nv.MaNV,
            nv.LuongTheoGio,
            COUNT(cc.MaCC) AS SoCaLam,
            SUM(ISNULL(cc.TongPhut, 0)) AS TongPhut,
            CAST(CAST(SUM(ISNULL(cc.TongPhut, 0)) AS decimal(18,4)) / 60.0 AS decimal(10,2)) AS TongGio,
            CAST(ROUND((CAST(SUM(ISNULL(cc.TongPhut, 0)) AS decimal(18,4)) / 60.0) * CAST(nv.LuongTheoGio AS decimal(18,4)), 0) AS money) AS TongLuong
        FROM dbo.NhanVien nv
        LEFT JOIN dbo.ChamCong cc
            ON nv.MaNV = cc.MaNV
           AND cc.TrangThai = N'Đã checkout'
           AND cc.ThoiGianCheckOut IS NOT NULL
           AND CAST(cc.ThoiGianCheckIn AS date) BETWEEN @TuNgay AND @DenNgay
        GROUP BY
            nv.MaNV,
            nv.LuongTheoGio
    )
    SELECT
        @Thang AS Thang,
        @Nam AS Nam,
        @TuNgay AS TuNgay,
        @DenNgay AS DenNgay,
        SUM(SoCaLam) AS TongSoCaLam,
        COUNT(CASE WHEN SoCaLam > 0 THEN 1 END) AS TongNhanVienCoLam,
        SUM(TongPhut) AS TongPhut,
        SUM(TongGio) AS TongGio,
        SUM(TongLuong) AS TongLuongPhaiChuyen
    FROM TongTheoNhanVien;
END;

GO
/****** Object:  StoredProcedure [dbo].[CongThuc_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CongThuc_CT] @MaSP varchar(5), @MaNL varchar(5) AS
BEGIN SET NOCOUNT ON; SELECT ct.MaSP,sp.TenSP,ct.MaNL,nl.TenNL,nl.DonViTinh,ct.DinhLuong FROM CongThuc ct INNER JOIN SanPham sp ON ct.MaSP=sp.MaSP INNER JOIN NguyenLieu nl ON ct.MaNL=nl.MaNL WHERE ct.MaSP=@MaSP AND ct.MaNL=@MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[CongThuc_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Công thức ---------- */
CREATE   PROC [dbo].[CongThuc_DS] AS
BEGIN SET NOCOUNT ON; SELECT ct.MaSP,sp.TenSP,ct.MaNL,nl.TenNL,nl.DonViTinh,ct.DinhLuong FROM CongThuc ct INNER JOIN SanPham sp ON ct.MaSP=sp.MaSP INNER JOIN NguyenLieu nl ON ct.MaNL=nl.MaNL ORDER BY ct.MaSP,ct.MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[CongThuc_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CongThuc_Sua] @MaSP varchar(5), @MaNL varchar(5), @DinhLuong decimal(18,3) AS BEGIN SET NOCOUNT ON; UPDATE CongThuc SET DinhLuong=@DinhLuong WHERE MaSP=@MaSP AND MaNL=@MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[CongThuc_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CongThuc_Them] @MaSP varchar(5), @MaNL varchar(5), @DinhLuong decimal(18,3) AS BEGIN SET NOCOUNT ON; INSERT INTO CongThuc(MaSP,MaNL,DinhLuong) VALUES(@MaSP,@MaNL,@DinhLuong); END;



GO
/****** Object:  StoredProcedure [dbo].[CongThuc_TheoSanPham]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CongThuc_TheoSanPham] @MaSP varchar(5) AS
BEGIN SET NOCOUNT ON; SELECT ct.MaSP,sp.TenSP,ct.MaNL,nl.TenNL,nl.DonViTinh,ct.DinhLuong FROM CongThuc ct INNER JOIN SanPham sp ON ct.MaSP=sp.MaSP INNER JOIN NguyenLieu nl ON ct.MaNL=nl.MaNL WHERE ct.MaSP=@MaSP ORDER BY ct.MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[CongThuc_LuuNhieu] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[CongThuc_LuuNhieu]
    @MaSP varchar(5),
    @DanhSachJson nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @MaSP = NULLIF(UPPER(LTRIM(RTRIM(ISNULL(@MaSP, '')))), '');

    IF @MaSP IS NULL
        THROW 50020, N'Vui lòng chọn sản phẩm.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.SanPham WHERE MaSP = @MaSP)
        THROW 50021, N'Sản phẩm không tồn tại.', 1;

    IF ISJSON(@DanhSachJson) <> 1
        THROW 50022, N'Danh sách nguyên liệu không hợp lệ.', 1;

    CREATE TABLE #NguyenLieuCongThuc
    (
        MaNL varchar(5) NULL,
        DinhLuong decimal(18,3) NULL
    );

    INSERT INTO #NguyenLieuCongThuc(MaNL, DinhLuong)
    SELECT
        NULLIF(UPPER(LTRIM(RTRIM(MaNL))), ''),
        DinhLuong
    FROM OPENJSON(@DanhSachJson)
    WITH
    (
        MaNL varchar(5) '$.MaNL',
        DinhLuong decimal(18,3) '$.DinhLuong'
    );

    IF NOT EXISTS (SELECT 1 FROM #NguyenLieuCongThuc)
        THROW 50023, N'Công thức phải có ít nhất một nguyên liệu.', 1;

    IF EXISTS (SELECT 1 FROM #NguyenLieuCongThuc WHERE MaNL IS NULL)
        THROW 50024, N'Vui lòng chọn nguyên liệu cho tất cả các dòng.', 1;

    IF EXISTS (SELECT 1 FROM #NguyenLieuCongThuc WHERE DinhLuong IS NULL OR DinhLuong <= 0)
        THROW 50025, N'Định lượng phải lớn hơn 0.', 1;

    IF EXISTS (
        SELECT MaNL
        FROM #NguyenLieuCongThuc
        GROUP BY MaNL
        HAVING COUNT(*) > 1
    )
        THROW 50026, N'Không được chọn trùng nguyên liệu trong cùng một công thức.', 1;

    IF EXISTS (
        SELECT 1
        FROM #NguyenLieuCongThuc item
        LEFT JOIN dbo.NguyenLieu nl ON nl.MaNL = item.MaNL
        WHERE nl.MaNL IS NULL
    )
        THROW 50027, N'Nguyên liệu không tồn tại.', 1;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM dbo.CongThuc
        WHERE MaSP = @MaSP;

        INSERT INTO dbo.CongThuc(MaSP, MaNL, DinhLuong)
        SELECT @MaSP, MaNL, DinhLuong
        FROM #NguyenLieuCongThuc;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;
        THROW;
    END CATCH;

    SELECT
        CAST(1 AS bit) AS Success,
        N'Lưu công thức thành công.' AS Message,
        @MaSP AS MaSP,
        COUNT(*) AS SoNguyenLieu
    FROM #NguyenLieuCongThuc;
END;
GO
/****** Object:  StoredProcedure [dbo].[CongThuc_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CongThuc_Xoa] @MaSP varchar(5), @MaNL varchar(5) AS BEGIN SET NOCOUNT ON; DELETE FROM CongThuc WHERE MaSP=@MaSP AND MaNL=@MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[CongThuc_XoaTheoSanPham] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[CongThuc_XoaTheoSanPham]
    @MaSP varchar(5)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @MaSP = NULLIF(UPPER(LTRIM(RTRIM(ISNULL(@MaSP, '')))), '');

    IF @MaSP IS NULL
        THROW 50030, N'Vui lòng chọn sản phẩm.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.SanPham WHERE MaSP = @MaSP)
        THROW 50031, N'Sản phẩm không tồn tại.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.CongThuc WHERE MaSP = @MaSP)
        THROW 50032, N'Sản phẩm không có công thức để xóa.', 1;

    BEGIN TRY
        BEGIN TRAN;

        DELETE FROM dbo.CongThuc
        WHERE MaSP = @MaSP;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[CTHD_DSTheoMaHD]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CTHD_DSTheoMaHD] @MaHD varchar(9) AS
BEGIN SET NOCOUNT ON; SELECT ct.MaHD,ct.MaSP,sp.TenSP,ct.SoLuong,ct.DonGia,ct.ThanhTien,ct.MaKM,km.TenKM FROM CTHD ct INNER JOIN SanPham sp ON ct.MaSP=sp.MaSP LEFT JOIN KhuyenMai km ON ct.MaKM=km.MaKM WHERE ct.MaHD=@MaHD ORDER BY ct.MaSP; END;



GO
/****** Object:  StoredProcedure [dbo].[HoaDon_CapNhatTongTien]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   5. HoaDon_CapNhatTongTien: thống nhất SYSDATETIME()
   ========================================================= */
CREATE   PROC [dbo].[HoaDon_CapNhatTongTien]
    @MaHD varchar(9)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.HoaDon
    SET TongTien = ISNULL((
        SELECT SUM(
            CASE
                WHEN km.MaKM IS NULL THEN ct.SoLuong * ct.DonGia
                WHEN km.LoaiKM = N'Tiền mặt' THEN
                    CASE WHEN (ct.SoLuong * ct.DonGia) - km.GiaTri < 0 THEN 0 ELSE (ct.SoLuong * ct.DonGia) - km.GiaTri END
                WHEN km.LoaiKM = N'Phần trăm' THEN
                    CASE WHEN km.GiaTri >= 100 THEN 0 ELSE (ct.SoLuong * ct.DonGia) * (1 - km.GiaTri / 100.0) END
                ELSE ct.SoLuong * ct.DonGia
            END
        )
        FROM dbo.CTHD ct
        LEFT JOIN dbo.KhuyenMai km
            ON ct.MaKM = km.MaKM
            AND CAST(SYSDATETIME() AS date) BETWEEN km.NgayBatDau AND km.NgayKetThuc
            AND km.TrangThai = N'Đang áp dụng'
            AND km.GiaTri >= 0
            AND (
                km.LoaiKM = N'Tiền mặt'
                OR (km.LoaiKM = N'Phần trăm' AND km.GiaTri <= 100)
            )
        WHERE ct.MaHD = @MaHD
    ), 0)
    WHERE MaHD = @MaHD;
END;

GO
/****** Object:  StoredProcedure [dbo].[CTHD_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CTHD_Sua]
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
            THROW 50001, N'H�a don kh�ng t?n t?i.', 1;
        END

        IF @TrangThaiOrder = N'DaHuy' OR @TrangThaiThanhToan = N'DaThanhToan' OR @TrangThaiThanhToan = N'ChoThanhToanQR'
        BEGIN
            THROW 50006, N'H�a don d� thanh to�n ho?c d� h?y, kh�ng th? ch?nh s?a chi ti?t m�n.', 1;
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
/****** Object:  StoredProcedure [dbo].[CTHD_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CTHD_Them] @MaHD varchar(9), @MaSP varchar(5), @SoLuong int, @DonGia money, @MaKM varchar(5)=NULL AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        DECLARE @TrangThaiOrder nvarchar(30), @TrangThaiThanhToan nvarchar(30);
        SELECT @TrangThaiOrder = TrangThaiOrder, @TrangThaiThanhToan = TrangThaiThanhToan FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK) WHERE MaHD = @MaHD;
        IF @TrangThaiOrder IS NULL THROW 50001, N'Hóa đơn không tồn tại.', 1;
        IF @TrangThaiOrder = N'DaHuy' OR @TrangThaiThanhToan = N'DaThanhToan' OR @TrangThaiThanhToan = N'ChoThanhToanQR'
            THROW 50006, N'Hóa đơn đã thanh toán hoặc đã hủy, không thể thêm chi tiết món.', 1;
        IF EXISTS (SELECT 1 FROM CTHD WITH (UPDLOCK, HOLDLOCK) WHERE MaHD = @MaHD AND MaSP = @MaSP)
            UPDATE CTHD SET SoLuong = SoLuong + @SoLuong, DonGia = @DonGia, MaKM = @MaKM WHERE MaHD = @MaHD AND MaSP = @MaSP;
        ELSE
            INSERT INTO CTHD(MaHD,MaSP,SoLuong,DonGia,MaKM) VALUES(@MaHD,@MaSP,@SoLuong,@DonGia,@MaKM);
        EXEC HoaDon_CapNhatTongTien @MaHD;
        COMMIT TRAN;
    END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRAN; THROW; END CATCH
END;



GO
/****** Object:  StoredProcedure [dbo].[CTHD_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CTHD_Xoa] @MaHD varchar(9), @MaSP varchar(5) AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        DECLARE @TrangThaiOrder nvarchar(30), @TrangThaiThanhToan nvarchar(30);
        SELECT @TrangThaiOrder = TrangThaiOrder, @TrangThaiThanhToan = TrangThaiThanhToan FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK) WHERE MaHD = @MaHD;
        IF @TrangThaiOrder IS NULL THROW 50001, N'Hóa đơn không tồn tại.', 1;
        IF @TrangThaiOrder = N'DaHuy' OR @TrangThaiThanhToan = N'DaThanhToan' OR @TrangThaiThanhToan = N'ChoThanhToanQR'
            THROW 50006, N'Hóa đơn đã thanh toán hoặc đã hủy, không thể xóa chi tiết món.', 1;
        DELETE FROM CTHD WHERE MaHD=@MaHD AND MaSP=@MaSP;
        EXEC HoaDon_CapNhatTongTien @MaHD;
        COMMIT TRAN;
    END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRAN; THROW; END CATCH
END;



GO
/****** Object:  StoredProcedure [dbo].[CTPN_DSTheoMaPN]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CTPN_DSTheoMaPN] @MaPN varchar(9) AS
BEGIN SET NOCOUNT ON; SELECT ct.MaPN,ct.MaNL,nl.TenNL,nl.DonViTinh,ct.SoLuong,ct.DonGia,ct.ThanhTien FROM CTPN ct INNER JOIN NguyenLieu nl ON ct.MaNL=nl.MaNL WHERE ct.MaPN=@MaPN ORDER BY ct.MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[PhieuNhap_CapNhatTongTien]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Phiếu nhập ---------- */
CREATE   PROC [dbo].[PhieuNhap_CapNhatTongTien] @MaPN varchar(9) AS
BEGIN SET NOCOUNT ON; UPDATE PhieuNhap SET TongTien=ISNULL((SELECT SUM(ThanhTien) FROM CTPN WHERE MaPN=@MaPN),0) WHERE MaPN=@MaPN; END;



GO
/****** Object:  StoredProcedure [dbo].[CTPN_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CTPN_Sua] @MaPN varchar(9), @MaNL varchar(5), @SoLuong int, @DonGia money AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        DECLARE @SoLuongCu int;
        SELECT @SoLuongCu=SoLuong FROM CTPN WHERE MaPN=@MaPN AND MaNL=@MaNL;
        IF @SoLuongCu IS NULL BEGIN RAISERROR(N'Chi tiết phiếu nhập không tồn tại.',16,1); ROLLBACK; RETURN; END
        IF @SoLuong < @SoLuongCu AND (SELECT ISNULL(SoLuongTon,0) FROM NguyenLieu WHERE MaNL=@MaNL) < (@SoLuongCu - @SoLuong)
        BEGIN RAISERROR(N'Không thể sửa giảm vì nguyên liệu đã được sử dụng, kho không đủ để hoàn tác.',16,1); ROLLBACK; RETURN; END
        UPDATE CTPN SET SoLuong=@SoLuong,DonGia=@DonGia WHERE MaPN=@MaPN AND MaNL=@MaNL;
        UPDATE NguyenLieu SET SoLuongTon=ISNULL(SoLuongTon,0)-@SoLuongCu+@SoLuong, GiaNhap=@DonGia, NgayCapNhat=GETDATE() WHERE MaNL=@MaNL;
        EXEC PhieuNhap_CapNhatTongTien @MaPN;
        COMMIT;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;



GO
/****** Object:  StoredProcedure [dbo].[CTPN_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CTPN_Them] @MaPN varchar(9), @MaNL varchar(5), @SoLuong int, @DonGia money AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO CTPN(MaPN,MaNL,SoLuong,DonGia) VALUES(@MaPN,@MaNL,@SoLuong,@DonGia);
    UPDATE NguyenLieu SET SoLuongTon=ISNULL(SoLuongTon,0)+@SoLuong, GiaNhap=@DonGia, NgayCapNhat=GETDATE() WHERE MaNL=@MaNL;
    EXEC PhieuNhap_CapNhatTongTien @MaPN;
END;



GO
/****** Object:  StoredProcedure [dbo].[CTPN_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[CTPN_Xoa] @MaPN varchar(9), @MaNL varchar(5) AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        DECLARE @SoLuongCu int;
        SELECT @SoLuongCu=SoLuong FROM CTPN WHERE MaPN=@MaPN AND MaNL=@MaNL;
        IF @SoLuongCu IS NULL BEGIN RAISERROR(N'Chi tiết phiếu nhập không tồn tại.',16,1); ROLLBACK; RETURN; END
        IF (SELECT ISNULL(SoLuongTon,0) FROM NguyenLieu WHERE MaNL=@MaNL) < @SoLuongCu
        BEGIN RAISERROR(N'Không thể xóa vì nguyên liệu đã được sử dụng, kho không đủ để hoàn tác.',16,1); ROLLBACK; RETURN; END
        DELETE FROM CTPN WHERE MaPN=@MaPN AND MaNL=@MaNL;
        UPDATE NguyenLieu SET SoLuongTon=ISNULL(SoLuongTon,0)-@SoLuongCu, NgayCapNhat=GETDATE() WHERE MaNL=@MaNL;
        EXEC PhieuNhap_CapNhatTongTien @MaPN;
        COMMIT;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;



GO
/****** Object:  StoredProcedure [dbo].[HoaDon_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[HoaDon_CT] @MaHD varchar(9) AS
BEGIN SET NOCOUNT ON; SELECT h.MaHD,h.NgayLap,h.MaNV,nv.TenNV,h.MaKH,kh.TenKH,h.MaBan,b.TenBan,h.TongTien,h.GhiChu FROM HoaDon h INNER JOIN NhanVien nv ON h.MaNV=nv.MaNV LEFT JOIN KhachHang kh ON h.MaKH=kh.MaKH INNER JOIN Ban b ON h.MaBan=b.MaBan WHERE h.MaHD=@MaHD; END;



GO
/****** Object:  StoredProcedure [dbo].[HoaDon_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[HoaDon_DS] AS
BEGIN SET NOCOUNT ON; SELECT h.MaHD,h.NgayLap,h.MaNV,nv.TenNV,h.MaKH,kh.TenKH,h.MaBan,b.TenBan,h.TongTien,h.GhiChu FROM HoaDon h INNER JOIN NhanVien nv ON h.MaNV=nv.MaNV LEFT JOIN KhachHang kh ON h.MaKH=kh.MaKH INNER JOIN Ban b ON h.MaBan=b.MaBan ORDER BY h.NgayLap DESC,h.MaHD DESC; END;



GO
/****** Object:  StoredProcedure [dbo].[HoaDon_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[HoaDon_Sua] @MaHD varchar(9), @NgayLap date, @MaNV varchar(5), @MaKH varchar(5)=NULL, @GhiChu nvarchar(100)=NULL, @MaBan varchar(5) AS
BEGIN SET NOCOUNT ON; UPDATE HoaDon SET NgayLap=@NgayLap,MaNV=@MaNV,MaKH=@MaKH,GhiChu=@GhiChu,MaBan=@MaBan WHERE MaHD=@MaHD; END;



GO
/****** Object:  StoredProcedure [dbo].[HoaDon_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[HoaDon_Them] @MaHD varchar(9)=NULL, @NgayLap date, @MaNV varchar(5), @MaKH varchar(5)=NULL, @GhiChu nvarchar(100)=NULL, @MaBan varchar(5), @ClientRequestId varchar(36)=NULL AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        IF @ClientRequestId IS NOT NULL AND LTRIM(RTRIM(@ClientRequestId)) <> ''
        BEGIN
            DECLARE @ExistingMaHD varchar(9);
            SELECT @ExistingMaHD = MaHD FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK) WHERE ClientRequestId = @ClientRequestId;
            IF @ExistingMaHD IS NOT NULL
            BEGIN
                COMMIT;
                RETURN;
            END
        END
        IF @MaHD IS NULL OR LTRIM(RTRIM(@MaHD))=''
        BEGIN
            DECLARE @Prefix varchar(6)=FORMAT(@NgayLap,'yyMMdd');
            DECLARE @MaxNumber int;
            SELECT @MaxNumber=MAX(TRY_CONVERT(int,RIGHT(MaHD,3))) FROM HoaDon WITH(UPDLOCK,HOLDLOCK) WHERE MaHD LIKE @Prefix + '%';
            SET @MaHD=dbo.fn_NextCode(@Prefix,@MaxNumber,3);
        END
        INSERT INTO HoaDon(MaHD,NgayLap,MaNV,MaKH,TongTien,GhiChu,MaBan,ClientRequestId) VALUES(@MaHD,@NgayLap,@MaNV,@MaKH,0,@GhiChu,@MaBan,@ClientRequestId);
        COMMIT;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;



GO
/****** Object:  StoredProcedure [dbo].[HoaDon_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   6. HoaDon_Xoa: không crash FK ThanhToan, không xóa hóa đơn đã thanh toán
   ========================================================= */
CREATE   PROC [dbo].[HoaDon_Xoa]
    @MaHD varchar(9)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF NOT EXISTS (SELECT 1 FROM dbo.HoaDon WHERE MaHD = @MaHD)
        BEGIN
            RAISERROR(N'Hóa đơn không tồn tại.', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END;

        IF EXISTS (
            SELECT 1
            FROM dbo.HoaDon
            WHERE MaHD = @MaHD
              AND TrangThaiThanhToan = N'DaThanhToan'
        )
        BEGIN
            RAISERROR(N'Hóa đơn đã thanh toán, không được xóa vật lý. Hãy lưu lịch sử hoặc xử lý hủy theo nghiệp vụ.', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END;

        DELETE FROM dbo.ThanhToan WHERE MaHD = @MaHD;
        DELETE FROM dbo.CTHD WHERE MaHD = @MaHD;
        DELETE FROM dbo.HoaDon WHERE MaHD = @MaHD;

        COMMIT TRAN;

        SELECT CAST(1 AS bit) AS Success,
               N'Xóa hóa đơn chưa thanh toán thành công.' AS Message,
               @MaHD AS MaHD;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END;

GO
/****** Object:  StoredProcedure [dbo].[KhachHang_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[KhachHang_CT] @MaKH varchar(5) AS BEGIN SET NOCOUNT ON; SELECT * FROM KhachHang WHERE MaKH=@MaKH; END;



GO
/****** Object:  StoredProcedure [dbo].[KhachHang_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Khách hàng ---------- */
CREATE   PROC [dbo].[KhachHang_DS] AS BEGIN SET NOCOUNT ON; SELECT * FROM KhachHang ORDER BY TenKH; END;



GO
/****** Object:  StoredProcedure [dbo].[KhachHang_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[KhachHang_Sua] @MaKH varchar(5), @TenKH nvarchar(50), @SDT varchar(10), @DiemTichLuy int=0 AS
BEGIN
    SET NOCOUNT ON;

    SET @TenKH = LTRIM(RTRIM(ISNULL(@TenKH, N'')));
    SET @SDT = LTRIM(RTRIM(ISNULL(@SDT, '')));

    IF @TenKH = N''
    BEGIN
        RAISERROR(N'Tên khách hàng không được để trống.', 16, 1);
        RETURN;
    END

    IF @SDT = ''
    BEGIN
        RAISERROR(N'Số điện thoại không được để trống.', 16, 1);
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM dbo.KhachHang
        WHERE LTRIM(RTRIM(TenKH)) = @TenKH
          AND MaKH <> @MaKH
    )
    BEGIN
        RAISERROR(N'Tên khách hàng đã tồn tại.', 16, 1);
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM dbo.KhachHang
        WHERE LTRIM(RTRIM(SDT)) = @SDT
          AND MaKH <> @MaKH
    )
    BEGIN
        RAISERROR(N'Số điện thoại khách hàng đã tồn tại.', 16, 1);
        RETURN;
    END

    UPDATE KhachHang SET TenKH=@TenKH, SDT=@SDT, DiemTichLuy=ISNULL(@DiemTichLuy,0) WHERE MaKH=@MaKH;
END;



GO
/****** Object:  StoredProcedure [dbo].[KhachHang_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[KhachHang_Them] @MaKH varchar(5)=NULL, @TenKH nvarchar(50), @SDT varchar(10), @DiemTichLuy int=0 AS
BEGIN
    SET NOCOUNT ON;

    SET @TenKH = LTRIM(RTRIM(ISNULL(@TenKH, N'')));
    SET @SDT = LTRIM(RTRIM(ISNULL(@SDT, '')));

    IF @TenKH = N''
    BEGIN
        RAISERROR(N'Tên khách hàng không được để trống.', 16, 1);
        RETURN;
    END

    IF @SDT = ''
    BEGIN
        RAISERROR(N'Số điện thoại không được để trống.', 16, 1);
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM dbo.KhachHang
        WHERE LTRIM(RTRIM(TenKH)) = @TenKH
    )
    BEGIN
        RAISERROR(N'Tên khách hàng đã tồn tại.', 16, 1);
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM dbo.KhachHang
        WHERE LTRIM(RTRIM(SDT)) = @SDT
    )
    BEGIN
        RAISERROR(N'Số điện thoại khách hàng đã tồn tại.', 16, 1);
        RETURN;
    END

    IF @MaKH IS NULL OR LTRIM(RTRIM(@MaKH))=''
    BEGIN
        DECLARE @MaxNumber int;
        SELECT @MaxNumber=MAX(TRY_CONVERT(int,SUBSTRING(MaKH,3,20))) FROM KhachHang WITH(UPDLOCK,HOLDLOCK) WHERE MaKH LIKE 'KH%' AND TRY_CONVERT(int,SUBSTRING(MaKH,3,20)) IS NOT NULL;
        SET @MaKH=dbo.fn_NextCode('KH',@MaxNumber,2);
    END
    INSERT INTO KhachHang(MaKH,TenKH,SDT,DiemTichLuy) VALUES(@MaKH,@TenKH,@SDT,ISNULL(@DiemTichLuy,0));
END;



GO
/****** Object:  StoredProcedure [dbo].[KhachHang_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[KhachHang_Xoa] @MaKH varchar(5) AS BEGIN SET NOCOUNT ON; DELETE FROM KhachHang WHERE MaKH=@MaKH; END;



GO
/****** Object:  StoredProcedure [dbo].[KhuyenMai_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[KhuyenMai_CT] @MaKM varchar(5) AS BEGIN SET NOCOUNT ON; SELECT * FROM KhuyenMai WHERE MaKM=@MaKM; END;



GO
/****** Object:  StoredProcedure [dbo].[KhuyenMai_DangApDung]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[KhuyenMai_DangApDung] @Ngay date AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM KhuyenMai
    WHERE @Ngay BETWEEN NgayBatDau AND NgayKetThuc
      AND TrangThai=N'Đang áp dụng'
      AND GiaTri >= 0
      AND (LoaiKM = N'Tiền mặt' OR (LoaiKM = N'Phần trăm' AND GiaTri <= 100));
END;


GO
/****** Object:  StoredProcedure [dbo].[KhuyenMai_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Khuyến mãi ---------- */
CREATE   PROC [dbo].[KhuyenMai_DS] AS BEGIN SET NOCOUNT ON; SELECT * FROM KhuyenMai ORDER BY NgayBatDau DESC; END;



GO
/****** Object:  StoredProcedure [dbo].[KhuyenMai_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[KhuyenMai_Sua] @MaKM varchar(5), @TenKM nvarchar(255), @LoaiKM nvarchar(50), @GiaTri decimal(18,2), @NgayBatDau date, @NgayKetThuc date, @DieuKienApDung nvarchar(500), @TrangThai nvarchar(50) AS
BEGIN SET NOCOUNT ON; UPDATE KhuyenMai SET TenKM=@TenKM,LoaiKM=@LoaiKM,GiaTri=@GiaTri,NgayBatDau=@NgayBatDau,NgayKetThuc=@NgayKetThuc,DieuKienApDung=@DieuKienApDung,TrangThai=@TrangThai WHERE MaKM=@MaKM; END;



GO
/****** Object:  StoredProcedure [dbo].[KhuyenMai_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[KhuyenMai_Them] @MaKM varchar(5)=NULL, @TenKM nvarchar(255), @LoaiKM nvarchar(50), @GiaTri decimal(18,2), @NgayBatDau date, @NgayKetThuc date, @DieuKienApDung nvarchar(500), @TrangThai nvarchar(50) AS
BEGIN
    SET NOCOUNT ON;
    IF @MaKM IS NULL OR LTRIM(RTRIM(@MaKM))=''
    BEGIN
        DECLARE @MaxNumber int;
        SELECT @MaxNumber=MAX(TRY_CONVERT(int,SUBSTRING(MaKM,3,20))) FROM KhuyenMai WITH(UPDLOCK,HOLDLOCK) WHERE MaKM LIKE 'KM%' AND TRY_CONVERT(int,SUBSTRING(MaKM,3,20)) IS NOT NULL;
        SET @MaKM=dbo.fn_NextCode('KM',@MaxNumber,2);
    END
    INSERT INTO KhuyenMai(MaKM,TenKM,LoaiKM,GiaTri,NgayBatDau,NgayKetThuc,DieuKienApDung,TrangThai) VALUES(@MaKM,@TenKM,@LoaiKM,@GiaTri,@NgayBatDau,@NgayKetThuc,@DieuKienApDung,@TrangThai);
END;



GO
/****** Object:  StoredProcedure [dbo].[KhuyenMai_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[KhuyenMai_Xoa] @MaKM varchar(5) AS BEGIN SET NOCOUNT ON; DELETE FROM KhuyenMai WHERE MaKM=@MaKM; END;



GO
/****** Object:  StoredProcedure [dbo].[LoaiSP_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[LoaiSP_CT] @MaLoaiSP varchar(5) AS BEGIN SET NOCOUNT ON; SELECT * FROM LoaiSP WHERE MaLoaiSP=@MaLoaiSP; END;



GO
/****** Object:  StoredProcedure [dbo].[LoaiSP_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Loại sản phẩm ---------- */
CREATE   PROC [dbo].[LoaiSP_DS] AS BEGIN SET NOCOUNT ON; SELECT * FROM LoaiSP ORDER BY MaLoaiSP; END;



GO
/****** Object:  StoredProcedure [dbo].[LoaiSP_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[LoaiSP_Sua] @MaLoaiSP varchar(5), @TenLoaiSP nvarchar(30) AS BEGIN SET NOCOUNT ON; UPDATE LoaiSP SET TenLoaiSP=@TenLoaiSP WHERE MaLoaiSP=@MaLoaiSP; END;



GO
/****** Object:  StoredProcedure [dbo].[LoaiSP_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[LoaiSP_Them] @MaLoaiSP varchar(5)=NULL, @TenLoaiSP nvarchar(30) AS
BEGIN
    SET NOCOUNT ON;
    IF @MaLoaiSP IS NULL OR LTRIM(RTRIM(@MaLoaiSP))=''
    BEGIN
        DECLARE @MaxNumber int;
        SELECT @MaxNumber=MAX(TRY_CONVERT(int,SUBSTRING(MaLoaiSP,2,20))) FROM LoaiSP WITH(UPDLOCK,HOLDLOCK) WHERE MaLoaiSP LIKE 'L%' AND TRY_CONVERT(int,SUBSTRING(MaLoaiSP,2,20)) IS NOT NULL;
        SET @MaLoaiSP=dbo.fn_NextCode('L',@MaxNumber,2);
    END
    INSERT INTO LoaiSP(MaLoaiSP,TenLoaiSP) VALUES(@MaLoaiSP,@TenLoaiSP);
END;



GO
/****** Object:  StoredProcedure [dbo].[LoaiSP_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[LoaiSP_Xoa] @MaLoaiSP varchar(5) AS BEGIN SET NOCOUNT ON; DELETE FROM LoaiSP WHERE MaLoaiSP=@MaLoaiSP; END;



GO
/****** Object:  StoredProcedure [dbo].[Luong_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[Luong_CT] @MaLuong int AS BEGIN SET NOCOUNT ON; SELECT * FROM Luong WHERE MaLuong=@MaLuong; END;



GO
/****** Object:  StoredProcedure [dbo].[Luong_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Lương ---------- */
CREATE   PROC [dbo].[Luong_DS] AS BEGIN SET NOCOUNT ON; SELECT * FROM Luong ORDER BY Nam DESC, Thang DESC, MaLuong; END;



GO
/****** Object:  StoredProcedure [dbo].[Luong_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[Luong_Sua] @MaLuong int, @Thang int, @Nam int, @LuongCoBan money, @PhuCap money=0, @Thuong money=0, @Phat money=0 AS
BEGIN SET NOCOUNT ON; UPDATE Luong SET Thang=@Thang,Nam=@Nam,LuongCoBan=@LuongCoBan,PhuCap=@PhuCap,Thuong=@Thuong,Phat=@Phat WHERE MaLuong=@MaLuong; END;



GO
/****** Object:  StoredProcedure [dbo].[Luong_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[Luong_Them] @MaLuong int=NULL, @Thang int, @Nam int, @LuongCoBan money, @PhuCap money=0, @Thuong money=0, @Phat money=0 AS
BEGIN
    SET NOCOUNT ON;
    IF @MaLuong IS NULL SELECT @MaLuong=ISNULL(MAX(MaLuong),0)+1 FROM Luong WITH(UPDLOCK,HOLDLOCK);
    INSERT INTO Luong(MaLuong,Thang,Nam,LuongCoBan,PhuCap,Thuong,Phat) VALUES(@MaLuong,@Thang,@Nam,@LuongCoBan,@PhuCap,@Thuong,@Phat);
END;



GO
/****** Object:  StoredProcedure [dbo].[Luong_TheoThangNam]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[Luong_TheoThangNam] @Thang int, @Nam int AS BEGIN SET NOCOUNT ON; SELECT * FROM Luong WHERE Thang=@Thang AND Nam=@Nam; END;



GO
/****** Object:  StoredProcedure [dbo].[Luong_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[Luong_Xoa] @MaLuong int AS BEGIN SET NOCOUNT ON; DELETE FROM Luong WHERE MaLuong=@MaLuong; END;



GO
/****** Object:  StoredProcedure [dbo].[NguyenLieu_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[NguyenLieu_CT] @MaNL varchar(5) AS BEGIN SET NOCOUNT ON; SELECT * FROM NguyenLieu WHERE MaNL=@MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[NguyenLieu_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Nguyên liệu ---------- */
CREATE   PROC [dbo].[NguyenLieu_DS] AS BEGIN SET NOCOUNT ON; SELECT * FROM NguyenLieu ORDER BY MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[NguyenLieu_SapHet]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[NguyenLieu_SapHet] AS BEGIN SET NOCOUNT ON; SELECT * FROM NguyenLieu WHERE ISNULL(SoLuongTon,0)<=MucTonToiThieu ORDER BY MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[NguyenLieu_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[NguyenLieu_Sua] @MaNL varchar(5), @TenNL nvarchar(50), @DonViTinh nvarchar(20), @SoLuongTon int=0, @GiaNhap money, @NgayCapNhat date, @MucTonToiThieu int AS
BEGIN SET NOCOUNT ON; UPDATE NguyenLieu SET TenNL=@TenNL,DonViTinh=@DonViTinh,SoLuongTon=@SoLuongTon,GiaNhap=@GiaNhap,NgayCapNhat=@NgayCapNhat,MucTonToiThieu=@MucTonToiThieu WHERE MaNL=@MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[NguyenLieu_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[NguyenLieu_Them] @MaNL varchar(5)=NULL, @TenNL nvarchar(50), @DonViTinh nvarchar(20), @SoLuongTon int=0, @GiaNhap money, @NgayCapNhat date, @MucTonToiThieu int AS
BEGIN
    SET NOCOUNT ON;
    IF @MaNL IS NULL OR LTRIM(RTRIM(@MaNL))=''
    BEGIN
        DECLARE @MaxNumber int;
        SELECT @MaxNumber=MAX(TRY_CONVERT(int,SUBSTRING(MaNL,3,20))) FROM NguyenLieu WITH(UPDLOCK,HOLDLOCK) WHERE MaNL LIKE 'NL%' AND TRY_CONVERT(int,SUBSTRING(MaNL,3,20)) IS NOT NULL;
        SET @MaNL=dbo.fn_NextCode('NL',@MaxNumber,2);
    END
    INSERT INTO NguyenLieu(MaNL,TenNL,DonViTinh,SoLuongTon,GiaNhap,NgayCapNhat,MucTonToiThieu) VALUES(@MaNL,@TenNL,@DonViTinh,@SoLuongTon,@GiaNhap,@NgayCapNhat,@MucTonToiThieu);
END;



GO
/****** Object:  StoredProcedure [dbo].[NguyenLieu_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[NguyenLieu_Xoa] @MaNL varchar(5) AS BEGIN SET NOCOUNT ON; DELETE FROM NguyenLieu WHERE MaNL=@MaNL; END;



GO
/****** Object:  StoredProcedure [dbo].[NhanVien_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[NhanVien_CT] @MaNV varchar(5) AS
BEGIN
    SET NOCOUNT ON;
    SELECT nv.*, l.Thang, l.Nam, l.LuongCoBan, l.PhuCap, l.Thuong, l.Phat, l.TongLuong
    FROM NhanVien nv INNER JOIN Luong l ON nv.MaLuong=l.MaLuong
    WHERE nv.MaNV=@MaNV;
END;



GO
/****** Object:  StoredProcedure [dbo].[NhanVien_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Nhân viên ---------- */
CREATE   PROC [dbo].[NhanVien_DS] AS
BEGIN
    SET NOCOUNT ON;
    SELECT nv.*, l.Thang, l.Nam, l.LuongCoBan, l.PhuCap, l.Thuong, l.Phat, l.TongLuong
    FROM NhanVien nv INNER JOIN Luong l ON nv.MaLuong=l.MaLuong
    ORDER BY nv.MaNV;
END;



GO
/****** Object:  StoredProcedure [dbo].[NhanVien_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[NhanVien_Sua] @MaNV varchar(5), @TenNV nvarchar(50), @GioiTinh nvarchar(5), @NgaySinh date=NULL, @SDT varchar(10), @ChucVu nvarchar(20), @MaLuong int, @LuongTheoGio money=20000 AS
BEGIN SET NOCOUNT ON; UPDATE NhanVien SET TenNV=@TenNV,GioiTinh=@GioiTinh,NgaySinh=@NgaySinh,SDT=@SDT,ChucVu=@ChucVu,MaLuong=@MaLuong,LuongTheoGio=@LuongTheoGio WHERE MaNV=@MaNV; END;



GO
/****** Object:  StoredProcedure [dbo].[NhanVien_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[NhanVien_Them] @MaNV varchar(5)=NULL, @TenNV nvarchar(50), @GioiTinh nvarchar(5), @NgaySinh date=NULL, @SDT varchar(10), @ChucVu nvarchar(20), @MaLuong int, @LuongTheoGio money=20000 AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        IF @LuongTheoGio < 0 OR @LuongTheoGio > 1000000 BEGIN RAISERROR(N'Lương theo giờ không hợp lệ.',16,1); ROLLBACK; RETURN; END
        IF @MaNV IS NULL OR LTRIM(RTRIM(@MaNV))=''
        BEGIN
            DECLARE @MaxNumber int;
            SELECT @MaxNumber=MAX(TRY_CONVERT(int,SUBSTRING(MaNV,3,20))) FROM NhanVien WITH(UPDLOCK,HOLDLOCK) WHERE MaNV LIKE 'NV%' AND TRY_CONVERT(int,SUBSTRING(MaNV,3,20)) IS NOT NULL;
            SET @MaNV=dbo.fn_NextCode('NV',@MaxNumber,2);
        END
        INSERT INTO NhanVien(MaNV,TenNV,GioiTinh,NgaySinh,SDT,ChucVu,MaLuong,LuongTheoGio) VALUES(@MaNV,@TenNV,@GioiTinh,@NgaySinh,@SDT,@ChucVu,@MaLuong,@LuongTheoGio);
        COMMIT;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;



GO
/****** Object:  StoredProcedure [dbo].[NhanVien_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[NhanVien_Xoa] @MaNV varchar(5) AS BEGIN SET NOCOUNT ON; DELETE FROM NhanVien WHERE MaNV=@MaNV; END;



GO
/****** Object:  StoredProcedure [dbo].[PhieuNhap_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[PhieuNhap_CT] @MaPN varchar(9) AS
BEGIN SET NOCOUNT ON; SELECT pn.MaPN,pn.NgayNhap,pn.MaNV,nv.TenNV,pn.TongTien FROM PhieuNhap pn INNER JOIN NhanVien nv ON pn.MaNV=nv.MaNV WHERE pn.MaPN=@MaPN; END;



GO
/****** Object:  StoredProcedure [dbo].[PhieuNhap_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[PhieuNhap_DS] AS
BEGIN SET NOCOUNT ON; SELECT pn.MaPN,pn.NgayNhap,pn.MaNV,nv.TenNV,pn.TongTien FROM PhieuNhap pn INNER JOIN NhanVien nv ON pn.MaNV=nv.MaNV ORDER BY pn.NgayNhap DESC,pn.MaPN DESC; END;



GO
/****** Object:  StoredProcedure [dbo].[PhieuNhap_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[PhieuNhap_Sua] @MaPN varchar(9), @NgayNhap date, @MaNV varchar(5) AS BEGIN SET NOCOUNT ON; UPDATE PhieuNhap SET NgayNhap=@NgayNhap,MaNV=@MaNV WHERE MaPN=@MaPN; END;



GO
/****** Object:  StoredProcedure [dbo].[PhieuNhap_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[PhieuNhap_Them] @MaPN varchar(9)=NULL, @NgayNhap date, @MaNV varchar(5) AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        IF @MaPN IS NULL OR LTRIM(RTRIM(@MaPN))=''
        BEGIN
            DECLARE @Prefix varchar(6)='PN'+FORMAT(@NgayNhap,'yyMM');
            DECLARE @MaxNumber int;
            SELECT @MaxNumber=MAX(TRY_CONVERT(int,RIGHT(MaPN,3))) FROM PhieuNhap WITH(UPDLOCK,HOLDLOCK) WHERE MaPN LIKE @Prefix + '%';
            SET @MaPN=dbo.fn_NextCode(@Prefix,@MaxNumber,3);
        END
        INSERT INTO PhieuNhap(MaPN,NgayNhap,MaNV,TongTien) VALUES(@MaPN,@NgayNhap,@MaNV,0);
        COMMIT;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;



GO
/****** Object:  StoredProcedure [dbo].[PhieuNhap_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[PhieuNhap_Xoa] @MaPN varchar(9) AS BEGIN SET NOCOUNT ON; DELETE FROM CTPN WHERE MaPN=@MaPN; DELETE FROM PhieuNhap WHERE MaPN=@MaPN; END;



GO
/****** Object:  StoredProcedure [dbo].[SanPham_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[SanPham_CT] @MaSP varchar(5) AS
BEGIN
    SET NOCOUNT ON;
    SELECT sp.MaSP, sp.TenSP, sp.DonGia, sp.MaLoaiSP, l.TenLoaiSP, sp.TrangThai
    FROM SanPham sp INNER JOIN LoaiSP l ON sp.MaLoaiSP=l.MaLoaiSP
    WHERE sp.MaSP=@MaSP;
END;



GO
/****** Object:  StoredProcedure [dbo].[SanPham_DoiTrangThai]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[SanPham_DoiTrangThai] @MaSP varchar(5), @TrangThai nvarchar(30) AS
BEGIN
    SET NOCOUNT ON;
    IF @TrangThai NOT IN (N'Đang bán',N'Tạm ngưng',N'Ngừng bán') BEGIN RAISERROR(N'Trạng thái sản phẩm không hợp lệ.',16,1); RETURN; END
    UPDATE SanPham SET TrangThai=@TrangThai WHERE MaSP=@MaSP;
END;



GO
/****** Object:  StoredProcedure [dbo].[SanPham_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Sản phẩm ---------- */
CREATE   PROC [dbo].[SanPham_DS] AS
BEGIN
    SET NOCOUNT ON;
    SELECT sp.MaSP, sp.TenSP, sp.DonGia, sp.MaLoaiSP, l.TenLoaiSP, sp.TrangThai
    FROM SanPham sp INNER JOIN LoaiSP l ON sp.MaLoaiSP=l.MaLoaiSP
    ORDER BY sp.MaSP;
END;



GO
/****** Object:  StoredProcedure [dbo].[SanPham_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[SanPham_Sua] @MaSP varchar(5), @TenSP nvarchar(50), @DonGia money, @MaLoaiSP varchar(5), @TrangThai nvarchar(30) AS
BEGIN
    SET NOCOUNT ON;
    IF @DonGia <= 0 OR @DonGia > 10000000 BEGIN RAISERROR(N'Đơn giá phải lớn hơn 0 và không vượt quá 10.000.000đ.',16,1); RETURN; END
    IF @TrangThai NOT IN (N'Đang bán',N'Tạm ngưng',N'Ngừng bán') BEGIN RAISERROR(N'Trạng thái sản phẩm không hợp lệ.',16,1); RETURN; END
    UPDATE SanPham SET TenSP=@TenSP, DonGia=@DonGia, MaLoaiSP=@MaLoaiSP, TrangThai=@TrangThai WHERE MaSP=@MaSP;
END;



GO
/****** Object:  StoredProcedure [dbo].[SanPham_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[SanPham_Them] @MaSP varchar(5)=NULL, @TenSP nvarchar(50), @DonGia money, @MaLoaiSP varchar(5), @TrangThai nvarchar(30)=N'Tạm ngưng' AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;
        IF @DonGia <= 0 OR @DonGia > 10000000 BEGIN RAISERROR(N'Đơn giá phải lớn hơn 0 và không vượt quá 10.000.000đ.',16,1); ROLLBACK; RETURN; END
        IF @TrangThai NOT IN (N'Đang bán',N'Tạm ngưng',N'Ngừng bán') SET @TrangThai=N'Tạm ngưng';
        IF @MaSP IS NULL OR LTRIM(RTRIM(@MaSP))=''
        BEGIN
            DECLARE @MaxNumber int;
            SELECT @MaxNumber=MAX(TRY_CONVERT(int,SUBSTRING(MaSP,3,20))) FROM SanPham WITH(UPDLOCK,HOLDLOCK) WHERE MaSP LIKE 'SP%' AND TRY_CONVERT(int,SUBSTRING(MaSP,3,20)) IS NOT NULL;
            SET @MaSP=dbo.fn_NextCode('SP',@MaxNumber,2);
        END
        INSERT INTO SanPham(MaSP,TenSP,DonGia,MaLoaiSP,TrangThai) VALUES(@MaSP,@TenSP,@DonGia,@MaLoaiSP,@TrangThai);
        COMMIT;
    END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK; THROW; END CATCH
END;



GO
/****** Object:  StoredProcedure [dbo].[SanPham_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[SanPham_Xoa] @MaSP varchar(5) AS BEGIN SET NOCOUNT ON; DELETE FROM SanPham WHERE MaSP=@MaSP; END;



GO
/****** Object:  StoredProcedure [dbo].[TaiKhoan_CT]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[TaiKhoan_CT] @MaTK varchar(10) AS
BEGIN SET NOCOUNT ON; SELECT tk.MaTK,tk.TenDangNhap,tk.MatKhau,tk.MaNV,nv.TenNV,tk.VaiTro FROM TaiKhoan tk INNER JOIN NhanVien nv ON tk.MaNV=nv.MaNV WHERE tk.MaTK=@MaTK; END;



GO
/****** Object:  StoredProcedure [dbo].[TaiKhoan_DangNhap]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[TaiKhoan_DangNhap] @TenDangNhap varchar(50), @MatKhau varchar(255) AS
BEGIN SET NOCOUNT ON; SELECT tk.MaTK,tk.TenDangNhap,tk.MaNV,nv.TenNV,tk.VaiTro FROM TaiKhoan tk INNER JOIN NhanVien nv ON tk.MaNV=nv.MaNV WHERE tk.TenDangNhap=@TenDangNhap AND tk.MatKhau=@MatKhau; END;



GO
/****** Object:  StoredProcedure [dbo].[TaiKhoan_DS]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ---------- Tài khoản ---------- */
CREATE   PROC [dbo].[TaiKhoan_DS] AS
BEGIN SET NOCOUNT ON; SELECT tk.MaTK,tk.TenDangNhap,tk.MaNV,nv.TenNV,tk.VaiTro FROM TaiKhoan tk INNER JOIN NhanVien nv ON tk.MaNV=nv.MaNV ORDER BY tk.MaTK; END;



GO
/****** Object:  StoredProcedure [dbo].[TaiKhoan_Sua]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[TaiKhoan_Sua] @MaTK varchar(10), @TenDangNhap varchar(50), @MatKhau varchar(255), @MaNV varchar(5), @VaiTro nvarchar(50) AS
BEGIN SET NOCOUNT ON; UPDATE TaiKhoan SET TenDangNhap=@TenDangNhap,MatKhau=@MatKhau,MaNV=@MaNV,VaiTro=@VaiTro WHERE MaTK=@MaTK; END;



GO
/****** Object:  StoredProcedure [dbo].[TaiKhoan_Them]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[TaiKhoan_Them] @MaTK varchar(10)=NULL, @TenDangNhap varchar(50), @MatKhau varchar(255), @MaNV varchar(5), @VaiTro nvarchar(50) AS
BEGIN
    SET NOCOUNT ON;
    IF @MaTK IS NULL OR LTRIM(RTRIM(@MaTK))=''
    BEGIN
        DECLARE @MaxNumber int;
        SELECT @MaxNumber=MAX(TRY_CONVERT(int,SUBSTRING(MaTK,3,20))) FROM TaiKhoan WITH(UPDLOCK,HOLDLOCK) WHERE MaTK LIKE 'TK%' AND TRY_CONVERT(int,SUBSTRING(MaTK,3,20)) IS NOT NULL;
        SET @MaTK=dbo.fn_NextCode('TK',@MaxNumber,2);
    END
    INSERT INTO TaiKhoan(MaTK,TenDangNhap,MatKhau,MaNV,VaiTro) VALUES(@MaTK,@TenDangNhap,@MatKhau,@MaNV,@VaiTro);
END;



GO
/****** Object:  StoredProcedure [dbo].[TaiKhoan_Xoa]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[TaiKhoan_Xoa] @MaTK varchar(10) AS BEGIN SET NOCOUNT ON; DELETE FROM TaiKhoan WHERE MaTK=@MaTK; END;



GO
/****** Object:  StoredProcedure [dbo].[ThanhToan_ChiTietTheoHoaDon]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   10. Proc kiểm tra trạng thái thanh toán của hóa đơn
   ========================================================= */

CREATE   PROC [dbo].[ThanhToan_ChiTietTheoHoaDon]
    @MaHD varchar(9)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        hd.MaHD,
        hd.NgayLap,
        hd.MaNV,
        hd.MaKH,
        hd.MaBan,
        hd.TongTien,
        hd.TrangThaiOrder,
        hd.TrangThaiThanhToan,
        hd.PhuongThucThanhToan,
        hd.ThoiGianTao,
        hd.ThoiGianThanhToan,
        hd.GhiChu
    FROM dbo.HoaDon hd
    WHERE hd.MaHD = @MaHD;

    SELECT
        tt.MaThanhToan,
        tt.MaHD,
        tt.MaPhienThanhToan,
        tt.PhuongThuc,
        tt.SoTien,
        tt.NoiDungChuyenKhoan,
        tt.MaGiaoDich,
        tt.TrangThai,
        tt.ThoiGianTao,
        tt.ThoiGianHetHan,
        tt.ThoiGianThanhToan,
        tt.GhiChu
    FROM dbo.ThanhToan tt
    WHERE tt.MaHD = @MaHD
    ORDER BY tt.MaThanhToan DESC;
END

GO
/****** Object:  StoredProcedure [dbo].[ThanhToan_DanhSachChoKiemTra]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   5. Proc xem các thanh toán cần kiểm tra thủ công
   ========================================================= */

CREATE   PROC [dbo].[ThanhToan_DanhSachChoKiemTra]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        tt.MaThanhToan,
        tt.MaHD,
        tt.MaPhienThanhToan,
        tt.PhuongThuc,
        tt.SoTien,
        tt.NoiDungChuyenKhoan,
        tt.MaGiaoDich,
        tt.TrangThai,
        tt.ThoiGianTao,
        tt.ThoiGianHetHan,
        tt.ThoiGianThanhToan,
        tt.GhiChu
    FROM dbo.ThanhToan tt
    WHERE tt.TrangThai = N'ChoKiemTra'
    ORDER BY tt.ThoiGianTao DESC, tt.MaThanhToan DESC;
END

GO
/****** Object:  StoredProcedure [dbo].[ThanhToan_HuyOrder]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   4. Hủy order
   - Hủy thì trả bàn về Trống
   ========================================================= */
CREATE   PROC [dbo].[ThanhToan_HuyOrder]
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
        RAISERROR(N'H�a don kh�ng t?n t?i.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TrangThaiThanhToan = N'DaThanhToan' AND @ChoPhepKhiDaThanhToan = 0
    BEGIN
        RAISERROR(N'H�a don d� thanh to�n, kh�ng du?c h?y.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TrangThaiThanhToan = N'DaThanhToan' AND @ChoPhepKhiDaThanhToan = 1
    BEGIN
        UPDATE dbo.ThanhToan
        SET TrangThai = N'DaHuy',
            GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | Qu?n l� ho�n don/h?y thanh to�n. ', ISNULL(@LyDo, N'')), 255)
        WHERE MaHD = @MaHD AND TrangThai = N'DaThanhToan';

        UPDATE dbo.HoaDon
        SET TrangThaiOrder = N'DaHuy',
            TrangThaiThanhToan = N'DaHuy',
            GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | Qu?n l� ho�n don: ', ISNULL(@LyDo, N'Kh�ng c�')), 100)
        WHERE MaHD = @MaHD;
    END
    ELSE
    BEGIN
        UPDATE dbo.ThanhToan
        SET TrangThai = N'DaHuy',
            GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | H?y order. ', ISNULL(@LyDo, N'')), 255)
        WHERE MaHD = @MaHD
          AND TrangThai IN (N'ChuaThanhToan', N'ChoThanhToanQR', N'HetHan', N'ChoKiemTra');

        UPDATE dbo.HoaDon
        SET TrangThaiOrder = N'DaHuy',
            TrangThaiThanhToan = N'DaHuy',
            GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | L� do h?y: ', ISNULL(@LyDo, N'Kh�ng c�')), 100)
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
            SET TrangThai = N'Tr?ng'
            WHERE MaBan = @MaBan;
        END;
    END;

    COMMIT TRANSACTION;

    SELECT 
        1 AS Success, 
        CASE 
            WHEN @TrangThaiThanhToan = N'DaThanhToan' THEN N'Ho�n don v� h?y thanh to�n th�nh c�ng.'
            ELSE N'H?y order th�nh c�ng.'
        END AS Message;
END;
GO
/****** Object:  StoredProcedure [dbo].[ThanhToan_QRHetHan]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   6. QR hết hạn 1 hóa đơn
   - Hết hạn chưa thanh toán thì hủy order và trả bàn Trống
   ========================================================= */
CREATE   PROC [dbo].[ThanhToan_QRHetHan]
    @MaHD varchar(9)
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

    IF @TrangThaiThanhToan = N'DaThanhToan'
    BEGIN
        RAISERROR(N'Hóa đơn đã thanh toán, không xử lý hết hạn.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    UPDATE dbo.ThanhToan
    SET TrangThai = N'HetHan',
        GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | QR quá hạn 15 phút.'), 255)
    WHERE MaHD = @MaHD
      AND PhuongThuc = N'QR'
      AND TrangThai = N'ChoThanhToanQR'
      AND ThoiGianHetHan <= SYSDATETIME();

    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR(N'Không có phiên QR quá hạn cần xử lý.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    UPDATE dbo.HoaDon
    SET TrangThaiOrder = N'DaHuy',
        TrangThaiThanhToan = N'HetHan',
        GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | QR quá hạn, order bị hủy.'), 100)
    WHERE MaHD = @MaHD;

    IF @MaBan IS NOT NULL AND @MaBan <> 'MVE'
    BEGIN
        UPDATE dbo.Ban
        SET TrangThai = N'Trống'
        WHERE MaBan = @MaBan;
    END;

    COMMIT TRANSACTION;

    SELECT
        CAST(1 AS bit) AS Success,
        N'QR quá hạn, order đã bị hủy và bàn đã trả về Trống nếu là đơn tại quán.' AS Message,
        @MaHD AS MaHD,
        @MaBan AS MaBan;
END

GO
/****** Object:  StoredProcedure [dbo].[ThanhToan_SuaOrder]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   5. Sửa order
   - Không hủy order
   - Hủy phiên QR hiện tại
   - Bàn vẫn Đang phục vụ
   ========================================================= */
CREATE   PROC [dbo].[ThanhToan_SuaOrder]
    @MaHD varchar(9)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE
        @MaBan varchar(5),
        @TrangThaiThanhToan nvarchar(30),
        @TrangThaiOrder nvarchar(30);

    SELECT
        @MaBan = MaBan,
        @TrangThaiThanhToan = TrangThaiThanhToan,
        @TrangThaiOrder = TrangThaiOrder
    FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
    WHERE MaHD = @MaHD;

    IF @MaBan IS NULL
    BEGIN
        RAISERROR(N'Hóa đơn không tồn tại.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TrangThaiOrder = N'DaHuy'
    BEGIN
        RAISERROR(N'Hóa đơn đã hủy, không được sửa.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TrangThaiThanhToan = N'DaThanhToan'
    BEGIN
        RAISERROR(N'Hóa đơn đã thanh toán, không được sửa.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    UPDATE dbo.ThanhToan
    SET TrangThai = N'DaHuy',
        GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | Hủy phiên thanh toán do nhân viên bấm Sửa.'), 255)
    WHERE MaHD = @MaHD
      AND TrangThai IN (N'ChuaThanhToan', N'ChoThanhToanQR');

    UPDATE dbo.HoaDon
    SET TrangThaiOrder = N'DangOrder',
        TrangThaiThanhToan = N'ChuaThanhToan',
        PhuongThucThanhToan = NULL,
        ThoiGianThanhToan = NULL
    WHERE MaHD = @MaHD;

    IF @MaBan IS NOT NULL AND @MaBan <> 'MVE'
    BEGIN
        UPDATE dbo.Ban
        SET TrangThai = N'Đang phục vụ'
        WHERE MaBan = @MaBan;
    END;

    COMMIT TRANSACTION;

    SELECT
        CAST(1 AS bit) AS Success,
        N'Order đã chuyển về trạng thái sửa. Bàn tại quán vẫn Đang phục vụ.' AS Message,
        @MaHD AS MaHD,
        @MaBan AS MaBan;
END

GO
/****** Object:  StoredProcedure [dbo].[ThanhToan_TaoQR]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   5. Proc: Tạo phiên QR 15 phút
   ========================================================= */

CREATE   PROC [dbo].[ThanhToan_TaoQR]
    @MaHD varchar(9)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE
        @TongTien money,
        @TrangThaiThanhToan nvarchar(30),
        @TrangThaiOrder nvarchar(30),
        @Lan int,
        @MaPhienThanhToan nvarchar(50),
        @NoiDung nvarchar(255),
        @HetHan datetime2(0);

    SELECT
        @TongTien = TongTien,
        @TrangThaiThanhToan = TrangThaiThanhToan,
        @TrangThaiOrder = TrangThaiOrder
    FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
    WHERE MaHD = @MaHD;

    IF @TongTien IS NULL
    BEGIN
        RAISERROR(N'Hóa đơn không tồn tại.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TrangThaiOrder = N'DaHuy'
    BEGIN
        RAISERROR(N'Hóa đơn đã hủy, không thể tạo QR.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TrangThaiThanhToan = N'DaThanhToan'
    BEGIN
        RAISERROR(N'Hóa đơn đã thanh toán, không được tạo QR mới.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TongTien <= 0
    BEGIN
        RAISERROR(N'Tổng tiền hóa đơn không hợp lệ.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    UPDATE dbo.ThanhToan
    SET TrangThai = N'DaHuy',
        GhiChu = ISNULL(GhiChu, N'') + N' | Hủy phiên QR cũ do tạo QR mới.'
    WHERE MaHD = @MaHD
      AND PhuongThuc = N'QR'
      AND TrangThai = N'ChoThanhToanQR';

    SELECT @Lan = COUNT(*) + 1
    FROM dbo.ThanhToan WITH (UPDLOCK, HOLDLOCK)
    WHERE MaHD = @MaHD
      AND PhuongThuc = N'QR';

    SET @MaPhienThanhToan = CONCAT(@MaHD, N'-L', @Lan);
    SET @NoiDung = @MaPhienThanhToan;
    SET @HetHan = DATEADD(MINUTE, 15, SYSDATETIME());

    INSERT INTO dbo.ThanhToan
    (
        MaHD,
        MaPhienThanhToan,
        PhuongThuc,
        SoTien,
        NoiDungChuyenKhoan,
        MaGiaoDich,
        TrangThai,
        ThoiGianTao,
        ThoiGianHetHan,
        ThoiGianThanhToan,
        GhiChu
    )
    VALUES
    (
        @MaHD,
        @MaPhienThanhToan,
        N'QR',
        @TongTien,
        @NoiDung,
        NULL,
        N'ChoThanhToanQR',
        SYSDATETIME(),
        @HetHan,
        NULL,
        N'Phiên QR có hiệu lực 15 phút.'
    );

    UPDATE dbo.HoaDon
    SET TrangThaiOrder = N'ChoThanhToan',
        TrangThaiThanhToan = N'ChoThanhToanQR',
        PhuongThucThanhToan = N'QR'
    WHERE MaHD = @MaHD;

    COMMIT TRANSACTION;

    SELECT
        CAST(1 AS bit) AS Success,
        N'Tạo QR thành công.' AS Message,
        @MaHD AS MaHD,
        @MaPhienThanhToan AS MaPhienThanhToan,
        @TongTien AS SoTien,
        @NoiDung AS NoiDungChuyenKhoan,
        @HetHan AS ThoiGianHetHan;
END

GO
/****** Object:  StoredProcedure [dbo].[ThanhToan_TienMat]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   FINAL PATCH PAYMENT LOGIC
   Logic mới:
   - Thanh toán xong KHÔNG trả bàn ngay nếu tại quán.
   - Chỉ Hủy / QR hết hạn / Khách đi mới trả bàn về Trống.
   - Mang về dùng MVE, không đụng bàn vật lý.
   ========================================================= */


/* =========================================================
   1. Thanh toán tiền mặt
   - Hóa đơn chuyển HoanThanh / DaThanhToan / TienMat
   - Bàn tại quán vẫn Đang phục vụ
   - MVE vẫn Trống
   ========================================================= */
CREATE   PROC [dbo].[ThanhToan_TienMat]
    @MaHD varchar(9),
    @GhiChu nvarchar(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE
        @TongTien money,
        @MaBan varchar(5),
        @TrangThaiThanhToan nvarchar(30),
        @TrangThaiOrder nvarchar(30);

    SELECT
        @TongTien = TongTien,
        @MaBan = MaBan,
        @TrangThaiThanhToan = TrangThaiThanhToan,
        @TrangThaiOrder = TrangThaiOrder
    FROM dbo.HoaDon WITH (UPDLOCK, HOLDLOCK)
    WHERE MaHD = @MaHD;

    IF @TongTien IS NULL
    BEGIN
        RAISERROR(N'Hóa đơn không tồn tại.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TrangThaiOrder = N'DaHuy'
    BEGIN
        RAISERROR(N'Hóa đơn đã hủy, không thể thanh toán.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TrangThaiThanhToan = N'DaThanhToan'
    BEGIN
        RAISERROR(N'Hóa đơn đã thanh toán, không được thanh toán lần 2.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @TongTien <= 0
    BEGIN
        RAISERROR(N'Tổng tiền hóa đơn không hợp lệ.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    UPDATE dbo.ThanhToan
    SET TrangThai = N'DaHuy',
        GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | Hủy phiên chờ do thanh toán tiền mặt.'), 255)
    WHERE MaHD = @MaHD
      AND TrangThai IN (N'ChuaThanhToan', N'ChoThanhToanQR');

    INSERT INTO dbo.ThanhToan
    (
        MaHD,
        MaPhienThanhToan,
        PhuongThuc,
        SoTien,
        NoiDungChuyenKhoan,
        MaGiaoDich,
        TrangThai,
        ThoiGianTao,
        ThoiGianHetHan,
        ThoiGianThanhToan,
        GhiChu
    )
    VALUES
    (
        @MaHD,
        NULL,
        N'TienMat',
        @TongTien,
        NULL,
        NULL,
        N'DaThanhToan',
        SYSDATETIME(),
        NULL,
        SYSDATETIME(),
        @GhiChu
    );

    UPDATE dbo.HoaDon
    SET TrangThaiOrder = N'HoanThanh',
        TrangThaiThanhToan = N'DaThanhToan',
        PhuongThucThanhToan = N'TienMat',
        ThoiGianThanhToan = SYSDATETIME()
    WHERE MaHD = @MaHD;

    /*
       KHÔNG trả bàn ở đây.
       Vì logic quán: khách thanh toán xong mới đem nước vào bàn ngồi.
       Bàn chỉ về Trống khi nhân viên bấm Khách đi.
    */

    COMMIT TRANSACTION;

    SELECT
        CAST(1 AS bit) AS Success,
        N'Thanh toán tiền mặt thành công. Bàn tại quán vẫn giữ trạng thái Đang phục vụ cho đến khi khách đi.' AS Message,
        @MaHD AS MaHD,
        @MaBan AS MaBan;
END

GO
/****** Object:  StoredProcedure [dbo].[ThanhToan_XacNhanQRTheoPhien]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =========================================================
   7. ThanhToan_XacNhanQRTheoPhien:
      - Tìm bằng mã phiên chính xác.
      - Nếu webhook đến muộn sau hạn: ChoKiemTra, không tự hủy cứng khi tiền đã về.
      - Thanh toán thành công vẫn không trả bàn.
   ========================================================= */
CREATE   PROC [dbo].[ThanhToan_XacNhanQRTheoPhien]
    @MaGiaoDich nvarchar(100),
    @MaPhienThanhToan nvarchar(50),
    @SoTien money,
    @NoiDungChuyenKhoan nvarchar(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    IF @MaGiaoDich IS NULL OR LTRIM(RTRIM(@MaGiaoDich)) = N''
    BEGIN
        RAISERROR(N'Mã giao dịch không được để trống.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @MaPhienThanhToan IS NULL OR LTRIM(RTRIM(@MaPhienThanhToan)) = N''
    BEGIN
        RAISERROR(N'Mã phiên thanh toán không được để trống.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF @SoTien <= 0
    BEGIN
        RAISERROR(N'Số tiền giao dịch không hợp lệ.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM dbo.ThanhToan WITH (UPDLOCK, HOLDLOCK)
        WHERE MaGiaoDich = @MaGiaoDich
    )
    BEGIN
        RAISERROR(N'Mã giao dịch đã tồn tại, không xử lý trùng.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    DECLARE
        @MaThanhToan int,
        @MaHD varchar(9),
        @SoTienCanThu money,
        @HetHan datetime2(0),
        @TrangThaiThanhToanRecord nvarchar(30),
        @TrangThaiOrder nvarchar(30),
        @TrangThaiThanhToan nvarchar(30),
        @MaBan varchar(5);

    SELECT TOP 1
        @MaThanhToan = tt.MaThanhToan,
        @MaHD = tt.MaHD,
        @SoTienCanThu = tt.SoTien,
        @HetHan = tt.ThoiGianHetHan,
        @TrangThaiThanhToanRecord = tt.TrangThai
    FROM dbo.ThanhToan tt WITH (UPDLOCK, HOLDLOCK)
    WHERE tt.PhuongThuc = N'QR'
      AND tt.MaPhienThanhToan = @MaPhienThanhToan
    ORDER BY tt.MaThanhToan DESC;

    IF @MaThanhToan IS NULL
    BEGIN
        INSERT INTO dbo.ThanhToan
        (
            MaHD,
            MaPhienThanhToan,
            PhuongThuc,
            SoTien,
            NoiDungChuyenKhoan,
            MaGiaoDich,
            TrangThai,
            ThoiGianTao,
            GhiChu
        )
        VALUES
        (
            NULL,
            @MaPhienThanhToan,
            N'QR',
            @SoTien,
            @NoiDungChuyenKhoan,
            @MaGiaoDich,
            N'ChoKiemTra',
            SYSDATETIME(),
            N'Mã phiên thanh toán không tồn tại. Giao dịch cần kiểm tra thủ công.'
        );

        COMMIT TRANSACTION;

        SELECT CAST(0 AS bit) AS Success,
               N'Mã phiên QR không tồn tại. Giao dịch đã đưa vào Chờ kiểm tra.' AS Message;
        RETURN;
    END;

    SELECT
        @TrangThaiOrder = hd.TrangThaiOrder,
        @TrangThaiThanhToan = hd.TrangThaiThanhToan,
        @MaBan = hd.MaBan
    FROM dbo.HoaDon hd WITH (UPDLOCK, HOLDLOCK)
    WHERE hd.MaHD = @MaHD;

    IF @TrangThaiThanhToanRecord <> N'ChoThanhToanQR'
       OR @TrangThaiOrder = N'DaHuy'
       OR @TrangThaiThanhToan IN (N'DaThanhToan', N'DaHuy', N'HetHan')
    BEGIN
        UPDATE dbo.ThanhToan
        SET MaGiaoDich = @MaGiaoDich,
            TrangThai = N'ChoKiemTra',
            NoiDungChuyenKhoan = ISNULL(@NoiDungChuyenKhoan, NoiDungChuyenKhoan),
            GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | Giao dịch đến khi phiên/hóa đơn không còn ở trạng thái tự xác nhận.'), 255)
        WHERE MaThanhToan = @MaThanhToan;

        COMMIT TRANSACTION;

        SELECT CAST(0 AS bit) AS Success,
               N'Phiên QR hoặc hóa đơn không còn hợp lệ để xác nhận tự động. Giao dịch đã đưa vào Chờ kiểm tra.' AS Message,
               @MaHD AS MaHD;
        RETURN;
    END;

    IF @HetHan IS NOT NULL AND @HetHan < SYSDATETIME()
    BEGIN
        UPDATE dbo.ThanhToan
        SET MaGiaoDich = @MaGiaoDich,
            TrangThai = N'ChoKiemTra',
            NoiDungChuyenKhoan = ISNULL(@NoiDungChuyenKhoan, NoiDungChuyenKhoan),
            GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | Giao dịch đến muộn sau khi QR hết hạn. Cần tra soát/hoàn tiền hoặc duyệt thủ công.'), 255)
        WHERE MaThanhToan = @MaThanhToan;

        UPDATE dbo.HoaDon
        SET TrangThaiThanhToan = N'ChoKiemTra'
        WHERE MaHD = @MaHD
          AND TrangThaiThanhToan <> N'DaThanhToan';

        COMMIT TRANSACTION;

        SELECT CAST(0 AS bit) AS Success,
               N'Giao dịch đến muộn sau khi QR hết hạn. Hệ thống chuyển sang Chờ kiểm tra, không tự hủy cứng khi tiền đã về.' AS Message,
               @MaHD AS MaHD;
        RETURN;
    END;

    IF @SoTien <> @SoTienCanThu
    BEGIN
        UPDATE dbo.ThanhToan
        SET MaGiaoDich = @MaGiaoDich,
            TrangThai = N'ChoKiemTra',
            NoiDungChuyenKhoan = ISNULL(@NoiDungChuyenKhoan, NoiDungChuyenKhoan),
            GhiChu = LEFT(CONCAT(
                ISNULL(GhiChu, N''),
                N' | Sai số tiền. Cần thu: ',
                CONVERT(nvarchar(50), @SoTienCanThu),
                N', nhận: ',
                CONVERT(nvarchar(50), @SoTien)
            ), 255)
        WHERE MaThanhToan = @MaThanhToan;

        UPDATE dbo.HoaDon
        SET TrangThaiThanhToan = N'ChoKiemTra'
        WHERE MaHD = @MaHD
          AND TrangThaiThanhToan <> N'DaThanhToan';

        COMMIT TRANSACTION;

        SELECT CAST(0 AS bit) AS Success,
               N'Sai số tiền. Giao dịch đã đưa vào Chờ kiểm tra.' AS Message,
               @MaHD AS MaHD;
        RETURN;
    END;

    UPDATE dbo.ThanhToan
    SET MaGiaoDich = @MaGiaoDich,
        SoTien = @SoTien,
        NoiDungChuyenKhoan = ISNULL(@NoiDungChuyenKhoan, NoiDungChuyenKhoan),
        TrangThai = N'DaThanhToan',
        ThoiGianThanhToan = SYSDATETIME(),
        GhiChu = LEFT(CONCAT(ISNULL(GhiChu, N''), N' | QR được xác nhận tự động theo mã phiên chính xác.'), 255)
    WHERE MaThanhToan = @MaThanhToan;

    UPDATE dbo.HoaDon
    SET TrangThaiOrder = N'HoanThanh',
        TrangThaiThanhToan = N'DaThanhToan',
        PhuongThucThanhToan = N'QR',
        ThoiGianThanhToan = SYSDATETIME()
    WHERE MaHD = @MaHD;

    COMMIT TRANSACTION;

    SELECT CAST(1 AS bit) AS Success,
           N'Thanh toán QR thành công. Nếu là đơn tại quán, bàn vẫn Đang phục vụ cho đến khi khách đi.' AS Message,
           @MaHD AS MaHD,
           @MaPhienThanhToan AS MaPhienThanhToan,
           @MaBan AS MaBan;
END;

GO
/****** Object:  StoredProcedure [dbo].[ThanhToan_XacNhanQRWebhook]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   3. Proc LIKE cũ nếu còn ai gọi thì vẫn cập nhật đúng logic mới
   - Nên để C# gọi ThanhToan_XacNhanQRTheoPhien
   - Proc này chỉ giữ để tương thích nếu code cũ gọi
   ========================================================= */
CREATE   PROC [dbo].[ThanhToan_XacNhanQRWebhook]
    @MaGiaoDich nvarchar(100),
    @SoTien money,
    @NoiDungChuyenKhoan nvarchar(255)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MaPhienThanhToan nvarchar(50);

    SELECT TOP 1
        @MaPhienThanhToan = tt.MaPhienThanhToan
    FROM dbo.ThanhToan tt
    WHERE tt.PhuongThuc = N'QR'
      AND tt.TrangThai = N'ChoThanhToanQR'
      AND tt.MaPhienThanhToan IS NOT NULL
      AND @NoiDungChuyenKhoan LIKE N'%' + tt.MaPhienThanhToan + N'%'
    ORDER BY tt.ThoiGianTao DESC;

    IF @MaPhienThanhToan IS NULL
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM dbo.ThanhToan
            WHERE MaGiaoDich = @MaGiaoDich
        )
        BEGIN
            RAISERROR(N'Mã giao dịch đã tồn tại, không xử lý trùng.', 16, 1);
            RETURN;
        END;

        INSERT INTO dbo.ThanhToan
        (
            MaHD,
            MaPhienThanhToan,
            PhuongThuc,
            SoTien,
            NoiDungChuyenKhoan,
            MaGiaoDich,
            TrangThai,
            ThoiGianTao,
            GhiChu
        )
        VALUES
        (
            NULL,
            NULL,
            N'QR',
            @SoTien,
            @NoiDungChuyenKhoan,
            @MaGiaoDich,
            N'ChoKiemTra',
            SYSDATETIME(),
            N'Không tìm thấy phiên QR hợp lệ trong nội dung chuyển khoản.'
        );

        SELECT
            CAST(0 AS bit) AS Success,
            N'Không tìm thấy phiên QR hợp lệ. Giao dịch đã đưa vào Chờ kiểm tra.' AS Message;
        RETURN;
    END;

    EXEC dbo.ThanhToan_XacNhanQRTheoPhien
        @MaGiaoDich = @MaGiaoDich,
        @MaPhienThanhToan = @MaPhienThanhToan,
        @SoTien = @SoTien,
        @NoiDungChuyenKhoan = @NoiDungChuyenKhoan;
END

GO
/****** Object:  StoredProcedure [dbo].[ThanhToan_XuLyTatCaQRHetHan]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* =========================================================
   7. Xử lý tất cả QR hết hạn
   - Hết hạn chưa thanh toán thì hủy order và trả bàn Trống
   ========================================================= */
CREATE   PROC [dbo].[ThanhToan_XuLyTatCaQRHetHan]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Expired TABLE
    (
        MaHD varchar(9) PRIMARY KEY
    );

    INSERT INTO @Expired(MaHD)
    SELECT DISTINCT tt.MaHD
    FROM dbo.ThanhToan tt WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN dbo.HoaDon hd WITH (UPDLOCK, HOLDLOCK)
        ON tt.MaHD = hd.MaHD
    WHERE tt.PhuongThuc = N'QR'
      AND tt.TrangThai = N'ChoThanhToanQR'
      AND tt.ThoiGianHetHan IS NOT NULL
      AND tt.ThoiGianHetHan <= SYSDATETIME()
      AND hd.TrangThaiThanhToan <> N'DaThanhToan'
      AND hd.TrangThaiOrder <> N'DaHuy';

    UPDATE tt
    SET tt.TrangThai = N'HetHan',
        tt.GhiChu = LEFT(CONCAT(ISNULL(tt.GhiChu, N''), N' | QR quá hạn 15 phút, xử lý tự động.'), 255)
    FROM dbo.ThanhToan tt
    INNER JOIN @Expired e
        ON tt.MaHD = e.MaHD
    WHERE tt.PhuongThuc = N'QR'
      AND tt.TrangThai = N'ChoThanhToanQR'
      AND tt.ThoiGianHetHan <= SYSDATETIME();

    UPDATE hd
    SET hd.TrangThaiOrder = N'DaHuy',
        hd.TrangThaiThanhToan = N'HetHan',
        hd.GhiChu = LEFT(CONCAT(ISNULL(hd.GhiChu, N''), N' | QR quá hạn, order bị hủy.'), 100)
    FROM dbo.HoaDon hd
    INNER JOIN @Expired e
        ON hd.MaHD = e.MaHD;

    UPDATE b
    SET b.TrangThai = N'Trống'
    FROM dbo.Ban b
    INNER JOIN dbo.HoaDon hd
        ON b.MaBan = hd.MaBan
    INNER JOIN @Expired e
        ON hd.MaHD = e.MaHD
    WHERE b.MaBan <> 'MVE';

    DECLARE @SoHoaDonXuLy int = (SELECT COUNT(*) FROM @Expired);

    COMMIT TRANSACTION;

    SELECT
        CAST(1 AS bit) AS Success,
        @SoHoaDonXuLy AS SoHoaDonXuLy,
        N'Đã xử lý các QR hết hạn. Các bàn tại quán liên quan đã trả về Trống.' AS Message;
END

GO
/****** Object:  StoredProcedure [dbo].[ThongKe_DoanhThuTheoNgay]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE   PROC [dbo].[ThongKe_DoanhThuTheoNgay]
    @TuNgay date,
    @DenNgay date
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NgayLap, SUM(ISNULL(TongTien,0)) AS DoanhThu
    FROM dbo.HoaDon
    WHERE NgayLap BETWEEN @TuNgay AND @DenNgay
      AND TrangThaiThanhToan = N'DaThanhToan'
    GROUP BY NgayLap
    ORDER BY NgayLap;
END;

GO
/****** Object:  StoredProcedure [dbo].[ThongKe_NhapHangTheoThang]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[ThongKe_NhapHangTheoThang] @Thang int, @Nam int AS
BEGIN SET NOCOUNT ON; SELECT MONTH(NgayNhap) AS Thang,YEAR(NgayNhap) AS Nam,SUM(TongTien) AS TongTienNhap FROM PhieuNhap WHERE MONTH(NgayNhap)=@Thang AND YEAR(NgayNhap)=@Nam GROUP BY MONTH(NgayNhap),YEAR(NgayNhap); END;



GO
/****** Object:  StoredProcedure [dbo].[ThongKe_SanPhamBanChay]    Script Date: 05/30/2026 11:12:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE   PROC [dbo].[ThongKe_SanPhamBanChay]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT sp.MaSP, sp.TenSP, SUM(ct.SoLuong) AS TongSoLuong, SUM(ct.ThanhTien) AS TongTien
    FROM dbo.CTHD ct
    INNER JOIN dbo.SanPham sp ON ct.MaSP = sp.MaSP
    INNER JOIN dbo.HoaDon hd ON ct.MaHD = hd.MaHD
    WHERE hd.TrangThaiThanhToan = N'DaThanhToan'
    GROUP BY sp.MaSP, sp.TenSP
    ORDER BY TongSoLuong DESC;
END;

GO
USE [master]
GO
ALTER DATABASE [QLCHCF] SET  READ_WRITE 
GO
